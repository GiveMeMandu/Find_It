using Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Page
{
    public abstract class View : BaseViewModel
    {
        protected UIManager _uiManager => Global.UIManager;

        public abstract void Init(params object[] parameters);
        public virtual void OnClose() { }
        public virtual bool BlockEscape => false;
        public virtual bool AutoSelect => true;

        public virtual void Focus()
        {
            var selectable = GetComponentInChildren<Selectable>();
            if (selectable != null && AutoSelect)
            {
                selectable.Select();
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Called when the escape key is pressed while this view is active.
        /// </summary>
        public virtual void OnEscapePressed()
        {
        }
        // public abstract void Refresh();
    }
}