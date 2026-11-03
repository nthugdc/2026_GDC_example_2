# 第 8 章 · 自由創作

場景：`Assets/_GDC/Scenes/Ch8_Free.unity`

起點和終點之間什麼都沒有。用學到的東西，**做一個你自己的關卡**。

這一章的主角沿用**你第 7 章寫的 State Machine**（頭上會顯示 State），按 **J** 可以攻擊。
如果第 7 章還沒寫完，角色可能會卡在 Run 或 Jump —— 先回去把 7-A、7-B 補完。

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

### 2. 用木箱設計關卡

- 把 `Assets/_GDC/Prefabs/Crate` 拖進 Scene 視窗就能放一個木箱（這就是 **Prefab**：做好一次、到處重複用）
- 疊 3 個以上就跳不過去，一定要用 J 打碎
- 終點島上已經示範了一堆，可以參考

### 3. 讓關卡「有設計」

挑至少一個：

- 木箱後面接岩漿：打碎之後要馬上跳
- 冰面上的木箱：滑過去就停不下來
- 高台上的木箱：打碎之後掉下來，變成踏腳石

---

## 加分題（挑喜歡的做）

<details>
<summary>新的 Animation Event</summary>

在 `Animations/Ch7/Hero_Run_Ch7` 加 Footstep 事件，並在你的 `Ch7_HeroStateMachine` 加一個 `public void Footstep()`。
（GDCFx.Puff、move.FeetPosition() 的用法看第 6 章）
</details>

<details>
<summary>新的 State</summary>

在 enum 加一個 `Fall`（往下掉）：Jump 的時候垂直速度變負就換成 Fall。
Animator 裡也要加一個叫 Fall 的 State（Clip 可以先用 Jump 的）。
</details>

<details>
<summary>做一組自己的 Rule Tile</summary>

Project 按右鍵 → **Create → 2D → Tiles → Rule Tile**。
Default Sprite 放 `Art/Tiles/Ground_Inner`，再照第 2 章的方法加規則。
</details>

<details>
<summary>把第 3 章的挖磚帶過來</summary>

把 `Ch3_TileProbe` 也掛到這章的 Player 上，Inspector 指定 Ground 圖層和三種磁磚。
</details>

---

## 最後 10 分鐘：作品展示

每人 1 分鐘，玩給大家看，說說看：

- 你的關卡哪裡最難？
- 用了哪些磁磚、哪些圖層？
- 木箱放在哪裡、為什麼？
- 下次想加什麼？

---

## 常見問題

**Q：拖進來的木箱沒有掉下來 / 打不碎？**
A：要用 `Prefabs/Crate`，不要只拖 `Art/Tiles/Crate` 那張圖（圖片沒有碰撞和 Breakable）。

**Q：場景改壞了？**
A：選單 **GDC > 重建章節 > 第 8 章**（你寫的腳本不會被動到，但你畫的關卡會被清掉）。
