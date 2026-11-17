from pathlib import Path
from PIL import Image,ImageDraw
import json
r=Path('Docs/ArtReferences/PetitPlanet')
stills=json.loads((r/'stills.json').read_text(encoding='utf-8'))
pc={48,50,52,54,56,58,60,74,76,80,82,90,92}
bb={132,134,136,138,140,142,144,146,148,149,152,154}
gr={193,195,199,207,209}
accepted=[];excluded=[]
for a in stills:
 n=int(a['file'].split('-')[1]);g=a['group']
 good=g=='famitsu' or (g=='pcgamer' and n in pc) or (g=='beebom' and n in bb) or (g=='gamesradar' and n in gr)
 a['kind']='screenshot';(accepted if good else excluded).append(a)
for label in ['review','official']:
 for a in json.loads((r/f'frames-{label}.json').read_text()):
  t=a['seconds'];good=(t<468 and t not in [28,44]) if label=='review' else t not in [8,12,28,108,116,120]
  a['kind']='video frame';(accepted if good else excluded).append(a)
(r/'selected.json').write_text(json.dumps(accepted,ensure_ascii=False,indent=2),encoding='utf-8')
(r/'excluded.json').write_text(json.dumps(excluded,ensure_ascii=False,indent=2),encoding='utf-8')
for start in range(0,len(accepted),12):
 b=Image.new('RGB',(1440,1180),'#eae9df');d=ImageDraw.Draw(b)
 for j,a in enumerate(accepted[start:start+12]):
  im=Image.open(r/a['file']);im.thumbnail((470,260));x=j%3*480;y=j//3*295;b.paste(im,(x+(470-im.width)//2,y));d.text((x+8,y+263),a['file'],fill='#333')
 b.save(r/f'selected-board-{start//12+1:02d}.jpg',quality=94)
print('Selected',len(accepted),'stills',sum(a['kind']=='screenshot' for a in accepted),'video frames',sum(a['kind']=='video frame' for a in accepted),'Excluded',len(excluded))
