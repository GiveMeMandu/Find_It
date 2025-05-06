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
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;
using System.Runtime.InteropServices;

namespace AnyPortrait
{

	[Serializable]
	public class apTransform_Mesh
	{
		// Members
		//--------------------------------------------
		[SerializeField]
		public int _meshUniqueID = -1;

		[SerializeField]
		public int _transformUniqueID = -1;

		[SerializeField]
		public string _nickName = "";

		[NonSerialized]
		public apMesh _mesh = null;

		[SerializeField]
		public apMatrix _matrix = new apMatrix();//이건 기본 Static Matrix

		[SerializeField]
		public Color _meshColor2X_Default = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[SerializeField]
		public bool _isVisible_Default = true;



		[SerializeField]
		public int _depth = 0;

		//삭제 [v1.4.2]
		//[SerializeField]
		//public int _level = 0;//Parent부터 내려오는 Level


		//Shader 정보
		[SerializeField]
		public apPortrait.SHADER_TYPE _shaderType = apPortrait.SHADER_TYPE.AlphaBlend;

		[SerializeField]
		public bool _isCustomShader = false;

		[SerializeField]
		public Shader _customShader = null;

		public enum RENDER_TEXTURE_SIZE
		{
			s_64, s_128, s_256, s_512, s_1024
		}

		[SerializeField]
		public RENDER_TEXTURE_SIZE _renderTexSize = RENDER_TEXTURE_SIZE.s_256;

		//추가 : Socket
		//Bake할때 소켓을 생성한다.
		[SerializeField]
		public bool _isSocket = false;


		//[SerializeField]
		//public Color _color = new Color(0.5f, 0.5f, 0.5f, 1.0f);//<<이걸 쓰는 곳이 없는데요?

		//계산용 변수
		///// <summary>Parent로부터 누적된 WorldMatrix. 자기 자신의 Matrix는 포함되지 않는다.</summary>
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_Parent = apMatrix3x3.identity;

		////추가
		///// <summary>누적되지 않은 기본 Pivot Transform + Modifier 결과만 가지고 있는 값이다.</summary>
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_Local = apMatrix3x3.identity;

		//World Transform을 구하기 위해선
		// World Transform = [Parent World] x [To Parent] x [Modified]

		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorld = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorldWithoutMod = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_ToParent = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_LocalModified = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_World = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_WorldWithoutMod = new apMatrix();


		//추가 20.8.10 만약 Rigging이 적용된 상태라면, 부모 메시 그룹으로부터는 Modifier가 적용되지 않은 World Transform을 받아야 한다.
		//에디터 변수로은 NonSerialized이지만, Opt에서는 Bake된 값이다.
		//기본값은 false
		[NonSerialized]
		private bool _isIgnoreParentModWorldMatrixByRigging = false;

		


		//20.8.5 Inverse Matrix 삭제. 사용하는 곳이 없다.
		//[NonSerialized]
		//public apMatrix _invMatrix_TFResult_World = new apMatrix();

		//[NonSerialized]
		//public apMatrix _invMatrix_TFResult_WorldWithoutMod = new apMatrix();

		//Clipping Mask 관련
		//Child : -> 연결될 Parent MeshTransform을 저장한다. [여기서는 Depth는 Parent를 공유한다.]
		//Parent : -> 하위에 연결된 MeshTransform을 저장한다.
		[SerializeField]
		public bool _isClipping_Parent = false;

		[SerializeField]
		public bool _isClipping_Child = false;

		//Child Transform 일때 -> Parent 위주로 저장을 하자
		//public int _clipParentMeshTransformID = -1;
		[SerializeField]
		public int _clipIndexFromParent = -1;//렌더링 순서가 되는 Index : 1, 2, 3(1먼저 출력한다)

		[NonSerialized]
		public apTransform_Mesh _clipParentMeshTransform = null;

		//Parent Transform 일때
		[Serializable]
		public class ClipMeshSet
		{
			[SerializeField] public int _transformID = -1;//이건 저장한다.

			[NonSerialized]
			public apTransform_Mesh _meshTransform = null;

			//삭제 v1.5.0 : MeshTF에서 참조를 하도록 한다.
			// [NonSerialized]
			// public apRenderUnit _renderUnit = null;

