using UnityEngine;
using UnityEngine.EventSystems;

namespace VolFx
{
    [AddComponentMenu("")]
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static Plane k_GroundPlaneXY = new Plane(Vector3.forward, 0.0f);
        
        public float _mov;
        public float _lerp;
        
        private bool _follow;
        
        // =======================================================================
        public void OnBeginDrag(PointerEventData eventData)
        {
            Cursor.visible = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var world = _getWordPosition(eventData.position);
            world.z = 0f;
            
            transform.position = Vector3.Lerp(transform.position, world, Time.deltaTime * _lerp);
            transform.position = Vector3.MoveTowards(transform.position, world, Time.deltaTime * _mov);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Cursor.visible = true;
        }

        // =======================================================================
        private Vector3 _getWordPosition(Vector2 pos)
        {
            var camRay = Camera.main.ScreenPointToRay(new Vector3(pos.x, pos.y, Camera.main.farClipPlane));
            k_GroundPlaneXY.Raycast(camRay, out var d);

            return camRay.GetPoint(d);
        }
    }
}