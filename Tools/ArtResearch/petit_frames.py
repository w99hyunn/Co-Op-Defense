import subprocess,json,sys
from pathlib import Path
from PIL import Image,ImageDraw
import imageio_ffmpeg
root=Path('Docs/ArtReferences/PetitPlanet')
name=sys.argv[1] if len(sys.argv)>1 else 'review'
meta=json.loads((root/('video-probe.json' if name=='review' else 'video-'+name+'.json')).read_text(encoding='utf-8-sig'))
times=list(range(4,int(meta['duration'])-3,8 if name=='review' else 4))
frames=[]
for i,t in enumerate(times):
 f=f'{name}-{t:04d}s.jpg'
 subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-hide_banner','-loglevel','error','-ss',str(t),'-i',str(root/f'video-{name}.mp4'),'-frames:v','1','-q:v','2','-y',str(root/f)],check=True)
 frames.append({'file':f,'seconds':t,'source':meta['webpage_url']+f'&t={t}s','title':meta['title']})
(root/f'frames-{name}.json').write_text(json.dumps(frames,indent=2),encoding='utf-8')
for start in range(0,len(frames),12):
 board=Image.new('RGB',(1440,4*295),'#edece5');d=ImageDraw.Draw(board)
 for j,item in enumerate(frames[start:start+12]):
  im=Image.open(root/item['file']);im.thumbnail((470,260));x=j%3*480;y=j//3*295
  board.paste(im,(x+(470-im.width)//2,y));d.text((x+8,y+263),item['file'],fill='#333333')
 board.save(root/f'contact-{name}-{start//12+1:02d}.jpg',quality=93)
print(name,len(frames))
