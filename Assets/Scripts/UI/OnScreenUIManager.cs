using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    [DefaultExecutionOrder(-10)]
    public class OnScreenUIManager : MonoBehaviour
    {
        public static OnScreenUIManager Instance { get; private set; }
        public bool ShouldLockInput { get; private set; }
        public event Action<bool> OnShouldLockInputChanged;
        
        private CanvasGroup _uiShowing;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }
            
            Destroy(gameObject);
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseCurrentUI();
            }
        }

        public void ShowUI(CanvasGroup ui)
        {
            _uiShowing = ui;
            _uiShowing.gameObject.SetActive(true);
            Tween.Alpha(ui, 1, 0.2f);
            ShouldLockInput = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnShouldLockInputChanged?.Invoke(ShouldLockInput);
        }

        public void CloseCurrentUI()
        {
            if (_uiShowing == null) return;
            
            var ui = _uiShowing;
            
            _uiShowing = null;
            ShouldLockInput = false;
            OnShouldLockInputChanged?.Invoke(ShouldLockInput);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            Tween.Alpha(ui, 0, 0.2f)
                .OnComplete(() =>
                {
                    ui.gameObject.SetActive(false);
                });
        }
    }
}