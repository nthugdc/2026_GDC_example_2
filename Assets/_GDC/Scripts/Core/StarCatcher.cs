using UnityEngine;
using UnityEngine.U2D.IK;

/// <summary>
/// 第 5 章的小遊戲：用 IK 控制的手去碰星星，碰到就得分，全部碰到就過關。
/// 會自動找場景裡第一個 LimbSolver2D，用它的 Effector（手）來判定。學生不用改。
/// </summary>
public class StarCatcher : MonoBehaviour
{
    [Tooltip("手離星星多近算碰到")]
    public float catchRadius = 0.45f;
    public Transform[] stars;

    LimbSolver2D solver;
    int caught;

    void Update()
    {
        if (solver == null)
        {
            solver = FindFirstObjectByType<LimbSolver2D>();
            if (solver == null)
            {
                if (GameManager.Instance != null) GameManager.Instance.Fail("還沒有設定 IK（Limb Solver 2D）");
                return;
            }
        }

        IKChain2D chain = solver.GetChain(0);
        if (chain == null || chain.effector == null) return;
        if (GameManager.Instance != null && !GameManager.Instance.isCleared) GameManager.Instance.statusMessage = "";

        Vector3 hand = chain.effector.position;
        foreach (Transform s in stars)
        {
            if (s == null || !s.gameObject.activeSelf) continue;
            s.Rotate(0f, 0f, 90f * Time.deltaTime);
            if (Vector2.Distance(hand, s.position) > catchRadius) continue;

            s.gameObject.SetActive(false);
            GDCFx.Puff(s.position, GDCPalette.Coin, 0.5f);
            caught++;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
                if (caught >= stars.Length) GameManager.Instance.Clear("過關！手抓到所有星星了");
            }
        }
    }
}
