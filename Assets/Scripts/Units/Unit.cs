using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Unit : NetworkBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
        health.ServerOnDie += HandleServerOnDie;
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
        health.ServerOnDie -= HandleServerOnDie;
    }

    [Server]
    private void HandleServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    [Client]
    public void Select()
    {
        if (!hasAuthority)
        {
            return;
        }
        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority)
        {
            return;
        }
        onDeselected?.Invoke();
    }

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!hasAuthority)
        {
            return;
        }
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    #endregion
}
