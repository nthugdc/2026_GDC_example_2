# 第 4 章 · Sprite Sheet 動畫（35 分鐘）

場景：`Assets/_GDC/Scenes/Ch4_SpriteSheet.unity`
要改的檔案：`Assets/_GDC/Scripts/Student/Ch4_HeroAnimator.cs`

一按播放：角色怎麼是**一整排小人**？因為這張圖還沒切！

---

## 先認識三個名詞

| 名詞 | 是什麼 | 在哪裡 |
|---|---|---|
| **Animation Clip** | 一段動畫（例如「跑步」）。記錄「第幾秒換成哪張圖」 | Project 裡的 `.anim` 檔 |
| **Animator Controller** | 一張流程圖：有哪些 State、什麼時候從一個 State 換到另一個 | Project 裡的 `.controller` 檔 |
| **Animator**（component） | 掛在角色身上的元件，負責「照著 Controller 播動畫」 | Player 的 Inspector |

關係是：**Animator** 用一個 **Animator Controller**，Controller 裡的每個 **State** 放一個 **Animation Clip**。

---

## Sprite Sheet 是什麼

一張大圖裡排了好幾格動作，每一格就是動畫的一 **frame**（幀）。播放時**快速換圖**，看起來就在動 —— 跟手翻書、跟第 2 章的岩漿一樣。

`Hero_Sheet_Unsliced` 有 12 格，每格 128 × 128 像素：

| 格子 | 動作 |
|---|---|
| 0 ~ 3 | Idle（待機） |
| 4 ~ 9 | Run（跑步） |
| 10、11 | Jump（跳躍） |

---

## 步驟一：切圖（Slice）

1. Project 選 `Assets/_GDC/Art/Characters/Hero_Sheet_Unsliced`
2. Inspector 的 **Sprite Mode** 改成 **Multiple**（一張圖裡有很多張 Sprite），按 **Apply**
3. 按 **Open Sprite Editor**
4. 左上 **Slice** → Type 選 **Grid By Cell Size** → Pixel Size 填 `128`、`128` → **Slice**
5. 右上 **Apply**，關掉視窗

Project 裡那張圖現在可以展開，裡面有 `_0` 到 `_11` 十二張 Sprite。

## 步驟二：換掉角色的圖

選 Player，**Sprite Renderer** 的 **Sprite** 換成 `Hero_Sheet_Unsliced_0`。按播放，終於只剩一個人了。

## 步驟三：做 Animation Clip

**Window → Animation → Animation** 開啟 Animation 視窗，先認識它：

| 區域 | 用途 |
|---|---|
| 左上 Clip 下拉選單 | 切換要編輯哪個 Clip；最下面的 **Create New Clip** 可以新增 |
| **Preview** / 播放鍵 | 在編輯模式預覽動畫（不用按 Play） |
| 紅色 **Record** 鍵 | 錄影模式：這時改的任何數值都會變成 Keyframe（第 5 章會用） |
| **Timeline**（時間軸） | 上面的刻度是時間，菱形是 **Keyframe**（關鍵影格） |
| **Samples**（Sample Rate） | 一秒幾格。右上 ⋮ → Show Sample Rate 才看得到 |

1. 選 Player，按 **Create**，存成 `Idle`
2. Project 裡把 `_0` ~ `_3` 四張圖**一起選起來**，拖進 Timeline —— 每張圖變成一個 Keyframe
3. 左上 Clip 選單 → **Create New Clip**，存成 `Run`，拖入 `_4` ~ `_9`
4. 一樣做 `Jump`，拖入 `_10`、`_11`

| Samples | 效果 |
|---|---|
| 60（預設） | 快到看不清楚 |
| 12 | Run 剛好 |
| 6 | Idle 剛好（慢慢呼吸） |

## 步驟四：Animator 狀態機

**Window → Animation → Animator**。剛剛做的三個 Clip 已經變成三個 **State**（方塊）。

