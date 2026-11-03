# 第 6 章 · Animation Event（30 分鐘）

場景：`Assets/_GDC/Scenes/Ch6_AnimEvent.unity`
要改的檔案：`Assets/_GDC/Scripts/Student/Ch6_HeroAttack.cs`

三堆木箱擋住了路。按 **J** 揮劍把它打碎 —— 但要讓劍在「揮到前面的那一瞬間」才打碎，不是一按 J 就碎。

---

## Animation Event 是什麼

在 Animation Clip 的時間軸上插一面小旗子：**動畫播到那裡時，Unity 會去呼叫你腳本裡的函式**。

```
Hero_Attack   [ 0 舉劍 ][ 1 揮下 ][ 2 命中 ][ 3 收招 ]
                                    ▲
                              AttackHit()
```

為什麼不在按 J 的時候直接打碎？因為劍還舉在頭上，箱子就碎了 —— 看起來很假。
**畫面上的動作，和遊戲裡的效果要對在同一個時間點**，這就是 Animation Event 在做的事。

常見的用法：

| 用途 | 放在哪一格 |
|---|---|
| 腳步聲、揚塵 | 跑步動畫腳著地的那幾格 |
| 攻擊判定 | 攻擊動畫揮到前方的那一格 |
| 發射子彈、放特效 | 手往前推的那一格 |
| 動作結束通知 | 動畫的最後一格（第 7 章會用到） |

## 四條規則

1. 函式要是 **public**，Function 下拉選單才找得到
2. 腳本要掛在「**有 Animator 的那個 GameObject**」上
3. 參數最多一個，型別只能是 **float、int、string、Object**（也可以沒有參數）
4. 名字要完全一樣。Event 找不到函式時，Console 會出現：
   `AnimationEvent 'XXX' on animation 'YYY' has no receiver!`

---

## 步驟一：寫好要被呼叫的函式（TODO 6-A ~ 6-C）

| TODO | 目標 |
|---|---|
| 6-A | 按 J 時用 `anim.SetTrigger("Attack")` 播攻擊動畫 |
| 6-B | `AttackHit()`：找出劍前方範圍內的木箱，呼叫它的 `Break()` |
| 6-C | `Footstep()`：在腳邊冒一團煙 |

這一章的 Animator Controller 已經有 **Attack** 這個 **Trigger** Parameter，和 Any State → Attack 的 Transition。

### 為什麼攻擊用 Trigger？

| Parameter | 適合 |
|---|---|
| Bool | 「持續」的狀態：在不在地上 |
| **Trigger** | 「一次性」的事件：攻擊、受傷。扣一下，被 Transition 用掉就自己放開 |

### Attack → Idle 這條箭頭，Has Exit Time 是打勾的！

第 4 章說「動作遊戲幾乎都要關掉 Has Exit Time」—— 這裡是例外：
攻擊動畫要**播完**才回到 Idle，所以這條箭頭剛好需要 Has Exit Time。打開 Animator 視窗點那條箭頭看看。

## 步驟二：在 Animation 視窗加 Event

1. 選 Player，打開 **Window → Animation → Animation**
2. 左上 Clip 選單選 **Hero_Attack_Ch6**
3. 點時間軸上方的刻度，把紅線移到**第 2 格**（劍揮到前方）
4. 按 **Add Event**（時間軸左上方，有 + 號的小旗子按鈕）
5. 時間軸上多出一面白色小旗子；點它，Inspector 的 **Function** 選 **AttackHit**
6. 換到 **Hero_Run_Ch6**，在第 1、4 格（腳著地）各加一個 Event，Function 選 **Footstep**

按播放，按 J 打木箱；跑起來腳邊會揚起灰塵。

> 想調整 Event 的時間？直接拖動小旗子。想刪掉？選小旗子按 Delete。

---

## 一定要親手試的錯誤

| 試試看 | 會發生什麼 | 學到什麼 |
|---|---|---|
| 把 `AttackHit` 改成 `void AttackHit()`（拿掉 public） | Function 下拉選單找不到它 | Event 只能呼叫 public 函式 |
| 把 Event 拖到第 0 格 | 劍還舉在頭上，木箱就碎了 | 時間點決定手感 |
| 把函式名稱改成 `AttackHit2` | Console 出現 no receiver 錯誤 | 名字要完全一樣 |

## 調參數

選 Player，在 Ch6_HeroAttack 元件上：

| 欄位 | 作用 |
|---|---|
| Attack Reach | 劍打得到多遠 |
| Attack Radius | 攻擊範圍多大 |

選取 Player 時，Scene 視窗會畫出**黃色圓圈** = 攻擊範圍。

---

## 常見問題

**Q：按 J 沒反應？**
A：6-A 還沒寫，或 Trigger 名字打錯（要一模一樣：`Attack`）。

**Q：攻擊動畫有播，但木箱不會碎？**
A：Event 沒加、加錯 Clip（要加在 Hero_Attack_Ch6），或 6-B 還沒寫。

**Q：Console 出現「has no receiver」？**
A：Event 的 Function 名稱，在 Player 身上找不到同名的 public 函式。
