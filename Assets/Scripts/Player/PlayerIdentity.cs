using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerIdentity : NetworkBehaviour
    {
        public static PlayerIdentity LocalPlayer { get; private set; }

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