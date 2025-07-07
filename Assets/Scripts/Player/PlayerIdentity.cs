using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerIdentity : NetworkBehaviour
    {
        public static PlayerIdentity LocalPlayer { get; private set; }

        [SerializeField] private TMP_Text playerNameText;
        [ShowInInspector] public ulong ClientId => _clientId.Value;

        [ShowInInspector]
        public byte TeamId
        {
            get => _teamId.Value;
            set
            {
                Debug.LogWarning("CAN NOT MODIFY TEAM ID UNLESS ITS THROUGH INSPECTOR");
                _teamId.Value = value;
            }
        }

        private NetworkVariable<ulong> _clientId = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<byte> _teamId = new(writePerm: NetworkVariableWritePermission.Server);
        private NetworkVariable<FixedString32Bytes> _playerName = new(writePerm: NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            LocalPlayer = this;
            _clientId.Value = NetworkManager.LocalClientId;

#if UNITY_EDITOR
            _playerName.Value = Random.value.ToString("F1");
#else
            _playerName.Value = SteamClient.Name;
#endif
        }


        private void Start()
        {
            _playerName.OnValueChanged += (_, newValue) => { playerNameText.text = newValue.ToString(); };

            playerNameText.text = _playerName.Value.ToString();
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