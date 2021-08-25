using UnityEngine;
using Mirror;

public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint;

    public Transform GetAimAtPoint()
    {
        return aimAtPoint;
    }
}
