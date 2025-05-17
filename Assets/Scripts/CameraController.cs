using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEditor;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] public CinemachineCamera[] cameras;
    [SerializeField] private float defaultOffset = -8.5f;   // camelCase field
    public float DefaultOffset => defaultOffset; 

    [Header("Impulse Source (required for shake)")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private float shaketime;
    private int shakingCamIndex = -1;
    private Dictionary<int, Tween> activePulseTweens = new();


    /// <summary>
    /// Simple method to zoom a specified camera to a new Z position over duration.
    /// </summary>
    /// <param name="camIndex">Index in the cameras array.</param>
    /// <param name="newZ">The final Z position.</param>
    /// <param name="duration">Tween duration.</param>
    /// <param name="ease">DOTween ease function.</param>
    ///
    private void Start()
    {
        BlendTo(0);
    }
    
    void Update()
    {
        shakingCamIndex = shakingCamIndex;
        if (shaketime > 0f && shakingCamIndex != -1)
        {
            var perlin = cameras[shakingCamIndex].GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlin != null)
            {
                DOTween.To(() => perlin.AmplitudeGain, x => perlin.AmplitudeGain = x, 0f, shaketime).SetEase(Ease.OutExpo);

            }

            shakingCamIndex = -1;
        
        }

        // Debug test
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Testing shake...");
            ShakeCamera(0, 15f, 0.5f);
        }
    }

    private bool IsValid(int index)
    {
        return index >= 0 && index < cameras.Length && cameras[index] != null;
    }
    public Tween ZoomZ(int camIndex, float targetZoom, float duration, Ease ease)
    {
        if (!IsValid(camIndex)) return null;

        var camObj = cameras[camIndex];
        var vcam = camObj.GetComponent<CinemachineVirtualCamera>();

        if (vcam != null)
        {
            float start = vcam.m_Lens.OrthographicSize;
            return DOTween.To(() => start, x => vcam.m_Lens.OrthographicSize = x, targetZoom, duration).SetEase(ease);
        }

        return camObj.transform.DOMoveZ(targetZoom, duration).SetEase(ease);
    }
    
    // Switches priority so that camId > others (simple blend)
    public void BlendTo(int camId, int high = 20, int low = 0)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            var vcam = cameras[i];
            vcam.gameObject.SetActive(true);
            vcam.Priority = (i == camId) ? high : low;
        }
        Debug.Log($"BlendTo({camId})  →  Wide:{cameras[0].Priority}  Player:{cameras[1].Priority}  Enemy:{cameras[2].Priority}");
    }

    public Tween PulseCamera(int camIndex, float pulseIntensity, float pulseInTime = 0.15f, float pulseOutTime = 0.3f)
    {
        if (!IsValid(camIndex)) return null;

        var cineCam = cameras[camIndex];
        var originalFOV = cineCam.Lens.FieldOfView;
        var targetFOV = originalFOV * (1 - pulseIntensity);

        // Pulse in, then out
        return DOTween.Sequence()
            .Append(DOTween.To(
                () => cineCam.Lens.FieldOfView,
                v => cineCam.Lens.FieldOfView = v,
                targetFOV,
                pulseInTime
            ))
            .Append(DOTween.To(
                () => cineCam.Lens.FieldOfView,
                v => cineCam.Lens.FieldOfView = v,
                originalFOV,
                pulseOutTime
            ));
    }


    
    // Sets Follow + LookAt on the chosen vcam
    public void Track(int camId, Transform target)
    {
        if (camId < 0 || camId >= cameras.Length) return;
        var vcam = cameras[camId];
        vcam.Follow = target;
        vcam.LookAt = target;
    }
    
    
    public void ShakeCamera(int camIndex, float strength = 2.5f, float duration = 0.5f)
    {
        if (!IsValid(camIndex)) return;

        var perlin = cameras[camIndex].GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin != null)
        {
            shaketime = duration;
            shakingCamIndex = camIndex;
            perlin.AmplitudeGain = strength;
        }
    }

    /// <summary>
    /// Shakes the given camera for camera shake effects.
    /// </summary>
    /// <param name="camIndex">Index in the cameras array.</param>
    /// <param name="time">How long the shake lasts.</param>
    /// <param name="strength">Shake strength on each axis.</param>
    /// <param name="vibrato">How many shakes per second.</param>
    /// <param name="randomness">Randomness factor in degrees.</param>
    /// <param name="fadeOut">If true, shake fades out over time.</param>
    /// <summary>
    /// Move the specified camera back to an "idle" Z position (or any default position).
    /// </summary>
    
    public Tween ResetZoom(int camIndex, float duration, Ease ease)
    {
        if (camIndex < 0 || camIndex >= cameras.Length) return null;
        Tween resetCamZTween = cameras[camIndex].transform.DOMoveZ(-8.408978f, duration).SetEase(ease);
        return resetCamZTween;
    }

    /// <summary>
    /// Focus camera on a specific transform over time. 
    /// For example, could reposition the camera or adjust Cinemachine's Follow/LookAt.
    /// </summary>
    public Tween FocusOnTarget(int camIndex, Transform target, float duration)
    {
        if (camIndex < 0 || camIndex >= cameras.Length) return null;
        // Example: direct reposition - or you might do something with CinemachineVirtualCamera's Follow
        Vector3 newPos = new Vector3(target.position.x, target.position.y, cameras[camIndex].transform.position.z);
        Tween focusOnTargetCamTween = cameras[camIndex].transform.DOMove(newPos, duration).SetEase(Ease.InOutSine);
        return focusOnTargetCamTween;
    }

    

}