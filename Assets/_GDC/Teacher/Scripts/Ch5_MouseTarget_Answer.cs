using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>第 5 章解答（教師版場景使用）。說明見 Teacher/Answers/Ch5_MouseTarget.txt</summary>
public class Ch5_MouseTarget_Answer : MonoBehaviour
{
    public float followSpeed = 15f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || cam == null) return;

        // 5-A
        Vector3 goal = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        goal.z = 0f;

        // 5-B
        transform.position = Vector3.Lerp(transform.position, goal, followSpeed * Time.deltaTime);
    }
}
