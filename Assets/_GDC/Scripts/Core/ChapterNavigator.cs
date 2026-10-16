using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// 章節切換：R 重玩、N 下一章、P 上一章。
/// 章節順序就是 Build Settings 裡的場景順序，走到頭會繞回另一端。
/// </summary>
public class ChapterNavigator : MonoBehaviour
{
    // 教師版場景不在 Build Settings 裡，沒辦法用程式切換
    bool navigable;
    // 場景載入要等到這一幀結束才生效，先擋住避免重複觸發
    bool isLoading;

    void Awake()
    {
        navigable = SceneManager.GetActiveScene().buildIndex >= 0;
        if (navigable) return;

        Debug.Log("[GDC] 這是教師版場景（不在 Build Settings 裡），R / N / P 無法切換。" +
                  "請直接從 Project 視窗開啟你要的場景。");

        HudDisplay hud = FindFirstObjectByType<HudDisplay>();
        if (hud != null) hud.controlsHint = "教師版場景：R / N / P 無法使用";
    }

    void Update()
    {
        if (!navigable || isLoading) return;

        // 注意：本專案使用「新版 Input System」，所以不是 Input.GetKeyDown
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rKey.wasPressedThisFrame) Load(SceneManager.GetActiveScene().buildIndex);
        if (kb.nKey.wasPressedThisFrame) Step(+1);
        if (kb.pKey.wasPressedThisFrame) Step(-1);
    }

    /// <summary>往前或往後一章。走到最後一章再按 N 會繞回第 0 章，所以 N 一定有反應。</summary>
    void Step(int delta)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (count <= 0) return;

        int next = SceneManager.GetActiveScene().buildIndex + delta;
        next = ((next % count) + count) % count;   // 負數也能正確繞回去
        Load(next);
    }

    void Load(int buildIndex)
    {
        isLoading = true;
        SceneManager.LoadScene(buildIndex);
    }
}
