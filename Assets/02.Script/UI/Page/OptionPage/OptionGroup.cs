using UnityEngine;
using I2.Loc;

namespace OptionPageNamespace
{
    public class OptionGroup : MonoBehaviour
    {
        public Localize labelText;
        public OptionSlider optionSliderPrefab;
        public OptionDropdown optionDropdownPrefab;
        public OptionToggle optionTogglePrefab;
        public OptionGamepadBinding optionGamepadBindingPrefab;
        public OptionButton optionButtonPrefab;

        void Awake()
        {
            optionSliderPrefab.gameObject.SetActive(false);
            optionDropdownPrefab.gameObject.SetActive(false);
            optionTogglePrefab.gameObject.SetActive(false);
            optionGamepadBindingPrefab.gameObject.SetActive(false);
            optionButtonPrefab.gameObject.SetActive(false);
        }

        public OptionSlider CreateOptionSlider()
        {
            var optionSlider = Instantiate(optionSliderPrefab, this.transform);
            optionSlider.gameObject.SetActive(true);
            return optionSlider;
        }
        public OptionDropdown CreateOptionDropdown()
        {
            var optionDropdown = Instantiate(optionDropdownPrefab, this.transform);
            optionDropdown.gameObject.SetActive(true);
            return optionDropdown;
        }

        public OptionToggle CreateOptionToggle()
        {
            var optionToggle = Instantiate(optionTogglePrefab, this.transform);
            optionToggle.gameObject.SetActive(true);
            return optionToggle;
        }

        public OptionGamepadBinding CreateOptionGamepadBinding()
        {
            var optionGamepadBinding = Instantiate(optionGamepadBindingPrefab, this.transform);
            optionGamepadBinding.gameObject.SetActive(true);
            return optionGamepadBinding;
        }

        public OptionButton CreateOptionButton()
        {
            var optionButton = Instantiate(optionButtonPrefab, this.transform);
            optionButton.gameObject.SetActive(true);
            return optionButton;
        }
    }
}
