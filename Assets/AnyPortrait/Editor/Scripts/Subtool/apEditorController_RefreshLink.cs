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
using System;
using System.Collections.Generic;

using AnyPortrait;
using System.Text;

namespace AnyPortrait
{
	public partial class apEditorController
	{
		//--------------------------------------------------
		// 3-0. 초기화
		//--------------------------------------------------
		public void PortraitReadyToEdit()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			//추가 19.6.3 : MaterialSet 데이터를 갱신해야한다.
			LinkMaterialSets(Editor._portrait);

			Editor._portrait.ReadyToEdit();

			//추가
			//썸네일을 찾아서 연결해보자
			string thumnailPath = Editor._portrait._imageFilePath_Thumbnail;
			if (string.IsNullOrEmpty(thumnailPath))
			{
				Editor._portrait._thumbnailImage = null;
			}
			else
			{
				Texture2D thumnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumnailPath);
				Editor._portrait._thumbnailImage = thumnailImage;
			}

			RefreshMeshGroups();

			//Selection.activeGameObject = Editor.Select.Portrait.gameObject;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			//이름을 갱신하자
			CheckAndRefreshGameObjectNames(Editor._portrait);

			//추가 : 삭제되어야 하는 데이터가 있을 수도 있다. 검색하자
			//CheckAndRemoveUnusedModifierData(Editor._portrait, true, true);//<<보류


			//SetDirty를 해주자
			apEditorUtil.SetDirty(_editor);
		}




		//추가 20.4.6 : PortraitReadyToEdit의 비동기 처리를 위한 코드들
		public void PortraitReadyToEdit_AsyncStep()
		{
			string thumnailPath = Editor._portrait._imageFilePath_Thumbnail;
			if (string.IsNullOrEmpty(thumnailPath))
			{
				Editor._portrait._thumbnailImage = null;
			}
			else
			{
				Texture2D thumnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumnailPath);
				Editor._portrait._thumbnailImage = thumnailImage;
			}

			RefreshMeshGroups();

			//Selection.activeGameObject = Editor.Select.Portrait.gameObject;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			//이름을 갱신하자
			CheckAndRefreshGameObjectNames(Editor._portrait);

			//추가 : 삭제되어야 하는 데이터가 있을 수도 있다. 검색하자
			//CheckAndRemoveUnusedModifierData(Editor._portrait, true, true);//<<보류

			//SetDirty를 해주자
			apEditorUtil.SetDirty(_editor);
		}

	}
}