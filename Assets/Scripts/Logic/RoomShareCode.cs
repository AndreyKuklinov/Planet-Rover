using System;
using System.IO;
using System.IO.Compression;
using Draft = RoomWorkshop.Draft;
using Tile = RoomWorkshop.Tile;

// R1 uses unpadded URL-safe Base64. Only interior cells are transmitted, in row-major order.
internal static class RoomShareCode
{
    public const string Prefix = "R1.";
    enum Layout { Sparse, SparseWithBackground, Packed, Deflated }
    const int TileKinds = 7;
    const int PackedBlock = 17; // 17 base-seven digits fit in six bytes.

    public static string Encode(Draft room)
    {
        byte[] cells = new byte[(room.width - 2) * (room.height - 2)];
        int[] counts = new int[TileKinds];
        int n = 0;
        for (int y = 1; y < room.height - 1; y++)
            for (int x = 1; x < room.width - 1; x++)
            {
                byte tile = (byte)room.cells[y * room.width + x];
                cells[n++] = tile;
                counts[tile]++;
            }

        Layout layout = Layout.Sparse;
        byte[] best = Sparse(cells, 0);
        byte common = 0;
        for (byte i = 1; i < TileKinds; i++)
            if (counts[i] > counts[common]) common = i;
        if (common != 0) Consider(Layout.SparseWithBackground, Sparse(cells, common));
        Consider(Layout.Packed, Pack(cells));
        using (var compressed = new MemoryStream())
        {
            using (var zip = new DeflateStream(compressed, CompressionLevel.Optimal, true))
                zip.Write(cells, 0, cells.Length);
            Consider(Layout.Deflated, compressed.ToArray());
        }

        using var output = new MemoryStream();
        bool dimensions = room.width != 17 || room.height != 9;
        int flags = (room.lockRatio ? 0 : 1) | (room.countdown ? 2 : 0)
            | (room.mirrorX ? 4 : 0) | (room.mirrorY ? 8 : 0)
            | (dimensions ? 16 : 0) | (room.seconds != 30 ? 32 : 0) | ((int)layout << 6);
        output.WriteByte((byte)flags);
        if (dimensions) { output.WriteByte((byte)room.width); output.WriteByte((byte)room.height); }
        if (room.seconds != 30) WriteUInt(output, room.seconds);
        output.Write(best, 0, best.Length);
        return Prefix + Convert.ToBase64String(output.ToArray()).TrimEnd('=').Replace('+','-').Replace('/','_');

        void Consider(Layout option, byte[] bytes)
        {
            if (bytes.Length >= best.Length) return;
            layout = option;
            best = bytes;
        }
    }

    // Encode only differences from the background. One integer combines the gap and tile.
    static byte[] Sparse(byte[] cells, byte background)
    {
        using var output = new MemoryStream();
        if (background != 0) output.WriteByte(background);
        int next = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            int tile = cells[i];
            if (tile == background) continue;
            int symbol = tile < background ? tile : tile - 1;
            WriteUInt(output, (i - next) * (TileKinds - 1) + symbol);
            next = i + 1;
        }
        return output.ToArray();
    }

    static void WriteUInt(Stream output, int value)
    {
        while (value >= 128)
        {
            output.WriteByte((byte)((value & 127) | 128));
            value >>= 7;
        }
        output.WriteByte((byte)value);
    }

    static int ReadUInt(BinaryReader reader)
    {
        int value = 0;
        for (int shift = 0; shift <= 14; shift += 7)
        {
            int b = reader.ReadByte();
            value |= (b & 127) << shift;
            if (b < 128) return value;
        }
        throw new FormatException();
    }

    static byte[] Pack(byte[] cells)
    {
        using var output = new MemoryStream();
        for (int start = 0; start < cells.Length; start += PackedBlock)
        {
            int end = Math.Min(start + PackedBlock, cells.Length);
            ulong value = 0, range = 1;
            for (int i = end - 1; i >= start; i--)
            {
                value = value * TileKinds + cells[i];
                range *= TileKinds;
            }
            for (ulong max = range - 1; max != 0; max >>= 8)
            {
                output.WriteByte((byte)(value & 255));
                value >>= 8;
            }
        }
        return output.ToArray();
    }

    public static Draft Decode(string text)
    {
        // The packed fallback bounds every code, even for a 99 × 99 room.
        if (!text.StartsWith(Prefix, StringComparison.Ordinal) || text.Length > 4500) return null;
        try
        {
            string body = text.Substring(Prefix.Length).Replace('-','+').Replace('_','/');
            byte[] bytes = Convert.FromBase64String(body.PadRight((body.Length + 3) / 4 * 4, '='));
            using var input = new MemoryStream(bytes);
            using var reader = new BinaryReader(input);
            int flags = reader.ReadByte();
            int width = (flags & 16) != 0 ? reader.ReadByte() : 17;
            int height = (flags & 16) != 0 ? reader.ReadByte() : 9;
            int seconds = (flags & 32) != 0 ? ReadUInt(reader) : 30;
            if (width < 3 || width > 99 || width % 2 == 0 || height < 3 || height > 99 || height % 2 == 0
                || seconds < 1 || seconds > 3600) return null;

            byte[] cells = new byte[(width - 2) * (height - 2)];
            switch ((Layout)(flags >> 6))
            {
                case Layout.Packed:
                    for (int start = 0; start < cells.Length; start += PackedBlock)
                    {
                        int end = Math.Min(start + PackedBlock, cells.Length);
                        ulong range = 1, value = 0;
                        for (int i = start; i < end; i++) range *= TileKinds;
                        for (ulong place = 1; place < range; place <<= 8)
                            value |= (ulong)reader.ReadByte() * place;
                        if (value >= range) return null;
                        for (int i = start; i < end; i++) { cells[i] = (byte)(value % TileKinds); value /= TileKinds; }
                    }
                    break;
                case Layout.Deflated:
                    using (var zip = new DeflateStream(input, CompressionMode.Decompress, true))
                    {
                        int offset = 0;
                        while (offset < cells.Length)
                        {
                            int read = zip.Read(cells, offset, cells.Length - offset);
                            if (read == 0) return null;
                            offset += read;
                        }
                        if (zip.ReadByte() != -1) return null;
                    }
                    break;
                default:
                    int background = (Layout)(flags >> 6) == Layout.SparseWithBackground ? reader.ReadByte() : 0;
                    if (background >= TileKinds) return null;
                    if (background != 0) Array.Fill(cells,(byte)background);
                    int next = 0;
                    while (input.Position < input.Length)
                    {
                        int token = ReadUInt(reader);
                        int index = next + token / (TileKinds - 1);
                        if (index >= cells.Length) return null;
                        int symbol = token % (TileKinds - 1);
                        cells[index] = (byte)(symbol < background ? symbol : symbol + 1);
                        next = index + 1;
                    }
                    break;
            }
            if (input.Position != input.Length) return null;
            var draft = new Draft { width = width, height = height, seconds = seconds, cells = new Tile[width * height],
                lockRatio = (flags & 1) == 0, countdown = (flags & 2) != 0, mirrorX = (flags & 4) != 0, mirrorY = (flags & 8) != 0 };
            for (int i = 0; i < cells.Length; i++)
                draft.cells[(1 + i / (width - 2)) * width + 1 + i % (width - 2)] = (Tile)cells[i];
            return draft;
        }
        catch (FormatException) { return null; }
        catch (InvalidDataException) { return null; }
        catch (IOException) { return null; }
    }
}
