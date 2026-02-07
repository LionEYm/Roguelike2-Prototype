using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Randomness")]
    [SerializeField] private int Seed;

    [Header("Prefabs")]
    [SerializeField] private GameObject WallTile;
    [SerializeField] private GameObject FloorTile;

    [Header("Dungeon info")]
    [SerializeField] private int Size;
    private int _cellSizeOffset = 5;
    private CellType[,] _dungeon;

    [Header("Dungeon Modifiers")]
    [SerializeField] private List<DungeonModifierStrategyBase> DungeonModifiers;
    public enum CellType
    {
        Floor,
        Wall
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(Size<=5)
        {
            Debug.LogError("Dungeon generator with size too small");
        }
        if(WallTile==null || FloorTile==null)
        {
            Debug.LogError("Dungeon generator with null floor/wall tile");

        }

        if(DungeonModifiers==null || DungeonModifiers.Count<=0)
        {
            Debug.LogError("Dungeon generator with null or empty modifiers");
        }

        InitializeSeed();
        GenerateDungeon();
        ModifyDungeon();
        InstantiateTiles();
        //PrintDungeonToConsole();
    }

    private void InitializeSeed()
    {

        Seed = Seed == 0 ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : Seed;
        Random.InitState(Seed);
        Debug.Log($"<color=red>{Seed}</color=red>");
    }

    private void GenerateDungeon()
    {
        //initialze dungeon as walls
        _dungeon= new CellType[Size,Size];
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                _dungeon[x,y] = CellType.Wall;
            }
        }
        //starter cell
        var current = new Vector2Int(1,1);
        _dungeon[current.x, current.y] = CellType.Floor;
        int c = 0;
        Stack<Vector2Int> _trail= new Stack<Vector2Int>();
        do
        {
            c++;
            //get empty neighbors
            var emptyNeighbors=GetNeighbors(current.x,current.y,CellType.Wall);
            //no neighbors, backtrack
            if (emptyNeighbors == null || emptyNeighbors.Count == 0)
            {
                current = _trail.Pop();
            }
            else
            {
                _trail.Push(current);
                var neighbor = LionsHelper.HelperFunctions.GetWeightedRandom(emptyNeighbors.ToArray());
                _dungeon[neighbor.x, neighbor.y] = CellType.Floor;
                var midX = (current.x + neighbor.x) / 2;
                var midY = (current.y + neighbor.y) / 2;
                _dungeon[midX,midY] = CellType.Floor;

                current = neighbor;
            }    
            if (c > 50000)
            {
                Debug.LogError("Dungeon base while is broken");
                break;
            }
        }
        while (_trail.Count!=0);
    }

    private List<Vector2Int> GetNeighbors(int orgX, int orgY, CellType targetCellType)
    {
        var alligbleNeighbors = new List<Vector2Int>();
        // 2-step moves: (±2,0), (0,±2)
        TryAddNeighbor(orgX + 2, orgY);
        TryAddNeighbor(orgX - 2, orgY);
        TryAddNeighbor(orgX, orgY + 2);
        TryAddNeighbor(orgX, orgY - 2);

        return alligbleNeighbors;

        void TryAddNeighbor(int nx, int ny)
        {
            // must be inside the interior (keep borders as walls)
            if (nx <= 0 || ny <= 0 || nx >= Size - 1 || ny >= Size - 1)
                return;

            // Only consider cells that are still walls (uncarved)
            if (_dungeon[nx, ny] == CellType.Wall)
                alligbleNeighbors.Add(new Vector2Int(nx, ny));
        }
    }


    private void ModifyDungeon()
    {
        //ORDER OF THE LIST MATTERS!!!!
        foreach (var modifier in DungeonModifiers)
        {
            for (int i = 0; i < modifier.GetAmount(Size); i++)
            {
                modifier.Action(ref _dungeon);
            }
        }
    }

    private void InstantiateTiles()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                var tilePrefab = _dungeon[x, y] == CellType.Floor ? FloorTile : WallTile;
                var go = Instantiate(tilePrefab, new Vector3(x * _cellSizeOffset, 0, y * _cellSizeOffset), Quaternion.identity, this.transform);
                //weird rotate cus prefab is off
                if(_dungeon[x, y] == CellType.Floor)
                {
                    go.transform.RotateAround(go.transform.position, transform.up, 180f);
                }
                
            }
        }
    }

    private void PrintDungeonToConsole()
    {
        if (_dungeon == null)
        {
            Debug.LogWarning("Dungeon grid is null. Generate the dungeon first.");
            return;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        for (int y = Size - 1; y >= 0; y--)
        {
            for (int x = 0; x < Size; x++)
            {
                builder.Append(_dungeon[x, y] == CellType.Wall ? '#' : '0');
                builder.Append(' ');
            }
            builder.AppendLine();
        }

        Debug.Log(builder.ToString());
    }

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
