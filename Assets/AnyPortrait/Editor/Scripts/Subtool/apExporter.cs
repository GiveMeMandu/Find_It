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


using AnyPortrait;
using NGIFforUnity;
using UnityEditor;

#if UNITY_2017_4_OR_NEWER
using UnityEditor.Media;
#endif

namespace AnyPortrait
{

	/// <summary>
	/// Editor에 포함되어서 Export를 담당한다.
	/// Texture Render / GIF Export / 백업용 Txt
	
	/// </summary>
	public class apExporter
	{
		// Members
		//----------------------------------------------
		private apEditor _editor = null;
		private RenderTexture _renderTexture = null;
		private RenderTexture _renderTexture_GrayscaleAlpha = null;

		private apGL.WindowParameters _glWindowParam = new apGL.WindowParameters();
		
		//Step 전용 변수
		private apNGIFforUnity _ngif = new apNGIFforUnity();
		private string _gif_FilePath = "";
		
		public string GIF_FilePath { get { return _gif_FilePath; } }

		private FileStream _gifFileStream = null;

		//추가 : 11.5
#if UNITY_2017_4_OR_NEWER
		private MediaEncoder _mediaEncoder = null;
		private VideoTrackAttributes _videoAttr;

		private int _dstMP4SizeWidth = 0;
		private int _dstMP4SizeHeight = 0;
#endif


		//RT, Clip 캘리브레이션 캐시
		//RT, Clip 캘리브레이션은 렌더 텍스쳐를 생성해야하므로, 작업이 다소 오래 걸린다.
		//같은 요청값을 바탕으로 캡쳐가 연속으로 실행된다면
		//캐시를 하면 될 것 같다.
		private bool _isCalibrationCache = false;
		
		//조건이 모두 맞아야 하며, 하나라도 맞지 않는다면 캐시는 해제된다.

		//캐시 조건값
		private int _caliCache_Cond_EditorSize_Width = 0;
		private int _caliCache_Cond_EditorSize_Height = 0;
		private float _caliCache_Cond_ClipRectCenterPosX = 0.0f;//float이므로 작은 값의 범위 비교
		private float _caliCache_Cond_ClipRectCenterPosY = 0.0f;//float이므로 작은 값의 범위 비교
		private int _caliCache_Cond_ClipSizeWidth = 0;
		private int _caliCache_Cond_ClipSizeHeight = 0;
		private int _caliCache_Cond_DstSizeWidth = 0;
		private int _caliCache_Cond_DstSizeHeight = 0;

		//캐시 결과값
		private int _caliCache_Result_RTSize_Width = 0;
		private int _caliCache_Result_RTSize_Height = 0;
		private int _caliCache_Result_ClipArea_PosX = 0;
		private int _caliCache_Result_ClipArea_PosY = 0;
		private int _caliCache_Result_ClipArea_Width = 0;
		private int _caliCache_Result_ClipArea_Height = 0;


		// Init
		//----------------------------------------------
		public apExporter(apEditor editor)
		{
			_editor = editor;
			
		}

		public void Clear()
		{
#if UNITY_2017_4_OR_NEWER
			if(_mediaEncoder != null)
			{
				try
				{
					_mediaEncoder.Dispose();
					_mediaEncoder = null;
				}
				catch(Exception ex)
				{
					Debug.LogError("AnyPortrait : MovieExporter Clear Exception : " + ex);
				}
				_mediaEncoder = null;
			}
#endif
		}


		/// <summary>
		/// 이전 기록중 캐싱된 기록을 제거한다.
		/// Sequence의 처음 또는 모든 렌더링 시에 호출하자
		/// </summary>
		public void ClearCache()
		{
			_isCalibrationCache = false;
		}


		// Functions
		//----------------------------------------------
		public Texture2D RenderToTexture(apMeshGroup meshGroup,
										int winPosX, int winPosY,
										int srcSizeWidth, int srcSizeHeight,
										int dstSizeWidth, int dstSizeHeight,
										Color clearColor)
		{
			if (_editor == null)
			{
				return null;
			}


			// 1. 렌더링을 한다.
			//--------------------------------------------------------------------

			//기존 방식 : 전체 화면을 기준으로 모두 렌더링을 하고(>RenderTexture) -> Texture2D에 클리핑해서 적용한다.
			//변경 방식 : 미리 클리핑 영역으로 화면 포커스를 이동한 후, Dst만큼 확대한다 -> 그대로 Texture2D에 적용한다.
			//apGL의 Window Size를 바꾸어준다.

			//-------------------->> 기존 방식

			#region [미사용 코드]
			//int rtSizeWidth = ((int)_editor.position.width);
			//int rtSizeHeight = ((int)_editor.position.height);

			////winPosY -= 10;
			//int guiOffsetX = apGL._posX_NotCalculated;
			//int guiOffsetY = apGL._posY_NotCalculated;

			//int clipPosX = winPosX - (srcSizeWidth / 2);
			//int clipPosY = winPosY - (srcSizeHeight / 2);

			//clipPosX += guiOffsetX;
			//clipPosY += guiOffsetY + 15;

			//int clipPosX_Right = clipPosX + srcSizeWidth;
			//int clipPosY_Bottom = clipPosY + srcSizeHeight;

			//if (clipPosX < 0)		{ clipPosX = 0; }
			//if (clipPosY < 0)		{ clipPosY = 0; }
			//if (clipPosX_Right > rtSizeWidth)	{ clipPosX_Right = rtSizeWidth; }
			//if (clipPosY_Bottom > rtSizeHeight)	{ clipPosY_Bottom = rtSizeHeight; }

			//int clipWidth = (clipPosX_Right - clipPosX);
			//int clipHeight = (clipPosY_Bottom - clipPosY);
			//if (clipWidth <= 0 || clipHeight <= 0)
			//{
			//	Debug.LogError("RenderToTexture Failed : Clip Area is over Screen");
			//	return null;
			//}

			//meshGroup.RefreshForce();
			//meshGroup.UpdateRenderUnits(0.0f, true);



			////Pass-1. 일반 + MaskParent를 Alpha2White 렌더링. 이걸로 나중에 알파 채널용 텍스쳐를 만든다.
			////--------------------------------------------------------------------------------------------------------
			//_renderTexture_GrayscaleAlpha = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture_GrayscaleAlpha.antiAliasing = 1;
			//_renderTexture_GrayscaleAlpha.wrapMode = TextureWrapMode.Clamp;

			//RenderTexture.active = null;
			//RenderTexture.active = _renderTexture_GrayscaleAlpha;

			////기본 
			//Color maskClearColor = new Color(clearColor.a, clearColor.a, clearColor.a, 1.0f);
			//GL.Clear(false, true, maskClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			//apGL.DrawBoxGL(Vector2.zero, 50000, 50000, maskClearColor, false, true);//<<이걸로 배경을 깔자
			//GL.Flush();

			////System.Threading.Thread.Sleep(50);

			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
			//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			//	{
			//		if (renderUnit._meshTransform != null)
			//		{
			//			if (renderUnit._meshTransform._isClipping_Parent)
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					//RenderTexture.active = _renderTexture_GrayscaleAlpha;
			//					apGL.DrawRenderUnit_Basic_Alpha2White(renderUnit);
			//				}
			//			}
			//			else if (renderUnit._meshTransform._isClipping_Child)
			//			{
			//				//Pass
			//				//Alpha 렌더링에서 Clipping Child는 제외한다. 어차피 Parent의 Alpha보다 많을 수 없으니..
			//			}
			//			else
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					//RenderTexture.active = _renderTexture_GrayscaleAlpha;
			//					apGL.DrawRenderUnit_Basic_Alpha2White(renderUnit);
			//				}
			//			}
			//		}
			//	}
			//}

			//System.Threading.Thread.Sleep(5);

			//Texture2D resultTex_SrcSize_Alpha = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			//resultTex_SrcSize_Alpha.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);
			//resultTex_SrcSize_Alpha.Apply();


			////Pass-2. 기본 렌더링
			////--------------------------------------------------------------------------------------------------------
			////1. Clip Parent의 MaskTexture를 미리 구워서 Dictionary에 넣는다.
			//Dictionary<apRenderUnit, Texture2D> bakedClipMaskTextures = new Dictionary<apRenderUnit, Texture2D>();


			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
			//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			//	{
			//		if (renderUnit._meshTransform != null)
			//		{
			//			if (renderUnit._meshTransform._isClipping_Parent)
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					Texture2D clipMaskTex = apGL.GetMaskTexture_ClippingParent(renderUnit);
			//					if (clipMaskTex != null)
			//					{
			//						bakedClipMaskTextures.Add(renderUnit, clipMaskTex);
			//					}
			//					else
			//					{
			//						Debug.LogError("Clip Testure Bake Failed");
			//					}

			//				}
			//			}
			//		}
			//	}
			//}

			//System.Threading.Thread.Sleep(5);


			//_renderTexture = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture.antiAliasing = 1;
			//_renderTexture.wrapMode = TextureWrapMode.Clamp;

			//RenderTexture.active = null;
			//RenderTexture.active = _renderTexture;

			//Color opaqueClearColor = new Color(clearColor.r * clearColor.a, clearColor.g * clearColor.a, clearColor.b * clearColor.a, 1.0f);

			////GL.Clear(true, true, clearColor, -100.0f);//이전
			//GL.Clear(false, true, opaqueClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			//apGL.DrawBoxGL(Vector2.zero, 50000, 50000, opaqueClearColor, false, true);//<<이걸로 배경을 깔자
			//GL.Flush();

			////System.Threading.Thread.Sleep(50);

			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
			//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			//	{
			//		if (renderUnit._meshTransform != null)
			//		{
			//			if (renderUnit._meshTransform._isClipping_Parent)
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					if (bakedClipMaskTextures.ContainsKey(renderUnit))
			//					{
			//						apGL.DrawRenderUnit_ClippingParent_Renew_WithoutRTT(renderUnit,
			//									renderUnit._meshTransform._clipChildMeshes,
			//									bakedClipMaskTextures[renderUnit]);
			//					}


			//					////RenderTexture.active = _renderTexture;//<<클리핑 뒤에는 다시 연결해줘야한다.
			//				}
			//			}
			//			else if (renderUnit._meshTransform._isClipping_Child)
			//			{
			//				//Pass
			//			}
			//			else
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					RenderTexture.active = _renderTexture;
			//					apGL.DrawRenderUnit_Basic(renderUnit);
			//				}
			//			}
			//		}
			//	}
			//}

			//System.Threading.Thread.Sleep(5);


			//Texture2D resultTex_SrcSize = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			//resultTex_SrcSize.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);

			//resultTex_SrcSize.Apply(); 
			#endregion

			//--------------------<< 기존 방식
			

			//-------------------->> 새로운 방식2

			apGL.GetWindowParameters(_glWindowParam);

			int rtSizeWidth = ((int)_editor.position.width);
			int rtSizeHeight = ((int)_editor.position.height);

			//winPosY -= 10;
			int guiOffsetX = apGL._posX_NotCalculated;
			int guiOffsetY = apGL._posY_NotCalculated;

			int clipPosX = winPosX - (srcSizeWidth / 2);
			int clipPosY = winPosY - (srcSizeHeight / 2);

			clipPosX += guiOffsetX;
			clipPosY += guiOffsetY + 15;

			int clipPosX_Right = clipPosX + srcSizeWidth;
			int clipPosY_Bottom = clipPosY + srcSizeHeight;

			if (clipPosX < 0)	{ clipPosX = 0; }
			if (clipPosY < 0)	{ clipPosY = 0; }
			if (clipPosX_Right > rtSizeWidth)	{ clipPosX_Right = rtSizeWidth; }
			if (clipPosY_Bottom > rtSizeHeight)	{ clipPosY_Bottom = rtSizeHeight; }

			int clipWidth = (clipPosX_Right - clipPosX);
			int clipHeight = (clipPosY_Bottom - clipPosY);
			if (clipWidth <= 0 || clipHeight <= 0)
			{
				Debug.LogError("RenderToTexture Failed : Clip Area is over Screen");
				return null;
			}

			clipPosY += 2;

			//크기 왜곡 이슈 > 테스트로 숨겨본다.
			clipHeight = (int)(clipHeight * 0.979f);//<<TODO 이게 비율인지 절대값인지 확인 필요
			srcSizeHeight = (int)(srcSizeHeight * 0.979f);

			if(dstSizeWidth != srcSizeWidth || dstSizeHeight != srcSizeHeight)
			{
				float resizeRatio_X = (float)dstSizeWidth / (float)srcSizeWidth;
				float resizeRatio_Y = (float)dstSizeHeight / (float)srcSizeHeight;
				float resizeRatio = 1.0f;
				if(resizeRatio_X > 1.0f || resizeRatio_Y > 1.0f)
				{
					//하나라도 증가하는 방향이면, 더 크게 확대하는 방향으로 설정
					resizeRatio = Mathf.Max(resizeRatio_X, resizeRatio_Y, 1.0f);
				}
				else
				{
					//둘다 감소(혹은 유지)라면 더 많이 축소하는 방향으로 설정
					resizeRatio = Mathf.Min(resizeRatio_X, resizeRatio_Y, 1.0f);
				}
				

				//크기 제한이 있다.
				//너무 크거나 너무 작아진다면 제한해야한다.
				int expectedRtSizeWidth = (int)(rtSizeWidth * resizeRatio);
				int expectedRtSizeHeight = (int)(rtSizeHeight * resizeRatio);
				if(resizeRatio > 1.0f)
				{
					float limitedRatioX = resizeRatio;
					float limitedRatioY = resizeRatio;
					bool isSizeLimited = false;
					if(expectedRtSizeWidth > 4096)
					{
						//Debug.Log("Width is Over 4096");
						limitedRatioX = (4096.0f / (float)rtSizeWidth);
						isSizeLimited = true;
					}
					if (expectedRtSizeHeight > 4096)
					{
						//Debug.Log("Height is Over 4096");
						limitedRatioY = (4096.0f / (float)rtSizeHeight);
						isSizeLimited = true;
					}

					if (isSizeLimited)
					{
						//확대되어 제한된 만큼, 작은 쪽으로 ResizeRatio를 맞춘다.
						resizeRatio = Mathf.Min(limitedRatioX, limitedRatioY);
						//Debug.Log("Limited Ratio : " + resizeRatio);
					}
				}
				else if(resizeRatio < 1.0f)
				{
					float limitedRatioX = resizeRatio;
					float limitedRatioY = resizeRatio;
					bool isSizeLimited = false;

					if(expectedRtSizeWidth < 256)
					{
						//Debug.Log("Width is Under 256");
						limitedRatioX = (256.0f / (float)rtSizeWidth);
						isSizeLimited = true;
					}
					if (expectedRtSizeHeight < 256)
					{
						//Debug.Log("Height is Under 256");
						limitedRatioY = (256.0f / (float)rtSizeHeight);
						isSizeLimited = true;
					}
					if (isSizeLimited)
					{
						//축소되어 제한된 만큼, 큰 쪽으로 ResizeRatio를 맞춘다.
						resizeRatio = Mathf.Max(limitedRatioX, limitedRatioY);
						//Debug.Log("Limited Ratio : " + resizeRatio);
					}
				}
				

				//Debug.Log("Rescale Ratio : " + resizeRatio);

				Vector2 scrollPos_Prev = apGL._windowScroll;
				Vector2 scrollPos_InEditor = scrollPos_Prev + new Vector2(apGL._posX_NotCalculated, apGL._posY_NotCalculated) + apGL.WindowSizeHalf;
				scrollPos_InEditor *= resizeRatio;
				scrollPos_InEditor -= (new Vector2(apGL._posX_NotCalculated, apGL._posY_NotCalculated) + apGL.WindowSizeHalf);

				//apGL._windowScroll = scrollPos_InEditor;
				//apGL._zoom /= resizeRatio;

				//apGL._windowWidth = (int)(apGL._windowWidth * resizeRatio);
				//apGL._windowHeight = (int)(apGL._windowHeight * resizeRatio);
				apGL.SetScreenClippingSizeTmp(new Vector4(-100, -100, 200, 200));
				rtSizeWidth = (int)(rtSizeWidth * resizeRatio);
				rtSizeHeight = (int)(rtSizeHeight * resizeRatio);
				apGL._posX_NotCalculated = (int)(apGL._posX_NotCalculated * resizeRatio);
				apGL._posY_NotCalculated = (int)(apGL._posY_NotCalculated * resizeRatio);

				clipPosX = (int)(clipPosX * resizeRatio);
				clipPosY = (int)(clipPosY * resizeRatio);
				clipWidth = (int)(clipWidth * resizeRatio);
				clipHeight = (int)(clipHeight * resizeRatio);

				srcSizeWidth = (int)(srcSizeWidth * resizeRatio);
				srcSizeHeight = (int)(srcSizeHeight * resizeRatio);
			}


			meshGroup.RefreshForce();
			meshGroup.UpdateRenderUnits(0.0f, true);

			//int newRtSizeWidth = GetProperRenderTextureSize(rtSizeWidth);
			//int newRtSizeHeight = GetProperRenderTextureSize(rtSizeHeight);
			//clipPosX += (newRtSizeWidth - rtSizeWidth) / 2;
			//clipPosY += (newRtSizeHeight - rtSizeHeight) / 2;
			//rtSizeWidth = newRtSizeWidth;
			//rtSizeHeight = newRtSizeHeight;

			//Pass-1. 일반 + MaskParent를 Alpha2White 렌더링. 이걸로 나중에 알파 채널용 텍스쳐를 만든다.
			//--------------------------------------------------------------------------------------------------------
			_renderTexture_GrayscaleAlpha = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture_GrayscaleAlpha.antiAliasing = 1;
			_renderTexture_GrayscaleAlpha.isPowerOfTwo = false;
			_renderTexture_GrayscaleAlpha.wrapMode = TextureWrapMode.Clamp;

			RenderTexture.active = null;
			RenderTexture.active = _renderTexture_GrayscaleAlpha;

			//기본 
			Color maskClearColor = new Color(clearColor.a, clearColor.a, clearColor.a, 1.0f);
			GL.Clear(false, true, maskClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, maskClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			//System.Threading.Thread.Sleep(50);

			//변경
			List<apRenderUnit> renderUnits = meshGroup.SortedBuffer.SortedRenderUnits;
			int nRenderUnits = renderUnits.Count;

			//변경 19.11.23 : ExtraOption-Depth에 의한 순서 변경도 적용
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White_ForExport(renderUnit);
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
							//Alpha 렌더링에서 Clipping Child는 제외한다. 어차피 Parent의 Alpha보다 많을 수 없으니..
						}
						else
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White_ForExport(renderUnit);
							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);

