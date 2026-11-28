from pathlib import Path
from collections import deque
import re,sys
from build_dated_iteration import unpack,D
from functools import lru_cache
dirs=[-1j,-1,1j,1]
def route(rows,opening=()):
 h=len(rows);w=len(rows[0]);index={complex(x,y):c for y,row in enumerate(rows) for x,c in enumerate(row) if c!='#'}
 floor={p for p,c in index.items() if c not in "tTxD"};p=next(p for p,c in index.items() if c=='@')
 seeds={p:c for p,c in index.items() if c in 'abc'}
 beds={p:c.lower() for p,c in index.items() if c in 'ABC'};goals=set(beds)
 cmds=[]
 def cmd(op,t,d):return f'{op},{int(t.real)-w//2},{h//2-int(t.imag)},{dirs.index(d)}'
 def walk(start,free):
  paths={start:[]};q=deque([start])
  while q:
   u=q.popleft()
   for d in dirs:
    v=u+d
    while v in free:
     if v not in paths:paths[v]=paths[u]+[cmd('move',v,d)];q.append(v)
     v+=d
  return paths
 def stances(t,d,free):
  v=t-d
  while v in free:yield v;v-=d
 def search(p,seeds,goals):
  if not goals:return []
  free=floor-beds.keys()-seeds.keys();paths=walk(p,free)
  options=[]
  for s,c in seeds.items():
   free2=free|{s}
   for d in dirs:
    for a in stances(s,d,free):
     if a not in paths:continue
     grab=paths[a]+[cmd('grab',s,d)]
     paths2=walk(a,free2)
     for g in goals:
      if beds[g]!=c:continue
      for b in stances(g,d,free2):
       if b not in paths2:continue
       act=grab+paths2[b]+[cmd('plant',g,d)]
       options.append((len(act),abs(s-a)+abs(g-b),act,s,g,b))
  for _,__,act,s,g,b in sorted(options,key=lambda o:(o[0],o[1])):
   tail=search(b,{q:c for q,c in seeds.items() if q!=s},goals-{g})
   if tail is not None:return act+tail
  for s,c in seeds.items():
   other={q:v for q,v in seeds.items() if q!=s}
   free2=free|{s}
   for di in dirs:
    for a in stances(s,di,free):
     if a not in paths:continue
     moved=walk(a,free2)
     for b,travel in moved.items():
      t=b+di
      if t not in free2 or t==s:continue
      later=walk(b,free2-{t})
      for dj in dirs:
       if dj==di:continue
       for grab2 in stances(t,dj,free2-{t}):
        if grab2 not in later:continue
        last=walk(grab2,free2)
        for g in goals:
         if beds[g]!=c:continue
         for end in stances(g,dj,free2):
          if end not in last:continue
          tail=search(end,other,goals-{g})
          if tail is not None:return paths[a]+[cmd('grab',s,di)]+travel+[cmd('drop',t,di)]+later[grab2]+[cmd('grab',t,dj)]+last[end]+[cmd('plant',g,dj)]+tail
  return None
 # Optional authored openings exercise multi-hand and handoff mechanics.
 hands={}
 for group in opening:
  simultaneous=[]
  for op,x,y,di in group:
   t=complex(x,y);d=dirs[di]
   simultaneous.append(cmd(op,t,d))
   if op=='grab':hands[di]=seeds.pop(t)
   elif op=='swap':hands[di],seeds[t]=seeds[t],hands[di]
   elif op=='dye':hands[di]='b';floor.add(t)
   elif op in ('switch','wait'):pass
   elif op=='plant':assert beds[t]==hands.pop(di);goals.remove(t)
   elif op=='drop':assert t==p+d;seeds[t]=hands.pop(di)
   elif op=='move':p=t
  cmds.append('+'.join(simultaneous))
 while hands:
  free=floor-beds.keys()-seeds.keys();paths=walk(p,free);options=[]
  for di,c in hands.items():
   for target in goals:
    if beds[target]!=c:continue
    for a in stances(target,dirs[di],free):
     if a in paths:options.append((len(paths[a]),abs(a-target),di,target,a))
  if not options:
   for di in hands:
    d=dirs[di]
    possible=[a for a in paths if a+d in free]
    if possible:break
   a=min(possible,key=lambda a:len(paths[a]));cmds+=paths[a]+[cmd('drop',a+d,d)];seeds[a+d]=hands.pop(di);p=a
   continue
  _,__,di,target,a=min(options,key=lambda o:o[:2])
  cmds+=paths[a]+[cmd('plant',target,dirs[di])];p=a;goals.remove(target);hands.pop(di)
 tail=search(p,seeds,goals);assert tail is not None
 return cmds+tail
if __name__=='__main__':
 from garden_openings import openings
 result=[]
 for i in range(1,int(sys.argv[1])+1):
  name=f'26_11_27_{i:03d}';rows=unpack((Path('Library/DatedGardenPlaytest')/D/(name+'.prefab')).read_text())
  acts=route(rows,openings.get(i,[]));print(i,len(acts),flush=True);result.append(name+'|'+';'.join(acts))
 Path('Library/DatedGardenPlaytest/routes.txt').write_text('\n'.join(result)+'\n')
