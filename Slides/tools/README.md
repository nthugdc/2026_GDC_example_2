# 簡報產生工具

| 產出 | 內容 |
|---|---|
| `Slides/NTHU_GDC_Tilemap_Animation.pptx` | 完整版（第 0～8 章） |
| `Slides/NTHU_GDC_Animation_Week2.pptx` | 第二堂（從動畫開始：更新專案、上週回顧、第 4～8 章） |

**Tilemap 的頁面（完整版第 5～33 頁）不會被重新產生**：`build_slides.py` 每次都從現有的完整版把那幾頁原封不動複製過來。
要改 Tilemap 的頁面，請直接在 PowerPoint / Google 簡報改完整版那幾頁（改完下次重產也會保留）。

## 重新產生

```bash
cd Slides/tools
python3 -m venv .venv && .venv/bin/pip install python-pptx numpy pillow   # 第一次
.venv/bin/python build_slides.py
soffice --headless --convert-to pdf --outdir .. ../NTHU_GDC_Tilemap_Animation.pptx ../NTHU_GDC_Animation_Week2.pptx
```

`build_slides.py` 需要截圖（放在 `.cache/`，不進 git）。沒有的話先照下面重拍。

## 重拍截圖

需要：Unity Editor 開著這個專案（`unity status` 是 ready）、Unity CLI、COSMIC 桌面的 `cosmic-screenshot`（第一次會跳權限視窗）。
截圖期間 Unity 視窗要佔滿螢幕、不要切到別的視窗。

```bash
# 編輯器畫面：shot.sh 名稱 cs/腳本.cs [trim]
./shot.sh ch4_sheet cs/ch4_sheet.cs trim
PRE=cs/ch4_animation_pre.cs ./shot.sh ch4_animation cs/ch4_animation.cs   # Animation 視窗要先選物件
./shot.sh ch4_animator cs/ch4_animator.cs; SHOT_WAIT=3 ./shot.sh ch4_animator cs/again.cs   # 剛開的視窗要拍第二次

# 遊戲畫面：game.sh 名稱 [場景路徑]
./game.sh Ch6_AnimEvent_answer Assets/_GDC/Teacher/Scenes/Ch6_AnimEvent_Answer.unity
```

`cs/` 底下每支腳本對應一張截圖，最後 `return` 要裁切的視窗範圍；共用的小工具在 `lib.cs`。
攻擊瞬間的畫面（`Ch6_AnimEvent_play1~4`、`Ch7_StateMachine_play1~4`）是在 Play Mode 把 `Time.timeScale` 調到 0.06、
按 J 之後連拍的。

### 已知的坑

- Wayland 下 Unity 拍不到自己的編輯器視窗，所以用系統截圖再依視窗位置裁切；螢幕縮放倍率由 `shot.sh` 自動算
- COSMIC 是平鋪式視窗管理：**不要開浮動視窗**，所有工具視窗都用 `Dock()` 停靠在主視窗裡；
  殘留的浮動 Animator / Animation / Sprite Editor 視窗要先關掉，否則 Unity 主視窗會被擠成一半
- Animator 視窗會記住上次的縮放：`cs/ch*_animator.cs` 會先清掉 `m_ViewTransforms`
- Animator 的 Parameters 清單用程式開啟時常常是空的，所以簡報裡的 Parameter 用表格呈現
- 截圖上的標註框座標：先用 `grid.py` 產生帶格線的預覽圖再讀座標，標完要整張看一次
