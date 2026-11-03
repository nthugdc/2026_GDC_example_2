#!/bin/bash
# 截遊戲畫面（攝影機）：game.sh 輸出名稱 [場景路徑]   場景路徑不給 = 目前的場景（Play Mode 也可以）
HERE=$(cd "$(dirname "$0")" && pwd); PROJ=$(cd "$HERE/../.." && pwd); OUT=$HERE/.cache/game; mkdir -p "$OUT"
cd "$PROJ"
[ -n "$2" ] && unity --no-banner command open_scene --path "$2" >/dev/null 2>&1
unity --no-banner command capture_game_view --source camera --width 1920 --height 1080 --result-only 2>&1 | python3 -c "
import sys,json,base64
d=json.loads(sys.stdin.read()); open('$OUT/$1.png','wb').write(base64.b64decode(d['base64'])); print('saved $1')"
