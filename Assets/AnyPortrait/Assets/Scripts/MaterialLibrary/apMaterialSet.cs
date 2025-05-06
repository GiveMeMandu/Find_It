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
using System.IO;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// MaterialLibrary에서 추가된 재질 설정들
	/// Preset 데이터 : Reserved와 Custom Preset이 있다.
	/// Portrait의 데이터 : Reserved + Preset으로 저장된다.
	/// </summary>
	[Serializable]
	public class apMaterialSet
	{
		// Members
		//-----------------------------------------------
		[SerializeField]
		public string _name = "";

		[SerializeField]
		public int _uniqueID = -1;//고유의 ID (Preset과 Portrait 데이터는 서로 다른 ID를 갖는다.)

		//기본 설정들
		
		
		//[v1.5.0 삭제] 저장되지 않는 값이므로 부정확하다. 실시간으로 조회하자
		//파일로 저장된 프리셋인 경우
		//[NonSerialized]
		//public bool _isReserved = false; //ID에 따라서 Reserved인지, 아니면 CustomPreset인지 알 수 있다.

		[SerializeField]
		public int _linkedPresetID = -1;//Portrait 데이터이면 : 연결된 프리셋의 ID

		

		//Portrait 데이터인 경우
		[NonSerialized]
		public apMaterialSet _linkedPresetMaterial = null;//연결된 프리셋

		[SerializeField]
		public bool _isDefault = true;//다른 설정이 없다면 이 재질이 전체에 적용되는가 (MeshTF에 재질 ID가 별도로 지정되지 않는 경우)

		
		public enum ICON : int
		{
			Unlit = 0,
			Lit = 1,
			LitSpecular = 2,
			LitSpecularEmission = 3,
			LitRimlight = 4,
			LitRamp = 5,
			Effect = 6,
			Cartoon = 7,
			Custom1 = 8,
			Custom2 = 9,
			Custom3 = 10,
			UnlitVR = 11,
			LitVR = 12,
			UnlitMergeable = 13,
			LitMergeable = 14
		}

		[SerializeField]
		public ICON _icon = ICON.Unlit;



		[SerializeField]
		public string _shaderPath_Normal_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_Normal_Additive = "";

		[SerializeField]
		public string _shaderPath_Normal_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_Normal_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_Clipped_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_Clipped_Additive = "";

		[SerializeField]
		public string _shaderPath_Clipped_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_Clipped_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_L_Normal_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_L_Normal_Additive = "";

		[SerializeField]
		public string _shaderPath_L_Normal_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_L_Normal_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_Additive = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_AlphaMask = "";

		//실제 Shader Asset들
		//파일에서 열때 > Path 위주
		//Portrait에서 저장할 때 > Shader 에셋과 Path 비교하여 결정
		[SerializeField, NonBackupField]
		public Shader _shader_Normal_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Normal_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Normal_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Normal_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_AlphaMask = null;




		public enum SHADER_PROP_TYPE
		{
			Float = 0,
			Int = 1,
			Vector = 2,
			Texture = 3,
			Color = 4,
			Keyword = 5//추가 v1.5.1
		}


		[Serializable]
		public class PropertySet
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public bool _isReserved = false;//이게 True이면 값을 설정할 수 없다.

			[SerializeField]
			public bool _isOptionEnabled = true;

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
			public bool _value_Bool = true;

			//텍스쳐는 조금 다르다.
			//공통된 하나의 텍스쳐를 이용할지, 이미지(TextureData)마다 다르게 텍스쳐를 입력할지 고를 수 있다.
			[SerializeField]
			public bool _isCommonTexture = true;

			[SerializeField]
			public string _commonTexturePath = "";//<<파일에서 읽을땐 이 경로를 이용한다.

			[SerializeField, NonBackupField]
			public Texture _value_CommonTexture = null;//<Texture2D가 아니라 Texture이다.

			//이 값은 파일로 저장되지 않는다.
			[Serializable]
			public class ImageTexturePair
			{
				[SerializeField]
				public int _textureDataID = -1;

				[NonSerialized]
				public apTextureData _targetTextureData = null;

				[SerializeField]
				public string _textureAssetPath = "";

				[SerializeField, NonBackupField]
				public Texture _textureAsset = null;

				public ImageTexturePair()
				{

				}

				public void CopyFromSrc(ImageTexturePair src)
				{
					_textureDataID = src._textureDataID;
					_targetTextureData = src._targetTextureData;
					_textureAssetPath = src._textureAssetPath;
					_textureAsset = src._textureAsset;
				}
			}

			[SerializeField, NonBackupField]
			public List<ImageTexturePair> _imageTexturePairs = new List<ImageTexturePair>();

			public PropertySet()
			{

			}

			public void CopyFromSrc(PropertySet srcProp)
			{
				_name = srcProp._name;
				_isReserved = srcProp._isReserved;
				_isOptionEnabled = srcProp._isOptionEnabled;
				_propType = srcProp._propType;

				_value_Float = srcProp._value_Float;
				_value_Int = srcProp._value_Int;
				_value_Vector = srcProp._value_Vector;
				_value_Color = srcProp._value_Color;
				_value_Bool = srcProp._value_Bool;

				_isCommonTexture = srcProp._isCommonTexture;

				_commonTexturePath = srcProp._commonTexturePath;
				_value_CommonTexture = srcProp._value_CommonTexture;

				for (int i = 0; i < srcProp._imageTexturePairs.Count; i++)
				{
					ImageTexturePair newImgTexPair = new ImageTexturePair();
					newImgTexPair.CopyFromSrc(srcProp._imageTexturePairs[i]);

					_imageTexturePairs.Add(newImgTexPair);
				}
				
			}


			public PropertySet SetFloat(float floatValue)
			{
				_value_Float = floatValue;
				return this;
			}

			public PropertySet SetInt(int intValue)
			{
				_value_Int = intValue;
				return this;
			}

			public PropertySet SetVector(Vector4 vectorValue)
			{
				_value_Vector = vectorValue;
				return this;
			}

			public PropertySet SetColor(Color colorValue)
			{
				_value_Color = colorValue;
				return this;
			}

			public PropertySet SetKeyword(bool enableValue)
			{
				_value_Bool = enableValue;
				return this;
			}


		}

		[SerializeField]
		public List<PropertySet> _propertySets = new List<PropertySet>();
		

		[SerializeField]
		public bool _isNeedToSetBlackColoredAmbient = false;

		//v1.5.1 : 쉐이더 프로퍼티에 지정되지 않은 기본값은 외부의 재질로부터 얻을 수 있다.
		//프리셋으로 저장하기 위해서는 경로를 같이 입력한다.
		[SerializeField, NonBackupField] public Material _referenceMat = null;
		[SerializeField] public string _referenceMaterialPath = "";



		// Init
		//-----------------------------------------------
		public apMaterialSet()
		{
			//Material mat;
			//mat.Set
			
		}

		public void Init()
		{
			_name = "";
			_uniqueID = -1;
			_linkedPresetID = -1;
			_linkedPresetMaterial = null;
			_isDefault = false;

			_icon = ICON.Unlit;

			_shaderPath_Normal_AlphaBlend = "";
			_shaderPath_Normal_Additive = "";
			_shaderPath_Normal_SoftAdditive = "";
			_shaderPath_Normal_Multiplicative = "";
			_shaderPath_Clipped_AlphaBlend = "";
			_shaderPath_Clipped_Additive = "";
			_shaderPath_Clipped_SoftAdditive = "";
			_shaderPath_Clipped_Multiplicative = "";
			_shaderPath_L_Normal_AlphaBlend = "";
			_shaderPath_L_Normal_Additive = "";
			_shaderPath_L_Normal_SoftAdditive = "";
			_shaderPath_L_Normal_Multiplicative = "";
			_shaderPath_L_Clipped_AlphaBlend = "";
			_shaderPath_L_Clipped_Additive = "";
			_shaderPath_L_Clipped_SoftAdditive = "";
			_shaderPath_L_Clipped_Multiplicative = "";
			_shaderPath_AlphaMask = "";

			_shader_Normal_AlphaBlend = null;
			_shader_Normal_Additive = null;
			_shader_Normal_SoftAdditive = null;
			_shader_Normal_Multiplicative = null;
			_shader_Clipped_AlphaBlend = null;
			_shader_Clipped_Additive = null;
			_shader_Clipped_SoftAdditive = null;
			_shader_Clipped_Multiplicative = null;
			_shader_L_Normal_AlphaBlend = null;
			_shader_L_Normal_Additive = null;
			_shader_L_Normal_SoftAdditive = null;
			_shader_L_Normal_Multiplicative = null;
			_shader_L_Clipped_AlphaBlend = null;
			_shader_L_Clipped_Additive = null;
			_shader_L_Clipped_SoftAdditive = null;
			_shader_L_Clipped_Multiplicative = null;
			_shader_AlphaMask = null;

			if(_propertySets == null)
			{
				_propertySets = new List<PropertySet>();
			}
			_propertySets.Clear();

			_isNeedToSetBlackColoredAmbient = false;

			_referenceMat = null;
			_referenceMaterialPath = "";
		}

		// Link
		//-----------------------------------------------



		// Functions
		//-----------------------------------------------
		public apMaterialSet MakeReserved(	int uniqueID, 
											string name, 
											ICON icon,
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
											bool isNeedToSetBlackColoredAmbient
										)
		{
			_uniqueID = uniqueID;
			_name = name;

			//if(_uniqueID < 10)
			//{
			//	_isReserved = true;
			//}
			//else
			//{
			//	_isReserved = false;
			//}
			_linkedPresetID = -1;
			_linkedPresetMaterial = null;
			_isDefault = false;

			_icon = icon;


			_shaderPath_Normal_AlphaBlend = shaderPath_Normal_AlphaBlend;
			_shaderPath_Normal_Additive = shaderPath_Normal_Additive;
			_shaderPath_Normal_SoftAdditive = shaderPath_Normal_SoftAdditive;
			_shaderPath_Normal_Multiplicative = shaderPath_Normal_Multiplicative;
			_shaderPath_Clipped_AlphaBlend = shaderPath_Clipped_AlphaBlend;
			_shaderPath_Clipped_Additive = shaderPath_Clipped_Additive;
			_shaderPath_Clipped_SoftAdditive = shaderPath_Clipped_SoftAdditive;
			_shaderPath_Clipped_Multiplicative = shaderPath_Clipped_Multiplicative;
			_shaderPath_L_Normal_AlphaBlend = shaderPath_L_Normal_AlphaBlend;
			_shaderPath_L_Normal_Additive = shaderPath_L_Normal_Additive;
			_shaderPath_L_Normal_SoftAdditive = shaderPath_L_Normal_SoftAdditive;
			_shaderPath_L_Normal_Multiplicative = shaderPath_L_Normal_Multiplicative;
			_shaderPath_L_Clipped_AlphaBlend = shaderPath_L_Clipped_AlphaBlend;
			_shaderPath_L_Clipped_Additive = shaderPath_L_Clipped_Additive;
			_shaderPath_L_Clipped_SoftAdditive = shaderPath_L_Clipped_SoftAdditive;
			_shaderPath_L_Clipped_Multiplicative = shaderPath_L_Clipped_Multiplicative;
			_shaderPath_AlphaMask = shaderPath_AlphaMask;

			if(_propertySets == null)
			{
				_propertySets = new List<PropertySet>();
			}
			_propertySets.Clear();

			_isNeedToSetBlackColoredAmbient = isNeedToSetBlackColoredAmbient;

			_referenceMat = null;
			_referenceMaterialPath = "";

#if UNITY_EDITOR

			//ValidateShaderPaths();//유효성 검사 추가 (v1.5.0) >> 이거 일단 제외. 검색이 너무 오래 걸린다.
			LoadShaderAssets();
#endif



			return this;
		}


		
