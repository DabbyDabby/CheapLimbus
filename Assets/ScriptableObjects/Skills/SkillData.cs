using UnityEngine;

public enum SkillKind { Attack, Buff, Heal }

[CreateAssetMenu(menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("UI")]
    public SkillKind kind;
    public string skillName;
    public Sprite icon;

    [Header("Combat")]
    public int baseDamage = 6;
    public int healAmount = 150;
    public PoseFrame[] poses;        // leave empty if you animate differently
    public float moveTime = .20f;    // dash speed, lunge time, etc.
}