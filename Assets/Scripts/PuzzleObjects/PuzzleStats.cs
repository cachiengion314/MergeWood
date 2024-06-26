using System;
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
            var sprite = PuzzleManager.Instance.GetSpriteBaseOn(value);
            GetComponentInChildren<SpriteRenderer>().sprite = sprite;
            puzzleValue = value;
        }
    }

    private void Start()
    {
        dragAndDrop.onDroppedToFloor += DragAndDrop_onDroppedToFloor;
        dragAndDrop.onDragBegan += DragAndDrop_onDragBegan;
        dragAndDrop.onDragMove += DragAndDrop_onDragMove;
        dragAndDrop.onDragCollided += DragAndDrop_onDragCollided;
        dragAndDrop.onDragEnd += DragAndDrop_onDragEnd;

        valueText.text = puzzleValue.ToString();
    }



    private void Update()
    {
#if UNITY_EDITOR
        Utility.DrawQuad(LastLandingPos, .5f, 0);
#endif
    }

    private void OnDestroy()
    {
        dragAndDrop.onDroppedToFloor -= DragAndDrop_onDroppedToFloor;
        dragAndDrop.onDragBegan -= DragAndDrop_onDragBegan;
        dragAndDrop.onDragMove -= DragAndDrop_onDragMove;
        dragAndDrop.onDragCollided -= DragAndDrop_onDragCollided;
    }

    private void DragAndDrop_onDragBegan()
    {
        PuzzleManager.Instance.SetPuzzleBlockValueAt(LastLandingPos, 0, null);
    }

    private void DragAndDrop_onDragMove()
    {
        DetectChangingGridPos();
    }

    private void DragAndDrop_onDragCollided(Vector2 inputPos)
    {
        var currBlock = PuzzleManager.Instance.CurrentBeingDragged;
        var currBlockPuzzleStats = currBlock.GetComponent<PuzzleStats>();

        var collidedBlockValue = gridWorld.GetValueAt(inputPos);
        if (currBlockPuzzleStats.PuzzleValue != collidedBlockValue) return;

        currBlockPuzzleStats.PoolDestroy();
        PuzzleManager.Instance.SetPuzzleBlockValueAt(
                inputPos,
                gridWorld.GetValueAt(inputPos) + 1,
                PuzzleManager.Instance.GetPuzzleBlockAt(inputPos)
        );
        PuzzleManager.Instance.CurrentBeingDragged = null;

        PuzzleManager.Instance.CheckDownBlocks();
    }

    private void DragAndDrop_onDroppedToFloor(Vector2 targetPosition)
    {
        PuzzleManager.Instance.SetPuzzleBlockValueAt(targetPosition, puzzleValue, gameObject);

        PuzzleManager.Instance.IsTweening = true;
        LeanTween
            .move(gameObject, targetPosition, .1f * PuzzleManager.Instance.TweenSlowFactor)
            .setOnComplete(() =>
        {
            PuzzleManager.Instance.IsTweening = false;
        });
        CheckRuleAt(targetPosition);

        isDetectChangingGridPos = false;
    }

    private void DragAndDrop_onDragEnd()
    {
        isDetectChangingGridPos = false;
    }

    void DetectChangingGridPos()
    {
        if (isDetectChangingGridPos) return;

        Vector2 currGridPos = gridWorld.ConvertWorldPosToGridPos(transform.position);
        Vector2 currWorldPos = gridWorld.ConvertGridPosToWorldPos(currGridPos);
        if (!currWorldPos.Equals(LastLandingPos))
        {
            if (this != PuzzleManager.Instance.CurrentBeingDragged.GetComponent<PuzzleStats>())
                return;
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
        var neighbors = gridWorld.FindNeighborPosAt(currPosition);
        foreach (var neighborPos in neighbors)
        {
            if (!MatchingRule.IsPassedDownBlock(currPosition, neighborPos)) continue;
            if (gridWorld.GetValueAt(neighborPos) != puzzleValue) continue;
            // Passed matching rule
            PuzzleManager.Instance.MatchTo(
                neighborPos, currPosition, gameObject, PuzzleManager.Instance.CheckDownBlocks
            );
            return;
        }
    }
}
