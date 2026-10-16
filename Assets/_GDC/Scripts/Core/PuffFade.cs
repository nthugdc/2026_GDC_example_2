using UnityEngine;

/// <summary>GDCFx.Puff 用的淡出動畫。</summary>
public class PuffFade : MonoBehaviour
{
    public float startSize = 0.35f;
    public float life = 0.4f;

    SpriteRenderer sr;
    float t;
    Color baseColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / life;
        transform.localScale = Vector3.one * Mathf.Lerp(startSize, startSize * 2.2f, k);
        transform.position += Vector3.up * Time.deltaTime * 0.6f;
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - k));
        if (k >= 1f) Destroy(gameObject);
    }
}
