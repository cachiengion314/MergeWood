using System.Collections.Generic;
using UnityEngine;

public class GridWorld : MonoBehaviour
{
    public static GridWorld Instance { get; private set; }
    [SerializeField] Vector2Int gridSize;
    public int[,] Grid { get; private set; }
    public Vector2 Offset { get; private set; }
    readonly Vector2[] directions = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };

    private void Awake()
    {
        Instance = this;
        BakingGridWorld();
    }

    private void Update()
    {
        DrawGrid();
    }

    void BakingGridWorld()
    {
        Grid = new int[gridSize.x, gridSize.y];
        Offset = new Vector2(gridSize.x / 2f - .5f, gridSize.y / 2f - .5f);

        for (int x = 0; x < gridSize.x; ++x)
        {
            for (int y = 0; y < gridSize.y; ++y)
            {
                Grid[x, y] = 0;
            }
        }
    }

    public Vector2 ConvertGridPosToWorldPos(Vector2 gridPos)
    {
        if (IsGridPosOutsideAt(gridPos)) return new Vector2(-1, -1);

        return new Vector2(gridPos.x, gridPos.y) - Offset;
    }

    public Vector2 ConvertWorldPosToGridPos(Vector2 worldPos)
    {
        int xRound = Mathf.RoundToInt(worldPos.x + Offset.x);
        int yRound = Mathf.RoundToInt(worldPos.y + Offset.y);
        var gridPos = new Vector2(xRound, yRound);
        if (IsGridPosOutsideAt(gridPos)) return new Vector2(-1, -1);

        return gridPos;
    }

    public int GetWorldPosValueAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        if (IsGridPosOutsideAt(gridPos)) return -1;
        return Grid[(int)gridPos.x, (int)gridPos.y];
    }

    public void SetWorldPosValueAt(Vector2 worldPos, int value = 1)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        SetGridPosValueAt(gridPos, value);
    }
    public void SetGridPosValueAt(Vector2 gridPos, int value = 1)
    {
        if (IsGridPosOutsideAt(gridPos)) return;
        Grid[(int)gridPos.x, (int)gridPos.y] = value;
    }

    public bool IsWorldPosOutsideAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        return IsGridPosOutsideAt(gridPos);
    }

    public bool IsGridPosOutsideAt(Vector2 gridPos)
    {
        if (gridPos.x >= Grid.GetLength(0) || gridPos.x < 0) return true;
        if (gridPos.y >= Grid.GetLength(1) || gridPos.y < 0) return true;
        return false;
    }

    public bool IsWorldPosOccupiedAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        return IsGridPosOccupiedAt(gridPos);
    }

    public bool IsGridPosOccupiedAt(Vector2 gridPos)
    {
        if (gridPos.x >= Grid.GetLength(0) || gridPos.x < 0) return true;
        if (gridPos.y >= Grid.GetLength(1) || gridPos.y < 0) return true;
        var xRound = Mathf.RoundToInt(gridPos.x);
        var yRound = Mathf.RoundToInt(gridPos.y);
        if (Grid[xRound, yRound] > 0) return true;
        return false;
    }

    public bool IsWorldDirObstructedAt(Vector2 worldPos, Vector2 worldDir)
    {
        var gridPos = ConvertWorldPosToGridPos(worldPos);
        var desWorldPos = worldPos + worldDir;
        var desGridPos = ConvertWorldPosToGridPos(desWorldPos);
        var gridDir = desGridPos - gridPos;
        return IsGridDirObstructedAt(gridPos, gridDir);
    }

    public bool IsGridDirObstructedAt(Vector2 gridPos, Vector2 gridDir)
    {
        if (IsGridPosOutsideAt(gridPos)) return true;

        var nextGridPos = gridPos + gridDir.normalized;
        Utility.Print("gridDir " + gridDir);
        Utility.Print("nextGridPos " + nextGridPos);
        if (IsGridPosOccupiedAt(nextGridPos)) return true;

        return false;
    }

    public bool IsDirDiagonal(Vector2 dir)
    {
        var nextPos = Vector2.zero + dir.normalized;
        int xRound = Mathf.RoundToInt(nextPos.x);
        int yRound = Mathf.RoundToInt(nextPos.y);
        if (yRound == 0) return false;

        return Mathf.Abs(xRound / yRound) == 1;
    }

    /// <summary>
    /// For debug only
    /// </summary>
    void DrawGrid()
    {
        for (int x = 0; x < Grid.GetLength(0); x += 1)
        {
            for (int y = 0; y < Grid.GetLength(1); y += 1)
            {
                Vector2 pos = GridUtility.ConvertGridPosToWorldPos(new Vector2Int(x, y), Offset);
                Utility.DrawQuad(pos, 1, 0);
                if (Grid[x, y] > 0)
                {
                    Utility.DrawQuad(pos, .8f, 1);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector2(gridSize.x, gridSize.y));
    }

    /// <summary>
    /// Helpers function section
    /// </summary>
    public Vector2 FindFlooredWorldPosAt(Vector2 worldPos)
    {
        Vector2 flooredGridPos = FindFlooredGridPosAt(worldPos);
        if (flooredGridPos.x < 0) return new Vector2(-1, -1);

        Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, Offset);
        return flooredWorldPos;
    }

    public Vector2 FindFlooredGridPosAt(Vector2 gridPos)
    {
        if (IsGridPosOutsideAt(gridPos)) return new Vector2(-1, -1);

        int flooredY = 0;
        int roudedX = Mathf.RoundToInt(gridPos.x);
        while (Grid[roudedX, flooredY] > 0)
        {
            flooredY++;
        }
        var flooredGridPos = new Vector2(roudedX, flooredY);

        if (IsGridPosOccupiedAt(flooredGridPos)) return new Vector2(-1, -1);
        return flooredGridPos;
    }

    public List<Vector2> FindNeighborBlockWorldPosAt(Vector2 worldPos)
    {
        List<Vector2> blockWorldPoses = new();
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        var blockGridPoses = FindNeighborBlockGridPosAt(gridPos);
        foreach (var gPos in blockGridPoses)
        {
            blockWorldPoses.Add(
                GridUtility.ConvertGridPosToWorldPos(gPos, Offset)
            );
        }
        return blockWorldPoses;
    }

    public List<Vector2> FindNeighborBlockGridPosAt(Vector2 gridPos)
    {
        List<Vector2> blockGridPoses = new();
        if (IsGridPosOutsideAt(gridPos)) return blockGridPoses;

        for (int i = 0; i < directions.Length; ++i)
        {
            Vector2 nextGridPos = gridPos + directions[i];
            if (IsGridPosOutsideAt(nextGridPos)) continue;

            if (Grid[(int)nextGridPos.x, (int)nextGridPos.y] > 0)
            {
                blockGridPoses.Add(nextGridPos);
            }
        }
        return blockGridPoses;
    }

}
