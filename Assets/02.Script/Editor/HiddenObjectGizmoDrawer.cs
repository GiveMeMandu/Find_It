using UnityEngine;
using UnityEditor;

namespace EditorRuntime
{
    // 에디터 스크립트 - 씬에서 Hidden 태그 객체를 표시
    [InitializeOnLoad]
    public static class HiddenObjectGizmoDrawer
    {
        // 초기화 플래그
        private static bool initialized = false;
        
        // 기즈모 표시 여부 - 에디터 설정 저장
        private static bool showGizmos = true;
        
        // 기즈모 설정 저장 키
        private const string ShowGizmosKey = "HiddenObjectGizmoDrawer_ShowGizmos";
        private const string LabelSizeKey = "HiddenObjectGizmoDrawer_LabelSize";
        private const string GizmoColorKey = "HiddenObjectGizmoDrawer_GizmoColor";
        private const string LabelColorKey = "HiddenObjectGizmoDrawer_LabelColor";
        
        // 기본 설정값
        private static int labelSize = 12;
        private static Color gizmoColor = new Color(1f, 0.6f, 0.1f, 0.8f); // 주황색
        private static Color labelColor = new Color(1f, 0.6f, 0.1f, 1f);
        
        // 정적 생성자
        static HiddenObjectGizmoDrawer()
        {
            // 설정 불러오기
            showGizmos = EditorPrefs.GetBool(ShowGizmosKey, true);
            labelSize = EditorPrefs.GetInt(LabelSizeKey, 12);
            
            // 색상 설정 로드
            string gizmoColorHex = EditorPrefs.GetString(GizmoColorKey, ColorUtility.ToHtmlStringRGBA(gizmoColor));
            ColorUtility.TryParseHtmlString("#" + gizmoColorHex, out gizmoColor);
            
            string labelColorHex = EditorPrefs.GetString(LabelColorKey, ColorUtility.ToHtmlStringRGBA(labelColor));
            ColorUtility.TryParseHtmlString("#" + labelColorHex, out labelColor);
            
            // 이미 초기화된 경우 리턴
            if (initialized) return;
            
            // 이벤트 등록
            SceneView.duringSceneGui += OnSceneGUI;
            initialized = true;
            
            // 에디터 종료 시 정리
            EditorApplication.quitting += OnEditorQuitting;
        }
        
        // 에디터 종료 시 호출
        private static void OnEditorQuitting()
        {
            if (initialized)
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                initialized = false;
            }
        }
        
        // 메뉴 아이템 - 기즈모 표시 토글
        [MenuItem("Tools/Hidden Objects/Toggle Gizmos _h")]
        private static void ToggleGizmos()
        {
            showGizmos = !showGizmos;
            EditorPrefs.SetBool(ShowGizmosKey, showGizmos);
            
            // 변경 사항 즉시 반영을 위해 씬 뷰 리프레시
            SceneView.RepaintAll();
        }
        
        // 메뉴 체크 표시 업데이트
        [MenuItem("Tools/Hidden Objects/Toggle Gizmos _h", true)]
        private static bool ToggleGizmosValidate()
        {
            return true;
        }
        
        // VFX 설정 창 열기
        [MenuItem("Tools/Hidden Objects/Settings")]
        private static void OpenSettings()
        {
            HiddenObjectSettingsWindow.ShowWindow();
        }
        
        // Scene GUI 렌더링
        private static void OnSceneGUI(SceneView sceneView)
        {
            // 기즈모 표시가 꺼져 있으면 렌더링 안함
            if (!showGizmos) return;
            
            // "Hidden" 태그가 있는 모든 객체 찾기
            GameObject[] hiddenObjects = GameObject.FindGameObjectsWithTag("Hidden");
            Camera sceneCamera = sceneView.camera;
            
            foreach (GameObject hiddenObject in hiddenObjects)
            {
                if (hiddenObject == null || !hiddenObject.activeInHierarchy) 
                    continue;
                
                // 카메라 시야에 있는지 대략적으로 확인
                if (IsVisibleToCamera(hiddenObject.transform.position, sceneCamera))
                {
                    DrawHiddenObjectGizmo(hiddenObject);
                }
            }
        }
        