| 方塊 | 意思 |
|---|---|
| 橘色 State | **Default State**：一開始播的那個（右鍵 → Set as Layer Default State 可以換） |
| **Entry** | 起點，連到 Default State |
| **Any State** | 「不管現在在哪個 State」—— 從這裡拉出去的箭頭，任何時候都能觸發 |
| **Exit** | 離開這個 State Machine（這章用不到） |

### Parameter（參數）

左側 **Parameters** 分頁按 **+**。Parameter 是腳本跟 Animator 溝通的「訊號」，有四種：

| 型別 | 值 | 適合 | 腳本怎麼設 |
|---|---|---|---|
| **Float** | 小數 | 速度、血量 | `SetFloat` |
| **Int** | 整數 | 第幾種武器 | `SetInteger` |
| **Bool** | 是 / 否 | 在不在地上 | `SetBool` |
| **Trigger** | 扣一下就自動放開 | 攻擊、受傷（第 6 章會用） | `SetTrigger` |

這章要加：`Speed`（Float）、`Grounded`（Bool，**記得打勾**當預設值）。

### Transition（轉換）

在 State 上按右鍵 → **Make Transition**，拉出箭頭；點箭頭，在 Inspector 設定：

| 欄位 | 意思 |
|---|---|
| **Has Exit Time** | 打勾 = 目前的動畫要播到 Exit Time 才准切換 |
| **Exit Time** | 播到幾成才切（1 = 播完） |
| **Transition Duration** | 兩段動畫交疊淡入淡出的時間。Sprite 動畫設 0 |
| **Conditions** | 什麼條件成立才切 |

| 從 | 到 | Conditions |
|---|---|---|
| Idle | Run | Speed **Greater** 0.1 |
| Run | Idle | Speed **Less** 0.1 |
| Any State | Jump | Grounded **false** |
| Jump | Idle | Grounded **true** |

**每一條箭頭**都要取消勾選 **Has Exit Time**，**Transition Duration** 改成 `0`。

### 一定要親手試的錯誤：Has Exit Time

先留一條箭頭的 Has Exit Time 打勾，按播放。你會發現**按下去要等一下才開始跑**。
動作遊戲的操作動畫幾乎都會把它關掉。（第 6 章的攻擊動畫會遇到「該打勾」的情況，到時候對照看看。）

## 步驟五：寫腳本（TODO 4-A ~ 4-C）

Animator 只看 Parameter，**誰來更新 Parameter？** 就是 `Ch4_HeroAnimator.cs`。

| TODO | 目標 |
|---|---|
| 4-A | 每一幀把 `Speed` 設成水平速度有多快 |
| 4-B | 每一幀把 `Grounded` 設成有沒有站在地上 |
| 4-C | 按左就翻轉圖片（flipX），按右就不翻 |

### 一定要親手試的錯誤：負的速度

4-A 先直接把水平速度傳進去，不加絕對值。往右跑很正常 —— **往左跑卻一直是 Idle**。
因為往左的速度是負的，`-5 > 0.1` 不成立。這就是為什麼要用 `Mathf.Abs`。

## 除錯：Play 的時候看 Animator

按播放後，選 Player 並開著 Animator 視窗：

- 正在播的 State 底下會有**藍色進度條**
- Parameters 分頁的數值會**即時跳動**
- 箭頭被觸發的瞬間會亮起來

動畫不對的時候，先看這裡：Parameter 有沒有變？變了但箭頭沒觸發 → Conditions 寫錯；Parameter 沒變 → 腳本沒寫對。

---

## 常見問題

**Q：Console 出現黃字「Parameter 'Speed' does not exist」？**
A：腳本裡的名字跟 Animator 的 Parameter 名不一樣。大小寫也要一模一樣。

**Q：角色一直在 Jump 動畫？**
A：`Grounded` 的預設值沒打勾，或 4-B 還沒寫。

**Q：Animation 視窗按 Create 沒反應？**
A：要先在 Hierarchy 選取 Player。
