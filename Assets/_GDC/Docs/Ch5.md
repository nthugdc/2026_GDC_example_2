# 第 5 章 · 2D Skeletal Animation 與 IK（35 分鐘）

場景：`Assets/_GDC/Scenes/Ch5_Skeleton.unity`
要改的檔案：`Assets/_GDC/Scripts/Student/Ch5_MouseTarget.cs`

---

## Skeletal Animation vs Sprite Sheet

| | Sprite Sheet（第 4 章） | Skeletal Animation（這章） |
|---|---|---|
| 美術要畫 | 每個動作的每一個 frame | **只要一張圖** |
| 動畫怎麼做 | 換圖 | 轉 **Bone**，圖跟著彎 |
| 動作數量多時 | 圖檔爆炸 | 只多幾個 Animation Clip |
| 適合 | 像素風、小角色 | 大角色、動作很多、要接 IK |

## 三個名詞

| 名詞 | 是什麼 |
|---|---|
| **Bone**（骨頭） | 一段有長度、可以旋轉的線。Bone 可以有 parent：轉上臂，前臂和手會跟著動 |
| **Mesh**（網格） | 把圖切成很多小三角形。每個三角形的頂點（vertex）可以被 Bone 拉動 |
| **Weight**（權重） | 每個頂點「跟哪根 Bone 走、跟多少」。手肘附近的頂點可能一半跟上臂、一半跟前臂 |

Bone 一轉 → 頂點照 Weight 移動 → 三角形變形 → 圖就彎了。

---

## 步驟一：在 Skinning Editor 建 Bone

1. Project 選 `Assets/_GDC/Art/Characters/RigHero` → **Open Sprite Editor**
2. 左上角下拉選單從 Sprite Editor 切到 **Skinning Editor**

| 工具（左側） | 用途 |
|---|---|
| **Preview Pose** | 拖動 Bone 預覽變形（不會存檔）；**Restore Pose** 還原 |
| **Edit Bone** | 移動、旋轉已經建好的 Bone |
| **Create Bone** | 一節一節點出 Bone |
| **Auto Geometry** | 自動產生 Mesh |
| **Auto Weights** | 自動算 Weight |
| **Weight Brush** | 用筆刷手動修 Weight |

3. **Create Bone**：
   - 從屁股點一下、往上點一下 → 脊椎；繼續往上點 → 胸口、頭（每點一下就多一節，按右鍵結束）
   - 從肩膀開始點：上臂 → 前臂 → 手（**右手至少要三節**，等一下 IK 要用最後一節）
4. **Auto Geometry** → 右下 **Generate For All Visible**
5. **Auto Weights** → **Generate**
6. 右上 **Apply**

> 用 **Preview Pose** 拖拖看 Bone，圖有跟著彎就成功了。

### 修 Weight：Weight Brush

Preview Pose 時如果頭被手臂拉走、肚子跟著腿彎，就是 Weight 不對：

1. 選 **Weight Brush**，在右下面板選要「加強」的 Bone（例如 Head）
2. 在要修的地方刷幾下（顏色 = 那根 Bone 的影響範圍）
3. 再用 Preview Pose 確認

## 步驟二：Sprite Skin → Create Bones

1. 選場景裡的 `RigHero`
2. **Add Component → Sprite Skin**
3. 按 **Create Bones**，Hierarchy 裡的 RigHero 底下多出一串 Bone 物件（就是 Transform，可以直接旋轉）

## 步驟三：用 Record 錄一段 Idle

1. 選 RigHero，Animation 視窗 → **Create**，存成 `RigIdle`
2. 按紅色 **Record** 鍵（時間軸變紅 = 錄影中）
3. 時間 0：在 Hierarchy 選頭或手的 Bone，轉一下再轉回來（會產生 Keyframe）
4. 時間 1 秒：用旋轉工具（E）把 Bone 轉一點
5. 時間 2 秒：轉回跟 0 秒一樣
6. 關掉 Record，按播放

> Record 模式下改的任何數值都會被錄成 Keyframe。**錄完一定要關掉**，不然之後改東西都會被錄進去。

## 步驟四：IK —— 拉手，手臂自己彎

**IK（Inverse Kinematics）**：平常是「轉肩膀 → 手跟著動」（Forward Kinematics）；IK 反過來，**指定手要到哪，肩膀和手肘自己算角度**。

| 名詞 | 意思 |
|---|---|
| **IK Manager 2D** | 掛在角色最上層，管理這個角色所有的 IK |
| **Solver** | 一條「要用 IK 算的 Bone 鏈」 |
| **Effector** | 鏈的最末端（這裡是手） |
| **Target** | Effector 要去的地方（場景裡的 HandTarget） |

| Solver 種類 | 鏈的長度 | 適合 |
|---|---|---|
| **Limb** | 剛好兩節 | 手臂、腿（只有一種彎法，用 Flip 選方向） |
| **CCD** | 任意長 | 尾巴、觸手 |
| **FABRIK** | 任意長 | 繩子、長脖子，動起來比較順 |

1. 選 RigHero，**Add Component → IK Manager 2D**
2. Solvers 清單按 **+** → **Limb**
3. 新出現的 Limb 物件上：**Effector** 拖入「手」的 Bone，**Target** 拖入 `HandTarget`（綠色小圈圈）
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
A：Skinning Editor 裡還沒按 Apply，或這張圖沒有 Bone。

**Q：圖彎得很醜、身體被手臂拉走？**
A：Weight 不好。用 Weight Brush 修，或重新 Generate。

**Q：IK 沒反應？**
A：Effector 要選**最末端**那節（手），而且那節往上要至少還有兩節。

**Q：錄好的 Idle 跟 IK 打架？**
A：IK 在動畫之後計算，所以 IK 控制的那隻手會蓋過動畫，這是正常的。
