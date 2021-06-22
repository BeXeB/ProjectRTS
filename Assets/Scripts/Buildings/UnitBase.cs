using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    public static event Action<int> SeverOnPlayerDie;
    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;


    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += HandleServerOnDie;
        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        health.ServerOnDie += HandleServerOnDie;
        ServerOnBaseDespawned?.Invoke(this);
    }

    [Server]
    private void HandleServerOnDie()
    {

        SeverOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client



    #endregion
}
