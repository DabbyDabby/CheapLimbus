using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CursorController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private RectTransform dashboardArea;
    [SerializeField] private RectTransform originPoint;
    [SerializeField] private UILineDrag lineDrag;
    public List<SkillSlot> skillSlots;
    public bool snapped;
    private SkillSlot _closestSkillSlot;
    private float _closestDistance;
    public Vector3 currentStartPos;
    public List<SkillSlot> selectedSkills = new List<SkillSlot>();
    private int _currentColumnIndex;
    public List<List<SkillSlot>> dashboardSlots = new List<List<SkillSlot>>();
    private SkillSlot _currentSkillSlot;
    public Camera cam;
    
    private CanvasGroup _cursorCanvasGroup;
    public bool isDragging;

    private void Awake()
    {
        _cursorCanvasGroup = GetComponent<CanvasGroup>();
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

        dashboardSlots = new List<List<SkillSlot>>();
        
        _currentColumnIndex = 0;

        // Populates dashboard 2D List
        int slotPerPair = 2;
        List<SkillSlot> pair = new List<SkillSlot>();
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i % slotPerPair == 0)
            {
                pair = new List<SkillSlot>();
                dashboardSlots.Add(pair);
            }
            pair.Add(skillSlots[i]);
        }
    }

    // Method for other classes to access and change the current snapped state of the cursor
    public void SetSnapped(bool snap) { snapped = snap; }

    public void HighlightSkills(int columnIndex)
    {
        for (int i = 0; i < dashboardSlots.Count; i++)
        {
            var pair = dashboardSlots[i];
            if (i == columnIndex)
            {
                foreach(SkillSlot slot in pair)
                {
                    slot.SetSelectable(true);
                    slot.transform.localScale = Vector3.one * 1.2f;
                    Debug.Log($"SELECTABLE: {slot}");
                } 
            }
            else
            {
                foreach(SkillSlot slot in pair)
                {
                    slot.SetSelectable(false);
                    slot.transform.localScale = Vector3.one;
                } 
            }
        }
    }

    public void HightlightSkillsReset()
    {
        for (int i = 0; i < dashboardSlots.Count; i++)
        {
            var pair = dashboardSlots[i];
            
            foreach(SkillSlot slot in pair)
            {
                slot.SetSelectable(false);
                slot.transform.localScale = Vector3.one;
            } 
        }
    }
    

    // Skill Selection Logic: Only 1 Skill in a column may be selected, starting from the first column, then the second, etc...
    public void DetectSkillSelection()
    {
        HighlightSkills(_currentColumnIndex);
    }
    
    public void DetectSkillDeselection(SkillSlot skill)
    {
        Vector3 eastDirection = Vector3.right;
        
        Vector3 directionToCursor = (transform.localPosition - skill.transform.localPosition).normalized;
        
        // Returns an angle between 0 and 180 dregrees, with 0 being east and 180 being west
        float skillSlotAngle = Vector3.Angle(eastDirection, directionToCursor);
        float sign = Mathf.Sin(Vector3.Dot(Vector3.right, directionToCursor));
        
        if (skillSlotAngle > 90f && sign < -0.4f)
        {
            Debug.Log("Skill deselected!");
            DeselectSkill(skill);
            _currentColumnIndex--;
            lineDrag.RemoveSegment();
        }
    }
    
    // Skill selection method, later used for combat start and checks for the SkillSlot script
    public void SelectSkill(SkillSlot skill)
    {
        // Pass method if the exact same skill has already been selected
        if (selectedSkills.Contains(skill)) return;
        
        // Alternation
        // Check starts at 0 selected skills (List index = 0 means, at at least 1 selected skill)
        for (int i = selectedSkills.Count - 1; i >= 0; i--)
        {
            // If last selected skill's column index is the same as the now selected one, deselect last selected skill
            if (selectedSkills[i].columnIndex == skill.columnIndex)
            {
                DeselectSkill(selectedSkills[i]);
                lineDrag.RemoveSegment();
            }
        }
        
        selectedSkills.Add(skill);
        _currentColumnIndex = skill.columnIndex;
        SetStartPos(skill.transform.localPosition);
        
        // Lock skill selection and check which skills can be selected next
        DetectSkillSelection();
        Debug.Log($"Slot {skill} added");
    }
    
    // Skill removal method
    public void DeselectSkill(SkillSlot skill)
    {
        if (selectedSkills.Count == 0) return;
        
        selectedSkills.RemoveAt(selectedSkills.Count - 1);
        if (selectedSkills == null || selectedSkills.Count == 0)
        {
           SetStartPos(originPoint.localPosition);
        }
        else
        {
            SetStartPos(selectedSkills[^1].transform.localPosition);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        _cursorCanvasGroup.blocksRaycasts = false;
        _cursorCanvasGroup.alpha = 0.6f;
        transform.DOScale(Vector3.one * 0.5f, 0.2f);
        
        DetectSkillSelection();

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

        Vector3 mousePos = Input.mousePosition;
        bool inRange = false;
        
        foreach (List<SkillSlot> pair in dashboardSlots)
        {
            foreach (SkillSlot slot in pair)
            {
                var position = RectTransformUtility.WorldToScreenPoint(null, slot.transform.position);
                float distance = Vector3.Distance(mousePos, position);
                Debug.Log($@"Mouse: {mousePos} || Position: {position}");

                if (distance < 80 && slot != _currentSkillSlot && slot.columnIndex > _currentColumnIndex)
                {
                    inRange = true;
                    _currentSkillSlot = slot;
                    SelectSkill(slot);
                    SetSnapped(true);
                    lineDrag.EditCurrentSegment(slot.transform.localPosition);
                    lineDrag.SetStartPos(slot.transform.localPosition);
                    lineDrag.AddNewSegment(slot.transform.localPosition);
                    break;
                    // breaks ensure only 1 slot is selected, else it would constantly loop through each skill
                }
                
            }
            if (inRange) break;
        }
        
        foreach (List<SkillSlot> pair in dashboardSlots)
        {
            foreach (SkillSlot slot in pair)
            {
                var position = slot.transform.localPosition;
                float distance = Vector3.Distance(transform.localPosition, position);

                if (distance > 81)
                {
                    inRange = false;
                    SetSnapped(false);
                    break;
                    // breaks ensure only 1 slot is selected, else it would constantly loop through each skill
                }
                
            }
            if (!inRange) break;
        }
        
        if (selectedSkills.Count > 0 && !snapped)
        {
            DetectSkillDeselection(selectedSkills[^1]);
        }
        
        // Pass the in-game cursor's local position to the line
        lineDrag.EditCurrentSegment(transform.localPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HightlightSkillsReset();
        isDragging = false;
        _cursorCanvasGroup.blocksRaycasts = true;
        _cursorCanvasGroup.alpha = 1f;
        transform.DOScale(Vector3.one, 0.1f);
        // Animate snapping the cursor back to its origin position
        if (!snapped)
        {
            if (selectedSkills == null || selectedSkills.Count == 0) { 
                Debug.Log("No skills selected, returning to TRUE origin point.");
                SetStartPos(originPoint.localPosition);
                lineDrag.ResetLineStart();
            }
            lineDrag.RemoveSegment();
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
        _closestDistance = Mathf.Infinity; // Ensure the first slot checked is always closer
        
        // Get cursor position
        Vector3 cursorPosition = transform.localPosition;
        
        // For each skill slot in the skillSlot list, calculate the distance between it and the cursor's position 
        foreach (var socket in skillSlots)
        {
            float distance = Vector3.Distance(cursorPosition, socket.transform.position);
            
            // Cursor moves to Whichever skill is the closest to it 
            if (distance < _closestDistance)
            {
                SetSnapped(true);
                _closestDistance = distance;
                _closestSkillSlot = socket;
                SetStartPos(socket.transform.localPosition);
                lineDrag.RemoveSegment();
                transform.DOLocalMove(_closestSkillSlot.transform.localPosition, 0.0f);
            }
        }
    }
}