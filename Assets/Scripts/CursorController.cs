using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CursorController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private RectTransform dashboardArea;
    [SerializeField] private RectTransform originPoint;
    [SerializeField] private UILineDrag lineDrag;
    [SerializeField] private CursorSocketEnd cursorSocketEnd;
    public List<SkillSlot> skillSlots;
    private SkillSlot _closestSkillSlot;
    private float _closestDistance;
    private Vector3 _currentStartPos;
    public List<SkillSlot> selectedSkills = new List<SkillSlot>();
    private int _currentColumnIndex;
    private List<List<SkillSlot>> _dashboardSlots = new List<List<SkillSlot>>();
    private SkillSlot _currentSkillSlot;
    public Camera cam;
    
    private CanvasGroup _cursorCanvasGroup;
    public bool isDragging;
    private CombatManager _combatManager;

    private void Awake()
    {
        _cursorCanvasGroup = GetComponent<CanvasGroup>();
    }
    
    // Method for other classes to access and change the current starting position of the cursor
    private void SetStartPos(Vector3 pos)
    {
        _currentStartPos = pos;
    }

    private void Start()
    {
        // Reset variables
        SetStartPos(originPoint.localPosition);

        _dashboardSlots = new List<List<SkillSlot>>();
        
        _currentColumnIndex = 0;

        // Populates dashboard 2D List
        int slotPerPair = 2;
        List<SkillSlot> pair = new List<SkillSlot>();
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (i % slotPerPair == 0)
            {
                pair = new List<SkillSlot>();
                _dashboardSlots.Add(pair);
            }
            pair.Add(skillSlots[i]);
            
            skillSlots[i].columnIndex = i / slotPerPair;
        }
        _combatManager = FindFirstObjectByType<CombatManager>();
    }
    

    private void HighlightSkills(int columnIndex)
    {
        for (int i = 0; i < _dashboardSlots.Count; i++)
        {
            var pair = _dashboardSlots[i];
            if (i == columnIndex)
            {
                foreach(SkillSlot slot in pair)
                {
                    slot.SetSelectable(true);
                    slot.transform.localScale = Vector3.one * 0.8f * 1.2f;
                    Debug.Log($"SELECTABLE: {slot}");
                } 
            }
            else
            {
                foreach(SkillSlot slot in pair)
                {
                    slot.SetSelectable(false);
                    slot.transform.localScale = Vector3.one * 0.8f;
                } 
            }
        }
    }

    private void HighlightSkillsReset()
    {
        foreach (var pair in _dashboardSlots)
        {
            foreach(SkillSlot slot in pair)
            {
                slot.SetSelectable(false);
                slot.transform.localScale = Vector3.one;
            }
        }
    }
    

    // Skill Selection Logic: Only 1 Skill in a column may be selected, starting from the first column, then the second, etc...
    private void DetectSkillSelection()
    {
        HighlightSkills(_currentColumnIndex);
    }
    
    private void DetectSkillDeselection(SkillSlot skill)
    {
        Vector3 mousePos = Input.mousePosition;
        var position = RectTransformUtility.WorldToScreenPoint(null, skill.transform.position);
        var distance = position.x - mousePos.x;
        
        if (distance > 60)
        {
            Debug.Log("Skill deselected!");
            DeselectSkill();
            lineDrag.RemoveSegment();
        }
    }
    
    // Skill selection method, later used for combat start and checks for the SkillSlot script
    private void SelectSkill(SkillSlot skill)
    {
        // Pass method if the exact same skill has already been selected
        if (selectedSkills.Contains(skill)) return;
        
        // Alternation
        // Check starts at 0 selected skills
        for (int i = selectedSkills.Count - 1; i >= 0; i--)
        {
            // If last selected skill's column index is the same as the now selected one, deselect last selected skill
            if (selectedSkills[i].columnIndex == skill.columnIndex)
            {
                DeselectSkill();
                lineDrag.RemoveSegment();
            }
        }
        
        selectedSkills.Add(skill);
        _currentSkillSlot = skill;
        _currentColumnIndex++;
        SetStartPos(skill.transform.localPosition);
        
        // Lock skill selection and check which skills can be selected next
        DetectSkillSelection();
        Debug.Log($"Slot {skill} added");
    }
    
    // Skill removal method
    private void DeselectSkill()
    {
        if (selectedSkills.Count == 0) return;
        
        selectedSkills.RemoveAt(selectedSkills.Count - 1);
        _currentColumnIndex--;
        if (selectedSkills == null || selectedSkills.Count == 0)
        {
           SetStartPos(originPoint.localPosition);
           lineDrag.SetStartPos(originPoint.localPosition);
           _currentColumnIndex = 0;
            _currentSkillSlot = null;
        }
        else
        {
            SetStartPos(selectedSkills[^1].transform.localPosition);
            lineDrag.SetStartPos(selectedSkills[^1].transform.localPosition);
            _currentSkillSlot = selectedSkills[^1];
        }
        HighlightSkills(_currentColumnIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _currentColumnIndex = selectedSkills.Count;
        isDragging = true;
        _cursorCanvasGroup.blocksRaycasts = false;
        _cursorCanvasGroup.alpha = 0.6f;
        transform.DOScale(Vector3.one * 0.5f, 0.2f);
        
        DetectSkillSelection();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dashboardArea, eventData.position, eventData.pressEventCamera, out localPoint);
        
        // Set the initial segment's start position at the cursor's current local position
        if (selectedSkills == null || selectedSkills.Count == 0)
        {
            lineDrag.AddNewSegment(originPoint.localPosition);
        }
        else
        {
            lineDrag.SetStartPos(selectedSkills[^1].transform.localPosition);
            lineDrag.AddNewSegment(selectedSkills[^1].transform.localPosition);
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dashboardArea, eventData.position, eventData.pressEventCamera, out var localPoint
        );
            
        // Smoothly move the in-game cursor to the current mouse position
        transform.localPosition = localPoint;

        Vector3 mousePos = Input.mousePosition;
        
        foreach (List<SkillSlot> pair in _dashboardSlots)
        {
            foreach (SkillSlot slot in pair)
            {
                var position = RectTransformUtility.WorldToScreenPoint(null, slot.transform.position);
                
                float distance = Vector3.Distance(mousePos, position);

                //Debug.Log($@"Mouse: {mousePos} || Position: {position}");

                if (distance < 50 && !selectedSkills.Contains(slot) && (slot.isSelectable || slot.columnIndex == _currentColumnIndex-1))
                {
                    SelectSkill(slot);
                    lineDrag.EditCurrentSegment(slot.transform.localPosition);
                    lineDrag.SetStartPos(slot.transform.localPosition);
                    lineDrag.AddNewSegment(slot.transform.localPosition);
                    break;
                    // breaks ensure only 1 slot is selected, else it would constantly loop through each skill
                }
            }
        }
        if (selectedSkills.Count > 0)
        {
            DetectSkillDeselection(selectedSkills[^1]);
        }
        
        // Pass the in-game cursor's local position to the line
        lineDrag.EditCurrentSegment(transform.localPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HighlightSkillsReset();
        isDragging = false;
        _cursorCanvasGroup.blocksRaycasts = true;
        _cursorCanvasGroup.alpha = 1f;
        transform.DOScale(Vector3.one, 0.1f);
        
        // Animate snapping the cursor back to its origin position
        if (selectedSkills == null || selectedSkills.Count == 0) { 
            _currentSkillSlot = null;
            Debug.Log("No skills selected, returning to TRUE origin point.");
            SetStartPos(originPoint.localPosition);
            _currentColumnIndex = 0;
            lineDrag.ResetLineStart();
        }
        
        Vector3 mousePos = Input.mousePosition;
        Vector3 cursorSocketEndPos = RectTransformUtility.WorldToScreenPoint(null, cursorSocketEnd.transform.position);
        float cursorSocketEndDistance = Vector3.Distance(mousePos, cursorSocketEndPos);
        
        if (cursorSocketEnd != null) {
            if (cursorSocketEndDistance < 50 && selectedSkills is { Count: 3 })
            {
                _combatManager.currentSlot = selectedSkills[^1];
                _combatManager.ApplyInstantSkills(selectedSkills);
                _combatManager.StartCombat();
                Debug.Log($@"COMBAT COMMAND ACKNOWLEDGED");
                
                selectedSkills.Clear();
                lineDrag.ResetLineStart();
                SetStartPos(originPoint.localPosition);
                transform.DOLocalMove(_currentStartPos, 0.0f);
                
            }
            else
            {
                lineDrag.RemoveSegment();
                transform.DOLocalMove(_currentStartPos, 0.0f);
            }
        }
        
        
        Debug.Log("Drag ended.");
    }
}