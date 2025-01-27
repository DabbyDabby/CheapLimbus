using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<CursorSocket> cursorSockets;
    public Lynne lynne;
    private void Awake()
    {
        cursorSockets = GetComponentsInChildren<CursorSocket>().ToList();
        foreach (var socket in cursorSockets)
        {
            socket.SkillsList = lynne.skills;
        }
    }
}
