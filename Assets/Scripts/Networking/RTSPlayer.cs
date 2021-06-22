using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class RTSPlayer : NetworkBehaviour
{
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerUnitSpawnedHandler;
        Unit.ServerOnUnitDespawned += ServerUnitDespawnedHandler;
        Building.ServerOnBuildingSpawned += ServerBuildingSpawnedHandler;
        Building.ServerOnBuildingDespawned += ServerBuildingDespawnedHandler;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerUnitSpawnedHandler;
        Unit.ServerOnUnitDespawned -= ServerUnitDespawnedHandler;
        Building.ServerOnBuildingSpawned -= ServerBuildingSpawnedHandler;
        Building.ServerOnBuildingDespawned -= ServerBuildingDespawnedHandler;
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

    private void ServerBuildingSpawnedHandler(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myBuildings.Add(building);
    }

    private void ServerBuildingDespawnedHandler(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
        {
            return;
        }

        myBuildings.Remove(building);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active)
        {
            return;
        }
        Unit.AuthorityOnUnitSpawned += AuthorityUnitSpawnedHandler;
        Unit.AuthorityOnUnitDespawned += AuthorityUnitDespawnedHandler;
        Building.AuthorityOnBuildingSpawned += AuthorityBuildingSpawnedHandler;
        Building.AuthorityOnBuildingDespawned += AuthorityBuildingDespawnedHandler;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority)
        {
            return;
        }
        Unit.AuthorityOnUnitSpawned -= AuthorityUnitSpawnedHandler;
        Unit.AuthorityOnUnitDespawned -= AuthorityUnitDespawnedHandler;
        Building.AuthorityOnBuildingSpawned -= AuthorityBuildingSpawnedHandler;
        Building.AuthorityOnBuildingDespawned -= AuthorityBuildingDespawnedHandler;
    }

    private void AuthorityUnitSpawnedHandler(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityUnitDespawnedHandler(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void AuthorityBuildingSpawnedHandler(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityBuildingDespawnedHandler(Building building)
    {
        myBuildings.Remove(building);
    }

    #endregion
}
