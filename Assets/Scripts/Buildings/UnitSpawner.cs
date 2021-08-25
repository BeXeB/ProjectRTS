using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform unitSpawnPoint;
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxQueue = 5;
    [SerializeField] private float unitSpawnInterval = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private void Update()
    {
        if (isServer)
        {
            ProduceUnits();
        }

        if (isClient)
        {
            UpdateDispaly();
        }
    }

    #region Server

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0)
        {
            return;
        }

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnInterval)
        {
            return;
        }

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);
        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + unitInstance.transform.forward);
        queuedUnits--;
        unitTimer = 0;
    }

    [Command]
    private void CmdSpawnUnit()
    {
        if (queuedUnits == maxQueue)
        {
            return;
        }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.GetResources() < unitPrefab.GetResourceCost())
        {
            return;
        }

        queuedUnits++;

        player.RemoveResources(unitPrefab.GetResourceCost());
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
        NetworkServer.Destroy(gameObject);
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

    private void ClientHandleQueuedUnitsUpdated(int oldValue, int newValue)
    {
        remainingUnitsText.text = newValue.ToString();
    }

    private void UpdateDispaly()
    {
        float newProgress = unitTimer / unitSpawnInterval;

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    #endregion
}
