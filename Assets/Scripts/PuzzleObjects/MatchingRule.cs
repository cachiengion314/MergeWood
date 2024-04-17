using UnityEngine;

public class MatchingRule
{
    public static bool IsPassedDownBlock(Vector2 currBlockPos, Vector2 checkBlockPos)
    {
        var dir = (checkBlockPos - currBlockPos).normalized;
        if (dir.Equals(Vector2.down)) return true;
        return false;
    }
}
