using UnityEngine;
using static DungeonGenerator;

public abstract class DungeonModifierStrategyBase : ScriptableObject
{
    public abstract void Action(ref CellType[,] dungeon);

    public abstract int GetAmount(int dungeonSize);

    public virtual void Initialize(CellType[,] dungeon) { }
}
