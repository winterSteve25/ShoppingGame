using Objective;
using Steamworks;
using Unity.Netcode;

namespace Player
{
    public class PlayerIdentity : NetworkBehaviour
    {
        public static PlayerIdentity LocalPlayer { get; private set; }
        
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