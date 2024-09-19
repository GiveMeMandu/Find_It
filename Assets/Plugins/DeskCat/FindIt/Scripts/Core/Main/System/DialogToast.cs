using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class DialogToast : MonoBehaviour, IPointerClickHandler
    {
        public Text dialogText;
        public Transform OriginalTransform;
        public Vector2 AnchorPosition;
        public void Initialize(string dialog, Transform objTransform)
        {
            dialogText.text = dialog;
            transform.SetParent(objTransform);
            gameObject.SetActive(false);
            GetComponent<RectTransform>().anchoredPosition = AnchorPosition;
            gameObject.SetActive(true);
            transform.gameObject.GetComponent<Animator>().enabled = true;
            transform.SetParent(OriginalTransform);
        }
        
        public void DisableAnimator() {
            transform.gameObject.GetComponent<Animator>().enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }
    }
}