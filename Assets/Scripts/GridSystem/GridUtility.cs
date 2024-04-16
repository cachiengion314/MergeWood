using UnityEngine;

public class GridUtility
{
    public static Vector2 ConvertGridPosToWorldPos(Vector2 gridPos, Vector2 offset)
    {
        return new Vector2(gridPos.x, gridPos.y) - offset;
    }

    public static Vector2 ConvertWorldPosToGridPos(Vector2 worldPos, Vector2 offset)
    {
        int xRound = Mathf.RoundToInt(worldPos.x + offset.x);
        int yRound = Mathf.RoundToInt(worldPos.y + offset.y);

        return new Vector2(xRound, yRound);
    }
}
