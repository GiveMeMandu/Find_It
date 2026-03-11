using System.Collections.Generic;
using UnityEngine;
using Kamgam.UGUIWorldImage;
using UnityEngine.Rendering.Universal;
using Manager;
using Pooling;
using Sirenix.OdinInspector;
using ShowIf = Sirenix.OdinInspector.ShowIfAttribute;
#if KAMGAM_RENDER_PIPELINE_URP
using UnityEngine.Rendering.Universal;
#endif

[System.Serializable]
    public class PreSpawnEntry
    {
        [Tooltip("직접 할당할 프리팹")]
        [LabelText("프리팹")]
        public GameObject prefab;

        [Tooltip("프리팹이 없을 때 Resources 폴더에서 로드할 경로 (예: Prefabs/Tutorial/MyObj)")]
        [LabelText("Resources 경로")]
        [ShowIf("@prefab == null")]
        public string resourcePath;
    }

    public class ScreenSplat : MonoBehaviour
    {
        [LabelText("WorldImage 프리팹")]
        [SerializeField] private WorldImage worldImagePrefab;
        [LabelText("최대 활성화 수 (-1: 무제한)")]
        [SerializeField] private int maxActiveCount = -1;
        [LabelText("WorldImage 풀")]
        [SerializeField] private List<WorldImage> worldImagePool = new List<WorldImage>();
        [LabelText("스폰 플랫폼")]
        [SerializeField] private GameObject SpawnPlatform;
        [LabelText("객체 간 최소 거리 (X축)")]
        [SerializeField] private float minSpawnDistance = 2f;
        [Header("WorldImage UI 위치 설정")]
        [LabelText("UI 최소 위치")]
        [SerializeField] private Vector2 minWorldImagePosition = new Vector2(-500f, -500f);
        [LabelText("UI 최대 위치")]
        [SerializeField] private Vector2 maxWorldImagePosition = new Vector2(500f, 500f);

        [Header("시작 시 자동 스폰")]
        [SerializeField] private bool usePreSpawn = false;
        [ShowIf("usePreSpawn")]
        [LabelText("스폰 목록")]
        [SerializeField] private List<PreSpawnEntry> preSpawnEntries = new List<PreSpawnEntry>();

        [Header("카메라 설정")]
        [LabelText("URP 렌더러 인덱스")]
        [Tooltip("WorldImage 카메라에 적용할 URP 렌더러 인덱스 (Project Settings > Graphics > URP Global Settings)")]
        [SerializeField] private int cameraRendererIndex = 0;

        // 활성화된 WorldImage와 연결된 GameObject 매핑
        private Dictionary<WorldImage, GameObject> activeWorldImages = new Dictionary<WorldImage, GameObject>();
        // WorldImage별 onCameraReady 콜백 매핑 (ReleaseWorldImage 시 구독 해제용)
        private Dictionary<WorldImage, System.Action<WorldObjectCamera>> cameraReadyCallbacks = new Dictionary<WorldImage, System.Action<WorldObjectCamera>>();
        private int currentSpawnIndex = 0; // 현재 소환 인덱스 (X축 위치 계산용)

        void Start()
        {
            if (!usePreSpawn) return;

            foreach (var entry in preSpawnEntries)
            {
                GameObject prefab = entry.prefab;
                if (prefab == null && !string.IsNullOrEmpty(entry.resourcePath))
                {
                    prefab = Resources.Load<GameObject>(entry.resourcePath);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"ScreenSplat: 프리팹을 찾을 수 없습니다: Resources/{entry.resourcePath}");
                        continue;
                    }
                }

                if (prefab != null)
                    ShowWorldObject(prefab);
            }
        }

        void Awake()
        {
            // 모든 WorldImage를 비활성화 상태로 초기화
            foreach (var worldImage in worldImagePool)
            {
                if (worldImage != null)
                {
                    worldImage.gameObject.SetActive(false);
                    
                    // WorldImage에 추적 컴포넌트 추가
                    var tracker = worldImage.GetComponent<WorldImageTracker>();
                    if (tracker == null)
                    {
                        tracker = worldImage.gameObject.AddComponent<WorldImageTracker>();
                    }
                    tracker.Initialize(this);
                }

                // 렌더러 인덱스 적용 구독 (풀 전체에 영구 등록)
                worldImage.OnCameraCreated += ApplyRendererToCamera;
            }
        }

        // WorldImage 카메라에 cameraRendererIndex를 적용
        private void ApplyRendererToCamera(WorldObjectCamera woCamera)
        {
            var data = woCamera.Camera.GetUniversalAdditionalCameraData();
            data.SetRenderer(cameraRendererIndex);
        }

        // 외부에서 GameObject를 전달하면 WorldImage를 할당하고 활성화
        // onCameraReady: 카메라 생성 완료 시 호출될 콜백 (URP 렌더러 교체 등 카메라 커스텀 용도)
        public WorldImage ShowWorldObject(GameObject targetObject, System.Action<WorldObjectCamera> onCameraReady = null)
        {
            if (targetObject == null)
            {
                Debug.LogWarning("ScreenSplat: targetObject가 null입니다.");
                return null;
            }
            
            // X축으로만 거리를 두고 소환 위치 계산
            Vector3 spawnPosition = SpawnPlatform.transform.position + Vector3.up + new Vector3(currentSpawnIndex * minSpawnDistance, 0, 0);
            currentSpawnIndex++;
            
            var targetObjectSpawned = Global.PoolManager.Pull<PoolObject>
            (targetObject, spawnPosition, Quaternion.Euler(0f, -180f, 0));

            // 최대 활성화 수 체크 (-1이 아닌 경우)
            if (maxActiveCount >= 0 && activeWorldImages.Count >= maxActiveCount)
            {
                Debug.LogWarning($"ScreenSplat: 최대 활성화 수({maxActiveCount})에 도달했습니다.");
                return null;
            }

            // 사용 가능한 WorldImage를 풀에서 찾기
            WorldImage availableWorldImage = GetAvailableWorldImage();
            if (availableWorldImage == null)
            {
                Debug.LogWarning("ScreenSplat: 사용 가능한 WorldImage가 없습니다.");
                return null;
            }

            // 비활성화 상태에서 위치·오브젝트를 모두 설정한 뒤 활성화
            // (SetActive 이전에 설정해야 첫 프레임에 빈 상태로 보이지 않음)
            RectTransform rectTransform = availableWorldImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 randomPosition = new Vector2(
                    Random.Range(minWorldImagePosition.x, maxWorldImagePosition.x),
                    Random.Range(minWorldImagePosition.y, maxWorldImagePosition.y)
                );
                rectTransform.anchoredPosition = randomPosition;
            }

            availableWorldImage.AddWorldObject(targetObjectSpawned.transform);

            // SetActive 이전에 구독해야 카메라 생성 이벤트를 놓치지 않음
            if (onCameraReady != null)
            {
                availableWorldImage.OnCameraCreated += onCameraReady;
                cameraReadyCallbacks[availableWorldImage] = onCameraReady;
            }

            // 모든 준비가 끝난 후 활성화
            availableWorldImage.gameObject.SetActive(true);

            // SetActive 이후 카메라가 이미 생성된 경우 즉시 콜백 실행
            if (onCameraReady != null && availableWorldImage.ObjectCamera != null)
                onCameraReady.Invoke(availableWorldImage.ObjectCamera);

            // 매핑 저장
            activeWorldImages[availableWorldImage] = targetObjectSpawned.gameObject;
            
            return availableWorldImage;
        }

        // 특정 GameObject를 숨기고 WorldImage 해제
        public void HideWorldObject(GameObject targetObject)
        {
            if (targetObject == null) return;

            // 해당 GameObject와 연결된 WorldImage 찾기
            WorldImage targetWorldImage = null;
            foreach (var pair in activeWorldImages)
            {
                if (pair.Value == targetObject)
                {
                    targetWorldImage = pair.Key;
                    break;
                }
            }

            if (targetWorldImage != null)
            {
                ReleaseWorldImage(targetWorldImage);
            }
        }

        // WorldImage가 비활성화될 때 호출되는 내부 메서드
        internal void OnWorldImageDisabled(WorldImage worldImage)
        {
            if (activeWorldImages.TryGetValue(worldImage, out GameObject targetObject))
            {
                ReleaseWorldImage(worldImage);
            }
        }

        // WorldImage를 정리하고 풀로 반환
        private void ReleaseWorldImage(WorldImage worldImage)
        {
            if (worldImage == null) return;

            // 연결된 GameObject가 있다면 비활성화
            if (activeWorldImages.TryGetValue(worldImage, out GameObject targetObject))
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
                }
                
                // WorldObject 제거
                worldImage.RemoveWorldObject(targetObject.transform);
                
                // 매핑에서 제거
                activeWorldImages.Remove(worldImage);
            }

            // WorldImage 비활성화
            worldImage.gameObject.SetActive(false);

            // onCameraReady 콜백 구독 해제
            if (cameraReadyCallbacks.TryGetValue(worldImage, out var callback))
            {
                worldImage.OnCameraCreated -= callback;
                cameraReadyCallbacks.Remove(worldImage);
            }
        }

        // 사용 가능한 WorldImage 찾기 (없으면 새로 생성)
        private WorldImage GetAvailableWorldImage()
        {
            // 기존 풀에서 비활성화된 WorldImage 찾기
            foreach (var worldImage in worldImagePool)
            {
                if (worldImage != null && !worldImage.gameObject.activeSelf)
                {
                    return worldImage;
                }
            }

            // 사용 가능한 WorldImage가 없으면 새로 생성
            if (worldImagePrefab != null)
            {
                WorldImage newWorldImage = Instantiate(worldImagePrefab, transform);
                newWorldImage.gameObject.SetActive(false);
                
                // 추적 컴포넌트 추가
                var tracker = newWorldImage.GetComponent<WorldImageTracker>();
                if (tracker == null)
                {
                    tracker = newWorldImage.gameObject.AddComponent<WorldImageTracker>();
                }
                tracker.Initialize(this);

                // 렌더러 인덱스 적용 구독
                newWorldImage.OnCameraCreated += ApplyRendererToCamera;

                // 풀에 추가
                worldImagePool.Add(newWorldImage);

                return newWorldImage;
            }

            return null;
        }

        void OnDestroy()
        {
            // 정리
            activeWorldImages.Clear();
            cameraReadyCallbacks.Clear();
        }
    }