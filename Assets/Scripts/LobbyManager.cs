using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;
    public Transform[] lobbyPositions;

    private void Awake()
    {
        Instance = this;

        if (lobbyPositions == null || lobbyPositions.Length == 0)
        {
            lobbyPositions = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject posObj = new GameObject("LobbyPos_" + (i + 1));
                posObj.transform.position = new Vector3(i * 2.5f, 0, 0);
                lobbyPositions[i] = posObj.transform;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            AssignLobbyPosition(clientId);
        }
    }

    private void AssignLobbyPosition(ulong clientId)
    {
        GameObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        int assignedIndex = GetFreePositionIndex(clientId);

        if (assignedIndex >= 0 && assignedIndex < lobbyPositions.Length)
        {
            playerObject.transform.position = lobbyPositions[assignedIndex].position;
        }
    }

    private int GetFreePositionIndex(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            return 0;
        }

        int totalPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
        if (totalPlayers <= lobbyPositions.Length)
        {
            return totalPlayers - 1; 
        }

        return -1;
    }

    public void CheckIfAllReady()
    {
        int totalPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
        int readyPlayers = 0;

        for (int i = 0; i < totalPlayers; i++)
        {
            PlayerLobby player = NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject.GetComponent<PlayerLobby>();
            if (player != null && player.isReady.Value)
            {
                readyPlayers++;
            }
        }

        if (IsServer && readyPlayers == totalPlayers && totalPlayers > 0)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

    }
}
