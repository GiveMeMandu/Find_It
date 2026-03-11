using System;
using UnityEngine;

namespace Pooling
{
    public class PoolObject : MonoBehaviour, IPoolable<PoolObject>
    {
        [Header("풀 오브젝트 키")]
        [SerializeField] protected string poolKey;
        private Action<PoolObject> returnToPool; //* 다시 Push할 때 일어날 액션
        
        // 재귀 호출 방지를 위한 플래그 추가
        private bool _isReturning = false;
        protected virtual void OnDisable()
        {
            // 이미 반환 중인 경우 중복 호출 방지
            if (!_isReturning)
            {
                ReturnToPool();
            }
        }

        public virtual void Initialize(Action<PoolObject> returnAction)
        {
            // Debug.Log("Initialize" + this.gameObject.name + " with " + returnAction.Method.Name);
            //cache reference to return action
            this.returnToPool = returnAction;
        }

        public void ReturnToPool()
        {
            // 이미 반환 중이면 무시
            if (_isReturning || returnToPool == null) return;
            
            _isReturning = true;
            returnToPool.Invoke(this);
            _isReturning = false;
        }

        public string GetPoolKey()
        {
            if (string.IsNullOrEmpty(poolKey))
            {
                return transform.name;
            }
            return poolKey;
        }

        public void SetPoolKey(string key)
        {
            poolKey = key;
        }
    }
}