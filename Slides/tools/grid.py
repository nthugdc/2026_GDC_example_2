# 在截圖上畫每 100 px 的格線與座標（讀標註框座標用）：grid.py 輸出 輸入 [縮放]
import sys
from PIL import Image, ImageDraw
out, f = sys.argv[1], sys.argv[2]
s = float(sys.argv[3]) if len(sys.argv) > 3 else 0.5
im = Image.open(f).convert('RGB'); W, H = im.size
im = im.resize((int(W * s), int(H * s))); d = ImageDraw.Draw(im)
for x in range(0, W, 100):
    d.line([(x * s, 0), (x * s, H * s)], fill=(255, 0, 0) if x % 500 == 0 else (120, 0, 0)); d.text((x * s + 2, 2), str(x), fill=(255, 255, 0))
for y in range(0, H, 100):
    d.line([(0, y * s), (W * s, y * s)], fill=(255, 0, 0) if y % 500 == 0 else (120, 0, 0)); d.text((2, y * s + 2), str(y), fill=(255, 255, 0))
im.save(out)
