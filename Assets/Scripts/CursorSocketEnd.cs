using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorSocketEnd : MonoBehaviour, IDropHandler
{
    public CombatManager combatManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatManager = GetComponent<CombatManager>();
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
        
    }
}
