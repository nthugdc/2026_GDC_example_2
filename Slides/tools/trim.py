# 裁掉圖片下方的空白（以底部附近的背景色為準，忽略最後幾列邊框）
import sys, numpy as np
from PIL import Image
for f in sys.argv[1:]:
    im = Image.open(f).convert('RGB'); a = np.asarray(im).astype(int)
    bg = a[-30, a.shape[1] // 2]
    diff = (np.abs(a - bg).sum(axis=2) > 24).mean(axis=1) > 0.004
    diff[-6:] = False
    rows = np.where(diff)[0]
    if len(rows):
        im.crop((0, 0, im.width, min(im.height, rows[-1] + 24))).save(f)
