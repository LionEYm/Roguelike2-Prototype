using UnityEngine;

public class HiddenRoom : MonoBehaviour
{
    /*
     * Wall encoding (1 bit per direction):
     * 0 = regular wall
     * 1 = destroyable wall
     *
     * Bit layout:
     * Bit 0 : Up
     * Bit 1 : Down
     * Bit 2 : Left
     * Bit 3 : Right
     */
    [SerializeField]
    private byte wallData;

    public enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    [Header("Wall Prefabs")]
    [SerializeField] private GameObject RegularWallPrefab;
    [SerializeField] private GameObject DestroyableWallPrefab;

    [Header("Wall Transforms")]
    [SerializeField] private Transform UpAnchor;
    [SerializeField] private Transform DownAnchor;
    [SerializeField] private Transform LeftAnchor;
    [SerializeField] private Transform RightAnchor;

    public void Initialize()
    {
        SpawnWall(Direction.Up, UpAnchor);
        SpawnWall(Direction.Down, DownAnchor);
        SpawnWall(Direction.Left, LeftAnchor);
        SpawnWall(Direction.Right, RightAnchor);
    }

    // ---------------- WALL LOGIC ----------------

    private void SpawnWall(Direction dir, Transform anchor)
    {
        if (anchor == null)
        {
            Debug.LogWarning($"{name}: Missing anchor for {dir}");
            return;
        }

        GameObject prefab = IsDestroyable(dir)
            ? DestroyableWallPrefab
            : RegularWallPrefab;

        if (prefab == null)
        {
            Debug.LogWarning($"{name}: Missing wall prefab for {dir}");
            return;
        }

        Instantiate(prefab, anchor.position, anchor.rotation, anchor);
    }


    // -------- PUBLIC API --------

    public bool IsDestroyable(Direction dir)
    {
        return ((wallData >> (int)dir) & 1) == 1;
    }

    public void SetDestroyable(Direction dir, bool destroyable)
    {
        if (destroyable)
        {
            wallData |= (byte)(1 << (int)dir);   // set bit
        }
        else
        {
            wallData &= (byte)~(1 << (int)dir);  // clear bit
        }
    }
}
