using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CursorController2 : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private RectTransform dashboardArea;
    [SerializeField] private RectTransform originPoint;
    [SerializeField] private UILineDrag2 lineDrag;
    public List<SkillSlot> skillSlots;
    public static bool snapped = false;
    private SkillSlot closestSkillSlot;
    private float closestDistance;
    

    private CanvasGroup canvasGroup;
    private bool isDragging;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        //skillSlots = GetComponents<SkillSlot>().ToList();
        foreach (var socket in skillSlots)
        {
            Debug.Log($"Slot {socket} added");
            //Debug.Log($"✅ Skill slot {name} registered.");
        }
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

    public async void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        // Animate snapping the cursor back to its origin position
        if (!snapped)
        {
            transform.DOLocalMove(originPoint.localPosition, 0.0f);
            lineDrag.ResetLineStart();  // Optionally reset the line once snapping is done
        }
        else
        {
            SnapToClosestSlot();
        }
        // Optionally handle snapping back logic or finalize the drag
        Debug.Log("Drag ended.");
    }

    public void SnapToClosestSlot()
    {
        closestSkillSlot = null;
        closestDistance = 0;
        Vector3 cursorPosition = transform.localPosition;
        foreach (var socket in skillSlots)
        {
            float distance = Vector3.Distance(cursorPosition, socket.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSkillSlot = socket;
                transform.DOLocalMove(closestSkillSlot.transform.position, 0.0f);
            }
        }
    }
}