using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform unitSpawnPoint;


    #region Server

    [Command]
    private void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(unitPrefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }

    public override void OnStartServer()
    {
        health.ServerOnDie += HandleServerOnDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= HandleServerOnDie;
    }

    [Server]
    private void HandleServerOnDie()
    {
        //NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasAuthority)
        {
            return;
        }
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        CmdSpawnUnit();
    }

    #endregion
}
