using System;
using System.Collections.Generic;
using UnityEngine;
public class Skill : ScriptableObject, ISkill
{
    public List<int> damage;
    public List<Move> Moves;
    public Sprite skillIcon;
    public virtual SkillResult Execute(int coins)
    {
        return new SkillResult();
    }
    public void Clash(bool isWin)
    {
        //do win clash or move clash here, domove yadad yada
    }
}
[Serializable]
public class Move
{
    public List<Sprite> sprites;
}
[Serializable]
public class SkillResult
{
    public int Damage;
    //debuf, healing, etc...
}
