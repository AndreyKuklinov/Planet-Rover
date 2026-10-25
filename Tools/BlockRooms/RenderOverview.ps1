Add-Type -AssemblyName System.Drawing
$blockFolder = Join-Path (Get-Location) 'Assets/Prefabs/Levels/showcase 2026/blocks'
$guidNames = @{}
$itemNames = @{}
foreach ($item in Get-ChildItem -LiteralPath 'Assets/Scriptable Objects/Items' -Filter '*.asset') {
    $itemGuid = (Select-String -LiteralPath ($item.FullName + '.meta') -Pattern '^guid: ').Line.Substring(6)
    $itemNames[$itemGuid] = $item.BaseName
}
$prefabSources = @(Get-ChildItem -LiteralPath 'Assets/Prefabs/Level Objects' -Filter '*.prefab') + @(Get-Item -LiteralPath 'Assets/Prefabs/Rover/Rover.prefab')
foreach ($source in $prefabSources) {
    $guidLine = Select-String -LiteralPath ($source.FullName + '.meta') -Pattern '^guid: '
    $guidNames[$guidLine.Line.Substring(6)] = $source.BaseName
}
$symbols = @{'Rover'='@';'Rock'='r';'Cog'='c';'Bucket'='b';'Liquid'='w';'Cloud'='C';'Flower'='f';'Sapling'='p';'Button'='L';'Door'='D';'Saw'='S';'Rocket'='R';'Timed'='T'}
$bitmap = [System.Drawing.Bitmap]::new(1350, 1490)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#f5f2eb'))
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$titleFont = [System.Drawing.Font]::new('Segoe UI',20,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$roomFont = [System.Drawing.Font]::new('Segoe UI',12,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$legendFont = [System.Drawing.Font]::new('Segoe UI',12,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
$symbolFont = [System.Drawing.Font]::new('Consolas',10,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$ink = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#202c38'))
$wall = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#354653'))
$floorBrush = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#e3e9e7'))
$orange = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#ef9b60'))
$blue = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#75b5df'))
$neutral = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#ccc5e2'))
$white = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
$gridPen = [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#cbd6d2'))
$graphics.DrawString('Planet Rover / Basic room blocks',$titleFont,$ink,20,12)
$graphics.DrawString('Authored prefab layouts: 35 rooms, 17 x 9 cells. Orange = A, blue = B, purple = neutral.',$legendFont,$ink,20,43)
$graphics.DrawString('@ rover   r rock   c cog   b bucket   w liquid   C cloud   p plant   f flower   L lever   D door   S saw   R rocket   T timer',$legendFont,$ink,20,63)
$index = 0
foreach ($file in Get-ChildItem -LiteralPath $blockFolder -Filter '*.prefab' | Sort-Object Name) {
    $baseX = 12 + ($index % 5) * 268
    $baseY = 97 + [math]::Floor($index / 5) * 198
    $graphics.FillRectangle($white,$baseX,$baseY,258,186)
    $titleRect = [System.Drawing.RectangleF]::new($baseX+8,$baseY+7,244,36)
    $graphics.DrawString($file.BaseName,$roomFont,$ink,$titleRect)
    $mapX = $baseX + 26
    $mapY = $baseY + 47
    for ($row=0;$row -lt 9;$row++) {
        for ($col=0;$col -lt 17;$col++) {
            $graphics.FillRectangle($floorBrush,$mapX+$col*12,$mapY+$row*12,12,12)
            $graphics.DrawRectangle($gridPen,$mapX+$col*12,$mapY+$row*12,12,12)
        }
    }
    $raw = [System.IO.File]::ReadAllText($file.FullName)
    $blocks = [regex]::Matches($raw,'(?ms)^--- !u!1001 &.*?(?=^--- !u!|\z)')
    $seen = @{}
    $requests = @()
    foreach ($instance in $blocks) {
        $sourceMatch = [regex]::Match($instance.Value,'m_SourcePrefab: \{fileID: 100100000, guid: ([a-f0-9]+)')
        if (-not $sourceMatch.Success) { continue }
        $name = $guidNames[$sourceMatch.Groups[1].Value]
        if (-not $name) { throw "Unknown source in $($file.Name)" }
        if ($name -eq 'Rocket') {
            foreach ($request in [regex]::Matches($instance.Value,'<RequiredItems>k__BackingField.Array.data\[\d+\]\s+value:\s+objectReference: \{fileID: 11400000, guid: ([a-f0-9]+)')) {
                $requests += $itemNames[$request.Groups[1].Value]
            }
        }
        $xMatch = [regex]::Match($instance.Value,'propertyPath: m_LocalPosition.x\s+value: ([-.0-9]+)')
        $yMatch = [regex]::Match($instance.Value,'propertyPath: m_LocalPosition.y\s+value: ([-.0-9]+)')
        if (-not $xMatch.Success -or -not $yMatch.Success) { throw 'Missing coordinates' }
        $col = [int][math]::Round([double]::Parse($xMatch.Groups[1].Value,[cultureinfo]::InvariantCulture) + 7.5)
        $row = [int][math]::Round(4.5 - [double]::Parse($yMatch.Groups[1].Value,[cultureinfo]::InvariantCulture))
        if ($col -lt 0 -or $col -ge 17 -or $row -lt 0 -or $row -ge 9) { throw "Out of range cell $col,$row" }
        if ($seen.ContainsKey("$col,$row")) { throw 'Duplicate object in drawing' }
        $seen["$col,$row"] = $true
        $px = $mapX + $col*12
        $py = $mapY + $row*12
        if ($name -eq 'Obstacle') { $graphics.FillRectangle($wall,$px,$py,12,12); continue }
        $brush = if ($name.EndsWith(' A')) {$orange} elseif ($name.EndsWith(' B')) {$blue} else {$neutral}
        if ($name -eq 'Rover') {$brush=$ink}
        if ($name -eq 'Rocket') {$brush=$white}
        $graphics.FillRectangle($brush,$px+1,$py+1,10,10)
        $symbol = $symbols[$name.Split(' ')[0]]
        if (-not $symbol) { throw "No symbol for $name" }
        $textBrush = if ($name -eq 'Rover') {$white} else {$ink}
        $graphics.DrawString($symbol,$symbolFont,$textBrush,$px+2,$py)
    }
    $requestText = ($requests | Group-Object | ForEach-Object { if ($_.Count -gt 1) { "$($_.Count)x $($_.Name)" } else { $_.Name } }) -join ', '
    $graphics.DrawString("Deliver: $requestText",$legendFont,$ink,$baseX+15,$baseY+161)
    $index++
}
if ($index -ne 35) { throw 'Expected 35 rooms' }
$imagePath = Join-Path $blockFolder 'overview.png'
$bitmap.Save($imagePath,[System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()
foreach ($resource in @($titleFont,$roomFont,$legendFont,$symbolFont,$ink,$wall,$floorBrush,$orange,$blue,$neutral,$white,$gridPen)) { $resource.Dispose() }
Write-Output $imagePath
