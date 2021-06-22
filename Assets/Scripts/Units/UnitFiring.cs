using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpwanPoint = null;
    [SerializeField] private float fireRange = 5.2f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 150f;
    private float lastFireTime;

    #region Server

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        if (target == null)
        {
            return;
        }
        if (!CanFireAtTarget())
        {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpwanPoint.position);
            GameObject projectile = Instantiate(projectilePrefab, projectileSpwanPoint.position, projectileRotation);
            NetworkServer.Spawn(projectile, connectionToClient);
            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }

    #endregion
}
