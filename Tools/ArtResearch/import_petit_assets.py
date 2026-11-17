from pathlib import Path
import shutil,zipfile,requests
from bs4 import BeautifulSoup
dest=Path('Assets/00_Co-Op/04_Images')
src=Path('C:/Users/tlrms/.codex/generated_images/01a1da3f-75eb-7ce3-bf0f-91ddec30d5d2')
for name,original in {
 'PetitGarden.png':'exec-c3aec4d6-0b27-4128-b3b7-a37e89828122.png',
 'PetitCast.png':'exec-1dced1dd-f247-4691-8601-32e49c650b91.png',
 'PetitPanels.png':'exec-1c02ce50-d922-4dd2-862b-e7c95dceff76.png',
}.items(): shutil.copy2(src/original,dest/name)
fonts=Path('Assets/00_Co-Op/05_Fonts');fonts.mkdir(exist_ok=True)
with zipfile.ZipFile('Docs/ArtReferences/PetitPlanet/naver-fonts.zip') as z:
 for f in z.namelist():
  if f.endswith(('NanumSquareRoundB.ttf','NanumSquareRoundEB.ttf')):
   (fonts/Path(f).name).write_bytes(z.read(f))
url='https://help.naver.com/service/30016/contents/18088?osType=PC&lang=ko'
s=BeautifulSoup(requests.get(url,timeout=30).text,'html.parser')
text=s.get_text('\n',strip=True)
(fonts/'Nanum-License.txt').write_text('Source: '+url+'\nOfficial font distribution: https://hangeul.naver.com/hangeul_static/webfont/zips/nanum-all_new.zip\n\n'+text,encoding='utf-8')
print('3 art files, 2 official font weights and license copied')
