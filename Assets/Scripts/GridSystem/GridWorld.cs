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

    public bool IsWorldPosBlockedAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        return IsGridPosBlockedAt(gridPos);
    }

    public bool IsGridPosOutsideAt(Vector2 gridPos)
    {
        if (gridPos.x >= Grid.GetLength(0) || gridPos.x < 0) return true;
        if (gridPos.y >= Grid.GetLength(1) || gridPos.y < 0) return true;
        return false;
    }

    public bool IsGridPosBlockedAt(Vector2 gridPos)
    {
        if (gridPos.x >= Grid.GetLength(0) || gridPos.x < 0) return true;
        if (gridPos.y >= Grid.GetLength(1) || gridPos.y < 0) return true;
        if (Grid[(int)gridPos.x, (int)gridPos.y] > 0) return true;
        return false;
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

    public int GetWorldPosValueAt(Vector2 worldPos)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        return Grid[(int)gridPos.x, (int)gridPos.y];
    }

    public void SetWorldPosValueAt(Vector2 worldPos, int value = 1)
    {
        Vector2 gridPos = GridUtility.ConvertWorldPosToGridPos(worldPos, Offset);
        SetGridPosValueAt(gridPos, value);
    }

    /// <summary>
    /// Helpers function section
    /// </summary>
    public void SetGridPosValueAt(Vector2 gridPos, int value = 1)
    {
        if (IsGridPosOutsideAt(gridPos)) return;
        Grid[(int)gridPos.x, (int)gridPos.y] = value;
    }

    public Vector2 FindFlooredWorldPosAt(Vector2 worldPos)
    {
        Vector2 flooredGridPos = FindFlooredGridPosAt(worldPos);
        if (flooredGridPos.x < 0) return new Vector2(-1, -1);

        Vector2 flooredWorldPos = GridUtility.ConvertGridPosToWorldPos(flooredGridPos, Offset);
        return flooredWorldPos;
    }

    public Vector2 FindFlooredGridPosAt(Vector2 gridPos)
    {
        if (IsGridPosBlockedAt(gridPos)) return new Vector2(-1, -1);

        int flooredY = 0;
        int roudedX = Mathf.RoundToInt(gridPos.x);
        while (Grid[roudedX, flooredY] > 0)
        {
            flooredY++;
        }
        var flooredGridPos = new Vector2(roudedX, flooredY);

        if (IsGridPosBlockedAt(flooredGridPos)) return new Vector2(-1, -1);
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
