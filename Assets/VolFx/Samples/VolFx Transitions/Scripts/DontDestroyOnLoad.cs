using System.Collections;
using UnityEngine;

namespace VolFx
{
    [AddComponentMenu("")]
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private IEnumerator Start()
        {
            if (transform.parent != null)
            {
                transform.SetParent(null);
                yield return null;
            }
            
            DontDestroyOnLoad(gameObject);
        }
    }
}