#if UNITY_EDITOR
		public void LoadShaderAssets()
		{	
			if(_shader_Normal_AlphaBlend == null)		{ _shader_Normal_AlphaBlend = 			LoadShader(_shaderPath_Normal_AlphaBlend); }
			if(_shader_Normal_Additive == null)			{ _shader_Normal_Additive =				LoadShader(_shaderPath_Normal_Additive); }
			if(_shader_Normal_SoftAdditive == null)		{ _shader_Normal_SoftAdditive =			LoadShader(_shaderPath_Normal_SoftAdditive); }
			if(_shader_Normal_Multiplicative == null)	{ _shader_Normal_Multiplicative =		LoadShader(_shaderPath_Normal_Multiplicative); }
			if(_shader_Clipped_AlphaBlend == null)		{ _shader_Clipped_AlphaBlend =			LoadShader(_shaderPath_Clipped_AlphaBlend); }
			if(_shader_Clipped_Additive == null)		{ _shader_Clipped_Additive =			LoadShader(_shaderPath_Clipped_Additive); }
			if(_shader_Clipped_SoftAdditive == null)	{ _shader_Clipped_SoftAdditive =		LoadShader(_shaderPath_Clipped_SoftAdditive); }
			if(_shader_Clipped_Multiplicative == null)	{ _shader_Clipped_Multiplicative =		LoadShader(_shaderPath_Clipped_Multiplicative); }
			if(_shader_L_Normal_AlphaBlend == null)		{ _shader_L_Normal_AlphaBlend =			LoadShader(_shaderPath_L_Normal_AlphaBlend); }
			if(_shader_L_Normal_Additive == null)		{ _shader_L_Normal_Additive =			LoadShader(_shaderPath_L_Normal_Additive); }
			if(_shader_L_Normal_SoftAdditive == null)	{ _shader_L_Normal_SoftAdditive =		LoadShader(_shaderPath_L_Normal_SoftAdditive); }
			if(_shader_L_Normal_Multiplicative == null) { _shader_L_Normal_Multiplicative =		LoadShader(_shaderPath_L_Normal_Multiplicative); }
			if(_shader_L_Clipped_AlphaBlend == null)	{ _shader_L_Clipped_AlphaBlend =		LoadShader(_shaderPath_L_Clipped_AlphaBlend); }
			if(_shader_L_Clipped_Additive == null)		{ _shader_L_Clipped_Additive =			LoadShader(_shaderPath_L_Clipped_Additive); }
			if(_shader_L_Clipped_SoftAdditive == null)	{ _shader_L_Clipped_SoftAdditive =		LoadShader(_shaderPath_L_Clipped_SoftAdditive); }
			if(_shader_L_Clipped_Multiplicative == null){ _shader_L_Clipped_Multiplicative =	LoadShader(_shaderPath_L_Clipped_Multiplicative); }
			if(_shader_AlphaMask == null)				{ _shader_AlphaMask =					LoadShader(_shaderPath_AlphaMask); }
		}

		private Shader LoadShader(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				return null;
			}
			return AssetDatabase.LoadAssetAtPath<Shader>(path);
		}

		//v1.5.1 : 추가 참조 재질을 로드한다.
		public void LoadRefMaterial()
		{
			if(_referenceMat == null)
			{
				_referenceMat = LoadMaterial(_referenceMaterialPath);
			}
		}

		private Material LoadMaterial(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				return null;
			}
			return AssetDatabase.LoadAssetAtPath<Material>(path);
		}

		/// <summary>
		/// 모든 Shader가 로드되었는가
		/// </summary>
		/// <returns></returns>
		public bool IsAllShaderLoaded()
		{
			if(_shader_Normal_AlphaBlend == null
				|| _shader_Normal_Additive == null
				|| _shader_Normal_SoftAdditive == null
				|| _shader_Normal_Multiplicative == null
				|| _shader_Clipped_AlphaBlend == null
				|| _shader_Clipped_Additive == null
				|| _shader_Clipped_SoftAdditive == null
				|| _shader_Clipped_Multiplicative == null
				|| _shader_L_Normal_AlphaBlend == null
				|| _shader_L_Normal_Additive == null
				|| _shader_L_Normal_SoftAdditive == null
				|| _shader_L_Normal_Multiplicative == null
				|| _shader_L_Clipped_AlphaBlend == null
				|| _shader_L_Clipped_Additive == null
				|| _shader_L_Clipped_SoftAdditive == null
				|| _shader_L_Clipped_Multiplicative == null
				|| _shader_AlphaMask == null)
			{
				//하나라도 로드가 안되었다.
				return false;
			}
			return true;
		}

		//Shader 파일을 열고 유효하지 않다면 같은 이름의 파일을 어디선가 찾는다.
		public void ValidateShaderPaths()
		{
			ValidateShader(ref _shaderPath_Normal_AlphaBlend);
			ValidateShader(ref _shaderPath_Normal_Additive);
			ValidateShader(ref _shaderPath_Normal_SoftAdditive);
			ValidateShader(ref _shaderPath_Normal_Multiplicative);
			ValidateShader(ref _shaderPath_Clipped_AlphaBlend);
			ValidateShader(ref _shaderPath_Clipped_Additive);
			ValidateShader(ref _shaderPath_Clipped_SoftAdditive);
			ValidateShader(ref _shaderPath_Clipped_Multiplicative);
			ValidateShader(ref _shaderPath_L_Normal_AlphaBlend);
			ValidateShader(ref _shaderPath_L_Normal_Additive);
			ValidateShader(ref _shaderPath_L_Normal_SoftAdditive);
			ValidateShader(ref _shaderPath_L_Normal_Multiplicative);
			ValidateShader(ref _shaderPath_L_Clipped_AlphaBlend);
			ValidateShader(ref _shaderPath_L_Clipped_Additive);
			ValidateShader(ref _shaderPath_L_Clipped_SoftAdditive);
			ValidateShader(ref _shaderPath_L_Clipped_Multiplicative);
			ValidateShader(ref _shaderPath_AlphaMask);
		}


		private void ValidateShader(ref string shaderPath)
		{
			Shader shaderAsset = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
			
			if(shaderAsset != null)
			{
				//유효하다.
				return;
			}

			//유효하지 않다면
			//파일 이름만 찾아서 경로를 찾자
			if(string.IsNullOrEmpty(shaderPath))
			{
				//이건 유효하진 않지만 수정도 못한다. 일단 true
				return;
			}

			//파일 이름을 찾자
			int iLastSlash = shaderPath.LastIndexOf('/');
			string strFileName = shaderPath;
			if(iLastSlash >= 0 && iLastSlash < shaderPath.Length - 1)
			{
				strFileName = shaderPath.Substring(iLastSlash + 1);
			}

			//이제 이 이름을 Assets 폴더 내에서 찾자
			string assetsPath = Application.dataPath;
			if(assetsPath.EndsWith("/"))
			{
				assetsPath = assetsPath.Substring(0, assetsPath.Length - 1);
			}
			DirectoryInfo di_Assets = new DirectoryInfo(assetsPath);

			if(!di_Assets.Exists)
			{
				//Assets 폴더를 찾지 못했다. (수정 불가)
				return;
			}
			string result = FindShaderAssetByFileName(ref strFileName, di_Assets);
			if(string.IsNullOrEmpty(result))
			{
				//이건 유효하진 않지만 수정도 못한다. 일단 true
				return;
			}

			result = result.Replace('\\', '/');

			//경로를 "Assets/"로 시작하도록 만든다.
			string strRootPath = assetsPath.Replace('\\', '/');
			result = "Assets/" + result.Substring(strRootPath.Length);

			//이제 열어보자
			shaderAsset = AssetDatabase.LoadAssetAtPath<Shader>(result);
			if(shaderAsset != null)
			{
				//새로운 경로에서 열린다! > 경로 변경
				shaderPath = result;
			}
		}

		
		private string FindShaderAssetByFileName(ref string fileName, DirectoryInfo curDir)
		{
			FileInfo[] files = curDir.GetFiles();
			int nFiles = files != null ? files.Length : 0;
			if(nFiles > 0)
			{
				for (int i = 0; i < nFiles; i++)
				{
					FileInfo fi = files[i];
					if(string.Equals(fi.Name, fileName))
					{
						//찾았다! 전체 경로 리턴
						return fi.FullName;
					}
				}
			}

			//자식 디렉토리 찾기
			DirectoryInfo[] childDirs = curDir.GetDirectories();
			int nChildDirs = childDirs != null ? childDirs.Length : 0;
			if(nChildDirs > 0)
			{
				for (int i = 0; i < nChildDirs; i++)
				{
					DirectoryInfo childDir = childDirs[i];
					string result = FindShaderAssetByFileName(ref fileName, childDir);
					if(!string.IsNullOrEmpty(result))
					{
						//결과를 리턴한다.
						return result;
					}
				}
			}
			return null;
		}


