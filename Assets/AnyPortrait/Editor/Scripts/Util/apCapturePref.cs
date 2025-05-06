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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// v1.5.1 : 캡쳐 설정을 파일로 저장할 수 있다.
	/// 멤버는 없고 Static 함수만 제공한다.
	/// </summary>
	public static class apCapturePref
	{
		public static string EXP { get { return "cpref"; } }
		/// <summary>
		/// 현재 설정을 저장한다.
		/// </summary>
		public static bool SaveCurrent(apEditor editor, string savePath)
		{
			if(editor == null || string.IsNullOrEmpty(savePath))
			{
				return false;
			}

			FileStream fs = null;
			StreamWriter sw = null;

			try
			{
				fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs);

				//각 항목을 저장하자
				//키 (4) + :(1) + 값을 저장하자
				sw.WriteLine("POSX:" + editor._captureFrame_PosX);
				sw.WriteLine("POSY:" + editor._captureFrame_PosY);

				sw.WriteLine("SRCW:" + editor._captureFrame_SrcWidth);
				sw.WriteLine("SRCH:" + editor._captureFrame_SrcHeight);

				sw.WriteLine("DSTW:" + editor._captureFrame_DstWidth);
				sw.WriteLine("DSTH:" + editor._captureFrame_DstHeight);

				sw.WriteLine("SPUW:" + editor._captureFrame_SpriteUnitWidth);
				sw.WriteLine("SPUH:" + editor._captureFrame_SpriteUnitHeight);
				sw.WriteLine("SPMA:" + editor._captureFrame_SpriteMargin);

				sw.WriteLine("COLR:" + editor._captureFrame_Color.r);
				sw.WriteLine("COLG:" + editor._captureFrame_Color.g);
				sw.WriteLine("COLB:" + editor._captureFrame_Color.b);
				sw.WriteLine("COLA:" + editor._captureFrame_Color.a);

				sw.WriteLine("PHYS:" + (editor._captureFrame_IsPhysics ? "T" : "F"));
				sw.WriteLine("ASPF:" + (editor._isCaptureAspectRatioFixed ? "T" : "F"));

				sw.WriteLine("GIFQ:" + (int)editor._captureFrame_GIFQuality);
				sw.WriteLine("LOOP:" + editor._captureFrame_GIFSampleLoopCount);

				sw.WriteLine("SPIW:" + (int)editor._captureSpritePackImageWidth);
				sw.WriteLine("SPIH:" + (int)editor._captureSpritePackImageHeight);
				sw.WriteLine("STRM:" + (int)editor._captureSpriteTrimSize);

				sw.WriteLine("MXML:" + (editor._captureSpriteMeta_XML ? "T" : "F"));
				sw.WriteLine("MJSN:" + (editor._captureSpriteMeta_JSON ? "T" : "F"));
				sw.WriteLine("MTXT:" + (editor._captureSpriteMeta_TXT ? "T" : "F"));

				sw.WriteLine("SCPX:" + editor._captureSprite_ScreenPos.x);
				sw.WriteLine("SCPY:" + editor._captureSprite_ScreenPos.y);

				sw.WriteLine("ZOOM:" + editor._captureSprite_ScreenZoom);
				sw.WriteLine("FOCX:" + editor._captureFocusOffsetPX_X);
				sw.WriteLine("FOCY:" + editor._captureFocusOffsetPX_Y);

				sw.Flush();

				sw.Close();
				fs.Close();

				sw = null;
				fs = null;

				return true;
			}
			catch(Exception ex)
			{ 
				Debug.LogError("AnyPortrait : Capture Pref Error\n" + ex.ToString());
				
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

				return false;
			}
		}

		public enum LOAD_RESULT
		{
			Success,
			Failed_NoFile,
			Failed_Unknown,
		}

		/// <summary>
		/// 설정을 로드하여 가져온다.
		/// </summary>
		public static LOAD_RESULT LoadFile(apEditor editor, string filePath)
		{
			if(editor == null || string.IsNullOrEmpty(filePath))
			{
				return LOAD_RESULT.Failed_Unknown;
			}

			FileInfo fi = new FileInfo(filePath);
			if(!fi.Exists)
			{
				//실패 : 파일이 없다.
				return LOAD_RESULT.Failed_NoFile;
			}

			FileStream fs = null;
			StreamReader sr = null;

			try
			{
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs);

				while (true)
				{
					if (sr.Peek() < 0)
					{
						break;
					}

					//키 (4) + :(1) + 값을 저장하자
					string strRead = sr.ReadLine();

					if(strRead.Length < 5)
					{
						continue;
					}

					string strKey = strRead.Substring(0, 4);
					string strValue = strRead.Substring(5);

					if (string.Equals(strKey, "POSX")) { int.TryParse(strValue, out editor._captureFrame_PosX); }
					else if (string.Equals(strKey, "POSY")) { int.TryParse(strValue, out editor._captureFrame_PosY); }
					else if (string.Equals(strKey, "SRCW")) { int.TryParse(strValue, out editor._captureFrame_SrcWidth); }
					else if (string.Equals(strKey, "SRCH")) { int.TryParse(strValue, out editor._captureFrame_SrcHeight); }
					else if (string.Equals(strKey, "DSTW")) { int.TryParse(strValue, out editor._captureFrame_DstWidth); }
					else if (string.Equals(strKey, "DSTH")) { int.TryParse(strValue, out editor._captureFrame_DstHeight); }
					else if (string.Equals(strKey, "SPUW")) { int.TryParse(strValue, out editor._captureFrame_SpriteUnitWidth); }
					else if (string.Equals(strKey, "SPUH")) { int.TryParse(strValue, out editor._captureFrame_SpriteUnitHeight); }
					else if (string.Equals(strKey, "SPMA")) { int.TryParse(strValue, out editor._captureFrame_SpriteMargin); }
					else if (string.Equals(strKey, "COLR")) { float.TryParse(strValue, out editor._captureFrame_Color.r); }
					else if (string.Equals(strKey, "COLG")) { float.TryParse(strValue, out editor._captureFrame_Color.g); }
					else if (string.Equals(strKey, "COLB")) { float.TryParse(strValue, out editor._captureFrame_Color.b); }
					else if (string.Equals(strKey, "COLA")) { float.TryParse(strValue, out editor._captureFrame_Color.a); }
					else if (string.Equals(strKey, "PHYS")) { editor._captureFrame_IsPhysics = strValue.StartsWith("T"); }
					else if (string.Equals(strKey, "ASPF")) { editor._isCaptureAspectRatioFixed = strValue.StartsWith("T"); }
					else if (string.Equals(strKey, "GIFQ"))
					{
						editor._captureFrame_GIFQuality = (apEditor.CAPTURE_GIF_QUALITY)int.Parse(strValue);
					}
					else if (string.Equals(strKey, "LOOP")) { int.TryParse(strValue, out editor._captureFrame_GIFSampleLoopCount); }
					else if (string.Equals(strKey, "SPIW"))
					{
						editor._captureSpritePackImageWidth = (apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE)int.Parse(strValue);
					}
					else if (string.Equals(strKey, "SPIH"))
					{
						editor._captureSpritePackImageHeight = (apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE)int.Parse(strValue);
					}
					else if (string.Equals(strKey, "STRM"))
					{
						editor._captureSpriteTrimSize = (apEditor.CAPTURE_SPRITE_TRIM_METHOD)int.Parse(strValue);
					}
					else if (string.Equals(strKey, "MXML")) { editor._captureSpriteMeta_XML = strValue.StartsWith("T"); }
					else if (string.Equals(strKey, "MJSN")) { editor._captureSpriteMeta_JSON = strValue.StartsWith("T"); }
					else if (string.Equals(strKey, "MTXT")) { editor._captureSpriteMeta_TXT = strValue.StartsWith("T"); }
					else if (string.Equals(strKey, "SCPX")) { float.TryParse(strValue, out editor._captureSprite_ScreenPos.x); }
					else if (string.Equals(strKey, "SCPY")) { float.TryParse(strValue, out editor._captureSprite_ScreenPos.y); }
					else if (string.Equals(strKey, "ZOOM")) { int.TryParse(strValue, out editor._captureSprite_ScreenZoom); }
					else if (string.Equals(strKey, "FOCX")) { int.TryParse(strValue, out editor._captureFocusOffsetPX_X); }
					else if (string.Equals(strKey, "FOCY")) { int.TryParse(strValue, out editor._captureFocusOffsetPX_Y); }
				}

				sr.Close();
				fs.Close();

				return LOAD_RESULT.Success;
			}
			catch(Exception ex)
			{ 
				Debug.LogError("AnyPortrait : Capture Pref Error\n" + ex.ToString());
				
				if(sr != null)
				{
					sr.Close();
					sr = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}

				return LOAD_RESULT.Failed_Unknown;
			}
		}
		
	}
}