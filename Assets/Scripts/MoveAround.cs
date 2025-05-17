using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MoveAround : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites; // [0] = idle, [1] = clash-win, [2] = dash, [3] = clash-loss

    [Header("Camera Manager (New)")]
    [SerializeField] private CameraController cameraMgr; // ADDED: Reference to your new camera manager

    [SerializeField] private int camIndex; // which camera in the cameras array to manipulate

    [Header("Clash Settings")]
    public static float fastApproachRatio = 0.7f;
    public static float slowApproachRatio = 0.3f;
    public float stopOffset = 0.2f;
    public float bounceTime = 0.1f;
    public float bounceDistance = 3f;
    public float respite = 0.5f;

    [Header("VFX")]
    public ParticleSystem[] vfx;
    public float vfxDuration = 0.5f;

    private GameObject _target;
    public Vector3 vfxSlashOriginPosition;
    private float _vfxTempX;
    private float _vfxTempY;

    private void Awake()
    {
        // Identify the target
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
            Debug.LogError("MoveAround must be on object tagged 'Lynne' or 'Enemy'.");
        }

        // Default sprite
        if (sprites.Length > 0)
            spriteRenderer.sprite = sprites[0];

        // VFX
        if (vfx != null && vfx.Length > 0)
        {
            foreach (var fx in vfx)
                fx.gameObject.SetActive(false);

            vfxSlashOriginPosition = vfx[0].transform.localPosition;
        }
    }

    public void ResetSprite()
    {
        spriteRenderer.sprite = sprites[0];
        vfx[0].transform.localPosition = vfxSlashOriginPosition;
    }
    
    public IEnumerator DashToClashPoint(float dashDuration)
    {
        if (!_target) yield break;

        // Switch to dash sprite
        if (sprites.Length > 2)
            spriteRenderer.sprite = sprites[2];

        Vector3 ownPos = transform.position;
        Vector3 targetPos = _target.transform.position;
        Vector3 midpoint = (ownPos + targetPos) / 2f;
        Vector3 directionToMid = (midpoint - ownPos).normalized;

        Vector3 finalPos = midpoint - directionToMid * stopOffset;
        float totalDist = Vector3.Distance(ownPos, finalPos);

        float fastTime = dashDuration * fastApproachRatio; 
        float slowTime = dashDuration * slowApproachRatio;

        float fastDist = totalDist * fastApproachRatio;
        float slowDist = totalDist * slowApproachRatio;

        Vector3 fastPos = ownPos + directionToMid * fastDist;
        Vector3 slowPos = fastPos + directionToMid * slowDist;

        // --- USE CAMERA MANAGER INSTEAD OF cam[0] ---
        // Phase 1: fast approach
        
        Tween fastApproachTween = transform.DOMove(fastPos, fastTime).SetEase(Ease.OutQuad);
        yield return fastApproachTween.WaitForCompletion(); // wait for phase 1
        
        Tween slowApproachTween = transform.DOMove(slowPos, slowTime).SetEase(Ease.InOutSine);
        yield return slowApproachTween.WaitForCompletion(); // wait for phase 2
        
        _vfxTempX = midpoint.x;
        _vfxTempY = 1.73f; // 1.73

        // Done dashing
    }
    
    public IEnumerator DashToTarget(float dashDuration, float offset)
    {
        if (!_target) yield break;

        // Set dash sprite if available
        if (sprites.Length > 2)
            spriteRenderer.sprite = sprites[2];

        Vector3 ownPos = transform.position;
        Vector3 targetPos = _target.transform.position;

        // Determine the direction the attacker is facing (left or right)
        float direction = Mathf.Sign(targetPos.x - ownPos.x); // +1 if right of attacker, -1 if left

        // Compute target-relative final position with signed offset
        Vector3 finalPos = new Vector3(targetPos.x + offset * direction, ownPos.y, ownPos.z);

        Vector3 directionToFinal = (finalPos - ownPos).normalized;
        float totalDist = Vector3.Distance(ownPos, finalPos);

        float fastTime = dashDuration * fastApproachRatio; 
        float slowTime = dashDuration * slowApproachRatio;

        float fastDist = totalDist * fastApproachRatio;
        float slowDist = totalDist * slowApproachRatio;

        Vector3 fastPos = ownPos + directionToFinal * fastDist;
        Vector3 slowPos = fastPos + directionToFinal * slowDist;

        // Phase 1: fast approach
        Tween fastTween = transform.DOMove(fastPos, fastTime).SetEase(Ease.OutQuad);
        yield return fastTween.WaitForCompletion();

        // Phase 2: slow approach
        Tween slowTween = transform.DOMove(slowPos, slowTime).SetEase(Ease.InOutSine);
        yield return slowTween.WaitForCompletion();

        // Update VFX position reference if used
        _vfxTempX = finalPos.x;
        _vfxTempY = transform.position.y;
    }
    


    
    public IEnumerator BounceOnKick()
    {
        Vector3 startPos = transform.position;
        Vector3 bouncePos = startPos + new Vector3(0, 1.5f, 0);

        yield return transform.DOMove(bouncePos, 0.25f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return transform.DOMove(startPos, 0.2f).SetEase(Ease.InQuad).WaitForCompletion();
    }
    
    public void WinClash()
    {
        // We now call camera shake via cameraMgr
        if (cameraMgr != null)
        {
            cameraMgr.ShakeCamera(0, 10f, 0.5f);
            cameraMgr.PulseCamera(0, 0.03f);
        }
        // Switch sprite
        if (sprites.Length > 1)
            spriteRenderer.sprite = sprites[1];

        EmitVFX(_vfxTempX, _vfxTempY);
    }

    public void LoseClash()
    {
        Vector3 ownPos = transform.position;
        if (!_target) return;

        Vector3 targetPos = _target.transform.position;
        Vector3 midpoint = (ownPos + targetPos) / 2f;
        Vector3 directionToMid = (midpoint - ownPos).normalized;

        float distVar = Random.Range(-0.14f, 0.14f);
        float xNew = ownPos.x - directionToMid.x * bounceDistance * (1 + distVar);
        float zNew = ownPos.z - directionToMid.z * bounceDistance * (1 + distVar);

        transform.DOMoveX(xNew, bounceTime).SetEase(Ease.OutSine);
        transform.DOMoveZ(zNew, bounceTime).SetEase(Ease.OutSine);

        if (sprites.Length > 3)
            spriteRenderer.sprite = sprites[3];

        // DOVirtual.DelayedCall(bounceTime + respite, () =>
        // {
        // if (cameraMgr != null)
        // cameraMgr.ResetZoom(camIndex, -8.5f, 0.2f);
        // });
    }

    private void EmitVFX(float vfxX, float vfxY)
    {
        if (vfx == null || vfx.Length < 2) return;
        
        vfx[0].transform.localPosition = vfxSlashOriginPosition;
        vfx[1].transform.localPosition = new Vector3(vfxX, vfxY, 0);

        vfx[0].gameObject.SetActive(true);
        vfx[1].gameObject.SetActive(true);

        vfx[0].Play();
        vfx[1].Play();

        DOVirtual.DelayedCall(vfxDuration, () =>
        {
            vfx[0].Stop();
            vfx[1].Stop();
            vfx[0].gameObject.SetActive(false);
            vfx[1].gameObject.SetActive(false);
        });
    }
}
