using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Objective.Bingo
{
    public class BingoCell : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text text;

        private int _progress;
        private int _amountNeeded;

        public void Init(int progress, ItemStack needed)
        {
            _amountNeeded = needed.Amount;
            _progress = progress;
            text.text = $"{_progress}/{_amountNeeded}";
            icon.sprite = needed.Item.Icon;
        }

        public void Modify(int amount)
        {
            _progress += amount;
            text.text = $"{_progress}/{_amountNeeded}";
        }
    }
}