using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public Targetable GetTarget()
    {
        return target;
    }

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += HandleServerOnGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= HandleServerOnGameOver;
    }

    [Server]
    private void HandleServerOnGameOver()
    {
        ClearTarget();
    }

    [Command]
    public void CmdSetTarget(GameObject targetGo)
    {
        if (!targetGo.TryGetComponent<Targetable>(out Targetable target))
        {
            return;
        }
        this.target = target;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    #endregion
}
