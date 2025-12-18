using UnityEngine;


public class ButtonTeleport : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
    public Transform teleportTarget;

    public void Teleport()
    {
        if (teleportationProvider == null || teleportTarget == null)
        {
            Debug.LogError("Teleport fehlt Referenz!");
            return;
        }

        UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest
        {
            destinationPosition = teleportTarget.position,
            destinationRotation = teleportTarget.rotation
        };

        teleportationProvider.QueueTeleportRequest(request);
    }
}