			/// <summary>
			/// 백업용 생성자
			/// </summary>
			public ClipMeshSet()
			{

			}

			public ClipMeshSet(int transformID)
			{
				_transformID = transformID;
				_meshTransform = null;
				//_renderUnit = null;//삭제 v1.5.0
			}

			public ClipMeshSet(apTransform_Mesh meshTransform)
			{
				_transformID = meshTransform._transformUniqueID;
				_meshTransform = meshTransform;
				//_renderUnit = renderUnit;//삭제 v1.5.0
			}
		}

		[SerializeField]
		public List<ClipMeshSet> _clipChildMeshes = new List<ClipMeshSet>();



		[NonSerialized]
		public apRenderUnit _linkedRenderUnit = null;

		[SerializeField]
		public bool _isAlways2Side = false;//<<추가. 항상 양면 렌더링을 하는 경우

		//추가 9.25 : 그림자 옵션
		[SerializeField]
		public bool _isUsePortraitShadowOption = true;//<<그냥 apPortrait의 그림자 옵션을 따를 것인가
		[SerializeField]
		public apPortrait.SHADOW_CASTING_MODE _shadowCastingMode = apPortrait.SHADOW_CASTING_MODE.Off;
		[SerializeField]
		public bool _receiveShadow = false;

		
		//추가 19.6.9 : MaterialSet 옵션과 Property 초기화 옵션
		[SerializeField]
		public int _materialSetID = -1;

		[NonSerialized]
		public apMaterialSet _linkedMaterialSet = null;

		[SerializeField]
		public bool _isUseDefaultMaterialSet = true;//<<이게 True이면 MaterialSetID에 상관없이 Default 설정의 MatSet이 적용된다. 

		//Material Set을 사용하는 것과 별개로, 커스텀하게 Property를 설정할 수 있다.
		[Serializable]
		public class CustomMaterialProperty
		{
			[SerializeField]
			public string _name = "";

			public enum SHADER_PROP_TYPE
			{
				Float = 0, 
				Int = 1,
				Vector = 2,
				Texture = 3,
				Color = 4,
				Keyword = 5,//추가 v1.5.1
			}

			[SerializeField]
			public SHADER_PROP_TYPE _propType = SHADER_PROP_TYPE.Float;

			[SerializeField]
			public float _value_Float = 0.0f;

			[SerializeField]
			public int _value_Int = 0;

			[SerializeField]
			public Vector4 _value_Vector = new Vector4(0, 0, 0, 0);

			[SerializeField]
			public Color _value_Color = new Color(0, 0, 0, 1);

			[SerializeField]
			public bool _value_Keyword = true;

			[SerializeField]
			public Texture _value_Texture = null;

			public CustomMaterialProperty()
			{

			}

			public void MakeEmpty()
			{
				_name = "<No Name>";
				_propType = SHADER_PROP_TYPE.Float;
				_value_Float = 0.0f;
				_value_Int = 0;
				_value_Vector = Vector4.zero;
				_value_Color = new Color(0, 0, 0, 1);
				_value_Texture = null;
				_value_Keyword = true;
			}

			public void CopyFromSrc(CustomMaterialProperty src)
			{
				_name = src._name;
				_propType = src._propType;
				_value_Float = src._value_Float;
				_value_Int = src._value_Int;
				_value_Vector = src._value_Vector;
				_value_Color = src._value_Color;
				_value_Texture = src._value_Texture;
				_value_Keyword = src._value_Keyword;
			}
		}

		[SerializeField]
		public List<CustomMaterialProperty> _customMaterialProperties = new List<CustomMaterialProperty>();


		// Init
		//--------------------------------------------
		/// <summary>
		/// 백업용 생성자. 코드에서 호출하지 말자
		/// </summary>
		public apTransform_Mesh()
		{

		}

		public apTransform_Mesh(int uniqueID)
		{
			_transformUniqueID = uniqueID;
		}

		public void RegistIDToPortrait(apPortrait portrait)
		{
			portrait.RegistUniqueID(apIDManager.TARGET.Transform, _transformUniqueID);
		}


