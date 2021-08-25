using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourceText = null;

    private RTSPlayer player;
    private void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

            if (player != null)
            {
                ClientHandleOnResourcesUpdated(player.GetResources());
                player.ClientOnResourcesUpdated += ClientHandleOnResourcesUpdated;
            }
        }
    }

    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleOnResourcesUpdated;
    }

    private void ClientHandleOnResourcesUpdated(int resources)
    {
        resourceText.text = $"Resources: {resources}";
    }
}
