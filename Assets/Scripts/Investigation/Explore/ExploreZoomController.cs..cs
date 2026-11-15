
using UnityEngine;
using UnityEngine.EventSystems;

public class ExploreZoomController : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler
{
    [Header("Khung giới hạn")]
    [SerializeField] private RectTransform exploreViewport;

    [Header("Đối tượng được Zoom và kéo")]
    [SerializeField] private RectTransform exploreContent;

    [Header("Giới hạn Zoom")]
    [SerializeField] private float zoomStep = 0.2f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 2.5f;

    private float currentZoom = 1f;
    private Vector2 lastPointerPosition;

    private void Start()
    {
        if (exploreViewport == null)
            exploreViewport = transform as RectTransform;

        if (exploreViewport == null || exploreContent == null)
        {
            Debug.LogError(
                "[ZOOM] Chưa gán ExploreViewport hoặc ExploreContent."
            );
            return;
        }

        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        exploreContent.localScale = Vector3.one * currentZoom;

        ClampContentPosition();
    }

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
        if (exploreContent == null || exploreViewport == null)
            return;

        currentZoom = Mathf.Clamp(value, minZoom, maxZoom);

        exploreContent.localScale =
            Vector3.one * currentZoom;

        // Sau khi zoom, giới hạn lại cả 4 cạnh.
        ClampContentPosition();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (exploreViewport == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            exploreViewport,
            eventData.position,
            eventData.pressEventCamera,
            out lastPointerPosition
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (exploreContent == null ||
            exploreViewport == null)
            return;

        if (currentZoom <= minZoom)
            return;

        Vector2 currentPointerPosition;

        bool converted =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                exploreViewport,
                eventData.position,
                eventData.pressEventCamera,
                out currentPointerPosition
            );

        if (!converted)
            return;

        Vector2 delta =
            currentPointerPosition - lastPointerPosition;

        // Di chuyển nội dung theo thao tác kéo chuột.
        exploreContent.position +=
            exploreViewport.TransformVector(delta);

        lastPointerPosition = currentPointerPosition;

        // Không cho nội dung bị kéo ra ngoài khung.
        ClampContentPosition();
    }

    private void ClampContentPosition()
    {
        if (exploreViewport == null || exploreContent == null)
            return;

        // Lấy 4 góc của ExploreContent trong hệ tọa độ Viewport.
        Vector3[] corners = new Vector3[4];
        exploreContent.GetWorldCorners(corners);

        float contentMinX = float.MaxValue;
        float contentMaxX = float.MinValue;
        float contentMinY = float.MaxValue;
        float contentMaxY = float.MinValue;

        for (int i = 0; i < 4; i++)
        {
            Vector3 localCorner =
                exploreViewport.InverseTransformPoint(corners[i]);

            contentMinX = Mathf.Min(contentMinX, localCorner.x);
            contentMaxX = Mathf.Max(contentMaxX, localCorner.x);
            contentMinY = Mathf.Min(contentMinY, localCorner.y);
            contentMaxY = Mathf.Max(contentMaxY, localCorner.y);
        }

        Rect viewportRect = exploreViewport.rect;

        Vector2 correction = Vector2.zero;

        // Giới hạn cạnh trái và phải.
        if (contentMaxX - contentMinX <= viewportRect.width)
        {
            correction.x =
                viewportRect.center.x -
                (contentMinX + contentMaxX) / 2f;
        }
        else if (contentMinX > viewportRect.xMin)
        {
            correction.x = viewportRect.xMin - contentMinX;
        }
        else if (contentMaxX < viewportRect.xMax)
        {
            correction.x = viewportRect.xMax - contentMaxX;
        }

        // Giới hạn cạnh dưới và trên.
        if (contentMaxY - contentMinY <= viewportRect.height)
        {
            correction.y =
                viewportRect.center.y -
                (contentMinY + contentMaxY) / 2f;
        }
        else if (contentMinY > viewportRect.yMin)
        {
            correction.y = viewportRect.yMin - contentMinY;
        }
        else if (contentMaxY < viewportRect.yMax)
        {
            correction.y = viewportRect.yMax - contentMaxY;
        }

        // Chuyển độ dịch từ tọa độ Viewport sang tọa độ World.
        exploreContent.position +=
            exploreViewport.TransformVector(correction);
    }
}
