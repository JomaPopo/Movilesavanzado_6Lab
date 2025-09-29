using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ReadyButtonUI : MonoBehaviour
{
    public Button readyButton;           // arrastrar el Button en el inspector
    public TMP_Text readyButtonText;         // opcional: texto del botón para mostrar Ready/Unready

    private PlayerLobby localPlayerLobby;
    private Coroutine waitCoroutine;

    private void Start()
    {
        if (readyButton == null)
        {
            Debug.LogError("ReadyButtonUI: asigna el Button en el inspector.");
            return;
        }

        // Deshabilitado hasta que tengamos el Player local
        readyButton.onClick.RemoveAllListeners();
        readyButton.interactable = false;
        UpdateButtonText(false);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        TryBindToLocalPlayer();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        Unbind();
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Si el conectado es el local, intentamos bindear (útil para clientes)
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            TryBindToLocalPlayer();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Si el que se fue era el local player, limpiamos binding
        if (localPlayerLobby != null && localPlayerLobby.NetworkObject != null &&
            localPlayerLobby.NetworkObject.OwnerClientId == clientId)
        {
            Unbind();
        }
    }

    private void TryBindToLocalPlayer()
    {
        if (NetworkManager.Singleton == null) { return; }

        ulong localId = NetworkManager.Singleton.LocalClientId;
        Unity.Netcode.NetworkClient localClient;
        bool exists = NetworkManager.Singleton.ConnectedClients.TryGetValue(localId, out localClient);

        if (!exists || localClient == null || localClient.PlayerObject == null)
        {
            // si todavía no está spawnedeado, esperar hasta que exista
            if (waitCoroutine == null)
            {
                waitCoroutine = StartCoroutine(WaitForLocalPlayer());
            }
            return;
        }

        PlayerLobby player = localClient.PlayerObject.GetComponent<PlayerLobby>();
        Bind(player);
    }

    private IEnumerator WaitForLocalPlayer()
    {
        while (true)
        {
            if (NetworkManager.Singleton == null) { yield break; }

            ulong localId = NetworkManager.Singleton.LocalClientId;
            Unity.Netcode.NetworkClient localClient;
            bool exists = NetworkManager.Singleton.ConnectedClients.TryGetValue(localId, out localClient);

            if (exists && localClient != null && localClient.PlayerObject != null)
            {
                PlayerLobby player = localClient.PlayerObject.GetComponent<PlayerLobby>();
                Bind(player);
                waitCoroutine = null;
                yield break;
            }

            yield return null;
        }
    }

    private void Bind(PlayerLobby player)
    {
        if (player == null) { return; }

        localPlayerLobby = player;

        // Habilitar botón y apuntarlo a ToggleReady()
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(localPlayerLobby.ToggleReady);
        readyButton.interactable = true;

        // Subscribirse al cambio de NetworkVariable para actualizar texto
        localPlayerLobby.isReady.OnValueChanged += OnReadyValueChanged;

        // Estado inicial del botón
        UpdateButtonText(localPlayerLobby.isReady.Value);
    }

    private void Unbind()
    {
        if (localPlayerLobby == null) { return; }

        // Quitar listener del NetworkVariable y del botón
        localPlayerLobby.isReady.OnValueChanged -= OnReadyValueChanged;
        readyButton.onClick.RemoveAllListeners();
        readyButton.interactable = false;

        localPlayerLobby = null;
        UpdateButtonText(false);
    }

    private void OnReadyValueChanged(bool previousValue, bool newValue)
    {
        UpdateButtonText(newValue);
    }

    private void UpdateButtonText(bool isReady)
    {
        if (readyButtonText == null) { return; }
        if (isReady)
        {
            readyButtonText.text = "Unready";
        }
        else
        {
            readyButtonText.text = "Ready";
        }
    }
}
