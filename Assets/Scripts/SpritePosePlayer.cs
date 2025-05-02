using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// A single-pose sprite player: feeds an ordered list of poses (sprite + hold time)
/// to DOTween so you can trigger it from your battle code or simply let it run
/// on enable.  Add it to the same GameObject that has the SpriteRenderer.
/// </summary>

[System.Serializable]
public struct PoseFrame
{
    public Sprite sprite;
    [Min(0f)] public float hold;

    [Tooltip("Percentage of total skill damage dealt when this frame appears (0-100).")]
    [Range(0,100)] public int hitPercent;   // 0 = no damage; 25 = quarter, etc.
}


[RequireComponent(typeof(SpriteRenderer))]
public class SpritePosePlayer : MonoBehaviour
{
    public PoseFrame[] poses;                    // filled by SkillData at runtime
    public event Action<int> OnFrame;            // <--- NEW

    private SpriteRenderer _sr;
    private Sequence       _seq;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        BuildSequence();                         // build once, reuse
    }

    // ───────────────────────── core builder ─────────────────────────
    private void BuildSequence()
    {
        _seq = DOTween.Sequence().SetAutoKill(false).Pause();

        for (int i = 0; i < poses.Length; i++)
        {
            int step = i;                        // capture loop variable
            _seq.AppendCallback(() =>
                {
                    _sr.sprite = poses[step].sprite; // swap sprite
                    OnFrame?.Invoke(step);           // <--- NEW callback
                })
                .AppendInterval(poses[step].hold);   // wait hold time
        }
    }

    /// <summary>Plays the prepared sequence once and blocks until finished.</summary>
    public IEnumerator PlayRoutine()
    {
        if (_seq == null) BuildSequence();       // safety
        _seq.Restart();
        yield return _seq.WaitForCompletion();
    }
}
