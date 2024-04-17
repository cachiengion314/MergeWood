using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class Stats : MonoBehaviour
{
    [Header("Injected Dependencies")]
    public ObjectPool<GameObject> puzzleBlockPool;
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
    [SerializeField] TextMeshPro valueText;
    [SerializeField] DragAndDrop dragAndDrop;

    [Header("Settings")]
    public Vector2 LastLandingPos;

    private void Start()
    {
        dragAndDrop.onMovedToTarget += DragAndDrop_OnMovedToTarget;
        dragAndDrop.onDragBegan += DragAndDrop_OnDragBegan;

        valueText.text = puzzleValue.ToString();
    }

    private void OnDestroy()
    {
        dragAndDrop.onMovedToTarget -= DragAndDrop_OnMovedToTarget;
        dragAndDrop.onDragBegan -= DragAndDrop_OnDragBegan;
    }

    private void DragAndDrop_OnDragBegan()
    {
        GridWorld.Instance.SetWorldPosValueAt(LastLandingPos, 0);
    }

    private void DragAndDrop_OnMovedToTarget(Vector2 targetPosition)
    {
        GridWorld.Instance.SetWorldPosValueAt(targetPosition, puzzleValue);
        LastLandingPos = targetPosition;

        CheckRuleAt(targetPosition);
    }

    public void AutoPoolRelease()
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

            GridWorld.Instance.SetWorldPosValueAt(targetPosition, 0);
            AutoPoolRelease();
        }
    }
}
