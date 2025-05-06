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
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
using AnyPortrait;

#if UNITY_EDITOR
namespace AnyPortrait
{
	/// <summary>
	/// MaterialSet을 설정하는데 도움을 주는 Library 클래스
	/// apEditor에서 관리하고 있다.
	/// Portrait에 있는 MaterialSet 정보와 파일 정보를 열어서 편집할 수 있게 해준다.
	/// </summary>
	public class apMaterialLibrary
	{
		// Members
		//------------------------------------------------------
		private List<apMaterialSet> _presets = new List<apMaterialSet>();
		public List<apMaterialSet> Presets { get {  return _presets; } }

		private bool _isFirstLoad = false;

		//추가 : 기본 경로 (20.4.21)
		private string _basePath = "";

		//예약된 재질 설정
		public const int RESERVED_MAT_ID__Unlit = 0;
		public const int RESERVED_MAT_ID__Unlit_V2 = 2;//추가 v1.4.7 : 이게 기본이다
		public const int RESERVED_MAT_ID__Lit = 11;
		public const int RESERVED_MAT_ID__Bumped = 12;
		public const int RESERVED_MAT_ID__Bumped_Specular = 13;
		public const int RESERVED_MAT_ID__Bumped_Specular_Emissive = 14;
		public const int RESERVED_MAT_ID__Bumped_Rimlight = 15;
		public const int RESERVED_MAT_ID__Bumped_Ramp = 16;
		public const int RESERVED_MAT_ID__LWRP_Unlit = 51;
		public const int RESERVED_MAT_ID__LWRP_2D_Lit = 52;
		public const int RESERVED_MAT_ID__KeepAlpha_Unlit = 61;
		public const int RESERVED_MAT_ID__KeepAlpha_Lit = 62;
		public const int RESERVED_MAT_ID__VR_Unlit = 71;
		public const int RESERVED_MAT_ID__VR_Lit = 72;
		public const int RESERVED_MAT_ID__VR_Unlit_ScaleOffset = 73;
		public const int RESERVED_MAT_ID__VR_Lit_ScaleOffset = 74;

		//추가 22.1.5
		public const int RESERVED_MAT_ID__Mergeable_Unlit = 63;
		public const int RESERVED_MAT_ID__Mergeable_Lit = 64;

		public const int RESERVED_MAT_ID__URP_Unlit = 81;
		public const int RESERVED_MAT_ID__URP_Lit = 82;
		public const int RESERVED_MAT_ID__URP_2DLit = 83;

		public const int RESERVED_MAT_ID__URP21_Unlit = 84;
		public const int RESERVED_MAT_ID__URP21_Lit = 85;
		public const int RESERVED_MAT_ID__URP21_BumpedLit = 86;
		public const int RESERVED_MAT_ID__URP21_2DLit = 87;
		public const int RESERVED_MAT_ID__URP21_2DBumpedLit = 88;

		public const int RESERVED_MAT_ID__URP21_MergeableUnlit = 89;//<<이것도 추가
		public const int RESERVED_MAT_ID__URP21_Mergeable2DLit = 90;


		public const int RESERVED_MAT_ID__URP23_Unlit = 92;
		public const int RESERVED_MAT_ID__URP23_Lit = 93;
		public const int RESERVED_MAT_ID__URP23_BumpedLit = 94;
		public const int RESERVED_MAT_ID__URP23_2DLit = 95;
		public const int RESERVED_MAT_ID__URP23_2DBumpedLit = 96;

		public const int RESERVED_MAT_ID__URP23_MergeableUnlit = 97;
		public const int RESERVED_MAT_ID__URP23_Mergeable2DLit = 98;

		//Reserved는 200 이내이면 된다.



		public enum PRESET_TYPE
		{
			/// <summary>삭제, 편집 불가능한 예약된 프리셋 (기본 Unlit)</summary>
			Reserved_NotRemovable,
			/// <summary>편집은 불가능하고 삭제는 가능한 예약된 프리셋 (패키지에서 로드)</summary>
			Reserved_Removable,
			/// <summary>프리셋이 아니거나 커스텀 프리셋이다. (삭제, 편집 가능)</summary>
			NoneOrCustom

		}

		// Init
		//------------------------------------------------------
		public apMaterialLibrary(string basePath)
		{
			if(_presets == null)
			{
				_presets = new List<apMaterialSet>();
			}
			_basePath = basePath;

			ClearPresets();

			_isFirstLoad = false;
		}


		public void ClearPresets()
		{
			//Debug.Log("Clear MaterialLibrary Presets");
			_presets.Clear();
		}


		// Functions
		//------------------------------------------------------
		private void MakeReservedPresets()
		{
			//Debug.Log("Make Reserved MaterialLibrary Presets");
			//ClearPresets();

			//1. 기본 Unlit 타입의 MaterialSet
			apMaterialSet mat_UnlitLegacy = MakeReservedPreset(RESERVED_MAT_ID__Unlit, "Unlit (Legacy)", apMaterialSet.ICON.Unlit,
												"apShader_Transparent",
												"apShader_Transparent_Additive",
												"apShader_Transparent_SoftAdditive",
												"apShader_Transparent_Multiplicative",
												"apShader_ClippedWithMask",
												"apShader_ClippedWithMask_Additive",
												"apShader_ClippedWithMask_SoftAdditive",
												"apShader_ClippedWithMask_Multiplicative",
												"Linear/apShader_L_Transparent",
												"Linear/apShader_L_Transparent_Additive",
												"Linear/apShader_L_Transparent_SoftAdditive",
												"Linear/apShader_L_Transparent_Multiplicative",
												"Linear/apShader_L_ClippedWithMask",
												"Linear/apShader_L_ClippedWithMask_Additive",
												"Linear/apShader_L_ClippedWithMask_SoftAdditive",
												"Linear/apShader_L_ClippedWithMask_Multiplicative",
												"apShader_AlphaMask",
												true);

			
			if(mat_UnlitLegacy != null)
			{
				mat_UnlitLegacy.CheckAndRemoveDuplicatedProperties();//중복 프로퍼티 제거
			}

			//v1.4.7 : 새로운 Unlit. 완전히 라이팅을 받지 않는다.
			apMaterialSet mat_UnlitV2 = MakeReservedPreset(RESERVED_MAT_ID__Unlit_V2, "Unlit (v2)", apMaterialSet.ICON.Unlit,
												"Unlit 2/apShader_Unlit2_T_Alpha",
												"Unlit 2/apShader_Unlit2_T_Add",
												"Unlit 2/apShader_Unlit2_T_Soft",
												"Unlit 2/apShader_Unlit2_T_Mul",
												"Unlit 2/apShader_Unlit2_C_Alpha",
												"Unlit 2/apShader_Unlit2_C_Add",
												"Unlit 2/apShader_Unlit2_C_Soft",
												"Unlit 2/apShader_Unlit2_C_Mul",
												"Unlit 2/Linear/apShader_L_Unlit2_T_Alpha",
												"Unlit 2/Linear/apShader_L_Unlit2_T_Add",
												"Unlit 2/Linear/apShader_L_Unlit2_T_Soft",
												"Unlit 2/Linear/apShader_L_Unlit2_T_Mul",
												"Unlit 2/Linear/apShader_L_Unlit2_C_Alpha",
												"Unlit 2/Linear/apShader_L_Unlit2_C_Add",
												"Unlit 2/Linear/apShader_L_Unlit2_C_Soft",
												"Unlit 2/Linear/apShader_L_Unlit2_C_Mul",
												"Unlit 2/apShader_Unlit2_AlphaMask",
												false);//이건 Black Ambient가 필요없다.

			if(mat_UnlitV2 != null)
			{
				mat_UnlitV2.CheckAndRemoveDuplicatedProperties();//중복 프로퍼티 제거
			}
			
			


			//Advanced 패키지가 로드되었다는 가정하에
			if(	GetPresetUnit(RESERVED_MAT_ID__Lit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__Bumped) != null ||
				GetPresetUnit(RESERVED_MAT_ID__Bumped_Specular) != null ||
				GetPresetUnit(RESERVED_MAT_ID__Bumped_Specular_Emissive) != null ||
				GetPresetUnit(RESERVED_MAT_ID__Bumped_Rimlight) != null ||
				GetPresetUnit(RESERVED_MAT_ID__Bumped_Ramp) != null)
			{
				MakeReserved_Advanced(true);
			}

			//LWRP 패키지가 로드되었다는 가정하에
			if(GetPresetUnit(RESERVED_MAT_ID__LWRP_Unlit) != null)
			{	
				MakeReserved_LWRPUnlit(true);
			}

			//LWRP 2D 패키지가 로드되었다는 가정하에
			if(GetPresetUnit(RESERVED_MAT_ID__LWRP_2D_Lit) != null)
			{	
				MakeReserved_LWRP_2DLit(true);
			}

			//추가 19.10.6 : VR 패키지가 로드되었다는 가정하에
			if(GetPresetUnit(RESERVED_MAT_ID__VR_Unlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__VR_Lit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__VR_Unlit_ScaleOffset) != null ||
				GetPresetUnit(RESERVED_MAT_ID__VR_Lit_ScaleOffset) != null)
			{	
				MakeReserved_VR(true);
			}

			//추가 19.10.27 : KeepAlpha 패키지 확인 후 초기화
			if(GetPresetUnit(RESERVED_MAT_ID__KeepAlpha_Unlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__KeepAlpha_Lit) != null)
			{	
				MakeReserved_KeepAlpha(true);
			}

			//추가 22.1.5 : Mergeable 패키지 확인
			if(GetPresetUnit(RESERVED_MAT_ID__Mergeable_Unlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__Mergeable_Lit) != null)
			{
				MakeReserved_Mergeable(true);
			}


			//추가 20.1.24 : URP 패키지 확인 후 초기화
			if(GetPresetUnit(RESERVED_MAT_ID__URP_Unlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP_Lit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP_2DLit) != null)
			{
				MakeReserved_URP(true);
			}

			//추가 21.12.20 : URP(21) 패키지 확인 후 초기화
			if(GetPresetUnit(RESERVED_MAT_ID__URP21_Unlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP21_Lit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP21_BumpedLit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP21_2DLit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP21_2DBumpedLit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP21_MergeableUnlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP21_Mergeable2DLit) != null)
			{
				MakeReserved_URP21(true);
			}

			//v1.5.0 : URP(23) 패키지 확인 후 초기화
			if(GetPresetUnit(RESERVED_MAT_ID__URP23_Unlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP23_Lit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP23_BumpedLit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP23_2DLit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP23_2DBumpedLit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP23_MergeableUnlit) != null ||
				GetPresetUnit(RESERVED_MAT_ID__URP23_Mergeable2DLit) != null)
			{
				MakeReserved_URP23(true);
			}
		}


		


