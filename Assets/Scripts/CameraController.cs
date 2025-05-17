using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] public CinemachineCamera[] cameras;
    [SerializeField] private float defaultOffset = -8.5f;   // camelCase field
    [SerializeField] private float defaultLensFOV = 60f; // For fallback cams without group framing

    public float DefaultOffset => defaultOffset; 

    [Header("Impulse Source (required for shake)")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private float shaketime;
    private int shakingCamIndex = -1;
    private Dictionary<int, Tween> activePulseTweens = new();
    private Dictionary<int, Vector2> defaultFovRanges = new();



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
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].TryGetComponent(out CinemachineGroupFraming framing))
            {
                defaultFovRanges[i] = framing.FovRange;
            }
        }
    }
    
    void Update()
    {
        if (shaketime > 0f && shakingCamIndex != -1)
        {
            var perlin = cameras[shakingCamIndex].GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlin != null)
            {
                DOTween.To(() => perlin.AmplitudeGain, x => perlin.AmplitudeGain = x, 0f, shaketime).SetEase(Ease.OutExpo);

            }

            shakingCamIndex = -1;
        
        }
    }

    private bool IsValid(int index)
    {
        return index >= 0 && index < cameras.Length && cameras[index] != null;
    }
    public Tween ZoomZ(int camIndex, float targetMinFOV, float targetFOV, float duration, Ease ease)
    {
        if (!IsValid(camIndex)) return null;

        var cineCam = cameras[camIndex];

        // Case 1: Group framing cam — animate FOV range min
        if (cineCam.TryGetComponent(out CinemachineGroupFraming groupFraming))
        {
            return DOTween.To(
                () => groupFraming.FovRange.x,
                v => groupFraming.FovRange = new Vector2(v, groupFraming.FovRange.y),
                targetMinFOV,
                duration
            ).SetEase(ease);
        }

        // Case 2: Single-target focused cam — animate Lens.Horizontal FOV
        float currentFOV = cineCam.Lens.FieldOfView;

        return DOTween.To(
            () => cineCam.Lens.FieldOfView,
            fov => cineCam.Lens.FieldOfView = fov,
            targetFOV,
            duration
        ).SetEase(ease);
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

    public Tween PulseCamera(int camIndex, float pulseIntensity, float pulseInTime = 0.3f, float pulseOutTime = 0.5f)
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
    
    public Tween ResetZoom(int camIndex, float duration, Ease ease)
    {
        if (!IsValid(camIndex)) return null;

        var cineCam = cameras[camIndex];

        // Case 1: Group framing cam → restore FOV range min
        if (cineCam.TryGetComponent(out CinemachineGroupFraming groupFraming))
        {
            if (defaultFovRanges.TryGetValue(camIndex, out Vector2 originalRange))
            {
                return DOTween.To(
                    () => groupFraming.FovRange.x,
                    v => groupFraming.FovRange = new Vector2(v, groupFraming.FovRange.y),
                    originalRange.x,
                    duration
                ).SetEase(ease);
            }
        }

        // Case 2: Single-target cam → restore default lens FOV
        return DOTween.To(
            () => cineCam.Lens.FieldOfView,
            fov => cineCam.Lens.FieldOfView = fov,
            defaultLensFOV,
            duration
        ).SetEase(ease);
    }
}