using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QteManager qteManager;
    [SerializeField] private MoveAround playerMover; 
    [SerializeField] private MoveAround enemyMover;
    [SerializeField] private Unit playerUnit;
    [SerializeField] private Unit enemyUnit;
    [SerializeField] private CameraController cameraMgr;
    [SerializeField] private int camIndex = 0;
    
    [Header("Coin Settings")]
    [SerializeField] private int playerCoins = 3;
    [SerializeField] private int enemyCoins = 3;
    [SerializeField] public SkillSlot currentSlot;   // drag the slot in, or set it in code
    private SkillData ChosenSkill => currentSlot ? currentSlot.Skill : null;
    


    [Header("Clash Timing")]
    [Tooltip("Initial time window (seconds) for each dash + QTE")]
    [SerializeField] private float initialTimeWindow = 2.0f;

    [Tooltip("Minimum possible time window")]
    [SerializeField] private float minTimeWindow = 1.0f;

    [Tooltip("How much the time window is reduced each normal clash")]
    [SerializeField] private float timeWindowDecrement = 0.2f;

    [Tooltip("Required key presses for Mashing QTE")]
    [SerializeField] private int requiredPressesMashing = 8;

    private float _currentTimeWindow;
    private bool _inCombat = false;
    
    // current chosen UI slot and its data


    private void Start()
    {
        playerUnit = playerMover.GetComponent<Unit>();
        enemyUnit  = enemyMover.GetComponent<Unit>();
        Debug.Log($"playerUnit={playerUnit}, enemyUnit={enemyUnit}");

        qteManager.SetUIActive(false);
        camIndex = 0;
    }


    // Called by CursorSocket script after the user drags the cursor to the "socket"
    public void StartCombat()
    {
        if (!_inCombat)
        {
            StartCoroutine(CombatFlow());
        }
    }
    
    /// <summary>
    /// Applies every non-attack skill in the player’s selected chain and sets
    /// currentSlot to the single attack skill that must be executed later.
    /// Call once immediately before CombatFlow() starts.
    /// </summary>
    public void ApplyInstantSkills(List<SkillSlot> selectedSlots)
    {
        currentSlot = null;                         // reset from last turn

        foreach (SkillSlot slot in selectedSlots)
        {
            SkillData sd = slot.Skill;              // property with capital S
            if (sd.kind == SkillKind.Attack)
                currentSlot = slot;             // remember
            else
                playerUnit.ApplyBuff(sd);
        }

        if (currentSlot == null)
            Debug.LogWarning("No attack skill in the selected chain!");
    }

    
    public IEnumerator PerformSequentialZooms(float fastTime, float slowTime, int camId = 0)
    {
        if (cameraMgr == null) yield break;

        // First zoom
        Tween firstZoom = cameraMgr.ZoomZ(camIndex, -7f, fastTime, Ease.OutExpo);
        if (firstZoom != null)
        {
            // Wait for first zoom to complete
            yield return firstZoom.WaitForCompletion();
        }

        // Second zoom
        Tween secondZoom = cameraMgr.ZoomZ(camIndex, -6f, slowTime, Ease.OutExpo);
        if (secondZoom != null)
        {
            // Wait for second zoom to complete
            yield return secondZoom.WaitForCompletion();
        }
    }
    
    void CameraSlowmo(float timescale, float duration)
    {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, timescale, 0.05f)
            .OnComplete(() => DOTween.To(
                () => Time.timeScale, x => Time.timeScale = x, 1f, duration));
    }

    void CameraPulse(float zoomIn, float duration)
    {
        cameraMgr.ZoomZ(0, zoomIn, duration * 0.5f, Ease.InExpo)
            ?.OnComplete(() =>
                cameraMgr.ZoomZ(0, cameraMgr.DefaultOffset, 0.25f, Ease.OutExpo));
    }

    
    IEnumerator ExecuteSkill(SkillData data, Unit attacker, Unit target)
    {
        if (data == null) { Debug.LogError("null SkillData"); yield break; }

        SpritePosePlayer spp = attacker.PosePlayer;
        int total = data.totalDamage;

        // 1️⃣  create & attach the handler
        System.Action<int> onFrame = null;
        onFrame = step =>
        {
            int pct = data.poses[step].hitPercent;
            if (pct > 0)
            {
                int dmg = Mathf.RoundToInt(total * pct / 100f);
                target.TakeDamage(dmg);
                Debug.Log($"Frame {step} triggered — hitPercent = {pct}");
            }

            if (step == 1) CameraSlowmo(0.3f, .15f);
        };
        spp.OnFrame += onFrame;

        // 2️⃣  dash + play animation
        Vector3 dir = (target.Tf.position - attacker.Tf.position).normalized;
        Vector3 behind = target.Tf.position + dir * (target.SpriteRenderer.bounds.extents.x + 1.3f);
        
        // —— Camera focus on attacker ————————————————
        int attackCam = attacker == playerUnit ? 1 : 2;
        int high = attacker == playerUnit ? 21 : 22;  // unique high per side
        cameraMgr.Track(attackCam, attacker.Tf);        // follow attacker
        cameraMgr.BlendTo(attackCam, high);

        Debug.Log($"Playing {data.poses.Length} poses for {attacker.name} -> {target.name}");
        Tween dash = attacker.Tf.DOMove(behind, data.moveTime).SetEase(Ease.OutExpo);
        IEnumerator spriteCo = spp.PlayRoutine();

        yield return dash.WaitForCompletion();
        yield return attacker.StartCoroutine(spriteCo);

        // 3️⃣  detach the handler to avoid leaks / double-damage next use
        spp.OnFrame -= onFrame;
        
        // —— Blend back to default wide camera ————————
        //cameraMgr.BlendTo(0);                           // 0 = wide combat cam
    }


    // Rewinds the camera and waits until the tween finishes.
    private IEnumerator ZoomBack(float duration = 0.25f)
    {
        Tween tw = cameraMgr.ZoomZ(camIndex, cameraMgr.DefaultOffset, duration, Ease.OutExpo);
        if (tw != null) yield return tw.WaitForCompletion();
    }


    private IEnumerator CombatFlow()
    {
        _inCombat = true;

        // Reset coins/time
        playerCoins = 3;
        enemyCoins = 3;
        _currentTimeWindow = initialTimeWindow;
        
        Debug.Log($"Combat started! Player: {playerCoins} coins, Enemy: {enemyCoins} coins");

        while (playerCoins > 0 && enemyCoins > 0)
        {
            // If EITHER side is at 1 coin => Mashing QTE
            bool needMashing = (playerCoins == 1 || enemyCoins == 1);

            float timeWindow = needMashing
                ? 2.0f // static 2s if side is at 1 coin 
                : _currentTimeWindow;

            // 1) Both sides dash in parallel for 'timeWindow' 
            //    We'll do it in a coroutine that ensures each dash is done in ~2s
            //    Meanwhile, we also run a QTE in parallel for that same timeWindow

            float fastTime = timeWindow * MoveAround.fastApproachRatio;
            float slowTime = timeWindow * MoveAround.slowApproachRatio;
            
            
            Coroutine zoomToClash = StartCoroutine(PerformSequentialZooms(fastTime, slowTime, 0));

            // Start the dash coroutines
            Coroutine dashCoPlayer = StartCoroutine(playerMover.DashToClashPoint(timeWindow));
            Coroutine dashCoEnemy = StartCoroutine(enemyMover.DashToClashPoint(timeWindow));

            // 2) Start QTE
            //    If need Mashing => do Mashing QTE
            //    else => do Single-Press
            qteManager.SetUIActive(true);
            qteManager.BeginQte(needMashing, requiredPressesMashing, timeWindow);

            // Wait for QTE to finish
            // QTEManager sets qteManager.IsActive = false once done
            yield return new WaitUntil(() => !qteManager.IsActive);

            // Also wait for dash to definitely finish 
            // so they remain dashing the entire timeWindow 
            yield return zoomToClash;
            yield return dashCoPlayer;
            yield return dashCoEnemy;

            // 3) Check result (WasSuccess) 
            if (qteManager.WasSuccess)
            {
                // Player wins => enemy coin--
                enemyCoins--;
                Debug.Log($"Player wins the clash! Enemy coins now {enemyCoins}");

                // MoveAround visuals
                playerMover.WinClash();
                enemyMover.LoseClash();

                // Decrement future time window if not a mash clash
                if (!needMashing)
                {
                    _currentTimeWindow = Mathf.Max(
                        minTimeWindow, 
                        _currentTimeWindow - timeWindowDecrement
                    );
                }
            }
            else
            {
                // Player fails => player coin--
                playerCoins--;
                Debug.Log($"Enemy wins the clash! Player coins now {playerCoins}");

                // MoveAround visuals
                playerMover.LoseClash();
                enemyMover.WinClash();

                // Decrement future time window if not a mash clash
                if (!needMashing)
                {
                    _currentTimeWindow = Mathf.Max(
                        minTimeWindow,
                        _currentTimeWindow - timeWindowDecrement
                    );
                }
            }

            // Optional short delay 
            yield return new WaitForSeconds(0.5f);
        }

        // Someone hit 0
        if (playerCoins <= 0)
        {
            yield return new WaitForSeconds(0.2f);
            if (cameraMgr != null) {
                yield return ZoomBack();
            }
            Debug.Log("Enemy wins! Player is out of coins.");
            yield return StartCoroutine(ExecuteSkill(ChosenSkill, enemyUnit, playerUnit));

        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            if (cameraMgr != null) {
                yield return ZoomBack();
            }
            Debug.Log("Player wins! Enemy is out of coins.");
            yield return StartCoroutine(ExecuteSkill(ChosenSkill, playerUnit, enemyUnit));

        }

        Debug.Log("Combat finished!");
        
        // Next turn: user must drag the cursor again to re-trigger StartCombat()
    }

    // If we want to forcibly end (optional)
    public void EndCombat()
    {
        playerMover.ResetSprite();
        enemyMover.ResetSprite();
        StopAllCoroutines();
        qteManager.CancelQTE();
        _inCombat = false;
        qteManager.SetUIActive(false);
        Debug.Log("Combat forcibly ended.");
    }
}
