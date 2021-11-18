using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
public class RTSPlayer : NetworkBehaviour
{

    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private Transform cameraTrasnform = null;
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerChanged))]
    private bool isPartyOwner = false;
    [SyncVar(hook = nameof(ClientHandleNameChanged))]
    private string displayName;
    private Color teamColor = new Color();

    public event Action<int> ClientOnResourcesUpdated;
    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorotyOnPartyOwnerChanged;

    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();


    public string GetDisplayName()
    {
        return displayName;
    }
    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }
    public Transform GetCameraTrasform()
    {
        return cameraTrasnform;
    }
    public Color GetTeamColor()
    {
        return teamColor;
    }
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

    [Server]
    public void SetDisplayName(string newValue)
    {
        displayName = newValue;
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
    [Server]
    public void SetTeamColor(Color color)
    {
        teamColor = color;
    }
    [Server]
    public void SetPartyOwner(bool newValue)
    {
        isPartyOwner = newValue;
    }

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerUnitSpawnedHandler;
        Unit.ServerOnUnitDespawned += ServerUnitDespawnedHandler;
        Building.ServerOnBuildingSpawned += ServerBuildingSpawnedHandler;
        Building.ServerOnBuildingDespawned += ServerBuildingDespawnedHandler;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerUnitSpawnedHandler;
        Unit.ServerOnUnitDespawned -= ServerUnitDespawnedHandler;
        Building.ServerOnBuildingSpawned -= ServerBuildingSpawnedHandler;
        Building.ServerOnBuildingDespawned -= ServerBuildingDespawnedHandler;
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner)
        {
            return;
        }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
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

    public override void OnStartClient()
    {
        if (NetworkServer.active)
        {
            return;
        }
        DontDestroyOnLoad(gameObject);
        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

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
        ClientOnInfoUpdated?.Invoke();

        if (!isClientOnly)
        {
            return;
        }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if (!hasAuthority)
        {
            return;
        }

        Unit.AuthorityOnUnitSpawned -= AuthorityUnitSpawnedHandler;
        Unit.AuthorityOnUnitDespawned -= AuthorityUnitDespawnedHandler;
        Building.AuthorityOnBuildingSpawned -= AuthorityBuildingSpawnedHandler;
        Building.AuthorityOnBuildingDespawned -= AuthorityBuildingDespawnedHandler;
    }

    private void ClientHandleNameChanged(string oldValue, string newValue)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    private void AuthorityHandlePartyOwnerChanged(bool oldValue, bool newValue)
    {
        if (!hasAuthority)
        {
            return;
        }

        AuthorotyOnPartyOwnerChanged?.Invoke(newValue);
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
