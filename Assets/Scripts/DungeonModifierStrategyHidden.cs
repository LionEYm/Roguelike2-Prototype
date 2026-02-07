using UnityEngine;
using static DungeonGenerator;

[CreateAssetMenu(fileName = "Dungeon Modifier Strategy Hidden", menuName = "Dungeon/Modifier Strategy/Hidden")]

public class DungeonModifierStrategyHidden : DungeonModifierStrategyBase
{
    [SerializeField] private GameObject HiddenRoomPrefab;
    [SerializeField] private int CellSizeOffset = 5;

    private void OnValidate()
    {
        if (HiddenRoomPrefab == null || HiddenRoomPrefab.GetComponent<HiddenRoom>() == null)
        {
            Debug.LogError($"{name}: HiddenRoomPrefab must have a HiddenRoom component");
        }
    }

    public override void Action(ref CellType[,] dungeon)
    {
        int width = dungeon.GetLength(0);
        int height = dungeon.GetLength(1);

        int x, y;
        do
        {
            x = Random.Range(1, width - 1);
            y = Random.Range(1, height - 1);
        }
        while (dungeon[x, y] == CellType.Wall);

        // Ensure this cell is floor (hidden room occupies a floor tile)
        dungeon[x, y] = CellType.Floor;

        GameObject go = Instantiate(
            HiddenRoomPrefab,
            new Vector3(x * CellSizeOffset, 0f, y * CellSizeOffset),
            Quaternion.identity
        );

        HiddenRoom hiddenRoom = go.GetComponent<HiddenRoom>();
        if (hiddenRoom == null)
            return;

        // ---- Determine wall types based on dungeon neighbors ----

        SetWallFromNeighbor(hiddenRoom, dungeon, x, y+1, HiddenRoom.Direction.Up);
        SetWallFromNeighbor(hiddenRoom, dungeon, x, y-1, HiddenRoom.Direction.Down);
        SetWallFromNeighbor(hiddenRoom, dungeon, x-1, y, HiddenRoom.Direction.Left);
        SetWallFromNeighbor(hiddenRoom, dungeon, x+1, y,HiddenRoom.Direction.Right);
        hiddenRoom.Initialize();
    }

    private void SetWallFromNeighbor(
        HiddenRoom room,
        CellType[,] dungeon,
        int x,
        int y,

        HiddenRoom.Direction dir)
    {

        bool destroyable = false;

        if (x >= 0 && y >= 0 &&
            x < dungeon.GetLength(0) &&
            y < dungeon.GetLength(1))
        {
            destroyable = dungeon[x, y] == CellType.Floor;
        }

        room.SetDestroyable(dir, destroyable);
    }

    public override int GetAmount(int dungeonSize)
    {
        return dungeonSize / 2;
    }
}