		// Functions
		//--------------------------------------------
		public void ReadyToCalculate()
		{
			_matrix.MakeMatrix();

			//변경
			//[Parent World x To Parent x Local TF] 조합으로 변경

			if (_matrix_TF_ParentWorld == null)					{ _matrix_TF_ParentWorld = new apMatrix(); }
			if (_matrix_TF_ParentWorldWithoutMod == null)		{ _matrix_TF_ParentWorldWithoutMod = new apMatrix(); }
			if (_matrix_TF_ToParent == null)					{ _matrix_TF_ToParent = new apMatrix(); }
			if (_matrix_TF_LocalModified == null)				{ _matrix_TF_LocalModified = new apMatrix(); }
			if (_matrix_TFResult_World == null)					{ _matrix_TFResult_World = new apMatrix(); }
			if (_matrix_TFResult_WorldWithoutMod == null)		{ _matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			//20.8.6 : Inverse Matrix 삭제
			//if (_invMatrix_TFResult_World == null)				{ _invMatrix_TFResult_World = new apMatrix(); }
			//if (_invMatrix_TFResult_WorldWithoutMod == null)	{ _invMatrix_TFResult_WorldWithoutMod = new apMatrix(); }


			_matrix_TF_ParentWorld.SetIdentity();
			_matrix_TF_ParentWorldWithoutMod.SetIdentity();
			_matrix_TF_ToParent.SetIdentity();
			_matrix_TF_LocalModified.SetIdentity();

			//ToParent는 Pivot이므로 고정
			_matrix_TF_ToParent.SetMatrix(_matrix, true);

			_matrix_TFResult_World.SetIdentity();
			_matrix_TFResult_WorldWithoutMod.SetIdentity();
			
			//20.8.6 : Inverse Matrix 삭제
			//_invMatrix_TFResult_World.SetIdentity();
			//_invMatrix_TFResult_WorldWithoutMod.SetIdentity();

			//CalculatedLog.ReadyToRecord();

			//추가 20.8.10 : 리깅이 적용되면 부모의 WorldMatrix는 "모디파이어가 적용 안된 버전"을 인식해야한다.
			_isIgnoreParentModWorldMatrixByRigging = false;//지금은 초기화
		}


		//public void SetModifiedTransform(apMatrix matrix_modified, apCalculatedLog calResultStack_CalLog)
		public void SetModifiedTransform(apMatrix matrix_modified)
		{
			_matrix_TF_LocalModified.SetMatrix(matrix_modified, false);
		}

		//추가 20.8.10
		//만약 리깅이 적용되었다면 이 함수를 호출해야한다.
		public void SetRiggingApplied()
		{
			//리깅이 적용되면 부모의 WorldMatrix는 "모디파이어가 적용 안된 버전"을 인식해야한다.
			_isIgnoreParentModWorldMatrixByRigging = true;
		}

		/// <summary>
		/// Parent의 Matrix를 추가한다. (Parent x This)
		/// </summary>
		/// <param name="matrix_parentTransform"></param>
		//public void AddWorldMatrix_Parent(apMatrix3x3 matrix_parentTransform)
		public void AddWorldMatrix_Parent(apMatrix matrix_parentTransform, apMatrix matrix_parentTransformNoMod)
		{
			_matrix_TF_ParentWorld.SetMatrix(matrix_parentTransform, false);
			_matrix_TF_ParentWorldWithoutMod.SetMatrix(matrix_parentTransformNoMod, false);
		}

		public void MakeTransformMatrix()
		{
			//중요! 계산에 적용되는 Matrix를 여기서 Make하자 (20.9.6)
			//_matrix_TF_ToParent.MakeMatrix();
			_matrix_TF_LocalModified.MakeMatrix();
			
			_matrix_TF_ParentWorld.MakeMatrix();
			_matrix_TF_ParentWorldWithoutMod.MakeMatrix();

			//이전 : 20.10.29에 위치 아래로 조금 위치를 바꿈
			////[R]
			////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//_matrix_TFResult_World.OnBeforeRMultiply();
			//_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();

			//_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent, false);
			_matrix_TFResult_World.SetMatrix(_matrix_TF_ToParent, false);//첫 계산이므로 Set을 해야한다. (20.10.28)

			//[R] <<<<
			//추가 20.10.28 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			_matrix_TFResult_World.OnBeforeRMultiply();
			
			_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified, false);			
			


			//_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ToParent, false);
			_matrix_TFResult_WorldWithoutMod.SetMatrix(_matrix_TF_ToParent, false);//첫 계산이므로 Set을 해야한다. (20.10.28)

			//[R] <<<<
			//추가 20.10.28 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();


			//변경 20.8.10 : 리깅이 적용된 경우엔, 부모의 Modified 정보를 무시해야한다.
			if(_isIgnoreParentModWorldMatrixByRigging)
			{
				//리깅이 적용된 경우
				_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorldWithoutMod, false);
				_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorldWithoutMod, false);
			}
			else
			{
				//기본
				_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld, false);

