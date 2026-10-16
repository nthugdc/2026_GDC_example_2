using UnityEngine;

/// <summary>掉出世界或按下重生鍵時，把物件送回起點並歸零速度。</summary>
public class Respawner : MonoBehaviour
{
    [Header("重生設定")]
    [Tooltip("Y 座標低於這個值就重生")]
    public float killY = -12f;

    Vector3 startPos;
    Rigidbody2D rb;

    void Awake()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (transform.position.y < killY) Respawn();
    }

    /// <summary>回到起點。</summary>
    public void Respawn()
    {
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        if (rb != null)
        {
            // Unity 6 已把 velocity 改名為 linearVelocity
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
