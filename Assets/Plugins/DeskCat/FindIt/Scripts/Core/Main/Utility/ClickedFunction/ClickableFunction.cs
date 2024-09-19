using System.Collections;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ClickableFunction : MonoBehaviour
    {
        public void ToggleVisible(GameObject target)
        {
            target.SetActive(!target.activeSelf);
        }
        
        public void DisableAnimator()
        {
            GetComponent<Animator>().enabled = false;
        }

        public void DebugText(string text)
        {
            Debug.Log(text);
        }

        public void SetVisible(bool visibility)
        {
            gameObject.SetActive(visibility);
        }
    }
}