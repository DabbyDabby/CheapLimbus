using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class SkillSlot : MonoBehaviour
{
    //IPointerEnterHandler, 
    //, IPointerExitHandler
    // , IDropHandler
    public Skill skill;
    public List<Skill> skillsList;
    public int columnIndex;
    private Image _image;
    private bool _hasSegment;
    [SerializeField] public CursorController cursor;
    [SerializeField] public UILineDrag lineDrag;
    public List<SkillSlot> selectedSkills;
    private bool _isSelectable = false;
    
    private CanvasGroup _skillSlotCanvasGroup;
    
    private void Awake()
    {
        _skillSlotCanvasGroup = GetComponent<CanvasGroup>();
        if (_skillSlotCanvasGroup == null) _skillSlotCanvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _image = GetComponent<Image>();
        foreach (SkillSlot slot in selectedSkills)
        {
            Destroy(slot.gameObject);
        }
        
        selectedSkills.Clear();
    }
    
    public void RefreshSkills()                                                                   
    {
        skill = skillsList[Random.Range(0, skillsList.Count)];
        _image.sprite = skill.skillIcon;
    }

    public void SetSelectable(bool selectable)
    {
        _isSelectable = selectable;
        _skillSlotCanvasGroup.interactable = selectable;
        _skillSlotCanvasGroup.blocksRaycasts = selectable;
    }
    
    // When the cursor is dropped onto the skill slot, snap cursor to skill slot
    // public void OnDrop(PointerEventData eventData)
    // {
    //     Debug.Log("OnDrop triggered.");
    //     if (eventData.pointerDrag != null && cursor.isDragging)
    //     {
    //         // Snap the dragged cursor to the slot's local position
    //         eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
    //         cursor.SetSnapped(true);
    //     }
    // }
    
    // When the cursor enters a skill slot's radius
    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     // If the cursor is being dragged and there is no segment attached to the skill slot
    //     // Tell the cursor it's being snapped to the closest skill and "select" it
    //     // Set the cursor's starting position to the skill slot's position
    //     // Once cursor enters the skill's radius, the last line segment's end point snaps onto the skill slot
    //     // Set the next segment's starting point to the skill slot's position and create a new segment
    //     // "hasSegment" to prevent prefab spam of line segments
    //     // Scale the current skill slot for visual feedback
    //     if (cursor.isDragging && !_hasSegment) {
    //         
    //         cursor.SetSnapped(true);
    //         lineDrag.EditCurrentSegment(transform.localPosition);
    //         lineDrag.SetStartPos(transform.localPosition);
    //         lineDrag.AddNewSegment(transform.localPosition);
    //         _hasSegment = true;
    //         //transform.localScale = Vector3.one * 1.2f;
    //         Debug.Log($"✅ Cursor SNAPPED"); 
    //     }
    // }

    // When the cursor exits a skill slot's radius
    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     // If the cursor is being dragged, tell the cursor it is no longer snapped to that skill slot
    //     // Shrink skill slot back for visual feedback
    //     Vector3 eastDirection = Vector3.right;
    //     
    //     Vector3 directionToCursor = (cursor.transform.localPosition - transform.localPosition).normalized;
    //     
    //     float distance = Vector3.Distance(eventData.position, transform.localPosition); 
    //     
    //     if (cursor.isDragging && distance > 100 && _hasSegment)
    //     {
    //         cursor.SetSnapped(false);
    //         //transform.localScale = Vector3.one;
    //         Debug.Log($"XXX Cursor no longer snapped.");
    //         _hasSegment = false;
    //     }
    // }
}
