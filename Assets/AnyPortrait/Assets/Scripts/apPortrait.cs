/*
*	Copyright (c) RainyRizzle Inc. All rights reserved
*	Contact to : www.rainyrizzle.com , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express permission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

using UnityEngine;
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_2017_1_OR_NEWER
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Core class with animations, meshes, and texture data
	/// </summary>
	public class apPortrait : MonoBehaviour
	{
		// Members
		//-----------------------------------------------------
		//public int _testVar = 0;

		//텍스쳐 등록 정보
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apTextureData> _textureData = new List<apTextureData>();

		//메시 등록 정보
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apMesh> _meshes = new List<apMesh>();

		//메시 그룹 등록 정보
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apMeshGroup> _meshGroups = new List<apMeshGroup>();


		//컨트롤 파라미터 등록 정보 [이건 Editor / Opt Realtime에 모두 적용된다.]
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public apController _controller = new apController();

		//애니메이션 등록 정보 [이건 Editor / Opt Runtime에 모두 적용된다]
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apAnimClip> _animClips = new List<apAnimClip>();


		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public apAnimPlayManager _animPlayManager = new apAnimPlayManager();

		//추가 20.11.23 : 애니메이션 처리 가속화를 위한 별도의 업데이트와 매핑 클래스
		//실시간으로 생성되며 Opt용이다.
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public apAnimPlayMapping _animPlayMapping = null;

		//추가 22.5.18 : 애니메이션 스크립트 함수가 바로 실행되지 않고 지연되어 실행되도록 만든 클래스
		[NonSerialized]
		private apAnimPlayDeferredRequest _animPlayDeferredRequest = null;


		//RootUnit으로 적용되는 MainMeshGroup을 여러개를 둔다.
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<int> _mainMeshGroupIDList = new List<int>();

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public List<apMeshGroup> _mainMeshGroupList = new List<apMeshGroup>();



		// 루트 유닛을 여러 개를 둔다 (루트 유닛은 애니메이션이 적용되는 MeshGroup이다)
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public List<apRootUnit> _rootUnits = new List<apRootUnit>();




		// 유니크 IDs
		[NonBackupField]
		private apIDManager _IDManager = new apIDManager();


		//추가 21.1.22 : 편집용 VisibiliePreset을 여기에 추가한다.
		[NonBackupField, SerializeField]
		private apVisibilityPresets _visiblePreset = new apVisibilityPresets();

		public apVisibilityPresets VisiblePreset { get { if (_visiblePreset == null) { _visiblePreset = new apVisibilityPresets(); } return _visiblePreset; } }



		//추가 21.6.3 : 작업 편의를 위한 가이드라인들
		[NonBackupField, SerializeField, HideInInspector]
		private apGuideLines _guideLines = new apGuideLines();
		public apGuideLines GuideLines { get { if (_guideLines == null) { _guideLines = new apGuideLines(); } return _guideLines; } }


		//추가 v1.5.0 : 컨트롤 파라미터 스냅샷
		[NonBackupField, SerializeField, HideInInspector]
		private apControlParamValueSnapShot _controlParamSnapShot = null;
		public apControlParamValueSnapShot ControlParamSnapShot
		{
			get { if (_controlParamSnapShot == null) { _controlParamSnapShot = new apControlParamValueSnapShot(); } return _controlParamSnapShot; }
		}


		//Runtime 계열 Members
		// 이후 "최적화" 버전에서는 이하의 Member만 적용한다.
		// Runtime 계열의 인스턴스들은 모두 opt를 앞에 붙인다.
		// 중요) 백업은 하지 않는다.
		//---------------------------------------------
		/// <summary>A List of Optimized Root Units executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptRootUnit> _optRootUnitList = new List<apOptRootUnit>();

		/// <summary>Currently selected and running Root Unit</summary>
		[NonSerialized]
		public apOptRootUnit _curPlayingOptRootUnit = null;//<<현재 재생중인 OptRootUnit

		[NonSerialized]
		private apOptRootUnit _prevOptRootUnit = null;//v1.4.7 추가 : 이전 프레임에서 선택되었던 Opt 루트 유닛

		/// <summary>A List of Optimized Transforms executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptTransform> _optTransforms = null;

		/// <summary>A List of Optimized Meshes executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptMesh> _optMeshes = null;



		// Opt Texture를 Bake하여 관리한다.
		/// <summary>[Please do not use it] A List of Optimized Texture Data executed at runtime</summary>
		[SerializeField, NonBackupField]
		public List<apOptTextureData> _optTextureData = null;

		//[v1.4.0] 빠른 텍스쳐 접근
		[NonSerialized, NonBackupField]
		private Dictionary<string, apOptTextureData> _mapping_OptTextureData = null;


		//추가
		// Material 중에서 Batch가 될만한 것들은 중앙에서 관리를 한다.
		/// <summary>[Please do not use it] A List of Batched Materials executed at runtime</summary>
		[SerializeField, NonBackupField]
		public apOptBatchedMaterial _optBatchedMaterial = null;



		/// <summary>
		/// Listener to receive animation events.
		/// Since it is called by "UnitySendMessage", its object must be a class inherited from "MonoBehaviour".
		/// </summary>
		[SerializeField, NonBackupField]
		public MonoBehaviour _optAnimEventListener = null;


		//추가 21.9.24 : 이벤트를 UnityEvent를 이용해서 호출할 수 있다.
		//- 이 데이터는 Bake 또는 모드 변경시에 생성한다. (Serialize 됨)
		//- Link시에 AnimEvent와 연결된다.
		public enum ANIM_EVENT_CALL_MODE : int
		{
			SendMessage = 0,//기본 방식
			Callback = 1,//추가된 방식
		}
		[SerializeField, NonBackupField]
		public ANIM_EVENT_CALL_MODE _animEventCallMode = ANIM_EVENT_CALL_MODE.SendMessage;


		//UnityEvent 방식인 경우 : Wrapper 리스트를 만들자
		[SerializeField, NonBackupField, HideInInspector]
		public apUnityEventWrapper _unityEventWrapper;







		public enum INIT_STATUS
		{
			Ready,
			AsyncLoading,
			Completed
		}
		[NonSerialized]
		private INIT_STATUS _initStatus = INIT_STATUS.Ready;

		/// <summary>Status of Initialization</summary>
		public INIT_STATUS InitializationStatus { get { return _initStatus; } }

		//비동기 로딩이 끝났을때 발생하는 이벤트
		/// <summary>The delegate type of the function to be called when the asynchronous initialization finishes.</summary>
		/// <param name="portrait">Initialized Portrait</param>
		public delegate void OnAsyncLinkCompleted(apPortrait portrait);
		private OnAsyncLinkCompleted _funcAyncLinkCompleted = null;


		//기본 데이터가 저장될 하위 GameObject
		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup = null;

		//작업 저장 효율성을 위해서 일부 메인 데이터를 GameObject 형식으로 저장한다. (크윽 Undo)
		//저장되는 타겟은 Mesh와 MeshGroup
		//- Mesh는 Vertex 데이터가 많아서 저장 필요성이 있다.
		//- MeshGroup은 ModMesh의 데이터가 많아서 저장 필요성이 있다.
		//직렬화가 가능하거나 연동 전용 객체인 RootUnit과 Image은 GameObject로 만들지 않는다.
		//AnimClip과 Param은 Realtime과 연동하는 객체이므로 별도로 분리하지 않는다.
		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup_Mesh = null;

		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup_MeshGroup = null;

		/// <summary>[Please do not use it]</summary>
		[NonBackupField]
		public GameObject _subObjectGroup_Modifier = null;


		/// <summary>[Please do not use it] Frame Per Seconds</summary>
		[SerializeField]
		public int _FPS = 30;

		//이전 : Important가 아닐 때의 타이머 > UpdateToken 방식으로 변경
		//[NonBackupField]
		//private float _timePerFrame = 1.0f / 30.0f;

		//[NonBackupField]
		//private float _tDelta = 0.0f;

		//추가 2.28 : Important가 아닌 업데이트를 위한 토큰
		[NonSerialized, NonBackupField]
		private apOptUpdateChecker.UpdateToken _updateToken = null;

		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public float _bakeScale = 0.01f;//Bake시 0.01을 곱한다.

		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public float _bakeZSize = 1.0f;//<<현재 Depth에 따라 1 차이를 준다.



		//이미지 저장 경로를 저장하자
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public string _imageFilePath_Thumbnail = "";

		[NonSerialized]
		public Texture2D _thumbnailImage = null;




		public enum SHADER_TYPE
		{
			/// <summary>(Default) Alpha Blended Interpolation</summary>
			AlphaBlend = 0,
			/// <summary>Additive</summary>
			Additive = 1,
			/// <summary>Soft Additive</summary>
			SoftAdditive = 2,
			/// <summary>2X Multiplicative</summary>
			Multiplicative = 3
		}


		//물리 옵션 - Editor / Opt (기본값은 On)
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public bool _isPhysicsPlay_Editor = true;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public bool _isPhysicsSupport_Editor = true;//<<옵션과 관계없이 지금 물리를 지원하는가


		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public bool _isPhysicsPlay_Opt = true;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public int _updateCount = 0;

		//추가 20.7.9 : 물리에서 사용되는 타이머를 여기서 공통으로 만들어서 사용한다.
		//Opt 포함
		[NonSerialized, NonBackupField]
		private System.Diagnostics.Stopwatch _physicsTimer = null;
		[NonSerialized, NonBackupField]
		private float _physicsDeltaTime = 0.0f;
		public float PhysicsDeltaTime { get { return _physicsDeltaTime; } }
		private const float PHYSICS_MAX_DELTA_TIME = 0.05f;//20FPS보다 낮은 FPS에서는 물리 시간이 고정이다.
		private const float PHYSICS_SKIP_DELTA_TIME = 1.5f;//지나치게 FPS가 낮거나 앱이 중단되었다면 해당 프레임에서는 물리 시간을 0으로 만들어야 한다.


		//물리에 주는 외력을 관리하는 객체
		//저장되는 값은 없고, API만 제공한다.
		//Editor/Runtime 모두 사용 가능
		private apForceManager _forceManager = new apForceManager();

		/// <summary>Manager controlling physical effects</summary>
		public apForceManager ForceManager { get { return _forceManager; } }

		/// <summary>[Please do not use it]</summary>
		[HideInInspector, NonBackupField]
		public GameObject _bakeUnlinkedGroup = null;

		/// <summary>
		/// [Please do not use it]
		/// Instead of setting this variable, use function "SetImportant(bool isImportant)" instead.
		/// </summary>
		[SerializeField]
		public bool _isImportant = true;


		//자동 시작하는 AnimClipID
		//-1이면 자동으로 시작되는 AnimClip은 없다.
		/// <summary>[Please do not use it]</summary>
		[SerializeField]
		public int _autoPlayAnimClipID = -1;

		//Inititialize 직후에 이 값을 True로 한다.
		//Show RootUnit에서 이 값이 True일때 자동 재생을 검토한다. (단 한번 실행된다.)
		[NonSerialized]
		private bool _isAutoPlayCheckable = false;


		//최적화된 Portrait
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector]
		public bool _isOptimizedPortrait = false;//<<이게 True이면 에디터로 작업할 수 없다.

		//기본 Portrait라면..
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector, NonBackupField]//Mono 타입으로 저장은 하되 백업은 안됨
		public apPortrait _bakeTargetOptPortrait = null;//Opt Target Bake시 타겟이 되는 Portrait

		//최적화 Portrait라면..
		/// <summary>[Please do not use it]</summary>
		[SerializeField, HideInInspector, NonBackupField]//Mono 타입으로 저장은 하되 백업은 안됨
		public apPortrait _bakeSrcEditablePortrait = null;//Opt Target Bake시 (자신이 OptPortrait 일때) 그 소스가 되는 Portrait (타겟이 불확실할 경우 경고 메시지를 주기 위함)


		//추가 3.22 : SortingLayer 관련
		//모든 Mesh는 동일한 Sorting Layer Name/Order를 가진다.
		//Bake할 때 그 값이 같아야 한다.
		[SerializeField]
		public int _sortingLayerID = 0;

		[SerializeField]
		public int _sortingOrder = 0;

		//추가 19.8.18 : Sorting Option이 나뉜다.
		public enum SORTING_ORDER_OPTION
		{
			SetOrder = 0,               //[기본값] _sortingOrder를 공통적으로 적용
			Disabled = 1,               //Sorting Order를 적용하지 않음
			DepthToOrder = 2,           //Depth의 값을 Order로 적용 (실시간)
			ReverseDepthToOrder = 3,    //(Max-Depth)의 값을 Order로 적용 (실시간)
		}
		[SerializeField]
		public SORTING_ORDER_OPTION _sortingOrderOption = SORTING_ORDER_OPTION.SetOrder;

		//추가 21.1.31		
		[SerializeField]
		public int _sortingOrderPerDepth = 1;//Depth마다 SortingOrder가 몇씩 증가하는가 (최소 1)



		// 추가 4.26 : Mecanim 설정
		[SerializeField]
		public bool _isUsingMecanim = false;

		[SerializeField]
		public string _mecanimAnimClipResourcePath = "";

		//추가 6.7
		//Empty State용 Animation Clip을 만든다.
		//Motion이 비어있으면 제어가 안된다.
		[SerializeField, NonBackupField]//백업은 안된다.
		public AnimationClip _emptyAnimClipForMecanim = null;

		//연결된 메카님 정보
		[SerializeField, NonBackupField]//백업은 안된다.
		public Animator _animator = null;



		//AnimClip 의 Asset과 연결 데이터
		//[SerializeField, NonBackupField]
		//public List<apAnimMecanimData_AssetPair> _animClipAssetPairs = new List<apAnimMecanimData_AssetPair>();

		//메카님 레이어 정보 (Blend 포함)
		[SerializeField, NonBackupField]
		public List<apAnimMecanimData_Layer> _animatorLayerBakedData = new List<apAnimMecanimData_Layer>();

		[SerializeField, NonBackupField]
		public List<apPSDSet> _bakedPsdSets = new List<apPSDSet>();

		//추가 22.6.22 : 보조 텍스쳐 생성용 데이터
		[SerializeField, NonBackupField]
		public List<apPSDSecondarySet> _bakedPsdSecondarySet = new List<apPSDSecondarySet>();

		//추가 9.19
		//빌보드 처리를 위한 카메라
		public enum BILLBOARD_TYPE
		{
			None = 0,
			Billboard = 1,
			UpFixed = 2
		}
		[SerializeField, HideInInspector]
		public BILLBOARD_TYPE _billboardType = BILLBOARD_TYPE.None;

		//추가 v1.5.0 : 빌보드시 부모의 Rotation을 적용할지 여부.
		public enum BILLBOARD_PARENT_ROTATION : int
		{	
			Ignore = 0,
			//단순히 부모의 Rotation을 더한다. (Pitch-Yaw-Roll 처럼 동작한다.)
			PitchYawRoll = 1,
			//Up 벡터만 같도록 회전시킨다.
			SyncUpVector = 2,
		}

		[SerializeField]
		public BILLBOARD_PARENT_ROTATION _billboardParentRotation = BILLBOARD_PARENT_ROTATION.Ignore;



		[NonSerialized]
		public Transform _transform = null;

		#region [미사용 코드] 이전 코드 : 단일 카메라만 지원
		//[NonSerialized]
		//private Camera _curCamera = null;
		//[NonSerialized]
		//private Transform _curCameraTransform = null;
		//[NonSerialized]
		//private float _zDepthOnPerspectiveCam = 0.0f; 
		#endregion

		//변경 : 1개 또는 다수의 카메라를 자동으로 탐색하여 처리
		[NonSerialized]
		private apOptMainCamera _mainCamera = null;

		//추가 19.9.24 : 멀티 카메라를 지원하기 위한 옵션 (VR인 경우에만 지원한다.)
		public enum VR_SUPPORT_MODE
		{
			None = 0,
			SingleCamera = 1,
			MultiCamera = 2,
		}
		[SerializeField]
		public VR_SUPPORT_MODE _vrSupportMode = VR_SUPPORT_MODE.None;

		//VR이 켜진 경우 렌더 텍스쳐의 크기 (최적화가 안되므로)
		public enum VR_RT_SIZE
		{
			ByMeshSettings = 0,
			ByEyeTextureSize = 1
		}

		[SerializeField]
		public VR_RT_SIZE _vrRenderTextureSize = VR_RT_SIZE.ByMeshSettings;

		//추가 v1.5.0 : 카메라 자동 갱신 모드
		//기본적으론 현재 카메라가 유효하지 않게 된 경우만 갱신을 하는데, 옵션에 따라선 항상 모든 씬 카메라를 모니터링한다.
		//이 옵션은 Bake로 설정할 수 있고, 스크립트로 변경 가능하다.
		/// <summary>
		/// How to check camera changes
		/// </summary>
		public enum CAMERA_CHECK_MODE : int
		{
			/// <summary>
			/// Cameras are checked again only if the current camera becomes invalid. (default)
			/// </summary>
			CurrentCameraMainly = 0,
			/// <summary>
			/// Always check all cameras in the scene.
			/// </summary>
			AllSceneCameras = 1,
		}
		[SerializeField] public CAMERA_CHECK_MODE _cameraCheckMode = CAMERA_CHECK_MODE.CurrentCameraMainly;



		//추가 19.9.24 : 빌보드인 경우, 카메라의 SortMode를 강제로 Orthographic 고정할 것인지 여부 (기본값은 True)
		[SerializeField]
		public bool _isForceCamSortModeToOrthographic = true;

		//추가 9.25 : 그림자 생성 모드 > 유니티 Enum( UnityEngine.Rendering.ShadowCastingMode과 동일하지만 일부러 따로 만듬
		public enum SHADOW_CASTING_MODE
		{
			Off = 0, On = 1, TwoSided = 2, ShadowsOnly = 3
		}
		[SerializeField] public SHADOW_CASTING_MODE _meshShadowCastingMode = SHADOW_CASTING_MODE.Off;
		[SerializeField] public bool _meshReceiveShadow = false;


		//추가 v1.5.0 : Light Probe/Reflection Probe 옵션
		public enum LIGHT_PROBE_USAGE : int
		{
			Off = 0,
			BlendProbes = 1,
			UseProxyVolume = 2,
			CustomProvided = 3
		}
		[SerializeField] public LIGHT_PROBE_USAGE _meshLightProbeUsage = LIGHT_PROBE_USAGE.Off;//기본값은 끄기

		public enum REFLECTION_PROBE_USAGE : int
		{
			Off = 0,
			BlendProbes = 1,
			BlendProbesAndSkybox = 2,
			Simple = 3
		}
		[SerializeField] public REFLECTION_PROBE_USAGE _meshReflectionProbeUsage = REFLECTION_PROBE_USAGE.Off;


		//Unity 2017 이상 : Timeline 연동 : 별도의 함수 없이 바로 시작
#if UNITY_2017_1_OR_NEWER
		[Serializable]
		public class TimelineTrackPreset
		{
			[SerializeField, NonBackupField]
			public PlayableDirector _playableDirector;

			[SerializeField, NonBackupField]
			public string _trackName;

			[SerializeField, NonBackupField]
			public int _layer = 0;

			[SerializeField, NonBackupField]
			public apAnimPlayUnit.BLEND_METHOD _blendMethod = apAnimPlayUnit.BLEND_METHOD.Additive;
		}
		[SerializeField, NonBackupField]
		public TimelineTrackPreset[] _timelineTrackSets;
#endif


		//추가 3.29 : 에디터의 Hierarchy에서 보여지는 순서에 대한 ID 리스트 클래스
		[SerializeField, NonBackupField]
		public apObjectOrders _objectOrders = new apObjectOrders();

		//추가 19.5.26 : ModMeshSet을 사용한 "v1.1.7에 적용된용량 최적화 빌드가 되었는가"
		[SerializeField, NonBackupField]
		public bool _isSizeOptimizedV117 = false;


		//추가 19.6.2 : MaterialSet를 저장하자. Bake시 이용함.
		[SerializeField]
		public List<apMaterialSet> _materialSets = new List<apMaterialSet>();


		//20.8.11 : 메시의 플립 체크시, "리깅 본을 검사"하는 항목을 추가할 수 있다.
		//- 리깅 여부 관계없이 체크 (Check regardless of Rigging) : 대신 다소 느려질 수 있음
		//- 리깅된 메시는 제외 (Excluding mesh with Rigging applied)
		public enum FLIPPED_MESH_CHECK : int
		{
			/// <summary>트랜스폼만 체크한다.(기본값)</summary>
			TransformOnly = 0,
			/// <summary>부모 트랜스폼과 리깅 본을 모두 체크</summary>
			All = 1,

		}
		[SerializeField]
		public FLIPPED_MESH_CHECK _flippedMeshOption = FLIPPED_MESH_CHECK.TransformOnly;

		//20.8.5 본과 자식 메시 그룹의 행렬 계산 모드를 결정할 수 있다.
		public enum ROOT_BONE_SCALE_METHOD : int
		{
			Default = 0,
			SkewScale = 1
		}

		[SerializeField]
		public ROOT_BONE_SCALE_METHOD _rootBoneScaleMethod = ROOT_BONE_SCALE_METHOD.Default;


		//추가 22.5.15 : 애니메이션 전환시, 지정되지 않은 컨트롤 파라미터 애니메이션의 값을 "기본값"으로 할지 "마지막 값을 유지할지" 옵션
		public enum UNSPECIFIED_ANIM_CONTROL_PARAM : int
		{
			RevertToDefaultValue = 0,
			KeepLastValue = 1,
		}
		[SerializeField, HideInInspector]
		public UNSPECIFIED_ANIM_CONTROL_PARAM _unspecifiedAnimControlParamOption = UNSPECIFIED_ANIM_CONTROL_PARAM.RevertToDefaultValue;//기본값 : CP 기본값으로 복원



		//추가 20.9.14 : 만약 작업 전에 프리팹으로서 생성된 객체였다면,
		//Diconnect 하기 전에 "프리팹으로서의 Root Game Object"와 "프리팹 에셋"을 "복구용"으로 지정하자
		//Optimized 객체도 별도로 프리팹이 될 수 있으므로, 따로 작업한다.
		//이값은 에디터에서 처음 조회할 때 갱신한다.
		//[Legacy] : Disconnect 하더라도 프리팹 정보가 남아있다. 복구시 이 변수들이 필요 없을 수 있다.
		//[2018.3] : Disconnect 하면 프리팹 정보가 완전히 사라진다. 복구시 이 변수들이 꼭 필요하며, Inspector에서 이 정보도 날릴 수 있다.

		[SerializeField, NonBackupField, HideInInspector]
		public GameObject _rootGameObjectAsPrefabInstanceForRestore = null;

		[SerializeField, NonBackupField, HideInInspector]
		public UnityEngine.Object _srcPrefabAssetForRestore = null;

		//추가 20.9.15 : 이전 프레임에서의 위치와 현재 프레임의 InvRotation Matrix를 계산하자.
		//빌보드 처리를 위함
		[NonSerialized]
		private Vector3 _posW_Prev1F = Vector3.zero;

		[NonSerialized]
		private Matrix4x4 _rotationOnlyMatrixIfBillboard = Matrix4x4.identity;

		[NonSerialized]
		private Matrix4x4 _invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;



		//추가 21.6.7 : 다른 Portrait에 연동해서 재생할 수 있다.
		//애니메이션, 컨트롤 파라미터 따로 연동할 수 있다.
		[NonSerialized]
		private bool _isSyncParent = false;//다른 Portrait가 이 Portrait에 자식으로서 동기화 되었는가

		[NonSerialized]
		private List<apPortrait> _syncChildPortraits = null;//동기화된 자식 Portrait들

		[NonSerialized]
		private bool _isSyncChild = false;//다른 Portrait에 애니메이션이나 컨트롤 파라미터가 연동된다.

		//이전
		//private enum SYNC_METHOD
		//{
		//	None,
		//	/// <summary>애니메이션만 동기화된다.</summary>
		//	AnimationOnly,
		//	/// <summary>컨트롤 파라미터만 동기화된다.</summary>
		//	ControlParamOnly,
		//	/// <summary>애니메이션과 컨트롤 파라미터가 동기화된다.</summary>
		//	AnimationAndControlParam
		//}
		//[NonSerialized]
		//private SYNC_METHOD _syncMethod = SYNC_METHOD.None;

		//변경 21.9.18 : 각각의 요청에 따른 동기화를 각각의 변수에 저장하자
		[NonSerialized]
		private bool _isSync_Animation = false;
		[NonSerialized]
		private bool _isSync_ControlParam = false;
		[NonSerialized]
		private bool _isSync_Bone = false;
		[NonSerialized]
		private bool _isSync_RootUnit = false;


		[NonSerialized]
		private apPortrait _syncParentPortrait = null;

		//애니메이션, 컨트롤 파라미터 동기화 처리
		[NonSerialized]
		private apSyncPlay _syncPlay = null;


		//추가 21.10.7 : 시간 옵션
		//스크립트를 이용해서 어느 시간을 사용할지 결정한다. (배속)
		public enum DELTA_TIME_OPTION : int
		{
			DeltaTime,
			UnscaledDeltaTime,
			MultipliedDeltaTime,
			MultipliedUnscaledDeltaTime,
			//추가 22.1.8 : 함수를 이용해서 현재 시간을 받자
			CustomFunction,
		}

		private bool _isDeltaTimeOptionChanged = false;
		private DELTA_TIME_OPTION _deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;//기본값. Init할 때 초기화
		private float _deltaTimeMultiplier = 1.0f;//배속.
		private float _tCurUpdate = 0.0f;//실제 업데이트 값
										 //커스텀 방식으로 조회할 때
		public delegate float OnDeltaTimeRequested(object savedObject);
		private OnDeltaTimeRequested _funcDeltaTimeRequested = null;
		private object _deltaTimeRequestSavedObject = null;//업데이트 시간 콜백시 특수한 처리를 위한 값



		//추가 21.12.22 : 재질 병합 기능을 사용하는가
		//재질 병합은 "주도적으로 하는 Portrait"와 그것에 연결된 다른 Portrait들을 대상으로 한다.
		//함수는 "주도적으로 하는 Portrait" 위주로 한다.
		[NonSerialized]
		private bool _isUseMergedMat = false;
		[NonSerialized]
		private apPortrait _mergeMatMainPortrait = null;
		[NonSerialized]
		private List<apPortrait> _mergedMatSubPortraits = null;
		[NonSerialized]
		public apOptMergedMaterial _optMergedMaterial = null;

		//추가 22.7.7 : 불연속적인 이동(일명 텔레포트)을 하는 경우, 물리 효과 (지글본, 물리 재질)가 갑자기 튀는 경우가 있다.
		//옵션에 따라서는 텔레포트시 해당 프레임에서는 물리 효과의 "이전 위치" 계산을 생략해야한다.
		/// <summary>텔레포트 발생시 물리 처리 보정</summary>
		[SerializeField] public bool _isTeleportCorrectionOption = false;
		/// <summary>텔레포트인지 판정하는 이동 거리</summary>
		[SerializeField] public float _teleportMovementDist = 10.0f;
		
		//[v1.5.0] 텔레포트 옵션에 크기/회전 추가하고, 각각 체크 여부 옵션가짐		
		[SerializeField] public float _teleportRotationOffset = 30.0f;
		[SerializeField] public float _teleportScaleOffset = 0.2f;
		[SerializeField] public bool _teleportPositionEnabled = true;
		[SerializeField] public bool _teleportRotationEnabled = true;
		[SerializeField] public bool _teleportScaleEnabled = true;

		[NonSerialized] private bool _isTeleportChecked = false;//이전에 텔레포트가 체크되었는가.
		[NonSerialized] private Vector3 _teleportCheck_PosPrev = Vector3.zero;//이전 프레임에서의 텔레포트

		//[v1.5.0]
		[NonSerialized] private Vector3 _teleportCheck_ScalePrev = Vector3.one;//[v1.5.0] 텔레포트의 스케일 체크
		[NonSerialized] private Vector3 _teleportCheck_RotationPrev = Vector3.zero;//[v1.5.0] 텔레포트의 회전 체크


		[NonSerialized] public bool _isCurrentTeleporting = false;//현재 프레임에서 텔레포트가 발생했는가
		[NonSerialized] private int _preventPhysicsCount = 0;//v1.4.9 : 특정 상황에서 일정 프레임동안 물리가 비활성

		[NonSerialized] public bool _isPhysicsEnabledInPrevFrame = false;//이전 프레임에서 물리가 유효하게 동작했는가


		//추가 [v1.4.7]
		//루트 유닛이 전환되는 경우 물리 효과가 튀면 안된다.
		[NonSerialized] public bool _isCurrentRootUnitChanged = false;


		// 추가 v1.4.7
		// 메시 갱신 빈도를 제어해서 낮은 프레임에서 동작하는 것처럼 보일 수 있다.
		public enum MESH_UPDATE_FREQUENCY : int
		{
			EveryFrames = 0,//기본값
			FixedFrames_NotSync = 1,//동기화 되지 않은 고정 프레임
			FixedFrames_Sync = 2,//동기화된 고정 프레임
		}
		[SerializeField] public MESH_UPDATE_FREQUENCY _meshRefreshRateOption = MESH_UPDATE_FREQUENCY.EveryFrames;

		//추가 v1.4.9
		//메시 갱신 빈도가 TimeScale에 영향을 받을지 여부
		//- TimeScale에 영향을 받지 않으면 화면 전체 갱신 비율이 일관되게 낮아지는 효과
		//- TimeScale에 영향르 받으면 가속된 애니메이션에 맞추어 갱신 FPS도 빨라져서 고정 FPS 애니메이션으로 보이는 효과
		public enum FIXED_UPDATE_FPS_SCALE_OPTION : int
		{
			/// <summary>낮은 FPS로 업데이트될 때, FPS는 변하지 않는다. (기본값)</summary>
			Fixed = 0,
			/// <summary>낮은 FPS로 업데이트될 때, FPS가 TimeScale에 영향을 받는다.</summary>
			ChangedByTimeScale = 1,
		}
		[SerializeField] public FIXED_UPDATE_FPS_SCALE_OPTION _meshRefreshFPSScaleOption = FIXED_UPDATE_FPS_SCALE_OPTION.Fixed;
	


		/// <summary>
		/// 메시 갱신 방식이 고정 프레임인 경우의 FPS (최소 1, 최대 60)
		/// </summary>
		[SerializeField] public int _meshRefreshRateFPS = 24;
		[NonSerialized] private bool _isMeshRefreshFrame = false;//현재 프레임이 메시를 갱신하는 프레임인가?
		[NonSerialized] private float _tMeshRefreshTimer = 0.0f;
		private const int MESH_REFRESH_FPS_MIN = 1;
		private const int MESH_REFRESH_FPS_MAX = 30;//최대 30FPS (그 이상은 60 FPS 기준으로 "연속 재생 <-> 끊김"의 반복이다)

		//Important가 꺼진 경우의 업데이트 정보
		[NonSerialized] private bool _isNotImportant_UpdateFrame = false;//이게 True면 No-Important의 경우에 갱신되는 프레임이다.
		[NonSerialized] private float _tNotImportant_Elapsed = 0.0f;



		// 추가 v1.4.8 : 업데이트 시점
		public enum PROCESS_EVENT_ON : int
		{
			LateUpdate = 0,//이게 기본값
			Update = 1,//이건 Execution Order를 수정해야한다.
		}
		[SerializeField] public PROCESS_EVENT_ON _mainProcessEvent = PROCESS_EVENT_ON.LateUpdate;

		// 추가 v1.4.8 : 루트 모션
		public enum ROOT_MOTION_MODE : int
		{
			None = 0,
			/// <summary>루트본이 부모 객체의 중심에 위치하도록 설정한다.</summary>
			LockToCenter = 1,
			/// <summary>루트본의 움직임을 대상 Transform에 적용한다.</summary>
			MoveParentTransform = 2
		}
		//루트모션 옵션. 이 값이 실제로 반영될지는 다른 조건을 확인해야 하며, 업데이트시엔 _rootMotionValidatedMode를 이용하자
		[SerializeField] public ROOT_MOTION_MODE _rootMotionModeOption = ROOT_MOTION_MODE.None;


		//이동/고정 옵션 (축별로)
		//이동이 아니더라도 LockToCenter에서도 적용된다.
		public enum ROOT_MOTION_MOVE_TYPE_PER_AXIS : int
		{
			/// <summary>이 축에 대해서는 루트 모션을 사용하지 않음</summary>
			Disabled = 0,
			/// <summary>해당 축에 대해서 위치 고정 및 이동이 된다.</summary>
			PositionOnly = 1,
			/// <summary>해당 축에 대해서 위치 보정 및 속도 보정이 이루어진다. (기본 옵션)</summary>
			PositionAndVelocity = 2,
		}
		[SerializeField] public ROOT_MOTION_MOVE_TYPE_PER_AXIS _rootMotionAxisOption_X = ROOT_MOTION_MOVE_TYPE_PER_AXIS.PositionAndVelocity;
		[SerializeField] public ROOT_MOTION_MOVE_TYPE_PER_AXIS _rootMotionAxisOption_Y = ROOT_MOTION_MOVE_TYPE_PER_AXIS.PositionAndVelocity;

		//루트 모션이 활성화되려면
		//- 루트 유닛마다 루트 본이 지정되어 있어야 한다.
		//- 움직임이 적용되거나 Local Pos > Zero의 기준이 되는 Parent가 있어야 한다.

		//적용되는 Parent Transform에 대한 옵션
		public enum ROOT_MOTION_TARGET_TRANSFORM : int
		{
			/// <summary>바로 위의 Parent. 별도로 지정하지 않아도 된다.</summary>
			Parent = 0,
			/// <summary>직접 지정하는 Transform. 이것도 부모 조건은 충족해야한다.</summary>
			SpecifiedTransform = 1,
		}
		[SerializeField] public ROOT_MOTION_TARGET_TRANSFORM _rootMotionTargetTransformType = ROOT_MOTION_TARGET_TRANSFORM.Parent;
		[SerializeField, NonBackupField] public Transform _rootMotionSpecifiedParentTransform = null;

		
		
		

		//조건을 체크하여 실제로 RootMotion이 어떻게 동작하는지의 모드 (유효성 검사 결과)
		[NonSerialized] public ROOT_MOTION_MODE _rootMotionValidatedMode = ROOT_MOTION_MODE.None;
		[NonSerialized] public Transform _rootMotionValidated_ParentTransform = null;//바로 위 부모 Transform
		[NonSerialized] public Transform _rootMotionValidated_BaseTransform = null;//부모 또는 더 위 부모 Transform. 위치 이동이 적용된다.
		[NonSerialized] private apOptRootUnit.FUNC_ROOTMOTION_UPDATE _funcRootMotionEvent = null;

		//Base Transform이 가진 컴포넌트에 따라서 위치가 적용되는 방식이 다르다.
		public enum ROOT_MOTION_BASE_COMPONENT
		{
			Transform, Rigidbody2D, Rigidbody3D
		}
		[NonSerialized] private ROOT_MOTION_BASE_COMPONENT _rootMotionValidated_BaseComponentType = ROOT_MOTION_BASE_COMPONENT.Transform;
		[NonSerialized] private Rigidbody2D _rootMotionValidated_BaseCom_Rigidbody2D = null;
		[NonSerialized] private Rigidbody _rootMotionValidated_BaseCom_Rigidbody3D = null;

		//업데이트에서 발생한 루트 모션 이동 변위 요청
		[NonSerialized] private bool _rootMotion_IsRequestedMove = false;
		[NonSerialized] private Vector3 _rootMotion_RequestedPos = Vector3.zero;


		//추가 [v1.5.0] IK 처리 알고리즘 - 에디터/런타임 별개 지정 가능
		public enum IK_METHOD : int
		{
			FABRIK = 0,//v1.5.0에서 추가된 버전. 꼬임 현상이 적고 속도가 빠르다.
			CCD = 1,//이전 버전의 IK 처리 방식
		}

		//v1.5.0 추가 : IK 처리 방식 (런타임)
		[SerializeField] public IK_METHOD _IKMethod = IK_METHOD.CCD;//기본은 이전 버전에서 제작한 사람들을 위해서 기본값은 CCD 유지. 대신 새로 생성할땐 FABRIK을 지정한다.

		//v1.5.0 추가 : 보이지 않는 메시를 업데이트하기 여부 (물리 움직임의 일관성을 위함)
		public enum INVISIBLE_MESH_UPDATE : int
		{
			//업데이트하지 않음 (기본값)
			NotUpdate = 0,
			//보이지 않더라도 업데이트를 한다. (물리 효과를 자주 사용하는 경우에 선택 가능)
			Update = 1,
		}
		[SerializeField] public INVISIBLE_MESH_UPDATE _invisibleMeshUpdate = INVISIBLE_MESH_UPDATE.NotUpdate;


		//v1.5.1 추가 : 클리핑 메시 업데이트 타이밍을 변경할 수 있다.
		public enum CLIPPING_MESH_UPDATE : int
		{
			/// <summary>Update에서 처리한다. (기본값)</summary>
			InUpdateRoutine = 0,
			/// <summary>렌더링 직전에 처리한다. (비동기 시스템 대비용)</summary>
			BeforeRendering = 1,
		}

		[SerializeField] public CLIPPING_MESH_UPDATE _clippingMeshUpdate = CLIPPING_MESH_UPDATE.InUpdateRoutine;





		// Init
		//-----------------------------------------------------
		void Awake()
		{

			if (Application.isPlaying)
			{
				if (_FPS < 10)
				{
					_FPS = 10;
				}
				//_isImportant = true;

				//이전 > UpdateToken으로 변경
				//_timePerFrame = 1.0f / (float)_FPS;
				//_tDelta = _timePerFrame * UnityEngine.Random.Range(0.0f, 1.0f);


				if (_initStatus == INIT_STATUS.Ready)
				{
					//_initStatus = INIT_STATUS.Ready;
					_funcAyncLinkCompleted = null;
				}
			}
		}



		void Start()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
#endif
				if (_FPS < 10)
				{
					_FPS = 10;
				}

				//이전 > UpdateToken 방식으로 변경
				//_timePerFrame = 1.0f / (float)_FPS;
				//_tDelta = _timePerFrame * UnityEngine.Random.Range(0.0f, 1.0f);


				if (_initStatus == INIT_STATUS.Ready)
				{
					Initialize();

					//자동으로 시작을 해보자
					//ShowRootUnit(); //<< Initialize에 이미 ShowRootUnit이 포함되어 있다.

					_updateCount = 0;
					//_updateKeyIndex = 0;
				}

				_controller.InitRequest();
#if UNITY_EDITOR
			}
#endif

		}

		//삭제될 때
		void OnDestroy()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
#endif

				//추가 21.12.22 : 재질 병합시, 병합을 먼저 해제해야한다.
				if (_isUseMergedMat)
				{
					UnmergeMaterials();
				}


				//생성된 재질을 삭제하자.
				apOptSharedMaterial.I.OnPortraitDestroyed(this);
				_optBatchedMaterial.Clear(true);
#if UNITY_EDITOR
			}
#endif
		}




		//Enable / Disable 이벤트
		private void OnEnable()
		{
			//v1.5.1 : Clipping 메시 업데이트 (Unity 2017부터)
#if UNITY_2017_1_OR_NEWER
			if(_clippingMeshUpdate == CLIPPING_MESH_UPDATE.BeforeRendering)
			{
#if UNITY_EDITOR
				if(Application.isPlaying)
				{
#endif
					Application.onBeforeRender -= OnUpdateMaskMeshBeforeRendering;
					Application.onBeforeRender += OnUpdateMaskMeshBeforeRendering;

#if UNITY_EDITOR
				}
#endif
			}
#endif
		}


		private void OnDisable()
		{
			//v1.5.1 : Clipping 메시 업데이트 해제
#if UNITY_2017_1_OR_NEWER
			if(_clippingMeshUpdate == CLIPPING_MESH_UPDATE.BeforeRendering)
			{
#if UNITY_EDITOR
				if(Application.isPlaying)
				{
#endif
					Application.onBeforeRender -= OnUpdateMaskMeshBeforeRendering;
#if UNITY_EDITOR
				}
#endif
			}
#endif
		}

		// Update
		//-----------------------------------------------------
		void Update()
		{

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif	

#if UNITY_EDITOR
			try
			{	
#endif
				if (_initStatus != INIT_STATUS.Completed)
				{
					//로딩이 다 되지 않았다면 처리를 하지 않는다.
					return;
				}

				// < Update 코드 >
				//- Main Process 전에 호출되어야 하는 코드들 실행한다.
				//- Sync Child에서는 호출되지 않는다.
				//- 업데이트 타이밍 계산을 위해 "Update" 단계에서 꼭 실행되어야 하는 함수들이 실행된다.
				if (!_isSyncChild)
				{
					if (_isImportant)
					{
						//[ Important가 활성화된 경우 ]
						if (_meshRefreshRateOption == MESH_UPDATE_FREQUENCY.FixedFrames_Sync)
						{
							//매시 갱신 빈도가 동기화된 낮은 프레임이라면
							apOptFixedFrameChecker.I.OnUpdate(_meshRefreshRateFPS);
						}
					}
					else
					{
						//[ Important가 비활성화된 경우 ]
						//이전
						//_updateToken = apOptUpdateChecker.I.AddRequest(_updateToken, _FPS, Time.unscaledDeltaTime);

						//v1.4.8 변경 : 이제 OnUpdate에서 바로 IsUpdate/ElapsedTime이 계산이 된다.
						_updateToken = apOptUpdateChecker.I.OnUpdate(_updateToken, _FPS, Time.unscaledDeltaTime);
					}
				}
					
				// < Main Process - (Update 옵션) >
				//- Main Process를 "Update" 함수에서 실행하도록 옵션이 설정된 경우
				if(_mainProcessEvent == PROCESS_EVENT_ON.Update)
				{
					MainProcess();
				}	


#if UNITY_EDITOR
			}
			catch (Exception ex)
			{
				Debug.LogError("AnyPortrait Update Error", this.gameObject);
				Debug.LogException(ex, this.gameObject);
			}
#endif
		}



		void LateUpdate()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}

			try
			{
#endif
				if (_initStatus != INIT_STATUS.Completed)
				{
					//로딩이 다 되지 않았다면 처리를 하지 않는다.
					return;
				}

				// < Main Process - (LateUpdate 옵션) >
				// Main Process를 "LateUpdate" 함수에서 실행하도록 옵션이 설정된 경우
				if (_mainProcessEvent == PROCESS_EVENT_ON.LateUpdate)
				{
					MainProcess();
				}



				// < LateUpdate 코드 >
				//- 메인 프로세스와 별개인 후처리를 진행한다.
				//- 타이머 객체의 LateUpdate 함수를 호출한다.
				//- Post Update (위치 갱신 등)을 호출한다.
				if(_isImportant)
				{
					//Important + FixedFrames_Sync일때의 타이머 이벤트
					if(_meshRefreshRateOption == MESH_UPDATE_FREQUENCY.FixedFrames_Sync)
					{
						//Late Update에서 호출을 하자
						apOptFixedFrameChecker.I.OnLateUpdate(_meshRefreshRateFPS);
					}
				}
				else
				{
					//Late Update에서 토큰 갱신
					apOptUpdateChecker.I.OnLateUpdate();
				}

				//업데이트가 끝나고 위치 정보 등을 저장한다.
				PostUpdate();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait LateUpdate Error", this.gameObject);
				Debug.LogException(ex, this.gameObject);
			}
#endif

			#region [미사용 코드]
			////추가 21.4.3 : 출력할게 없다면 스크립트를 중단한다.
			//if (_curPlayingOptRootUnit == null)
			//{
			//	_prevOptRootUnit = null;
			//	return;
			//}

			////추가 21.6.8 : 동기화되어서 수동적으로 동작해야한다면 여기서 업데이트를 하지 않는다. (부모가 업데이트 함수를 호출해줘야 한다.)
			//if (_isSyncChild)
			//{
			//	return;
			//}


			////추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
			//CalculatePhysicsTimer();


			////추가 22.7.7 : 물리 텔레포트를 보정하기 위한 감지 함수를 호출한다.
			//CheckTeleport();


			//#region [핵심 코드 >>> Update에서 넘어온 코드]
			////_tDelta += Time.deltaTime;//<<이전 방식 (Important가 아닌 경우)

			//#region [사용 : 1프레임 지연 없이 사용하는 경우. 단, 외부 처리에 대해서는 Request 방식으로 처리해야한다.]

			////추가 21.10.7 : 업데이트 시간 계산 옵션이 적용된다. 기존의 Time.deltaTime만 사용하지는 않는다.
			//switch (_deltaTimeOption)
			//{
			//	case DELTA_TIME_OPTION.DeltaTime: _tCurUpdate = Time.deltaTime; break;
			//	case DELTA_TIME_OPTION.UnscaledDeltaTime: _tCurUpdate = Time.unscaledDeltaTime; break;
			//	case DELTA_TIME_OPTION.MultipliedDeltaTime: _tCurUpdate = Time.deltaTime * _deltaTimeMultiplier; break;
			//	case DELTA_TIME_OPTION.MultipliedUnscaledDeltaTime: _tCurUpdate = Time.unscaledDeltaTime * _deltaTimeMultiplier; break;
			//	case DELTA_TIME_OPTION.CustomFunction:
			//		{
			//			//추가 22.1.8: 콜백 함수 이용
			//			if (_funcDeltaTimeRequested != null)
			//			{
			//				_tCurUpdate = _funcDeltaTimeRequested(_deltaTimeRequestSavedObject);
			//			}
			//			else
			//			{
			//				//함수가 없다면 옵션 변경
			//				Debug.Log("AnyPortrait : The callback function to get the update time was null, so it's back to the default option.");
			//				_deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;
			//				_tCurUpdate = Time.deltaTime;
			//			}
			//		}
			//		break;
			//}


			////힘 관련 업데이트
			//ForceManager.Update(_tCurUpdate);

			////애니메이션 업데이트
			//_animPlayManager.Update(_tCurUpdate);

			////추가 20.11.23 : 애니메이션 정보가 모디파이어 처리에 반영되도록 매핑 클래스를 동작시킨다.
			//_animPlayMapping.Update();


			////추가 : 애니메이션 업데이트가 끝났다면 ->
			////다른 스크립트에서 요청한 ControlParam 수정 정보를 반영한다.
			//_controller.CompleteRequests();
			//#endregion


			////if (_tDelta > _timePerFrame)
			////if(true)
			//if (_curPlayingOptRootUnit != null)
			//{
			//	//추가 9.19 : Camera 체크
			//	//if(_billboardType != BILLBOARD_TYPE.None)
			//	//{
			//	//	CheckAndRefreshCameras();
			//	//} >> 이전 : 빌보드가 아닌 경우 생략

			//	//변경 : 언제나
			//	CheckAndRefreshCameras();


			//	//전체 업데이트하는 코드
			//	//일정 프레임마다 업데이트를 한다.
			//	//#if UNITY_EDITOR
			//	//					Profiler.BeginSample("Portrait - Update Transform");
			//	//#endif
			//	if (_isImportant)
			//	{
			//		//v1.4.7 : 매시 갱신 빈도 옵션을 적용한다.
			//		_isMeshRefreshFrame = false;
			//		switch (_meshRefreshRateOption)
			//		{
			//			case MESH_UPDATE_FREQUENCY.EveryFrames:
			//				// [ 메시 갱신 빈도가 "매프레임 (기본값)"인 경우
			//				_isMeshRefreshFrame = true;//항상 갱신
			//				break;

			//			case MESH_UPDATE_FREQUENCY.FixedFrames_NotSync:
			//				{
			//					// [ 메시 갱신 빈도가 "고정 프레임 + 동기화 안됨"인 경우
			//					_tMeshRefreshTimer += Time.unscaledDeltaTime;
			//					int refreshFPS = Mathf.Clamp(_meshRefreshRateFPS, MESH_REFRESH_FPS_MIN, MESH_REFRESH_FPS_MAX);
			//					float secPerFrame = 1.0f / (float)refreshFPS;
			//					if(_tMeshRefreshTimer > secPerFrame)
			//					{
			//						_tMeshRefreshTimer -= secPerFrame;
			//						_isMeshRefreshFrame = true;
			//					}
			//				}
			//				break;

			//			case MESH_UPDATE_FREQUENCY.FixedFrames_Sync:
			//				// [ 메시 갱신 빈도가 "고정 프레임 + 동기화됨"인 경우
			//				//동기화된 타이머로부터 업데이트 여부를 받자
			//				_isMeshRefreshFrame = apOptFixedFrameChecker.I.IsUpdatable(_meshRefreshRateFPS);

			//				//Late Update에서 호출을 하자
			//				apOptFixedFrameChecker.I.OnLateUpdate(_meshRefreshRateFPS);
			//				break;
			//		}

			//		//v1.4.7
			//		if(!_isMeshRefreshFrame)
			//		{
			//			//현재 프레임이 메시가 갱신되지 않는 (=건너뛰는) 프레임인데,
			//			//다음의 경우엔 프레임 계산에 상관없이 무조건 메시를 갱신해야한다.
			//			//- Root Unit이 변경된 경우 > 여기서 체크한다.
			//			//- 상하좌우 플립이 된 경우 > UpdateTransforms의 내부의 CheckFlippedTransform() 구문에서 체크한다.

			//			//Root Unit이 변경된 경우
			//			if(_prevOptRootUnit != _curPlayingOptRootUnit)
			//			{
			//				_isMeshRefreshFrame = true;
			//			}
			//		}

			//		//v1.4.7 변경 : UpdateTransform에 _isMeshRefreshFrame를 입력해서 메시 갱신 여부를 전달하자.
			//		_curPlayingOptRootUnit.UpdateTransforms(_tCurUpdate, _isMeshRefreshFrame);

			//	}
			//	else
			//	{	
			//		//중앙에서 관리하는 토큰 업데이트

			//		//이전
			//		//if (apOptUpdateChecker.I.GetUpdatable(_updateToken))
			//		//변경 v1.4.8 : Update에서 생성/갱신된 토큰의 값을 바로 사용한다.
			//		bool isUpdatedFrame = _updateToken != null && _updateToken.IsUpdatable;

			//		if (isUpdatedFrame)
			//		{
			//			_curPlayingOptRootUnit.UpdateTransforms(_updateToken.ResultElapsedTime, true);
			//		}
			//		else
			//		{
			//			_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
			//		}

			//		//메시 갱신 변수 자체는 매프레임 동작하게 만든다. (마스크등의 이슈로)
			//		_isMeshRefreshFrame = true;


			//		//추가 v1.4.8 : 업데이트 여부에 상관없이 토큰 후처리를 위해 호출한다.
			//		apOptUpdateChecker.I.OnLateUpdate();
			//	}

			//}

			//PostUpdate();//추가 20.9.15 : 현재 프레임의 위치등을 저장하자.

			////추가 21.6.8 : 동기화된 객체라면, 자식 객체들의 업데이트를 대신 해주자
			//if (_isSyncParent)
			//{
			//	int nChildPortrait = _syncChildPortraits.Count;
			//	apPortrait childPortrait = null;
			//	bool isAnyRemovedPortrait = false;
			//	for (int i = 0; i < nChildPortrait; i++)
			//	{
			//		childPortrait = _syncChildPortraits[i];
			//		if (childPortrait == null)
			//		{
			//			isAnyRemovedPortrait = true;
			//			continue;
			//		}

			//		//업데이트를 대신 호출해준다.
			//		childPortrait.UpdateAsSyncChild(_tCurUpdate, _isMeshRefreshFrame);
			//	}
			//	if (isAnyRemovedPortrait)
			//	{
			//		//알게모르게 삭제된게 있었다;
			//		//리스트에서 제거해주자
			//		_syncChildPortraits.RemoveAll(delegate (apPortrait a)
			//		{
			//			return a == null;
			//		});

			//		//만약 모두 삭제되었다.
			//		if (_syncChildPortraits.Count == 0)
			//		{
			//			//동기화 해제
			//			_isSyncParent = false;
			//			_syncChildPortraits = null;
			//		}
			//	}
			//}
			//#endregion


			////v1.4.7 : Cur > Prev로 OptRootUnit 저장
			//_prevOptRootUnit = _curPlayingOptRootUnit; 
			#endregion
		}




		private void FixedUpdate()
		{
			//v1.4.8 : 루트모션으로 RigidBody를 움직이는 경우는 FixedUpdate에서 처리해야한다.
			if(_rootMotionValidatedMode == ROOT_MOTION_MODE.MoveParentTransform
				&& _rootMotion_IsRequestedMove)
			{
				ProcessRootMotion_FixedUpdate();
			}
		}









		// 추가 v1.4.8 : 옵션에 따라서 메인 업데이트 함수의 위치를 정할 수 있다.
		//- Update / Main Process / Late Update 코드가 분리된다.
		//- Update도 시간 관련 코드를 호출한다.
		//- LateUpdate의 코드는 Main Process과 무관한 업데이트 시간 관련 코드만 포함된다. (PostUpdate 포함)
		//- Sync Child는 Update/LateUpdate에서 Post Update 외에는 호출하지 않는다. (업데이트 시간 코드도)
		//- Update + Main Process에 타이밍 로직이 있으므로, Sync Child는 부모의 업데이트 타이밍을 그대로 받아야 한다.
		private void MainProcess()
		{
			//추가 22.5.18 : 스크립트에 의한 애니메이션 요청을 처리한다.
			if (_animPlayDeferredRequest.IsAnyRequests())
			{
				_animPlayDeferredRequest.ProcessRequests();
			}

			// 업데이트 Delta Time 계산
			CalculateDeltaTime();

			//추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
			CalculatePhysicsTimer();

			//추가 22.7.7 : 물리 텔레포트를 보정하기 위한 감지 함수를 호출한다.
			CheckTeleport();

			//메시 갱신 프레임 / 간헐적 업데이트 여부를 계산한다.
			CalculateRefreshFrame();

			//추가 21.6.8
			//동기화되어서 수동적으로 동작해야한다면 여기서 업데이트를 하지 않는다.
			//(부모가 업데이트 함수를 호출해줘야 한다.)
			if (_isSyncChild)
			{
				return;
			}

			//추가 21.4.3 : 출력할게 없다면 스크립트를 중단한다.
			if (_curPlayingOptRootUnit == null)
			{
				//만약 동기화 업데이트라면, 중단하기 전에 동기화된 자식 객체를 업데이트를 해야한다.
				if(_isSyncParent)
				{
					UpdateSyncedChildren();
				}

				_prevOptRootUnit = null;
				return;
			}

			//힘 관련 업데이트
			ForceManager.Update(_tCurUpdate);

			//애니메이션 업데이트
			_animPlayManager.Update(_tCurUpdate);

			//추가 20.11.23 : 애니메이션 정보가 모디파이어 처리에 반영되도록 매핑 클래스를 동작시킨다.
			_animPlayMapping.Update();

			//추가 : 애니메이션 업데이트가 끝났다면 ->
			//다른 스크립트에서 요청한 ControlParam 수정 정보를 반영한다.
			_controller.CompleteRequests();

			if (_curPlayingOptRootUnit != null)
			{
				//Camera 체크
				CheckAndRefreshCameras();

				//전체 업데이트하는 코드
				//일정 프레임마다 업데이트를 한다.
				if (_isImportant)
				{
					// [ Important : 매프레임 업데이트 ]
					//애니메이션 업데이트로 인해서 Root Unit이 바뀌었다면 메시를 갱신해야한다.
					//v1.4.7
					if(!_isMeshRefreshFrame)
					{
						//현재 프레임이 메시가 갱신되지 않는 (=건너뛰는) 프레임인데,
						//다음의 경우엔 프레임 계산에 상관없이 무조건 메시를 갱신해야한다.
						//- Root Unit이 변경된 경우 > 여기서 체크한다.
						//- 상하좌우 플립이 된 경우 > UpdateTransforms의 내부의 CheckFlippedTransform() 구문에서 체크한다.
						
						//Root Unit이 변경된 경우
						if(_prevOptRootUnit != _curPlayingOptRootUnit)
						{
							_isMeshRefreshFrame = true;
						}
					}

					//v1.4.7 변경 : UpdateTransform에 _isMeshRefreshFrame를 입력해서 메시 갱신 여부를 전달하자.
					_curPlayingOptRootUnit.UpdateTransforms(	_tCurUpdate,
																_isMeshRefreshFrame,
																_funcRootMotionEvent);
					
				}
				else
				{	
					// [ Not Important : 간헐적 업데이트 ]
					//중앙에서 관리하는 토큰 업데이트
					if (_isNotImportant_UpdateFrame)
					{
						//현재 업데이트되는 프레임이다.
						_curPlayingOptRootUnit.UpdateTransforms(_tNotImportant_Elapsed, true, _funcRootMotionEvent);
					}
					else
					{
						//업데이트되지 않는 프레임이다.
#if UNITY_2017_1_OR_NEWER
						//2017부터는 옵션에 따라 Mask Mesh 갱신 여부 설정 (렌더링 전에 시행할 수도 있다.)
						if(_clippingMeshUpdate == CLIPPING_MESH_UPDATE.InUpdateRoutine)
						{
							_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
						}
#else
						//2017 전에는 옵션 무관하게 여기서 바로 Mask Mesh 갱신
						_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
#endif				
					}
				}
			}


			//동기화된 부모라면 자식 객체를 업데이트하자
			if(_isSyncParent)
			{
				UpdateSyncedChildren();
			}


			//v1.4.7 : Cur > Prev로 OptRootUnit 저장
			_prevOptRootUnit = _curPlayingOptRootUnit;
		}


		// 업데이트시의 Delta Time을 계산한다.
		private void CalculateDeltaTime()
		{
			switch (_deltaTimeOption)
			{	
				case DELTA_TIME_OPTION.UnscaledDeltaTime:
					_tCurUpdate = Time.unscaledDeltaTime;
					break;

				case DELTA_TIME_OPTION.MultipliedDeltaTime:
					_tCurUpdate = Time.deltaTime * _deltaTimeMultiplier;
					break;
					
				case DELTA_TIME_OPTION.MultipliedUnscaledDeltaTime:
					_tCurUpdate = Time.unscaledDeltaTime * _deltaTimeMultiplier;
					break;

				case DELTA_TIME_OPTION.CustomFunction:
					{
						//추가 22.1.8: 콜백 함수 이용
						if (_funcDeltaTimeRequested != null)
						{
							_tCurUpdate = _funcDeltaTimeRequested(_deltaTimeRequestSavedObject);
						}
						else
						{
							//함수가 없다면 옵션 변경
							Debug.Log("AnyPortrait : The callback function to get the update time was null, so it's back to the default option.");
							_deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;
							_tCurUpdate = Time.deltaTime;
						}
					}
					break;

				case DELTA_TIME_OPTION.DeltaTime:
				default:
					_tCurUpdate = Time.deltaTime;
					break;
			}
		}

		/// <summary>
		/// Important 옵션을 고려하여 메시가 갱신되는 프레임 또는 No-Important의 업데이트 프레임을 구하자
		/// </summary>
		private void CalculateRefreshFrame()
		{
			//메시 갱신과 관련된 값들
			_isMeshRefreshFrame = false;

			//Important가 아닌 경우
			_isNotImportant_UpdateFrame = false;
			_tNotImportant_Elapsed = 0.0f;

			if (_isImportant)
			{
				// [ Important : 매 프레임 업데이트를 한다. ]
				// 모디파이어는 항상 업데이트 되지만, 메시에는 옵션에 따라 반영이 안될 수도 있다.

				//v1.4.7 : 매시 갱신 빈도 옵션을 적용한다.
				
				switch (_meshRefreshRateOption)
				{
					case MESH_UPDATE_FREQUENCY.EveryFrames:
						// [ 메시 갱신 빈도가 "매프레임 (기본값)"인 경우
						_isMeshRefreshFrame = true;//항상 갱신
						break;

					case MESH_UPDATE_FREQUENCY.FixedFrames_NotSync:
						{
							// [ 메시 갱신 빈도가 "고정 프레임 + 동기화 안됨"인 경우

							//[v1.4.8] 옵션에 따라 타이머가 다르게 동작한다.
							if(_meshRefreshFPSScaleOption == FIXED_UPDATE_FPS_SCALE_OPTION.Fixed)
							{
								//고정이라면 > Unscaled Delta Time을 기준으로 타이머 게산
								_tMeshRefreshTimer += Time.unscaledDeltaTime;
							}
							else
							{
								//v1.4.8에서 추가
								//가변이라면 > CalculateDeltaTime() 함수에서 연산된 Delta Time을 이용하여 타이머 계산
								_tMeshRefreshTimer += _tCurUpdate;
							}

							
							int refreshFPS = Mathf.Clamp(_meshRefreshRateFPS, MESH_REFRESH_FPS_MIN, MESH_REFRESH_FPS_MAX);
							float secPerFrame = 1.0f / (float)refreshFPS;
							if(_tMeshRefreshTimer > secPerFrame)
							{
								_tMeshRefreshTimer -= secPerFrame;
								_isMeshRefreshFrame = true;
							}
						}
						break;

					case MESH_UPDATE_FREQUENCY.FixedFrames_Sync:
						// [ 메시 갱신 빈도가 "고정 프레임 + 동기화됨"인 경우
						//동기화된 타이머로부터 업데이트 여부를 받자
						_isMeshRefreshFrame = apOptFixedFrameChecker.I.IsUpdatable(_meshRefreshRateFPS);
						break;
				}

				//v1.4.7
				if(!_isMeshRefreshFrame)
				{
					//현재 프레임이 메시가 갱신되지 않는 (=건너뛰는) 프레임인데,
					//다음의 경우엔 프레임 계산에 상관없이 무조건 메시를 갱신해야한다.
					//- Root Unit이 변경된 경우 > 여기서 체크한다.
					//- 상하좌우 플립이 된 경우 > UpdateTransforms의 내부의 CheckFlippedTransform() 구문에서 체크한다.
						
					//Root Unit이 변경된 경우
					if(_prevOptRootUnit != _curPlayingOptRootUnit)
					{
						_isMeshRefreshFrame = true;
					}
				}	
			}
			else
			{	
				// [ Important OFF : 간헐적으로 업데이트를 한다.  ]
				//모디파이어와 달리 메시는 매번 갱신한다. (마스크 이슈 때문에)
				//중앙에서 관리하는 토큰 업데이트

				//이전
				//if (apOptUpdateChecker.I.GetUpdatable(_updateToken))
				//변경 v1.4.8 : Update에서 생성/갱신된 토큰의 값을 바로 사용한다.
				_isNotImportant_UpdateFrame = _updateToken != null ? _updateToken.IsUpdatable : false;
				_tNotImportant_Elapsed = _isNotImportant_UpdateFrame ? _updateToken.ResultElapsedTime : 0.0f;

				//메시 갱신 변수 자체는 매프레임 동작하게 만든다. (마스크등의 이슈로)
				_isMeshRefreshFrame = true;
			}
		}



		//v1.4.8 추가 : 루트 모션 처리
		private void ProcessRootMotion(apOptRootUnit curRootUnit)
		{
			//1. 루트 모션 옵션을 확인한다.
			if(_rootMotionValidatedMode == ROOT_MOTION_MODE.None)
			{
				return;
			}

			Transform curParent = _transform.parent;
			if(curParent == null
				|| _rootMotionValidated_ParentTransform == null
				|| _rootMotionValidated_ParentTransform != curParent)
			{
				//Parent가 없다면 실패
				return;
			}

			

			if(_curPlayingOptRootUnit == null || _curPlayingOptRootUnit != curRootUnit)
			{
				//현재 루트 유닛이 없다면 실패
				return;
			}

			//1. Root Bone을 찾자
			apOptBone curRootBone = _curPlayingOptRootUnit.RootMotionBone;

			if(curRootBone == null)
			{
				//루트 본이 없다.
				return;
			}

			Vector2 rootBonePosL = curRootBone._worldMatrix.Pos;//본의 좌표 (Local이다)
			Vector2 rootBonePosL_NoMod = curRootBone._worldMatrix_NonModified.Pos;//만약 Modifier가 없었다면

			//Self(apPortrait)와 Root Bone의 위치 차이 만큼 apPortrait를 역방향으로 움직인다.
			//위치를 같은 좌표계로 비교해야한다.

			//Root Unit은 Scale이 들어갔고 나머지 위치는 항상 원점이다.
			//하위 오브젝트들 ( Bone 포함)도 Identity이므로, RootUnit의 Transform에서 World 좌표를 구하자
			
			Vector3 rootBonePosW = _curPlayingOptRootUnit._transform.TransformPoint(new Vector3(rootBonePosL.x, rootBonePosL.y, 0.0f));
			
			
			//World 상에서 이동을 한다.
			//Parent 기준으로 Root Bone이 중심에 달라붙도록 apPortrait를 움직인다.
			//위치 변위량만큼 역이동하는게 아니라 현재 위치 차이를 적용하는 것
			Vector3 portraitPosW_Prev = _transform.position;
			
			Vector3 rootBonePosL_OfParent = curParent.InverseTransformPoint(rootBonePosW);

			//Parent 기준으로 Root Bone이 원점으로 가도록 apPortrait를 이동시킨다.
			//[축별로 적용]
			Vector3 curLocalPosition = _transform.localPosition;

			if(_rootMotionAxisOption_X != ROOT_MOTION_MOVE_TYPE_PER_AXIS.Disabled)
			{
				//X축 옵션이 활성화되어 있다면
				curLocalPosition.x += -1.0f * rootBonePosL_OfParent.x;
			}
			if(_rootMotionAxisOption_Y != ROOT_MOTION_MOVE_TYPE_PER_AXIS.Disabled)
			{
				//Y축 옵션이 활성화되어 있다면
				curLocalPosition.y += -1.0f * rootBonePosL_OfParent.y;
			}

			//< Lock To Center 처리 결과 적용 >
			_transform.localPosition = curLocalPosition;


			//여기까지 하면 "Lock to Center" 옵션은 종료
			if (_rootMotionValidatedMode != ROOT_MOTION_MODE.MoveParentTransform
				|| _rootMotionValidated_BaseTransform == null)
			{
				return;
			}

			//옵션이 Parent Transform이라면
			//Portrait의 이동 변위의 역 만큼 Base Transform을 이동시켜야 한다.
			//(Lock된 변위만큼 이동해야 겉보기에 똑같아진다)
			Vector3 portraitPosW_Next = _transform.position;
			
			//루트 모션에서 프레임 진행에 따른 Mod-Pos의 변위의 합을 정리했다. 이 값을 이용하자
			Vector2 rootMotionDeltaPos = _curPlayingOptRootUnit.GetRootMotionDeltaPos();//루트 모션에 적용하기 위한 루트 본의 이번 프레임 변위.역으로 계산해서 이동 변위를 구하자

			//축에 따라서 이동값 제한
			if(_rootMotionAxisOption_X == ROOT_MOTION_MOVE_TYPE_PER_AXIS.Disabled)
			{
				//X축으로는 Delta Pos 적용 안하는 경우
				rootMotionDeltaPos.x = 0.0f;
			}

			if(_rootMotionAxisOption_Y == ROOT_MOTION_MOVE_TYPE_PER_AXIS.Disabled)
			{
				//Y축으로는 Delta Pos 적용 안하는 경우
				rootMotionDeltaPos.y = 0.0f;
			}

			Vector3 rootBoneDeltaPos = _curPlayingOptRootUnit._transform.TransformVector(new Vector3(rootMotionDeltaPos.x, rootMotionDeltaPos.y, 0.0f));

			

			

			//<옵션>
			//1. Physics (2D/3D)라면 > RigidBody에 넣어야 한다.

			switch (_rootMotionValidated_BaseComponentType)
			{
				case ROOT_MOTION_BASE_COMPONENT.Rigidbody2D:
					{
						if(_rootMotionValidated_BaseCom_Rigidbody2D != null)
						{
							//RigidBody 2D에 위치 적용 > Fixed Update로 예약
							_rootMotion_IsRequestedMove = true;
							_rootMotion_RequestedPos += rootBoneDeltaPos;
						}
						else
						{
							//(Fallback) Transform에 적용
							_rootMotionValidated_BaseTransform.position = _rootMotionValidated_BaseTransform.position + rootBoneDeltaPos;
						}
					}
					break;

				case ROOT_MOTION_BASE_COMPONENT.Rigidbody3D:
					{
						if(_rootMotionValidated_BaseCom_Rigidbody3D != null)
						{
							//RigidBody 3D에 위치 적용 > Fixed Update로 예약
							_rootMotion_IsRequestedMove = true;
							_rootMotion_RequestedPos += rootBoneDeltaPos;
						}
						else
						{
							//(Fallback) Transform에 적용
							_rootMotionValidated_BaseTransform.position = _rootMotionValidated_BaseTransform.position + rootBoneDeltaPos;
						}
					}
					break;

				case ROOT_MOTION_BASE_COMPONENT.Transform:
					{
						//Transform에 적용
						_rootMotionValidated_BaseTransform.position = _rootMotionValidated_BaseTransform.position + rootBoneDeltaPos;
					}
					break;
			}

		}

		/// <summary>
		/// Fixed Update에서 실행되는 루트 모션의 처리.
		/// 로트 모션의 대상이 "RigidBody (2D/3D)"인 경우엔 Fixed Update에서 수행되어야 한다.
		/// </summary>
		private void ProcessRootMotion_FixedUpdate()
		{
			if(_rootMotionValidatedMode != ROOT_MOTION_MODE.MoveParentTransform
				|| !_rootMotion_IsRequestedMove)
			{
				return;
			}

			//다만, Position은 순간이동 등에 사용되는 것이므로, DeltaPos가 너무 커서는 안된다.
			//기준은 1
			if(Mathf.Abs(_rootMotion_RequestedPos.x) > 1.0f
				|| Mathf.Abs(_rootMotion_RequestedPos.y) > 1.0f
				|| Mathf.Abs(_rootMotion_RequestedPos.z) > 1.0f)
			{
				//Delta값이 너무 크다면 순간이동 효과가 발생한다.
				//1 미만으로 만들어야 한다.
				//Debug.Log("너무 큰 이동 : " + _rootMotion_RequestedPos + " > " + _rootMotion_RequestedPos.normalized);
				_rootMotion_RequestedPos.Normalize();
				
			}

			//Delta Pos의 값의 축별 할당은 ProcessRootMotion에서 수행했다.
			//여기서는 속도 보정 옵션을 축별로 어떻게 할 것인지만 체크한다.
			bool isVelocityCorrection_X = (_rootMotionAxisOption_X == ROOT_MOTION_MOVE_TYPE_PER_AXIS.PositionAndVelocity);
			bool isVelocityCorrection_Y = (_rootMotionAxisOption_Y == ROOT_MOTION_MOVE_TYPE_PER_AXIS.PositionAndVelocity);

			//속도 보정 옵션
			//- 현재 속도와 Delta에 따른 예상 속도의 차이가 너무 크다면 보정하는 기능
			//- 원래 현재 속도는 "메인의 오브젝트 속도" + "루트 본의 움직임에 따른 순간 속도"의 합이어야 한다.
			//- 메인의 오브젝트 속도가 중요하므로, 사실 루트 본의 움직임의 속도가 메인이 되어서는 안된다.
			
			//- 문제가 되는 경우는 다음과 같다.
			//  > 중력이 적용하여 아래로 속력이 높은 상태에서 위로 점프하는 모션이 있어도 충분히 반영되지 않는다. (루트본 방향과 속력의 차이가 크고 방향이 반대)
			//  > 앞으로 이동하는데 속도가 앞으로 크게 향하지 않아서 이동이 버벅거린다.
			//  > (부작용-1) 앞으로의 이동에 속도를 보정하니 이동이 멈춰도 슬라이딩이 발생한다.
			//  > (부작용-2) 감속 보정을 하니 서있는 상태에서는 속도가 0으로 보정되어버려서 움직이질 못한다.
			
			//- 즉, 루트 본의 이동 속력을 강하게 보정하면 "슬라이딩(가속)/이동 못함(감속)" 문제가 발생해버린다.
			//- "속도의 방향이 반대"이며, "속도 차이가 너무 커서 이동을 제대로 표현 못할 정도"인 경우에 보간(30%)을 수행한다.
			//- 속도 차이는 경험값에 의해서 결정된다.

			//노트 : MovePosition는 사용하지 않는다. 중력이 무시되기 때문

			float tDelta = Time.fixedDeltaTime;			

			switch (_rootMotionValidated_BaseComponentType)
			{
				case ROOT_MOTION_BASE_COMPONENT.Rigidbody2D:
					{
						if(_rootMotionValidated_BaseCom_Rigidbody2D != null)
						{	
							Vector2 nextPos = _rootMotionValidated_BaseCom_Rigidbody2D.position;
							nextPos += new Vector2(_rootMotion_RequestedPos.x, _rootMotion_RequestedPos.y);

							if((isVelocityCorrection_X || isVelocityCorrection_Y) && tDelta > 0.0f)
							{
								Vector3 expectedVel = _rootMotion_RequestedPos / tDelta;
								expectedVel.x = Mathf.Clamp(expectedVel.x, -100.0f, 100.0f);
								expectedVel.y = Mathf.Clamp(expectedVel.y, -100.0f, 100.0f);

								
								//현재 속도
								//[v1.5.1] Unity 6용 분기
#if UNITY_6000_0_OR_NEWER
								Vector2 curVelocity = _rootMotionValidated_BaseCom_Rigidbody2D.linearVelocity;
#else
								Vector2 curVelocity = _rootMotionValidated_BaseCom_Rigidbody2D.velocity;
#endif

								

								//[X 축 - 옵션에 따라 조정]
								if(isVelocityCorrection_X)
								{
									//- 루트본의 기대 속도와 현재 속도의 방향이 반대다.
									//- 절대값으로 현재 속도가 기대 속도보다 더 크다
									//- 속도의 차이는 경험값으로 2 차이 이상 차이가 난다. (너무 작은 값 사이에서 반대 부호간 보간을 하면 0이 되버리므로)
									if(expectedVel.x * curVelocity.x < 0.0f
										&& Mathf.Abs(curVelocity.x) > Mathf.Abs(expectedVel.x)
										&& Mathf.Abs(curVelocity.x - expectedVel.x) > 2.0f)
									{
										//루트본의 기대 속도보다 현재 속도의 크기가 더 커서 루트본의 움직임을 제대로 표현할 수 없다.
										//보간을 수행한다. (30%)
										curVelocity.x = (expectedVel.x * 0.3f) + (curVelocity.x * 0.7f);
									}
								}

								//[Y 축 - 옵션에 따라 조정]
								if(isVelocityCorrection_Y)
								{
									//- 루트본의 기대 속도와 현재 속도의 방향이 반대다.
									//- 절대값으로 현재 속도가 기대 속도보다 더 크다
									//- 속도의 차이는 경험값으로 2 차이 이상 차이가 난다. (너무 작은 값 사이에서 반대 부호간 보간을 하면 0이 되버리므로)
									if(expectedVel.y * curVelocity.y < 0.0f
										&& Mathf.Abs(curVelocity.y) > Mathf.Abs(expectedVel.y)
										&& Mathf.Abs(curVelocity.y - expectedVel.y) > 2.0f)
									{
										//루트본의 기대 속도보다 현재 속도의 크기가 더 커서 루트본의 움직임을 제대로 표현할 수 없다.
										//보간을 수행한다. (30%)
										curVelocity.y = (expectedVel.y * 0.3f) + (curVelocity.y * 0.7f);
									}
								}

								//속도 보정
								//[v1.5.1] Unity 6용 분기
#if UNITY_6000_0_OR_NEWER
								_rootMotionValidated_BaseCom_Rigidbody2D.linearVelocity = curVelocity;
#else
								_rootMotionValidated_BaseCom_Rigidbody2D.velocity = curVelocity;
#endif
								
								
							}

							//위치 이동
							_rootMotionValidated_BaseCom_Rigidbody2D.position = nextPos;
						}
					}
					break;

				case ROOT_MOTION_BASE_COMPONENT.Rigidbody3D:
					{
						if(_rootMotionValidated_BaseCom_Rigidbody3D != null)
						{
							Vector3 nextPos = _rootMotionValidated_BaseCom_Rigidbody3D.position;
							nextPos += _rootMotion_RequestedPos;

							if((isVelocityCorrection_X || isVelocityCorrection_Y) && tDelta > 0.0f)
							{
								Vector3 expectedVel = _rootMotion_RequestedPos / tDelta;
								expectedVel.x = Mathf.Clamp(expectedVel.x, -100.0f, 100.0f);
								expectedVel.y = Mathf.Clamp(expectedVel.y, -100.0f, 100.0f);
								expectedVel.z = Mathf.Clamp(expectedVel.z, -100.0f, 100.0f);

								//현재 속도
								//[v1.5.1] Unity 6용 분기
#if UNITY_6000_0_OR_NEWER
								Vector3 curVelocity = _rootMotionValidated_BaseCom_Rigidbody3D.linearVelocity;
#else
								Vector3 curVelocity = _rootMotionValidated_BaseCom_Rigidbody3D.velocity;
#endif


								//[X/Z 축 - 옵션에 따라 조정] > 3D에서는 Z 축도 X옵션에 포함시킨다.
								if(isVelocityCorrection_X)
								{
									//X축 보간
									if(expectedVel.x * curVelocity.x < 0.0f
										&& Mathf.Abs(curVelocity.x) > Mathf.Abs(expectedVel.x)
										&& Mathf.Abs(curVelocity.x - expectedVel.x) > 2.0f)
									{
										//보간을 수행한다. (30%)
										curVelocity.x = (expectedVel.x * 0.3f) + (curVelocity.x * 0.7f);
									}

									//Z축 보간
									if(expectedVel.z * curVelocity.z < 0.0f
										&& Mathf.Abs(curVelocity.z) > Mathf.Abs(expectedVel.z)
										&& Mathf.Abs(curVelocity.z - expectedVel.z) > 2.0f)
									{
										//보간을 수행한다. (30%)
										curVelocity.z = (expectedVel.z * 0.3f) + (curVelocity.z * 0.7f);
									}
								}

								//[Y 축 - 옵션에 따라 조정]
								if(isVelocityCorrection_Y)
								{
									if (expectedVel.y * curVelocity.y < 0.0f
										&& Mathf.Abs(curVelocity.y) > Mathf.Abs(expectedVel.y)
										&& Mathf.Abs(curVelocity.y - expectedVel.y) > 2.0f)
									{
										//루트본의 기대 속도와 현재 속도의 방향이 반대다.
										curVelocity.y = (expectedVel.y * 0.3f) + (curVelocity.y * 0.7f);
									}
								}

								//속도 보정
								//[v1.5.1] Unity 6용 분기
#if UNITY_6000_0_OR_NEWER
								_rootMotionValidated_BaseCom_Rigidbody3D.linearVelocity = curVelocity;
#else
								_rootMotionValidated_BaseCom_Rigidbody3D.velocity = curVelocity;
#endif
								
							}

							//위치 이동
							_rootMotionValidated_BaseCom_Rigidbody3D.position = nextPos;
						}
					}
					break;
			}

			_rootMotion_IsRequestedMove = false;
			_rootMotion_RequestedPos = Vector3.zero;
		}





		public void UpdateForce()
		{
			//강제로 업데이트를 한다.
#if UNITY_EDITOR
			try
			{
#endif
				if (_initStatus == INIT_STATUS.Ready)
				{
					Initialize();
				}


				if (_initStatus != INIT_STATUS.Completed)
				{
					//로딩이 끝나지 않았다면 처리를 하지 않는다.
					return;
				}


				//추가 22.5.18 : 스크립트에 의한 애니메이션 요청을 처리한다.
				if (_animPlayDeferredRequest.IsAnyRequests())
				{
					_animPlayDeferredRequest.ProcessRequests();
				}

				//추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
				CalculatePhysicsTimer();

				//추가 22.7.7 : 물리 텔레포트를 보정하기 위한 감지 함수를 호출한다.
				CheckTeleport();



				if (_animPlayManager.IsPlaying_Editor)
				{
#if UNITY_EDITOR
					_animPlayManager.Update_Editor(0.0f);
					Debug.Log("Update - Editor");
#endif
				}
				else
				{
					if (_curPlayingOptRootUnit != null)
					{

						//힘 관련 업데이트
						//ForceManager.Update(0.0f);

						//애니메이션 업데이트
						//_animPlayManager.Update(0.0f);
						//에디터 미리보기 용으로 처리해야한다.
						_animPlayManager.Update_InspectorPreview();

						//추가 20.11.23 : 애니메이션 정보가 모디파이어 처리에 반영되도록 매핑 클래스를 동작시킨다.
						_animPlayMapping.Update();

						_curPlayingOptRootUnit.UpdateTransforms(0.0f, true, null);
					}
				}

				//일정 프레임마다 업데이트를 한다.
				//_optRootUnit.UpdateTransforms(_tDelta);

				PostUpdate();//추가 20.9.15 : 현재 프레임의 위치등을 저장하자.

				//v1.4.7 : Prev에 현재 OptRootUnit 저장
				_prevOptRootUnit = _curPlayingOptRootUnit;
#if UNITY_EDITOR
			}
			catch (Exception ex)
			{
				Debug.LogError("Portrait Exception : " + ex.ToString());
			}
#endif
		}



		//동기화된 다른 객체들을 업데이트한다.
		private void UpdateSyncedChildren()
		{
			if(!_isSyncParent)
			{
				return;
			}
			int nChildPortrait = _syncChildPortraits.Count;
			apPortrait childPortrait = null;
			bool isAnyRemovedPortrait = false;
			
			for (int i = 0; i < nChildPortrait; i++)
			{
				childPortrait = _syncChildPortraits[i];
				if (childPortrait == null)
				{
					isAnyRemovedPortrait = true;
					continue;
				}

				//업데이트를 대신 호출해준다.
				childPortrait.UpdateAsSyncChild(	_tCurUpdate,
													_isMeshRefreshFrame,
													_isImportant,
													_isNotImportant_UpdateFrame,
													_tNotImportant_Elapsed);
			}

			if (isAnyRemovedPortrait)
			{
				//알게모르게 삭제된게 있었다;
				//리스트에서 제거해주자
				_syncChildPortraits.RemoveAll(delegate (apPortrait a)
				{
					return a == null;
				});

				//만약 모두 삭제되었다.
				if (_syncChildPortraits.Count == 0)
				{
					//동기화 해제
					_isSyncParent = false;
					_syncChildPortraits = null;
				}
			}
		}



		//추가 21.6.8
		//애니메이션이나 컨트롤 파라미터가 다른 Portrait에 동기화된 경우, 부모 apPortrait로부터 업데이트를 대신 호출받는다.

		//변경 v1.4.8 : 부모의 Important여부 및 Important가 아닌 경우의 업데이트 여부/경과 시간이 추가되었다.
		public void UpdateAsSyncChild(	float updateTime,
										bool isMeshRefreshFrame,
										//Parent의 Important 속성을 따른다.
										bool isParentImportant,

										//Parent가 Not-Important인 경우
										bool isNotImportantUpdateFrame,//이 프레임이 업데이트 되는 프레임인가
										float notImportantElapsedTime
										) 
		{
			if (!_isSyncChild || _syncPlay == null)
			{
				return;
			}


			if (_initStatus != INIT_STATUS.Completed)
			{
				//로딩이 다 되지 않았다면 처리를 하지 않는다.
				return;
			}

			//추가 21.4.3 : 출력할게 없다면 스크립트를 중단한다.
			if (_curPlayingOptRootUnit == null)
			{
				_prevOptRootUnit = null;
				return;
			}

			//추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
			CalculatePhysicsTimer();

			//추가 22.7.7 : 물리 텔레포트를 보정하기 위한 감지 함수를 호출한다.
			CheckTeleport();

			_tCurUpdate = updateTime;

			//힘 관련 업데이트
			ForceManager.Update(_tCurUpdate);

			//애니메이션 업데이트 <동기화>

			//이전
			//if(_syncMethod == SYNC_METHOD.AnimationOnly
			//	|| _syncMethod == SYNC_METHOD.AnimationAndControlParam)

			if (_isSync_Animation)//변경 21.9.18
			{
				//동기화된 업데이트
				_animPlayManager.UpdateAsSyncChild(_tCurUpdate, _syncPlay);
			}
			else
			{
				//일반 업데이트
				_animPlayManager.Update(_tCurUpdate);
			}


			//추가 20.11.23 : 애니메이션 정보가 모디파이어 처리에 반영되도록 매핑 클래스를 동작시킨다.
			_animPlayMapping.Update();


			//컨트롤 파라미터 동기화를 하자 <동기화>
			//if(_syncMethod == SYNC_METHOD.ControlParamOnly
			//	|| _syncMethod == SYNC_METHOD.AnimationAndControlParam)

			if (_isSync_ControlParam)//변경 21.9.18
			{
				_syncPlay.SyncControlParams();
			}

			//다른 스크립트에서 요청한 ControlParam 수정 정보를 반영한다.
			_controller.CompleteRequests();


			//추가 21.9.21
			if (_isSync_RootUnit)
			{
				//Root Unit 동기화를 하자
				_syncPlay.SyncRootUnit();
			}


			if (_curPlayingOptRootUnit != null)
			{
				//TODO : 이전과 RootUnit이 다르다면 물리 식이 변경되어야 한다.


				//추가 9.19 : 빌보드, Camera 체크
				CheckAndRefreshCameras();


				//전체 업데이트하는 코드
				//일정 프레임마다 업데이트를 한다.
				//if (_isImportant)//이전
				if(isParentImportant)//변경 v1.4.8 : 부모의 Important 설정을 따라간다.
				{
					if(!isMeshRefreshFrame)
					{
						//만약 Root Unit이 변경될 때는 무조건 한번 갱신을 해야한다. [v1.4.7]
						if(_curPlayingOptRootUnit != _prevOptRootUnit)
						{
							isMeshRefreshFrame = true;
						}
					}

					//_curPlayingOptRootUnit.UpdateTransforms(Time.deltaTime);//일반
					_curPlayingOptRootUnit.UpdateTransformsAsSyncChild(_tCurUpdate, _isSync_Bone, isMeshRefreshFrame, _funcRootMotionEvent);//동기화용
				}
				else
				{
					//새로운 방식 : 중앙에서 관리하는 토큰 업데이트

					//이전
					//if (apOptUpdateChecker.I.GetUpdatable(_updateToken))
					//변경 v1.4.8 : Update에서 생성/갱신된 토큰의 값을 바로 사용한다.
					//bool isUpdatedFrame = _updateToken != null && _updateToken.IsUpdatable;

					//부모의 Not-Important 설정을 이용한다.

					if (isNotImportantUpdateFrame)
					{	
						_curPlayingOptRootUnit.UpdateTransformsAsSyncChild(notImportantElapsedTime, _isSync_Bone, true, _funcRootMotionEvent);//동기화용
					}
					else
					{
#if UNITY_2017_1_OR_NEWER
						//2017부터는 옵션에 따라 Mask Mesh 갱신 여부 설정 (렌더링 전에 시행할 수도 있다.)
						if(_clippingMeshUpdate == CLIPPING_MESH_UPDATE.InUpdateRoutine)
						{
							_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
						}
#else
						//2017 전에는 옵션 무관하게 여기서 바로 Mask Mesh 갱신
						_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
#endif
					}
				}

			}

			PostUpdate();//추가 20.9.15 : 현재 프레임의 위치등을 저장하자.

			//v1.4.7
			_prevOptRootUnit = _curPlayingOptRootUnit;
		}



		//추가 22.7.7 : 물리 텔레포트 업데이트
		private void CheckTeleport()
		{
			_isCurrentTeleporting = false;//일단 false

			//v1.4.2 : 텔레포트 보정 옵션에 관계없이 이전 프레임에 물리 연산 기록이 없다면 이건 텔레포트된 프레임과 동일하게 처리해야한다.
			if(!_isPhysicsEnabledInPrevFrame)
			{
				//물리가 이전 프레임에서 발생하지 않았다면 이건 텔레포트 프레임이다. [물리가 활성화되어 있어도 이전 프레임 기준으로 체크]
				_isCurrentTeleporting = true;
				
			}

			if(_preventPhysicsCount > 0)
			{
				//Debug.Log("물리 방지 카운트 [" + _preventPhysicsCount + "]");
				_isCurrentTeleporting = true;
				_preventPhysicsCount -= 1;
			}

			if (_isTeleportCorrectionOption
				&& _transform != null
				&& (_teleportPositionEnabled || _teleportRotationEnabled || _teleportScaleEnabled)//세부 옵션이 있다.
				)
			{
				//텔레포트 보정 옵션이 켜져있고 텔레포트를 체크할 수 있는 상황이라면
				//v1.5.0 : 옵션에 따라 위치, 회전, 크기 비교를 한다.
				if(_teleportPositionEnabled)
				{
					// [ 위치 비교 ]
					Vector3 curPos = _transform.position;

					if (_isTeleportChecked)
					{
						//이전의 위치가 저장되었다면
						if (Mathf.Abs(curPos.x - _teleportCheck_PosPrev.x) > _teleportMovementDist
							|| Mathf.Abs(curPos.y - _teleportCheck_PosPrev.y) > _teleportMovementDist
							|| Mathf.Abs(curPos.z - _teleportCheck_PosPrev.z) > _teleportMovementDist)
						{
							//텔레포트가 발생했다!
							_isCurrentTeleporting = true;
						}
					}

					//위치 저장
					_teleportCheck_PosPrev = _transform.position;
				}

				if(_teleportRotationEnabled)
				{
					// [ 회전 각도 비교 ]
					Vector3 curRotationAngle = _transform.rotation.eulerAngles;

					if (_isTeleportChecked && !_isCurrentTeleporting)
					{
						//이전의 각도가 저장되었다면 + 이전 단계에서 아직 텔레포트가 발생하지 않았다면
						if (Mathf.Abs(curRotationAngle.x - _teleportCheck_RotationPrev.x) > _teleportRotationOffset
							|| Mathf.Abs(curRotationAngle.y - _teleportCheck_RotationPrev.y) > _teleportRotationOffset
							|| Mathf.Abs(curRotationAngle.z - _teleportCheck_RotationPrev.z) > _teleportRotationOffset)
						{
							//텔레포트가 발생했다!
							_isCurrentTeleporting = true;
						}
					}

					//각도 저장
					_teleportCheck_RotationPrev = curRotationAngle;
				}

				if(_teleportScaleEnabled)
				{
					// [ 크기 비교 ] - World이므로 Lossy Scale을 체크한다.
					Vector3 curLossyScale = _transform.lossyScale;

					if (_isTeleportChecked && !_isCurrentTeleporting)
					{
						//이전의 크기가 저장되었다면 + 이전 단계에서 아직 텔레포트가 발생하지 않았다면
						if (Mathf.Abs(curLossyScale.x - _teleportCheck_ScalePrev.x) > _teleportScaleOffset
							|| Mathf.Abs(curLossyScale.y - _teleportCheck_ScalePrev.y) > _teleportScaleOffset
							|| Mathf.Abs(curLossyScale.z - _teleportCheck_ScalePrev.z) > _teleportScaleOffset)
						{
							//텔레포트가 발생했다!
							_isCurrentTeleporting = true;
						}
					}

					//크기 저장
					_teleportCheck_ScalePrev = curLossyScale;
				}

				//한번이라도 이 코드가 수행되었다면 _teleportCheck_PosPrev 등의 변수에 값이 저장되었을 것이다.
				_isTeleportChecked = true;
			}

			//물리 연산 활성화 여부를 저장한다. (다음 프레임에서 써먹기 위함)
			_isPhysicsEnabledInPrevFrame = _isPhysicsPlay_Opt;
		}



#if UNITY_2017_1_OR_NEWER
		//추가 3.9 : 타임라인을 포함한 씬에서의 시뮬레이션을 위한 함수 (초기화와 업데이트)
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void InitializeAsSimulating()
		{
			//추가) AnimPlayer를 추가했다.
			_animPlayManager.LinkPortrait(this);
		}
#endif

#if UNITY_2017_1_OR_NEWER
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="deltaTime"></param>
		public void UpdateForceAsSimulating(float deltaTime)
		{
			//강제로 업데이트를 한다.
			//시뮬레이션을 위한 것이므로, 애니메이션이 포함된다.
#if UNITY_EDITOR
			try
			{
#endif
				if (_initStatus == INIT_STATUS.Ready)
				{
					Initialize();
				}

				
				if(_initStatus != INIT_STATUS.Completed)
				{
					//로딩이 끝나지 않았다면 처리를 하지 않는다.
					return;
				}

				//추가 20.7.9 : 물리에서 공통적으로 사용할 DeltaTime을 계산한다.
				CalculatePhysicsTimer();

				//물리 업데이트
				ForceManager.Update(deltaTime);

				//애니메이션 업데이트
				_animPlayManager.Update(deltaTime);

				if (_curPlayingOptRootUnit != null)
				{
					//추가 9.19 : Camera 체크
					CheckAndRefreshCameras();
					//if(_billboardType != BILLBOARD_TYPE.None)
					//{
					//	CheckCameraAndBillboard();
					//}

					_curPlayingOptRootUnit.UpdateTransforms(0.0f, true, null
					);
				}

				//일정 프레임마다 업데이트를 한다.
				//_optRootUnit.UpdateTransforms(_tDelta);
				

#if UNITY_EDITOR
			}
			catch (Exception ex)
			{
				Debug.LogError("Portrait Exception : " + ex.ToString());
			}
#endif
		}
#endif

		//변경 20.7.9 [EnterPlayMode 기능 관련 개선 코드]
		//유니티 2019.3에서 EnterPlayMode에 들어갈 때 Domain Reload를 스킵할 수 있다.
		//문제는, Link를 하기 위해서 _initStatus가 초기화 되어야 하는 apPortrait인데, 이게 Completed 상태로 그냥 남아버릴 수 있다.
		//여러가지 처리가 있을 수 있지만, 그냥 Domain Reload가 스킵된 상태로 게임에 진입하면 모든 apPortrait를 찾아서
		//강제 초기화 함수를 실행하도록 만들자.
#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void InitForceWhenDomainReloadSkipped()
		{
			//Debug.LogWarning("InitForceWhenSkipDomainReload");
			//존재하는 모든 apPortrait를 찾는다.
			
			//v1.4.8 : Unity 2023용 코드 분기
#if UNITY_2023_1_OR_NEWER
			apPortrait[] portraitsInScene = GameObject.FindObjectsByType<apPortrait>(FindObjectsSortMode.None);
#else
			apPortrait[] portraitsInScene = GameObject.FindObjectsOfType<apPortrait>();
#endif

			if(portraitsInScene != null && portraitsInScene.Length > 0)
			{
				for (int i = 0; i < portraitsInScene.Length; i++)
				{
					portraitsInScene[i].ResetInitStatusToReadyWhenDomainReloadSkipped();
				}
			}
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		private void ResetInitStatusToReadyWhenDomainReloadSkipped()
		{
			//Debug.LogError("Init Status [" + _initStatus + " > INIT_STATUS.Ready] (" + gameObject.name + ")");
			_initStatus = INIT_STATUS.Ready;
		}
#endif



		//v1.5.1 : 클리핑 마스크 업데이트를 렌더링 직전에 수행하기
		private void OnUpdateMaskMeshBeforeRendering()
		{
			if(_initStatus != INIT_STATUS.Completed)
			{
				return;
			}
			if(_curPlayingOptRootUnit == null)
			{
				return;
			}

			_curPlayingOptRootUnit.UpdateTransformsOnlyMaskMesh();
		}


		// Event
		//-----------------------------------------------------

#if UNITY_EDITOR

		//추가 12.13
		//apOptMesh의 OnValidate에서 호출되는 함수.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void OnMeshResetInEditor()
		{


			if (Application.isEditor && !Application.isPlaying)
			{
				//19.10.26 : 빌보드를 일단 끈다.
				apPortrait.BILLBOARD_TYPE billboardType = _billboardType;

				try
				{
					if (_optMeshes != null && _optMeshes.Count > 0)
					{
						//Debug.LogError("OnMeshResetInEditor : " + this.name);

						for (int i = 0; i < _optMeshes.Count; i++)
						{
							_optMeshes[i].InitMesh(true);
							_optMeshes[i].ResetMeshAndMaterialIfMissing();
						}
						UpdateForce();
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("AnyPortrait : Refresh Meshes Failed\n" + ex);
				}

				_billboardType = billboardType;//복구

				//추가 22.1.9 : 첫번째 루트 유닛만 보여준다.
				int nOptRootUnits = _optRootUnitList != null ? _optRootUnitList.Count : 0;
				if (nOptRootUnits > 0)
				{
					ShowRootUnitWhenBake(_optRootUnitList[0]);
				}
			}
		}
#endif


		// Functions
		//-----------------------------------------------------

		/// <summary>
		/// Show one of the Root Units. 
		/// The Root Unit can have an animation clip that starts automatically, or it can be the first Root Unit.
		/// </summary>
		public void ShowRootUnit()
		{
			//RootUnit 플레이 조건
			//1. 자동 시작 AnimClip이 있다면 그걸 가지고 있는 RootUnit을 시작한다.
			//2. 없다면 0번 RootUnit을 재생

			apOptRootUnit targetOptRootUnit = null;
			apAnimClip firstPlayAnimClip = null;

			if (_isAutoPlayCheckable && _autoPlayAnimClipID >= 0)
			{
				apAnimClip curAnimClip = null;
				for (int i = 0; i < _animClips.Count; i++)
				{
					curAnimClip = _animClips[i];
					if (curAnimClip._uniqueID == _autoPlayAnimClipID
						&& curAnimClip._targetOptTranform != null)
					{
						if (curAnimClip._targetOptTranform._rootUnit != null)
						{
							//자동 재생할 Root Unit을 찾았다.
							targetOptRootUnit = curAnimClip._targetOptTranform._rootUnit;
							firstPlayAnimClip = curAnimClip;

							break;
						}
					}
				}
			}

			_isAutoPlayCheckable = false;

			//없다면 0번 RootUnit 을 선택한다.
			if (targetOptRootUnit == null)
			{
				if (_optRootUnitList.Count > 0)
				{
					targetOptRootUnit = _optRootUnitList[0];
				}
			}

			
			apOptRootUnit prevRootUnit = _curPlayingOptRootUnit;//v1.4.7 추가

			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.Show();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}

			//v1.4.7 : 루트 유닛이 변경되는 것을 체크한다.
			_isCurrentRootUnitChanged = prevRootUnit != _curPlayingOptRootUnit;

			//자동 재생을 한다.
			if (firstPlayAnimClip != null)
			{
				//이전
				//PlayNoDebug(firstPlayAnimClip._name);

				if (!_isUsingMecanim)
				{
					//변경
					Play(firstPlayAnimClip._name);
				}

			}

			//만약 숨어있다가 나타날때 위치가 바뀌어있었다면 워프 가능성이 있다.
			//이 경우를 대비해서 물리 위치를 현재 위치로 갱신해두자
			if (_transform != null)
			{
				_transform = transform;
			}
			_posW_Prev1F = _transform.position;


			//v1.4.9 : 루트유닛이 바뀌면 한번이 아닌 3번의 프레임동안 물리가 비활성화된다.
			//1번으로는 물리가 이상하게 작동하는 듯
			if(_isCurrentRootUnitChanged)
			{
				_preventPhysicsCount = 3;
			}
		}

		/// <summary>
		/// Makes the input Root Unit visible. 
		/// If it has an animation clip that plays automatically, this animation clip will play automatically.
		/// </summary>
		/// <param name="targetOptRootUnit">Root Unit to be visible</param>
		public void ShowRootUnit(apOptRootUnit targetOptRootUnit)
		{
			apAnimClip firstPlayAnimClip = null;
			if (_isAutoPlayCheckable && _autoPlayAnimClipID >= 0)
			{
				//자동 재생은 제한적으로 실행한다.
				//targetOptRootUnit에 포함된 AnimClip만 실행된다.
				apAnimClip curAnimClip = null;
				for (int i = 0; i < _animClips.Count; i++)
				{
					curAnimClip = _animClips[i];
					if (curAnimClip._uniqueID == _autoPlayAnimClipID
						&& curAnimClip._targetOptTranform != null)
					{
						if (curAnimClip._targetOptTranform._rootUnit != null
							&& curAnimClip._targetOptTranform._rootUnit == targetOptRootUnit)
						{
							//자동 재생할 AnimClip을 찾았다.
							firstPlayAnimClip = curAnimClip;
							break;
						}
					}
				}
			}

			_isAutoPlayCheckable = false;


			apOptRootUnit prevRootUnit = _curPlayingOptRootUnit;//v1.4.7 추가

			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.Show();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}


			//v1.4.7 : 루트 유닛이 변경되는 것을 체크한다.
			_isCurrentRootUnitChanged = prevRootUnit != _curPlayingOptRootUnit;


			//자동 재생을 한다.
			if (firstPlayAnimClip != null)
			{
				//Play(firstPlayAnimClip._name);

				if (!_isUsingMecanim)
				{
					//변경
					Play(firstPlayAnimClip._name);
				}
			}

			//v1.4.9 : 루트유닛이 바뀌면 한번이 아닌 3번의 프레임동안 물리가 비활성화된다.
			//1번으로는 물리가 이상하게 작동하는 듯
			if(_isCurrentRootUnitChanged)
			{
				_preventPhysicsCount = 3;
			}
		}


		//추가 21.9.21 : 애니메이션 재생 없이 루트 유닛을 전환하는 함수		
		public void SwitchRootUnitWithoutPlayAnim(apOptRootUnit targetOptRootUnit)
		{
			_isAutoPlayCheckable = false;

			apOptRootUnit prevRootUnit = _curPlayingOptRootUnit;//v1.4.7 추가

			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.Show();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}

			//v1.4.7 : 루트 유닛이 변경되는 것을 체크한다.
			_isCurrentRootUnitChanged = prevRootUnit != _curPlayingOptRootUnit;


			//v1.4.9 : 루트유닛이 바뀌면 한번이 아닌 3번의 프레임동안 물리가 비활성화된다.
			//1번으로는 물리가 이상하게 작동하는 듯
			if(_isCurrentRootUnitChanged)
			{
				_preventPhysicsCount = 3;
			}

			//애니메이션은 모두 중지한다.
			StopAll();
		}




		/// <summary>
		/// Hide all Root Units
		/// </summary>
		public void HideRootUnits()
		{
			StopAll();

			//추가 21.4.3
			//StopAll이 적용되려면 업데이트가 한번 되어야 한다.
			//Hide되면 애니메이션이 업데이트되지 않으므로, 여기서 강제로 업데이트를 한번 더 하자
			if (!_isUsingMecanim)
			{
				//_animPlayManager.Update(0.0f);
				_animPlayManager.ReleaseAllPlayUnitAndQueues();
			}

			//모두 숨기기
			_curPlayingOptRootUnit = null;

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].Hide();
			}

			_isCurrentRootUnitChanged = false;
		}



		//추가 21.3.14 : 실행중인 RootUnit을 리턴한다.
		/// <summary>
		/// Return the currently playing Root Unit.
		/// </summary>
		/// <returns>Root Unit currently playing. If not, return null</returns>
		public apOptRootUnit GetCurrentRootUnit()
		{
			return _curPlayingOptRootUnit;
		}

		/// <summary>
		/// Return the index of the currently playing Root Unit.
		/// </summary>
		/// <returns>Index of the currently playing Root Unit. If not, return -1</returns>
		public int GetCurrentRootUnitIndex()
		{
			if (_curPlayingOptRootUnit == null)
			{
				return -1;
			}
			return _optRootUnitList.IndexOf(_curPlayingOptRootUnit);
		}



		/// <summary>
		/// Initializes the command buffer for clipping mask processing.
		/// </summary>
		/// <param name="targetOptRootUnit">Target Root Unit</param>
		/// <param name="isRegistToCamera">If True, re-register the command buffers to the camera after initialization.</param>
		public void ResetMeshCommandBuffer(apOptRootUnit targetOptRootUnit, bool isRegistToCamera)
		{
			if (targetOptRootUnit == null)
			{
				return;
			}
			targetOptRootUnit.ResetCommandBuffer(isRegistToCamera);
		}

		/// <summary>
		/// [Please do not use it]
		/// Bake Function likes "ShowRootUnit" using Default Visible Value.
		/// </summary>
		/// <param name="targetOptRootUnit">Target Root Unit</param>
		public void ShowRootUnitWhenBake(apOptRootUnit targetOptRootUnit)
		{
			_curPlayingOptRootUnit = null;
			apOptRootUnit optRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				optRootUnit = _optRootUnitList[i];
				if (optRootUnit == targetOptRootUnit)
				{
					//이건 Show를 하자
					optRootUnit.ShowWhenBake();
					_curPlayingOptRootUnit = targetOptRootUnit;
				}
				else
				{
					//이건 Hide
					optRootUnit.Hide();
				}
			}

			_isCurrentRootUnitChanged = false;
		}



		/// <summary>Turn physical effects on or off.</summary>
		/// <param name="isPhysicEnabled"></param>
		public void SetPhysicEnabled(bool isPhysicEnabled)
		{
			_isPhysicsPlay_Opt = isPhysicEnabled;
		}


		///// <summary>
		///// [Do not use this function]
		///// </summary>
		//public void IgnoreAnimAutoPlayOption()
		//{
		//	_isAutoPlayCheckable = false;
		//}

		//--------------------------------------------------------------------------------------
		// Runtime Optimized
		//--------------------------------------------------------------------------------------
		//첫 Bake 후 또는 시작후 로딩시 Modifier -> 해당 OptTransform을 연결한다.
		/// <summary>
		/// Initialize before updating. 
		/// This is done automatically if you do not call the function directly. 
		/// "AsyncInitialize()" is recommended when it takes a lot of execution time.
		/// </summary>
		public bool Initialize()
		{
			// < 단계 1 > : 기본 초기화 (비동기 함수에서는 비동기 전에 호출하는 구문)

			//Debug.Log("LinkModifierAndMeshGroups_Opt");
			if (_initStatus != INIT_STATUS.Ready)
			{
				//엥 비동기 로딩 중이거나 로딩이 끝났네염
				//Debug.LogError(">>> 이미 로딩이 된 상태. 초기화 필요");
				return false;
			}

			if (_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;



			//HideRootUnits();//삭제 21.5.27

			_funcAyncLinkCompleted = null;
			_isAutoPlayCheckable = true;

			_prevOptRootUnit = null;//추가 v1.4.7


			// < 단계 2 > : Hide Root Unit 전의 초기화 | 비동기에서는 첫 Yield 이전의 실행될 코드

			//추가 20.7.5 : 컨트롤 파라미터를 초기화 (이게 왜 없었지)
			_controller.InitRuntime(this);


			//추가 20.11.23 : 모디파이어 최적화를 위한 애니메이션 매핑 클래스
			//생성과 동시에 링크가 된다.
			if (_animPlayMapping == null)
			{
				_animPlayMapping = new apAnimPlayMapping(this);
			}
			else
			{
				//다시 링크를 하자
				_animPlayMapping.Link(this);
			}


			//추가 12.7 : OptRootUnit도 Link를 해야한다.
			for (int iOptRootUnit = 0; iOptRootUnit < _optRootUnitList.Count; iOptRootUnit++)
			{
				_optRootUnitList[iOptRootUnit].Link(this);
			}

			//MeshGroup -> OptTransform을 돌면서 처리
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				_optTransforms[iOptTransform].ClearResultParams(false);
			}

			//HideRootUnits();//삭제 21.5.27 : 동기 초기화에서는 필요없다.



			// < 단계 3 > : Batched Mat, OptTransform, 초기화

			//추가 : BatchedMat도 연결
			_optBatchedMaterial.Link(this);

			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].InitMesh(true);//<<이때 ShowHide도 결정된다.
				_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
			}

			apOptTransform curOptTransform = null;
			List<apOptModifierUnitBase> curModifiers = null;
			apOptModifierUnitBase curModifier = null;
			List<apOptParamSetGroup> curParamSetGroups = null;
			apOptParamSetGroup curParamSetGroup = null;
			List<apOptParamSet> curParamSets = null;


			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				curOptTransform = _optTransforms[iOptTransform];

				curModifiers = curOptTransform._modifierStack._modifiers;
				for (int iMod = 0; iMod < curModifiers.Count; iMod++)
				{
					curModifier = curModifiers[iMod];

					//Portrait를 연결해준다.
					curModifier.Link(this, curOptTransform);

					curParamSetGroups = curModifier._paramSetGroupList;
					for (int iPSGroup = 0; iPSGroup < curParamSetGroups.Count; iPSGroup++)
					{
						curParamSetGroup = curParamSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (curParamSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								curParamSetGroup._keyControlParam = GetControlParam(curParamSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								break;
						}


						curParamSets = curParamSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < curParamSets.Count; iParamSet++)
						{
							//Param Set Link
							curParamSets[iParamSet].LinkParamSetGroup(curParamSetGroup, this);
						}
					}
				}

				//이전
				//optTransform.RefreshModifierLink();//이 코드는 사용되지 않는다. Root OptTransform에서만 호출해야한다.
			}


			// < 단계 4 > : Root Unit, Anim Clip 초기화

			apOptRootUnit curRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				curRootUnit = _optRootUnitList[i];

				curRootUnit._rootOptTransform.ClearResultParams(true);
				curRootUnit._rootOptTransform.RefreshModifierLink(true, true);

				//추가 20.8.30
				curRootUnit._rootOptTransform.Initialize(true, true, this);
			}

			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkOpt(this);
			}

			//AnimPlayer를 추가했다.
			_animPlayManager.LinkPortrait(this);


			// < 단계 5 > : 메타 데이터를 초기화

			//추가 22.5.18 [v1.4.0] 지연된 애니메이션 실행 요청
			if (_animPlayDeferredRequest == null)
			{
				_animPlayDeferredRequest = new apAnimPlayDeferredRequest(_animPlayManager);
			}
			_animPlayDeferredRequest.Ready();


			//추가 21.9.24 : 유니티 이벤트를 사용하는 경우
			if (_animEventCallMode == ANIM_EVENT_CALL_MODE.Callback)
			{
				if (_unityEventWrapper == null)
				{
					_unityEventWrapper = new apUnityEventWrapper();
				}
				_unityEventWrapper.Link(this);
			}

			//추가 22.6.8 : 애니메이션, 텍스쳐등을 빠르게 접근하기 위한 매핑 변수 생성
			MakeFastReferMapping();




			//여기로 옮기기
			HideRootUnits();


			//추가 21.10.7 : 배속 옵션 초기화 (함수 호출이 없었다면)
			if (!_isDeltaTimeOptionChanged)
			{
				_deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;
				_deltaTimeMultiplier = 1.0f;
				_funcDeltaTimeRequested = null;
				_deltaTimeRequestSavedObject = null;
			}

			//추가 21.12.22 : 재질 병합 관련 변수 초기화
			_isUseMergedMat = false;
			_mergeMatMainPortrait = null;
			_mergedMatSubPortraits = null;

			//추가 22.7.7 : 텔레포트 관련 변수 초기화
			_isTeleportChecked = false;//이전에 텔레포트가 체크되었는가.
			_teleportCheck_PosPrev = Vector3.zero;//이전 프레임에서의 텔레포트
			_teleportCheck_ScalePrev = Vector3.one;//[v1.5.0] 텔레포트의 스케일 체크
			_teleportCheck_RotationPrev = Vector3.zero;//[v1.5.0] 텔레포트의 회전 체크

			_isCurrentTeleporting = false;//현재 프레임에서 텔레포트가 발생했는가
			_isPhysicsEnabledInPrevFrame = false;//이전에 물리 연산이 있었는가
			_curPlayingOptRootUnit = null;

			//추가 v1.4.7 : 루트유닛 변경에 따른 물리 튀는 문제 변수 초기화
			_isCurrentRootUnitChanged = false;

			//추가 v1.4.8 : 루트 모션 유효성 체크
			ValidateRootMotion();



			//로딩 끝
			_initStatus = INIT_STATUS.Completed;

			CleanUpMeshesCommandBuffers();

			//추가 : 초기화시 카메라 갱신 로직 필요
			CheckAndRefreshCameras(false);//false : 여기서는 카메라 변경(초기화>발견)시에도 커맨드 버퍼를 갱신하지 않는다. ShowRootUnit에서 버퍼가 생성될 것임

			ShowRootUnit();

			return true;
		}



		//-------------------------------------------
		// 비동기 방식의 로딩
		//-------------------------------------------
		/// <summary>
		/// Initialize asynchronously using coroutine. It does the same thing as the "Initialize ()" function.
		/// </summary>
		/// <returns>It returns False if it is already initialized or in progress. If it is true, it means that the initialization starts normally.</returns>
		public bool AsyncInitialize()
		{
			if (_initStatus != INIT_STATUS.Ready)
			{
				//오잉 비동기 로딩중이거나 로딩이 끝났네염
				return false;
			}

			//비동기 로딩 시작
			_initStatus = INIT_STATUS.AsyncLoading;

			if (_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;

			_prevOptRootUnit = null;//추가 v1.4.7

			//지연된 플레이 요청 초기화 (여기선 HideRootUnits보단 미리 호출되어야 한다.)
			if (_animPlayDeferredRequest == null)
			{
				_animPlayDeferredRequest = new apAnimPlayDeferredRequest(_animPlayManager);
			}
			_animPlayDeferredRequest.Ready();



			HideRootUnits();



			StartCoroutine(LinkOptCoroutine());

			return true;
		}


		/// <summary>
		/// Initialize asynchronously using coroutine. It does the same thing as the "Initialize ()" function.
		/// </summary>
		/// <param name="onAsyncLinkCompleted">Functions to receive callbacks when initialization is complete.</param>
		/// <returns>It returns False if it is already initialized or in progress. If it is true, it means that the initialization starts normally.></returns>
		public bool AsyncInitialize(OnAsyncLinkCompleted onAsyncLinkCompleted)
		{
			if (_initStatus != INIT_STATUS.Ready)
			{
				//오잉 비동기 로딩중이거나 로딩이 끝났네염
				return false;
			}

			//비동기 로딩 시작
			_initStatus = INIT_STATUS.AsyncLoading;

			_funcAyncLinkCompleted = onAsyncLinkCompleted;

			//for (int i = 0; i < _optMeshes.Count; i++)
			//{
			//	_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
			//	_optMeshes[i].Hide();
			//}

			if (_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;

			_prevOptRootUnit = null;//추가 v1.4.7


			//지연된 플레이 요청 초기화 (여기선 HideRootUnits보단 미리 호출되어야 한다.)
			if (_animPlayDeferredRequest == null)
			{
				_animPlayDeferredRequest = new apAnimPlayDeferredRequest(_animPlayManager);
			}
			_animPlayDeferredRequest.Ready();



			HideRootUnits();

			StartCoroutine(LinkOptCoroutine());

			return true;

		}

		/// <summary>
		/// Initialize using coroutine. 
		/// This function runs at low CPU usage by setting the "time interval at which Yield is called" by the user. 
		/// However, the processing time may be very long.
		/// </summary>
		/// <param name="timePerYield">Time value for whether Yield is called every few milliseconds during initialization.(10ms ~ 1000ms)</param>
		/// <param name="onAsyncLinkCompleted">Functions to receive callbacks when initialization is complete.</param>
		/// <returns>It returns False if it is already initialized or in progress. If it is true, it means that the initialization starts normally.></returns>
		public bool AsyncInitialize(int timePerYield, OnAsyncLinkCompleted onAsyncLinkCompleted = null)
		{
			if (_initStatus != INIT_STATUS.Ready)
			{
				//오잉 비동기 로딩중이거나 로딩이 끝났네염
				return false;
			}

			//비동기 로딩 시작
			_initStatus = INIT_STATUS.AsyncLoading;

			apAsyncTimer asyncTimer = new apAsyncTimer(timePerYield);

			_funcAyncLinkCompleted = onAsyncLinkCompleted;

			if (_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;

			_prevOptRootUnit = null;//추가 v1.4.7

			//지연된 플레이 요청 초기화 (여기선 HideRootUnits보단 미리 호출되어야 한다.)
			if (_animPlayDeferredRequest == null)
			{
				_animPlayDeferredRequest = new apAnimPlayDeferredRequest(_animPlayManager);
			}
			_animPlayDeferredRequest.Ready();


			HideRootUnits();


			StartCoroutine(LinkOptCoroutineWithAsyncTimer(asyncTimer));

			return true;

		}


		private IEnumerator LinkOptCoroutine()
		{
			// < 단계 2 > : Hide Root Unit 전의 초기화 | 비동기에서는 첫 Yield 이전의 실행될 코드

			//추가 20.7.5 : 컨트롤 파라미터를 초기화 (이게 왜 없었지)
			_controller.InitRuntime(this);

			//추가 20.11.23 : 모디파이어 최적화를 위한 애니메이션 매핑 클래스
			//생성과 동시에 링크가 된다.
			if (_animPlayMapping == null)
			{
				_animPlayMapping = new apAnimPlayMapping(this);
			}
			else
			{
				//다시 링크를 하자
				_animPlayMapping.Link(this);
			}


			//추가 12.7 : OptRootUnit도 Link를 해야한다.
			for (int iOptRootUnit = 0; iOptRootUnit < _optRootUnitList.Count; iOptRootUnit++)
			{
				_optRootUnitList[iOptRootUnit].Link(this);
			}


			//MeshGroup -> OptTransform을 돌면서 처리
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				_optTransforms[iOptTransform].ClearResultParams(false);
			}

			HideRootUnits();



			//Wait
			yield return new WaitForEndOfFrame();

			// < 단계 3 > : Batched Mat, OptTransform, 초기화

			//추가 : BatchedMat도 연결
			_optBatchedMaterial.Link(this);

			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].InitMesh(true);
				_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
				_optMeshes[i].Hide();//<<비동기에서는 바로 Hide
			}

			int nLoad = 0;

			apOptTransform curOptTransform = null;
			List<apOptModifierUnitBase> curModifiers = null;
			apOptModifierUnitBase curModifier = null;
			List<apOptParamSetGroup> curParamSetGroups = null;
			apOptParamSetGroup curParamSetGroup = null;
			List<apOptParamSet> curParamSets = null;

			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				curOptTransform = _optTransforms[iOptTransform];

				curModifiers = curOptTransform._modifierStack._modifiers;

				for (int iMod = 0; iMod < curModifiers.Count; iMod++)
				{
					curModifier = curModifiers[iMod];

					//Portrait를 연결해준다.
					curModifier.Link(this, curOptTransform);

					//Wait
					nLoad++;
					if (nLoad > 5)
					{
						nLoad = 0;
						yield return new WaitForEndOfFrame();
					}


					curParamSetGroups = curModifier._paramSetGroupList;
					for (int iPSGroup = 0; iPSGroup < curParamSetGroups.Count; iPSGroup++)
					{
						curParamSetGroup = curParamSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (curParamSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								curParamSetGroup._keyControlParam = GetControlParam(curParamSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								break;
						}


						curParamSets = curParamSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < curParamSets.Count; iParamSet++)
						{
							//Param Set Link
							curParamSets[iParamSet].LinkParamSetGroup(curParamSetGroup, this);
						}
					}

					//Wait
					nLoad++;
					if (nLoad > 5)
					{
						nLoad = 0;
						yield return new WaitForEndOfFrame();
					}
				}

				//이전
				//optTransform.RefreshModifierLink();

			}

			//Wait
			yield return new WaitForEndOfFrame();



			// < 단계 4 > : Root Unit, Anim Clip 초기화

			apOptRootUnit curRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				curRootUnit = _optRootUnitList[i];

				curRootUnit._rootOptTransform.ClearResultParams(true);
				curRootUnit._rootOptTransform.RefreshModifierLink(true, true);

				//추가 20.8.30
				curRootUnit._rootOptTransform.Initialize(true, true, this);
			}

			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkOpt(this);
			}


			//추가) AnimPlayer를 추가했다.
			_animPlayManager.LinkPortrait(this);
			_isAutoPlayCheckable = true;


			//Wait
			yield return new WaitForEndOfFrame();



			// < 단계 5 > : 메타 데이터를 초기화


			//추가 22.5.18 [v1.4.0] 지연된 애니메이션 실행 요청
			if (_animPlayDeferredRequest == null)
			{
				_animPlayDeferredRequest = new apAnimPlayDeferredRequest(_animPlayManager);
			}
			_animPlayDeferredRequest.Ready();


			//추가 21.9.24 : 유니티 이벤트를 사용하는 경우
			if (_animEventCallMode == ANIM_EVENT_CALL_MODE.Callback)
			{
				if (_unityEventWrapper == null)
				{
					_unityEventWrapper = new apUnityEventWrapper();
				}
				_unityEventWrapper.Link(this);
			}

			//추가 22.6.8 : 애니메이션, 텍스쳐등을 빠르게 접근하기 위한 매핑 변수 생성
			MakeFastReferMapping();


			//추가 21.10.7 : 배속 옵션 초기화 (함수 호출이 없었다면)
			if (!_isDeltaTimeOptionChanged)
			{
				_deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;
				_deltaTimeMultiplier = 1.0f;
				_funcDeltaTimeRequested = null;
				_deltaTimeRequestSavedObject = null;
			}

			//추가 21.12.22 : 재질 병합 관련 변수 초기화
			_isUseMergedMat = false;
			_mergeMatMainPortrait = null;
			_mergedMatSubPortraits = null;


			//추가 22.7.7 : 텔레포트 관련 변수 초기화
			_isTeleportChecked = false;//이전에 텔레포트가 체크되었는가.
			_teleportCheck_PosPrev = Vector3.zero;//이전 프레임에서의 텔레포트
			_teleportCheck_ScalePrev = Vector3.one;//[v1.5.0] 텔레포트의 스케일 체크
			_teleportCheck_RotationPrev = Vector3.zero;//[v1.5.0] 텔레포트의 회전 체크

			_isCurrentTeleporting = false;//현재 프레임에서 텔레포트가 발생했는가
			_isPhysicsEnabledInPrevFrame = false;//이전에 물리 연산이 있었는가
			_curPlayingOptRootUnit = null;

			//추가 v1.4.7 : 루트유닛 변경에 따른 물리 튀는 문제 변수 초기화
			_isCurrentRootUnitChanged = false;


			//추가 v1.4.8 : 루트 모션 유효성 체크
			ValidateRootMotion();

			//Wait
			yield return new WaitForEndOfFrame();


			//끝!
			_initStatus = INIT_STATUS.Completed;

			CleanUpMeshesCommandBuffers();

			//if(_optRootUnitList.Count > 0)
			//{
			//	ShowRootUnit(_optRootUnitList[0]);//일단 첫번째 RootUnit이 나온다.
			//}

			//추가 : 초기화시 카메라 갱신 로직 필요
			CheckAndRefreshCameras(false);//false : 초기화시엔 카메라 변경(초기화>발견)시에도 커맨드 버퍼를 리셋하지 않는다.

			ShowRootUnit();



			if (_funcAyncLinkCompleted != null)
			{
				//콜백 이벤트 호출
				_funcAyncLinkCompleted(this);
				_funcAyncLinkCompleted = null;
			}


		}


		//추가 19.5.28 : AsyncTimer를 이용하여 LinkOpCoroutine를 개선한 버전.
		//실제로 실행 시간 타이머가 동작한다.
		private IEnumerator LinkOptCoroutineWithAsyncTimer(apAsyncTimer asyncTimer)
		{
			// < 단계 2 > : Hide Root Unit 전의 초기화 | 비동기에서는 첫 Yield 이전의 실행될 코드

			//추가 20.7.5 : 컨트롤 파라미터를 초기화 (이게 왜 없었지)
			_controller.InitRuntime(this);


			//랜덤하게 프레임을 쉬어주자
			int nWaitRandom = UnityEngine.Random.Range(0, 5);
			for (int i = 0; i < nWaitRandom; i++)
			{
				yield return new WaitForEndOfFrame();
			}


			//추가 20.11.23 : 모디파이어 최적화를 위한 애니메이션 매핑 클래스
			//생성과 동시에 링크가 된다.
			if (_animPlayMapping == null)
			{
				_animPlayMapping = new apAnimPlayMapping(this);
			}
			else
			{
				//다시 링크를 하자
				_animPlayMapping.Link(this);
			}



			//추가 12.7 : OptRootUnit도 Link를 해야한다.
			for (int iOptRootUnit = 0; iOptRootUnit < _optRootUnitList.Count; iOptRootUnit++)
			{
				yield return _optRootUnitList[iOptRootUnit].LinkAsync(this, asyncTimer);
			}


			//MeshGroup -> OptTransform을 돌면서 처리
			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				_optTransforms[iOptTransform].ClearResultParams(false);
			}

			HideRootUnits();




			//타이머에 의해서 Wait
			if (asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}


			// < 단계 3 > : Batched Mat, OptTransform, 초기화

			//BatchedMat도 연결
			_optBatchedMaterial.Link(this);


			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].InitMesh(true);
				_optMeshes[i].InstantiateMaterial(_optBatchedMaterial);//재질 Batch 정보를 넣고 초기화
				_optMeshes[i].Hide();//<<비동기에서는 바로 Hide

				//타이머에 의해서 Wait
				if (asyncTimer.IsYield())
				{
					yield return asyncTimer.WaitAndRestart();
				}
			}

			apOptTransform curOptTransform = null;
			List<apOptModifierUnitBase> curModifiers = null;
			apOptModifierUnitBase curModifier = null;
			List<apOptParamSetGroup> curParamSetGroups = null;
			apOptParamSetGroup curParamSetGroup = null;
			List<apOptParamSet> curParamSets = null;


			for (int iOptTransform = 0; iOptTransform < _optTransforms.Count; iOptTransform++)
			{
				curOptTransform = _optTransforms[iOptTransform];

				curModifiers = curOptTransform._modifierStack._modifiers;
				for (int iMod = 0; iMod < curModifiers.Count; iMod++)
				{
					curModifier = curModifiers[iMod];

					//추가 : Portrait를 연결해준다.
					curModifier.Link(this, curOptTransform);

					//타이머에 의해서 Wait
					if (asyncTimer.IsYield())
					{
						yield return asyncTimer.WaitAndRestart();
					}


					curParamSetGroups = curModifier._paramSetGroupList;
					for (int iPSGroup = 0; iPSGroup < curParamSetGroups.Count; iPSGroup++)
					{
						curParamSetGroup = curParamSetGroups[iPSGroup];

						//List<apModifierParamSet> paramSets = mod._paramSetList;
						//1. Key를 세팅해주자
						switch (curParamSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.Static:
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								curParamSetGroup._keyControlParam = GetControlParam(curParamSetGroup._keyControlParamID);
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								break;
						}


						curParamSets = curParamSetGroup._paramSetList;

						for (int iParamSet = 0; iParamSet < curParamSets.Count; iParamSet++)
						{
							apOptParamSet paramSet = curParamSets[iParamSet];

							//Link를 해주자
							paramSet.LinkParamSetGroup(curParamSetGroup, this);
						}

						//타이머에 의해서 Wait
						if (asyncTimer.IsYield())
						{
							yield return asyncTimer.WaitAndRestart();
						}
					}
				}
			}

			//Wait
			yield return new WaitForEndOfFrame();


			// < 단계 4 > : Root Unit, Anim Clip 초기화
			apOptRootUnit curRootUnit = null;
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				curRootUnit = _optRootUnitList[i];

				curRootUnit._rootOptTransform.ClearResultParams(true);
				yield return curRootUnit._rootOptTransform.RefreshModifierLinkAsync(true, true, asyncTimer);

				//추가 20.8.30
				yield return curRootUnit._rootOptTransform.InitializeAsync(true, true, asyncTimer, this);
			}

			//타이머에 의해서 Wait
			if (asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}



			for (int i = 0; i < _animClips.Count; i++)
			{
				yield return _animClips[i].LinkOptAsync(this, asyncTimer);
			}



			//타이머에 의해서 Wait
			if (asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}


			//추가) AnimPlayer를 추가했다.
			yield return _animPlayManager.LinkPortraitAsync(this, asyncTimer);
			_isAutoPlayCheckable = true;


			//Wait
			yield return new WaitForEndOfFrame();


			// < 단계 5 > : 메타 데이터를 초기화
			//추가 22.5.18 [v1.4.0] 지연된 애니메이션 실행 요청
			if (_animPlayDeferredRequest == null)
			{
				_animPlayDeferredRequest = new apAnimPlayDeferredRequest(_animPlayManager);
			}
			_animPlayDeferredRequest.Ready();


			//추가 21.9.24 : 유니티 이벤트를 사용하는 경우
			if (_animEventCallMode == ANIM_EVENT_CALL_MODE.Callback)
			{
				if (_unityEventWrapper == null)
				{
					_unityEventWrapper = new apUnityEventWrapper();
				}
				_unityEventWrapper.Link(this);
			}

			//추가 22.6.8 : 애니메이션, 텍스쳐등을 빠르게 접근하기 위한 매핑 변수 생성
			MakeFastReferMapping();



			//추가 21.10.7 : 배속 옵션 초기화 (함수 호출이 없었다면)
			if (!_isDeltaTimeOptionChanged)
			{
				_deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;
				_deltaTimeMultiplier = 1.0f;
				_funcDeltaTimeRequested = null;
				_deltaTimeRequestSavedObject = null;
			}

			//추가 21.12.22 : 재질 병합 관련 변수 초기화
			_isUseMergedMat = false;
			_mergeMatMainPortrait = null;
			_mergedMatSubPortraits = null;


			//추가 22.7.7 : 텔레포트 관련 변수 초기화
			_isTeleportChecked = false;//이전에 텔레포트가 체크되었는가.
			_teleportCheck_PosPrev = Vector3.zero;//이전 프레임에서의 텔레포트
			_teleportCheck_ScalePrev = Vector3.one;//[v1.5.0] 텔레포트의 스케일 체크
			_teleportCheck_RotationPrev = Vector3.zero;//[v1.5.0] 텔레포트의 회전 체크

			_isCurrentTeleporting = false;//현재 프레임에서 텔레포트가 발생했는가
			_isPhysicsEnabledInPrevFrame = false;//이전에 물리 연산이 있었는가.
			_curPlayingOptRootUnit = null;

			//추가 v1.4.7 : 루트유닛 변경에 따른 물리 튀는 문제 변수 초기화
			_isCurrentRootUnitChanged = false;

			//추가 v1.4.8 : 루트 모션 유효성 체크
			ValidateRootMotion();

			//Wait
			yield return new WaitForEndOfFrame();



			//끝!
			_initStatus = INIT_STATUS.Completed;

			CleanUpMeshesCommandBuffers();

			//if(_optRootUnitList.Count > 0)
			//{
			//	ShowRootUnit(_optRootUnitList[0]);//일단 첫번째 RootUnit이 나온다.
			//}

			//추가 : 초기화시 카메라 갱신 로직 필요
			CheckAndRefreshCameras(false);//false : 초기화시엔 카메라 변경(초기화 > 발견)시에도 커맨드 버퍼를 초기화하지 않는다.

			ShowRootUnit();



			//AsyncTimer 끝
			asyncTimer.OnCompleted();
			asyncTimer = null;

			if (_funcAyncLinkCompleted != null)
			{
				//콜백 이벤트 호출
				_funcAyncLinkCompleted(this);
				_funcAyncLinkCompleted = null;
			}


		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void SetFirstInitializeAfterBake()
		{
			_initStatus = INIT_STATUS.Ready;

			if (_transform == null)
			{
				_transform = transform;
			}

			//Transform 추가시 위치를 초기화하자 (20.9.15)
			_posW_Prev1F = _transform.position;
			_rotationOnlyMatrixIfBillboard = Matrix4x4.identity;
			_invRotationOnlyMatrixIfBillboard = Matrix4x4.identity;

			_prevOptRootUnit = null;//추가 v1.4.7
		}


		/// <summary>
		/// 추가 22.6.8 : string으로 인게임 중에 텍스쳐나 애니메이션을 참조할 때, 참조 속도를 높이기 위핸 매핑 변수 초기화
		/// </summary>
		private void MakeFastReferMapping()
		{
			//텍스쳐 참조용 매핑
			if (_mapping_OptTextureData == null)
			{
				_mapping_OptTextureData = new Dictionary<string, apOptTextureData>();
			}
			_mapping_OptTextureData.Clear();
			int nOptTextureData = _optTextureData != null ? _optTextureData.Count : 0;
			apOptTextureData curOptTexture = null;

			if (nOptTextureData > 0)
			{
				for (int i = 0; i < nOptTextureData; i++)
				{
					curOptTexture = _optTextureData[i];

					if (_mapping_OptTextureData.ContainsKey(curOptTexture._name))
					{
						continue;
					}
					_mapping_OptTextureData.Add(curOptTexture._name, curOptTexture);
				}
			}
		}


		/// <summary>
		/// Link시에 "루트 모션(Root Motion)"옵션을 그대로 적용할 수 있는지 유효성을 체크한다.
		/// </summary>
		private void ValidateRootMotion()
		{
			//바로 위 부모 Transform
			_rootMotionValidated_ParentTransform = null;

			//부모 또는 더 위 부모 Transform. 위치 이동이 적용된다.
			_rootMotionValidated_BaseTransform = null;

			_rootMotionValidatedMode = ROOT_MOTION_MODE.None;
			_funcRootMotionEvent = null;

			_rootMotionValidated_BaseComponentType = ROOT_MOTION_BASE_COMPONENT.Transform;
			_rootMotionValidated_BaseCom_Rigidbody2D = null;
			_rootMotionValidated_BaseCom_Rigidbody3D = null;

			_rootMotion_IsRequestedMove = false;
			_rootMotion_RequestedPos = Vector3.zero;



			if(!Application.isPlaying)
			{
				//플레이 중이 아니라면 루트 모션 옵션은 비활성화된 상태로 종료
				return;
			}

			if(_rootMotionModeOption == ROOT_MOTION_MODE.None)
			{
				//해당 옵션이 사용되지 않는다면 더 검토하진 않는다.
				return;
			}

			//모든 축의 옵션이 Disabled라면 비활성화된다.
			if(_rootMotionAxisOption_X == ROOT_MOTION_MOVE_TYPE_PER_AXIS.Disabled
				&& _rootMotionAxisOption_Y == ROOT_MOTION_MOVE_TYPE_PER_AXIS.Disabled)
			{
				return;
			}

			//옵션에 관계없이, Parent는 무조건 있어야 한다.
			if(transform.parent == null)
			{
				Debug.LogError("AnyPortrait : [Root Motion Failed] The Parent Transform required to operate as Root Motion does not exist.", this.gameObject);
				return;
			}

			//부모 Transform 할당
			_rootMotionValidated_ParentTransform = transform.parent;//Lock to Center까지는 현재의 Parent를 이용한다.

			//루트 모션의 "중앙 고정" 또는 "이동값 반영"의 경우는
			//> "모든 루트 유닛들이 유효한 루트 모션 본을 가져야 한다.
			//> "Parent Transform"을 가져야 한다. (그리고 유효해야함)

			//두 조건을 충족하지 않는다면 None 타입으로 강제된다.

			if (_rootMotionModeOption == ROOT_MOTION_MODE.MoveParentTransform)
			{
				//Move Parent Transform의 경우에는 이동시킬 대상이 지정되어야 한다.
				if (_rootMotionTargetTransformType == ROOT_MOTION_TARGET_TRANSFORM.Parent)
				{
					//그냥 바로 위 Parent를 이용한다면
					_rootMotionValidated_BaseTransform = _rootMotionValidated_ParentTransform;
				}
				else
				{
					//별도로 설정된 Transform을 이용한다면
					_rootMotionValidated_BaseTransform = _rootMotionSpecifiedParentTransform;
				}

				//적용된 Transform의 유효성을 검사한다.
				if (_rootMotionValidated_BaseTransform == null)
				{
					//Parent Transform이 없다 > 취소
					Debug.LogError("AnyPortrait : [Root Motion Failed] The Parent Transform required to operate as Root Motion does not exist.", this.gameObject);
					_rootMotionValidatedMode = ROOT_MOTION_MODE.None;//None으로 변경됨
					return;
				}

				//Parent Transform이 부모여야 한다.
				if (!transform.IsChildOf(_rootMotionValidated_BaseTransform) || transform == _rootMotionValidated_BaseTransform)
				{
					//같거나 유효한 부모-자식 관계가 아니라면 > 취소
					Debug.LogError("AnyPortrait : [Root Motion Failed] The Parent Transform is not the parent object of this Portrait.", this.gameObject);
					_rootMotionValidatedMode = ROOT_MOTION_MODE.None;//None으로 변경됨
					return;
				}

				//Base Transform이 가진 컴포넌트를 확인하자
				Rigidbody2D comp_Rigidbody2D = _rootMotionValidated_BaseTransform.GetComponent<Rigidbody2D>();
				if(comp_Rigidbody2D != null)
				{
					//Rigid Body 2D를 움직이자
					_rootMotionValidated_BaseComponentType = ROOT_MOTION_BASE_COMPONENT.Rigidbody2D;
					_rootMotionValidated_BaseCom_Rigidbody2D = comp_Rigidbody2D;
				}
				else
				{
					Rigidbody comp_Rigidbody3D = _rootMotionValidated_BaseTransform.GetComponent<Rigidbody>();
					if(comp_Rigidbody3D != null)
					{
						//Rigid Body 3D를 움직이자
						_rootMotionValidated_BaseComponentType = ROOT_MOTION_BASE_COMPONENT.Rigidbody3D;
						_rootMotionValidated_BaseCom_Rigidbody3D = comp_Rigidbody3D;
					}
					else
					{
						//그냥 Transform을 움직이자
						_rootMotionValidated_BaseComponentType = ROOT_MOTION_BASE_COMPONENT.Transform;
					}
				}
				
				
				
			}
			


			bool isAllHasRootMotionBone = true;
			int nRootUnits = _optRootUnitList != null ? _optRootUnitList.Count : 0;
			if(nRootUnits > 0)
			{
				apOptRootUnit curRootUnit = null;
				for (int i = 0; i < nRootUnits; i++)
				{
					curRootUnit = _optRootUnitList[i];
					if(curRootUnit.RootMotionBone == null)
					{
						//하나라도 루트 모션용 본이 없다면
						isAllHasRootMotionBone = false;
						break;
					}
				}
			}

			if(!isAllHasRootMotionBone)
			{
				//루트 모션용 본이 하나라도 없다면
				//루트 모션은 비활성화된다.
				Debug.LogError("AnyPortrait : [Root Motion Failed] There is a Root Unit that does not have a Bone for Root Motion.\nPlease specify the Bone for Root Motion in all Mesh Groups set as Root Units.", this.gameObject);
				_rootMotionValidatedMode = ROOT_MOTION_MODE.None;
				return;
			}

			//완료
			_rootMotionValidatedMode = _rootMotionModeOption;

			if(_rootMotionValidatedMode != ROOT_MOTION_MODE.None)
			{
				_funcRootMotionEvent = ProcessRootMotion;//업데이트 이벤트 할당
			}
		}


		//--------------------------------------------------------------------------------------
		// Editor
		//--------------------------------------------------------------------------------------



		// Get / Set
		//-----------------------------------------------------



		//--------------------------------------------------------------------------------------
		// API
		//--------------------------------------------------------------------------------------
		// Play
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Play the animation
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData Play(string animClipName,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null)
				{ return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.Play(animClipName, layer, blendMethod, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.Play(animPlayData, layer, blendMethod, playOption, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Play the animation
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData Play(apAnimPlayData animPlayData,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{

#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null)
				{ return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.Play(animPlayData, layer, blendMethod, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.Play(animPlayData, layer, blendMethod, playOption, isAutoEndIfNotloop);
				return animPlayData;
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}





		private apAnimPlayData PlayNoDebug(string animClipName,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
			if (_animPlayManager == null)
			{ return null; }

			if (_isUsingMecanim)
			{
				//메카님이 켜진 경우 함수를 제어할 수 없다.
				//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
				return null;
			}

			return _animPlayManager.Play(animClipName, layer, blendMethod, playOption, isAutoEndIfNotloop, false);
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueued(string animClipName,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{

#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.PlayQueued(animClipName, layer, blendMethod, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.PlayQueued(animPlayData, layer, blendMethod, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif

		}



		/// <summary>
		/// Wait for the previous animation to finish, then play it.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueued(apAnimPlayData animPlayData,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{

#if UNITY_EDITOR
			try
			{
#endif

				if (_animPlayManager == null)
				{ return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.PlayQueued(animPlayData, layer, blendMethod, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.PlayQueued(animPlayData, layer, blendMethod, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif

		}

		/// <summary>
		/// Play the animation smoothly.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFade(string animClipName,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFade(animClipName, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.CrossFade(animPlayData, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Play the animation smoothly.
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFade(apAnimPlayData animPlayData,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFade(animPlayData, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.CrossFade(animPlayData, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueued(string animClipName,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFadeQueued(animClipName, layer, blendMethod, fadeTime, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.CrossFadeQueued(animPlayData, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueued(apAnimPlayData animPlayData,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFadeQueued(animPlayData, layer, blendMethod, fadeTime, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.CrossFadeQueued(animPlayData, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
				return animPlayData;
				
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif

		}
		//----------------------------------------------------------------------------

		//추가 1.14 : 특정 프레임부터 재생을 한다. (Play, PlayQueued, CrossFade, CrossFadeQueued + At)

		//그 외에는 동일
		/// <summary>
		/// Play the animation at the specified frame.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayAt(string animClipName, int frame,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{

#if UNITY_EDITOR
			try
			{
#endif

				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.PlayAt(animClipName, frame, layer, blendMethod, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.PlayAt(animPlayData, frame, layer, blendMethod, playOption, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Play the animation at the specified frame.
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayAt(apAnimPlayData animPlayData,
									int frame,
									int layer = 0,
									apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
									apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
									bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.PlayAt(animPlayData, frame, layer, blendMethod, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.PlayAt(animPlayData, frame, layer, blendMethod, playOption, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueuedAt(string animClipName, int frame,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.PlayQueuedAt(animClipName, frame, layer, blendMethod, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.PlayQueuedAt(animPlayData, frame, layer, blendMethod, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueuedAt(apAnimPlayData animPlayData, int frame,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.PlayQueuedAt(animPlayData, frame, layer, blendMethod, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.PlayQueuedAt(animPlayData, frame, layer, blendMethod, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Play the animation at the specified frame smoothly.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeAt(string animClipName, int frame,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFadeAt(animClipName, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.CrossFadeAt(animPlayData, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Play the animation at the specified frame smoothly.
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeAt(apAnimPlayData animPlayData, int frame,
											float fadeTime = 0.3f,
											int layer = 0,
											apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
											apAnimPlayManager.PLAY_OPTION playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer,
											bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFadeAt(animPlayData, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.CrossFadeAt(animPlayData, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueuedAt(string animClipName, int frame,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFadeQueuedAt(animClipName, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				apAnimPlayData animPlayData = _animPlayManager.GetAnimPlayData_Opt(animClipName);
				if (animPlayData == null)
				{
					return null;
				}
				_animPlayDeferredRequest.CrossFadeQueuedAt(animPlayData, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}




		/// <summary>
		/// Wait for the previous animation to finish, then play it at the specified frame smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animPlayData">Target animation playdata</param>
		/// <param name="frame">Frame at the time the animation is played</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueuedAt(apAnimPlayData animPlayData, int frame,
												float fadeTime = 0.3f,
												int layer = 0,
												apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation,
												bool isAutoEndIfNotloop = false)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return null; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다.
					Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return null;
				}

				//이전
				//return _animPlayManager.CrossFadeQueuedAt(animPlayData, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.CrossFadeQueuedAt(animPlayData, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
				return animPlayData;

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif

		}
		//-------------------------------------------------------------

		/// <summary>
		/// End all animations playing on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		/// <param name="fadeTime">Fade Time</param>
		public void StopLayer(int layer, float fadeTime = 0.0f)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
					return;
				}

				//이전
				//_animPlayManager.StopLayer(layer, fadeTime);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.StopLayer(layer, fadeTime);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// End all animations.
		/// </summary>
		/// <param name="fadeTime">Fade Time</param>
		public void StopAll(float fadeTime = 0.0f)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
					return;
				}

				//이전
				//_animPlayManager.StopAll(fadeTime);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.StopAll(fadeTime);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Pause all animations playing on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		public void PauseLayer(int layer)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
					//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return;
				}

				//이전
				//_animPlayManager.PauseLayer(layer);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.PauseLayer(layer);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Pause all animations.
		/// </summary>
		public void PauseAll()
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
					//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return;
				}

				//이전
				//_animPlayManager.PauseAll();

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.PauseAll();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Resume all animations paused on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		public void ResumeLayer(int layer)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
					//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return;
				}

				//이전
				//_animPlayManager.ResumeLayer(layer);

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.ResumeLayer(layer);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Resume all animations.
		/// </summary>
		public void ResumeAll()
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_animPlayManager == null) { return; }

				if (_isUsingMecanim)
				{
					//메카님이 켜진 경우 함수를 제어할 수 없다. > Stop 계열에서는 경고문을 사용하지 말자
					//Debug.LogError("AnyPortrait : This function does not work because Mecanim Animator is active. Please use _animator.");
					return;
				}

				//이전
				//_animPlayManager.ResumeAll();

				//변경 22.5.18 : 지연된 플레이 요청
				_animPlayDeferredRequest.ResumeAll();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Register the listener object to receive animation events. It must be a class inherited from MonoBehaviour.
		/// </summary>
		/// <param name="listenerObject">Listener</param>
		public void RegistAnimationEventListener(MonoBehaviour listenerObject)
		{
			_optAnimEventListener = listenerObject;
		}

		/// <summary>
		/// Animation PlayManager
		/// </summary>
		public apAnimPlayManager PlayManager
		{
			get
			{
				return _animPlayManager;
			}
		}

		/// <summary>
		/// Is Animation Clip Playing?
		/// </summary>
		/// <param name="animClipName"></param>
		/// <returns></returns>
		public bool IsPlaying(string animClipName)
		{
			return _animPlayManager.IsPlaying(animClipName);
		}





		/// <summary>
		/// Sets the speed of the animation.
		/// </summary>
		/// <param name="animClipName"></param>
		/// <param name="speed">Speed of Animation (Default : 1)</param>
		public void SetAnimationSpeed(string animClipName, float speed)
		{
			_animPlayManager.SetAnimSpeed(animClipName, speed);
		}

		/// <summary>
		/// Sets the speed of all animations.
		/// </summary>
		/// <param name="speed">Speed of Animation (Default : 1)</param>
		public void SetAnimationSpeed(float speed)
		{
			_animPlayManager.SetAnimSpeed(speed);
		}

		/// <summary>
		/// Restores the speed of all animations to their default values.
		/// </summary>
		public void ResetAnimationSpeed()
		{
			_animPlayManager.ResetAnimSpeed();
		}

		/// <summary>
		/// Returns a list of AnimPlayData instances with animation information.
		/// </summary>
		public List<apAnimPlayData> AnimationPlayDataList
		{
			get
			{
				return _animPlayManager.PlayDataList;
			}
		}

		/// <summary>
		/// Finds and returns an instance of AnimPlayData with animation information by name.
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public apAnimPlayData GetAnimationPlayData(string animClipName)
		{
			return _animPlayManager.GetAnimPlayData_Opt(animClipName);
		}

		/// <summary>
		/// Returns the status in which the animation plays.
		/// Unlike the IsPlaying () function, it returns an Enum type.
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public apAnimPlayData.AnimationPlaybackStatus GetAnimationPlaybackStatus(string animClipName)
		{
			return _animPlayManager.GetAnimationPlaybackStatus(animClipName);
		}

		//v1.4.2에서 추가

		/// <summary>
		/// Return the current frame at which the animation is played.
		/// (The returned value is of type int, but internally, it is operated with data of type float.)
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public int GetAnimationCurrentFrame(string animClipName)
		{
			return _animPlayManager.GetAnimationCurrentFrame(animClipName);
		}

		/// <summary>
		/// Return "Start Frame" attribute of the animation.
		/// Return -1 if there is no target animation.
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public int GetAnimationStartFrame(string animClipName)
		{
			return _animPlayManager.GetAnimationStartFrame(animClipName);
		}

		/// <summary>
		/// Return "End Frame" attribute of the animation.
		/// Return -1 if there is no target animation.
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public int GetAnimationEndFrame(string animClipName)
		{
			return _animPlayManager.GetAnimationEndFrame(animClipName);
		}

		/// <summary>
		/// Return the playing time as a value between 0 and 1.
		/// Return -1 if there is no target animation clip.
		/// </summary>
		/// <param name="animClipName">Animation Clip Name</param>
		/// <returns></returns>
		public float GetAnimationNormalizedTime(string animClipName)
		{
			return _animPlayManager.GetAnimationNormalizedTime(animClipName);
		}



		//추가 3.6 : 타임라인 관련 함수들
		// 외부 제어 함수들 - 주로 Timeline
#if UNITY_2017_1_OR_NEWER
		/// <summary>
		/// Connect the Timeline to apPortrait.
		/// Enter the PlayableDirector with timeline and the name of Track.
		/// If the connection is successful, it will play automatically when the Timeline is played.
		/// </summary>
		/// <param name="playableDirector">PlayableDirector with Timeline</param>
		/// <param name="trackName">The name of the track to which apPortrait is connected.</param>
		/// <param name="layer">The index when multiple tracks are blended to play the animation. It starts from 0.</param>
		/// <param name="blendMethod">How multiple tracks are blended</param>
		/// <returns>If the connection fails for some reason, such as a wrong name or no PlaybleDirector, false is returned.</returns>
		public bool AddTimelineTrack(UnityEngine.Playables.PlayableDirector playableDirector, string trackName, int layer = 0, apAnimPlayUnit.BLEND_METHOD blendMethod = apAnimPlayUnit.BLEND_METHOD.Additive)
		{
			return _animPlayManager.AddTimelineTrack(playableDirector, trackName, layer, blendMethod);
		}

		/// <summary>
		/// Any invalid Timeline track information is removed.
		/// </summary>
		public void RemoveInvalidTimelineTracks()
		{
			_animPlayManager.RemoveInvalidTimelineTracks();
		}

		/// <summary>
		/// All Timeline track data are removed.
		/// </summary>
		public void RemoveAllTimelineTracks()
		{
			_animPlayManager.RemoveAllTimelineTracks();
		}

		/// <summary>
		/// Track information containing the requested PlayableDirector will be deleted.
		/// </summary>
		/// <param name="playableDirector">A PlayableDirector that contains the track information you want to remove.</param>
		public void UnlinkTimelinePlayableDirector(UnityEngine.Playables.PlayableDirector playableDirector)
		{
			_animPlayManager.UnlinkTimelinePlayableDirector(playableDirector);
		}

		/// <summary>
		/// If False is entered, apPortrait is not under the control of the Timeline, even if it is associated with the Timeline.
		/// </summary>
		/// <param name="isEnabled">Whether to be controlled from the Timeline. (Default is True)</param>
		public void SetTimelineEnable(bool isEnabled)
		{
			_animPlayManager.SetTimelineEnable(isEnabled);
		}

#endif


		//---------------------------------------------------------------------------------------
		// apPortrait의 외부 노출 함수에서 초기화 안된 경우 안내문 보여주기 (에디터만)
		//---------------------------------------------------------------------------------------

#if UNITY_EDITOR
		//v1.4.7 : 초기화 전에 실행했다면 에러 원인에 대한 로그를 보여주자
		private void ShowErrorMsgIfNotInitialized_Editor(Exception ex)
		{	
			if(InitializationStatus != INIT_STATUS.Completed)
			{
				Debug.LogWarning("AnyPortrait : An error occurred because the function was called before [Initialization].\n"
								+ "Please call the Initialize() function directly or try again after initialization (approx. 1 frame).\n"
								+ "Please check the manual for more details.\n"
								+ "( https://rainyrizzle.github.io/en/AdvancedManual/AD_InitializeScript.html )", this.gameObject);
			}
			Debug.LogException(ex, this.gameObject);
		}
#endif
		//---------------------------------------------------------------------------------------
		// 물리 제어
		//---------------------------------------------------------------------------------------
		/// <summary>
		/// Initialize all forces and physical effects by touch.
		/// This function is equivalent to executing "ClearForce()" and "ClearTouch()" together.
		/// </summary>
		public void ClearForceAndTouch()
		{
			_forceManager.ClearAll();
		}

		/// <summary>Initialize all physical forces.</summary>
		public void ClearForce()
		{
			_forceManager.ClearForce();
		}

		/// <summary>
		/// Remove the target force.
		/// </summary>
		/// <param name="forceUnit">The target force you want to remove</param>
		public void RemoveForce(apForceUnit forceUnit)
		{
			_forceManager.RemoveForce(forceUnit);
		}




		/// <summary>
		/// Adds force applied radially at a specific point.
		/// </summary>
		/// <param name="pointPosW">Center position of force in world space</param>
		/// <param name="radius">Radius to which force is applied</param>
		/// <returns>Applied force information</returns>
		public apForceUnit AddForce_Point(Vector2 pointPosW, float radius)
		{
			return _forceManager.AddForce_Point(pointPosW, radius);
		}

		/// <summary>
		/// Add force with direction.
		/// </summary>
		/// <param name="directionW">Direction vector</param>
		/// <returns>Applied force information</returns>
		public apForceUnit AddForce_Direction(Vector2 directionW)
		{
			return _forceManager.AddForce_Direction(directionW);
		}

		/// <summary>
		/// Add a force that changes direction periodically.
		/// </summary>
		/// <param name="directionW">Direction vector</param>
		/// <param name="waveSizeX">How much the direction changes on the X axis</param>
		/// <param name="waveSizeY">How much the direction changes on the Y axis</param>
		/// <param name="waveTimeX">The time the force changes on the X axis</param>
		/// <param name="waveTimeY">The time the force changes on the Y axis</param>
		/// <returns>Applied force information</returns>
		public apForceUnit AddForce_Direction(Vector2 directionW, float waveSizeX, float waveSizeY, float waveTimeX, float waveTimeY)
		{
			return _forceManager.AddForce_Direction(directionW, new Vector2(waveSizeX, waveSizeY), new Vector2(waveTimeX, waveTimeY));
		}

		/// <summary>
		/// Is any force being applied?
		/// </summary>
		public bool IsAnyForceEvent
		{
			get { return _forceManager.IsAnyForceEvent; }
		}

		/// <summary>
		/// The force applied at the requested position is calculated
		/// </summary>
		/// <param name="targetPosW">Position in world space</param>
		/// <returns>Calculated Force</returns>
		public Vector2 GetForce(Vector2 targetPosW)
		{
			return _forceManager.GetForce(targetPosW);
		}

		/// <summary>
		/// Add a physics effect to pull meshes using the touch.
		/// </summary>
		/// <param name="posW">First touch position in world space</param>
		/// <param name="radius">Radius of pulling force</param>
		/// <returns>Added touch information with "TouchID"</returns>
		public apPullTouch AddTouch(Vector2 posW, float radius)
		{
			return _forceManager.AddTouch(posW, radius);
		}

		/// <summary>Initialize all physical forces by touch.</summary>
		public void ClearTouch()
		{
			_forceManager.ClearTouch();
		}

		/// <summary>
		/// Removes physical effects by touch with the requested ID.
		/// </summary>
		/// <param name="touchID">Touch ID</param>
		public void RemoveTouch(int touchID)
		{
			_forceManager.RemoveTouch(touchID);
		}

		/// <summary>
		/// Removes physical effects by touch with the requested Data.
		/// </summary>
		/// <param name="touchID">Touch Data</param>
		public void RemoveTouch(apPullTouch touch)
		{
			_forceManager.RemoveTouch(touch);
		}

		/// <summary>
		/// Returns a physical effect by touch with the requested ID.
		/// </summary>
		/// <param name="touchID">Touch ID</param>
		/// <returns>Requested touch information (return null if touchID is not valid)</returns>
		public apPullTouch GetTouch(int touchID)
		{
			return _forceManager.GetTouch(touchID);
		}

		/// <summary>
		/// Update the position of the added touch.
		/// </summary>
		/// <param name="touchID">Touch ID</param>
		/// <param name="posW">World Position</param>
		public void SetTouchPosition(int touchID, Vector2 posW)
		{
			_forceManager.SetTouchPosition(touchID, posW);
		}

		/// <summary>
		/// Update the position of the added touch.
		/// </summary>
		/// <param name="touch">Added Touch information</param>
		/// <param name="posW">World Position</param>
		public void SetTouchPosition(apPullTouch touch, Vector2 posW)
		{
			_forceManager.SetTouchPosition(touch, posW);
		}

		/// <summary>
		/// Is any force by touch being applied?
		/// </summary>
		public bool IsAnyTouchEvent { get { return _forceManager.IsAnyTouchEvent; } }

		/// <summary>
		/// [Please do not use it] Temporary code used for touch calculations
		/// </summary>
		public int TouchProcessCode { get { return _forceManager.TouchProcessCode; } }



		//--------------------------------------------------------------------------------------
		// Control Param 제어 요청
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Set the value of the Control Parameter with an "Integer" value.
		/// </summary>
		/// <param name="controlParamName">Name of the target Control Parameter</param>
		/// <param name="intValue">Integer Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamInt(string controlParamName, int intValue, float overlapWeight = 1.0f)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueInt(intValue, overlapWeight);

			//controlParam._int_Cur = intValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._int_Cur = Mathf.Clamp(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
			//}

			return true;
		}

		/// <summary>
		/// Set the value of the Control Parameter with an "Float" value.
		/// </summary>
		/// <param name="controlParamName">Name of the target Control Parameter</param>
		/// <param name="floatValue">Float Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamFloat(string controlParamName, float floatValue, float overlapWeight = 1.0f)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueFloat(floatValue, overlapWeight);

			//controlParam._float_Cur = floatValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._float_Cur = Mathf.Clamp(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);
			//}

			return true;
		}

		/// <summary>
		/// Set the value of the Control Parameter with an "Vector2" value.
		/// </summary>
		/// <param name="controlParamName">Name of the target Control Parameter</param>
		/// <param name="vec2Value">Vector2 Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamVector2(string controlParamName, Vector2 vec2Value, float overlapWeight = 1.0f)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueVector2(vec2Value, overlapWeight);

			//controlParam._vec2_Cur = vec2Value;
			////if(controlParam._isRange)
			//{
			//	controlParam._vec2_Cur.x = Mathf.Clamp(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
			//	controlParam._vec2_Cur.y = Mathf.Clamp(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);
			//}

			return true;
		}


		/// <summary>
		/// Set the value of the Control Parameter with an "Integer" value.
		/// </summary>
		/// <param name="controlParam">Tareget Control Parameter</param>
		/// <param name="intValue">Integer Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamInt(apControlParam controlParam, int intValue, float overlapWeight = 1.0f)
		{
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueInt(intValue, overlapWeight);

			//controlParam._int_Cur = intValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._int_Cur = Mathf.Clamp(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
			//}

			return true;
		}

		/// <summary>
		/// Set the value of the Control Parameter with an "Float" value.
		/// </summary>
		/// <param name="controlParam">Target Control Parameter</param>
		/// <param name="floatValue">Float Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamFloat(apControlParam controlParam, float floatValue, float overlapWeight = 1.0f)
		{
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueFloat(floatValue, overlapWeight);

			//controlParam._float_Cur = floatValue;
			////if(controlParam._isRange)
			//{
			//	controlParam._float_Cur = Mathf.Clamp(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);
			//}

			return true;
		}


		/// <summary>
		/// Set the value of the Control Parameter with an "Vector2" value.
		/// </summary>
		/// <param name="controlParam">Target Control Parameter</param>
		/// <param name="vec2Value">Vector2 Value</param>
		/// <param name="overlapWeight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamVector2(apControlParam controlParam, Vector2 vec2Value, float overlapWeight = 1.0f)
		{
			if (controlParam == null)
			{ return false; }

			controlParam.RequestSetValueVector2(vec2Value, overlapWeight);

			//controlParam._vec2_Cur = vec2Value;
			////if(controlParam._isRange)
			//{
			//	controlParam._vec2_Cur.x = Mathf.Clamp(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
			//	controlParam._vec2_Cur.y = Mathf.Clamp(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);
			//}

			return true;
		}


		/// <summary>
		/// Is there a parameter with the requested name?
		/// </summary>
		/// <param name="controlParamName">Name of Control Parameter</param>
		/// <returns></returns>
		public bool IsControlParamExist(string controlParamName)
		{
			return GetControlParam(controlParamName) != null;
		}

		/// <summary>
		/// Restores the value of the parameter to its default value.
		/// </summary>
		/// <param name="controlParamName">Name of Control Parameter</param>
		/// <returns>If the requested parameter is not found, it returns false.</returns>
		public bool SetControlParamDefaultValue(string controlParamName)
		{
			apControlParam controlParam = GetControlParam(controlParamName);
			if (controlParam == null)
			{ return false; }

			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					controlParam._int_Cur = controlParam._int_Def;
					break;

				case apControlParam.TYPE.Float:
					controlParam._float_Cur = controlParam._float_Def;
					break;

				case apControlParam.TYPE.Vector2:
					controlParam._vec2_Cur = controlParam._vec2_Def;
					break;
			}

			return true;
		}


		//--------------------------------------------------------------------------------------------------
		// Bone Transform 요청 (Rotation, Scale, Position-IK, LookAt)
		// 요청된 Bone을 검색하는 기능도 추가하고, 한번 검색된 Bone은 별도의 리스트로 넣어서 관리하자
		//--------------------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the Bone with the requested name.
		/// (It first searches for the currently executing Root Unit, and returns the bones retrieved from all Root Units.)
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <returns>Optimized Bone</returns>
		public apOptBone GetBone(string boneName)
		{
			//일단 "현재 재생중인 RootUnit"에서 검색하고,
			//그 다음에 "전체 목록"에서 검색한다.
			apOptBone resultBone = null;
			if (_curPlayingOptRootUnit != null)
			{
				resultBone = _curPlayingOptRootUnit.GetBone(boneName);

				if (resultBone != null)
				{
					return resultBone;
				}
			}

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				resultBone = _optRootUnitList[i].GetBone(boneName);
				if (resultBone != null)
				{
					return resultBone;
				}
			}

			return null;

		}

		/// <summary>
		/// Finds the Bone with the requested name in a specific Root Unit and returns it.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <returns>Optimized Bone</returns>
		public apOptBone GetBone(int rootUnitIndex, string boneName)
		{
			if (rootUnitIndex < 0 || rootUnitIndex >= _optRootUnitList.Count)
			{
				Debug.LogError("AnyPortrait : GetBone() Failed. The index is out of the list range. [" + rootUnitIndex + " / " + _optRootUnitList.Count + "]");
				return null;
			}

			return _optRootUnitList[rootUnitIndex].GetBone(boneName);
		}

		/// <summary>
		/// If there is a socket for this bone, it returns a Transform of the socket.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <returns></returns>
		public Transform GetBoneSocket(apOptBone optBone)
		{
			if (optBone == null) { return null; }
			return optBone._socketTransform;
		}

		/// <summary>
		/// If there is a socket for this bone, it returns a Transform of the socket.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <returns></returns>
		public Transform GetBoneSocket(string boneName)
		{
			apOptBone optBone = GetBone(boneName);
			if (optBone == null) { return null; }
			return optBone._socketTransform;
		}

		/// <summary>
		/// If there is a socket for this bone, it returns a Transform of the socket.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <returns></returns>
		public Transform GetBoneSocket(int rootUnitIndex, string boneName)
		{
			apOptBone optBone = GetBone(rootUnitIndex, boneName);
			if (optBone == null) { return null; }
			return optBone._socketTransform;
		}

		/// <summary>
		/// Set the position of the bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBonePosition(apOptBone optBone, Vector3 position, Space space, float weight = 1.0f)
		{
			if (optBone == null) { return false; }
			if (optBone._parentOptTransform == null) { return false; }
			if (optBone._parentOptTransform._rootUnit == null) { return false; }

			if (space == Space.World) { position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position); }
			optBone.SetPosition(position, weight);
			return true;
		}

		/// <summary>
		/// Set the rotation of the bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneRotation(apOptBone optBone, float angle, Space space, float weight = 1.0f)
		{
			if (optBone == null) { return false; }
			if (optBone._parentOptTransform == null) { return false; }
			if (optBone._parentOptTransform._rootUnit == null) { return false; }

			if (space == Space.World) { angle -= optBone._parentOptTransform._rootUnit._transform.rotation.eulerAngles.z; }
			angle -= 90.0f;
			angle = apUtil.AngleTo180(angle);

			optBone.SetRotation(angle, weight);
			return true;
		}


		/// <summary>
		/// Set the scale of the bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneScale(apOptBone optBone, Vector3 scale, Space space, float weight = 1.0f)
		{
			if (optBone == null) { return false; }
			if (optBone._parentOptTransform == null) { return false; }
			if (optBone._parentOptTransform._rootUnit == null) { return false; }

			if (space == Space.World)
			{
				scale.x /= optBone._parentOptTransform._rootUnit._transform.lossyScale.x;
				scale.y /= optBone._parentOptTransform._rootUnit._transform.lossyScale.y;
			}
			optBone.SetScale(scale, weight);
			return true;
		}

		// Overloads

		/// <summary>
		/// Set the position of the bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBonePosition(string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBonePosition(GetBone(boneName), position, space, weight);
		}


		/// <summary>
		/// Set the position of the bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBonePosition(int rootUnitIndex, string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBonePosition(GetBone(rootUnitIndex, boneName), position, space, weight);
		}

		/// <summary>
		/// Set the rotation of the bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneRotation(string boneName, float angle, Space space, float weight = 1.0f)
		{
			return SetBoneRotation(GetBone(boneName), angle, space, weight);
		}

		/// <summary>
		/// Set the rotation of the bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneRotation(int rootUnitIndex, string boneName, float angle, Space space, float weight = 1.0f)
		{
			return SetBoneRotation(GetBone(rootUnitIndex, boneName), angle, space, weight);
		}


		/// <summary>
		/// Set the scale of the bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneScale(string boneName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetBoneScale(GetBone(boneName), scale, space, weight);
		}

		/// <summary>
		/// Set the scale of the bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>If the bone does not exist, it returns false.</returns>
		public bool SetBoneScale(int rootUnitIndex, string boneName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetBoneScale(GetBone(rootUnitIndex, boneName), scale, space, weight);
		}

		// Bone IK

		/// <summary>
		/// Set the position of the bone according to the IK function. IK is calculated according to the connected Bone setting.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="position">Target Position. Depending on the IK calculation, it may not be able to move to the requested location.</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <param name="isContinuous">If this value is True, the IK calculation refers to the previous frame and produces good results.</param>
		/// <returns>Returns false if there is no bone or IK is not possible.</returns>
		public bool SetBoneIK(apOptBone optBone, Vector3 position, Space space, float weight = 1.0f, bool isContinuous = true)
		{
			if (optBone == null)
			{
				Debug.LogError("AnyPortrait : No Opt Bone");
				return false;
			}
			if (optBone._parentOptTransform == null)
			{
				Debug.LogError("AnyPortrait : No Opt Transform");
				return false;
			}
			if (optBone._parentOptTransform._rootUnit == null)
			{
				Debug.LogError("AnyPortrait : No Opt Root Unit");
				return false;
			}

			if (space == Space.World)
			{
				position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position);
			}

			return optBone.RequestIK(position, Mathf.Clamp01(weight), isContinuous, _IKMethod);
		}

		/// <summary>
		/// Set the position of the bone according to the IK function. IK is calculated according to the connected Bone setting.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Target Position. Depending on the IK calculation, it may not be able to move to the requested location.</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <param name="isContinuous">If this value is True, the IK calculation refers to the previous frame and produces good results.</param>
		/// <returns>Returns false if there is no bone or IK is not possible.</returns>
		public bool SetBoneIK(string boneName, Vector3 position, Space space, float weight = 1.0f, bool isContinuous = true)
		{
			return SetBoneIK(GetBone(boneName), position, space, weight, isContinuous);
		}

		/// <summary>
		/// Set the position of the bone according to the IK function. IK is calculated according to the connected Bone setting.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Target Position. Depending on the IK calculation, it may not be able to move to the requested location.</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <param name="isContinuous">If this value is True, the IK calculation refers to the previous frame and produces good results.</param>
		/// <returns>Returns false if there is no bone or IK is not possible.</returns>
		public bool SetBoneIK(int rootUnitIndex, string boneName, Vector3 position, Space space, float weight = 1.0f, bool isContinuous = true)
		{
			return SetBoneIK(GetBone(rootUnitIndex, boneName), position, space, weight, isContinuous);
		}


		// Bone IK

		/// <summary>
		/// Let the bone look at the requested point. Similar to IK, except that it is targeted to one bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBoneLookAt(apOptBone optBone, Vector3 position, Space space, float weight = 1.0f)
		{
			if (optBone == null) { return false; }
			if (optBone._parentOptTransform == null) { return false; }
			if (optBone._parentOptTransform._rootUnit == null) { return false; }

			if (space == Space.World) { position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position); }

			//이전
			//float angle = Mathf.Atan2(position.y - optBone.PositionWithouEditing.y, position.x - optBone.PositionWithouEditing.x) * Mathf.Rad2Deg;
			//angle -= optBone._defaultMatrix._angleDeg;

			//변경 > 이것도 미사용
			//IKSpace로 옮겨야 한다.
			//Vector2 targetPos = optBone._worldMatrix.ConvertForIK(position);
			//Vector2 startPos = optBone._worldMatrix.ConvertForIK(optBone.PositionWithouEditing);
			
			//float angle = Mathf.Atan2(targetPos.y - startPos.y, targetPos.x - startPos.x) * Mathf.Rad2Deg;

			//이전 : IK Angle에 넣어서 계산하기
			// > 문제 : v1.5.0에서는 중첩된 IK 연산이 지원되면서 연산 순서가 중요해졌는데, 이 함수는 IK 연산보다 먼저 실행되면서 IK 결과가 덮어 씌워지게 된다.
			//optBone.AddIKAngle(apUtil.AngleTo180(angle), 0.0f, weight);

			//다시 변경 v1.5.0
			//입력값 그대로 넣은 후에 다른 연산이 끝난 후 연산한다.
			optBone.SetLookAtExternal(position, weight);
			return true;
		}


		/// <summary>
		/// Let the bone look at the requested point. Similar to IK, except that it is targeted to one bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBoneLookAt(string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBoneLookAt(GetBone(boneName), position, space, weight);
		}


		/// <summary>
		/// Let the bone look at the requested point. Similar to IK, except that it is targeted to one bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBoneLookAt(int rootUnitIndex, string boneName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetBoneLookAt(GetBone(rootUnitIndex, boneName), position, space, weight);
		}


		//1.0.4 Functions
		/// <summary>
		/// Limits the updated location of the Bone.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="positionValue">Position value to limit</param>
		/// <param name="constraintBound">Types of axis and range to limit</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraint(apOptBone optBone, float positionValue, ConstraintBound constraintBound, Space space)
		{
			if (optBone == null) { return false; }
			if (optBone._parentOptTransform == null) { return false; }
			if (optBone._parentOptTransform._rootUnit == null) { return false; }

			if (space == Space.World)
			{
				Vector3 position = optBone._parentOptTransform._rootUnit._transform.position;
				switch (constraintBound)
				{
					case ConstraintBound.Xprefer:
					case ConstraintBound.Xmin:
					case ConstraintBound.Xmax:
						position.x = positionValue;
						break;

					case ConstraintBound.Yprefer:
					case ConstraintBound.Ymin:
					case ConstraintBound.Ymax:
						position.y = positionValue;
						break;
				}
				position = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(position);
				switch (constraintBound)
				{
					case ConstraintBound.Xprefer:
					case ConstraintBound.Xmin:
					case ConstraintBound.Xmax:
						positionValue = position.x;
						break;

					case ConstraintBound.Yprefer:
					case ConstraintBound.Ymin:
					case ConstraintBound.Ymax:
						positionValue = position.y;
						break;
				}
			}
			optBone.SetPositionConstraint(positionValue, constraintBound);
			return true;
		}

		/// <summary>
		/// Limits the updated location of the Bone.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="positionValue">Position value to limit</param>
		/// <param name="constraintBound">Types of axis and range to limit</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraint(string boneName, float positionValue, ConstraintBound constraintBound, Space space)
		{
			return SetBonePositionConstraint(GetBone(boneName), positionValue, constraintBound, space);
		}

		/// <summary>
		/// Limits the updated location of the Bone.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="positionValue">Position value to limit</param>
		/// <param name="constraintBound">Types of axis and range to limit</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraint(int rootUnitIndex, string boneName, float positionValue, ConstraintBound constraintBound, Space space)
		{
			return SetBonePositionConstraint(GetBone(rootUnitIndex, boneName), positionValue, constraintBound, space);
		}


		/// <summary>
		/// Adjusts the current position based on the default position of the bone. 
		/// You can create an effect that moves the surface by entering the location of the ground or wall.
		/// </summary>
		/// <param name="optBone">Target Bone</param>
		/// <param name="defaultSurfacePosition">Default location to be recognized as surface</param>
		/// <param name="currentSurfacePosition">Current position to be recognized as a surface</param>
		/// <param name="minSurfacePosition">Minimum value of surface position</param>
		/// <param name="maxSurfacePosition">Maximum value of surface position</param>
		/// <param name="constraintSurface">Axis of the surface</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraintBySurface(apOptBone optBone,
														float defaultSurfacePosition,
														float currentSurfacePosition,
														float minSurfacePosition,
														float maxSurfacePosition,
														ConstraintSurface constraintSurface, Space space)
		{
			if (optBone == null) { return false; }
			if (optBone._parentOptTransform == null) { return false; }
			if (optBone._parentOptTransform._rootUnit == null) { return false; }

			if (space == Space.World)
			{
				Vector3 defaultPos = optBone._parentOptTransform._rootUnit._transform.position;
				Vector3 currentPos = defaultPos;
				Vector3 minPos = defaultPos;
				Vector3 maxPos = defaultPos;
				switch (constraintSurface)
				{
					case ConstraintSurface.Xsurface:
						defaultPos.x = defaultSurfacePosition;
						currentPos.x = currentSurfacePosition;
						minPos.x = minSurfacePosition;
						maxPos.x = maxSurfacePosition;
						break;

					case ConstraintSurface.Ysurface:
						defaultPos.y = defaultSurfacePosition;
						currentPos.y = currentSurfacePosition;
						minPos.y = minSurfacePosition;
						maxPos.y = maxSurfacePosition;
						break;
				}
				defaultPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(defaultPos);
				currentPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(currentPos);
				minPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(minPos);
				maxPos = optBone._parentOptTransform._rootUnit._transform.InverseTransformPoint(maxPos);

				switch (constraintSurface)
				{
					case ConstraintSurface.Xsurface:
						defaultSurfacePosition = defaultPos.x;
						currentSurfacePosition = currentPos.x;
						minSurfacePosition = minPos.x;
						maxSurfacePosition = maxPos.x;
						break;

					case ConstraintSurface.Ysurface:
						defaultSurfacePosition = defaultPos.y;
						currentSurfacePosition = currentPos.y;
						minSurfacePosition = minPos.y;
						maxSurfacePosition = maxPos.y;
						break;
				}
			}
			optBone.SetPositionConstraintSurface(defaultSurfacePosition, currentSurfacePosition, minSurfacePosition, maxSurfacePosition, constraintSurface);
			return true;
		}


		/// <summary>
		/// Adjusts the current position based on the default position of the bone. 
		/// You can create an effect that moves the surface by entering the location of the ground or wall.
		/// </summary>
		/// <param name="boneName">Bone Name</param>
		/// <param name="defaultSurfacePosition">Default location to be recognized as surface</param>
		/// <param name="currentSurfacePosition">Current position to be recognized as a surface</param>
		/// <param name="minSurfacePosition">Minimum value of surface position</param>
		/// <param name="maxSurfacePosition">Maximum value of surface position</param>
		/// <param name="constraintSurface">Axis of the surface</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraintBySurface(string boneName,
														float defaultSurfacePosition,
														float currentSurfacePosition,
														float minSurfacePosition,
														float maxSurfacePosition,
														ConstraintSurface constraintSurface, Space space)
		{
			return SetBonePositionConstraintBySurface(GetBone(boneName),
														defaultSurfacePosition,
														currentSurfacePosition,
														minSurfacePosition,
														maxSurfacePosition,
														constraintSurface, space);
		}


		/// <summary>
		/// Adjusts the current position based on the default position of the bone. 
		/// You can create an effect that moves the surface by entering the location of the ground or wall.
		/// </summary>
		/// /// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="boneName">Bone Name</param>
		/// <param name="defaultSurfacePosition">Default location to be recognized as surface</param>
		/// <param name="currentSurfacePosition">Current position to be recognized as a surface</param>
		/// <param name="minSurfacePosition">Minimum value of surface position</param>
		/// <param name="maxSurfacePosition">Maximum value of surface position</param>
		/// <param name="constraintSurface">Axis of the surface</param>
		/// <param name="space">Space of Position</param>
		/// <returns>Returns false if there is no bone or computation is not possible.</returns>
		public bool SetBonePositionConstraintBySurface(int rootUnitIndex,
														string boneName,
														float defaultSurfacePosition,
														float currentSurfacePosition,
														float minSurfacePosition,
														float maxSurfacePosition,
														ConstraintSurface constraintSurface, Space space)
		{
			return SetBonePositionConstraintBySurface(GetBone(rootUnitIndex, boneName),
														defaultSurfacePosition,
														currentSurfacePosition,
														minSurfacePosition,
														maxSurfacePosition,
														constraintSurface, space);
		}
		//-------------------------------------------------------------------------------------------------------
		// OptTransform에 대한 참조/제어
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Find the Optimized Transform and return it.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public apOptTransform GetOptTransform(int rootUnitIndex, string transformName)
		{
			if (rootUnitIndex < 0 || rootUnitIndex >= _optRootUnitList.Count)
			{
				Debug.LogError("AnyPortrait : GetOptTransform() Failed. The index is out of the list range. [" + rootUnitIndex + " / " + _optRootUnitList.Count + "]");
				return null;
			}

			return _optRootUnitList[rootUnitIndex].GetTransform(transformName);
		}

		/// <summary>
		/// Find the Optimized Transform and return it.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public apOptTransform GetOptTransform(string transformName)
		{
			//일단 "현재 재생중인 RootUnit"에서 검색하고,
			//그 다음에 "전체 목록"에서 검색한다.
			apOptTransform resultTransform = null;
			if (_curPlayingOptRootUnit != null)
			{
				resultTransform = _curPlayingOptRootUnit.GetTransform(transformName);

				if (resultTransform != null)
				{
					return resultTransform;
				}
			}

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				resultTransform = _optRootUnitList[i].GetTransform(transformName);
				if (resultTransform != null)
				{
					return resultTransform;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns the socket of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <returns></returns>
		public Transform GetOptTransformSocket(apOptTransform optTransform)
		{
			if (optTransform == null) { return null; }
			return optTransform._socketTransform;
		}

		/// <summary>
		/// Returns the socket of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public Transform GetOptTransformSocket(string transformName)
		{
			apOptTransform optTransform = GetOptTransform(transformName);
			if (optTransform == null) { return null; }
			return optTransform._socketTransform;
		}

		/// <summary>
		/// Returns the socket of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <returns></returns>
		public Transform GetOptTransformSocket(int rootUnitIndex, string transformName)
		{
			apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
			if (optTransform == null) { return null; }
			return optTransform._socketTransform;
		}


		/// <summary>
		/// Set the position of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformPosition(apOptTransform optTransform, Vector3 position, Space space, float weight = 1.0f)
		{
			if (optTransform == null) { return false; }
			if (optTransform._rootUnit == null) { return false; }

			if (space == Space.World) { position = optTransform._rootUnit._transform.InverseTransformPoint(position); }
			optTransform.SetPosition(position, weight);
			return true;
		}


		/// <summary>
		/// Set the rotation of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformRotation(apOptTransform optTransform, float angle, Space space, float weight = 1.0f)
		{
			if (optTransform == null) { return false; }
			if (optTransform._rootUnit == null) { return false; }

			if (space == Space.World) { angle -= optTransform._rootUnit._transform.rotation.eulerAngles.z; }
			angle -= 90.0f;
			angle = apUtil.AngleTo180(angle);

			optTransform.SetRotation(angle, weight);
			return true;
		}


		/// <summary>
		/// Set the scale of the Optimized Transform.
		/// </summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Scale</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformScale(apOptTransform optTransform, Vector3 scale, Space space, float weight = 1.0f)
		{
			if (optTransform == null) { return false; }
			if (optTransform._rootUnit == null) { return false; }

			if (space == Space.World)
			{
				scale.x /= optTransform._rootUnit._transform.lossyScale.x;
				scale.y /= optTransform._rootUnit._transform.lossyScale.y;
			}
			optTransform.SetScale(scale, weight);
			return true;
		}

		/// <summary>
		/// Set the position of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformPosition(int rootUnitIndex, string transformName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetOptTransformPosition(GetOptTransform(rootUnitIndex, transformName), position, space, weight);
		}

		/// <summary>
		/// Set the position of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="position">Position</param>
		/// <param name="space">Space of Position</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformPosition(string transformName, Vector3 position, Space space, float weight = 1.0f)
		{
			return SetOptTransformPosition(GetOptTransform(transformName), position, space, weight);
		}


		/// <summary>
		/// Set the rotation of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformRotation(int rootUnitIndex, string transformName, float angle, Space space, float weight = 1.0f)
		{
			return SetOptTransformRotation(GetOptTransform(rootUnitIndex, transformName), angle, space, weight);
		}


		/// <summary>
		/// Set the rotation of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="angle">Angle (Degree)</param>
		/// <param name="space">Space of Rotation</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformRotation(string transformName, float angle, Space space, float weight = 1.0f)
		{
			return SetOptTransformRotation(GetOptTransform(transformName), angle, space, weight);
		}


		/// <summary>
		/// Set the scale of the Optimized Transform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Scale</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformScale(int rootUnitIndex, string transformName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetOptTransformScale(GetOptTransform(rootUnitIndex, transformName), scale, space, weight);
		}


		/// <summary>
		/// Set the scale of the Optimized Transform.
		/// </summary>
		/// <param name="transformName">Opt-Transform Name</param>
		/// <param name="scale">Scale</param>
		/// <param name="space">Space of Scale</param>
		/// <param name="weight">The degree to which the value is applied (0.0 ~ 1.0)</param>
		/// <returns>Returns False if the transform does not exist or can not be computed.</returns>
		public bool SetOptTransformScale(string transformName, Vector3 scale, Space space, float weight = 1.0f)
		{
			return SetOptTransformScale(GetOptTransform(transformName), scale, space, weight);
		}



		//-------------------------------------------------------------------------------------------------------
		// 텍스쳐 교체. 
		// Opt Transform 하나만 바꾸거나
		// 전체 Atlas를 교체한다.
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Find and return the texture applied to Opt-Meshes.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <returns></returns>
		public apOptTextureData GetOptTextureData(string optTextureName)
		{
			if (_optTextureData == null || _optTextureData.Count == 0)
			{
				return null;
			}

			//[v1.4.0] 추가 22.6.8 : 매핑 이용
			if (_mapping_OptTextureData != null)
			{
				apOptTextureData result = null;
				_mapping_OptTextureData.TryGetValue(optTextureName, out result);
				if (result != null)
				{
					return result;
				}
			}

			//실패시
			//이전 (GC 발생)
			//return _optTextureData.Find(delegate (apOptTextureData a)
			//{
			//	return string.Equals(a._name, optTextureName);
			//});

			//변경 v1.5.0
			s_GetOptTextureData_TextureName = optTextureName;
			return _optTextureData.Find(s_GetOptTextureDataByName_Func);
		}

		private static string s_GetOptTextureData_TextureName = null;
		private static Predicate<apOptTextureData> s_GetOptTextureDataByName_Func = FUNC_GetOptTextureDataByName;
		private static bool FUNC_GetOptTextureDataByName(apOptTextureData a)
		{
			return string.Equals(a._name, s_GetOptTextureData_TextureName);
		}


		//추가 : Batched Material 속성 초기화 (다시 Shared로 유도하기 위해)
		/// <summary>
		/// Initializes the properties of the all materials in a batch.
		/// (It also works when using Merged Mateial.)
		/// </summary>
		public void ResetMeshMaterialToBatchAll()
		{
#if UNITY_EDITOR
			try
			{
#endif
				//추가 21.12.27 : 병합된 경우를 우선시 한다.
				if (_isUseMergedMat && _optMergedMaterial != null)
				{
					_optMergedMaterial.ResetAllProperties();
				}
				else
				{
					//그렇지 않은 경우 (대다수)
					_optBatchedMaterial.ResetAllProperties();
				}

				//추가 v1.5.0 : Extra 설정도 초기화하기 위해서 텍스쳐의 Extra 참조 데이터도 초기화한다.
				ResetTextureForExtraOptionAll();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//추가 : Batched Material 속성 초기화 (다시 Shared로 유도하기 위해)
		/// <summary>
		/// Initializes the properties of the material in a batch.
		/// </summary>
		/// <param name="optTextureName">Target Texture Name (same as "Image" Name of AnyPortrait Editor)</param>
		public void ResetMeshMaterialToBatchByTextureName(string optTextureName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [ResetMeshMaterialToBatchByTextureName(string)] does not work when using the Merged Material.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);
				if (targetTextureData == null) { return; }

				_optBatchedMaterial.ResetProperties(targetTextureData._textureID);

				//추가 v1.5.0 : Extra 설정도 초기화하기 위해서 텍스쳐의 Extra 참조 데이터도 초기화한다.
				targetTextureData.SetRuntimeTexture(null);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		//[추가 v1.5.0]
		//메시의 이미지는 아니지만 Extra 옵션에 의해서 참조될 수 있는 텍스쳐 데이터를 초기화한다.
		/// <summary>
		/// Reset texture data referenced by Extra Option to initial values.
		/// </summary>
		public void ResetTextureForExtraOptionAll()
		{
			int nTextures = _optTextureData != null ? _optTextureData.Count : 0;
			if(nTextures == 0)
			{
				return;
			}

			for (int i = 0; i < nTextures; i++)
			{
				_optTextureData[i].SetRuntimeTexture(null);
			}
		}

		/// <summary>
		/// Reset specific texture data referenced by the Extra Option to its initial value.
		/// </summary>
		/// <param name="optTextureName">Target Texture Name (same as "Image" Name of AnyPortrait Editor)</param>
		public void ResetTextureForExtraOptionByName(string optTextureName)
		{
			int nTextures = _optTextureData != null ? _optTextureData.Count : 0;
			if(nTextures == 0)
			{
				return;
			}

			apOptTextureData targetTextureData = GetOptTextureData(optTextureName);
			if(targetTextureData != null)
			{
				targetTextureData.SetRuntimeTexture(null);
			}
		}




		/// <summary>
		/// Replaces the main texture applied to meshes in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImageAll(string optTextureName, Texture2D texture)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshImageAll(string, Texture2D)] does not work when using the Merged Material.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);
				if (targetTextureData == null) { return; }

				//targetTextureData.SetMeshTextureAll(texture);//<<이전 : 이 함수를 이용하면 모두 Instanced Material이 된다.

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshImageAll(targetTextureData._textureID, texture);


				//[v1.5.0 변경]
				//일반 이미지의 경우, Extra Option을 대비하기 위해서도 텍스쳐를 교체할 필요가 있다.
				targetTextureData.SetRuntimeTexture(texture);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Change the texture of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomImageAll(string optTextureName, Texture2D texture, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif

				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomImageAll(string, Texture2D, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomImageAll(Texture2D, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomImageAll(targetTextureData._textureID, texture, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Change the texture of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomImageAll(string optTextureName, Texture2D texture, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
#if UNITY_EDITOR
			try
			{
#endif

				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomImageAll(string, Texture2D, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomImageAll(Texture2D, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomImageAll(targetTextureData._textureID, texture, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Replace the main color applied to meshes in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColorAll(string optTextureName, Color color2X)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshColorAll(string, Color)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshColorAll(Color)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }
				//targetTextureData.SetMeshColorAll(color2X);//<<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshColorAll(targetTextureData._textureID, color2X);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Replace the main color's alpha applied to meshes in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		public void SetMeshAlphaAll(string optTextureName, float alpha)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshAlphaAll(string, float)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshAlphaAll(float)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }

				//targetTextureData.SetMeshAlphaAll(alpha);//<<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshAlphaAll(targetTextureData._textureID, alpha);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif

		}


		/// <summary>
		/// Change the color of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomColorAll(string optTextureName, Color color, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomColorAll(string, Color, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomColorAll(Color, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }
				//targetTextureData.SetCustomColorAll(color, propertyName);//<<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomColorAll(targetTextureData._textureID, color, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Change the color of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomColorAll(string optTextureName, Color color, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomColorAll(string, Color, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomColorAll(Color, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }
				//targetTextureData.SetCustomColorAll(color, propertyName);//<<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomColorAll(targetTextureData._textureID, color, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Change the color's alpha of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomAlphaAll(string optTextureName, float alpha, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomAlphaAll(string, float, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomAlphaAll(float, string)] function instead.");
					return;
				}


				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }
				//targetTextureData.SetCustomAlphaAll(alpha, propertyName);//이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomAlphaAll(targetTextureData._textureID, alpha, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Change the color's alpha of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomAlphaAll(string optTextureName, float alpha, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomAlphaAll(string, float, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomAlphaAll(float, string)] function instead.");
					return;
				}


				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }
				//targetTextureData.SetCustomAlphaAll(alpha, propertyName);//이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomAlphaAll(targetTextureData._textureID, alpha, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Change the float property of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomFloatAll(string optTextureName, float floatValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif

				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomFloatAll(string, float, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomFloatAll(float, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }


				//targetTextureData.SetCustomFloatAll(floatValue, propertyName);//<<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomFloatAll(targetTextureData._textureID, floatValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Change the float property of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomFloatAll(string optTextureName, float floatValue, int propertyNameID)//ID를 사용한 버전
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomFloatAll(string, float, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomFloatAll(float, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }


				//targetTextureData.SetCustomFloatAll(floatValue, propertyName);//<<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomFloatAll(targetTextureData._textureID, floatValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Change the int property of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomIntAll(string optTextureName, int intValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomIntAll(string, int, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomIntAll(int, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }

				//targetTextureData.SetCustomIntAll(intValue, propertyName);//이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomIntAll(targetTextureData._textureID, intValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Change the int property of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomIntAll(string optTextureName, int intValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomIntAll(string, int, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomIntAll(int, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }

				//targetTextureData.SetCustomIntAll(intValue, propertyName);//이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomIntAll(targetTextureData._textureID, intValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Change the Vector4 property of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomVector4All(string optTextureName, Vector4 vector4Value, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomVector4All(string, Vector4, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomVector4All(Vector4, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }

				//targetTextureData.SetCustomVector4All(vector4Value, propertyName);//<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomVector4All(targetTextureData._textureID, vector4Value, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Change the Vector4 property of the shader in a batch.
		/// </summary>
		/// <param name="optTextureName">Opt-Texture Name</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomVector4All(string optTextureName, Vector4 vector4Value, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_optTextureData == null || _optTextureData.Count == 0) { return; }

				//추가 21.12.31 : Merged Mat을 사용한다면 이 함수는 동작하지 않는다.
				if (_isUseMergedMat)
				{
					Debug.Log("AnyPortrait : [SetMeshCustomVector4All(string, Vector4, string)] does not work when using the Merged Material." +
						"\nPlease use the [SetMeshCustomVector4All(Vector4, string)] function instead.");
					return;
				}

				apOptTextureData targetTextureData = GetOptTextureData(optTextureName);

				if (targetTextureData == null) { return; }

				//targetTextureData.SetCustomVector4All(vector4Value, propertyName);//<이전

				//변경 12.13 : Batch Material쪽으로 변경
				_optBatchedMaterial.SetMeshCustomVector4All(targetTextureData._textureID, vector4Value, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//Mesh Material을 초기화하여 Batch가 되도록 만든다.
		/// <summary>Initialize the material of the target Opt-Transform so that it can be batch processed.</summary>
		/// <param name="optTransform">Target Opt-Transform</param>
		public void ResetMeshMaterialToBatch(apOptTransform optTransform)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.ResetMaterialToBatch();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>Initialize the material of the target Opt-Transform so that it can be batch processed.</summary>
		/// <param name="transformName">Opt-Transform Name</param>
		public void ResetMeshMaterialToBatch(string transformName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.ResetMaterialToBatch();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>Initialize the material of the target Opt-Transform so that it can be batch processed.</summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Opt-Transform Name</param>
		public void ResetMeshMaterialToBatch(int rootUnitIndex, string transformName)
		{
#if UNITY_EDITOR
			try
			{
#endif

				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.ResetMaterialToBatch();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Initialize all Command Buffers for clipping mask processing.
		/// </summary>
		public void CleanUpMeshesCommandBuffers()
		{
			int nOptMeshes = _optMeshes != null ? _optMeshes.Count : 0;
			if(nOptMeshes > 0)
			{
				for (int i = 0; i < nOptMeshes; i++)
				{
					_optMeshes[i].ClearCameraData();
				}
			}
		}

		/// <summary>
		/// Initialize or re-register all Command Buffers for clipping mask processing.
		/// </summary>
		/// <param name="isOnlyActiveRootUnit">If True, all Command Buffers of Root Units except the currently executing are initialized. If false, re-register the buffers of all Root Units.</param>
		public void ResetMeshesCommandBuffers(bool isOnlyActiveRootUnit)
		{
			if (isOnlyActiveRootUnit)
			{
				// [ 활성화된 루트 유닛만 갱신 ]
				//RootUnit 단위로 Reset을 한다. ]
				int nRootUnits = _optRootUnitList != null ? _optRootUnitList.Count : 0;
				apOptRootUnit rootUnit = null;
				if(nRootUnits > 0)
				{
					for (int i = 0; i < nRootUnits; i++)
					{
						rootUnit = _optRootUnitList[i];
						bool isRegistToCamera = (rootUnit == _curPlayingOptRootUnit);
						ResetMeshCommandBuffer(rootUnit, isRegistToCamera);
					}
				}
				
			}
			else
			{
				// [ 모든 Opt Mesh 대상으로 갱신 ]				
				int nOptMeshes = _optMeshes != null ? _optMeshes.Count : 0;
				if(nOptMeshes > 0)
				{
					for (int i = 0; i < nOptMeshes; i++)
					{
						_optMeshes[i].ResetMaskParentSetting();
					}
				}
				
			}

		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImage(apOptTransform optTransform, Texture2D texture)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetMeshTexture(texture);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImage(string transformName, Texture2D texture)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetMeshTexture(texture);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		public void SetMeshImage(int rootUnitIndex, string transformName, Texture2D texture)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetMeshTexture(texture);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main color of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColor(apOptTransform optTransform, Color color2X)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetMeshColor(color2X);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main color of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColor(string transformName, Color color2X)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetMeshColor(color2X);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main color of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color2X">Color (2X) to replace</param>
		public void SetMeshColor(int rootUnitIndex, string transformName, Color color2X)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null)
				{
					return;
				}
				optTransform._childMesh.SetMeshColor(color2X);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//Set Mesh Alpha 추가됨

		/// <summary>
		/// Set the main color's alpha of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		public void SetMeshAlpha(apOptTransform optTransform, float alpha)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetMeshAlpha(alpha);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main color's alpha of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void SetMeshAlpha(string transformName, float alpha)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetMeshAlpha(alpha);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main color's alpha of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void SetMeshAlpha(int rootUnitIndex, string transformName, float alpha)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null)
				{
					return;
				}
				optTransform._childMesh.SetMeshAlpha(alpha);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//추가 4.13 : 등록된 Image를 이용하는 방법 (제안 감사합니다.)

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="imageName">Name of the image registered</param>
		public void SetMeshImage(apOptTransform optTransform, string imageName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				apOptTextureData optTextureData = GetOptTextureData(imageName);
				if (optTextureData == null)
				{
					Debug.LogError("AnyPortrait : There are no registered textures. [" + imageName + "]");
					return;
				}
				optTransform._childMesh.SetMeshTexture(optTextureData._texture);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="imageName">Name of the image registered</param>
		public void SetMeshImage(string transformName, string imageName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				apOptTextureData optTextureData = GetOptTextureData(imageName);
				if (optTextureData == null)
				{
					Debug.LogError("AnyPortrait : There are no registered textures. [" + imageName + "]");
					return;
				}
				optTransform._childMesh.SetMeshTexture(optTextureData._texture);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the main texture of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="imageName">Name of the image registered</param>
		public void SetMeshImage(int rootUnitIndex, string transformName, string imageName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				apOptTextureData optTextureData = GetOptTextureData(imageName);
				if (optTextureData == null)
				{
					Debug.LogError("AnyPortrait : There are no registered textures. [" + imageName + "]");
					return;
				}
				optTransform._childMesh.SetMeshTexture(optTextureData._texture);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Set the texture property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomImage(apOptTransform optTransform, Texture2D texture, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTexture(texture, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomImage (string propName) 오버로드 추가
		/// <summary>
		/// Set the texture property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomImage(string transformName, Texture2D texture, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTexture(texture, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the texture property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomImage(int rootUnitIndex, string transformName, Texture2D texture, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTexture(texture, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Set the texture property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomImage(apOptTransform optTransform, Texture2D texture, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTexture(texture, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomImage (int ID) 오버로드 추가
		/// <summary>
		/// Set the texture property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomImage(string transformName, Texture2D texture, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTexture(texture, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the texture property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="texture">Texture2D to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomImage(int rootUnitIndex, string transformName, Texture2D texture, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTexture(texture, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the color property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomColor(apOptTransform optTransform, Color color, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomColor(color, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomColor (str propName) 오버로드 추가
		/// <summary>
		/// Set the color property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomColor(string transformName, Color color, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomColor(color, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the color property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomColor(int rootUnitIndex, string transformName, Color color, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomColor(color, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Set the color property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomColor(apOptTransform optTransform, Color color, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomColor(color, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomColor (int ID)의 오버로드 추가
		/// <summary>
		/// Set the color property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomColor(string transformName, Color color, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomColor(color, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the color property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="color">Color to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomColor(int rootUnitIndex, string transformName, Color color, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomColor(color, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}




		/// <summary>
		/// Set the color's alpha property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomAlpha(apOptTransform optTransform, float alpha, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomAlpha(alpha, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//[v1.4.5] SetMeshCustomAlpha(str propName)의 오버로드 추가
		/// <summary>
		/// Set the color's alpha property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomAlpha(string transformName, float alpha, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomAlpha(alpha, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the color's alpha property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomAlpha(int rootUnitIndex, string transformName, float alpha, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomAlpha(alpha, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Set the color's alpha property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomAlpha(apOptTransform optTransform, float alpha, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomAlpha(alpha, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomAlpha(int ID)의 오버로드 추가
		/// <summary>
		/// Set the color's alpha property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomAlpha(string transformName, float alpha, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomAlpha(alpha, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the color's alpha property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomAlpha(int rootUnitIndex, string transformName, float alpha, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomAlpha(alpha, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}




		/// <summary>
		/// Set the float property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomFloat(apOptTransform optTransform, float floatValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomFloat(floatValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomFloat(str propName)의 오버로드 추가
		/// <summary>
		/// Set the float property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomFloat(string transformName, float floatValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomFloat(floatValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the float property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomFloat(int rootUnitIndex, string transformName, float floatValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomFloat(floatValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}





		/// <summary>
		/// Set the float property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomFloat(apOptTransform optTransform, float floatValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomFloat(floatValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomFloat(int ID)의 오버로드 추가
		/// <summary>
		/// Set the float property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomFloat(string transformName, float floatValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomFloat(floatValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the float property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="floatValue">Float Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomFloat(int rootUnitIndex, string transformName, float floatValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomFloat(floatValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Set the int property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomInt(apOptTransform optTransform, int intValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomInt(intValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomInt(str propName)의 오버로드 추가
		/// <summary>
		/// Set the int property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomInt(string transformName, int intValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomInt(intValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the int property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomInt(int rootUnitIndex, string transformName, int intValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomInt(intValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Set the int property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomInt(apOptTransform optTransform, int intValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomInt(intValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomInt (int ID)의 오버로드 추가
		/// <summary>
		/// Set the int property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomInt(string transformName, int intValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomInt(intValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set the int property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="intValue">Int Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomInt(int rootUnitIndex, string transformName, int intValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomInt(intValue, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Set the Vector4 property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomVector4(apOptTransform optTransform, Vector4 vector4Value, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomVector4(vector4Value, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//[v1.4.5] SetMeshCustomVector4(str propName)의 오버로드 추가
		/// <summary>
		/// Set the Vector4 property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomVector4(string transformName, Vector4 vector4Value, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomVector4(vector4Value, propertyName);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the Vector4 property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomVector4(int rootUnitIndex, string transformName, Vector4 vector4Value, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomVector4(vector4Value, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}




		/// <summary>
		/// Set the Vector4 property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomVector4(apOptTransform optTransform, Vector4 vector4Value, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomVector4(vector4Value, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//[v1.4.5] SetMeshCustomVector4(int ID)의 오버로드 추가
		/// <summary>
		/// Set the Vector4 property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomVector4(string transformName, Vector4 vector4Value, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomVector4(vector4Value, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the Vector4 property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="vector4Value">Vector4 Value to replace</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomVector4(int rootUnitIndex, string transformName, Vector4 vector4Value, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomVector4(vector4Value, propertyNameID);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}




		//추가 12.02
		/// <summary>
		/// Set the TextureOffset property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureOffsetValue">TextureOffset Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureOffset(apOptTransform optTransform, Vector2 textureOffsetValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTextureOffset(textureOffsetValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomTextureOffset(str propName)의 오버로드 추가
		/// <summary>
		/// Set the TextureOffset property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureOffsetValue">TextureOffset Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureOffset(string transformName, Vector2 textureOffsetValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTextureOffset(textureOffsetValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the TextureOffset property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureOffsetValue">TextureOffset Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureOffset(int rootUnitIndex, string transformName, Vector2 textureOffsetValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTextureOffset(textureOffsetValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}




		/// <summary>
		/// Set the TextureScale property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureScaleValue">TextureScale Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureScale(apOptTransform optTransform, Vector2 textureScaleValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTextureScale(textureScaleValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		//[v1.4.5] SetMeshCustomTextureScale(str propName)의 오버로드 추가
		/// <summary>
		/// Set the TextureScale property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureScaleValue">TextureScale Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureScale(string transformName, Vector2 textureScaleValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTextureScale(textureScaleValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set the TextureScale property of the Opt-Mesh.
		/// (If it is changed, batch processing is not performed.)
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="textureScaleValue">TextureScale Value to replace</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomTextureScale(int rootUnitIndex, string transformName, Vector2 textureScaleValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetCustomTextureScale(textureScaleValue, propertyName);

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}






		/// <summary>
		/// Show Opt-Mesh
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		public void ShowMesh(apOptTransform optTransform)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetHideForce(false);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Show Opt-Mesh
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void ShowMesh(string transformName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetHideForce(false);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Show Opt-Mesh
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void ShowMesh(int rootUnitIndex, string transformName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetHideForce(false);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Hide Opt-Mesh
		/// </summary>
		/// <param name="optTransform">Opt-Transform with the target Opt-Mesh</param>
		public void HideMesh(apOptTransform optTransform)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetHideForce(true);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Hide Opt-Mesh
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void HideMesh(string transformName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetHideForce(true);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Hide Opt-Mesh
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		public void HideMesh(int rootUnitIndex, string transformName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				apOptTransform optTransform = GetOptTransform(rootUnitIndex, transformName);
				if (optTransform == null || optTransform._childMesh == null) { return; }
				optTransform._childMesh.SetHideForce(true);
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Hide Portrait (All Root Units)
		/// </summary>
		public void Hide()
		{
#if UNITY_EDITOR
			try
			{
#endif
				HideRootUnits();

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Show Portrait (A default Root Unit)
		/// </summary>
		public void Show()
		{
#if UNITY_EDITOR
			try
			{
#endif
				ShowRootUnit();
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Set alpha for all meshes (main color)
		/// </summary>
		/// <param name="alpha">Alpha value (0~1)</param>
		public void SetMeshAlphaAll(float alpha)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetAlpha(alpha);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshAlphaAll_WithoutTextureID(alpha);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set alpha for all meshes (custom property)
		/// </summary>
		/// <param name="alpha">Alpha value (0~1)</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomAlphaAll(float alpha, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomAlpha(alpha, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomAlphaAll_WithoutTextureID(alpha, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set alpha for all meshes (custom property)
		/// </summary>
		/// <param name="alpha">Alpha value (0~1)</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomAlphaAll(float alpha, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomAlpha(alpha, propertyNameID);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomAlphaAll_WithoutTextureID(alpha, propertyNameID);
				}

#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}




		/// <summary>
		/// Set color for all meshes (main color 2x)
		/// </summary>
		/// <param name="color2X">Color (2X Multiply)</param>
		public void SetMeshColorAll(Color color2X)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetColor(color2X);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshColorAll_WithoutTextureID(color2X);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set color for all meshes (custom property)
		/// </summary>
		/// <param name="color">Color (2X Multiply)</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomColorAll(Color color, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomColor(color, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomColorAll_WithoutTextureID(color, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set color for all meshes (custom property)
		/// </summary>
		/// <param name="color">Color (2X Multiply)</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomColorAll(Color color, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomColor(color, propertyNameID);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomColorAll_WithoutTextureID(color, propertyNameID);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//추가 22.1.3 : Custom Image All (Texture, Offset, Scale) 확장
		/// <summary>
		/// Set image for all meshes (custom property)
		/// </summary>
		/// <param name="texture">Image Texture</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomImageAll(Texture2D texture, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomImage(texture, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomImageAll_WithoutTextureID(texture, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set image for all meshes (custom property)
		/// </summary>
		/// <param name="texture">Image Texture</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomImageAll(Texture2D texture, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomImage(texture, propertyNameID);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomImageAll_WithoutTextureID(texture, propertyNameID);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		//추가 21.1.3
		/// <summary>
		/// Set TextureOffset for all meshes (custom property)
		/// </summary>		
		public void SetMeshCustomTextureOffsetAll(Vector2 textureOffsetValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomImageOffset(textureOffsetValue, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록
					_optBatchedMaterial.SetMeshCustomImageOffsetAll_WithoutTextureID(textureOffsetValue, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set TextureScale for all meshes (custom property)
		/// </summary>		
		public void SetMeshCustomTextureScaleAll(Vector2 textureScaleValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomImageScale(textureScaleValue, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록
					_optBatchedMaterial.SetMeshCustomImageScaleAll_WithoutTextureID(textureScaleValue, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}



		/// <summary>
		/// Set Float Property of shader for all meshes
		/// </summary>
		/// <param name="floatValue">Float Value</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomFloatAll(float floatValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomFloat(floatValue, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomFloatAll_WithoutTextureID(floatValue, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set Float Property of shader for all meshes
		/// </summary>
		/// <param name="floatValue">Float Value</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomFloatAll(float floatValue, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomFloat(floatValue, propertyNameID);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomFloatAll_WithoutTextureID(floatValue, propertyNameID);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set Int Property of shader for all meshes
		/// </summary>
		/// <param name="intValue">Integer Value</param>
		/// <param name="propertyName">Shader Property Name</param>
		public void SetMeshCustomIntAll(int intValue, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomInt(intValue, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomIntAll_WithoutTextureID(intValue, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}

		/// <summary>
		/// Set Int Property of shader for all meshes
		/// </summary>
		/// <param name="intValue">Integer Value</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomIntAll(int intValue, int propertyNameID)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomInt(intValue, propertyNameID);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomIntAll_WithoutTextureID(intValue, propertyNameID);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set Vector4 Property of shader for all meshes
		/// </summary>
		/// <param name="vector4Value">Vector4 Value</param>
		/// <param name="propertyName">Shder Property Name</param>
		public void SetMeshCustomVector4All(Vector4 vector4Value, string propertyName)
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomVector4(vector4Value, ref propertyName);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomVector4All_WithoutTextureID(vector4Value, propertyName);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}


		/// <summary>
		/// Set Vector4 Property of shader for all meshes
		/// </summary>
		/// <param name="vector4Value">Vector4 Value</param>
		/// <param name="propertyNameID">Shader Property Name ID. Use Shader.PropertyToID to get it.</param>
		public void SetMeshCustomVector4All(Vector4 vector4Value, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
#if UNITY_EDITOR
			try
			{
#endif
				if (_isUseMergedMat && _mergeMatMainPortrait != null && _optMergedMaterial != null)
				{
					//추가 21.12.29 : 병합된 경우
					_optMergedMaterial.SetCustomVector4(vector4Value, propertyNameID);
				}
				else
				{
					//변경 22.1.9 : Batched 되도록 변경
					_optBatchedMaterial.SetMeshCustomVector4All_WithoutTextureID(vector4Value, propertyNameID);
				}
#if UNITY_EDITOR
			}
			catch(Exception ex)
			{
				//v1.4.7 : 초기화 전에 함수 호출시 에러 로그가 찍히도록
				ShowErrorMsgIfNotInitialized_Editor(ex);
				throw;
			}
#endif
		}







		//Added 3.22 (1.0.2)
		//Sorting Order Features
		/// <summary>
		/// Set the Sorting Layer.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(string sortingLayerName)
		{
			//이름으로부터 SortingLayerID를 찾자
			if (SortingLayer.layers == null || SortingLayer.layers.Length == 0)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. There is no SortingLayer is this project.");
				return;
			}
			int targetSortingLayerID = -1;
			bool isTargetSortingLayerFound = false;
			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				if (string.Equals(SortingLayer.layers[i].name, sortingLayerName))
				{
					isTargetSortingLayerFound = true;
					targetSortingLayerID = SortingLayer.layers[i].id;
					break;
				}
			}
			//못찾았다.
			if (!isTargetSortingLayerFound)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. Could not find layer with requested name. <" + sortingLayerName + ">");
				return;
			}

			//Sorting Layer 적용
			_sortingLayerID = targetSortingLayerID;
			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].SetSortingLayer(sortingLayerName, _sortingLayerID);
			}
		}

		/// <summary>
		/// Set the Sorting Order
		/// </summary>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(int sortingOrder)
		{
			_sortingOrder = sortingOrder;
			for (int i = 0; i < _optMeshes.Count; i++)
			{
				_optMeshes[i].SetSortingOrder(sortingOrder);
			}
		}




		/// <summary>
		/// Changes the Sorting Layer of the specified OptTransform.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="optTransform">Target OptTransform</param>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(apOptTransform optTransform, string sortingLayerName)
		{
			//이름으로부터 SortingLayerID를 찾자
			if (SortingLayer.layers == null || SortingLayer.layers.Length == 0)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. There is no SortingLayer is this project.");
				return;
			}
			if (optTransform == null || optTransform._childMesh == null)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. OptTransform is null or it does not have a mesh.");
				return;
			}
			int targetSortingLayerID = -1;
			bool isTargetSortingLayerFound = false;
			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				if (string.Equals(SortingLayer.layers[i].name, sortingLayerName))
				{
					isTargetSortingLayerFound = true;
					targetSortingLayerID = SortingLayer.layers[i].id;
					break;
				}
			}
			//못찾았다.
			if (!isTargetSortingLayerFound)
			{
				Debug.LogError("AnyPortrait : SetSortingLayer() Failed. Could not find layer with requested name. <" + sortingLayerName + ">");
				return;
			}

			//Sorting Layer 적용
			optTransform._childMesh.SetSortingLayer(sortingLayerName, targetSortingLayerID);
		}

		/// <summary>
		/// Changes the Sorting Layer of the specified OptTransform.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(string transformName, string sortingLayerName)
		{
			SetSortingLayer(GetOptTransform(transformName), sortingLayerName);
		}

		/// <summary>
		/// Changes the Sorting Layer of the specified OptTransform.
		/// Use the name of the sorting layer set in the "Tags and Layers Manager" of the Unity project.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingLayerName">Layer Name in Sorting Layers</param>
		public void SetSortingLayer(int rootUnitIndex, string transformName, string sortingLayerName)
		{
			SetSortingLayer(GetOptTransform(rootUnitIndex, transformName), sortingLayerName);
		}

		/// <summary>
		/// Set the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="optTransform">Target OptTransform</param>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(apOptTransform optTransform, int sortingOrder)
		{
			if (optTransform == null || optTransform._childMesh == null)
			{
				Debug.LogError("AnyPortrait : SetSortingOrder() Failed. OptTransform is null or it does not have a mesh.");
				return;
			}

			_sortingOrder = sortingOrder;
			optTransform._childMesh.SetSortingOrder(sortingOrder);
		}

		/// <summary>
		/// Set the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(string transformName, int sortingOrder)
		{
			SetSortingOrder(GetOptTransform(transformName), sortingOrder);
		}

		/// <summary>
		/// Set the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <param name="sortingOrder">Sorting Order (Default is 0)</param>
		public void SetSortingOrder(int rootUnitIndex, string transformName, int sortingOrder)
		{
			SetSortingOrder(GetOptTransform(rootUnitIndex, transformName), sortingOrder);
		}


		/// <summary>
		/// Get Name of Sorting Layer
		/// If Failed, "Unknown Layer" is returned
		/// </summary>
		/// <returns></returns>
		public string GetSortingLayerName()
		{
			if (SortingLayer.layers == null || SortingLayer.layers.Length == 0)
			{
				return "Unknown Layer";
			}

			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				if (SortingLayer.layers[i].id == _sortingLayerID)
				{
					return SortingLayer.layers[i].name;
				}
			}

			return "Unknown Layer";
		}

		/// <summary>
		/// Get Sorting Order
		/// </summary>
		/// <returns></returns>
		public int GetSortingOrder()
		{
			return _sortingOrder;
		}


		//추가 21.1.31
		/// <summary>
		/// Get the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="optTransform">Target OptTransform</param>
		/// <returns>Sorting Order value. -1 is returned if the requested OptTransform does not exist or does not have a mesh.</returns>
		public int GetSortingOrder(apOptTransform optTransform)
		{
			if (optTransform == null)
			{
				Debug.LogError("AnyPortrait : GetSortingOrder() Failed. The OptTransform entered as an argument is null.");
				return -1;
			}
			if (optTransform._childMesh == null)
			{
				Debug.LogError("AnyPortrait : GetSortingOrder() Failed. The requested OptTransform does not have a mesh.");
				return -1;
			}
			return optTransform._childMesh.GetSortingOrder();
		}

		/// <summary>
		/// Get the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <returns>Sorting Order value. -1 is returned if the requested OptTransform does not exist or does not have a mesh.</returns>
		public int GetSortingOrder(string transformName)
		{
			return GetSortingOrder(GetOptTransform(transformName));
		}

		/// <summary>
		/// Get the Sorting Order of the specified OptTransform.
		/// </summary>
		/// <param name="rootUnitIndex">Root Unit Index</param>
		/// <param name="transformName">Name of Opt-Transform with the target Opt-Mesh</param>
		/// <returns>Sorting Order value. -1 is returned if the requested OptTransform does not exist or does not have a mesh.</returns>
		public int GetSortingOrder(int rootUnitIndex, string transformName)
		{
			return GetSortingOrder(GetOptTransform(rootUnitIndex, transformName));
		}





		//추가 19.8.19
		//Sorting Order Option에 관련된 함수들 추가
		/// <summary>
		/// Set whether the [Sorting Order] of the meshes will be changed automatically.
		/// This function works only when the value of [Sorting Order Option] of apPortrait is [Depth To Order] or [Reverse Depth To Order].
		/// </summary>
		/// <param name="isEnabled">Whether the sorting order is automatically updated (Default is true)</param>
		public void SetSortingOrderChangedAutomatically(bool isEnabled)
		{
			if (_optRootUnitList == null)
			{
				return;
			}

			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].SetSortingOrderChangedAutomatically(isEnabled);
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void RefreshSortingOrderByDepth()
		{
			if (_optRootUnitList == null)
			{
				return;
			}
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].RefreshSortingOrderByDepth();
			}
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ApplySortingOptionToOptRootUnits()
		{
			if (_optRootUnitList == null)
			{
				return;
			}
			for (int i = 0; i < _optRootUnitList.Count; i++)
			{
				_optRootUnitList[i].SetSortingOrderOption(_sortingOrderOption, _sortingOrderPerDepth);
			}
		}
#endif

		//-------------------------------------------------------------------------------------------------------
		// 업데이트 관련 처리
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// If the "Important" setting is True, the physics effect and animation are activated and updated every frame.
		/// </summary>
		/// <param name="isImportant"></param>
		public void SetImportant(bool isImportant)
		{
			if (_isImportant != isImportant)
			{
				_isImportant = isImportant;
			}
		}

		/// <summary>
		/// If the "Important" option is off, the character is executed according to the specified FPS or lower.
		/// </summary>
		/// <param name="fps"></param>
		public void SetFPSForNotImportant(int fps)
		{
			_FPS = fps;
		}


		//v1.4.5 추가 : 메시 갱신 빈도
		/// <summary>
		/// Set the meshes to be updated Every Frame. The animation plays smoothly. (default)
		/// </summary>
		public void SetUpdateMeshesEveryFrame()
		{
			_meshRefreshRateOption = MESH_UPDATE_FREQUENCY.EveryFrames;			
		}

		/// <summary>
		/// Set the meshes to be updated at regular intervals. Enter the update frequency as FPS.
		/// </summary>
		/// <param name="fps">Frames per second, which refers to the update frequency. Please enter a value between 1 and 30.</param>
		/// <param name="isSyncUpdate">If True, it updates at the same frame as other characters with the same FPS settings.</param>
		public void SetUpdateMeshesPerTime(int fps, bool isSyncUpdate)
		{
			if(isSyncUpdate)
			{
				_meshRefreshRateOption = MESH_UPDATE_FREQUENCY.FixedFrames_Sync;
			}
			else
			{
				_meshRefreshRateOption = MESH_UPDATE_FREQUENCY.FixedFrames_NotSync;
			}
			_meshRefreshRateFPS = fps;
			//Min ~ Max 사이 값으로 설정하자
			if(_meshRefreshRateFPS < MESH_REFRESH_FPS_MIN)
			{
				_meshRefreshRateFPS = MESH_REFRESH_FPS_MIN;
			}
			else if(_meshRefreshRateFPS > MESH_REFRESH_FPS_MAX)
			{
				_meshRefreshRateFPS = MESH_REFRESH_FPS_MAX;
			}
		}


		// 초기화
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ReadyToEdit()
		{
			//ID리셋 / 텍스쳐 준비 / Null 메시 삭제
			ReadyToEdit_Step1();

			//메시 준비
			ReadyToEdit_Step2();

			//메시 그룹 기본 연결
			ReadyToEdit_Step3();

			//클리핑 레이어 연결
			ReadyToEdit_Step4();

			//부모-자식 메시 그룹 연결 / Bone 준비
			ReadyToEdit_Step5();

			//Render Unit 연결
			ReadyToEdit_Step6();

			//Anim Clip 준비
			ReadyToEdit_Step7();

			//모디파이어 준비 (LinkAndRefreshInEditor)
			ReadyToEdit_Step8();

			//Root Unit 연결
			ReadyToEdit_Step9();
		}




		// ReadyToStep의 내용들을 각각의 서브 함수들로 분리한다.
		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step1()
		{
			//ID 리스트 일단 리셋
			ClearRegisteredUniqueIDs();

			//컨트롤 / 컨트롤 파라미터 리셋
			_controller.Ready(this);
			_controller.SetDefaultAll();


			for (int iTexture = 0; iTexture < _textureData.Count; iTexture++)
			{
				_textureData[iTexture].ReadyToEdit(this);
			}

			_meshes.RemoveAll(delegate (apMesh a)
			{
				return a == null;
			});
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step2()
		{
			for (int iMeshes = 0; iMeshes < _meshes.Count; iMeshes++)
			{
				//내부 MeshComponent들의 레퍼런스를 연결하자
				_meshes[iMeshes].ReadyToEdit(this);

				//텍스쳐를 연결하자
				int textureID = -1;

				if (!_meshes[iMeshes].IsTextureDataLinked)//연결이 안된 경우
				{
					textureID = _meshes[iMeshes].LinkedTextureDataID;
					_meshes[iMeshes].SetTextureData(GetTexture(textureID));
				}

				_meshes[iMeshes].LinkEdgeAndVertex();
			}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step3()
		{
			_meshGroups.RemoveAll(delegate (apMeshGroup a)
			{
				return a == null;
			});


			//메시 그룹도 비슷하게 해주자
			//1. 메시/메시 그룹을 먼저 연결
			//2. Parent-Child는 그 다음에 연결 (Child 먼저 / Parent는 나중에)
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				meshGroup.Init(this);

				//1. Mesh 연결
				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					meshGroup._childMeshTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

					int childIndex = meshGroup._childMeshTransforms[iChild]._meshUniqueID;
					if (childIndex >= 0)
					{
						apMesh existMesh = GetMesh(childIndex);
						if (existMesh != null)
						{
							meshGroup._childMeshTransforms[iChild]._mesh = existMesh;
						}
						else
						{
							meshGroup._childMeshTransforms[iChild]._mesh = null;
						}
					}
					else
					{
						meshGroup._childMeshTransforms[iChild]._mesh = null;
					}
				}

				//1-2. MeshGroup 연결
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					meshGroup._childMeshGroupTransforms[iChild].RegistIDToPortrait(this);//추가 : ID를 알려주자

					int childIndex = meshGroup._childMeshGroupTransforms[iChild]._meshGroupUniqueID;
					if (childIndex >= 0)
					{
						apMeshGroup existMeshGroup = GetMeshGroup(childIndex);
						if (existMeshGroup != null)
						{
							meshGroup._childMeshGroupTransforms[iChild]._meshGroup = existMeshGroup;
						}
						else
						{
							meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
						}
					}
					else
					{
						meshGroup._childMeshGroupTransforms[iChild]._meshGroup = null;
					}
				}
			}

			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//2. 하위 MeshGroup 연결
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[iChild];

					if (childMeshGroupTransform._meshGroupUniqueID >= 0)
					{
						apMeshGroup existMeshGroup = GetMeshGroup(childMeshGroupTransform._meshGroupUniqueID);
						if (existMeshGroup != null)
						{
							childMeshGroupTransform._meshGroup = existMeshGroup;

							childMeshGroupTransform._meshGroup._parentMeshGroupID = meshGroup._uniqueID;
							childMeshGroupTransform._meshGroup._parentMeshGroup = meshGroup;


						}
						else
						{
							childMeshGroupTransform._meshGroup = null;
						}
					}
					else
					{
						childMeshGroupTransform._meshGroup = null;
					}
				}

				//다만, 없어진 Mesh Group은 정리해주자
				meshGroup._childMeshTransforms.RemoveAll(delegate (apTransform_Mesh a)
				{
					return a._mesh == null;
				});
				meshGroup._childMeshGroupTransforms.RemoveAll(delegate (apTransform_MeshGroup a)
				{
					return a._meshGroup == null;
				});
			}
		}


		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step4()
		{
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//추가) Clipping Layer를 위해서 Mesh Transform끼리 연결을 해준다.
				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					//연결하기 전에
					//Child는 초기화해준다.
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
					meshTransform._isClipping_Child = false;
					meshTransform._clipIndexFromParent = -1;
					meshTransform._clipParentMeshTransform = null;

					if (meshTransform._clipChildMeshes == null)
					{
						meshTransform._clipChildMeshes = new List<apTransform_Mesh.ClipMeshSet>();
					}

					meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
					{
						//조건에 맞지 않는 Clipping Child를 삭제한다.
						//1. ID가 맞지 않다.
						//2. MeshGroup에 존재하지 않다.
						return a._transformID < 0 || (meshGroup.GetMeshTransform(a._transformID) == null);
					});


					//추가 19.6.9 : Material Set 연결
					if (meshTransform._isUseDefaultMaterialSet)
					{
						//Default MatSet을 사용하는 경우
						meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
						if (meshTransform._linkedMaterialSet != null)
						{
							meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
						}
					}
					else
					{
						//별도의 MatSet을 설정한 경우
						if (meshTransform._materialSetID >= 0)
						{
							meshTransform._linkedMaterialSet = GetMaterialSet(meshTransform._materialSetID);
							if (meshTransform._linkedMaterialSet == null)
							{
								//존재하지 않는 Material Set
								meshTransform._materialSetID = -1;
								//Debug.LogError("Material Set 잘못 연결 후 초기화");
							}
						}
						else
						{
							meshTransform._linkedMaterialSet = null;
						}

						//만약 연결이 안된다면 > Default를 찾아서 무조건 연결한다.
						if (meshTransform._linkedMaterialSet == null)
						{
							meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
							if (meshTransform._linkedMaterialSet != null)
							{
								meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
							}
						}
					}
				}

				for (int iChild = 0; iChild < meshGroup._childMeshTransforms.Count; iChild++)
				{
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iChild];
					if (meshTransform._isClipping_Parent)
					{
						//최대 3개의 하위 Mesh를 검색해서 연결한다.
						//찾은 이후엔 Sort를 해준다.

						for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
						{
							apTransform_Mesh.ClipMeshSet clipSet = meshTransform._clipChildMeshes[iClip];
							int childMeshID = clipSet._transformID;
							apTransform_Mesh childMeshTF = meshGroup.GetMeshTransform(childMeshID);
							if (childMeshTF != null)
							{
								clipSet._meshTransform = childMeshTF;
								//clipSet._renderUnit = meshGroup.GetRenderUnit(childMeshTF);//삭제 v1.5.0
							}
							else
							{
								clipSet._meshTransform = null;
								clipSet._transformID = -1;
								//clipSet._renderUnit = null;//삭제 v1.5.0
							}
						}

						meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
						{
							return a._transformID < 0;
						});


					}
					else
					{
						meshTransform._clipChildMeshes.Clear();


					}

					meshTransform.SortClipMeshTransforms();
				}

			}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step5()
		{
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//2. 상위 MeshGroup 연결
				int parentUniqueID = meshGroup._parentMeshGroupID;
				if (parentUniqueID >= 0)
				{
					meshGroup._parentMeshGroup = GetMeshGroup(parentUniqueID);
					if (meshGroup._parentMeshGroup == null)
					{
						meshGroup._parentMeshGroupID = -1;
					}
				}
				else
				{
					meshGroup._parentMeshGroup = null;
				}
			}

			//Bone 연결 
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];

				//Root 리스트는 일단 날리고 BoneAll 리스트를 돌면서 필요한걸 넣어주자
				//이후엔 Root -> Child 방식으로 순회
				meshGroup._boneList_Root.Clear();
				if (meshGroup._boneList_All != null)
				{
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						apBone bone = meshGroup._boneList_All[iBone];

						//먼저 ID를 ID Manager에 등록한다.
						RegistUniqueID(apIDManager.TARGET.Bone, bone._uniqueID);

						apBone parentBone = null;
						if (bone._parentBoneID >= 0)
						{
							parentBone = meshGroup.GetBone(bone._parentBoneID);
						}

						bone.Link(meshGroup, parentBone, this);

						if (parentBone == null)
						{
							//Parent가 없다면 Root 본이다.
							meshGroup._boneList_Root.Add(bone);
						}
					}

					//추가 5.9 : Bone의 Check Validation 함수를 호출해야 한다.
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						meshGroup._boneList_All[iBone].CheckIKControllerValidation();
					}
				}


				int curBoneIndex = 0;
				for (int iRoot = 0; iRoot < meshGroup._boneList_Root.Count; iRoot++)
				{
					apBone rootBone = meshGroup._boneList_Root[iRoot];
					//TODO : MeshGroup이 Transform으로 있는 경우에 Transform Matrix를 넣어줘야한다.
					rootBone.LinkRecursive(0);
					curBoneIndex = rootBone.SetBoneIndex(curBoneIndex) + 1;
				}
			}

			////본 계층 / IK Chain도 다시 점검
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];

			//}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step6()
		{
			//Render Unit도 체크해주자
			//아무 순서대로 하지 말고, Root이 MeshGroup을 찾아서 재귀적으로 한 뒤, 처리되지 못한 MeshGroup을 체크해야한다.
			//Step5에서 MeshGroup간의 Parent-Child 연결이 완료되었으니 가능하다.
			List<apMeshGroup> processedMeshGroups = new List<apMeshGroup>();

			//이전
			//for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = _meshGroups[iMeshGroup];
			//	//meshGroup.SetAllRenderUnitForceUpdate();
			//	meshGroup.RefreshForce();
			//	meshGroup.SortRenderUnits(true);
			//	meshGroup.SortBoneListByLevelAndDepth();
			//}

			//변경 v1.4.2 : 루트 메시 그룹을 중심으로 재귀적으로 호출하자 (Sort RenderUnit 특성상)
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];
				if(meshGroup._parentMeshGroup == null)
				{
					//루트 메시 그룹에 대해서만 재귀 함수 호출
					ReadyToEdit_RefrestMeshGroupRecursive(meshGroup, meshGroup, processedMeshGroups);
				}
			}

			//다시 돌면서, 처리되지 않은 나머지 메시 그룹을 찾아서 처리하자
			for (int iMeshGroup = 0; iMeshGroup < _meshGroups.Count; iMeshGroup++)
			{
				apMeshGroup meshGroup = _meshGroups[iMeshGroup];
				if(processedMeshGroups.Contains(meshGroup))
				{
					//이미 처리가 되었다.
					continue;
				}

				//누락된 메시 그룹에 대해서도 호출
				ReadyToEdit_RefrestMeshGroupRecursive(meshGroup, meshGroup._parentMeshGroup != null ? meshGroup._parentMeshGroup : meshGroup, processedMeshGroups);
			}
		}

		//추가 v1.4.2 : 초기화시 메시 그룹의 Refresh/SortRenderUnit을 호출할 때, 그냥하는게 아니라 재귀적으로 하도록
		private void ReadyToEdit_RefrestMeshGroupRecursive(apMeshGroup curMeshGroup, apMeshGroup rootMeshGroup, List<apMeshGroup> processedList)
		{
			if (curMeshGroup == null)
			{
				return;
			}

			curMeshGroup.SetDirtyToReset();//추가 1.4.2 : 초기화시 Reset 플래그를 올리자
			curMeshGroup.RefreshForce();
			if(curMeshGroup._parentMeshGroup == null || curMeshGroup == rootMeshGroup)
			{
				//이게 루트 메시 그룹이라면
				//Sort 후 TF에 Depth Assign까지 수행한다.
				curMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);
			}
			else
			{
				//이게 루트 메시 그룹이 아니라면, 위에서 이미 Depth Assign이 되었으므로 정렬만 한다.
				curMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);
			}
			curMeshGroup.SortBoneListByLevelAndDepth();

			//결과 리스트에 추가
			processedList.Add(curMeshGroup);

			//자식 메시 그룹이 있다면 (이건 Step3에서 연결이 된다)
			int nChildMeshGroups = curMeshGroup._childMeshGroupTransforms != null ? curMeshGroup._childMeshGroupTransforms.Count : 0;
			if(nChildMeshGroups == 0)
			{
				return;
			}

			apTransform_MeshGroup childMeshGroupTF = null;
			for (int i = 0; i < nChildMeshGroups; i++)
			{
				childMeshGroupTF = curMeshGroup._childMeshGroupTransforms[i];
				if(childMeshGroupTF == null)
				{
					continue;
				}
				if(childMeshGroupTF._meshGroup == null
					|| childMeshGroupTF._meshGroup == curMeshGroup
					|| childMeshGroupTF._meshGroup == rootMeshGroup)
				{
					continue;
				}

				ReadyToEdit_RefrestMeshGroupRecursive(childMeshGroupTF._meshGroup, rootMeshGroup, processedList);

			}
		}



		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step7()
		{
			//Anim Clip 준비도 하자
			_animClips.RemoveAll(delegate(apAnimClip a)
			{
				return 
				a == null || //Null이거나
				(a._targetMeshGroupID >= 0 && GetMeshGroup(a._targetMeshGroupID) == null);//TargetMeshGroup ID는 있는데, MeshGroup은 존재하지 않는 경우
			});

			for (int i = 0; i < _animClips.Count; i++)
			{
				_animClips[i].LinkEditor(this);
				_animClips[i].RemoveUnlinkedTimeline();
			}
		}

		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step8()
		{
			//5. Modifier 세팅
			LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

			
		}


		/// <summary>[Please do not use it]</summary>
		public void ReadyToEdit_Step9()
		{
			// Main MeshGroup 연결
			// 수정) "다중" MainMeshGroup으로 변경

			if (_mainMeshGroupList == null)		{ _mainMeshGroupList = new List<apMeshGroup>(); }
			else								{ _mainMeshGroupList.Clear(); }

			if (_mainMeshGroupIDList == null)
			{
				_mainMeshGroupIDList = new List<int>();
			}


			for (int iMGID = 0; iMGID < _mainMeshGroupIDList.Count; iMGID++)
			{
				int mainMeshGroupID = _mainMeshGroupIDList[iMGID];
				bool isValidMeshGroupID = false;

				if (mainMeshGroupID >= 0)
				{
					apMeshGroup mainMeshGroup = GetMeshGroup(mainMeshGroupID);
					if (mainMeshGroup != null)
					{
						if (!_mainMeshGroupList.Contains(mainMeshGroup))
						{
							_mainMeshGroupList.Add(mainMeshGroup);
							isValidMeshGroupID = true;
						}
					}
				}
				if (!isValidMeshGroupID)
				{
					_mainMeshGroupIDList[iMGID] = -1;//<<이건 삭제하자
				}
			}

			//일단 유효하지 못한 ID는 삭제하자
			_mainMeshGroupIDList.RemoveAll(delegate (int a)
			{
				return a < 0;
			});

			_rootUnits.Clear();

			for (int iMainMesh = 0; iMainMesh < _mainMeshGroupList.Count; iMainMesh++)
			{
				apMeshGroup meshGroup = _mainMeshGroupList[iMainMesh];

				apRootUnit newRootUnit = new apRootUnit();

				newRootUnit.SetPortrait(this);
				newRootUnit.SetMeshGroup(meshGroup);

				_rootUnits.Add(newRootUnit);
			}
		}


		//-------------------------------------------------------------------------------------------------
		// LinkAndRefreshInEditor
		//-------------------------------------------------------------------------------------------------

		//Editor 상태에서
		//MeshGroup을 참조하는 객체들 간의 레퍼런스를 연결하고 갱신한다.
		//Editor 실행시와 객체 추가/삭제시 호출해주자
		//최적화 코드 추가 20.4.3
		//- 1. 모든 항목을 링크할 지, 특정 애니메이션만 링크를 다시 할지 결정한다. (애니메이션 편집 정보는 해당 메시 그룹 외에는 다른 영향이 없으므로)
		//- 2. Async 처리도 가능하도록 개선.
		/// <summary>[Please do not use it]</summary>
		/// <param name="curSelectedMeshGroup">현재 선택된 메시 그룹. RenderUnit과 Mod 정보를 한번 더 갱신한다. (이것 외의 링크를 제한하는 역할은 없음)</param>
		/// <param name="targetAnimClip">현재 편집중인 AnimClip. 이게 null이면 전체 갱신. 대상이 있다면 불필요한 링크 작업은 생략된다.(isResetLink가 false인 경우에 한해서)</param>
		public void LinkAndRefreshInEditor(bool isResetLink,
											apUtil.LinkRefreshRequest linkRefreshRequest
											//apMeshGroup editorSelectedMeshGroup
											//, apAnimClip targetAnimClip
											)
		{
			//UnityEngine.Profiling.Profiler.BeginSample("AnyPortrait Link And Refresh In Editor");
			
			//4.1 리셋이 필요한지 검사한다.
			//겸사겸사 불필요한 데이터도 삭제한다.

			//UnityEngine.Profiling.Profiler.BeginSample("Link 1 - Remove null");
			
			//노트 v1.5.0 : 이 대리자 코드는 GC를 발생시키지 않는다.
			int nTextureRemoved = _textureData.RemoveAll(delegate(apTextureData a)
			{
				return a == null;
			});
			int nMeshRemoved = _meshes.RemoveAll(delegate(apMesh a)
			{
				return a == null;
			});
			int nMeshGroupRemoved = _meshGroups.RemoveAll(delegate(apMeshGroup a)
			{
				return a == null;
			});
			int nAnimClipRemoved = _animClips.RemoveAll(delegate(apAnimClip a)
			{
				return a == null;
			});

			//UnityEngine.Profiling.Profiler.EndSample();


			//UnityEngine.Profiling.Profiler.BeginSample("Link 2 - Remove Null Mod");

			//TODO : 단순히 MeshGroup을 루틴을 돌아서 처리하면, Root Mesh Group보다 Child Mesh Group이 나중에 처리되서,
			//Root Mesh Group의 Link가 다 끊기게 된다.
			//따라서 Child 부터 처리를 해야한다.
			//<REV_MG>
			List<apMeshGroup> revMeshGroups = GetReverseMeshGroupList(_meshGroups);
			int nRevMeshGroups = revMeshGroups != null ? revMeshGroups.Count : 0;

			int nModRemoved = 0;

			//<REV_MG>
			apMeshGroup curMeshGroup = null;
			for (int i = 0; i < nRevMeshGroups; i++)
			{
				curMeshGroup = revMeshGroups[i];

				//유효하지 않은 모디파이어들을 여기서 삭제한다.
				//int curNumModRemoved = curMeshGroup._modifierStack.RemoveInvalidModifiers();
				if(curMeshGroup._modifierStack._modifiers != null)
				{
					int curNumModRemoved = curMeshGroup._modifierStack._modifiers.RemoveAll(delegate(apModifierBase a)
					{
						return a == null;
					});

					nModRemoved += (curNumModRemoved > 0) ? curNumModRemoved : 0;
				}
			}

			if(!isResetLink)
			{
				if(nTextureRemoved > 0 ||
					nMeshRemoved > 0 ||
					nMeshGroupRemoved > 0 ||
					nAnimClipRemoved > 0 ||
					nModRemoved > 0)
				{
					isResetLink = true;
				}
			}

			//UnityEngine.Profiling.Profiler.EndSample();
			

			//4.1 추가
			// 만약 isResetLink= true라면
			// ReadyToEdit와 같이 
			if (isResetLink)
			{
				//UnityEngine.Profiling.Profiler.BeginSample("Link 3 - Reset <체크>");

				//변경. 이 코드를 isResetLink 안으로 이동시킴 (20.4.3)
				//--------------------------------------

				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-1");

				_controller.Ready(this);

				int nMeshes = _meshes != null ? _meshes.Count : 0;
				if(nMeshes > 0)
				{
					for (int iMesh = 0; iMesh < nMeshes; iMesh++)
					{
						_meshes[iMesh].LinkEdgeAndVertex();
					}
				}
				

				//UnityEngine.Profiling.Profiler.EndSample();
				//--------------------------------------


				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-2");

				//텍스쳐도 리셋
				int nTextureData = _textureData != null ? _textureData.Count : 0;
				if(nTextureData > 0)
				{
					for (int iTexture = 0; iTexture < nTextureData; iTexture++)
					{
						_textureData[iTexture].ReadyToEdit(this);
					}
				}
				

				nMeshes = _meshes != null ? _meshes.Count : 0;
				if(nMeshes > 0)
				{
					apMesh mesh = null;
					for (int iMeshes = 0; iMeshes < nMeshes; iMeshes++)
					{
						//내부 MeshComponent들의 레퍼런스를 연결하자
						mesh = _meshes[iMeshes];
						
						mesh.ReadyToEdit(this);

						//텍스쳐를 연결하자
						int textureID = mesh.LinkedTextureDataID;
						mesh.SetTextureData(GetTexture(textureID));

						mesh.LinkEdgeAndVertex();
					}
				}
				


				//UnityEngine.Profiling.Profiler.EndSample();


				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-3");

				//1. 메시/메시 그룹을 먼저 연결
				//2. Parent-Child는 그 다음에 연결 (Child 먼저 / Parent는 나중에)
				//<REV_MG>
				for (int iMeshGroup = 0; iMeshGroup < nRevMeshGroups; iMeshGroup++)
				{
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					meshGroup.Init(this);

					int nChildMeshTFs = meshGroup._childMeshTransforms != null ? meshGroup._childMeshTransforms.Count : 0;
					if(nChildMeshTFs > 0)
					{
						meshGroup._childMeshTransforms.RemoveAll(delegate(apTransform_Mesh a)
						{
							return a == null;
						});

						//개수 변경
						nChildMeshTFs = meshGroup._childMeshTransforms != null ? meshGroup._childMeshTransforms.Count : 0;

						//1. Mesh 연결 + Clipping 연결
						apTransform_Mesh meshTransform = null;
						for (int iChild = 0; iChild < nChildMeshTFs; iChild++)
						{
							meshTransform = meshGroup._childMeshTransforms[iChild];
							meshTransform.RegistIDToPortrait(this);//추가 : ID를 알려주자

							int meshID = meshTransform._meshUniqueID;
							if (meshID >= 0)
							{
								if (meshTransform._mesh == null)
								{
									//Mesh가 연결 안된 경우
									apMesh existMesh = GetMesh(meshID);
									if (existMesh != null)
									{
										meshTransform._mesh = existMesh;
									}
									else
									{
										meshTransform._mesh = null;
									}
								}

								//--------------
								//추가) Clipping Layer를 위해서 Mesh Transform끼리 연결을 해준다.

								
								if (meshTransform._clipChildMeshes == null)
								{
									meshTransform._clipChildMeshes = new List<apTransform_Mesh.ClipMeshSet>();
								}

								meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
								{
									//조건에 맞지 않는 Clipping Child를 삭제한다.
									//1. ID가 맞지 않다.
									//2. MeshGroup에 존재하지 않다.
									return a._transformID < 0 || (meshGroup.GetMeshTransform(a._transformID) == null);
								});
								
								//-------------
							}
							else
							{
								//Mesh ID가 유효하지 않은 MeshTransform
								meshTransform._mesh = null;
							}

							//추가 19.6.9 : Material Set 연결
							if (meshTransform._isUseDefaultMaterialSet)
							{
								//기본값의 MatSet을 사용하자.
								meshTransform._linkedMaterialSet = GetDefaultMaterialSet();
								if(meshTransform._linkedMaterialSet != null)
								{
									//ID도 바꿔주자.
									meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
								}
							}
							else
							{
								if (meshTransform._materialSetID >= 0)
								{
									meshTransform._linkedMaterialSet = GetMaterialSet(meshTransform._materialSetID);
									if (meshTransform._linkedMaterialSet == null)
									{
										//존재하지 않는 Material Set
										meshTransform._materialSetID = -1;
									}
								}
								else
								{
									meshTransform._linkedMaterialSet = null;
								}
							}
						}
					}

					

					


					//1-2. MeshGroup 연결
					int nChildMeshGroupTFs = meshGroup._childMeshGroupTransforms != null ? meshGroup._childMeshGroupTransforms.Count : 0;
					if(nChildMeshGroupTFs > 0)
					{
						meshGroup._childMeshGroupTransforms.RemoveAll(delegate(apTransform_MeshGroup a)
						{
							return a == null;
						});

						nChildMeshGroupTFs = meshGroup._childMeshGroupTransforms != null ? meshGroup._childMeshGroupTransforms.Count : 0;

						apTransform_MeshGroup meshGroupTF = null;

						for (int iChild = 0; iChild < nChildMeshGroupTFs; iChild++)
						{
							meshGroupTF = meshGroup._childMeshGroupTransforms[iChild];
							meshGroupTF.RegistIDToPortrait(this);//추가 : ID를 알려주자

							int meshGroupID = meshGroupTF._meshGroupUniqueID;
							if (meshGroupID >= 0)
							{	
								if (meshGroupTF._meshGroup == null)
								{
									//MeshGroup이 연결이 안된 경우
									apMeshGroup existMeshGroup = GetMeshGroup(meshGroupID);
									if (existMeshGroup != null)
									{
										meshGroupTF._meshGroup = existMeshGroup;
									}
									else
									{
										meshGroupTF._meshGroup = null;
										//Debug.LogError("MeshGroup이 없는 MeshGroupTransform 발견 : " + meshGroup._childMeshGroupTransforms[iChild]._nickName);
									}
								}
							}
							else
							{
								//MeshGroup ID가 유효하지 않은 MeshGroupTransform 발견
								meshGroupTF._meshGroup = null;
							}
						}						
					}
				}

				//UnityEngine.Profiling.Profiler.EndSample();

				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-4");

				//<REV_MG>
				for (int iMeshGroup = 0; iMeshGroup < nRevMeshGroups; iMeshGroup++)
				{
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					//2. 하위 MeshGroup 연결
					int nChildMeshGroupTFs = meshGroup._childMeshGroupTransforms != null ? meshGroup._childMeshGroupTransforms.Count : 0;
					if(nChildMeshGroupTFs > 0)
					{
						for (int iChild = 0; iChild < nChildMeshGroupTFs; iChild++)
						{
							apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[iChild];

							if (childMeshGroupTransform._meshGroupUniqueID >= 0)
							{
								apMeshGroup existMeshGroup = GetMeshGroup(childMeshGroupTransform._meshGroupUniqueID);
								if (existMeshGroup != null)
								{
									childMeshGroupTransform._meshGroup = existMeshGroup;

									childMeshGroupTransform._meshGroup._parentMeshGroupID = meshGroup._uniqueID;
									childMeshGroupTransform._meshGroup._parentMeshGroup = meshGroup;
								}
								else
								{
									childMeshGroupTransform._meshGroup = null;
								}
							}
							else
							{
								childMeshGroupTransform._meshGroup = null;
							}
						}
					}
					

					//다만, 없어진 Mesh Group은 정리해주자
					int nChildMeshTFs = meshGroup._childMeshTransforms != null ? meshGroup._childMeshTransforms.Count : 0;
					if(nChildMeshTFs > 0)
					{
						meshGroup._childMeshTransforms.RemoveAll(delegate (apTransform_Mesh a)
						{
							return a._mesh == null;
						});
					}

					nChildMeshGroupTFs = meshGroup._childMeshGroupTransforms != null ? meshGroup._childMeshGroupTransforms.Count : 0;					
					if(nChildMeshGroupTFs > 0)
					{
						meshGroup._childMeshGroupTransforms.RemoveAll(delegate (apTransform_MeshGroup a)
						{
							return a._meshGroup == null;
						});
					}
				}

				//UnityEngine.Profiling.Profiler.EndSample();

				//Link에서 SortRenderUnit 변경사항 (v1.4.2)
				//이전 : SortRenderUnit을 호출하여 RenderUnit / TF의 Depth를 갱신한 후 Clipping, RenderUnit 리셋과 같은 후속 처리를 한다.
				// >> 렌더유닛이 완성되지 않았거나 서브 메시 그룹이 먼저 호출되는 경우 Depth가 잘못 적용되는 문제가 발생한다.

				//변경
				//- 순서를 변경하여 RenderUnit을 먼저 체크 및 생성한다.
				//- Sort는 값 할당 없이 먼저 수행한다.
				//- Link 이후, Root Mesh Group에 한해서 Depth 할당을 다시 한다.

				//참고 REV_MG는 Child > Root 순서로 호출되는 리스트다.


				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-5");

				//<REV_MG>
				for (int iMeshGroup = 0; iMeshGroup < nRevMeshGroups; iMeshGroup++)
				{
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					//기존 렌더유닛 검토 및 다시 생성) (위치 변경 v1.4.2)
					meshGroup.ResetRenderUnitsWithoutRefreshEditor();

					//단순 정렬
					meshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);


					//추가 : Clipping 후속 처리를 한다.
					apTransform_Mesh meshTransform = null;
					int nChildMeshTFs = meshGroup._childMeshTransforms != null ? meshGroup._childMeshTransforms.Count : 0;
					if(nChildMeshTFs > 0)
					{
						for (int iChild = 0; iChild < nChildMeshTFs; iChild++)
						{
							meshTransform = meshGroup._childMeshTransforms[iChild];

							if (meshTransform._isClipping_Parent)
							{
								//Clipped Mesh를 검색해서 연결한다.
								//찾은 이후엔 Sort를 해준다.
								int nClipMeshes = meshTransform._clipChildMeshes != null ? meshTransform._clipChildMeshes.Count : 0;
								if(nClipMeshes > 0)
								{
									for (int iClip = 0; iClip < nClipMeshes; iClip++)
									{
										apTransform_Mesh.ClipMeshSet clipSet = meshTransform._clipChildMeshes[iClip];
										int childMeshID = clipSet._transformID;
										apTransform_Mesh childMeshTF = meshGroup.GetMeshTransform(childMeshID);
										if (childMeshTF != null)
										{
											clipSet._meshTransform = childMeshTF;
											//clipSet._renderUnit = meshGroup.GetRenderUnit(childMeshTF);//삭제 v1.5.0
										}
										else
										{
											clipSet._meshTransform = null;
											clipSet._transformID = -1;
											//clipSet._renderUnit = null;//삭제 v1.5.0
										}
									}

									meshTransform._clipChildMeshes.RemoveAll(delegate (apTransform_Mesh.ClipMeshSet a)
									{
										return a._transformID < 0;
									});
								}
							}
							else
							{
								if(meshTransform._clipChildMeshes == null)
								{
									meshTransform._clipChildMeshes = new List<apTransform_Mesh.ClipMeshSet>();
								}
								meshTransform._clipChildMeshes.Clear();//<<이건 일단 초기화 하지말자
							}

							meshTransform.SortClipMeshTransforms();
						}
					}
					


					//이전 >> 위치가 변경되었다 [v1.4.2]
					////여기서 RenderUnit을 모두 리셋한다. (기존 렌더유닛 검토 및 다시 생성)
					//meshGroup.ResetRenderUnitsWithoutRefreshEditor();


					meshGroup.RefreshAutoClipping();
					if (meshGroup._rootRenderUnit != null)
					{
						meshGroup._rootRenderUnit.ReadyToUpdate();
					}
				}


				//UnityEngine.Profiling.Profiler.EndSample();

				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-6");

				//추가 [v1.4.2] Root Mesh Group에 대해 Depth를 갱신하는 Sorting을 여기서 하자
				//Root Mesh Group만 체크하므로 [REV_MG]를 따르지 않는다.
				int nMeshGroups = _meshGroups != null ? _meshGroups.Count : 0;
				if(nMeshGroups > 0)
				{
					for (int iMeshGroup = 0; iMeshGroup < nMeshGroups; iMeshGroup++)
					{
						apMeshGroup meshGroup = _meshGroups[iMeshGroup];
						if(meshGroup._parentMeshGroup == null && meshGroup._parentMeshGroupID < 0)
						{
							//Root MeshGroup인 경우
							//Sort 후 Depth 할당까지 하자
							meshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);
						}
					}
				}
				

				//Debug.LogWarning("<Link And Refresh In Editor> : Modifier Test");

				//UnityEngine.Profiling.Profiler.EndSample();

				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-7");

				//3. MeshGroup -> Modifier를 돌면서 삭제된 meshTransform / meshGroupTransform / Bone을 잡고 있는 경우 삭제한다.
				//<REV_MG>
				for (int iMeshGroup = 0; iMeshGroup < nRevMeshGroups; iMeshGroup++)
				{
					apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

					int nModifiers = meshGroup._modifierStack._modifiers != null ? meshGroup._modifierStack._modifiers.Count : 0;
					if(nModifiers == 0)
					{
						continue;
					}
					
					meshGroup._modifierStack._modifiers.RemoveAll(delegate(apModifierBase a)
					{
						return a == null;
					});

					//모디파이어 개수 다시 계산
					nModifiers = meshGroup._modifierStack._modifiers != null ? meshGroup._modifierStack._modifiers.Count : 0;

					for (int iMod = 0; iMod < nModifiers; iMod++)
					{
						apModifierBase modifier = meshGroup._modifierStack._modifiers[iMod];
						if(modifier == null)
						{
							continue;
						}

						//여기서 Modifier Link를 다시 해야한다.
						
						//apMeshGroup meshGroupOfTransform = null;
						apMeshGroup meshGroupOfBone = null;

						int nPSGs = modifier._paramSetGroup_controller != null ? modifier._paramSetGroup_controller.Count : 0;

						if(nPSGs > 0)
						{
							continue;
						}

						for (int iPSG = 0; iPSG < nPSGs; iPSG++)
						{
							apModifierParamSetGroup modPSG = modifier._paramSetGroup_controller[iPSG];

							switch (modPSG._syncTarget)
							{
								case apModifierParamSetGroup.SYNC_TARGET.Bones:
								case apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey:
									//안쓰는 값
									break;
								case apModifierParamSetGroup.SYNC_TARGET.Controller:
									//Controller 체크해볼 필요 있다.
									modPSG._keyControlParam = _controller.FindParam(modPSG._keyControlParamID);
									
									break;

								case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
									modPSG._keyAnimClip = GetAnimClip(modPSG._keyAnimClipID);
									modPSG._keyAnimTimeline = null;
									modPSG._keyAnimTimelineLayer = null;
									if(modPSG._keyAnimClip != null)
									{
										modPSG._keyAnimTimeline = modPSG._keyAnimClip.GetTimeline(modPSG._keyAnimTimelineID);
										if(modPSG._keyAnimTimeline != null)
										{
											modPSG._keyAnimTimelineLayer = modPSG._keyAnimTimeline.GetTimelineLayer(modPSG._keyAnimTimelineLayerID);
										}
									}
									
									break;
							}

							int nParamSets = modPSG._paramSetList != null ? modPSG._paramSetList.Count : 0;
							if(nParamSets == 0)
							{
								continue;
							}

							for (int iPS = 0; iPS < nParamSets; iPS++)
							{
								apModifierParamSet modPS = modPSG._paramSetList[iPS];

								if (modPS._meshData != null)
								{
									//하위의 MeshGroup Transform이 삭제될 수 있도록
									//적절하지 않은 MeshData를 삭제하자
									//int nRemoved = modPS._meshData.RemoveAll(delegate (apModifiedMesh a)
									modPS._meshData.RemoveAll(delegate (apModifiedMesh a)
									{
										if (meshGroup != null)
										{
											if (a._isMeshTransform)
											{
												//MeshTransform이 유효한지 찾자
												a._transform_Mesh = meshGroup.GetMeshTransformRecursive(a._transformUniqueID);
												if (a._transform_Mesh == null || a._transform_Mesh._mesh == null)
												{
													//Mesh Transform이 없다. 삭제
													return true;
												}
											}
											else
											{
												//MeshGroupTransform이 유효한지 찾자
												a._transform_MeshGroup = meshGroup.GetMeshGroupTransformRecursive(a._transformUniqueID);
												if (a._transform_MeshGroup == null || a._transform_MeshGroup._meshGroup == null)
												{
													//MeshGroup Transform이 없다. 삭제
													return true;
												}
											}
										}

										return false;
									});
								}

								//적절하지 않은 Bone Data를 삭제하자
								if (modPS._boneData != null)
								{
									modPS._boneData.RemoveAll(delegate (apModifiedBone a)
									{
										meshGroupOfBone = GetMeshGroup(a._meshGropuUniqueID_Bone);

										if(meshGroupOfBone != null)
										{
											a._bone = meshGroupOfBone.GetBone(a._boneID);
											if(a._bone == null)
											{
												//Bone이 없다. 삭제
												//Debug.LogError("ModBone - Bone : 삭제됨");
												return true;
											}
										}

										return false;
									});
								}
							}
						}
					}
				}
				
				//UnityEngine.Profiling.Profiler.EndSample();

				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-8");

				//Root Unit도 갱신하자
				if (_mainMeshGroupList == null)		{ _mainMeshGroupList = new List<apMeshGroup>(); }
				else								{ _mainMeshGroupList.Clear(); }

				if (_mainMeshGroupIDList == null) { _mainMeshGroupIDList = new List<int>(); }

				for (int iMGID = 0; iMGID < _mainMeshGroupIDList.Count; iMGID++)
				{
					int mainMeshGroupID = _mainMeshGroupIDList[iMGID];
					bool isValidMeshGroupID = false;

					if (mainMeshGroupID >= 0)
					{
						apMeshGroup mainMeshGroup = GetMeshGroup(mainMeshGroupID);
						if (mainMeshGroup != null)
						{
							if (!_mainMeshGroupList.Contains(mainMeshGroup))
							{
								_mainMeshGroupList.Add(mainMeshGroup);
								isValidMeshGroupID = true;
							}
						}
					}
					if (!isValidMeshGroupID)
					{
						_mainMeshGroupIDList[iMGID] = -1;//<<이건 삭제하자
					}
				}

				//일단 유효하지 못한 ID는 삭제하자
				_mainMeshGroupIDList.RemoveAll(delegate (int a)
				{
					return a < 0;
				});

				//기존의 RootUnit중 삭제할 것 먼저 빼자
				if (_rootUnits == null)
				{
					_rootUnits = new List<apRootUnit>();
				}
				_rootUnits.RemoveAll(delegate (apRootUnit a)
				{
					//유효한 MeshGroup을 가지지 않는 경우
					return a._childMeshGroup == null
							|| !_meshGroups.Contains(a._childMeshGroup)
							|| !_mainMeshGroupList.Contains(a._childMeshGroup);
				});
				


				//UnityEngine.Profiling.Profiler.EndSample();

				//UnityEngine.Profiling.Profiler.BeginSample("Link 3-9");

				//재활용을 위해서 리스트를 새로 만들자
				List<apRootUnit> prevRootUnits = new List<apRootUnit>();
				for (int iRootUnit = 0; iRootUnit < _rootUnits.Count; iRootUnit++)
				{
					prevRootUnits.Add(_rootUnits[iRootUnit]);
				}

				//리스트 클리어
				_rootUnits.Clear();
				for (int iMainMesh = 0; iMainMesh < _mainMeshGroupList.Count; iMainMesh++)
				{
					apMeshGroup meshGroup = _mainMeshGroupList[iMainMesh];
					
					//재활용 가능한지 확인하자
					//이전 (GC 발생)
					//apRootUnit existRootUnit = prevRootUnits.Find(delegate(apRootUnit a)
					//{
					//	return a._childMeshGroup == meshGroup;
					//});

					//변경 v1.5.0
					s_LinkRefresh_RootUnitMeshGroup = meshGroup;
					apRootUnit existRootUnit = prevRootUnits.Find(s_LinkRefresh_FindRootUnitByMeshGroup_Func);


					if (existRootUnit != null)
					{
						//있다. 리스트에 넣자
						existRootUnit.SetPortrait(this);
						_rootUnits.Add(existRootUnit);
					}
					else
					{
						//없다. 새로 추가
						apRootUnit newRootUnit = new apRootUnit();

						newRootUnit.SetPortrait(this);
						newRootUnit.SetMeshGroup(meshGroup);

						_rootUnits.Add(newRootUnit);
					}
				}


				//UnityEngine.Profiling.Profiler.EndSample();
				
				//UnityEngine.Profiling.Profiler.EndSample();
			}
			//isResetLink 끝-------------------
			
			//모든 모디파이어가 아닌 특정 AnimClip에 대한 모디파이어를 제외할 것인가.
			bool isLinkAllMeshGroups = false;
			bool isSkipAllAnimModifier = false;//모든 Anim 모디파이어 스킵
			bool isSkipUnselectedAnimPSGs = false;//선택되지 않은 Anim 모디파이어의 PSG 스킵
			apAnimClip selectedAnimClip = null;

			
			bool isNeedToRefreshOtherMeshGroups = false;
			apMeshGroup curSelectedMeshGroup = null;


			//UnityEngine.Profiling.Profiler.BeginSample("Link 4");

			//메시 그룹 링크 다시 확인하는 코드
			//최적화 20.4.3 : 이 코드들이 에디터를 무겁게 한다.
			//만약 특정 메시 그룹을 선택했거나, 특정 AnimClip을 선택했다면, 그 외의 메시 그룹을 갱신하지 말자.
			//revMeshGroups에서 대상이 되는 메시 그룹의 부모/자식들은 제외한다.
			if(linkRefreshRequest != null)
			{
				if(linkRefreshRequest.Request_MeshGroup == apUtil.LR_REQUEST__MESHGROUP.SelectedMeshGroup && linkRefreshRequest.MeshGroup != null)
				{
					//대상이 되는 메시 그룹과 관련된 모든 메시 그룹을 찾자.
					List<apMeshGroup> targetMeshGroups = new List<apMeshGroup>();

					if(linkRefreshRequest.MeshGroup != null)
					{
						FindAllParentAndChildrenMeshGroups(linkRefreshRequest.MeshGroup, targetMeshGroups);
					}

					revMeshGroups = targetMeshGroups;//일부 메시 그룹만 선택을 한다.
					nRevMeshGroups = revMeshGroups != null ? revMeshGroups.Count : 0;//개수도 다시 계산

					//Debug.LogWarning("Cur MeshGroups : " + revMeshGroups.Count);
				}

				//모든 객체를 대상으로 Link를 한다.
				isLinkAllMeshGroups = linkRefreshRequest.Request_MeshGroup == apUtil.LR_REQUEST__MESHGROUP.AllMeshGroups;

				//모든 Anim 모디파이어를 생략한다. (그 외의 모디파이어는 생략하지 않음)
				isSkipAllAnimModifier = linkRefreshRequest.Request_Modifier == apUtil.LR_REQUEST__MODIFIER.AllModifiers_ExceptAnimMods;

				//특정 AnimClip에 대한 PSG를 제외한 나머지를 생략한다. (Anim 모디파이어 중에서)
				isSkipUnselectedAnimPSGs = linkRefreshRequest.Request_PSG == apUtil.LR_REQUEST__PSG.SelectedAnimClipPSG_IfAnimModifier;
				selectedAnimClip = linkRefreshRequest.AnimClip;

				isNeedToRefreshOtherMeshGroups = (revMeshGroups.Count > 1);
				curSelectedMeshGroup = linkRefreshRequest.MeshGroup;
			}
			else
			{
				isLinkAllMeshGroups = true;//Request가 없다면 모든 객체를 대상으로 Link 수행
			}

			//UnityEngine.Profiling.Profiler.EndSample();


			//UnityEngine.Profiling.Profiler.BeginSample("Link 5");

			//<REV_MG>
			for (int iMeshGroup = 0; iMeshGroup < nRevMeshGroups; iMeshGroup++)
			{
				apMeshGroup meshGroup = revMeshGroups[iMeshGroup];

				//meshGroup._modifierStack.RefreshAndSort(false);//이전
				//첫 Link시, 잘못된 데이터가 있으면 삭제를 한다.
				//UnityEngine.Profiling.Profiler.BeginSample("Link 5-1 - Refresh And Sort");

				meshGroup._modifierStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.Keep,
															apModifierStack.REFRESH_OPTION_REMOVE.RemoveNullModifiers);//변경 22.12.13

				//UnityEngine.Profiling.Profiler.EndSample();

				//UnityEngine.Profiling.Profiler.BeginSample("Link 5-2 - Bone");

				//Bone 연결 
				//Root 리스트는 일단 날리고 BoneAll 리스트를 돌면서 필요한걸 넣어주자
				//이후엔 Root -> Child 방식으로 순회
				if(meshGroup._boneList_Root == null) { meshGroup._boneList_Root = new List<apBone>(); }
				meshGroup._boneList_Root.Clear();

				int nBoneListAll = meshGroup._boneList_All != null ? meshGroup._boneList_All.Count : 0;
				if(nBoneListAll > 0)
				{
					apBone bone = null;
					for (int iBone = 0; iBone < nBoneListAll; iBone++)
					{
						bone = meshGroup._boneList_All[iBone];
						if (bone._childBones == null)
						{
							bone._childBones = new List<apBone>();
						}
						bone._childBones.Clear();
					}

					for (int iBone = 0; iBone < nBoneListAll; iBone++)
					{
						bone = meshGroup._boneList_All[iBone];

						apBone parentBone = null;
						if (bone._parentBoneID >= 0)
						{
							parentBone = meshGroup.GetBone(bone._parentBoneID);
						}

						bone.Link(meshGroup, parentBone, this);

						if (parentBone == null)
						{
							//Parent가 없다면 Root 본이다.
							meshGroup._boneList_Root.Add(bone);
						}
					}

					//추가 5.9 : Bone의 Check Validation 함수를 호출해야 한다.
					for (int iBone = 0; iBone < nBoneListAll; iBone++)
					{
						meshGroup._boneList_All[iBone].CheckIKControllerValidation();
					}
				}


				int curBoneIndex = 0;
				int nRootBoneList = meshGroup._boneList_Root != null ? meshGroup._boneList_Root.Count : 0;
				if(nRootBoneList > 0)
				{
					for (int iRoot = 0; iRoot < nRootBoneList; iRoot++)
					{
						apBone rootBone = meshGroup._boneList_Root[iRoot];
						//TODO : MeshGroup이 Transform으로 있는 경우에 Transform Matrix를 넣어줘야한다.
						rootBone.LinkRecursive(0);
						curBoneIndex = rootBone.SetBoneIndex(curBoneIndex) + 1;
					}
				}
				

				//UnityEngine.Profiling.Profiler.EndSample();
				//UnityEngine.Profiling.Profiler.BeginSample("Link 5-3 - Modifier");

				List<apModifierBase> modifiers = meshGroup._modifierStack._modifiers;
				int nModifiers = modifiers != null ? modifiers.Count : 0;
				if (nModifiers > 0)
				{
					for (int iMod = 0; iMod < nModifiers; iMod++)
					{
						apModifierBase mod = modifiers[iMod];

						//추가 : Portrait를 연결해준다.
						mod.LinkPortrait(this);

						mod._meshGroup = GetMeshGroup(mod._meshGroupUniqueID);

						//삭제 조건1 - MeshGroup이 없다
						if (mod._meshGroup == null)
						{
							//Debug.LogError("No MeshGroup Modifier");
							continue;
						}

						//>> 최적화 20.4.3
						if (isSkipAllAnimModifier && mod.IsAnimated)
						{
							//요청에 따라 Anim 모디파이어는 생략하자
							continue;
						}


						List<apModifierParamSetGroup> paramSetGroups = mod._paramSetGroup_controller;
						int nPSGs = paramSetGroups != null ? paramSetGroups.Count : 0;
						if (nPSGs > 0)
						{
							for (int iPSGroup = 0; iPSGroup < nPSGs; iPSGroup++)
							{
								apModifierParamSetGroup paramSetGroup = paramSetGroups[iPSGroup];

								//>> 최적화 20.4.3 : 만약 애니메이션 설정시, 해당 애니메이션에 관련된 것들만 갱신한다.
								if (isSkipUnselectedAnimPSGs && mod.IsAnimated)
								{
									if (paramSetGroup._keyAnimClip != linkRefreshRequest.AnimClip)
									{

										//특정 AnimClip을 제외한 다른 AnimClip에 대한 ParamSetGroup은 Link를 생략한다.
										continue;
									}
								}

								int nParamSets = paramSetGroup._paramSetList != null ? paramSetGroup._paramSetList.Count : 0;

								//List<apModifierParamSet> paramSets = mod._paramSetList;
								//1. Key를 세팅해주자
								switch (paramSetGroup._syncTarget)
								{
									case apModifierParamSetGroup.SYNC_TARGET.Static:
										break;

									case apModifierParamSetGroup.SYNC_TARGET.Controller:
										paramSetGroup._keyControlParam = GetControlParam(paramSetGroup._keyControlParamID);
										break;

									case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
										{
											//AnimClip과 연동을 먼저 한다.
											// ParamSetGroup -> AnimClip과 연동
											paramSetGroup._keyAnimClip = GetAnimClip(paramSetGroup._keyAnimClipID);
											if (paramSetGroup._keyAnimClip == null)
											{
												paramSetGroup._keyAnimClipID = -1;//<<삭제 하자
												break;
											}

											paramSetGroup._keyAnimTimeline = paramSetGroup._keyAnimClip.GetTimeline(paramSetGroup._keyAnimTimelineID);

											if (paramSetGroup._keyAnimTimeline == null)
											{
												paramSetGroup._keyAnimTimelineID = -1;
												break;
											}

											paramSetGroup._keyAnimTimelineLayer = paramSetGroup._keyAnimTimeline.GetTimelineLayer(paramSetGroup._keyAnimTimelineLayerID);

											if (paramSetGroup._keyAnimTimelineLayer == null)
											{
												paramSetGroup._keyAnimTimelineLayerID = -1;
												break;
											}

											//추가) 상호 연동을 해주자
											paramSetGroup._keyAnimTimelineLayer.LinkParamSetGroup(paramSetGroup);

											//키프레임이면 여기서 한번더 링크를 해주자
											
											if (nParamSets > 0)
											{
												//[v1.5.0]
												//버그로 인하여 "동일한 키프레임"을 가리키는 ParamSet이 생성되기도 한다.
												//리스트를 만들어서 이미 동일한 키프레임의 경우 무효로 만들자
												List<apAnimKeyframe> checkedKeyframes = new List<apAnimKeyframe>();

												apModifierParamSet paramSet = null;
												for (int iPS = 0; iPS < nParamSets; iPS++)
												{
													paramSet = paramSetGroup._paramSetList[iPS];
													int keyframeID = paramSet._keyframeUniqueID;

													apAnimKeyframe targetKeyframe = paramSetGroup._keyAnimTimelineLayer.GetKeyframeByID(keyframeID);
													if (targetKeyframe != null)
													{
														if(!checkedKeyframes.Contains(targetKeyframe))
														{
															//연결되지 않은 키프레임이다.
															paramSet.LinkSyncKeyframe(targetKeyframe);
															checkedKeyframes.Add(targetKeyframe);
														}
														else
														{
															//Debug.LogError("에러 : 이미 연결이 완료된 키프레임에 다른 ModParamSet이 연결을 시도했다.");
															paramSet._keyframeUniqueID = -1;//삭제 처리
														}
														
													}
													else
													{
														//못찾았다. > Keyframe 연동 에러
														paramSet._keyframeUniqueID = -1;
													}

												}

												//"키프레임 연동" 방식에서 비어있는 키프레임이라면?
												paramSetGroup._paramSetList.RemoveAll(delegate (apModifierParamSet a)
												{
													return a._keyframeUniqueID < 0;
												});
											}
										}
										break;
								}




								List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;
								nParamSets = paramSetGroup._paramSetList != null ? paramSetGroup._paramSetList.Count : 0;
								if (nParamSets > 0)
								{
									apModifierParamSet paramSet = null;
									for (int iParamSet = 0; iParamSet < nParamSets; iParamSet++)
									{
										paramSet = paramSets[iParamSet];

										//Link를 해주자
										paramSet.LinkParamSetGroup(paramSetGroup);

										List<apModifiedMesh> meshData = paramSet._meshData;
										apTransform_Mesh meshTransform = null;
										apTransform_MeshGroup meshGroupTransform = null;
										apRenderUnit renderUnit = null;


										//1. ModMesh
										int nMeshData = meshData != null ? meshData.Count : 0;
										if (nMeshData > 0)
										{
											for (int iMesh = 0; iMesh < meshData.Count; iMesh++)
											{
												apModifiedMesh modMesh = meshData[iMesh];

												//추가 : Modifier의 meshGroup과 Transform의 MeshGroup을 분리한다.
												apMeshGroup meshGroupOfTransform = null;

												if (modMesh._isRecursiveChildTransform)
												{
													//Mesh Group 다시 링크 (다르다)
													meshGroupOfTransform = GetMeshGroup(modMesh._meshGroupUniqueID_Transform);
												}
												else
												{
													//동일한 MeshGroup이다.
													meshGroupOfTransform = meshGroup;
												}

												modMesh._meshGroupUniqueID_Modifier = meshGroup._uniqueID;



												//변경 : 타입 대신 값을 보고 판단한다.
												if (modMesh._transformUniqueID >= 0 && meshGroupOfTransform != null)
												{
													if (modMesh._isMeshTransform)
													{
														meshTransform = meshGroupOfTransform.GetMeshTransform(modMesh._transformUniqueID);

														if (meshTransform != null)
														{
															renderUnit = meshGroup.GetRenderUnit(meshTransform);
															modMesh.Link_MeshTransform(meshGroup, meshGroupOfTransform, meshTransform, renderUnit, this);
														}
													}
													else
													{
														meshGroupTransform = meshGroupOfTransform.GetMeshGroupTransform(modMesh._transformUniqueID);

														if (meshGroupTransform != null)
														{
															renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
															modMesh.Link_MeshGroupTransform(meshGroup, meshGroupOfTransform, meshGroupTransform, renderUnit);
														}
													}
												}
											}


											//int nRemove = paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
											paramSet._meshData.RemoveAll(delegate (apModifiedMesh a)
											{
												return a._meshGroupOfModifier == null || a._meshGroupOfTransform == null;
											});
										}
										


										//---------------------------------------------------------------------------------
										//2. Bone 연동을 하자

										List<apModifiedBone> boneData = paramSet._boneData;
										apModifiedBone modBone = null;

										int nBoneData = boneData != null ? boneData.Count : 0;
										if (nBoneData > 0)
										{
											for (int iModBone = 0; iModBone < nBoneData; iModBone++)
											{
												modBone = boneData[iModBone];
												apMeshGroup meshGroupOfBone = GetMeshGroup(modBone._meshGropuUniqueID_Bone);
												apMeshGroup meshGroupOfModifier = GetMeshGroup(modBone._meshGroupUniqueID_Modifier);
												if (meshGroupOfBone == null || meshGroupOfModifier == null)
												{
													//Link Error : Mod Bone 링크 실패 [MeshGroup]
													continue;
												}

												apBone bone = meshGroupOfBone.GetBone(modBone._boneID);
												if (bone == null)
												{
													//Link Error : Mod Bone 링크 실패
													continue;
												}

												meshGroupTransform = meshGroupOfModifier.GetMeshGroupTransformRecursive(modBone._transformUniqueID);
												if (meshGroupTransform == null)
												{
													//Link Error : Mod Bone 링크 실패 [MeshGroup Transform]
													continue;
												}

												renderUnit = meshGroupOfModifier.GetRenderUnit(meshGroupTransform._transformUniqueID, false);
												if (renderUnit == null)
												{
													//Debug.LogError("Link Error : Mod Bone 링크 실패 [Render Unit]");
													//continue;
													//다시 체크 및 보정
													if (meshGroupOfBone == meshGroupOfModifier)
													{
														meshGroupTransform = meshGroupOfModifier._rootMeshGroupTransform;
													}
													else
													{
														meshGroupTransform = meshGroupOfModifier.FindChildMeshGroupTransform(meshGroupOfBone);
													}

													if (meshGroupTransform != null)
													{
														renderUnit = meshGroupOfModifier.GetRenderUnit(meshGroupTransform._transformUniqueID, false);
														//Debug.LogError("잘못된 ModBone 연결이 보정되었다.");
														modBone.Init(meshGroupOfModifier._uniqueID, meshGroupOfBone._uniqueID, meshGroupTransform._transformUniqueID, bone);
													}
												}

												modBone.Link(meshGroupOfModifier, meshGroupOfBone, bone, renderUnit, meshGroupTransform);
											}

											//연동 안된 ModBone은 삭제하자
											//---------------------------------------------------------------------------------
											boneData.RemoveAll(delegate (apModifiedBone a)
											{
												return a._bone == null || a._meshGroup_Bone == null || a._meshGroup_Modifier == null;
											});
										}
										

									}
								}
							}
						}
						


						//mod.RefreshParamSet();
						if (mod.IsAnimated)
						{
							//애니메이션 타입이라면 > 일부 AnimClip에 대한 처리하자.
							mod.RefreshParamSet(linkRefreshRequest);//<<단순 변경시 linkRefreshRequest로 입력 되어야 한다.
																	//mod.RefreshParamSet(null);//<<삭제시 이게 null로 입력되어야 한다.
						}
						else
						{
							mod.RefreshParamSet(null);
						}
					}

					meshGroup._modifierStack._modifiers.RemoveAll(delegate (apModifierBase a)
					{
						return a._meshGroup == null;
					});
				}
				
				//UnityEngine.Profiling.Profiler.EndSample();
				//ModStack의 CalculateParam을 모두 지우고 다시 만들자
				
				//이 조건문 추가 20.4.3 : 모든 메시 그룹에 대해서 Refresh를 할 경우에만
				//단, 여러개의 메시그룹을 대상으로 하는 경우에는 타겟을 제외한 나머지 객체는 Refresh를 해야한다.
				//UnityEngine.Profiling.Profiler.BeginSample("Link 5-4 - Refresh Link");

				if(isLinkAllMeshGroups)
				{
					//Debug.Log(">>>> All MeshGroups (Other)");
					meshGroup.RefreshModifierLink(null);
				}
				else if(curSelectedMeshGroup != null 
					&& isNeedToRefreshOtherMeshGroups
					&& curSelectedMeshGroup != meshGroup)
				{
					//Debug.Log(">>>> 타겟이 아닌 다른 메시 그룹의 모디파이어를 Refresh [" + meshGroup._name + "]");
					meshGroup.RefreshModifierLink(linkRefreshRequest);
				}

				//UnityEngine.Profiling.Profiler.EndSample();
			}


			//UnityEngine.Profiling.Profiler.EndSample();
			//UnityEngine.Profiling.Profiler.BeginSample("Link 6");

			if(curSelectedMeshGroup != null && !isLinkAllMeshGroups)
			{
				//모든 메시 그룹을 대상으로 한게 아닌데 대상 메시 그룹이 있다면
				curSelectedMeshGroup.RefreshModifierLink(linkRefreshRequest);
			}

			//UnityEngine.Profiling.Profiler.EndSample();
			//UnityEngine.Profiling.Profiler.BeginSample("Link 7");

			if (isSkipUnselectedAnimPSGs && linkRefreshRequest.AnimClip != null)
			{
				//특정 AnimClip만 검사
				linkRefreshRequest.AnimClip.LinkEditor(this);
				linkRefreshRequest.AnimClip.RemoveUnlinkedTimeline();
			}
			else
			{
				//전체 검사
				int nAnimClips = _animClips != null ? _animClips.Count : 0;
				if (nAnimClips > 0)
				{
					apAnimClip animClip = null;
					for (int i = 0; i < _animClips.Count; i++)
					{
						animClip = _animClips[i];
						animClip.LinkEditor(this);
						animClip.RemoveUnlinkedTimeline();
					}
				}
				
			}
			

			//UnityEngine.Profiling.Profiler.EndSample();
			//UnityEngine.Profiling.Profiler.BeginSample("Link 8");
			
			//추가 9.30 : 만약, 선택한 MeshGroup에 하위 MeshGroup이 있다면,
			//전체적으로 하위 MeshGroup으로의 연결을 다시 해야한다.
			//위에서 연결이 흐트러졌기 때문
			
			if(linkRefreshRequest != null && curSelectedMeshGroup != null)
			{
				//하위에 메시 그룹이 있거나, 모든 메시 그룹을 대상으로 하지 않았을 경우
				if((curSelectedMeshGroup._childMeshGroupTransforms != null && curSelectedMeshGroup._childMeshGroupTransforms.Count > 0)
					|| !isLinkAllMeshGroups)
				{
					//UnityEngine.Profiling.Profiler.BeginSample("Link 8-1");
					curSelectedMeshGroup.LinkModMeshRenderUnits(linkRefreshRequest);
					//UnityEngine.Profiling.Profiler.EndSample();

					//UnityEngine.Profiling.Profiler.BeginSample("Link 8-2");
					curSelectedMeshGroup.RefreshModifierLink(linkRefreshRequest);
					//UnityEngine.Profiling.Profiler.EndSample();
				}
			}


			//UnityEngine.Profiling.Profiler.EndSample();

			//삭제 v1.5.0 : GC.Collect는 너무 많은 성능 스파이크를 일으킨다.
			//Debug.LogError("TODO : GC가 너무 커서 여기서 성능이 크게 떨어진다.");
			//Debug.LogError("일반적인 작업에서는 GC를 호출하지 않고, 메뉴 전환시에 GC를 호출하자");
			//System.GC.Collect();

			//UnityEngine.Profiling.Profiler.EndSample();
			
		}


		private static apMeshGroup s_LinkRefresh_RootUnitMeshGroup = null;
		private static Predicate<apRootUnit> s_LinkRefresh_FindRootUnitByMeshGroup_Func = FUNC_LinkRefresh_FindRootUnitByMeshGroup;
		private static bool FUNC_LinkRefresh_FindRootUnitByMeshGroup(apRootUnit a)
		{
			return a._childMeshGroup == s_LinkRefresh_RootUnitMeshGroup;
		}



		// Bake
		//----------------------------------------------------------------



		// 참조용 리스트 관리
		//----------------------------------------------------------------

		//메모리 할당을 방지하는 변수
		private List<apMeshGroup> _tmpReverseMeshGroups = null;

		/// <summary>
		/// 계층적으로 설계된 MeshGroup에 맞게 리스트를 다시 정리하여 리턴한다.
		/// 인덱스 앞쪽에는 Child가 위치하고, 뒤로 갈 수록 Parent/Root가 나타난다.
		/// </summary>
		/// <param name="srcMeshGroup"></param>
		/// <returns></returns>
		private List<apMeshGroup> GetReverseMeshGroupList(List<apMeshGroup> srcMeshGroups)
		{
			if(_tmpReverseMeshGroups == null)
			{
				_tmpReverseMeshGroups = new List<apMeshGroup>();
			}
			_tmpReverseMeshGroups.Clear();

			int nSrcMeshGroups = srcMeshGroups != null ? srcMeshGroups.Count : 0;
			if (nSrcMeshGroups > 0)
			{
				apMeshGroup curMeshGroup = null;
				for (int i = 0; i < nSrcMeshGroups; i++)
				{
					curMeshGroup = srcMeshGroups[i];
					if (!_tmpReverseMeshGroups.Contains(curMeshGroup))
					{
						FindReverseMeshGroupListRecursive(curMeshGroup, _tmpReverseMeshGroups);
					}
				}
			}
			
			return _tmpReverseMeshGroups;
		}


		private void FindReverseMeshGroupListRecursive(apMeshGroup curMeshGroup, List<apMeshGroup> resultList)
		{
			int nChildTFs = curMeshGroup._childMeshGroupTransforms != null ? curMeshGroup._childMeshGroupTransforms.Count : 0;
			if(nChildTFs > 0)
			{
				apTransform_MeshGroup childMeshGroupTransform = null;
				apMeshGroup childMeshGroup = null;
				for (int iChild = 0; iChild < nChildTFs; iChild++)
				{
					childMeshGroupTransform = curMeshGroup._childMeshGroupTransforms[iChild];
					childMeshGroup = childMeshGroupTransform._meshGroup;
					if (childMeshGroup != null && childMeshGroup != curMeshGroup)
					{
						FindReverseMeshGroupListRecursive(childMeshGroup, resultList);
					}
				}
			}
			if (!resultList.Contains(curMeshGroup))
			{
				resultList.Add(curMeshGroup);
			}
		}


		/// <summary>
		/// 추가 20.4.3 : 입력된 메시 그룹을 포함하여 관련된 모든 메시 그룹을 찾는다. (부모/자식 모두)
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="resultList"></param>
		private void FindAllParentAndChildrenMeshGroups(apMeshGroup targetMeshGroup, List<apMeshGroup> resultList)
		{
			//먼저 최상위 부모를 찾자
			apMeshGroup rootParentMG = targetMeshGroup;
			if (rootParentMG._parentMeshGroup != null)
			{
				while (true)
				{
					if(rootParentMG._parentMeshGroup == null)
					{
						break;
					}
					rootParentMG = rootParentMG._parentMeshGroup;
				}
			}
			
			//Recursive 방식ㅇ로 Root MG
			FindReverseMeshGroupListRecursive(rootParentMG, resultList);
		}


		// ID 관리
		//----------------------------------------------------------------
		//유니크 아이디는 몇가지 타입에 맞게 통합해서 관리한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ClearRegisteredUniqueIDs()
		{
			_IDManager.Clear();
		}

		// 발급된 ID는 관리를 위해 회수한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="target"></param>
		/// <param name="ID"></param>
		public void RegistUniqueID(apIDManager.TARGET target, int ID)
		{
			_IDManager.RegistID(target, ID);
		}
#region [미사용 코드]
		//public void RegistUniqueID_Texture(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Texture.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Texture.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Vertex(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Vert.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Vert.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Mesh(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Mesh.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Mesh.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_MeshGroup(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_MeshGroup.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_MeshGroup.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Transform(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Transform.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Transform.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_Moifier(int uniqueID)
		//{
		//	if (!_registeredUniqueIDs_Modifier.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_Modifier.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_ControlParam(int uniqueID)
		//{
		//	if(!_registeredUniqueIDs_ControlParam.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_ControlParam.Add(uniqueID);
		//	}
		//}

		//public void RegistUniqueID_AnimClip(int uniqueID)
		//{
		//	if(!_registeredUniqueIDs_AnimClip.Contains(uniqueID))
		//	{
		//		_registeredUniqueIDs_AnimClip.Add(uniqueID);
		//	}
		//} 
#endregion



		// 새로운 ID를 발급한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public int MakeUniqueID(apIDManager.TARGET targetType)
		{
			int resultID = _IDManager.MakeUniqueID(targetType);
			//수정 20.1.16 : 발급 즉시 등록하자 (중복 막기 위함)
			//if(resultID >= 0)
			//{
			//	RegistUniqueID(taTyperget, resultID);
			//}

			//MakeUniqueID에서 이미 등록이 되었다. (22.7.12)

			return resultID;
		}
#region [미사용 코드]
		//private int MakeUniqueID(List<int> IDList)
		//{
		//	int nextID = -1;
		//	int cntCheck = 0;
		//	while(true)
		//	{
		//		nextID = UnityEngine.Random.Range(1000, 99999999);
		//		if(!IDList.Contains(nextID))
		//		{
		//			IDList.Add(nextID);
		//			return nextID;
		//		}

		//		cntCheck++;
		//		//회수 제한에 걸렸다.
		//		if(cntCheck > 100)
		//		{
		//			break;
		//		}
		//	}

		//	for (int i = 1; i < 99999999; i++)
		//	{
		//		if(!IDList.Contains(i))
		//		{
		//			IDList.Add(i);
		//			return i;
		//		}
		//	}
		//	return -1;//<< 실패
		//}
		//public int MakeUniqueID_Texture()		{ return MakeUniqueID(_registeredUniqueIDs_Texture); }
		//public int MakeUniqueID_Vertex()		{ return MakeUniqueID(_registeredUniqueIDs_Vert); }
		//public int MakeUniqueID_Mesh()			{ return MakeUniqueID(_registeredUniqueIDs_Mesh); }
		//public int MakeUniqueID_MeshGroup()		{ return MakeUniqueID(_registeredUniqueIDs_MeshGroup); }
		//public int MakeUniqueID_Transform()		{ return MakeUniqueID(_registeredUniqueIDs_Transform); }
		//public int MakeUniqueID_Modifier()		{ return MakeUniqueID(_registeredUniqueIDs_Modifier); }
		//public int MakeUniqueID_ControlParam()	{ return MakeUniqueID(_registeredUniqueIDs_ControlParam); }
		//public int MakeUniqueID_AnimClip()		{ return MakeUniqueID(_registeredUniqueIDs_AnimClip); } 
#endregion


		// 객체 삭제시 ID 회수
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="target"></param>
		/// <param name="unusedID"></param>
		public void PushUnusedID(apIDManager.TARGET target, int unusedID)
		{
			_IDManager.PushUnusedID(target, unusedID);
		}


		//모든 ID를 리셋하고 다시 등록한다.
		//Undo용
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void RefreshAllUniqueIDs()
		{
			_IDManager.Clear();
			
			
			//1. Texture
			apTextureData curTextureData = null;
			for (int i = 0; i < _textureData.Count; i++)
			{
				curTextureData = _textureData[i];
				if(curTextureData == null) { continue; }

				_IDManager.RegistID(apIDManager.TARGET.Texture, curTextureData._uniqueID);
			}

			//2. Mesh + Vertex + Pin
			apMesh curMesh = null;
			for (int i = 0; i < _meshes.Count; i++)
			{
				curMesh = _meshes[i];
				if(curMesh == null) { continue; }

				_IDManager.RegistID(apIDManager.TARGET.Mesh, curMesh._uniqueID);
				curMesh.RefreshVertexAndPinIDs();//<<Vertex ID를 등록한다.
			}

			//3. MeshGroup + Transform + Modifier + Bone
			apMeshGroup curMeshGroup = null;
			for (int i = 0; i < _meshGroups.Count; i++)
			{
				curMeshGroup = _meshGroups[i];
				if(curMeshGroup == null) { continue; }
				
				_IDManager.RegistID(apIDManager.TARGET.MeshGroup, curMeshGroup._uniqueID);


				//MeshGroup -> Transform
				apTransform_Mesh meshTF = null;
				for (int iMeshTF = 0; iMeshTF < curMeshGroup._childMeshTransforms.Count; iMeshTF++)
				{
					meshTF = curMeshGroup._childMeshTransforms[iMeshTF];
					if(meshTF == null) { continue; }

					_IDManager.RegistID(apIDManager.TARGET.Transform, meshTF._transformUniqueID);
				}

				apTransform_MeshGroup mgTF = null;
				for (int iMGTF = 0; iMGTF < curMeshGroup._childMeshGroupTransforms.Count; iMGTF++)
				{
					mgTF = curMeshGroup._childMeshGroupTransforms[iMGTF];
					if(mgTF == null) { continue; }

					_IDManager.RegistID(apIDManager.TARGET.Transform, mgTF._transformUniqueID);
				}

				if(curMeshGroup._rootMeshGroupTransform != null)
				{
					_IDManager.RegistID(	apIDManager.TARGET.Transform, 
											curMeshGroup._rootMeshGroupTransform._transformUniqueID);
				}

				//MeshGroup -> Modifier
				apModifierBase modifier = null;
				for (int iMod = 0; iMod < curMeshGroup._modifierStack._modifiers.Count; iMod++)
				{
					modifier = curMeshGroup._modifierStack._modifiers[iMod];
					if(modifier == null) { continue; }

					_IDManager.RegistID(	apIDManager.TARGET.Modifier,
											modifier._uniqueID);
				}

				apBone bone = null;
				for (int iBone = 0; iBone < curMeshGroup._boneList_All.Count; iBone++)
				{
					bone = curMeshGroup._boneList_All[iBone];
					if(bone == null) { continue; }

					_IDManager.RegistID(	apIDManager.TARGET.Bone,
											bone._uniqueID);

				}
			}

			//4. Control Param
			apControlParam controlParam = null;
			for (int i = 0; i < _controller._controlParams.Count; i++)
			{
				controlParam = _controller._controlParams[i];
				if(controlParam == null) { continue; }

				_IDManager.RegistID(	apIDManager.TARGET.ControlParam,
										controlParam._uniqueID);
			}

			//5. AnimClip + AnimTimeline + AnimTimeline Layer + AnimKeyframe
			apAnimClip animClip = null;
			apAnimTimeline timeline = null;
			apAnimTimelineLayer timelineLayer = null;
			apAnimKeyframe keyframe = null;
			for (int iAnimClip = 0; iAnimClip < _animClips.Count; iAnimClip++)
			{
				animClip = _animClips[iAnimClip];
				if(animClip == null) { continue; }

				_IDManager.RegistID(	apIDManager.TARGET.AnimClip,
										animClip._uniqueID);

				//Timeline
				for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
				{
					timeline = animClip._timelines[iTimeline];
					if(timeline == null) { continue; }

					_IDManager.RegistID(	apIDManager.TARGET.AnimTimeline,
											timeline._uniqueID);

					//Timeline Layer
					for (int iTimelineLayer = 0; iTimelineLayer < timeline._layers.Count; iTimelineLayer++)
					{
						timelineLayer = timeline._layers[iTimelineLayer];
						if(timelineLayer == null) { continue; }

						_IDManager.RegistID(	apIDManager.TARGET.AnimTimelineLayer,
												timelineLayer._uniqueID);

						//Keyframe
						for (int iKeyframe = 0; iKeyframe < timelineLayer._keyframes.Count; iKeyframe++)
						{
							keyframe = timelineLayer._keyframes[iKeyframe];
							if(keyframe == null)
							{
								continue;
							}

							_IDManager.RegistID(	apIDManager.TARGET.AnimKeyFrame,
													keyframe._uniqueID);

						}
					}
				}
			}


		}



		// 추가 20.7.9 : 물리 타이머 갱신 (런타임/에디터 갱신시 호출할 것)
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void CalculatePhysicsTimer()
		{
			if(_physicsTimer == null)
			{
				_physicsTimer = new System.Diagnostics.Stopwatch();
				_physicsTimer.Start();
			}
			float nextDeltaTime = (float)(_physicsTimer.ElapsedMilliseconds / 1000.0f);
			if(nextDeltaTime > 0.0f)
			{
				_physicsDeltaTime = nextDeltaTime;

				//변경
				//v1.4.2 : 경과 시간이 지나치게 크다면 앱이 중단되었거나 FPS가 떨어졌던 것이다.
				//이 경우엔 아예 Delta Time을 0으로 만들어서 현재 프레임을 무효로 만들어야 한다.
				if (_physicsDeltaTime > PHYSICS_SKIP_DELTA_TIME)
				{
					//지나치게 긴 물리 시간 > 0초로 만든다.
					_physicsDeltaTime = 0.0f;
				}
				else if (_physicsDeltaTime > PHYSICS_MAX_DELTA_TIME)
				{
					//적당히 시간이 조금 오버했다. Max로 한정하자
					_physicsDeltaTime = PHYSICS_MAX_DELTA_TIME;
				}


				_physicsTimer.Stop();
				_physicsTimer.Reset();
				_physicsTimer.Start();
			}

			//_physicsDeltaTime = Time.unscaledDeltaTime;
		}

		//화면 캡쳐시에는 물리 시간이 강제된다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void SetPhysicsTimerWhenCapture(float tDelta)
		{
			if(_physicsTimer == null)
			{
				_physicsTimer = new System.Diagnostics.Stopwatch();
				_physicsTimer.Start();
			}

			_physicsDeltaTime = tDelta;
			_physicsTimer.Stop();
			_physicsTimer.Reset();
			_physicsTimer.Start();
		}


		/// <summary>
		/// 추가 22.6.11 : 물리 타이머의 시간을 리셋한다.
		/// </summary>
		public void ResetPhysicsTimer()
		{
			if(_physicsTimer == null)
			{
				_physicsTimer = new System.Diagnostics.Stopwatch();
				_physicsTimer.Start();
			}
			_physicsDeltaTime = 0;
			_physicsTimer.Stop();
			_physicsTimer.Reset();
			_physicsTimer.Start();
		}


#region [미사용 코드]
		//public void PushUniqueID_Texture(int uniquedID)			{ _registeredUniqueIDs_Texture.Remove(uniquedID); }
		//public void PushUniqueID_Vertex(int uniquedID)			{ _registeredUniqueIDs_Vert.Remove(uniquedID); }
		//public void PushUniqueID_Mesh(int uniquedID)			{ _registeredUniqueIDs_Mesh.Remove(uniquedID); }
		//public void PushUniqueID_MeshGroup(int uniquedID)		{ _registeredUniqueIDs_MeshGroup.Remove(uniquedID); }
		//public void PushUniqueID_Transform(int uniquedID)		{ _registeredUniqueIDs_Transform.Remove(uniquedID); }
		//public void PushUniqueID_Modifier(int uniquedID)		{ _registeredUniqueIDs_Modifier.Remove(uniquedID); }
		//public void PushUniqueID_ControlParam(int uniquedID)	{ _registeredUniqueIDs_ControlParam.Remove(uniquedID); }
		//public void PushUniqueID_AnimClip(int uniquedID)		{ _registeredUniqueIDs_AnimClip.Remove(uniquedID); } 
#endregion

		//카메라 관련
		//-------------------------------------------------------------------------------------------------------
		private void CheckAndRefreshCameras(bool isResetCommandBufferWhenCameraChanged = true)
		{
			if(_mainCamera == null)
			{
				_mainCamera = new apOptMainCamera(this);
			}

			if(_transform == null)
			{
				_transform = transform;
			}

			//변경 v1.5.0
			//Refresh의 단계를 세분화
			//1. 빌보드 여부
			//- 빌보드가 아닌 경우 : 카메라 갱신만 한다.
			//- 빌보드인 경우 : 카메라의 Matrix 비교 및 재연산도 한다.

			//2. 멀티 카메라 여부
			//- 멀티 카메라가 아닌 경우 : "지금 카메라가 유효하지 않은 경우"만 갱신을 한다.
			//- 멀티 카메라인 경우 : 항상 씬의 모든 카메라와 비교를 한다. < 이거 옵션으로 설정 가능 (기본 비활성)
			//>> 이걸 멀티 카메라 여부 대신 옵션을 별도로 둔다.

			bool isCameraChanged = false;

			//변경 2019.9.24 : 멀티 카메라도 지원하도록 래핑
			if(_billboardType == BILLBOARD_TYPE.None)
			{
				//빌보드가 아니라면 단순 카메라 리스트 검사(false, false)
				isCameraChanged = _mainCamera.Refresh(false, false, _cameraCheckMode);
				if(isCameraChanged && isResetCommandBufferWhenCameraChanged)
				{
					//카메라가 변경되었다면 > 커맨드 버퍼를 갱신한다.
					ResetMeshesCommandBuffers(false);
				}
				return;
			}
			
			//빌보드라면 카메라의 매트릭스까지 계산(false, true)
			isCameraChanged = _mainCamera.Refresh(false, true, _cameraCheckMode);
			if(isCameraChanged && isResetCommandBufferWhenCameraChanged)
			{
				//카메라가 변경되었다면 > 커맨드 버퍼를 갱신한다.
				ResetMeshesCommandBuffers(false);
			}

			//조건문 추가 v1.5.0 : 바라보는 카메라가 없는 경우에 갱신하면 안된다.
			if(_mainCamera.GetNumberOfCamera() != apOptMainCamera.NumberOfCamera.None)
			{
				if(_billboardType == BILLBOARD_TYPE.Billboard)
				{
					//전체 빌보드
					_transform.rotation = _mainCamera.Rotation;
				}
				else
				{
					//Up 고정 빌보드
					_transform.rotation = Quaternion.LookRotation(_mainCamera.Forward, Vector3.up);//변경
				}

				//추가 v1.5.0
				//옵션에 따라선, 부모 Transform의 Rotation을 더하거나 Up Vector를 맞춘다.
				if(_billboardParentRotation != BILLBOARD_PARENT_ROTATION.Ignore)
				{
					//Ignore 외의 값을 갖는 경우
					Transform parentTF = _transform.parent;
					if(parentTF != null)
					{
						if(_billboardParentRotation == BILLBOARD_PARENT_ROTATION.PitchYawRoll)
						{
							//일반 더하기 연산 (Local) - Yaw-Pitch-Roll로 동작
							//_transform.rotation *= parentTF.rotation;
							_transform.rotation *= parentTF.localRotation;
						}
						else
						{
							//Up Vector 동기화
							_transform.rotation = Quaternion.LookRotation(_mainCamera.Forward, parentTF.up);
						}
						
					}
				}
				
				
				//카메라 좌표계에서의 Z값 (ZDepth)
				//_zDepthOnPerspectiveCam = _curCamera.worldToCameraMatrix.MultiplyPoint3x4(_transform.position).z;//미사용 코드

				//여기선 Orthographic SortMode로 해야한다.
				//_curCamera.transparencySortMode = TransparencySortMode.Orthographic;

				//추가 20.9.15
				//Rotation 전용의 행렬/역행렬을 준비한다.
				_rotationOnlyMatrixIfBillboard = Matrix4x4.TRS(Vector3.zero, _transform.rotation, Vector3.one);
				_invRotationOnlyMatrixIfBillboard = _rotationOnlyMatrixIfBillboard.inverse;
			}
			
		}

		//추가. 업데이트가 끝나면 이 함수를 호출하자.
		private void PostUpdate()
		{
			//빌보드인 경우, 현재 프레임에서의 위치를 저장한다. (나중에 "이전 프레임의 위치"로서 가져올 수 있게)
			_posW_Prev1F = _transform.position;


			//추가 v1.4.7 : 루트 유닛 변경에 따른 물리 튐 현상 버그 변수 초기화
			_isCurrentRootUnitChanged = false;
		}


		/// <summary>
		/// [Please do not use it] 
		/// </summary>
		/// <returns></returns>
		public apOptMainCamera GetMainCamera()
		{
			return _mainCamera;
		}

		//v1.5.0 추가
		//: 실시간으로 카메라를 자동으로 감지하는 기능을 사용하고 있을 때,
		//현재 카메라를 주로 감지할 지, 항상 모든 카메라를 감지할지 결정하는 옵션
		/// <summary>
		/// When using the automatic camera detection function, set whether to mainly check the current camera or always check all cameras.
		/// </summary>
		/// <param name="cameraCheckMode">How to check cameras</param>
		public void SetCameraCheckMethod(CAMERA_CHECK_MODE cameraCheckMode)
		{
			_cameraCheckMode = cameraCheckMode;
		}
	


		//카메라를 직접 지정하는 함수
		/// <summary>
		/// Reset to automatically detect cameras in the scene.
		/// </summary>
		public void FindRenderingCamerasAutomatically()
		{
			if(_mainCamera == null)
			{
				Debug.LogError("AnyPortrait - The camera module has not been initialized yet.");
				return;
			}

			if (_mainCamera.SetRefreshAutomatically())
			{
				//기존에는 자동이 아니었는데 이번에 자동으로 갱신되도록 변경되었다.
				//>> 강제로 갱신
				bool isCameraChanged = _mainCamera.Refresh(true, _billboardType != BILLBOARD_TYPE.None, _cameraCheckMode);
				if(isCameraChanged)
				{
					ResetMeshesCommandBuffers(false);
				}
			}
		}

		/// <summary>
		/// Manually set the cameras that render this character.
		/// When this function is called, the character does not automatically recognize the change even if the cameras in the scene change.
		/// Call the "FindRenderingCamerasAutomatically()" function to make the character automatically recognize the cameras again.
		/// </summary>
		/// <param name="cameras">Cameras placed in the scene you want to set</param>
		/// <returns>The number of cameras that can actually render the character. Returns -1 if an error occurs</returns>
		public int SetRenderingCameras(params Camera[] cameras)
		{
			if(_mainCamera == null)
			{
				Debug.LogError("AnyPortrait - The camera module has not been initialized yet.");
				return -1;
			}

			int result = _mainCamera.SetCameras(cameras);

			//추가 v1.5.0 : 이 함수를 호출하면 클리핑 마스크를 위한 커맨드 버퍼를 같이 갱신해야 한다.
			//(사용자에게 맡기지만 말자)
			ResetMeshesCommandBuffers(false);

			return result;
		}
		


		//추가 20.9.15 : 지글본의 좌표계 변환 처리를 위한 특별 함수
		//이 함수의 내용은 지글본 코드(apOptBone)를 확인하자
		/// <summary>
		/// [Please do not use it] 
		/// </summary>
		public Vector3 OffsetPos2World_Prev(Vector3 posOffset)
		{
			//Debug.Log("Offset Pos Check : Cur : " + _transform.position.x + " / Prev : " + _posW_Prev1F.x);
			return _rotationOnlyMatrixIfBillboard.MultiplyPoint3x4(posOffset) + _posW_Prev1F;
		}

		/// <summary>
		/// [Please do not use it] 
		/// </summary>
		public Vector3 WorldPos2OffsetPos(Vector3 worldPos)
		{
			return _invRotationOnlyMatrixIfBillboard.MultiplyPoint3x4(worldPos - _transform.position);
		}


		// ID로 오브젝트 참조
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apTextureData GetTexture(int uniqueID)
		{
			//이전 (GC 발생)
			//return _textureData.Find(delegate (apTextureData a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetTexture_ID = uniqueID;
			return _textureData.Find(s_GetTextureByID_Func);
		}

		private static int s_GetTexture_ID = -1;
		private static Predicate<apTextureData> s_GetTextureByID_Func = FUNC_GetTextureByID;
		private static bool FUNC_GetTextureByID(apTextureData a)
		{
			return a._uniqueID == s_GetTexture_ID;
		}

		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apMesh GetMesh(int uniqueID)
		{
			//이전 (GC 발생)
			//return _meshes.Find(delegate (apMesh a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetMesh_ID = uniqueID;
			return _meshes.Find(s_GetMeshByID_Func);
		}

		private static int s_GetMesh_ID = -1;
		private static Predicate<apMesh> s_GetMeshByID_Func = FUNC_GetMeshByID;
		private static bool FUNC_GetMeshByID(apMesh a)
		{
			return a._uniqueID == s_GetMesh_ID;
		}


		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apMeshGroup GetMeshGroup(int uniqueID)
		{
			//이전 (GC 발생)
			//return _meshGroups.Find(delegate (apMeshGroup a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetMeshGroup_ID = uniqueID;
			return _meshGroups.Find(s_GetMeshGroupByID_Func);
		}

		private static int s_GetMeshGroup_ID = -1;
		private static Predicate<apMeshGroup> s_GetMeshGroupByID_Func = FUNC_GetMeshGroupByID;
		private static bool FUNC_GetMeshGroupByID(apMeshGroup a)
		{
			return a._uniqueID == s_GetMeshGroup_ID;
		}

		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apControlParam GetControlParam(int uniqueID)
		{
			//이전 (GC 발생)
			//return _controller._controlParams.Find(delegate (apControlParam a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetControlParam_ID = uniqueID;
			return _controller._controlParams.Find(s_GetControlParamByID_Func);
		}

		private static int s_GetControlParam_ID = -1;
		private static Predicate<apControlParam> s_GetControlParamByID_Func = FUNC_GetControlParamByID;
		private static bool FUNC_GetControlParamByID(apControlParam a)
		{
			return a._uniqueID == s_GetControlParam_ID;
		}

		/// <summary>
		/// Get Control Parameter
		/// </summary>
		/// <param name="controlParamName">Control Parameter Name</param>
		/// <returns></returns>
		public apControlParam GetControlParam(string controlParamName)
		{
			//이전 (GC 발생)
			//return _controller._controlParams.Find(delegate (apControlParam a)
			//{
			//	return string.Equals(a._keyName, controlParamName);
			//});

			//변경 v1.5.0
			s_GetControlParam_Name = controlParamName;
			return _controller._controlParams.Find(s_GetControlParamByName_Func);
		}

		private static string s_GetControlParam_Name = null;
		private static Predicate<apControlParam> s_GetControlParamByName_Func = FUNC_GetControlParamByName;
		private static bool FUNC_GetControlParamByName(apControlParam a)
		{
			return string.Equals(a._keyName, s_GetControlParam_Name);
		}




		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public apAnimClip GetAnimClip(int uniqueID)
		{
			//이전 (GC 발생)
			//return _animClips.Find(delegate (apAnimClip a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetAnimClip_ID = uniqueID;
			return _animClips.Find(s_GetAnimClipByID);
		}

		private static int s_GetAnimClip_ID = -1;
		private static Predicate<apAnimClip> s_GetAnimClipByID = FUNC_GetAnimClipByID;
		private static bool FUNC_GetAnimClipByID(apAnimClip a)
		{
			return a._uniqueID == s_GetAnimClip_ID;
		}


		// ID로 오브젝트 참조 - RealTime
		//-------------------------------------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="transformID"></param>
		/// <returns></returns>
		public apOptTransform GetOptTransform(int transformID)
		{
			if (transformID < -1)
			{
				return null;
			}

			if (_optTransforms == null)
			{
				return null;
			}
			//이전 (GC 발생)
			//return _optTransforms.Find(delegate (apOptTransform a)
			//{
			//	return a._transformID == transformID;
			//});

			//변경 v1.5.0
			s_GetOptTransform_ID = transformID;
			return _optTransforms.Find(s_GetOptTransformByID_Func);
		}

		private static int s_GetOptTransform_ID = -1;
		private static Predicate<apOptTransform> s_GetOptTransformByID_Func = FUNC_GetOptTransformByID;
		private static bool FUNC_GetOptTransformByID(apOptTransform a)
		{
			return a._transformID == s_GetOptTransform_ID;
		}



		/// <summary>
		/// [Please do not use it] (For Editor, not Runtime)
		/// </summary>
		/// <param name="meshGroupUniqueID"></param>
		/// <returns></returns>
		public apOptTransform GetOptTransformAsMeshGroup(int meshGroupUniqueID)
		{
			//Debug.Log("GetOptTransformAsMeshGroup [" + meshGroupUniqueID + "]");
			if (meshGroupUniqueID < 0)
			{
				//Debug.LogError("ID < 0");
				return null;
			}
			if (_optTransforms == null)
			{
				//Debug.LogError("OptTranforms is Null");
				return null;
			}

			//이전 (GC 발생)
			//return _optTransforms.Find(delegate (apOptTransform a)
			//{
			//	return a._meshGroupUniqueID == meshGroupUniqueID;
			//});

			//변경 v1.5.0
			s_GetOptTransformAsMeshGroup_ID = meshGroupUniqueID;
			return _optTransforms.Find(s_GetOptTransformAsMeshGroup_Func);
		}

		private static int s_GetOptTransformAsMeshGroup_ID = -1;
		private static Predicate<apOptTransform> s_GetOptTransformAsMeshGroup_Func = FUNC_GetOptTransformAsMeshGroup;
		private static bool FUNC_GetOptTransformAsMeshGroup(apOptTransform a)
		{
			return a._meshGroupUniqueID == s_GetOptTransformAsMeshGroup_ID;
		}

		/// <summary>
		/// Get Root Unit with Index
		/// </summary>
		/// <param name="rootUnitIndex"></param>
		/// <returns></returns>
		public apOptRootUnit GetOptRootUnit(int rootUnitIndex)
		{
			if(_optRootUnitList.Count == 0)
			{
				return null;
			}
			if(rootUnitIndex < 0 || rootUnitIndex >= _optRootUnitList.Count)
			{
				return null;
			}
			return _optRootUnitList[rootUnitIndex];
		}

		


		//추가 19.6.3 : MaterialSet에 관련
		public apMaterialSet GetMaterialSet(int uniqueID)
		{
			//이전 (GC 발생)
			//return _materialSets.Find(delegate(apMaterialSet a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetMaterialSet_ID = uniqueID;
			return _materialSets.Find(s_GetMaterialSetByID_Func);
		}


		private static int s_GetMaterialSet_ID = -1;
		private static Predicate<apMaterialSet> s_GetMaterialSetByID_Func = FUNC_GetMaterialSetByID;
		private static bool FUNC_GetMaterialSetByID(apMaterialSet a)
		{
			return a._uniqueID == s_GetMaterialSet_ID;
		}


		public apMaterialSet GetDefaultMaterialSet()
		{
			//이전 (GC 발생)
			//return _materialSets.Find(delegate (apMaterialSet a)
			//{
			//	return a._isDefault;
			//});

			//변경 v1.5.0
			return _materialSets.Find(s_GetDefaultMaterialSet);
		}

		private static Predicate<apMaterialSet> s_GetDefaultMaterialSet = FUNC_GetDefaultMaterialSet;
		private static bool FUNC_GetDefaultMaterialSet(apMaterialSet a)
		{
			return a._isDefault;
		}



		// 다른 포트레이트와 동기화 (21.6.7)
		//--------------------------------------------------------------------------
		/// <summary>
		/// If it is synchronized with other apPortraits, unsynchronize it.
		/// If this is a synchronized parent, all child objects are unsynchronized.
		/// If this is a synchronized child, exclude it from its parent.
		/// </summary>
		public void Unsynchronize()
		{
			if(_isSyncParent)
			{
				if(_syncChildPortraits != null)
				{
					//자식들의 동기화를 모두 해제한다.
					apPortrait childPortrait = null;
					for (int i = 0; i < _syncChildPortraits.Count; i++)
					{	
						childPortrait = _syncChildPortraits[i];
						if(childPortrait == null || childPortrait == this)
						{
							continue;
						}
						childPortrait._isSyncChild = false;
						childPortrait._isSyncParent = false;
						childPortrait._syncChildPortraits = null;
						childPortrait._syncParentPortrait = null;
						childPortrait._syncPlay = null;
					}
				}
				_isSyncParent = false;
				_syncChildPortraits = null;
			}
			if(_isSyncChild)
			{
				if(_syncParentPortrait != null)
				{
					//부모로부터 동기화를 해제한다.
					if(_syncParentPortrait._syncChildPortraits != null
						&& _syncParentPortrait._syncChildPortraits.Contains(this))
					{
						_syncParentPortrait._syncChildPortraits.Remove(this);

						if(_syncParentPortrait._syncChildPortraits.Count == 0)
						{
							//부모 객체의 모든 동기화가 해제되었다.
							_syncParentPortrait._isSyncParent = false;
							_syncParentPortrait._syncChildPortraits = null;
							_syncParentPortrait._isSyncChild = false;
							_syncParentPortrait._syncParentPortrait = null;
							_syncParentPortrait._syncPlay = null;
						}
					}
				}
				
				_isSyncChild = false;
				_syncParentPortrait = null;
				
				//이전
				//_syncMethod = SYNC_METHOD.None;

				//변경
				_isSync_Animation = false;
				_isSync_ControlParam = false;
				_isSync_Bone = false;
				_isSync_RootUnit = false;

				if(_syncPlay != null)
				{
					_syncPlay.Unsynchronize();
				}
				_syncPlay = null;

				//중요 : 동기화가 해제 되면 일부 모디파이어의 코드가 다르게 동작해야한다.
				int nRootUnits = _optRootUnitList != null ? _optRootUnitList.Count : 0;
				if (nRootUnits > 0)
				{
					for (int i = 0; i < nRootUnits; i++)
					{
						_optRootUnitList[i]._rootOptTransform._modifierStack.DisableSync();
					}
				}
			}
		}

		/// <summary>
		/// Synchronizes the updated values of animation or control parameters with other apPortrait.
		/// If synchronization succeeds, this apPortrait is registered and updated as a child of the target's apPortrait.
		/// </summary>
		/// <param name="targetPortrait">Target parent apPortrait.</param>
		/// <param name="syncAnimation">Synchronize the playback state of animation clips of the same name.</param>
		/// <param name="syncControlParam">Synchronize the values of control parameters of the same name.</param>
		/// <returns>Returns True if synchronization is successful.</returns>
		public bool Synchronize(apPortrait targetPortrait, bool syncAnimation, bool syncControlParam)
		{
			return Synchronize(targetPortrait, syncAnimation, syncControlParam, 
								false, false, SYNC_BONE_OPTION.MatchFromRoot);
		}


		/// <summary>
		/// Synchronizes the updated values of animation, control parameters or root units with other apPortrait.
		/// If synchronization succeeds, this apPortrait is registered and updated as a child of the target's apPortrait.
		/// </summary>
		/// <param name="targetPortrait">Target parent apPortrait.</param>
		/// <param name="syncAnimation">Synchronize the playback state of animation clips of the same name.</param>
		/// <param name="syncControlParam">Synchronize the values of control parameters of the same name.</param>
		/// <param name="syncRootUnit">Synchronize the transition of Root Units.(If animation is synchronized, this value is forced to false.)</param>
		/// <returns>Returns True if synchronization is successful.</returns>
		public bool Synchronize(apPortrait targetPortrait, bool syncAnimation, bool syncControlParam, bool syncRootUnit)
		{
			return Synchronize(targetPortrait, syncAnimation, syncControlParam, 
								syncRootUnit, 
								false, SYNC_BONE_OPTION.MatchFromRoot);
		}

		/// <summary>
		/// Synchronizes the updated values of animation, control parameters, bones or root units with other apPortrait.
		/// If synchronization succeeds, this apPortrait is registered and updated as a child of the target's apPortrait.
		/// </summary>
		/// <param name="targetPortrait">Target parent apPortrait.</param>
		/// <param name="syncAnimation">Synchronize the playback state of animation clips of the same name.</param>
		/// <param name="syncControlParam">Synchronize the values of control parameters of the same name.</param>
		/// <param name="syncBones">Synchronize the movement of bones with the same name and structure.</param>
		/// <param name="syncBoneOption">Option to synchronize bones (this option is ignored if syncBones is false)</param>
		/// <param name="syncRootUnit">Synchronize the transition of Root Units.(If animation is synchronized, this value is forced to false.)</param>
		/// <returns>Returns True if synchronization is successful.</returns>
		//public bool Synchronize(apPortrait targetPortrait, bool syncAnimation, bool syncControlParam, bool syncBones, SYNC_BONE_OPTION syncBoneOption, bool syncRootUnit)
		public bool Synchronize(apPortrait targetPortrait, bool syncAnimation, bool syncControlParam, bool syncRootUnit, bool syncBones, SYNC_BONE_OPTION syncBoneOption)//인자 순서를 바꾼다.
		{
			if (targetPortrait == null || targetPortrait == this)
			{
				Debug.LogError("AnyPortrait : [Sync failed] Target is null");
				return false;
			}
			if(_isSyncParent)
			{
				Debug.LogError("AnyPortrait : [Sync failed] This apPortrait is a parent object that has already been synced.");
				return false;
			}

			if(targetPortrait._isSyncChild)
			{
				Debug.LogError("AnyPortrait : [Sync failed] The target is already synced to another apPortrait.");
				return false;
			}

			
			//잘못된 코드
			//if(syncAnimation && syncRootUnit)
			//{
			//	//만약 애니메이션 동기화를 하면 루트 유닛 동기화는 false가 된다.
			//	syncRootUnit = false;
			//}

			//반대. Animation에 RootUnit 동기화 기능이 없었다!
			//Animation을 켜면 RootUnit 동기화를 같이 켜야한다.
			
			if(syncAnimation)
			{
				syncRootUnit = true;
			}

			if(!syncAnimation && !syncControlParam && !syncBones && !syncRootUnit)
			{
				Debug.LogError("AnyPortrait : [Sync failed] This function does not work because all arguments are false. To unsynchronize, use the Unsynchronize() function instead.");
				return false;
			}

			//연동할게 없다면 요청 항목에서 삭제
			if (syncAnimation)
			{
				int nAnimClips = _animClips != null ? _animClips.Count : 0;
				int nTargetAnimClips = targetPortrait._animClips != null ? targetPortrait._animClips.Count : 0;

				if (nAnimClips == 0 || nTargetAnimClips == 0)
				{
					syncAnimation = false;
				}
			}

			if (syncControlParam)
			{
				if(_controller._controlParams == null || targetPortrait._controller._controlParams == null)
				{
					syncControlParam = false;
				}
			}
			
			if(syncRootUnit)
			{
				//루트 유닛이 모두 2개 이상이어야 하고, 개수가 같아야 한다.
				int nRootUnit_Target = targetPortrait._optRootUnitList != null ? targetPortrait._optRootUnitList.Count : 0;
				int nRootUnit_Self = _optRootUnitList != null ? _optRootUnitList.Count : 0;

				if(nRootUnit_Target <= 1 || nRootUnit_Self <= 1)
				{
					//루트 유닛이 1 이하라면 syncRootUnit 요청은 무시된다.
					
					if(!syncAnimation)
					{
						//단 경고문은 애니메이션 동기화가 아닌 경우에만 보여주자
						Debug.LogWarning("AnyPortrait : If there are 1 or fewer Root Units, the synchronization request to the Root Unit is ignored.");
					}
					
					syncRootUnit = false;
				}
				else if(nRootUnit_Target != nRootUnit_Self)
				{
					//루트 유닛의 개수가 다르다면 syncRootUnit 요청은 무시된다.
					
					Debug.LogError("AnyPortrait : Since the number of Root Units between the two Portraits is different, the synchronization request for switching Root Units is ignored.");
					syncRootUnit = false;
				}
			}


			if(!syncAnimation && !syncControlParam && !syncBones && !syncRootUnit)
			{
				return false;
			}

			

			if(_isSyncChild)
			{
				if(targetPortrait == _syncParentPortrait)
				{
					//이미 동기화가 되었다.
					//부모의 입장에서 이 객체가 등록되었는지 한번 더 확인하자
					_syncParentPortrait._isSyncParent = true;
					if(_syncParentPortrait._syncChildPortraits == null)
					{
						_syncParentPortrait._syncChildPortraits = new List<apPortrait>();
					}
					if(!_syncParentPortrait._syncChildPortraits.Contains(this))
					{
						_syncParentPortrait._syncChildPortraits.Add(this);
					}
					return true;
				}
			}

			//만약 새로 등록하는 거라면
			_isSyncChild = true;
			_syncParentPortrait = targetPortrait;

			
			//이전
			//if(syncAnimation && syncControlParam)
			//{
			//	_syncMethod = SYNC_METHOD.AnimationAndControlParam;
			//}
			//else if(syncAnimation && !syncControlParam)
			//{
			//	_syncMethod = SYNC_METHOD.AnimationOnly;
			//}
			//else if(!syncAnimation && syncControlParam)
			//{
			//	_syncMethod = SYNC_METHOD.ControlParamOnly;
			//}
			//else
			//{
			//	_syncMethod = SYNC_METHOD.None;
			//}

			//변경 21.9.18
			_isSync_Animation = syncAnimation;
			_isSync_ControlParam = syncControlParam;
			_isSync_Bone = syncBones;
			_isSync_RootUnit = syncRootUnit;



			//동기화용 객체 생성
			_syncPlay = new apSyncPlay(this, _syncParentPortrait, syncAnimation, syncControlParam, syncBones, syncBoneOption, syncRootUnit);

			_syncParentPortrait._isSyncParent = true;
			if(_syncParentPortrait._syncChildPortraits == null)
			{
				_syncParentPortrait._syncChildPortraits = new List<apPortrait>();
			}
			if(!_syncParentPortrait._syncChildPortraits.Contains(this))
			{
				_syncParentPortrait._syncChildPortraits.Add(this);
			}


			//중요 : 동기화가 되면 일부 모디파이어의 코드가 다르게 동작해야한다.
			int nRootUnits = _optRootUnitList != null ? _optRootUnitList.Count : 0;
			if (nRootUnits > 0)
			{
				for (int i = 0; i < nRootUnits; i++)
				{
					//모디파이어 스택에서 Sync를 활성화
					//- 리깅 LUT가 Sync용으로 변경된다.
					//- 이 코드는 SyncPlay를 생성한 후에 실행되어야 한다.
					_optRootUnitList[i]._rootOptTransform._modifierStack.EnableSync();
				}
			}
			

			return true;
		}

		

		// 추가 21.10.7 : 업데이트 시간 배속 설정
		/// <summary>
		/// Decide which the interval value of time will be used when updating the apPortrait.
		/// If the Important option is disabled or Mecanim (Animator) is used, this option does not apply except for some features.
		/// </summary>
		/// <param name="useUnscaleDeltaTime">If true, Time.unscaledDeltaTime is used, if false, Time.deltaTime is used. (Default is false)</param>
		public void SetUpdateTimeMethod(bool useUnscaleDeltaTime)
		{
			_isDeltaTimeOptionChanged = true;

			if(useUnscaleDeltaTime)
			{
				_deltaTimeOption = DELTA_TIME_OPTION.UnscaledDeltaTime;
			}
			else
			{
				_deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;
			}
			_deltaTimeMultiplier = 1.0f;
			_funcDeltaTimeRequested = null;
			_deltaTimeRequestSavedObject = null;
		}

		/// <summary>
		/// Decide which the interval value of time will be used when updating the apPortrait.
		/// If the Important option is disabled or Mecanim (Animator) is used, this option does not apply except for some features.
		/// </summary>
		/// <param name="useUnscaleDeltaTime">If true, Time.unscaledDeltaTime is used, if false, Time.deltaTime is used. (Default is false)</param>
		/// <param name="multiplier">The multiplier that accelerates the update. (Default is 1.0f)</param>
		public void SetUpdateTimeMethod(bool useUnscaleDeltaTime, float multiplier)
		{
			_isDeltaTimeOptionChanged = true;

			if(useUnscaleDeltaTime)
			{
				_deltaTimeOption = DELTA_TIME_OPTION.MultipliedUnscaledDeltaTime;
			}
			else
			{
				_deltaTimeOption = DELTA_TIME_OPTION.MultipliedDeltaTime;
			}
			_deltaTimeMultiplier = multiplier;
			_funcDeltaTimeRequested = null;
			_deltaTimeRequestSavedObject = null;
		}

		//추가 1.3.5 22.1.8 : 콜백으로 업데이트 시간 제어
		/// <summary>
		/// Decide which the interval value of time will be used when updating the apPortrait.
		/// You can control the update rate of multiple characters at once by using a callback function that return the Delta Time.
		/// If the Important option is disabled or Mecanim (Animator) is used, this option does not apply except for some features.
		/// </summary>
		/// <param name="onDeltaTimeRequested">A callback function that returns the Delta Time (float OnDeltaTimeRequested(object))</param>
		/// <param name="savedObject">A key object for distinguishing characters. It is passed as an argument when calling the callback function. (nullable)</param>
		public void SetUpdateTimeMethod(OnDeltaTimeRequested onDeltaTimeRequested, object savedObject = null)
		{
			_isDeltaTimeOptionChanged = true;

			if(onDeltaTimeRequested == null)
			{
				Debug.Log("AnyPortrait : onDeltaTimeRequested is null, so it will be restored to the default option.");
				_deltaTimeOption = DELTA_TIME_OPTION.DeltaTime;
				_deltaTimeMultiplier = 1.0f;
				_funcDeltaTimeRequested = null;
				_deltaTimeRequestSavedObject = null;
				return;
			}

			_deltaTimeOption = DELTA_TIME_OPTION.CustomFunction;
			_deltaTimeMultiplier = 1.0f;
			_funcDeltaTimeRequested = onDeltaTimeRequested;
			_deltaTimeRequestSavedObject = savedObject;
		}

		


		// 추가 21.12.22 : 재질 합치기
		//모든 함수들은 부모에게 가서 시도해야한다.
		//Merge된 재질들을 모두 해제한다.
		/// <summary>
		/// Release all merged materials. You can also call it from apPortrait rather than Main.
		/// </summary>
		public void UnmergeMaterials()
		{
			//메인 오브젝트가 있다면 (자신 포함)
			//> 해당 메인 오브젝트가 일괄적으로 모든 연결된 Portrait에 대해서 UnmergeProcess를 호출한다.
			//> UnmergeProcess를 수행하면 연결이 종료된다.
			//메인 오브젝트가 없다면 (연결이 뭔가 해제됨)
			//> 그냥 스스로 실행한다.
			if(!_isUseMergedMat)
			{	
				return;
			}

			if(_mergeMatMainPortrait != null && _mergeMatMainPortrait != this)
			{
				//다른 Main Portrait가 있었다면 Main에게 시키자
				_mergeMatMainPortrait.UnmergeMaterialsAsMain();
			}
			else
			{
				//그렇지 않다면 자신이 Main으로서 해제를 한다.
				UnmergeMaterialsAsMain();
			}
		}

		/// <summary>Main으로서 재질 병합을 해제한다.</summary>
		private void UnmergeMaterialsAsMain()
		{
			if(_optMergedMaterial != null)
			{
				_optMergedMaterial.OnDestroy();
			}

			//연결된 모든 Portrait에 Unmerged를 호출하자
			int nSubPortraits = _mergedMatSubPortraits != null ? _mergedMatSubPortraits.Count : 0;
			if(nSubPortraits > 0)
			{
				apPortrait curSubPortrait = null;
				for (int i = 0; i < nSubPortraits; i++)
				{
					curSubPortrait = _mergedMatSubPortraits[i];
					if(curSubPortrait == null || curSubPortrait == this)
					{
						continue;
					}

					curSubPortrait.UnmergeMaterialsProcess();
				}
			}

			//마지막으로 자기 자신의 Unmerged를 수행한다.
			UnmergeMaterialsProcess();
		}


		/// <summary>재질 병합을 초기화한다.</summary>
		private void UnmergeMaterialsProcess()
		{
			//모든 OptMesh의 재질이나 VertexID 등을 Merge 전으로 초기화한다.
			int nOptMeshes = _optMeshes != null ? _optMeshes.Count : 0;
			if(nOptMeshes > 0)
			{
				apOptMesh curOptMesh = null;
				for (int i = 0; i < nOptMeshes; i++)
				{
					curOptMesh = _optMeshes[i];
					
					//각 Mesh의 Merged 정보를 삭제한다.
					curOptMesh.ReleaseMergedMaterial();
				}
			}

			//끝난 후 연결을 해제하자
			_isUseMergedMat = false;
			_mergeMatMainPortrait = null;
			_mergedMatSubPortraits = null;
			_optMergedMaterial = null;
		}




		/// <summary>
		/// Merge materials with adding other apPortraits.
		/// This function merges its own materials together, even if there are no other apPortraits.
		/// </summary>
		/// <param name="otherPortraits">It can also be merged with materials from other apPortraits. If you only want to merge this character's internal materials, you don't need to input anything.</param>
		public void MergeMaterials(params apPortrait[] otherPortraits)
		{	
			//재질을 모두 병합한다.
			//Portrait들이 추가되지 않았어도 된다. (자기 자신만 병합되어도 됨)
			ResetMeshMaterialToBatchAll();//모든 외부에 의한 재질 변화를 초기화한다.
			UnmergeMaterials();

			
			int nOtherPortraits = otherPortraits != null ? otherPortraits.Length : 0;
			apPortrait curOtherPortrait = null;
			if (nOtherPortraits > 0)
			{
				//추가에 앞서서
				//다른 Portrait도 Unmerge를 수행한다.
				for (int i = 0; i < nOtherPortraits; i++)
				{
					curOtherPortrait = otherPortraits[i];

					if (curOtherPortrait == null || curOtherPortrait == this)
					{
						continue;
					}

					//중요 : 일단 모든 외부에 의한 재질 변화를 초기화할 것
					curOtherPortrait.ResetMeshMaterialToBatchAll();

					if (curOtherPortrait._isUseMergedMat)
					{
						//이미 재질 병합에 포함되었다면
						curOtherPortrait.UnmergeMaterials();
						
					}
				}
			}

			//병합 대상인 Portrait 리스트를 초기화한다.
			_isUseMergedMat = true;
			_mergeMatMainPortrait = this;
			if(_mergedMatSubPortraits == null)
			{
				_mergedMatSubPortraits = new List<apPortrait>();
			}
			_mergedMatSubPortraits.Clear();

			//기존 정보가 있다면 일단 상테
			if(_optMergedMaterial != null)
			{
				_optMergedMaterial.OnDestroy();
			}
			_optMergedMaterial = new apOptMergedMaterial();

			//병합할 Portrait 리스트를 만들자
			List<apPortrait> targetPortraits = new List<apPortrait>();
			targetPortraits.Add(this);//일단 자기 자신 포함
			
			
				
			if (nOtherPortraits > 0)
			{

				for (int i = 0; i < nOtherPortraits; i++)
				{
					curOtherPortrait = otherPortraits[i];

					if(curOtherPortrait == null || curOtherPortrait == this)
					{
						continue;
					}

					//이미 포함되어 있다면
					if(_mergedMatSubPortraits.Contains(curOtherPortrait))
					{
						continue;
					}

					//리스트에 추가한다.
					_mergedMatSubPortraits.Add(curOtherPortrait);
					targetPortraits.Add(curOtherPortrait);

					//Sub가 되는 Other Portrait에도 Merged 여부와 값을 연결하자
					curOtherPortrait._isUseMergedMat = true;
					curOtherPortrait._mergedMatSubPortraits = null;
					curOtherPortrait._mergeMatMainPortrait = this;
					curOtherPortrait._optMergedMaterial = _optMergedMaterial;//병합된 재질 데이터를 공유한다.
				}
			}


			_isUseMergedMat = true;
			_mergeMatMainPortrait = this;

			//병합 재질을 완성한다.
			_optMergedMaterial.MakeMergeMaterials(targetPortraits);

		}
	}

}