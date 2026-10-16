using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>第 3 章解答（教師版場景使用）。說明見 Teacher/Answers/Ch3_TileProbe.txt</summary>
public class Ch3_TileProbe_Answer : MonoBehaviour
{
    public Tilemap groundMap;
    public TileBase iceTile;
    public TileBase lavaTile;
    public TileBase brickTile;
    public float normalAcceleration = 60f;
    public float iceAcceleration = 3f;

    HeroMove move;
    Respawner respawner;
    Camera cam;

    void Awake()
    {
        move = GetComponent<HeroMove>();
        respawner = GetComponent<Respawner>();
        cam = Camera.main;
    }

    void Update()
    {
        if (groundMap == null) return;

        // 3-A
        Vector3Int feetCell = groundMap.WorldToCell(move.FeetPosition());
        TileBase under = groundMap.GetTile(feetCell);

        // 3-B
        if (under == iceTile) move.acceleration = iceAcceleration;
        else move.acceleration = normalAcceleration;

        // 3-C
        if (under == lavaTile)
        {
            respawner.Respawn();
            GameManager.Instance.Fail("踩到岩漿了！回到起點");
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        bool dig = mouse.leftButton.wasPressedThisFrame;
        bool place = mouse.rightButton.wasPressedThisFrame;
        if (!dig && !place) return;

        // 3-D
        Vector3 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        world.z = 0f;
        Vector3Int mouseCell = groundMap.WorldToCell(world);

        // 3-E
        if (dig && groundMap.GetTile(mouseCell) == brickTile)
        {
            groundMap.SetTile(mouseCell, null);
            GameManager.Instance.AddScore(1);
        }

        // 3-F
        if (place && !groundMap.HasTile(mouseCell) && GameManager.Instance.score > 0)
        {
            groundMap.SetTile(mouseCell, brickTile);
            GameManager.Instance.AddScore(-1);
        }
    }
}
