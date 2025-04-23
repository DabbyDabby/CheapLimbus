using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
    public class SkillData : ScriptableObject
    {
        public Sprite icon;
        public int baseDamage;
        public PoseFrame[] poses;        // keep if you still want pose animations
        public float moveTime = .2f;     // dash / lunge etc.
    }
}
