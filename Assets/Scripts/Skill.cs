using DG.Tweening;
using UnityEngine;

public enum SkillType
{
    SingleSlash,
    OverheadSlash,
    CrescentMoonKick,
    Buff,
    Debuff
}

[System.Serializable]
public class Skill : MonoBehaviour
{
    [Header("Skill 1: Single Slash")]
    public Sprite[] singleSlashFrames;      // e.g. frames or relevant sprites for single slash
    public float singleSlashDamage = 5f;
    public float singleSlashDuration = 0.5f;

    [Header("Skill 2: Overhead Slash + Stab")]
    public Sprite[] overheadSlashFrames;
    public float overheadSlashDamage = 10f;
    public float overheadSlashDuration = 1f;

    [Header("Skill 3: CrescentMoonKick + Swing")]
    public Sprite[] crescentKickFrames;
    public float crescentKickDamage = 8f;
    public float crescentKickDuration = 1f;

    public Sprite skillIcon;
    public SkillType skillType;
    public int baseDamage;

    // If you want an animation or effect reference:
    // public AnimationClip skillAnimation;
    // public ParticleSystem skillEffect;

    // Example method that applies the skill's effect 
    
    /// <summary>
    /// Plays the selected skill's animation (via DOTween) 
    /// and applies damage to the defender.
    /// </summary>
    public void PlaySkillAnimation(SkillType type, SpriteRenderer userSpriteRenderer, GameObject defender)
    {
        switch (type)
        {
            case SkillType.SingleSlash:
                AnimateSingleSlash(userSpriteRenderer, defender);
                break;

            case SkillType.OverheadSlash:
                AnimateOverheadSlash(userSpriteRenderer, defender);
                break;

            case SkillType.CrescentMoonKick:
                AnimateCrescentMoonKick(userSpriteRenderer, defender);
                break;
        }
    }

    #region Skill Animations
    private void AnimateSingleSlash(SpriteRenderer sr, GameObject defender)
    {
        // Example: show a DOTween scale “slash” effect
        // Then apply damage after the animation completes (or partway through).
        
        // Change to first slash frame, if available
        if (singleSlashFrames.Length > 0)
            sr.sprite = singleSlashFrames[0];

        Sequence slashSequence = DOTween.Sequence()
            .Append(sr.transform
                .DOScale(1.2f, singleSlashDuration * 0.5f)
                .SetEase(Ease.OutQuad))
            .Append(sr.transform
                .DOScale(1.0f, singleSlashDuration * 0.5f)
                .SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                // Apply damage
                defender.GetComponent<Unit>()?.TakeDamage((int)singleSlashDamage);
            });
    }

    private void AnimateOverheadSlash(SpriteRenderer sr, GameObject defender)
    {
        // Overhead slash example
        if (overheadSlashFrames.Length > 0)
            sr.sprite = overheadSlashFrames[0];

        Sequence overheadSeq = DOTween.Sequence()
            // Slight rotation down for overhead
            .Append(sr.transform.DORotate(new Vector3(0, 0, 45), overheadSlashDuration * 0.3f).SetEase(Ease.OutExpo))
            // Rotate up for the stab
            .Append(sr.transform.DORotate(new Vector3(0, 0, -30), overheadSlashDuration * 0.3f).SetEase(Ease.OutExpo))
            .Append(sr.transform.DORotate(Vector3.zero, overheadSlashDuration * 0.4f).SetEase(Ease.InOutSine))
            .OnComplete(() =>
            {
                defender.GetComponent<Unit>()?.TakeDamage((int)overheadSlashDamage);
            });
    }

    private void AnimateCrescentMoonKick(SpriteRenderer sr, GameObject defender)
    {
        // Kick + wide swing example
        if (crescentKickFrames.Length > 0)
            sr.sprite = crescentKickFrames[0];

        Sequence kickSequence = DOTween.Sequence()
            // Kick
            .Append(sr.transform.DORotate(new Vector3(0, 0, -45), crescentKickDuration * 0.3f).SetEase(Ease.OutSine))
            // Then wide swing
            .Append(sr.transform.DORotate(new Vector3(0, 0, 30), crescentKickDuration * 0.3f).SetEase(Ease.OutSine))
            .Append(sr.transform.DORotate(Vector3.zero, crescentKickDuration * 0.4f).SetEase(Ease.InOutSine))
            .OnComplete(() =>
            {
                defender.GetComponent<Unit>()?.TakeDamage((int)crescentKickDamage);
            });
    }
    #endregion

}