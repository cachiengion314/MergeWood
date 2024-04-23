using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class PuzzleStats : MonoBehaviour
{
    [Header("Injected Dependencies")]
    [Tooltip("gridWorld will be injected via Instantiate method, not now.")]
    public GridWorld gridWorld;
    public ObjectPool<GameObject> puzzleBlockPool;
    [SerializeField] ParticleSystem smallHitFX;
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
        dragAndDrop.onDragCollided += DragAndDrop_onDragCollided;

        valueText.text = puzzleValue.ToString();
    }

    private void Update()
    {
        DetectChangingGridPos();

#if UNITY_EDITOR
        Utility.DrawQuad(LastLandingPos, .5f, 0);
#endif
    }

    private void OnDestroy()
    {
        dragAndDrop.onDroppedToFloor -= DragAndDrop_onDroppedToFloor;
        dragAndDrop.onDragBegan -= DragAndDrop_onDragBegan;
        dragAndDrop.onDragCollided -= DragAndDrop_onDragCollided;
    }

    private void DragAndDrop_onDragBegan()
    {
        PuzzleManager.Instance.SetPuzzleBlockValueAt(LastLandingPos, 0, null);
    }

    private void DragAndDrop_onDragCollided(Vector2 inputPos)
    {
        var currBlock = PuzzleManager.Instance.CurrentBeingDragged;
        var currBlockPuzzleStats = currBlock.GetComponent<PuzzleStats>();

        var collidedBlockValue = gridWorld.GetWorldPosValueAt(inputPos);
        if (currBlockPuzzleStats.PuzzleValue != collidedBlockValue) return;

        currBlockPuzzleStats.PoolDestroy();
        PuzzleManager.Instance.SetPuzzleBlockValueAt(
                inputPos,
                gridWorld.GetWorldPosValueAt(inputPos) + 1,
                PuzzleManager.Instance.GetPuzzleBlockAt(inputPos)
        );
        PuzzleManager.Instance.CurrentBeingDragged = null;

        PuzzleManager.Instance.CheckDownBlocks();
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

        Vector2 currGridPos = gridWorld.ConvertWorldPosToGridPos(transform.position);
        Vector2 currWorldPos = gridWorld.ConvertGridPosToWorldPos(currGridPos);
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
        var fx = Instantiate(smallHitFX, transform.position, Quaternion.identity);
        fx.Play();
    }

    void CheckRuleAt(Vector2 currPosition)
    {
        var neighbors = gridWorld.FindNeighborWorldPosAt(currPosition);
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
