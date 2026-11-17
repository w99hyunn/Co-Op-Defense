import requests, json, hashlib, re, io, concurrent.futures
from pathlib import Path
from urllib.parse import urljoin
from bs4 import BeautifulSoup
from PIL import Image, ImageDraw

ROOT=Path('Docs/ArtReferences/PetitPlanet'); ROOT.mkdir(parents=True,exist_ok=True)
PAGES={
 'famitsu':'https://www.famitsu.com/article/202511/57109',
 'pcgamer':'https://www.pcgamer.com/games/life-sim/petit-planet-is-exactly-the-animal-crossing-clone-it-looked-like-but-after-15-hours-of-it-im-shocked-by-how-many-new-ideas-it-has-too/',
 'gamespot':'https://www.gamespot.com/articles/petit-planet-has-the-potential-to-take-animal-crossings-cozy-sim-crown/1100-6536422/',
 'beebom':'https://beebom.com/petit-planet-hands-on-impressions/',
 'gamesradar':'https://www.gamesradar.com/games/simulation/1-hour-in-genshin-impact-devs-cozy-life-sim-feels-like-cosmic-animal-crossing-but-that-might-not-be-a-bad-thing/',
 'official':'https://planet.hoyoverse.com/en-us/home?lang=en-us',
}
sess=requests.Session();sess.headers['User-Agent']='Mozilla/5.0'
items=[]; seen=set()
for name,url in PAGES.items():
 try:
  html=sess.get(url,timeout=35).text
  (ROOT/(name+'.html')).write_text(html,encoding='utf-8')
  soup=BeautifulSoup(html,'html.parser'); candidates=[]
  if name=='famitsu':
   candidates=re.findall(r'https://cimg.kgl-systems.io/camion/files/famitsu/57109/[^\s"<>?\\]+',html)
  else:
   scope=soup.select_one('article') or soup
   for img in scope.select('img'):
    for attr in ['data-original','data-src','src']:
     src=img.get(attr,'')
     if src.startswith('http'): candidates.append(src)
    ss=img.get('srcset','')
    if ss: candidates.append(ss.split(',')[-1].strip().split(' ')[0])
  for src in dict.fromkeys(candidates):
   if src in seen: continue
   seen.add(src);items.append({'source':url,'group':name,'image_url':src})
  print(name,len(candidates),flush=True)
 except Exception as e: print(name,str(e),flush=True)

def fetch(pair):
 i,item=pair
 try:
  data=sess.get(item['image_url'],timeout=30).content
  im=Image.open(io.BytesIO(data)).convert('RGB')
  if im.width<700 or im.height<380: return None
  item.update(width=im.width,height=im.height,sha256=hashlib.sha256(im.tobytes()).hexdigest())
  item['file']=f"still-{i:03d}-{item['group']}.jpg"
  im.save(ROOT/item['file'],quality=94)
  return item
 except Exception: return None
results=[];hashes=set()
with concurrent.futures.ThreadPoolExecutor(max_workers=8) as ex:
 for item in ex.map(fetch,enumerate(items)):
  if not item: continue
  if item['sha256'] in hashes: (ROOT/item['file']).unlink();continue
  hashes.add(item['sha256']);results.append(item)
(ROOT/'stills.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf-8')
print('Unique qualifying stills:',len(results))
for start in range(0,len(results),12):
 subset=results[start:start+12];board=Image.new('RGB',(1440,4*295),'#ecebe7');d=ImageDraw.Draw(board)
 for j,item in enumerate(subset):
  im=Image.open(ROOT/item['file']);im.thumbnail((470,260));x=j%3*480;y=j//3*295
  board.paste(im,(x+(470-im.width)//2,y));d.text((x+8,y+263),item['file'],fill='#333333')
 board.save(ROOT/f'contact-stills-{start//12+1:02d}.jpg',quality=92)
