using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CursorController2 : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private RectTransform dashboardArea;
    [SerializeField] private RectTransform originPoint;
    [SerializeField] private UILineDrag2 lineDrag;
    

    private CanvasGroup canvasGroup;
    private bool isDragging;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = false;
        lineDrag.ResetLineStart();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dashboardArea, eventData.position, eventData.pressEventCamera, out localPoint);

        // Set the initial segment's start position at the cursor's current local position
        lineDrag.AddNewSegment(transform.localPosition);
    }


    public void OnDrag(PointerEventData eventData)
{
    if (!isDragging) return;

    Vector2 localPoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        dashboardArea, eventData.position, eventData.pressEventCamera, out localPoint
    );

    // Smoothly move the in-game cursor to the current mouse position
    transform.localPosition = localPoint;

    // Pass the in-game cursor's local position to the line
    lineDrag.EditCurrentSegment(transform.localPosition);
}

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // Optionally handle snapping back logic or finalize the drag
        Debug.Log("Drag ended.");
    }
}