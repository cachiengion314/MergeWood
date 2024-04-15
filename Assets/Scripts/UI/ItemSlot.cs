using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
  public void OnDrop(PointerEventData eventData)
  {
    if (eventData.pointerDrag != null)
    {
      eventData.pointerDrag
        .GetComponent<RectTransform>()
        .anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
    }
  }
}
