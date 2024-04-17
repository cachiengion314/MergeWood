using TMPro;
using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Injected dependencies")]
    public int PuzzleValue;
    [SerializeField] TextMeshPro valueText;
    [SerializeField] DragAndDrop dragAndDrop;

    [Header("Settings")]
    public Vector2 LastLandingPos;

    private void Start()
    {
        dragAndDrop.onMovedToTarget += DragAndDrop_OnMovedToTarget;
        dragAndDrop.onDragBegan += DragAndDrop_OnDragBegan;

        valueText.text = PuzzleValue.ToString();
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
        GridWorld.Instance.SetWorldPosValueAt(targetPosition, PuzzleValue);
        LastLandingPos = targetPosition;
    }
}
