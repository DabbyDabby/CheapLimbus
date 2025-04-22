using System;
using UnityEngine;

/// <summary>
/// Wraps every combat‑relevant component and stat a character needs
/// so that skills & managers can work with a single, strongly‑typed API
/// instead of scattered GetComponent calls.
/// </summary>
[DisallowMultipleComponent]
public class Unit : MonoBehaviour
{
    // ──────────────── Editable stats ────────────────
    [Header("Core Stats")]
    [SerializeField] private int _maxHP = 200;
    [SerializeField] private int _chargePotency = 0;
    public int maxHP => maxHP;
    public int currentHP { get; private set; }

    [Header("Battle Resources")]
    [SerializeField] private int _maxCoins = 3;          // tries / stamina for QTEs
    public int coins { get; private set; }

    // ──────────────── Cached components ────────────────
    public SpriteRenderer SpriteRenderer { get; private set; }
    public SpritePosePlayer PosePlayer   { get; private set; }
    public Transform        Tf           { get; private set; }

    // ──────────────── Events ────────────────
    public event Action<Unit,int> OnDamaged;    // (who, dmg)
    public event Action<Unit,int> OnHealed;     // (who, amount)
    public event Action<Unit>     OnDeath;

    private void Awake()
    {
        currentHP = _maxHP;
        coins     = _maxCoins;
        

        // Cache components once – faster & cleaner
        SpriteRenderer = GetComponent<SpriteRenderer>();
        PosePlayer     = GetComponent<SpritePosePlayer>();
        Tf             = transform;
    }

    // ──────────────── Public API ────────────────
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHP == 0) return;

        currentHP = Mathf.Max(currentHP - amount, 0);
        OnDamaged?.Invoke(this, amount);

        if (currentHP == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHP == maxHP) return;

        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHealed?.Invoke(this, amount);
    }

    public void GainCharge(int amount)
    {
        if (_chargePotency >= 5) return;
        
    }

    // ──────────────── Internals ────────────────
    private void Die()
    {
        // Broadcast first so listeners can react before the object disappears
        OnDeath?.Invoke(this);
        // Optional: play death animation here or disable input/QTE handler
    }
}
