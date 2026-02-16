using System.Collections.Generic;
using UnityEngine;
using static DungeonGenerator;

[CreateAssetMenu(fileName = "Dungeon Modifier Strategy Hidden", menuName = "Dungeon/Modifier Strategy/Hidden")]

///
/// This modifier adds random hidden rooms throughout the dungeon (while making sure they dont block areas)
///
public class DungeonModifierStrategyHidden : DungeonModifierStrategyBase
{
    [SerializeField] private GameObject HiddenRoomPrefab;
    [SerializeField] private int CellSizeOffset = 5;
    private int floorAmount;
    private GameObject _parent;

    private void OnValidate()
    {
        if (HiddenRoomPrefab == null || HiddenRoomPrefab.GetComponent<HiddenRoom>() == null)
        {
            Debug.LogError($"{name}: HiddenRoomPrefab must have a HiddenRoom component");
        }
    }

    public override void Initialize(CellType[,] dungeon)
    {
        CalculateFloorAmount(dungeon);
        _badCanidates = new bool[dungeon.GetLength(0), dungeon.GetLength(1)];
        _parent = new GameObject();
        _parent.name = "Hidden Rooms";
    }

    public override void Action(ref CellType[,] dungeon)
    {
        var cords = GetCords(dungeon, floorAmount);
        if (cords == (1, 1))
            return;
        int x = cords.x;
        int y = cords.y;

        // Ensure this cell is floor (hidden room occupies a floor tile)
        dungeon[x, y] = CellType.Object;
        floorAmount--;

        GameObject go = Instantiate(
            HiddenRoomPrefab,
            new Vector3(x * CellSizeOffset, 0f, y * CellSizeOffset),
            Quaternion.identity
        );
        go.transform.parent = _parent.transform;

        HiddenRoom hiddenRoom = go.GetComponent<HiddenRoom>();
        if (hiddenRoom == null)
            return;

        // ---- Determine wall types based on dungeon neighbors ----

        SetWallFromNeighbor(dungeon, x, y + 1, HiddenRoom.Direction.Up);
        SetWallFromNeighbor(dungeon, x, y - 1, HiddenRoom.Direction.Down);
        SetWallFromNeighbor(dungeon, x - 1, y, HiddenRoom.Direction.Left);
        SetWallFromNeighbor(dungeon, x + 1, y, HiddenRoom.Direction.Right);
        hiddenRoom.Initialize();


        void SetWallFromNeighbor(CellType[,] dungeon, int x, int y, HiddenRoom.Direction dir)
        {

            bool destroyable = false;

            if (x >= 0 && y >= 0 &&
                x < dungeon.GetLength(0) &&
                y < dungeon.GetLength(1))
            {
                destroyable = dungeon[x, y] == CellType.Floor;
            }

            hiddenRoom.SetDestroyable(dir, destroyable);
        }
    }

    private int[,] _visitedStamp;
    private bool[,] _badCanidates; //if true then bad.
    private int _stamp;
    private readonly Stack<(int x, int y)> _stack = new Stack<(int x, int y)>(2048);

    private (int x, int y) GetCords(CellType[,] dungeon, int totalFloors)
    {
        int width = dungeon.GetLength(0);
        int height = dungeon.GetLength(1);

        EnsureVisitedBuffer(width, height);

        const int maxAttempts = 2000;

        // We expect: after blocking one tile, we should still reach all remaining floors
        int expected = totalFloors - 1;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // 1) Pick a candidate floor tile to block
            int x, y,c=0;
            do
            {
                x = Random.Range(1, width - 1);
                y = Random.Range(1, height - 1);
                c++;
            }
            while ((dungeon[x, y] != CellType.Floor || _badCanidates[x,y]) && c<1000);

            // 2) Temporarily block it
            dungeon[x, y] = CellType.Object;

            // 3) Find a start floor tile
            if (!TryFindAnyFloor(dungeon, width, height, out int sx, out int sy))
            {
                dungeon[x, y] = CellType.Floor;
                continue;
            }

            // 4) Flood fill to count reachable floors
            int connected = FloodCountFloors(dungeon, width, height, sx, sy);

            // 5) Revert candidate before deciding
            dungeon[x, y] = CellType.Floor;

            if (connected == expected)
            {
                // caller will commit by setting Object and decrementing totalFloors once
                return (x, y);
            }
            _badCanidates[x, y] = true;
        }

        // fallback (shouldn’t happen often)
        return (1, 1);
    }

    private void EnsureVisitedBuffer(int width, int height)
    {
        if (_visitedStamp == null || _visitedStamp.GetLength(0) != width || _visitedStamp.GetLength(1) != height)
        {
            _visitedStamp = new int[width, height];
            _stamp = 0;
        }
    }

    private bool TryFindAnyFloor(CellType[,] dungeon, int width, int height, out int x, out int y)
    {
        for (int i = 1; i < width - 1; i++)
            for (int j = 1; j < height - 1; j++)
            {
                if (dungeon[i, j] == CellType.Floor)
                {
                    x = i; y = j;
                    return true;
                }
            }

        x = y = 0;
        return false;
    }

    private int FloodCountFloors(CellType[,] dungeon, int width, int height, int sx, int sy)
    {
        _stamp++;
        if (_stamp == int.MaxValue)
        {
            System.Array.Clear(_visitedStamp, 0, _visitedStamp.Length);
            _stamp = 0;
        }

        _stack.Clear();
        _stack.Push((sx, sy));
        _visitedStamp[sx, sy] = _stamp;

        int count = 0;

        while (_stack.Count > 0)
        {
            var (x, y) = _stack.Pop();
            count++;

            TryPush(x + 1, y);
            TryPush(x - 1, y);
            TryPush(x, y + 1);
            TryPush(x, y - 1);
        }

        return count;

        void TryPush(int nx, int ny)
        {
            if (nx <= 0 || ny <= 0 || nx >= width - 1 || ny >= height - 1)
                return;

            if (_visitedStamp[nx, ny] == _stamp)
                return;

            if (dungeon[nx, ny] != CellType.Floor)
                return;

            _visitedStamp[nx, ny] = _stamp;
            _stack.Push((nx, ny));
        }
    }

    public override int GetAmount(int dungeonSize)
    {
        return dungeonSize / 2;
    }

    private void CalculateFloorAmount(CellType[,] dungeon)
    {
        floorAmount = 0;
        for (int i = 0; i < dungeon.GetLength(0); i++)
        {
            for (int j = 0; j < dungeon.GetLength(1); j++)
            {
                if (dungeon[i, j] == CellType.Floor)
                    floorAmount++;
            }
        }
    }
}
