# 第 6 章 · 自由創作

場景：`Assets/_GDC/Scenes/Ch6_Free.unity`

起點和終點之間什麼都沒有。用今天學到的東西，**做一個你自己的關卡**。

---

## 必做

### 1. 畫出一條路

- Tile Palette 選 **Palette_All**（全部磁磚都在裡面）
- **Active Tilemap 要選對層**：

| 想畫的東西 | 畫在哪層 |
|---|---|
| 地面、平台、冰、紅磚 | Ground（有碰撞） |
| 岩漿 | Hazard（碰到回起點） |
| 背景牆 | Background |
| 樹叢（擋在角色前面） | Foreground |
| 小草裝飾 | Decor |

> 角色一次約跳 2 格高、4 格遠。

### 2. 寫一支自己的腳本：腳步揚起灰塵

這次**沒有填空**，從零開始：

1. `Assets/_GDC/Scripts/Student` 按右鍵 → **Create → MonoBehaviour Script**，命名 `Ch6_Footstep`
2. 寫一個 **public** 的函式，名字叫 `Footstep`，沒有參數
3. 在裡面呼叫 `GDCFx.Puff(位置, 顏色)`，在角色腳邊冒一團煙
4. 把腳本掛到 Player 上

用得到的東西：

| 東西 | 說明 |
|---|---|
| `GDCFx.Puff(Vector3 position, Color color)` | 在 position 冒一團會淡掉的煙（已經寫好） |
| `GetComponent<HeroMove>()` | 拿到移動腳本 |
| `HeroMove.FeetPosition()` | 腳底的世界座標 |
| `HeroMove.IsGrounded()` | 有沒有站在地上 |
| `new Color(r, g, b, a)` | 顏色，每個數字 0 ~ 1 |

### 3. Animation Event：讓動畫呼叫你的函式

1. 選 Player，Animation 視窗左上選 **Hero_Run_Ch6**
2. 時間軸移到**腳著地**的那一格（看預覽，大概是第 1 格和第 4 格）
3. 按時間軸上方的 **Add Event**（小旗子＋號）
4. 點那個小旗子，Function 選 `Footstep`

按播放跑起來，每一步都會揚起灰塵！

> Animation Event 就是「動畫播到這裡時，幫我呼叫某個函式」。
> 腳步聲、攻擊判定、特效出現的時機，都是這樣跟動畫對齊的。

> 這一章的角色沿用你**第 4 章寫的 `Ch4_HeroAnimator`**。如果第 4 章還沒寫完，角色會一直停在 Idle —— 回去補完就好。

---

## 加分題（挑喜歡的做）

<details>
<summary>做一組自己的 Rule Tile</summary>

Project 按右鍵 → **Create → 2D → Tiles → Rule Tile**。
Default Sprite 放 `Art/Tiles/Ground_Inner`，再照第 2 章的方法加規則。
</details>

<details>
<summary>把第 3 章的挖磚帶過來</summary>

把 `Ch3_TileProbe` 也掛到這章的 Player 上，Inspector 指定 Ground 圖層和三種磁磚。
</details>

<details>
<summary>Animation Event 的進階用法</summary>

Jump 動畫第 0 格加一個事件，呼叫另一個函式，冒顏色不一樣的煙。
</details>

---

## 常見問題

**Q：Function 下拉選單找不到 Footstep？**
A：三件事：函式是 `public` 嗎？腳本有掛在 Player 上嗎？腳本有沒有編譯錯誤（Console 紅字）？

**Q：Console 出現「AnimationEvent 'Footstep' has no receiver」？**
A：動畫事件加了，但 Player 身上沒有任何腳本有 `Footstep` 這個函式。
