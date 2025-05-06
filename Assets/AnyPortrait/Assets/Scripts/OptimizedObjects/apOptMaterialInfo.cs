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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnyPortrait
{
	/// <summary>
	/// v1.1.7에 추가된 Material Library/Set의 Opt로 Bake된 클래스
	/// 재질의 기본 설정들을 담고 있다.
	/// 실제 재질은 없으며, 이 정보를 바탕으로 재질 초기화 및 Batched/Shared를 처리한다.
	/// 이 클래스의 정보로 Key가 된다.
	/// 이 값이 없거나 Baked되지 않았다면 기존의 값 (Texture, Shader_Normal)만으로만 Key를 만들어야 한다.
	/// Custom Shader도 적용 가능하며, 만약 Clipped Child여도 재질 초기화를 위해서 필요하다 (Batched가 안되더라도..)
	/// </summary>
	[Serializable]
	public class apOptMaterialInfo
	{
		// Members
		//---------------------------------------------------
		//값이 Bake되었는가
		[SerializeField]
		public bool _isBaked = false;//<<이게 false라면 v1.1.7 이전의 방식으로 저장해야한다.

		//기본 키값
		[SerializeField, NonBackupField]
		public Texture2D _mainTex = null;

		[SerializeField, NonBackupField]
		public int _textureID = -1;

		[SerializeField, NonBackupField]
		public Shader _shader = null;
		
		//추가된 속성 정보
		//이거때문에 결국 v1.1.7과 이전 버전이 갈린 것이다.
		[Serializable]
		public class Property_Float
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public float _value = 0.0f;

			public Property_Float() { }
			public Property_Float(string name, float value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Int
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public int _value = 0;

			public Property_Int() { }
			public Property_Int(string name, int value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Vector
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public Vector4 _value = Vector4.zero;

			public Property_Vector() { }
			public Property_Vector(string name, Vector4 value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Texture
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public Texture _value = null;

			public Property_Texture() { }
			public Property_Texture(string name, Texture value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Color
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public Color _value = Color.clear;

			public Property_Color() { }
			public Property_Color(string name, Color value)
			{
				_name = name;
				_value = value;
			}
		}

		//v1.5.1 추가 : 키워드를 설정할 수 있다.
		[Serializable]
		public class Property_Keyword
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public bool _value = true;

			public Property_Keyword() {}
			public Property_Keyword(string name, bool value)
			{
				_name = name;
				_value = value;
			}
		}




		[SerializeField, NonBackupField]
		public Property_Float[] _props_Float = null;

		[SerializeField, NonBackupField]
		public Property_Int[] _props_Int = null;

		[SerializeField, NonBackupField]
		public Property_Vector[] _props_Vector = null;

		[SerializeField, NonBackupField]
		public Property_Texture[] _props_Texture = null;

		[SerializeField]
		public Property_Color[] _props_Color = null;

		[SerializeField]
		public Property_Keyword[] _props_Keywords = null;//v1.5.1

		//변경 21.12.22 : 예약된 프로퍼티를 const 변수로 빼자
		private const string RESERVED_PROP__COLOR = "_Color";
		private const string RESERVED_PROP__MAIN_TEX = "_MainTex";
		private const string RESERVED_PROP__MASK_TEX = "_MaskTex";
		private const string RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET = "_MaskScreenSpaceOffset";

		//추가 21.12.22 : 9개의 병합용 Tex를 더 받을 수 있다. 하나는 MainTex를 사용하여 총 10개
		private const string RESERVED_PROP__MERGED_TEX_1 = "_MergedTex1";
		private const string RESERVED_PROP__MERGED_TEX_2 = "_MergedTex2";
		private const string RESERVED_PROP__MERGED_TEX_3 = "_MergedTex3";
		private const string RESERVED_PROP__MERGED_TEX_4 = "_MergedTex4";
		private const string RESERVED_PROP__MERGED_TEX_5 = "_MergedTex5";
		private const string RESERVED_PROP__MERGED_TEX_6 = "_MergedTex6";
		private const string RESERVED_PROP__MERGED_TEX_7 = "_MergedTex7";
		private const string RESERVED_PROP__MERGED_TEX_8 = "_MergedTex8";
		private const string RESERVED_PROP__MERGED_TEX_9 = "_MergedTex9";


		//추가 21.12.24 : 병합 가능한지 여부 : 병합용 텍스쳐가 있어야 하며, 알파블렌딩이어야 한다.
		[SerializeField]
		public bool _isMergable = false;


		// Init
		//---------------------------------------------------
		public apOptMaterialInfo()
		{

		}

		public void Clear()
		{
			_isBaked = false;
			_mainTex = null;
			_textureID = -1;
			_shader = null;
			
			_props_Float = null;
			_props_Int = null;
			_props_Vector = null;
			_props_Texture = null;
			_props_Color = null;
			_props_Keywords = null;
		}

		


		// Bake
		//---------------------------------------------------
#if UNITY_EDITOR
		public void Bake(apTransform_Mesh srcMeshTransform, apPortrait portrait, bool isLinearSpace, int textureDataID, int srcTextureDataID, apMaterialLibrary materialLibrary)
		{
			Clear();

			//1. 기본 텍스쳐를 만든다.
			Texture2D mainTex = null;

			if(srcMeshTransform._mesh != null
				&& srcMeshTransform._mesh.LinkedTextureData != null)
			{
				mainTex = srcMeshTransform._mesh.LinkedTextureData._image;
			}

			if(mainTex == null)
			{
				return;
			}

			//2. Shader를 연결한다.
			Shader targetShader = null;
			apMaterialSet srcMatSet = null;

			apMaterialSet defaultMatSet = portrait.GetDefaultMaterialSet();
			apMaterialSet libraryMatSet = materialLibrary.Presets[0];//<<Library의 첫번째 프리셋

			bool isClippedChild = srcMeshTransform._isClipping_Child;

			if (srcMeshTransform._isCustomShader)
			{
				//2-1. Custom Shader를 사용하는 경우
				targetShader = srcMeshTransform._customShader;
			}
			else
			{
				srcMatSet = srcMeshTransform._linkedMaterialSet;
				if (srcMatSet == null)
				{
					srcMatSet = defaultMatSet;
				}

				if(srcMatSet == null)
				{
					srcMatSet = libraryMatSet;
				}

				if (srcMatSet == null)
				{
					return;
				}

				//조건에 맞는 Shader를 받아오자
				targetShader = srcMatSet.GetShader(srcMeshTransform._shaderType, isClippedChild, isLinearSpace);

				//만약 현재 Material Set에 Shader가 없다면, Default Shader에서 가져오자.
				if(targetShader == null)
				{
					targetShader = defaultMatSet.GetShader(srcMeshTransform._shaderType, isClippedChild, isLinearSpace);
				}
			}

			if(targetShader == null)
			{
				//Shader가 없으면 Material Liabrary에서 가져오자
				targetShader = libraryMatSet.GetShader(srcMeshTransform._shaderType, isClippedChild, isLinearSpace);
			}

			
			if(targetShader == null || mainTex == null)
			{
				return;
			}

			// 일단 MainTex와 Shader를 연결하자
			_isBaked = true;
			_mainTex = mainTex;
			_textureID = textureDataID;
			_shader = targetShader;
			
			//3. 프로퍼티
			List<Property_Float> list_Float = new List<Property_Float>();
			List<Property_Int> list_Int = new List<Property_Int>();
			List<Property_Vector> list_Vector = new List<Property_Vector>();
			List<Property_Texture> list_Texture = new List<Property_Texture>();
			List<Property_Color> list_Color = new List<Property_Color>();
			List<Property_Keyword> list_Keyword = new List<Property_Keyword>();

			//테스트를 위해서 Material을 생성
			//Material mat_Test = new Material(_shader);
			
			//쉐이더의 프로퍼티를 찾자 (v1.5.1)
			HashSet<string> shaderPropNames = new HashSet<string>();
			
			if(_shader != null)
			{
				int nShaderProps = ShaderUtil.GetPropertyCount(_shader);
				if(nShaderProps > 0)
				{
					for (int iProp = 0; iProp < nShaderProps; iProp++)
					{
						string propName = ShaderUtil.GetPropertyName(_shader, iProp);
						if(!shaderPropNames.Contains(propName))
						{
							//쉐이더의 프로퍼티를 등록하자
							shaderPropNames.Add(propName);
						}
					}
				}
			}

			HashSet<string> registeredProps = new HashSet<string>();
			HashSet<string> registerdKeywords = new HashSet<string>();//키워드 타입은 별도로 처리


			//3-1. 먼저 MaterialSet의 정보를 리스트로 저장한다.
			if (srcMatSet != null)
			{
				int nProps = srcMatSet._propertySets != null ? srcMatSet._propertySets.Count : 0;
				for (int iSrcProp = 0; iSrcProp < nProps; iSrcProp++)
				{
					apMaterialSet.PropertySet srcProp = srcMatSet._propertySets[iSrcProp];
					if (srcProp._isReserved ||
						!srcProp._isOptionEnabled)
					{
						continue;
					}

					//변경 21.12.22 : const 변수로 바뀌었으며, "병합용" 텍스쳐가 추가되었다.
					if(	string.Equals(srcProp._name, RESERVED_PROP__COLOR) ||
						string.Equals(srcProp._name, RESERVED_PROP__MAIN_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET) ||

						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_1) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_2) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_3) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_4) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_5) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_6) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_7) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_8) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_9) ||

						string.IsNullOrEmpty(srcProp._name))

					{
						//이 값은 사용할 수 없다. Reserved임
						continue;
					}

					if(srcProp._propType == apMaterialSet.SHADER_PROP_TYPE.Keyword)
					{
						//키워드 타입이라면
						bool isAlreadyRegisterd = registerdKeywords.Contains(srcProp._name);
						if(isAlreadyRegisterd)
						{
							//이미 등록이 되었다.
							continue;
						}

						//이 키워드를 등록하자
						registerdKeywords.Add(srcProp._name);

						//리스트에 추가한다.
						AddProperty_Keyword(list_Keyword, srcProp._name, srcProp._value_Bool);
					}
					else
					{
						//일반 프로퍼티 타입이라면
						//프로퍼티가 있는 경우에만
						bool isHasProp = shaderPropNames.Contains(srcProp._name);//v1.5.1
						if (!isHasProp)
						{
							//없는 Property.
							continue;
						}

						//중복 체크를 한다.
						bool isAlreadyRegistered = registeredProps.Contains(srcProp._name);
						if(isAlreadyRegistered)
						{
							//이미 등록이 되었다.
							continue;
						}

						//이 프로퍼티 이름을 등록하자
						registeredProps.Add(srcProp._name);

						switch (srcProp._propType)
						{
							case apMaterialSet.SHADER_PROP_TYPE.Float:
								AddProperty_Float(list_Float, srcProp._name, srcProp._value_Float);
								break;

							case apMaterialSet.SHADER_PROP_TYPE.Int:
								AddProperty_Int(list_Int, srcProp._name, srcProp._value_Int);
								break;

							case apMaterialSet.SHADER_PROP_TYPE.Vector:
								AddProperty_Vector(list_Vector, srcProp._name, srcProp._value_Vector);
								break;

							case apMaterialSet.SHADER_PROP_TYPE.Texture:
								if (srcProp._isCommonTexture)
								{
									//공통 텍스쳐인 경우
									AddProperty_Texture(list_Texture, srcProp._name, srcProp._value_CommonTexture);
								}
								else
								{
									//TextureData에 해당하는 정보가 있는지 확인하자.
									//없어도 null값을 넣는다. (null로 초기화하고 싶을 때도 있겠징...)
									//Debug.Log("Bake > [" + textureDataID + "]");
									apMaterialSet.PropertySet.ImageTexturePair imgTexPair = srcProp._imageTexturePairs.Find(delegate (apMaterialSet.PropertySet.ImageTexturePair a)
									{	
										//이전 [버그 : ImageTexturePair는 랜덤 ID로 구성된 반면, 파라미터인 textureDataID는 0, 1, 2로 증가하는 Opt ID이다.]
										//return a._textureDataID == textureDataID;
										return a._textureDataID == srcTextureDataID;
									});
									if (imgTexPair != null)
									{
										//Debug.Log(">> 제대로 적용됨 [" + srcProp._name + "] >> " + (imgTexPair._textureAsset != null ? imgTexPair._textureAsset.name : "Null"));
										AddProperty_Texture(list_Texture, srcProp._name, imgTexPair._textureAsset);
									}
									else
									{
										//Debug.LogError("Null 값이 적용되었다 [" + srcProp._name + "]");
										AddProperty_Texture(list_Texture, srcProp._name, null);
									}
								}
								break;

							case apMaterialSet.SHADER_PROP_TYPE.Color:
								AddProperty_Color(list_Color, srcProp._name, srcProp._value_Color);
								break;
						}
					}
				}
			}

			//[추가 v1.5.1] 기본 레퍼런스 재질이 있다면, 그 값을 활용할 수 있다.
			//단 이미 추가된 프로퍼티는 제외 (우선 순위가 낮다)
			Shader refShader = null;
			Material refMat = null;
			if(srcMatSet != null
				&& srcMatSet._referenceMat != null)
			{
				refMat = srcMatSet._referenceMat;
				refShader = refMat.shader;
			}

			if(refMat != null && refShader != null)
			{
				int nRefProps = ShaderUtil.GetPropertyCount(refShader);
				if(nRefProps > 0)
				{
					for (int iRefProp = 0; iRefProp < nRefProps; iRefProp++)
					{
						string refPropName = ShaderUtil.GetPropertyName(refShader, iRefProp);

						//고정적으로 사용되는 프로퍼티의 값은 받아오지 않는다.
						if(string.Equals(refPropName, RESERVED_PROP__COLOR) ||
							string.Equals(refPropName, RESERVED_PROP__MAIN_TEX) ||
							string.Equals(refPropName, RESERVED_PROP__MASK_TEX) ||
							string.Equals(refPropName, RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET) ||

							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_1) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_2) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_3) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_4) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_5) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_6) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_7) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_8) ||
							string.Equals(refPropName, RESERVED_PROP__MERGED_TEX_9) ||

							string.IsNullOrEmpty(refPropName))
						{
							//이 값은 사용할 수 없다. Reserved임
							continue;
						}

						//프로퍼티가 있는 경우에만
						bool isHasProp = shaderPropNames.Contains(refPropName);//v1.5.1
						if (!isHasProp)
						{
							//없는 Property.
							continue;
						}

						//중복 체크를 한다.
						bool isAlreadyRegistered = registeredProps.Contains(refPropName);
						if(isAlreadyRegistered)
						{
							//이미 등록이 되었다.
							continue;
						}

						//이 프로퍼티 이름을 등록하자
						registeredProps.Add(refPropName);

						//타입에 따라 재질의 값을 가져오자
						ShaderUtil.ShaderPropertyType refPropType = ShaderUtil.GetPropertyType(refShader, iRefProp);
						//Debug.Log("프로퍼티를 캐릭터에 입력 [" + refPropName + " / " + refPropType + "]");

						switch(refPropType)
						{
							case ShaderUtil.ShaderPropertyType.Float:
							case ShaderUtil.ShaderPropertyType.Range:
								{
									float refValue_Float = refMat.GetFloat(refPropName);
									AddProperty_Float(list_Float, refPropName, refValue_Float);

									//Debug.Log("참조 재질에서 추가 [" + refPropName + " / " + refPropType + "] : " + refValue_Float);
								}
								break;
#if UNITY_2021_1_OR_NEWER
							case ShaderUtil.ShaderPropertyType.Int:
								{
									int refValue_Int = refMat.GetInteger(refPropName);
									AddProperty_Int(list_Int, refPropName, refValue_Int);

									//Debug.Log("참조 재질에서 추가 [" + refPropName + " / " + refPropType + "] : " + refValue_Int);
								}
								break;
#endif

							case ShaderUtil.ShaderPropertyType.Vector:
								{
									Vector4 refValue_Vec = refMat.GetVector(refPropName);
									AddProperty_Vector(list_Vector, refPropName, refValue_Vec);

									//Debug.Log("참조 재질에서 추가 [" + refPropName + " / " + refPropType + "] : " + refValue_Vec);
								}
								break;

							case ShaderUtil.ShaderPropertyType.TexEnv:
								{
									//이 방식은 강제로 Common Texture 방식이다.
									Texture refValue_Tex = refMat.GetTexture(refPropName);
									AddProperty_Texture(list_Texture, refPropName, refValue_Tex);

									//Debug.Log("참조 재질에서 추가 [" + refPropName + " / " + refPropType + "] : " + (refValue_Tex != null ? refValue_Tex.name : "<NULL>"));
								}
								break;

							case ShaderUtil.ShaderPropertyType.Color:
								{
									Color refValue_Color = refMat.GetColor(refPropName);
									AddProperty_Color(list_Color, refPropName, refValue_Color);

									//Debug.Log("참조 재질에서 추가 [" + refPropName + " / " + refPropType + "] : " + refValue_Color);
								}
								break;
						}
					}
				}

				//활성화된 키워드도 바로 포함해서 적용한다.
				string[] keywords = refMat.shaderKeywords;
				int nKeywords = keywords != null ? keywords.Length : 0;
				if(nKeywords > 0)
				{
					//키워드를 등록하자
					for (int iKeyword = 0; iKeyword < nKeywords; iKeyword++)
					{
						string curKeyword = keywords[iKeyword];
						
						bool isAlreadyRegisterd = registerdKeywords.Contains(curKeyword);
						if(isAlreadyRegisterd)
						{
							//이미 등록이 되었다.
							continue;
						}

						//이 키워드를 등록하자
						registerdKeywords.Add(curKeyword);

						//리스트에 추가한다.
						AddProperty_Keyword(list_Keyword, curKeyword, true);
					}
				}

			}


			//3-2. MeshTransform의 속성에 Overwrite 설정이 있을 수도 있다.
			if(srcMeshTransform._customMaterialProperties != null &&
				srcMeshTransform._customMaterialProperties.Count > 0)
			{
				int nCustomProps = srcMeshTransform._customMaterialProperties.Count;
				for (int iSrcProp = 0; iSrcProp < nCustomProps; iSrcProp++)
				{
					apTransform_Mesh.CustomMaterialProperty srcProp = srcMeshTransform._customMaterialProperties[iSrcProp];

					//변경 21.12.22 : const 변수로 바뀌었으며, "병합용" 텍스쳐가 추가되었다.
					if(	string.Equals(srcProp._name, RESERVED_PROP__COLOR) ||
						string.Equals(srcProp._name, RESERVED_PROP__MAIN_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_1) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_2) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_3) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_4) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_5) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_6) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_7) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_8) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_9) ||
						string.IsNullOrEmpty(srcProp._name))
					{
						//이 값은 사용할 수 없다. Reserved임
						continue;
					}

					if(srcProp._propType != apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Keyword)
					{
						//프로퍼티가 쉐이더에 있는 경우에만 (프로퍼티 타입의 경우)
						//bool isHasProp = mat_Test.HasProperty(srcProp._name);
						bool isHasProp = shaderPropNames.Contains(srcProp._name);//v1.5.1
						if(!isHasProp)
						{
							//없는 Property.
							continue;
						}
					}
					

					//여기서는 중복체크를 하지 않는다. 있으면 덮어 씌울 것이기 때문

					//이름을 비교하여, 기존의 값이 있다면 덮어 씌우고, 없으면 새로 만들기
					switch (srcProp._propType)
					{
						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Float:
							AddProperty_Float(list_Float, srcProp._name, srcProp._value_Float);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Int:
							AddProperty_Int(list_Int, srcProp._name, srcProp._value_Int);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Vector:
							AddProperty_Vector(list_Vector, srcProp._name, srcProp._value_Vector);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Texture:
							AddProperty_Texture(list_Texture, srcProp._name, srcProp._value_Texture);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Color:
							AddProperty_Color(list_Color, srcProp._name, srcProp._value_Color);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Keyword:
							AddProperty_Keyword(list_Keyword, srcProp._name, srcProp._value_Keyword);
							break;
					}
				}
			}

			


			//리스트의 값을 변수로 저장한다.			
			if(list_Float.Count > 0)
			{
				//정렬부터 
				list_Float.Sort(delegate(Property_Float a, Property_Float b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Float = new Property_Float[list_Float.Count];
				for (int i = 0; i < list_Float.Count; i++)
				{
					_props_Float[i] = list_Float[i];
				}
			}
			
			if(list_Int.Count > 0)
			{
				//정렬부터 
				list_Int.Sort(delegate(Property_Int a, Property_Int b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Int = new Property_Int[list_Int.Count];
				for (int i = 0; i < list_Int.Count; i++)
				{
					_props_Int[i] = list_Int[i];
				}
			}

			if(list_Vector.Count > 0)
			{
				//정렬부터 
				list_Vector.Sort(delegate(Property_Vector a, Property_Vector b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Vector = new Property_Vector[list_Vector.Count];
				for (int i = 0; i < list_Vector.Count; i++)
				{
					_props_Vector[i] = list_Vector[i];
				}
			}

			if(list_Texture.Count > 0)
			{
				//정렬부터 
				list_Texture.Sort(delegate(Property_Texture a, Property_Texture b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Texture = new Property_Texture[list_Texture.Count];
				for (int i = 0; i < list_Texture.Count; i++)
				{
					_props_Texture[i] = list_Texture[i];
				}
			}

			if(list_Color.Count > 0)
			{
				//정렬부터 
				list_Color.Sort(delegate(Property_Color a, Property_Color b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Color = new Property_Color[list_Color.Count];
				for (int i = 0; i < list_Color.Count; i++)
				{
					_props_Color[i] = list_Color[i];
				}
			}

			//추가 v1.5.1 : 키워드 정렬
			if(list_Keyword.Count > 0)
			{
				//정렬부터 
				list_Keyword.Sort(delegate(Property_Keyword a, Property_Keyword b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Keywords = new Property_Keyword[list_Keyword.Count];
				for (int i = 0; i < list_Keyword.Count; i++)
				{
					_props_Keywords[i] = list_Keyword[i];
				}
			}

			


			//추가 21.12.24
			//이게 병합 가능한 재질인지 확인하자
			//기본 속성은 있어야 한다.
			//클리핑 속성은 없어야 한다.
			//병합 속성 9개 모두 있어야 한다.
			_isMergable = false;

			//변경 v1.5.1
			if(shaderPropNames.Contains(RESERVED_PROP__COLOR)
				&& shaderPropNames.Contains(RESERVED_PROP__MAIN_TEX)
				&& !shaderPropNames.Contains(RESERVED_PROP__MASK_TEX)
				&& !shaderPropNames.Contains(RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_1)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_2)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_3)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_4)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_5)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_6)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_7)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_8)
				&& shaderPropNames.Contains(RESERVED_PROP__MERGED_TEX_9)
				)
			{
				_isMergable = true;
			}

			//UnityEngine.Object.DestroyImmediate(mat_Test);//삭제 v1.5.1
		}
		
#endif
		//Property 값들을 추가하자.
		//이름을 기준으로 겹체는게 있으면 값을 덮어씌움.
		public void AddProperty_Float(List<Property_Float> propList, string name, float value)
		{
			Property_Float existProp = propList.Find(delegate(Property_Float a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Float(name, value)); }
		}

		public void AddProperty_Int(List<Property_Int> propList, string name, int value)
		{
			Property_Int existProp = propList.Find(delegate(Property_Int a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Int(name, value)); }
		}

		public void AddProperty_Vector(List<Property_Vector> propList, string name, Vector4 value)
		{
			Property_Vector existProp = propList.Find(delegate(Property_Vector a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Vector(name, value)); }
		}

		public void AddProperty_Texture(List<Property_Texture> propList, string name, Texture value)
		{
			Property_Texture existProp = propList.Find(delegate(Property_Texture a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Texture(name, value)); }
		}

		public void AddProperty_Color(List<Property_Color> propList, string name, Color value)
		{
			Property_Color existProp = propList.Find(delegate(Property_Color a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Color(name, value)); }
		}

		public void AddProperty_Keyword(List<Property_Keyword> propList, string name, bool value)
		{
			Property_Keyword existProp = propList.Find(delegate(Property_Keyword a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Keyword(name, value)); }
		}

		// Make from Src : Batched / Shared Material에서 만들때는 이 함수를 이용하자
		//---------------------------------------------------
		public void MakeFromSrc(apOptMaterialInfo srcMatInfo)
		{
			Clear();

			_isBaked = true;
			_mainTex = srcMatInfo._mainTex;
			_textureID = srcMatInfo._textureID;
			_shader = srcMatInfo._shader;
			
			int nSrcProp_Float = srcMatInfo.NumProp_Float;
			int nSrcProp_Int = srcMatInfo.NumProp_Int;
			int nSrcProp_Vector = srcMatInfo.NumProp_Vector;
			int nSrcProp_Texture = srcMatInfo.NumProp_Texture;
			int nSrcProp_Color = srcMatInfo.NumProp_Color;
			int nSrcProp_Keyword = srcMatInfo.NumProp_Keyword;

			if(nSrcProp_Float > 0)
			{
				_props_Float = new Property_Float[nSrcProp_Float];
				for (int i = 0; i < nSrcProp_Float; i++)
				{
					Property_Float srcProp = srcMatInfo._props_Float[i];
					_props_Float[i] = new Property_Float(srcProp._name, srcProp._value);
				}
			}

			if(nSrcProp_Int > 0)
			{
				_props_Int = new Property_Int[nSrcProp_Int];
				for (int i = 0; i < nSrcProp_Int; i++)
				{
					Property_Int srcProp = srcMatInfo._props_Int[i];
					_props_Int[i] = new Property_Int(srcProp._name, srcProp._value);
				}
			}

			if(nSrcProp_Vector > 0)
			{
				_props_Vector = new Property_Vector[nSrcProp_Vector];
				for (int i = 0; i < nSrcProp_Vector; i++)
				{
					Property_Vector srcProp = srcMatInfo._props_Vector[i];
					_props_Vector[i] = new Property_Vector(srcProp._name, srcProp._value);
				}
			}

			if(nSrcProp_Texture > 0)
			{
				_props_Texture = new Property_Texture[nSrcProp_Texture];
				for (int i = 0; i < nSrcProp_Texture; i++)
				{
					Property_Texture srcProp = srcMatInfo._props_Texture[i];
					_props_Texture[i] = new Property_Texture(srcProp._name, srcProp._value);
				}
			}

			if(nSrcProp_Color > 0)
			{
				_props_Color = new Property_Color[nSrcProp_Color];
				for (int i = 0; i < nSrcProp_Color; i++)
				{
					Property_Color srcProp = srcMatInfo._props_Color[i];
					_props_Color[i] = new Property_Color(srcProp._name, srcProp._value);
				}
			}

			if(nSrcProp_Keyword > 0)
			{
				_props_Keywords = new Property_Keyword[nSrcProp_Keyword];
				for (int i = 0; i < nSrcProp_Keyword; i++)
				{
					Property_Keyword srcProp = srcMatInfo._props_Keywords[i];
					_props_Keywords[i] = new Property_Keyword(srcProp._name, srcProp._value);
				}
			}
		}


		// Functions
		//---------------------------------------------------
		public static bool IsSameInfo(apOptMaterialInfo infoA, apOptMaterialInfo infoB)
		{
			if(	infoA._mainTex != infoB._mainTex ||
				infoA._textureID != infoB._textureID ||
				infoA._shader != infoB._shader ||
				
				infoA.NumProp_Float != infoB.NumProp_Float ||
				infoA.NumProp_Int != infoB.NumProp_Int ||
				infoA.NumProp_Vector != infoB.NumProp_Vector ||
				infoA.NumProp_Texture != infoB.NumProp_Texture ||
				infoA.NumProp_Color != infoB.NumProp_Color ||
				infoA.NumProp_Keyword != infoB.NumProp_Keyword)
			{
				//기본 속성에서 차이가 있다.
				return false;
			}

			//이제 상세 설정이 모두 동일한지 확인해야한다.
			//하나라도 다르면 패스
			//이름이나 속성을 순서대로 비교해서 하나라도 다르면 다른 것이다.
			//정렬을 했기 때문에 순서대로 비교하면 된다.

			int numFloat = infoA.NumProp_Float;
			int numInt = infoA.NumProp_Int;
			int numVector = infoA.NumProp_Vector;
			int numTexture = infoA.NumProp_Texture;
			int numColor = infoA.NumProp_Color;
			int numKeyword = infoA.NumProp_Keyword;

			//1. Float
			if(numFloat > 0)
			{
				Property_Float propA = null;
				Property_Float propB = null;
				for (int i = 0; i < numFloat; i++)
				{
					propA = infoA._props_Float[i];
					propB = infoB._props_Float[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(Mathf.Abs(propA._value - propB._value) > 0.0001f)
					{
						return false;
					}
				}
			}

			//2. Int
			if(numInt > 0)
			{
				Property_Int propA = null;
				Property_Int propB = null;
				for (int i = 0; i < numInt; i++)
				{
					propA = infoA._props_Int[i];
					propB = infoB._props_Int[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(propA._value != propB._value)
					{
						return false;
					}
				}
			}

			//3. Vector
			if(numVector > 0)
			{
				Property_Vector propA = null;
				Property_Vector propB = null;
				for (int i = 0; i < numVector; i++)
				{
					propA = infoA._props_Vector[i];
					propB = infoB._props_Vector[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(	Mathf.Abs(propA._value.x - propB._value.x) > 0.0001f ||
						Mathf.Abs(propA._value.y - propB._value.y) > 0.0001f ||
						Mathf.Abs(propA._value.z - propB._value.z) > 0.0001f ||
						Mathf.Abs(propA._value.w - propB._value.w) > 0.0001f)
					{
						return false;
					}
				}
			}

			//4. Texture
			if(numTexture > 0)
			{
				Property_Texture propA = null;
				Property_Texture propB = null;
				for (int i = 0; i < numTexture; i++)
				{
					propA = infoA._props_Texture[i];
					propB = infoB._props_Texture[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(propA._value != propB._value)
					{
						return false;
					}
				}
			}


			//5. Color
			if(numColor > 0)
			{
				Property_Color propA = null;
				Property_Color propB = null;
				for (int i = 0; i < numColor; i++)
				{
					propA = infoA._props_Color[i];
					propB = infoB._props_Color[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(	Mathf.Abs(propA._value.r - propB._value.r) > 0.001f ||
						Mathf.Abs(propA._value.g - propB._value.g) > 0.001f ||
						Mathf.Abs(propA._value.b - propB._value.b) > 0.001f ||
						Mathf.Abs(propA._value.a - propB._value.a) > 0.001f)
					{
						return false;
					}
				}
			}

			//6. Keyword (v1.5.1)
			if(numKeyword > 0)
			{
				Property_Keyword propA = null;
				Property_Keyword propB = null;
				for (int i = 0; i < numKeyword; i++)
				{
					propA = infoA._props_Keywords[i];
					propB = infoB._props_Keywords[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(propA._value != propB._value)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// 기본 재질 속성을 제외한 사용자 정의 속성들을 입력된 Material에 입력한다.
		/// (Float, Int, Vector, Texture, Color)
		/// </summary>
		/// <param name="targetMaterial"></param>
		public void SetMaterialProperties(Material targetMaterial)
		{
			if(targetMaterial == null)
			{
				return;
			}

			int nProp_Float = NumProp_Float;
			int nProp_Int = NumProp_Int;
			int nProp_Vector = NumProp_Vector;
			int nProp_Texture = NumProp_Texture;
			int nProp_Color = NumProp_Color;
			int nProp_Keyword = NumProp_Keyword;

			//1. Float
			if(nProp_Float > 0)
			{
				Property_Float propF = null;
				for (int i = 0; i < nProp_Float; i++)
				{
					propF = _props_Float[i];
					targetMaterial.SetFloat(propF._name, propF._value);
				}
			}

			//2. Int
			if(nProp_Int > 0)
			{
				Property_Int propI = null;
				for (int i = 0; i < nProp_Int; i++)
				{
					propI = _props_Int[i];
#if UNITY_2021_1_OR_NEWER
					targetMaterial.SetInteger(propI._name, propI._value);
#else
					targetMaterial.SetInt(propI._name, propI._value);
#endif				
				}
			}

			//3. Vector
			if(nProp_Vector > 0)
			{
				Property_Vector propV = null;
				for (int i = 0; i < nProp_Vector; i++)
				{
					propV = _props_Vector[i];
					targetMaterial.SetVector(propV._name, propV._value);
				}
			}

			//4. Texture
			if(nProp_Texture > 0)
			{
				Property_Texture propT = null;
				for (int i = 0; i < nProp_Texture; i++)
				{
					propT = _props_Texture[i];
					targetMaterial.SetTexture(propT._name, propT._value);
				}
			}

			//5. Color
			if(nProp_Color > 0)
			{
				Property_Color propC = null;
				for (int i = 0; i < nProp_Color; i++)
				{
					propC = _props_Color[i];
					targetMaterial.SetColor(propC._name, propC._value);
				}
			}

			//6. Keyword v1.5.1
			if(nProp_Keyword > 0)
			{
				Property_Keyword propK = null;
				for (int i = 0; i < nProp_Keyword; i++)
				{
					propK = _props_Keywords[i];
					
					if(propK._value)
					{
						//키워드 활성화
						targetMaterial.EnableKeyword(propK._name);
					}
					else
					{
						//키워드 비활성화
						targetMaterial.DisableKeyword(propK._name);
					}
				}
			}
		}

		// Get / Set
		//---------------------------------------------------
		public int NumProp_Float	{ get { return _props_Float != null ? _props_Float.Length : 0; } }
		public int NumProp_Int		{ get { return _props_Int != null ? _props_Int.Length : 0; } }
		public int NumProp_Vector	{ get { return _props_Vector != null ? _props_Vector.Length : 0; } }
		public int NumProp_Texture	{ get { return _props_Texture != null ? _props_Texture.Length : 0; } }
		public int NumProp_Color	{ get { return _props_Color != null ? _props_Color.Length : 0; } }
		public int NumProp_Keyword	{ get { return _props_Keywords != null ? _props_Keywords.Length : 0; }  }
	}
}