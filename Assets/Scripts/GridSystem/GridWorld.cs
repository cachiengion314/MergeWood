using System.Collections.Generic;
using UnityEngine;

public class GridWorld : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize;
    public int[,] Grid { get; private set; }
    public Vector2 Offset { get; private set; }
    readonly Vector2[] directions = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };
    readonly Vector2[] diagonalDirections = {
        new(-1, -1), new(-1, 1), new(1, 1), new(1, -1)
        };

    private void Awake()
    {
        BakingGridWorld();
    }

    private void Update()
    {
#if UNITY_EDITOR
        DrawGrid();
#endif
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

    public int GetValueAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        if (IsGridPosOutsideAt(gridPos)) return -1;
        return Grid[(int)gridPos.x, (int)gridPos.y];
    }

    public void SetValueAt(Vector2 worldPos, int value = 1)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        SetGridPosValueAt(gridPos, value);
    }
    public void SetGridPosValueAt(Vector2 gridPos, int value = 1)
    {
        if (IsGridPosOutsideAt(gridPos)) return;
        Grid[(int)gridPos.x, (int)gridPos.y] = value;
    }

    public bool IsPosOutsideAt(Vector2 worldPos)
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

    public bool IsPosOccupiedAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        return IsGridPosOccupiedAt(gridPos);
    }

    public bool IsGridPosOccupiedAt(Vector2 gridPos)
    {
        var xRound = Mathf.RoundToInt(gridPos.x);
        var yRound = Mathf.RoundToInt(gridPos.y);

        if (xRound >= Grid.GetLength(0) || xRound < 0) return true;
        if (yRound >= Grid.GetLength(1) || yRound < 0) return true;
        if (Grid[xRound, yRound] > 0) return true;
        return false;
    }

    public bool IsDirectionObstructedAt(Vector2 worldPos, Vector2 worldDir)
    {
        var gridPos = ConvertWorldPosToGridPos(worldPos);
        return IsGridDirObstructedAt(gridPos, worldDir);
    }

    public bool IsGridDirObstructedAt(Vector2 gridPos, Vector2 gridDir)
    {
        if (IsGridPosOutsideAt(gridPos)) return true;

        var nextGridPos = gridPos + gridDir.normalized;

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
    /// only for debug
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
    public Vector2 FindFlooredPosAt(Vector2 worldPos)
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

    public List<Vector2> FindNeighborPosAt(Vector2 worldPos)
    {
        List<Vector2> blockWorlds = new();
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        var blockGridPoses = FindNeighborGridPosAt(gridPos);
        foreach (var gPos in blockGridPoses)
        {
            blockWorlds.Add(
                GridUtility.ConvertGridPosToWorldPos(gPos, Offset)
            );
        }
        return blockWorlds;
    }

    public List<Vector2> FindNeighborGridPosAt(Vector2 gridPos)
    {
        List<Vector2> blockGrids = new();
        if (IsGridPosOutsideAt(gridPos)) return blockGrids;

        for (int i = 0; i < directions.Length; ++i)
        {
            Vector2 nextGridPos = gridPos + directions[i];
            if (IsGridPosOutsideAt(nextGridPos)) continue;

            if (Grid[(int)nextGridPos.x, (int)nextGridPos.y] > 0)
            {
                blockGrids.Add(nextGridPos);
            }
        }
        return blockGrids;
    }

    public bool IsDiagonalDirectionObstructedAt(Vector2 worldPos, Vector2 worldDir)
    {
        var gridPos = ConvertWorldPosToGridPos(worldPos);
        return IsDiagonalGridDirObstructedAt(gridPos, worldDir);
    }

    public bool IsDiagonalGridDirObstructedAt(Vector2 gridPos, Vector2 gridDir)
    {
        var splitDirections = SplitDiagonalGridDir(gridDir);
        int count = 0;
        foreach (var dir in splitDirections)
        {
            var nextGridPos = gridPos + dir;

            if (IsGridPosOccupiedAt(nextGridPos))
            {
                count++;
            }
        }
        if (count == 2) return true;
        return false;
    }

    public List<Vector2> SplitDiagonalGridDir(Vector2 gridDir)
    {
        List<Vector2> splits = new();
        if (!IsDirDiagonal(gridDir)) return splits;

        var xDir = new Vector2(gridDir.normalized.x, 0);
        var yDir = new Vector2(0, gridDir.normalized.y);
        splits.Add(xDir);
        splits.Add(yDir);

        return splits;
    }
}
