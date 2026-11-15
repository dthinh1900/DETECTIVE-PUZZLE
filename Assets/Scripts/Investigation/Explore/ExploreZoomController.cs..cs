using UnityEngine;
using UnityEngine.EventSystems;

public class ExploreZoomController : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler
{
    [Header("Đối tượng được Zoom")]
    [SerializeField] private RectTransform exploreContent;

    [Header("Giới hạn Zoom")]
    [SerializeField] private float zoomStep = 0.2f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 2.5f;

    private float currentZoom = 1f;
    private Vector2 lastPointerPosition;

    public void ZoomIn()
    {
        SetZoom(currentZoom + zoomStep);
    }

    public void ZoomOut()
    {
        SetZoom(currentZoom - zoomStep);
    }

    private void SetZoom(float value)
    {
        if (exploreContent == null)
            return;

        currentZoom = Mathf.Clamp(value, minZoom, maxZoom);

        exploreContent.localScale =
            Vector3.one * currentZoom;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPointerPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (exploreContent == null)
            return;

        if (currentZoom <= minZoom)
            return;

        Vector2 delta =
            eventData.position - lastPointerPosition;

        Canvas canvas = GetComponentInParent<Canvas>();

        float scaleFactor =
            canvas != null ? canvas.scaleFactor : 1f;

        exploreContent.anchoredPosition +=
            delta / scaleFactor;

        lastPointerPosition = eventData.position;
    }
}