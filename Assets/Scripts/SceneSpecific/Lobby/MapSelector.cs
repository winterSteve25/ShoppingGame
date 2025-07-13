using Objective;
using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.Lobby
{
    public class MapSelector : MonoBehaviour, IInteractable
    {
        [SerializeField] private int mapIndex;
        [SerializeField] private StartGameManager startGameManager;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material selectedMaterial;
        
        private MeshRenderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
            OnValueChanged(0, 0);
        }

        private void OnEnable()
        {
            startGameManager.SelectedMap.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            startGameManager.SelectedMap.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(int previousvalue, int newvalue)
        {
            if (newvalue != mapIndex)
            {
                _renderer.material = defaultMaterial;
            }
            else
            {
                _renderer.material = selectedMaterial;
            }
        }

        public bool Interact(NetworkObject player, bool left)
        {
            startGameManager.SelectedMap.Value = mapIndex;
            return true;
        }
    }
}