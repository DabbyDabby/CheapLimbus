using UnityEngine;

public class ClashManager : MonoBehaviour
{
    public QteManager qteManager;
    public MoveAround characterA;
    public MoveAround characterB;
    public CameraController cameraController;

    // public async UniTask StartClash()
    // {
    //     // Move both characters into clash
    //     await UniTask.WhenAll(characterA.PerformDash(), characterB.PerformDash());
    //
    //     // Switch to clash camera
    //     cameraController.SwitchToClashCam();
    //
    //     // Perform QTE
    //     bool didWin = await qteManager.StartQTE();
    //
    //     // Resolve clash movement (knockback for loser)
    //     await UniTask.WhenAll(
    //         characterA.ClashReaction(didWin),
    //         characterB.ClashReaction(!didWin)
    //     );
    //
    //     // If won, trigger attack sequence
    //     if (didWin)
    //     {
    //         await MoveAround.PerformAttack(characterA);
    //     }
    //
    //     // Shake camera on clash impact
    //     cameraController.ApplyShake();
    //
    //     // Return to default camera after delay
    //     await UniTask.Delay(1000);
    //     cameraController.SwitchToDefaultCam();
    // }
}