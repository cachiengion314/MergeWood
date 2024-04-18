using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class PuzzleStats : MonoBehaviour
{
    [Header("Injected Dependencies")]
    public ObjectPool<GameObject> puzzleBlockPool;
    [SerializeField] TextMeshPro valueText;
    [SerializeField] DragAndDrop dragAndDrop;

    [Header("Settings")]
    public Vector2 LastLandingPos;
    private int puzzleValue;
    public int PuzzleValue
    {
        get { return puzzleValue; }
        set
        {
            valueText.text = value.ToString();
            puzzleValue = value;
        }
    }

    private void Start()
    {
        dragAndDrop.onDroppedToFloor += DragAndDrop_onDroppedToFloor;
        dragAndDrop.onDragBegan += DragAndDrop_onDragBegan;

        valueText.text = puzzleValue.ToString();
    }

    private void OnDestroy()
    {
        dragAndDrop.onDroppedToFloor -= DragAndDrop_onDroppedToFloor;
        dragAndDrop.onDragBegan -= DragAndDrop_onDragBegan;
    }

    private void DragAndDrop_onDragBegan()
    {
        GridWorld.Instance.SetWorldPosValueAt(LastLandingPos, 0);
    }

    private void DragAndDrop_onDroppedToFloor(Vector2 targetPosition)
    {
        SyncMovePuzzleBlockTo(targetPosition, LastLandingPos);
        CheckRuleAt(targetPosition);

        LastLandingPos = targetPosition;
    }

    /// <summary>
    /// Like Destroy functioon
    /// </summary>
    public void PoolDestroy()
    {
        if (gameObject.activeSelf) puzzleBlockPool.Release(gameObject);
    }

    void CheckRuleAt(Vector2 targetPosition)
    {
        var neighbors = GridWorld.Instance.FindNeighborBlockWorldPosAt(targetPosition);
        foreach (var neighbor in neighbors)
        {
            if (!MatchingRule.IsPassedDownBlock(targetPosition, neighbor)) continue;
            if (GridWorld.Instance.GetWorldPosValueAt(neighbor) != puzzleValue) continue;
            // Passed matching rule
            LeanTween.move(gameObject, neighbor, .07f).setOnComplete(() =>
            {
                SyncValuePuzzleBlockAt(targetPosition, 0);
                SyncValuePuzzleBlockAt(neighbor, GridWorld.Instance.GetWorldPosValueAt(neighbor) + 1);
            });
            return;
        }
    }

    /// <summary>
    /// Sync value between GridWorld.Grid and SpawnPuzzleBlocks.ActivePuzzleBlocks
    /// </summary>
    /// <param name="worldPos"></param>
    void SyncValuePuzzleBlockAt(Vector2 worldPos, int value)
    {
        GridWorld.Instance.SetWorldPosValueAt(worldPos, value);
        var currPuzzleBlock = SpawnPuzzleBlocks.Instance.FindPuzzleBlockAt(worldPos);
        if (currPuzzleBlock == null) return;

        currPuzzleBlock.GetComponent<PuzzleStats>().PuzzleValue = value;
        if (value == 0)
        {
            SpawnPuzzleBlocks.Instance.RemovePuzzleBlockAt(worldPos); // remove its gameobj in spawn
        }
    }

    /// <summary>
    /// Sync position value between GridWorld.Grid and SpawnPuzzleBlocks.ActivePuzzleBlocks
    /// </summary>
    /// <param name="desWorldPos"></param>
    /// <param name="lastWorldPos"></param>
    void SyncMovePuzzleBlockTo(Vector2 desWorldPos, Vector2 lastWorldPos)
    {
        GridWorld.Instance.SetWorldPosValueAt(desWorldPos, puzzleValue);
        SpawnPuzzleBlocks.Instance.MovePuzzleBlockTo(desWorldPos, lastWorldPos);
    }
}