				//v1.4.4 : 부모 World Matrix를 적용할 때, 이전에는 No Mod에도 Mod가 적용된 버전을 사용했다.
				//왜지??
				//참고 : NoMod World Matrix의 경우 Bone 연산시 사용된다.
				//_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorld, false);//이전
				_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorldWithoutMod, false);//변경
			}

			
			_matrix_TFResult_World.MakeMatrix();
			_matrix_TFResult_WorldWithoutMod.MakeMatrix();
		}





		/// <summary>
		/// No Modified World Matrix를 단독으로 갱신한다. (모디파이어가 포함된 World Matrix는 여기서 갱신되지 않는다)
		/// 부모가 있다면 부모의
		/// </summary>
		/// <param name="matrix_parentTransformNoMod"></param>
		public void MakeNoModWorldMatrix(apMatrix matrix_parentTransformNoMod)
		{
			_matrix_TF_ToParent.SetMatrix(_matrix, true); //기본 매트릭스 갱신

			if(matrix_parentTransformNoMod == null)
			{
				_matrix_TF_ParentWorldWithoutMod.SetIdentity();
			}
			else
			{
				_matrix_TF_ParentWorldWithoutMod.SetMatrix(matrix_parentTransformNoMod, true);
			}
			
			//첫 호출
			_matrix_TFResult_WorldWithoutMod.SetMatrix(_matrix_TF_ToParent, false);

			//[R] RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();

			//부모 Matrix 적용
			_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorldWithoutMod, false);
			_matrix_TFResult_WorldWithoutMod.MakeMatrix();

			//디버그
			//Debug.Log("> MakeNoModWorldMatrix Mesh TF [" + _nickName + "]");
			//Debug.Log("> To Parent(Default) : " + _matrix_TF_ToParent.ToString());
			//Debug.Log("> Parent World : " + _matrix_TF_ParentWorldWithoutMod.ToString());
			//Debug.Log("> Result : " + _matrix_TFResult_WorldWithoutMod.ToString());
		}





		
