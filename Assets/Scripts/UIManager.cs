using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<SkillSlot> skillSlots;
    public Lynne lynne;
    private void Awake()
    {
        skillSlots = GetComponentsInChildren<SkillSlot>().ToList();
        //foreach (var socket in skillSlots)
        //{
            //socket.SkillsList = lynne.skills;
        //}
    }
}
