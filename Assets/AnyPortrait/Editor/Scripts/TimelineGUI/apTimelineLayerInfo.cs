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

namespace AnyPortrait
{

	public class apTimelineLayerInfo
	{
		//Members
		//-----------------------------------------------------------------------------
		public bool _isTimeline = false;
		public apAnimTimeline _timeline = null;
		public apAnimTimelineLayer _layer = null;
		public apAnimTimeline _parentTimeline = null;
		public apTimelineLayerInfo _parentInfo = null;
		/// <summary>
		/// 선택된 상태인가
		/// Timeline : Layer 중 하나라도 선택이 되었으면 True
		/// Layer : 해당 객체(Transform/Bone/ControlParam)이 선택되거나 Frame이 선택되면 True
		/// </summary>
		public bool _isSelected = false;

		/// <summary>
		/// 활성화된 상태인가
		/// 기본적으로 True. Editing 상태일 때 선택된 Timeline을 제외하고는 모두 False가 된다.
		/// </summary>
		public bool _isAvailable = false;

		public bool IsVisibleLayer
		{
			get
			{
				if (_isTimeline || _layer == null)
				{
					return true;
				}
				return _layer._guiLayerVisible && !_parentInfo.IsTimelineFolded;
			}
		}

		public enum LAYER_TYPE
		{
			Transform,
			Bone,
			ControlParam
		}
		public LAYER_TYPE _layerType = LAYER_TYPE.Transform;

		public float _guiLayerPosY = 0;
		public bool _isRenderable = false;

		//public bool _isTimelineFold = false;
		public bool IsTimelineFolded
		{
			get
			{
				if (_timeline == null || !_isTimeline)
				{
					return false;
				}
				return _timeline._guiTimelineFolded;
			}
		}


		// Init
		//-----------------------------------------------------------------------------
		public apTimelineLayerInfo(apAnimTimeline timeline)
		{
			_isTimeline = true;
			_timeline = timeline;

			_isSelected = false;
			_isAvailable = true;
		}

		public apTimelineLayerInfo(apAnimTimelineLayer timelineLayer, apAnimTimeline parentTimeline, apTimelineLayerInfo parentInfo)
		{
			_isTimeline = false;
			_layer = timelineLayer;
			_parentTimeline = parentTimeline;
			_parentInfo = parentInfo;

			_isSelected = false;
			_isAvailable = true;

			if (_layer._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				switch (_layer._linkModType)
				{
					case apAnimTimelineLayer.LINK_MOD_TYPE.None:
						_layerType = LAYER_TYPE.Transform;
						break;

					case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
					case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
						_layerType = LAYER_TYPE.Transform;
						break;

					case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
						_layerType = LAYER_TYPE.Bone;
						break;


				}
			}
			else//if(_layer._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				_layerType = LAYER_TYPE.ControlParam;
			}
		}

		public void ShowLayer()
		{
			if (_layer != null)
			{
				_layer._guiLayerVisible = true;
			}

			if (_timeline != null)
			{
				_timeline._guiTimelineFolded = false;
			}
		}

		// Get / Set
		//-----------------------------------------------------------------------------
		private static Color s_NotAvailableColor = new Color(0.2f ,0.2f, 0.2f, 1.0f);

		/// <summary>
		/// 선택 여부에 상관없는 기본 색상값 (글자색 체크때문에 사용)
		/// </summary>
		public Color GUIColor_Default
		{
			get
			{
				if (!_isAvailable) { return s_NotAvailableColor; }

				if (_isTimeline)	{ return _timeline._guiColor; }
				return _layer._guiColor;
			}
		}

		/// <summary>
		/// 선택에 따라 다른 색상이 보여질 수 있는 색상 값
		/// </summary>
		public void GetGUIColor_WithSelected(bool isFlashing, out Color bgColor, out Color keyframeColor)
		{
			if (!_isAvailable)
			{
				bgColor = s_NotAvailableColor;
				keyframeColor = s_NotAvailableColor;
				return;
			}

			Color resultColor;
			if (_isTimeline)	{ resultColor = _timeline._guiColor; }
			else				{ resultColor = _layer._guiColor; }

			bgColor = resultColor;
			keyframeColor = resultColor;

			//v1.4.7 추가
			//키프레임의 경우, 너무 어두운 경우에 한해서 밝기가 보정된다.
			float keyLum = (keyframeColor.r * 0.3f) + (keyframeColor.g * 0.6f) + (keyframeColor.b * 0.1f);
			if(keyLum < 0.4f)
			{
				if(keyLum < 0.001f)
				{
					//검은색에 가깝다면
					keyframeColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
				}
				else
				{
					//밝기를 보정하자
					keyframeColor *= 0.4f / keyLum;
				}				
			}
			
			
				
			if (_isSelected)
			{	
				//변경 v1.4.6 : 반짝반짝 거리는건 옵션 값으로 결정된다.
				if(isFlashing)
				{
					//반짝반짝 거리는 경우
					//변경 v1.4.5 : 약간 더 밝아지고, 애니메이션 효과가 추가된다.
					float animLerp = apEditorUtil.GetAnimatedHighlightWeight();//초당 0~1 사이의 값이 반복

					float lum = (bgColor.r * 0.3f) + (bgColor.g * 0.6f) + (bgColor.b * 0.1f);
					
					bgColor.r += (bgColor.r - lum) * 0.2f;
					bgColor.g += (bgColor.g - lum) * 0.2f;
					bgColor.b += (bgColor.b - lum) * 0.2f;

					bgColor *= (1.2f * (1.0f - animLerp)) + (1.8f * animLerp);//반짝반짝

					lum = (keyframeColor.r * 0.3f) + (keyframeColor.g * 0.6f) + (keyframeColor.b * 0.1f);
					
					keyframeColor.r += (keyframeColor.r - lum) * 0.2f;
					keyframeColor.g += (keyframeColor.g - lum) * 0.2f;
					keyframeColor.b += (keyframeColor.b - lum) * 0.2f;

					keyframeColor *= (1.2f * (1.0f - animLerp)) + (1.8f * animLerp);//반짝반짝

				}
				else
				{
					//이전의 하이라이팅의 경우
					//이전
					float lum = (bgColor.r + bgColor.g + bgColor.b) / 3.0f;
					//lum = (lum * 1.2f) + 0.1f;
					bgColor.r += (bgColor.r - lum) * 0.2f;
					bgColor.g += (bgColor.g - lum) * 0.2f;
					bgColor.b += (bgColor.b - lum) * 0.2f;

					bgColor *= 1.4f;

					lum = (keyframeColor.r + keyframeColor.g + keyframeColor.b) / 3.0f;
					keyframeColor.r += (keyframeColor.r - lum) * 0.2f;
					keyframeColor.g += (keyframeColor.g - lum) * 0.2f;
					keyframeColor.b += (keyframeColor.b - lum) * 0.2f;

					keyframeColor *= 1.4f;
				}
			}

			bgColor.a = 1.0f;
			keyframeColor.a = 1.0f;
		}

