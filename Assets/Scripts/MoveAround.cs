using UnityEngine;
using DG.Tweening;

public class MoveAround : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites; 
    // [0] = idle, [1] (optional), [2] = dash/clash

    [Header("Clash Settings")]
    [Tooltip("Time to move quickly toward the 80% point.")]
    public float fastApproachTime = 0.5f;

    [Tooltip("Time to slowly move the last 20%.")]
    public float slowApproachTime = 1.2f;

    [Tooltip("Maximum distance to stop before fully overlapping.")]
    public float stopOffset = 0.2f;

    [Tooltip("How long the losing character's bounce-back takes.")]
    public float bounceTime = 0.1f;

    [Tooltip("Maximum bounce distance if you lose.")]
    public float bounceDistance = 3f;
    
    [Tooltip("Duration of break time between clashes.")]
    public float respite = 0.5f;

    private GameObject _target;

    void Start()
    {
        // Assign target based on your tag
        if (CompareTag("Lynne"))
        {
            _target = GameObject.FindGameObjectWithTag("Enemy");
        }
        else if (CompareTag("Enemy"))
        {
            _target = GameObject.FindGameObjectWithTag("Lynne");
        }
        else
        {
            Debug.LogError("This script must be on an object tagged 'Lynne' or 'Enemy'.");
        }

        // Cache SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Default to idle if available
        if (sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    void Update()
    {
        // Press Space to initiate the clash
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_target) return;

            // Switch to dash/clash sprite, if available
            if (sprites.Length > 2)
            {
                spriteRenderer.sprite = sprites[2];
            }

            // Calculate the midpoint minus a small offset
            Vector3 ownPos = transform.position;
            Vector3 tgtPos = _target.transform.position;
            Vector3 midpoint = (ownPos + tgtPos) / 2f;
            
            // Slight offset so they don't overlap exactly
            // A - B * 0.2
            Vector3 directionToMid = (midpoint - ownPos).normalized;
            Vector3 finalPos = midpoint - directionToMid * stopOffset;
            float angleVariance = Random.Range(-0.14f, 0.14f);
            
            //  1) Move 80% of the distance quickly.
            //  2) Move the remaining 20% slowly (2–3 seconds).
            float totalDist = Vector3.Distance(ownPos, finalPos);
            float fastDist = totalDist * 0.8f;
            float slowDist = totalDist * 0.2f;

            Vector3 fastPos = ownPos + directionToMid * fastDist;
            Vector3 slowPos = fastPos + directionToMid * slowDist; 
            // slowPos should be the same as 'finalPos' if you add them exactly.

            // Create a DOTween sequence for chain animations
            Sequence clashSequence = DOTween.Sequence();

            // 1) Fast approach
            clashSequence.Append(
                transform.DOMove(fastPos, fastApproachTime)
            );

            // 2) Slow approach (2-3 seconds)
            clashSequence.Append(
                transform.DOMove(slowPos, slowApproachTime)
                      .SetEase(Ease.OutSine)
            );

            // 3) On complete of approach, do a random outcome
            //TODO Pass Skill Information along for multiple clashes
            clashSequence.AppendCallback(() =>
            {
                // Randomly pick a loser (50-50 chance)
                bool iLose = (Random.value < 0.5f);

                if (iLose)
                {
                    
                    // Bounce back from the midpoint
                    transform.DOMoveX(  (transform.position.x - directionToMid.x * bounceDistance) * (1 + angleVariance), bounceTime);
                    transform.DOMoveZ((transform.position.z - directionToMid.z * bounceDistance) * (1 + angleVariance), bounceTime);
                    spriteRenderer.sprite = sprites[3];
                }
                else
                {
                    transform.DOMoveX(  (transform.position.x - directionToMid.x * (bounceDistance / 5)) * (1 + angleVariance), bounceTime);
                    transform.DOMoveZ((transform.position.z - directionToMid.z * (bounceDistance / 5)) * (1 + angleVariance), bounceTime);
                    spriteRenderer.sprite = sprites[1];
                }
                //TODO sprite results of clash
            });

            // 4) After bounce completes, revert to idle
            clashSequence.AppendInterval(bounceTime + respite);
            clashSequence.AppendCallback(() =>
            {
                if (sprites.Length > 0)
                {
                    spriteRenderer.sprite = sprites[0];
                }
            });
        }
    }
}
