using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public List<Skill> playerInputSkills = new List<Skill>();
    public List<Skill> enemyInputSkills = new List<Skill>();

    public MoveAround actor1;
    public MoveAround actor2;
    public bool combatStart = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatStart = false;
    }

    public void SetCombatStart(bool value)
    {
        combatStart = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (combatStart)
        {
            actor1.DashToClashPoint();
            actor2.DashToClashPoint();
            SetCombatStart(false);
        }
    }
}
