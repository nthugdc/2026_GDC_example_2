# 第 5 章 · 2D 骨骼動畫與 IK（35 分鐘）

場景：`Assets/_GDC/Scenes/Ch5_Skeleton.unity`
要改的檔案：`Assets/_GDC/Scripts/Student/Ch5_MouseTarget.cs`

---

## 骨骼動畫 vs Sprite Sheet

| | Sprite Sheet（第 4 章） | 骨骼動畫（這章） |
|---|---|---|
| 美術要畫 | 每個動作的每一幀 | **只要一張圖** |
| 動畫怎麼做 | 換圖 | 轉骨頭，圖跟著彎 |
| 動作數量多時 | 圖檔爆炸 | 只多幾個動畫片段 |
| 適合 | 像素風、小角色 | 大角色、動作很多、要接 IK |

原理：把圖切成很多小三角形（**網格 Geometry**），每個頂點記住「我跟哪根骨頭走、跟多少」（**權重 Weights**）。骨頭一轉，頂點跟著動，圖就彎了。

---

## 步驟一：綁骨（Skinning Editor）

1. Project 選 `Assets/_GDC/Art/Characters/RigHero` → **Open Sprite Editor**
2. 左上角下拉選單從 Sprite Editor 切到 **Skinning Editor**
3. 左側工具列 **Create Bone**：
   - 從屁股點一下、往上點一下 → 脊椎
   - 繼續往上點 → 胸口、頭（每點一下就多一節，按右鍵結束）
   - 從肩膀開始點：上臂 → 前臂 → 手（**右手至少要三節**，等一下 IK 要用）
4. **Auto Geometry** → 右下 **Generate For All Visible**
5. **Auto Weights** → **Generate**
6. 右上 **Apply**

> 用 **Preview Pose** 工具拖拖看骨頭，圖有跟著彎就成功了。拖完按 **Reset Pose**。

## 步驟二：讓場景裡的角色長出骨頭

1. 選場景裡的 `RigHero`
2. **Add Component → Sprite Skin**
3. 按 **Create Bones**，Hierarchy 裡的 RigHero 底下多出一串骨頭物件

## 步驟三：錄一段 Idle 動畫

1. 選 RigHero，Animation 視窗 → **Create**，存成 `RigIdle`
2. 按紅色**錄影鍵**
3. 時間軸在 0 秒：什麼都不動，先在骨頭上按右鍵 Add Key（或轉一下再轉回來）
4. 移到 1 秒：在 Hierarchy 選頭或手的骨頭，用旋轉工具（E）轉一點
5. 移到 2 秒：轉回跟 0 秒一樣
6. 關掉錄影，按播放

## 步驟四：IK —— 拉手，手臂自己彎

**IK（Inverse Kinematics，反向運動學）**：平常是「轉肩膀 → 手跟著動」；IK 反過來，**指定手要到哪，肩膀和手肘自己算角度**。

1. 選 RigHero，**Add Component → IK Manager 2D**
2. Solvers 清單按 **+** → **Limb**（兩節手臂專用）
3. 新出現的 Limb 物件上：
   - **Effector**：拖入你的**手**骨頭
   - **Target**：拖入場景裡的 `HandTarget`（綠色小圈圈）
4. 手肘往奇怪的方向彎？勾 **Flip**

## 步驟五：寫腳本（TODO 5-A、5-B）

`HandTarget` 上掛著 `Ch5_MouseTarget`，讓它跟著滑鼠走 —— 手就會跟著滑鼠。用手去碰三顆星星就過關。

| TODO | 目標 |
|---|---|
| 5-A | 滑鼠的螢幕座標 → 世界座標（注意 z） |
| 5-B | 讓 HandTarget 往那裡移動 |

### 一定要親手試的錯誤：z

5-A 先不處理 z，按播放 —— 綠圈圈**消失了**。它被放到 z = -10，跑到攝影機後面去了。

### 調參數

| Follow Speed | 手感 |
|---|---|
| `3` | 慢吞吞，像在水裡 |
| `15` | 預設 |
| `60` | 幾乎黏在滑鼠上 |

---

## 常見問題

**Q：Create Bones 是灰色的？**
A：Skinning Editor 裡還沒按 Apply，或這張圖沒有骨頭。

**Q：圖彎得很醜、身體被手臂拉走？**
A：權重不好。Skinning Editor 的 **Weight Brush** 可以手動刷，或重新 Generate。

**Q：IK 沒反應？**
A：Effector 要選**最末端**那節（手），而且那節往上要至少還有兩節。
