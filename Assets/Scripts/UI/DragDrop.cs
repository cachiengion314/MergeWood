using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
  [Header("Injected dependencies")]
  [Range(0, 100)]
  [SerializeField] private float gravityForce;
  [SerializeField] private Canvas canvas;

  [Header("Components")]
  private CanvasGroup canvasGroup;
  private RectTransform rectTransform;
  [Header("Settings")]
  private bool isOnDrag = false;

  private void Start()
  {
    rectTransform = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
  }

  private void Update()
  {
    if (isOnDrag) return;
    rectTransform.anchoredPosition += gravityForce * 100 * Time.deltaTime * Vector2.down;
  }

  public void OnPointerDown(PointerEventData eventData)
  {

  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    canvasGroup.alpha = .6f;
    canvasGroup.blocksRaycasts = false;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    isOnDrag = false;
    canvasGroup.alpha = 1;
    canvasGroup.blocksRaycasts = true;
  }

  public void OnDrag(PointerEventData eventData)
  {
    isOnDrag = true;
    rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
  }
}
