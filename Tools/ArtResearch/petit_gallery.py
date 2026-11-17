from pathlib import Path
import json,html
r=Path('Docs/ArtReferences/PetitPlanet')
items=json.loads((r/'selected.json').read_text(encoding='utf-8'))
head='''<!doctype html><html lang="ko"><meta charset="utf-8"><title>Petit Planet reference library</title><style>
*{box-sizing:border-box}body{margin:0;padding:36px;background:#f6f3df;color:#514a39;font:16px/1.6 system-ui}h1{font-size:32px}header{max-width:1100px;margin:auto auto 32px}a{color:#386c60}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:24px}figure{margin:0;background:#fffcef;border-radius:18px;overflow:hidden;border:1px solid #dfdac2}img{display:block;width:100%;aspect-ratio:16/9;object-fit:contain;background:#e8e9db}figcaption{padding:14px}small{display:block}nav{display:flex;gap:16px;flex-wrap:wrap}</style><header><h1>Petit Planet · 화면과 영상 레퍼런스</h1><p>선별 154장: 정지 화면 74장 + 영상 프레임 80장. 같은 UI의 다른 장면도 포함하며 UI 유형 154종을 뜻하지 않습니다. 각 이미지에서 원본 파일을 열고 출처 링크로 실제 영상의 해당 시점에 이동할 수 있습니다.</p><nav><a href="Review.md">요소별 적용 근거</a><a href="selected.json">출처 목록</a><a href="../../Petit-Combat.png">현재 게임 화면</a></nav></header><main>'''
cards=[]
for a in items:
 f=html.escape(a['file']);src=html.escape(a['source']);label=f
 if 'seconds' in a:label+=f" · {a['seconds']//60:02d}:{a['seconds']%60:02d}"
 cards.append(f'<figure><a href="{f}"><img loading="lazy" src="{f}"></a><figcaption>{label}<small><a href="{src}" target="_blank">원본 출처 / 영상 시점</a></small></figcaption></figure>')
(r/'index.html').write_text(head+'\n'.join(cards)+'</main></html>',encoding='utf-8')
print('Gallery: 154 source-linked images')
