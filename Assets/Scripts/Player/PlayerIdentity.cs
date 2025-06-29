using System;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerIdentity : NetworkBehaviour
    {
        public static PlayerIdentity LocalPlayer { get; private set; }
        
        [SerializeField] private TMP_Text playerNameText;

        [ShowInInspector] public ulong ClientId => _clientId.Value;
        [ShowInInspector] public byte TeamId
        {
            get { return _teamId.Value; }
            set
            {
                Debug.LogWarning("CAN NOT MODIFY TEAM ID UNLESS ITS THROUGH INSPECTOR");
                _teamId.Value = value;
            }
        }

        private NetworkVariable<ulong> _clientId = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<byte> _teamId = new(writePerm: NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            LocalPlayer = this;
            _clientId.Value = NetworkManager.LocalClientId;
            UpdatePlayerNameRpc(SteamClient.Name);
        }

        private void Start()
        {
            if (!IsOwner && string.IsNullOrEmpty(playerNameText.text))
            {
                RequestPlayerNameRpc();
            }
        }

        [Rpc(SendTo.Owner)]
        private void RequestPlayerNameRpc(RpcParams param = default)
        {
            UpdatePlayerNameRpc(SteamClient.Name, RpcTarget.Single(param.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void UpdatePlayerNameRpc(string name)
        {
            playerNameText.text = name;
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void UpdatePlayerNameRpc(string name, RpcParams _)
        {
            playerNameText.text = name;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            LocalPlayer = null;
        }

        public void SetTeamId(byte teamId)
        {
            _teamId.Value = teamId;
        }
    }
}