#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void MatrixWrap_MakeMatrix(	ref Vector2 pos, float angleDeg, ref Vector2 scale, bool isInverseCalculate,
															ref apMatrix3x3 dstMtrxToSpace, ref apMatrix3x3 dstOnlyRotation,
															ref apMatrix3x3 dstMtrxToLowerSpace, ref apMatrix3x3 dstOnlyRotationInv,
															ref bool dstIsInverseCalculated_Space, ref bool dstIsInverseCalculated_OnlyRotation);
		
		#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void MatrixWrap_Set_OnBefore_RMul1(ref Vector2 dst_pos, ref float dst_angleDeg, ref Vector2 dst_scale,
																ref bool dst_isInitScalePositive_X, ref bool dst_isInitScalePositive_Y, ref bool dst_isAngleFlipped,
																ref apMatrix3x3 dst_mtrxToSpace, ref apMatrix3x3 dst_onlyRotation,
																ref bool dst_isInverseCalculated_Space, ref bool dst_isInverseCalculated_OnlyRotation,
																ref Vector2 srcSet_pos, float srcSet_angleDeg, ref Vector2 srcSet_scale,
																ref Vector2 srcRMul_pos, float srcRMul_angleDeg, ref Vector2 srcRMul_scale, ref apMatrix3x3 srcRMul_onlyRotation);

		#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void MatrixWrap_Set_OnBefore_RMul2(ref Vector2 dst_pos, ref float dst_angleDeg, ref Vector2 dst_scale,
																ref bool dst_isInitScalePositive_X, ref bool dst_isInitScalePositive_Y, ref bool dst_isAngleFlipped,
																ref apMatrix3x3 dst_mtrxToSpace, ref apMatrix3x3 dst_onlyRotation,
																ref bool dst_isInverseCalculated_Space, ref bool dst_isInverseCalculated_OnlyRotation,
																ref Vector2 srcSet_pos, float srcSet_angleDeg, ref Vector2 srcSet_scale,
																ref Vector2 srcRMul1_pos, float srcRMul1_angleDeg, ref Vector2 srcRMul1_scale, ref apMatrix3x3 srcRMul1_onlyRotation,
																ref Vector2 srcRMul2_pos, float srcRMul2_angleDeg, ref Vector2 srcRMul2_scale, ref apMatrix3x3 srcRMul2_onlyRotation
																);


		/// <summary>
		/// MakeTransformMatrix의 C++ DLL 버전
		/// </summary>
		public void MakeTransformMatrix_DLL()
		{
			//기존
			//_matrix_TF_LocalModified.MakeMatrix();
			//_matrix_TF_ParentWorldWithoutMod.MakeMatrix();
			//_matrix_TF_ParentWorld.MakeMatrix();

			//< C++ DLL >
			MatrixWrap_MakeMatrix(ref _matrix_TF_LocalModified._pos, _matrix_TF_LocalModified._angleDeg, ref _matrix_TF_LocalModified._scale, false,
									ref _matrix_TF_LocalModified._mtrxToSpace, ref _matrix_TF_LocalModified._mtrxOnlyRotation,
									ref _matrix_TF_LocalModified._mtrxToLowerSpace, ref _matrix_TF_LocalModified._mtrxOnlyRotationInv,
									ref _matrix_TF_LocalModified._isInverseCalculated_Space, ref _matrix_TF_LocalModified._isInverseCalculated_OnlyRotation);

			MatrixWrap_MakeMatrix(ref _matrix_TF_ParentWorldWithoutMod._pos, _matrix_TF_ParentWorldWithoutMod._angleDeg, ref _matrix_TF_ParentWorldWithoutMod._scale, false,
									ref _matrix_TF_ParentWorldWithoutMod._mtrxToSpace, ref _matrix_TF_ParentWorldWithoutMod._mtrxOnlyRotation,
									ref _matrix_TF_ParentWorldWithoutMod._mtrxToLowerSpace, ref _matrix_TF_ParentWorldWithoutMod._mtrxOnlyRotationInv,
									ref _matrix_TF_ParentWorldWithoutMod._isInverseCalculated_Space, ref _matrix_TF_ParentWorldWithoutMod._isInverseCalculated_OnlyRotation);

			MatrixWrap_MakeMatrix(ref _matrix_TF_ParentWorld._pos, _matrix_TF_ParentWorld._angleDeg, ref _matrix_TF_ParentWorld._scale, false,
									ref _matrix_TF_ParentWorld._mtrxToSpace, ref _matrix_TF_ParentWorld._mtrxOnlyRotation,
									ref _matrix_TF_ParentWorld._mtrxToLowerSpace, ref _matrix_TF_ParentWorld._mtrxOnlyRotationInv,
									ref _matrix_TF_ParentWorld._isInverseCalculated_Space, ref _matrix_TF_ParentWorld._isInverseCalculated_OnlyRotation);



			//기존
			//_matrix_TFResult_World.SetMatrix(_matrix_TF_ToParent, false);//첫 계산이므로 Set을 해야한다. (20.10.28)
			//_matrix_TFResult_World.OnBeforeRMultiply();
			//_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified, false);			
			//if(_isIgnoreParentModWorldMatrixByRigging)
			//{
			//	//리깅이 적용된 경우
			//	_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorldWithoutMod, false);
			//}
			//else
			//{
			//	//기본
			//	_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld, false);
			//}
			//_matrix_TFResult_World.MakeMatrix();


			//< C++ DLL >
			if(_isIgnoreParentModWorldMatrixByRigging)
			{
				//리깅이 적용된 경우
				MatrixWrap_Set_OnBefore_RMul2(ref _matrix_TFResult_World._pos, ref _matrix_TFResult_World._angleDeg, ref _matrix_TFResult_World._scale,
																ref _matrix_TFResult_World._isInitScalePositive_X, ref _matrix_TFResult_World._isInitScalePositive_Y, ref _matrix_TFResult_World._isAngleFlipped,
																ref _matrix_TFResult_World._mtrxToSpace, ref _matrix_TFResult_World._mtrxOnlyRotation,
																ref _matrix_TFResult_World._isInverseCalculated_Space, ref _matrix_TFResult_World._isInverseCalculated_OnlyRotation,
																ref _matrix_TF_ToParent._pos, _matrix_TF_ToParent._angleDeg, ref _matrix_TF_ToParent._scale,
																ref _matrix_TF_LocalModified._pos, _matrix_TF_LocalModified._angleDeg, ref _matrix_TF_LocalModified._scale, ref _matrix_TF_LocalModified._mtrxOnlyRotation,
																ref _matrix_TF_ParentWorldWithoutMod._pos, _matrix_TF_ParentWorldWithoutMod._angleDeg, ref _matrix_TF_ParentWorldWithoutMod._scale, ref _matrix_TF_ParentWorldWithoutMod._mtrxOnlyRotation
																);
			}
			else
			{
				//기본
				MatrixWrap_Set_OnBefore_RMul2(ref _matrix_TFResult_World._pos, ref _matrix_TFResult_World._angleDeg, ref _matrix_TFResult_World._scale,
																ref _matrix_TFResult_World._isInitScalePositive_X, ref _matrix_TFResult_World._isInitScalePositive_Y, ref _matrix_TFResult_World._isAngleFlipped,
																ref _matrix_TFResult_World._mtrxToSpace, ref _matrix_TFResult_World._mtrxOnlyRotation,
																ref _matrix_TFResult_World._isInverseCalculated_Space, ref _matrix_TFResult_World._isInverseCalculated_OnlyRotation,
																ref _matrix_TF_ToParent._pos, _matrix_TF_ToParent._angleDeg, ref _matrix_TF_ToParent._scale,
																ref _matrix_TF_LocalModified._pos, _matrix_TF_LocalModified._angleDeg, ref _matrix_TF_LocalModified._scale, ref _matrix_TF_LocalModified._mtrxOnlyRotation,
																ref _matrix_TF_ParentWorld._pos, _matrix_TF_ParentWorld._angleDeg, ref _matrix_TF_ParentWorld._scale, ref _matrix_TF_ParentWorld._mtrxOnlyRotation
																);
			}


			//기존
			//_matrix_TFResult_WorldWithoutMod.SetMatrix(_matrix_TF_ToParent, false);//첫 계산이므로 Set을 해야한다. (20.10.28)
			//_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();
			//if(_isIgnoreParentModWorldMatrixByRigging)
			//{	
			//	//리깅이 적용된 경우
			//	_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorldWithoutMod, false);
			//}
			//else
			//{
			//	//기본				
			//	_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorld, false);
			//}
			//_matrix_TFResult_WorldWithoutMod.MakeMatrix();

			//< C++ DLL >
			if(_isIgnoreParentModWorldMatrixByRigging)
			{
				//리깅이 적용된 경우
				MatrixWrap_Set_OnBefore_RMul1(ref _matrix_TFResult_WorldWithoutMod._pos, ref _matrix_TFResult_WorldWithoutMod._angleDeg, ref _matrix_TFResult_WorldWithoutMod._scale,
																ref _matrix_TFResult_WorldWithoutMod._isInitScalePositive_X, ref _matrix_TFResult_WorldWithoutMod._isInitScalePositive_Y, ref _matrix_TFResult_WorldWithoutMod._isAngleFlipped,
																ref _matrix_TFResult_WorldWithoutMod._mtrxToSpace, ref _matrix_TFResult_WorldWithoutMod._mtrxOnlyRotation,
																ref _matrix_TFResult_WorldWithoutMod._isInverseCalculated_Space, ref _matrix_TFResult_WorldWithoutMod._isInverseCalculated_OnlyRotation,
																ref _matrix_TF_ToParent._pos, _matrix_TF_ToParent._angleDeg, ref _matrix_TF_ToParent._scale,
																ref _matrix_TF_ParentWorldWithoutMod._pos, _matrix_TF_ParentWorldWithoutMod._angleDeg, ref _matrix_TF_ParentWorldWithoutMod._scale, ref _matrix_TF_ParentWorldWithoutMod._mtrxOnlyRotation
																);
			}
			else
			{
				//기본
				MatrixWrap_Set_OnBefore_RMul1(ref _matrix_TFResult_WorldWithoutMod._pos, ref _matrix_TFResult_WorldWithoutMod._angleDeg, ref _matrix_TFResult_WorldWithoutMod._scale,
																ref _matrix_TFResult_WorldWithoutMod._isInitScalePositive_X, ref _matrix_TFResult_WorldWithoutMod._isInitScalePositive_Y, ref _matrix_TFResult_WorldWithoutMod._isAngleFlipped,
																ref _matrix_TFResult_WorldWithoutMod._mtrxToSpace, ref _matrix_TFResult_WorldWithoutMod._mtrxOnlyRotation,
																ref _matrix_TFResult_WorldWithoutMod._isInverseCalculated_Space, ref _matrix_TFResult_WorldWithoutMod._isInverseCalculated_OnlyRotation,
																ref _matrix_TF_ToParent._pos, _matrix_TF_ToParent._angleDeg, ref _matrix_TF_ToParent._scale,
																ref _matrix_TF_ParentWorld._pos, _matrix_TF_ParentWorld._angleDeg, ref _matrix_TF_ParentWorld._scale, ref _matrix_TF_ParentWorld._mtrxOnlyRotation
																);
			}

		}



		// Clip 관련 코드
		//--------------------------------------------
		/// <summary>
		/// Parent 로서의 Clipping 세팅을 초기화한다. (Child일땐 초기화되지 않는다.)
		/// </summary>
		public void InitClipMeshAsParent()
		{
			_isClipping_Parent = false;
			if (_clipChildMeshes == null)
			{
				_clipChildMeshes = new List<ClipMeshSet>();
			}
			_clipChildMeshes.Clear();


			//미사용 코드
			//for (int i = 0; i < 3; i++)
			//{
			//	_clipChildMeshTransformIDs[i] = -1;
			//	_clipChildMeshTransforms[i] = null;
			//	_clipChildRenderUnits[i] = null;
			//}
		}


		//삭제 v1.5.0 : ClipMeshSet에서 MeshTF만 저장하기로 하면서 이 클래스가 필요 없어졌다.
		// private class RenderUnitTransformMeshSet
		// {
		// 	public apTransform_Mesh _meshTransform = null;
		// 	public apRenderUnit _renderUnit = null;
		// 	public RenderUnitTransformMeshSet(apTransform_Mesh meshTransform, apRenderUnit renderUnit)
		// 	{
		// 		_meshTransform = meshTransform;
		// 		_renderUnit = renderUnit;
		// 	}
		// }
		public void SortClipMeshTransforms()
		{
			if (_isClipping_Parent)
			{
				//이전
				//List<RenderUnitTransformMeshSet> childList = new List<RenderUnitTransformMeshSet>();

				//변경 v1.5.0 : RenderUnit 없이 MeshTF 만으로 Sort를 한다.
				List<apTransform_Mesh> sortedChildMeshTFs = new List<apTransform_Mesh>();

				int nClipMeshes = _clipChildMeshes != null ? _clipChildMeshes.Count : 0;
				if(nClipMeshes > 0)
				{
					ClipMeshSet clipMeshSet = null;
					for (int i = 0; i < nClipMeshes; i++)
					{
						clipMeshSet = _clipChildMeshes[i];
						if(clipMeshSet._meshTransform == null)
						{
							continue;
						}
						//이전
						// if (!childList.Exists(delegate (RenderUnitTransformMeshSet a)
						// {
						// 	return a._meshTransform == clipMeshSet._meshTransform;
						// }))
						// {
						// 	childList.Add(
						// 		new RenderUnitTransformMeshSet(clipMeshSet._meshTransform,
						// 										clipMeshSet._renderUnit));
						// }
						//변경 v1.5.0
						if(!sortedChildMeshTFs.Contains(clipMeshSet._meshTransform))
						{
							sortedChildMeshTFs.Add(clipMeshSet._meshTransform);
						}
					}
				}

				int nSortedChildMeshTFs = sortedChildMeshTFs.Count;

				if (nSortedChildMeshTFs > 1)
				{
					//이전
					// childList.Sort(delegate (RenderUnitTransformMeshSet a, RenderUnitTransformMeshSet b)
					// {
					// 	//Depth의 오름차순
					// 	return a._meshTransform._depth - b._meshTransform._depth;
					// });
					//변경 v1.5.0 : MeshTF 리스트로 변경
					sortedChildMeshTFs.Sort(delegate (apTransform_Mesh a, apTransform_Mesh b)
					{
						//Depth의 오름차순
						return a._depth - b._depth;
					});
				}
				

				_clipChildMeshes.Clear();
				if (nSortedChildMeshTFs == 0)
				{
					//Child가 없다면 Parent가 아니게 된다.
					_isClipping_Parent = false;
				}
				else
				{
					//이전
					// RenderUnitTransformMeshSet childRenderUnitMeshSet = null;

					// //리스트 순서대로 다시 재배치하자
					// for (int i = 0; i < childList.Count; i++)
					// {
					// 	childRenderUnitMeshSet = childList[i];
					// 	_clipChildMeshes.Add(new ClipMeshSet(childRenderUnitMeshSet._meshTransform, childRenderUnitMeshSet._renderUnit));
					// 	childRenderUnitMeshSet._meshTransform._clipParentMeshTransform = this;
					// 	childRenderUnitMeshSet._meshTransform._clipIndexFromParent = i;
					// 	childRenderUnitMeshSet._meshTransform._isClipping_Child = true;
					// }

					//변경 v1.5.0 : MeshTF만으로 Child 구성하기
					apTransform_Mesh curMeshTF = null;
					//리스트 순서대로 다시 재배치하자
					for (int i = 0; i < nSortedChildMeshTFs; i++)
					{
						curMeshTF = sortedChildMeshTFs[i];
						_clipChildMeshes.Add(new ClipMeshSet(curMeshTF));
						curMeshTF._clipParentMeshTransform = this;
						curMeshTF._clipIndexFromParent = i;
						curMeshTF._isClipping_Child = true;
					}					
				}
			}
		}

		public int GetChildClippedMeshes()
		{
			if (!_isClipping_Parent)
			{
				return -1;
			}
			return _clipChildMeshes.Count;
			//int nID = 0;
			//for (int i = 0; i < 3; i++)
			//{
			//	if(_clipChildMeshTransformIDs[i] >= 0)
			//	{
			//		nID++;
			//	}
			//}
			//return nID;
		}

		public void AddClippedChildMesh(apTransform_Mesh meshTransform)
		{
			_isClipping_Parent = true;

			//이전 (GC 발생)
			//ClipMeshSet existClipMeshSet = _clipChildMeshes.Find(delegate (ClipMeshSet a)
			//{
			//	return a._meshTransform == meshTransform;
			//});

			//변경 v1.5.0
			s_FindClipMeshSet_MeshTF = meshTransform;
			ClipMeshSet existClipMeshSet = _clipChildMeshes.Find(s_FindClipMeshSet_Func);
			
			if(existClipMeshSet != null)
			{
				//[ 이미 등록된 Clipped Mesh라면 ]

				//existClipMeshSet._renderUnit = renderUnit;//렌더 유닛은 갱신 > 삭제 v1.5.0

				SortClipMeshTransforms();//Clipped Mesh들을 Sorting한다.
				return;
			}

			//새로 추가한다.
			int clippIndex = _clipChildMeshes.Count;
			_clipChildMeshes.Add(new ClipMeshSet(meshTransform));

			meshTransform._isClipping_Child = true;
			meshTransform._clipIndexFromParent = clippIndex;

			SortClipMeshTransforms();
		}

		private static apTransform_Mesh s_FindClipMeshSet_MeshTF = null;
		private static Predicate<ClipMeshSet> s_FindClipMeshSet_Func = FUNC_FindClipMeshSet;
		private static bool FUNC_FindClipMeshSet(ClipMeshSet a)
		{
			return a._meshTransform == s_FindClipMeshSet_MeshTF;
		}


		// Get / Set
		//--------------------------------------------
		public bool IsRiggedChildMeshTF(apMeshGroup curMeshGroup)
		{
			if(curMeshGroup == null)
			{
				return false;
			}
			return _isIgnoreParentModWorldMatrixByRigging 
				&& (_linkedRenderUnit != null 
					&& _linkedRenderUnit._parentRenderUnit != null
					&& _linkedRenderUnit._parentRenderUnit != curMeshGroup._rootRenderUnit);
		}
		
	}

}