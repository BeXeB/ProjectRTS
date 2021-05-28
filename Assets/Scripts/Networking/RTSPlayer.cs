using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerUnitSpawnedHandler;
        Unit.ServerOnUnitDespawned += ServerUnitDespawnedHandler;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerUnitSpawnedHandler;
        Unit.ServerOnUnitDespawned -= ServerUnitDespawnedHandler;
    }

    private void ServerUnitSpawnedHandler(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myUnits.Add(unit);
    }

    private void ServerUnitDespawnedHandler(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myUnits.Remove(unit);
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (!isClientOnly)
        {
            return;
        }
        Unit.AuthorityOnUnitSpawned += AuthorityUnitSpawnedHandler;
        Unit.AuthorityOnUnitDespawned += AuthorityUnitDespawnedHandler;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly)
        {
            return;
        }
        Unit.AuthorityOnUnitSpawned -= AuthorityUnitSpawnedHandler;
        Unit.AuthorityOnUnitDespawned -= AuthorityUnitDespawnedHandler;
    }

    private void AuthorityUnitSpawnedHandler(Unit unit)
    {
        if (!hasAuthority)
        {
            return;
        }
        myUnits.Add(unit);
    }

    private void AuthorityUnitDespawnedHandler(Unit unit)
    {
        if (!hasAuthority)
        {
            return;
        }
        myUnits.Remove(unit);
    }

    #endregion
}
