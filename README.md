# 《磁磚與動畫工作坊》

NTHU GDC（清華大學遊戲創作社）Unity 2D 練習專案 — 給**完全沒有程式基礎**的新社員。

七個章節，約 3.5 小時的單次工作坊，帶你認識兩大主題：**Tilemap（磁磚地圖）** 與 **2D 動畫**（Sprite Sheet 與骨骼動畫）。

## 取得專案

> ⚠️ **請先安裝 Git LFS，再 clone。**
> 本專案的中文字型與圖檔存放在 Git LFS。沒裝就 clone 的話，
> 拉下來的會是幾行文字的「指標檔」而不是真正的檔案，
> Unity 會載不到字型，**場景裡的中文看板會整片變成方框（□□□）**。

```bash
# 1. 安裝 Git LFS（每台電腦只需要做一次）
git lfs install

# 2. 下載專案
git clone https://github.com/nthugdc/2026_GDC_example_2.git
```

### 怎麼裝 Git LFS

| 系統 | 做法 |
|---|---|
| Windows | 安裝 [Git for Windows](https://git-scm.com/download/win)（已內含 LFS），然後跑 `git lfs install` |
| macOS | `brew install git-lfs` |
| Ubuntu / Debian | `sudo apt install git-lfs` |

### 已經 clone 了才發現忘記裝？

不用重新 clone，補救兩行就好：

```bash
git lfs install
git lfs pull
```

### 怎麼確認 LFS 有正常運作

```bash
git lfs ls-files
```

有列出 `GDC_CJK.ttf` 之類的檔案就對了。
或者直接看 `Assets/_GDC/Art/Fonts/GDC_CJK.ttf` 的檔案大小 —
應該是 **約 4 MB**；如果只有 1 KB 左右，代表那是指標檔，LFS 沒生效。

## 開啟專案

1. 用 **Unity 6000.3.10f1** 開啟本專案
2. 打開 `Assets/_GDC/Scenes/Ch0_Grid.unity`
3. 按 ▶

> 第一次開啟會花幾分鐘匯入資源，屬正常現象。
> 操作：**A / D** 移動、**空白鍵** 跳、**R** 重玩、**N / P** 下一章 / 上一章。

## 📖 完整講義

**→ [Assets/_GDC/Docs/README.md](Assets/_GDC/Docs/README.md) ←**

裡面有環境須知、時間分配、資料夾地圖與給幹部的備課說明；每一章另有獨立講義（`Docs/Ch0.md` ~ `Ch6.md`）。

> ⚠️ 開始寫程式前請務必先看講義開頭：本專案只能用**新版 Input System**（不能用 `Input.GetKeyDown`）。
> 另外，**播放中改的數值一按停止就會還原**，要改參數請先停止播放。

## 📽️ 社課簡報

[`Slides/NTHU_GDC_Tilemap_Animation.pptx`](Slides/NTHU_GDC_Tilemap_Animation.pptx)（54 頁，可直接拖進 Google 簡報開啟）
・[PDF 版](Slides/NTHU_GDC_Tilemap_Animation.pdf)

## 章節一覽

| 章 | 主題 | 要做什麼 | 時間 |
|---|---|---|---|
| 0 | Grid 與 Tilemap | 只改 Inspector，認識格子與圖層 | 15 分 |
| 1 | Tile Palette | 自己畫地圖、加上碰撞 | 25 分 |
| 2 | Rule Tile 與動畫磁磚 | 改磁磚資產，補 Rule Tile 規則 | 30 分 |
| 3 | 用程式讀寫 Tilemap | 第一支腳本：踩冰、岩漿、挖磚放磚（6 個 TODO） | 35 分 |
| 4 | Sprite Sheet 動畫 | 切圖、Animator 狀態機、3 個 TODO | 35 分 |
| 5 | 2D 骨骼動畫與 IK | 綁骨、錄動畫、讓手跟著滑鼠（2 個 TODO） | 35 分 |
| 6 | 自由創作 | 自己畫關卡、自己寫 Animation Event 腳本 | 剩下的時間 |

## 資料夾

```
Assets/_GDC/
├── Scenes/        七章場景（要玩的）
├── Scripts/
│   ├── Student/   有 TODO 的腳本（要改的）
│   └── Core/      基礎建設（不用改）
├── Tiles/         磁磚資產與 Tile Palette
├── Art/           程式產生的磁磚圖、角色圖 + 中文字型
├── Animations/    動畫片段與 Animator Controller
├── Teacher/       教師版場景、參考解答、驗證工具
└── Docs/          講義

Assets/Editor/GDC/ 教材建置工具（學生可忽略）
```

## 給幹部

選單 **GDC**：

- **建置 > 全部重建** — 字型、Layer/Tag、美術素材、全部章節（含教師版）一次重做
- **重建章節 > 第 N 章** — 學生把某一章改壞了，一鍵還原
- **工具 > 檢查場景文字** — 掃描全部場景有沒有缺字或看板溢出

教師版場景在 `Assets/_GDC/Teacher/Scenes/`，每一章都能通關；解答與說明在 `Teacher/Answers/`。
`GDCAutopilot`（`Teacher/Scripts/`）可在 Play Mode 加到 Player 上，讓角色自動跑完關卡，上課時用來示範「這關是過得去的」。

> 第 2、5 章有需要在 Inspector 與 Skinning Editor 裡手動操作的步驟，建議社課前自己完整走一次。

## 授權

`Assets/_GDC/Art/Fonts/GDC_CJK.ttf` 為 Droid Sans Fallback 的副本，
授權 Apache License 2.0，用於顯示繁體中文。詳見同資料夾的 `LICENSE-GDC_CJK.txt`。
