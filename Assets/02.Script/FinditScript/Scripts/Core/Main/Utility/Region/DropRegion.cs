using DeskCat.FindIt.Scripts.Core.Main.Utility.DragObj;
using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Region
{
    public class DropRegion : MonoBehaviour
    {
        public string RegionName;
        public UnityEvent DropRegionEvent;

        private void OnMouseEnter()
        {
            CurrentDragInfo.CurrentDropRegion = this;
        }

        private void OnMouseExit()
        {
            CurrentDragInfo.CurrentDropRegion = null;
        }
    }
}