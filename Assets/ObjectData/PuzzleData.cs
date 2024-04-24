using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "ScriptableObjects/PuzzleData", order = 0)]
public class PuzzleData : ScriptableObject
{
    [Header("Elements")]
    public Sprite[] renderers;
}
