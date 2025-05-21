using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro.SpriteAssetUtilities;
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

    
    public IEnumerator PerformSequentialZooms(int camId = 0)
    {
        if (cameraMgr == null) yield break;
        cameraMgr.ResetZoom(camIndex, 0.2f, Ease.OutExpo);

        // First zoom
        Tween firstZoom = cameraMgr.ZoomZ(camIndex, 50f, 50f,1f, Ease.OutSine);
        if (firstZoom != null)
        {
            // Wait for first zoom to complete
            yield return firstZoom.WaitForCompletion();
        }

        // Second zoom
        Tween secondZoom = cameraMgr.ZoomZ(camIndex, 40f, 40f,0.5f, Ease.OutSine);
        if (secondZoom != null)
        {
            // Wait for second zoom to complete
            yield return secondZoom.WaitForCompletion();
        }
    }
    
    IEnumerator ExecuteSkill(SkillData data, Unit attacker, Unit target)
    {
        if (data == null) { Debug.LogError("null SkillData"); yield break; }

        attacker.PosePlayer.poses = data.poses;
        SpritePosePlayer spp = attacker.PosePlayer;
        int total = data.totalDamage;

        // —— Camera setup ————
        int attackCam = attacker == playerUnit ? 1 : 2;
        int high = attacker == playerUnit ? 21 : 22;
        cameraMgr.Track(attackCam, attacker.Tf);
        cameraMgr.BlendTo(attackCam, high);

        // Start playing the animation (this triggers OnFrame events)
        IEnumerator spriteCo = spp.PlayRoutine();
        Coroutine spriteRoutine = StartCoroutine(spriteCo);

        // Hook per-frame logic
        System.Action<int> onFrame = null;
        onFrame = step =>
        {
            var frame = data.poses[step];
            int pct = frame.hitPercent;

            if (pct > 0)
            {
                int dmg = Mathf.RoundToInt(total * pct / 100f);
                target.TakeDamage(dmg);
                Debug.Log($"Frame {step} triggered — hitPercent = {pct}");
            }

            switch (data.skillName)
            {
                case "Silent Step":
                    if (step == 1)
                    {
                        // Just camera zoom
                        //cameraMgr.ZoomZ(attackCam, 50f, 65f, frame.hold, Ease.OutSine);
                    }
                    else if (step == 2)
                    {
                        // Slash impact: shake + pulse
                        cameraMgr.PulseCamera(attackCam, 0.15f, Ease.OutSine, 0.1f, 0.5f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.5f);
                        StartCoroutine(target.GetComponent<MoveAround>().Knockback());
                        attacker.GetComponent<MoveAround>().SlashVFX(0);
                        //cameraMgr.ZoomZ(attackCam,40f, 45f, frame.hold, Ease.OutExpo);
                    }
                    else if (step == 3)
                    {
                        // Landed: return to default camera zoom
                        //cameraMgr.ZoomZ(attackCam,40f, 65f, frame.hold, Ease.OutExpo);
                        
                    }
                    break;

                case "Overhead Swing":
                    if (step == 3)
                    {
                        cameraMgr.PulseCamera(attackCam, 0.15f, Ease.OutSine, 0.1f, 0.5f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.5f);
                        attacker.GetComponent<MoveAround>().SlashVFX(2);
                        StartCoroutine(target.GetComponent<MoveAround>().Bounce());
                    }
                    
                    else if (step == 5)
                    {
                        cameraMgr.PulseCamera(attackCam, 0.15f, Ease.OutSine, 0.1f, 0.5f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.5f);
                        attacker.GetComponent<MoveAround>().SlashVFX(3);
                    }
                    break;

                case "Crescent Moon Kick":
                    if (step == 4)
                    {
                        cameraMgr.PulseCamera(attackCam, 0.15f, Ease.OutSine, 0.1f, 0.5f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.5f);
                        StartCoroutine(target.GetComponent<MoveAround>().Knockback(0.5f));
                        
                    }
                    else if (step == 6)
                    {
                        cameraMgr.PulseCamera(attackCam, 0.15f, Ease.OutSine, 0.1f, 0.5f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.5f);
                        StartCoroutine(target.GetComponent<MoveAround>().Bounce(3));
                    }
                    break;
                    
                case "Retribuzione":
                    if (step == 4)
                    {
                        cameraMgr.PulseCamera(attackCam, 0.15f, Ease.OutSine, 0.1f, 0.5f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.5f);
                        StartCoroutine(target.GetComponent<MoveAround>().Knockback(0.5f));
                        
                    }
                    else if (step == 6)
                    {
                        cameraMgr.PulseCamera(attackCam, 0.15f, Ease.OutSine, 0.1f, 0.5f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.5f);
                        StartCoroutine(target.GetComponent<MoveAround>().Bounce(3));
                    }
                    else if (step == 9)
                    {
                        cameraMgr.PulseCamera(attackCam, 0.2f, Ease.OutSine, 0.1f, 0.8f);
                        cameraMgr.ShakeCamera(attackCam, 10f, 0.8f);
                        StartCoroutine(target.GetComponent<MoveAround>().Knockback());
                        attacker.GetComponent<MoveAround>().SlashVFX(4);
                    }
                    break;
            }
        };

        spp.OnFrame += onFrame;

        // 1) Silent Step
        if (data.skillName == "Silent Step")
        {
            yield return new WaitForSeconds(0.05f); // Wait before reposition
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.3f, 1.75f, Ease.OutExpo);
            yield return new WaitForSeconds(0.2f); // Wait before reposition
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.25f, -5f, Ease.OutSine);
        }
        
        // — Step 3 dash towards enemy —
        if (data.skillName == "Overhead Swing")
        {
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.3f, 1.75f, Ease.OutSine);
            yield return new WaitForSeconds(1.2f); // Wait before reposition
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.1f, 1.2f, Ease.OutSine);
        }
        
        if (data.skillName == "Crescent Moon Kick")
        {
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.3f, 1.75f, Ease.OutSine);
            //yield return new WaitForSeconds(1.2f); // Wait before reposition
            //yield return attacker.GetComponent<MoveAround>().DashToTarget(0.1f, 1.2f);
        }
        
        if (data.skillName == "Retribuzione")
        {
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.3f, 1.2f, Ease.OutSine);
            yield return new WaitForSeconds(1.65f); // Wait before reposition
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.2f, 1.2f, Ease.OutSine);
            yield return new WaitForSeconds(0.3f); // Wait before reposition
            yield return attacker.GetComponent<MoveAround>().DashToTarget(0.2f, -5f, Ease.OutSine);
        }

        // Wait for the sprite animation to finish
        yield return spriteRoutine;

        spp.OnFrame -= onFrame;

        // —— Reset camera (optional) ————
        cameraMgr.BlendTo(0); // 0 = wide cam
    }


    // Rewinds the camera and waits until the tween finishes.
    private IEnumerator ZoomBack(float duration = 0.25f)
    {
        Tween tw = cameraMgr.ZoomZ(camIndex, 60f, -8.5f, duration, Ease.OutExpo);
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
            
            Coroutine zoomToClash = StartCoroutine(PerformSequentialZooms(0));

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
            
            int randomSfx = UnityEngine.Random.Range(0, 3);

            // 3) Check result (WasSuccess) 
            if (qteManager.WasSuccess)
            {
                // Player wins => enemy coin--
                enemyCoins--;
                Debug.Log($"Player wins the clash! Enemy coins now {enemyCoins}");

                // MoveAround visuals
                playerMover.WinClash();
                enemyMover.LoseClash();
                if (enemyCoins == 0)
                {
                    playerMover.PlaySFX(4);
                    cameraMgr.PulseCamera(0, 0.15f, Ease.OutExpo, 0.1f);
                }
                else
                {
                    playerMover.PlaySFX(randomSfx);
                }

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
                enemyMover.PlaySFX(randomSfx);

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