		//public void SetGUIColor(Color guiColor)
		//{
		//	if (_isTimeline) { _timeline._guiColor = guiColor; }
		//	else { _layer._guiColor = guiColor; }
		//}

		public Color GetTimelineColor(bool isFlashing)
		{
			if (!_isAvailable || !_isSelected) { return s_NotAvailableColor; }

			Color resultColor = Color.black;
			if (_isTimeline)	{ resultColor = _timeline._guiColor; }
			else				{ resultColor = _layer._guiColor; }

			float lum = (resultColor.r * 0.3f + resultColor.g * 0.6f + resultColor.b * 0.1f);
			//밝기를 보고, 0.25 근처가 되도록 만들자
			if (lum < 0.001f)
			{
				return new Color(0.23f, 0.23f, 0.23f);
			}

			float colorMul = 0.23f / lum;//어두우면 밝아지고, 너무 밝으면 줄어들도록

			resultColor.r *= colorMul;
			resultColor.g *= colorMul;
			resultColor.b *= colorMul;

			if (isFlashing)
			{
				//약간의 색상 반짝임 효과를 넣는다 [v1.4.5]
				float animLerp = apEditorUtil.GetAnimatedHighlightWeight();
				resultColor *= (0.9f * (1.0f - animLerp)) + (1.2f * animLerp);
			}
			resultColor.a = 1.0f;

			return resultColor;
		}

		public apImageSet.PRESET IconImgType
		{
			get
			{
				if (_isTimeline)
				{
					switch (_timeline._linkType)
					{
						case apAnimClip.LINK_TYPE.AnimatedModifier:
							return apImageSet.PRESET.Anim_WithMod;
						//case apAnimClip.LINK_TYPE.Bone: return apImageSet.PRESET.Anim_WithBone;
						case apAnimClip.LINK_TYPE.ControlParam:
							return apImageSet.PRESET.Anim_WithControlParam;
					}
				}
				else
				{

					switch (_parentTimeline._linkType)
					{
						case apAnimClip.LINK_TYPE.AnimatedModifier:
							if (_layer._linkedMeshTransform != null)
							{
								return apImageSet.PRESET.Hierarchy_Mesh;
							}
							if (_layer._linkedMeshGroupTransform != null)
							{
								return apImageSet.PRESET.Hierarchy_MeshGroup;
							}
							if (_layer._linkedBone != null)
							{
								return apImageSet.PRESET.Hierarchy_Bone;
							}
							return apImageSet.PRESET.Hierarchy_Modifier;
						//case apAnimClip.LINK_TYPE.Bone: return apImageSet.PRESET.Modifier_Rigging;
						case apAnimClip.LINK_TYPE.ControlParam:
							{
								if (_layer._linkedControlParam != null)
								{
									return apEditorUtil.GetControlParamPresetIconType(_layer._linkedControlParam._iconPreset);
								}
							}
							return apImageSet.PRESET.Hierarchy_Param;
					}
				}
				return apImageSet.PRESET.Edit_Record;
			}
		}

		public string DisplayName
		{
			get
			{
				if (_isTimeline)
				{ return _timeline.DisplayName; }
				else
				{ return _layer.DisplayName; }
			}
		}

		public int Depth
		{
			get
			{
				if (_layerType == LAYER_TYPE.ControlParam)
				{
					return 0;
				}
				else if (_layerType == LAYER_TYPE.Transform)
				{
					if (_layer._linkedMeshTransform != null &&
						_layer._linkedMeshTransform._linkedRenderUnit != null)
					{
						return _layer._linkedMeshTransform._linkedRenderUnit._guiIndex;
					}

					if (_layer._linkedMeshGroupTransform != null &&
						_layer._linkedMeshGroupTransform._linkedRenderUnit != null)
					{
						return _layer._linkedMeshGroupTransform._linkedRenderUnit._guiIndex;
					}
				}
				else
				{
					if (_layer._linkedBone != null)
					{
						return _layer._linkedBone._recursiveIndex;
					}
				}

				return 0;
			}
		}
	}

}