			Texture2D resultTex_SrcSize_Alpha = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			resultTex_SrcSize_Alpha.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);
			resultTex_SrcSize_Alpha.Apply();


			//Pass-2. 기본 렌더링
			//--------------------------------------------------------------------------------------------------------
			//1. Clip Parent의 MaskTexture를 미리 구워서 Dictionary에 넣는다.
			Dictionary<apRenderUnit, Texture2D> bakedClipMaskTextures = new Dictionary<apRenderUnit, Texture2D>();


			//변경 19.11.23 : ExtraOption-Depth인 경우 렌더링 순서가 바뀌어야한다.
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								Texture2D clipMaskTex = apGL.GetMaskTexture_ClippingParent(renderUnit);
								if (clipMaskTex != null)
								{
									bakedClipMaskTextures.Add(renderUnit, clipMaskTex);
								}
								else
								{
									Debug.LogError("Clip Testure Bake Failed");
								}

							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);


			_renderTexture = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture.antiAliasing = 1;
			_renderTexture.isPowerOfTwo = false;
			_renderTexture.wrapMode = TextureWrapMode.Clamp;

			RenderTexture.active = null;
			RenderTexture.active = _renderTexture;

			Color opaqueClearColor = new Color(clearColor.r * clearColor.a, clearColor.g * clearColor.a, clearColor.b * clearColor.a, 1.0f);

			GL.Clear(false, true, opaqueClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, opaqueClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			
			//변경 19.11.23 : ExtraOption-Depth인 경우 렌더링 순서가 바뀌어야한다.
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								if (bakedClipMaskTextures.ContainsKey(renderUnit))
								{
									apGL.DrawRenderUnit_ClippingParent_ForExport_WithoutRTT(
																			renderUnit,
																			renderUnit._meshTransform._clipChildMeshes,
																			bakedClipMaskTextures[renderUnit]
																			);
								}


								////RenderTexture.active = _renderTexture;//<<클리핑 뒤에는 다시 연결해줘야한다.
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
						}
						else
						{
							if (renderUnit._isVisible)
							{
								RenderTexture.active = _renderTexture;
								apGL.DrawRenderUnit_Basic_ForExport(renderUnit);
							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);


			Texture2D resultTex_SrcSize = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			resultTex_SrcSize.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);

			resultTex_SrcSize.Apply();



			//윈도우 크기 복구
			apGL.RecoverWindowSize(_glWindowParam);
			//--------------------<< 새로운 방식2


			RenderTexture.active = null;

			RenderTexture.ReleaseTemporary(_renderTexture_GrayscaleAlpha);
			RenderTexture.ReleaseTemporary(_renderTexture);
			
			// 2. 렌더링된 이미지를 가공한다.
			//--------------------------------------------------------------------


			_renderTexture = null;
			_renderTexture_GrayscaleAlpha = null;
			Texture2D resultTex_DstSize = null;

			//추가 : 가장자리 알파 문제를 수정하자
			//int blurSize = Mathf.Max(4, (dstSizeWidth / srcSizeWidth) + 1, (dstSizeHeight / srcSizeHeight) + 1);

			//Texture2D resultTex_FixAlphaBorder = MakeBlurImage(resultTex_SrcSize, resultTex_SrcSize_Alpha, blurSize);//<<이거 빼자

			if (dstSizeWidth != srcSizeWidth || dstSizeHeight != srcSizeHeight)
			{
				if(SystemInfo.supportsComputeShaders)
				{
					resultTex_DstSize = ResizeTextureWithComputeShader(resultTex_SrcSize, resultTex_SrcSize_Alpha, /*resultTex_FixAlphaBorder,*/ dstSizeWidth, dstSizeHeight);
				}

				if(resultTex_DstSize == null)
				{
					resultTex_DstSize = ResizeTexture(resultTex_SrcSize, resultTex_SrcSize_Alpha, /*resultTex_FixAlphaBorder,*/ dstSizeWidth, dstSizeHeight);
				}
				
				
			}
			else
			{
				
				if(SystemInfo.supportsComputeShaders)
				{
					resultTex_DstSize = MergeAlphaChannelWithComputeShader(resultTex_SrcSize, resultTex_SrcSize_Alpha);
				}
				if(resultTex_DstSize == null)
				{
					resultTex_DstSize = MergeAlphaChannel(resultTex_SrcSize, resultTex_SrcSize_Alpha/*, resultTex_FixAlphaBorder*/);
				}
				
			}

			System.Threading.Thread.Sleep(5);
			//기존 크기의 이미지는 삭제
			UnityEngine.Object.DestroyImmediate(resultTex_SrcSize);
			UnityEngine.Object.DestroyImmediate(resultTex_SrcSize_Alpha);
			//UnityEngine.Object.DestroyImmediate(resultTex_FixAlphaBorder);

			return resultTex_DstSize;

		}



		
		


		// [v1.4.6] 리뉴얼 버전. 코드가 너무 많아서 분기를 나눈다.
		public Texture2D RenderToTexture_V2(	apMeshGroup meshGroup,
												int srcSizeWidth, int srcSizeHeight,
												int dstSizeWidth, int dstSizeHeight,
												Color clearColor,
												//bool isPixelPerfect,
												int focusOffsetX, int focusOffsetY)
		{
			if (_editor == null)
			{
				return null;
			}

			//중요 : 렌더 크기 비율이 계속 바뀌므로 float형으로 크기를 계속 제어해야한다.
			//Float > Int 변환시 RoundToInt를 이용한다.
			
			//1. RT 비율 계산 : 적절한 RT 비율을 "색상 샘플링"을 이용한 캘리브레이션을 통해서 계산한다.			
			//2. RT-Clip 영역 계산 (1차) : RT 크기에 맞는 Clip 영역을 다시 구한다. 여기서도 "색상 샘플링" 캘리브레이션을 활용한다.
			//3. RT 확대/축소 계산 : Clip 영역이 Src 영역이 되는데, 측정된 Src 영역 (보정되어서 조금 다를 수 있음) > Dst 확대 비율에 맞게 미리 RT 크기를 변경한다. (이전에는 Src를 바탕으로 Compute Shader를 이용해서 확대를 했다)
			//4. (리스케일 필요시) 확대된 RT에 맞게 Clip 영역 재측정 (2차) : 이번엔 Dst 크기에 맞게 영역을 측정한다. 이번엔 영역 크기를 바꾸지 않으며, 클리핑 위치만 측정한다.
			//5. 마스크, 일반 렌더링 : RT 생성 > 렌더링 > 전체 복사 > 클리핑 복사 순서로 처리한다.
			//6. 알파/일반 병합 : Compute Shader를 이용해서 결과 생성


			// [ 1. RT 비율 계산 ]
			//: 적절한 RT 비율을 "색상 샘플링"을 이용한 캘리브레이션을 통해서 계산한다.

			//그래픽 디바이스에 맞게 RT를 생성해야 좌표계가 맞는다.
			//RT 비율이 안맞으면 크롭되는게 아니라 스케일이 되므로 제대로 연산할 수 없다.
			//기존에는 에디터 크기를 그대로 사용했는데, 그건 적절한 방식이 아니었다.

			//보정 계산 결과 변수들
			int rtSizeWidth = 0;
			int rtSizeHeight = 0;

			

			//에디터 크기는 기본적으로 1920 x 1080을 기본으로 하므로, 여기서는 2000을 기본으로 잡자
			int editorWindowWidth = Mathf.Max(((int)_editor.position.width), 2000);
			int editorWindowHeight = Mathf.Max(((int)_editor.position.height), 2000);

			float clipPosCenterX_F = _editor._captureFrame_PosX + apGL.WindowSizeHalf.x;
			float clipPosCenterY_F = (-1 * _editor._captureFrame_PosY) + apGL.WindowSizeHalf.y;



			//실제로 사용되는 값
			int clipPosX_InRT = 0;
			int clipPosY_InRT = 0;
			int clipAreaWidth_InRT = 0;
			int clipAreaHeight_InRT = 0;

			bool isUseCache = false;


			//캐시를 사용할 수 있다면 보정 로직을 생략할 수 있다.
			if(_isCalibrationCache)
			{
				//캐시가 저장되어 있다.
				//비교를 하자
				//모두 같아야 캐시를 사용할 수 있다.
				//하나라도 다르면 캐시 사용 불가 + 캐시 삭제
				if(_caliCache_Cond_EditorSize_Width == editorWindowWidth
					&& _caliCache_Cond_EditorSize_Height == editorWindowHeight
					&& Mathf.Abs(_caliCache_Cond_ClipRectCenterPosX - clipPosCenterX_F) < 0.01f
					&& Mathf.Abs(_caliCache_Cond_ClipRectCenterPosY - clipPosCenterY_F) < 0.01f
					&& _caliCache_Cond_ClipSizeWidth == srcSizeWidth
					&& _caliCache_Cond_ClipSizeHeight == srcSizeHeight
					&& _caliCache_Cond_DstSizeWidth == dstSizeWidth
					&& _caliCache_Cond_DstSizeHeight == dstSizeHeight)
				{
					//모든 조건이 성립한다면
					isUseCache = true;//캐시를 사용했다.

					//캐시의 값을 할당한다.
					rtSizeWidth = _caliCache_Result_RTSize_Width;
					rtSizeHeight = _caliCache_Result_RTSize_Height;

					clipPosX_InRT = _caliCache_Result_ClipArea_PosX;
					clipPosY_InRT = _caliCache_Result_ClipArea_PosY;
					clipAreaWidth_InRT = _caliCache_Result_ClipArea_Width;
					clipAreaHeight_InRT = _caliCache_Result_ClipArea_Height;

					//Debug.Log("캐시 사용됨");
				}
				else
				{
					//Debug.LogError("기존 캐시 삭제됨 (조건 다름)");
					isUseCache = false;
					ClearCache();
				}
			}


			//Float형 RT 사이즈. 계속되는 비례식을 거쳐야 하므로 Float 타입으로 쭉 계산한다.

			apGL.GetWindowParameters(_glWindowParam);


			if (!isUseCache)
			{
				//캐시를 사용하지 않았다면
				//보정 코드를 동작시킨다.

				//기본은 
				float rtSizeWidth_F = 0.0f;
				float rtSizeHeight_F = 0.0f;


				CalibrateRTSize(out rtSizeWidth_F, out rtSizeHeight_F);

				//Debug.LogWarning("기본 정보");
				//Debug.Log("보정 계산된 RT Size : " + rtSizeWidth_F + "x" + rtSizeHeight_F);

				//에디터 크기에 맞게 축소
				//단, 계산된 각각의 축이 크기가 EditorWindow보다는 커야 한다.
				//Height를 맞춘 경우의 Width의 값과 그 반대의 값을 모두 계산하여, 축이 커지는 경우를 채택한다.



				//RT 비율에 맞게 
				float rt2GLSize_ByX = (float)editorWindowWidth / rtSizeWidth_F;

				int calibratedHeight = Mathf.RoundToInt(rtSizeHeight_F * rt2GLSize_ByX);//Width 보정에 따른 수정된 Height값

				//일단 Height를 보정한다. 이후에 다시 보정할 수 있다.
				rtSizeWidth = editorWindowWidth;
				rtSizeHeight = calibratedHeight;//Height를 보정


				//Debug.Log("Pixel Perfect를 위한 Y 픽셀 스케일 비율 : " + rt2GLSize_ByX);

				// [ 2. RT-Clip 영역 계산 (1차) ]
				//: RT 크기에 맞는 Clip 영역을 다시 구한다. 여기서도 "색상 샘플링" 캘리브레이션을 활용한다.

				//Debug.Log("RT 보정 결과 : 기존 : " + editorWindowWidth + "x" + editorWindowHeight + " >> 보정 후 : " + rtSizeWidth + "x" + rtSizeHeight);
				int calibratedClipArea_MinX = 0;
				int calibratedClipArea_MaxX = 0;
				int calibratedClipArea_MinY = 0;
				int calibratedClipArea_MaxY = 0;

				int calibratedClipArea_Width = 0;
				int calibratedClipArea_Height = 0;

				System.Threading.Thread.Sleep(5);

				bool isClipAreaCalibrated = CalibrateClipArea(	rtSizeWidth, rtSizeHeight,
																clipPosCenterX_F, clipPosCenterY_F,
																srcSizeWidth, srcSizeHeight,
																out calibratedClipArea_MinX, out calibratedClipArea_MaxX,
																out calibratedClipArea_MinY, out calibratedClipArea_MaxY);

				if (!isClipAreaCalibrated)
				{
					//캘리브레이션에 실패함
					Debug.LogError("AnyPortrait : RenderToTexture Failed. (Clipping Calibration Failed)");
					return null;
				}


				calibratedClipArea_Width = (calibratedClipArea_MaxX - calibratedClipArea_MinX) + 1;
				calibratedClipArea_Height = (calibratedClipArea_MaxY - calibratedClipArea_MinY) + 1;

				//Debug.Log("보정 전 Clip 영역 : " + clipPosX_Base + ", " + clipPosY_Base + " (" + srcSizeWidth + "x" + srcSizeHeight + ")");
				//Debug.Log("보정 후 Clip 영역 : " + calibratedClipArea_MinX + ", " + calibratedClipArea_MinY + " (" + calibratedClipArea_Width + "x" + calibratedClipArea_Height + ")");

				//오차에 의해서 2px 이내라면, 요청된 크기로 바꿔서 그대로 사용하자 (위치는.. 흠)
				int difClipArea_Width = Mathf.Abs(calibratedClipArea_Width - srcSizeWidth);
				int difClipArea_Height = Mathf.Abs(calibratedClipArea_Height - srcSizeHeight);

				if (difClipArea_Width <= 2)
				{
					calibratedClipArea_Width = srcSizeWidth;
					//Debug.Log("Width 재보정. 오차가 적어서 요청 크기 사용 : " + srcSizeWidth);
				}

				if (difClipArea_Height <= 2)
				{
					calibratedClipArea_Height = srcSizeHeight;
					//Debug.Log("Height 재보정. 오차가 적어서 요청 크기 사용 : " + srcSizeHeight);
				}

				clipPosX_InRT = calibratedClipArea_MinX;
				clipPosY_InRT = calibratedClipArea_MinY;
				clipAreaWidth_InRT = calibratedClipArea_Width;
				clipAreaHeight_InRT = calibratedClipArea_Height;

				if (clipAreaWidth_InRT <= 0 || clipAreaHeight_InRT <= 0)
				{
					Debug.LogError("AnyPortrait : RenderToTexture Failed. (Clip Area is over Screen)");
					return null;
				}

				//Debug.Log("Clip Pos X (Left ~ Right) : " + clipPosX_InRT + " (Width : " + clipSrcWidth_InRT + ")");
				//Debug.Log("Clip Pos Y (Top ~ Bottom) : " + clipPosY_InRT + " (Height : " + clipSrcHeight_InRT + ")");

				
				// [ 3. RT 확대/축소 계산 ]
				// : Clip 영역이 Src 영역이 되는데, 측정된 Src 영역 (보정되어서 조금 다를 수 있음) > Dst 확대 비율에 맞게 미리 RT 크기를 변경한다.
				// (이전에는 Src를 바탕으로 Compute Shader를 이용해서 확대를 했다)

				//스케일을 해야하는지 판단
				bool isSameSize_Width = (clipAreaWidth_InRT == dstSizeWidth);
				bool isSameSize_Height = (clipAreaHeight_InRT == dstSizeHeight);

				bool isRTScaled = false;
				if (!isSameSize_Width || !isSameSize_Height)
				{
					//크기가 하나라도 스케일이 되어야 한다면
					//int rescaleRTWidth = rtSizeWidth;
					//int rescaleRTHeight = rtSizeHeight;

					//float resizeClipPosCenterX_F = clipPosCenterX_F;
					//float resizeClipPosCenterY_F = clipPosCenterY_F;

					//int prevRTSize_Width = rtSizeWidth;
					//int prevRTSize_Height = rtSizeHeight;
					if (!isSameSize_Width)
					{
						//Width 변경
						float rescale_X = (float)dstSizeWidth / (float)clipAreaWidth_InRT;
						
						clipPosX_InRT = Mathf.RoundToInt((float)clipPosX_InRT * rescale_X);
						rtSizeWidth = Mathf.RoundToInt((float)rtSizeWidth * rescale_X);
						
						isRTScaled = true;

					}

					if (!isSameSize_Height)
					{
						//Height 변경
						float rescale_Y = (float)dstSizeHeight / (float)clipAreaHeight_InRT;
						
						clipPosY_InRT = Mathf.RoundToInt((float)clipPosY_InRT * rescale_Y);
						rtSizeHeight = Mathf.RoundToInt((float)rtSizeHeight * rescale_Y);

						isRTScaled = true;
					}
					
					//Debug.Log("Clip 영역 측정에 따른 RT 크기 변경 : " + prevRTSize_Width + "x" + prevRTSize_Height + " >> " + rtSizeWidth + "x" + rtSizeHeight);

					clipAreaWidth_InRT = dstSizeWidth;
					clipAreaHeight_InRT = dstSizeHeight;


					//한번 더 RT 크기 체크를 하자.
					//픽셀 단위로 정확하게 만들기 위해서는 RT 크기가 정확해야 한다.
					//RT 크기의
					if(isRTScaled)
					{
						//2차 보정
						int cb2_ClipArea_MinX = 0;
						int cb2_ClipArea_MaxX = 0;
						int cb2_ClipArea_MinY = 0;
						int cb2_ClipArea_MaxY = 0;

						System.Threading.Thread.Sleep(5);

						bool isCB2_Calibrated = CalibrateClipArea(	rtSizeWidth, rtSizeHeight,
																	clipPosCenterX_F, clipPosCenterY_F,
																	srcSizeWidth, srcSizeHeight,
																	out cb2_ClipArea_MinX, out cb2_ClipArea_MaxX,
																	out cb2_ClipArea_MinY, out cb2_ClipArea_MaxY);

						

						if(isCB2_Calibrated)
						{
							int cb2_ClipArea_Width = (cb2_ClipArea_MaxX - cb2_ClipArea_MinX) + 1;
							int cb2_ClipArea_Height = (cb2_ClipArea_MaxY - cb2_ClipArea_MinY) + 1;

							//Debug.Log("2차 RT 크기 보정에서의 Clip Area 크기 : " + cb2_ClipArea_Width + " x " + cb2_ClipArea_Height + " (Dst 크기 : " + dstSizeWidth + " x " + dstSizeHeight + " )");

							//이게 Dst 크기와 같으면 상관 없다.
							//만약 Dst 크기와 다르다면 조금씩 RT Size를 변경해야한다.
							if(cb2_ClipArea_Width != dstSizeWidth || cb2_ClipArea_Height != dstSizeHeight)
							{
								//Debug.LogError("보정 후의 RT 크기가 맞지 않다.");

								//크기 보정을 위해서 RT 크기를 변경해봐야 한다.
								//For문을 이용해서 보정을 하자
								if(cb2_ClipArea_Width != dstSizeWidth)
								{
									//Width가 다른 경우
									//Debug.LogError("Width가 맞지 않다. : 현재 : " + cb2_ClipArea_Width + " >> 목표 : " + dstSizeWidth);

									//Width 차이값 * RT로의 비율 + 약간의 Bias만큼 RT를 줄이거나 늘려가면서 체크하자

									//Clip 1픽셀을 늘리거나 줄이기 위해 RT를 얼마나 늘려야 하는가 (비율 필요)
									float clip2RT_Width = (float)rtSizeWidth / (float)cb2_ClipArea_Width;

									bool isNeedToExpand = dstSizeWidth > cb2_ClipArea_Width;

									//순회 순서.
									//- Divide 방식을 이용한다.
									//- 초기에 정한 Min-Max 영역의 중간에서 먼저 체크. 동일하면 바로 리턴한다.
									//- 만약 아직 동일하지 않다면, 크기를 확대/축소할지 여부에 따라서 다음 범위를 정한다. (영역 1/2로 축소)
									//- 만약 다음 범위의 크기가 10 이하라면, 하나씩 For문을 돈다.

									int checkRange = Mathf.Max(Mathf.CeilToInt((float)(Mathf.Abs(dstSizeWidth - cb2_ClipArea_Width)) * clip2RT_Width), 1) + 5;

									int widthRange_Min = 0;
									int widthRange_Max = 0;
									if(isNeedToExpand)
									{
										//확장해야 하는 경우
										widthRange_Min = rtSizeWidth;
										widthRange_Max = rtSizeWidth + checkRange;
									}
									else
									{
										//축소해야하는 경우
										widthRange_Min = rtSizeWidth - checkRange;
										widthRange_Max = rtSizeWidth;
									}

									//Debug.Log("---- X : 3차 보정 시작 : " + widthRange_Min + " ~ " + widthRange_Max + " ----");


									int cb3_ClipArea_MinX = 0;
									int cb3_ClipArea_MaxX = 0;
									int cb3_ClipArea_MinY = 0;
									int cb3_ClipArea_MaxY = 0;
									
									int cb3_ClipArea_Width = 0;
									int cb3_DeltaWidthToDst = 0;

									//최적의 결과
									int nearestRTWidth = rtSizeWidth;
									int nearestDeltaClipWidth = Mathf.Abs(dstSizeWidth - cb2_ClipArea_Width);
									int nearestPosX = clipPosX_InRT;


									int curStepRange_Min = widthRange_Min;
									int curStepRange_Max = widthRange_Max;

									//반복하면서 테스트
									while(true)
									{
										//체크해야하는 간격이 10 이하면 하나씩 체크
										//그렇지 않다면 간격의 절반만 체크
										int rangeSize = curStepRange_Max - curStepRange_Min;

										if(rangeSize < 10)
										{
											//RT를 1픽셀씩 변경하면서 하나씩 체크한다.
											for (int curRTWidth = curStepRange_Min; curRTWidth <= curStepRange_Max; curRTWidth++)
											{
												CalibrateClipArea(	curRTWidth, rtSizeHeight,
																	clipPosCenterX_F, clipPosCenterY_F,
																	srcSizeWidth, srcSizeHeight,
																	out cb3_ClipArea_MinX, out cb3_ClipArea_MaxX,
																	out cb3_ClipArea_MinY, out cb3_ClipArea_MaxY);

												cb3_ClipArea_Width = (cb3_ClipArea_MaxX - cb3_ClipArea_MinX) + 1;
												cb3_DeltaWidthToDst = Mathf.Abs(cb3_ClipArea_Width - dstSizeWidth);

												if(cb3_ClipArea_Width == dstSizeWidth)
												{
													nearestRTWidth = curRTWidth;
													nearestDeltaClipWidth = 0;//정확히 도달
													nearestPosX = cb3_ClipArea_MinX;

													break;//더 체크할 필요가 없다.
												}
												else
												{
													//조금 다르다. 최소 차이값으로 갱신한다.
													if(cb3_DeltaWidthToDst < nearestDeltaClipWidth)
													{
														//차이가 더 줄었다면 현재 계산 값을 적용
														nearestRTWidth = curRTWidth;
														nearestDeltaClipWidth = cb3_DeltaWidthToDst;
														nearestPosX = cb3_ClipArea_MinX;
													}
												}
											}

											//이건 무조건 종료한다.
											break;
										}
										else
										{
											//절반만 체크하고 다음 영역을 설정한 뒤 다음 루틴으로 넘어간다.
											//단, 완전히 절반이 아닌 Max에 가까운 위치를 정한다. (Max 범위값이 계산이 한번 된 값이므로)
											int curRTWidth = (int)((curStepRange_Min * 0.2f) + (curStepRange_Max * 0.8f));

											CalibrateClipArea(	curRTWidth, rtSizeHeight,
																clipPosCenterX_F, clipPosCenterY_F,
																srcSizeWidth, srcSizeHeight,
																out cb3_ClipArea_MinX, out cb3_ClipArea_MaxX,
																out cb3_ClipArea_MinY, out cb3_ClipArea_MaxY);

											cb3_ClipArea_Width = (cb3_ClipArea_MaxX - cb3_ClipArea_MinX) + 1;
											cb3_DeltaWidthToDst = Mathf.Abs(cb3_ClipArea_Width - dstSizeWidth);

											if(cb3_ClipArea_Width == dstSizeWidth)
											{
												nearestRTWidth = curRTWidth;
												nearestDeltaClipWidth = 0;//정확히 도달
												nearestPosX = cb3_ClipArea_MinX;

												break;//더 체크할 필요가 없다.
											}
											else
											{
												//일단 갱신을 하자
												if(cb3_DeltaWidthToDst < nearestDeltaClipWidth)
												{
													//차이가 더 줄었다면 현재 계산 값을 적용
													nearestRTWidth = curRTWidth;
													nearestDeltaClipWidth = cb3_DeltaWidthToDst;
													nearestPosX = cb3_ClipArea_MinX;
												}

												//다음 루틴의 영역을 정하자
												if(cb3_ClipArea_Width < dstSizeWidth)
												{
													//Min ~~ [ Cur ~~ Max ]
													//클립 영역이 목표보다 아직 작다.
													//더 증가시켜야 하므로, 큰 영역 절반을 더 체크하자
													curStepRange_Min = curRTWidth;
												}
												else
												{
													//[ Min ~~ Cur ] ~~ Max
													//클립 영역이 목표보다 크다.
													//RT를 더 축소시켜야 하므로, 작은 영역 절반을 더 체크하자
													curStepRange_Max = curRTWidth;
												}
											}
										}
									}

									//결과를 적용하자
									rtSizeWidth = nearestRTWidth;
									clipPosX_InRT = nearestPosX;
								}

								if(cb2_ClipArea_Height != dstSizeHeight)
								{
									//Height가 다른 경우
									//Debug.LogError("Height가 맞지 않다. : 현재 : " + cb2_ClipArea_Height + " >> 목표 : " + dstSizeHeight);

									//Height 차이값 * RT로의 비율 + 약간의 Bias만큼 RT를 줄이거나 늘려가면서 체크하자

									//Clip 1픽셀을 늘리거나 줄이기 위해 RT를 얼마나 늘려야 하는가 (비율 필요)
									float clip2RT_Height = (float)rtSizeHeight / (float)cb2_ClipArea_Height;

									bool isNeedToExpand = dstSizeHeight > cb2_ClipArea_Height;

									//순회 순서.
									//- Divide 방식을 이용한다.
									//- 초기에 정한 Min-Max 영역의 중간에서 먼저 체크. 동일하면 바로 리턴한다.
									//- 만약 아직 동일하지 않다면, 크기를 확대/축소할지 여부에 따라서 다음 범위를 정한다. (영역 1/2로 축소)
									//- 만약 다음 범위의 크기가 10 이하라면, 하나씩 For문을 돈다.

									int checkRange = Mathf.Max(Mathf.CeilToInt((float)(Mathf.Abs(dstSizeHeight - cb2_ClipArea_Height)) * clip2RT_Height), 1) + 5;

									int heightRange_Min = 0;
									int heightRange_Max = 0;
									if(isNeedToExpand)
									{
										//확장해야 하는 경우
										heightRange_Min = rtSizeHeight;
										heightRange_Max = rtSizeHeight + checkRange;
									}
									else
									{
										//축소해야하는 경우
										heightRange_Min = rtSizeHeight - checkRange;
										heightRange_Max = rtSizeHeight;
									}

									//Debug.Log("---- Y : 3차 보정 시작 : " + heightRange_Min + " ~ " + heightRange_Max + " ----");

									int cb3_ClipArea_MinX = 0;
									int cb3_ClipArea_MaxX = 0;
									int cb3_ClipArea_MinY = 0;
									int cb3_ClipArea_MaxY = 0;
									
									int cb3_ClipArea_Height = 0;
									int cb3_DeltaHeightToDst = 0;

									//최적의 결과
									int nearestRTHeight = rtSizeHeight;
									int nearestDeltaClipHeight = Mathf.Abs(dstSizeHeight - cb2_ClipArea_Height);
									int nearestPosY = clipPosY_InRT;

									int curStepRange_Min = heightRange_Min;
									int curStepRange_Max = heightRange_Max;

									//int nStep = 0;

									//반복하면서 테스트
									while(true)
									{
										//체크해야하는 간격이 10 이하면 하나씩 체크
										//그렇지 않다면 간격의 절반만 체크
										int rangeSize = curStepRange_Max - curStepRange_Min;

										//nStep += 1;
										//Debug.Log("[" + nStep + "] 범위 : " + curStepRange_Min + " ~ " + curStepRange_Max);

										if(rangeSize < 10)
										{
											//Debug.LogError(" >> 하나씩 체크");

											//RT를 1픽셀씩 변경하면서 하나씩 체크한다.
											for (int curRTHeight = curStepRange_Min; curRTHeight <= curStepRange_Max; curRTHeight++)
											{
												CalibrateClipArea(	rtSizeWidth, curRTHeight,
																	clipPosCenterX_F, clipPosCenterY_F,
																	srcSizeWidth, srcSizeHeight,
																	out cb3_ClipArea_MinX, out cb3_ClipArea_MaxX,
																	out cb3_ClipArea_MinY, out cb3_ClipArea_MaxY);

												cb3_ClipArea_Height = (cb3_ClipArea_MaxY - cb3_ClipArea_MinY) + 1;
												cb3_DeltaHeightToDst = Mathf.Abs(cb3_ClipArea_Height - dstSizeHeight);

												if(cb3_ClipArea_Height == dstSizeHeight)
												{
													nearestRTHeight = curRTHeight;
													nearestDeltaClipHeight = 0;//정확히 도달
													nearestPosY = cb3_ClipArea_MinY;

													//Debug.Log(" >>> 정확한 RT 크기 찾음");

													break;//더 체크할 필요가 없다.
												}
												else
												{
													//조금 다르다. 최소 차이값으로 갱신한다.
													if(cb3_DeltaHeightToDst < nearestDeltaClipHeight)
													{
														//차이가 더 줄었다면 현재 계산 값을 적용
														nearestRTHeight = curRTHeight;
														nearestDeltaClipHeight = cb3_DeltaHeightToDst;
														nearestPosY = cb3_ClipArea_MinY;

														//Debug.Log(" >>> 더 최적화된 RT 크기 갱신 (차이값 : " + nearestDeltaClipHeight + ")");
													}
												}
											}

											//이건 무조건 종료한다.
											break;
										}
										else
										{
											//절반만 체크하고 다음 영역을 설정한 뒤 다음 루틴으로 넘어간다.
											//단, 완전히 절반이 아닌 Max에 가까운 위치를 정한다. (Max 범위값이 계산이 한번 된 값이므로)
											int curRTHeight = (int)((curStepRange_Min * 0.2f) + (curStepRange_Max * 0.8f));

											//Debug.LogError(" >> 중간만 체크 [" + curRTHeight + "]");

											CalibrateClipArea(	rtSizeWidth, curRTHeight,
																clipPosCenterX_F, clipPosCenterY_F,
																srcSizeWidth, srcSizeHeight,
																out cb3_ClipArea_MinX, out cb3_ClipArea_MaxX,
																out cb3_ClipArea_MinY, out cb3_ClipArea_MaxY);

											cb3_ClipArea_Height = (cb3_ClipArea_MaxY - cb3_ClipArea_MinY) + 1;
											cb3_DeltaHeightToDst = Mathf.Abs(cb3_ClipArea_Height - dstSizeHeight);

											if(cb3_ClipArea_Height == dstSizeHeight)
											{
												nearestRTHeight = curRTHeight;
												nearestDeltaClipHeight = 0;//정확히 도달
												nearestPosY = cb3_ClipArea_MinY;

												//Debug.Log(" >>> 정확한 RT 크기 찾음");

												break;//더 체크할 필요가 없다.
											}
											else
											{
												//일단 갱신을 하자
												if(cb3_DeltaHeightToDst < nearestDeltaClipHeight)
												{
													//차이가 더 줄었다면 현재 계산 값을 적용
													nearestRTHeight = curRTHeight;
													nearestDeltaClipHeight = cb3_DeltaHeightToDst;
													nearestPosY = cb3_ClipArea_MinY;

													//Debug.Log(" >>> 더 최적화된 RT 크기 갱신 (차이값 : " + nearestDeltaClipHeight + ")");
												}

												//다음 루틴의 영역을 정하자
												if(cb3_ClipArea_Height < dstSizeHeight)
												{
													//Min ~~ [ Cur ~~ Max ]
													//클립 영역이 목표보다 아직 작다.
													//더 증가시켜야 하므로, 큰 영역 절반을 더 체크하자
													curStepRange_Min = curRTHeight;

													//Debug.Log(" >>> 큰 영역으로 이동 [" + curStepRange_Min + " ~ " + curStepRange_Max + "]");
												}
												else
												{
													//[ Min ~~ Cur ] ~~ Max
													//클립 영역이 목표보다 크다.
													//RT를 더 축소시켜야 하므로, 작은 영역 절반을 더 체크하자
													curStepRange_Max = curRTHeight;

													//Debug.Log(" >>> 작은 영역으로 이동 [" + curStepRange_Min + " ~ " + curStepRange_Max + "]");
												}
											}
										}
									}

									//결과를 적용하자
									rtSizeHeight = nearestRTHeight;
									clipPosY_InRT = nearestPosY;
								}
							}
						}
					}
				}
			}

			

			//보정 결과를 캐시에 저장한다.
			//시퀸스 (스프라이트 시트, 동영상 등) 내보내기시 사용된다.
			SaveCache(	//Cond
						editorWindowWidth,
						editorWindowHeight,
						clipPosCenterX_F,
						clipPosCenterY_F,
						srcSizeWidth,
						srcSizeHeight,
						dstSizeWidth,
						dstSizeHeight,

						//Result
						rtSizeWidth,
						rtSizeHeight,
						clipPosX_InRT,
						clipPosY_InRT,
						clipAreaWidth_InRT,
						clipAreaHeight_InRT
						);

			System.Threading.Thread.Sleep(5);

			meshGroup.RefreshForce();
			meshGroup.UpdateRenderUnits(0.0f, true);

			

			////Pixel Perfect를 위한 클리핑 범위 (좌표계별로 별도)
			//Vector2 clipAreaPosGL_LB = new Vector2(clipPosCenterX_F - srcSizeWidth, clipPosCenterY_F - srcSizeHeight);
			
			////오프셋 이동이 있다면, GL 상에서 미세 조정을 해야한다. (RT 이동에 따른 GL 기준값 변화이므로 RT > GL 좌표계로 변환)
			//float clipRT2GLScaleX = (float)srcSizeWidth / (float)clipAreaWidth_InRT;
			//float clipRT2GLScaleY = (float)srcSizeHeight / (float)clipAreaHeight_InRT;
			
			//Vector2 posSizeGLPerPixel = new Vector2(clipRT2GLScaleX, clipRT2GLScaleY);//픽셀 퍼펙트시 사용되는 RT의 1픽셀당 GL 위치 크기
			//Vector2 focusGL = new Vector2((float)focusOffsetX * clipRT2GLScaleX, (float)focusOffsetY * clipRT2GLScaleY);
			//clipAreaPosGL_LB += focusGL;

			//클리핑 복사 범위:
			//기본적으로는
			//- 위치 : clipPosX_InRT, clipPosY_InRT / 크기 : "clipSrcWidth_InRT x clipSrcHeight_InRT" 만큼 복사한다.
			//- 이때 위치 오프셋 (focusOffsetX, focusOffsetY) 만큼 이동한다.
			//- 이때 복사 인덱스 범위가 이미지의 범위 바깥으로 나가면 안된다.
			//- 따라서 보정된 범위는 이미지 크기 (0, 0) ~ (rtSizeWidth x rtSizeHeight) 바깥으로 나가지 않도록 제한한다.
			int copiedFocusX = clipPosX_InRT + focusOffsetX;
			int copiedFocusY = clipPosY_InRT + focusOffsetY;
			int copiedFocusMaxX = (copiedFocusX + clipAreaWidth_InRT) - 1;
			int copiedFocusMaxY = (copiedFocusY + clipAreaHeight_InRT) - 1;
			//int copiedWidth = clipSrcWidth_InRT;
			//int copiedHeight = clipSrcHeight_InRT;

			if(copiedFocusX < 0) { copiedFocusX = 0; }
			else if(copiedFocusX > rtSizeWidth - 1) { copiedFocusX = rtSizeWidth - 1; }

			if(copiedFocusMaxX < 0) { copiedFocusMaxX = 0; }
			else if(copiedFocusMaxX > rtSizeWidth - 1) { copiedFocusMaxX = rtSizeWidth - 1; }

			if(copiedFocusY < 0) { copiedFocusY = 0; }
			else if(copiedFocusY > rtSizeHeight - 1) { copiedFocusY = rtSizeHeight - 1; }

			if(copiedFocusMaxY < 0) { copiedFocusMaxY = 0; }
			else if(copiedFocusMaxY > rtSizeHeight - 1) { copiedFocusMaxY = rtSizeHeight - 1; }

			int copiedWidth = (copiedFocusMaxX - copiedFocusX) + 1;
			int copiedHeight = (copiedFocusMaxY - copiedFocusY) + 1;

			//copied를 이용해서 크롭을 시도한다.
			//경우에 따라선 Width/Height가 줄어서 복사를 안할 수도 있다.
			bool isValidCropArea = (copiedWidth > 1) && (copiedHeight > 1);
			bool isAnyCropped = (copiedWidth != clipAreaWidth_InRT) || (copiedHeight != clipAreaHeight_InRT);//조금이라도 크롭이 되었다면, 배경 색을 칠해야한다.
			
			//기본 색상용 배열 크기
			int clearColorArrLength = clipAreaWidth_InRT * clipAreaHeight_InRT;


			// [ 5. 마스크, 일반 렌더링 ]
			// : RT 생성 > 렌더링 > 전체 복사 > 클리핑 복사 순서로 처리한다.

			
			//Pass-1. 일반 + MaskParent를 Alpha2White 렌더링. 이걸로 나중에 알파 채널용 텍스쳐를 만든다.
			//--------------------------------------------------------------------------------------------------------
			_renderTexture_GrayscaleAlpha = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture_GrayscaleAlpha.antiAliasing = 1;
			_renderTexture_GrayscaleAlpha.isPowerOfTwo = false;
			_renderTexture_GrayscaleAlpha.wrapMode = TextureWrapMode.Clamp;
			
			//v2에선 항상 Point
			_renderTexture_GrayscaleAlpha.filterMode = FilterMode.Point;

			RenderTexture.active = null;
			RenderTexture.active = _renderTexture_GrayscaleAlpha;

			//기본 
			Color maskClearColor = new Color(clearColor.a, clearColor.a, clearColor.a, 1.0f);//Alpha가 GrayScale로 적용된 색상
			GL.Clear(false, true, maskClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, maskClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			//System.Threading.Thread.Sleep(50);


			

			//변경
			List<apRenderUnit> renderUnits = meshGroup.SortedBuffer.SortedRenderUnits;
			int nRenderUnits = renderUnits.Count;

			//ExtraOption-Depth에 의한 순서 변경도 적용
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White_ForExport(	renderUnit
																					
																					////[ Pixel Perfect ]
																					//isPixelPerfect,
																					//clipAreaPosGL_LB,
																					//posSizeGLPerPixel
																					);
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
							//Alpha 렌더링에서 Clipping Child는 제외한다. 어차피 Parent의 Alpha보다 많을 수 없으니..
						}
						else
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White_ForExport(	renderUnit
																					
																					////[ Pixel Perfect ]
																					//isPixelPerfect,
																					//clipAreaPosGL_LB,
																					//posSizeGLPerPixel
																					);
							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);

			//RT > 텍스쳐 복사시 : RT > 전체 복사 > 부분 복사

			//<전체 복사>
			Texture2D copiedTexture_Alpha = new Texture2D(rtSizeWidth, rtSizeHeight, TextureFormat.ARGB32, false);
			copiedTexture_Alpha.wrapMode = TextureWrapMode.Clamp;
			copiedTexture_Alpha.filterMode = FilterMode.Point;//V2에선 항상 Point
			

			copiedTexture_Alpha.ReadPixels(new Rect(0, 0, rtSizeWidth, rtSizeHeight), 0, 0);
			copiedTexture_Alpha.Apply();


			//<부분 복사>
			Texture2D resultTex_GrayscaleAlpha = new Texture2D(clipAreaWidth_InRT, clipAreaHeight_InRT, TextureFormat.ARGB32, false);
			resultTex_GrayscaleAlpha.wrapMode = TextureWrapMode.Clamp;
			resultTex_GrayscaleAlpha.filterMode = FilterMode.Point; //V2에선 항상 Point

			
			//resultTex_SrcSize_Alpha.ReadPixels(new Rect(clipPosX_InRT, clipPosY_InRT, clipSrcWidth_InRT, clipSrcHeight_InRT), 0, 0);
			//resultTex_SrcSize_Alpha.Apply();

			//여기 코드 변경. (아래의 Color 부분도 같이)
			//이미지 크기 (clipSrcWidth_InRT x clipSrcHeight_InRT)에 비해서 복사 영역이 작을 수 있다.
			//그걸 대비하기 위해서 미리 배경 색으로 초기화를 해두자.
			//복사 범위는 위의 copied..를 이용하고, X축 또는 Y축 하나라도 크기가 0 이하면 복사를 하지 않는다.

			//기존. 그냥 복사하기
			//Graphics.CopyTexture(copiedTexture_Alpha, 0, 0, clipPosX_InRT, clipPosY_InRT, clipSrcWidth_InRT, clipSrcHeight_InRT, resultTex_GrayscaleAlpha, 0, 0, 0, 0);

			//필요한 경우 배경 색상을 복사한다.
			if(isAnyCropped)
			{
				//크롭이 되면서 복사되지 않는 구역이 있다.
				//기본 색상을 미리 칠해야한다.
				Color[] clearColors = new Color[clearColorArrLength];
				for (int iColor = 0; iColor < clearColorArrLength; iColor++)
				{
					clearColors[iColor] = maskClearColor;
				}

				resultTex_GrayscaleAlpha.SetPixels(clearColors);
			}

			//크롭 범위가 유효할 때
			if (isValidCropArea)
			{
				//크롭 범위만큼 복사한다.
				Graphics.CopyTexture(copiedTexture_Alpha, 0, 0, copiedFocusX, copiedFocusY, copiedWidth, copiedHeight, resultTex_GrayscaleAlpha, 0, 0, 0, 0);
			}
			
			resultTex_GrayscaleAlpha.Apply();

			//전체 복사된 텍스쳐는 삭제
			UnityEngine.Object.DestroyImmediate(copiedTexture_Alpha);


			RenderTexture.active = null;


			//Pass-2. 기본 렌더링
			//--------------------------------------------------------------------------------------------------------
			//1. Clip Parent의 MaskTexture를 미리 구워서 Dictionary에 넣는다.
			Dictionary<apRenderUnit, Texture2D> bakedClipMaskTextures = new Dictionary<apRenderUnit, Texture2D>();

			

			//ExtraOption-Depth인 경우 렌더링 순서가 바뀌어야한다.
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								Texture2D clipMaskTex = apGL.GetMaskTexture_ClippingParent(renderUnit);
								if (clipMaskTex != null)
								{
									bakedClipMaskTextures.Add(renderUnit, clipMaskTex);
								}
								else
								{
									Debug.LogError("Clip Testure Bake Failed");
								}

							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);

			
			_renderTexture = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			_renderTexture.isPowerOfTwo = false;
			_renderTexture.wrapMode = TextureWrapMode.Clamp;
			_renderTexture.filterMode = FilterMode.Point;//V2에선 항상 Point

			
			RenderTexture.active = _renderTexture;

			Color opaqueClearColor = new Color(clearColor.r * clearColor.a, clearColor.g * clearColor.a, clearColor.b * clearColor.a, 1.0f);

			GL.Clear(false, true, opaqueClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, opaqueClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			//ExtraOption-Depth인 경우 렌더링 순서가 바뀌어야한다.
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								if (bakedClipMaskTextures.ContainsKey(renderUnit))
								{
									apGL.DrawRenderUnit_ClippingParent_ForExport_WithoutRTT(	renderUnit,
																								renderUnit._meshTransform._clipChildMeshes,
																								bakedClipMaskTextures[renderUnit]
																								
																								////[ Pixel Perfect ]
																								//isPixelPerfect,
																								//clipAreaPosGL_LB,
																								//posSizeGLPerPixel
																								);
								}
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
						}
						else
						{
							if (renderUnit._isVisible)
							{
								RenderTexture.active = _renderTexture;
								apGL.DrawRenderUnit_Basic_ForExport(	renderUnit
																		////[ Pixel Perfect ]
																		//isPixelPerfect,
																		//clipAreaPosGL_LB,
																		//posSizeGLPerPixel
																		);
							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);


			//전체 렌더링 끝


			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 테스트. 전체 RT 복사하여 위치 검토해보자
			//Texture2D testFullTex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
			//testFullTex.wrapMode = TextureWrapMode.Clamp;
			//testFullTex.filterMode = FilterMode.Point;
			//testFullTex.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
			//testFullTex.Apply();

			//int testFull_Day = DateTime.Now.Day;
			//int testFull_Hour = DateTime.Now.Hour;
			//int testFull_Min = DateTime.Now.Minute;
			//int testFull_Sec = DateTime.Now.Second;
			//string testFullFilePath = "C:\\AnyWorks\\Test Full Render - " + testFull_Day + "-" + testFull_Hour + "-" + testFull_Min + "-" + testFull_Sec;

			//SaveTexture2DToPNG(testFullTex, testFullFilePath, false);
			//System.IO.FileInfo test_Fullfi = new System.IO.FileInfo(testFullFilePath + ".png");//Path 빈 문자열 확인했음 (21.9.10)

			//Application.OpenURL("file://" + test_Fullfi.Directory.FullName);
			//Application.OpenURL("file://" + test_Fullfi);
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


			//RT > 텍스쳐 복사시 : RT > 전체 복사 > 부분 복사

			//<전체 복사>
			Texture2D copiedTexture_Color = new Texture2D(rtSizeWidth, rtSizeHeight, TextureFormat.ARGB32, false);
			copiedTexture_Color.wrapMode = TextureWrapMode.Clamp;
			copiedTexture_Color.filterMode = FilterMode.Point;//V2에선 항상 Point
			

			copiedTexture_Color.ReadPixels(new Rect(0, 0, rtSizeWidth, rtSizeHeight), 0, 0);
			copiedTexture_Color.Apply();

			//SaveDebugTexture(copiedTexture_Color, "리사이즈 후 전체 캡쳐");
			//Debug.LogError("클리핑 : " + clipPosX_InRT + ", " + clipPosY_InRT + " (" + clipSrcWidth_InRT + "x" + clipSrcHeight_InRT + ")");


			//<부분 복사>
			Texture2D resultTex_Color = new Texture2D(clipAreaWidth_InRT, clipAreaHeight_InRT, TextureFormat.ARGB32, false);
			resultTex_Color.wrapMode = TextureWrapMode.Clamp;
			resultTex_Color.filterMode = FilterMode.Point;//V2에선 항상 Point

			//크롭이 조금이라도 되었다면
			if(isAnyCropped)
			{
				//크롭이 되면서 복사되지 않는 구역이 있다.
				//기본 색상을 미리 칠해야한다.
				Color[] clearColors = new Color[clearColorArrLength];
				for (int iColor = 0; iColor < clearColorArrLength; iColor++)
				{
					clearColors[iColor] = opaqueClearColor;
				}

				resultTex_Color.SetPixels(clearColors);
			}

			
			//크롭 범위가 유효할 때
			if (isValidCropArea)
			{
				//크롭 영역만큼 복사한다.
				Graphics.CopyTexture(copiedTexture_Color, 0, 0, copiedFocusX, copiedFocusY, copiedWidth, copiedHeight, resultTex_Color, 0, 0, 0, 0);
			}
			resultTex_Color.Apply();

			

			//전체 복사된 텍스쳐는 삭제
			//UnityEngine.Object.DestroyImmediate(testFullTex);
			UnityEngine.Object.DestroyImmediate(copiedTexture_Color);

			//윈도우 크기 복구
			apGL.RecoverWindowSize(_glWindowParam);
			//--------------------<< 새로운 방식2


			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			//테스트로 RenderTexture (전체)를 저장한다.
			//int test_Day = DateTime.Now.Day;
			//int test_Hour = DateTime.Now.Hour;
			//int test_Min = DateTime.Now.Minute;
			//int test_Sec = DateTime.Now.Second;
			//string testFilePath = "C:\\AnyWorks\\TestFullFile-" + test_Day + "-" + test_Hour + "-" + test_Min + "-" + test_Sec;

			////Texture2D testFullSize = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
			//Texture2D testFullSize = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false, false);
			//testFullSize.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
			//testFullSize.Apply();

			//SaveTexture2DToPNG(testFullSize, testFilePath, false);
			//UnityEngine.Object.DestroyImmediate(testFullSize);

			//System.IO.FileInfo test_fi = new System.IO.FileInfo(testFilePath + ".png");//Path 빈 문자열 확인했음 (21.9.10)

			//Application.OpenURL("file://" + test_fi.Directory.FullName);
			//Application.OpenURL("file://" + test_fi);
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



			RenderTexture.active = null;


			RenderTexture.ReleaseTemporary(_renderTexture_GrayscaleAlpha);
			RenderTexture.ReleaseTemporary(_renderTexture);


			
			// 2. 렌더링된 이미지를 가공한다.
			//--------------------------------------------------------------------


			_renderTexture = null;
			_renderTexture_GrayscaleAlpha = null;
			Texture2D resultTex_Merged = null;

			//추가 : 가장자리 알파 문제를 수정하자
			//int blurSize = Mathf.Max(4, (dstSizeWidth / srcSizeWidth) + 1, (dstSizeHeight / srcSizeHeight) + 1);

			//Debug.LogWarning("Resize 여부 체크");
			//Debug.Log("Src Size : " + srcSizeWidth + "x" + srcSizeHeight);
			//Debug.Log("Dst Size : " + dstSizeWidth + "x" + dstSizeHeight);


			//기존 버전과 다르게 
			//이미 리사이징을 했으므로
			//여기서는 채널 병합만 한다.
			if(SystemInfo.supportsComputeShaders)
			{
				resultTex_Merged = MergeAlphaChannelWithComputeShader(resultTex_Color, resultTex_GrayscaleAlpha);
			}
			if(resultTex_Merged == null)
			{
				resultTex_Merged = MergeAlphaChannel(resultTex_Color, resultTex_GrayscaleAlpha/*, resultTex_FixAlphaBorder*/);
			}
			

			System.Threading.Thread.Sleep(5);
			//기존 크기의 이미지는 삭제
			UnityEngine.Object.DestroyImmediate(resultTex_Color);
			UnityEngine.Object.DestroyImmediate(resultTex_GrayscaleAlpha);

			return resultTex_Merged;

		}



		public void CalibrateRTSize(out float calibratedRTWidth, out float calibratedRTHeight)
		{
			//샘플 사이즈는 에디터 크기를 가져오며, 최소 값은 넘겨야 한다.
			//종횡비만 계산하면 되므로, 에디터 크기와 완전히 같을 필요는 없다.
			int editorSize_Width = Mathf.Max((int)_editor.position.width, 100);
			int editorSize_Height = Mathf.Max((int)_editor.position.height, 100);

			calibratedRTWidth = editorSize_Width;
			calibratedRTHeight = editorSize_Height;

			//정확한 측정을 위해 두배로 늘린다.
			int baseRTSize_Width = editorSize_Width * 2;
			int baseRTSize_Height = editorSize_Height * 2;

			//Debug.Log("보정 전 스크린 크기 : " + baseRTSize_Width + "x" + baseRTSize_Height);

			//R 채널만 가진 Render Texture를 만든다.
			RenderTexture renderTexture = RenderTexture.GetTemporary(baseRTSize_Width, baseRTSize_Height, 8, RenderTextureFormat.ARGB32);
			renderTexture.isPowerOfTwo = false;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.filterMode = FilterMode.Point;

			RenderTexture.active = null;
			RenderTexture.active = renderTexture;

			//배경을 검은색으로 만들자
			Color black = Color.black;
			GL.Clear(false, true, black);
			apGL.DrawBoxGL(Vector2.zero, baseRTSize_Width * 2, baseRTSize_Height * 2, black, false, true);
			GL.Flush();

			//가운데에 흰색 박스를 그리자

			Vector2 centerPos = new Vector2(editorSize_Width / 2, editorSize_Height / 2);
			
			//Rect Size는 EditorSize의 짧은 축의 60%로 설정한다.
			int minAxis = Mathf.Min(editorSize_Width, editorSize_Height);
			int rectSize = Mathf.Max(Mathf.FloorToInt((float)minAxis * 0.3f), 30);

			int pos_Left = Mathf.RoundToInt(centerPos.x - (rectSize / 2.0f));
			int pos_Bottom = Mathf.RoundToInt(centerPos.y - (rectSize / 2.0f));

			//apGL.DrawBoxGL_PixelPerfect(centerPos, rectSize, rectSize, Color.white, false, true);
			apGL.DrawBoxGL_PixelPerfect(pos_Left, pos_Bottom, rectSize, rectSize, Color.white, false, true);

			GL.Flush();

			//텍스쳐 2D로 가져온다.
			Texture2D resultTex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
			resultTex.filterMode = FilterMode.Point;
			resultTex.wrapMode = TextureWrapMode.Clamp;
			
			resultTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			resultTex.Apply();

			RenderTexture.active = null;

			//렌더 텍스쳐를 해제한다.
			RenderTexture.ReleaseTemporary(renderTexture);

			//색상을 검토하자
			Color[] colors = resultTex.GetPixels();

			int textureWidth = resultTex.width;
			int textureHeight = resultTex.height;
			int arrTotalSize = colors.Length;

			//색상 X의 Min-Max를 구한다.
			bool isAnyMinMax = false;
			int minX = 0;
			int minY = 0;
			int maxX = 0;
			int maxY = 0;

			Color curColor = Color.white;
			float bias = 0.5f;
			for (int iX = 0; iX < textureWidth; iX++)
			{
				for (int iY = 0; iY < textureHeight; iY++)
				{
					int iColor = (iY * textureWidth) + iX;
					if(iColor >= arrTotalSize)
					{
						continue;
					}

					curColor = colors[iColor];

					if(curColor.r > bias)
					{
						//사각형 데이터가 있다면
						if(!isAnyMinMax)
						{
							//초기화
							isAnyMinMax = true;
							minX = iX;
							maxX = iX;
							
							minY = iY;
							maxY = iY;							
						}
						else
						{
							minX = Mathf.Min(iX, minX);
							maxX = Mathf.Max(iX, maxX);
							
							minY = Mathf.Min(iY, minY);
							maxY = Mathf.Max(iY, maxY);
						}
						
						
					}

				}
			}


			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			////테스트로 RenderTexture (전체)를 저장한다.
			//int test_Day = DateTime.Now.Day;
			//int test_Hour = DateTime.Now.Hour;
			//int test_Min = DateTime.Now.Minute;
			//int test_Sec = DateTime.Now.Second;
			//string testFilePath = "C:\\AnyWorks\\Calibration-" + test_Day + "-" + test_Hour + "-" + test_Min + "-" + test_Sec;

			//SaveTexture2DToPNG(resultTex, testFilePath, false);
			//System.IO.FileInfo test_fi = new System.IO.FileInfo(testFilePath + ".png");//Path 빈 문자열 확인했음 (21.9.10)

			//Application.OpenURL("file://" + test_fi.Directory.FullName);
			//Application.OpenURL("file://" + test_fi);
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

			if(isAnyMinMax)
			{
				int rectWidth = (maxX - minX) + 1;
				int rectHeight = (maxY - minY) + 1;

				//측정된 Rect를 계산하자
				if(rectWidth > 0 && rectHeight > 0)
				{	
					//측정된 사각형의 종횡비를 구하자
					if(rectWidth != rectHeight)
					{
						//Rect가 실제와 RT간의 비율이 같지 않다면, 
						//이후에 RT를 생성할 때 크기를 변경해야 한다.
						//Width가 더 길다면 (AspectRatio > 1)
						//RT Width는 1/AspectRatio 만큼 줄여야 한다.
						//또는 그 반대
						float aspectRatio = (float)rectWidth / (float)rectHeight;

						//AspectRatio의 반대만큼 Width를 가감한다. > Float값으로 리턴 가능
						calibratedRTWidth = ((float)baseRTSize_Width / aspectRatio);
						calibratedRTHeight = baseRTSize_Height;

						//Debug.Log("- AspectRatio : " + aspectRatio);
						//Debug.Log("- 보정 후 RT 기본 크기 : " + calibratedRTWidth + "x" + calibratedRTHeight);
					}
				}
			}


			//색상 인식용 텍스쳐를 삭제한다.
			UnityEngine.Object.DestroyImmediate(resultTex);

		}



		//이전에 계산된 RT 영역을 바탕으로 클리핑 영역을 계산한다.
		public bool CalibrateClipArea(	int RTWidth, int RTHeight,
										float clipRectCenterPosX, float clipRectCenterPosY,
										int clipSizeWidth, int clipSizeHeight,										
										out int resultClipRTPosX_Min, out int resultClipRTPosX_Max,
										out int resultClipRTPosY_Min, out int resultClipRTPosY_Max)
		{
			//- 주어진 RT 크기에 맞게 RT를 만든다. (검은색)
			//- GL 좌표계를 이용하여 ClipRect 영역을 사각형으로 그린다. (흰색)
			//- 색상 인식을 통해서 RT 상의 텍스쳐 좌표계 에서의 Clip 좌표의 좌측 하단을 찾는다.

			//Render Texture를 만든다.
			RenderTexture renderTexture = RenderTexture.GetTemporary(RTWidth, RTHeight, 8, RenderTextureFormat.ARGB32);
			renderTexture.isPowerOfTwo = false;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.filterMode = FilterMode.Point;

			RenderTexture.active = null;
			RenderTexture.active = renderTexture;

			//배경을 검은색으로 만들자
			Color black = Color.black;
			GL.Clear(false, true, black);
			apGL.DrawBoxGL(Vector2.zero, RTWidth * 2, RTHeight * 2, black, false, true);
			GL.Flush();

			//클리핑 영역에 흰색 박스를 그리자 (PixelPerfect)
			//Vector2 centerPos = new Vector2(clipRectCenterPosX, clipRectCenterPosY);
			int posLeft = Mathf.FloorToInt(clipRectCenterPosX - (clipSizeWidth / 2));
			int posBottom = Mathf.FloorToInt(clipRectCenterPosY - (clipSizeHeight / 2));

			//apGL.DrawBoxGL_PixelPerfect(centerPos, clipSizeWidth, clipSizeHeight, Color.white, false, true);
			apGL.DrawBoxGL_PixelPerfect(posLeft, posBottom, clipSizeWidth, clipSizeHeight, Color.white, false, true);
			GL.Flush();


			//텍스쳐 2D로 가져온다.
			Texture2D resultTex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
			resultTex.filterMode = FilterMode.Point;
			resultTex.wrapMode = TextureWrapMode.Clamp;

			resultTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			resultTex.Apply();

			RenderTexture.active = null;

			//렌더 텍스쳐를 해제한다.
			RenderTexture.ReleaseTemporary(renderTexture);


			//색상을 검토하자
			Color[] colors = resultTex.GetPixels();

			int textureWidth = resultTex.width;
			int textureHeight = resultTex.height;
			int arrTotalSize = colors.Length;

			//색상 X의 Min-Max를 구한다.
			bool isAnyMinMax = false;
			int minX = 0;
			int minY = 0;
			int maxX = 0;
			int maxY = 0;

			Color curColor = Color.white;
			float bias = 0.5f;

			//흰색이 있는 구역을 먼저 찾자
			//빠르게 찾기 위해서, 10픽셀씩 이동하며 찾고, 중앙부터 찾는다.
			int iX_AnyData = 0;
			int iY_AnyData = 0;
			bool isFindAnyData = false;

			int centerTexWidth = textureWidth / 2;
			int centerTexHeight = textureHeight / 2;
			
			//1. X를 기준으로 중앙 > 왼쪽으로 찾기
			int iX = 0;
			int iY = 0;

			iX = centerTexWidth;
			while(true)
			{
				if(iX < 0) {  break; }

				//1-1. Y를 기준으로 중앙 > 위쪽으로 찾기
				iY = centerTexHeight;

				while (true)
				{
					if(iY >= textureHeight) { break; }

					int iColor = (iY * textureWidth) + iX;
					if (iColor >= arrTotalSize) { continue; }

					curColor = colors[iColor];

					if (curColor.r > bias)
					{
						//데이터를 찾았다.
						isFindAnyData = true;
						iX_AnyData = iX;
						iY_AnyData = iY;
						break;
					}

					//Y를 10픽셀 증가시킨다.
					iY += 10;
				}

				if(isFindAnyData) { break; }

				//1-2. Y를 기준으로 중앙 > 아래쪽으로 찾기
				iY = centerTexHeight;
				while (true)
				{
					if(iY < 0) { break; }

					int iColor = (iY * textureWidth) + iX;
					if (iColor >= arrTotalSize) { continue; }

					curColor = colors[iColor];

					if (curColor.r > bias)
					{
						//데이터를 찾았다.
						isFindAnyData = true;
						iX_AnyData = iX;
						iY_AnyData = iY;
						break;
					}

					//Y를 10픽셀 감소시킨다.
					iY -= 10;
				}

				if(isFindAnyData) { break; }

				//X를 10 감소 시킨다.
				iX -= 10;
			}


			if(!isFindAnyData)
			{
				//찾지 못한 경우
				//2. X를 기준으로 중앙 > 오른쪽으로 찾기

				while(true)
				{
					if(iX >= textureWidth) {  break; }

					//2-1. Y를 기준으로 중앙 > 위쪽으로 찾기
					iY = centerTexHeight;

					while (true)
					{
						if(iY >= textureHeight) { break; }

						int iColor = (iY * textureWidth) + iX;
						if (iColor >= arrTotalSize) { continue; }

						curColor = colors[iColor];

						if (curColor.r > bias)
						{
							//데이터를 찾았다.
							isFindAnyData = true;
							iX_AnyData = iX;
							iY_AnyData = iY;
							break;
						}

						//Y를 10픽셀 증가시킨다.
						iY += 10;
					}

					if(isFindAnyData) { break; }

					//2-2. Y를 기준으로 중앙 > 아래쪽으로 찾기
					iY = centerTexHeight;
					while (true)
					{
						if(iY < 0) { break; }

						int iColor = (iY * textureWidth) + iX;
						if (iColor >= arrTotalSize) { continue; }

						curColor = colors[iColor];

						if (curColor.r > bias)
						{
							//데이터를 찾았다.
							isFindAnyData = true;
							iX_AnyData = iX;
							iY_AnyData = iY;
							break;
						}

						//Y를 10픽셀 감소시킨다.
						iY -= 10;
					}

					if(isFindAnyData) { break; }

					//X를 10 증가 시킨다.
					iX += 10;
				}
			}

			if (isFindAnyData)
			{
				//데이터를 찾았다면
				// > 그 위치로부터 좌-우 / 상-하로 Min-Max를 찾는다.
				isAnyMinMax = true;
				minX = iX_AnyData;
				maxX = iX_AnyData;

				minY = iY_AnyData;
				maxY = iY_AnyData;

				//왼쪽으로 이동 (X 감소)
				for (iX = iX_AnyData; iX >= 0; iX--)
				{
					int iColor = (iY_AnyData * textureWidth) + iX;
					if (iColor >= arrTotalSize)
					{
						continue;
					}

					curColor = colors[iColor];

					if (curColor.r > bias)
					{
						//Min X 갱신
						minX = Mathf.Min(iX, minX);
					}
					else
					{
						//Min X가 없다면 빠른 종료
						break;
					}
				}

				//오른쪽으로 이동 (X 증가)
				for (iX = iX_AnyData; iX < textureWidth; iX++)
				{
					int iColor = (iY_AnyData * textureWidth) + iX;
					if (iColor >= arrTotalSize)
					{
						continue;
					}

					curColor = colors[iColor];

					if (curColor.r > bias)
					{
						//Max X 갱신
						maxX = Mathf.Max(iX, maxX);
					}
					else
					{
						//Max X가 없다면 빠른 종료
						break;
					}
				}

				//아래로 이동
				for (iY = iY_AnyData; iY >= 0; iY--)
				{
					int iColor = (iY * textureWidth) + iX_AnyData;
					if (iColor >= arrTotalSize)
					{
						continue;
					}

					curColor = colors[iColor];

					if (curColor.r > bias)
					{
						//Min Y 갱신
						minY = Mathf.Min(iY, minY);
					}
					else
					{
						//Min Y가 없다면 빠른 종료
						break;
					}
				}

				//위로 이동
				for (iY = iY_AnyData; iY < textureHeight; iY++)
				{
					int iColor = (iY * textureWidth) + iX_AnyData;
					if (iColor >= arrTotalSize)
					{
						continue;
					}

					curColor = colors[iColor];

					if (curColor.r > bias)
					{
						//Max Y 갱신
						maxY = Mathf.Max(iY, maxY);
					}
					else
					{
						//Max Y가 없다면 빠른 종료
						break;
					}
				}
			}
			else
			{
				//데이터를 찾지 못했다면 전체 검색

				for (iX = 0; iX < textureWidth; iX++)
				{
					for (iY = 0; iY < textureHeight; iY++)
					{
						int iColor = (iY * textureWidth) + iX;
						if (iColor >= arrTotalSize)
						{
							continue;
						}

						curColor = colors[iColor];

						if (curColor.r > bias)
						{
							//사각형 데이터가 있다면
							if (!isAnyMinMax)
							{
								//초기화
								isAnyMinMax = true;
								minX = iX;
								maxX = iX;

								minY = iY;
								maxY = iY;
							}
							else
							{
								minX = Mathf.Min(iX, minX);
								maxX = Mathf.Max(iX, maxX);

								minY = Mathf.Min(iY, minY);
								maxY = Mathf.Max(iY, maxY);
							}
						}

					}
				}
			}


			


			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			//테스트로 RenderTexture (전체)를 저장한다.
			//SaveDebugTexture(resultTex, "클립 보정 체크");
			//if(isDebug)
			//{
			//	SaveDebugTexture(resultTex, "클립 보정 체크");
			//}
			//int test_Day = DateTime.Now.Day;
			//int test_Hour = DateTime.Now.Hour;
			//int test_Min = DateTime.Now.Minute;
			//int test_Sec = DateTime.Now.Second;
			//string testFilePath = "C:\\AnyWorks\\Cal Clip Area - " + test_Day + "-" + test_Hour + "-" + test_Min + "-" + test_Sec;

			//SaveTexture2DToPNG(resultTex, testFilePath, false);
			//System.IO.FileInfo test_fi = new System.IO.FileInfo(testFilePath + ".png");//Path 빈 문자열 확인했음 (21.9.10)

			//Application.OpenURL("file://" + test_fi.Directory.FullName);
			//Application.OpenURL("file://" + test_fi);
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

			//색상 인식용 텍스쳐를 삭제한다.
			UnityEngine.Object.DestroyImmediate(resultTex);

			resultClipRTPosX_Min = 0;
			resultClipRTPosX_Max = 0;
			resultClipRTPosY_Min = 0;
			resultClipRTPosY_Max = 0;

			if (isAnyMinMax)
			{
				resultClipRTPosX_Min = minX;
				resultClipRTPosX_Max = maxX;
				resultClipRTPosY_Min = minY;
				resultClipRTPosY_Max = maxY;

				return true;
			}


			return false;

		}

		public enum SIZE_CHECK_RESULT
		{
			InValid, Valid, SizeWarning, SizeOver
		}

		/// <summary>
		/// 요청된 크기가 최대 값을 넘었다면 경고 메시지를 보여줘야 한다. (v2 기준)		
		/// </summary>
		public SIZE_CHECK_RESULT IsDstSizeOverLimit(	int srcSize_X, int srcSize_Y,
										int dstSize_X, int dstSize_Y,
										out int properDstSizeX, out int properDstSizeY)
		{
			properDstSizeX = 0;
			properDstSizeY = 0;

			if(srcSize_X <= 0 || srcSize_Y <= 0)
			{
				properDstSizeX = 0;
				properDstSizeY = 0;
				return SIZE_CHECK_RESULT.InValid;
			}

			//rtSizeWidth, rtSizeHeight의 크기가 최대 이미지를 벗어나면 에러 메시지 출력하고 처리 막기
			int maxTextureSize = MaxTextureResolution();			

			int editorWindowWidth = Mathf.Max((int)_editor.position.width, 2000);
			int editorWindowHeight = Mathf.Max((int)_editor.position.height, 2000);

			float scaleX = ((float)dstSize_X / (float)srcSize_X);
			float scaleY = ((float)dstSize_Y / (float)srcSize_Y);

			int RTSize_X = (int)((float)editorWindowWidth * scaleX);
			int RTSize_Y = (int)((float)editorWindowHeight * scaleY);

			if(RTSize_X > maxTextureSize
				|| RTSize_Y > maxTextureSize
				|| dstSize_X > maxTextureSize
				|| dstSize_Y > maxTextureSize)
			{
				//크기가 오버되었다.
				//각 축별로 최대 Dst 크기를 지정하자
				//Src > MaxTexture Size에 도달하는 비율을 계산

				//RT 확대 비율
				//(Dst / Src) * EditorWindow = Max
				//Dst / Src = Max / EditorWindow
				//Dst = (Max / EditorWindow) * Src

				properDstSizeX = Mathf.FloorToInt(((float)maxTextureSize / (float)editorWindowWidth) * (float)srcSize_X);
				properDstSizeY = Mathf.FloorToInt(((float)maxTextureSize / (float)editorWindowHeight) * (float)srcSize_Y);

				//200을 넘겼다면, 10 단위 (아래값)로 나누어서 보여주자
				if(properDstSizeX > 200)
				{
					properDstSizeX = (properDstSizeX / 10) * 10;
				}

				if(properDstSizeY > 200)
				{
					properDstSizeY = (properDstSizeY / 10) * 10;
				}

				return SIZE_CHECK_RESULT.SizeOver;//크기가 오버되었다.
			}

			properDstSizeX = 0;
			properDstSizeY = 0;

			//사이즈는 오버되지 않았더라도,
			//스케일되는 RT 크기가 6000이 넘었다면 시간이 꽤 걸리므로 경고가 나와야 한다.
			if(RTSize_X > 6000 || RTSize_Y > 6000)
			{
				return SIZE_CHECK_RESULT.SizeWarning;
			}


			//괜찮당
			return SIZE_CHECK_RESULT.Valid;

		}




		public bool SaveTexture2DToPNG(Texture2D srcTexture2D, string filePathWithExtension, bool isAutoDestroy)
		{
			try
			{
				if (srcTexture2D == null)
				{
					return false;
				}

				File.WriteAllBytes(filePathWithExtension + ".png", srcTexture2D.EncodeToPNG());

				if (isAutoDestroy)
				{
					UnityEngine.Object.DestroyImmediate(srcTexture2D);
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("SaveTexture2DToPNG Exception : " + ex);

				if (isAutoDestroy)
				{
					UnityEngine.Object.Destroy(srcTexture2D);
				}
				return false;
			}
		}


		private void SaveDebugTexture(Texture2D srcTexture2D, string fileBaseName)
		{
			int day = DateTime.Now.Day;
			int hour = DateTime.Now.Hour;
			int min = DateTime.Now.Minute;
			int sec = DateTime.Now.Second;
			string testFullFilePath = "C:\\AnyWorks\\" + fileBaseName + " - " + day + "-" + hour + "-" + min + "-" + sec;

			SaveTexture2DToPNG(srcTexture2D, testFullFilePath, false);
			System.IO.FileInfo test_Fullfi = new System.IO.FileInfo(testFullFilePath + ".png");//Path 빈 문자열 확인했음 (21.9.10)

			Application.OpenURL("file://" + test_Fullfi.Directory.FullName);
			Application.OpenURL("file://" + test_Fullfi);
		}




		public void SaveCache(	//Cond
								int editorSize_Width,
								int editorSize_Height,
								float clipRectCenterPosX,
								float clipRectCenterPosY,
								int srcSizeWidth,
								int srcSizeHeight,
								int dstSizeWidth,
								int dstSizeHeight,

								//Result
								int calibratedRTSize_Width,
								int calibratedRTSize_Height,
								int calibratedClipArea_PosX,
								int calibratedClipArea_PosY,
								int calibratedClipArea_Width,
								int calibratedClipArea_Height
								)
		{
			_isCalibrationCache = true;
		
			_caliCache_Cond_EditorSize_Width = editorSize_Width;
			_caliCache_Cond_EditorSize_Height = editorSize_Height;
			_caliCache_Cond_ClipRectCenterPosX = clipRectCenterPosX;
			_caliCache_Cond_ClipRectCenterPosY = clipRectCenterPosY;
			_caliCache_Cond_ClipSizeWidth = srcSizeWidth;
			_caliCache_Cond_ClipSizeHeight = srcSizeHeight;
			_caliCache_Cond_DstSizeWidth = dstSizeWidth;
			_caliCache_Cond_DstSizeHeight = dstSizeHeight;

			_caliCache_Result_RTSize_Width = calibratedRTSize_Width;
			_caliCache_Result_RTSize_Height = calibratedRTSize_Height;

			_caliCache_Result_ClipArea_PosX = calibratedClipArea_PosX;
			_caliCache_Result_ClipArea_PosY = calibratedClipArea_PosY;
			
			_caliCache_Result_ClipArea_Width = calibratedClipArea_Width;
			_caliCache_Result_ClipArea_Height = calibratedClipArea_Height;
		}




		//TODO : 이 값을 이용할 것

		/// <summary>
		/// RT를 포함한 텍스쳐의 최대 해상도.
		/// 이것보다 크다면 생성을 취소해야한다.
		/// </summary>
		/// <returns></returns>
		public int MaxTextureResolution()
		{	
			return Mathf.Min(16384, SystemInfo.maxTextureSize);
		}

		//------------------------------------------------------------------------------------------
		public bool MakeGIFHeader(	string filePath,
									apAnimClip animClip,
									int dstSizeWidth, int dstSizeHeight)
		{
			//일단 파일 스트림을 꺼준다.
			if(_gifFileStream != null)
			{
				try
				{
					_gifFileStream.Close();
				}
				catch(Exception) { }
				_gifFileStream = null;
			}

			float secPerFrame = 1.0f / (float)animClip.FPS;

			//파일 스트림을 만들고 GIF 헤더 작성
			try
			{
				_gifFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

				_ngif.WriteHeader(_gifFileStream);
				_ngif.SetGIFSetting((int)((secPerFrame * 100.0f) + 0.5f), 0, dstSizeWidth, dstSizeHeight);
			}
			catch(Exception)
			{
				if(_gifFileStream != null)
				{
					_gifFileStream.Close();
				}
				_gifFileStream = null;
				return false;
			}

			return true;
		}


		public bool AddGIFFrame(Texture2D frameImage, bool isFirstFrame, int quality)
		{
			if(_gifFileStream == null)
			{
				//이미 처리가 끝났네요.
				return false;
			}

			try
			{
				_ngif.AddFrame(frameImage, _gifFileStream, isFirstFrame, quality);
			}
			catch(Exception)
			{

			}
			UnityEngine.Object.DestroyImmediate(frameImage);
			return true;
		}

		public void EndGIF()
		{
			if(_gifFileStream == null)
			{
				//이미 처리가 끝났네요.
				return;
			}

			try
			{
				_ngif.Finish(_gifFileStream);

				_gifFileStream.Close();
				_gifFileStream = null;
			}
			catch(Exception)
			{
				if (_gifFileStream != null)
				{
					_gifFileStream.Close();
				}
				_gifFileStream = null;
			}
		}
		


		// MP4 함수들
		//-------------------------------------------------------------------------------
		public bool MakeMP4Animation(	string filePath, 
										apAnimClip animClip,
										int dstSizeWidth, int dstSizeHeight
										)
		{
			Clear();

			if (_editor == null || _editor._portrait == null || animClip == null)
			{
				return false;
			}

#if UNITY_2017_4_OR_NEWER

			_dstMP4SizeWidth = dstSizeWidth;
			_dstMP4SizeHeight = dstSizeHeight;
			
			int frameRate = animClip.FPS;
			
			_videoAttr.frameRate = new MediaRational(frameRate);
			_videoAttr.width = (uint)_dstMP4SizeWidth;
			_videoAttr.height = (uint)_dstMP4SizeHeight;
			_videoAttr.includeAlpha = false;
			
#if UNITY_2018_1_OR_NEWER
			//2018에서는 더 고화질의 영상을 뽑을 수 있다.
			_videoAttr.bitRateMode = UnityEditor.VideoBitrateMode.High;
#endif

			try
			{
				_mediaEncoder = new MediaEncoder(filePath, _videoAttr);
			}
			catch(Exception)
			{
				Clear();
				return false;
			}

			return true;
#else
			return false;
#endif
		}


		public bool AddMP4Frame(Texture2D frameImage)
		{
#if UNITY_2017_4_OR_NEWER
			if(_mediaEncoder == null)
			{
				return false;
			}

			try
			{
				//사이즈는 이미 조절되서 들어온다.
				////사이즈가 안맞다면 리사이즈
				//if(frameImage.width != _dstMP4SizeWidth || 
				//	frameImage.height != _dstMP4SizeHeight)
				//{
				//	Texture2D newTex = ResizeTexture(frameImage, _dstMP4SizeWidth, _dstMP4SizeHeight);
				//	UnityEngine.Object.DestroyImmediate(frameImage);
				//	frameImage = newTex;
				//}
				
				//포맷을 바꿔야 한다.
				Texture2D newTex = new Texture2D(frameImage.width, frameImage.height, TextureFormat.RGBA32, false);
				newTex.SetPixels(frameImage.GetPixels());
				newTex.Apply();

				UnityEngine.Object.DestroyImmediate(frameImage);
				frameImage = newTex;

				_mediaEncoder.AddFrame(frameImage);
			}
			catch(Exception)
			{

			}
			UnityEngine.Object.DestroyImmediate(frameImage);
			return true;			
#else
			return false;
#endif
		}


		public void EndMP4()
		{
			Clear();
		}


		// Sub Functions
		//---------------------------------------------------------------------------------
		/// <summary>
		/// Fix를 위해서 Blur 이미지를 만든다.
		/// Fix용이므로 만약 Alpha 채널에 해당하는 값이라면 Blur에 포함되지 않는것이 특징
		/// 매우 강한 색상으로 블러가 된다.
		/// </summary>
		/// <param name="srcTex"></param>
		/// <param name="srcAlphaTex"></param>
		/// <param name="blurSize"></param>
		/// <returns></returns>
		private Texture2D MakeBlurImage(Texture2D srcTex, Texture2D srcAlphaTex, int blurSize)
		{
			
			Color[] srcColorArr = srcTex.GetPixels(0);
			Color[] srcAlphaArr = srcAlphaTex.GetPixels(0);

			float width = srcTex.width;
			float height = srcTex.height;
 
			//Make New
			Texture2D resultTex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
 
			//Make destination array
			int length = (int)width * (int)height;
			Color[] resultColorArr = new Color[length];
			float[] resultWeightArr = new float[length];
			for (int i = 0; i < length; i++)
			{
				resultColorArr[i] = Color.clear;
				resultWeightArr[i] = 0.0f;
			}


			if(blurSize < 2)
			{
				blurSize = 2;
			}

			//Color resultColor = Color.black;
			Color curColor = Color.black;
			float curAlpha = 0.0f;
			Color curSubColor = Color.black;
			int iSubTex = 0;

			//Dst -> Sample 방식이 아니라
			//Src -> 결과 누적 방식
			//조금 더 빠를 것이다.
			for (int i = 0; i < length; i++)
			{	
				float iX = (float)i % width;
				float iY = Mathf.Floor((float)i / width);

				curColor = srcColorArr[i];
				curAlpha = srcAlphaArr[i].r;

				//resultColor = Color.clear;

				if(curAlpha < 0.8f)
				{
					//0.5 이하는 블러처리 하지 않는다.
					//자기 자신에만 값 넣는다.
					resultColorArr[i] += curColor;
					resultWeightArr[i] += 1.0f;
				}
				else
				{
					//0.5 이상은 주변에 블러 샘플링을 넣어준다.
					int iX_A = (int)Mathf.Max(iX - blurSize, 0);
					int iX_B = (int)Mathf.Min(iX + blurSize, width - 1);
					int iY_A = (int)Mathf.Max(iY - blurSize, 0);
					int iY_B = (int)Mathf.Min(iY + blurSize, height - 1);

					for (int iSubX = iX_A; iSubX <= iX_B; iSubX++)
					{
						for (int iSubY = iY_A; iSubY <= iY_B; iSubY++)
						{
							iSubTex = iSubX + (int)(iSubY * width);
							resultColorArr[iSubTex] += curColor;
							resultWeightArr[iSubTex] += 1.0f;
						}
					}
				}
			}

			for (int i = 0; i < length; i++)
			{
				resultColorArr[i] /= resultWeightArr[i];
			}

			resultTex.SetPixels(resultColorArr);
			resultTex.Apply();

			
 
			//*** Return
			return resultTex;
		}



		public Texture2D ResizeTexture(	Texture2D srcColorTex, 
										Texture2D srcAlphaTex, 
										//Texture2D srcBlurTex, 
										int dstWidth, int dstHeight)
		{
			Color[] srcColorArr = srcColorTex.GetPixels(0);
			Color[] srcAlphaArr = srcAlphaTex.GetPixels(0);
			//Color[] srcBlurArr = srcBlurTex.GetPixels(0);

			int iSrcWidth = srcColorTex.width;
			int iSrcHeight = srcColorTex.height;

			float fSrcWidth = iSrcWidth;
			float fSrcHeight = iSrcHeight;
 
			//New Size
			float fDstWidth = dstWidth;
			float fDstHeight = dstHeight;
 
			//Make New
			Texture2D resultTex = new Texture2D(dstWidth, dstHeight, TextureFormat.ARGB32, false);
 
			//Make destination array
			int srcLength = srcColorTex.width * srcColorTex.height;
			int dstLength = dstWidth * dstHeight;

			Color[] resultColorArr = new Color[dstLength];
			float[] resultWeightArr = new float[dstLength];

			for (int i = 0; i < dstLength; i++)
			{
				resultColorArr[i] = Color.clear;
				resultWeightArr[i] = 0.0f;
			}
			
			
			Color resultColor = new Color();
			Color curColor;
			float curAlpha = 0.0f;

			//Offset_Src : 이미지가 작아질 때 1보다 크다
			//Offset_Dst : 이미지가 커질때 1보다 크다

			float offsetSrcX = fSrcWidth / fDstWidth;
			float offsetSrcY = fSrcHeight / fDstHeight;
			float offsetDstX = fDstWidth / fSrcWidth;
			float offsetDstY = fDstHeight / fSrcHeight;

			float offsetSrcX_Half = Mathf.Max(offsetSrcX * 0.5f, 0);
			float offsetSrcY_Half = Mathf.Max(offsetSrcY * 0.5f, 0);
			float offsetDstX_Half = Mathf.Max(offsetDstX * 0.5f, 0);
			float offsetDstY_Half = Mathf.Max(offsetDstY * 0.5f, 0);

			//이미지가 커질땐 조금 더 오버 샘플링을 해야한다.
			if (dstWidth > iSrcWidth)
			{
				//offsetSrcX_Half += 1;
				offsetDstX_Half += 1;
			}
			if (dstHeight > iSrcHeight)
			{
				//offsetSrcY_Half += 1;
				offsetDstY_Half += 1;
			}


			float maxDiff_SrcX = offsetSrcX_Half + 1;
			float maxDiff_SrcY = offsetSrcY_Half + 1;
			float maxDiff_DstX = offsetDstX_Half + 1;
			float maxDiff_DstY = offsetDstY_Half + 1;

			
			//Dst -> Sample에서
			//Src -> 결과 누적 방식으로 변경
			for (int iSrc = 0; iSrc < srcLength; iSrc++)
			{
				int iSrcX = iSrc % iSrcWidth;
				int iSrcY = iSrc / iSrcWidth;

				curColor = srcColorArr[iSrc];
				curAlpha = srcAlphaArr[iSrc].r;

				resultColor = curColor;
				resultColor.a = curAlpha;

				float fDstX_Expect = (iSrcX * fDstWidth) / fSrcWidth;
				float fDstY_Expect = (iSrcY * fDstHeight) / fSrcHeight;

				int iSrcX_Min = iSrcX - (int)offsetSrcX_Half;
				int iSrcY_Min = iSrcY - (int)offsetSrcY_Half;
				int iSrcX_Max = iSrcX + (int)offsetSrcX_Half;
				int iSrcY_Max = iSrcY + (int)offsetSrcY_Half;

				int iDstX_Min = (int)Mathf.Max(Mathf.Floor((iSrcX_Min * fDstWidth) / fSrcWidth - offsetDstX_Half), 0);
				int iDstX_Max = (int)Mathf.Min(Mathf.Ceil((iSrcX_Max * fDstWidth) / fSrcWidth + offsetDstX_Half), dstWidth - 1);
				int iDstY_Min = (int)Mathf.Max(Mathf.Floor((iSrcY_Min * fDstHeight) / fSrcHeight - offsetDstY_Half), 0);
				int iDstY_Max = (int)Mathf.Min(Mathf.Ceil((iSrcY_Max * fDstHeight) / fSrcHeight + offsetDstY_Half), dstHeight - 1);

				float fSrcX_Expect = 0.0f;
				float fSrcY_Expect = 0.0f;

				float dstDiffX = 0.0f;
				float dstDiffY = 0.0f;

				float srcDiffX = 0.0f;
				float srcDiffY = 0.0f;

				float srcWeight = 0.0f;
				float dstWeight = 0.0f;
				float curWeight = 0.0f;
				int iDst = 0;

				for (int iDstX = iDstX_Min; iDstX <= iDstX_Max; iDstX++)
				{
					for (int iDstY = iDstY_Min; iDstY <= iDstY_Max; iDstY++)
					{
						//1. dstDiff : 원래 이 Src가 원하던 Dst와 현재 Dst와의 차이
						//2. srdDiff : 현재 Dst의 Src와 현재 Src와의 차이
						fSrcX_Expect = (iDstX * fSrcWidth) / fDstWidth;
						fSrcY_Expect = (iDstY * fSrcHeight) / fDstHeight;

						dstDiffX = Mathf.Abs(fDstX_Expect - iDstX);
						dstDiffY = Mathf.Abs(fDstY_Expect - iDstY);

						srcDiffX = Mathf.Abs(fSrcX_Expect - iSrcX);
						srcDiffY = Mathf.Abs(fSrcY_Expect - iSrcY);

						//float srcWeight = Mathf.Pow(1.0f - (srcDiffX / maxDiff_SrcX), 2) * Mathf.Pow(1.0f - (srcDiffY / maxDiff_SrcY), 2);
						//float dstWeight = Mathf.Pow(1.0f - (dstDiffX / maxDiff_DstX), 2) * Mathf.Pow(1.0f - (dstDiffY / maxDiff_DstY), 2);
						srcWeight = (1.0f - (srcDiffX / maxDiff_SrcX)) * (1.0f - (srcDiffY / maxDiff_SrcY));
						dstWeight = (1.0f - (dstDiffX / maxDiff_DstX)) * (1.0f - (dstDiffY / maxDiff_DstY));

						curWeight = srcWeight * dstWeight * (curAlpha + 0.1f);//0.1은 Bias

						iDst = (iDstY * dstWidth) + iDstX;

						resultColorArr[iDst] += resultColor * curWeight;
						resultWeightArr[iDst] += curWeight;
					}
				}
			}

			

			for (int i = 0; i < dstLength; i++)
			{
				resultColorArr[i] /= resultWeightArr[i];
			}
			
			//*** Set Pixels
			resultTex.SetPixels(resultColorArr);
			resultTex.Apply();

			
			
			//*** Return
			return resultTex;
		}



		public Texture2D MergeAlphaChannel(	Texture2D srcColorTex, 
											Texture2D srcAlphaTex 
											//Texture2D srcBlurTex
											)
		{
 			Color[] srcColorArr = srcColorTex.GetPixels(0);
			Color[] srcAlphaArr = srcAlphaTex.GetPixels(0);
			//Color[] srcBlurArr = srcBlurTex.GetPixels(0);
			
			//New Size
			float width = srcColorTex.width;
			float height = srcColorTex.height;
 
			//Make New
			Texture2D resultTex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
 
			//Make destination array
			int length = (int)width * (int)height;
			Color[] resultColorArr = new Color[length];
 
			Color resultColor = Color.clear;
			Color curColor;
			//Color curBlur;
			float curAlpha = 0.0f;

			for (int i = 0; i < length; i++)
			{	
				resultColor = Color.clear;
				curColor = srcColorArr[i];
				curAlpha = srcAlphaArr[i].r;
				//curBlur = srcBlurArr[i];

				//if (curAlpha > 0.0f && curAlpha < 1.0f)
				//{
				//	resultColor = curBlur * (1.0f - curAlpha) + curColor * curAlpha;
				//	resultColor.a = curAlpha;
				//}
				//else
				//{
				//	resultColor = curColor;
				//	resultColor.a = curAlpha;
				//}
				resultColor = curColor;
				resultColor.a = curAlpha;

				//resultColor = curBlurTemp;

				resultColorArr[i] = resultColor;
			}
 
			//*** Set Pixels
			resultTex.SetPixels(resultColorArr);
			resultTex.Apply();
 
			//*** Return
			return resultTex;
		}
		

		private Texture2D BlurTextureWithComputeShader(	Texture2D srcColorTex, 
														Texture2D srcAlphaTex, int blurSize)
		{
			//추가 20.4.21
			string basePath = "Assets/AnyPortrait/";
			if(_editor != null)
			{
				basePath = apPathSetting.I.CurrentPath;
			}
			//ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/AnyPortrait/Editor/Scripts/Subtool/CShader/apCShader_Blur.compute");
			ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(basePath + "Editor/Scripts/Subtool/CShader/apCShader_Blur.compute");

			if(cShader == null)
			{
				//Debug.LogError("No Compute Shader");
				return null;
			}

			int kernel = cShader.FindKernel("CSMain");
			if(kernel < 0)
			{
				//커널 찾기에 실패했다.
				return null;
			}

			int srcWidth = srcColorTex.width;
			int srcHeight = srcColorTex.height;

			RenderTexture resultTex = RenderTexture.GetTemporary(srcWidth, srcHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			resultTex.enableRandomWrite = true;
			resultTex.Create();

			cShader.SetTexture(kernel, "Result", resultTex);
			
			cShader.SetTexture(kernel, "SrcColorTex", srcColorTex);
			cShader.SetTexture(kernel, "SrcAlphaTex", srcAlphaTex);

			cShader.SetInt("srcWidth", srcWidth);
			cShader.SetInt("srcHeight", srcHeight);
			cShader.SetInt("blurSize", blurSize);

			cShader.Dispatch(kernel, srcWidth / 8 + 1, srcHeight / 8 + 1, 1);

			RenderTexture.active = resultTex;
			Texture2D blurredTex = new Texture2D(srcWidth, srcHeight, TextureFormat.ARGB32, false);
			blurredTex.ReadPixels(new Rect(0, 0, resultTex.width, resultTex.height), 0, 0);
			blurredTex.Apply();

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(resultTex);
			

			return blurredTex;
		}


		private Texture2D ResizeTextureWithComputeShader(	Texture2D srcColorTex, 
															Texture2D srcAlphaTex,
															int dstWidth, int dstHeight)
		{
			int srcWidth = srcColorTex.width;
			int srcHeight = srcColorTex.height;

			int blurSize = 4;
			if(dstWidth < srcWidth)
			{
				blurSize = Mathf.Max(blurSize, srcWidth / dstWidth);
			}
			if(dstHeight < srcHeight)
			{
				blurSize = Mathf.Max(blurSize, srcHeight / dstHeight);
			}

			Texture2D blurTexture = BlurTextureWithComputeShader(srcColorTex, srcAlphaTex, blurSize);
			if(blurTexture == null)
			{
				//Debug.LogError("No Pre-Texture");
				return null;
			}

			//추가 20.4.21
			string basePath = "Assets/AnyPortrait/";
			if(_editor != null)
			{
				basePath = apPathSetting.I.CurrentPath;
			}

			ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(basePath + "Editor/Scripts/Subtool/CShader/apCShader_ResizeTexture.compute");

			if(cShader == null)
			{
				//Debug.LogError("No Compute Shader");
				return null;
			}

			int kernel = cShader.FindKernel("CSMain");
			if(kernel < 0)
			{
				//커널 찾기에 실패했다.
				return null;
			}

			

			float offsetSrcX = (float)srcWidth / (float)dstWidth;
			float offsetSrcY = (float)srcHeight / (float)dstHeight;
			float offsetDstX = (float)dstWidth / (float)srcWidth;
			float offsetDstY = (float)dstHeight / (float)srcHeight;
			int offsetSrcX_Half = (int)Mathf.Max(offsetSrcX * 0.5f, 1);
			int offsetSrcY_Half = (int)Mathf.Max(offsetSrcY * 0.5f, 1);
			int offsetDstX_Half = (int)Mathf.Max(offsetDstX * 0.5f, 1);
			int offsetDstY_Half = (int)Mathf.Max(offsetDstY * 0.5f, 1);
			
			//이미지가 커질땐 조금 더 오버 샘플링을 해야한다.
			if(dstWidth > srcWidth)
			{
				offsetSrcX_Half += 2;
				offsetDstX_Half += 1;
			}
			if(dstHeight > srcHeight)
			{
				offsetSrcY_Half += 2;
				offsetDstY_Half += 1;
			}
			
			RenderTexture resultTex = RenderTexture.GetTemporary(dstWidth, dstHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			resultTex.enableRandomWrite = true;
			resultTex.Create();

			cShader.SetTexture(kernel, "Result", resultTex);
			
			cShader.SetTexture(kernel, "SrcColorTex", srcColorTex);
			cShader.SetTexture(kernel, "SrcAlphaTex", srcAlphaTex);
			cShader.SetTexture(kernel, "SrcBlurTex", blurTexture);
			

			cShader.SetInt("srcWidth", srcWidth);
			cShader.SetInt("srcHeight", srcHeight);
			cShader.SetInt("dstWidth", dstWidth);
			cShader.SetInt("dstHeight", dstHeight);
			cShader.SetInt("srcOffsetX", offsetSrcX_Half);
			cShader.SetInt("srcOffsetY", offsetSrcY_Half);
			cShader.SetInt("dstOffsetX", offsetDstX_Half);
			cShader.SetInt("dstOffsetY", offsetDstY_Half);

			cShader.Dispatch(kernel, dstWidth / 8 + 1, dstHeight / 8 + 1, 1);

			RenderTexture.active = resultTex;
			Texture2D resizeTex = new Texture2D(dstWidth, dstHeight, TextureFormat.ARGB32, false);
			resizeTex.ReadPixels(new Rect(0, 0, resultTex.width, resultTex.height), 0, 0);
			resizeTex.Apply();

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(resultTex);
			UnityEngine.GameObject.DestroyImmediate(blurTexture);

			return resizeTex;
		}



		public Texture2D MergeAlphaChannelWithComputeShader(Texture2D srcColorTex, Texture2D srcAlphaTex)
		{
 			int width = srcColorTex.width;
			int height = srcColorTex.height;
			int blurSize = 4;

			Texture2D blurTexture = BlurTextureWithComputeShader(srcColorTex, srcAlphaTex, blurSize);
			if(blurTexture == null)
			{
				//Debug.LogError("No Pre-Texture");
				return null;
			}

			//추가 20.4.21
			string basePath = "Assets/AnyPortrait/";
			if(_editor != null)
			{
				basePath = apPathSetting.I.CurrentPath;
			}

			//ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/AnyPortrait/Editor/Scripts/Subtool/CShader/apCShader_MergeChannels.compute");
			ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(basePath + "Editor/Scripts/Subtool/CShader/apCShader_MergeChannels.compute");


			if(cShader == null)
			{
				//Debug.LogError("No Compute Shader");
				return null;
			}

			int kernel = cShader.FindKernel("CSMain");
			if(kernel < 0)
			{
				//커널 찾기에 실패했다.
				return null;
			}
			
			
			RenderTexture resultTex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			resultTex.enableRandomWrite = true;
			resultTex.Create();

			cShader.SetTexture(kernel, "Result", resultTex);
			
			cShader.SetTexture(kernel, "SrcColorTex", srcColorTex);
			cShader.SetTexture(kernel, "SrcAlphaTex", srcAlphaTex);
			cShader.SetTexture(kernel, "SrcBlurTex", blurTexture);
			
			cShader.Dispatch(kernel, width / 8 + 1, height / 8 + 1, 1);

			RenderTexture.active = resultTex;
			Texture2D resizeTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
			resizeTex.ReadPixels(new Rect(0, 0, resultTex.width, resultTex.height), 0, 0);
			resizeTex.Apply();

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(resultTex);

			UnityEngine.GameObject.DestroyImmediate(blurTexture);

			return resizeTex;
		}

		//GIF 함수
		//----------------------------------------------


		// Get / Set
		//----------------------------------------------


		//-------------------------------------------------------------------------------
		private int GetProperRenderTextureSize(int size)
		{
			if(size < 256)			{ return 256; }
			else if(size < 512)		{ return 512; }
			else if(size < 1024)	{ return 1024; }
			else if(size < 2048)	{ return 2048; }
			else { return 4096; }
		}
	}

}