using UnityEngine;
using TMPro;
using Mirror;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerNameText = null;
    [SerializeField] GameObject gameOverDispalyParent = null;
    private void Start()
    {
        GameOverHandler.ClientOnGameOver += HandleClientOnGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= HandleClientOnGameOver;
    }

    private void HandleClientOnGameOver(string winner)
    {
        winnerNameText.text = $"{winner} Has Won!";
        gameOverDispalyParent.SetActive(true);
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
