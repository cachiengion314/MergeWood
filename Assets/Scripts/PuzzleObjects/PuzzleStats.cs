using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class PuzzleStats : MonoBehaviour
{
    [Header("Injected Dependencies")]
    public GridWorld gridWorld;
    public ObjectPool<GameObject> puzzleBlockPool;
    [SerializeField] TextMeshPro valueText;
    [SerializeField] DragAndDrop dragAndDrop;

    [Header("Settings")]
    bool isDetectChangingGridPos;
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
        dragAndDrop.onDragMove += DragAndDrop_onDragMove;

        valueText.text = puzzleValue.ToString();
    }

    private void Update()
    {
        DetectChangingGridPos();

        Utility.DrawQuad(LastLandingPos, .5f, 0);
    }

    private void OnDestroy()
    {
        dragAndDrop.onDroppedToFloor -= DragAndDrop_onDroppedToFloor;
        dragAndDrop.onDragBegan -= DragAndDrop_onDragBegan;
        dragAndDrop.onDragBegan -= DragAndDrop_onDragMove;
    }

    private void DragAndDrop_onDragBegan()
    {
        PuzzleManager.Instance.SetPuzzleBlockValueAt(LastLandingPos, 0, null);
    }

    private void DragAndDrop_onDragMove()
    {

    }

    private void DragAndDrop_onDroppedToFloor(Vector2 targetPosition)
    {
        PuzzleManager.Instance.SetPuzzleBlockValueAt(targetPosition, puzzleValue, gameObject);
        CheckRuleAt(targetPosition);

        isDetectChangingGridPos = false;
    }

    void DetectChangingGridPos()
    {
        if (!dragAndDrop.IsOnDrag) return;
        if (isDetectChangingGridPos) return;

        Vector2 currGridPos = GridUtility.ConvertWorldPosToGridPos(
            transform.position, gridWorld.Offset
        );
        Vector2 currWorldPos = GridUtility.ConvertGridPosToWorldPos(
            currGridPos, gridWorld.Offset
        );
        if (!currWorldPos.Equals(LastLandingPos))
        {
            isDetectChangingGridPos = true;

            PuzzleManager.Instance.CheckDownBlocks();
        }
    }

    /// <summary>
    /// Like Destroy functioon but for objects pooling
    /// </summary>
    public void PoolDestroy()
    {
        if (gameObject.activeSelf) puzzleBlockPool.Release(gameObject);
    }

    void CheckRuleAt(Vector2 currPosition)
    {
        var neighbors = gridWorld.FindNeighborBlockWorldPosAt(currPosition);
        foreach (var neighborPos in neighbors)
        {
            if (!MatchingRule.IsPassedDownBlock(currPosition, neighborPos)) continue;
            if (gridWorld.GetWorldPosValueAt(neighborPos) != puzzleValue) continue;
            // Passed matching rule
            PuzzleManager.Instance.MatchTo(
                neighborPos, currPosition, gameObject, PuzzleManager.Instance.CheckDownBlocks
            );
            return;
        }
    }
}
