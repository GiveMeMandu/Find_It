using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(Selectable))]
    public class AutoSelectOnHover : MonoBehaviour, IPointerEnterHandler
    {
        private Selectable selectable;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (selectable != null && selectable.interactable && selectable.IsActive())
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}
