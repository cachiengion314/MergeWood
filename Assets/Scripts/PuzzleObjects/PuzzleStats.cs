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
        SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(LastLandingPos, 0, null);
    }

    private void DragAndDrop_onDragMove()
    {

    }

    private void DragAndDrop_onDroppedToFloor(Vector2 targetPosition)
    {
        SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(targetPosition, puzzleValue, gameObject);
        CheckRuleAt(targetPosition);

        isDetectChangingGridPos = false;
    }

    void DetectChangingGridPos()
    {
        if (!dragAndDrop.IsOnDrag) return;
        if (isDetectChangingGridPos) return;

        Vector2 currGridPos = GridUtility.ConvertWorldPosToGridPos(
            transform.position, GridWorld.Instance.Offset
        );
        Vector2 currWorldPos = GridUtility.ConvertGridPosToWorldPos(
            currGridPos, GridWorld.Instance.Offset
        );
        if (!currWorldPos.Equals(LastLandingPos))
        {
            isDetectChangingGridPos = true;

            SpawnPuzzleBlocks.Instance.CheckDownBlocks();
        }
    }

    public void CheckDownBlockAt(Vector2 worldPos)
    {
        var currBlock = SpawnPuzzleBlocks.Instance.GetPuzzleBlockAt(worldPos);
        if (currBlock == null) return;

        var downPos1 = worldPos + Vector2.down;
        if (GridWorld.Instance.GetWorldPosValueAt(downPos1) == 0)
        {
            var downPos2 = worldPos + Vector2.down * 2;
            if (
                GridWorld.Instance.GetWorldPosValueAt(worldPos) ==
                GridWorld.Instance.GetWorldPosValueAt(downPos2)
            )
            {
                // Passed matching rule at down postion2, we remove currBlock
                LeanTween.move(currBlock, downPos2, .07f).setOnComplete(() =>
                {
                    SpawnPuzzleBlocks.Instance.RemovePuzzleBlockRendererAt(worldPos);
                    SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(worldPos, 0, null);
                    SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(
                           downPos2,
                           GridWorld.Instance.GetWorldPosValueAt(downPos2) + 1,
                           SpawnPuzzleBlocks.Instance.GetPuzzleBlockAt(downPos2)
                       );
                    CheckDownBlockAt(worldPos + Vector2.up);
                });
                return;
            }
            // Not passed matching rule, we move currBlock down to empty space
            LeanTween.move(currBlock, downPos1, .07f).setOnComplete(() =>
            {
                SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(worldPos, 0, null);
                SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(
                       downPos1,
                       currBlock.GetComponent<PuzzleStats>().PuzzleValue,
                       currBlock
                   );
                CheckDownBlockAt(worldPos + Vector2.up);
            });
        }
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
        foreach (var neighborPos in neighbors)
        {
            if (!MatchingRule.IsPassedDownBlock(targetPosition, neighborPos)) continue;
            if (GridWorld.Instance.GetWorldPosValueAt(neighborPos) != puzzleValue) continue;
            // Passed matching rule
            LeanTween.move(gameObject, neighborPos, .07f).setOnComplete(() =>
            {
                SpawnPuzzleBlocks.Instance.RemovePuzzleBlockRendererAt(targetPosition);
                SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(targetPosition, 0, null);
                SpawnPuzzleBlocks.Instance.SetPuzzleBlockAt(
                    neighborPos,
                    GridWorld.Instance.GetWorldPosValueAt(neighborPos) + 1,
                    SpawnPuzzleBlocks.Instance.GetPuzzleBlockAt(neighborPos)
                );
            });
            return;
        }
    }
}
