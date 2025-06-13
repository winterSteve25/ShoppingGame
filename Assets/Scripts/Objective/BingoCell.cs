using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Objective
{
    public class BingoCell : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text text;

        private int _amountNeeded;

        public void Init(in ItemStack itemStack)
        {
            _amountNeeded = itemStack.Amount;
            text.text = "0/" + _amountNeeded;
        }
    }
}