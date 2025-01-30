using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IDropHandler
{
    public Skill skill;
    public List<Skill> SkillsList;
    public int columnIndex;
    private Image _image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _image = GetComponent<Image>();

        UILineDrag2 lineManager = FindObjectOfType<UILineDrag2>();
        if (lineManager != null)
        {
            //lineManager.RegisterSkillSlot(this);
            Debug.Log($"✅ Skill slot {name} registered.");
        }
    }


    public void RefreshSkills()
    {
        skill = SkillsList[Random.Range(0, SkillsList.Count)];
        _image.sprite = skill.skillIcon;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        //lineManager.SelectSkill(skill, columnIndex);

        Debug.Log("OnDrop triggered.");
        if (eventData.pointerDrag != null)
        {
            // Snap the dragged cursor to the slot's local position
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        }
    }

}
