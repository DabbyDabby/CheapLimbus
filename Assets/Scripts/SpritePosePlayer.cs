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
    public Sprite sprite;                 // artwork for this pose
    [Min(0f)] public float hold;          // seconds to keep it on screen
}

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePosePlayer : MonoBehaviour
{
    [Tooltip("If left empty, component searches itself.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Ordered list of poses for this animation.")]
    public PoseFrame[] poses;

    [Tooltip("Start the animation automatically in OnEnable().")]
    public bool playOnEnable = true;

    [Tooltip("Loop forever.  Set to false for one‑shot combat skills.")]
    public bool loop;

    private Sequence _sequence;

    private void Awake()
    {
        // Fallback to the local SpriteRenderer so the user can just drag & drop the script
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        BuildSequence();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }

    // ─────────────────── public control API ───────────────────

    /// <summary>Start (or restart) the pose animation.</summary>
    public void Play()
    {
        if (_sequence == null) BuildSequence();
        _sequence.Restart();
    }

    /// <summary>Pause the sequence at its current frame.</summary>
    public void Stop() => _sequence?.Pause();

    /// <summary>Coroutine‑friendly variant that waits until the last pose has finished.</summary>
    public IEnumerator PlayRoutine()
    {
        Play();
        yield return _sequence.WaitForCompletion();
    }

    // ─────────────────── internal helpers ───────────────────

    private void BuildSequence()
    {
        if (poses == null || poses.Length == 0)
        {
            Debug.LogWarning($"{nameof(SpritePosePlayer)} on {name} has no poses.");
            return;
        }

        _sequence = DOTween.Sequence()
                          .SetAutoKill(false)  // reuse between turns
                          .Pause();

        foreach (var pf in poses)
        {
            // Capture variable to avoid closure pitfall
            Sprite capturedSprite = pf.sprite;
            _sequence.AppendCallback(() => spriteRenderer.sprite = capturedSprite)
                     .AppendInterval(Mathf.Max(0f, pf.hold));
        }

        if (loop) _sequence.SetLoops(-1);
    }
}
