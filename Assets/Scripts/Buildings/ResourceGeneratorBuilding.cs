using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceGeneratorBuilding : NetworkBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private int resourcesPerInterval = 10;
    [SerializeField] private float interval = 2f;

    private float timer;
    private RTSPlayer player;

    [ServerCallback]
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            player.AddResources(resourcesPerInterval);
            timer += interval;
        }
    }

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        health.ServerOnDie += HandleServerOnDie;
        GameOverHandler.ServerOnGameOver += HandleServerOnGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= HandleServerOnDie;
        GameOverHandler.ServerOnGameOver -= HandleServerOnGameOver;
    }

    private void HandleServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void HandleServerOnGameOver()
    {
        enabled = false;
    }
}
