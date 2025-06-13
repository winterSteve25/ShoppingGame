using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace SceneSpecific.MainMenu
{
    public class JoinLobbyMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform friendList;
        [SerializeField] private JoinLobbyButton buttonPrefab;
        
        private void Start()
        {
            var frs = SteamFriends.GetFriends()
                .Where(f => f.IsPlayingThisGame)
                .ToList();

            foreach (var fr in frs)
            {
                var btn = Instantiate(buttonPrefab, friendList);
                btn.Init(fr);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(friendList);
        }
    }
}