        // 기즈모 표시 상태 가져오기
        public static bool IsGizmoVisible()
        {
            return showGizmos;
        }
        
        // 기즈모 표시 상태 설정
        public static void SetGizmoVisible(bool visible)
        {
            showGizmos = visible;
            EditorPrefs.SetBool(ShowGizmosKey, showGizmos);
            SceneView.RepaintAll();
        }
        
        // 기즈모 설정 가져오기
        public static int GetLabelSize() => labelSize;
        public static Color GetGizmoColor() => gizmoColor;
        public static Color GetLabelColor() => labelColor;
        
        // 기즈모 설정 저장하기
        public static void SaveSettings(int newLabelSize, Color newGizmoColor, Color newLabelColor)
        {
            // 텍스쳐 캐시 초기화 (색상 변경되었을 가능성)
            _backgroundTexture = null;
            
            // 새 설정 저장
            labelSize = newLabelSize;
            gizmoColor = newGizmoColor;
            labelColor = newLabelColor;
            
            // EditorPrefs에 저장
            EditorPrefs.SetInt(LabelSizeKey, labelSize);
            EditorPrefs.SetString(GizmoColorKey, ColorUtility.ToHtmlStringRGBA(gizmoColor));
            EditorPrefs.SetString(LabelColorKey, ColorUtility.ToHtmlStringRGBA(labelColor));
            
            // 변경사항 즉시 반영
            SceneView.RepaintAll();
        }
        
        // 카메라 시야 체크
        private static bool IsVisibleToCamera(Vector3 position, Camera camera)
        {
            Vector3 viewportPoint = camera.WorldToViewportPoint(position);
            return (viewportPoint.z > 0 && 
                    viewportPoint.x > -0.5f && viewportPoint.x < 1.5f &&
                    viewportPoint.y > -0.5f && viewportPoint.y < 1.5f);
        }
        
        // 숨겨진 오브젝트 기즈모 그리기
        private static void DrawHiddenObjectGizmo(GameObject hiddenObject)
        {
            // 오브젝트의 실제 영역 계산 - 기본값으로 초기화
            Bounds bounds = new Bounds(hiddenObject.transform.position, Vector3.one);
            bool foundBounds = false;
            
            // 1. 콜라이더로 영역 계산 먼저 시도
            Collider collider = hiddenObject.GetComponent<Collider>();
            Collider2D collider2D = hiddenObject.GetComponent<Collider2D>();
            
            if (collider != null)
            {
                // 3D 콜라이더 사용
                bounds = collider.bounds;
                foundBounds = true;
            }
            else if (collider2D != null)
            {
                // 2D 콜라이더 사용
                bounds = collider2D.bounds;
                foundBounds = true;
            }
            
            // 2. 콜라이더가 없으면 렌더러 확인
            if (!foundBounds)
            {
                Renderer renderer = hiddenObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // 3D 오브젝트인 경우 렌더러 바운드 사용
                    bounds = renderer.bounds;
                    foundBounds = true;
                }
            }
            
            // 3. UI 요소 확인
            if (!foundBounds)
            {
                RectTransform rectTransform = hiddenObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // UI 오브젝트인 경우 RectTransform 사용
                    bounds = new Bounds(rectTransform.position, Vector3.zero);
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);
                    
