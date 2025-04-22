using UnityEngine;

public interface ISkill
{
    public Skill Execute(int coins);
    public void Clash(bool iWin);
}
