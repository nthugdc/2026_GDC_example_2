#!/bin/bash
# 截一張編輯器畫面：shot.sh 名稱 cs/腳本.cs [trim]
#   腳本是 eval_file 的方法本體（前面會接上 lib.cs），最後 return "x,y,w,h"（points）或 "full"
#   環境變數 PRE=另一支 cs：先執行它、等 2 秒再執行主腳本（例如 Animation 視窗要先選物件）
#   環境變數 SHOT_WAIT：主腳本執行後等幾秒再截圖（預設 2）
# 需要：Unity Editor 開著且 unity status 是 ready；COSMIC 桌面的 cosmic-screenshot
set -e
HERE=$(cd "$(dirname "$0")" && pwd)
PROJ=$(cd "$HERE/../.." && pwd)
OUT=$HERE/.cache/shots; RAW=$HERE/.cache/raw; mkdir -p "$OUT" "$RAW"
PY=$HERE/.venv/bin/python
cd "$PROJ"
run() { cat "$HERE/lib.cs" "$1" > "$HERE/.cache/_run.cs"; unity --no-banner command eval_file --file "$HERE/.cache/_run.cs" --result-only 2>&1; }
if [ -n "$PRE" ]; then run "$HERE/$PRE" >/dev/null; sleep 2; fi
res=$(run "$HERE/$2" | python3 -c "import sys,json
d=json.load(sys.stdin); print(d.get('result') or ('ERR '+json.dumps(d)[:600]))")
case "$res" in ERR*) echo "$1: $res"; exit 1;; esac
unity --no-banner command editor_focus --result-only >/dev/null 2>&1 || true
sleep ${SHOT_WAIT:-2}
rm -f "$RAW"/*
timeout 60 cosmic-screenshot --interactive=false --modal=false --notify=false -s "$RAW" >/dev/null
# 螢幕縮放倍率：截圖寬度 ÷ 主視窗在 Unity 裡的寬度（points）。自動算，換螢幕也不用改
main=$(unity --no-banner command eval --code 'var r = UnityEditor.EditorGUIUtility.GetMainWindowPosition(); return (r.x * 2 + r.width).ToString();' --result-only 2>&1 | python3 -c "import sys,json; print(json.load(sys.stdin)['result'])")
"$PY" - "$1" "$res" "$OUT" "$RAW" "$main" <<'PY'
import sys, glob
from PIL import Image
name, res, out, raw, main = sys.argv[1:6]
im = Image.open(sorted(glob.glob(raw + '/*'))[-1]).convert('RGB')
if res != 'full':
    x, y, w, h = [float(v) for v in res.split(',')[:4]]
    s = im.width / float(main)
    im = im.crop((int(x * s), int(y * s), int((x + w) * s), int((y + h) * s)))
im.save(f'{out}/{name}.png')
print(name, res, im.size)
PY
if [ "$3" = trim ]; then "$PY" "$HERE/trim.py" "$OUT/$1.png"; fi
