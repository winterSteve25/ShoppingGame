using Player;
using Unity.Netcode;

namespace Managers
{
    public class TeamManager : NetworkBehaviour
    {
        public void LocalJoinTeam1()
        {
            PlayerIdentity.LocalPlayer.SetTeamId(0);
        }
        
        public void LocalJoinTeam2()
        {
            PlayerIdentity.LocalPlayer.SetTeamId(1);
        }
    }
}