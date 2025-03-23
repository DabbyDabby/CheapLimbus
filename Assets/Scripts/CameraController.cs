using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera[] cameras;

    /// <summary>
    /// Simple method to zoom a specified camera to a new Z position over duration.
    /// </summary>
    /// <param name="camIndex">Index in the cameras array.</param>
    /// <param name="newZ">The final Z position.</param>
    /// <param name="duration">Tween duration.</param>
    /// <param name="ease">DOTween ease function.</param>
    public void ZoomZ(int camIndex, float newZ, float duration, Ease ease)
    {
        if (camIndex < 0 || camIndex >= cameras.Length) return;
        cameras[camIndex].transform.DOMoveZ(newZ, duration).SetEase(ease);
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
    public void ShakeCamera(int camIndex, float time, Vector3 strength, int vibrato, float randomness, bool fadeOut = true)
    {
        if (camIndex < 0 || camIndex >= cameras.Length) return;
        cameras[camIndex].transform.DOShakePosition(time, strength, vibrato, randomness, false, fadeOut);
    }

    /// <summary>
    /// Move the specified camera back to an "idle" Z position (or any default position).
    /// </summary>
    public void ResetZoom(int camIndex, float defaultZ, float duration)
    {
        if (camIndex < 0 || camIndex >= cameras.Length) return;
        cameras[camIndex].transform.DOMoveZ(defaultZ, duration);
    }

    /// <summary>
    /// Focus camera on a specific transform over time. 
    /// For example, could reposition the camera or adjust Cinemachine's Follow/LookAt.
    /// </summary>
    public void FocusOnTarget(int camIndex, Transform target, float duration)
    {
        if (camIndex < 0 || camIndex >= cameras.Length) return;
        // Example: direct reposition - or you might do something with CinemachineVirtualCamera's Follow
        Vector3 newPos = new Vector3(target.position.x, target.position.y, cameras[camIndex].transform.position.z);
        cameras[camIndex].transform.DOMove(newPos, duration).SetEase(Ease.InOutSine);
    }
}