		private apMaterialSet MakeReservedPreset(int uniqueID, 
											string name, 
											apMaterialSet.ICON icon,
											string shaderPath_Normal_AlphaBlend,
											string shaderPath_Normal_Additive,
											string shaderPath_Normal_SoftAdditive,
											string shaderPath_Normal_Multiplicative,
											string shaderPath_Clipped_AlphaBlend,
											string shaderPath_Clipped_Additive,
											string shaderPath_Clipped_SoftAdditive,
											string shaderPath_Clipped_Multiplicative,
											string shaderPath_L_Normal_AlphaBlend,
											string shaderPath_L_Normal_Additive,
											string shaderPath_L_Normal_SoftAdditive,
											string shaderPath_L_Normal_Multiplicative,
											string shaderPath_L_Clipped_AlphaBlend,
											string shaderPath_L_Clipped_Additive,
											string shaderPath_L_Clipped_SoftAdditive,
											string shaderPath_L_Clipped_Multiplicative,
											string shaderPath_AlphaMask,
											bool isNeedToSetBlackColoredAmbient,
											bool isVRSupported = false,
											bool isUseShaderGraph = false)
		{

			//[v1.5.0] 변경
			//이미 저장된 프리셋을 다시 로드하는데, 굳이 Shader를 다시 매핑할 필요는 없다.
			//정보가 모두 유효하다면 다시 로드하는 것을 생략한다. (로딩 시간 느려지지 않게)
			//Debug.LogError("이미 생성된 재질 프리셋을 다시 로드함");
			bool isNewPreset = false;

			apMaterialSet materialSet = GetPresetUnit(uniqueID);
			if(materialSet == null)
			{
				materialSet = new apMaterialSet();
				_presets.Add(materialSet);

				isNewPreset = true;
			}

			string strExp = !isUseShaderGraph ? ".shader" : ".shadergraph";//변경 20.1.24 : 쉐이더 그래프

			//변경 20.4.21 : 고정 경로가 아니라 입력받은 경로를 이용하자
			//기본 경로는 "Assets/AnyPortrait/"

			//기존 데이터를 사용할 지 테스트를 하자
			
			bool isMakeNewReserved = false;
			if(isNewPreset)
			{
				//기존에 없었던 프리셋이라면
				isMakeNewReserved = true;
			}
			else
			{
				//기존에 있었던 프리셋이라면 조건을 체크한다.
				//모든 Shader Asset들이 존재해야 한다.
				bool isAllShaderLoaded = materialSet.IsAllShaderLoaded();
				if(!isAllShaderLoaded)
				{
					//하나라도 로드가 안된 Shader가 있다.
					isMakeNewReserved = true;
					
				}
			}

			//기존의 데이터를 그대로 활용한다.
			if(!isMakeNewReserved)
			{
				//에셋 로드까지 완료된 재질 프리셋은 강제로 다시 로드하지 않는다
				return materialSet;
			}

			//새로운 재질 프리셋이라면
			if(isNewPreset)
			{
				//Debug.Log("로드 안된 재질 프리셋 [" + name + "] >> 완전히 초기화");
				materialSet.MakeReserved(	uniqueID, 
										name, 
										icon,
										_basePath + "Assets/Shaders/" + shaderPath_Normal_AlphaBlend + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_Normal_Additive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_Normal_SoftAdditive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_Normal_Multiplicative + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_Clipped_AlphaBlend + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_Clipped_Additive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_Clipped_SoftAdditive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_Clipped_Multiplicative + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Normal_AlphaBlend + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Normal_Additive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Normal_SoftAdditive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Normal_Multiplicative + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Clipped_AlphaBlend + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Clipped_Additive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Clipped_SoftAdditive + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_L_Clipped_Multiplicative + strExp,
										_basePath + "Assets/Shaders/" + shaderPath_AlphaMask + strExp,
										isNeedToSetBlackColoredAmbient);

				//공통된 기본 프로퍼티 추가
				materialSet.AddProperty("_Color", true, apMaterialSet.SHADER_PROP_TYPE.Color);
				materialSet.AddProperty("_MainTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				if(!isVRSupported)
				{
					materialSet.AddProperty("_MaskTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				}
				else
				{
					materialSet.AddProperty("_MaskTex_L", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
					materialSet.AddProperty("_MaskTex_R", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				}
			
				materialSet.AddProperty("_MaskScreenSpaceOffset", true, apMaterialSet.SHADER_PROP_TYPE.Vector);

			}
			else
			{
				//Debug.Log("로드 안된 재질 프리셋 [" + name + "] >> 에셋 로딩");
				//새롭지는 않다면 로드만 시도하자
#if UNITY_EDITOR
				materialSet.LoadShaderAssets();
				materialSet.LoadRefMaterial();
#endif
			}
			
			return materialSet;
		}
		





		//[v1.5.0 추가]
		//Reserved Preset에서 에셋이 누락된 경우, 해당 에셋 경로 정보를 재입력한다.
		public void ReinputReservedPresetPath(apMaterialSet targetSet)
		{
			if(targetSet == null) { return; }

			switch (targetSet._uniqueID)
			{
				case RESERVED_MAT_ID__Unlit://Unlit
					ReinputPaths(	targetSet,
									"apShader_Transparent",
									"apShader_Transparent_Additive",
									"apShader_Transparent_SoftAdditive",
									"apShader_Transparent_Multiplicative",
									"apShader_ClippedWithMask",
									"apShader_ClippedWithMask_Additive",
									"apShader_ClippedWithMask_SoftAdditive",
									"apShader_ClippedWithMask_Multiplicative",
									"Linear/apShader_L_Transparent",
									"Linear/apShader_L_Transparent_Additive",
									"Linear/apShader_L_Transparent_SoftAdditive",
									"Linear/apShader_L_Transparent_Multiplicative",
									"Linear/apShader_L_ClippedWithMask",
									"Linear/apShader_L_ClippedWithMask_Additive",
									"Linear/apShader_L_ClippedWithMask_SoftAdditive",
									"Linear/apShader_L_ClippedWithMask_Multiplicative",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__Unlit_V2://Unlit v2
					ReinputPaths(	targetSet,
									"Unlit 2/apShader_Unlit2_T_Alpha",
									"Unlit 2/apShader_Unlit2_T_Add",
									"Unlit 2/apShader_Unlit2_T_Soft",
									"Unlit 2/apShader_Unlit2_T_Mul",
									"Unlit 2/apShader_Unlit2_C_Alpha",
									"Unlit 2/apShader_Unlit2_C_Add",
									"Unlit 2/apShader_Unlit2_C_Soft",
									"Unlit 2/apShader_Unlit2_C_Mul",
									"Unlit 2/Linear/apShader_L_Unlit2_T_Alpha",
									"Unlit 2/Linear/apShader_L_Unlit2_T_Add",
									"Unlit 2/Linear/apShader_L_Unlit2_T_Soft",
									"Unlit 2/Linear/apShader_L_Unlit2_T_Mul",
									"Unlit 2/Linear/apShader_L_Unlit2_C_Alpha",
									"Unlit 2/Linear/apShader_L_Unlit2_C_Add",
									"Unlit 2/Linear/apShader_L_Unlit2_C_Soft",
									"Unlit 2/Linear/apShader_L_Unlit2_C_Mul",
									"Unlit 2/apShader_Unlit2_AlphaMask");
					break;

					//[ Advanced ]
				case RESERVED_MAT_ID__Lit:
					ReinputPaths(	targetSet,
									"Advanced/Lit/apShader_Lit_T_Alpha",
									"Advanced/Lit/apShader_Lit_T_Add",
									"Advanced/Lit/apShader_Lit_T_Soft",
									"Advanced/Lit/apShader_Lit_T_Mul",
									"Advanced/Lit/apShader_Lit_C_Alpha",
									"Advanced/Lit/apShader_Lit_C_Add",
									"Advanced/Lit/apShader_Lit_C_Soft",
									"Advanced/Lit/apShader_Lit_C_Mul",
									"Advanced/Lit/Linear/apShader_L_Lit_T_Alpha",
									"Advanced/Lit/Linear/apShader_L_Lit_T_Add",
									"Advanced/Lit/Linear/apShader_L_Lit_T_Soft",
									"Advanced/Lit/Linear/apShader_L_Lit_T_Mul",
									"Advanced/Lit/Linear/apShader_L_Lit_C_Alpha",
									"Advanced/Lit/Linear/apShader_L_Lit_C_Add",
									"Advanced/Lit/Linear/apShader_L_Lit_C_Soft",
									"Advanced/Lit/Linear/apShader_L_Lit_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__Bumped:
					ReinputPaths(	targetSet,
									"Advanced/Bumped/apShader_Bumped_T_Alpha",
									"Advanced/Bumped/apShader_Bumped_T_Add",
									"Advanced/Bumped/apShader_Bumped_T_Soft",
									"Advanced/Bumped/apShader_Bumped_T_Mul",
									"Advanced/Bumped/apShader_Bumped_C_Alpha",
									"Advanced/Bumped/apShader_Bumped_C_Add",
									"Advanced/Bumped/apShader_Bumped_C_Soft",
									"Advanced/Bumped/apShader_Bumped_C_Mul",
									"Advanced/Bumped/Linear/apShader_L_Bumped_T_Alpha",
									"Advanced/Bumped/Linear/apShader_L_Bumped_T_Add",
									"Advanced/Bumped/Linear/apShader_L_Bumped_T_Soft",
									"Advanced/Bumped/Linear/apShader_L_Bumped_T_Mul",
									"Advanced/Bumped/Linear/apShader_L_Bumped_C_Alpha",
									"Advanced/Bumped/Linear/apShader_L_Bumped_C_Add",
									"Advanced/Bumped/Linear/apShader_L_Bumped_C_Soft",
									"Advanced/Bumped/Linear/apShader_L_Bumped_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__Bumped_Specular:
					ReinputPaths(	targetSet,
									"Advanced/BumpedSpecular/apShader_BumpSpec_T_Alpha",
									"Advanced/BumpedSpecular/apShader_BumpSpec_T_Add",
									"Advanced/BumpedSpecular/apShader_BumpSpec_T_Soft",
									"Advanced/BumpedSpecular/apShader_BumpSpec_T_Mul",
									"Advanced/BumpedSpecular/apShader_BumpSpec_C_Alpha",
									"Advanced/BumpedSpecular/apShader_BumpSpec_C_Add",
									"Advanced/BumpedSpecular/apShader_BumpSpec_C_Soft",
									"Advanced/BumpedSpecular/apShader_BumpSpec_C_Mul",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Alpha",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Add",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Soft",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Mul",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Alpha",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Add",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Soft",
									"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__Bumped_Specular_Emissive:
					ReinputPaths(	targetSet,
									"Advanced/BumpedSpecularEmission/apShader_BSE_T_Alpha",
									"Advanced/BumpedSpecularEmission/apShader_BSE_T_Add",
									"Advanced/BumpedSpecularEmission/apShader_BSE_T_Soft",
									"Advanced/BumpedSpecularEmission/apShader_BSE_T_Mul",
									"Advanced/BumpedSpecularEmission/apShader_BSE_C_Alpha",
									"Advanced/BumpedSpecularEmission/apShader_BSE_C_Add",
									"Advanced/BumpedSpecularEmission/apShader_BSE_C_Soft",
									"Advanced/BumpedSpecularEmission/apShader_BSE_C_Mul",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Alpha",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Add",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Soft",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Mul",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Alpha",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Add",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Soft",
									"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__Bumped_Rimlight:
					ReinputPaths(	targetSet,
									"Advanced/BumpedRimlight/apShader_BumpRim_T_Alpha",
									"Advanced/BumpedRimlight/apShader_BumpRim_T_Add",
									"Advanced/BumpedRimlight/apShader_BumpRim_T_Soft",
									"Advanced/BumpedRimlight/apShader_BumpRim_T_Mul",
									"Advanced/BumpedRimlight/apShader_BumpRim_C_Alpha",
									"Advanced/BumpedRimlight/apShader_BumpRim_C_Add",
									"Advanced/BumpedRimlight/apShader_BumpRim_C_Soft",
									"Advanced/BumpedRimlight/apShader_BumpRim_C_Mul",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Alpha",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Add",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Soft",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Mul",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Alpha",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Add",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Soft",
									"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__Bumped_Ramp:
					ReinputPaths(	targetSet,
									"Advanced/BumpedRamp/apShader_BumpRamp_T_Alpha",
									"Advanced/BumpedRamp/apShader_BumpRamp_T_Add",
									"Advanced/BumpedRamp/apShader_BumpRamp_T_Soft",
									"Advanced/BumpedRamp/apShader_BumpRamp_T_Mul",
									"Advanced/BumpedRamp/apShader_BumpRamp_C_Alpha",
									"Advanced/BumpedRamp/apShader_BumpRamp_C_Add",
									"Advanced/BumpedRamp/apShader_BumpRamp_C_Soft",
									"Advanced/BumpedRamp/apShader_BumpRamp_C_Mul",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Alpha",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Add",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Soft",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Mul",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Alpha",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Add",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Soft",
									"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Mul",
									"apShader_AlphaMask");
					break;

					//[LWRP]
				case RESERVED_MAT_ID__LWRP_Unlit:
					ReinputPaths(	targetSet,
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Alpha",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Additive",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_SoftAdditive",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Multiplicative",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Alpha",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Additive",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_SoftAdditive",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Multiplicative",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Alpha",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Additive",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_SoftAdditive",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Multiplicative",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Alpha",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Additive",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_SoftAdditive",
									"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Multiplicative",
									"Advanced/LWRP Unlit/apShader_LWRPUnlit_AlphaMask");
					break;

					//[LWRP 2D]
				case RESERVED_MAT_ID__LWRP_2D_Lit:
					ReinputPaths(	targetSet,
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_AlphaBlend",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Additive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_SoftAdditive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Multiplicative",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_AlphaBlend",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Additive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_SoftAdditive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Multiplicative",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_AlphaBlend",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Additive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_SoftAdditive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Multiplicative",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_AlphaBlend",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Additive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_SoftAdditive",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Multiplicative",
									"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_AlphaMask");
					break;

					//[Keep Alpha]
				case RESERVED_MAT_ID__KeepAlpha_Unlit:
					ReinputPaths(	targetSet,
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Alpha",
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Add",
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Soft",
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Mul",
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Alpha",
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Add",
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Soft",
									"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Mul",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Alpha",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Add",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Soft",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Mul",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Alpha",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Add",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Soft",
									"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__KeepAlpha_Lit:
					ReinputPaths(	targetSet,
									"Advanced/KeepAlphaLit/apShader_KALit_T_Alpha",
									"Advanced/KeepAlphaLit/apShader_KALit_T_Add",
									"Advanced/KeepAlphaLit/apShader_KALit_T_Soft",
									"Advanced/KeepAlphaLit/apShader_KALit_T_Mul",
									"Advanced/KeepAlphaLit/apShader_KALit_C_Alpha",
									"Advanced/KeepAlphaLit/apShader_KALit_C_Add",
									"Advanced/KeepAlphaLit/apShader_KALit_C_Soft",
									"Advanced/KeepAlphaLit/apShader_KALit_C_Mul",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Alpha",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Add",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Soft",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Mul",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Alpha",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Add",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Soft",
									"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Mul",
									"apShader_AlphaMask");
					break;

					//[VR]
				case RESERVED_MAT_ID__VR_Unlit:
					ReinputPaths(	targetSet,
									"Advanced/UnlitVR/apShader_UnlitVR_T_Alpha",
									"Advanced/UnlitVR/apShader_UnlitVR_T_Add",
									"Advanced/UnlitVR/apShader_UnlitVR_T_Soft",
									"Advanced/UnlitVR/apShader_UnlitVR_T_Mul",
									"Advanced/UnlitVR/apShader_UnlitVR_C_Alpha",
									"Advanced/UnlitVR/apShader_UnlitVR_C_Add",
									"Advanced/UnlitVR/apShader_UnlitVR_C_Soft",
									"Advanced/UnlitVR/apShader_UnlitVR_C_Mul",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Alpha",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Add",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Soft",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Mul",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Alpha",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Add",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Soft",
									"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__VR_Lit:
					ReinputPaths(	targetSet,
									"Advanced/LitVR/apShader_LitVR_T_Alpha",
									"Advanced/LitVR/apShader_LitVR_T_Add",
									"Advanced/LitVR/apShader_LitVR_T_Soft",
									"Advanced/LitVR/apShader_LitVR_T_Mul",
									"Advanced/LitVR/apShader_LitVR_C_Alpha",
									"Advanced/LitVR/apShader_LitVR_C_Add",
									"Advanced/LitVR/apShader_LitVR_C_Soft",
									"Advanced/LitVR/apShader_LitVR_C_Mul",
									"Advanced/LitVR/Linear/apShader_L_LitVR_T_Alpha",
									"Advanced/LitVR/Linear/apShader_L_LitVR_T_Add",
									"Advanced/LitVR/Linear/apShader_L_LitVR_T_Soft",
									"Advanced/LitVR/Linear/apShader_L_LitVR_T_Mul",
									"Advanced/LitVR/Linear/apShader_L_LitVR_C_Alpha",
									"Advanced/LitVR/Linear/apShader_L_LitVR_C_Add",
									"Advanced/LitVR/Linear/apShader_L_LitVR_C_Soft",
									"Advanced/LitVR/Linear/apShader_L_LitVR_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__VR_Unlit_ScaleOffset:
					ReinputPaths(	targetSet,
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Alpha",
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Add",
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Soft",
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Mul",
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Alpha",
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Add",
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Soft",
									"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Mul",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Alpha",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Add",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Soft",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Mul",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Alpha",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Add",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Soft",
									"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Mul",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__VR_Lit_ScaleOffset:
					ReinputPaths(	targetSet,
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Alpha",
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Add",
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Soft",
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Mul",
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Alpha",
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Add",
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Soft",
									"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Mul",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Alpha",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Add",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Soft",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Mul",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Alpha",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Add",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Soft",
									"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Mul",
									"apShader_AlphaMask");
					break;

					//[Mergeable]
				case RESERVED_MAT_ID__Mergeable_Unlit:
					ReinputPaths(	targetSet,
									"Advanced/Mergeable/Unlit/apShader_Mergeable_Unlit_T_Alpha",//Merged
									"apShader_Transparent_Additive",
									"apShader_Transparent_SoftAdditive",
									"apShader_Transparent_Multiplicative",
									"apShader_ClippedWithMask",
									"apShader_ClippedWithMask_Additive",
									"apShader_ClippedWithMask_SoftAdditive",
									"apShader_ClippedWithMask_Multiplicative",
									"Advanced/Mergeable/Unlit/Linear/apShader_L_Mergeable_Unlit_T_Alpha",////Merged
									"Linear/apShader_L_Transparent_Additive",
									"Linear/apShader_L_Transparent_SoftAdditive",
									"Linear/apShader_L_Transparent_Multiplicative",
									"Linear/apShader_L_ClippedWithMask",
									"Linear/apShader_L_ClippedWithMask_Additive",
									"Linear/apShader_L_ClippedWithMask_SoftAdditive",
									"Linear/apShader_L_ClippedWithMask_Multiplicative",
									"apShader_AlphaMask");
					break;

				case RESERVED_MAT_ID__Mergeable_Lit:
					ReinputPaths(	targetSet,
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Alpha",
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Add",//Add
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Soft",//SoftAdd
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Mul",//Mul
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Alpha",
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Add",//C-Add
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Soft",//C-SoftAdd
									"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Mul",//C-Mul
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Alpha",
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Add",//Add
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Soft",//SoftAdd
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Mul",//Mul
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Alpha",
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Add",//C-Add
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Soft",//C-SoftAdd
									"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Mul",//C-Mul
									"apShader_AlphaMask");
					break;

					//[URP]
				case RESERVED_MAT_ID__URP_Unlit:
					ReinputPaths(	targetSet,
									"Advanced/URP Unlit/apShaderGraph_URP_Unlit_T_Alpha",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Multiplicative",
									"Advanced/URP Unlit/apShaderGraph_URP_Unlit_C_Alpha",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Multiplicative",
									"Advanced/URP Unlit/Linear/apShaderGraph_L_URP_Unlit_T_Alpha",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Multiplicative",
									"Advanced/URP Unlit/Linear/apShaderGraph_L_URP_Unlit_C_Alpha",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Multiplicative",
									"Advanced/URP Common/apShaderGraph_URP_Common_AlphaMask",
									true);
					break;

				case RESERVED_MAT_ID__URP_Lit:
					ReinputPaths(	targetSet,
									"Advanced/URP Lit/apShaderGraph_URP_Lit_T_Alpha",
									"Advanced/URP Common/apShaderGraph_URP_Common_Lit_T_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Lit_T_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Multiplicative",
									"Advanced/URP Lit/apShaderGraph_URP_Lit_C_Alpha",
									"Advanced/URP Common/apShaderGraph_URP_Common_Lit_C_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Lit_C_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Multiplicative",
									"Advanced/URP Lit/Linear/apShaderGraph_L_URP_Lit_T_Alpha",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_T_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_T_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Multiplicative",
									"Advanced/URP Lit/Linear/apShaderGraph_L_URP_Lit_C_Alpha",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_C_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_C_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Multiplicative",
									"Advanced/URP Common/apShaderGraph_URP_Common_AlphaMask",
									true);
					break;

					//[URP 2D]
				case RESERVED_MAT_ID__URP_2DLit:
					ReinputPaths(	targetSet,
									"Advanced/URP 2D Lit (Experimental)/apShaderGraph_URP_2DLit_T_Alpha",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Multiplicative",
									"Advanced/URP 2D Lit (Experimental)/apShaderGraph_URP_2DLit_C_Alpha",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Multiplicative",
									"Advanced/URP 2D Lit (Experimental)/Linear/apShaderGraph_L_URP_2DLit_T_Alpha",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Multiplicative",
									"Advanced/URP 2D Lit (Experimental)/Linear/apShaderGraph_L_URP_2DLit_C_Alpha",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
									"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Multiplicative",
									"Advanced/URP Common/apShaderGraph_URP_Common_AlphaMask",
									true);
					break;

					//[URP 21]
				case RESERVED_MAT_ID__URP21_Unlit:
					ReinputPaths(	targetSet,
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Alpha",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",								
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Alpha",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Alpha",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Alpha",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask",
									true);
					break;

				case RESERVED_MAT_ID__URP21_Lit:
					ReinputPaths(	targetSet,
									"Advanced/URP (2021)/Lit/apShaderGraph_URP21_Lit_T_Alpha",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",								
									"Advanced/URP (2021)/Lit/apShaderGraph_URP21_Lit_C_Alpha",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Lit/Linear/apShaderGraph_URP21_L_Lit_T_Alpha",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/Lit/Linear/apShaderGraph_URP21_L_Lit_C_Alpha",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask",
									true);
					break;

				case RESERVED_MAT_ID__URP21_BumpedLit:
					ReinputPaths(	targetSet,
									"Advanced/URP (2021)/Bumped Lit/apShaderGraph_URP21_BumpedLit_T_Alpha",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/Bumped Lit/apShaderGraph_URP21_BumpedLit_C_Alpha",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Bumped Lit/Linear/apShaderGraph_URP21_L_BumpedLit_T_Alpha",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/Bumped Lit/Linear/apShaderGraph_URP21_L_BumpedLit_C_Alpha",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask",
									true);
					break;

				case RESERVED_MAT_ID__URP21_2DLit:
					ReinputPaths(	targetSet,
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Alpha",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Alpha",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Alpha",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Alpha",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask",
									true);
					break;

				case RESERVED_MAT_ID__URP21_2DBumpedLit:
					ReinputPaths(	targetSet,
									"Advanced/URP (2021)/2D Bumped Lit/apShaderGraph_URP21_2DBumpedLit_T_Alpha",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/2D Bumped Lit/apShaderGraph_URP21_2DBumpedLit_C_Alpha",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/2D Bumped Lit/Linear/apShaderGraph_URP21_L_2DBumpedLit_T_Alpha",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",
									"Advanced/URP (2021)/2D Bumped Lit/Linear/apShaderGraph_URP21_L_2DBumpedLit_C_Alpha",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",
									"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask",
									true);
					break;

				case RESERVED_MAT_ID__URP21_MergeableUnlit:
					ReinputPaths(	targetSet,
									"Advanced/URP (2021)/Mergeable Unlit/apShaderGraph_URP21_MergeableUnlit_T_Alpha",//Merged
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Alpha",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

									"Advanced/URP (2021)/Mergeable Unlit/Linear/apShaderGraph_URP21_L_MergeableUnlit_T_Alpha",//Merged
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Alpha",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

									"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask",
									true);
					break;

				case RESERVED_MAT_ID__URP21_Mergeable2DLit:
					ReinputPaths(	targetSet,
									"Advanced/URP (2021)/Mergeable 2D Lit/apShaderGraph_URP21_Mergeable2DLit_T_Alpha",//Merged
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Alpha",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
									"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
									"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

									"Advanced/URP (2021)/Mergeable 2D Lit/Linear/apShaderGraph_URP21_L_Mergeable2DLit_T_Alpha",//Merged
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Alpha",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
									"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
									"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

									"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask",
									true);
					break;

					//[URP 23]
				case RESERVED_MAT_ID__URP23_Unlit:
					ReinputPaths(	targetSet,
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask",
								true);
					break;

				case RESERVED_MAT_ID__URP23_Lit:
					ReinputPaths(	targetSet,
								"Advanced/URP (2023)/Lit/apShaderGraph_URP23_Lit_T_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Lit/apShaderGraph_URP23_Lit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Lit/Linear/apShaderGraph_URP23_L_Lit_T_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Lit/Linear/apShaderGraph_URP23_L_Lit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask",
								true);
					break;

				case RESERVED_MAT_ID__URP23_BumpedLit:
					ReinputPaths(	targetSet,
								"Advanced/URP (2023)/Bumped Lit/apShaderGraph_URP23_BumpedLit_T_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Bumped Lit/apShaderGraph_URP23_BumpedLit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Bumped Lit/Linear/apShaderGraph_URP23_L_BumpedLit_T_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Bumped Lit/Linear/apShaderGraph_URP23_L_BumpedLit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask",
								true);
					break;

				case RESERVED_MAT_ID__URP23_2DLit:
					ReinputPaths(	targetSet,
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask",
								true);
					break;

				case RESERVED_MAT_ID__URP23_2DBumpedLit:
					ReinputPaths(	targetSet,
								"Advanced/URP (2023)/2D Bumped Lit/apShaderGraph_URP23_2DBumpedLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/2D Bumped Lit/apShaderGraph_URP23_2DBumpedLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/2D Bumped Lit/Linear/apShaderGraph_URP23_L_2DBumpedLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/2D Bumped Lit/Linear/apShaderGraph_URP23_L_2DBumpedLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask",
								true);
					break;


				case RESERVED_MAT_ID__URP23_MergeableUnlit:
					ReinputPaths(	targetSet,
								"Advanced/URP (2023)/Mergeable Unlit/apShaderGraph_URP23_MergeableUnlit_T_Alpha",//Merged
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Mergeable Unlit/Linear/apShaderGraph_URP23_L_MergeableUnlit_T_Alpha",//Merged
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask",
								true);
					break;

				case RESERVED_MAT_ID__URP23_Mergeable2DLit:
					ReinputPaths(	targetSet,
								"Advanced/URP (2023)/Mergeable 2D Lit/apShaderGraph_URP23_Mergeable2DLit_T_Alpha",//Merged
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Mergeable 2D Lit/Linear/apShaderGraph_URP23_L_Mergeable2DLit_T_Alpha",//Merged
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask",
								true);
					break;
			}
		}

		private void ReinputPaths(	apMaterialSet targetSet,
									string shaderPath_Normal_AlphaBlend,
									string shaderPath_Normal_Additive,
									string shaderPath_Normal_SoftAdditive,
									string shaderPath_Normal_Multiplicative,
									string shaderPath_Clipped_AlphaBlend,
									string shaderPath_Clipped_Additive,
									string shaderPath_Clipped_SoftAdditive,
									string shaderPath_Clipped_Multiplicative,
									string shaderPath_L_Normal_AlphaBlend,
									string shaderPath_L_Normal_Additive,
									string shaderPath_L_Normal_SoftAdditive,
									string shaderPath_L_Normal_Multiplicative,
									string shaderPath_L_Clipped_AlphaBlend,
									string shaderPath_L_Clipped_Additive,
									string shaderPath_L_Clipped_SoftAdditive,
									string shaderPath_L_Clipped_Multiplicative,
									string shaderPath_AlphaMask,
									bool isShaderGraph = false)
		{
			if(targetSet == null)
			{
				return;
			}

			string strExp = !isShaderGraph ? ".shader" : ".shadergraph";//변경 20.1.24 : 쉐이더 그래프
			string prePath = _basePath + "Assets/Shaders/";

			shaderPath_Normal_AlphaBlend =		prePath + shaderPath_Normal_AlphaBlend + strExp;
			shaderPath_Normal_Additive =		prePath + shaderPath_Normal_Additive + strExp;
			shaderPath_Normal_SoftAdditive =	prePath + shaderPath_Normal_SoftAdditive + strExp;
			shaderPath_Normal_Multiplicative =	prePath + shaderPath_Normal_Multiplicative + strExp;

			shaderPath_Clipped_AlphaBlend =		prePath + shaderPath_Clipped_AlphaBlend + strExp;
			shaderPath_Clipped_Additive =		prePath + shaderPath_Clipped_Additive + strExp;
			shaderPath_Clipped_SoftAdditive =	prePath + shaderPath_Clipped_SoftAdditive + strExp;
			shaderPath_Clipped_Multiplicative = prePath + shaderPath_Clipped_Multiplicative + strExp;

			shaderPath_L_Normal_AlphaBlend =	prePath + shaderPath_L_Normal_AlphaBlend + strExp;
			shaderPath_L_Normal_Additive =		prePath + shaderPath_L_Normal_Additive + strExp;
			shaderPath_L_Normal_SoftAdditive =	prePath + shaderPath_L_Normal_SoftAdditive + strExp;
			shaderPath_L_Normal_Multiplicative = prePath + shaderPath_L_Normal_Multiplicative + strExp;

			shaderPath_L_Clipped_AlphaBlend =	prePath + shaderPath_L_Clipped_AlphaBlend + strExp;
			shaderPath_L_Clipped_Additive =		prePath + shaderPath_L_Clipped_Additive + strExp;
			shaderPath_L_Clipped_SoftAdditive = prePath + shaderPath_L_Clipped_SoftAdditive + strExp;
			shaderPath_L_Clipped_Multiplicative = prePath + shaderPath_L_Clipped_Multiplicative + strExp;

			shaderPath_AlphaMask = prePath + shaderPath_AlphaMask + strExp;

			//Shader 에셋이 Null인 경우에 Path를 다시 입력한다.
			if(targetSet._shader_Normal_AlphaBlend == null)		{ targetSet._shaderPath_Normal_AlphaBlend =		shaderPath_Normal_AlphaBlend; }
			if(targetSet._shader_Normal_Additive == null)		{ targetSet._shaderPath_Normal_Additive =		shaderPath_Normal_Additive; }
			if(targetSet._shader_Normal_SoftAdditive == null)	{ targetSet._shaderPath_Normal_SoftAdditive =	shaderPath_Normal_SoftAdditive; }
			if(targetSet._shader_Normal_Multiplicative == null)	{ targetSet._shaderPath_Normal_Multiplicative =	shaderPath_Normal_Multiplicative; }

			if(targetSet._shader_Clipped_AlphaBlend == null)		{ targetSet._shaderPath_Clipped_AlphaBlend =		shaderPath_Clipped_AlphaBlend; }
			if(targetSet._shader_Clipped_Additive == null)			{ targetSet._shaderPath_Clipped_Additive =			shaderPath_Clipped_Additive; }
			if(targetSet._shader_Clipped_SoftAdditive == null)		{ targetSet._shaderPath_Clipped_SoftAdditive =		shaderPath_Clipped_SoftAdditive; }
			if(targetSet._shader_Clipped_Multiplicative == null)	{ targetSet._shaderPath_Clipped_Multiplicative =	shaderPath_Clipped_Multiplicative; }

			if(targetSet._shader_L_Normal_AlphaBlend == null)		{ targetSet._shaderPath_L_Normal_AlphaBlend =		shaderPath_L_Normal_AlphaBlend; }
			if(targetSet._shader_L_Normal_Additive == null)			{ targetSet._shaderPath_L_Normal_Additive =			shaderPath_L_Normal_Additive; }
			if(targetSet._shader_L_Normal_SoftAdditive == null)		{ targetSet._shaderPath_L_Normal_SoftAdditive =		shaderPath_L_Normal_SoftAdditive; }
			if(targetSet._shader_L_Normal_Multiplicative == null)	{ targetSet._shaderPath_L_Normal_Multiplicative =	shaderPath_L_Normal_Multiplicative; }

			if(targetSet._shader_L_Clipped_AlphaBlend == null)		{ targetSet._shaderPath_L_Clipped_AlphaBlend =		shaderPath_L_Clipped_AlphaBlend; }
			if(targetSet._shader_L_Clipped_Additive == null)		{ targetSet._shaderPath_L_Clipped_Additive =		shaderPath_L_Clipped_Additive; }
			if(targetSet._shader_L_Clipped_SoftAdditive == null)	{ targetSet._shaderPath_L_Clipped_SoftAdditive =	shaderPath_L_Clipped_SoftAdditive; }
			if(targetSet._shader_L_Clipped_Multiplicative == null)	{ targetSet._shaderPath_L_Clipped_Multiplicative =	shaderPath_L_Clipped_Multiplicative; }

			if(targetSet._shader_AlphaMask == null)		{ targetSet._shaderPath_AlphaMask =	shaderPath_AlphaMask; }
		}
		




		public apMaterialSet AddNewPreset(apMaterialSet srcMatSet, bool isFromPreset, string name)
		{
			int newID = GetNewCustomID();
			if (newID < 0)
			{
				return null;
			}

			if (string.IsNullOrEmpty(name))
			{
				name = "<No Name>";
			}

			apMaterialSet matSet = new apMaterialSet();
			
			if(srcMatSet != null)
			{
				matSet.CopyFromSrc(srcMatSet, newID, isFromPreset, true, false);
				matSet._name = name;
			}
			else
			{
				matSet.Init();

				//초기화 후 ID와 이름 설정
				matSet._uniqueID = newID;
				matSet._name = name;

				matSet.AddProperty("_Color", true, apMaterialSet.SHADER_PROP_TYPE.Color);
				matSet.AddProperty("_MainTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				matSet.AddProperty("_MaskTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				matSet.AddProperty("_MaskScreenSpaceOffset", true, apMaterialSet.SHADER_PROP_TYPE.Vector);
			}
			
			_presets.Add(matSet);

			
			Save();

			return matSet;
		}

		public void RemovePreset(int targetUniqueID)
		{
			//apPhysicsPresetUnit targetUnit = GetPresetUnit(targetUniqueID);
			//if (targetUnit == null)
			//{
			//	return;
			//}
			//if (targetUnit._isReserved)
			//{
			//	return;
			//}

			//_units.Remove(targetUnit);
			Save();

		}


		private int GetNewCustomID()
		{
			//사용자 ID는 100부터 시작
			//랜덤값으로 만듭시다.
			//랜덤은 1000부터 99999
			//에러시 ID는 200부터 999
			int cnt = 0;
			while (true)
			{
				int nextID = UnityEngine.Random.Range(1000, 999999);
				if (GetPresetUnit(nextID) == null)
				{
					//엥 없네영. 사용 가능
					return nextID;
				}

				cnt++;
				if(cnt > 100)
				{
					//100번이 되도록 ID가 중복이 되었다구요?
					break;
				}
			}
			//최대 999
			//200부터 하나씩 돌면서 체크
			for (int iCurID = 200; iCurID <= 999; iCurID++)
			{
				int nextID = iCurID;
				if (GetPresetUnit(nextID) == null)
				{
					//엥 없네영. 사용 가능
					return nextID;
				}
			}
			return -1;//ID 얻기 실패
		}



		// 외부에서 추가된 Shader로 Reserved Preset 만들기
		//------------------------------------------------------------------------------------------
		/// <summary>
		/// Advanced Shader들이 포함된 프리셋을 만든다.
		/// 인자 : true이면 이미 등록된 경우에만 갱신, false이면 강제로 만든다.
		/// </summary>
		/// <param name="makeWhenAlreadyAdded">true이면 이미 등록된 경우에만 갱신, false이면 강제로 만든다.</param>
		public void MakeReserved_Advanced(bool makeWhenAlreadyAdded)
		{
			// 2. Lit
			if ((!makeWhenAlreadyAdded) || (GetPresetUnit(RESERVED_MAT_ID__Lit) != null))
			{
				//Lit를 추가하거나 갱신할 수 있을 때
				apMaterialSet matSet_Lit = MakeReservedPreset(RESERVED_MAT_ID__Lit, "Lit", apMaterialSet.ICON.Lit,
												"Advanced/Lit/apShader_Lit_T_Alpha",
												"Advanced/Lit/apShader_Lit_T_Add",
												"Advanced/Lit/apShader_Lit_T_Soft",
												"Advanced/Lit/apShader_Lit_T_Mul",
												"Advanced/Lit/apShader_Lit_C_Alpha",
												"Advanced/Lit/apShader_Lit_C_Add",
												"Advanced/Lit/apShader_Lit_C_Soft",
												"Advanced/Lit/apShader_Lit_C_Mul",
												"Advanced/Lit/Linear/apShader_L_Lit_T_Alpha",
												"Advanced/Lit/Linear/apShader_L_Lit_T_Add",
												"Advanced/Lit/Linear/apShader_L_Lit_T_Soft",
												"Advanced/Lit/Linear/apShader_L_Lit_T_Mul",
												"Advanced/Lit/Linear/apShader_L_Lit_C_Alpha",
												"Advanced/Lit/Linear/apShader_L_Lit_C_Add",
												"Advanced/Lit/Linear/apShader_L_Lit_C_Soft",
												"Advanced/Lit/Linear/apShader_L_Lit_C_Mul",
												"apShader_AlphaMask",
												false);

				if(matSet_Lit != null)
				{
					matSet_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			// 3. Bumped
			if ((!makeWhenAlreadyAdded) || (GetPresetUnit(RESERVED_MAT_ID__Bumped) != null))
			{
				//Bumped를 추가하거나 갱신할 수 있을 때
				apMaterialSet matSet_Bumped = MakeReservedPreset(RESERVED_MAT_ID__Bumped, "Bumped", apMaterialSet.ICON.Lit,
													"Advanced/Bumped/apShader_Bumped_T_Alpha",
													"Advanced/Bumped/apShader_Bumped_T_Add",
													"Advanced/Bumped/apShader_Bumped_T_Soft",
													"Advanced/Bumped/apShader_Bumped_T_Mul",
													"Advanced/Bumped/apShader_Bumped_C_Alpha",
													"Advanced/Bumped/apShader_Bumped_C_Add",
													"Advanced/Bumped/apShader_Bumped_C_Soft",
													"Advanced/Bumped/apShader_Bumped_C_Mul",
													"Advanced/Bumped/Linear/apShader_L_Bumped_T_Alpha",
													"Advanced/Bumped/Linear/apShader_L_Bumped_T_Add",
													"Advanced/Bumped/Linear/apShader_L_Bumped_T_Soft",
													"Advanced/Bumped/Linear/apShader_L_Bumped_T_Mul",
													"Advanced/Bumped/Linear/apShader_L_Bumped_C_Alpha",
													"Advanced/Bumped/Linear/apShader_L_Bumped_C_Add",
													"Advanced/Bumped/Linear/apShader_L_Bumped_C_Soft",
													"Advanced/Bumped/Linear/apShader_L_Bumped_C_Mul",
													"apShader_AlphaMask",
													false);

				if(matSet_Bumped != null)
				{
					matSet_Bumped.AddProperty_Texture("_BumpMap", false, false);
					matSet_Bumped.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}


			// 4. Bumped Specular
			if ((!makeWhenAlreadyAdded) || (GetPresetUnit(RESERVED_MAT_ID__Bumped_Specular) != null))
			{
				//Bumped Specular를 추가하거나 갱신할 수 있을 때
				apMaterialSet matSet_BumpedSpecular = MakeReservedPreset(RESERVED_MAT_ID__Bumped_Specular, "Bumped Specular", apMaterialSet.ICON.LitSpecular,
												"Advanced/BumpedSpecular/apShader_BumpSpec_T_Alpha",
												"Advanced/BumpedSpecular/apShader_BumpSpec_T_Add",
												"Advanced/BumpedSpecular/apShader_BumpSpec_T_Soft",
												"Advanced/BumpedSpecular/apShader_BumpSpec_T_Mul",
												"Advanced/BumpedSpecular/apShader_BumpSpec_C_Alpha",
												"Advanced/BumpedSpecular/apShader_BumpSpec_C_Add",
												"Advanced/BumpedSpecular/apShader_BumpSpec_C_Soft",
												"Advanced/BumpedSpecular/apShader_BumpSpec_C_Mul",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Alpha",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Add",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Soft",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_T_Mul",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Alpha",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Add",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Soft",
												"Advanced/BumpedSpecular/Linear/apShader_L_BumpSpec_C_Mul",
												"apShader_AlphaMask",
												false);

				if(matSet_BumpedSpecular != null)
				{
					matSet_BumpedSpecular.AddProperty_Texture("_BumpMap", false, false);
					matSet_BumpedSpecular.AddProperty("_SpecularPower", false, apMaterialSet.SHADER_PROP_TYPE.Float).SetFloat(5.0f);
					matSet_BumpedSpecular.AddProperty_Texture("_SpecularMap", false, false);
					matSet_BumpedSpecular.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}


			// 5. Bumped Specular Emission
			if ((!makeWhenAlreadyAdded) || (GetPresetUnit(RESERVED_MAT_ID__Bumped_Specular_Emissive) != null))
			{
				//Bumped Specular Emission를 추가하거나 갱신할 수 있을 때

				apMaterialSet matSet_BSE = MakeReservedPreset(RESERVED_MAT_ID__Bumped_Specular_Emissive, "Bumped Specular Emissison", apMaterialSet.ICON.LitSpecularEmission,
													"Advanced/BumpedSpecularEmission/apShader_BSE_T_Alpha",
													"Advanced/BumpedSpecularEmission/apShader_BSE_T_Add",
													"Advanced/BumpedSpecularEmission/apShader_BSE_T_Soft",
													"Advanced/BumpedSpecularEmission/apShader_BSE_T_Mul",
													"Advanced/BumpedSpecularEmission/apShader_BSE_C_Alpha",
													"Advanced/BumpedSpecularEmission/apShader_BSE_C_Add",
													"Advanced/BumpedSpecularEmission/apShader_BSE_C_Soft",
													"Advanced/BumpedSpecularEmission/apShader_BSE_C_Mul",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Alpha",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Add",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Soft",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_T_Mul",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Alpha",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Add",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Soft",
													"Advanced/BumpedSpecularEmission/Linear/apShader_L_BSE_C_Mul",
													"apShader_AlphaMask",
													false);

				if(matSet_BSE != null)
				{
					matSet_BSE.AddProperty_Texture("_BumpMap", false, false);
					matSet_BSE.AddProperty("_SpecularPower", false, apMaterialSet.SHADER_PROP_TYPE.Float).SetFloat(5.0f);
					matSet_BSE.AddProperty_Texture("_SpecularMap", false, false);
					matSet_BSE.AddProperty("_EmissionColor", false, apMaterialSet.SHADER_PROP_TYPE.Color).SetColor(Color.white);
					matSet_BSE.AddProperty_Texture("_EmissionMap", false, false);
					matSet_BSE.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}



			// 6. Bumped Rimlight
			if ((!makeWhenAlreadyAdded) || (GetPresetUnit(RESERVED_MAT_ID__Bumped_Rimlight) != null))
			{
				//Bumped Rimlight를 추가하거나 갱신할 수 있을 때
				apMaterialSet matSet_BRimlight = MakeReservedPreset(RESERVED_MAT_ID__Bumped_Rimlight, "Bumped Rimlight", apMaterialSet.ICON.LitRimlight,
													"Advanced/BumpedRimlight/apShader_BumpRim_T_Alpha",
													"Advanced/BumpedRimlight/apShader_BumpRim_T_Add",
													"Advanced/BumpedRimlight/apShader_BumpRim_T_Soft",
													"Advanced/BumpedRimlight/apShader_BumpRim_T_Mul",
													"Advanced/BumpedRimlight/apShader_BumpRim_C_Alpha",
													"Advanced/BumpedRimlight/apShader_BumpRim_C_Add",
													"Advanced/BumpedRimlight/apShader_BumpRim_C_Soft",
													"Advanced/BumpedRimlight/apShader_BumpRim_C_Mul",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Alpha",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Add",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Soft",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_T_Mul",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Alpha",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Add",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Soft",
													"Advanced/BumpedRimlight/Linear/apShader_L_BumpRim_C_Mul",
													"apShader_AlphaMask",
													false);

				if(matSet_BRimlight != null)
				{
					matSet_BRimlight.AddProperty_Texture("_BumpMap", false, false);
					matSet_BRimlight.AddProperty("_RimPower", false, apMaterialSet.SHADER_PROP_TYPE.Float).SetFloat(2.0f);
					matSet_BRimlight.AddProperty("_RimColor", false, apMaterialSet.SHADER_PROP_TYPE.Color).SetColor(Color.white);
					matSet_BRimlight.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}


			// 7. Bumped Ramp
			if ((!makeWhenAlreadyAdded) || (GetPresetUnit(RESERVED_MAT_ID__Bumped_Ramp) != null))
			{
				//Bumped Ramp를 추가하거나 갱신할 수 있을 때
				apMaterialSet matSet_BumpRamp = MakeReservedPreset(RESERVED_MAT_ID__Bumped_Ramp, "Bumped Ramp", apMaterialSet.ICON.LitRamp,
													"Advanced/BumpedRamp/apShader_BumpRamp_T_Alpha",
													"Advanced/BumpedRamp/apShader_BumpRamp_T_Add",
													"Advanced/BumpedRamp/apShader_BumpRamp_T_Soft",
													"Advanced/BumpedRamp/apShader_BumpRamp_T_Mul",
													"Advanced/BumpedRamp/apShader_BumpRamp_C_Alpha",
													"Advanced/BumpedRamp/apShader_BumpRamp_C_Add",
													"Advanced/BumpedRamp/apShader_BumpRamp_C_Soft",
													"Advanced/BumpedRamp/apShader_BumpRamp_C_Mul",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Alpha",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Add",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Soft",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_T_Mul",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Alpha",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Add",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Soft",
													"Advanced/BumpedRamp/Linear/apShader_L_BumpRamp_C_Mul",
													"apShader_AlphaMask",
													false);

				if(matSet_BumpRamp != null)
				{
					matSet_BumpRamp.AddProperty_Texture("_BumpMap", false, false);
					matSet_BumpRamp.AddProperty_Texture("_RampMap", false, true);
					matSet_BumpRamp.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}
		}

		public bool MakeReserved_LWRPUnlit(bool isCheckShader)
		{
			//필요한 Shader가 모두 존재하는지 확인
			if (isCheckShader)
			{
				if (!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_AlphaMask") ||

					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Alpha") ||
					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Additive") ||
					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_T_SoftAdditive") ||
					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Multiplicative") ||
					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Alpha") ||
					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Additive") ||
					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_C_SoftAdditive") ||
					!IsShaderImported("Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Multiplicative") ||

					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Alpha") ||
					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Additive") ||
					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_SoftAdditive") ||
					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Multiplicative") ||
					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Alpha") ||
					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Additive") ||
					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_SoftAdditive") ||
					!IsShaderImported("Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Multiplicative")
					)
				{
					//하나라도 존재하지 않으면 False
					return false;
				}
			}

			apMaterialSet matSet_LWRP_Unlit = MakeReservedPreset(RESERVED_MAT_ID__LWRP_Unlit, "LWRP Unlit", apMaterialSet.ICON.Unlit, 
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Alpha",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Additive",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_SoftAdditive",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_T_Multiplicative",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Alpha",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Additive",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_SoftAdditive",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_C_Multiplicative",

								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Alpha",
								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Additive",
								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_SoftAdditive",
								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_T_Multiplicative",
								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Alpha",
								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Additive",
								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_SoftAdditive",
								"Advanced/LWRP Unlit/Linear/apShader_L_LWRPUnlit_C_Multiplicative",
								"Advanced/LWRP Unlit/apShader_LWRPUnlit_AlphaMask",
								true);

			if(matSet_LWRP_Unlit != null)
			{
				matSet_LWRP_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
			}

			return true;
		}

		public bool MakeReserved_LWRP_2DLit(bool isCheckShader)
		{
			//필요한 Shader가 모두 존재하는지 확인
			if (isCheckShader)
			{
				if (!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_AlphaMask") ||

					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_AlphaBlend") ||
					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Additive") ||
					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_SoftAdditive") ||
					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Multiplicative") ||
					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_AlphaBlend") ||
					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Additive") ||
					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_SoftAdditive") ||
					!IsShaderImported("Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Multiplicative")
					)
				{
					//하나라도 존재하지 않으면 False
					return false;
				}
			}

			apMaterialSet matSet_LWRP_2DLit = MakeReservedPreset(RESERVED_MAT_ID__LWRP_2D_Lit, "LWRP 2D Lit", apMaterialSet.ICON.Lit, 
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_AlphaBlend",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Additive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_SoftAdditive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Multiplicative",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_AlphaBlend",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Additive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_SoftAdditive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Multiplicative",

								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_AlphaBlend",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Additive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_SoftAdditive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_T_Multiplicative",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_AlphaBlend",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Additive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_SoftAdditive",
								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_Lit_C_Multiplicative",

								"Advanced/LWRP 2D Lit (Experimental)/apShader_LWRP2D_AlphaMask",
								false);

			if(matSet_LWRP_2DLit != null)
			{
				matSet_LWRP_2DLit.CheckAndRemoveDuplicatedProperties();//중복 제거
			}

			return true;
		}


		public void MakeReserved_VR(bool isCheckShader)
		{
			//Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__VR_Unlit) != null))
			{
				apMaterialSet matSet_VR_Unlit = MakeReservedPreset(RESERVED_MAT_ID__VR_Unlit, "VR Unlit", apMaterialSet.ICON.UnlitVR, 
								"Advanced/UnlitVR/apShader_UnlitVR_T_Alpha",
								"Advanced/UnlitVR/apShader_UnlitVR_T_Add",
								"Advanced/UnlitVR/apShader_UnlitVR_T_Soft",
								"Advanced/UnlitVR/apShader_UnlitVR_T_Mul",
								"Advanced/UnlitVR/apShader_UnlitVR_C_Alpha",
								"Advanced/UnlitVR/apShader_UnlitVR_C_Add",
								"Advanced/UnlitVR/apShader_UnlitVR_C_Soft",
								"Advanced/UnlitVR/apShader_UnlitVR_C_Mul",

								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Alpha",
								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Add",
								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Soft",
								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_T_Mul",
								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Alpha",
								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Add",
								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Soft",
								"Advanced/UnlitVR/Linear/apShader_L_UnlitVR_C_Mul",

								"apShader_AlphaMask", 
								true,
								true);

				if(matSet_VR_Unlit != null)
				{
					matSet_VR_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}
			
			//Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__VR_Lit) != null))
			{
				apMaterialSet matSet_VR_Lit = MakeReservedPreset(RESERVED_MAT_ID__VR_Lit, "VR Lit", apMaterialSet.ICON.LitVR, 
								"Advanced/LitVR/apShader_LitVR_T_Alpha",
								"Advanced/LitVR/apShader_LitVR_T_Add",
								"Advanced/LitVR/apShader_LitVR_T_Soft",
								"Advanced/LitVR/apShader_LitVR_T_Mul",
								"Advanced/LitVR/apShader_LitVR_C_Alpha",
								"Advanced/LitVR/apShader_LitVR_C_Add",
								"Advanced/LitVR/apShader_LitVR_C_Soft",
								"Advanced/LitVR/apShader_LitVR_C_Mul",

								"Advanced/LitVR/Linear/apShader_L_LitVR_T_Alpha",
								"Advanced/LitVR/Linear/apShader_L_LitVR_T_Add",
								"Advanced/LitVR/Linear/apShader_L_LitVR_T_Soft",
								"Advanced/LitVR/Linear/apShader_L_LitVR_T_Mul",
								"Advanced/LitVR/Linear/apShader_L_LitVR_C_Alpha",
								"Advanced/LitVR/Linear/apShader_L_LitVR_C_Add",
								"Advanced/LitVR/Linear/apShader_L_LitVR_C_Soft",
								"Advanced/LitVR/Linear/apShader_L_LitVR_C_Mul",

								"apShader_AlphaMask", 
								false,
								true);

				if(matSet_VR_Lit != null)
				{
					matSet_VR_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//Unlit (with ScaleOffset)
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__VR_Unlit_ScaleOffset) != null))
			{
				apMaterialSet matSet_VR_UnlitScaleOffset = MakeReservedPreset(RESERVED_MAT_ID__VR_Unlit_ScaleOffset, "VR Unlit (with ScaleOffset)", apMaterialSet.ICON.UnlitVR, 
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Alpha",
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Add",
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Soft",
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_T_Mul",
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Alpha",
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Add",
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Soft",
								"Advanced/UnlitVRWithScaleOffset/apShader_UnlitVRSO_C_Mul",

								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Alpha",
								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Add",
								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Soft",
								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_T_Mul",
								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Alpha",
								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Add",
								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Soft",
								"Advanced/UnlitVRWithScaleOffset/Linear/apShader_L_UnlitVRSO_C_Mul",

								"apShader_AlphaMask", 
								true,
								true);

				if(matSet_VR_UnlitScaleOffset != null)
				{
					matSet_VR_UnlitScaleOffset.CheckAndRemoveDuplicatedProperties();
				}
			}
			
			//Lit (with ScaleOffset)
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__VR_Lit_ScaleOffset) != null))
			{
				apMaterialSet matSet_VR_LitScaleOffset = MakeReservedPreset(RESERVED_MAT_ID__VR_Lit_ScaleOffset, "VR Lit (with ScaleOffset)", apMaterialSet.ICON.LitVR, 
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Alpha",
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Add",
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Soft",
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_T_Mul",
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Alpha",
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Add",
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Soft",
								"Advanced/LitVRWithScaleOffset/apShader_LitVRSO_C_Mul",

								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Alpha",
								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Add",
								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Soft",
								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_T_Mul",
								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Alpha",
								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Add",
								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Soft",
								"Advanced/LitVRWithScaleOffset/Linear/apShader_L_LitVRSO_C_Mul",

								"apShader_AlphaMask", 
								false,
								true);

				if(matSet_VR_LitScaleOffset != null)
				{
					matSet_VR_LitScaleOffset.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}
		}


		//추가 19.10.27 : RenderTexture를 카메라의 렌더 타겟으로 지정하는 경우, 이 재질을 이용해야한다.
		public void MakeReserved_KeepAlpha(bool isCheckShader)
		{
			//Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__KeepAlpha_Unlit) != null))
			{
				apMaterialSet matSet_KeepAlpha_Unlit = MakeReservedPreset(RESERVED_MAT_ID__KeepAlpha_Unlit, "KeepAlpha Unlit", apMaterialSet.ICON.Unlit, 
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Alpha",
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Add",
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Soft",
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_T_Mul",
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Alpha",
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Add",
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Soft",
								"Advanced/KeepAlphaUnlit/apShader_KAUnlit_C_Mul",

								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Alpha",
								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Add",
								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Soft",
								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_T_Mul",
								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Alpha",
								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Add",
								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Soft",
								"Advanced/KeepAlphaUnlit/Linear/apShader_L_KAUnlit_C_Mul",

								"apShader_AlphaMask", 
								true);

				if(matSet_KeepAlpha_Unlit != null)
				{
					matSet_KeepAlpha_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}
			
			//Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__KeepAlpha_Lit) != null))
			{
				apMaterialSet matSet_KeepAlpha_Lit = MakeReservedPreset(RESERVED_MAT_ID__KeepAlpha_Lit, "KeepAlpha Lit", apMaterialSet.ICON.Lit, 
								"Advanced/KeepAlphaLit/apShader_KALit_T_Alpha",
								"Advanced/KeepAlphaLit/apShader_KALit_T_Add",
								"Advanced/KeepAlphaLit/apShader_KALit_T_Soft",
								"Advanced/KeepAlphaLit/apShader_KALit_T_Mul",
								"Advanced/KeepAlphaLit/apShader_KALit_C_Alpha",
								"Advanced/KeepAlphaLit/apShader_KALit_C_Add",
								"Advanced/KeepAlphaLit/apShader_KALit_C_Soft",
								"Advanced/KeepAlphaLit/apShader_KALit_C_Mul",

								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Alpha",
								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Add",
								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Soft",
								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_T_Mul",
								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Alpha",
								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Add",
								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Soft",
								"Advanced/KeepAlphaLit/Linear/apShader_L_KALit_C_Mul",

								"apShader_AlphaMask", 
								false);

				if(matSet_KeepAlpha_Lit != null)
				{
					matSet_KeepAlpha_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}
			
		}


		//추가 22.1.5 : 병합 가능한 재질들
		public void MakeReserved_Mergeable(bool isCheckShader)
		{
			//Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__Mergeable_Unlit) != null))
			{
				apMaterialSet matSet_Unlit = MakeReservedPreset(RESERVED_MAT_ID__Mergeable_Unlit, "Mergeable Unlit", apMaterialSet.ICON.UnlitMergeable,
								"Advanced/Mergeable/Unlit/apShader_Mergeable_Unlit_T_Alpha",//Merged
								
								"apShader_Transparent_Additive",
								"apShader_Transparent_SoftAdditive",
								"apShader_Transparent_Multiplicative",
								"apShader_ClippedWithMask",
								"apShader_ClippedWithMask_Additive",
								"apShader_ClippedWithMask_SoftAdditive",
								"apShader_ClippedWithMask_Multiplicative",
								
								"Advanced/Mergeable/Unlit/Linear/apShader_L_Mergeable_Unlit_T_Alpha",////Merged

								"Linear/apShader_L_Transparent_Additive",
								"Linear/apShader_L_Transparent_SoftAdditive",
								"Linear/apShader_L_Transparent_Multiplicative",
								"Linear/apShader_L_ClippedWithMask",
								"Linear/apShader_L_ClippedWithMask_Additive",
								"Linear/apShader_L_ClippedWithMask_SoftAdditive",
								"Linear/apShader_L_ClippedWithMask_Multiplicative",
								"apShader_AlphaMask",
								true);

				//프로퍼티 추가
				if(matSet_Unlit != null)
				{
					matSet_Unlit.AddProperty_Texture("_MergedTex1", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex2", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex3", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex4", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex5", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex6", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex7", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex8", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex9", true, true);

					matSet_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__Mergeable_Lit) != null))
			{
				apMaterialSet matSet_Lit = MakeReservedPreset(RESERVED_MAT_ID__Mergeable_Lit, "Mergeable Lit", apMaterialSet.ICON.LitMergeable,
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Alpha",
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Add",//Add
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Soft",//SoftAdd
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_T_Mul",//Mul
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Alpha",
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Add",//C-Add
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Soft",//C-SoftAdd
								"Advanced/Mergeable/Lit/apShader_Mergeable_Lit_C_Mul",//C-Mul
								
								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Alpha",

								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Add",//Add
								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Soft",//SoftAdd
								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_T_Mul",//Mul
								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Alpha",
								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Add",//C-Add
								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Soft",//C-SoftAdd
								"Advanced/Mergeable/Lit/Linear/apShader_L_Mergeable_Lit_C_Mul",//C-Mul
								"apShader_AlphaMask",
								false);

				if(matSet_Lit != null)
				{
					//프로퍼티 추가
					matSet_Lit.AddProperty_Texture("_MergedTex1", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex2", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex3", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex4", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex5", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex6", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex7", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex8", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex9", true, true);

					matSet_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}
		}


		//추가 20.1.24 : 유니티 2019.3에 추가된 URP를 등록하자.
		//Shader가 아닌 Shader Graph를 사용한다는 점이 차이점이다.
		public void MakeReserved_URP(bool isCheckShader)
		{
			//Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP_Unlit) != null))
			{
				apMaterialSet matSet_URP_Unlit = MakeReservedPreset(RESERVED_MAT_ID__URP_Unlit, "URP Unlit", apMaterialSet.ICON.Unlit, 
								"Advanced/URP Unlit/apShaderGraph_URP_Unlit_T_Alpha",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Multiplicative",
								"Advanced/URP Unlit/apShaderGraph_URP_Unlit_C_Alpha",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Multiplicative",

								"Advanced/URP Unlit/Linear/apShaderGraph_L_URP_Unlit_T_Alpha",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Multiplicative",
								"Advanced/URP Unlit/Linear/apShaderGraph_L_URP_Unlit_C_Alpha",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Multiplicative",

								"Advanced/URP Common/apShaderGraph_URP_Common_AlphaMask", 
								true, 
								false, 
								true);//<<Shader Graph를 이용한다.

				if(matSet_URP_Unlit != null)
				{
					matSet_URP_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}
			
			//Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP_Lit) != null))
			{
				apMaterialSet matSet_URP_Lit = MakeReservedPreset(RESERVED_MAT_ID__URP_Lit, "URP Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP Lit/apShaderGraph_URP_Lit_T_Alpha",
								"Advanced/URP Common/apShaderGraph_URP_Common_Lit_T_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Lit_T_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Multiplicative",
								"Advanced/URP Lit/apShaderGraph_URP_Lit_C_Alpha",
								"Advanced/URP Common/apShaderGraph_URP_Common_Lit_C_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Lit_C_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Multiplicative",

								"Advanced/URP Lit/Linear/apShaderGraph_L_URP_Lit_T_Alpha",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_T_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_T_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Multiplicative",
								"Advanced/URP Lit/Linear/apShaderGraph_L_URP_Lit_C_Alpha",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_C_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Lit_C_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Multiplicative",

								"Advanced/URP Common/apShaderGraph_URP_Common_AlphaMask", 
								false,
								false, 
								true);//<<Shader Graph를 이용한다.

				if(matSet_URP_Lit != null)
				{
					matSet_URP_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//2D Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP_2DLit) != null))
			{
				apMaterialSet matSet_URP_2DLit = MakeReservedPreset(RESERVED_MAT_ID__URP_2DLit, "URP 2D Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP 2D Lit (Experimental)/apShaderGraph_URP_2DLit_T_Alpha",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_T_Multiplicative",
								"Advanced/URP 2D Lit (Experimental)/apShaderGraph_URP_2DLit_C_Alpha",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/apShaderGraph_URP_Common_Unlit_C_Multiplicative",

								"Advanced/URP 2D Lit (Experimental)/Linear/apShaderGraph_L_URP_2DLit_T_Alpha",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_T_Multiplicative",
								"Advanced/URP 2D Lit (Experimental)/Linear/apShaderGraph_L_URP_2DLit_C_Alpha",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Additive",
								"Advanced/URP Common/Linear/apShaderGraph_L_URP_Common_Unlit_C_Multiplicative",

								"Advanced/URP Common/apShaderGraph_URP_Common_AlphaMask", 
								false,
								false, 
								true);//<<Shader Graph를 이용한다.

				if(matSet_URP_2DLit != null)
				{
					matSet_URP_2DLit.CheckAndRemoveDuplicatedProperties();//중복 체크
				}
			}
			
		}






		//추가 21.12.20 : URP 21 세트를 만든다.
		public void MakeReserved_URP21(bool isCheckShader)
		{
			//Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP21_Unlit) != null))
			{
				apMaterialSet matSet_URP21_Unlit = MakeReservedPreset(RESERVED_MAT_ID__URP21_Unlit, "URP (2021) Unlit", apMaterialSet.ICON.Unlit, 
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Alpha",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Alpha",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Alpha",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Alpha",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask", 
								true, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_URP21_Unlit != null)
				{
					matSet_URP21_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP21_Lit) != null))
			{
				apMaterialSet matSet_URP21_Lit = MakeReservedPreset(RESERVED_MAT_ID__URP21_Lit, "URP (2021) Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2021)/Lit/apShaderGraph_URP21_Lit_T_Alpha",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
								"Advanced/URP (2021)/Lit/apShaderGraph_URP21_Lit_C_Alpha",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Lit/Linear/apShaderGraph_URP21_L_Lit_T_Alpha",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

								"Advanced/URP (2021)/Lit/Linear/apShaderGraph_URP21_L_Lit_C_Alpha",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_URP21_Lit != null)
				{
					matSet_URP21_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//Bumped Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP21_BumpedLit) != null))
			{
				apMaterialSet matSet_Bumped = MakeReservedPreset(RESERVED_MAT_ID__URP21_BumpedLit, "URP (2021) Bumped Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2021)/Bumped Lit/apShaderGraph_URP21_BumpedLit_T_Alpha",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
								"Advanced/URP (2021)/Bumped Lit/apShaderGraph_URP21_BumpedLit_C_Alpha",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Bumped Lit/Linear/apShaderGraph_URP21_L_BumpedLit_T_Alpha",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

								"Advanced/URP (2021)/Bumped Lit/Linear/apShaderGraph_URP21_L_BumpedLit_C_Alpha",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Bumped != null)
				{
					//노멀맵 텍스쳐 추가
					matSet_Bumped.AddProperty_Texture("_BumpMap", false, false);
					matSet_Bumped.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}
			
			//2D Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP21_2DLit) != null))
			{
				apMaterialSet matSet_URP21_2DLit = MakeReservedPreset(RESERVED_MAT_ID__URP21_2DLit, "URP (2021) 2D Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Alpha",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Alpha",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Alpha",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Alpha",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_URP21_2DLit != null)
				{
					matSet_URP21_2DLit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//2D Bumped Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP21_2DBumpedLit) != null))
			{
				apMaterialSet matSet_Bumped = MakeReservedPreset(RESERVED_MAT_ID__URP21_2DBumpedLit, "URP (2021) 2D Bumped Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2021)/2D Bumped Lit/apShaderGraph_URP21_2DBumpedLit_T_Alpha",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
								"Advanced/URP (2021)/2D Bumped Lit/apShaderGraph_URP21_2DBumpedLit_C_Alpha",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/2D Bumped Lit/Linear/apShaderGraph_URP21_L_2DBumpedLit_T_Alpha",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

								"Advanced/URP (2021)/2D Bumped Lit/Linear/apShaderGraph_URP21_L_2DBumpedLit_C_Alpha",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Bumped != null)
				{
					//노멀맵 텍스쳐 추가
					matSet_Bumped.AddProperty_Texture("_BumpMap", false, false);
					matSet_Bumped.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}
		
			
			//Mergeable Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP21_MergeableUnlit) != null))
			{
				apMaterialSet matSet_Unlit = MakeReservedPreset(RESERVED_MAT_ID__URP21_MergeableUnlit, "URP (2021) Mergeable Unlit", apMaterialSet.ICON.UnlitMergeable, 
								"Advanced/URP (2021)/Mergeable Unlit/apShaderGraph_URP21_MergeableUnlit_T_Alpha",//Merged
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Alpha",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Mergeable Unlit/Linear/apShaderGraph_URP21_L_MergeableUnlit_T_Alpha",//Merged
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Alpha",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask", 
								true, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Unlit != null)
				{
					//프로퍼티 추가
					matSet_Unlit.AddProperty_Texture("_MergedTex1", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex2", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex3", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex4", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex5", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex6", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex7", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex8", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex9", true, true);

					matSet_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}

			//Mergeable Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP21_Mergeable2DLit) != null))
			{
				apMaterialSet matSet_Lit = MakeReservedPreset(RESERVED_MAT_ID__URP21_Mergeable2DLit, "URP (2021) Mergeable 2D Lit", apMaterialSet.ICON.LitMergeable, 
								"Advanced/URP (2021)/Mergeable 2D Lit/apShaderGraph_URP21_Mergeable2DLit_T_Alpha",//Merged
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_T_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_T_Multiplicative",
								
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Alpha",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
								"Advanced/URP (2021)/2D Lit/apShaderGraph_URP21_2DLit_C_Additive",
								"Advanced/URP (2021)/Unlit/apShaderGraph_URP21_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Mergeable 2D Lit/Linear/apShaderGraph_URP21_L_Mergeable2DLit_T_Alpha",//Merged
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_T_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_T_Multiplicative",

								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Alpha",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
								"Advanced/URP (2021)/2D Lit/Linear/apShaderGraph_URP21_L_2DLit_C_Additive",
								"Advanced/URP (2021)/Unlit/Linear/apShaderGraph_URP21_L_Unlit_C_Multiplicative",

								"Advanced/URP (2021)/Common/apShaderGraph_URP21_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Lit != null)
				{
					//프로퍼티 추가
					matSet_Lit.AddProperty_Texture("_MergedTex1", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex2", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex3", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex4", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex5", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex6", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex7", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex8", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex9", true, true);

					matSet_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}
		}


		//추가 21.12.20 : URP 23 세트를 만든다.
		public void MakeReserved_URP23(bool isCheckShader)
		{
			//Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP23_Unlit) != null))
			{
				apMaterialSet matSet_URP23_Unlit = MakeReservedPreset(RESERVED_MAT_ID__URP23_Unlit, "URP (2023) Unlit", apMaterialSet.ICON.Unlit, 
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask", 
								true, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_URP23_Unlit != null)
				{
					matSet_URP23_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP23_Lit) != null))
			{
				apMaterialSet matSet_URP23_Lit = MakeReservedPreset(RESERVED_MAT_ID__URP23_Lit, "URP (2023) Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2023)/Lit/apShaderGraph_URP23_Lit_T_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Lit/apShaderGraph_URP23_Lit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Lit/Linear/apShaderGraph_URP23_L_Lit_T_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Lit/Linear/apShaderGraph_URP23_L_Lit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_URP23_Lit != null)
				{
					matSet_URP23_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//Bumped Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP23_BumpedLit) != null))
			{
				apMaterialSet matSet_Bumped = MakeReservedPreset(RESERVED_MAT_ID__URP23_BumpedLit, "URP (2023) Bumped Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2023)/Bumped Lit/apShaderGraph_URP23_BumpedLit_T_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Bumped Lit/apShaderGraph_URP23_BumpedLit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Bumped Lit/Linear/apShaderGraph_URP23_L_BumpedLit_T_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Bumped Lit/Linear/apShaderGraph_URP23_L_BumpedLit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Bumped != null)
				{
					//노멀맵 텍스쳐 추가
					matSet_Bumped.AddProperty_Texture("_BumpMap", false, false);

					matSet_Bumped.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}
			
			//2D Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP23_2DLit) != null))
			{
				apMaterialSet matSet_2DLit = MakeReservedPreset(RESERVED_MAT_ID__URP23_2DLit, "URP (2023) 2D Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_2DLit != null)
				{
					matSet_2DLit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
			}

			//2D Bumped Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP23_2DBumpedLit) != null))
			{
				apMaterialSet matSet_Bumped = MakeReservedPreset(RESERVED_MAT_ID__URP23_2DBumpedLit, "URP (2023) 2D Bumped Lit", apMaterialSet.ICON.Lit, 
								"Advanced/URP (2023)/2D Bumped Lit/apShaderGraph_URP23_2DBumpedLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/2D Bumped Lit/apShaderGraph_URP23_2DBumpedLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/2D Bumped Lit/Linear/apShaderGraph_URP23_L_2DBumpedLit_T_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/2D Bumped Lit/Linear/apShaderGraph_URP23_L_2DBumpedLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Bumped != null)
				{
					//노멀맵 텍스쳐 추가
					matSet_Bumped.AddProperty_Texture("_BumpMap", false, false);

					matSet_Bumped.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}
		
			
			//Mergeable Unlit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP23_MergeableUnlit) != null))
			{
				apMaterialSet matSet_Unlit = MakeReservedPreset(RESERVED_MAT_ID__URP23_MergeableUnlit, "URP (2023) Mergeable Unlit", apMaterialSet.ICON.UnlitMergeable, 
								"Advanced/URP (2023)/Mergeable Unlit/apShaderGraph_URP23_MergeableUnlit_T_Alpha",//Merged
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Mergeable Unlit/Linear/apShaderGraph_URP23_L_MergeableUnlit_T_Alpha",//Merged
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Alpha",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask", 
								true, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Unlit != null)
				{
					//프로퍼티 추가
					matSet_Unlit.AddProperty_Texture("_MergedTex1", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex2", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex3", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex4", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex5", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex6", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex7", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex8", true, true);
					matSet_Unlit.AddProperty_Texture("_MergedTex9", true, true);

					matSet_Unlit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}

			//Mergeable Lit
			if ((!isCheckShader) || (GetPresetUnit(RESERVED_MAT_ID__URP23_Mergeable2DLit) != null))
			{
				apMaterialSet matSet_Lit = MakeReservedPreset(RESERVED_MAT_ID__URP23_Mergeable2DLit, "URP (2023) Mergeable 2D Lit", apMaterialSet.ICON.LitMergeable, 
								"Advanced/URP (2023)/Mergeable 2D Lit/apShaderGraph_URP23_Mergeable2DLit_T_Alpha",//Merged
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_T_Multiplicative",
								
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/apShaderGraph_URP23_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/apShaderGraph_URP23_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Mergeable 2D Lit/Linear/apShaderGraph_URP23_L_Mergeable2DLit_T_Alpha",//Merged
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_T_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_T_Multiplicative",

								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Alpha",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/2D Lit/Linear/apShaderGraph_URP23_L_2DLit_C_Additive",
								"Advanced/URP (2023)/Unlit/Linear/apShaderGraph_URP23_L_Unlit_C_Multiplicative",

								"Advanced/URP (2023)/Common/apShaderGraph_URP23_Common_AlphaMask", 
								false, //빛 제거 필요(Unlit인 경우 True)
								false, //VR
								true);//Shader Graph 유무

				if(matSet_Lit != null)
				{
					//프로퍼티 추가
					matSet_Lit.AddProperty_Texture("_MergedTex1", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex2", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex3", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex4", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex5", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex6", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex7", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex8", true, true);
					matSet_Lit.AddProperty_Texture("_MergedTex9", true, true);

					matSet_Lit.CheckAndRemoveDuplicatedProperties();//중복 제거
				}
				
			}
		}



		private bool IsShaderImported(string shaderPath, bool isUseShaderGraph = false)
		{
			//이전
			//string fullPath = "Assets/AnyPortrait/Assets/Shaders/" + shaderPath + (!isUseShaderGraph ? ".shader" : ".shadergraph");

			//변경
			string fullPath = _basePath + "Assets/Shaders/" + shaderPath + (!isUseShaderGraph ? ".shader" : ".shadergraph");

			bool result = (AssetDatabase.LoadAssetAtPath<Shader>(fullPath) != null);
			//if(!result)
			//{
			//	Debug.LogError("Shader [" + shaderPath + "] is not imported");
			//}
			return result;
		}




		// Save / Load
		//------------------------------------------------------------------------------------------
		public void Save()
		{
			FileStream fs = null;
			StreamWriter sw = null;

			string filePath = Application.dataPath + "/../AnyPortrait_MaterialLibrary.txt";

			//Debug.Log("Save MaterialLibrary Presets");

			try
			{
				MakeReservedPresets();

				//이전
				//fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제 (UTF8 강제)
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				for (int i = 0; i < _presets.Count; i++)
				{
					_presets[i].Save(sw);

					if (i < _presets.Count - 1)
					{
						sw.WriteLine("--");//구분자
					}
				}

				sw.Flush();

				if(sw != null)
				{
					sw.Close();
					sw = null;
				}
				if(fs != null)
				{
					fs.Close();
					fs = null;
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("MaterialLibrary Save Exception : " + ex);

				if(sw != null)
				{
					sw.Close();
					sw = null;
				}
				if(fs != null)
				{
					fs.Close();
					fs = null;
				}
			}
		}

		public void Load()
		{
			FileStream fs = null;
			StreamReader sr = null;

			//Debug.Log("Load MaterialLibrary Presets");


			_isFirstLoad = true;
			//string defaultPath = Application.dataPath;
			//string filePath = defaultPath.Substring(0, defaultPath.Length - 6) + "/AnyPortrait_MaterialLibrary.txt";
			string filePath = Application.dataPath + "/../AnyPortrait_MaterialLibrary.txt";

			try
			{
				
				ClearPresets();
				MakeReservedPresets();//Reserved가 추가되지 않았으면 자동으로 미리 추가하자

				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 문제 + 인코딩 문제
				filePath = apUtil.ConvertEscapeToPlainText(filePath);
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8);

				List<string> strData = new List<string>();
				//유효한 데이터를 긁어온 후,
				//구분자를 만나면>>
				//하나씩 Unit으로 만들어준다.
				//일단 새로 하나 생성 후, 로드한 뒤,
				//겹치는게 있으면... 패스 (Save를 먼저 하세요)

				while (true)
				{
					if (sr.Peek() < 0)
					{
						//남은게 있으면 이것도 처리
						if (strData.Count > 0)
						{
							apMaterialSet newUnit = new apMaterialSet();
							newUnit.Load(strData);

							if (newUnit._uniqueID < 0)
							{
								continue;
							}

							//이제 추가 가능한 데이터인지 확인하자
							if (GetPresetUnit(newUnit._uniqueID) == null)
							{
								//로드된 데이터로 에셋 추가 [v1.5.1]
								newUnit.LoadShaderAssets();
								newUnit.LoadRefMaterial();

								_presets.Add(newUnit);//추가!
							}
							strData.Clear();
						}
						break;
					}
					string strRead = sr.ReadLine();
					if (strRead.Length < 4)
					{
						//구분자를 만난 듯 하다.
						apMaterialSet newUnit = new apMaterialSet();
						newUnit.Load(strData);

						if (newUnit._uniqueID < 0)
						{
							continue;
						}

						//이제 추가 가능한 데이터인지 확인하자
						if (GetPresetUnit(newUnit._uniqueID) == null)
						{
							//로드된 데이터로 에셋 추가 [v1.5.1]
							newUnit.LoadShaderAssets();
							newUnit.LoadRefMaterial();

							_presets.Add(newUnit);//추가!
						}

						strData.Clear();
					}
					else
					{
						//데이터 누적
						strData.Add(strRead);
					}
				}



				if (sr != null)
				{
					sr.Close();
					sr = null;
				}

				if (fs != null)
				{
					fs.Close();
					fs = null;
				}
			}
			catch (Exception ex)
			{
				if (ex is FileNotFoundException)
				{

				}
				else
				{
					Debug.LogError("MaterialLibrary Load Exception : " + ex);
				}


				if (sr != null)
				{
					sr.Close();
					sr = null;
				}

				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				//일단 저장을 한번 더 하자 (파일이 없을 수 있음)
				Save();
			}

			
		}



		// Get / Set
		//------------------------------------------------------
		public apMaterialSet GetPresetUnit(int uniqueID)
		{
			//이전 (GC 발생)
			//return _presets.Find(delegate (apMaterialSet a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetPresetUnit_ID = uniqueID;
			return _presets.Find(s_GetPresetUnitByID_Func);
		}

		private static int s_GetPresetUnit_ID = -1;
		private static Predicate<apMaterialSet> s_GetPresetUnitByID_Func = FUNC_GetPresetUnitByID;
		private static bool FUNC_GetPresetUnitByID(apMaterialSet a)
		{
			return a._uniqueID == s_GetPresetUnit_ID;
		}




		public bool IsLoaded
		{
			get
			{
				return _isFirstLoad && _presets.Count > 0;
			}
		}

		//private List<apMaterialSet> _presets = new List<apMaterialSet>();
		//public List<apMaterialSet> Presets { get {  return _presets; } }

		//추가 v1.4.7 : 기본 프리셋을 리턴한다.
		//이전에는 0번째인 Unlit (Default > Legacy)였다면, 이젠 Unlit (v2)를 리턴한다.
		public apMaterialSet GetDefaultPreset()
		{
			if(_presets != null)
			{
				return GetPresetUnit(RESERVED_MAT_ID__Unlit_V2);
			}
			return null;
		}

		public PRESET_TYPE GetPresetType(apMaterialSet matSet)
		{
			if(matSet == null)
			{
				return PRESET_TYPE.NoneOrCustom;
			}

			switch (matSet._uniqueID)
			{
				//삭제 불가능한 프리셋 타입
				case RESERVED_MAT_ID__Unlit:
				case RESERVED_MAT_ID__Unlit_V2:
					return PRESET_TYPE.Reserved_NotRemovable;

				//삭제는 가능한 프리셋 타입
				case RESERVED_MAT_ID__Lit:
				case RESERVED_MAT_ID__Bumped:
				case RESERVED_MAT_ID__Bumped_Specular:
				case RESERVED_MAT_ID__Bumped_Specular_Emissive:
				case RESERVED_MAT_ID__Bumped_Rimlight:
				case RESERVED_MAT_ID__Bumped_Ramp:
				case RESERVED_MAT_ID__LWRP_Unlit:
				case RESERVED_MAT_ID__LWRP_2D_Lit:
				case RESERVED_MAT_ID__KeepAlpha_Unlit:
				case RESERVED_MAT_ID__KeepAlpha_Lit:
				case RESERVED_MAT_ID__VR_Unlit:
				case RESERVED_MAT_ID__VR_Lit:
				case RESERVED_MAT_ID__VR_Unlit_ScaleOffset:
				case RESERVED_MAT_ID__VR_Lit_ScaleOffset:
				case RESERVED_MAT_ID__Mergeable_Unlit:
				case RESERVED_MAT_ID__Mergeable_Lit:
				case RESERVED_MAT_ID__URP_Unlit:
				case RESERVED_MAT_ID__URP_Lit:
				case RESERVED_MAT_ID__URP_2DLit:
				case RESERVED_MAT_ID__URP21_Unlit:
				case RESERVED_MAT_ID__URP21_Lit:
				case RESERVED_MAT_ID__URP21_BumpedLit:
				case RESERVED_MAT_ID__URP21_2DLit:
				case RESERVED_MAT_ID__URP21_2DBumpedLit:
				case RESERVED_MAT_ID__URP21_MergeableUnlit:
				case RESERVED_MAT_ID__URP21_Mergeable2DLit:
				case RESERVED_MAT_ID__URP23_Unlit:
				case RESERVED_MAT_ID__URP23_Lit:
				case RESERVED_MAT_ID__URP23_BumpedLit:
				case RESERVED_MAT_ID__URP23_2DLit:
				case RESERVED_MAT_ID__URP23_2DBumpedLit:
				case RESERVED_MAT_ID__URP23_MergeableUnlit:
				case RESERVED_MAT_ID__URP23_Mergeable2DLit:
					return PRESET_TYPE.Reserved_Removable;
			}

			//프리셋이 아니거나 커스텀
			return PRESET_TYPE.NoneOrCustom;
		}
		
	}
}
#endif