$path = 'Tools/AndreyRooms/rooms.json'
$pack = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
function Obj([string]$kind,[int]$x,[int]$y,[string[]]$requests=@()) {
    [ordered]@{prefab=$kind;x=$x;y=$y;requests=$requests}
}
function Room([int]$number,[string]$name,[object[]]$objects) {
    $room=$pack.rooms[$number-1]
    $room | Add-Member -NotePropertyName name -NotePropertyValue ($number.ToString('00')+' '+$name) -Force
    $room.objects=$objects
}
Room 1 'Bucket Collection' @(
    (Obj Rover 10 4),(Obj Bucket 2 4),(Obj Bucket 18 2),(Obj Bucket 16 9),
    (Obj Rocket 10 7 @('Bucket','Bucket','Bucket')))
Room 2 'First Flower' @(
    (Obj Rover 10 5),(Obj Bucket 4 5),(Obj 'Liquid A' 18 3),(Obj 'Sapling A' 18 8),(Obj Rocket 10 9 @('Flower A')))
Room 3 'Three Plant Route' @(
    (Obj Rover 9 5),(Obj Bucket 13 9),(Obj 'Liquid A' 9 1),(Obj 'Liquid A' 5 6),(Obj 'Liquid A' 17 2),
    (Obj 'Sapling A' 13 2),(Obj 'Sapling A' 9 8),(Obj 'Sapling A' 16 9),(Obj Rocket 17 5 @('Flower A','Flower A','Flower A')))
Room 4 'Two Flower Docks' @(
    (Obj Rover 9 5),(Obj Bucket 18 2),(Obj Bucket 12 9),
    (Obj 'Liquid A' 8 1),(Obj 'Liquid A' 8 5),(Obj 'Liquid A' 5 7),(Obj 'Liquid A' 5 10),(Obj 'Liquid A' 18 8),(Obj 'Liquid A' 18 10),
    (Obj 'Sapling A' 7 2),(Obj 'Sapling A' 11 1),(Obj 'Sapling A' 17 1),(Obj 'Sapling A' 11 4),(Obj 'Sapling A' 7 8),(Obj 'Sapling A' 17 10),
    (Obj Rocket 3 3 @('Flower A','Flower A','Flower A')),(Obj Rocket 3 8 @('Flower A','Flower A','Flower A')))
Room 5 'Bucket Rewind' @(
    (Obj Rover 10 5),(Obj Bucket 2 6),(Obj Bucket 17 5),(Obj Rocket 10 7 @('Bucket','Bucket','Bucket')))
Room 6 'Flower Rewind' @(
    (Obj Rover 10 5),(Obj Bucket 13 9),(Obj 'Liquid A' 4 3),(Obj 'Liquid A' 14 1),(Obj 'Liquid A' 17 10),
    (Obj 'Sapling A' 5 7),(Obj Rocket 16 5 @('Bucket','Flower A','Flower A')))
Room 7 'Restock Both Docks' @(
    (Obj Rover 10 5),(Obj Bucket 4 4),(Obj 'Liquid A' 9 3),(Obj 'Liquid A' 11 9),(Obj 'Sapling A' 18 1),(Obj 'Sapling A' 3 8),
    (Obj Rocket 18 6 @('Flower A','Flower A','Flower A')),(Obj Rocket 15 8 @('Bucket','Bucket','Bucket')))
Room 8 'Split Contracts' @(
    (Obj Rover 11 5),(Obj Bucket 12 3),(Obj Bucket 16 9),(Obj 'Liquid A' 2 7),(Obj 'Liquid A' 19 2),(Obj 'Sapling A' 8 2),(Obj 'Sapling A' 11 9),
    (Obj Rocket 3 3 @('Flower A','Bucket','Bucket')),(Obj Rocket 18 8 @('Bucket','Flower A','Flower A')))
Room 9 'Spend the Bucket' @(
    (Obj Rover 9 5),(Obj Bucket 5 5),(Obj 'Liquid A' 8 1),(Obj 'Liquid A' 14 2),(Obj 'Liquid A' 16 10),
    (Obj 'Sapling A' 10 1),(Obj 'Sapling A' 6 8),(Obj 'Sapling A' 12 7),
    (Obj Rocket 2 5 @('Bucket')),(Obj Rocket 16 6 @('Flower A','Flower A','Flower A')))
Room 10 'Bucket in the Garden' @(
    (Obj Rover 10 5),(Obj Bucket 18 3),(Obj 'Liquid A' 4 4),(Obj 'Liquid A' 7 5),(Obj 'Liquid A' 4 6),
    (Obj 'Sapling A' 3 5),(Obj 'Sapling A' 6 4),(Obj 'Sapling A' 5 6),
    (Obj Rocket 5 5 @('Bucket')),(Obj Rocket 13 8 @('Flower A','Flower A','Flower A')))
Room 11 'Bucket Depot' @(
    (Obj Rover 10 5),(Obj Bucket 1 1),(Obj Bucket 4 1),(Obj Bucket 2 2),(Obj Bucket 3 2),(Obj Bucket 2 3),(Obj Bucket 4 3),(Obj Bucket 1 4),(Obj Bucket 1 5),
    (Obj 'Liquid A' 11 3),(Obj 'Liquid A' 20 3),(Obj 'Liquid A' 16 7),(Obj 'Sapling A' 18 1),(Obj 'Sapling A' 12 4),(Obj 'Sapling A' 13 8),
    (Obj Rocket 9 3 @('Bucket')),(Obj Rocket 19 3 @('Bucket')),(Obj Rocket 16 5 @('Flower A','Flower A','Flower A')),
    (Obj Rocket 14 7 @('Bucket')),(Obj Rocket 10 10 @('Bucket')))
Room 12 'Nursery Warehouse' @(
    (Obj Rover 10 5),(Obj Bucket 2 2),(Obj Bucket 14 1),(Obj Bucket 10 10),(Obj Bucket 19 7),
    (Obj 'Liquid A' 17 1),(Obj 'Liquid A' 19 1),(Obj 'Liquid A' 20 1),(Obj 'Liquid A' 18 2),(Obj 'Liquid A' 19 2),
    (Obj 'Liquid A' 17 3),(Obj 'Liquid A' 18 3),(Obj 'Liquid A' 20 3),(Obj 'Liquid A' 19 4),
    (Obj 'Sapling A' 8 1),(Obj 'Sapling A' 9 1),(Obj 'Sapling A' 8 2),(Obj 'Sapling A' 9 2),
    (Obj 'Sapling A' 4 6),(Obj 'Sapling A' 5 6),(Obj 'Sapling A' 4 7),(Obj 'Sapling A' 5 7),
    (Obj 'Sapling A' 16 6),(Obj 'Sapling A' 17 6),(Obj 'Sapling A' 16 7),(Obj 'Sapling A' 17 7),
    (Obj Rocket 8 7 @('Flower A','Flower A','Flower A')),(Obj Rocket 12 3 @('Flower A','Flower A','Flower A')),
    (Obj Rocket 13 9 @('Flower A','Flower A','Flower A')),(Obj Rocket 15 5 @('Bucket')),(Obj Rocket 6 10 @('Bucket')))
$pack | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $path
