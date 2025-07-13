using Unity.Cinemachine;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class UILockedCMInputAxisController : MonoBehaviour
    {
        private CinemachineInputAxisController _controller;

        private void Start()
        {
            _controller = GetComponent<CinemachineInputAxisController>();
        }

        private void OnEnable()
        {
            OnScreenUIManager.Instance.OnShouldLockInputChanged += InstanceOnOnShouldLockInputChanged;
        }

        private void OnDisable()
        {
            OnScreenUIManager.Instance.OnShouldLockInputChanged -= InstanceOnOnShouldLockInputChanged;
        }

        private void InstanceOnOnShouldLockInputChanged(bool obj)
        {
            _controller.enabled = !obj;
        }
    }
}