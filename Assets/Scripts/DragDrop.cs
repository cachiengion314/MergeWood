using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
  [SerializeField] private Canvas canvas;
  private CanvasGroup canvasGroup;

  private RectTransform rectTransform;
  private void Start()
  {
    rectTransform = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
  }

  private void Update()
  {

  }

  public void OnPointerDown(PointerEventData eventData)
  {
    // Debug.Log("OnPointerDown");
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    // Debug.Log("OnBeginDrag");
    canvasGroup.alpha = .6f;
    canvasGroup.blocksRaycasts = false;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    // Debug.Log("OnEndDrag");
    canvasGroup.alpha = 1;
    canvasGroup.blocksRaycasts = true;
  }

  public void OnDrag(PointerEventData eventData)
  {
    rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
  }
}
