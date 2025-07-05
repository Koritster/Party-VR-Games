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
        Quaternion rotation = hubSpot.rotation;

        TeleportRequest teleportRequest = new()
        {
            destinationPosition = destination,
            destinationRotation = rotation
        };

        if (!m_TeleportationProvider.QueueTeleportRequest(teleportRequest))
        {
            Debug.LogWarning("Failed to queue teleport request");
        }
    }

    public void TeleportPlayer(Transform tpArea)
    {
        TeleportationProvider m_TeleportationProvider = GetComponentInChildren<TeleportationProvider>();
        Vector3 destination = tpArea.position;
        Quaternion rotation = tpArea.rotation;

        TeleportRequest teleportRequest = new()
        {
            destinationPosition = destination,
            destinationRotation = rotation
        };

        if (!m_TeleportationProvider.QueueTeleportRequest(teleportRequest))
        {
            Debug.LogWarning("Failed to queue teleport request");
        }
    }
}
