using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ui
{
    public class SwapText : MonoBehaviour
    {
        public string GamepadText;
        public string KeyboardText;
        private Text _infoText;

        private void Awake()
        {
            _infoText = GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (CheckActiveControlModel.CurrentControlState == CheckActiveControlModel.ControlModel.CmKeyboard)
            {
                _infoText.text = KeyboardText;
            }
            else
            {
                Assert.IsTrue(CheckActiveControlModel.CurrentControlState == CheckActiveControlModel.ControlModel.CmGamepad);
                _infoText.text = GamepadText;
            }
        }
    }
}