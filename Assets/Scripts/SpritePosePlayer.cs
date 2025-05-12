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
    
    //public PoseEvent eventType;   // optional one-shot effect
    public AudioClip sfx;         // optional SFX for this frame
    public GameObject vfxPrefab;  // optional VFX to spawn
    public Vector2 vfxOffset;     // offset from target's position
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
        if (!_sr)                    // null check
        {
            Debug.LogError($"{name}: SpriteRenderer missing!");
            enabled = false;         // stops Update/LateUpdate
            return;
        }
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
        if (poses == null || poses.Length == 0)
        {
            Debug.LogWarning($"{name}: No poses to play!");
            yield break;
        }

        for (int i = 0; i < poses.Length; i++)
        {
            PoseFrame pose = poses[i];
            _sr.sprite = pose.sprite;
            
            

            OnFrame?.Invoke(i); // ✅ THIS IS CRUCIAL

            yield return new WaitForSeconds(pose.hold);
        }
    }

}
