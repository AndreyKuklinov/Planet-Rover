param([string]$SourcePath = 'Tools/AndreyRooms/reference.png')
Add-Type -AssemblyName System.Drawing
$source = [System.Drawing.Bitmap]::new((Join-Path (Get-Location) $SourcePath))
$rooms = @()
$xs = @(27,462,897)
$ys = @(69,318,567,814)
New-Item -ItemType Directory -Path tmp/andrey-grids -Force | Out-Null
for ($family=0;$family -lt 3;$family++) {
    for ($variant=0;$variant -lt 4;$variant++) {
        $number = 1 + $family*4 + $variant
        $left = $xs[$family]; $top = $ys[$variant]
        $rows = @()
        for ($y=0;$y -lt 12;$y++) {
            $line = ''
            for ($x=0;$x -lt 22;$x++) {
                $cx = [int][math]::Round($left+($x+0.5)*342/22)
                $cy = [int][math]::Round($top+($y+0.5)*186/12)
                $votes=0
                foreach ($dx in @(-2,0,2)) { foreach ($dy in @(-2,0,2)) {
                    $p=$source.GetPixel($cx+$dx,$cy+$dy)
                    $spread=[math]::Max($p.R,[math]::Max($p.G,$p.B))-[math]::Min($p.R,[math]::Min($p.G,$p.B))
                    if($p.R -ge 40 -and $p.R -le 150 -and $spread -le 15) { $votes++ }
                } }
                $line += $(if($votes -ge 5){'#'}else{'.'})
            }
            $rows += $line
        }
        $rooms += [ordered]@{number=$number;width=22;height=12;walls=$rows;objects=@()}
        $zoom=[System.Drawing.Bitmap]::new(735,430)
        $g=[System.Drawing.Graphics]::FromImage($zoom)
        $g.Clear([System.Drawing.Color]::FromArgb(30,30,30))
        $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $g.DrawImage($source,[System.Drawing.Rectangle]::new(40,42,684,372),$left,$top,342,186,[System.Drawing.GraphicsUnit]::Pixel)
        $font=[System.Drawing.Font]::new('Consolas',13,[System.Drawing.FontStyle]::Bold)
        $g.DrawString("Room $number",$font,[System.Drawing.Brushes]::White,8,0)
        for($x=0;$x -lt 22;$x++) {$g.DrawString("$x",$font,[System.Drawing.Brushes]::Yellow,[float](40+$x*684/22),23)}
        for($y=0;$y -lt 12;$y++) {$g.DrawString("$y",$font,[System.Drawing.Brushes]::Yellow,7,[float](43+$y*31))}
        $zoom.Save((Join-Path (Get-Location) ('tmp/andrey-grids/'+$number.ToString('00')+'.png')))
        $g.Dispose();$zoom.Dispose();$font.Dispose()
    }
}
$source.Dispose()
[ordered]@{source='reference.png';rooms=$rooms} | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath tmp/andrey-walls.json
