using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QTEManager qteManager;
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
    private SkillData chosenSkill => currentSlot ? currentSlot.Skill : null;
    


    [Header("Clash Timing")]
    [Tooltip("Initial time window (seconds) for each dash + QTE")]
    [SerializeField] private float initialTimeWindow = 2.0f;

    [Tooltip("Minimum possible time window")]
    [SerializeField] private float minTimeWindow = 1.0f;

    [Tooltip("How much the time window is reduced each normal clash")]
    [SerializeField] private float timeWindowDecrement = 0.2f;

    [Tooltip("Required key presses for Mashing QTE")]
    [SerializeField] private int requiredPressesMashing = 8;

    private float currentTimeWindow;
    private bool inCombat = false;
    
    // current chosen UI slot and its data


    private void Start()
    {
        playerUnit = playerMover.GetComponent<Unit>();
        enemyUnit  = enemyMover.GetComponent<Unit>();

        qteManager.SetUIActive(false);
        camIndex = cameraMgr.cameras.Length;
    }


    // Called by CursorSocket script after the user drags the cursor to the "socket"
    public void StartCombat()
    {
        if (!inCombat)
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
        currentSlot = null;                               // reset from last turn

        foreach (SkillSlot slot in selectedSlots)
        {
            SkillData sd = slot.Skill;
            switch (sd.kind)
            {
                case SkillKind.Buff:
                    playerUnit.ApplyBuff(sd);             // write this in Unit.cs
                    break;

                case SkillKind.Heal:
                    playerUnit.Heal(sd.healAmount);
                    break;

                case SkillKind.Attack:
                    currentSlot = slot;                   // remember for ExecuteSkill
                    break;
            }
        }

        if (currentSlot == null)
            Debug.LogWarning("No attack skill in the selected chain!");
    }

    
    public IEnumerator PerformSequentialZooms(float fastTime, float slowTime, int camIndex = 0)
    {
        if (cameraMgr == null) yield break;

        // First zoom
        Tween firstZoom = cameraMgr.ZoomZ(camIndex, -7f, fastTime, Ease.OutSine);
        if (firstZoom != null)
        {
            // Wait for first zoom to complete
            yield return firstZoom.WaitForCompletion();
        }

        // Second zoom
        Tween secondZoom = cameraMgr.ZoomZ(camIndex, -6f, slowTime, Ease.OutSine);
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
        cameraMgr.ZoomZ(0, zoomIn, duration * 0.5f, Ease.InQuad)
            ?.OnComplete(() =>
                cameraMgr.ZoomZ(0, cameraMgr.DefaultOffset, 0.25f, Ease.OutQuad));
    }

    
    IEnumerator ExecuteSkill(SkillData data, Unit attacker, Unit target)
    {
        if (data == null) yield break;

        // 1) Subscribe
        SpritePosePlayer spp = attacker.PosePlayer;
        int total = data.totalDamage;                    // cache
        spp.OnFrame += step =>
        {
            int pct = data.poses[step].hitPercent;
            if (pct > 0)
            {
                int dmg = Mathf.RoundToInt(total * pct / 100f);
                target.TakeDamage(dmg);
                // ► place VFX / SFX / camera tweens here if you want them on-hit
            }

            // example extra: dash ends exactly when frame 1 shows
            if (step == 1) CameraSlowmo(0.3f, .15f);
        };

        // 2) PRE-movement (dash towards enemy)
        Vector3 dashTarget = target.Tf.position
                             - (target.Tf.position - attacker.Tf.position).normalized * .6f;
        Tween dash = attacker.Tf.DOMove(dashTarget, data.moveTime).SetEase(Ease.OutQuad);

        // 3) Play sprite animation in parallel
        IEnumerator spriteCo = spp.PlayRoutine();

        // 4) Wait for both to finish
        yield return dash.WaitForCompletion();
        yield return attacker.StartCoroutine(spriteCo);

        // 5) Cleanup
        spp.OnFrame -= null;               // clear all listeners

        // return to neutral
        attacker.Tf.DOMove(attacker.SpawnPos, .25f);
        attacker.SpriteRenderer.sprite = data.poses[0].sprite;   // idle again
        Tween rewind = attacker.Tf.DOMove(attacker.SpawnPos, .25f);
        cameraMgr.ZoomZ(0, cameraMgr.DefaultOffset, .25f, Ease.OutQuad);
        yield return rewind.WaitForCompletion();
    }



    private IEnumerator CombatFlow()
    {
        inCombat = true;

        // Reset coins/time
        playerCoins = 3;
        enemyCoins = 3;
        currentTimeWindow = initialTimeWindow;
        
        Debug.Log($"Combat started! Player: {playerCoins} coins, Enemy: {enemyCoins} coins");

        while (playerCoins > 0 && enemyCoins > 0)
        {
            // If EITHER side is at 1 coin => Mashing QTE
            bool needMashing = (playerCoins == 1 || enemyCoins == 1);

            float timeWindow = needMashing
                ? 2.0f // static 2s if side is at 1 coin 
                : currentTimeWindow;

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
            qteManager.BeginQTE(needMashing, requiredPressesMashing, timeWindow);

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
                    currentTimeWindow = Mathf.Max(
                        minTimeWindow, 
                        currentTimeWindow - timeWindowDecrement
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
                    currentTimeWindow = Mathf.Max(
                        minTimeWindow,
                        currentTimeWindow - timeWindowDecrement
                    );
                }
            }

            // Optional short delay 
            yield return new WaitForSeconds(0.5f);
        }

        // Someone hit 0
        if (playerCoins <= 0 && enemyCoins <= 0)
        {
            yield return new WaitForSeconds(0.2f);
            if (cameraMgr != null) {
                Tween camZoomOut = cameraMgr.ZoomZ(camIndex, -8.5f, 0.2f, Ease.OutQuad);
                if (camZoomOut != null)
                {
                    // Wait for first zoom to complete
                    yield return camZoomOut.WaitForCompletion();
                }
            }
            Debug.Log("It's a tie! Both sides lost all coins simultaneously.");
            yield return StartCoroutine(ExecuteSkill(chosenSkill, playerUnit, enemyUnit));
        }
        else if (playerCoins <= 0)
        {
            yield return new WaitForSeconds(0.2f);
            if (cameraMgr != null) {
                Tween camZoomOut = cameraMgr.ZoomZ(camIndex, -8.5f, 0.2f, Ease.OutQuad);
                if (camZoomOut != null)
                {
                    // Wait for first zoom to complete
                    yield return camZoomOut.WaitForCompletion();
                }
            }
            Debug.Log("Enemy wins! Player is out of coins.");
            yield return StartCoroutine(ExecuteSkill(chosenSkill, playerUnit, enemyUnit));

        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            if (cameraMgr != null) {
                Tween camZoomOut = cameraMgr.ZoomZ(0, -8.5f, 0.2f, Ease.OutQuad);
                if (camZoomOut != null)
                {
                    // Wait for first zoom to complete
                    yield return camZoomOut.WaitForCompletion();
                    Debug.Log("CAMERA ZOOMED OUT.");
                }
            }
            Debug.Log("Player wins! Enemy is out of coins.");
            yield return StartCoroutine(ExecuteSkill(chosenSkill, playerUnit, enemyUnit));

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
        inCombat = false;
        qteManager.SetUIActive(false);
        Debug.Log("Combat forcibly ended.");
    }
}
