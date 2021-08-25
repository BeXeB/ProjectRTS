using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] float chaseRange = 5f;

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
        agent.ResetPath();
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        if (target != null)
        {
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }

        if (!agent.hasPath)
        {
            return;
        }
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            return;
        }
        agent.ResetPath();
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            return;
        }

        agent.SetDestination(hit.position);
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    #endregion
}
