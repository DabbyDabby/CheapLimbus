using System.Collections;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class QteManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI displayBox;
    [SerializeField] private TextMeshProUGUI passBox;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioSource speakerQte;

    [Header("QTE Settings")]
    [Tooltip("How long to display PASS/FAIL after a QTE finishes")]
    [SerializeField] private float resultDisplayTime = 1.0f;

    // QTE state
    public bool IsActive { get; private set; } = false;
    public bool WasSuccess { get; private set; } = false;

    public void SetUIActive(bool isActive)
    {
        displayBox.alpha = isActive ? 1.0f : 0.0f;
        passBox.alpha = isActive ? 1.0f : 0.0f;
    }

    // Called by CombatManager: either single-press or mashing
    public void BeginQte(bool isMashing, int requiredPresses, float timeWindow)
    {
        if (IsActive) return; // already running

        IsActive = true;
        WasSuccess = false;

        // Clear UI
        passBox.text = "";
        displayBox.text = "";

        // Start different coroutines
        if (isMashing)
        {
            StartCoroutine(MashingQTE(requiredPresses, timeWindow));
        }
        else
        {
            StartCoroutine(SinglePressQTE(timeWindow));
        }
    }

    // If forcibly canceled
    public void CancelQTE()
    {
        if (!IsActive) return;
        StopAllCoroutines(); // kills the QTE coroutines
        IsActive = false;
        WasSuccess = false;

        passBox.text = "";
        displayBox.text = "";
    }

    private IEnumerator SinglePressQTE(float timeWindow)
    {
        // Pick random key: E=1, R=2, T=3
        int randomKey = Random.Range(1, 4);

        float timer = timeWindow;
        bool pressed = false;
        bool correct = false;

        // UI
        passBox.text = "";
        displayBox.text = $"[{GetKeyChar(randomKey)}]";

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // if user presses any key
            if (!pressed && Input.anyKeyDown)
            {
                pressed = true;
                if (Input.GetKeyDown(GetKeyCode(randomKey)))
                {
                    correct = true;
                    speakerQte.PlayOneShot(coinSound);
                }
                break;
            }
            yield return null;
        }

        // If correct => success
        // If never pressed or wrong => fail
        WasSuccess = correct;

        displayBox.text = "";
        passBox.text = correct ? "Great!" : "FAIL!";

        yield return new WaitForSeconds(resultDisplayTime);

        passBox.text = "";
        IsActive = false;
    }

    private IEnumerator MashingQTE(int requiredPresses, float timeWindow)
    {
        // Also pick random key E=1, R=2, T=3
        int randomKey = Random.Range(1, 4);

        int mashCount = 0;
        float timer = timeWindow;

        displayBox.text = $"[{GetKeyChar(randomKey)}] 0/{requiredPresses}";
        passBox.text = "";

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // if user hits the correct key
            if (Input.GetKeyDown(GetKeyCode(randomKey)))
            {
                speakerQte.PlayOneShot(coinSound);
                mashCount++;
                displayBox.text = 
                    $"[{GetKeyChar(randomKey)}] {mashCount}/{requiredPresses}";

                if (mashCount >= requiredPresses)
                {
                    // success
                    break;
                }
            }
            yield return null;
        }

        // success if mashCount >= requiredPresses
        WasSuccess = (mashCount >= requiredPresses);

        displayBox.text = "";
        passBox.text = WasSuccess ? "Great!" : "FAIL!";

        yield return new WaitForSeconds(resultDisplayTime);

        passBox.text = "";
        IsActive = false;
    }

    private KeyCode GetKeyCode(int id)
    {
        switch (id)
        {
            case 1: return KeyCode.Q;
            case 2: return KeyCode.W;
            case 3: return KeyCode.E;
            default: return KeyCode.None;
        }
    }

    private char GetKeyChar(int id)
    {
        switch (id)
        {
            case 1: return 'Q';
            case 2: return 'W';
            case 3: return 'E';
            default: return '?';
        }
    }
}
