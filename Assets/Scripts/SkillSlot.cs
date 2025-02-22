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
    public bool isSelectable = false;
    
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
        isSelectable = selectable;
        _skillSlotCanvasGroup.interactable = selectable;
    }
}
