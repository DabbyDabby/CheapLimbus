using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.VFX;

public class MoveAround : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    // [0] = idle, [1] = clash-win, [2] = dash, [3] = clash-loss

    [Header("Clash Settings")] public float fastApproachTime = 0.5f;
    public float slowApproachTime = 1.2f;
    public float stopOffset = 0.2f;
    public float bounceTime = 0.1f;
    public float bounceDistance = 3f;
    public float respite = 0.5f;

    [Header("VFX Settings")] public ParticleSystem[] vfx; // Assign this in the Inspector
    public float vfxDuration = 0.5f; // Time before disabling the VFX
    private Vector3 _vfxPosition;

    private GameObject _target;
    private Vector3 _midpoint;

    public async UniTask DashToClashPoint()
    {
        if (!_target) return;

        Vector3 ownPos = transform.position;
        Vector3 targetPos = _target.transform.position;
        _midpoint = (ownPos + targetPos) / 2f;

        Vector3 directionToMid = (_midpoint - ownPos).normalized;
        Vector3 finalPos = _midpoint - directionToMid * stopOffset;

        float totalDist = Vector3.Distance(ownPos, finalPos);
        float fastDist = totalDist * 0.7f;
        float slowDist = totalDist * 0.3f;

        Vector3 fastPos = ownPos + directionToMid * fastDist;
        Vector3 slowPos = fastPos + directionToMid * slowDist;

        // Switch to dash sprite
        spriteRenderer.sprite = sprites[2];

        // Move 80% of the way quickly, then 20% slowly
        await transform.DOMove(fastPos, fastApproachTime).SetEase(Ease.OutQuad).ToUniTask();
        await transform.DOMove(slowPos, slowApproachTime).SetEase(Ease.InOutSine).ToUniTask();

        // Trigger Clash Effect
        await ResolveClash(directionToMid);
    }

    async UniTask ResolveClash(Vector3 directionToMid)
    {
        float angleVariance = Random.Range(-0.14f, 0.14f);
        bool iLose = (Random.Range(0f, 1f) < 0.5f);

        if (iLose)
        {
            // Clash loss (knockback)
            transform.DOMoveX((transform.position.x - directionToMid.x * bounceDistance) * (1 + angleVariance),
                bounceTime).SetEase(Ease.OutSine);
            transform.DOMoveZ((transform.position.z - directionToMid.z * bounceDistance) * (1 + angleVariance),
                bounceTime).SetEase(Ease.OutSine);
            spriteRenderer.sprite = sprites[3]; // Loss sprite
        }
        else
        {
            // Clash win
            spriteRenderer.sprite = sprites[1]; // Winning sprite
            EmitVFX(_vfxPosition);
        }

        await UniTask.WaitForSeconds(bounceTime + respite);

        // Reset to idle sprite
        if (sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    async UniTask EmitVFX(Vector3 vfxPos)
    {
        // Play Clash VFX at the current position
        if (vfx != null)
        {
            vfx[0].transform.localPosition = vfxPos;
            vfx[1].transform.localPosition = new Vector3(_target.transform.localPosition.x, 1.7f, _target.transform.localPosition.z);
            vfx[0].gameObject.SetActive(true);
            vfx[1].gameObject.SetActive(true);
            vfx[0].Play();
            vfx[1].Play();
        }
            
        await UniTask.WaitForSeconds(vfxDuration);
        
        // Stop VFX after duration
        if (vfx != null)
        {
            vfx[0].Stop();
            vfx[1].Stop();
            vfx[0].gameObject.SetActive(false);
            vfx[1].gameObject.SetActive(false);
        }
    }

void Start()
    {
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

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0]; // Default to idle
        }

        // Ensure VFX is disabled at the start
        if (vfx != null)
        {
            foreach (var effects in vfx)
            {
                effects.gameObject.SetActive(false);
            }
            _vfxPosition = vfx[0].transform.localPosition;
        }
    }

    async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
           DashToClashPoint().Forget();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            EmitVFX(_vfxPosition);
        }
    }
}
