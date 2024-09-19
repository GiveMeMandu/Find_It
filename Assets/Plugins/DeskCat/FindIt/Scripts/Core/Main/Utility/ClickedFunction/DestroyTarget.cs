using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main
{
    public class DestroyTarget : MonoBehaviour
    {
        public GameObject Target;

        public void DestroyTargetObject()
        {
            Destroy(Target);
        }
    }
}
