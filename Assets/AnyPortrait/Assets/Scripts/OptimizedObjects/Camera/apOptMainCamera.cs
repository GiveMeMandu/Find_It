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

using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 빌보드를 위해서 apPortrait가 가지는 "자신을 바라보는 카메라" 정보
	/// 멀티 카메라인 경우, 카메라들의 중심을 기준으로 가상의 처리가 가능하다.
	/// </summary>
	public class apOptMainCamera
	{
		// Members
		//-------------------------------------------------------
		public enum NumberOfCamera
		{
			None,
			Single,
			Multiple
		}
		private NumberOfCamera _numberOfCameraType = NumberOfCamera.None;

		//씬의 카메라 데이터를 모두 저장하자.
		public class CameraData
		{
			public Camera _camera = null;
			public Transform _transform = null;


			public CameraData(Camera camera)
			{
				_camera = camera;
				_transform = _camera.transform;
			}
		}

		private List<CameraData> _cameraDataList = new List<CameraData>();
		private Dictionary<Camera, CameraData> _cam2CameraData = new Dictionary<Camera, CameraData>();

		private List<CameraData> _renderCameraDataList = new List<CameraData>();

		//한번 Refresh하면 다른 OptMesh들이 Refresh를 해야할지 검토할 수 있도록 Refresh Key를 만들고 비교하자.
		public int _refreshKey = -1;


		private CameraData _mainData = null;

		private int _nCameras = 0;
		private int _nRenderCameras = 0;

		//LR 카메라
		private enum DualCameraType
		{
			None,
			LeftRight,
			Unknown
		}
		private DualCameraType _dualCameraType = DualCameraType.None;
		private CameraData _leftEyeData = null;
		private CameraData _rightEyeData = null;

		private apPortrait _parentPortrait = null;
		

		//리턴하는 값
		private Matrix4x4 _worldToCameraMatrix = Matrix4x4.identity;
		private float _zDepthToCam = 0.0f;
		private Quaternion _centerRotation = Quaternion.identity;
		private Vector3 _centerForward = Vector3.forward;
		private Vector3 _centerPos = Vector3.zero;

		//렌더 카메라를 기준으로 Forward와 Orthographic의 공통값을 체크
		private bool _isAllSameForward = false;
		private bool _isAllOrthographic = false;

		public enum PersOrthoType
		{
			Unknown,
			Perspective,
			Orthographic,
			Mixed
		}
		private PersOrthoType _persOrthoType = PersOrthoType.Unknown;

		private apPortrait.VR_SUPPORT_MODE _vrSupportMode = apPortrait.VR_SUPPORT_MODE.None;
#if UNITY_2019_1_OR_NEWER
		private apPortrait.VR_RT_SIZE _vrRTSize = apPortrait.VR_RT_SIZE.ByMeshSettings;
#endif

		//자동으로 카메라를 찾고 갱신할지, 수동으로 지정할지 확인
		private bool _isAutomaticRefresh = true;

		//계산용 변수
		private CameraData _cal_curCamData = null;
		private Camera _cal_curSceneCamera = null;
		private Vector3 _cal_Forward = Vector3.zero;




		// Init
		//-------------------------------------------------------
		public apOptMainCamera(apPortrait portrait)
		{
			_parentPortrait = portrait;

			Clear();

			_refreshKey = -1;
			_isAutomaticRefresh = true;

			_vrSupportMode = _parentPortrait._vrSupportMode;
#if UNITY_2019_1_OR_NEWER
			_vrRTSize = _parentPortrait._vrRenderTextureSize;
#endif
		}

		public void Clear()
		{
			_numberOfCameraType = NumberOfCamera.None;

			if (_cameraDataList == null)
			{
				_cameraDataList = new List<CameraData>();
			}
			if (_cam2CameraData == null)
			{
				_cam2CameraData = new Dictionary<Camera, CameraData>();
			}
			if (_renderCameraDataList == null)
			{
				_renderCameraDataList = new List<CameraData>();
			}
			_cameraDataList.Clear();
			_cam2CameraData.Clear();
			_renderCameraDataList.Clear();
			_nCameras = 0;
			_nRenderCameras = 0;

			_mainData = null;

			_dualCameraType = DualCameraType.None;
			_leftEyeData = null;
			_rightEyeData = null;
			
			_worldToCameraMatrix = Matrix4x4.identity;
			_zDepthToCam = 0.0f;

			_centerRotation = Quaternion.identity;
			_centerForward = Vector3.forward;
			_centerPos = Vector3.zero;

			_persOrthoType = PersOrthoType.Unknown;

			_isAllSameForward = false;
			_isAllOrthographic = false;
		}


		// Functions
		//-------------------------------------------------------
		public bool SetRefreshAutomatically()
		{
			bool isResult = (_isAutomaticRefresh == false);
			_isAutomaticRefresh = true;

			return isResult;
		}

		public int SetCameras(params Camera[] cameras)
		{
			if(cameras == null)
			{
				return 0;
			}
			if(cameras.Length == 0)
			{
				return 0;
			}

			//실제 카메라가 지정되면 자동 갱신이 취소된다.
			_isAutomaticRefresh = false;

			//다시 Refresh를 해야한다.
			if (_numberOfCameraType != NumberOfCamera.None)
			{
				//일단 초기화
				Clear();
			}

			int nParamCameras = cameras.Length;
			_cal_curSceneCamera = null;

			for (int i = 0; i < nParamCameras; i++)
			{
				_cal_curSceneCamera = cameras[i];
				if (_cal_curSceneCamera != null && IsLookPortrait(_cal_curSceneCamera))
				{
					//바라보고 있는 카메라다. > 일단 모두 추가
					CameraData newCamData = new CameraData(_cal_curSceneCamera);
					_cameraDataList.Add(newCamData);
					_cam2CameraData.Add(_cal_curSceneCamera, newCamData);

					//이전
					//if (_parentPortrait._isForceCamSortModeToOrthographic)
					//수정 21.1.15 : Billboard가 아닌 경우에도 Orthographic으로 고정되는 문제. 빌보드일때만 고정하자
					if (_parentPortrait._isForceCamSortModeToOrthographic
						&& _parentPortrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
					{
						//강제로 Orthographic으로 고정한다.
						_cal_curSceneCamera.transparencySortMode = TransparencySortMode.Orthographic;
					}
				}
			}

			_nCameras = _cameraDataList.Count;

			if (_nCameras == 0)
			{
				//실제 유효한 카메라가 없다.
				_numberOfCameraType = NumberOfCamera.None;
				_persOrthoType = PersOrthoType.Unknown;

				//Debug.LogError("> 카메라 없음");
			}
			else
			{
				if (_nCameras == 1 || _vrSupportMode != apPortrait.VR_SUPPORT_MODE.MultiCamera)
				{
					//싱글 카메라 (카메라가 한개이거나 [멀티 카메라 VR]을 지원하지 않는 경우)
					_numberOfCameraType = NumberOfCamera.Single;

					_mainData = _cameraDataList[0];

					_persOrthoType = (_mainData._camera.orthographic ? PersOrthoType.Orthographic : PersOrthoType.Perspective);

					_renderCameraDataList.Add(_mainData);//렌더 카메라는 1개
					_nRenderCameras = 1;
				}
				else
				{
					//멀티 카메라
					_numberOfCameraType = NumberOfCamera.Multiple;

					//메인 카메라를 찾자
					//카메라의 속성을 먼저 보자

					_cal_curCamData = null;

					//두바퀴 돌아서 체크
					//일단 Left, Right 타입을 체크한다.

					for (int i = 0; i < _nCameras; i++)
					{
						_cal_curCamData = _cameraDataList[i];
						_renderCameraDataList.Add(_cal_curCamData);//<<렌더 카메라로 추가한다.

						switch (_cal_curCamData._camera.stereoTargetEye)
						{
							case StereoTargetEyeMask.Left:
								{
									if (_leftEyeData == null)
									{
										_leftEyeData = _cal_curCamData;
									}
								}
								break;
							case StereoTargetEyeMask.Right:
								{
									if (_rightEyeData == null)
									{
										_rightEyeData = _cal_curCamData;
									}
								}
								break;
						}
					}

					_nRenderCameras = _renderCameraDataList.Count;

					//두번째 체크. 만약 LR 카메라가 하나라도 발견되지 않았다면, 이름이라도 확인하자.
					if (_leftEyeData == null || _rightEyeData == null)
					{
						_cal_curCamData = null;
						for (int i = 0; i < _nCameras; i++)
						{
							_cal_curCamData = _cameraDataList[i];

							string camName = _cal_curCamData._transform.gameObject.name.ToLower();

							if (camName.Contains("left"))
							{
								//이름에 Left가 들어갔다면
								if (_leftEyeData == null)
								{
									_leftEyeData = _cal_curCamData;
								}
							}
							else if (camName.Contains("right"))
							{
								//이름에 Right가 들어갔다면
								if (_rightEyeData == null)
								{
									_rightEyeData = _cal_curCamData;
								}
							}

							if (_leftEyeData != null && _rightEyeData != null)
							{
								break;
							}
						}
					}

					//LR 타입 체크
					if (_leftEyeData != null && _rightEyeData != null)
					{
						//LR 둘다 있다.
						_dualCameraType = DualCameraType.LeftRight;
						_mainData = _leftEyeData;

						if (_leftEyeData._camera.orthographic && _rightEyeData._camera.orthographic)
						{
							_persOrthoType = PersOrthoType.Orthographic;
						}
						else if (!_leftEyeData._camera.orthographic && !_rightEyeData._camera.orthographic)
						{
							_persOrthoType = PersOrthoType.Perspective;
						}
						else
						{
							_persOrthoType = PersOrthoType.Mixed;
						}

						//Debug.LogWarning("> LR 두개의 카메라");
					}
					else
					{
						//LR 둘중 하나라도 없다.
						_dualCameraType = DualCameraType.Unknown;
						_mainData = _cameraDataList[0];

						_persOrthoType = (_mainData._camera.orthographic ? PersOrthoType.Orthographic : PersOrthoType.Perspective);

						//Debug.LogWarning("> 여러개의 카메라");
					}
				}
			}

			//RefreshKey도 교체하자
			if (_refreshKey < 0)
			{
				_refreshKey = 10;
			}
			else
			{
				_refreshKey++;
				if (_refreshKey > 9999) { _refreshKey = 10; }
			}

			RefreshMatrix();

			return _nCameras;
		}



		//v1.5.0 변경
		//- 카메라를 감지하는 방식 인자 추가
		//- 카메라의 변경이 발생하면 true 리턴 (커맨드버퍼 갱신 위함)
		public bool Refresh(	bool isRefreshForce,//강제로 갱신할 것인지 여부
								bool isRefreshMatrix,
								apPortrait.CAMERA_CHECK_MODE checkCameraMode)//v1.5.0 : 유효성 테스트를 할 때 씬의 모든 카메라와 비교를 한다.
		{
			//그대로 유지할지 확인
			//- 카메라의 개수
			//- 현재 카메라가 유효한지

			//자동 갱신 옵션이 꺼진 경우
			if (!_isAutomaticRefresh)
			{
				if(isRefreshMatrix)
				{
					RefreshMatrix();
				}
				else
				{
					RefreshForwardAndOrthographic();//v1.5.0 : 카메라의 Forward/Orthographic만 체크한다.
				}
				return false;
			}

			//씬의 카메라를 계속 참조하는건 옳지 않다.
			//현재 카메라가 설정되었다면 외부에 새로운 카메라가 생겼다 할지라도 갱신하지는 않는다.

			bool isNeedToRefresh = false;

			//카메라 개수도 여기에 포함된다.
			int nSceneCameras = Camera.allCamerasCount;

			//강제로 변경하는 것이 아니라면 변경이 발생했는지 체크하자
			if(!isRefreshForce)
			{
				//초기화 여부 체크
				if (_numberOfCameraType == NumberOfCamera.None
					|| _mainData == null
					//|| _cameraDataList.Count == 0
					|| _nCameras == 0
					|| _refreshKey < 0)
				{
					//초기화 된 상태이거나 씬의 카메라 개수가 다르다면
					isNeedToRefresh = true;
				}

				//v1.5.0
				if(!isNeedToRefresh
					&& nSceneCameras == 0)
				{
					//만약 카메라 개수가 0개인데 데이터가 있다면
					isNeedToRefresh = true;
				}


				if(!isNeedToRefresh)
				{
					//존재하는 카메라가 유효하지 않는지 체크
					if (_numberOfCameraType == NumberOfCamera.Single)
					{
						if (!IsLookPortrait(_mainData._camera))
						{
							//Single 카메라가 제대로 바라보고 있지 않다면
							isNeedToRefresh = true;
						}
					}
					else if (_numberOfCameraType == NumberOfCamera.Multiple)
					{
						_cal_curCamData = null;
						for (int i = 0; i < _cameraDataList.Count; i++)
						{
							_cal_curCamData = _cameraDataList[i];

							//하나씩 테스트하여 null이거나 제대로 바라보고 있지 않다면
							if (_cal_curCamData == null || !IsLookPortrait(_cal_curCamData._camera))
							{
								isNeedToRefresh = true;
								break;
							}
						}
					}
				}

				//v1.5.0 추가 : 옵션에 따라선 Scene 카메라들을 지속적으로 검사한다. (보통 비활성화)
				if(!isNeedToRefresh
					&& checkCameraMode == apPortrait.CAMERA_CHECK_MODE.AllSceneCameras)
				{
					//모든 카메라를 상시 체크 (개수로 체크한다)
					int nValidSceneCameras = 0;
					if (nSceneCameras > 0)
					{
						Camera[] sceneCameras = Camera.allCameras;

						for (int i = 0; i < nSceneCameras; i++)
						{
							_cal_curSceneCamera = sceneCameras[i];
							if (_cal_curSceneCamera != null && IsLookPortrait(_cal_curSceneCamera))
							{
								nValidSceneCameras += 1;								
							}
						}
					}

					if(nValidSceneCameras != _nCameras)
					{
						//Debug.Log("유효한 카메라 개수가 변경됨 (자동 감지) : " + _nCameras + " > " + nValidSceneCameras);
						isNeedToRefresh = true;
					}

				}
			}

			if (!isNeedToRefresh)
			{
				//제대로 동작 중이다.
				//Matrix만 갱신하고 끝
				if(isRefreshMatrix)
				{
					RefreshMatrix();
				}
				else
				{
					RefreshForwardAndOrthographic();//v1.5.0 : 카메라의 Forward/Orthographic만 체크한다.
				}
				
				return false;//카메라가 변경되지 않았다.
			}

			//기존에 설정된 카메라가 아직 유효하다.
			if (nSceneCameras == 0)
			{
				//카메라가 없네요.
				if (_numberOfCameraType != NumberOfCamera.None)
				{
					Clear();
				}
				return true;//카메라는 없으며, 커맨드 버퍼를 삭제해야한다.
			}
			
			//다시 Refresh를 해야한다.
			if (_numberOfCameraType != NumberOfCamera.None)
			{
				//일단 초기화
				Clear();
			}

			//Debug.Log("새로운 카메라 탐색");


			//씬 카메라를 모두 갱신해주자. (일단 VR 모드 상관없이)
			if (nSceneCameras > 0)
			{
				Camera[] sceneCameras = Camera.allCameras;

				for (int i = 0; i < nSceneCameras; i++)
				{
					_cal_curSceneCamera = sceneCameras[i];
					if (_cal_curSceneCamera != null && IsLookPortrait(_cal_curSceneCamera))
					{
						//바라보고 있는 카메라다. > 일단 모두 추가
						CameraData newCamData = new CameraData(_cal_curSceneCamera);
						_cameraDataList.Add(newCamData);
						_cam2CameraData.Add(_cal_curSceneCamera, newCamData);

						//이전
						//if (_parentPortrait._isForceCamSortModeToOrthographic)

						//수정 21.1.15 : Billboard가 아닌 경우에도 Orthographic으로 고정되는 문제
						if (_parentPortrait._isForceCamSortModeToOrthographic
							&& _parentPortrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
						{
							//강제로 Orthographic으로 고정한다.
							_cal_curSceneCamera.transparencySortMode = TransparencySortMode.Orthographic;
						}
					}
				}

				_nCameras = _cameraDataList.Count;

				if (_nCameras == 0)
				{
					//실제 유효한 카메라가 없다.
					_numberOfCameraType = NumberOfCamera.None;
					_persOrthoType = PersOrthoType.Unknown;

					//Debug.LogError("> 카메라 없음");
				}
				else
				{
					if (_nCameras == 1 || _vrSupportMode != apPortrait.VR_SUPPORT_MODE.MultiCamera)
					{
						//싱글 카메라 (카메라가 한개이거나 [멀티 카메라 VR]을 지원하지 않는 경우)
						_numberOfCameraType = NumberOfCamera.Single;

						_mainData = _cameraDataList[0];

						_persOrthoType = (_mainData._camera.orthographic ? PersOrthoType.Orthographic : PersOrthoType.Perspective);

						//Debug.LogWarning("> 1개의 카메라");

						_renderCameraDataList.Add(_mainData);//렌더 카메라는 1개
						_nRenderCameras = 1;
					}
					else
					{
						//멀티 카메라
						_numberOfCameraType = NumberOfCamera.Multiple;

						//메인 카메라를 찾자
						//카메라의 속성을 먼저 보자

						//- 왼쪽 눈, 오른쪽 눈에 해당하는 카메라를 찾는다. > 존재하면 Center Rotation을 만들 수 있다.

						//1) LR 둘다 있을 때 > LR 타입 + L이 메인 + 중점에서 Matrix 계산
						//2) LR 하나라도 없을 때 > "알 수 없는" 타입 + 첫번째 카메라가 메인 + 전체 중점에서 Matrix 계산

						_cal_curCamData = null;

						//두바퀴 돌아서 체크
						//일단 Left, Right 타입을 체크한다.

						for (int i = 0; i < _nCameras; i++)
						{
							_cal_curCamData = _cameraDataList[i];
							_renderCameraDataList.Add(_cal_curCamData);//<<렌더 카메라로 추가한다.

							switch (_cal_curCamData._camera.stereoTargetEye)
							{
								case StereoTargetEyeMask.Left:
									{
										if (_leftEyeData == null)
										{
											_leftEyeData = _cal_curCamData;
										}
									}
									break;
								case StereoTargetEyeMask.Right:
									{
										if (_rightEyeData == null)
										{
											_rightEyeData = _cal_curCamData;
										}
									}
									break;
							}
						}

						_nRenderCameras = _renderCameraDataList.Count;

						//두번째 체크. 만약 LR 카메라가 하나라도 발견되지 않았다면, 이름이라도 확인하자.
						if (_leftEyeData == null || _rightEyeData == null)
						{
							_cal_curCamData = null;
							for (int i = 0; i < _nCameras; i++)
							{
								_cal_curCamData = _cameraDataList[i];

								string camName = _cal_curCamData._transform.gameObject.name.ToLower();

								if (camName.Contains("left"))
								{
									//이름에 Left가 들어갔다면
									if (_leftEyeData == null)
									{
										_leftEyeData = _cal_curCamData;
									}
								}
								else if (camName.Contains("right"))
								{
									//이름에 Right가 들어갔다면
									if (_rightEyeData == null)
									{
										_rightEyeData = _cal_curCamData;
									}
								}

								if (_leftEyeData != null && _rightEyeData != null)
								{
									break;
								}
							}
						}

						//LR 타입 체크
						if (_leftEyeData != null && _rightEyeData != null)
						{
							//LR 둘다 있다.
							_dualCameraType = DualCameraType.LeftRight;
							_mainData = _leftEyeData;

							if (_leftEyeData._camera.orthographic && _rightEyeData._camera.orthographic)
							{
								_persOrthoType = PersOrthoType.Orthographic;
							}
							else if (!_leftEyeData._camera.orthographic && !_rightEyeData._camera.orthographic)
							{
								_persOrthoType = PersOrthoType.Perspective;
							}
							else
							{
								_persOrthoType = PersOrthoType.Mixed;
							}

							//Debug.LogWarning("> LR 두개의 카메라");
						}
						else
						{
							//LR 둘중 하나라도 없다.
							_dualCameraType = DualCameraType.Unknown;
							_mainData = _cameraDataList[0];

							_persOrthoType = (_mainData._camera.orthographic ? PersOrthoType.Orthographic : PersOrthoType.Perspective);

							//Debug.LogWarning("> 여러개의 카메라");
						}


					}
				}
			}
			
			//RefreshKey도 교체하자
			if (_refreshKey < 0)
			{
				_refreshKey = 10;
			}
			else
			{
				_refreshKey++;
				if (_refreshKey > 9999) { _refreshKey = 10; }
			}

			
			RefreshMatrix();

			return true;
			
		}

		private void RefreshMatrix()
		{
			if (_numberOfCameraType == NumberOfCamera.None)
			{
				_worldToCameraMatrix = Matrix4x4.identity;
				_zDepthToCam = 0.0f;
				return;
			}

			if (_numberOfCameraType == NumberOfCamera.Single)
			{
				//Single 방식이라면
				_worldToCameraMatrix = _mainData._camera.worldToCameraMatrix;

				_isAllSameForward = true;
				_isAllOrthographic = _mainData._camera.orthographic;
			}
			else
			{
				//Multiple 방식이라면
				//평균 Transform을 계산해야한다.
				_centerPos = Vector3.zero;

				if (_dualCameraType == DualCameraType.LeftRight)
				{
					//Left, Right 카메라가 있는 경우
					//위치 : Center
					//각도 : Lerp
					//Forward Vector : Add
					_centerPos = (_leftEyeData._transform.position * 0.5f) + (_rightEyeData._transform.position * 0.5f);
					_centerRotation = Quaternion.Lerp(_leftEyeData._transform.rotation, _rightEyeData._transform.rotation, 0.5f);
					_centerForward = (_leftEyeData._transform.forward * 0.5f) + (_rightEyeData._transform.forward * 0.5f);
				}
				else
				{
					//여러개의 불특정 카메라가 있는 경우
					//위치 : Center
					//각도 : Main
					//Forward Vector : Main
					for (int i = 0; i < _nCameras; i++)
					{
						_cal_curCamData = _cameraDataList[i];
						_centerPos += _cal_curCamData._transform.position;
					}
					_centerPos.x /= _nCameras;
					_centerPos.y /= _nCameras;
					_centerPos.z /= _nCameras;

					_centerRotation = _mainData._transform.rotation;
				}

				//- 위치는 평균의 중점
				//- 각도/크기는 첫 카메라의 것을 이용

				//World To Camera Matrix를 만들자
				//https://forum.unity.com/threads/reproducing-cameras-worldtocameramatrix.365645/ : 참조
				_worldToCameraMatrix = Matrix4x4.Inverse(Matrix4x4.TRS(_centerPos, _centerRotation, Vector3.one));
				_worldToCameraMatrix.m20 *= -1;
				_worldToCameraMatrix.m21 *= -1;
				_worldToCameraMatrix.m22 *= -1;
				_worldToCameraMatrix.m23 *= -1;


				//렌더 카메라를 확인하자
				if (_nRenderCameras <= 0)
				{
					_isAllSameForward = true;
					_isAllOrthographic = true;
				}
				else if (_nRenderCameras == 1)
				{
					_isAllSameForward = true;

					_cal_curCamData = _renderCameraDataList[0];
					_isAllOrthographic = _cal_curCamData._camera.orthographic;
				}
				else
				{
					_isAllSameForward = true;

					_cal_curCamData = _renderCameraDataList[0];
					_isAllOrthographic = _cal_curCamData._camera.orthographic;
					_cal_Forward = _cal_curCamData._transform.forward;

					for (int i = 1; i < _nRenderCameras; i++)
					{
						_cal_curCamData = _renderCameraDataList[i];

						if (!_cal_curCamData._camera.orthographic)
						{
							_isAllOrthographic = false;
						}
						if (_isAllSameForward)
						{
							if (Mathf.Abs(_cal_curCamData._transform.forward.x - _cal_Forward.x) > 0.001f ||
								Mathf.Abs(_cal_curCamData._transform.forward.y - _cal_Forward.y) > 0.001f ||
								Mathf.Abs(_cal_curCamData._transform.forward.z - _cal_Forward.z) > 0.001f)
							{
								_isAllSameForward = false;
							}
						}

						if(!_isAllOrthographic && !_isAllSameForward)
						{
							break;
						}
					}
				}
			}

			_zDepthToCam = _worldToCameraMatrix.MultiplyPoint3x4(_parentPortrait._transform.position).z;
		}


		/// <summary>
		/// 추가 v1.5.0 : Refresh Matrix가 호출되지 않은 상황에서 카메라의 Orthographic과 Forward를 체크한다.
		/// 클리핑 마스크 텍스쳐 사이즈 최적화 규칙을 체크하기 위함.
		/// Refresh Matrix의 라이트 버전이다.
		/// </summary>
		private void RefreshForwardAndOrthographic()
		{
			switch(_numberOfCameraType)
			{
				case NumberOfCamera.None:
					return;

				case NumberOfCamera.Single:
					{
						// [ Single 방식 ]
						_isAllSameForward = true;
						_isAllOrthographic = _mainData._camera.orthographic;
					}
					break;

				case NumberOfCamera.Multiple:
					{
						// [ Multiple 방식 ]
						if (_nRenderCameras <= 0)
						{
							_isAllSameForward = true;
							_isAllOrthographic = true;
						}
						else if (_nRenderCameras == 1)
						{
							_isAllSameForward = true;

							_cal_curCamData = _renderCameraDataList[0];
							_isAllOrthographic = _cal_curCamData._camera.orthographic;
						}
						else
						{
							_isAllSameForward = true;

							_cal_curCamData = _renderCameraDataList[0];
							_isAllOrthographic = _cal_curCamData._camera.orthographic;
							_cal_Forward = _cal_curCamData._transform.forward;

							for (int i = 1; i < _nRenderCameras; i++)
							{
								_cal_curCamData = _renderCameraDataList[i];

								if (!_cal_curCamData._camera.orthographic)
								{
									_isAllOrthographic = false;
								}

								if (_isAllSameForward)
								{
									if (Mathf.Abs(_cal_curCamData._transform.forward.x - _cal_Forward.x) > 0.001f ||
										Mathf.Abs(_cal_curCamData._transform.forward.y - _cal_Forward.y) > 0.001f ||
										Mathf.Abs(_cal_curCamData._transform.forward.z - _cal_Forward.z) > 0.001f)
									{
										_isAllSameForward = false;
									}
								}

								if(!_isAllOrthographic && !_isAllSameForward)
								{
									break;
								}
							}
						}
					}
					break;
			}
		}



		private bool IsLookPortrait(Camera camera)
		{
			if (camera == null)
			{
				return false;
			}
			return (camera.cullingMask == (camera.cullingMask | (1 << _parentPortrait.gameObject.layer)) && camera.enabled);
		}

		// Get / Set
		//-------------------------------------------------------
		public Matrix4x4 WorldToCameraMatrix
		{
			get { return _worldToCameraMatrix; }
		}

		public float ZDepthToCamera
		{
			get { return _zDepthToCam; }
		}

		public PersOrthoType CameraPersOrthoType
		{
			get { return _persOrthoType; }
		}


		public bool IsValid
		{
			get { return _numberOfCameraType != NumberOfCamera.None; }
		}

		public Transform MainTransform
		{
			get { return _mainData._transform; }
		}

		public Quaternion Rotation
		{
			get
			{
				if (_numberOfCameraType == NumberOfCamera.Multiple &&
					_dualCameraType == DualCameraType.LeftRight)
				{
					return _centerRotation;
				}

				return _mainData._transform.rotation;
			}
		}

		public Vector3 Forward
		{
			get
			{
				if (_numberOfCameraType == NumberOfCamera.Multiple &&
					_dualCameraType == DualCameraType.LeftRight)
				{
					return _centerForward;
				}

				return _mainData._transform.forward;
			}
		}

		public Vector3 Positon
		{
			get
			{
				if (_numberOfCameraType == NumberOfCamera.Multiple &&
					_dualCameraType == DualCameraType.LeftRight)
				{
					return _centerPos;
				}

				return _mainData._transform.position;
			}
		}

		public List<CameraData> RenderCameraDataList
		{
			get { return _renderCameraDataList; }
		}

		public bool IsAllSameForward
		{
			get { return _isAllSameForward; }
		}

		public bool IsAllOrthographic
		{
			get {  return _isAllOrthographic; }
		}

		public apPortrait.VR_SUPPORT_MODE VRSupportMode
		{
			get {  return _vrSupportMode; }
		}

		public apPortrait.VR_RT_SIZE VRRenderTextureSize
		{
			get
			{
#if UNITY_2019_1_OR_NEWER
				return _vrRTSize;
#else
				return apPortrait.VR_RT_SIZE.ByMeshSettings;
#endif
			}
		}

		public NumberOfCamera GetNumberOfCamera()
		{
			return _numberOfCameraType;
		}
	}
}