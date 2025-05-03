using UnityEngine;
using UnityEditor;
using Effect;
using System.Collections.Generic;
namespace EditorRuntime
{
    // 에디터 스크립트
    [InitializeOnLoad]
    public static class VFXObjectGizmoDrawer
    {
        // 초기화 플래그
        private static bool initialized = false;
        
        // 기즈모 표시 여부 - 에디터 설정 저장
        private static bool showGizmos = true;
        
        // 기즈모 설정 저장 키
        private const string ShowGizmosKey = "VFXObjectGizmoDrawer_ShowGizmos";
        private const string LabelSizeKey = "VFXObjectGizmoDrawer_LabelSize";
        private const string GizmoColorKey = "VFXObjectGizmoDrawer_GizmoColor";
        private const string LabelColorKey = "VFXObjectGizmoDrawer_LabelColor";
        private const string ShowPlayingStatusKey = "VFXObjectGizmoDrawer_ShowPlayingStatus";
        
        // 기본 설정값
        private static int labelSize = 12;
        private static Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.8f);
        private static Color labelColor = new Color(0.2f, 0.8f, 1f, 1f);
        private static bool showPlayingStatus = true;
        
        // 정적 생성자
        static VFXObjectGizmoDrawer()
        {
            // 설정 불러오기
            showGizmos = EditorPrefs.GetBool(ShowGizmosKey, true);
            labelSize = EditorPrefs.GetInt(LabelSizeKey, 12);
            
            // 색상 설정 로드
            string gizmoColorHex = EditorPrefs.GetString(GizmoColorKey, ColorUtility.ToHtmlStringRGBA(gizmoColor));
            ColorUtility.TryParseHtmlString("#" + gizmoColorHex, out gizmoColor);
            
            string labelColorHex = EditorPrefs.GetString(LabelColorKey, ColorUtility.ToHtmlStringRGBA(labelColor));
            ColorUtility.TryParseHtmlString("#" + labelColorHex, out labelColor);
            
            showPlayingStatus = EditorPrefs.GetBool(ShowPlayingStatusKey, true);
            
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
        [MenuItem("Tools/VFX/Toggle Gizmos _g")]
        private static void ToggleGizmos()
        {
            showGizmos = !showGizmos;
            EditorPrefs.SetBool(ShowGizmosKey, showGizmos);
            
            // 변경 사항 즉시 반영을 위해 씬 뷰 리프레시
            SceneView.RepaintAll();
        }
        
        // 메뉴 체크 표시 업데이트
        [MenuItem("Tools/VFX/Toggle Gizmos _g", true)]
        private static bool ToggleGizmosValidate()
        {
            // Unity 2019부터는 Menu.SetChecked가 아니라 체크 상태를 반환하는 방식 사용
            return true;
        }
        
        // VFX 설정 창 열기
        [MenuItem("Tools/VFX/Settings")]
        private static void OpenSettings()
        {
            VFXSettingsWindow.ShowWindow();
        }
        
        // Scene GUI 렌더링
        private static void OnSceneGUI(SceneView sceneView)
        {
            // 기즈모 표시가 꺼져 있으면 렌더링 안함
            if (!showGizmos) return;
            
            VFXObject[] vfxObjects = Object.FindObjectsOfType<VFXObject>();
            Camera sceneCamera = sceneView.camera;
            
            foreach (VFXObject vfxObject in vfxObjects)
            {
                if (vfxObject == null || !vfxObject.gameObject.activeInHierarchy) 
                    continue;
                
                // 카메라 시야에 있는지 대략적으로 확인
                if (IsVisibleToCamera(vfxObject.transform.position, sceneCamera))
                {
                    DrawVFXGizmo(vfxObject);
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
        public static bool GetShowPlayingStatus() => showPlayingStatus;
        
        // 기즈모 설정 저장하기
        public static void SaveSettings(int newLabelSize, Color newGizmoColor, Color newLabelColor, bool newShowPlayingStatus)
        {
            // 텍스쳐 캐시 초기화 (색상 변경되었을 가능성)
            _backgroundTexture = null;
            
            // 새 설정 저장
            labelSize = newLabelSize;
            gizmoColor = newGizmoColor;
            labelColor = newLabelColor;
            showPlayingStatus = newShowPlayingStatus;
            
            // EditorPrefs에 저장
            EditorPrefs.SetInt(LabelSizeKey, labelSize);
            EditorPrefs.SetString(GizmoColorKey, ColorUtility.ToHtmlStringRGBA(gizmoColor));
            EditorPrefs.SetString(LabelColorKey, ColorUtility.ToHtmlStringRGBA(labelColor));
            EditorPrefs.SetBool(ShowPlayingStatusKey, showPlayingStatus);
            
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
        
        // VFX 기즈모 그리기
        private static void DrawVFXGizmo(VFXObject vfxObject)
        {
            // 오브젝트의 실제 영역 계산
            Bounds bounds = new Bounds(vfxObject.transform.position, Vector3.one);
            bool foundBounds = false;
            
            // 1. 콜라이더로 영역 계산 먼저 시도
            Collider collider = vfxObject.GetComponent<Collider>();
            Collider2D collider2D = vfxObject.GetComponent<Collider2D>();
            
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
                Renderer renderer = vfxObject.GetComponent<Renderer>();
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
                RectTransform rectTransform = vfxObject.GetComponent<RectTransform>();
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
                bounds = new Bounds(vfxObject.transform.position, Vector3.zero);
                bool hasChildBounds = false;
                
                // 자식 콜라이더 확인
                Collider[] childColliders = vfxObject.GetComponentsInChildren<Collider>();
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
                    Collider2D[] childColliders2D = vfxObject.GetComponentsInChildren<Collider2D>();
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
                    Renderer[] childRenderers = vfxObject.GetComponentsInChildren<Renderer>();
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
                bounds = new Bounds(vfxObject.transform.position, new Vector3(1f, 1f, 1f));
            }

            // 박스 그리기
            Handles.color = gizmoColor;
            Handles.DrawWireCube(bounds.center, bounds.size);

            // 효과 클래스 이름 표시
            string effectName = vfxObject.GetType().Name;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = labelColor;
            style.fontSize = labelSize;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.background = MakeTexture(1, 1, new Color(0f, 0f, 0f, 0.5f));

            // 이름 표시 위치 계산 (박스 위쪽)
            Vector3 labelPosition = bounds.center + new Vector3(0, bounds.extents.y + 0.2f, 0);
            
            // 씬 뷰에 텍스트 표시
            Handles.Label(labelPosition, effectName, style);
            
            // 클래스 이름 아래에 상태 정보 추가 (재생 중인지 여부)
            if (showPlayingStatus && vfxObject.isPlaying)
            {
                GUIStyle playingStyle = new GUIStyle();
                playingStyle.normal.textColor = new Color(0.2f, 1f, 0.2f);
                playingStyle.fontSize = Mathf.Max(8, labelSize - 2);
                playingStyle.alignment = TextAnchor.MiddleCenter;
                playingStyle.normal.background = MakeTexture(1, 1, new Color(0f, 0f, 0f, 0.3f));
                
                Vector3 statusPosition = labelPosition + new Vector3(0, 0.2f, 0);
                Handles.Label(statusPosition, "Playing", playingStyle);
            }
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
    
    // VFX 설정 창
    public class VFXSettingsWindow : EditorWindow
    {
        // 설정 저장 변수
        private bool showGizmos;
        private int labelSize;
        private Color gizmoColor;
        private Color labelColor;
        private bool showPlayingStatus;
        
        // 설정 초기화 
        private bool resetSettings = false;
        
        // 편집 상태 추적
        private bool settingsChanged = false;
        
        // 미리보기 이미지
        private Texture2D previewTexture;
        
        // 설정 탭
        private int selectedTab = 0;
        private readonly string[] tabNames = { "기본 설정", "시각 효과" };
        
        // 설정 창 열기
        [MenuItem("Tools/VFX/Settings")]
        public static void ShowWindow()
        {
            VFXSettingsWindow window = GetWindow<VFXSettingsWindow>("VFX 설정");
            window.minSize = new Vector2(350, 450);
            window.maxSize = new Vector2(500, 650);
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
            showGizmos = VFXObjectGizmoDrawer.IsGizmoVisible();
            labelSize = VFXObjectGizmoDrawer.GetLabelSize();
            gizmoColor = VFXObjectGizmoDrawer.GetGizmoColor();
            labelColor = VFXObjectGizmoDrawer.GetLabelColor();
            showPlayingStatus = VFXObjectGizmoDrawer.GetShowPlayingStatus();
            
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
        
        // 기본 설정
        private void DrawBasicSettings()
        {
            EditorGUI.BeginChangeCheck();
            
            // 기즈모 표시 여부 설정
            showGizmos = EditorGUILayout.Toggle("기즈모 표시", showGizmos);
            
            // 라벨 표시 크기
            labelSize = EditorGUILayout.IntSlider("라벨 크기", labelSize, 8, 24);
            
            // 플레잉 상태 표시 여부
            showPlayingStatus = EditorGUILayout.Toggle("재생 상태 표시", showPlayingStatus);
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("단축키 'G'로 기즈모 표시를 토글할 수 있습니다.", MessageType.Info);
            
            if (EditorGUI.EndChangeCheck())
            {
                settingsChanged = true;
                UpdatePreviewTexture();
            }
        }
        
        // 시각 효과 설정
        private void DrawVisualSettings()
        {
            EditorGUI.BeginChangeCheck();
            
            // 색상 설정
            gizmoColor = EditorGUILayout.ColorField("기즈모 색상", gizmoColor);
            labelColor = EditorGUILayout.ColorField("라벨 색상", labelColor);
            
            if (EditorGUI.EndChangeCheck())
            {
                settingsChanged = true;
                UpdatePreviewTexture();
            }
        }
        
        // UI 그리기
        private void OnGUI()
        {
            GUILayout.Space(10);
            
            // 탭 선택
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            
            GUILayout.Space(15);
            
            // 탭 내용 그리기
            switch (selectedTab)
            {
                case 0: // 기본 설정
                    DrawBasicSettings();
                    break;
                case 1: // 시각 효과
                    DrawVisualSettings();
                    break;
            }
            
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
                resetSettings = true;
                showGizmos = true;
                labelSize = 12;
                gizmoColor = new Color(0.2f, 0.8f, 1f, 0.8f);
                labelColor = new Color(0.2f, 0.8f, 1f, 1f);
                showPlayingStatus = true;
                
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
                VFXObjectGizmoDrawer.SetGizmoVisible(showGizmos);
                VFXObjectGizmoDrawer.SaveSettings(labelSize, gizmoColor, labelColor, showPlayingStatus);
                
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