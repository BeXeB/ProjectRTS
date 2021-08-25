using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    public event Action<int> ClientOnResourcesUpdated;
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();


    public int GetResources()
    {
        return resources;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }
    [Server]
    public void AddResources(int amount)
    {
        resources += amount;
    }
    [Server]
    public void RemoveResources(int amount)
    {
        resources -= amount;
    }

    public bool CanPlaceBuilding(BoxCollider collider, Vector3 position)
    {
        if (Physics.CheckBox(position + collider.center, collider.size / 2, Quaternion.identity, buildingBlockLayer))
        {
            return false;
        }

        foreach (Building building in myBuildings)
        {
            if ((position - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }
        return false;
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

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 position)
    {
        Building buildingToPlace = null;
        foreach (Building building in buildings)
        {
            if (building.GetBuildingId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null)
        {
            return;
        }

        if (resources < buildingToPlace.GetPrice())
        {
            return;
        }

        BoxCollider collider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(collider, position))
        {
            return;
        }

        GameObject instance = Instantiate(buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(instance, connectionToClient);

        RemoveResources(buildingToPlace.GetPrice());
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

    private void ClientHandleResourcesUpdated(int oldValue, int newValue)
    {
        ClientOnResourcesUpdated?.Invoke(newValue);
    }

    #endregion
}
