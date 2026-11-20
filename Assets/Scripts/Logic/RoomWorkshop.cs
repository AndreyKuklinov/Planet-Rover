using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Room))]
public class RoomWorkshop : MonoBehaviour
{
    public enum Tile { Empty, RockA, RockB, RocketA, RocketB, Wall, Rover }
    enum Mode { Edit, Play, Result }

    [Serializable]
    public class Draft
    {
        public int width = 17, height = 9, seconds = 30;
        public bool lockRatio = true, countdown, mirrorX, mirrorY;
        public Tile[] cells = new Tile[17 * 9];

        public static Draft Starter()
        {
            var d = new Draft();
            d.cells[4 * 17 + 8] = Tile.Rover;
            d.cells[4 * 17 + 10] = Tile.RockA;
            d.cells[4 * 17 + 14] = Tile.RocketA;
            d.cells[3 * 17 + 8] = Tile.RockB;
            d.cells[1 * 17 + 8] = Tile.RocketB;
            foreach (int cell in new[] { 36, 37, 54, 107, 108, 115 }) d.cells[cell] = Tile.Wall;
            d.FillBorder();
            return d;
        }

        public bool IsInterior(int x, int y) =>
            x > 0 && x < width - 1 && y > 0 && y < height - 1;

        public void FillBorder()
        {
            for (int x = 0; x < width; x++)
                cells[x] = cells[(height - 1) * width + x] = Tile.Wall;
            for (int y = 1; y < height - 1; y++)
                cells[y * width] = cells[y * width + width - 1] = Tile.Wall;
        }

        public bool Paint(int x, int y, Tile tile)
        {
            if (!IsInterior(x, y) || cells[y * width + x] == tile) return false;
            if (tile == Tile.Rover)
            {
                int old = Array.IndexOf(cells, Tile.Rover);
                if (old >= 0) cells[old] = Tile.Empty;
            }
            cells[y * width + x] = tile;
            return true;
        }

        public void Resize(int w, int h)
        {
            var resized = new Tile[w * h];
            for (int y = 1; y < Mathf.Min(h, height) - 1; y++)
                Array.Copy(cells, y * width + 1, resized, y * w + 1, Mathf.Min(w, width) - 2);
            width = w;
            height = h;
            cells = resized;
            FillBorder();
        }

        public bool Valid => width >= 3 && height >= 3 && width <= 99 && height <= 99
            && width % 2 == 1 && height % 2 == 1 && cells?.Length == width * height
            && Array.TrueForAll(cells, t => (uint)t <= (uint)Tile.Rover) && seconds > 0;
    }

    const string SaveKey = "PlanetRover.Workshop.Draft.v1";
    static readonly string[] Labels = { "Eraser", "Rock A", "Rock B", "Rocket A", "Rocket B", "Obstacle", "Rover" };
    static readonly string[] NumberFields = { "Width", "Height", "Seconds" };
    [SerializeField] GameObject rockA, rockB, rocket, wall, rover;
    [SerializeField] Texture2D[] icons;
    [SerializeField] Font hudFont;
    static readonly Color Hull = new Color32(40, 38, 36, 247);
    static readonly Color Rim = new Color32(139, 119, 95, 255);
    static readonly Color Slot = new Color32(61, 57, 52, 255);
    static readonly Color Amber = new Color32(255, 191, 100, 255);
    static readonly Color Cream = new Color32(250, 239, 215, 255);
    static readonly Color Muted = new Color32(206, 195, 178, 255);
    GUIStyle lettering;
    bool settingsOpen;
    string editingNumber, numberText, focusNumber;
    GUIStyle numberStyle;
    Rect editingRect;
    int numberKeyFrame = -1;
    public bool EditingNumber => editingNumber != null || focusNumber != null || numberKeyFrame == Time.frameCount;
    Draft draft;
    LobbyManager lobby;
    Room template;
    Mode mode;
    Tile brush = Tile.RockA;
    Rover liveRover;
    Camera playCamera;
    Rect originalViewport;
    readonly List<Tile[]> undo = new();
    float elapsed;
    int rockets, delivered;
    string message = "";
    bool painting;
    Tile[] beforeStroke;
    Vector2Int lastCell;

