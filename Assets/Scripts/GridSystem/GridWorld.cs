using UnityEngine;

public class GridWorld : MonoBehaviour
{
    public static GridWorld Instance { get; private set; }
    [SerializeField] Vector2Int gridSize;
    int[,] grid;
    public Vector2 offset;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BakingGridWorld();
    }

    private void Update()
    {
        DrawGrid();
    }

    public bool IsWorldPosBlockedAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, offset);
        return IsGridPosBlockedAt(gridPos);
    }

    public bool IsGridPosBlockedAt(Vector2 gridPos)
    {
        if (grid[(int)gridPos.x, (int)gridPos.y] > 0)
        {
            return true;
        }
        return false;
    }

    public void SetWorldPosBlockedAt(Vector2 worldPos, int value = 1)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, GridWorld.Instance.offset);
        SetGridPosBlockedAt(gridPos, value);
    }

    public void SetGridPosBlockedAt(Vector2 gridPos, int value = 1)
    {
        if (IsGridPosBlockedAt(gridPos)) return;
        grid[(int)gridPos.x, (int)gridPos.y] = value;
    }

    public Vector2 FindFlooredWorldPosAt(Vector2 worldPos)
    {
        Vector2 flooredGridPos = FindFlooredGridPosAt(worldPos);
        Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, offset);
        return flooredWorldPos;
    }

    public Vector2 FindFlooredGridPosAt(Vector2 gridPos)
    {
        int flooredY = 0;
        int roudedX = Mathf.RoundToInt(gridPos.x);
        while (grid[roudedX, flooredY] > 0)
        {
            flooredY++;
        }
        return new Vector2(roudedX, flooredY);
    }

    void BakingGridWorld()
    {
        grid = new int[gridSize.x, gridSize.y];
        offset = new Vector2(gridSize.x / 2f, gridSize.y / 2f);

        for (int x = 0; x < gridSize.x; ++x)
        {
            for (int y = 0; y < gridSize.y; ++y)
            {
                grid[x, y] = 0;
            }
        }
    }

    void DrawGrid()
    {
        for (int x = 0; x < grid.GetLength(0); x += 1)
        {
            for (int y = 0; y < grid.GetLength(1); y += 1)
            {
                Vector2 pos = GridUtility.ConvertGridPosToWorldPos(new Vector2Int(x, y), offset);
                Utility.DrawQuad(pos, 1, 0);

                if (grid[x, y] > 0)
                {
                    Utility.DrawQuad(pos, .5f, 1);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector2(gridSize.x, gridSize.y));
    }
}
