"""
產生社課簡報（深色、16:9）。

  完整版  Slides/NTHU_GDC_Tilemap_Animation.pptx
  本週版  Slides/NTHU_GDC_Animation_Week2.pptx（第二堂：從動畫開始）

Tilemap 的頁面（原檔第 5～33 頁：介面導覽 ~ 第 3 章）直接從現有的完整版複製，不重新產生，
所以第一堂的內容永遠不會被動到。動畫部分（第 4～8 章）由這支腳本產生。

用法（在 Slides/tools 底下）：
    .venv/bin/python build_slides.py
需要的截圖在 .cache/shots 與 .cache/game（由 shot.sh、game.sh 產生，見 README.md）。
"""
import copy, io, math, os, sys
from PIL import Image, ImageDraw, ImageFont
from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE, MSO_CONNECTOR
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.oxml.ns import qn

HERE = os.path.dirname(os.path.abspath(__file__))
PROJ = os.path.abspath(os.path.join(HERE, '..', '..'))
ART = PROJ + '/Assets/_GDC/Art'
SHOTS = HERE + '/.cache/shots'
GAME = HERE + '/.cache/game'
ANN = HERE + '/.cache/ann'
LOGO = HERE + '/logo.png'
FULL = PROJ + '/Slides/NTHU_GDC_Tilemap_Animation.pptx'
WEEK2 = PROJ + '/Slides/NTHU_GDC_Animation_Week2.pptx'
os.makedirs(ANN, exist_ok=True)

BOLD_TTC = '/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc'
FONT = 'Noto Sans TC'
MONO = 'Noto Sans Mono'
BG = RGBColor(0x1B, 0x21, 0x2C)
PANEL = RGBColor(0x26, 0x30, 0x3E)
CODEBG = RGBColor(0x12, 0x17, 0x1F)
WHITE = RGBColor(0xF5, 0xF7, 0xFA)
DIM = RGBColor(0x8A, 0x97, 0xA6)
MINT = RGBColor(0x06, 0xD6, 0xA0)
YEL = RGBColor(0xFF, 0xD1, 0x66)
RED = RGBColor(0xEF, 0x47, 0x6F)
BLUE = RGBColor(0x3A, 0x86, 0xFF)
ORANGE = RGBColor(0xE0, 0x7A, 0x1F)
FOOTER = 'Unity 2D 社課 · 磁磚與動畫工作坊'
SW, SH = 13.333, 7.5


def shot(n): return f'{SHOTS}/{n}.png'
def game(n): return f'{GAME}/{n}.png'
def tile(n): return f'{ART}/Tiles/{n}.png'


# ══════════════════════════════════════════════════════════════════════════
# 圖片標註
# ══════════════════════════════════════════════════════════════════════════

