using UnityEngine;
using static DungeonGenerator;

[CreateAssetMenu(fileName = "Dungeon Modifier Strategy Random Floor", menuName = "Dungeon/Modifier Strategy/Random Floor")]
public class DungeonModifierStrategyRandomFloor : DungeonModifierStrategyBase
{
    public override void Action(ref CellType[,] dungeon)
    {
        int x, y;
        do
        {
            x = Random.Range(1, dungeon.GetLength(0) - 1);
            y = Random.Range(1, dungeon.GetLength(1) - 1);
        } while (dungeon[x, y] == CellType.Floor);

        dungeon[x, y] = CellType.Floor;
    }

    public override int GetAmount(int dungeonSize)
    {
        return dungeonSize * 2 / 3;
    }
}
