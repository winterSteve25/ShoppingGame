using Objective;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.Lobby
{
    public class GameSettingOpenUI : MonoBehaviour, IInteractable
    {
        [SerializeField] private CanvasGroup ui;

        public bool Interact(NetworkObject player, bool left)
        {
            OnScreenUIManager.Instance.ShowUI(ui);
            return true;
        }
    }
}