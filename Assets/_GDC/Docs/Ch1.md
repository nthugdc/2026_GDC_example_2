# 第 1 章 · 用 Tile Palette 畫地圖（25 分鐘）

場景：`Assets/_GDC/Scenes/Ch1_Palette.unity`　　**這一章不用寫程式。**

一按播放，角色就一直往下掉。這一章你要：加上碰撞、補路、拆牆、蓋階梯。

---

## 任務一：地面沒有碰撞

Tilemap 本身只負責「畫」。要讓角色站得住，得幫它加上碰撞。

1. **停止播放**，選 `Grid` 底下的 `Ground`
2. **Add Component → Tilemap Collider 2D**
   - 現在每一格都有一個小方塊碰撞了，按播放試試，角色站得住了
3. 把 Tilemap Collider 2D 的 **Composite Operation** 改成 **Merge**
4. **Add Component → Composite Collider 2D**
   - Unity 會**自動**多加一個 **Rigidbody 2D**
5. 把那個 Rigidbody 2D 的 **Body Type** 改成 **Static**

### 為什麼要 Composite？

| 只有 Tilemap Collider 2D | 加上 Composite Collider 2D |
|---|---|
| 每一格一個方塊，一長條地面是幾十個方塊黏在一起 | 合併成一整塊 |
| 角色走過接縫有時會「卡一下」 | 平滑 |
| 碰撞器數量多，效能差 | 少很多 |

### 一定要親手試的錯誤

第 5 步先**不要**改成 Static，直接按播放。整片地面會**掉下去**！

因為 Rigidbody 2D 預設是 **Dynamic**（會受重力影響）。地面不該動，所以要 Static。

---

## 任務二：用 Tile Palette 畫地圖

1. **Window → 2D → Tile Palette**，把視窗拖到順手的地方
2. 上方下拉選單選 **Palette_Basic**
3. **Active Tilemap** 選 **Ground**（畫錯層的話碰撞會不對！）
4. 在調色盤上點一塊磁磚，再到 Scene 視窗點擊就能畫

| 工具 | 快捷鍵 | 用途 |
|---|---|---|
| 筆刷 Paint | `B` | 一格一格畫 |
| 方框 Box Fill | `U` | 拖出一個長方形一次填滿 |
| 橡皮擦 Erase | `D` | 擦掉（也可以按住 Shift 用筆刷擦） |
| 吸管 Pick | `I` | 從地圖上吸一塊磁磚來用 |

### 要完成的三件事

1. **補坑**：用筆刷把中間的坑補起來（坑太寬跳不過去）
2. **拆牆**：用橡皮擦擦掉擋路的高牆
3. **蓋階梯**：終點在高台上，用方框工具疊出一階一階的樓梯

> 角色一次大約能跳 2 格高、4 格遠。

---

## 常見問題

**Q：畫了但角色還是穿過去？**
A：Active Tilemap 選錯了。只有 Ground 這層有碰撞。

**Q：畫上去的磁磚在背景後面看不到？**
A：一樣是畫到別層去了，用 Ctrl+Z 復原，確認 Active Tilemap 是 Ground。

**Q：Scene 視窗點下去沒反應？**
A：Tile Palette 視窗上方的工具要選筆刷（B），不是選取（S）。
