using UnityEngine;

public interface ISkill
{
    public SkillResult Execute(int coins);
    public void Clash(bool iWin);
}
