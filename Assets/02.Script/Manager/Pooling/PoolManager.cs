using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    public class PoolManager : MMSingleton<PoolManager>
    {
        private Dictionary<string, object> genericPools = new Dictionary<string, object>();
        private Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();
        private Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();

        // OnDisable 도중 SetParent가 불가능한 오브젝트를 다음 프레임에 이동시키기 위한 큐
        private readonly Queue<(string key, Component obj)> _deferredPushQueue = new Queue<(string, Component)>();

        private void LateUpdate()
        {
            // 지연된 SetParent 처리
            while (_deferredPushQueue.Count > 0)
            {
                var (key, obj) = _deferredPushQueue.Dequeue();
                if (obj == null || obj.gameObject == null) continue;
                obj.transform.SetParent(GetPoolParent(key));
            }
        }

        private Transform GetPoolParent(string key)
        {
            if (!poolParents.TryGetValue(key, out Transform parent))
            {
                // 풀 부모 오브젝트 생성
                GameObject poolParentObj = new GameObject($"Pool_{key}");
                poolParentObj.transform.SetParent(transform);
                parent = poolParentObj.transform;
                poolParents[key] = parent;
            }
            return parent;
        }

        public void CreatePool<T>(string key, T prefab, Action<T> onPull = null, Action<T> onPush = null, int preSpawn = 0) where T : Component
        {
            if (!genericPools.ContainsKey(key))
            {
                if (prefab != null && !prefabRegistry.ContainsKey(key))
                {
                    prefabRegistry[key] = prefab.gameObject;
                }
                
                var pool = new Stack<T>();
                genericPools[key] = pool;

                Transform poolParent = GetPoolParent(key);

                for (int i = 0; i < preSpawn; i++)
                {
                    var obj = Instantiate(prefab);
                    obj.gameObject.SetActive(false);
                    obj.transform.SetParent(poolParent);
                    pool.Push(obj);
                }
            }
        }

        private T GetOrCreatePooledObject<T>(string key, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            // 풀이 없으면 자동으로 생성
            if (!genericPools.ContainsKey(key))
            {
                T component = GetOrCreateComponent<T>(key, prefab);
                if (component == null) return null;
                CreatePool(key, component);
            }

            var pool = genericPools[key] as Stack<T>;
            if(pool == null)
            {
                Debug.LogError($"풀 생성 실패: {key} 키에 대한 풀이 없습니다. 타입은 {genericPools[key].GetType()}입니다.");
                return null;
            }
            T obj;

            if (pool.Count > 0)
                obj = pool.Pop();
            else
            {
                GameObject prefabToInstantiate = prefab;
                if (prefabToInstantiate == null)
                {
                    prefabRegistry.TryGetValue(key, out prefabToInstantiate);
                }

                if (prefabToInstantiate == null)
                {
                    prefabToInstantiate = Resources.Load<GameObject>(key);
                }

                if (prefabToInstantiate == null)
                {
                    Debug.LogError($"ArgumentException: The Object you want to instantiate is null. Prefab for key '{key}' not found.");
                    return null;
                }
                
                obj = Instantiate(prefabToInstantiate, position, rotation).GetComponent<T>();
            }

            // 1. 먼저 부모 지정
            if (parent != null)
                obj.transform.SetParent(parent);
            else
                obj.transform.SetParent(null);
            // 2. 그 다음 위치와 회전 설정
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            obj.gameObject.SetActive(true);

            // InjectDependencies(obj);

            return obj;
        }

        private T GetOrCreateComponent<T>(string key, GameObject prefab) where T : Component
        {
            T component = null;
            if (prefab != null)
            {
                component = prefab.GetComponent<T>();
            }
            else
            {
                var loadedPrefab = Resources.Load<GameObject>(key);
                component = loadedPrefab?.GetComponent<T>();
            }

            if (component == null)
            {
                bool canAutoAdd = typeof(T).IsSubclassOf(typeof(MonoBehaviour)) && !typeof(T).IsAbstract;

                if (canAutoAdd)
                {
                    component = prefab != null
                        ? prefab.AddComponent<T>()
                        : Resources.Load<GameObject>(key).AddComponent<T>();
                    Debug.Log($"Component of type {typeof(T)} was not found for key {key}, automatically added.");
                }
                else
                {
                    Debug.LogError($"Cannot find component of type {typeof(T)} for key {key} and it cannot be automatically added!");
                }
            }
            return component;
        }

        private bool InitializePoolObject<T>(T obj, string key, Action<T> onPull = null, Action<T> onPush = null, Transform parent = null) where T : Component
        {
            bool foundPoolable = false;
            Action<T> returnAction = tComp =>
            {
                onPush?.Invoke(tComp);
                Push(key, tComp);
            };

            // 먼저 기존 PoolObject가 있는지 확인
            var existingPoolObject = obj.GetComponent<PoolObject>();
            if (existingPoolObject != null)
            {
                existingPoolObject.Initialize(comp =>
                {
                    onPush?.Invoke(obj as T);
                    Push(key, obj);
                });
                foundPoolable = true;
            }
            else
            {
                // 다른 IPoolable<T> 구현체 찾기 시도
                foreach (var component in obj.GetComponents<Component>())
                {
                    if (component is IPoolable<T> poolableT)
                    {
                        poolableT.Initialize(returnAction);
                        foundPoolable = true;
                        break;
                    }
                }

                // IPoolable이 없으면 PoolObject를 추가
                if (!foundPoolable)
                {
                    var poolObject = obj.gameObject.AddComponent<PoolObject>();
                    poolObject.Initialize(comp => Push(key, obj));
                    Debug.Log($"PoolObject {obj.gameObject.name}에 IPoolable이 없어서 기본 PoolObject를 추가했습니다.");
                }
            }

            // 부모가 지정되지 않은 경우 풀 부모 아래에 배치
            if (parent == null && obj.transform.parent == null)
            {
                obj.transform.SetParent(GetPoolParent(key));
            }

            onPull?.Invoke(obj);
            return true;
        }

        public T Pull<T>(string key, Vector3 position, Quaternion rotation, Transform parent = null, Action<T> onPull = null, Action<T> onPush = null) where T : Component
        {
            var obj = GetOrCreatePooledObject<T>(key, null, position, rotation, parent);
            if (obj != null) InitializePoolObject(obj, key, onPull, onPush, parent);
            return obj;
        }

        public T Pull<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, Action<T> onPull = null, Action<T> onPush = null) where T : Component
        {
            string key = GetObjKey(prefab);
            var obj = GetOrCreatePooledObject<T>(key, prefab, position, rotation, parent);
            if (obj != null) InitializePoolObject(obj, key, onPull, onPush, parent);
            return obj;
        }

        public void Push<T>(string key, T obj) where T : Component
        {
            if (!genericPools.ContainsKey(key))
            {
                Debug.LogError($"Pool with key {key} doesn't exist!");
                return;
            }

            var pool = genericPools[key] as Stack<T>;

            if (obj.gameObject.activeSelf)
            {
                // 풀이 직접 비활성화하는 경우: SetParent 먼저 → SetActive(false)
                // (SetActive 이후 OnDisable → ReturnToPool 재진입을 막기 위해 순서 유지)
                obj.transform.SetParent(GetPoolParent(key));
                obj.gameObject.SetActive(false);
            }
            else
            {
                // 외부(DestroySelf 등)가 SetActive(false)를 먼저 호출해서
                // OnDisable 도중 ReturnToPool이 실행된 상황.
                // 현재 프레임에는 SetParent 불가능하므로 다음 프레임으로 지연 처리.
                _deferredPushQueue.Enqueue((key, obj));
            }

            pool.Push(obj);
        }

        public void PushToPool<T>(T obj) where T : Component, IPoolable<T>
        {
            Push(obj.GetPoolKey(), obj);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetPools()
        {
            if (_instance != null)
            {
                _instance.genericPools.Clear();
                _instance.poolParents.Clear();
                _instance.prefabRegistry.Clear();
            }
            _instance = null;
        }

        private string GetObjKey(GameObject obj)
        {
            string poolKey = obj.GetComponent<PoolObject>()?.GetPoolKey();
            if (string.IsNullOrEmpty(poolKey))
            {
                Debug.LogWarning($"{obj.name} 가 오브젝트 키 없어서 그냥 오브젝트 이름으로 키를 대신함");
                return obj.name;
            }
            return poolKey;
        }
    }

    public interface IPoolable<T> where T : Component
    {
        void Initialize(Action<T> OnPush);
        void ReturnToPool();
        string GetPoolKey();
        void SetPoolKey(string key);
    }
}