def ann(src, out, boxes=(), crop=None, scale=None, inside=False):
    """在截圖上畫黃框與綠色編號。boxes：(x0, y0, x1, y1, 編號或 None)，座標是原圖像素。
    一般圖：編號放在左邊加出來的一條底色上；橫條圖（寬高比 > 3）：放上方；inside=True：放框內左上角。"""
    im = Image.open(src).convert('RGB')
    ox = oy = 0
    if crop:
        im = im.crop(crop); ox, oy = crop[0], crop[1]
    if scale:
        im = im.resize((int(im.width * scale), int(im.height * scale)), Image.LANCZOS)
    s = scale or 1
    wide = im.width / im.height > 3
    base = min(im.width, im.height)
    lw = max(3, int(base / 160)) if not wide else max(3, int(im.width / 450))
    r = max(14, int(base / 26)) if not wide else max(14, int(im.width / 80))
    if inside: r = int(r * 0.6)
    f = ImageFont.truetype(BOLD_TTC, int(r * 1.3))
    numbered = [b for b in boxes if len(b) > 4 and b[4] is not None]
    pad = int(r * 2.6) if numbered and not inside else 0
    if pad:
        size = (im.width, im.height + pad) if wide else (im.width + pad, im.height)
        canvas = Image.new('RGB', size, (27, 33, 44))
        canvas.paste(im, (0, pad) if wide else (pad, 0)); im = canvas
    d = ImageDraw.Draw(im)
    px, py = (0, pad) if wide else (pad, 0)
    for b in boxes:
        x0, y0, x1, y1 = [(v - o) * s for v, o in zip(b[:4], (ox, oy, ox, oy))]
        x0 += px; x1 += px; y0 += py; y1 += py
        d.rounded_rectangle([x0, y0, x1, y1], radius=lw * 2, outline=(255, 209, 102), width=lw)
        if len(b) > 4 and b[4] is not None:
            if inside:
                cx, cy = x0 + r + lw * 2, y0 + r + lw * 2
            elif wide:
                cx, cy = min(max(r, (x0 + x1) / 2), im.width - r), pad / 2
                d.line([(cx, cy + r), (cx, y0)], fill=(255, 209, 102), width=max(2, lw // 2))
            else:
                cx, cy = pad / 2, min(max(r, (y0 + y1) / 2), im.height - r)
                d.line([(cx + r, cy), (x0, cy)], fill=(255, 209, 102), width=max(2, lw // 2))
            d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(6, 214, 160))
            d.text((cx, cy - r * 0.08), str(b[4]), fill=(27, 33, 44), font=f, anchor='mm')
    path = f'{ANN}/{out}.png'
    im.save(path)
    return path


def crop_img(src, out, box, scale=None):
    im = Image.open(src).convert('RGB').crop(box)
    if scale: im = im.resize((int(im.width * scale), int(im.height * scale)), Image.LANCZOS)
    p = f'{ANN}/{out}.png'; im.save(p); return p


# ══════════════════════════════════════════════════════════════════════════
# 投影片元件
# ══════════════════════════════════════════════════════════════════════════

class Deck:
    def __init__(self):
        self.prs = Presentation()
        self.prs.slide_width, self.prs.slide_height = Inches(SW), Inches(SH)
        self.page = 0

    @property
    def blank(self): return self.prs.slide_layouts[6]


def set_font(run, size, color=WHITE, bold=False, mono=False):
    run.font.size = Pt(size); run.font.bold = bold; run.font.color.rgb = color
    name = MONO if mono else FONT
    run.font.name = name
    rPr = run._r.get_or_add_rPr()
    for tag in ('a:ea', 'a:cs'):
        el = rPr.find(qn(tag))
        if el is None:
            el = rPr.makeelement(qn(tag), {}); rPr.append(el)
        el.set('typeface', name)
    rPr.set('lang', 'zh-TW')


def text(sl, x, y, w, h, content, size=20, color=WHITE, bold=False, align=PP_ALIGN.LEFT,
         anchor=MSO_ANCHOR.TOP, mono=False, spacing=1.15):
    """content：字串（\\n 換行）或段落清單；段落可以是 [(文字, 顏色, 粗體), ...]。"""
    tb = sl.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    tf = tb.text_frame; tf.word_wrap = True; tf.vertical_anchor = anchor
    tf.margin_left = tf.margin_right = Inches(0.05); tf.margin_top = tf.margin_bottom = Inches(0.03)
    paras = content.split('\n') if isinstance(content, str) else content
    for i, para in enumerate(paras):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.alignment = align; p.line_spacing = spacing
        runs = [(para, color, bold)] if isinstance(para, str) else para
        for t, c, b in runs:
            r = p.add_run(); r.text = t; set_font(r, size, c, b, mono)
    return tb


def rect(sl, x, y, w, h, fill=PANEL, line=None, shape=MSO_SHAPE.ROUNDED_RECTANGLE, radius=0.08, lw=1.5):
    s = sl.shapes.add_shape(shape, Inches(x), Inches(y), Inches(w), Inches(h))
    if fill is None: s.fill.background()
    else: s.fill.solid(); s.fill.fore_color.rgb = fill
    if line is None: s.line.fill.background()
    else: s.line.color.rgb = line; s.line.width = Pt(lw)
    if shape == MSO_SHAPE.ROUNDED_RECTANGLE:
        s.adjustments[0] = radius
    s.shadow.inherit = False
    return s


def box(sl, x, y, w, h, label, fill=PANEL, color=WHITE, size=18, line=None, bold=True, sub=None):
    """有文字的方塊（畫流程圖用）。"""
    rect(sl, x, y, w, h, fill=fill, line=line)
    if sub:
        text(sl, x, y + 0.05, w, h * 0.55, label, size, color, bold, PP_ALIGN.CENTER, MSO_ANCHOR.BOTTOM)
        text(sl, x, y + h * 0.55, w, h * 0.45, sub, size - 5, DIM if fill != MINT else BG, False, PP_ALIGN.CENTER)
    else:
        text(sl, x, y, w, h, label, size, color, bold, PP_ALIGN.CENTER, MSO_ANCHOR.MIDDLE)


def arrow(sl, x1, y1, x2, y2, color=YEL, width=2.5, label=None, lsize=13, lcolor=None, loff=(0, -0.32)):
    c = sl.shapes.add_connector(MSO_CONNECTOR.STRAIGHT, Inches(x1), Inches(y1), Inches(x2), Inches(y2))
    c.line.color.rgb = color; c.line.width = Pt(width)
    ln = c.line._get_or_add_ln()
    tail = ln.makeelement(qn('a:tailEnd'), {'type': 'triangle', 'w': 'med', 'len': 'med'}); ln.append(tail)
    if label:
        mx, my = (x1 + x2) / 2 + loff[0], (y1 + y2) / 2 + loff[1]
        text(sl, mx - 1.3, my, 2.6, 0.32, label, lsize, lcolor or color, True, PP_ALIGN.CENTER)
    return c


def badge(sl, x, y, n, d=0.46, fill=MINT, color=BG):
    c = sl.shapes.add_shape(MSO_SHAPE.OVAL, Inches(x), Inches(y), Inches(d), Inches(d))
    c.fill.solid(); c.fill.fore_color.rgb = fill; c.line.fill.background(); c.shadow.inherit = False
    tf = c.text_frame; tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    p = tf.paragraphs[0]; p.alignment = PP_ALIGN.CENTER
    r = p.add_run(); r.text = str(n); set_font(r, int(d * 40), color, True)


def image(sl, path, x, y, w, h, border=True, align='center'):
    """把圖片等比縮放塞進 (x, y, w, h)。"""
    im = Image.open(path); ar = im.width / im.height
    if w / h > ar: iw, ih = h * ar, h
    else: iw, ih = w, w / ar
    ix = x + (w - iw) / 2 if align == 'center' else x
    iy = y + (h - ih) / 2
    if border:
        rect(sl, ix - 0.04, iy - 0.04, iw + 0.08, ih + 0.08, fill=RGBColor(0x3A, 0x47, 0x58), shape=MSO_SHAPE.RECTANGLE)
    sl.shapes.add_picture(path, Inches(ix), Inches(iy), Inches(iw), Inches(ih))
    return ix, iy, iw, ih


def footer(dk, sl):
    sl.shapes.add_picture(LOGO, Inches(0.55), Inches(7.0), height=Inches(0.3))
    text(sl, 1.2, 6.98, 6, 0.35, FOOTER, 11, DIM)
    text(sl, 12.0, 6.98, 0.8, 0.35, str(dk.page), 11, DIM, align=PP_ALIGN.RIGHT)


def new(dk, title=None, chapter=None, sub=None):
    sl = dk.prs.slides.add_slide(dk.blank)
    bg = sl.background.fill; bg.solid(); bg.fore_color.rgb = BG
    dk.page += 1
    if title:
        rect(sl, 0.55, 0.52, 0.09, 0.55, fill=MINT, shape=MSO_SHAPE.RECTANGLE)
        text(sl, 0.8, 0.38, 10.5, 0.8, title, 30, WHITE, True, anchor=MSO_ANCHOR.MIDDLE)
        if sub:
            text(sl, 0.82, 1.08, 11, 0.5, sub, 16, DIM)
    if chapter is not None:
        rect(sl, 11.35, 0.5, 1.45, 0.48, fill=PANEL)
        text(sl, 11.35, 0.5, 1.45, 0.48, chapter, 14, MINT, True, PP_ALIGN.CENTER, MSO_ANCHOR.MIDDLE)
    footer(dk, sl)
    return sl


def bullets(sl, x, y, w, items, size=20, gap=0.12, color=WHITE, num=False, start=1):
    """條列。items：字串或 (主文, 說明)。num=True 用編號圓圈。"""
    cy = y
    for i, it in enumerate(items):
        main, note = (it, None) if isinstance(it, str) else it
        if num:
            badge(sl, x, cy + 0.02, i + start, 0.42); tx = x + 0.6
        else:
            rect(sl, x + 0.05, cy + 0.17, 0.13, 0.13, fill=MINT, shape=MSO_SHAPE.OVAL); tx = x + 0.35
        lines = main.count('\n') + 1
        h = lines * size / 72 * 1.35 + 0.05
        text(sl, tx, cy, w - (tx - x), h, main, size, color)
        cy += h
        if note:
            nh = (note.count('\n') + 1) * (size - 4) / 72 * 1.35 + 0.05
            text(sl, tx, cy - 0.02, w - (tx - x), nh, note, size - 4, DIM)
            cy += nh
        cy += gap
    return cy


def code(sl, x, y, w, h, src, size=13):
    rect(sl, x, y, w, h, fill=CODEBG, radius=0.04)
    paras = []
    for line in src.strip('\n').split('\n'):
        c = YEL if 'TODO' in line else (DIM if line.strip().startswith('//') else WHITE)
        paras.append([(line if line else ' ', c, False)])
    text(sl, x + 0.2, y + 0.15, w - 0.4, h - 0.3, paras, size, mono=True, spacing=1.05)


def callout(sl, x, y, w, h, title, body, color=YEL, size=17):
    rect(sl, x, y, w, h, fill=PANEL)
    rect(sl, x, y, 0.08, h, fill=color, shape=MSO_SHAPE.RECTANGLE)
    text(sl, x + 0.25, y + 0.12, w - 0.4, 0.45, title, size + 1, color, True)
    text(sl, x + 0.25, y + 0.58, w - 0.4, h - 0.65, body, size, WHITE)


def table(sl, x, y, colw, rows, size=16, rowh=0.46, head=True):
    cy = y
    for ri, row in enumerate(rows):
        fill = RGBColor(0x2F, 0x3D, 0x4F) if (head and ri == 0) else (PANEL if ri % 2 else RGBColor(0x22, 0x2B, 0x38))
        rect(sl, x, cy, sum(colw), rowh, fill=fill, shape=MSO_SHAPE.RECTANGLE)
        cx = x
        for ci, cell in enumerate(row):
            col = MINT if (head and ri == 0) else WHITE
            if isinstance(cell, tuple): cell, col = cell
            text(sl, cx + 0.12, cy, colw[ci] - 0.2, rowh, cell, size, col, head and ri == 0, anchor=MSO_ANCHOR.MIDDLE)
            cx += colw[ci]
        cy += rowh
    return cy


def divider(dk, n, title, goal, mins, shot_path, do):
    sl = new(dk)
    text(sl, 0.9, 1.2, 4, 1.6, f'{n:02d}', 110, MINT, True)
    text(sl, 0.95, 3.0, 6.2, 1.0, title, 38 if len(title) <= 14 else 28, WHITE, True)
    text(sl, 0.97, 3.95, 6.0, 0.6, goal, 20, DIM)
    rect(sl, 0.97, 4.75, 1.9, 0.5, fill=PANEL)
    text(sl, 0.97, 4.75, 1.9, 0.5, f'約 {mins} 分鐘' if isinstance(mins, int) else mins, 16, YEL, True, PP_ALIGN.CENTER, MSO_ANCHOR.MIDDLE)
    rect(sl, 3.0, 4.75, 3.6, 0.5, fill=PANEL)
    text(sl, 3.0, 4.75, 3.6, 0.5, do, 16, MINT, True, PP_ALIGN.CENTER, MSO_ANCHOR.MIDDLE)
    image(sl, shot_path, 7.2, 1.3, 5.6, 4.6)
    return sl


# ══════════════════════════════════════════════════════════════════════════
# 從舊簡報複製頁面（Tilemap 部分原封不動）
# ══════════════════════════════════════════════════════════════════════════

def copy_slide(dk, src_slide):
    """把另一份簡報的投影片整頁複製過來：背景、所有形狀、圖片。頁尾頁碼換成新的。"""
    dk.page += 1
    sl = dk.prs.slides.add_slide(dk.blank)
    src_bg = src_slide._element.cSld.bg
    if src_bg is not None:
        old = sl._element.cSld.bg
        if old is not None: sl._element.cSld.remove(old)
        sl._element.cSld.insert(0, copy.deepcopy(src_bg))
    rid_map = {}
    for rel in src_slide.part.rels.values():
        if 'image' in rel.reltype:
            _, new_rid = sl.part.get_or_add_image_part(io.BytesIO(rel.target_part.blob))
            rid_map[rel.rId] = new_rid
    tree = sl.shapes._spTree
    for shp in src_slide.shapes:
        el = copy.deepcopy(shp._element)
        for blip in el.iter(qn('a:blip')):
            rid = blip.get(qn('r:embed'))
            if rid in rid_map: blip.set(qn('r:embed'), rid_map[rid])
        tree.append(el)
    # 頁碼：右下角那個只有數字的文字框
    for shp in sl.shapes:
        if shp.has_text_frame and shp.text_frame.text.strip().isdigit() and shp.left > Inches(11.5) and shp.top > Inches(6.8):
            r = shp.text_frame.paragraphs[0].runs[0]; r.text = str(dk.page)
    return sl


# ══════════════════════════════════════════════════════════════════════════
# 圖解用的小圖
# ══════════════════════════════════════════════════════════════════════════

RIG_BONES = [((160, 222), (160, 295)), ((160, 295), (160, 370)), ((160, 370), (160, 480)),
             ((203, 345), (248, 280)), ((248, 280), (280, 215)), ((280, 215), (292, 190)),
             ((117, 345), (72, 280)), ((72, 280), (40, 215)), ((40, 215), (28, 190)),
             ((182, 222), (192, 125)), ((192, 125), (196, 36)), ((138, 222), (128, 125)), ((128, 125), (124, 36))]
RIG_RADIUS = [42, 44, 58, 17, 14, 18, 17, 14, 18, 22, 19, 22, 19]
BONE_COLORS = [(239, 71, 111), (255, 209, 102), (6, 214, 160), (58, 134, 255), (131, 56, 236), (255, 120, 60),
               (58, 134, 255), (131, 56, 236), (255, 120, 60), (17, 138, 178), (6, 214, 160), (17, 138, 178), (6, 214, 160)]


def rig_images():
    """骨骼角色的三張說明圖：Bone、Mesh、Weight。"""
    rig = Image.open(ART + '/Characters/RigHero.png').convert('RGBA')
    S = 2
    W, H = rig.width * S, rig.height * S
    big = rig.resize((W, H))

    def canvas():
        c = Image.new('RGBA', (W, H), (40, 48, 60, 255)); c.paste(big, (0, 0), big); return c

    def P(p): return (p[0] * S, (rig.height - p[1]) * S)

    def seg(px, py, a, b):
        ax, ay = a; bx, by = b; dx, dy = bx - ax, by - ay
        t = max(0, min(1, ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy)))
        return math.hypot(px - ax - dx * t, py - ay - dy * t)

    # Bone
    b = canvas(); d = ImageDraw.Draw(b)
    for a, e in RIG_BONES:
        (ax, ay), (ex, ey) = P(a), P(e)
        L = math.hypot(ex - ax, ey - ay); nx, ny = -(ey - ay) / L * 10, (ex - ax) / L * 10
        d.polygon([(ax + nx, ay + ny), (ex, ey), (ax - nx, ay - ny)], fill=(255, 209, 102, 235), outline=(27, 33, 44))
        d.ellipse([ax - 8, ay - 8, ax + 8, ay + 8], fill=(6, 214, 160), outline=(27, 33, 44), width=2)
    b.save(ANN + '/rig_bone.png')

    # Mesh：每 16 px 一格，有畫到的格子畫成兩個三角形
    m = canvas(); d = ImageDraw.Draw(m); step = 16
    alpha = rig.split()[3]
    for gy in range(0, rig.height, step):
        for gx in range(0, rig.width, step):
            if alpha.crop((gx, gy, gx + step, gy + step)).getbbox() is None: continue
            x0, y0, x1, y1 = gx * S, gy * S, (gx + step) * S, (gy + step) * S
            col = (6, 214, 160, 255)
            d.rectangle([x0, y0, x1, y1], outline=col, width=1); d.line([(x0, y1), (x1, y0)], fill=col, width=1)
    m.save(ANN + '/rig_mesh.png')

    # Weight：每個像素塗上「影響最大的那根 Bone」的顏色
    wimg = canvas(); px = wimg.load(); src = big.load()
    for y in range(0, H, 2):
        for x in range(0, W, 2):
            if src[x, y][3] < 40: continue
            sx, sy = x / S, rig.height - y / S
            best = min(range(len(RIG_BONES)), key=lambda i: max(0, seg(sx, sy, *RIG_BONES[i]) - RIG_RADIUS[i]) + 0.15 * seg(sx, sy, *RIG_BONES[i]))
            c = BONE_COLORS[best]; o = src[x, y]
            mix = tuple(int(o[k] * 0.35 + c[k] * 0.65) for k in range(3)) + (255,)
            for dx in (0, 1):
                for dy in (0, 1):
                    if x + dx < W and y + dy < H: px[x + dx, y + dy] = mix
    wimg.save(ANN + '/rig_weight.png')


def sheet_image():
    """Hero_Sheet 加上格線與格號。"""
    sheet = Image.open(ART + '/Characters/Hero_Sheet.png').convert('RGBA')
    c = Image.new('RGBA', (sheet.width, sheet.height + 40), (27, 33, 44, 255)); c.paste(sheet, (0, 0), sheet)
    d = ImageDraw.Draw(c); f = ImageFont.truetype(BOLD_TTC, 26)
    for k in range(12):
        d.rectangle([k * 128, 0, k * 128 + 127, 127], outline=(90, 110, 130), width=2)
        d.text((k * 128 + 64, 148), str(k), fill=(255, 209, 102), font=f, anchor='mm')
    c.save(ANN + '/sheet.png')


def attack_strip():
    """攻擊 4 格 + 第 2 格的事件旗子。"""
    a = Image.open(ART + '/Characters/Hero_Attack.png').convert('RGBA')
    c = Image.new('RGBA', (a.width, a.height + 70), (27, 33, 44, 255)); c.paste(a, (0, 0), a)
    d = ImageDraw.Draw(c); f = ImageFont.truetype(BOLD_TTC, 22)
    names = ['0 舉劍', '1 揮下', '2 命中', '3 收招']
    for k in range(4):
        d.rectangle([k * 128, 0, k * 128 + 127, 127], outline=(90, 110, 130), width=2)
        d.text((k * 128 + 64, 148), names[k], fill=(255, 209, 102) if k == 2 else (200, 210, 220), font=f, anchor='mm')
    c.save(ANN + '/attack_strip.png')


def icon(src, out, box=None):
    im = Image.open(src).convert('RGBA')
    if box: im = im.crop(box)
    p = f'{ANN}/icon_{out}.png'; im.save(p); return p


# ══════════════════════════════════════════════════════════════════════════
# 開場頁
# ══════════════════════════════════════════════════════════════════════════

def title_slide(dk, week2=False):
    sl = dk.prs.slides.add_slide(dk.blank)
    sl.background.fill.solid(); sl.background.fill.fore_color.rgb = BG; dk.page += 1
    sl.shapes.add_picture(LOGO, Inches(0.9), Inches(0.9), height=Inches(2.2))
    text(sl, 0.95, 3.45, 11.5, 0.5, '清大遊戲創作社 NTHU GDC · 社課簡報' + ('（第二堂）' if week2 else ''), 20, MINT, True)
    text(sl, 0.92, 3.95, 11.6, 1.1, 'Unity 2D 動畫：Sprite Sheet 到 State Machine' if week2 else 'Unity 2D 磁磚與動畫工作坊',
         30 if week2 else 40, WHITE, True)
    text(sl, 0.95, 5.0, 11.5, 0.6, 'Sprite Sheet · Animator · Skeletal Animation · Animation Event · State Machine' if week2
         else 'Tilemap · Sprite Sheet · Skeletal Animation · Animation Event · State Machine', 18, DIM)
    rect(sl, 0.95, 5.95, 4.6, 0.62, fill=PANEL)
    text(sl, 1.15, 5.95, 4.4, 0.62, [[('報告人　', DIM, False), ('社長 李久恩 Joe', WHITE, True)]], 18, anchor=MSO_ANCHOR.MIDDLE)
    image(sl, game('Ch7_StateMachine_play2'), 8.3, 1.0, 4.4, 2.5)


def learn_slide(dk, week2=False):
    sl = new(dk, '今天你會學到什麼')
    hero = icon(ART + '/Characters/Hero_Sheet.png', 'hero', (0, 0, 128, 128))
    atk = icon(ART + '/Characters/Hero_Attack.png', 'atk', (256, 0, 384, 128))
    rigi = icon(ART + '/Characters/RigHero.png', 'rig', (0, 300, 320, 512))
    crate = tile('Crate')
    if week2:
        cards = [(icon(ART + '/Characters/Hero_Sheet.png', 'run', (640, 0, 768, 128)), 'Sprite Sheet', '切圖、做 Animation Clip'),
                 (hero, 'Animator', 'State、Transition、Parameter'),
                 (rigi, 'Skeletal Animation', 'Bone、Weight、IK'),
                 (atk, 'Animation Event', '動畫在對的那一格呼叫函式'),
                 (crate, 'State Machine', '用 enum + switch 寫角色'),
                 (tile('Lava_0'), '自由創作', '做自己的關卡')]
        cols = 3
    else:
        cards = [(tile('Stone'), '用磁磚畫地圖', 'Tilemap、Tile Palette'),
                 (tile('Ground_T'), '自動接邊的磁磚', 'Rule Tile、Animated Tile'),
                 (tile('Brick'), '用程式改地圖', '踩到什麼、挖磚、放磚'),
                 (hero, 'Sprite Sheet', 'Animator、State'),
                 (rigi, 'Skeletal Animation', 'Bone、Weight、IK'),
                 (atk, 'Animation Event', '動畫呼叫函式'),
                 (crate, 'State Machine', 'enum + switch'),
                 (tile('Lava_0'), '自由創作', '做自己的關卡')]
        cols = 4
    cw = 11.8 / cols
    for i, (src, t1, t2) in enumerate(cards):
        cx, cy = 0.8 + (i % cols) * cw, 1.55 + (i // cols) * 2.65
        rect(sl, cx, cy, cw - 0.3, 2.35)
        image(sl, src, cx + (cw - 0.3) / 2 - 0.5, cy + 0.2, 1.0, 1.0, border=False)
        badge(sl, cx + 0.2, cy + 0.2, i + 1, 0.42)
        text(sl, cx + 0.1, cy + 1.3, cw - 0.5, 0.5, t1, 19 if cols == 3 else 16, WHITE, True, PP_ALIGN.CENTER)
        text(sl, cx + 0.1, cy + 1.78, cw - 0.5, 0.45, t2, 14 if cols == 3 else 12, DIM, align=PP_ALIGN.CENTER)


def flow_slide(dk, week2=False):
    sl = new(dk, '今日流程' if week2 else '課程流程', sub='一章一關，玩得動才算完成。難度一章比一章高')
    rows = [['章', '主題', '你要做的事', '時間']]
    first = [['0', 'Grid 與 Tilemap', '只改 Inspector', '15 分'], ['1', 'Tile Palette', '畫地圖、加碰撞', '25 分'],
             ['2', 'Rule Tile 與 Animated Tile', '改磁磚、補規則', '30 分'], ['3', '用程式讀寫 Tilemap', '第一次寫程式（填空）', '35 分']]
    second = [['4', 'Sprite Sheet 與 Animator', 'Slice、Clip、State、Transition', '40 分'],
              ['5', 'Skeletal Animation 與 IK', 'Bone、Weight、Record、IK', '40 分'],
              ['6', 'Animation Event', '揮劍打碎木箱、跑步揚塵', '30 分'],
              ['7', 'State Machine', 'enum + switch 寫主角', '30 分'],
              ['8', '自由創作', '自己畫關卡', '剩下的時間']]
    if week2:
        rows += second
        table(sl, 1.3, 1.75, [0.9, 3.9, 4.4, 1.6], rows, 18, 0.62)
        text(sl, 1.3, 5.75, 10.8, 0.5, '中間休息 10 分鐘（第 5 章之後）', 16, DIM)
    else:
        rows += [[(c, DIM) if k == 0 else c for k, c in enumerate(r)] for r in []] + first
        rows += [[('—', DIM), ('第二堂', MINT), '', '']] + second
        table(sl, 1.3, 1.6, [0.9, 3.9, 4.4, 1.6], rows, 15, 0.45)


def before_slide(dk):
    sl = new(dk, '開始之前', sub='只要做一次')
    bullets(sl, 0.9, 1.8, 6.4, [
        ('用 Unity 6000.3.10f1 開啟專案', 'clone 前先 git lfs install，不然中文會變方框'),
        ('打開 Assets/_GDC/Scenes/Ch0_Grid', ''),
        ('按上方的「播放」鍵', '第一次開會跑幾分鐘，正常')], 20, 0.25, num=True)
    rect(sl, 7.7, 1.8, 4.9, 4.6)
    text(sl, 7.95, 1.95, 4.5, 0.5, '遊戲操作', 20, MINT, True)
    table(sl, 7.95, 2.6, [1.6, 2.8], [['按鍵', '功能'], ['A / D', '左右移動'], ['空白鍵', '跳'], ['J', '攻擊（第 6 章起）'],
                                        ['R', '重玩本章'], ['N / P', '下一章 / 上一章']], 17, 0.55)


def update_slide(dk):
    sl = new(dk, '開始之前：先更新專案', sub='這次新增了第 6、7 章（第 8 章是原本的自由創作）')
    callout(sl, 0.8, 1.65, 5.75, 2.75, '用 git clone 的',
            '在專案資料夾開終端機，依序輸入：', MINT, 18)
    code(sl, 1.1, 2.75, 5.2, 1.3, 'git pull\ngit lfs pull', 18)
    callout(sl, 6.8, 1.65, 5.75, 2.75, '下載 zip 的',
            '1. GitHub 專案頁面 → Code → Download ZIP\n2. 解壓到「新的資料夾」，不要覆蓋舊的\n3. Unity Hub → Add → 選新資料夾', YEL, 17)
    bullets(sl, 0.9, 4.75, 11.8, [
        ('上週自己寫的腳本想留著？更新前先另存一份（例如 Ch3_TileProbe.cs）', ''),
        ('git pull 出現 conflict（衝突）就舉手', ''),
        ('更新成功：Assets/_GDC/Scenes/ 裡看得到 Ch6_AnimEvent、Ch7_StateMachine、Ch8_Free', '')], 17, 0.02)
    text(sl, 0.9, 6.45, 11.8, 0.4, 'github.com/nthugdc/2026_GDC_example_2', 15, MINT, True)


def recap_slide(dk):
    sl = new(dk, '上週回顧：Tilemap')
    items = [(tile('Stone'), 'Grid 與 Tilemap', 'Grid 是方格紙、Tilemap 是描圖紙、Tile 是印章'),
             (tile('Brick'), 'Tile Palette', '筆刷 B、方框 U、橡皮擦 D；Active Tilemap 要選對'),
             (tile('Ground_T'), 'Rule Tile / Animated Tile', '看鄰居自動換圖；好幾張圖輪流播'),
             (tile('Ice'), '程式讀寫 Tilemap', 'WorldToCell → GetTile / SetTile；z 要歸零')]
    for i, (src, t, d) in enumerate(items):
        y = 1.5 + i * 1.22
        rect(sl, 0.8, y, 11.8, 1.1)
        image(sl, src, 1.0, y + 0.15, 0.8, 0.8, border=False)
        text(sl, 2.1, y + 0.08, 4.4, 0.5, t, 20, MINT, True)
        text(sl, 2.1, y + 0.55, 10.2, 0.5, d, 16, WHITE)
    text(sl, 0.8, 6.45, 11.8, 0.4, '今天：讓角色「動起來」—— 動畫、呼叫函式的動畫、狀態機', 16, YEL, True)


def end_slide(dk):
    sl = dk.prs.slides.add_slide(dk.blank)
    sl.background.fill.solid(); sl.background.fill.fore_color.rgb = BG; dk.page += 1
    sl.shapes.add_picture(LOGO, Inches(4.4), Inches(1.0), height=Inches(2.25))
    text(sl, 0, 3.6, SW, 1.0, '今天辛苦了！', 44, WHITE, True, PP_ALIGN.CENTER)
    text(sl, 0, 4.55, SW, 0.6, 'Sprite Sheet · Animator · Skeletal Animation · Animation Event · State Machine', 18, DIM, align=PP_ALIGN.CENTER)
    text(sl, 0, 5.4, SW, 0.5, '專案：github.com/nthugdc/2026_GDC_example_2', 17, MINT, align=PP_ALIGN.CENTER)
    text(sl, 0, 5.85, SW, 0.5, '講義：Assets/_GDC/Docs/README.md', 16, DIM, align=PP_ALIGN.CENTER)
    text(sl, 0, 6.6, SW, 0.5, '清大遊戲創作社 NTHU GDC · 社長 李久恩 Joe', 14, DIM, align=PP_ALIGN.CENTER)


# ══════════════════════════════════════════════════════════════════════════
# 動畫部分（第 4～8 章），完整版與本週版共用
# ══════════════════════════════════════════════════════════════════════════

def chapter4(dk):
    C = '第 4 章'
    divider(dk, 4, 'Sprite Sheet 與 Animator', '切圖、做 Animation Clip、用 Animator 切換', 40,
            game('Ch4_SpriteSheet_answer'), '目標：角色會跑會跳')

    sl = new(dk, 'Sprite Sheet 是什麼？', C, '一張大圖排了好幾格動作，快速換圖就會動（跟手翻書一樣）')
    image(sl, ANN + '/sheet.png', 0.7, 1.8, 11.9, 1.2, border=False)
    for (a, b, n, col) in [(0, 4, 'Idle', MINT), (4, 10, 'Run', YEL), (10, 12, 'Jump', BLUE)]:
        x0 = 0.7 + a * 11.9 / 12; w = (b - a) * 11.9 / 12
        rect(sl, x0 + 0.05, 3.25, w - 0.1, 0.5, fill=col)
        text(sl, x0, 3.25, w, 0.5, n, 16, BG, True, PP_ALIGN.CENTER, MSO_ANCHOR.MIDDLE)
    table(sl, 2.0, 4.3, [3.0, 6.3], [['名詞', '意思'], ['Sprite', '一張圖（遊戲裡看到的圖片）'],
                                     ['Sprite Sheet', '很多張 Sprite 排在同一張大圖裡'], ['frame', '動畫的「一格」'],
                                     ['Slice（切圖）', '告訴 Unity 每一格在大圖的哪裡']], 16, 0.46)

    sl = new(dk, '角色怎麼是一整排？', C)
    image(sl, game('Ch4_SpriteSheet_student'), 0.7, 1.45, 7.4, 4.2)
    callout(sl, 8.5, 1.6, 4.2, 2.6, '因為圖還沒切！', '整張 12 格的圖\n被當成「一張」Sprite 貼上去了', YEL, 18)

    # 三個名詞的關係
    sl = new(dk, '先認識三個名詞', C, '動畫相關的東西分成三層，名字很像，分清楚就不會亂')
    box(sl, 0.8, 1.8, 3.2, 1.3, 'Animator', PANEL, MINT, 22, line=MINT, sub='掛在 Player 上的元件')
    box(sl, 5.05, 1.8, 3.2, 1.3, 'Animator Controller', PANEL, YEL, 19, line=YEL, sub='.controller：State 流程圖')
    box(sl, 9.3, 1.8, 3.2, 1.3, 'Animation Clip', PANEL, BLUE, 21, line=BLUE, sub='.anim：一段動畫')
    arrow(sl, 4.05, 2.45, 5.0, 2.45, label='用', loff=(0, -0.42))
    arrow(sl, 8.3, 2.45, 9.25, 2.45)
    text(sl, 7.9, 3.15, 2.2, 0.35, '每個 State 放一個', 13, YEL, True, PP_ALIGN.CENTER)
    image(sl, ann(shot('ch4_player'), 'ch4_player', [(0, 271, 480, 295, 1), (5, 301, 480, 326, 2)], crop=(0, 0, 481, 600)),
          0.8, 3.5, 4.4, 3.2)
    bullets(sl, 5.7, 3.6, 6.9, [('Player 上的 Animator 元件', '負責「照著 Controller 播動畫」'),
                                ('Controller 欄位：現在是 None', '等一下做 Animation Clip 時 Unity 會自動建一個')], 17, 0.15, num=True)

    sl = new(dk, '步驟一：Slice（1）', C, 'Project 選 Art/Characters/Hero_Sheet_Unsliced')
    image(sl, ann(shot('ch4_sheet'), 'ch4_sheet', [(5, 177, 465, 202, 1), (405, 640, 462, 664, 2), (20, 328, 460, 352, 3)],
                  crop=(0, 0, 481, 680)), 0.7, 1.5, 5.6, 5.2)
    bullets(sl, 6.8, 1.7, 5.8, [('Sprite Mode 改成 Multiple', '一張圖裡有很多張 Sprite'), ('按右下角 Apply', ''),
                                ('按 Open Sprite Editor', '')], 19, 0.18, num=True)

    sl = new(dk, '步驟一：Slice（2）', C, 'Sprite Editor 視窗')
    image(sl, ann(shot('ch4_spriteeditor'), 'ch4_se_bar', [(150, 74, 215, 100, 1)], crop=(0, 68, 700, 104), scale=1.4),
          0.7, 1.6, 6.8, 0.6)
    image(sl, crop_img(shot('ch4_spriteeditor'), 'ch4_se_sheet', (80, 410, 1800, 585)), 0.7, 2.5, 6.8, 0.75)
    bullets(sl, 8.0, 1.6, 4.8, [('左上 Slice', ''), ('Type 選 Grid By Cell Size', ''), ('Pixel Size 填 128、128', ''),
                                ('按 Slice，再按右上 Apply', '')], 18, 0.12, num=True)
    callout(sl, 0.7, 3.6, 6.8, 1.6, '切好了', 'Project 裡那張圖可以展開，\n裡面有 _0 到 _11 十二張 Sprite', MINT, 17)
    callout(sl, 0.7, 5.4, 12.0, 1.2, '步驟二：換掉角色的圖',
            'Player 的 Sprite Renderer > Sprite 換成 Hero_Sheet_Unsliced_0，畫面上只剩一個人', YEL, 16)

    sl = new(dk, 'Animation 視窗導覽', C, '選 Player → Window > Animation > Animation')
    image(sl, ann(shot('ch4_animation'), 'ch4_anim', [(0, 110, 255, 131, 1), (0, 85, 70, 103, 2), (75, 84, 97, 104, 3),
                                                     (515, 84, 1000, 104, 4), (515, 136, 1000, 178, 5)],
                  crop=(0, 55, 1000, 220)), 0.7, 1.45, 11.9, 2.0)
    table(sl, 0.8, 3.45, [0.6, 2.9, 8.2], [['', '區域', '用途'],
                                           ['1', 'Clip 選單', '切換要編輯哪個 Animation Clip；最下面 Create New Clip 可以新增'],
                                           ['2', 'Preview / 播放', '在編輯模式預覽動畫，不用按 Play'],
                                           ['3', 'Record（紅點）', '錄影模式：改的數值都會變成 Keyframe（第 5 章會用）'],
                                           ['4', 'Timeline', '時間軸，上面的刻度是時間'],
                                           ['5', 'Keyframe', '菱形：「這個時間點，Sprite 換成哪一張」']], 15, 0.5)

    sl = new(dk, '步驟三：做 Animation Clip', C)
    bullets(sl, 0.8, 1.55, 6.6, [('選 Player，按 Create，存成 Idle', 'Unity 會順便建好 Animator Controller'),
                                 ('Project 把 _0 ~ _3 一起選起來，拖進 Timeline', '每張圖變成一個 Keyframe'),
                                 ('Clip 選單 > Create New Clip', 'Run 拖入 _4 ~ _9；Jump 拖入 _10、_11')], 18, 0.2, num=True)
    text(sl, 7.8, 1.55, 4.8, 0.5, 'Sample Rate：一秒播幾格', 18, MINT, True)
    table(sl, 7.8, 2.15, [1.8, 3.0], [['Samples', '效果'], ['60（預設）', '快到看不清楚'], ['12', 'Run 剛好'], ['6', 'Idle 剛好']], 16, 0.52)
    callout(sl, 7.8, 4.45, 4.8, 1.6, '找不到 Samples？', 'Animation 視窗右上角 ⋮\n> Show Sample Rate', YEL, 15)

    sl = new(dk, 'Animator 視窗導覽', C, 'Window > Animation > Animator（這是做好之後的樣子）')
    image(sl, ann(shot('ch4_animator'), 'ch4_animator', [(256, 134, 456, 168, 1), (256, 258, 456, 292, 2), (570, 106, 820, 154, 3),
                                                        (1192, 258, 1392, 292, 4)], crop=(190, 95, 1420, 420), inside=True),
          0.7, 1.5, 6.6, 1.9)
    table(sl, 0.7, 3.7, [0.6, 2.5, 8.7], [['', '方塊', '意思'],
                                          ['1', 'Any State', '「不管現在在哪個 State」—— 從這裡拉出的箭頭，任何時候都能觸發'],
                                          ['2', 'Entry', '起點，連到 Default State'],
                                          ['3', 'Default State（橘色）', '一開始播的 State（右鍵 > Set as Layer Default State 可以換）'],
                                          ['4', 'Exit', '離開這個 State Machine（這章用不到）']], 15, 0.55)
    text(sl, 7.6, 1.6, 5.0, 1.8, '每個灰色方塊是一個 State，\n裡面放一個 Animation Clip。\n箭頭叫 Transition。', 17, WHITE)

    sl = new(dk, 'Parameter：腳本跟 Animator 的「訊號」', C, 'Animator 視窗左側 Parameters 分頁 → +')
    table(sl, 0.8, 1.7, [1.6, 2.6, 3.3, 4.2], [['型別', '值', '適合', '腳本怎麼設'],
                                               [('Float', YEL), '小數', '速度、血量', 'anim.SetFloat("Speed", 5f)'],
                                               [('Int', YEL), '整數', '第幾種武器', 'anim.SetInteger("Weapon", 2)'],
                                               [('Bool', YEL), 'true / false', '在不在地上', 'anim.SetBool("Grounded", true)'],
                                               [('Trigger', YEL), '扣一下就自己放開', '攻擊、受傷（第 6 章）', 'anim.SetTrigger("Attack")']], 16, 0.62)
    callout(sl, 0.8, 5.1, 11.7, 1.4, '這章要加兩個', 'Speed（Float）、Grounded（Bool，記得打勾當預設值，不然一開始就在 Jump）', MINT, 16)

    sl = new(dk, '步驟四：拉 Transition', C, '在 State 上按右鍵 > Make Transition，拉到另一個 State')
    table(sl, 0.8, 1.7, [2.0, 1.7, 3.6], [['從', '到', 'Conditions'], ['Idle', 'Run', 'Speed Greater 0.1'], ['Run', 'Idle', 'Speed Less 0.1'],
                                          ['Any State', 'Jump', 'Grounded = false'], ['Jump', 'Idle', 'Grounded = true']], 16, 0.55)
    callout(sl, 8.6, 1.7, 4.0, 2.75, '每一條箭頭都要', '取消勾選 Has Exit Time\nTransition Duration 改 0\n\n下一頁說明為什麼', YEL, 16)
    text(sl, 0.8, 4.75, 11.8, 1.2, '讀法：「在 Idle 的時候，如果 Speed 大於 0.1，就換到 Run」\n'
         '同一個 State 可以拉很多條箭頭；Any State 拉出的箭頭在任何 State 都能觸發', 16, DIM)

    sl = new(dk, 'Transition 的設定', C, '點一條箭頭，看 Inspector')
    image(sl, ann(shot('ch4_transition'), 'ch4_tr', [(0, 276, 480, 302, 1), (0, 302, 480, 325, 2), (5, 530, 478, 592, 3)],
                  crop=(0, 0, 481, 620)), 0.7, 1.5, 4.8, 5.2)
    table(sl, 5.9, 1.6, [0.6, 2.7, 3.6], [['', '欄位', '意思'],
                                          ['1', 'Has Exit Time', '打勾 = 目前的動畫要播到 Exit Time 才准切換'],
                                          ['2', 'Settings > Exit Time', '播到幾成才切（1 = 播完）'],
                                          ['2', 'Transition Duration', '兩段動畫淡入淡出的時間；Sprite 動畫設 0'],
                                          ['3', 'Conditions', '什麼條件成立才切；+ 新增']], 14, 0.72)
    callout(sl, 5.9, 5.3, 6.9, 1.35, '親手試錯', '留一條 Has Exit Time 打勾按播放：按下去要等一下才開始跑！', RED, 15)

    sl = new(dk, '步驟五：寫腳本', C, 'Scripts/Student/Ch4_HeroAnimator.cs：每一幀把 Parameter 告訴 Animator')
    code(sl, 0.7, 1.55, 7.2, 4.6, '''// ────────── TODO 4-A ──────────
// 把 Animator 的 Speed 參數設成
// 「水平速度有多快」。

// ────────── TODO 4-B ──────────
// 把 Grounded 參數設成「有沒有站在地上」。

// ────────── TODO 4-C ──────────
// 按左就翻轉圖片，按右就不翻；
// 沒按的時候維持原本的方向。''', 15)
    table(sl, 8.2, 1.55, [2.3, 2.2], [['要用的', '是什麼'], ['anim.SetFloat', '設 Float'], ['anim.SetBool', '設 Bool'],
                                      ['rb.linearVelocity.x', '水平速度'], ['move.IsGrounded()', '在地上嗎'], ['sr.flipX', '左右翻轉']], 14, 0.5)
    callout(sl, 8.2, 4.75, 4.5, 1.4, '試錯：往左跑一直是 Idle？', '往左速度是負的，-5 > 0.1 不成立\n→ 要用 Mathf.Abs', RED, 13)

    sl = new(dk, '除錯：Play 的時候看 Animator', C, '按播放，選 Player，開著 Animator 視窗')
    bullets(sl, 0.8, 1.6, 6.3, [('正在播的 State 底下有藍色進度條', ''), ('Parameters 的數值即時跳動', ''),
                                ('箭頭被觸發的瞬間會亮起來', '')], 18, 0.2)
    box(sl, 7.4, 1.6, 5.2, 0.8, '動畫不對？', PANEL, YEL, 20, line=YEL)
    box(sl, 7.4, 2.9, 5.2, 0.8, 'Parameter 有沒有在變？', PANEL, WHITE, 17)
    arrow(sl, 10.0, 2.4, 10.0, 2.9)
    box(sl, 7.4, 4.35, 2.4, 1.2, '沒變', PANEL, RED, 17, sub='腳本沒寫對\n或名字打錯')
    box(sl, 10.2, 4.35, 2.4, 1.2, '有變', PANEL, MINT, 17, sub='Conditions\n設錯了')
    arrow(sl, 9.0, 3.7, 8.6, 4.35); arrow(sl, 11.0, 3.7, 11.4, 4.35)
    text(sl, 0.8, 4.4, 6.3, 1.5, 'Console 黃字「Parameter \'Speed\' does not exist」\n= 腳本裡的名字跟 Animator 不一樣（大小寫也算）', 15, DIM)


def chapter5(dk):
    C = '第 5 章'
    divider(dk, 5, 'Skeletal Animation 與 IK', '一張圖長出 Bone，手跟著滑鼠走', 40, game('Ch5_Skeleton_answer'), '目標：用手碰到 3 顆星')

    sl = new(dk, 'Skeletal Animation vs Sprite Sheet', C)
    table(sl, 0.8, 1.6, [2.6, 4.4, 4.6], [['', 'Sprite Sheet（第 4 章）', 'Skeletal Animation（這章）'],
                                          ['美術要畫', '每個動作的每一個 frame', '只要一張圖'], ['怎麼動', '換圖', '轉 Bone，圖跟著彎'],
                                          ['動作很多時', '圖檔爆炸', '只多幾個 Animation Clip'], ['適合', '像素風、小角色', '大角色、要接 IK']], 18, 0.65)

    sl = new(dk, '三個名詞：Bone、Mesh、Weight', C)
    for i, (img, t, d) in enumerate([('rig_bone', 'Bone', '有長度、會轉的骨頭\n有 parent：轉上臂，前臂跟著動'),
                                     ('rig_mesh', 'Mesh', '把圖切成很多小三角形\n頂點可以被 Bone 拉動'),
                                     ('rig_weight', 'Weight', '每個頂點「跟哪根 Bone 走」\n顏色 = 主要跟哪根')]):
        x = 0.8 + i * 4.1
        image(sl, ANN + f'/{img}.png', x + 0.6, 1.4, 2.6, 3.8, border=False)
        text(sl, x, 5.25, 3.8, 0.5, t, 22, MINT, True, PP_ALIGN.CENTER)
        text(sl, x, 5.75, 3.8, 0.9, d, 14, WHITE, align=PP_ALIGN.CENTER)

    sl = new(dk, '步驟一：Skinning Editor', C, 'RigHero → Open Sprite Editor → 左上角切到 Skinning Editor')
    image(sl, ann(shot('ch5_skinning'), 'ch5_sk', [(20, 160, 180, 186, 4), (20, 300, 180, 326, 1), (20, 414, 180, 440, 2),
                                                  (20, 618, 180, 644, 3), (20, 678, 180, 704, 5)], crop=(0, 40, 1250, 886)),
          0.7, 1.5, 6.3, 4.6)
    table(sl, 7.3, 1.55, [0.5, 2.3, 3.0], [['', '工具', '用途'], ['1', 'Create Bone', '一節一節點出 Bone'],
                                           ['2', 'Auto Geometry', '自動產生 Mesh'], ['3', 'Auto Weights', '自動算 Weight'],
                                           ['4', 'Preview Pose', '拖動 Bone 預覽（不存檔）'], ['5', 'Weight Brush', '手動修 Weight']], 14, 0.55)
    text(sl, 7.3, 5.1, 5.4, 1.4, '順序：Create Bone → Auto Geometry\n→ Generate → Auto Weights → Generate\n→ 右上 Apply', 15, YEL, True)

    sl = new(dk, 'Bone 要怎麼點？', C)
    image(sl, ANN + '/rig_bone.png', 0.7, 1.4, 3.6, 5.4, border=False)
    bullets(sl, 4.8, 1.6, 7.9, [('一節一節點下去', '每點一下就多一節，按右鍵結束'),
                                ('右手至少要三節：上臂、前臂、手', '等一下 IK 要用最後一節「手」'),
                                ('Bone 別點得太靠近頭', '不然頭會被手臂拉走'),
                                ('點錯了？', 'Edit Bone 可以拖動，Delete 刪掉')], 19, 0.2, num=True)

    sl = new(dk, '修 Weight：Weight Brush', C, 'Preview Pose 時頭被手臂拉走？就是 Weight 不對')
    image(sl, ANN + '/rig_weight.png', 0.7, 1.4, 3.6, 5.4, border=False)
    bullets(sl, 4.8, 1.6, 7.9, [('選 Weight Brush', ''), ('在右下面板選要「加強」的 Bone（例如 Head）', ''),
                                ('在要修的地方刷幾下', '顏色 = 那根 Bone 的影響範圍'), ('再用 Preview Pose 確認，最後 Apply', '')], 19, 0.2, num=True)

    sl = new(dk, '步驟二：Sprite Skin → Create Bones', C, '選場景的 RigHero → Add Component → Sprite Skin')
    image(sl, ann(shot('ch5_spriteskin'), 'ch5_skin', [(0, 161, 480, 185, 1), (75, 755, 215, 780, 2)], crop=(0, 0, 481, 800)),
          0.7, 1.5, 4.6, 5.2)
    image(sl, ann(shot('ch5_hier'), 'ch5_hier', [(55, 150, 455, 420, None)], crop=(0, 0, 468, 430)), 5.8, 1.5, 3.4, 3.1)
    bullets(sl, 9.6, 1.6, 3.2, [('加 Sprite Skin', ''), ('按 Create Bones', 'RigHero 底下多出一串 Bone（就是 Transform）')], 17, 0.15, num=True)

    sl = new(dk, '步驟三：用 Record 錄一段 Idle', C, '選 RigHero → Animation 視窗 → Create')
    image(sl, ann(shot('ch5_record'), 'ch5_rec', [(75, 84, 97, 104, 1), (515, 84, 1150, 104, 2), (40, 140, 470, 236, 3)],
                  crop=(0, 55, 1150, 260)), 0.7, 1.45, 11.9, 2.0)
    bullets(sl, 0.8, 3.75, 6.6, [('按紅色 Record', '時間軸變紅 = 錄影中'), ('移到 1 秒，用旋轉工具（E）轉一下 Bone', '自動多出 Keyframe'),
                                ('移到 2 秒，轉回原本的角度', '關掉 Record，按 Preview 看看')], 17, 0.12, num=True)
    callout(sl, 7.8, 3.6, 4.8, 2.0, '錄完一定要關 Record', '不然之後改的任何東西\n都會被錄進動畫裡', RED, 15)

    sl = new(dk, 'IK：拉手，手臂自己彎', C)
    box(sl, 0.8, 1.6, 5.6, 0.9, 'FK（Forward Kinematics）', PANEL, WHITE, 18)
    text(sl, 0.8, 2.6, 5.6, 0.9, '轉肩膀 → 手肘跟著動 → 手跟著動\n（Record 錄動畫就是這樣）', 16, DIM, align=PP_ALIGN.CENTER)
    box(sl, 6.9, 1.6, 5.6, 0.9, 'IK（Inverse Kinematics）', PANEL, MINT, 18, line=MINT)
    text(sl, 6.9, 2.6, 5.6, 0.9, '指定「手要到哪」→\n肩膀和手肘自己算角度', 16, WHITE, align=PP_ALIGN.CENTER)
    table(sl, 0.8, 3.75, [2.8, 8.9], [['名詞', '意思'], ['IK Manager 2D', '掛在角色最上層，管理這個角色所有的 IK'],
                                      ['Solver', '一條「要用 IK 算的 Bone 鏈」'], ['Effector', '鏈的最末端（這裡是手）'],
                                      ['Target', 'Effector 要去的地方（場景裡的 HandTarget）']], 16, 0.52)

    sl = new(dk, 'IK Solver 三種', C, 'IK Manager 2D 的 Solvers 清單按 + 會看到這三種')
    table(sl, 0.8, 1.7, [2.3, 2.6, 6.8], [['Solver', 'Bone 鏈長度', '適合'], [('Limb', MINT), '剛好兩節', '手臂、腿。只有一種彎法，用 Flip 選方向'],
                                          [('CCD', YEL), '任意長', '尾巴、觸手'], [('FABRIK', YEL), '任意長', '繩子、長脖子，動起來比較順']], 18, 0.7)
    callout(sl, 0.8, 4.9, 11.7, 1.35, '這章用 Limb', '手臂是「上臂 + 前臂」兩節，最後一節「手」當 Effector', MINT, 16)

    sl = new(dk, '步驟四：設定 Limb Solver', C)
    image(sl, ann(shot('ch5_limb'), 'ch5_limb', [(5, 163, 480, 213, 3), (5, 214, 480, 238, 4)], crop=(0, 0, 481, 420)),
          0.7, 1.45, 5.4, 4.7)
    bullets(sl, 6.6, 1.5, 6.1, [('RigHero 加 IK Manager 2D', ''), ('Solvers 按 + > Limb', '會多出一個 Limb 物件'),
                                ('Effector 拖入「手」的 Bone', 'Target 拖入 HandTarget（綠色小圈圈）'),
                                ('手肘往奇怪的方向彎？勾 Flip', '')], 17, 0.12, num=True)

    sl = new(dk, '步驟五：寫腳本', C, 'Scripts/Student/Ch5_MouseTarget.cs：讓 HandTarget 跟著滑鼠')
    code(sl, 0.7, 1.55, 6.7, 3.2, '''// ────────── TODO 5-A ──────────
// 讀出滑鼠的螢幕座標，換成世界座標，
// 存進 goal。記得處理 z。

// ────────── TODO 5-B ──────────
// 讓這個物件往 goal 移動。
// 先直接設成 goal，可以動了再改成平滑跟隨''', 15)
    callout(sl, 0.7, 5.0, 6.7, 1.4, '試錯：綠圈圈消失了？', '跟第 3 章一樣的 z 陷阱：它跑到攝影機後面去了', RED, 15)
    image(sl, game('Ch5_Skeleton_answer'), 7.8, 1.55, 4.9, 2.76)
    text(sl, 7.8, 4.45, 4.9, 0.9, '用手碰到 3 顆星星就過關\nFollow Speed：3 很飄、60 黏在滑鼠上', 15, DIM, align=PP_ALIGN.CENTER)


def chapter6(dk):
    C = '第 6 章'
    divider(dk, 6, 'Animation Event', '讓動畫在對的那一格呼叫你的函式', 30,
            crop_img(game('Ch6_AnimEvent_play2'), 'ch6_div', (0, 330, 1300, 1080)), '目標：揮劍打碎木箱')

    sl = new(dk, 'Animation Event 是什麼？', C, '在 Animation Clip 的時間軸上插一面小旗子：播到那裡，就呼叫你的函式')
    ix, iy, iw, ih = image(sl, ANN + '/attack_strip.png', 1.5, 2.2, 10.3, 2.2, border=False)
    fx = ix + iw * (2 / 4)                       # 第 2 格開始的位置
    rect(sl, fx - 0.04, iy - 0.25, 0.08, ih * 0.62, fill=YEL, shape=MSO_SHAPE.RECTANGLE)
    flag = sl.shapes.add_shape(MSO_SHAPE.ISOSCELES_TRIANGLE, Inches(fx), Inches(iy - 0.3), Inches(0.35), Inches(0.3))
    flag.rotation = 90; flag.fill.solid(); flag.fill.fore_color.rgb = YEL; flag.line.fill.background()
    text(sl, fx + 0.35, iy - 0.42, 3.0, 0.45, 'AttackHit()', 18, YEL, True, mono=True)
    text(sl, 0.8, 4.85, 11.8, 1.4, 'Hero_Attack 有 4 格。在第 2 格（劍揮到前方）放一個 Animation Event，Function 選 AttackHit。\n'
         '動畫播到第 2 格的瞬間，Unity 就會去呼叫 Player 身上的 AttackHit()。', 17, WHITE)

    sl = new(dk, '為什麼不在按 J 的時候直接打碎？', C)
    image(sl, crop_img(game('Ch6_AnimEvent_play1'), 'ch6_p1', (0, 330, 1300, 1080)), 0.7, 1.5, 5.8, 3.4)
    image(sl, crop_img(game('Ch6_AnimEvent_play2'), 'ch6_p2', (0, 330, 1300, 1080)), 6.8, 1.5, 5.8, 3.4)
    text(sl, 0.7, 5.0, 5.8, 0.5, '按下 J 的瞬間：劍還沒揮到', 16, DIM, align=PP_ALIGN.CENTER)
    text(sl, 6.8, 5.0, 5.8, 0.5, '第 2 格：劍揮到前方，木箱才碎', 16, MINT, True, align=PP_ALIGN.CENTER)
    callout(sl, 0.7, 5.6, 11.9, 1.0, '畫面上的動作，跟遊戲裡的效果，要對在同一個時間點', '', YEL, 16)

    sl = new(dk, 'Animation Event 常見用途', C)
    table(sl, 0.8, 1.7, [3.4, 4.2, 4.1], [['用途', '放在哪一格', '呼叫的函式（例）'],
                                          ['腳步聲、揚塵', 'Run 動畫腳著地的那幾格', 'Footstep()'],
                                          ['攻擊判定', 'Attack 動畫揮到前方的那一格', 'AttackHit()'],
                                          ['發射子彈、放特效', '手往前推的那一格', 'Shoot()'],
                                          ['動作結束通知', '動畫的最後一格（第 7 章）', 'AttackEnd()']], 17, 0.65)

    sl = new(dk, '四條規則', C, '記不得就打開 Ch6_HeroAttack.cs 最上面的說明')
    bullets(sl, 0.8, 1.6, 11.6, [('函式要是 public', 'Event 的 Function 下拉選單才找得到'),
                                 ('腳本要掛在「有 Animator 的那個 GameObject」上', '這章是 Player'),
                                 ('參數最多一個：float、int、string 或 Object', '也可以沒有參數'),
                                 ('名字要完全一樣', '找不到時 Console 會出現下面這行錯誤')], 19, 0.14, num=True)
    code(sl, 0.8, 5.55, 11.8, 0.9, "AnimationEvent 'AttackHit' on animation 'Hero_Attack_Ch6' has no receiver!", 15)

    sl = new(dk, '這章的 Animator：Trigger 與 Has Exit Time', C)
    image(sl, ann(shot('ch6_animator'), 'ch6_animator', [(894, 356, 1144, 406, None)], crop=(190, 95, 1420, 420)), 0.7, 1.5, 6.6, 1.8)
    image(sl, ann(shot('ch6_exit'), 'ch6_exit', [(0, 276, 480, 302, None)], crop=(0, 0, 481, 330)), 7.7, 1.5, 4.9, 3.3)
    text(sl, 0.7, 3.55, 6.8, 3.0, '・Attack 是 Trigger：扣一下扳機，被 Any State → Attack 用掉就自己放開\n'
         '・右邊是 Attack → Idle 這條箭頭：Has Exit Time「打勾」！\n'
         '　攻擊動作要播完才回 Idle —— 第 4 章說要關掉，這裡剛好相反\n'
         '・關不關，看的是「要不要等動畫播完」', 15, WHITE)

    sl = new(dk, '步驟一：寫要被呼叫的函式', C, 'Scripts/Student/Ch6_HeroAttack.cs')
    code(sl, 0.7, 1.5, 7.3, 5.0, '''// ────────── TODO 6-A ──────────（Update 裡）
// 按下 J 的那一幀，讓 Animator 播攻擊動畫。

public void AttackHit()
{
    // ────────── TODO 6-B ──────────
    // 找出 AttackCenter() 周圍 attackRadius 內
    // 所有的碰撞器，是木箱的就打碎。
}

public void Footstep()
{
    // ────────── TODO 6-C ──────────
    // 在腳邊冒一團煙（顏色用 dustColor）。
}''', 14)
    table(sl, 8.3, 1.5, [2.6, 1.9], [['要用的', '是什麼'], ['anim.SetTrigger', '扣 Trigger'], ['Physics2D\n.OverlapCircleAll', '圓裡所有碰撞器'],
                                     ['GetComponent\n<Breakable>()', '是不是木箱'], ['box.Break()', '打碎'], ['GDCFx.Puff', '冒煙'],
                                     ['move.FeetPosition()', '腳底位置']], 13, 0.62)

    sl = new(dk, 'Player 身上有什麼', C, '選 Player 看 Inspector')
    image(sl, ann(shot('ch6_player'), 'ch6_player', [(0, 271, 480, 296, 1), (0, 298, 480, 323, 2), (0, 325, 480, 350, 3),
                                                    (5, 418, 480, 470, 4)], crop=(0, 0, 481, 560)), 0.7, 1.45, 5.4, 5.2)
    table(sl, 6.6, 1.55, [0.5, 2.6, 3.0], [['', '元件', '作用'], ['1', 'Animator', 'Controller：Hero_Ch6'],
                                           ['2', 'Hero Anim Driver', '第 4 章的參考做法：Speed、Grounded、面向'],
                                           ['3', 'Ch6_HeroAttack', '你要寫的腳本'], ['4', 'Attack Reach / Radius', '劍打得到多遠、範圍多大']], 14, 0.72)
    text(sl, 6.6, 5.4, 6.0, 0.8, '選取 Player 時，Scene 視窗的黃色圓圈 = 攻擊範圍', 15, YEL)

    sl = new(dk, '步驟二：在 Animation 視窗加 Event', C, '選 Player → Window > Animation > Animation')
    image(sl, ann(shot('ch6_attack_anim'), 'ch6_anim', [(0, 110, 255, 131, 1), (446, 108, 474, 132, 3), (1035, 104, 1070, 128, 4)],
                  crop=(0, 55, 1300, 220)), 0.7, 1.45, 11.9, 1.9)
    bullets(sl, 0.8, 3.55, 11.6, [('Clip 選單選 Hero_Attack_Ch6', '（這張圖是教師版，已經加好 Event）'),
                                  ('點時間軸上方的刻度，把紅線移到第 2 格', '劍揮到前方的那一格'),
                                  ('按 Add Event（有 + 號的小旗子按鈕）', ''),
                                  ('時間軸上多出一面白色小旗子', '下一頁：選它要呼叫哪個函式')], 17, 0.05, num=True)

    sl = new(dk, '步驟三：選 Function', C, '點那面小旗子，Inspector 會變成 Animation Event 的設定')
    image(sl, ann(shot('ch6_event_insp'), 'ch6_ev', [(5, 98, 480, 124, None)], crop=(0, 0, 481, 170)), 0.7, 1.55, 6.0, 2.1)
    bullets(sl, 7.1, 1.6, 5.6, [('Function 下拉選 AttackHit', '只會列出 Player 身上的 public 函式'),
                                ('換到 Hero_Run_Ch6', ''), ('第 1、4 格（腳著地）各加一個 Event', 'Function 選 Footstep')], 17, 0.15, num=True)
    callout(sl, 0.7, 4.1, 6.0, 2.3, '小技巧', '想調時間：直接拖動小旗子\n想刪掉：選小旗子按 Delete\n函式有參數時，下面會多出欄位可以填', MINT, 15)

    sl = new(dk, '完成！看看 Event 的效果', C, '按 J：劍揮到第 2 格才碎；上面的木箱會掉下來補位')
    for k in range(4):
        image(sl, crop_img(game(f'Ch6_AnimEvent_play{k + 1}'), f'ch6_seq{k}', (450, 380, 1150, 1000)), 0.7 + k * 3.05, 1.6, 2.85, 2.5)
        text(sl, 0.7 + k * 3.05, 4.2, 2.85, 0.4, ['舉劍', '命中：AttackHit()', '上面的掉下來', '再打一次'][k], 14,
             MINT if k == 1 else DIM, k == 1, PP_ALIGN.CENTER)
    text(sl, 0.7, 5.0, 11.9, 1.0, '跑起來時，腳邊每一步都會揚起灰塵：那是 Run 動畫第 1、4 格的 Footstep()', 16, WHITE)

    sl = new(dk, '親手試錯', C)
    table(sl, 0.8, 1.6, [4.0, 4.1, 3.6], [['試試看', '會發生什麼', '學到什麼'],
                                          ['拿掉 AttackHit 的 public', 'Function 下拉找不到它', '只能呼叫 public 函式'],
                                          ['把 Event 拖到第 0 格', '劍還舉在頭上，木箱就碎了', '時間點決定手感'],
                                          ['函式改名成 AttackHit2', 'Console 出現 no receiver', '名字要完全一樣'],
                                          ['Event 加在 Animations/Hero_Run', '第 4 章出現 no receiver', '共用的 Clip 不要亂加']], 15, 0.7)


def chapter7(dk):
    C = '第 7 章'
    divider(dk, 7, 'State Machine', '用程式決定角色「現在在做什麼」', 30,
            crop_img(game('Ch7_StateMachine_play2'), 'ch7_div', (0, 330, 1300, 1080)), '目標：寫出主角的狀態機')

    sl = new(dk, 'State Machine 是什麼？', C, '同一時間只會處在一個 State，照規則換到下一個')
    cols = [(RGBColor(0x2E, 0xC4, 0x6B), '綠燈'), (RGBColor(0xF5, 0xC5, 0x18), '黃燈'), (RGBColor(0xE7, 0x4C, 0x3C), '紅燈')]
    for i, (c, n) in enumerate(cols):
        cx = 1.3 + i * 4.0
        o = sl.shapes.add_shape(MSO_SHAPE.OVAL, Inches(cx), Inches(1.9), Inches(1.4), Inches(1.4))
        o.fill.solid(); o.fill.fore_color.rgb = c; o.line.fill.background()
        text(sl, cx - 0.3, 3.4, 2.0, 0.5, n, 20, WHITE, True, PP_ALIGN.CENTER)
    arrow(sl, 2.85, 2.6, 5.15, 2.6, label='30 秒到')
    arrow(sl, 6.85, 2.6, 9.15, 2.6, label='3 秒到')
    arrow(sl, 10.0, 3.95, 2.0, 3.95, color=DIM, label='30 秒到', loff=(0, 0.05), lcolor=DIM)
    table(sl, 0.8, 4.6, [2.6, 9.1], [['名詞', '在紅綠燈裡'], ['State', '亮哪一種燈（永遠只有一種）'],
                                     ['Transition', '從一種燈換到另一種'], ['Condition', '什麼時候換（30 秒到）']], 16, 0.5)

    sl = new(dk, '主角的 State Machine', C)
    pos = {'Idle': (1.0, 1.9), 'Attack': (5.6, 1.9), 'Run': (1.0, 5.1), 'Jump': (5.6, 5.1)}
    for n, (x, y) in pos.items():
        box(sl, x, y, 2.2, 0.85, n, PANEL if n != 'Idle' else ORANGE, WHITE, 22, line=MINT)
    arrow(sl, 3.2, 2.1, 5.6, 2.1, label='按 J')
    arrow(sl, 5.6, 2.55, 3.2, 2.55, color=MINT, label='AttackEnd()（Event）', loff=(0, 0.08), lcolor=MINT)
    arrow(sl, 1.5, 2.75, 1.5, 5.1, label='按方向鍵', loff=(-0.85, -0.15))
    arrow(sl, 2.6, 5.1, 2.6, 2.75, label='放開', loff=(0.65, -0.15))
    arrow(sl, 3.2, 5.52, 5.6, 5.52, label='離地')
    arrow(sl, 6.2, 5.1, 3.2, 2.75, label='落地', loff=(0.55, -0.1))
    callout(sl, 8.4, 2.4, 4.2, 3.5, '規則', '・同一時間只會是一個 State\n・每個 State 只管「我什麼時候該換、換去哪」\n・頭上的黃字 = 目前的 State', MINT, 15)

    sl = new(dk, '你早就用過 State Machine 了', C, 'Animator 就是一個「用畫的」State Machine')
    image(sl, crop_img(shot('ch4_animator'), 'ch4_graph', (190, 95, 1420, 420)), 0.7, 1.6, 5.9, 1.6)
    image(sl, crop_img(shot('ch7_animator'), 'ch7_graph', (190, 95, 1420, 420)), 6.9, 1.6, 5.9, 1.6)
    text(sl, 0.7, 3.4, 5.9, 0.9, '第 4 章：State + Transition（箭頭）\n切換由 Animator 決定', 16, DIM, align=PP_ALIGN.CENTER)
    text(sl, 6.9, 3.4, 5.9, 0.9, '第 7 章：只有 State，一條箭頭都沒有\n切換由「你的程式」決定', 16, MINT, True, align=PP_ALIGN.CENTER)
    callout(sl, 0.7, 4.7, 12.0, 1.5, '為什麼要改用程式？', '角色「能不能動、能不能攻擊」這種遊戲規則，跟動畫綁在一起時，\n寫在同一個地方比較好讀、好改。', YEL, 16)

    sl = new(dk, '程式的三個積木', C)
    code(sl, 0.7, 1.5, 6.3, 1.1, 'enum HeroState { Idle, Run, Jump, Attack }', 15)
    text(sl, 7.3, 1.55, 5.4, 1.0, [[('enum', YEL, True), ('：列出所有可能的 State\nstate 這個變數永遠是其中一個', WHITE, False)]], 16)
    code(sl, 0.7, 2.85, 6.3, 2.0, '''switch (state)
{
    case HeroState.Idle:  ...  break;
    case HeroState.Run:   ...  break;
}''', 15)
    text(sl, 7.3, 2.9, 5.4, 1.9, [[('switch', YEL, True), ('：每一幀只執行「目前那個 State」那一段\n每一段只做一件事：檢查條件，決定要不要換\n', WHITE, False)],
                                  [('⚠ 每個 case 結尾一定要 break;', RED, True)]], 16)
    code(sl, 0.7, 5.1, 6.3, 1.1, 'ChangeState(HeroState.Run);', 15)
    text(sl, 7.3, 5.15, 5.4, 1.2, [[('ChangeState()', YEL, True), ('（已寫好）：換 State 的唯一入口\n記下新 State、anim.Play 播動畫、更新頭上的字', WHITE, False)]], 16)

    sl = new(dk, 'Idle：寫好的範例', C, 'Ch7_HeroStateMachine.cs，照著它的樣子寫其他 State')
    code(sl, 0.7, 1.55, 7.6, 2.5, '''case HeroState.Idle:
    if (attackPressed) ChangeState(HeroState.Attack);
    else if (!grounded) ChangeState(HeroState.Jump);
    else if (moving) ChangeState(HeroState.Run);
    break;''', 16)
    bullets(sl, 0.8, 4.35, 7.6, [('else if：同一幀只換一次', '條件同時成立時，寫在前面的優先（攻擊 > 跳 > 走）'),
                                 ('條件都不成立：什麼都不做', '留在 Idle')], 17, 0.1)
    table(sl, 8.7, 1.55, [1.9, 2.1], [['變數', '意思'], ['attackPressed', '這一幀按了 J'], ['moving', '有按左右鍵'],
                                      ['grounded', '站在地上'], ['falling', '垂直速度 ≤ 0']], 14, 0.55)

    sl = new(dk, 'TODO 7-A ~ 7-C', C)
    code(sl, 0.7, 1.45, 7.4, 5.0, '''case HeroState.Run:
    // ────────── TODO 7-A ──────────
    // 按 J → Attack；腳離地 → Jump；
    // 放開方向鍵 → Idle。
    break;

case HeroState.Jump:
    // ────────── TODO 7-B ──────────
    // 站到地上，而且已經不是往上衝 → Idle
    break;

void ChangeState(HeroState next) { ...
    // ────────── TODO 7-C ──────────
    // 進入 Attack → move.enabled = false，水平速度歸零
    // 進入其他 State → 恢復移動''', 13)
    table(sl, 8.4, 1.45, [1.1, 3.2], [['TODO', '想一想'], ['7-A', '三個條件誰優先？看 Idle 怎麼排'],
                                      ['7-B', '為什麼不能只看 grounded？\n（跳上台階時還在往上衝）'], ['7-C', '進入 Attack 時要做的事\n＝狀態機的 OnEnter']], 13, 0.95)

    sl = new(dk, 'Attack 怎麼結束？', C, '程式狀態機 + Animation Event：動畫決定「何時結束」，程式決定「結束後去哪」')
    ix, iy, iw, ih = image(sl, ANN + '/attack_strip.png', 1.5, 2.1, 10.3, 2.2, border=False)
    for frac, name, col, ty in [(2 / 4, 'AttackHit()', YEL, -0.55), (3.8 / 4, 'AttackEnd()', MINT, -0.55)]:
        fx = ix + iw * frac
        rect(sl, fx - 0.04, iy - 0.2, 0.08, ih * 0.62, fill=col, shape=MSO_SHAPE.RECTANGLE)
        text(sl, fx - 2.5 if col == YEL else fx - 1.2, iy + ty, 2.4, 0.4, name, 17, col, True,
             PP_ALIGN.RIGHT if col == YEL else PP_ALIGN.LEFT, mono=True)
    code(sl, 2.5, 4.7, 8.3, 1.4, '''public void AttackEnd()        // 由 Event 呼叫（已寫好）
{
    ChangeState(HeroState.Idle);
}''', 15)

    sl = new(dk, '除錯：頭上的 State', C, 'Play 時，頭上的黃字和 Inspector 的 State 欄位會即時變化')
    image(sl, crop_img(game('Ch7_StateMachine_play2'), 'ch7_atk', (450, 380, 1150, 1000)), 0.7, 1.5, 3.4, 3.0)
    image(sl, crop_img(game('Ch7_StateMachine_play4'), 'ch7_idle', (450, 380, 1150, 1000)), 4.3, 1.5, 3.4, 3.0)
    text(sl, 0.7, 4.6, 3.4, 0.4, '攻擊中：Attack', 15, YEL, True, PP_ALIGN.CENTER)
    text(sl, 4.3, 4.6, 3.4, 0.4, 'AttackEnd 之後：Idle', 15, MINT, True, PP_ALIGN.CENTER)
    image(sl, ann(shot('ch7_player'), 'ch7_player', [(5, 390, 480, 416, None)], crop=(0, 260, 481, 480)), 8.1, 1.5, 4.6, 2.2)
    text(sl, 8.1, 3.9, 4.6, 1.2, '卡在 Jump 下不來？\n→ 7-B 還沒寫好', 16, WHITE)

    sl = new(dk, '親手試錯', C)
    table(sl, 0.8, 1.6, [4.3, 4.1, 3.3], [['試試看', '會發生什麼', '學到什麼'],
                                          ['7-B 只寫 if (grounded)，跳上台階', '還在往上衝就變 Idle，動畫閃一下', '想清楚「真的落地」'],
                                          ['7-C 只鎖住、不恢復', '打一下之後再也不能動', '進入和離開要成對'],
                                          ['拿掉某個 case 的 break;', '編譯錯誤（CS0163）', 'C# 的 switch 規則'],
                                          ['改 Animator 裡 State 的名字', 'State could not be found', 'anim.Play 的名字要一致']], 15, 0.72)

    sl = new(dk, 'Animator vs 程式：什麼時候用哪個？', C)
    table(sl, 0.8, 1.6, [2.4, 4.6, 4.7], [['', 'Animator（第 4 章）', '程式 State Machine（這章）'],
                                          ['怎麼做', '拉箭頭、設 Conditions', 'enum + switch'],
                                          ['好處', '看得到、不用寫程式、有淡入淡出', '邏輯集中、好讀好改，能順便做遊戲規則'],
                                          ['適合', '單純的動畫切換', '角色行為和動畫綁在一起時']], 16, 0.75)
    callout(sl, 0.8, 4.9, 11.7, 1.4, '實際遊戲常常兩個一起用', '程式決定「角色現在在做什麼」，Animator 負責「動畫怎麼播得好看」', MINT, 16)


def chapter8(dk):
    C = '第 8 章'
    divider(dk, 8, '自由創作時間', '用今天學到的東西，做一個你自己的關卡', '剩下的時間', game('Ch8_Free_answer'), '目標：從起點走到終點')

    sl = new(dk, '必做 1：畫出一條路', C, 'Ch8_Free：起點和終點之間什麼都沒有')
    image(sl, ann(shot('ch8_palette'), 'ch8_palette', [(25, 68, 370, 92, None)], crop=(0, 0, 481, 300)), 0.7, 1.6, 5.4, 3.3)
    table(sl, 6.6, 1.6, [3.1, 2.9], [['想畫的東西', 'Active Tilemap'], ['地面、平台、冰、紅磚', 'Ground'],
                                     ['岩漿（碰到回起點）', 'Hazard'], ['背景牆', 'Background'], ['樹叢（擋在角色前面）', 'Foreground'], ['小草', 'Decor']], 16, 0.52)
    text(sl, 0.7, 5.2, 5.4, 1.0, '角色約可跳 2 格高、4 格遠\n主角沿用你第 7 章寫的 State Machine', 16, DIM)

    sl = new(dk, '必做 2：用木箱設計關卡', C, '把 Assets/_GDC/Prefabs/Crate 拖進 Scene 視窗就能放一個木箱')
    image(sl, crop_img(game('Ch8_Free_answer'), 'ch8_crate', (900, 300, 1920, 1080)), 0.7, 1.5, 5.6, 4.3)
    bullets(sl, 6.7, 1.6, 6.0, [('Prefab：做好一次、到處重複用', '拖進來就有 Rigidbody、Collider、Breakable'),
                                ('疊 3 個以上就跳不過去', '一定要用 J 打碎'),
                                ('讓關卡「有設計」', '木箱後面接岩漿、冰上的木箱、\n打碎後掉下來變踏腳石…')], 17, 0.2, num=True)

    sl = new(dk, '加分題（挑喜歡的做）', C)
    table(sl, 0.8, 1.6, [3.2, 8.5], [['題目', '提示'],
                                     ['新的 Animation Event', 'Hero_Run_Ch7 加 Footstep，State Machine 腳本加 public void Footstep()'],
                                     ['新的 State：Fall', 'enum 加 Fall；Jump 時垂直速度變負就換 Fall；Animator 也要加 Fall State'],
                                     ['自己的 Rule Tile', 'Create > 2D > Tiles > Rule Tile，照第 2 章加規則'],
                                     ['帶入挖磚', '把 Ch3_TileProbe 掛到這章的 Player']], 15, 0.8)

    sl = new(dk, '自由創作時間怎麼進行', C)
    rows = [('先做完必做', '路走得通、有岩漿、至少一堆要打碎的木箱'), ('再挑加分題', ''),
            ('卡住就舉手', '也可以開 Teacher/Scenes 的教師版對照'),
            ('場景改壞了？', '選單 GDC > 重建章節 > 第 8 章（腳本不會被動到）'),
            ('最後 10 分鐘：作品展示', '每人 1 分鐘，玩給大家看')]
    bullets(sl, 0.8, 1.6, 7.6, rows, 18, 0.1, num=True)
    rect(sl, 8.9, 1.6, 3.8, 4.6)
    text(sl, 9.1, 1.75, 3.4, 0.5, '展示時說說看', 19, MINT, True)
    text(sl, 9.1, 2.35, 3.4, 3.6, '・你的關卡哪裡最難？\n・木箱放在哪、為什麼？\n・你的 State Machine\n　有幾個 State？\n・下次想加什麼？', 16, WHITE, spacing=1.5)


def animation_section(dk):
    chapter4(dk); chapter5(dk); chapter6(dk); chapter7(dk); chapter8(dk)


def compress(path):
    """把簡報裡的 PNG 縮到最長邊 1800、降成 256 色（檔案小一半以上，截圖文字仍清楚）。"""
    import zipfile
    tmp = path + '.tmp'
    with zipfile.ZipFile(path) as zin, zipfile.ZipFile(tmp, 'w', zipfile.ZIP_DEFLATED) as zout:
        for item in zin.infolist():
            data = zin.read(item.filename)
            if item.filename.startswith('ppt/media/') and item.filename.endswith('.png'):
                im = Image.open(io.BytesIO(data))
                if im.mode not in ('RGB', 'RGBA', 'P'): im = im.convert('RGBA')
                if im.mode != 'P':
                    m = max(im.size)
                    if m > 1800:
                        k = 1800 / m; im = im.resize((int(im.width * k), int(im.height * k)), Image.LANCZOS)
                    method = Image.Quantize.FASTOCTREE if im.mode == 'RGBA' else Image.Quantize.MEDIANCUT
                    buf = io.BytesIO(); im.quantize(256, method=method).save(buf, 'PNG', optimize=True)
                    if len(buf.getvalue()) < len(data): data = buf.getvalue()
            zout.writestr(item, data)
    os.replace(tmp, path)


# ══════════════════════════════════════════════════════════════════════════

def main():
    if not os.path.exists(LOGO):
        sys.exit('找不到 logo.png（社團 LOGO，透明背景）')
    src = Presentation(FULL)              # 讀取現有的完整版：第 5～33 頁原封不動複製
    tilemap_pages = [src.slides[i] for i in range(4, 33)]
    assert '介面導覽' in ''.join(s.text_frame.text for s in tilemap_pages[0].shapes if s.has_text_frame)
    sheet_image(); attack_strip(); rig_images()

    # 完整版
    dk = Deck()
    title_slide(dk); learn_slide(dk); flow_slide(dk); before_slide(dk)
    for s in tilemap_pages: copy_slide(dk, s)
    update_slide(dk)
    animation_section(dk)
    end_slide(dk)
    dk.prs.save(FULL + '.new')

    # 本週版
    wk = Deck()
    title_slide(wk, week2=True); update_slide(wk); recap_slide(wk); learn_slide(wk, week2=True); flow_slide(wk, week2=True)
    copy_slide(wk, tilemap_pages[0])      # 介面導覽
    copy_slide(wk, tilemap_pages[1])      # 兩個鐵則
    animation_section(wk)
    end_slide(wk)
    wk.prs.save(WEEK2)

    os.replace(FULL + '.new', FULL)
    compress(FULL); compress(WEEK2)
    print('完整版', len(dk.prs.slides), '頁；本週版', len(wk.prs.slides), '頁')


if __name__ == '__main__':
    main()
