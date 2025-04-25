using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI element that shows one skill icon, lets the combat system pick it, and
/// knows whether the player can currently select it.
/// </summary>
[RequireComponent(typeof(Image))]
public class SkillSlot : MonoBehaviour
{
    // ─────────────── Inspector fields ───────────────
    [Header("Skill pool (drag SkillData assets here)")]
    public List<SkillData> skillsList = new List<SkillData>();

    [Header("Runtime references")]                      // these can stay null for now
    [SerializeField] private CursorController cursor;    // optional: only if your cursor script needs the slot
    [SerializeField] private UILineDrag       lineDrag;  // optional: draws connection lines

    // ─────────────── Public API ───────────────
    public SkillData Skill { get; private set; }         // the skill currently shown on this slot
    public bool isSelectable { get; private set; }
    public int columnIndex { get; private set; }

    // ─────────────── Private state ───────────────
    private Image      _image;
    private CanvasGroup _canvasGroup;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        _image       = GetComponent<Image>();
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        RefreshSkill();               // pick an initial icon so the grid isn't empty
        SetSelectable(false);         // UI comes up dimmed; CombatManager enables on its turn
    }

    /// <summary>
    /// Picks a random SkillData from <see cref="skillsList"/> and updates the icon.
    /// Call this when re‑rolling the dashboard.
    /// </summary>
    public void RefreshSkill()
    {
        if (skillsList == null || skillsList.Count == 0)
        {
            Debug.LogWarning($"{name}: skillsList is empty – drag SkillData assets onto the list in the Inspector.");
            return;
        }

        Skill        = skillsList[Random.Range(0, skillsList.Count)];
        _image.sprite = Skill.icon;
    }

    /// <summary>
    /// Turns the highlight/interaction of the slot on or off.
    /// The CombatManager should call this at the start/end of the chain‑selection phase.
    /// </summary>
    public void SetSelectable(bool selectable)
    {
        isSelectable           = selectable;
        _canvasGroup.interactable = selectable;
        _canvasGroup.alpha        = selectable ? 1f : 0.5f;
    }
}
