param([string]$OutputDirectory='Assets/Prefabs/Levels/showcase 2026/andrey')
Add-Type -AssemblyName System.Drawing
$pack=Get-Content -LiteralPath Tools/AndreyRooms/rooms.json -Raw | ConvertFrom-Json
$canvas=[System.Drawing.Bitmap]::new(1020,1110)
$g=[System.Drawing.Graphics]::FromImage($canvas)
$g.Clear([System.Drawing.ColorTranslator]::FromHtml('#f4f1e9'))
$g.TextRenderingHint=[System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$title=[System.Drawing.Font]::new('Segoe UI',20,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$font=[System.Drawing.Font]::new('Segoe UI',12,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$small=[System.Drawing.Font]::new('Segoe UI',11,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
$symbolFont=[System.Drawing.Font]::new('Consolas',11,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$ink=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#263541'))
$wall=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#4b565b'))
$floor=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#e2e8e4'))
$orange=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#f49b63'))
$bucket=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#bfcbe4'))
$white=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
$pen=[System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#cbd5d0'))
$g.DrawString("Andrey's rooms / source-image transcriptions",$title,$ink,18,12)
$g.DrawString('22 x 12 cells including walls / provisional StandardTime 60 seconds / startup reflections enabled',$small,$ink,18,42)
$g.DrawString('@ rover    b empty bucket    w orange liquid    p orange sapling    R rocket / requests listed below each map',$small,$ink,18,60)
$families=@('01-04  Buckets and sprouts','05-08  Learning to reset','09-12  Accidental bucket loss')
for($family=0;$family -lt 3;$family++) {$g.DrawString($families[$family],$font,$ink,18+$family*336,87)}
foreach($room in $pack.rooms) {
    $family=[int][math]::Floor(($room.number-1)/4)
    $row=($room.number-1)%4
    $bx=12+$family*336;$by=110+$row*246
    $g.FillRectangle($white,$bx,$by,324,234)
    $g.DrawString($room.name,$font,$ink,$bx+8,$by+7)
    $mx=$bx+18;$my=$by+32
    for($y=0;$y -lt 12;$y++) {for($x=0;$x -lt 22;$x++) {
        $px=$mx+$x*13;$py=$my+$y*13
        if($room.walls[$y][$x] -eq '#') {$g.FillRectangle($wall,$px,$py,13,13)} else {
            $g.FillRectangle($floor,$px,$py,13,13);$g.DrawRectangle($pen,$px,$py,13,13)
        }
    }}
    foreach($obj in $room.objects) {
        $px=$mx+$obj.x*13;$py=$my+$obj.y*13
        $color=$orange;$symbol='?';$letter=$ink
        switch($obj.prefab) {
            'Rover' {$symbol='@';$color=$ink;$letter=$white}
            'Bucket' {$symbol='b';$color=$bucket}
            'Liquid A' {$symbol='w'}
            'Sapling A' {$symbol='p'}
            'Rocket' {$symbol='R';$color=$white}
        }
        $g.FillRectangle($color,$px+1,$py+1,11,11)
        $g.DrawString($symbol,$symbolFont,$letter,$px+2,$py)
    }
    $requests=@($room.objects | Where-Object prefab -eq Rocket | ForEach-Object {
        ($_.requests | Group-Object | ForEach-Object { "$($_.Count)x $($_.Name.Replace('Flower A','flower').Replace('Bucket','bucket'))" }) -join ' + '
    }) -join '; '
    $rect=[System.Drawing.RectangleF]::new($bx+10,$by+192,306,39)
    $g.DrawString("Rockets: $requests",$small,$ink,$rect)
}
$path=Join-Path (Get-Location) (Join-Path $OutputDirectory overview.png)
$canvas.Save($path,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$canvas.Dispose()
foreach($item in @($title,$font,$small,$symbolFont,$ink,$wall,$floor,$orange,$bucket,$white,$pen)) {$item.Dispose()}
Write-Output $path
