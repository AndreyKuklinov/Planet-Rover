# Row and column coordinates include the border. Direction order up, left, down, right.
openings={
6:[[('grab',3,4,1),('grab',5,4,3),('grab',4,2,0),('grab',4,6,2)],
   [('plant',1,4,1),('plant',7,4,3),('plant',4,1,0),('plant',4,7,2)]],
}
openings.update({
21:[[('grab',2,4,1),('grab',8,4,3)],[('move',5,2,0)],[('plant',2,2,1),('plant',8,2,3)]],
23:[[('grab',4,2,3)],[('swap',5,2,3)],[('move',2,3,2)],[('move',6,3,3)],[('move',6,2,0)],[('plant',7,2,3)]],
24:[[('grab',4,2,0),('grab',3,4,1),('grab',5,4,3)]],
25:[[('grab',4,2,3)],[('dye',6,2,3)],[('plant',8,2,3)]],
26:[[('grab',3,3,3),('switch',1,3,1)],[('move',7,3,3)],[('move',7,2,0)],[('plant',8,2,3)]],
27:[[('grab',4,3,3)],[('wait',6,3,0)],[('move',8,3,3)],[('move',8,2,0)],[('plant',9,2,3)]],
29:[[('grab',4,3,1),('grab',8,3,3),('grab',6,4,2)]],
30:[[('move',5,5,2)],[('grab',5,4,0)],[('dye',5,2,0)],[('plant',5,1,0)]],
})
openings[26] += [[op] for op in [('move',7,3,2),('move',2,3,1),('move',2,4,2),('move',1,4,1),('move',1,5,2),('grab',2,5,3),('move',3,5,3),('move',3,3,0),('move',7,3,3),('move',7,4,2),('plant',8,4,3)]]
openings[30][:1]=[[('move',4,3,1)],[('move',4,5,2)],[('move',5,5,3)]]