#endif

		public PropertySet AddProperty(string name, bool isReserved, SHADER_PROP_TYPE propType)
		{
			//[v1.5.1] 중복 체크를 해야한다.
			//이름이 빈칸이라면 일단 추가는 해준다.
			if(!string.IsNullOrEmpty(name))
			{
				PropertySet existPropSet = GetPropertySet(name);
				if(existPropSet != null)
				{
					//Debug.Log("중복된 프로퍼티 발견 [" + name + "]");
					return existPropSet;
				}
			}

			PropertySet newPropertySet = new PropertySet();
			newPropertySet._name = name;
			newPropertySet._isReserved = isReserved;
			newPropertySet._isOptionEnabled = true;
			newPropertySet._propType = propType;

			if(_propertySets == null)
			{
				_propertySets = new List<PropertySet>();
			}
			_propertySets.Add(newPropertySet);//중복 체크된 추가 [v1.5.1]

			return newPropertySet;
		}

		public PropertySet AddProperty_Texture(string name, bool isControlledByAnyPortrait, bool isCommonTexture)
		{
			//중복 체크를 한다.
			if(!string.IsNullOrEmpty(name))
			{
				PropertySet existPropSet = GetPropertySet(name);
				if(existPropSet != null)
				{
					//Debug.Log("중복된 프로퍼티 발견 [" + name + "]");
					return existPropSet;
				}
			}

			PropertySet newPropertySet = new PropertySet();
			newPropertySet._name = name;
			newPropertySet._isReserved = isControlledByAnyPortrait;
			newPropertySet._isOptionEnabled = true;
			newPropertySet._propType = SHADER_PROP_TYPE.Texture;
			newPropertySet._isCommonTexture = isCommonTexture;//<<텍스쳐 타입은 이거 추가

			if(_propertySets == null)
			{
				_propertySets = new List<PropertySet>();
			}
			_propertySets.Add(newPropertySet);//중복 체크된 추가 [v1.5.1]

			return newPropertySet;
		}

		/// <summary>
		/// [v1.5.1] 프로퍼티를 추가한다. 중복 체크 로직이 추가됨
		/// </summary>
		public PropertySet AddProperty(PropertySet newProp)
		{
			//중복 체크 후 프로퍼티 추가
			if(newProp == null)
			{
				return null;
			}

			//이름이 빈칸이라면 중복체크를 하지 않는다. (임시 편집을 위한 여유 코드)
			if(!string.IsNullOrEmpty(newProp._name))
			{
				//그 외에는 중복 체크를 하자
				PropertySet existProp = GetPropertySet(newProp._name);
				if(existProp != null)
				{
					//이미 동일 이름의 프로퍼티가 있다면
					return existProp;
				}
			}

			if(_propertySets == null)
			{
				_propertySets = new List<PropertySet>();
			}
			_propertySets.Add(newProp);//중복 체크된 추가 [v1.5.1]
			return newProp;
		}


		// [v1.5.1] 프로퍼티 중복 체크용
		public PropertySet GetPropertySet(string propName)
		{
			int nPropSet = _propertySets != null ? _propertySets.Count : 0;
			if(nPropSet == 0)
			{
				return null;
			}

			//이름으로 찾자
			// return _propertySets.Find(delegate(PropertySet a)
			// {
			// 	return string.Equals(a._name, propName);
			// });

			s_GetPropSet_Name = propName;
			return _propertySets.Find(s_GetPropSetByName);
		}

		private static string s_GetPropSet_Name = "";
		private static bool Func_GetPropSetByName(PropertySet a)
		{
			return string.Equals(a._name, s_GetPropSet_Name);
		}
		private static Predicate<PropertySet> s_GetPropSetByName = Func_GetPropSetByName;

		
		/// <summary>
		/// MaterialSet으로 부터 생성한다.
		/// </summary>
		/// <param name="srcMat"></param>
		/// <param name="uniqueID"></param>
		/// <param name="isFromPreset"></param>
		/// <param name="isDefault"></param>
		public void CopyFromSrc(apMaterialSet srcMat, int uniqueID, bool isFromPreset, bool isPreset, bool isDefault)
		{
			_uniqueID = uniqueID;
			_name = srcMat._name;

			
			if (!isPreset)
			{
				//프리셋이 아닌 경우
				//_isReserved = false;
				if (isFromPreset)
				{
					//Src가 Preset인 경우
					_linkedPresetID = srcMat._uniqueID;
					_linkedPresetMaterial = srcMat;
				}
				else
				{
					//Src가 일반 MaterialSet인 경우
					//같은 프리셋 공유
					_linkedPresetID = srcMat._linkedPresetID;
					_linkedPresetMaterial = srcMat._linkedPresetMaterial;
				}


				_isDefault = isDefault;//이건 Optimized Bake시 적용
			}
			else
			{
				//프리셋인 경우
				//_isReserved = false;
				_linkedPresetID = -1;
				_linkedPresetMaterial = null;
			}

			_icon = srcMat._icon;



			_shader_Normal_AlphaBlend = null;
			_shader_Normal_Additive = null;
			_shader_Normal_SoftAdditive = null;
			_shader_Normal_Multiplicative = null;

			_shader_Clipped_AlphaBlend = null;
			_shader_Clipped_Additive = null;
			_shader_Clipped_SoftAdditive = null;
			_shader_Clipped_Multiplicative = null;

			_shader_L_Normal_AlphaBlend = null;
			_shader_L_Normal_Additive = null;
			_shader_L_Normal_SoftAdditive = null;
			_shader_L_Normal_Multiplicative = null;

			_shader_L_Clipped_AlphaBlend = null;
			_shader_L_Clipped_Additive = null;
			_shader_L_Clipped_SoftAdditive = null;
			_shader_L_Clipped_Multiplicative = null;

			_shader_AlphaMask = null;


			//변경 : 22.7.11 : Shader 에셋도 복사
			if(srcMat._shader_Normal_AlphaBlend != null)		{ _shader_Normal_AlphaBlend =		srcMat._shader_Normal_AlphaBlend; }
			if(srcMat._shader_Normal_Additive != null)			{ _shader_Normal_Additive =			srcMat._shader_Normal_Additive; }
			if(srcMat._shader_Normal_SoftAdditive != null)		{ _shader_Normal_SoftAdditive =		srcMat._shader_Normal_SoftAdditive; }
			if(srcMat._shader_Normal_Multiplicative != null)	{ _shader_Normal_Multiplicative =	srcMat._shader_Normal_Multiplicative; }

			if(srcMat._shader_Clipped_AlphaBlend != null)		{ _shader_Clipped_AlphaBlend =		srcMat._shader_Clipped_AlphaBlend; }
			if(srcMat._shader_Clipped_Additive != null)			{ _shader_Clipped_Additive =		srcMat._shader_Clipped_Additive; }
			if(srcMat._shader_Clipped_SoftAdditive != null)		{ _shader_Clipped_SoftAdditive =	srcMat._shader_Clipped_SoftAdditive; }
			if(srcMat._shader_Clipped_Multiplicative != null)	{ _shader_Clipped_Multiplicative =	srcMat._shader_Clipped_Multiplicative; }

			if(srcMat._shader_L_Normal_AlphaBlend != null)		{ _shader_L_Normal_AlphaBlend =		srcMat._shader_L_Normal_AlphaBlend; }
			if(srcMat._shader_L_Normal_Additive != null)		{ _shader_L_Normal_Additive =		srcMat._shader_L_Normal_Additive; }
			if(srcMat._shader_L_Normal_SoftAdditive != null)	{ _shader_L_Normal_SoftAdditive =	srcMat._shader_L_Normal_SoftAdditive; }
			if(srcMat._shader_L_Normal_Multiplicative != null)	{ _shader_L_Normal_Multiplicative = srcMat._shader_L_Normal_Multiplicative; }

			if(srcMat._shader_L_Clipped_AlphaBlend != null)		{ _shader_L_Clipped_AlphaBlend =	srcMat._shader_L_Clipped_AlphaBlend; }
			if(srcMat._shader_L_Clipped_Additive != null)		{ _shader_L_Clipped_Additive =		srcMat._shader_L_Clipped_Additive; }
			if(srcMat._shader_L_Clipped_SoftAdditive != null)	{ _shader_L_Clipped_SoftAdditive =	srcMat._shader_L_Clipped_SoftAdditive; }
			if(srcMat._shader_L_Clipped_Multiplicative != null)	{ _shader_L_Clipped_Multiplicative = srcMat._shader_L_Clipped_Multiplicative; }

			if(srcMat._shaderPath_AlphaMask != null)			{ _shader_AlphaMask =				srcMat._shader_AlphaMask; }

			_shaderPath_Normal_AlphaBlend =		srcMat._shaderPath_Normal_AlphaBlend;
			_shaderPath_Normal_Additive =		srcMat._shaderPath_Normal_Additive;
			_shaderPath_Normal_SoftAdditive =	srcMat._shaderPath_Normal_SoftAdditive;
			_shaderPath_Normal_Multiplicative = srcMat._shaderPath_Normal_Multiplicative;
			_shaderPath_Clipped_AlphaBlend =	srcMat._shaderPath_Clipped_AlphaBlend;
			_shaderPath_Clipped_Additive =		srcMat._shaderPath_Clipped_Additive;
			_shaderPath_Clipped_SoftAdditive =	srcMat._shaderPath_Clipped_SoftAdditive;
			_shaderPath_Clipped_Multiplicative = srcMat._shaderPath_Clipped_Multiplicative;
			_shaderPath_L_Normal_AlphaBlend =	srcMat._shaderPath_L_Normal_AlphaBlend;
			_shaderPath_L_Normal_Additive =		srcMat._shaderPath_L_Normal_Additive;
			_shaderPath_L_Normal_SoftAdditive = srcMat._shaderPath_L_Normal_SoftAdditive;
			_shaderPath_L_Normal_Multiplicative =	srcMat._shaderPath_L_Normal_Multiplicative;
			_shaderPath_L_Clipped_AlphaBlend =	srcMat._shaderPath_L_Clipped_AlphaBlend;
			_shaderPath_L_Clipped_Additive =	srcMat._shaderPath_L_Clipped_Additive;
			_shaderPath_L_Clipped_SoftAdditive =	srcMat._shaderPath_L_Clipped_SoftAdditive;
			_shaderPath_L_Clipped_Multiplicative =	srcMat._shaderPath_L_Clipped_Multiplicative;
			_shaderPath_AlphaMask =				srcMat._shaderPath_AlphaMask;

			if(_propertySets == null)
			{
				_propertySets = new List<PropertySet>();
			}
			_propertySets.Clear();

			_isNeedToSetBlackColoredAmbient = srcMat._isNeedToSetBlackColoredAmbient;

			//v1.5.1 : 참조용 재질
			_referenceMat = srcMat._referenceMat;
			_referenceMaterialPath = srcMat._referenceMaterialPath;

#if UNITY_EDITOR
			LoadShaderAssets();
			LoadRefMaterial();
#endif

			//프로퍼티 복사
			int nSrcProps = srcMat._propertySets != null ? srcMat._propertySets.Count : 0;
			if(nSrcProps > 0)
			{
				for (int i = 0; i < nSrcProps; i++)
				{
					PropertySet newPropSet = new PropertySet();
					newPropSet.CopyFromSrc(srcMat._propertySets[i]);

					//이전
					//_propertySets.Add(newPropSet);

					//변경 v1.5.1 : 중복 체크 후 추가
					AddProperty(newPropSet);
				}
			}
			
		}



		// Save / Load
		//-----------------------------------------------
		public void Save(StreamWriter sw)
		{
			try
			{
				//KEY 4글자 + 값
				sw.WriteLine("NAME" + _name);
				sw.WriteLine("UNID" + _uniqueID);

				sw.WriteLine("ICON" + (int)_icon);

				//  G   /     L  +    N   /   C     +     AB    /   AD   /    SA      /    MP
				//Gamma / Linear + Noraml / Clipped + AlphaBlend/Additive/SoftAdditive/Multiplicative
				sw.WriteLine("GNAB" + _shaderPath_Normal_AlphaBlend);
				sw.WriteLine("GNAD" + _shaderPath_Normal_Additive);
				sw.WriteLine("GNSA" + _shaderPath_Normal_SoftAdditive);
				sw.WriteLine("GNMP" + _shaderPath_Normal_Multiplicative);
				sw.WriteLine("GCAB" + _shaderPath_Clipped_AlphaBlend);
				sw.WriteLine("GCAD" + _shaderPath_Clipped_Additive);
				sw.WriteLine("GCSA" + _shaderPath_Clipped_SoftAdditive);
				sw.WriteLine("GCMP" + _shaderPath_Clipped_Multiplicative);

				sw.WriteLine("LNAB" + _shaderPath_L_Normal_AlphaBlend);
				sw.WriteLine("LNAD" + _shaderPath_L_Normal_Additive);
				sw.WriteLine("LNSA" + _shaderPath_L_Normal_SoftAdditive);
				sw.WriteLine("LNMP" + _shaderPath_L_Normal_Multiplicative);
				sw.WriteLine("LCAB" + _shaderPath_L_Clipped_AlphaBlend);
				sw.WriteLine("LCAD" + _shaderPath_L_Clipped_Additive);
				sw.WriteLine("LCSA" + _shaderPath_L_Clipped_SoftAdditive);
				sw.WriteLine("LCMP" + _shaderPath_L_Clipped_Multiplicative);

				sw.WriteLine("MASK" + _shaderPath_AlphaMask);

				sw.WriteLine("AMBC" + (_isNeedToSetBlackColoredAmbient ? "true" : "false"));

				//v1.5.1 : 참조 재질 경로
				sw.WriteLine("REFM" + _referenceMaterialPath);

				//Prop은.. 구분자로 개수 확인
				sw.WriteLine("PROP" + _propertySets.Count);

				for (int i = 0; i < _propertySets.Count; i++)
				{
					PropertySet propSet = _propertySets[i];

					sw.WriteLine("PNAM" + propSet._name);
					sw.WriteLine("PTYP" + (int)propSet._propType);
					sw.WriteLine("PTRV" + (propSet._isReserved ? "true" : "false"));
					sw.WriteLine("PVFL" + propSet._value_Float);
					sw.WriteLine("PVIT" + propSet._value_Int);
					sw.WriteLine("PVVX" + propSet._value_Vector.x);
					sw.WriteLine("PVVY" + propSet._value_Vector.y);
					sw.WriteLine("PVVZ" + propSet._value_Vector.z);
					sw.WriteLine("PVVW" + propSet._value_Vector.w);
					sw.WriteLine("PVCR" + propSet._value_Color.r);
					sw.WriteLine("PVCG" + propSet._value_Color.g);
					sw.WriteLine("PVCB" + propSet._value_Color.b);
					sw.WriteLine("PVCA" + propSet._value_Color.a);

					sw.WriteLine("PCMT" + (propSet._isCommonTexture ? "true" : "false"));
					sw.WriteLine("PCTP" + propSet._commonTexturePath);

					sw.WriteLine(">>>>>>>>");//구분자 : 이 구문을 만나면 파싱한 PropSet을 리스트에 넣자
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("MaterialPreset Write Exception : " + ex);
			}
		}


		public void Load(List<string> loadedStringSet)
		{

			_name = "";
			_uniqueID = -1;

			string strKey = "";
			string strValue = "";
			string strCur = "";

			if(_propertySets == null)
			{
				_propertySets = new List<PropertySet>();
			}
			_propertySets.Clear();
			int nPropSets = 0;

			PropertySet newPropSet = null;

			for (int i = 0; i < loadedStringSet.Count; i++)
			{
				strCur = loadedStringSet[i];
				if (strCur.Length < 4)
				{ continue; }

				//Key가 4글자
				//나머지가 Value
				strKey = strCur.Substring(0, 4);

				if (strCur.Length > 4)
				{
					strValue = strCur.Substring(4);
				}
				else
				{
					strValue = "";
				}

				try
				{
					if(strKey == "NAME") { _name = strValue; }
					else if(strKey == "UNID")
					{
						_uniqueID = int.Parse(strValue);
						//if(_uniqueID < 10)
						//{
						//	_isReserved = true;
						//}
						//else
						//{
						//	_isReserved = false;
						//}
					}
					else if(strKey == "ICON") { _icon = (ICON)(int.Parse(strValue)); }

					else if(strKey == "GNAB") { _shaderPath_Normal_AlphaBlend = strValue; }
					else if(strKey == "GNAD") { _shaderPath_Normal_Additive = strValue; }
					else if(strKey == "GNSA") { _shaderPath_Normal_SoftAdditive = strValue; }
					else if(strKey == "GNMP") { _shaderPath_Normal_Multiplicative = strValue; }
					else if(strKey == "GCAB") { _shaderPath_Clipped_AlphaBlend = strValue; }
					else if(strKey == "GCAD") { _shaderPath_Clipped_Additive = strValue; }
					else if(strKey == "GCSA") { _shaderPath_Clipped_SoftAdditive = strValue; }
					else if(strKey == "GCMP") { _shaderPath_Clipped_Multiplicative = strValue; }

					else if(strKey == "LNAB") { _shaderPath_L_Normal_AlphaBlend = strValue; }
					else if(strKey == "LNAD") { _shaderPath_L_Normal_Additive = strValue; }
					else if(strKey == "LNSA") { _shaderPath_L_Normal_SoftAdditive = strValue; }
					else if(strKey == "LNMP") { _shaderPath_L_Normal_Multiplicative = strValue; }
					else if(strKey == "LCAB") { _shaderPath_L_Clipped_AlphaBlend = strValue; }
					else if(strKey == "LCAD") { _shaderPath_L_Clipped_Additive = strValue; }
					else if(strKey == "LCSA") { _shaderPath_L_Clipped_SoftAdditive = strValue; }
					else if(strKey == "LCMP") { _shaderPath_L_Clipped_Multiplicative = strValue; }

					else if(strKey == "MASK") { _shaderPath_AlphaMask = strValue; }
					else if(strKey == "AMBC") { _isNeedToSetBlackColoredAmbient = strValue.Contains("true"); }

					//v1.5.1 : 참조 재질 경로
					else if(strKey == "REFM") { _referenceMaterialPath = strValue; }

					else if(strKey == "PROP")
					{
						nPropSets = int.Parse(strValue);

						if(nPropSets > 0)
						{
							//새로운 PropSet을 만든다.
							newPropSet = new PropertySet();
						}
					}
					else if(strKey == "PNAM") { if(newPropSet != null) { newPropSet._name = strValue; } }
					else if(strKey == "PTYP") { if(newPropSet != null) { newPropSet._propType = (SHADER_PROP_TYPE)(int.Parse(strValue)); } }
					else if(strKey == "PTRV") { if(newPropSet != null) { newPropSet._isReserved = strValue.Contains("true"); } }
					else if(strKey == "PVFL") { if(newPropSet != null) { newPropSet._value_Float = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVIT") { if(newPropSet != null) { newPropSet._value_Int = int.Parse(strValue); } }
					else if(strKey == "PVVX") { if(newPropSet != null) { newPropSet._value_Vector.x = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVVY") { if(newPropSet != null) { newPropSet._value_Vector.y = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVVZ") { if(newPropSet != null) { newPropSet._value_Vector.z = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVVW") { if(newPropSet != null) { newPropSet._value_Vector.w = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCR") { if(newPropSet != null) { newPropSet._value_Color.r = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCG") { if(newPropSet != null) { newPropSet._value_Color.g = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCB") { if(newPropSet != null) { newPropSet._value_Color.b = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCA") { if(newPropSet != null) { newPropSet._value_Color.a = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PCMT") { if(newPropSet != null) { newPropSet._isCommonTexture = strValue.Contains("true"); } }
					else if(strKey == "PCTP") { if(newPropSet != null) { newPropSet._commonTexturePath = strValue; } }
					else if(strKey == ">>>>")
					{
						//지금까지 만든 PropSet을 리스트에 넣자.
						if (newPropSet != null)
						{
							//이전
							//_propertySets.Add(newPropSet);

							//변경 v1.5.1 : 중복 체크 후 추가
							AddProperty(newPropSet);

							newPropSet = new PropertySet();//새로운 PropSet 생성
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("MaterialSet Load Exception : " + ex);
				}
			}

			if(newPropSet != null)
			{
				newPropSet = null;
			}
		}


		// 중복 체크용 함수
		//-----------------------------------------------
		/// <summary>
		/// [v1.5.1] 중복된 프로퍼티를 삭제한다. 버그 해결용
		/// </summary>
		public void CheckAndRemoveDuplicatedProperties()
		{
			int nProps = _propertySets != null ? _propertySets.Count : 0;
			if(nProps == 0)
			{
				return;
			}

			HashSet<string> checkPropNames = new HashSet<string>();//찾은 이름들
			HashSet<string> checkPropNames_Reserved = new HashSet<string>();//찾은 이름들 (Reserved 중복 체크)
			List<PropertySet> removePropSets = null;

			PropertySet curProp = null;
			for (int i = 0; i < nProps; i++)
			{
				curProp = _propertySets[i];

				//중복 체크 예외
				//- 이름이 빈칸이거나 (, )가 포함됨
				//- Reserved인 경우는 Reserved용 이름에서 비교를 해야한다.
				if(string.IsNullOrEmpty(curProp._name))
				{
					continue;
				}

				if(curProp._name.Contains("(") || curProp._name.Contains(")"))
				{
					continue;
				}

				//중복 체크를 하자
				if(curProp._isReserved)
				{
					// [ Reserved인 경우 ]
					//일반 리스트에 등록을 먼저 하자
					if(!checkPropNames.Contains(curProp._name))
					{
						checkPropNames.Add(curProp._name);
					}

					if(checkPropNames_Reserved.Contains(curProp._name))
					{
						//Reserved에도 등록된 이름이다. > 제거하자
						if(removePropSets == null)
						{
							removePropSets = new List<PropertySet>();
						}
						removePropSets.Add(curProp);
					}
					else
					{
						//등록되지 않은 이름이다.
						checkPropNames_Reserved.Add(curProp._name);
					}
				}
				else
				{
					// [ 사용자 추가 Property인 경우 ]
					if(checkPropNames.Contains(curProp._name))
					{
						//이미 등록된 이름이다. > 제거하자
						if(removePropSets == null)
						{
							removePropSets = new List<PropertySet>();
						}
						removePropSets.Add(curProp);
					}
					else
					{
						//등록되지 않은 이름이다.
						checkPropNames.Add(curProp._name);
					}
				}

				
			}

			//중복이 있다면 삭제를 하자
			int nRemoveProps = removePropSets != null ? removePropSets.Count : 0;
			if(nRemoveProps > 0)
			{
				PropertySet removeProp = null;
				for (int i = 0; i < nRemoveProps; i++)
				{
					removeProp = removePropSets[i];
					//Debug.Log("프로퍼티 삭제 - " + removeProp._name);

					_propertySets.Remove(removeProp);
				}
			}
			
		}

		// Get / Set
		//-----------------------------------------------
		public Shader GetShader(apPortrait.SHADER_TYPE shaderType, bool isClippedChild, bool isLinearSpace)
		{
			if(!isLinearSpace)
			{
				//Gamma Space
				switch (shaderType)
				{
					case apPortrait.SHADER_TYPE.AlphaBlend:
						if(!isClippedChild)		{ return _shader_Normal_AlphaBlend; }
						else					{ return _shader_Clipped_AlphaBlend;}

					case apPortrait.SHADER_TYPE.Additive:
						if(!isClippedChild)		{ return _shader_Normal_Additive; }
						else					{ return _shader_Clipped_Additive;}

					case apPortrait.SHADER_TYPE.SoftAdditive:
						if(!isClippedChild)		{ return _shader_Normal_SoftAdditive; }
						else					{ return _shader_Clipped_SoftAdditive;}

					case apPortrait.SHADER_TYPE.Multiplicative:
						if(!isClippedChild)		{ return _shader_Normal_Multiplicative; }
						else					{ return _shader_Clipped_Multiplicative;}
				}
			}
			else
			{
				//Linear Space
				switch (shaderType)
				{
					case apPortrait.SHADER_TYPE.AlphaBlend:
						if(!isClippedChild)		{ return _shader_L_Normal_AlphaBlend; }
						else					{ return _shader_L_Clipped_AlphaBlend;}

					case apPortrait.SHADER_TYPE.Additive:
						if(!isClippedChild)		{ return _shader_L_Normal_Additive; }
						else					{ return _shader_L_Clipped_Additive;}

					case apPortrait.SHADER_TYPE.SoftAdditive:
						if(!isClippedChild)		{ return _shader_L_Normal_SoftAdditive; }
						else					{ return _shader_L_Clipped_SoftAdditive;}

					case apPortrait.SHADER_TYPE.Multiplicative:
						if(!isClippedChild)		{ return _shader_L_Normal_Multiplicative; }
						else					{ return _shader_L_Clipped_Multiplicative;}
				}
			}

			return null;
		}

		/// <summary>
		/// [v1.5.1] 임시로 사용할 이름을 리턴한다. 기존에는 그냥 고정 이름을 사용했는데, 이제 중복을 없애야 해서..
		/// </summary>
		/// <returns></returns>
		public string GetTempNewName()
		{
			int cnt = 1;
			string resultName = "";
			PropertySet propSet = null;
			while(true)
			{
				resultName = "(New Property " + cnt + ")";
				propSet = GetPropertySet(resultName);
				if(propSet == null)
				{
					//중복되는게 없다. 바로 리턴
					return resultName;
				}

				cnt += 1;

				if(cnt > 100)
				{
					break;
				}
			}

			//100개 이내에서 찾을 수 없었다.
			return "(New Property ...)";
		}
	}
}