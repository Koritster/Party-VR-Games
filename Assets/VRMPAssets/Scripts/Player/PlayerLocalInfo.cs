using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class PlayerLocalInfo : MonoBehaviour
{
    public bool isConnected;
    public bool isHost;

    public Transform hubSpot;

    public void TeleportPlayer()
    {
        TeleportationProvider m_TeleportationProvider = GetComponentInChildren<TeleportationProvider>();
        Vector3 destination = hubSpot.position;
        Quaternion rotation = Quaternion.Euler(0, 180f, 0); //Quaternion rotation = Quaternion.Euler(0, hubSpot.eulerAngles.y, 0); ;

        Debug.Log($"Teleporting player to {destination} with rotation {rotation.eulerAngles}");

        TeleportRequest teleportRequest = new()
        {
            destinationPosition = destination,
            destinationRotation = rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        if (!m_TeleportationProvider.QueueTeleportRequest(teleportRequest))
        {
            Debug.LogWarning("Failed to queue teleport request");
        }

        Debug.Log(this.transform.rotation.eulerAngles);
    }

    public void TeleportPlayer(Transform tpArea)
    {
        TeleportationProvider m_TeleportationProvider = GetComponentInChildren<TeleportationProvider>();
        Vector3 destination = tpArea.position;
        Quaternion rotation = Quaternion.Euler(0, tpArea.eulerAngles.y, 0); ;
        
        TeleportRequest teleportRequest = new()
        {
            destinationPosition = destination,
            destinationRotation = rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        if (!m_TeleportationProvider.QueueTeleportRequest(teleportRequest))
        {
            Debug.LogWarning("Failed to queue teleport request");
        }
    }
}
