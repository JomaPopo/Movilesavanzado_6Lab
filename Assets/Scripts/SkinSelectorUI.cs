using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class SkinSelectorUI : MonoBehaviour
{
    // Arrastra tus botones de la UI aquí en el Inspector
    public Button nextSkinButton;
    public Button previousSkinButton;

    private PlayerLobby localPlayerLobby;
    private Coroutine waitCoroutine;

    void Start()
    {
        // Desactivar botones hasta que encontremos al jugador local
        if (nextSkinButton != null) nextSkinButton.interactable = false;
        if (previousSkinButton != null) previousSkinButton.interactable = false;

        // Empezar a buscar al jugador
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        TryBindToLocalPlayer();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        Unbind();
    }

    private void OnClientConnected(ulong clientId)
    {
        // Si el que se conecta es este cliente, intentamos enlazar
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            TryBindToLocalPlayer();
        }
    }

    private void TryBindToLocalPlayer()
    {
        // Usamos una corutina para esperar a que el objeto del jugador sea creado
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        waitCoroutine = StartCoroutine(WaitForLocalPlayer());
    }

    private IEnumerator WaitForLocalPlayer()
    {
        // Esperar a que el NetworkManager y el cliente local estén listos
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient || NetworkManager.Singleton.LocalClient == null)
        {
            yield return null;
        }

        // Esperar hasta que el objeto del jugador del cliente local aparezca
        while (NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        PlayerLobby player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerLobby>();
        if (player != null)
        {
            Bind(player);
        }

        waitCoroutine = null;
    }

    private void Bind(PlayerLobby player)
    {
        if (player == null) return;
        localPlayerLobby = player;

        // Conectar el botón "Siguiente"
        if (nextSkinButton != null)
        {
            nextSkinButton.onClick.RemoveAllListeners();
            nextSkinButton.onClick.AddListener(localPlayerLobby.NextSkin);
            nextSkinButton.interactable = true;
        }

        // Conectar el botón "Anterior"
        if (previousSkinButton != null)
        {
            previousSkinButton.onClick.RemoveAllListeners();
            previousSkinButton.onClick.AddListener(localPlayerLobby.PreviousSkin);
            previousSkinButton.interactable = true;
        }
    }

    private void Unbind()
    {
        if (localPlayerLobby == null) return;

        if (nextSkinButton != null)
        {
            nextSkinButton.onClick.RemoveAllListeners();
            nextSkinButton.interactable = false;
        }
        if (previousSkinButton != null)
        {
            previousSkinButton.onClick.RemoveAllListeners();
            previousSkinButton.interactable = false;
        }

        localPlayerLobby = null;
    }
}