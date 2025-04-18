using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private MoveAround playerMover; 
    [SerializeField] private MoveAround enemyMover;

    [Header("Coin Settings")]
    [SerializeField] private int playerCoins = 3;
    [SerializeField] private int enemyCoins = 3;

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

    private void Start()
    {
        qteManager.SetUIActive(false);
    }

    // Called by CursorSocket script after the user drags the cursor to the "socket"
    public void StartCombat()
    {
        if (!inCombat)
        {
            StartCoroutine(CombatFlow());
        }
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
            Debug.Log("It's a tie! Both sides lost all coins simultaneously.");
            EndCombat();
        }
        else if (playerCoins <= 0)
        {
            Debug.Log("Enemy wins! Player is out of coins.");
            EndCombat();
        }
        else
        {
            Debug.Log("Player wins! Enemy is out of coins.");
            EndCombat();
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
