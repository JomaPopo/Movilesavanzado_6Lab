using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]


public class PlayerLobby : NetworkBehaviour
{
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<int> skinIndex = new NetworkVariable<int>(
      0,
      NetworkVariableReadPermission.Everyone,
      NetworkVariableWritePermission.Server
  );
    private PlayerSkin playerSkin;
    private int totalSkins;
    public override void OnNetworkSpawn()
    {
        playerSkin = GetComponent<PlayerSkin>();
        if (playerSkin != null)
        {
            // Usamos la cantidad de cabezas para saber cuántos skins hay
            totalSkins = playerSkin.heads.Length;

            skinIndex.OnValueChanged += OnSkinChanged;
            // Aplicar la skin inicial
            playerSkin.ChangeSkin(skinIndex.Value);
        }
    }
    public override void OnNetworkDespawn()
    {
        if (playerSkin != null)
        {
            skinIndex.OnValueChanged -= OnSkinChanged;
        }
    }
    private void OnSkinChanged(int previousValue, int newValue)
    {
        if (playerSkin != null)
        {
            playerSkin.ChangeSkin(newValue);
        }
    }

    public void ToggleReady()
    {
        if (IsOwner)
        {
            SetReadyServerRpc(!isReady.Value);
        }
    }

    public void ChangeSkin()
    {
        if (IsOwner && totalSkins > 0)
        {
            int newSkinIndex = (skinIndex.Value + 1) % totalSkins;
            ChangeSkinServerRpc(newSkinIndex);
        }
    }
    public void NextSkin()
    {
        if (IsOwner && totalSkins > 0)
        {
            int newSkinIndex = (skinIndex.Value + 1) % totalSkins;
            ChangeSkinServerRpc(newSkinIndex);
        }
    }

    public void PreviousSkin()
    {
        if (IsOwner && totalSkins > 0)
        {
            int newSkinIndex = skinIndex.Value - 1;
            if (newSkinIndex < 0)
            {
                newSkinIndex = totalSkins - 1; // Volver al último si estamos en el primero
            }
            ChangeSkinServerRpc(newSkinIndex);
        }
    }

    [ServerRpc]
    private void SetReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
    {
        isReady.Value = ready;
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.CheckIfAllReady();
        }
    }

    [ServerRpc]
    private void ChangeSkinServerRpc(int newSkinIndex, ServerRpcParams rpcParams = default)
    {
        skinIndex.Value = newSkinIndex;
    }
}
