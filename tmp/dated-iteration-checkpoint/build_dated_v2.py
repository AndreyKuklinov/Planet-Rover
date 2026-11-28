from pathlib import Path
from collections import Counter, deque
import re,uuid,random
from dated_sketches_v2 import maps
D=Path('Assets/Prefabs/Levels/dated')
T=D/'26_11_27_000.prefab'
base,*blocks=re.split(r'(?=^--- !u!1001 )',T.read_text(),flags=re.M)
def value(block,key):
    return re.search(r'propertyPath: '+re.escape(key)+r'\n      value: ([^\n]+)',block)[1]
def pos(block,axis,v):
    return re.sub(r'(propertyPath: m_LocalPosition\.'+axis+r'\n      value: )[^\n]+',lambda m:m[1]+str(v),block)
tiles={'Obstacle':'#','Rover':'@',**{'Seed '+c:c.lower() for c in 'ABC'},**{'Planting Platform '+c:c for c in 'ABC'}}
templates={tiles[value(b,'m_Name')]:b for b in blocks}
def pack(rows,index):
    name=f'26_11_27_{index:03d}'
    root=base.replace(T.stem,name)
    w,h=len(rows[0]),len(rows)
    def vec(x,y):return '{x: '+str(x)+', y: '+str(y)+', z: 0}'
    for old,new in [(vec(6,4),vec(w//2+1,h//2+1)),(vec(-5,-3),vec(-(w//2),-(h//2)))]:
        assert root.count('m_LocalPosition: '+old)==1
        root=root.replace('m_LocalPosition: '+old,'m_LocalPosition: '+new)
    pieces=[];refs=[]
    for y,row in enumerate(rows):
        for x,char in enumerate(row):
            if char=='.':continue
            block=templates[char]
            oldInst=re.search(r'^--- !u!1001 &(\d+)',block)[1]
            oldTr=re.search(r'^--- !u!4 &(\d+) stripped',block,re.M)[1]
            inst=50000000+len(refs)*2;tr=inst+1
            block=block.replace(oldInst,str(inst)).replace(oldTr,str(tr))
            block=pos(pos(block,'x',x-w//2+.5),'y',h//2-y+.5)
            pieces.append(block);refs.append('  - {fileID: '+str(tr)+'}\n')
    root,n=re.subn(r'(--- !u!4 &8794407347372646130\nTransform:.*?  m_Children:\n)(?:  - \{fileID: \d+\}\n)+',lambda m:m[1]+''.join(refs),root,flags=re.S)
    assert n==1
    return root+''.join(pieces)

dirs=(1,-1,1j,-1j)
def solve(rows):
    floor={complex(x,y) for y,r in enumerate(rows) for x,c in enumerate(r) if c!='#'}
    objs={complex(x,y):c for y,r in enumerate(rows) for x,c in enumerate(r) if c not in '.#'}
    spawn=next(p for p,c in objs.items() if c=='@')
    seeds={p:c for p,c in objs.items() if c.islower()}
    beds={p:c for p,c in objs.items() if c in 'ABC'}
    assert not (Counter(c.lower() for c in beds.values())-Counter(seeds.values()))
    def reach(p,walk):
        visited={p};q=[p]
        for u in q:
            for d in dirs:
                v=u+d
                if v in walk and v not in visited:visited.add(v);q.append(v)
        return visited
    # Restrict witnesses to adjacent grabs and plants, keeping the pickup hand
    # fixed until a legal adjacent drop. A grown planter still occupies its cell.
    def search(p,seeds,goals):
        if not goals:return []
        space=floor-beds.keys()-seeds.keys()
        starts=reach(p,space)
        for s,color in seeds.items():
            rest=seeds.copy();rest.pop(s)
            free=space|{s}
            for grab in dirs:
                stand=s-grab
                if stand not in starts:continue
                carry=reach(stand,free)
                for g in goals:
                    if beds[g].lower()!=color:continue
                    if g-grab in carry:
                        solution=search(g-grab,rest,goals-{g})
                        if solution is not None:return [(color,'direct')]+solution
                    for nextdir in dirs:
                        if nextdir==grab or g-nextdir not in carry:continue
                        for drop in carry:
                            if drop-grab not in carry or drop-nextdir not in reach(drop-grab,free-{drop}):continue
                            solution=search(g-nextdir,rest,goals-{g})
                            if solution is not None:return [(color,'transfer')]+solution
        return None
    return search(spawn,seeds,set(beds))

def unpack(text):
    root,*instances=re.split(r'(?=^--- !u!1001 )',text,flags=re.M)
    positions={}
    for block in instances:
        char=tiles[value(block,'m_Name')]
        assert re.search(r'm_SourcePrefab: .*guid: (\w+)',block)[1]==re.search(r'm_SourcePrefab: .*guid: (\w+)',templates[char])[1]
        x=float(value(block,'m_LocalPosition.x'));y=float(value(block,'m_LocalPosition.y'))
        assert (x,y) not in positions
        positions[x,y]=char
    xs,ys=zip(*positions)
    rows=[''.join(positions.get((x+min(xs),max(ys)-y),'.') for x in range(int(max(xs)-min(xs))+1)) for y in range(int(max(ys)-min(ys))+1)]
    ids=re.findall(r'^--- !u!\d+ &(\d+)',text,re.M);assert len(ids)==len(set(ids))
    for key in ('isTimed','canUseReset','allowHorizontalReflection','allowVerticalReflection'):assert '  '+key+': 1' in root
    return rows

from PIL import Image,ImageDraw,ImageFont
W,H=1000,900
preview=Image.new('RGB',(W,H),'#19282e');draw=ImageDraw.Draw(preview)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',15)
out=[]
for i,(title,grid) in enumerate(maps,1):
    rows=grid.strip().splitlines();w=len(rows[0])+2
    assert w%2 and len(rows)%2 and all(len(r)==w-2 for r in rows),(i,rows)
    rows=['#'*w]+['#'+r+'#' for r in rows]+['#'*w]
    text=pack(rows,i);assert unpack(text)==rows
    for reflected in (rows,rows[::-1],[r[::-1] for r in rows],[r[::-1] for r in rows[::-1]]):
        assert solve(reflected) is not None,(i,title)
    print(i,title,w,len(rows))
    p=D/f'26_11_27_{i:03d}.prefab';assert p.exists() and Path(str(p)+'.meta').exists()
    out.append((p,text))
    x0=((i-1)%4)*250;y0=((i-1)//4)*180
    draw.text((x0+12,y0+7),f'{i:03d}   {title}',font=font,fill='#f4e8cc')
    step=min(14,216//w,135//len(rows));ox=x0+(250-w*step)//2;oy=y0+32+(138-len(rows)*step)//2
    for y,r in enumerate(rows):
        for x,c in enumerate(r):
            xx=ox+x*step;yy=oy+y*step
            draw.rectangle((xx,yy,xx+step-1,yy+step-1),fill='#758990' if c=='#' else '#33464b')
            if c in '.#':continue
            color={'a':'#ffae50','b':'#5acfc4','c':'#e995be','@':'#fff5d5'}[c.lower()]
            box=(xx+2,yy+2,xx+step-3,yy+step-3)
            if c.islower():draw.ellipse(box,fill=color)
            elif c=='@':draw.polygon([(xx+step//2,yy+1),(xx+step-2,yy+step//2),(xx+step//2,yy+step-2),(xx+1,yy+step//2)],fill=color)
            else:draw.rectangle(box,outline=color,width=2)
for p,text in out:
    if p.read_text()!=text:p.write_text(text)

preview.save('tmp/dated-26-11-27-sketches.png')
