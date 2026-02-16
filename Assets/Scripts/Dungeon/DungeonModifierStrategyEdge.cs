using System.Collections.Generic;
using UnityEngine;
using static DungeonGenerator;

[CreateAssetMenu(fileName = "Dungeon Modifier Strategy Edge", menuName = "Dungeon/Modifier Strategy/Edge")]
public class DungeonModifierStrategyEdge : DungeonModifierStrategyBase
{
    [SerializeField] private List<GameObject> Props;

    // Cache of occupied "edge slots" so we don't place duplicates.
    // Key = tile (x,y) + direction to the wall.
    private HashSet<EdgeKey> _usedEdges;
    private GameObject _parent;

    private struct EdgeKey
    {
        public readonly int x, y;
        public readonly sbyte dx, dy; // direction

        public EdgeKey(int x, int y, int dx, int dy)
        {
            this.x = x; this.y = y;
            this.dx = (sbyte)dx; this.dy = (sbyte)dy;
        }

        public override int GetHashCode()
        {
            unchecked //so doesnt throw overflow
            {
                int h = 17;
                h = h * 31 + x;
                h = h * 31 + y;
                h = h * 31 + dx;
                h = h * 31 + dy;
                return h;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is not EdgeKey other) return false;
            return x == other.x && y == other.y && dx == other.dx && dy == other.dy;
        }
    }

    public override void Initialize(CellType[,] dungeon)
    {
        // IMPORTANT: ScriptableObjects persist; clear per generation.
        _usedEdges ??= new HashSet<EdgeKey>(1024);
        _usedEdges.Clear();
        _parent = new GameObject();
        _parent.name = "Props";
    }

    public override void Action(ref CellType[,] dungeon)
    {
        if (Props == null || Props.Count == 0) return;

        int width = dungeon.GetLength(0);
        int height = dungeon.GetLength(1);

        int x = 0, y = 0;
        bool found = false;

        // Find a FLOOR tile that has at least one WALL neighbor edge that is NOT already used.
        for (int c = 0; c < 1000; c++)
        {
            x = Random.Range(1, width - 1);
            y = Random.Range(1, height - 1);

            if (dungeon[x, y] == CellType.Floor && HasFreeWallNeighbor(dungeon, x, y, width, height))
            {
                found = true;
                break;
            }
        }

        if (!found) return;

        //                                              magic number... spooky....
        Vector3 floorCenter = new Vector3(x, 0, y) * 5f + new Vector3(2.5f, 0, -2.5f);

        // Directions + rotations (as before)
        List<(Vector2Int dir, float rot)> directions = new()
        {
            (new Vector2Int(-1, 0), 90),   // Left
            (new Vector2Int( 1, 0), 270),  // Right
            (new Vector2Int( 0,-1), 0),    // Down
            (new Vector2Int( 0, 1), 180)   // Up
        };

        // Try up to 4 directions (remove as you do)
        for (int i = 0; i < directions.Count; i++)
        {
            int index = Random.Range(0, directions.Count);
            var (dir, rot) = directions[index];

            int nx = x + dir.x;
            int ny = y + dir.y;

            // inside interior
            if (nx <= 0 || ny <= 0 || nx >= width - 1 || ny >= height - 1)
            {
                directions.RemoveAt(index);
                continue;
            }

            // Must be wall AND edge must be free (not cached)
            var edgeKey = new EdgeKey(x, y, dir.x, dir.y);
            if (dungeon[nx, ny] == CellType.Wall && !_usedEdges.Contains(edgeKey))
            {
                Vector3 wallOffset = new Vector3(dir.x * 1.8f, 0, dir.y * 1.8f);
                Vector3 spawnPos = floorCenter + wallOffset;
                Quaternion rotation = Quaternion.Euler(0, rot, 0);

                int propIndex = Random.Range(0, Props.Count);
                var go = Instantiate(Props[propIndex], spawnPos + new Vector3(0, 0.04f, 0), rotation);
                go.transform.parent = _parent.transform;

                // Mark this edge slot as taken so nothing else can place here again.
                _usedEdges.Add(edgeKey);
                return;
            }

            directions.RemoveAt(index);
        }
    }

    // Wall neighbor counts ONLY if that wall-adjacent edge is NOT already used by a prop.
    private bool HasFreeWallNeighbor(CellType[,] map, int x, int y, int width, int height)
    {
        // Cardinal directions
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx <= 0 || ny <= 0 || nx >= width - 1 || ny >= height - 1)
                continue;

            if (map[nx, ny] != CellType.Wall)
                continue;

            var edgeKey = new EdgeKey(x, y, dx[i], dy[i]);
            if (!_usedEdges.Contains(edgeKey))
                return true;
        }

        return false;
    }

    public override int GetAmount(int dungeonSize)
    {
        return (int)(dungeonSize * dungeonSize / 2.5f);
    }
}
