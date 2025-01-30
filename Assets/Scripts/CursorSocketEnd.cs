using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorSocketEnd : MonoBehaviour, IDropHandler
{
    [SerializeField] private UILineDrag2 lineDrag;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        }
    }
    
    public void InitiateCombat()
    {
        //List<SkillSlot> selectedSkills = lineDrag.GetSelectedSkills(); // Fetch selected skills

        //if (selectedSkills.Count == 0)
        //{
        //    Debug.LogWarning("No skills selected! Cannot start combat.");
        //    return;
        //}

        //Debug.Log("Combat Starting with Selected Skills:");
        //foreach (SkillSlot skill in selectedSkills)
        //{
        //    Debug.Log($"Skill: {skill.name} from Column {skill.columnIndex}");
        //}

        // TODO: Send `selectedSkills` data to the combat system
    }
}
