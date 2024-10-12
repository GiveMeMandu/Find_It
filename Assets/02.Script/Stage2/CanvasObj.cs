using UnityEngine;

namespace InGame
{
    [RequireComponent(typeof(Animator))]
    public class CanvasObj : LevelManagerCount
    {
        [SerializeField] private Transform brush;

        private Animator _animator;
        private string curAnim;

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            brush.gameObject.SetActive(false);
        }

        protected override void OnFoundObj(object sender, int e)
        {
            base.OnFoundObj(sender, e);
            if (brush.gameObject.activeSelf)
            {
                if (curFoundCount == curFoundCountMax / 4)
                {
                    ChangeAnimation("Scene1");
                }
                else if (curFoundCount == curFoundCountMax / 3)
                {
                    ChangeAnimation("Scene2");
                }
                else if (curFoundCount == curFoundCountMax / 2)
                {
                    ChangeAnimation("Scene3");
                }
                else if (curFoundCount == curFoundCountMax)
                {
                    ChangeAnimation("Scene4");
                }
            }
        }
        private void ChangeAnimation(string name, float crossFade = 0)
        {
            if (curAnim != name)
            {
                curAnim = name;
                _animator.CrossFade(name, crossFade);
            }
        }

        public void FoundBrush()
        {
            brush.gameObject.SetActive(true);
            OnFoundObj(this, curFoundCount);
        }
    }
}