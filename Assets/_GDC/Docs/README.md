# 《磁磚與動畫工作坊》— NTHU GDC 2D 練習專案

給**完全沒有程式基礎**的新社員，用一個下午（約 3.5 小時）認識 Unity 2D 的兩大主題：

- **Tilemap**：用「磁磚」畫地圖，而不是一個一個擺方塊
- **2D 動畫**：逐格換圖的 **Sprite Sheet**，以及讓一張圖「長出骨頭」的 **2D 骨骼動畫**

每一章都是一個可以玩的小關卡，難度一章一章往上加：

| 章 | 主題 | 你要做的事 | 時間 |
|---|---|---|---|
| 0 | Grid 與 Tilemap | 只改 Inspector | 15 分 |
| 1 | Tile Palette | 自己畫地圖、加碰撞 | 25 分 |
| 2 | Rule Tile 與動畫磁磚 | 改磁磚資產、補規則 | 30 分 |
| 3 | 用程式讀寫 Tilemap | 第一次寫程式（填空） | 35 分 |
| — | 休息 | | 10 分 |
| 4 | Sprite Sheet 動畫 | 切圖、Animator、填空 | 35 分 |
| 5 | 2D 骨骼動畫與 IK | 綁骨、錄動畫、填空 | 35 分 |
| 6 | 自由創作 | 自己畫關卡、自己寫腳本 | 剩下的時間 |

---

## 開始之前（很重要，只有兩件事）

### 1. 這個專案只能用「新版 Input System」

網路上的教學九成會寫 `Input.GetKeyDown(KeyCode.Space)` —— **這個專案不能用，會編譯失敗。**

| 網路上的舊寫法 | 本專案的寫法 |
|---|---|
| `Input.GetKey(KeyCode.D)` | `Keyboard.current.dKey.isPressed` |
| `Input.GetKeyDown(KeyCode.Space)` | `Keyboard.current.spaceKey.wasPressedThisFrame` |
| `Input.GetMouseButtonDown(0)` | `Mouse.current.leftButton.wasPressedThisFrame` |
| `Input.mousePosition` | `Mouse.current.position.ReadValue()` |

檔案最上面要加 `using UnityEngine.InputSystem;`。

### 2. 先停止播放，再改東西

**Play Mode（播放中）改的任何數值，一按停止就會全部還原。** 要改參數、畫地圖，一律先按停止。

---

## 怎麼玩

- 開啟 `Assets/_GDC/Scenes/Ch0_Grid.unity`，按播放
- **A / D** 或方向鍵移動，**空白鍵**跳
- **R** 重玩本章、**N** 下一章、**P** 上一章
- 每章的講義在 `Assets/_GDC/Docs/ChN.md`，畫面左上角的看板是精簡版

## 把場景改壞了怎麼辦？

選單 **GDC > 重建章節 > 第 N 章**，會把那一章還原成初始狀態（你寫的腳本不會被動到）。

## 資料夾地圖

| 位置 | 內容 |
|---|---|
| `_GDC/Scenes/` | 七章的關卡 |
| `_GDC/Scripts/Student/` | **你要改的腳本**（有 `TODO` 的地方） |
| `_GDC/Scripts/Core/` | 教材的基礎建設，不用改 |
| `_GDC/Tiles/` | 磁磚資產與調色盤（Palettes） |
| `_GDC/Art/` | 磁磚圖、角色圖 |
| `_GDC/Animations/` | 動畫片段與 Animator Controller |
| `_GDC/Teacher/` | 教師版場景、解答（先自己試過再看！） |

---

## 給幹部：備課與重建

- 所有場景與美術都是程式生成的，選單 **GDC > 建置 > 全部重建** 會依序重做字型、Layer/Tag、美術、14 個場景
- 教師版場景在 `_GDC/Teacher/Scenes/`，每一章都可以直接通關
- 教師工具 `GDCAutopilot`：Play Mode 中加到 Player 上，角色會自動往右跑、遇到坑就跳，可以示範「這關是過得去的」
- 角色和磁磚的顏色在 `Scripts/Core/GDCPalette.cs`，改完重跑「GDC > 建置 > 2. 產生美術素材」
