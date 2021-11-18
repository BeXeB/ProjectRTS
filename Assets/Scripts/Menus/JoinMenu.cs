using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class JoinMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPage = null;
    [SerializeField] private TMP_InputField ipInput = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSNetworkManager.ClientOnDisonnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSNetworkManager.ClientOnDisonnected -= HandleClientDisconnected;
    }

    public void Join()
    {
        string ip = ipInput.text;
        NetworkManager.singleton.networkAddress = ip;
        NetworkManager.singleton.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);

        landingPage.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
