using System;
using UnityEngine;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxhealth = 100;
    [SyncVar(hook = nameof(HandleHealthChanged))] private int currentHealth;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthChange;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxhealth;

        UnitBase.SeverOnPlayerDie += HandleServerGameOver;
    }

    public override void OnStopServer()
    {
        UnitBase.SeverOnPlayerDie -= HandleServerGameOver;
    }

    [Server]
    private void HandleServerGameOver(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId)
        {
            return;
        }

        DealDamage(currentHealth);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0)
        {
            return;
        }

        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    private void HandleHealthChanged(int oldHealth, int newHealth)
    {
        ClientOnHealthChange?.Invoke(newHealth, maxhealth);
    }

    #endregion
}
