using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CursorController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private RectTransform dashboardArea;
    [SerializeField] private RectTransform originPoint;
    [SerializeField] private UILineDrag lineDrag;
    public List<SkillSlot> skillSlots;
    public bool snapped;
    private SkillSlot _closestSkillSlot;
    private float _closestDistance;
    public Vector3 currentStartPos;
    public List<SkillSlot> selectedSkills;
    
    private CanvasGroup _canvasGroup;
    public bool isDragging;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    
    // Method for other classes to access and change the current starting position of the cursor
    public void SetStartPos(Vector3 pos)
    {
        currentStartPos = pos;
    }

    private void Start()
    {
        // Reset variables
        SetSnapped(false);
        SetStartPos(originPoint.localPosition);
    }

    // Method for other classes to access and change the current snapped state of the cursor
    public void SetSnapped(bool snap)
    {
        snapped = snap;
    }
    
    // Skill selection method, later used for combat start and checks for the SkillSlot script
    public void SelectSkill(SkillSlot skill)
    {
        if (selectedSkills == null) return;
        selectedSkills.Add(skill);
        Debug.Log($"Slot {skill} added");
        
    }
    
    // Skill removal method
    public void RemoveSkill(SkillSlot skill)
    {
        if (selectedSkills == null) return;
        selectedSkills.RemoveAt(selectedSkills.Count - 1);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = false;
        //lineDrag.ResetLineStart();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.6f;
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
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;
        // Animate snapping the cursor back to its origin position
        if (!snapped)
        {
            if (selectedSkills == null) { 
                currentStartPos = originPoint.localPosition;
            }
            
            transform.DOLocalMove(currentStartPos, 0.0f);
        }
        else
        {
            SnapToClosestSlot();
            lineDrag.EditCurrentSegment(transform.localPosition);
            lineDrag.SetStartPos(transform.localPosition);
        }
        // Optionally handle snapping back logic or finalize the drag
        Debug.Log("Drag ended.");
    }

    //Method for snapping cursor to the (last) closest skill slot
    public void SnapToClosestSlot() 
    {
        // Initialize variables
        _closestSkillSlot = null;
        _closestDistance = 0;
        
        // Get cursor position
        Vector3 cursorPosition = transform.localPosition;
        
        // For each skill slot in the skillSlot list, calculate the distance between it and the cursor's position 
        foreach (var socket in skillSlots)
        {
            float distance = Vector3.Distance(cursorPosition, socket.transform.position);
            
            // Cursor moves to Whichever skill is the closest to it 
            if (distance < _closestDistance)
            {
                _closestDistance = distance;
                _closestSkillSlot = socket;
                transform.DOLocalMove(_closestSkillSlot.transform.localPosition, 0.0f);
            }
        }
    }
}