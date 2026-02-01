using UnityEngine;

namespace VolFx
{
    [AddComponentMenu("")]
    public class SetActiveChildRandom : MonoBehaviour
    {
        public void Invoke()
        {
            var rnd = Random.Range(0, transform.childCount);

            while (transform.GetChild(rnd).gameObject.activeSelf)
                rnd = Random.Range(0, transform.childCount);
            
            for (var n = 0; n < transform.childCount; n++)
            {
                var child = transform.GetChild(n).gameObject;
                child.SetActive(n == rnd);
            }
        }
    }
}