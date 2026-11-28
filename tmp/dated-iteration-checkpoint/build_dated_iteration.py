from pathlib import Path
import sys
from dated_v3_maps import maps,changes
# Reuse the same serialization and round-trip checks as the previous dated batch.
exec(Path('Library/build_dated_v2.py').read_text().split('from PIL')[0].replace('from dated_sketches_v2 import maps',''))
tiles.update({'Cloud B':'x','Button A':'t','Door A':'D','Timed Emitter':'T'})
source=Path('Assets/Prefabs/Levels/deprecated/showcase 2026/slop/blocks')
for n in [16,24,28]:
 text=next(source.glob(f'{n} *.prefab')).read_text()
 for b in re.split(r'(?=^--- !u!1001 )',text,flags=re.M)[1:]:
  name=value(b,'m_Name')
  if name in ('Cloud B','Button A','Door A','Timed Emitter'):
   templates[tiles[name]]=re.sub(r'm_TransformParent: \{fileID: \d+\}', 'm_TransformParent: {fileID: 8794407347372646130}', b)
def build(destination):
 from dated_sketches_v2 import maps as old
 for i,(title,g) in enumerate(maps,1):
  rows=g.strip().splitlines(); w=len(rows[0])+2
  assert w%2 and len(rows)%2 and all(len(r)==w-2 for r in rows),(i,rows)
  rows=['#'*w]+['#'+a+'#' for a in rows]+['#'*w]
  dest=destination/f'26_11_27_{i:03d}.prefab';data=pack(rows,i)
  assert unpack(data)==rows
  if i<=20:
   r=old[i-1][1].strip().splitlines();ww=len(r[0])+2;previous=pack(['#'*ww]+['#'+z+'#' for z in r]+['#'*ww],i)
   assert dest.read_text() in (previous,data),('unexpected existing edit',dest)
   if i not in changes:continue
  elif dest.exists() and dest.read_text()!=data:raise Exception('Occupied slot: '+str(dest))
  dest.write_text(data)
  met=Path(str(dest)+'.meta')
  if not met.exists():
   # Use the same identity in the test copy and authors' project.
   met.write_text(re.sub(r'(?m)^guid: \w+', 'guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'PlanetRover/dated-iteration2/'+dest.stem).hex,Path(str(T)+'.meta').read_text()))
if __name__=='__main__':build(Path(sys.argv[1]))
