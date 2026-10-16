# 第 4 章 · Sprite Sheet 動畫（35 分鐘）

場景：`Assets/_GDC/Scenes/Ch4_SpriteSheet.unity`
要改的檔案：`Assets/_GDC/Scripts/Student/Ch4_HeroAnimator.cs`

一按播放：角色怎麼是**一整排小人**？因為這張圖還沒切！

---

## Sprite Sheet 是什麼

一張大圖裡排了好幾格動作，每一格就是動畫的一幀。播放時**快速換圖**，看起來就在動 —— 跟手翻書、跟第 2 章的岩漿一樣。

`Hero_Sheet_Unsliced` 有 12 格，每格 128 × 128 像素：

| 格子 | 動作 |
|---|---|
| 0 ~ 3 | 待機 Idle |
| 4 ~ 9 | 跑步 Run |
| 10、11 | 跳躍 Jump |

---

## 步驟一：切圖

1. Project 選 `Assets/_GDC/Art/Characters/Hero_Sheet_Unsliced`
2. Inspector 的 **Sprite Mode** 改成 **Multiple**，按 **Apply**
3. 按 **Open Sprite Editor**
4. 左上 **Slice** → Type 選 **Grid By Cell Size** → Pixel Size 填 `128`、`128` → **Slice**
5. 右上 **Apply**，關掉視窗

Project 裡那張圖現在可以展開，裡面有 `_0` 到 `_11` 十二張小圖。

## 步驟二：換掉角色的圖

選 Player，**Sprite Renderer** 的 **Sprite** 換成 `Hero_Sheet_Unsliced_0`。按播放，終於只剩一個人了。

## 步驟三：做動畫片段

1. 選 Player，**Window → Animation → Animation** 開啟動畫視窗
2. 按 **Create**，存成 `Idle`
3. Project 裡把 `_0` ~ `_3` 四張圖**一起選起來**，拖進動畫視窗的時間軸
4. 動畫視窗左上的下拉選單 → **Create New Clip**，存成 `Run`，拖入 `_4` ~ `_9`
5. 一樣做 `Jump`，拖入 `_10`、`_11`

> 動畫太快？動畫視窗右上角 ⋮ → **Show Sample Rate**，Samples 改小一點（例如 Idle 6、Run 12）。

## 步驟四：Animator 狀態機

**Window → Animation → Animator**。剛剛做的三個片段已經變成三個「狀態」方塊。

1. 左側 **Parameters** 分頁按 **+**：
   - `Speed`（Float）
   - `Grounded`（Bool），**記得打勾**當預設值
2. 在狀態上按右鍵 → **Make Transition**，拉出這些箭頭，點箭頭設定 **Conditions**：

| 從 | 到 | 條件 |
|---|---|---|
| Idle | Run | Speed **Greater** 0.1 |
| Run | Idle | Speed **Less** 0.1 |
| Any State | Jump | Grounded **false** |
| Jump | Idle | Grounded **true** |

3. **每一條箭頭**都要取消勾選 **Has Exit Time**，並把 **Transition Duration** 改成 `0`

### 一定要親手試的錯誤：Has Exit Time

先留一條箭頭的 Has Exit Time 打勾，按播放。你會發現**按下去要等一下才開始跑**。

Has Exit Time = 「這段動畫要播完才准切換」。走路循環要播完一圈才切，所以動作會延遲。
動作遊戲幾乎都會把它關掉。

## 步驟五：寫腳本（TODO 4-A ~ 4-C）

Animator 只會看參數，**誰來更新參數？** 就是 `Ch4_HeroAnimator.cs`。

| TODO | 目標 |
|---|---|
| 4-A | 每一幀把 `Speed` 設成水平速度有多快 |
| 4-B | 每一幀把 `Grounded` 設成有沒有站在地上 |
| 4-C | 按左就翻轉圖片，按右就不翻 |

### 一定要親手試的錯誤：負的速度

4-A 先直接把水平速度傳進去，不加絕對值。往右跑很正常 —— **往左跑卻一直是 Idle**。

因為往左的速度是負的，`-5 > 0.1` 不成立。這就是為什麼要用絕對值。

---

## 常見問題

**Q：Console 出現黃字「Parameter 'Speed' does not exist」？**
A：腳本裡的名字跟 Animator 的參數名不一樣。大小寫也要一模一樣。

**Q：角色一直在跳躍動畫？**
A：`Grounded` 的預設值沒打勾，或 4-B 還沒寫。

**Q：Animation 視窗按 Create 沒反應？**
A：要先在 Hierarchy 選取 Player。
