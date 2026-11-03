# 第 7 章 · State Machine（30 分鐘）

場景：`Assets/_GDC/Scenes/Ch7_StateMachine.unity`
要改的檔案：`Assets/_GDC/Scripts/Student/Ch7_HeroStateMachine.cs`

---

## State Machine 是什麼

**同一時間只會處在一個 State（狀態），並且依照規則從一個 State 換到另一個。**

生活裡的例子：紅綠燈。

```
 綠燈 ──(30 秒到)──▶ 黃燈 ──(3 秒到)──▶ 紅燈 ──(30 秒到)──▶ 綠燈
```

- 永遠只會亮一種燈（一個 State）
- 每一種燈只關心「我什麼時候該換、換成誰」（Transition）

我們的主角也一樣：

```
          按 J                    動畫播完（AttackEnd）
  Idle ─────────▶ Attack ───────────────────────▶ Idle
   │ ▲
 走 │ │ 停                 落地
   ▼ │          離地
   Run ─────────────────▶ Jump ──────────▶ Idle
```

## 你早就用過 State Machine 了

第 4 章的 **Animator** 就是一個「用畫的」State Machine：方塊是 **State**，箭頭是 **Transition**，箭頭上的 **Conditions** 就是規則。

這一章把同一件事**用程式寫出來**。打開這章的 Animator 看看：四個 State 都在，**一條箭頭都沒有** —— 換 State 全部由腳本決定。

## 程式裡的三個積木

| 積木 | 寫法 | 作用 |
|---|---|---|
| **enum** | `enum HeroState { Idle, Run, Jump, Attack }` | 列出所有可能的 State |
| **switch** | `switch (state) { case HeroState.Idle: … break; }` | 每一幀只執行「目前那個 State」的那一段 |
| **ChangeState()** | `ChangeState(HeroState.Run);` | 換 State 的唯一入口：記下新 State、播動畫、更新頭上的字 |

`ChangeState` 裡用的是 `anim.Play("Run")`：直接叫 Animator 播某個 State，不用 Parameter、不用箭頭。

> ⚠ C# 的每個 `case` 結尾一定要有 `break;`，少了會編譯失敗。

---

## TODO

`Idle` 那一段已經寫好，照著它的樣子寫：

| TODO | State | 規則 |
|---|---|---|
| 7-A | Run | 按 J → Attack；離地 → Jump；放開方向鍵 → Idle |
| 7-B | Jump | 站到地上，而且不是往上衝（`falling`）→ Idle |
| 7-C | ChangeState | 進入 Attack 時不准移動（`move.enabled = false`、水平速度歸零）；進入其他 State 時恢復 |

**Attack 什麼時候結束？** 不是程式檢查的 —— 是攻擊動畫最後一格的 **Animation Event** 呼叫 `AttackEnd()`（第 6 章學的）。這章的 Clip 已經幫你放好了。

### 頭上的黃字

角色頭上會顯示目前的 State。寫錯的時候先看它：卡在 Jump 下不來？就是 7-B 還沒寫好。

---

## 一定要親手試的錯誤

| 試試看 | 會發生什麼 | 學到什麼 |
|---|---|---|
| 7-B 只寫 `if (grounded)`，然後跳上台階 | 還在往上衝就變回 Idle，動畫閃一下（腳底偵測先碰到台階邊緣） | 條件要想清楚「什麼時候算真的落地」 |
| 7-C 只寫鎖住、不寫恢復 | 打一下之後角色再也不能動 | 「進入」和「離開」要成對 |
| 拿掉某個 `case` 的 `break;` | 編譯錯誤 | C# 的 switch 規則 |

---

## Animator vs 程式：什麼時候用哪個？

| | Animator（第 4 章） | 程式 State Machine（這章） |
|---|---|---|
| 怎麼做 | 拉箭頭、設 Conditions | enum + switch |
| 好處 | 看得到、不用寫程式、有過場淡入淡出 | 邏輯集中在一個地方，好讀好改；能順便做「鎖移動」這類遊戲規則 |
| 適合 | 單純的動畫切換 | 角色行為（能不能動、能不能攻擊）跟動畫綁在一起時 |

實際遊戲常常兩個一起用：程式決定「角色現在在做什麼」，Animator 負責「動畫怎麼播得好看」。

---

## 常見問題

**Q：一開始就卡在 Jump？**
A：角色出生在半空中，掉下來時進了 Jump。7-B 寫好就會回到 Idle。

**Q：Console 出現「Animator.GotoState: State could not be found」？**
A：`anim.Play` 的名字跟 Animator 裡的 State 名字不一樣。這章是用 enum 的名字，不要改 Animator 裡的 State 名稱。

**Q：攻擊後不會回到 Idle？**
A：Hero_Attack_Ch7 的最後一格要有 AttackEnd 事件（已經放好了），而且 `AttackEnd()` 要是 public。
