using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Animation
{
    public class BGScaleLerp : FindItLerp
    {
        public bool UseCustomScale = false;
        public Vector3 FromScale;
        public Vector3 ToScale;

        private void Awake()
        {
            if (UseCustomScale == false)
            {
                FromScale = Vector3.zero;
                ToScale = transform.localScale;
            }
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            var value = GetLerpValue(currentTime);
            transform.localScale = (FromScale * (1-value)) + (ToScale * value);
        }
   
    }
}