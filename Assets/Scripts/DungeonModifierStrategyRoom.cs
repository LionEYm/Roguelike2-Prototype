using UnityEngine;
using static DungeonGenerator;

[CreateAssetMenu(fileName = "Dungeon Modifier Strategy Room", menuName = "Dungeon/Modifier Strategy/Room")]
public class DungeonModifierStrategyRoom : DungeonModifierStrategyBase
{
    [Range(3, 10)]
    [SerializeField] private int RoomSize = 5;

    [Tooltip("How many random centers to try per room placement attempt.")]
    [SerializeField] private int MaxTriesPerRoom = 50;

    [Tooltip("Minimum fraction of cells in the target area that must currently be WALLS to allow carving.")]
    [Range(0f, 1f)]
    [SerializeField] private float MinWallFraction = 0.5f;

    public override void Action(ref CellType[,] dungeon)
    {
        if (dungeon == null)
            return;


        int width = dungeon.GetLength(0);
        int height = dungeon.GetLength(1);

        // Clamp room size to reasonable values relative to dungeon.
        int roomSize = Mathf.Clamp(RoomSize, 3, Mathf.Min(width, height) - 2);


        if (!TryFindRoomCenter(dungeon, roomSize, width, height, out int cx, out int cy))
            return; // Couldn't place this room; move on.

        CarveRoom(dungeon, cx, cy, roomSize);

    }

    /// <summary>
    /// Finds a random room center such that at least MinWallFraction of the target room area is walls.
    /// </summary>
    private bool TryFindRoomCenter(CellType[,] dungeon, int roomSize, int width, int height, out int cx, out int cy)
    {
        int half = roomSize / 2;

        // Ensure we can keep the whole room inside bounds.
        int minX = 1 + half;
        int maxX = width - 2 - half;
        int minY = 1 + half;
        int maxY = height - 2 - half;

        cx = cy = 0;


        // If dungeon is too small for this room size.
        if (minX > maxX || minY > maxY)
        {
            return false;
        }

        for (int t = 0; t < MaxTriesPerRoom; t++)
        {
            int x = Random.Range(minX, maxX + 1);
            int y = Random.Range(minY, maxY + 1);

            if (HasEnoughWalls(dungeon, x, y, half))
            {
                cx = x;
                cy = y;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if at least MinWallFraction of the cells in the candidate room area are walls.
    /// Room area is a square centered at (cx,cy) with radius 'half' (size = 2*half+1).
    /// </summary>
    private bool HasEnoughWalls(CellType[,] dungeon, int cx, int cy, int half)
    {
        int wallCount = 0;
        int total = 0;

        for (int x = cx - half; x <= cx + half; x++)
        {
            for (int y = cy - half; y <= cy + half; y++)
            {
                total++;
                if (dungeon[x, y] == CellType.Wall)
                    wallCount++;
            }
        }

        // "At least half" means wallCount/total >= 0.5
        return (float)wallCount / total >= MinWallFraction;
    }

    /// <summary>
    /// Carves the square room area into floor tiles.
    /// </summary>
    private void CarveRoom(CellType[,] dungeon, int cx, int cy, int roomSize)
    {
        int half = roomSize / 2;

        for (int x = cx - half; x <= cx + half; x++)
        {
            for (int y = cy - half; y <= cy + half; y++)
            {
                dungeon[x, y] = CellType.Floor;
            }
        }
    }

    // Keeping your method as-is; note this must exist on the base class for "override" to compile.
    public override int GetAmount(int dungeonSize)
    {
        return dungeonSize / 5;
    }
}
