using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Attack1", menuName = "SkillObjects/Attack1", order = 1)]
public class Attack1 : Skill //Just holds data and animation
{
    public override SkillResult Execute(int coins) //return HP lost/gain/etc....
    {
        if (coins == 2)
        {
            //play move 1 first sprite 3rd psrite ytadaadda
        }//else more logic on fancy animations depending on context for this skill

        var finalDamage = 5 + 5 + 7;
        return new SkillResult()
        {
            Damage = finalDamage
        };
    }
}
