using System;
using UnityEngine;

/// <summary>
/// Centralised combat stats & helpers for a single character.
/// Keeps HP, coins and the "Charge" resource, and exposes
/// convenience references to frequently‑used components.
/// </summary>
[DisallowMultipleComponent]
public class Unit : MonoBehaviour
{
    // ─────────────── Editable stats (Inspector) ───────────────
    [Header("Core Stats")]
    [SerializeField] private int maxHp = 200;

    [Header("Battle Resources")]
    [SerializeField] private int maxCoins = 3;           // tries / stamina for clashes
    [SerializeField] private int maxCharge = 5;         // cap before "overcharged" skill unlocks
    
    [SerializeField] private EKG_UI ekgPrefab;
    [SerializeField] private Canvas worldCanvas;

    // ─────────────── Runtime state ───────────────
    public int MaxHp        => maxHp;                  // read‑only publics
    public int CurrentHp    { get; private set; }

    public int Coins        { get; private set; }
    public int Charge       { get; private set; }       // 0‑5
    public Vector3 SpawnPos { get; private set; }   // PascalCase property


    // ─────────────── Cached components ───────────────
    public SpriteRenderer  SpriteRenderer { get; private set; }
    public SpritePosePlayer PosePlayer    { get; private set; }
    public Transform       Tf             { get; private set; }

    // ─────────────── Events (UI hooks) ───────────────
    public event Action<Unit,int> OnDamaged;        // (who, dmg)
    public event Action<Unit,int> OnHealed;         // (who, heal)
    public event Action<Unit,int> OnChargeChanged;  // (who, newCharge)
    public event Action<Unit>     OnChargeMaxed;    // fired exactly when Charge hits _maxCharge
    public event Action<Unit>     OnDeath;

    // ─────────────────────── Unity lifecycle ───────────────────────
    private void Awake()
    {
        CurrentHp = maxHp;
        Coins     = maxCoins;
        Charge    = 0;

        // cache components
        SpriteRenderer = GetComponent<SpriteRenderer>();
        PosePlayer     = GetComponent<SpritePosePlayer>();
        Tf             = transform;
        SpawnPos = transform.position;
        
        

        
        if (ekgPrefab && worldCanvas)
        {
            var ui = Instantiate(ekgPrefab, worldCanvas.transform);
            ui.Init(this, Camera.main, (RectTransform)worldCanvas.transform);
        }
        
    }

    // ─────────────────────── Public API ───────────────────────
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHp == 0) return;

        CurrentHp = Mathf.Max(CurrentHp - amount, 0);
        OnDamaged?.Invoke(this, amount);
        Debug.Log($"{name} took {amount} dmg → {CurrentHp}/{MaxHp} HP");

        if (CurrentHp == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHp == MaxHp) return;

        int before = CurrentHp;
        CurrentHp  = Mathf.Min(CurrentHp + amount, MaxHp);
        OnHealed?.Invoke(this, CurrentHp - before);
        Debug.Log($"{name} healed {CurrentHp - before} → {CurrentHp}/{MaxHp} HP");
    }

    /// <summary>
    /// Adds Charge (max 5). When the cap is reached for the first time it
    /// invokes <see cref="OnChargeMaxed"/> so the CombatManager can enable the
    /// overcharged skill slot.
    /// </summary>
    public void GainCharge(int amount = 1)
    {
        if (amount <= 0 || Charge >= maxCharge) return;

        int before = Charge;
        Charge = Mathf.Clamp(Charge + amount, 0, maxCharge);
        OnChargeChanged?.Invoke(this, Charge);
        Debug.Log($"{name} gained {Charge - before} Charge → {Charge}/{maxCharge}");

        if (Charge == maxCharge && before < maxCharge)
        {
            OnChargeMaxed?.Invoke(this);
            Debug.Log($"{name} is fully charged – overcharge skill unlocked!");
        }
    }

    /// <summary>
    /// Generic entry point for buff‑type SkillData. Extend this with a switch
    /// when you add more buff kinds (def up, speed up, etc.).
    /// </summary>
    public void ApplyBuff(SkillData buff)
    {
        if (buff == null) return;

        switch (buff.effect)
        {
            case BuffEffect.None:
                break;  
            
            case BuffEffect.Heal:
                Heal(buff.healAmount);
                break;

            case BuffEffect.GainCharge:
                GainCharge(buff.chargeGain);
                break;

            default:
                Debug.LogWarning($"BuffEffect '{buff.effect}' has no handler yet");
                break;
        }
    }



    // ─────────────────────── Internals ───────────────────────
    private void Die()
    {
        OnDeath?.Invoke(this);
        // TODO: disable input, play animation, etc.
        Debug.Log($"{name} died.");
    }
}
