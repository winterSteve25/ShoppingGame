using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class OnScreenUI : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<CanvasGroup>().alpha = 0;
            gameObject.SetActive(false);
        }
    }
}