                    foreach (Vector3 corner in corners)
                    {
                        bounds.Encapsulate(corner);
                    }
                    foundBounds = true;
                }
            }
            
            // 4. 자식 오브젝트의 렌더러나 콜라이더도 확인
            if (!foundBounds)
            {
                bounds = new Bounds(hiddenObject.transform.position, Vector3.zero);
                bool hasChildBounds = false;
                
                // 자식 콜라이더 확인
                Collider[] childColliders = hiddenObject.GetComponentsInChildren<Collider>();
                foreach (Collider childCollider in childColliders)
                {
                    if (childCollider != collider) // 부모가 아닌 진짜 자식인 경우만
                    {
                        if (!hasChildBounds)
                        {
                            bounds = childCollider.bounds;
                            hasChildBounds = true;
                        }
                        else
                        {
                            bounds.Encapsulate(childCollider.bounds);
                        }
                    }
                }
                
                // 자식 2D 콜라이더 확인
                if (!hasChildBounds)
                {
                    Collider2D[] childColliders2D = hiddenObject.GetComponentsInChildren<Collider2D>();
                    foreach (Collider2D childCollider in childColliders2D)
                    {
                        if (childCollider != collider2D) // 부모가 아닌 진짜 자식인 경우만
                        {
                            if (!hasChildBounds)
                            {
                                bounds = childCollider.bounds;
                                hasChildBounds = true;
                            }
                            else
                            {
                                bounds.Encapsulate(childCollider.bounds);
                            }
                        }
                    }
                }
                
                // 자식 렌더러 확인
                if (!hasChildBounds)
                {
                    Renderer[] childRenderers = hiddenObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer childRenderer in childRenderers)
                    {
                        if (!hasChildBounds)
                        {
                            bounds = childRenderer.bounds;
                            hasChildBounds = true;
                        }
                        else
                        {
                            bounds.Encapsulate(childRenderer.bounds);
                        }
                    }
                }
                
                foundBounds = hasChildBounds;
            }
            
            // 5. 어떤 바운드도 찾지 못한 경우 기본 크기 사용
            if (!foundBounds || bounds.size == Vector3.zero)
            {
                // 어떤 바운드도 결정할 수 없으면 기본 크기 사용
                bounds = new Bounds(hiddenObject.transform.position, new Vector3(1f, 1f, 1f));
            }

            // 박스 그리기
            Handles.color = gizmoColor;
            Handles.DrawWireCube(bounds.center, bounds.size);

            // "숨긴 물건" 표시 + 객체 이름
            string hiddenLabel = hiddenObject.name;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = labelColor;
            style.fontSize = labelSize;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.background = MakeTexture(1, 1, new Color(0f, 0f, 0f, 0.5f));

            // 이름 표시 위치 계산 (박스 위쪽)
            Vector3 labelPosition = bounds.center + new Vector3(0, bounds.extents.y + 0.2f, 0);
            
            // 씬 뷰에 텍스트 표시
            Handles.Label(labelPosition, hiddenLabel, style);
        }
        
        // 텍스처 생성 및 캐싱
        private static Texture2D _backgroundTexture;
        private static Texture2D MakeTexture(int width, int height, Color color)
        {
            // 이미 생성된 텍스처가 있으면 재사용
            if (_backgroundTexture != null)
                return _backgroundTexture;
                
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            _backgroundTexture = new Texture2D(width, height);
            _backgroundTexture.SetPixels(pixels);
            _backgroundTexture.Apply();
            return _backgroundTexture;
        }
    }
    
    // Hidden 오브젝트 설정 창
    public class HiddenObjectSettingsWindow : EditorWindow
    {
        // 설정 저장 변수
        private bool showGizmos;
        private int labelSize;
        private Color gizmoColor;
        private Color labelColor;
        
        // 편집 상태 추적
        private bool settingsChanged = false;
        
        // 미리보기 이미지
        private Texture2D previewTexture;
        
        // 설정 창 열기
        [MenuItem("Tools/Hidden Objects/Settings")]
        public static void ShowWindow()
        {
            HiddenObjectSettingsWindow window = GetWindow<HiddenObjectSettingsWindow>("숨긴 물건 설정");
            window.minSize = new Vector2(350, 400);
            window.maxSize = new Vector2(500, 600);
            window.ShowUtility();
        }
        
        // 윈도우가 활성화될 때 호출
        private void OnEnable()
        {
            // 현재 설정 불러오기
            LoadSettings();
            
            // 미리보기 생성
            UpdatePreviewTexture();
        }
        
        // 설정 불러오기
        private void LoadSettings()
        {
            showGizmos = HiddenObjectGizmoDrawer.IsGizmoVisible();
            labelSize = HiddenObjectGizmoDrawer.GetLabelSize();
            gizmoColor = HiddenObjectGizmoDrawer.GetGizmoColor();
            labelColor = HiddenObjectGizmoDrawer.GetLabelColor();
            
            settingsChanged = false;
        }
        
        // 미리보기 텍스처 업데이트
        private void UpdatePreviewTexture()
        {
            const int texWidth = 200;
            const int texHeight = 150;
            
            // 미리 있으면 제거
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
            }
            
            previewTexture = new Texture2D(texWidth, texHeight);
            Color[] pixels = new Color[texWidth * texHeight];
            
            // 배경색 - 짙은 회색
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            
            // 간단한 큐브 렌더링
            int centerX = texWidth / 2;
            int centerY = texHeight / 2;
            int boxSize = 60;
            
            // 큐브 영역에 색상 적용
            for (int y = centerY - boxSize / 2; y < centerY + boxSize / 2; y++)
            {
                for (int x = centerX - boxSize / 2; x < centerX + boxSize / 2; x++)
                {
                    // 테두리인 경우
                    bool isBorder = 
                        x == centerX - boxSize / 2 || 
                        x == centerX + boxSize / 2 - 1 || 
                        y == centerY - boxSize / 2 || 
                        y == centerY + boxSize / 2 - 1;
                    
                    if (isBorder)
                    {
                        // 테두리에는 gizmoColor 적용
                        int index = y * texWidth + x;
                        if (index >= 0 && index < pixels.Length)
                        {
                            pixels[index] = gizmoColor;
                        }
                    }
                }
            }
            
            previewTexture.SetPixels(pixels);
            previewTexture.Apply();
        }
        
        // UI 그리기
        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("숨긴 물건 기즈모 설정", EditorStyles.boldLabel);
            GUILayout.Space(15);
            
            EditorGUI.BeginChangeCheck();
            
            // 기즈모 표시 여부 설정
            showGizmos = EditorGUILayout.Toggle("기즈모 표시", showGizmos);
            
            // 라벨 표시 크기
            labelSize = EditorGUILayout.IntSlider("라벨 크기", labelSize, 8, 24);
            
            // 색상 설정
            gizmoColor = EditorGUILayout.ColorField("기즈모 색상", gizmoColor);
            labelColor = EditorGUILayout.ColorField("라벨 색상", labelColor);
            
            if (EditorGUI.EndChangeCheck())
            {
                settingsChanged = true;
                UpdatePreviewTexture();
            }
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("단축키 'H'로 숨긴 물건 기즈모 표시를 토글할 수 있습니다.", MessageType.Info);
            
            // 미리보기 표시
            GUILayout.Space(20);
            GUILayout.Label("미리보기:", EditorStyles.boldLabel);
            
            // 미리보기 중앙 정렬
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (previewTexture != null)
            {
                GUILayout.Box(previewTexture, GUILayout.Width(200), GUILayout.Height(150));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            
            // 기본값으로 초기화 버튼
            if (GUILayout.Button("기본 설정으로 초기화", GUILayout.Height(30)))
            {
                showGizmos = true;
                labelSize = 12;
                gizmoColor = new Color(1f, 0.6f, 0.1f, 0.8f); // 주황색
                labelColor = new Color(1f, 0.6f, 0.1f, 1f);
                
                settingsChanged = true;
                UpdatePreviewTexture();
            }
            
            GUILayout.Space(10);
            
            // 저장 및 닫기 버튼
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("취소", GUILayout.Height(30)))
            {
                Close();
            }
            
            GUI.enabled = settingsChanged;
            if (GUILayout.Button("적용", GUILayout.Height(30)))
            {
                // 변경 사항 저장
                HiddenObjectGizmoDrawer.SetGizmoVisible(showGizmos);
                HiddenObjectGizmoDrawer.SaveSettings(labelSize, gizmoColor, labelColor);
                
                settingsChanged = false;
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
        }
        
        // 윈도우가 닫힐 때 리소스 정리
        private void OnDisable()
        {
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
                previewTexture = null;
            }
        }
    }
}
