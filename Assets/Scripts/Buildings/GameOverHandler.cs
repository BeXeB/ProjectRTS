using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;

    #region Server

    private List<UnitBase> bases = new List<UnitBase>();

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += HandleServerOnBaseSpawned;
        UnitBase.ServerOnBaseDespawned += HandleServerOnBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= HandleServerOnBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= HandleServerOnBaseDespawned;
    }

    [Server]
    private void HandleServerOnBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void HandleServerOnBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);
        if (bases.Count != 1)
        {
            return;
        }

        int winnerId = bases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {winnerId}");

        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