    static Draft LoadDraft()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            try
            {
                var saved = JsonUtility.FromJson<Draft>(PlayerPrefs.GetString(SaveKey));
                if (saved != null && saved.Valid) return saved;
            }
            catch (ArgumentException) { }
        }
        return Draft.Starter();
    }

    public void Open(LobbyManager owner, Room source, Draft layout, bool play)
    {
        lobby = owner;
        template = source;
        draft = layout ?? LoadDraft();
        draft.FillBorder();
        if (!play) return;

        mode = Mode.Play;
        playCamera = Camera.main;
        if (playCamera != null)
        {
            originalViewport = playCamera.rect;
            FitPlayCamera();
        }
        var room = GetComponent<Room>();
        var grid = room.Grid;
        var topLeft = new Vector2Int(-draft.width / 2, draft.height / 2);
        var bottomLeft = new Vector2Int(topLeft.x, -topLeft.y);
        grid.Bounds.SetCorners(grid.CellToWorld(bottomLeft) - new Vector3(.5f,.5f),
            grid.CellToWorld(-bottomLeft) + new Vector3(.5f,.5f));
        room.SetReflections(draft.mirrorX, draft.mirrorY);
        // Room.Start registers these objects and applies the room's startup reflections.
        for (int i = 0; i < draft.cells.Length; i++)
        {
            var tile = draft.cells[i];
            if (tile == Tile.Empty) continue;
            var cell = topLeft + new Vector2Int(i % draft.width, -i / draft.width);
            var obj = Instantiate(Prefab(tile), grid.CellToWorld(cell), Quaternion.identity, grid.transform);
            if (tile == Tile.Rover) liveRover = obj.GetComponent<Rover>();
            if (tile != Tile.RocketA && tile != Tile.RocketB) continue;
            var collector = obj.GetComponent<ItemCollector>();
            collector.RequiredItems.Clear();
            collector.RequiredItems.Add((tile == Tile.RocketA ? rockA : rockB).GetComponent<Item>().ItemData);
            collector.AllObjectsCollected += OnDelivery;
            rockets++;
        }
    }

    GameObject Prefab(Tile tile) => tile switch
    {
        Tile.Rover => rover, Tile.Wall => wall, Tile.RockA => rockA, Tile.RockB => rockB,
        Tile.RocketA or Tile.RocketB => rocket, _ => throw new ArgumentOutOfRangeException(nameof(tile))
    };

    void OnDelivery() => delivered++;

    float InterfaceScale => Mathf.Min(Screen.width / (Screen.width < Screen.height ? 540f : 900f), Screen.height / 520f);

    void FitPlayCamera()
    {
        if (playCamera == null || Screen.height == 0) return;
        var viewport = originalViewport;
        viewport.height -= 38f * InterfaceScale / Screen.height;
        playCamera.rect = viewport;
    }

    void OnDisable()
    {
        if (playCamera != null) playCamera.rect = originalViewport;
    }

    void LateUpdate()
    {
        if (mode != Mode.Edit) FitPlayCamera();
        if (mode != Mode.Play) return;
        elapsed += Time.deltaTime;
        bool expired = draft.countdown && elapsed >= draft.seconds;
        if (!expired && (rockets == 0 || delivered < rockets)) return;
        mode = Mode.Result;
        message = expired ? "Time is up" : $"Room complete in {elapsed:0.0} s";
        liveRover.enabled = false;
    }

    void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(draft));
        PlayerPrefs.Save();
    }

    public void Play()
    {
        if (draft == null) return;
        if (Array.IndexOf(draft.cells, Tile.Rover) < 0) { message = "Place a rover to playtest."; return; }
        Save();
        lobby.OpenWorkshop(template, draft, true);
    }

    public void Back()
    {
        if (draft == null) return;
        if (mode != Mode.Edit) { lobby.OpenWorkshop(template, draft, false); return; }
        Save();
        lobby.LeaveWorkshop();
    }

    static int AspectPartner(int size, bool editingWidth)
    {
        int best = 3;
        float error = float.PositiveInfinity;
        for (int i = 3; i <= 99; i += 2)
        {
            float ratio = editingWidth ? (float)size / i : (float)i / size;
            float delta = Mathf.Abs(ratio - 16f / 9f);
            if (delta < error) { best = i; error = delta; }
        }
        return best;
    }

    void Resize(bool horizontal, int step)
    {
        int w = draft.width, h = draft.height;
        if (horizontal) { w = Mathf.Clamp(w + step, 3, 99); if (draft.lockRatio) h = AspectPartner(w, true); }
        else { h = Mathf.Clamp(h + step, 3, 99); if (draft.lockRatio) w = AspectPartner(h, false); }
        if (w == draft.width && h == draft.height) return;
        draft.Resize(w, h);
        undo.Clear();
    }

    static void Fill(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    void OnGUI()
    {
        if (draft == null) return;
        float scale = InterfaceScale;
        if (scale <= 0) return;
        var previous = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
        float width = Screen.width / scale, height = Screen.height / scale;
        if (mode == Mode.Edit) DrawEditor(width, height);
        else DrawGame(width, height);
        GUI.matrix = previous;
    }

    void Text(Rect rect, string text, int size, Color color,
        TextAnchor anchor = TextAnchor.MiddleLeft, bool bold = false)
    {
        if (lettering == null)
            lettering = new GUIStyle { font = hudFont, clipping = TextClipping.Clip };
        lettering.fontSize = size;
        lettering.normal.textColor = color;
        lettering.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        lettering.alignment = anchor;
        GUI.Label(rect, text, lettering);
    }

    static Rect Inset(Rect r, float inset) =>
        new Rect(r.x + inset, r.y + inset, r.width - 2 * inset, r.height - 2 * inset);

    static void Plate(Rect rect, Color edge, Color face)
    {
        Fill(rect, edge);
        Fill(Inset(rect, 1), face);
    }

    bool Button(Rect r, string label, bool selected = false, bool enabled = true)
    {
        bool hover = enabled && r.Contains(Event.current.mousePosition);
        Color face = selected ? Amber : hover ? new Color32(89, 79, 66, 255) : Slot;
        Plate(r, selected || hover ? Amber : Rim, face);
        Text(r, label, 14, selected ? Hull : enabled ? Cream : Muted, TextAnchor.MiddleCenter, true);
        return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
    }

    void Stamp(Rect rect, Tile tile)
    {
        if (tile == Tile.Empty)
        {
            Text(rect, "×", 26, Cream, TextAnchor.MiddleCenter, true);
            return;
        }
        GUI.DrawTexture(rect, icons[(int)tile-1], ScaleMode.ScaleToFit);
        if (tile == Tile.RocketA || tile == Tile.RocketB)
        {
            float badge = rect.width * .46f;
            GUI.DrawTexture(new Rect(rect.xMax-badge, rect.yMax-badge, badge, badge),
                icons[tile == Tile.RocketA ? 0 : 1], ScaleMode.ScaleToFit);
        }
    }

    void DrawGame(float w, float h)
    {
        Fill(new Rect(0,0,w,38), Hull);
        GUI.DrawTexture(new Rect(10,3,30,30),icons[(int)Tile.RocketA-1],ScaleMode.ScaleToFit);
        Text(new Rect(48,2,200,31), $"{delivered} / {rockets} rockets", 16, Cream, bold:true);
        float remaining = Mathf.Max(0, draft.seconds - elapsed);
        Text(new Rect(w/2-100,2,200,31), draft.countdown ? $"{remaining:0.0} s left" : $"{elapsed:0.0} s",
            17, draft.countdown ? Amber : Cream, TextAnchor.MiddleCenter, true);
        Fill(new Rect(0,35,w,3), Rim);
        if (draft.countdown) Fill(new Rect(0,35,w * remaining / draft.seconds,3), Amber);
        if (Button(new Rect(w-194,3,82,29),"Restart")) Play();
        if (Button(new Rect(w-105,3,96,29),"Edit room")) Back();
        if (mode == Mode.Result)
        {
            var result = new Rect(w/2-155,h/2-31,310,62);
            Plate(result, Amber, Hull);
            Text(result, message, 21, Cream, TextAnchor.MiddleCenter, true);
        }
    }

    void DrawEditor(float w, float h)
    {
        Event e = Event.current;
        if (editingNumber != null)
        {
            if (e.type == EventType.MouseDown && !editingRect.Contains(e.mousePosition))
                FinishNumber(true);
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Tab)
            {
                int count = draft.countdown ? 3 : 2;
                int index = Array.IndexOf(NumberFields, editingNumber);
                focusNumber = NumberFields[(index + count + (e.shift ? -1 : 1)) % count];
                FinishNumber(true);
                e.Use();
            }
            else if (e.type == EventType.KeyDown &&
                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Escape))
            {
                numberKeyFrame = Time.frameCount;
                FinishNumber(e.keyCode != KeyCode.Escape);
                e.Use();
            }
        }
        if (!EditingNumber && e.type == EventType.KeyDown && e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha7)
        {
            brush = (Tile)(((int)e.keyCode - (int)KeyCode.Alpha1 + 1) % Labels.Length);
            e.Use();
        }

        // Leave the scene camera's terrain visible beneath the grid.
        Fill(new Rect(0,0,w,48), Hull);
        Fill(new Rect(0,47,w,1), Rim);
        Stamp(new Rect(12,4,38,38), Tile.Rover);
        Text(new Rect(59,4,174,39), "Room workshop", 22, Cream, bold:true);
        if (w >= 640) Text(new Rect(241,4,100,39), $"{draft.width} × {draft.height}", 15, Muted);
        if (Button(new Rect(w-282,7,94,33),"Room setup",settingsOpen)) settingsOpen = !settingsOpen;
        if (Button(new Rect(w-181,7,73,33),"Lobby")) Back();
        if (Button(new Rect(w-101,7,90,33),"Playtest",true)) Play();

        var area = new Rect(16,60,w-32-(settingsOpen ? 216 : 0),h-168);
        float size = Mathf.Min(area.width/draft.width, area.height/draft.height);
        var board = new Rect(area.center.x-draft.width*size/2,area.center.y-draft.height*size/2,
            draft.width*size,draft.height*size);
        DrawBoard(board,size);
        if (settingsOpen) DrawSettings(new Rect(w-212,60,196,286));

        float trayWidth = Mathf.Min(620,w-16);
        float slotPitch = (trayWidth-109)/7;
        var tray = new Rect((w-trayWidth)/2,h-92,trayWidth,62);
        Plate(tray, Rim, Hull);
        for (int i=0; i<Labels.Length; i++)
        {
            var tile = (Tile)((i+1) % Labels.Length);
            var slot = new Rect(tray.x+5+i*slotPitch,tray.y+4,slotPitch-3,54);
            bool selected = brush == tile;
            bool hover = slot.Contains(e.mousePosition);
            Plate(slot, selected ? Amber : hover ? Cream : Rim, selected ? new Color32(96,72,47,255) : Slot);
            Text(new Rect(slot.x+5,slot.y+1,13,16),(i+1).ToString(),11,selected ? Amber : Muted);
            Stamp(new Rect(slot.center.x-16,slot.y+2,32,32),tile);
            Text(new Rect(slot.x,slot.yMax-20,slot.width,18), Labels[(int)tile],13,Cream,TextAnchor.MiddleCenter);
            if (selected) Fill(new Rect(slot.x+1,slot.yMax-3,slot.width-2,2),Amber);
            if (GUI.Button(slot,GUIContent.none,GUIStyle.none)) brush = tile;
        }
        if (Button(new Rect(tray.xMax-95,tray.y+14,83,32),"Undo",enabled:undo.Count>0))
        {
            draft.cells = undo[undo.Count-1];
            undo.RemoveAt(undo.Count-1);
        }
        Fill(new Rect(0,h-25,w,25), Hull);
        Text(new Rect(12,h-25,w-24,25), message.Length>0 ? message :
            "Drag to paint    ·    Right-drag to erase    ·    1–7 select tools    ·    Enter to test    ·    Esc to return",
            13, message.Length>0 ? Amber : Cream, TextAnchor.MiddleCenter);
    }

    void DrawSettings(Rect r)
    {
        Plate(r, Rim, Hull);
        float x = r.x+10, y = r.y+10;
        Text(new Rect(x,y,170,28),"Room setup",18,Cream,bold:true);
        int dw = Stepper(x,y+35,"Width",draft.width);
        if (dw != 0) Resize(true,dw*2);
        int dh = Stepper(x,y+67,"Height",draft.height);
        if (dh != 0) Resize(false,dh*2);
        if (Switch(x,y+103,"Lock 16:9",draft.lockRatio))
        {
            draft.lockRatio = !draft.lockRatio;
            if (draft.lockRatio) Resize(true,0);
        }
        bool mirrorOnLaunch = draft.mirrorX || draft.mirrorY;
        if (Switch(x,y+138,"Mirror on launch",mirrorOnLaunch))
            draft.mirrorX = draft.mirrorY = !mirrorOnLaunch;
        if (Switch(x,y+177,"Time limit",draft.countdown)) draft.countdown = !draft.countdown;
        if (draft.countdown)
        {
            int delta = Stepper(x,y+211,"Seconds",draft.seconds);
            if (delta != 0) draft.seconds = Mathf.Clamp(draft.seconds+5*delta,1,3600);
        }
        Text(new Rect(x,y+247,178,24),"Saves on playtest or exit",13,Muted);
    }

    bool Switch(float x,float y,string label,bool value)
    {
        var rect = new Rect(x,y,176,28);
        Text(rect,label,14,Cream);
        var indicator = new Rect(rect.xMax-44,y+3,44,22);
        Plate(indicator,value ? Amber : Rim,value ? Amber : Slot);
        Text(indicator,value ? "ON" : "OFF",11,value ? Hull : Muted,TextAnchor.MiddleCenter,true);
        return GUI.Button(rect,GUIContent.none,GUIStyle.none);
    }

    void FinishNumber(bool commit)
    {
        if (commit && int.TryParse(numberText, out int value))
        {
            if (editingNumber == "Seconds") draft.seconds = Mathf.Clamp(value,1,3600);
            else
            {
                bool width = editingNumber == "Width";
                int dimension = Mathf.Clamp(value,3,99) | 1;
                Resize(width,dimension - (width ? draft.width : draft.height));
            }
        }
        editingNumber = null;
        GUI.FocusControl(null);
    }

    int Stepper(float x,float y,string label,int value)
    {
        Text(new Rect(x,y,70,28),label,14,Cream);
        var field = new Rect(x+58,y,48,28);
        bool active = editingNumber == label;
        Plate(field,active ? Amber : Rim,Hull);
        if (numberStyle == null)
        {
            numberStyle = new GUIStyle(GUI.skin.textField) {
                font = hudFont, fontSize = 15, alignment = TextAnchor.MiddleCenter
            };
            numberStyle.normal.textColor = numberStyle.focused.textColor = Cream;
        }
        GUI.SetNextControlName(label);
        string entered = GUI.TextField(Inset(field,2),active ? numberText : value.ToString(),12,numberStyle);
        if (focusNumber == label && Event.current.type != EventType.Used)
            GUI.FocusControl(label);
        if (GUI.GetNameOfFocusedControl() == label)
        {
            if (focusNumber == label) focusNumber = null;
            editingNumber = label;
            numberText = entered;
            editingRect = field;
        }
        if (Button(new Rect(x+110,y,30,28),"−")) return -1;
        return Button(new Rect(x+146,y,30,28),"+") ? 1 : 0;
    }

    void DrawBoard(Rect board, float size)
    {
        Fill(board,new Color(0.16f,.06f,.025f,.12f));
        Color line = new Color(1f,.89f,.72f,.35f);
        float stroke = Mathf.Max(1f,1f/InterfaceScale);
        for (int x=0;x<=draft.width;x++) Fill(new Rect(board.x+x*size,board.y,stroke,board.height),line);
        for (int y=0;y<=draft.height;y++) Fill(new Rect(board.x,board.y+y*size,board.width,stroke),line);
        float corner = Mathf.Min(size,12);
        for (int i=0; i<4; i++)
        {
            bool right = i%2 == 1, bottom = i>=2;
            Fill(new Rect(right ? board.xMax-corner : board.x,bottom ? board.yMax-stroke*2 : board.y,
                corner,stroke*2),Cream);
            Fill(new Rect(right ? board.xMax-stroke*2 : board.x,bottom ? board.yMax-corner : board.y,
                stroke*2,corner),Cream);
        }
        for (int i=0;i<draft.cells.Length;i++)
        {
            if (draft.cells[i] == Tile.Empty) continue;
            Stamp(new Rect(board.x+i%draft.width*size,board.y+i/draft.width*size,size,size),draft.cells[i]);
        }
        Event e = Event.current;
        if (e.rawType == EventType.MouseUp) { painting = false; beforeStroke = null; }
        if (!board.Contains(e.mousePosition)) return;
        var pos = new Vector2Int(Mathf.Min(draft.width-1,(int)((e.mousePosition.x-board.x)/size)),
            Mathf.Min(draft.height-1,(int)((e.mousePosition.y-board.y)/size)));
        if (e.type == EventType.Repaint && draft.IsInterior(pos.x,pos.y))
        {
            var cell = new Rect(board.x+pos.x*size,board.y+pos.y*size,size,size);
            Fill(cell,new Color(1f,.82f,.48f,.22f));
            Fill(new Rect(cell.x,cell.y,cell.width,2),Amber);
            if (draft.cells[pos.y*draft.width+pos.x] == Tile.Empty && brush != Tile.Empty)
            {
                var previous = GUI.color;
                GUI.color = new Color(1,1,1,.65f);
                Stamp(cell,brush);
                GUI.color = previous;
            }
        }
        if (e.button != 0 && e.button != 1) return;
        if (e.type == EventType.MouseDown)
        {
            painting = true;
            beforeStroke = (Tile[])draft.cells.Clone();
            lastCell = pos;
        }
        else if (e.type != EventType.MouseDrag || !painting) return;
        var tool = e.button == 1 ? Tile.Empty : brush;
        int steps = Mathf.Max(1,Mathf.Abs(pos.x-lastCell.x),Mathf.Abs(pos.y-lastCell.y));
        for (int j=1;j<=steps;j++)
        {
            var p = Vector2Int.RoundToInt(Vector2.Lerp(lastCell,pos,(float)j/steps));
            if (!draft.Paint(p.x,p.y,tool) || beforeStroke == null) continue;
            undo.Add(beforeStroke);
            if (undo.Count > 30) undo.RemoveAt(0);
            beforeStroke = null;
        }
        lastCell = pos;
        message = "";
        e.Use();
    }
}
