using System.Collections;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "SkillObjects/Dash Behind")]
public class DashBehindSkill : Skill
{
    [SerializeField] private PoseFrame[] poses;
    [SerializeField] private float moveTime   = .20f;
    [SerializeField] private float backOffset = .60f;

    public void Play(GameObject attackerGO, GameObject targetGO, int coins)
    {
        // ── Grab components we need ──────────────────────────────
        var attacker   = attackerGO.transform;
        var target     = targetGO.transform;
        var posePlayer = attackerGO.GetComponent<SpritePosePlayer>();

        // 1️⃣  sprite poses
        //IEnumerator spriteCo = posePlayer.PlayRoutine(poses);

        // 2️⃣  dash movement
        Vector3 dirToTarget = (target.position - attacker.position).normalized;
        Vector3 behind      = target.position - dirToTarget * backOffset;

        Tween dash = attacker.DOMove(behind, moveTime)
            .SetEase(Ease.InOutQuad);

        // 3️⃣  wait for both to complete
        //yield return attackerGO.GetComponent<MonoBehaviour>()
        //    .StartCoroutine(spriteCo);
        //yield return dash.WaitForCompletion();
    }

    //public SkillResult Execute(int coins) =>
    //    new SkillResult { Damage = 8 };
}