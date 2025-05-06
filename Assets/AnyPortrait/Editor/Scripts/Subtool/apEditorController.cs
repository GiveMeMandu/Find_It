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
		// Member
		//--------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }

		

		// Init
		//--------------------------------------------------
		public apEditorController()
		{

		}

		public void SetEditor(apEditor editor)
		{
			_editor = editor;

		}


		// Functions
		//-----------------------------------------------------------------		
		
		// 3-1. 객체 참조
		//--------------------------------------------------
		//삭제 v1.5.0
		//public apTextureData GetTextureData(int uniqueID)
		//{
		//	if (Editor._portrait == null)
		//	{
		//		return null;
		//	}

		//	return Editor._portrait._textureData.Find(delegate (apTextureData a)
		//	{
		//		return a._uniqueID == uniqueID;
		//	});
		//}

		public apMesh GetMesh(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			//이전 (GC 발생)
			//return Editor._portrait._meshes.Find(delegate (apMesh a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_FindMesh_ID = uniqueID;
			return Editor._portrait._meshes.Find(s_FindMeshByID_Func);
		}

		private static int s_FindMesh_ID = -1;
		private static Predicate<apMesh> s_FindMeshByID_Func = FUNC_FindMeshByID;
		private static bool FUNC_FindMeshByID(apMesh a)
		{
			return a._uniqueID == s_FindMesh_ID;
		}


		public apMeshGroup GetMeshGroup(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			//이전 (GC 발생)
			//return Editor._portrait._meshGroups.Find(delegate (apMeshGroup a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_FindMeshGroup_ID = uniqueID;
			return Editor._portrait._meshGroups.Find(s_FindMeshGroupByID_Func);
		}

		private static int s_FindMeshGroup_ID = -1;
		private static Predicate<apMeshGroup> s_FindMeshGroupByID_Func = FUNC_FindMeshGroupByID;
		private static bool FUNC_FindMeshGroupByID(apMeshGroup a)
		{
			return a._uniqueID == s_FindMeshGroup_ID;
		}

		//삭제 v1.5.0
		//public apControlParam GetControlParam(string controlKeyName)
		//{
		//	if (Editor._portrait == null)
		//	{
		//		return null;
		//	}
		//	return Editor._portrait._controller.FindParam(controlKeyName);
		//}




		//--------------------------------------------------
		// 3. 객체의 추가 / 삭제
		//--------------------------------------------------
		/// <summary>
		/// Mesh와 MeshGroup은 Monobehaviour로 저장해야한다.
		/// 해당 GameObject가 포함될 Group이 있어야 Monobehaviour를 추가할 수 있다.
		/// 존재하면 추가하지 않는다.
		/// 모든 AddMesh/AddMeshGroup 함수 전에 호출한다.
		/// </summary>
		public void CheckAndMakeObjectGroup()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			apPortrait portrait = Editor._portrait;

			if (portrait._subObjectGroup == null)
			{
				portrait._subObjectGroup = new GameObject("EditorObjects");
				portrait._subObjectGroup.transform.parent = portrait.transform;
				portrait._subObjectGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_Mesh == null)
			{
				portrait._subObjectGroup_Mesh = new GameObject("Meshes");
				portrait._subObjectGroup_Mesh.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_Mesh.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_Mesh.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_Mesh.transform.localScale = Vector3.one;
				portrait._subObjectGroup_Mesh.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_MeshGroup == null)
			{
				portrait._subObjectGroup_MeshGroup = new GameObject("MeshGroups");
				portrait._subObjectGroup_MeshGroup.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_MeshGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_MeshGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_MeshGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_Modifier == null)
			{
				portrait._subObjectGroup_Modifier = new GameObject("Modifiers");
				portrait._subObjectGroup_Modifier.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_Modifier.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_Modifier.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_Modifier.transform.localScale = Vector3.one;
				portrait._subObjectGroup_Modifier.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			//임시로 HideFlag를 풀자
			//portrait._subObjectGroup.hideFlags = HideFlags.None;
			//portrait._subObjectGroup_Mesh.hideFlags = HideFlags.None;
			//portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.None;
			//portrait._subObjectGroup_Modifier.hideFlags = HideFlags.None;

			//다시 잠그자
			portrait._subObjectGroup.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			portrait._subObjectGroup_Mesh.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			portrait._subObjectGroup_Modifier.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
		}


		/// <summary>
		/// 오브젝트를 삭제하는 다이얼로그에 사용될 메시지를 만든다.
		/// </summary>
		public string GetRemoveItemMessage(apPortrait portrait, object removingItem, int nMaximumLines, string baseMsg, string warningMsg)
		{
			List<object> removingItems = new List<object>();
			removingItems.Add(removingItem);

			List<string> logs = Editor.Controller.GetChangedItemsWhenRemoving(portrait, removingItems, nMaximumLines + 2);
			int nLogs = logs != null ? logs.Count : 0;

			//if (!string.IsNullOrEmpty(strChangedItems))
			if(nLogs > 0)
			{
				string strChangedItems = "";

				for (int i = 0; i < nLogs; i++)
				{
					strChangedItems += logs[i];
					if(i < nMaximumLines - 1)
					{
						strChangedItems += "\n";
					}
					else
					{
						//끝. 만약 마지막 로그가 아니라면 "..."추가
						if(i < nLogs - 1)
						{
							strChangedItems += "...";
						}
						break;
					}
				}

				return baseMsg + "\n"
					+ warningMsg + "\n"
					+ strChangedItems;
			}
			else
			{
				return baseMsg;
			}
		}


		/// <summary>
		/// 오브젝트를 삭제하는 다이얼로그에 사용될 메시지를 만든다. (여러개)
		/// </summary>
		public string GetRemoveItemsMessage(apPortrait portrait, List<object> removingItems, int nMaximumLines, string baseMsg, string warningMsg)
		{
			List<string> logs = Editor.Controller.GetChangedItemsWhenRemoving(portrait, removingItems, nMaximumLines + 2);//2개 정도 더 로그를 모은다.
			int nLogs = logs != null ? logs.Count : 0;

			//if (!string.IsNullOrEmpty(strChangedItems))
			if(nLogs > 0)
			{
				string strChangedItems = "";

				for (int i = 0; i < nLogs; i++)
				{
					strChangedItems += logs[i];
					if(i < nMaximumLines - 1)
					{
						strChangedItems += "\n";
					}
					else
					{
						//끝. 만약 마지막 로그가 아니라면 "..."추가
						if(i < nLogs - 1)
						{
							strChangedItems += "...";
						}
						break;
					}
				}

				return baseMsg + "\n"
					+ warningMsg + "\n"
					+ strChangedItems;
			}
			else
			{
				return baseMsg;
			}
		}

		/// <summary>
		/// 오브젝트를 삭제하는 다이얼로그에 사용될 메시지를 만든다.
		/// (TF 리스트 2개를 받아서 합쳐서 출력하는 버전의 함수)
		/// </summary>
		/// <param name="portrait"></param>
		/// <param name="removingItems"></param>
		/// <param name="nMaximumLines"></param>
		/// <param name="baseMsg"></param>
		/// <param name="warningMsg"></param>
		/// <returns></returns>
		public string GetRemoveItemsMessage(apPortrait portrait, 
											List<apTransform_Mesh> removingItems_A, 
											List<apTransform_MeshGroup> removingItems_B,
											int nMaximumLines, string baseMsg, string warningMsg)
		{
			List<object> totalList = new List<object>();
			int nListA = removingItems_A != null ? removingItems_A.Count : 0;
			int nListB = removingItems_B != null ? removingItems_B.Count : 0;

			object curObj = null;
			if(nListA > 0)
			{	
				for (int i = 0; i < nListA; i++)
				{
					curObj = removingItems_A[i];
					if(curObj != null)
					{
						totalList.Add(curObj);
					}
				}
			}
			if(nListB > 0)
			{
				for (int i = 0; i < nListB; i++)
				{
					curObj = removingItems_B[i];
					if(curObj != null)
					{
						totalList.Add(curObj);
					}
				}
			}

			return GetRemoveItemsMessage(portrait, totalList, nMaximumLines, baseMsg, warningMsg);
		}



		public string GetRemoveItemsMessage(apPortrait portrait, List<apBone> removingBones, int nMaximumLines, string baseMsg, string warningMsg)
		{
			List<object> totalList = new List<object>();

			int nBones = removingBones != null ? removingBones.Count : 0;
			
			if(nBones > 0)
			{
				object curObj = null;
				for (int i = 0; i < nBones; i++)
				{
					curObj = removingBones[i];
					totalList.Add(curObj);
				}
			}
			return GetRemoveItemsMessage(portrait, totalList, nMaximumLines, baseMsg, warningMsg);
		}




		/// <summary>
		/// 어떤 항목을 삭제할 때, 이와 연관이 있는 항목들을 모두 열거하는 함수
		/// 지원하는 것은 TextureData, Mesh, MeshGroup, MeshTrasnform, MeshGroupTransform, Modifier
		/// 체크 순서는 TextureData -> Mesh -> MeshGroup -> Transform -> Modifier -> AnimClip 순이다.
		/// 연관된게 없으면 null 리턴
		/// </summary>
		/// <param name="removingItem"></param>
		/// <returns></returns>
		private List<string> GetChangedItemsWhenRemoving(apPortrait portrait, List<object> removingItems, int nMaximumLines)
		{
			int nRemoveItems = removingItems != null ? removingItems.Count : 0;
			if (portrait == null || nRemoveItems == 0)
			{
				return null;
			}

			//삭제 확인 -> 
			//Texture -> Mesh -> 연결된 MeshGroup들까지
			//Mesh -> (모든 메시 그룹) + MeshTransform
			//MeshGroup -> 1. (다른 모든 메시 그룹) + MeshGroupTransform / 2. MeshGroup이 연동된 AnimClip
			//MeshTransform / MeshGroupTransform -> 1. (직접 삭제되는 경우) 메시 그룹 / 2. 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
			//Bone -> 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
			//Modifier -> 연결된 Timeline을 가진 AnimClip
			//Control Param -> 1. Control Param과 연결된 모든 MeshGroup을 찾는다. / 2. Control Param 타입의 애니메이션 클립

			List<object> resultObjs = new List<object>();

			object removingItem = null;
			for (int i = 0; i < nRemoveItems; i++)
			{
				removingItem = removingItems[i];
				if (removingItem == null)
				{
					continue;
				}

				bool isEnoughLog = false;

				if (removingItem is apTextureData)
				{
					apTextureData removedTextureData = removingItem as apTextureData;
					if (removedTextureData != null)
					{
						//1. Mesh 찾기 -> 2. 그 Mesh가 연결된 MeshGroup 찾기
						List<apMesh> rel_Meshes = new List<apMesh>();
						int logs = GetChangedMeshesByTextureData(portrait, removedTextureData, rel_Meshes, resultObjs, nMaximumLines);

						//변경 로그가 충분히 수집되었다면 중단하고 나가자
						if(resultObjs.Count >= nMaximumLines)
						{
							isEnoughLog = true;
							break;
						}

						if (logs > 0)
						{
							for (int iMesh = 0; iMesh < rel_Meshes.Count; iMesh++)
							{
								GetChangedMeshGroupsByMesh(portrait, rel_Meshes[iMesh], null, resultObjs, nMaximumLines);


								//변경 로그가 충분히 수집되었다면 중단하고 나가자
								if(resultObjs.Count >= nMaximumLines)
								{
									isEnoughLog = true;
									break;
								}
							}
						}
					}
				}
				else if (removingItem is apMesh)
				{
					apMesh removedMesh = removingItem as apMesh;

					if (removedMesh != null)
					{
						//1. Mesh가 연결된 MeshGroup + TF 찾기
						GetChangedMeshGroupsByMesh(portrait, removedMesh, null, resultObjs, nMaximumLines);

						//변경 로그가 충분히 수집되었다면 중단하고 나가자
						if(resultObjs.Count >= nMaximumLines)
						{
							isEnoughLog = true;
							break;
						}
					}
				}
				else if (removingItem is apMeshGroup)
				{
					apMeshGroup removedMeshGroup = removingItem as apMeshGroup;

					if (removedMeshGroup != null)
					{
						//MeshGroup -> 1. (다른 모든 메시 그룹) + MeshGroupTransform / 2. MeshGroup이 연동된 AnimClip
						GetChangedMeshGroupsByMeshGroup(portrait, removedMeshGroup, null, resultObjs, nMaximumLines);

						//변경 로그가 충분히 수집되었다면 중단하고 나가자
						if(resultObjs.Count >= nMaximumLines)
						{
							isEnoughLog = true;
							break;
						}


						GetChangedAnimClipsByMeshGroup(portrait, removedMeshGroup, null, resultObjs, nMaximumLines);

						//변경 로그가 충분히 수집되었다면 중단하고 나가자
						if(resultObjs.Count >= nMaximumLines)
						{
							isEnoughLog = true;
							break;
						}
					}
				}
				else if (removingItem is apTransform_Mesh)
				{
					apTransform_Mesh removedMeshTransform = removingItem as apTransform_Mesh;

					//MeshTransform / MeshGroupTransform -> 1. (직접 삭제되는 경우) 메시 그룹 / 2. 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
					if (removedMeshTransform != null)
					{
						apMeshGroup parentMeshGroup = portrait._meshGroups.Find(delegate (apMeshGroup a)
						{
							return a.GetMeshTransform(removedMeshTransform._transformUniqueID) != null;
						});

						if (parentMeshGroup != null)
						{
							//resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);
							if(!resultObjs.Contains(parentMeshGroup))
							{
								resultObjs.Add(parentMeshGroup);

								//변경 로그가 충분히 수집되었다면 중단하고 나가자
								if(resultObjs.Count >= nMaximumLines)
								{
									isEnoughLog = true;
									break;
								}
							}

							//연결된 Modifier를 찾자
							GetChangedModifiersBySubObject(portrait, parentMeshGroup,
														removedMeshTransform,
														null,
														null,
														null,
														null,
														resultObjs, nMaximumLines);


							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}

							//연결된 AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
														removedMeshTransform,
														null,
														null,
														null,
														null,
														null,
														resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}
						}
					}
				}
				else if (removingItem is apTransform_MeshGroup)
				{
					apTransform_MeshGroup removedMeshGroupTransform = removingItem as apTransform_MeshGroup;

					//MeshTransform / MeshGroupTransform -> 1. (직접 삭제되는 경우) 메시 그룹 / 2. 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
					if (removedMeshGroupTransform != null)
					{
						apMeshGroup parentMeshGroup = portrait._meshGroups.Find(delegate (apMeshGroup a)
						{
							return a.GetMeshTransform(removedMeshGroupTransform._transformUniqueID) != null;
						});

						if (parentMeshGroup != null)
						{
							//resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);
							if(!resultObjs.Contains(parentMeshGroup))
							{
								resultObjs.Add(parentMeshGroup);

								//변경 로그가 충분히 수집되었다면 중단하고 나가자
								if(resultObjs.Count >= nMaximumLines)
								{
									isEnoughLog = true;
									break;
								}
							}


							//연결된 Modifier를 찾자
							GetChangedModifiersBySubObject(portrait, parentMeshGroup,
														null,
														removedMeshGroupTransform,
														null,
														null,
														null,
														resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}

							//연결된 AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
														null,
														removedMeshGroupTransform,
														null,
														null,
														null,
														null,
														resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}
						}
					}
				}
				else if (removingItem is apBone)
				{
					apBone removedBone = removingItem as apBone;

					//Bone -> 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
					if (removedBone != null)
					{
						if (removedBone._meshGroup != null)
						{
							apMeshGroup parentMeshGroup = removedBone._meshGroup;
							
							//resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);
							if(!resultObjs.Contains(parentMeshGroup))
							{
								resultObjs.Add(parentMeshGroup);

								//변경 로그가 충분히 수집되었다면 중단하고 나가자
								if(resultObjs.Count >= nMaximumLines)
								{
									isEnoughLog = true;
									break;
								}
							}

							//연결된 Modifier를 찾자
							GetChangedModifiersBySubObject(portrait, parentMeshGroup,
														null,
														null,
														removedBone,
														null,
														null, resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}

							//연결된 AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
														null,
														null,
														removedBone,
														null,
														null,
														null, resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}
						}
					}
				}
				else if (removingItem is apModifierBase)
				{
					apModifierBase removedModifier = removingItem as apModifierBase;

					//Modifier -> 연결된 Timeline을 가진 AnimClip
					if (removedModifier != null)
					{
						if (removedModifier._meshGroup != null)
						{
							apMeshGroup parentMeshGroup = removedModifier._meshGroup;

							//resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);
							if(!resultObjs.Contains(parentMeshGroup))
							{
								resultObjs.Add(parentMeshGroup);

								//변경 로그가 충분히 수집되었다면 중단하고 나가자
								if(resultObjs.Count >= nMaximumLines)
								{
									isEnoughLog = true;
									break;
								}
							}


							//연결된 AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
															null,
															null,
															null,
															removedModifier,
															null,
															null, resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}
						}
					}
				}
				else if (removingItem is apControlParam)
				{
					apControlParam removedControlParam = removingItem as apControlParam;

					//Control Param -> 1. Control Param과 연결된 모든 MeshGroup을 찾는다. / 2. Control Param 타입의 애니메이션 클립
					if (removedControlParam != null)
					{
						for (int iMeshGroup = 0; iMeshGroup < portrait._meshGroups.Count; iMeshGroup++)
						{
							apMeshGroup meshGroup = portrait._meshGroups[iMeshGroup];

							GetChangedModifiersBySubObject(portrait, meshGroup,
															null,
															null,
															null,
															removedControlParam,
															null, resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}
						}

						for (int iMeshGroup = 0; iMeshGroup < portrait._meshGroups.Count; iMeshGroup++)
						{
							apMeshGroup meshGroup = portrait._meshGroups[iMeshGroup];

							//AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, meshGroup,
														null,
														null,
														null,
														null,
														removedControlParam,
														null, resultObjs, nMaximumLines);

							//변경 로그가 충분히 수집되었다면 중단하고 나가자
							if(resultObjs.Count >= nMaximumLines)
							{
								isEnoughLog = true;
								break;
							}
						}
					}

				}

				//변경 로그가 충분히 수집되었다면 다른 객체를 확인 안한다.
				if(isEnoughLog)
				{
					break;
				}
				
			}


			int nResultObj = resultObjs.Count;
			List<string> resultLogs = new List<string>();

			if(nResultObj > 0)
			{
				object curObj = null;
				for (int i = 0; i < nResultObj; i++)
				{
					curObj = resultObjs[i];
					if(curObj == null)
					{
						continue;
					}
					if(curObj is apMesh) { resultLogs.Add("[Mesh] " + (curObj as apMesh)._name); }
					else if(curObj is apMeshGroup) { resultLogs.Add("[Mesh Group] " + (curObj as apMeshGroup)._name); }
					else if(curObj is apAnimClip) { resultLogs.Add("[Animation Clip] " + (curObj as apAnimClip)._name); }
					else if(curObj is apModifierBase) { resultLogs.Add("[Modifier] " + (curObj as apModifierBase).DisplayName); }
				}
			}


			#region [미사용 코드] String로 가공해서 리턴하는 경우의 코드
			////로그를 정리하자
			////최대 개수를 넘을 때 : 최대 개수 -1 + ...
			////최대 개수와 같을 때 : 최대 개수
			//if (resultLogs.Count > nMaximumLines)
			//{
			//	for (int iLog = 0; iLog < resultLogs.Count; iLog++)
			//	{
			//		strResult += resultLogs[iLog] + "\n";
			//		if (iLog >= nMaximumLines - 2)
			//		{
			//			break;
			//		}
			//	}
			//	strResult += "...";
			//}
			//else
			//{
			//	for (int iLog = 0; iLog < resultLogs.Count; iLog++)
			//	{
			//		strResult += resultLogs[iLog];
			//		if (iLog < resultLogs.Count - 1)
			//		{
			//			strResult += "\n";
			//		}
			//	}
			//}

			//return strResult; 
			#endregion

			//변경 v1.4.2 : 그냥 리스트 리턴
			return resultLogs;

		}

		/// <summary>TextureData를 포함하는 Mesh를 찾는다.</summary>
		private int GetChangedMeshesByTextureData(	apPortrait portrait,
													apTextureData textureData,
													List<apMesh> resultMeshes,
													//List<string> resultLogs,
													List<object> resultObjs,
													int maxLogs)
		{
			if (portrait == null || textureData == null)
			{
				return 0;
			}
			int nResult = 0;

			int nMeshes = portrait._meshes != null ? portrait._meshes.Count : 0;
			if(nMeshes == 0)
			{
				return 0;
			}

			apMesh curMesh = null;
			for (int i = 0; i < nMeshes; i++)
			{
				curMesh = portrait._meshes[i];
				if (curMesh.LinkedTextureDataID == textureData._uniqueID)
				{
					if(resultObjs.Contains(curMesh))
					{
						//로그 중복 체크
						continue;
					}

					resultMeshes.Add(curMesh);
					
					//resultLogs.Add("[Mesh] " + portrait._meshes[i]._name);
					resultObjs.Add(curMesh);
					
					nResult++;

					if(nResult >= maxLogs)
					{
						return nResult;
					}
				}
			}
			return nResult;
		}

		/// <summary>Mesh를 포함하는 MeshGroup들을 찾는다. (MeshTransform을 포함)</summary>
		private int GetChangedMeshGroupsByMesh(	apPortrait portrait,
												apMesh mesh,
												List<apTransform_Mesh> resultMeshTransforms,
												//List<string> resultLogs,
												List<object> resultObjs,
												int maxLogs)
		{
			if (portrait == null || mesh == null)
			{
				return 0;
			}

			int nMeshGroups = portrait._meshGroups != null ? portrait._meshGroups.Count : 0;
			if(nMeshGroups == 0)
			{
				return 0;
			}

			int nResult = 0;

			bool isRecordToMeshTFs = resultMeshTransforms != null;

			for (int iMG = 0; iMG < nMeshGroups; iMG++)
			{
				apMeshGroup meshGroup = portrait._meshGroups[iMG];

				//MeshTF를 추가로 기록하는게 아니라면, 한번 기록된 MeshGroup은 더이상 체크하지 않는다.
				if (!isRecordToMeshTFs)
				{
					if (resultObjs.Contains(meshGroup))
					{
						//로그 중복 체크
						continue;
					}
				}
				

				int nChildMeshTFs = meshGroup._childMeshTransforms != null ? meshGroup._childMeshTransforms.Count : 0;

				for (int iMeshTransform = 0; iMeshTransform < nChildMeshTFs; iMeshTransform++)
				{
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iMeshTransform];

					if (meshTransform._meshUniqueID == mesh._uniqueID)
					{
						if (resultMeshTransforms != null)
						{
							if(!resultMeshTransforms.Contains(meshTransform))
							{
								resultMeshTransforms.Add(meshTransform);
							}
						}

						//resultLogs.Add("[MeshGroup] " + meshGroup._name + " - " + meshTransform._nickName);
						
						//중복이 되지 않는 경우에만 추가
						if (!resultObjs.Contains(meshGroup))
						{
							resultObjs.Add(meshGroup);
							nResult++;

							if (nResult >= maxLogs)
							{
								return nResult;
							}

							if (!isRecordToMeshTFs)
							{
								//Mesh TF에 기록하지 않는다면, 이 메시 그룹의 다른 MeshTF는 체크할 필요가 없다.
								continue;
							}
						}
					}
				}
			}
			return nResult;
		}

		/// <summary>MeshGroup를 포함하는 다른 MeshGroup들을 찾는다. (MeshGroupTransform을 포함)</summary>
		private int GetChangedMeshGroupsByMeshGroup(	apPortrait portrait,
														apMeshGroup meshGroup,
														List<apTransform_MeshGroup> resultMeshGroupTransforms,
														//List<string> resultLogs,
														List<object> resultObjs,
														int maxLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}

			int nMeshGroups = portrait._meshGroups != null ? portrait._meshGroups.Count : 0;
			if(nMeshGroups == 0)
			{
				return 0;
			}

			bool isRecordToMeshGroupTFs = resultMeshGroupTransforms != null;

			int nResult = 0;
			for (int iMG = 0; iMG < portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup otherMeshGroup = portrait._meshGroups[iMG];
				if (otherMeshGroup == meshGroup)
				{
					continue;
				}

				//MeshGroup Transform에 기록하지 않을 때, 이미 추가된 메시 그룹은 더 체크하지 않는다.
				if(!isRecordToMeshGroupTFs)
				{
					if(resultObjs.Contains(otherMeshGroup))
					{
						continue;
					}
				}

				int nChildMeshGroupTFs = otherMeshGroup._childMeshGroupTransforms != null ? otherMeshGroup._childMeshGroupTransforms.Count : 0;
				if(nChildMeshGroupTFs == 0)
				{
					continue;
				}


				for (int iMeshTransform = 0; iMeshTransform < nChildMeshGroupTFs; iMeshTransform++)
				{
					apTransform_MeshGroup meshGroupTransform = otherMeshGroup._childMeshGroupTransforms[iMeshTransform];


					if (meshGroupTransform._meshGroupUniqueID == meshGroup._uniqueID)
					{
						if (resultMeshGroupTransforms != null)
						{
							if(!resultMeshGroupTransforms.Contains(meshGroupTransform))
							{
								resultMeshGroupTransforms.Add(meshGroupTransform);
							}
						}
						
						//resultLogs.Add("[MeshGroup] " + otherMeshGroup._name + " - " + meshGroupTransform._nickName);
						if(!resultObjs.Contains(otherMeshGroup))
						{
							resultObjs.Add(otherMeshGroup);
							nResult++;

							if(nResult >= maxLogs)
							{
								return nResult;
							}

							if (!isRecordToMeshGroupTFs)
							{
								//MeshGroup TF에 기록하지 않는다면, 이 메시 그룹의 다른 MeshGroupTF는 체크할 필요가 없다.
								continue;
							}
						}
						
					}
				}
			}
			return nResult;
		}




		/// <summary>MeshGroup를 포함하는 AnimClip을 찾는다.</summary>
		private int GetChangedAnimClipsByMeshGroup(	apPortrait portrait,
													apMeshGroup meshGroup,
													List<apAnimClip> resultAnimClip,
													//List<string> resultLogs,
													List<object> resultObjs,
													int maxLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}


			int nAnimClips = portrait._animClips != null ? portrait._animClips.Count : 0;
			if(nAnimClips == 0)
			{
				return 0;
			}

			int nResult = 0;
			for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
			{
				apAnimClip animClip = portrait._animClips[iAnimClip];

				if (animClip._targetMeshGroupID == meshGroup._uniqueID)
				{
					if (resultAnimClip != null)
					{
						if(!resultAnimClip.Contains(animClip))
						{
							resultAnimClip.Add(animClip);
						}
					}

					if (!resultObjs.Contains(animClip))
					{
						//resultLogs.Add("[Animation Clip] " + animClip._name);
						resultObjs.Add(animClip);

						nResult++;

						if (nResult > maxLogs)
						{
							return nResult;
						}
					}
				}
			}
			return nResult;
		}

		/// <summary>MeshGroup를 포함하며 Transform이나 Bone이나 ControlParam 또는 Modifier를 가지고있는 AnimClip을 찾는다.</summary>
		private int GetChangedAnimClipsBySubObject(apPortrait portrait, apMeshGroup meshGroup,
													apTransform_Mesh target_MeshTransform,
													apTransform_MeshGroup target_MeshGroupTransform,
													apBone target_Bone,
													apModifierBase target_Modifier,
													apControlParam target_ControlParam,
													List<apAnimClip> resultAnimClip,
													//List<string> resultLogs,
													List<object> resultObjs,
													int maxLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}

			int nAnimClips = portrait._animClips != null ? portrait._animClips.Count : 0;
			if(nAnimClips == 0)
			{
				return 0;
			}

			int nResult = 0;
			for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
			{
				apAnimClip animClip = portrait._animClips[iAnimClip];

				if (animClip._targetMeshGroupID != meshGroup._uniqueID)
				{
					continue;
				}

				//이미 포함되어 있다면
				if(resultObjs.Contains(animClip))
				{
					continue;
				}

				bool isChangedAnimClip = false;

				//조건이 맞는 Timeline이나 TimelineLayer중 하나라도 있다면 AnimClip은 연관성이 있다.
				for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
				{
					apAnimTimeline timeline = animClip._timelines[iTimeline];
					if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						//Modifier 꼬는 Transform/Bone을 찾고자 할 때
						if (target_Modifier != null)
						{
							//Modifier가 연결되는가
							if (target_Modifier == timeline._linkedModifier)
							{
								isChangedAnimClip = true;
								break;
							}
						}
						else
						{
							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								apAnimTimelineLayer timelineLayer = timeline._layers[iLayer];
								if (target_MeshTransform != null)
								{
									//MeshTransform을 체크하자
									if (target_MeshTransform._transformUniqueID == timelineLayer._transformID)
									{
										isChangedAnimClip = true;
										break;
									}
								}
								else if (target_MeshGroupTransform != null)
								{
									//MeshGroupTransform을 체크하자
									if (target_MeshGroupTransform._transformUniqueID == timelineLayer._transformID)
									{
										isChangedAnimClip = true;
										break;
									}
								}
								else if (target_Bone != null)
								{
									//Bone을 체크하자
									if (target_Bone._uniqueID == timelineLayer._boneID)
									{
										isChangedAnimClip = true;
										break;
									}
								}
							}
						}
					}
					else if (timeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						//ControlParam을 찾고자 할 때
						if (target_ControlParam != null)
						{
							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								apAnimTimelineLayer timelineLayer = timeline._layers[iLayer];
								if (timelineLayer._controlParamID == target_ControlParam._uniqueID)
								{
									//Control Param에 해당한다.
									isChangedAnimClip = true;
									break;
								}
							}
						}
					}

					if (isChangedAnimClip)
					{
						break;
					}
				}

				if (isChangedAnimClip)
				{
					if (resultAnimClip != null)
					{
						if(!resultAnimClip.Contains(animClip))
						{
							resultAnimClip.Add(animClip);
						}
					}
					//resultLogs.Add("[Animation Clip] " + animClip._name);
					if (!resultObjs.Contains(animClip))
					{
						resultObjs.Add(animClip);

						nResult++;

						if (nResult >= maxLogs)
						{
							return nResult;
						}
					}
				}

			}
			return nResult;
		}

		/// <summary>MeshGroup를 포함하며 Transform이나 Bone이나 ControlParam 또는 Modifier를 가지고있는 AnimClip을 찾는다.</summary>
		private int GetChangedModifiersBySubObject(apPortrait portrait, apMeshGroup meshGroup,
													apTransform_Mesh target_MeshTransform,
													apTransform_MeshGroup target_MeshGroupTransform,
													apBone target_Bone,
													apControlParam target_ControlParam,
													List<apModifierBase> resultModifier,
													//List<string> resultLogs,
													List<object> resultObjs,
													int maxLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}

			if(meshGroup._modifierStack == null)
			{
				return 0;
			}
			int nModifiers = meshGroup._modifierStack._modifiers != null ? meshGroup._modifierStack._modifiers.Count : 0;
			if(nModifiers == 0)
			{
				return 0;
			}

			
			int nResult = 0;
			for (int iMod = 0; iMod < nModifiers; iMod++)
			{
				apModifierBase modifier = meshGroup._modifierStack._modifiers[iMod];


				if(resultObjs.Contains(modifier))
				{
					//이미 기록되었다.
					continue;
				}


				bool isChangedModifier = false;

				if (modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller)
				{
					if (target_ControlParam != null)
					{
						//Control Param에 연결되는지 체크
						for (int iPSG = 0; iPSG < modifier._paramSetGroup_controller.Count; iPSG++)
						{
							apModifierParamSetGroup paramSetGroup = modifier._paramSetGroup_controller[iPSG];
							if (paramSetGroup._keyControlParamID == target_ControlParam._uniqueID)
							{
								//ControlParam이 포함된다.
								isChangedModifier = true;
								break;
							}
						}
					}
				}
				else
				{
					//Transform, Bone에 연결되는지 체크
					int nPSGs = modifier._paramSetGroup_controller != null ? modifier._paramSetGroup_controller.Count : 0;
					if (nPSGs > 0)
					{
						for (int iPSG = 0; iPSG < nPSGs; iPSG++)
						{
							apModifierParamSetGroup paramSetGroup = modifier._paramSetGroup_controller[iPSG];
							if (target_MeshTransform != null)
							{
								if (paramSetGroup._syncTransform_Mesh.Contains(target_MeshTransform))
								{
									//MeshTransform이 포함된다.
									isChangedModifier = true;
									break;
								}
							}
							else if (target_MeshGroupTransform != null)
							{
								if (paramSetGroup._syncTransform_MeshGroup.Contains(target_MeshGroupTransform))
								{
									//MeshGroupTransform이 포함된다.
									isChangedModifier = true;
									break;
								}
							}
							else if (target_Bone != null)
							{
								if (paramSetGroup._syncBone.Contains(target_Bone))
								{
									//Bone이 포함된다.
									isChangedModifier = true;
									break;
								}
							}
						}
					}
					
				}

				if (isChangedModifier)
				{
					if (resultModifier != null)
					{
						if(!resultModifier.Contains(modifier))
						{
							resultModifier.Add(modifier);
						}
					}
					//if (target_ControlParam != null)
					//{
					//	//ControlParam인 경우 MeshGroup + Modifier의 이름으로 알려주자
					//	resultLogs.Add("[MeshGroup] " + meshGroup.name + " - " + modifier.DisplayName);
					//}
					//else
					//{
					//	resultLogs.Add("[Modifier] " + modifier.DisplayName);
					//}

					if (!resultObjs.Contains(modifier))
					{
						resultObjs.Add(modifier);
						
						nResult++;

						if (nResult >= maxLogs)
						{
							return nResult;
						}
					}

					
				}

			}

			return nResult;
		}


		//-------------------------------------------------------------------------------


		//이미지를 삭제한다.
		public void RemoveTexture(apTextureData textureData)
		{
			//Undo - Remove Image
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_RemoveImage, Editor, Editor._portrait, textureData, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Image");


			if (textureData == Editor.Select.TextureData)
			{
				Editor.Select.SelectNone();//Select된 이미지라면 None으로 바꾸자
			}

			int removedUniqueID = textureData._uniqueID;

			//Editor._portrait.PushUniqueID_Texture(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Texture, removedUniqueID);


			Editor._portrait._textureData.Remove(textureData);
			//Editor._portrait.SortTextureData();


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//Debug.Log("Remove Texture");
			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

			Editor.Hierarchy.SetNeedReset();
			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);
		}


		/// <summary>여러개의 이미지들을 삭제한다 (21.10.10)</summary>
		public void RemoveTextures(List<apTextureData> textureDataList)
		{
			if(textureDataList == null || textureDataList.Count == 0)
			{
				return;
			}

			//Undo - Remove Image
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_RemoveImage, Editor, Editor._portrait, textureData, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Images");

			if (Editor.Select.TextureData != null)
			{
				if (textureDataList.Contains(Editor.Select.TextureData))
				{
					Editor.Select.SelectNone();//Select된 이미지라면 None으로 바꾸자
				}
			}

			apTextureData curTextureData = null;
			for (int i = 0; i < textureDataList.Count; i++)
			{
				curTextureData = textureDataList[i];
				int removedUniqueID = curTextureData._uniqueID;

				Editor._portrait.PushUnusedID(apIDManager.TARGET.Texture, removedUniqueID);
				Editor._portrait._textureData.Remove(curTextureData);
			}
			
			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(true);
		}






		/// <summary>
		/// 이미지를 추가한다.
		/// </summary>
		public apTextureData AddImage()
		{
			//int nextID = Editor._portrait.MakeUniqueID_Texture();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Texture);
			if (nextID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Texture Add Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AddTextureFailed_Title),
												Editor.GetText(TEXT.AddTextureFailed_Body),
												Editor.GetText(TEXT.Close));

				return null;
			}

			//Undo - Add Image
			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Main_AddImage, 
												Editor, 
												Editor._portrait, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			//apTextureData newTexture = new apTextureData(Editor._portrait._textureData.Count);

			apTextureData newTexture = new apTextureData(nextID);

			Editor._portrait._textureData.Add(newTexture);
			Editor.Select.SelectImage(newTexture);//<<Selection에도 추가
											   //Editor._portrait.SortTextureData();

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newTexture;
		}




		/// <summary>
		/// 추가 21.9.11 : 이미지의 크기를 변경하고자 한다면 다른 메시의 버텍스 위치등을 변경해야한다.
		/// </summary>
		/// <param name="portrait"></param>
		/// <param name="textureData"></param>
		/// <param name="nextWidth"></param>
		/// <param name="nextHeight"></param>
		public void ResizeTextureData(apPortrait portrait, apTextureData textureData, int nextWidth, int nextHeight, bool isResizeTransforms)
		{
			if(portrait == null
				|| textureData == null
				|| textureData._image == null)
			{
				return;
			}

			//기존 데이터가 0이라면 비율 변경 불가
			if(textureData._width == 0 || textureData._height == 0)
			{
				return;
			}

			//너무 작다면 변경
			if(nextWidth < 4) { nextWidth = 4; }
			if(nextHeight < 4) { nextHeight = 4; }



			//기존 데이터와 동일하다면 변경이 필요없다.
			if(textureData._width == nextWidth && textureData._height == nextHeight)
			{
				return;
			}

			

			
			int nMeshes = portrait._meshes != null ? portrait._meshes.Count : 0;


			if(nMeshes == 0)
			{
				//크기 갱신할 메시가 없다면 텍스쳐의 값만 변경하고 끝
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Image_SettingChanged, Editor, Editor._portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);
				textureData._width = nextWidth;
				textureData._height = nextHeight;
				return;
			}

			//모든 메시의 버텍스들의 위치를 바꾸어야 한다.
			//메시들과 연결된 모든 메시 그룹들의 메시 트랜스폼도 변경해야한다.
			apEditorUtil.SetRecord_PortraitAndAllMeshesAndAllMeshGroups(apUndoGroupData.ACTION.Image_SettingChanged, Editor, apEditorUtil.UNDO_STRUCT.ValueOnly);

			float resizeRatio_X = (float)nextWidth / (float)textureData._width;
			float resizeRatio_Y = (float)nextHeight / (float)textureData._height;

			int nMeshGroups = portrait._meshGroups != null ? portrait._meshGroups.Count : 0;

			apMesh curMesh = null;
			apMeshGroup curMeshGroup = null;
			apTransform_Mesh curMeshTF = null;

			//작업된 MeshTF는 중복해서 하지 않도록 체크할 것
			List<apTransform_Mesh> processedMeshTFs = new List<apTransform_Mesh>();


			for (int iMesh = 0; iMesh < nMeshes; iMesh++)
			{
				curMesh = portrait._meshes[iMesh];
								
				if(curMesh._textureData_Linked != textureData)
				{
					//해당되는 메시가 아니다.
					continue;
				}

				//Debug.Log("> 변경되는 메시 : " + curMesh._name);

				//비율을 계산하자
				//Vector2 prevOffsetPos = curMesh._offsetPos;

				float nextOffsetPosX = curMesh._offsetPos.x * resizeRatio_X;
				float nextOffsetPosY = curMesh._offsetPos.y * resizeRatio_Y;


				//apMatrix3x3 prevOffsetMatrix = apMatrix3x3.TRS(new Vector2(-curMesh._offsetPos.x, -curMesh._offsetPos.y), 0, Vector2.one);
				//apMatrix3x3 nextOffsetMatrix = apMatrix3x3.TRS(new Vector2(-nextOffsetPosX, -nextOffsetPosY), 0, Vector2.one);
				//apMatrix3x3 prevDefaultMatrix = apMatrix3x3.identity;

				//이 메시와 연결된 메시 TF들을 모든 메시 그룹에서 찾자

				//-----------------------------
				// 관련된 메시 그룹의 MeshTF 찾아서 변경 크기를 변경 (옵션에 따라)

				if (isResizeTransforms && nMeshGroups > 0)
				{
					for (int iMeshGroup = 0; iMeshGroup < nMeshGroups; iMeshGroup++)
					{
						curMeshGroup = portrait._meshGroups[iMeshGroup];

						int nMeshTFs = curMeshGroup._childMeshTransforms != null ? curMeshGroup._childMeshTransforms.Count : 0;
						if (nMeshTFs == 0)
						{
							continue;
						}

						List<apTransform_Mesh> linkedMeshTFs = curMeshGroup._childMeshTransforms.FindAll(delegate (apTransform_Mesh a)
						{
							return a._mesh == curMesh;
						});

						int nLinkedMeshTF = linkedMeshTFs != null ? linkedMeshTFs.Count : 0;

						if (nLinkedMeshTF == 0)
						{
							continue;
						}

						for (int iLinkedMeshTF = 0; iLinkedMeshTF < nLinkedMeshTF; iLinkedMeshTF++)
						{
							curMeshTF = linkedMeshTFs[iLinkedMeshTF];
							if (processedMeshTFs.Contains(curMeshTF))
							{
								continue;
							}
							processedMeshTFs.Add(curMeshTF);


							#region [미사용 코드]
							////1. MeshTF의 기본 위치를 바꾼다.

							//prevDefaultMatrix.SetMatrix(ref curMeshTF._matrix._mtrxToSpace);


							////계산 순서
							////- 위치를 0으로 이동
							////- 스케일 변화
							////- 위치를 새로운 위치로 변화
							////apMatrix3x3 recoverPos = apMatrix3x3.TRS(-1.0f * curMeshTF._matrix._pos, 0.0f, Vector2.one);
							//apMatrix3x3 resize = apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(1.0f / resizeRatio_X, 1.0f / resizeRatio_Y));
							//apMatrix3x3 moveNewPos = apMatrix3x3.TRS(new Vector2(-nextOffsetPosX, -nextOffsetPosY) - (-prevOffsetPos), 0.0f, Vector2.one);

							////prevDefaultMatrix.Multiply(ref recoverPos);							
							//prevDefaultMatrix.Multiply(ref moveNewPos);
							////prevDefaultMatrix.Multiply(ref resize);

							//Vector2 newPos = prevDefaultMatrix.MultiplyPoint(Vector2.zero);
							////newPos.x /= resizeRatio_X;
							////newPos.y /= resizeRatio_Y;
							////curMeshTF._matrix.SetPos(newPos, true);
							////curMeshTF._matrix.SetPos(prevDefaultMatrix._m02, prevDefaultMatrix._m12, true);
							////curMeshTF._matrix.SetScale(new Vector2(curMeshTF._matrix._scale.x / resizeRatio_X, curMeshTF._matrix._scale.y / resizeRatio_Y), true);


							////float newPosX = prevDefaultMatrix._m00 * (prevOffsetMatrix._m02 - nextOffsetMatrix._m02)
							////				+ prevDefaultMatrix._m01 * (prevOffsetMatrix._m12 - nextOffsetMatrix._m12)
							////				+ prevDefaultMatrix._m02;
							////float newPosY = prevDefaultMatrix._m10 * (prevOffsetMatrix._m02 - nextOffsetMatrix._m02)
							////				+ prevDefaultMatrix._m11 * (prevOffsetMatrix._m12 - nextOffsetMatrix._m12)
							////				+ prevDefaultMatrix._m12;

							////float newPosX = prevDefaultMatrix._m00 * (prevOffsetMatrix._m02 - (nextOffsetMatrix._m02 / resizeRatio_X))
							////				+ prevDefaultMatrix._m01 * (prevOffsetMatrix._m12 - (nextOffsetMatrix._m12 / resizeRatio_Y))
							////				+ prevDefaultMatrix._m02;
							////float newPosY = prevDefaultMatrix._m10 * (prevOffsetMatrix._m02 - (nextOffsetMatrix._m02 / resizeRatio_X))
							////				+ prevDefaultMatrix._m11 * (prevOffsetMatrix._m12 - (nextOffsetMatrix._m12 / resizeRatio_Y))
							////				+ prevDefaultMatrix._m12;

							////curMeshTF._matrix.SetPos(newPosX * resizeRatio_X, newPosY * resizeRatio_Y, true);
							////curMeshTF._matrix.SetPos(newPosX, newPosY, true);

							////2. MeshTF의 크기를 바꾼다. (역으로 해야 항상성을 유지할 것)
							////단, 그냥 스케일을 바꾸면 원래 위치가 아니게 되므로, 원래 W 위치를 저장한 후 역으로 계산한다.
							////Vector2 prevPosW = curMeshTF._matrix.MulPoint2(Vector2.zero);
							////Vector2 prevPosW = new Vector2(newPosX, newPosY);
							////Vector2 prevPosMat = curMeshTF._matrix._pos; 
							#endregion

							Vector2 prevScale = curMeshTF._matrix._scale;
							Vector2 nextScale = new Vector2(prevScale.x / resizeRatio_X, prevScale.y / resizeRatio_Y);
							curMeshTF._matrix.SetScale(nextScale, true);
						}

						curMeshGroup.SetDirtyToReset();
						curMeshGroup.RefreshForce(true);
					}
				}
				//-----------------------------


				//메시의 정보들을 변경한다.
				//UV는 변경하지 않는다.
				curMesh._offsetPos.x = nextOffsetPosX;
				curMesh._offsetPos.y = nextOffsetPosY;

				//Area도 변경
				curMesh._atlasFromPSD_LT.x *= resizeRatio_X;
				curMesh._atlasFromPSD_LT.y *= resizeRatio_Y;

				curMesh._atlasFromPSD_RB.x *= resizeRatio_X;
				curMesh._atlasFromPSD_RB.y *= resizeRatio_Y;

				curMesh.MakeOffsetPosMatrix();

				int nVerts = curMesh._vertexData != null ? curMesh._vertexData.Count : 0;
				apVertex curVert = null;
				for (int iVert = 0; iVert < nVerts; iVert++)
				{
					curVert = curMesh._vertexData[iVert];
					curVert._pos.x *= resizeRatio_X;
					curVert._pos.y *= resizeRatio_Y;
				}
			}


			textureData._width = nextWidth;
			textureData._height = nextHeight;


			if (nMeshGroups > 0)
			{
				for (int iMeshGroup = 0; iMeshGroup < nMeshGroups; iMeshGroup++)
				{
					curMeshGroup = portrait._meshGroups[iMeshGroup];
					curMeshGroup.SetDirtyToReset();
					curMeshGroup.RefreshForce(true);
				}
			}
		}





		private object _psdDialogLoadKey = null;
		private object _psdReimportDialogLoadKey = null;

		/// <summary>
		/// PSD 툴을 호출하여 자동 생성기를 돌린다.
		/// </summary>
		public void ShowPSDLoadDialog()
		{
			//원래는 데모에서 불가 > 이거는 풀어줌
			//if (apVersion.I.IsDemo)
			//{
			//	//데모 버전에서는 PSD Load를 지원하지 않습니다.
			//	EditorUtility.DisplayDialog(
			//		Editor.GetText(TEXT.DemoLimitation_Title),
			//		Editor.GetText(TEXT.DemoLimitation_Body),
			//		Editor.GetText(TEXT.Okay)
			//		);
			//}

			_psdDialogLoadKey = apPSDDialog.ShowWindow(_editor, OnPSDImageLoad);
			apPSDReimportDialog.CloseDialog();//<<Reimport는 닫는다.
			_psdReimportDialogLoadKey = null;
		}


		public void OnPSDImageLoad(bool isSuccess, object loadKey,
									string fileName, string filePath,
									List<apPSDLayerData> layerDataList,
									//float atlasScaleRatio, float meshGroupScaleRatio,
									int atlasScaleRatioX100, int meshGroupScaleRatioX100,
									int totalWidth, int totalHeight, int padding,
									int bakedTextureWidth, int bakedTextureHeight,
									int bakeMaximumNumAtlas, bool bakeBlurOption,
									string bakeDstFilePath,
									string bakeDstFileRelativePath)
		{
			if (_psdDialogLoadKey != loadKey)
			{
				_psdDialogLoadKey = null;
				return;
			}

			_psdDialogLoadKey = null;

			if (Editor._portrait == null || !isSuccess) { return; }

			//추가 v1.4.2 : Undo 등록
			//int undoID = apEditorUtil.SetRecordBeforeCreateOrDestroyMultipleObjects(Editor._portrait, "Import PSD");
			apEditorUtil.SetRecordBeforeCreateOrDestroyMultipleObjects(Editor._portrait, "Import PSD", false);//변경

			//이 과정에서 생성되는 모든 오브젝트들을 일괄적으로 Undo에 넣자
			List<MonoBehaviour> createdMonoObjects = new List<MonoBehaviour>();

			//이제 만들어봅시다.

			float atlasScaleRatio = (float)atlasScaleRatioX100 * 0.01f;
			float meshGroupScaleRatio = (float)meshGroupScaleRatioX100 * 0.01f;

			Vector2 centerPosOffset = new Vector2((float)totalWidth * 0.5f * meshGroupScaleRatio, (float)totalHeight * 0.5f * meshGroupScaleRatio);

			//1. Image 로드 + TextureData 생성

			//일단 먼저 -> Image를 로드해야함
			//로드하고 TextureData에도 추가를 한 뒤, LayerData와 연동 맵을 만든다.
			Dictionary<string, Texture2D> savedAtlasPath = new Dictionary<string, Texture2D>();
			Dictionary<apPSDLayerData, apTextureData> layerTextureMapping = new Dictionary<apPSDLayerData, apTextureData>();

			List<apTextureData> addedTextureDataList = new List<apTextureData>();

			//레이어 데이터에 따라서 이미지를 가져와서 리스트에 저장한다.
			//리스트에 저장하면서 새로운 apTextureData를 생성한다.
			for (int i = 0; i < layerDataList.Count; i++)
			{
				if (!layerDataList[i]._isImageLayer || !layerDataList[i]._isBakable)
				{
					continue;
				}

				string assetPath = layerDataList[i]._textureAssetPath;
				Texture2D savedImage = null;

				//Debug.Log("<" + layerDataList[i]._name + "> Image Path [" + assetPath + "]");
				if (savedAtlasPath.ContainsKey(assetPath))
				{
					savedImage = savedAtlasPath[assetPath];
				}
				else
				{
					savedImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (savedImage == null)
					{
						Debug.LogError("Image Is Null Path [" + assetPath + "]");
					}
					savedAtlasPath.Add(assetPath, savedImage);
				}

				if (savedImage != null)
				{
					apTextureData textureData = Editor._portrait._textureData.Find(delegate (apTextureData a)
					{
						return a._image == savedImage;
					});

					//Debug.Log("Texture Asset : " + savedImage.name + " / Size : " + savedImage.width + "x" + savedImage.height);

					if (textureData == null)
					{
						textureData = AddImage();
						//이전 코드 : 4096 해상도에서 문제가 발생한다.
						//textureData.SetImage(savedImage, savedImage.width, savedImage.height);

						//변경된 코드 : 4096 해상도에서도 동작함
						textureData.SetImage(savedImage, bakedTextureWidth, bakedTextureHeight);

						//textureData._isPSDFile = true;
						textureData._assetFullPath = AssetDatabase.GetAssetPath(savedImage);

						//Debug.Log("Add Imaged : " + savedImage.name);

						addedTextureDataList.Add(textureData);
					}

					layerTextureMapping.Add(layerDataList[i], textureData);
				}
			}


			//2. Transform을 생성하자

			//1. Root가 될 MeshGroup을 만든다.
			apMeshGroup rootMeshGroup = AddMeshGroup(false, false);//false : Undo는 수행하지 않고, Hierarchy를 Refresh하지 않는다.
			rootMeshGroup._name = fileName;

			createdMonoObjects.Add(rootMeshGroup);//Undo에 등록할 오브젝트 추가 [v1.4.2]


			//2. Parent가 없는 LayerData를 찾으면서 Mesh 또는 MeshGroup을 만들어주자
			//<추가> Depth는 LayerIndex와 같다.
			apPSDSet psdSet = AddNewPSDSet(false);


			//>> Import 정보를 PSD Set에 저장한다.
			psdSet.SetPSDBakeData(filePath, fileName,
									rootMeshGroup,
									addedTextureDataList,
									bakedTextureWidth, bakedTextureHeight,
									totalWidth, totalHeight,
									0, 0,
									bakeDstFilePath, bakeDstFileRelativePath,
									bakeMaximumNumAtlas, padding, bakeBlurOption,
									atlasScaleRatioX100, meshGroupScaleRatioX100
								);




			//3. Child가 있으면 재귀적으로 생성해준다.
			RecursiveParsePSDLayers(	layerDataList,
										0,
										rootMeshGroup,
										layerTextureMapping,
										atlasScaleRatio,
										meshGroupScaleRatio,
										centerPosOffset,
										padding,
										psdSet,
										createdMonoObjects);


			//정렬 후 Depth Assign까지 한다.
			rootMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);
			rootMeshGroup.RefreshForce();

			RefreshMeshGroups();


			//[v1.4.2] Undo에 생성된 객체들을 등록한다.
			//apEditorUtil.SetRecordCreateMultipleMonoObjects(createdMonoObjects, "Import PSD", true, undoID);//이전

			//변경 v1.4.2
			apEditorUtil.SetRecordCreateMultipleMonoObjects(createdMonoObjects, "Import PSD");


			

			Editor.OnAnyObjectAddedOrRemoved();//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));//추가 : 전체 리셋을 한다.
			
			//Hierarchy에 메시 그룹 필터가 꺼졌다면 켜야한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.MeshGroup, true);

			//생성된 메시 그룹을 선택한다.선택
			Editor.Select.SelectMeshGroup(rootMeshGroup);

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

			
			
		}

		private void RecursiveParsePSDLayers(List<apPSDLayerData> layerDataList,
												int curLevel,
												apMeshGroup parentMeshGroup,
												Dictionary<apPSDLayerData, apTextureData> layerTextureMapping,
												float atlasScaleRatio, float meshGroupScaleRatio,
												Vector2 centerPosOffset, int padding,
												apPSDSet psdSet,
												List<MonoBehaviour> createdMonoObjects)
		{
			int nLayers = layerDataList != null ? layerDataList.Count : 0;
			if(nLayers == 0)
			{
				return;
			}

			for (int i = 0; i < nLayers; i++)
			{
				apPSDLayerData curLayer = layerDataList[i];

				if (curLayer._hierarchyLevel != curLevel)
				{
					continue;
				}

				if (!curLayer._isBakable)
				{
					SetPSDLayerToPSDSet_NotBaked(curLayer, psdSet);
					continue;
				}

				if (curLayer._isImageLayer)
				{
					//이미지 레이어인 경우)
					//Mesh로 만들고 MeshTransform으로서 추가한다.
					//미리 Vertex를 Atlas 정보에 맞게 만들어주자
					apMesh newMesh = AddMesh(false, false);//Undo에 등록하지 않고 Hierarchy를 갱신하지도 않는다.

					if (newMesh == null)
					{
						Debug.LogError("PSD Load Error : No Mesh Created");
						continue;
					}

					//Undo용으로 추가된 객체에 등록 [v1.4.2]
					createdMonoObjects.Add(newMesh);

					apTextureData textureData = null;

					if (layerTextureMapping.ContainsKey(curLayer))
					{
						textureData = layerTextureMapping[curLayer];

						//이전 코드
						//newMesh._textureData = textureData;

						//변경 코드 4.1
						newMesh.SetTextureData(textureData);

						float resizeRatioW = (float)textureData._width / (float)curLayer._bakedData._width;
						float resizeRatioH = (float)textureData._height / (float)curLayer._bakedData._height;

						//실제 텍스쳐 에셋의 크기와 저장할때의 원본 이미지 크기는 다를 수 있다.
						//텍스쳐 에셋 크기를 존중하는게 기본이다.
						Vector2 offsetPos = new Vector2(
							(float)curLayer._bakedImagePos_Left + ((float)curLayer._bakedWidth * 0.5f),
							(float)curLayer._bakedImagePos_Top + ((float)curLayer._bakedHeight * 0.5f));



						offsetPos.x *= resizeRatioW;
						offsetPos.y *= resizeRatioH;

						float atlasPos_Left = curLayer._bakedImagePos_Left * resizeRatioW;
						float atlasPos_Right = (curLayer._bakedImagePos_Left + curLayer._bakedWidth) * resizeRatioW;
						float atlasPos_Top = curLayer._bakedImagePos_Top * resizeRatioH;
						float atlasPos_Bottom = (curLayer._bakedImagePos_Top + curLayer._bakedHeight) * resizeRatioH;

						float halfSize_W = (float)textureData._width * 0.5f;
						float halfSize_H = (float)textureData._height * 0.5f;

						atlasPos_Left -= halfSize_W;
						atlasPos_Right -= halfSize_W;
						atlasPos_Top -= halfSize_H;
						atlasPos_Bottom -= halfSize_H;

						//Padding도 적용하자
						//atlasPos_Left -= padding;
						atlasPos_Right += padding * 2;
						//atlasPos_Top -= padding * 2;
						atlasPos_Bottom += padding * 2;

						offsetPos.x -= halfSize_W;
						offsetPos.y -= halfSize_H;

						offsetPos.x += padding;
						offsetPos.y += padding;

						//PSD용 이므로 Atlas정보를 넣어주자
						newMesh._isPSDParsed = true;
						newMesh._isNeedToAskRemoveVertByPSDImport = true;//<<추가 20.7.6 : 버텍스 리셋 알림

						//이전
						//newMesh._atlasFromPSD_LT = new Vector2(atlasPos_Left, atlasPos_Top);
						//newMesh._atlasFromPSD_RB = new Vector2(atlasPos_Right, atlasPos_Bottom);

						//변경 21.3.4 : T, B에 대해서 잘못 값이 들어갔다.
						//T가 MaxY로 들어가야 한다.
						newMesh._atlasFromPSD_LT.x = Mathf.Min(atlasPos_Left, atlasPos_Right);
						newMesh._atlasFromPSD_LT.y = Mathf.Max(atlasPos_Top, atlasPos_Bottom);
						newMesh._atlasFromPSD_RB.x = Mathf.Max(atlasPos_Left, atlasPos_Right);
						newMesh._atlasFromPSD_RB.y = Mathf.Min(atlasPos_Top, atlasPos_Bottom);
						

						newMesh.ResetVerticesByRect(offsetPos, atlasPos_Left, atlasPos_Top, atlasPos_Right, atlasPos_Bottom);
						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
					}

					newMesh._name = curLayer._name + "_Mesh";

					//Parent에 MeshTransform을 등록하자
					apTransform_Mesh meshTransform = AddMeshToMeshGroup(newMesh, parentMeshGroup);

					if (meshTransform == null)
					{
						//EditorUtility.DisplayDialog("Error", "Creating Mesh is failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshCreationFailed_Title),
														Editor.GetText(TEXT.MeshCreationFailed_Body),
														Editor.GetText(TEXT.Close));
						return;
					}

					meshTransform._nickName = curLayer._name;

					//기준 위치를 잡아주자
					meshTransform._matrix = new apMatrix();
					if (curLevel == 0)
					{
						//meshTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio - centerPosOffset);
						meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset, false);
					}
					else
					{
						//meshTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio);
						meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio, false);
					}

					//추가 7.9 : Mesh > MeshTransform의 크기가 더이상 같지 않다.
					float meshScale = meshGroupScaleRatio / atlasScaleRatio;//MeshGroup의 확대 비율 / Atlas의 확대 비율
					meshTransform._matrix.SetScale(meshScale, false);

					meshTransform._matrix.MakeMatrix();

					//추가 21.3.8 : 이게 지금까지 적용되지 않았다.
					meshTransform._meshColor2X_Default = curLayer._transparentColor2X;
					meshTransform._isVisible_Default = curLayer._isVisible;//추가 v1.4.2

					if (curLayer._isClipping)
					{
						if (curLayer._isClippingValid && parentMeshGroup != null)
						{
							//Debug.Log("PSD Layer 클리핑 적용 [" + curLayer._name + "]");
							AddClippingMeshTransform(parentMeshGroup, meshTransform, false, false, false);
						}
					}

					//PSD Set에 저장
					SetPSDLayerToPSDSet(	curLayer, 
											psdSet, 
											meshTransform._transformUniqueID,
											textureData != null ? textureData._uniqueID : -1);
				}
				else
				{
					//폴더 레이어인 경우)
					//MeshGroup으로 만들고 MeshGroupTransform으로서 추가한다.
					//재귀적으로 하위 호출을 한다.
					apMeshGroup newMeshGroup = AddMeshGroup(false, false);//Undo를 호출하지 않고 Hierarchy를 갱신하지 않는다.
					if (newMeshGroup == null)
					{
						Debug.LogError("PSD Load Error : No MeshGroup Created");
						continue;
					}

					//[v1.4.2] Undo용 리스트에 추가
					createdMonoObjects.Add(newMeshGroup);


					newMeshGroup._name = curLayer._name + "_MeshGroup";

					apTransform_MeshGroup meshGroupTransform = AddMeshGroupToMeshGroup(newMeshGroup, parentMeshGroup, null);

					meshGroupTransform._nickName = curLayer._name;

					//기준 위치를 잡아주자
					meshGroupTransform._matrix = new apMatrix();
					if (curLevel == 0)
					{
						meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset, false);
					}
					else
					{
						meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio, false);
					}
					//MeshGroup의 스케일은 그대로
					meshGroupTransform._matrix.MakeMatrix();

					//v1.4.2 추가
					meshGroupTransform._isVisible_Default = curLayer._isVisible;


					//PSD Set에 저장
					SetPSDLayerToPSDSet(curLayer, psdSet, meshGroupTransform._transformUniqueID, -1);

					//자식 노드를 검색해서 처리하자
					if (curLayer._childLayers != null)
					{
						RecursiveParsePSDLayers(	curLayer._childLayers,
													curLayer._hierarchyLevel + 1,
													newMeshGroup,
													layerTextureMapping,
													atlasScaleRatio,
													meshGroupScaleRatio,
													centerPosOffset,
													padding,
													psdSet,
													createdMonoObjects);
					}

					//이전
					//newMeshGroup.SortRenderUnits(true);//<<렌더 유닛 리셋보다 Sort를 먼저하는 잘못되 코드
					//newMeshGroup.RefreshForce();

					//변경 [v1.4.2] : 렌더유닛 리셋과 Sort를 같이 하려면 다음과 같이 코드를 짜는게 좋다. 
					newMeshGroup.SetDirtyToReset();
					newMeshGroup.RefreshForce();
				}
			}
		}




		// 추가 : PSD Reimport
		public void ShowPSDReimportDialog()
		{
			_psdDialogLoadKey = null;
			_psdReimportDialogLoadKey = apPSDReimportDialog.ShowWindow(_editor, OnPSDImageReimport);
		}


		//PSD 파일을 다시 임포트한다.
		//OnPSDImageLoad 함수와 거의 동일한 방식이지만, 기존 Transform에 적용하고, 텍스쳐도 덮어쓰기 때문에 많은 부분이 더 추가된다.
		//"새로운 데이터"인지 "기존 데이터"인지 구분하는것이 중요
		public void OnPSDImageReimport(bool isSuccess, object loadKey,
										string fileName, string filePath,
										List<apPSDLayerData> layerDataList,
										int atlasScaleRatioX100, int meshGroupScaleRatioX100, int prevAtlasScaleRatioX100,
										int totalWidth, int totalHeight, int padding,
										int bakedTextureWidth, int bakedTextureHeight,
										int bakeMaximumNumAtlas, bool bakeBlurOption,
										float centerOffsetDeltaX, float centerOffsetDeltaY,
										string bakeDstFilePath, string bakeDstFileRelativePath,
										apPSDSet psdSet
										//float deltaScaleRatio
										)
		{
			if (_psdReimportDialogLoadKey != loadKey)
			{
				_psdReimportDialogLoadKey = null;
				return;
			}

			_psdReimportDialogLoadKey = null;

			if (Editor._portrait == null || !isSuccess || psdSet == null)
			{
				return;
			}
			if (psdSet._linkedTargetMeshGroup == null)
			{
				//Reimport할 MeshGroup이 없어도 실패
				return;
			}


			//추가 v1.4.2 : Undo 등록
			//int undoID = apEditorUtil.SetRecordBeforeCreateOrDestroyMultipleObjects(Editor._portrait, "Reimport PSD");
			apEditorUtil.SetRecordBeforeCreateOrDestroyMultipleObjects(Editor._portrait, "Reimport PSD", false);//변경

			//이 과정에서 생성되는 모든 오브젝트들을 일괄적으로 Undo에 넣자
			List<MonoBehaviour> createdMonoObjects = new List<MonoBehaviour>();


			//이제 만들어봅시다.

			float atlasScaleRatio = (float)atlasScaleRatioX100 * 0.01f;
			float meshGroupScaleRatio = (float)meshGroupScaleRatioX100 * 0.01f;
			float prevAtlasScaleRatio = (float)prevAtlasScaleRatioX100 * 0.01f;

			//Debug.Log("Reimport Result");
			//Debug.Log("  Prev Atlas Scale X100 : " + prevAtlasScaleRatioX100);
			//Debug.Log("  Next Atlas Scale X100 : " + atlasScaleRatioX100);
			//Debug.Log("  Mesh Group Scale X100 : " + meshGroupScaleRatioX100);

			Vector2 centerPosOffset = new Vector2((float)totalWidth * 0.5f * meshGroupScaleRatio, (float)totalHeight * 0.5f * meshGroupScaleRatio);
			centerPosOffset.x -= psdSet._nextBakeCenterOffsetDelta_X * meshGroupScaleRatio;//Reimport : CenterPosOffset이 수정된다.
			centerPosOffset.y -= psdSet._nextBakeCenterOffsetDelta_Y * meshGroupScaleRatio;

			//1. Image 로드 + TextureData 생성
			//사용된 TextureData는 삭제. 이미지 에셋은 삭제하지 않는다.

			//일단 먼저 -> Image를 로드해야함
			//로드하고 TextureData에도 추가를 한 뒤, LayerData와 연동 맵을 만든다.
			Dictionary<string, Texture2D> savedAtlasPath = new Dictionary<string, Texture2D>();
			Dictionary<apPSDLayerData, apTextureData> layerTextureMapping = new Dictionary<apPSDLayerData, apTextureData>();

			//Reimport : 이전의 apTextureData로서 덮어씌워진다. (실제로는 새로운게 생성되고 이건 삭제된다)
			List<apTextureData> removableTextureData = new List<apTextureData>();
			for (int i = 0; i < psdSet._targetTextureDataList.Count; i++)
			{
				removableTextureData.Add(psdSet._targetTextureDataList[i]._linkedTextureData);
			}

			List<apTextureData> addedTextureDataList = new List<apTextureData>();

			//레이어 데이터에 따라서 이미지를 가져와서 리스트에 저장한다.
			//리스트에 저장하면서 새로운 apTextureData를 생성한다.
			for (int i = 0; i < layerDataList.Count; i++)
			{
				if (!layerDataList[i]._isImageLayer || !layerDataList[i]._isBakable)
				{
					continue;
				}

				string assetPath = layerDataList[i]._textureAssetPath;
				Texture2D savedImage = null;

				//Debug.Log("<" + layerDataList[i]._name + "> Image Path [" + assetPath + "]");
				if (savedAtlasPath.ContainsKey(assetPath))
				{
					savedImage = savedAtlasPath[assetPath];
				}
				else
				{
					savedImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (savedImage == null)
					{
						Debug.LogError("Image Is Null Path [" + assetPath + "]");
					}
					savedAtlasPath.Add(assetPath, savedImage);
				}


				if (savedImage != null)
				{
					apTextureData textureData = Editor._portrait._textureData.Find(delegate (apTextureData a)
					{
						return a._image == savedImage;
					});

					//Debug.Log("Texture Asset : " + savedImage.name + " / Size : " + savedImage.width + "x" + savedImage.height);

					if (textureData == null)
					{
						textureData = AddImage();
						//이전 코드 : 4096 해상도에서 문제가 발생한다.
						//textureData.SetImage(savedImage, savedImage.width, savedImage.height);

						//변경된 코드 : 4096 해상도에서도 동작함
						textureData.SetImage(savedImage, bakedTextureWidth, bakedTextureHeight);

						//textureData._isPSDFile = true;
						textureData._assetFullPath = AssetDatabase.GetAssetPath(savedImage);

						//Debug.Log("Add Imaged : " + savedImage.name);

						addedTextureDataList.Add(textureData);

					}
					else
					{
						//Reimport 과정에서 이미지가 그대로 사용되었다고 하더라도,
						//PSD 이미지 정보는 바뀌었을 것이다.
						textureData.SetImage(savedImage, bakedTextureWidth, bakedTextureHeight);
						textureData._assetFullPath = AssetDatabase.GetAssetPath(savedImage);
					}

					layerTextureMapping.Add(layerDataList[i], textureData);

					//만약 이 apTextureData가 그대로 사용되면 삭제되어선 안된다.
					if (removableTextureData.Contains(textureData))
					{
						removableTextureData.Remove(textureData);
					}
				}
			}

			//2. Transform을 변경/생성하자
			// - Reimport 대상인 Transform >> 위치값과 Vertex/Texture를 변경한다.
			// - Reimport 대상이 아닌 Transform >> 무시
			// - Reimport를 해야하지만 Transform이 없는 경우 >> 새로 생성
			//Target MeshGroup은 있어야 한다.

			//>> Import 정보를 PSD Set에 저장한다.
			psdSet.SetPSDBakeData(filePath, fileName,
									psdSet._linkedTargetMeshGroup,
									addedTextureDataList,
									bakedTextureWidth, bakedTextureHeight,
									totalWidth, totalHeight,
									centerOffsetDeltaX, centerOffsetDeltaY,
									bakeDstFilePath, bakeDstFileRelativePath,
									bakeMaximumNumAtlas, padding, bakeBlurOption,
									atlasScaleRatioX100, meshGroupScaleRatioX100
								);


			apMeshGroup rootMeshGroup = psdSet._linkedTargetMeshGroup;

			RecursiveParsePSDLayers_Reimport(layerDataList, 0,
												rootMeshGroup,
												layerTextureMapping,
												//scaleRatio, 
												atlasScaleRatio, meshGroupScaleRatio, prevAtlasScaleRatio,
												centerPosOffset, padding,
												psdSet,
												createdMonoObjects);

			//이전
			//rootMeshGroup.SortRenderUnits(true);
			//rootMeshGroup.RefreshForce();

			//변경 v1.4.2 : 리셋 후 Depth 할당까지
			rootMeshGroup.SetDirtyToReset();
			rootMeshGroup.RefreshForce();
			rootMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);



			RefreshMeshGroups();

			//[v1.4.2] Undo에 생성된 객체들을 등록한다.
			//apEditorUtil.SetRecordCreateMultipleMonoObjects(createdMonoObjects, "Reimport PSD", true, undoID);
			apEditorUtil.SetRecordCreateMultipleMonoObjects(createdMonoObjects, "Reimport PSD");//변경


			

			//3. 마무리
			Editor.OnAnyObjectAddedOrRemoved();//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));//추가 : 전체 리셋을 한다.
			
			//Hierarchy에 메시 그룹 필터가 꺼졌다면 켜야한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.MeshGroup, true);

			//생성된 메시 그룹을 선택한다.선택
			if(rootMeshGroup != null)
			{
				Editor.Select.SelectMeshGroup(rootMeshGroup);
			}
			

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

		}



		private void RecursiveParsePSDLayers_Reimport(List<apPSDLayerData> layerDataList,
														int curLevel,
														apMeshGroup parentMeshGroup,
														Dictionary<apPSDLayerData, apTextureData> layerTextureMapping,
														float atlasScaleRatio, float meshGroupScaleRatio, float prevAtlasScaleRatio,
														Vector2 centerPosOffset, int padding,
														apPSDSet psdSet,
														List<MonoBehaviour> createdMonoObjects)
		{
			int nLayerData = layerDataList != null ? layerDataList.Count : 0;
			if(nLayerData == 0)
			{
				return;
			}

			//변경 v1.4.2
			//새로 추가되는 TF들을 이전의 "기존의 TF"의 바로 위쪽에 생성되도록 만든다.
			//없다면 맨 위.
			//layerDataList는 "같은 레벨의 레이어 정보"가 "Depth 순서대로 정렬된 것"이기 때문에
			//순서대로 체크하면 되겠다.

			//기존에 생성된 TF를 가져오자
			apTransform_Mesh lastMeshTF = null;
			apTransform_MeshGroup lastMeshGroupTF = null;

			for (int i = 0; i < nLayerData; i++)
			{
				apPSDLayerData curLayer = layerDataList[i];

				if (curLayer._hierarchyLevel != curLevel)
				{
					continue;
				}

				if (!curLayer._isBakable)
				{
					SetPSDLayerToPSDSet_NotBaked(curLayer, psdSet);
					continue;
				}



				// - Reimport 대상인 Transform >> 위치값과 Vertex/Texture를 변경한다.
				// - Reimport 대상이 아닌 Transform >> 무시 (이건 위에서 isBakable)로 체크한다.
				// - Reimport를 해야하지만 Transform이 없는 경우 >> 새로 생성

				bool isRemapSelected = curLayer._isRemapSelected;
				apTransform_Mesh remapMeshTF = null;
				apTransform_MeshGroup remapMeshGroupTF = null;
				if (isRemapSelected)
				{
					if (curLayer._isImageLayer)
					{
						remapMeshTF = curLayer._remap_MeshTransform;
						if (remapMeshTF == null)
						{
							isRemapSelected = false;
						}
					}
					else
					{
						remapMeshGroupTF = curLayer._remap_MeshGroupTransform;
						if (remapMeshGroupTF == null)
						{
							isRemapSelected = false;
						}
					}
				}


				if (curLayer._isImageLayer)
				{
					//이미지 레이어인 경우)

					if (remapMeshTF != null)
					{
						//1) Remap할 MeshTransform이 있다. > MeshTransform의 위치와 Vertex/Texture를 수정해야한다.
						apMesh remapMesh = remapMeshTF._mesh;

						apTextureData textureData = null;

						if (remapMesh != null && layerTextureMapping.ContainsKey(curLayer))
						{
							textureData = layerTextureMapping[curLayer];

							remapMesh.SetTextureData(textureData);

							float resizeRatioW = (float)textureData._width / (float)curLayer._bakedData._width;
							float resizeRatioH = (float)textureData._height / (float)curLayer._bakedData._height;

							//실제 텍스쳐 에셋의 크기와 저장할때의 원본 이미지 크기는 다를 수 있다.
							//텍스쳐 에셋 크기를 존중하는게 기본이다.
							Vector2 offsetPos = new Vector2(
								(float)curLayer._bakedImagePos_Left + ((float)curLayer._bakedWidth * 0.5f),
								(float)curLayer._bakedImagePos_Top + ((float)curLayer._bakedHeight * 0.5f));

							offsetPos.x *= resizeRatioW;
							offsetPos.y *= resizeRatioH;

							float atlasPos_Left = curLayer._bakedImagePos_Left * resizeRatioW;
							float atlasPos_Right = (curLayer._bakedImagePos_Left + curLayer._bakedWidth) * resizeRatioW;
							float atlasPos_Top = curLayer._bakedImagePos_Top * resizeRatioH;
							float atlasPos_Bottom = (curLayer._bakedImagePos_Top + curLayer._bakedHeight) * resizeRatioH;

							float halfSize_W = (float)textureData._width * 0.5f;
							float halfSize_H = (float)textureData._height * 0.5f;

							atlasPos_Left -= halfSize_W;
							atlasPos_Right -= halfSize_W;
							atlasPos_Top -= halfSize_H;
							atlasPos_Bottom -= halfSize_H;

							//Padding도 적용하자
							//atlasPos_Left -= padding;
							atlasPos_Right += padding * 2;
							//atlasPos_Top -= padding * 2;
							atlasPos_Bottom += padding * 2;

							offsetPos.x -= halfSize_W;
							offsetPos.y -= halfSize_H;

							offsetPos.x += padding;
							offsetPos.y += padding;

							//변경된 Atlas 정보를 넣어주자
							remapMesh.MoveVertexToRemappedAtlas(offsetPos,
																	curLayer._remapPosOffsetDelta_X, curLayer._remapPosOffsetDelta_Y,
																	new Vector2(atlasPos_Left, atlasPos_Top),//Atlas LT
																	new Vector2(atlasPos_Right, atlasPos_Bottom),//Atlas RB
																												 //deltaScaleRatio
																	prevAtlasScaleRatio,
																	atlasScaleRatio
																	);
							Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
						}

						//변경 20.2.19 : 이름을 강제로 바꾸면 문제가 생길 수도 있다.
						//if (remapMesh != null)
						//{
						//	//이름은 바꿔준다.
						//	remapMesh._name = curLayer._name + "_Mesh";
						//}
						//remapMeshTF._nickName = curLayer._name;


						//기준 위치를 잡아주자
						remapMeshTF._matrix = new apMatrix();
						if (curLevel == 0)
						{
							remapMeshTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset, false);
						}
						else
						{
							remapMeshTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio, false);
						}

						//추가 7.9 : Mesh > MeshTransform의 크기가 더이상 같지 않다.
						float meshScale = meshGroupScaleRatio / atlasScaleRatio;//MeshGroup의 확대 비율 / Atlas의 확대 비율
						remapMeshTF._matrix.SetScale(meshScale, false);

						remapMeshTF._matrix.MakeMatrix();


						//변경 21.3.8 : 알파가 적용안되는 버그를 해결하자
						remapMeshTF._meshColor2X_Default = curLayer._transparentColor2X;

						//기존의 Visible은 변경하지 않는다.


						//PSD Set에 저장
						SetPSDLayerToPSDSet(	curLayer, 
												psdSet, 
												remapMeshTF._transformUniqueID,
												textureData != null ? textureData._uniqueID : -1);

						//클리핑 처리는 하지 않는다.
						//if (curLayer._isClipping && curLayer._isClippingValid && parentMeshGroup != null)
						//{
						//	AddClippingMeshTransform(parentMeshGroup, meshTransform, false);
						//}

						//v1.4.2 마지막 TF 갱신
						lastMeshTF = remapMeshTF;
						lastMeshGroupTF = null;
					}
					else
					{
						//2) Remap할 MeshTransform이 없다. > 새로 생성해야한다.
						//Mesh로 만들고 MeshTransform으로서 추가한다.
						//미리 Vertex를 Atlas 정보에 맞게 만들어주자
						apMesh newMesh = AddMesh(false, false);//false : Undo 기록 없고 Hierarchy Refresh를 하지 않는다.
						if (newMesh == null)
						{
							Debug.LogError("PSD Load Error : No Mesh Created");
							continue;
						}

						createdMonoObjects.Add(newMesh);//Undo용으로 추가한다. [v1.4.2]


						apTextureData textureData = null;

						if (layerTextureMapping.ContainsKey(curLayer))
						{
							textureData = layerTextureMapping[curLayer];

							//이전 코드
							//newMesh._textureData = textureData;

							//변경 코드 4.1
							newMesh.SetTextureData(textureData);

							float resizeRatioW = (float)textureData._width / (float)curLayer._bakedData._width;
							float resizeRatioH = (float)textureData._height / (float)curLayer._bakedData._height;

							//실제 텍스쳐 에셋의 크기와 저장할때의 원본 이미지 크기는 다를 수 있다.
							//텍스쳐 에셋 크기를 존중하는게 기본이다.
							Vector2 offsetPos = new Vector2(
								(float)curLayer._bakedImagePos_Left + ((float)curLayer._bakedWidth * 0.5f),
								(float)curLayer._bakedImagePos_Top + ((float)curLayer._bakedHeight * 0.5f));



							offsetPos.x *= resizeRatioW;
							offsetPos.y *= resizeRatioH;

							float atlasPos_Left = curLayer._bakedImagePos_Left * resizeRatioW;
							float atlasPos_Right = (curLayer._bakedImagePos_Left + curLayer._bakedWidth) * resizeRatioW;
							float atlasPos_Top = curLayer._bakedImagePos_Top * resizeRatioH;
							float atlasPos_Bottom = (curLayer._bakedImagePos_Top + curLayer._bakedHeight) * resizeRatioH;

							float halfSize_W = (float)textureData._width * 0.5f;
							float halfSize_H = (float)textureData._height * 0.5f;

							atlasPos_Left -= halfSize_W;
							atlasPos_Right -= halfSize_W;
							atlasPos_Top -= halfSize_H;
							atlasPos_Bottom -= halfSize_H;

							//Padding도 적용하자
							//atlasPos_Left -= padding;
							atlasPos_Right += padding * 2;
							//atlasPos_Top -= padding * 2;
							atlasPos_Bottom += padding * 2;

							offsetPos.x -= halfSize_W;
							offsetPos.y -= halfSize_H;

							offsetPos.x += padding;
							offsetPos.y += padding;

							//PSD용 이므로 Atlas정보를 넣어주자
							newMesh._isPSDParsed = true;
							newMesh._isNeedToAskRemoveVertByPSDImport = true;//<<추가 20.7.6 : 버텍스 리셋 알림
							newMesh._atlasFromPSD_LT = new Vector2(atlasPos_Left, atlasPos_Top);
							newMesh._atlasFromPSD_RB = new Vector2(atlasPos_Right, atlasPos_Bottom);

							newMesh.ResetVerticesByRect(offsetPos, atlasPos_Left, atlasPos_Top, atlasPos_Right, atlasPos_Bottom);
							Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
						}

						newMesh._name = curLayer._name + "_Mesh";

						//Parent에 MeshTransform을 등록하자
						apTransform_Mesh meshTransform = AddMeshToMeshGroup(newMesh, parentMeshGroup, false);

						if (meshTransform == null)
						{
							//EditorUtility.DisplayDialog("Error", "Creating Mesh is failed", "Close");
							EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshCreationFailed_Title),
															Editor.GetText(TEXT.MeshCreationFailed_Body),
															Editor.GetText(TEXT.Close));
							return;
						}

						meshTransform._nickName = curLayer._name;

						//기준 위치를 잡아주자
						meshTransform._matrix = new apMatrix();
						if (curLevel == 0)
						{
							meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset, false);
						}
						else
						{
							meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio, false);
						}

						//추가 7.9 : Mesh > MeshTransform의 크기가 더이상 같지 않다.
						float meshScale = meshGroupScaleRatio / atlasScaleRatio;//MeshGroup의 확대 비율 / Atlas의 확대 비율
						meshTransform._matrix.SetScale(meshScale, false);

						meshTransform._matrix.MakeMatrix();

						//변경 21.3.8 : 알파가 적용안되는 버그를 해결하자
						meshTransform._meshColor2X_Default = curLayer._transparentColor2X;

						//v1.4.2 추가
						meshTransform._isVisible_Default = curLayer._isVisible;


						if (curLayer._isClipping && curLayer._isClippingValid && parentMeshGroup != null)
						{
							AddClippingMeshTransform(parentMeshGroup, meshTransform, false, false, false);
						}

						//PSD Set에 저장
						SetPSDLayerToPSDSet(	curLayer,
												psdSet,
												meshTransform._transformUniqueID,
												textureData != null ? textureData._uniqueID : -1);


						//v1.4.2 Depth를 순서에 맞게 넣을 수 있도록 해보자
						if (lastMeshTF != null || lastMeshGroupTF != null)
						{
							//이전의 TF가 존재한다면, Depth가 그 위에 들어가도록 해보자
							int lastDepth = 0;
							if (lastMeshTF != null)
							{
								lastDepth = lastMeshTF._depth;
							}
							else if (lastMeshGroupTF != null)
							{
								lastDepth = lastMeshGroupTF._depth;
							}
							
							//Depth를 다시 할당하자
							meshTransform._depth = lastDepth + 1;
						}
						else
						{
							//이전의 TF가 존재하지 않는다면, Depth가 최소값이 들어가야한다.
							//(기본적으로 생성하면 맨 위에 오도록 TF들 중에 최대값이 들어간다.)
							meshTransform._depth = parentMeshGroup.GetFirstDepth();
						}

						//새로 추가되는 TF보다 같거나 위의 위치한 TF들의 Depth를 모두 1씩 증가시키자
						int curAddedDepth = meshTransform._depth;

						//메시 그룹의 다른 TF의 Depth를 증가시켜서 자리를 비켜주자
						int nOtherMeshTFs = parentMeshGroup._childMeshTransforms != null ? parentMeshGroup._childMeshTransforms.Count : 0;
						int nOtherMeshGroupTFs = parentMeshGroup._childMeshGroupTransforms != null ? parentMeshGroup._childMeshGroupTransforms.Count : 0;

						if (nOtherMeshTFs > 0)
						{
							apTransform_Mesh otherMeshTF = null;
							for (int iOtherMeshTF = 0; iOtherMeshTF < nOtherMeshTFs; iOtherMeshTF++)
							{
								otherMeshTF = parentMeshGroup._childMeshTransforms[iOtherMeshTF];
								if(otherMeshTF == meshTransform
									|| otherMeshTF._depth < curAddedDepth)
								{
									//대상이 아니다.
									continue;
								}

								//Depth를 증가시켜서 위로 올리고 자리를 비우자
								otherMeshTF._depth += 1;
							}
						}
						if(nOtherMeshGroupTFs > 0)
						{
							apTransform_MeshGroup otherMeshGroupTF = null;
							for (int iOtherMeshGroupTF = 0; iOtherMeshGroupTF < nOtherMeshGroupTFs; iOtherMeshGroupTF++)
							{
								otherMeshGroupTF = parentMeshGroup._childMeshGroupTransforms[iOtherMeshGroupTF];
								if(otherMeshGroupTF._depth < curAddedDepth)
								{
									//대상이 아니다.
									continue;
								}

								//Depth를 증가시켜서 위로 올리고 자리를 비우자
								otherMeshGroupTF._depth += 1;
							}
						}


						//v1.4.2 마지막 TF 갱신
						lastMeshTF = meshTransform;
						lastMeshGroupTF = null;
					}
				}
				else
				{
					//폴더 레이어인 경우)
					if (remapMeshGroupTF != null)
					{
						//이름을 바꾸자 > 변경 20.2.19 : 이름 바꾸면 매핑에 문제가 생긴다.
						//remapMeshGroupTF._nickName = curLayer._name + "_MeshGroup";

						//기준 위치 수정
						remapMeshGroupTF._matrix = new apMatrix();
						if (curLevel == 0)
						{
							remapMeshGroupTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset, false);
						}
						else
						{
							remapMeshGroupTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio, false);
						}
						remapMeshGroupTF._matrix.MakeMatrix();

						//기존의 Transform의 Visible 여부는 수정하지 않는다.

						//자식 노드를 검색해서 처리하자
						if (curLayer._childLayers != null)
						{
							RecursiveParsePSDLayers_Reimport(curLayer._childLayers,
																curLayer._hierarchyLevel + 1,
																remapMeshGroupTF._meshGroup,
																layerTextureMapping,
																atlasScaleRatio, meshGroupScaleRatio, prevAtlasScaleRatio,
																centerPosOffset,
																padding, psdSet,
																createdMonoObjects);
						}

						//이전
						//remapMeshGroupTF._meshGroup.SortRenderUnits(true);
						//remapMeshGroupTF._meshGroup.RefreshForce();

						//변경 v1.4.2 : 리셋을 하는게 맞다.
						remapMeshGroupTF._meshGroup.SetDirtyToReset();
						remapMeshGroupTF._meshGroup.RefreshForce();

						//PSD Set에 저장
						SetPSDLayerToPSDSet(curLayer, psdSet, remapMeshGroupTF._transformUniqueID, -1);

						//v1.4.2 마지막 TF 갱신
						lastMeshTF = null;
						lastMeshGroupTF = remapMeshGroupTF;
					}
					else
					{
						//MeshGroup으로 만들고 MeshGroupTransform으로서 추가한다.
						//재귀적으로 하위 호출을 한다.
						apMeshGroup newMeshGroup = AddMeshGroup(false, false);//false : Undo, Refresh를 하지 않는다.
						if (newMeshGroup == null)
						{
							Debug.LogError("PSD Load Error : No MeshGroup Created");
							continue;
						}

						//Undo용으로 추가한다. [v1.4.2]
						createdMonoObjects.Add(newMeshGroup);



						newMeshGroup._name = curLayer._name + "_MeshGroup";

						apTransform_MeshGroup meshGroupTransform = AddMeshGroupToMeshGroup(newMeshGroup, parentMeshGroup, null, false);

						meshGroupTransform._nickName = curLayer._name;

						//기준 위치를 잡아주자
						meshGroupTransform._matrix = new apMatrix();
						if (curLevel == 0)
						{
							meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset, false);
						}
						else
						{
							meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio, false);
						}
						meshGroupTransform._matrix.MakeMatrix();

						//v1.4.2 추가
						meshGroupTransform._isVisible_Default = curLayer._isVisible;


						//PSD Set에 저장
						SetPSDLayerToPSDSet(curLayer, psdSet, meshGroupTransform._transformUniqueID, -1);


						//v1.4.2 Depth를 순서에 맞게 넣을 수 있도록 해보자
						if(lastMeshTF != null || lastMeshGroupTF != null)
						{
							//이전의 TF가 존재한다면, Depth가 그 위에 들어가도록 해보자
							int lastDepth = 0;
							if(lastMeshTF != null) { lastDepth = lastMeshTF._depth; }
							else if(lastMeshGroupTF != null) { lastDepth = lastMeshGroupTF._depth; }
							
							//Depth를 다시 할당하자
							meshGroupTransform._depth = lastDepth + 1;
						}
						else
						{
							//맨 아래에 추가된 것이라면 FirstDepth를 입력하자
							meshGroupTransform._depth = parentMeshGroup.GetFirstDepth();
						}

						//새로 추가되는 TF보다 같거나 위의 위치한 TF들의 Depth를 모두 1씩 증가시키자
						int curAddedDepth = meshGroupTransform._depth;

						//메시 그룹의 다른 TF의 Depth를 증가시켜서 자리를 비켜주자
						int nOtherMeshTFs = parentMeshGroup._childMeshTransforms != null ? parentMeshGroup._childMeshTransforms.Count : 0;
						int nOtherMeshGroupTFs = parentMeshGroup._childMeshGroupTransforms != null ? parentMeshGroup._childMeshGroupTransforms.Count : 0;

						if (nOtherMeshTFs > 0)
						{
							apTransform_Mesh otherMeshTF = null;

							for (int iOtherMeshTF = 0; iOtherMeshTF < nOtherMeshTFs; iOtherMeshTF++)
							{
								otherMeshTF = parentMeshGroup._childMeshTransforms[iOtherMeshTF];
								if(otherMeshTF._depth < curAddedDepth)
								{
									//대상이 아니다.
									continue;
								}

								//Depth를 증가시켜서 위로 올리고 자리를 비우자
								otherMeshTF._depth += 1;
							}
						}
						if(nOtherMeshGroupTFs > 0)
						{
							apTransform_MeshGroup otherMeshGroupTF = null;
							for (int iOtherMeshGroupTF = 0; iOtherMeshGroupTF < nOtherMeshGroupTFs; iOtherMeshGroupTF++)
							{
								otherMeshGroupTF = parentMeshGroup._childMeshGroupTransforms[iOtherMeshGroupTF];
								if(otherMeshGroupTF == meshGroupTransform
									|| otherMeshGroupTF._depth < curAddedDepth)
								{
									//대상이 아니다.
									continue;
								}

								//Depth를 증가시켜서 위로 올리고 자리를 비우자
								otherMeshGroupTF._depth += 1;
							}
						}


						//자식 노드를 검색해서 처리하자
						if (curLayer._childLayers != null)
						{
							RecursiveParsePSDLayers_Reimport(curLayer._childLayers, curLayer._hierarchyLevel + 1,
																newMeshGroup,
																layerTextureMapping,
																atlasScaleRatio, meshGroupScaleRatio, prevAtlasScaleRatio,
																centerPosOffset,
																padding, psdSet,
																createdMonoObjects);
						}

						//이전
						//newMeshGroup.SortRenderUnits(true);
						//newMeshGroup.RefreshForce();

						//변경 v1.4.2
						newMeshGroup.SetDirtyToReset();
						newMeshGroup.RefreshForce();

						//v1.4.2 마지막 TF 갱신
						lastMeshTF = null;
						lastMeshGroupTF = meshGroupTransform;
					}

				}
			}
		}


		private void SetPSDLayerToPSDSet(apPSDLayerData psdLayer, apPSDSet psdSet, int transformID, int textureDataID)
		{
			if (psdSet == null || psdLayer == null)
			{
				return;
			}

			

			psdSet.SetPSDLayerData(psdLayer._layerIndex,
									psdLayer._name,
									psdLayer._width,
									psdLayer._height,
									//psdLayer._posOffset_Left,
									//psdLayer._posOffset_Top,
									//psdLayer._posOffset_Right,
									//psdLayer._posOffset_Bottom,
									psdLayer._isImageLayer,
									//psdLayer._bakedWidth,
									//psdLayer._bakedHeight,
									//psdLayer._bakedImagePos_Left,
									//psdLayer._bakedImagePos_Top,
									transformID,
									textureDataID,
									(psdLayer._isRemapSelected ? psdLayer._remapPosOffsetDelta_X : 0.0f),
									(psdLayer._isRemapSelected ? psdLayer._remapPosOffsetDelta_Y : 0.0f),

									//추가 22.6.22
									psdLayer._bakedAtalsIndex,
									psdLayer._bakedWidth,
									psdLayer._bakedHeight,
									psdLayer._bakedImagePos_Left + psdSet._bakeOption_Padding,//Padding만큼 더 이동해야한다.
									psdLayer._bakedImagePos_Top + psdSet._bakeOption_Padding
								);
		}

		private void SetPSDLayerToPSDSet_NotBaked(apPSDLayerData psdLayer, apPSDSet psdSet)
		{
			if (psdSet == null || psdLayer == null)
			{
				return;
			}
			psdSet.SetNotBakedLayerData(
									psdLayer._layerIndex,
									psdLayer._name,
									psdLayer._isImageLayer
								);
		}

		public apPSDSet AddNewPSDSet(bool isUndoRecord)
		{
			if (Editor._portrait == null)
			{
				return null;
			}



			//새로운 ID를 만든다. (랜덤 + 검색) (랜덤 실패 후에는 특정 구역의 숫자 할당)
			int nextUniqueID = -1;
			int count = 0;
			bool isValidID = false;
			while (true)
			{
				nextUniqueID = UnityEngine.Random.Range(1000, 99999);
				bool isExistID = Editor._portrait._bakedPsdSets.Exists(delegate (apPSDSet a)
				{
					return a._uniqueID == nextUniqueID;
				});
				if (!isExistID)
				{
					//겹치는게 없다 => 유효한 ID
					isValidID = true;
					break;
				}
				count++;
				if (count > 50)
				{
					//50번이나 시도해서 실패했다.
					break;
				}
			}
			if (!isValidID)
			{
				//50번의 시도 안에 유효한 ID가 없으면 100~1000번대를 순서대로 체크한다.
				for (int i = 100; i < 1000; i++)
				{
					nextUniqueID = i;
					bool isExistID = Editor._portrait._bakedPsdSets.Exists(delegate (apPSDSet a)
					{
						return a._uniqueID == nextUniqueID;
					});
					if (!isExistID)
					{
						//겹치는게 없다 => 유효한 ID
						isValidID = true;
						break;
					}
				}
			}
			if (!isValidID)
			{
				Debug.LogError("AnyPortrait : A new PSD Import Set could not be created. please try again.");
				return null;
			}

			if (isUndoRecord)
			{
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.PSDSet_AddNewPSDSet, 
													Editor, 
													Editor._portrait, 
													//Editor._portrait, 
													false,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}


			apPSDSet newPsdSet = new apPSDSet(nextUniqueID);
			Editor._portrait._bakedPsdSets.Add(newPsdSet);

			return newPsdSet;
		}


		public apPSDSecondarySet AddNewPSDSecondarySet(apPSDSet srcPSDSet)
		{
			if (Editor._portrait == null || srcPSDSet == null)
			{
				return null;
			}

			//검사를 한다.
			//1. 모든 레이어가 Atlas가 유효해야한다.
			

			if(!srcPSDSet._isLastBaked)
			{
				//Bake된 적이 없다면
				return null;
			}


			bool isValidPSD = true;

			int nSrcLayers = srcPSDSet._layers != null ? srcPSDSet._layers.Count : 0;
			if (nSrcLayers > 0)
			{
				apPSDSetLayer curSrcLayer = null;
				for (int i = 0; i < nSrcLayers; i++)
				{
					curSrcLayer = srcPSDSet._layers[i];
					if (curSrcLayer._isBaked
						&& curSrcLayer._isImageLayer)
					{
						//Bake 기록이 하나라도 유효하지 않다면
						if(curSrcLayer._bakedAtlasIndex < 0
							|| curSrcLayer._bakedWidth < 0
							|| curSrcLayer._bakedHeight < 0
							|| curSrcLayer._bakedUniqueID < 0
							|| curSrcLayer._textureDataID < 0)
						{
							//유효하지 않은 레이어가 있다.
							isValidPSD = false;
							break;
						}
					}
				}
			}
			if(!isValidPSD)
			{
				//유효하지 않다.
				return null;
			}

			apPortrait portrait = Editor._portrait;


			//새로운 ID를 만든다. (랜덤 + 검색) (랜덤 실패 후에는 특정 구역의 숫자 할당)			
			int nPSDSecSets = portrait._bakedPsdSecondarySet != null ? portrait._bakedPsdSecondarySet.Count : 0;
			
			int nextUniqueID = -1;

			if (nPSDSecSets == 0)
			{
				//랜덤하게 생성
				nextUniqueID = UnityEngine.Random.Range(1000, 99999);
			}
			else
			{
				int count = 0;
				bool isValidID = false;

				while (true)
				{
					nextUniqueID = UnityEngine.Random.Range(1000, 99999);
					bool isExistID = portrait._bakedPsdSecondarySet.Exists(delegate (apPSDSecondarySet a)
					{
						return a._uniqueID == nextUniqueID;
					});
					if (!isExistID)
					{
						//겹치는게 없다 => 유효한 ID
						isValidID = true;
						break;
					}
					count++;
					if (count > 50)
					{
						//50번이나 시도해서 실패했다.
						break;
					}
				}
				if (!isValidID)
				{
					//50번의 시도 안에 유효한 ID가 없으면 100~1000번대를 순서대로 체크한다.
					for (int i = 100; i < 1000; i++)
					{
						nextUniqueID = i;
						bool isExistID = portrait._bakedPsdSecondarySet.Exists(delegate (apPSDSecondarySet a)
						{
							return a._uniqueID == nextUniqueID;
						});
						if (!isExistID)
						{
							//겹치는게 없다 => 유효한 ID
							isValidID = true;
							break;
						}
					}
				}
				if (!isValidID)
				{
					Debug.LogError("AnyPortrait : A new PSD Import Set could not be created. please try again.");
					return null;
				}
			}

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.PSDSet_AddNewPSDSet, 
													Editor, 
													Editor._portrait, 
													//Editor._portrait, 
													false,
													apEditorUtil.UNDO_STRUCT.StructChanged);

			apPSDSecondarySet newPSDSecSet = new apPSDSecondarySet();
			newPSDSecSet.SetUniqueID(nextUniqueID);
			newPSDSecSet.CopyFromPSDSet(srcPSDSet);
			portrait._bakedPsdSecondarySet.Add(newPSDSecSet);

			return newPSDSecSet;
		}




		//--------------------------------------------------------------------------------------------------------------------------------

		#region [미사용 코드]
		///// <summary>
		///// 메시를 삭제한다.
		///// </summary>
		///// <param name="iRemove"></param>
		//public void RemoveMesh(int iRemove)
		//{
		//	if (iRemove >= 0 && iRemove < Editor._portrait._meshes.Count)
		//	{
		//		apEditorUtil.SetRecord("Remove Mesh", Editor._portrait);

		//		apMesh removedMesh = Editor._portrait._meshes[iRemove];

		//		if (removedMesh == Editor.Select.Mesh)
		//		{
		//			Editor.Select.SetNone();
		//		}

		//		Editor._portrait._meshes.RemoveAt(iRemove);

		//		if (removedMesh != null)
		//		{	
		//			MonoBehaviour.DestroyImmediate(removedMesh.gameObject);
		//		}
		//	}
		//} 
		#endregion





		public apMesh AddMesh(bool isRecordUndo = true, bool isSelectAndRefreshHierarchy = true)
		{
			//ObjectGroup을 체크하여 만들어주자
			CheckAndMakeObjectGroup();

			//int nextID = Editor._portrait.MakeUniqueID_Mesh();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Mesh);
			if (nextID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Mesh Add Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshAddFailed_Title),
												Editor.GetText(TEXT.MeshAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}

			//Undo - Add Mesh
			if(isRecordUndo)
			{
				apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create Mesh");
			}
			

			int nMeshes = Editor._portrait._meshes.Count;

			//GameObject로 만드는 경우
			string newName = "New Mesh (" + nMeshes + ")";
			GameObject newGameObj = new GameObject(newName);
			newGameObj.transform.parent = Editor._portrait._subObjectGroup_Mesh.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			apMesh newMesh = newGameObj.AddComponent<apMesh>();
			//apMesh newMesh = new apMesh();


			newMesh._uniqueID = nextID;
			newMesh._name = newName;

			newMesh.ReadyToEdit(Editor._portrait);

			Editor._portrait._meshes.Add(newMesh);

			if (isSelectAndRefreshHierarchy)
			{
				Editor.Select.SelectMesh(newMesh);

				Editor.RefreshControllerAndHierarchy(false);

				//Mesh Hierarchy Filter를 활성화한다.
				Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Mesh, true);
			}
			


			if (isRecordUndo)
			{
				//Undo - Create 추가
				apEditorUtil.SetRecordCreateMonoObject(newMesh, "Create Mesh");
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			////프리팹이었다면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

			return newMesh;
		}

		/// <summary>
		/// 추가 21.3.6 : 옵션에 따라 이미지가 한개인 경우 > 새로 생성된 메시에 바로 이미지를 할당할 수 있다.
		/// </summary>
		/// <param name="mesh"></param>
		public void CheckAndSetImageToMeshAutomatically(apMesh mesh)
		{
			if(mesh == null || Editor == null || Editor._portrait == null)
			{
				return;
			}
			if(!Editor._option_SetAutoImageToMeshIfOnlyOneImageExist)
			{
				//옵션이 꺼진 상태
				return;
			}
			int nImages = (Editor._portrait._textureData != null) ? Editor._portrait._textureData.Count : 0;
			if(mesh._textureData_Linked != null || nImages != 1)
			{
				//조건이 맞지 않는다.
				//이미지가 이미 할당 되었거나, 이미지가 1개가 아닌 상태
				return;
			}

			apTextureData targetTextureData = Editor._portrait._textureData[0];
			
			if(targetTextureData == null)
			{
				return;
			}

			//연결하자
			mesh.SetTextureData(targetTextureData);

			//이건 안내문도 추가
			Editor.Notification("An image was automatically assigned to the added mesh by option.", false, false);
			
		}


		public void RemoveMesh(apMesh mesh)
		{
			if (mesh == Editor.Select.Mesh)
			{
				Editor.Select.SelectNone();
			}

			//Undo
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_RemoveMesh, Editor, Editor._portrait, mesh, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Mesh");

			Editor._portrait.PushUnusedID(apIDManager.TARGET.Mesh, mesh._uniqueID);

			Editor._portrait._meshes.Remove(mesh);

			//if (mesh != null)
			//{	
			//	MonoBehaviour.DestroyImmediate(mesh.gameObject);
			//}

			//추가
			if (mesh != null)
			{
				//Undo.DestroyObjectImmediate(mesh.gameObject);
				apEditorUtil.SetRecordDestroyMonoObject(mesh, "Remove Mesh");
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));


			//Editor.Hierarchy.RefreshUnits();
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

			////프리팹이었다면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}


		/// <summary>
		/// 여러개의 메시들을 삭제한다 (21.10.10)
		/// </summary>
		/// <param name="meshes"></param>
		public void RemoveMeshes(List<apMesh> meshes)
		{
			if(meshes == null || meshes.Count == 0)
			{
				return;
			}
			//선택된게 있으면 해제
			if (Editor.Select.Mesh != null)
			{
				if (meshes.Contains(Editor.Select.Mesh))
				{
					Editor.Select.SelectNone();
				}
			}
			

			//Undo
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_RemoveMesh, Editor, Editor._portrait, mesh, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Mesh");


			int nMeshes = meshes.Count;
			apMesh curMesh = null;
			for (int i = 0; i < nMeshes; i++)
			{
				curMesh = meshes[i];
				if (curMesh == null)
				{
					continue;
				}

				Editor._portrait.PushUnusedID(apIDManager.TARGET.Mesh, curMesh._uniqueID);

				Editor._portrait._meshes.Remove(curMesh);

				apEditorUtil.SetRecordDestroyMonoObject(curMesh, "Remove Mesh");
			}


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//Editor.Hierarchy.RefreshUnits();
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(true);
		}






		/// <summary>
		/// 추가 20.1.5 : 메시를 복사한다.
		/// </summary>
		/// <param name="srcMesh"></param>
		/// <returns></returns>
		public apMesh DuplicateMesh(apMesh srcMesh)
		{
			if (srcMesh == null
				|| Editor == null
				|| Editor.Select == null
				//|| Editor.Select.Mesh == null//이게 있으면 버그 [v1.4.2]
				|| Editor._portrait == null)
			{
				return null;
			}

			try
			{
				//일단 새로운 메시를 생성한다.
				
				//Undo는 AddMesh 함수가 아닌 여기서 처리한다.
				apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Duplicate Mesh");

				apMesh newMesh = AddMesh(false, false);//Undo는 아직 실행하지 않는다. (false 파라미터)

				//Mesh 의 값을 복사하자
				//1. 이름
				string newMeshName = srcMesh._name + " Copy";

				//중복되지 않은 이름을 찾는다.
				if (Editor._portrait._meshes.Exists(delegate (apMesh a)
				{ return string.Equals(newMeshName, a._name); })
					)
				{
					//오잉 똑같은 이름이 있네염...
					int copyIndex = -1;
					for (int iCopyIndex = 1; iCopyIndex < 1000; iCopyIndex++)
					{
						if (!Editor._portrait._meshes.Exists(delegate (apMesh a)
						{ return string.Equals(newMeshName + " (" + iCopyIndex + ")", a._name); }))
						{
							copyIndex = iCopyIndex;
							break;
						}
					}
					if (copyIndex < 0)
					{ newMeshName += " (1000+)"; }
					else
					{ newMeshName += " (" + copyIndex + ")"; }
				}

				newMesh._name = newMeshName;

				//2. 텍스쳐 데이터 복제
				newMesh.SetTextureData(srcMesh._textureData_Linked);

				Dictionary<apVertex, apVertex> srcVert2NewVert = new Dictionary<apVertex, apVertex>();
				Dictionary<apMeshEdge, apMeshEdge> srcEdge2NewEdge = new Dictionary<apMeshEdge, apMeshEdge>();

				//3. 버텍스 데이터 복제
				int nVert = srcMesh._vertexData == null ? 0 : srcMesh._vertexData.Count;
				if (nVert > 0)
				{
					apVertex srcVert = null;
					apVertex newVert = null;
					for (int iVert = 0; iVert < nVert; iVert++)
					{
						srcVert = srcMesh._vertexData[iVert];
						newVert = newMesh.AddVertexAutoUV(srcVert._pos);
						if (newVert == null)
						{
							//에러 발생 > 실패
							Debug.LogError("AnyPortrait : Duplicating the mesh failed. Vertices cannot be duplicated. The mesh's data was not generated normally. Please try again.");
							return newMesh;
						}
						newVert._pos = srcVert._pos;
						newVert._uv = srcVert._uv;
						newVert._zDepth = srcVert._zDepth;

						srcVert2NewVert.Add(srcVert, newVert);
					}
				}

				//4. 인덱스 버퍼 복제
				int nIndexBuffer = srcMesh._indexBuffer == null ? 0 : srcMesh._indexBuffer.Count;
				if (nIndexBuffer > 0)
				{
					newMesh._indexBuffer.Clear();
					for (int iIB = 0; iIB < nIndexBuffer; iIB++)
					{
						newMesh._indexBuffer.Add(srcMesh._indexBuffer[iIB]);
					}
				}

				//5. Edge 복사
				int nEdge = srcMesh._edges == null ? 0 : srcMesh._edges.Count;
				if (nEdge > 0)
				{
					apMeshEdge srcEdge = null;
					apMeshEdge newEdge = null;
					apVertex srcVert_1 = null;
					apVertex srcVert_2 = null;
					apVertex newVert_1 = null;
					apVertex newVert_2 = null;
					for (int iEdge = 0; iEdge < nEdge; iEdge++)
					{
						srcEdge = srcMesh._edges[iEdge];
						srcVert_1 = srcEdge._vert1;
						srcVert_2 = srcEdge._vert2;

						newVert_1 = null;
						newVert_2 = null;

						//SrcEdge의 버텍스들에 해당하는 "복제된"버텍스들이 있는지 확인
						if (srcVert2NewVert.ContainsKey(srcVert_1))
						{
							newVert_1 = srcVert2NewVert[srcVert_1];
						}
						if (srcVert2NewVert.ContainsKey(srcVert_2))
						{
							newVert_2 = srcVert2NewVert[srcVert_2];
						}

						if (newVert_1 == null || newVert_2 == null)
						{
							//에러 발생 > 실패
							Debug.LogError("AnyPortrait : Duplicating the mesh failed. Edges cannot be duplicated. The mesh's data was not generated normally. Please try again.");
							return newMesh;
						}

						//이제 이 버텍스들을 잇는 Edge를 생성하자
						newEdge = newMesh.MakeNewEdge(newVert_1, newVert_2, false);
						if (newEdge == null)
						{
							//Edge가 생성되지 않았다;
							//에러 발생 > 실패
							Debug.LogError("AnyPortrait : Duplicating the mesh failed. Edges cannot be duplicated. The mesh's data was not generated normally. Please try again.");
							return newMesh;
						}

						srcEdge2NewEdge.Add(srcEdge, newEdge);
					}
				}

				//6. Polygon 복사
				int nPolygons = srcMesh._polygons == null ? 0 : srcMesh._polygons.Count;
				if (nPolygons > 0)
				{
					apMeshPolygon srcPolygon = null;
					apMeshPolygon newPolygon = null;

					List<apVertex> srcVertList = null;
					List<apVertex> newVertList = new List<apVertex>();

					List<apMeshEdge> srcEdgeList = null;
					List<apMeshEdge> newEdgeList = new List<apMeshEdge>();

					List<apMeshEdge> srcHiddenEdgeList = null;
					List<apMeshEdge> newHiddenEdgeList = new List<apMeshEdge>();

					for (int iPoly = 0; iPoly < nPolygons; iPoly++)
					{
						srcPolygon = srcMesh._polygons[iPoly];
						newPolygon = null;

						srcVertList = null;
						newVertList.Clear();

						srcEdgeList = null;
						newEdgeList.Clear();

						srcHiddenEdgeList = null;
						newHiddenEdgeList.Clear();

						srcVertList = srcPolygon._verts;
						srcEdgeList = srcPolygon._edges;
						srcHiddenEdgeList = srcPolygon._hidddenEdges;

						//Src로 부터 복사를 하자
						int nSrcVert = srcVertList == null ? 0 : srcVertList.Count;
						int nSrcEdge = srcEdgeList == null ? 0 : srcEdgeList.Count;
						int nSrcHiddenEdge = srcHiddenEdgeList == null ? 0 : srcHiddenEdgeList.Count;

						for (int iSrcVert = 0; iSrcVert < nSrcVert; iSrcVert++)
						{
							//Src Vert > New Vert로 복사
							newVertList.Add(srcVert2NewVert[srcVertList[iSrcVert]]);
						}

						for (int iSrcEdge = 0; iSrcEdge < nSrcEdge; iSrcEdge++)
						{
							//Src Edge > New Edge로 복사
							newEdgeList.Add(srcEdge2NewEdge[srcEdgeList[iSrcEdge]]);
						}

						apMeshEdge srcHiddenEdge = null;
						apMeshEdge newHiddenEdge = null;

						apVertex newHiddenVert_1 = null;
						apVertex newHiddenVert_2 = null;
						for (int iSrcHiddenEdge = 0; iSrcHiddenEdge < nSrcHiddenEdge; iSrcHiddenEdge++)
						{
							//Src Edge > New Edge로 복사
							//직접적인 변환 테이블이 없으므로, Vert를 이용해서 만든뒤 추가해야한다.
							srcHiddenEdge = srcHiddenEdgeList[iSrcHiddenEdge];
							newHiddenVert_1 = srcVert2NewVert[srcHiddenEdge._vert1];
							newHiddenVert_2 = srcVert2NewVert[srcHiddenEdge._vert2];

							//Hidden Edge 생성
							newHiddenEdge = new apMeshEdge(newHiddenVert_1, newHiddenVert_2);
							newHiddenEdge._isHidden = true;

							newHiddenEdgeList.Add(newHiddenEdge);
						}

						//폴리곤을 만들고 데이터를 넣자
						newPolygon = new apMeshPolygon();
						newPolygon.SetVertexAndEdges(newVertList, newEdgeList);
						bool isValidPolygon = newPolygon.SetHiddenEdgesAndMakeTri(newHiddenEdgeList);

						if (!isValidPolygon)
						{
							//폴리곤이 생성되지 않았다;
							//에러 발생
							Debug.LogWarning("AnyPortrait : Polygon cannot be duplicated normally.");

							//자동으로 걍 생성하자
							newPolygon.MakeHiddenEdgeAndTri();
						}
						//폴리곤 추가
						newMesh._polygons.Add(newPolygon);
					}
				}

				//7. 그외 데이터 대입
				newMesh._portrait = srcMesh._portrait;
				newMesh._offsetPos = srcMesh._offsetPos;
				newMesh._isPSDParsed = srcMesh._isPSDParsed;
				newMesh._atlasFromPSD_LT = srcMesh._atlasFromPSD_LT;
				newMesh._atlasFromPSD_RB = srcMesh._atlasFromPSD_RB;
				newMesh._mirrorAxis = srcMesh._mirrorAxis;
				newMesh._isMirrorX = srcMesh._isMirrorX;

				//8. 핀 복제 (v1.4.0)
				if(newMesh._pinGroup == null)
				{
					//일단 생성
					newMesh._pinGroup = new apMeshPinGroup();					
				}
				newMesh._pinGroup.Clear();

				//복제
				if(srcMesh._pinGroup != null)
				{
					apMeshPinGroup srcPinGroup = srcMesh._pinGroup;
					apMeshPinGroup newPinGroup = newMesh._pinGroup;

					int nSrcPins = srcPinGroup._pins_All != null ? srcPinGroup._pins_All.Count : 0;
					int nSrcCurve = srcPinGroup._curves_All != null ? srcPinGroup._curves_All.Count : 0;
					
					Dictionary<apMeshPin, apMeshPin> srcPin2NewPin = new Dictionary<apMeshPin, apMeshPin>();

					//(1) 핀 복제
					if(nSrcPins > 0)
					{
						apMeshPin srcPin = null;
						apMeshPin newPin = null;
						for (int i = 0; i < nSrcPins; i++)
						{
							srcPin = srcPinGroup._pins_All[i];

							int nextUniqueID = Editor.Select.Portrait.MakeUniqueID(apIDManager.TARGET.MeshPin);
							newPin = newPinGroup.AddMeshPin(nextUniqueID, Vector2.zero, null, srcPin._range, srcPin._fade);
							
							newPin._defaultPos = srcPin._defaultPos;
							newPin._tangentType = srcPin._tangentType;

							srcPin2NewPin.Add(srcPin, newPin);
						}
					}

					//(2) 커브 복제 (연결)
					if(nSrcCurve > 0)
					{
						apMeshPinCurve srcCurve = null;
						apMeshPin srcPin_A = null;
						apMeshPin srcPin_B = null;

						apMeshPin newPin_A = null;
						apMeshPin newPin_B = null;
						for (int i = 0; i < nSrcCurve; i++)
						{
							srcCurve = srcPinGroup._curves_All[i];
							srcPin_A = srcCurve._prevPin;
							srcPin_B = srcCurve._nextPin;

							if(srcPin_A == null || srcPin_B == null)
							{
								continue;
							}
							
							newPin_A = srcPin2NewPin[srcPin_A];
							newPin_B = srcPin2NewPin[srcPin_B];

							if(newPin_A == null || newPin_B == null)
							{
								continue;
							}

							newPinGroup.LinkPins(newPin_A, newPin_B);
						}
					}

					newMesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
				}

				//Undo - Duplicate
				apEditorUtil.SetRecordCreateMonoObject(newMesh, "Duplicate Mesh");

				//메시가 추가되었다.
				Editor.OnAnyObjectAddedOrRemoved();

				Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));


				//선택하기 [v1.5.0]
				Editor.Select.SelectMesh(newMesh);

				//v1.5.0 : 복제시 원본의 다음에 위치시키도록 하자 (Refresh Hierarchy보다 먼저 호출해야함)
				if(Editor._portrait._objectOrders == null)
				{
					Editor._portrait._objectOrders = new apObjectOrders();
				}
				Editor._portrait._objectOrders.Sync(Editor._portrait);
				Editor._portrait._objectOrders.SetOrderToNext(	Editor._portrait,
																apObjectOrders.OBJECT_TYPE.Mesh,
																newMesh._uniqueID,//복제된 대상의 ID
																srcMesh._uniqueID);//원본의 ID


				//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
				Editor.RefreshControllerAndHierarchy(true);

				Editor.Notification(srcMesh._name + " > " + newMesh._name + " Duplicated", true, false);

				return newMesh;
			}
			catch (Exception ex)
			{
				Debug.LogError("AnyPortrait : Duplicating the mesh failed. (Exception : " + ex + ")");
				return null;
			}
		}


		/// <summary>
		/// 메시의 미러 복사를 한다.
		/// </summary>
		public void DuplicateMirrorVertices()
		{
			if (Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor._meshEditMirrorMode != apEditor.MESH_EDIT_MIRROR_MODE.Mirror
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null)
			{
				return;
			}
			int nVert = Editor.VertController.Vertices != null ? Editor.VertController.Vertices.Count : 0;
			if (nVert == 0)
			{
				return;
			}

			//Mirror Set을 리셋한다.
			Editor.MirrorSet.Refresh(Editor.Select.Mesh, true);

			if (Editor.MirrorSet._cloneVerts.Count == 0)
			{
				return;
			}
			apMesh mesh = Editor.Select.Mesh;
			apTextureData textureData = Editor.Select.Mesh.LinkedTextureData;

			apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_VertexCopied, 
											Editor, 
											mesh, 
											//mesh,
											false,
											apEditorUtil.UNDO_STRUCT.ValueOnly);

			//Vector2 meshOffset = mesh._offsetPos;
			//Vector2 imageHalfOffset = new Vector2(textureData._width * 0.5f, textureData._height * 0.5f);

			List<apMirrorVertexSet.CloneVertex> cloneVerts = Editor.MirrorSet._cloneVerts;
			List<apMirrorVertexSet.CloneVertex> crossVerts = Editor.MirrorSet._crossVerts;
			List<apMirrorVertexSet.CloneEdge> cloneEdges = Editor.MirrorSet._cloneEdges;


			Dictionary<apMirrorVertexSet.CloneVertex, apVertex> clone2Vert = new Dictionary<apMirrorVertexSet.CloneVertex, apVertex>();
			Dictionary<apVertex, apMirrorVertexSet.CloneVertex> vert2Clone = new Dictionary<apVertex, apMirrorVertexSet.CloneVertex>();

			apMirrorVertexSet.CloneVertex curCloneVert = null;
			apVertex mirrorVert = null;



			//1. Clone Vert를 모두 생성한다. (CrossVert 포함)
			// (isOnAxis인 경우, 새로 생성하지는 않고 위치만 변경한다. Dictionary엔 추가)
			for (int iClone = 0; iClone < cloneVerts.Count; iClone++)
			{
				curCloneVert = cloneVerts[iClone];
				if (curCloneVert._isOnAxis)
				{
					//위치를 보정한다.
					if (mesh._isMirrorX)
					{
						//X 보정
						curCloneVert._srcVert._pos.x = mesh._mirrorAxis.x;
					}
					else
					{
						//Y 보정
						curCloneVert._srcVert._pos.y = mesh._mirrorAxis.y;
					}
					mirrorVert = curCloneVert._srcVert;
				}
				else
				{
					//새로 생성한다.
					mirrorVert = mesh.AddVertexAutoUV(curCloneVert._pos);
				}

				clone2Vert.Add(curCloneVert, mirrorVert);
				vert2Clone.Add(mirrorVert, curCloneVert);
			}


			for (int iCross = 0; iCross < crossVerts.Count; iCross++)
			{
				curCloneVert = crossVerts[iCross];

				//Cross는 비교 없이 바로 만들면 된다.
				mirrorVert = mesh.AddVertexAutoUV(curCloneVert._pos);

				clone2Vert.Add(curCloneVert, mirrorVert);
				vert2Clone.Add(mirrorVert, curCloneVert);
			}


			//2. Cross Vert로 인해서 Edge가 쪼개져야 할 경우가 있다.
			apMirrorVertexSet.CloneEdge curCloneEdge = null;
			apMeshEdge splitEdge = null;
			apVertex curVert = null;


			for (int iCross = 0; iCross < crossVerts.Count; iCross++)
			{
				curCloneVert = crossVerts[iCross];
				curVert = clone2Vert[curCloneVert];

				splitEdge = curCloneVert._srcSplitEdge;
				if (splitEdge != null)
				{
					//기존의 Edge를 2개로 분리한다.
					//Edge 추가
					mesh.MakeNewEdge(curVert, splitEdge._vert1, false);
					mesh.MakeNewEdge(curVert, splitEdge._vert2, false);

					//기존의 Edge 삭제
					mesh.RemoveEdge(splitEdge);
				}

			}


			//3. 그 외의 CloneEdge를 실제 Edge로 만들자
			apVertex edgeVert1 = null;
			apVertex edgeVert2 = null;
			for (int iEdge = 0; iEdge < cloneEdges.Count; iEdge++)
			{
				curCloneEdge = cloneEdges[iEdge];

				edgeVert1 = clone2Vert[curCloneEdge._cloneVert1];
				edgeVert2 = clone2Vert[curCloneEdge._cloneVert2];
				mesh.MakeNewEdge(edgeVert1, edgeVert2, false);
			}

			//Mesh를 만들자
			mesh.MakeEdgesToPolygonAndIndexBuffer();
			Editor.VertController.UnselectVertex();//<<버텍스 선택은 모두 해제
			Editor.SetRepaint();

			Editor.MirrorSet.Refresh(mesh, true);

			//Pin-Weight 갱신
			//옵션이 없어도 무조건 Weight 갱신
			if(mesh._pinGroup != null)
			{
				mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
			}
		}


		//버텍스를 정렬한다.
		public enum VERTEX_ALIGN_REQUEST
		{
			MinX,
			CenterX,
			MaxX,
			MinY,
			CenterY,
			MaxY,
			DistributeX,
			DistributeY

		}
		public void AlignVertices(VERTEX_ALIGN_REQUEST alignType)
		{
			if (Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return;
			}

			if (Editor.Gizmos.IsFFDMode)
			{
				//FFD 모드에서는 처리가 안된다.
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if (vertices.Count <= 0 || selectedVertices.Count <= 1)
			{
				return;
			}

			apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_VertexMoved, 
											Editor, 
											mesh, 
											//mesh, 
											true,
											apEditorUtil.UNDO_STRUCT.ValueOnly);

			apVertex vert = null;
			float minX = 0.0f;
			float maxX = 0.0f;
			float minY = 0.0f;
			float maxY = 0.0f;

			//먼저 범위를 정한다.
			//Pos는 MeshOffset을 적용하지 않는다.
			vert = selectedVertices[0];
			minX = vert._pos.x;
			maxX = vert._pos.x;
			minY = vert._pos.y;
			maxY = vert._pos.y;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				vert = selectedVertices[i];

				minX = Mathf.Min(minX, vert._pos.x);
				maxX = Mathf.Max(maxX, vert._pos.x);
				minY = Mathf.Min(minY, vert._pos.y);
				maxY = Mathf.Max(maxY, vert._pos.y);
			}
			float centerX = minX * 0.5f + maxX * 0.5f;
			float centerY = minY * 0.5f + maxY * 0.5f;


			switch (alignType)
			{
				case VERTEX_ALIGN_REQUEST.MinX:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.x = minX;
					}
					break;

				case VERTEX_ALIGN_REQUEST.CenterX:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.x = centerX;
					}
					break;

				case VERTEX_ALIGN_REQUEST.MaxX:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.x = maxX;
					}
					break;

				case VERTEX_ALIGN_REQUEST.MinY:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.y = minY;
					}
					break;

				case VERTEX_ALIGN_REQUEST.CenterY:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.y = centerY;
					}
					break;

				case VERTEX_ALIGN_REQUEST.MaxY:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.y = maxY;
					}
					break;

				case VERTEX_ALIGN_REQUEST.DistributeX:
				case VERTEX_ALIGN_REQUEST.DistributeY:
					//일단 새로운 리스트를 만들어서 X, 또는 Y 순으로 Sort한다.
					{
						List<apVertex> sortedVerts = new List<apVertex>();
						for (int i = 0; i < selectedVertices.Count; i++)
						{
							sortedVerts.Add(selectedVertices[i]);
						}

						if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
						{
							//X 좌표로 정렬 (올림차순)
							sortedVerts.Sort(delegate (apVertex a, apVertex b)
							{
								return (int)((a._pos.x - b._pos.x) * 1000.0f);
							});
						}
						else
						{
							//Y 좌표로 정렬 (올림차순)
							sortedVerts.Sort(delegate (apVertex a, apVertex b)
							{
								return (int)((a._pos.y - b._pos.y) * 1000.0f);
							});
						}

						for (int i = 0; i < sortedVerts.Count; i++)
						{
							vert = sortedVerts[i];
							if (i == 0)
							{
								if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
								{
									vert._pos.x = minX;
								}
								else
								{
									vert._pos.y = minY;
								}
							}
							else if (i == sortedVerts.Count - 1)
							{
								if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
								{
									vert._pos.x = maxX;
								}
								else
								{
									vert._pos.y = maxY;
								}
							}
							else
							{
								float lerp = (float)i / (float)(sortedVerts.Count - 1);
								if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
								{
									vert._pos.x = minX * (1.0f - lerp) + maxX * lerp;
								}
								else
								{
									vert._pos.y = minY * (1.0f - lerp) + maxY * lerp;
								}
							}
						}

					}
					break;

			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}
		}





		//추가 20.7.6 : 만약 메시가 PSD 파일로부터 열린 경우, 버텍스 리셋이 필요할 수 있다.
		//이 경우에는 바로 리셋을 하고 MakeMesh탭을 열어주자
		//에디터 옵션이나 다이얼로그 결과에 따라 기능이 실행되지 않을 수 있는데, 일단 이 함수가 호출되면
		//이 메시에 대해서는 더이상 물어보지 않는다.
		public void AskRemoveVerticesIfImportedFromPSD(apMesh mesh)
		{
			if (!mesh._isNeedToAskRemoveVertByPSDImport)
			{
				return;
			}
			//결과에 상관없이 이 변수는 false로 강제
			mesh._isNeedToAskRemoveVertByPSDImport = false;

			//일단 에디터는 Dirty 호출
			apEditorUtil.SetDirty(_editor);

			//에디터에서 옵션이 없다면 패스
			if (!Editor._isNeedToAskRemoveVertByPSDImport)
			{
				return;
			}

			//메시의 버텍스가 4개가 아니라면 뭔가 편집했을 것이므로 예외
			int nVert = mesh._vertexData != null ? mesh._vertexData.Count : 0;
			if (nVert != 4)
			{
				return;
			}

			//물어보자
			int iBtn = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.RemoveMeshVertices_Title),
														Editor.GetText(TEXT.DLG_AskRemoveVerticesImportedFromPSD_Body),
														Editor.GetText(TEXT.Remove),
														Editor.GetUIWord(UIWORD.QuickGenerate),
														Editor.GetText(TEXT.Ignore));

			if (iBtn == 0)
			{
				//Undo
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_RemoveAllVertices, 
												Editor, 
												mesh, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				mesh._vertexData.Clear();
				mesh._indexBuffer.Clear();
				mesh._edges.Clear();
				mesh._polygons.Clear();

				mesh.MakeEdgesToPolygonAndIndexBuffer();


				//Pin-Weight 갱신
				//옵션이 없어도 무조건 Weight 갱신
				if(mesh._pinGroup != null)
				{
					mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
				}

				Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

				Editor.VertController.UnselectVertex();
				Editor.VertController.UnselectNextVertex();

				//Make Mesh Tab으로 전환하자
				Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
			}
			else if(iBtn == 1)
			{
				//추가 21.3.4 : PSD 파일에서 처음 열때 바로 메시 생성 가능
				//Generate 버튼
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_AutoGen, 
												Editor, 
												mesh, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				if (mesh._vertexData.Count > 0)
				{
					//바로 삭제
					mesh._vertexData.Clear();
					mesh._indexBuffer.Clear();
					mesh._edges.Clear();
					mesh._polygons.Clear();

					mesh.MakeEdgesToPolygonAndIndexBuffer();

					//Pin-Weight 갱신
					//옵션이 없어도 무조건 Weight 갱신
					if(mesh._pinGroup != null)
					{
						mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
					}

					Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

					Editor.VertController.UnselectVertex();
					Editor.VertController.UnselectNextVertex();
				}

				//프리셋 값을 이용하자
				//QuickGenerate에서는 프리셋에 따라 다르다
				bool preset_IsInnerMargin = false;
				int preset_Density = 1;
				int preset_InnerMargin = 5;
				int preset_OuterMargin = 10;
				switch (Editor._meshAutoGenV2Option_QuickPresetType)
				{
					case 0://Simple
						preset_IsInnerMargin = false;
						preset_Density = 1;
						preset_InnerMargin = 1;
						preset_OuterMargin = 5;
						break;

					case 1://Moderate
						preset_IsInnerMargin = true;
						preset_Density = 2;
						preset_InnerMargin = 5;
						preset_OuterMargin = 10;
						break;

					case 2://Complex
						preset_IsInnerMargin = true;
						preset_Density = 5;
						preset_InnerMargin = 5;
						preset_OuterMargin = 10;
						break;
				}


				Editor.MeshGeneratorV2.ReadyToRequest(Editor.Select.OnMeshAutoGeneratedV2);
				Editor.MeshGeneratorV2.AddRequest(	mesh,
													preset_Density,
													preset_OuterMargin,
													preset_InnerMargin,
													preset_IsInnerMargin);
				Editor.MeshGeneratorV2.StartGenerate();//시작!

				//프로그래스바도 출현
				Editor.StartProgressPopup("Mesh Generation", "Generating..", true, Editor.Select.OnAutoGenProgressCancel);
			}

			
		}


		//---------------------------------------------------------------------------------
		// Control Param 추가 / 제거 / 편집
		//---------------------------------------------------------------------------------
		public void AddParam()
		{
			//Undo - Add Param
			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Main_AddParam, 
												Editor, 
												Editor._portrait, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.StructChanged);


			//int iNextIndex = Editor._portrait.MakeUniqueID_ControlParam();
			int iNextIndex = 0;
			while (true)
			{
				apControlParam existParam = Editor.ParamControl.FindParam("New Param (" + iNextIndex + ")");
				if (existParam != null)
				{
					iNextIndex++;
				}
				else
				{
					break;
				}
			}
			string strNewName = "New Param (" + iNextIndex + ")";

			Editor.ParamControl.Ready(Editor._portrait);
			apControlParam newParam = Editor.ParamControl.AddParam_Float(strNewName, false, apControlParam.CATEGORY.Etc, 0.0f);
			if (newParam == null)
			{
				EditorUtility.DisplayDialog(
									Editor.GetText(TEXT.DemoLimitation_Title),
									Editor.GetText(TEXT.DemoLimitation_Body_AddParam),
									Editor.GetText(TEXT.Okay)
									);
				return;
			}
			newParam.Ready(Editor._portrait);
			if (newParam != null)
			{
				Editor.Select.SelectControlParam(newParam);
			}

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);

			//Param Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Param, true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();
		}



		//추가 21.6.13 : 파라미터 복사하기
		public apControlParam DuplicateParam(apControlParam srcParam)
		{
			if(srcParam == null)
			{
				return null;
			}

			//Undo - Add Param
			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.ControlParam_Duplicated, 
												Editor, 
												Editor._portrait, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.StructChanged);


			//int iNextIndex = Editor._portrait.MakeUniqueID_ControlParam();
			string nextName = srcParam._keyName + " Copy";

			if(Editor.ParamControl.FindParam(nextName) != null)
			{
				//"기존 이름 + Copy"가 겹친다. 숫자를 넣어서 겹치지 않는 이름을 찾자
				int iNextIndex = 1;

				while (true)
				{
					nextName = srcParam._keyName + " Copy (" + iNextIndex + ")";
					apControlParam existParam = Editor.ParamControl.FindParam(nextName);
					if (existParam != null)
					{
						iNextIndex++;
					}
					else
					{
						break;
					}
				}
			}

			
			Editor.ParamControl.Ready(Editor._portrait);
			apControlParam newParam = Editor.ParamControl.AddParam_Float(nextName, false, apControlParam.CATEGORY.Etc, 0.0f);
			if (newParam == null)
			{
				EditorUtility.DisplayDialog(
									Editor.GetText(TEXT.DemoLimitation_Title),
									Editor.GetText(TEXT.DemoLimitation_Body_AddParam),
									Editor.GetText(TEXT.Okay)
									);
				return null;
			}

			newParam.Ready(Editor._portrait);


			newParam.CopyFromControlParamWithoutName(srcParam);


			//if (newParam != null)
			//{
			//	Editor.Select.SetParam(newParam);
			//}

			//v1.5.0 : 복제시 원본의 다음에 위치시키도록 하자 (Refresh Hierarchy보다 먼저 호출해야함)
			if(Editor._portrait._objectOrders == null)
			{
				Editor._portrait._objectOrders = new apObjectOrders();
			}
			Editor._portrait._objectOrders.Sync(Editor._portrait);
			Editor._portrait._objectOrders.SetOrderToNext(	Editor._portrait,
															apObjectOrders.OBJECT_TYPE.ControlParam,
															newParam._uniqueID,//복제된 대상의 ID
															srcParam._uniqueID);//원본의 ID

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);

			//Param Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Param, true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newParam;
		}





		public void ChangeParamName(apControlParam cParam, string strNextName)
		{
			if (Editor._portrait == null)
			{
				return;
			}


			//string prevName = cParam._keyName;

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.ControlParam_SettingChanged, 
												Editor, 
												Editor._portrait, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			
			cParam._keyName = strNextName;
			Editor.RefreshControllerAndHierarchy(false);
		}

		public void RemoveParam(apControlParam cParam)
		{
			if (Editor.Select.Param == cParam)
			{
				Editor.Select.SelectNone();
			}

			//apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Main_RemoveParam, Editor, Editor._portrait, cParam, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Control Parameter");

			int removedParamID = cParam._uniqueID;

			//Editor._portrait.PushUniqueID_ControlParam(removedParamID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.ControlParam, removedParamID);

			Editor.ParamControl._controlParams.Remove(cParam);

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

			//Editor.Hierarchy.RefreshUnits();
			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);
		}


		/// <summary>
		/// 여러개의 컨트롤 파라미터들을 초기화한다. (21.10.10)
		/// </summary>
		/// <param name="cParams"></param>
		public void RemoveParams(List<apControlParam> cParams)
		{
			if(cParams == null || cParams.Count == 0)
			{
				return;
			}

			if(Editor.Select.Param != null
				&& cParams.Contains(Editor.Select.Param))
			{
				Editor.Select.SelectNone();
			}

			//apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Main_RemoveParam, Editor, Editor._portrait, cParam, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Control Parameter");

			int nParams = cParams.Count;
			apControlParam curParam = null;
			for (int iParam = 0; iParam < nParams; iParam++)
			{
				curParam = cParams[iParam];

				int removedParamID = curParam._uniqueID;

				Editor._portrait.PushUnusedID(apIDManager.TARGET.ControlParam, removedParamID);
				Editor.ParamControl._controlParams.Remove(curParam);
			}

			

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

			//Editor.Hierarchy.RefreshUnits();
			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(true);
		}




		



		public void SetControlParamPreset(apControlParam cParam, apControlParamPresetUnit preset)
		{
			if (cParam == null || preset == null)
			{
				return;
			}
			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.ControlParam_SettingChanged, 
												Editor, 
												Editor._portrait, 
												//cParam,
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			//이름이 겹치는 건 피해야한다.
			string nextName = preset._keyName;
			bool isNameOverwrite = false;
			//동일한 이름이 있다면 메시지로 알려주자
			if (Editor.ParamControl.FindParam(nextName) != null)
			{
				//이름이 겹친다.
				isNameOverwrite = true;
			}

			if (!isNameOverwrite)
			{
				//이름 적용
				cParam._keyName = preset._keyName;
			}
			cParam._category = preset._category;
			cParam._iconPreset = preset._iconPreset;
			cParam._valueType = preset._valueType;

			cParam._int_Def = preset._int_Def;
			cParam._float_Def = preset._float_Def;
			cParam._vec2_Def = preset._vec2_Def;

			cParam._int_Min = preset._int_Min;
			cParam._int_Max = preset._int_Max;
			cParam._float_Min = preset._float_Min;
			cParam._float_Max = preset._float_Max;
			cParam._vec2_Min = preset._vec2_Min;
			cParam._vec2_Max = preset._vec2_Max;

			cParam._label_Min = preset._label_Min;
			cParam._label_Max = preset._label_Max;
			cParam._snapSize = preset._snapSize;

			//추가 v1.4.5
			cParam._isNonInterpolatedIndex = preset._isNonInterpolatedIndex;

			Editor.RefreshControllerAndHierarchy(false);

			if (isNameOverwrite)
			{
				//이름이 중복되었음을 알려준다.
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamPreset_NameOverwrite_Title),
											Editor.GetText(TEXT.ControlParamPreset_NameOverwrite_Body),
											Editor.GetText(TEXT.Okay));
			}
		}

		//-------------------------------------------------------------------------------

		public apAnimClip AddAnimClip(bool isSetRecord = true, bool isRefresh = true)
		{
			if (isSetRecord)
			{
				//Undo - Add Animation
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Main_AddAnimation, 
													Editor, 
													Editor._portrait, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}

			//int iNextIndex = Editor._portrait.MakeUniqueID_AnimClip();
			int iNextIndex = 0;
			//int iNextUniqueID = Editor._portrait.MakeUniqueID_AnimClip();
			int iNextUniqueID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimClip);

			if (iNextUniqueID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Anim Clip Creating Failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimCreateFailed_Title),
												Editor.GetText(TEXT.AnimCreateFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}

			while (true)
			{
				bool isExist = Editor._portrait._animClips.Exists(delegate (apAnimClip a)
				{
					return a._name.Equals("New AnimClip (" + iNextIndex + ")");
				});

				if (isExist)
				{
					iNextIndex++;
				}
				else
				{
					break;
				}
			}

			string strNewName = "New AnimClip (" + iNextIndex + ")";

			apAnimClip newAnimClip = new apAnimClip();
			newAnimClip.Init(Editor._portrait, strNewName, iNextUniqueID);
			newAnimClip.LinkEditor(Editor._portrait);

			Editor._portrait._animClips.Add(newAnimClip);

			//이전
			//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
			//Editor.RefreshControllerAndHierarchy();

			if (isRefresh)
			{
				//변경 19.5.21
				Editor.RefreshControllerAndHierarchy(true);

				//Animation Hierarchy Filter를 활성화한다.
				Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Animation, true);



				//v1.5.0
			}
			

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newAnimClip;
		}


		public void RemoveAnimClip(apAnimClip animClip)
		{
			//Remove - Animation
			//apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Main_RemoveAnimation, Editor, Editor._portrait, animClip, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Animation Clip");

			if (Editor.Select.AnimClip == animClip)
			{
				Editor.Select.SelectNone();
			}

			int removedAnimClipID = animClip._uniqueID;

			//Editor._portrait.PushUniqueID_AnimClip(removedAnimClipID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimClip, removedAnimClipID);

			Editor._portrait._animClips.Remove(animClip);


			//MeshGroup의 각 Modifier의 "Animated" 계열의 링크를 모두 끊어야 한다.
			//타임라인 정보를 리셋
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

			////프리팹이었다면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}


		/// <summary>
		/// 여러개의 애니메이션을 삭제한다 (21.10.10)
		/// </summary>
		/// <param name="animClip"></param>
		public void RemoveAnimClips(List<apAnimClip> animClips)
		{
			if(animClips == null || animClips.Count == 0)
			{
				return;
			}

			//Remove - Animation
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Animation Clip");

			if(Editor.Select.AnimClip != null)
			{
				if(animClips.Contains(Editor.Select.AnimClip))
				{
					Editor.Select.SelectNone();
				}
			}

			int nAnimClips = animClips.Count;
			apAnimClip curAnimClip = null;
			for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
			{
				curAnimClip = animClips[iAnimClip];

				int removedAnimClipID = curAnimClip._uniqueID;

				Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimClip, removedAnimClipID);

				Editor._portrait._animClips.Remove(curAnimClip);
			}
			//MeshGroup의 각 Modifier의 "Animated" 계열의 링크를 모두 끊어야 한다.
			//타임라인 정보를 리셋
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(true);
		}




		/// <summary>
		/// AnimClip을 복제하자
		/// </summary>
		/// <param name="srcAnimClip"></param>
		public apAnimClip DuplicateAnimClip(apAnimClip srcAnimClip)
		{
			if (apVersion.I.IsDemo)//애니메이션 복제 불가
			{
				if (Editor._portrait._animClips.Count >= 2)
				{
					//이미 2개를 넘었다.
					//복사할 수 없다.
					EditorUtility.DisplayDialog(
						Editor.GetText(TEXT.DemoLimitation_Title),
						Editor.GetText(TEXT.DemoLimitation_Body_AddAnimation),
						Editor.GetText(TEXT.Okay)
						);

					return null;
				}
			}


			//Undo - Anim Clip 복사
			apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_DupAnimClip, 
																		Editor, 
																		Editor._portrait, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.StructChanged);

			//일단 복사
			apAnimClip dupAnimClip = AddAnimClip(true);

			if (dupAnimClip == null)
			{
				return null;
			}

			//1. AnimClip의 기본 정보를 복사한다.
			string dupAnimClipName = srcAnimClip._name + " Copy";

			//중복되지 않은 이름을 찾는다.
			if (Editor._portrait._animClips.Exists(delegate (apAnimClip a)
			{ return string.Equals(dupAnimClipName, a._name); }))
			{
				//오잉 똑같은 이름이 있네염...
				int copyIndex = -1;
				for (int iCopyIndex = 1; iCopyIndex < 1000; iCopyIndex++)
				{
					if (!Editor._portrait._animClips.Exists(delegate (apAnimClip a)
					{ return string.Equals(dupAnimClipName + " (" + iCopyIndex + ")", a._name); }))
					{
						copyIndex = iCopyIndex;
						break;
					}
				}
				if (copyIndex < 0) { dupAnimClipName += " (1000+)"; }
				else { dupAnimClipName += " (" + copyIndex + ")"; }
			}


			dupAnimClip._name = dupAnimClipName;
			dupAnimClip._portrait = srcAnimClip._portrait;
			dupAnimClip._targetMeshGroupID = srcAnimClip._targetMeshGroupID;
			dupAnimClip._targetMeshGroup = srcAnimClip._targetMeshGroup;
			dupAnimClip._targetOptTranform = srcAnimClip._targetOptTranform;

			dupAnimClip.SetOption_FPS(srcAnimClip.FPS);
			dupAnimClip.SetOption_StartFrame(srcAnimClip.StartFrame);
			dupAnimClip.SetOption_EndFrame(srcAnimClip.EndFrame);
			dupAnimClip.SetOption_IsLoop(srcAnimClip.IsLoop);

			//어떤 Src로 복사할지를 연결해둔다.
			Dictionary<apAnimTimeline, apAnimTimeline> dupTimelinePairs = new Dictionary<apAnimTimeline, apAnimTimeline>();
			Dictionary<apAnimTimelineLayer, apAnimTimelineLayer> dupTimelineLayerPairs = new Dictionary<apAnimTimelineLayer, apAnimTimelineLayer>();
			Dictionary<apAnimKeyframe, apAnimKeyframe> dupKeyframePairs = new Dictionary<apAnimKeyframe, apAnimKeyframe>();


			//2. Timeline을 하나씩 복사한다.
			for (int iT = 0; iT < srcAnimClip._timelines.Count; iT++)
			{
				apAnimTimeline srcTimeline = srcAnimClip._timelines[iT];
				apAnimTimeline dupTimeline = AddAnimTimeline(srcTimeline._linkType, srcTimeline._modifierUniqueID, dupAnimClip, false, false);
				if (dupTimeline == null)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Adding Failed.", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimDuplicatedFailed_Title),
													Editor.GetText(TEXT.AnimDuplicatedFailed_Body),
													Editor.GetText(TEXT.Close));
					return null;
				}

				dupTimeline._guiColor = srcTimeline._guiColor;
				dupTimeline._linkedModifier = srcTimeline._linkedModifier;
				dupTimeline._linkedOptModifier = srcTimeline._linkedOptModifier;

				//Dup - Src 순으로 복사된 Timeline을 저장하자.
				dupTimelinePairs.Add(dupTimeline, srcTimeline);

			}

			//Sync를 한번 해두자
			AddAndSyncAnimClipToModifier(dupAnimClip);

			foreach (KeyValuePair<apAnimTimeline, apAnimTimeline> timelinePair in dupTimelinePairs)
			{
				apAnimTimeline dupTimeline = timelinePair.Key;
				apAnimTimeline srcTimeline = timelinePair.Value;

				//3. TimelineLayer를 하나씩 복사한다.
				for (int iTL = 0; iTL < srcTimeline._layers.Count; iTL++)
				{
					apAnimTimelineLayer srcLayer = srcTimeline._layers[iTL];

					int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
					if (nextLayerID < 0)
					{
						//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimDuplicatedFailed_Title),
														Editor.GetText(TEXT.AnimDuplicatedFailed_Body),
														Editor.GetText(TEXT.Close));

						return null;
					}

					apAnimTimelineLayer dupLayer = new apAnimTimelineLayer();
					dupLayer.Link(dupTimeline._parentAnimClip, dupTimeline);

					//if (dupTimeline._linkType == apAnimClip.LINK_TYPE.Bone)
					//{
					//	Debug.LogError("TODO : Bone 타입의 Timeline 복제가 구현되지 않았다.");
					//}
					dupLayer._uniqueID = nextLayerID;
					dupLayer._parentAnimClip = srcLayer._parentAnimClip;
					dupLayer._parentTimeline = srcLayer._parentTimeline;
					//dupLayer._isMeshTransform = srcLayer._isMeshTransform;

					dupLayer._linkModType = srcLayer._linkModType;

					dupLayer._transformID = srcLayer._transformID;
					dupLayer._linkedMeshTransform = srcLayer._linkedMeshTransform;
					dupLayer._linkedMeshGroupTransform = srcLayer._linkedMeshGroupTransform;
					dupLayer._linkedOptTransform = srcLayer._linkedOptTransform;
					dupLayer._guiColor = srcLayer._guiColor;
					dupLayer._targetParamSetGroup = srcLayer._targetParamSetGroup;
					dupLayer._boneID = srcLayer._boneID;
					dupLayer._controlParamID = srcLayer._controlParamID;
					dupLayer._linkedControlParam = srcLayer._linkedControlParam;
					dupLayer._linkType = srcLayer._linkType;
					dupLayer._linkedBone = srcLayer._linkedBone;//<<Bone 추가

					//Debug.LogError("TODO : Timeline Layer복사시 Linked Opt Bone도 복사할 것");


					dupTimeline._layers.Add(dupLayer);


					dupTimelineLayerPairs.Add(dupLayer, srcLayer);

					//이게 왜 필요했지????? @ㅅ@??
					////여기서 Link 한번
					//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
					//Editor._portrait.LinkAndRefreshInEditor();

					//apModifierParamSetGroup modParamSetGroup = null;
					//if (dupTimeline._linkedModifier != null)
					//{
					//	modParamSetGroup = dupTimeline._linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
					//	{
					//		return a._keyAnimTimelineLayer == dupLayer;
					//	});
					//}
				}
			}

			AddAndSyncAnimClipToModifier(dupAnimClip);

			//여기서 리프레시 한번 더
			dupAnimClip.RefreshTimelines(null, null);


			foreach (KeyValuePair<apAnimTimelineLayer, apAnimTimelineLayer> layerPair in dupTimelineLayerPairs)
			{
				apAnimTimelineLayer dupLayer = layerPair.Key;
				apAnimTimelineLayer srcLayer = layerPair.Value;

				//4. 키프레임도 복사하자.
				for (int iK = 0; iK < srcLayer._keyframes.Count; iK++)
				{
					apAnimKeyframe srcKeyframe = srcLayer._keyframes[iK];

					apAnimKeyframe dupKeyframe = AddAnimKeyframe(srcKeyframe._frameIndex, dupLayer, false, false, false, false);
					if (dupKeyframe == null)
					{
						//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimDuplicatedFailed_Title),
														Editor.GetText(TEXT.AnimDuplicatedFailed_Body),
														Editor.GetText(TEXT.Close));
						return null;
					}

					//Curkey 복사
					dupKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, srcKeyframe._frameIndex);
					dupKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;
					dupKeyframe._isActive = srcKeyframe._isActive;



					//Control Type 복사
					//dupKeyframe._conSyncValue_Bool = srcKeyframe._conSyncValue_Bool;
					dupKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
					dupKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
					dupKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;
					//dupKeyframe._conSyncValue_Vector3 = srcKeyframe._conSyncValue_Vector3;
					//dupKeyframe._conSyncValue_Color = srcKeyframe._conSyncValue_Color;

					dupKeyframe.Link(dupLayer);



					dupKeyframePairs.Add(dupKeyframe, srcKeyframe);
				}
			}

			//각 Keyframe<->Modifier Param 을 일괄 연결한 뒤에..
			AddAndSyncAnimClipToModifier(dupAnimClip);

			//여기서부터 ModMesh/ModBone 를 복사하도록 하자
			foreach (KeyValuePair<apAnimKeyframe, apAnimKeyframe> keyframePair in dupKeyframePairs)
			{
				apAnimKeyframe dupKeyframe = keyframePair.Key;
				apAnimKeyframe srcKeyframe = keyframePair.Value;

				//apAnimTimelineLayer dupLayer = dupKeyframe._parentTimelineLayer;
				//apAnimTimeline dupTimeline = dupKeyframe._parentTimelineLayer._parentTimeline;

				if (dupKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//ModMesh 복사
					apModifiedMesh srcModMesh = srcKeyframe._linkedModMesh_Editor;
					apModifiedMesh dupModMesh = dupKeyframe._linkedModMesh_Editor;


					if (srcModMesh._vertices != null && srcModMesh._vertices.Count > 0)
					{
						if (dupModMesh._vertices == null) { dupModMesh._vertices = new List<apModifiedVertex>(); }
						dupModMesh._vertices.Clear();

						apModifiedVertex srcModVert = null;
						apModifiedVertex dupModVert = null;
						for (int i = 0; i < srcModMesh._vertices.Count; i++)
						{
							srcModVert = srcModMesh._vertices[i];
							dupModVert = new apModifiedVertex();

							dupModVert._modifiedMesh = dupModMesh;
							dupModVert._vertexUniqueID = srcModVert._vertexUniqueID;
							dupModVert._vertIndex = srcModVert._vertIndex;

							dupModVert._mesh = srcModVert._mesh;
							dupModVert._vertex = srcModVert._vertex;
							dupModVert._deltaPos = srcModVert._deltaPos;

							dupModVert._overlapWeight = srcModVert._overlapWeight;

							dupModMesh._vertices.Add(dupModVert);
						}
					}

					//추가 22.3.20 [v1.4.0] Pin 복사
					if(srcModMesh._pins != null && srcModMesh._pins.Count > 0)
					{
						if(dupModMesh._pins == null) { dupModMesh._pins = new List<apModifiedPin>(); }
						dupModMesh._pins.Clear();

						apModifiedPin srcModPin = null;
						apModifiedPin dupModPin = null;
						for (int i = 0; i < srcModMesh._pins.Count; i++)
						{
							srcModPin = srcModMesh._pins[i];
							dupModPin = new apModifiedPin();

							dupModPin._modifiedMesh =	srcModPin._modifiedMesh;
							dupModPin._pinUniqueID =	srcModPin._pinUniqueID;
							dupModPin._mesh =			srcModPin._mesh;
							dupModPin._pin =			srcModPin._pin;
							dupModPin._deltaPos =		srcModPin._deltaPos;

							dupModMesh._pins.Add(dupModPin);
						}
					}

					if (dupModMesh._transformMatrix == null) { dupModMesh._transformMatrix = new apMatrix(); }
					dupModMesh._transformMatrix.SetMatrix(srcModMesh._transformMatrix, true);

					dupModMesh._meshColor = srcModMesh._meshColor;
					dupModMesh._isVisible = srcModMesh._isVisible;

					//버그 수정 19.8.23 : Extra Option이 복제 안되는 문제 수정
					dupModMesh._isExtraValueEnabled = srcModMesh._isExtraValueEnabled;
					if (dupModMesh._extraValue == null)
					{
						dupModMesh._extraValue = new apModifiedMesh.ExtraValue();
						dupModMesh._extraValue.Init();
					}

					if (srcModMesh._isExtraValueEnabled && srcModMesh._extraValue != null)
					{
						dupModMesh._extraValue._isDepthChanged = srcModMesh._extraValue._isDepthChanged;
						dupModMesh._extraValue._deltaDepth = srcModMesh._extraValue._deltaDepth;
						dupModMesh._extraValue._isTextureChanged = srcModMesh._extraValue._isTextureChanged;
						dupModMesh._extraValue._linkedTextureData = srcModMesh._extraValue._linkedTextureData;
						dupModMesh._extraValue._textureDataID = srcModMesh._extraValue._textureDataID;
						dupModMesh._extraValue._weightCutout = srcModMesh._extraValue._weightCutout;
						dupModMesh._extraValue._weightCutout_AnimPrev = srcModMesh._extraValue._weightCutout_AnimPrev;
						dupModMesh._extraValue._weightCutout_AnimNext = srcModMesh._extraValue._weightCutout_AnimNext;
					}

				}
				else if (dupKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//Mod Bone 복사
					apModifiedBone srcModBone = srcKeyframe._linkedModBone_Editor;
					apModifiedBone dupModBone = dupKeyframe._linkedModBone_Editor;

					if (dupModBone._transformMatrix == null)
					{
						dupModBone._transformMatrix = new apMatrix();
					}

					dupModBone._transformMatrix.SetMatrix(srcModBone._transformMatrix, true);
				}
			}

			foreach (KeyValuePair<apAnimTimelineLayer, apAnimTimelineLayer> layerPair in dupTimelineLayerPairs)
			{
				//DupLayer를 Sort하자.
				layerPair.Key.SortAndRefreshKeyframes();//이전


				//Debug.Log("Dup Layer Keyframe Refreshed [" + layerPair.Key.DisplayName + "] : " + nRefreshed);
			}


			//버그 수정 19.8.23 : 애니메이션 이벤트도 추가해야한다.
			if (srcAnimClip._animEvents != null && srcAnimClip._animEvents.Count > 0)
			{
				apAnimEvent dupEvent = null;
				apAnimEvent srcEvent = null;
				for (int iEvent = 0; iEvent < srcAnimClip._animEvents.Count; iEvent++)
				{
					srcEvent = srcAnimClip._animEvents[iEvent];
					dupEvent = new apAnimEvent();
					dupEvent.CopyFromAnimEvent(srcEvent);

					dupAnimClip._animEvents.Add(dupEvent);
				}

			}




			dupAnimClip.RefreshTimelines(null, null);

			//AnimClip이 추가되었다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(srcAnimClip._targetMeshGroup));

			if(dupAnimClip != null)
			{
				Editor.Select.SelectAnimClip(dupAnimClip);
			}


			//v1.5.0 : 복제시 원본의 다음에 위치시키도록 하자 (Refresh Hierarchy보다 먼저 호출해야함)
			if(Editor._portrait._objectOrders == null)
			{
				Editor._portrait._objectOrders = new apObjectOrders();
			}
			Editor._portrait._objectOrders.Sync(Editor._portrait);
			Editor._portrait._objectOrders.SetOrderToNext(	Editor._portrait,
															apObjectOrders.OBJECT_TYPE.AnimClip,
															dupAnimClip._uniqueID,//복제된 대상의 ID
															srcAnimClip._uniqueID);//원본의 ID


			//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
			Editor.RefreshControllerAndHierarchy(true);

			//Refresh 추가
			//이전
			//Editor.Select.RefreshAnimEditing(true);

			//변경 22.5.15
			Editor.Select.AutoRefreshModifierExclusiveEditing();

			Editor.Notification(srcAnimClip._name + " > " + dupAnimClip._name + " Duplicated", true, false);



			return dupAnimClip;
		}





		//추가 20.1.15 : 다른 메시그룹에 연결된 상태로 AnimClip을 복사하자
		//Migration + Duplicate가 된 형태
		public apAnimClip DuplicateAnimClipWithOtherMeshGroup(	apAnimClip srcAnimClip,
																apMeshGroup srcMeshGroup,
																apMeshGroup dstMeshGroup,
																MeshGroupDupcliateConvert convertInfo)
		{
			//TODO : AnimClip을 복사하고 dstMeshGroup과 연결하자
			if(srcAnimClip == null
				|| srcMeshGroup == null
				|| dstMeshGroup == null
				|| convertInfo == null)
			{
				return null;
			}

			apAnimClip dstAnimClip = AddAnimClip(false);
			if(dstAnimClip == null)
			{
				Debug.LogError("AnyPortrait : Duplicating Animation Clip is failed. Please try again");
				return null;
			}

			
			//값을 복사하자
			string dstAnimClipName = srcAnimClip._name + " Copy";

			//중복되지 않은 이름을 찾는다.
			if (Editor._portrait._animClips.Exists(delegate (apAnimClip a)
			{ return string.Equals(dstAnimClipName, a._name); }))
			{
				//오잉 똑같은 이름이 있네염...
				int copyIndex = -1;
				for (int iCopyIndex = 1; iCopyIndex < 1000; iCopyIndex++)
				{
					if (!Editor._portrait._animClips.Exists(delegate (apAnimClip a)
					{ return string.Equals(dstAnimClipName + " (" + iCopyIndex + ")", a._name); }))
					{
						copyIndex = iCopyIndex;
						break;
					}
				}
				if (copyIndex < 0) { dstAnimClipName += " (1000+)"; }
				else { dstAnimClipName += " (" + copyIndex + ")"; }
			}
			//기본 정보 복사
			dstAnimClip._name = dstAnimClipName;
			dstAnimClip._portrait = srcAnimClip._portrait;

			dstAnimClip._targetMeshGroup = dstMeshGroup;
			dstAnimClip._targetMeshGroupID = dstMeshGroup._uniqueID;

			dstAnimClip.SetOption_FPS(srcAnimClip.FPS);
			dstAnimClip.SetOption_StartFrame(srcAnimClip.StartFrame);
			dstAnimClip.SetOption_EndFrame(srcAnimClip.EndFrame);
			dstAnimClip.SetOption_IsLoop(srcAnimClip.IsLoop);
			
			//애니메이션 이벤트 복사
			int nSrcEvents = srcAnimClip._animEvents == null ? 0 : srcAnimClip._animEvents.Count;
			apAnimEvent srcAnimEvent = null;
			apAnimEvent dstAnimEvent = null;

			for (int iEvent = 0; iEvent < nSrcEvents; iEvent++)
			{
				srcAnimEvent = srcAnimClip._animEvents[iEvent];
				if(srcAnimEvent == null)
				{
					continue;
				}
				dstAnimEvent = new apAnimEvent();
				dstAnimEvent.CopyFromAnimEvent(srcAnimEvent);
				dstAnimClip._animEvents.Add(dstAnimEvent);
			}


			//변환 정보 등록
			convertInfo.Src2Dst_AnimClip.Add(srcAnimClip, dstAnimClip);

			//타임라인 복사 및 연결
			int nSrcTimeline = srcAnimClip._timelines == null ? 0 : srcAnimClip._timelines.Count;

			apAnimTimeline srcTimeline = null;
			apAnimTimeline dstTimeline = null;

			apModifierBase srcLinkedModifier = null;
			apModifierBase dstLinkedModifier = null;

			if(dstAnimClip._timelines == null)
			{
				dstAnimClip._timelines = new List<apAnimTimeline>();
			}

			for (int iSrcTimeline = 0; iSrcTimeline < nSrcTimeline; iSrcTimeline++)
			{
				srcTimeline = srcAnimClip._timelines[iSrcTimeline];
				if(srcTimeline == null)
				{
					continue;
				}

				srcLinkedModifier = srcTimeline._linkedModifier;
				dstLinkedModifier = null;
				if(srcLinkedModifier != null
					&& convertInfo.Src2Dst_Modifier.ContainsKey(srcLinkedModifier))
				{
					dstLinkedModifier = convertInfo.Src2Dst_Modifier[srcLinkedModifier];
				}
				if(srcTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
					&& dstLinkedModifier == null)
				{
					//모디파이어가 없어서 복사를 못한다면 패스
					Debug.LogError("AnyPortrait : Duplicating Timeline is failed. Invalid Modifier");
					continue;
				}


				//Timeline을 만들자 > AddAnimTimeline 함수대신 이걸 직접 이용
				int nextTimelineID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimeline);
				if(nextTimelineID < 0)
				{
					Debug.LogError("AnyPortrait : Duplicating Timeline is failed. Please try again");
					continue;
				}

				dstTimeline = new apAnimTimeline();
				dstTimeline._uniqueID = nextTimelineID;
				dstTimeline._guiColor = srcTimeline._guiColor;
				dstTimeline._linkType = srcTimeline._linkType;
				dstTimeline._linkedModifier = dstLinkedModifier;
				dstTimeline._modifierUniqueID = dstLinkedModifier == null ? -1 : dstLinkedModifier._uniqueID;
				dstTimeline._guiTimelineFolded = srcTimeline._guiTimelineFolded;

				//등록
				dstAnimClip._timelines.Add(dstTimeline);
				
				convertInfo.Src2Dst_Timeline.Add(srcTimeline, dstTimeline);

				//타임라인 레이어도 만들자

				int nSrcTimelineLayer = srcTimeline._layers == null ? 0 : srcTimeline._layers.Count;
				apAnimTimelineLayer srcTimelineLayer = null;
				apAnimTimelineLayer dstTimelineLayer = null;

				bool isAnimModifierTimeline = (srcTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier);

				if(dstTimeline._layers == null)
				{
					dstTimeline._layers = new List<apAnimTimelineLayer>();
				}

				for (int iSrcLayer = 0; iSrcLayer < nSrcTimelineLayer; iSrcLayer++)
				{
					srcTimelineLayer = srcTimeline._layers[iSrcLayer];
					if(srcTimelineLayer == null)
					{
						continue;
					}

					//연결 정보를 찾자
					int dstTranformID = -1;
					int dstBoneID = -1;
					int dstControlParamID = -1;

					//복사가 가능한가
					if(isAnimModifierTimeline)
					{
						switch (srcTimelineLayer._linkModType)
						{
							case apAnimTimelineLayer.LINK_MOD_TYPE.None:
								break;

							case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
								{
									apBone dstLinkedBone = null;
									if(srcTimelineLayer._linkedBone != null
										&& convertInfo.Src2Dst_Bone.ContainsKey(srcTimelineLayer._linkedBone))
									{
										dstLinkedBone = convertInfo.Src2Dst_Bone[srcTimelineLayer._linkedBone];
									}

									dstBoneID = (dstLinkedBone == null) ? -1 : dstLinkedBone._uniqueID;
								}
								break;

							case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
								{
									apTransform_Mesh dstLinkedMeshTF = null;
									if(srcTimelineLayer._linkedMeshTransform != null
										&& convertInfo.Src2Dst_MeshTransform.ContainsKey(srcTimelineLayer._linkedMeshTransform))
									{
										dstLinkedMeshTF = convertInfo.Src2Dst_MeshTransform[srcTimelineLayer._linkedMeshTransform];
									}

									dstTranformID = (dstLinkedMeshTF == null) ? -1 : dstLinkedMeshTF._transformUniqueID;
								}
								break;

							case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
								{
									apTransform_MeshGroup dstLinkedMGTF = null;
									if(srcTimelineLayer._linkedMeshGroupTransform != null
										&& convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcTimelineLayer._linkedMeshGroupTransform))
									{
										dstLinkedMGTF = convertInfo.Src2Dst_MeshGroupTransform[srcTimelineLayer._linkedMeshGroupTransform];
									}

									dstTranformID = (dstLinkedMGTF == null) ? -1 : dstLinkedMGTF._transformUniqueID;
								}
								break;
						}
					}
					else
					{
						dstControlParamID = srcTimelineLayer._controlParamID;
					}

					//모두 -1이라면 문제가 있다
					if(dstTranformID == -1 && dstBoneID == -1 && dstControlParamID == -1)
					{
						Debug.LogError("AnyPortrait : Duplicating Timeline Layer is failed. No proper target.");
						continue;
					}


					int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
					if(nextLayerID < 0)
					{
						Debug.LogError("AnyPortrait : Duplicating Timeline Layer is failed. Please try again");
						continue;
					}

					//기본 정보 복사
					dstTimelineLayer = new apAnimTimelineLayer();
					dstTimelineLayer._uniqueID = nextLayerID;
					dstTimelineLayer._linkModType = srcTimelineLayer._linkModType;

					dstTimelineLayer._transformID = dstTranformID;
					dstTimelineLayer._boneID = dstBoneID;
					dstTimelineLayer._guiColor = srcTimelineLayer._guiColor;
					dstTimelineLayer._guiLayerVisible = srcTimelineLayer._guiLayerVisible;
					dstTimelineLayer._controlParamID = dstControlParamID;
					dstTimelineLayer._linkType = srcTimelineLayer._linkType;

					//등록
					dstTimeline._layers.Add(dstTimelineLayer);
					convertInfo.Src2Dst_TimelineLayer.Add(srcTimelineLayer, dstTimelineLayer);
					

					//키프레임을 복사하자
					int nSrcKeyframes = srcTimelineLayer._keyframes == null ? 0 : srcTimelineLayer._keyframes.Count;

					apAnimKeyframe srcKeyframe = null;
					apAnimKeyframe dstKeyframe = null;

					if(dstTimelineLayer._keyframes == null)
					{
						dstTimelineLayer._keyframes = new List<apAnimKeyframe>();
					}
					for (int iKeyframe = 0; iKeyframe < nSrcKeyframes; iKeyframe++)
					{
						srcKeyframe = srcTimelineLayer._keyframes[iKeyframe];
						if(srcKeyframe == null)
						{
							continue;
						}

						int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
						if(nextKeyframeID < 0)
						{
							Debug.LogError("AnyPortrait : Duplicating Keyframe is failed. Please try again");
							continue;
						}
						
						dstKeyframe = new apAnimKeyframe();

						dstKeyframe._uniqueID = nextKeyframeID;
						dstKeyframe._frameIndex = srcKeyframe._frameIndex;
						dstKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, dstKeyframe._frameIndex);
						dstKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;
						dstKeyframe._isActive = srcKeyframe._isActive;
						dstKeyframe._isLoopAsStart = srcKeyframe._isLoopAsStart;
						dstKeyframe._isLoopAsEnd = srcKeyframe._isLoopAsEnd;
						dstKeyframe._loopFrameIndex = srcKeyframe._loopFrameIndex;
						
						dstKeyframe._prevRotationBiasMode = srcKeyframe._prevRotationBiasMode;
						dstKeyframe._nextRotationBiasMode = srcKeyframe._nextRotationBiasMode;
						dstKeyframe._prevRotationBiasCount = srcKeyframe._prevRotationBiasCount;
						dstKeyframe._nextRotationBiasCount = srcKeyframe._nextRotationBiasCount;

						dstKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
						dstKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
						dstKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;


						//등록
						dstTimelineLayer._keyframes.Add(dstKeyframe);
						convertInfo.Src2Dst_Keyframe.Add(srcKeyframe, dstKeyframe);
					}

				}
			}

			//코멘트 v1.5.0
			//- 일반적으로 복제를 하면 Hierarchy에서 원본의 바로 아래에 위치시키는데,
			//- 여기선 메시 그룹 복제에 의해서 애니메이션이 복제된 것이어서 굳이 그럴 필요는 없다.


			return dstAnimClip;


		}








		public void ImportAnimClip(apRetarget retargetData, apMeshGroup targetMeshGroup, apAnimClip targetAnimClip, bool isMerge)
		{
			if (Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip != targetAnimClip
				|| !retargetData.IsAnimFileLoaded)
			{
				return;
			}

			//FFD 작업 중이라면 취소를 한다. [v1.4.2]
			if(Editor.Gizmos.IsFFDMode)
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}

			//Undo
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_ImportAnimClip, 
																		Editor, 
																		Editor._portrait, 
																		targetMeshGroup, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.StructChanged);


			//유효한 리스트만 정리한다. (타임라인레이어는 따로 정리해야한다.)
			List<apRetargetSubUnit> ret_Transforms = retargetData._animFile._transforms_Total.FindAll(delegate (apRetargetSubUnit a)
			{
				return a._isImported && (a._targetMeshTransform != null || a._targetMeshGroupTransform != null);
			});

			List<apRetargetSubUnit> ret_Bones = retargetData._animFile._bones_Total.FindAll(delegate (apRetargetSubUnit a)
			{
				return a._isImported && a._targetBone != null;
			});

			List<apRetargetControlParam> ret_ControlParams = retargetData._animFile._controlParams.FindAll(delegate (apRetargetControlParam a)
			{
				return a._isImported && a._targetControlParam != null;
			});

			List<apRetargetTimelineUnit> ret_Timelines = retargetData._animFile._timelineUnits.FindAll(delegate (apRetargetTimelineUnit a)
			{
				return a._isImported && a._targetTimeline != null;
			});

			List<apRetargetAnimEvent> ret_Events = retargetData._animFile._animEvents.FindAll(delegate (apRetargetAnimEvent a)
			{
				return a._isImported;
			});

			//0. AnimClip 기본 설정을 적용
			//1. 작업해야할 타임라인을 찾자
			//2. 타임라인의 레이어를 생성 또는 연결한다.
			// - 존재하는 타임라인 레이어라면 키프레임을 모두 삭제한다. (Merge가 아니라면)
			// - 타임라인, 타임라인 레이어의 기본 객체는 ret 리스트를 참조한다. 실패했다면 참조하지 않는다.
			//3. 타임라인의 키프레임들을 생성한다.
			//4. 생성된 키프레임의 modMesh, modBone을 생성+Link하고 값을 넣는다.
			//5. Event를 넣자

			targetAnimClip.SetOption_FPS(retargetData.AnimFile._FPS);
			targetAnimClip.SetOption_StartFrame(retargetData.AnimFile._startFrame);
			targetAnimClip.SetOption_EndFrame(retargetData.AnimFile._endFrame);
			targetAnimClip.SetOption_IsLoop(retargetData.AnimFile._isLoop);



			for (int iT = 0; iT < ret_Timelines.Count; iT++)
			{
				apRetargetTimelineUnit ret_TimelineUnit = ret_Timelines[iT];
				apAnimTimeline targetTimeline = ret_TimelineUnit._targetTimeline;

				List<apRetargetTimelineLayerUnit> ret_Layers = ret_TimelineUnit._layerUnits;
				for (int iLayer = 0; iLayer < ret_Layers.Count; iLayer++)
				{
					//레이어가 존재하고 있다면 기존의 키프레임을 지우고 참조
					//없다면 새로 만든다.

					apRetargetTimelineLayerUnit ret_LayerUnit = ret_Layers[iLayer];


					if (ret_TimelineUnit._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						//ControlParam 타입인 경우
						int srcControlParamID = ret_LayerUnit._controlParamID;
						//여기에 해당하는 실제 ControlParamID를 찾자
						apRetargetControlParam targetUnit = ret_ControlParams.Find(delegate (apRetargetControlParam a)
						{
							return a._controlParamUniqueID == srcControlParamID;
						});

						if (targetUnit == null)
						{
							//엥.. 매칭된게 없네요.
							//Import가 안되었나.
							//Debug.LogError("No Imported Control Param : " + ret_LayerUnit._displayName);
							continue;
						}

						//레이어에 연결된 ControlParam
						apControlParam targetControlParam = targetUnit._targetControlParam;

						//기존의 레이어가 있는가
						apAnimTimelineLayer targetTimelineLayer = targetTimeline.GetTimelineLayer(targetControlParam);
						if (targetTimelineLayer != null)
						{
							//<1> 기존 레이어가 있다.
							if (!isMerge)
							{
								//만약 Merge가 아닌 Replace라면 기존의 키프레임을 모두 지워야함
								List<apAnimKeyframe> prevKeyframes = new List<apAnimKeyframe>();
								for (int iKey = 0; iKey < targetTimelineLayer._keyframes.Count; iKey++)
								{
									prevKeyframes.Add(targetTimelineLayer._keyframes[iKey]);
								}
								RemoveKeyframes(prevKeyframes, false);
							}
						}
						else
						{
							//<2> 레이어를 새로 만들어야 한다.
							targetTimelineLayer = AddAnimTimelineLayer(targetControlParam, targetTimeline, false);
							if (targetTimelineLayer == null)
							{
								Debug.LogError("TImelineLayer Add Error");
								continue;
							}
						}

						//타겟 타임라인 레이어에 Keyframe을 채워넣어야 한다.
						List<apRetargetKeyframeUnit> ret_Keyframes = ret_LayerUnit._keyframeUnits;
						for (int iKey = 0; iKey < ret_Keyframes.Count; iKey++)
						{
							//키프레임을 추가한다.
							apRetargetKeyframeUnit keyUnit = ret_Keyframes[iKey];

							int frameIndex = keyUnit._frameIndex;

							//만약 겹치는 Keyframe이 있다면 삭제
							apAnimKeyframe overlapKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(frameIndex);
							if (overlapKeyframe != null)
							{
								RemoveKeyframe(overlapKeyframe, false);
							}

							apAnimKeyframe addedKeyframe = AddAnimKeyframe(frameIndex, targetTimelineLayer, false, false, false, false);
							if (addedKeyframe == null)
							{
								//Debug.LogError("Keyframe 생성 실패");
								continue;
							}

							//설정값을 복사하자
							addedKeyframe._isKeyValueSet = keyUnit._isKeyValueSet;
							addedKeyframe._isActive = keyUnit._isActive;

							addedKeyframe._isLoopAsStart = keyUnit._isLoopAsStart;
							addedKeyframe._isLoopAsEnd = keyUnit._isLoopAsEnd;
							addedKeyframe._loopFrameIndex = keyUnit._loopFrameIndex;

							addedKeyframe._activeFrameIndexMin = keyUnit._activeFrameIndexMin;
							addedKeyframe._activeFrameIndexMax = keyUnit._activeFrameIndexMax;

							addedKeyframe._activeFrameIndexMin_Dummy = keyUnit._activeFrameIndexMin_Dummy;
							addedKeyframe._activeFrameIndexMax_Dummy = keyUnit._activeFrameIndexMax_Dummy;

							if (addedKeyframe._curveKey != null)
							{
								addedKeyframe._curveKey = new apAnimCurve();
							}
							addedKeyframe._curveKey._prevTangentType = keyUnit._curve_PrevTangentType;
							addedKeyframe._curveKey._prevSmoothX = keyUnit._curve_PrevSmoothX;
							addedKeyframe._curveKey._prevSmoothY = keyUnit._curve_PrevSmoothY;
							addedKeyframe._curveKey._nextTangentType = keyUnit._curve_NextTangentType;
							addedKeyframe._curveKey._nextSmoothX = keyUnit._curve_NextSmoothX;
							addedKeyframe._curveKey._nextSmoothY = keyUnit._curve_NextSmoothY;

							addedKeyframe._conSyncValue_Int = keyUnit._conSyncValue_Int;
							addedKeyframe._conSyncValue_Float = keyUnit._conSyncValue_Float;
							addedKeyframe._conSyncValue_Vector2 = keyUnit._conSyncValue_Vector2;
						}


					}
					else if (ret_TimelineUnit._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						//Animated Modifier인 경우
						apAnimTimelineLayer.LINK_MOD_TYPE linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.None;
						int srcTransformID = -1;
						int srcBoneID = -1;
						apRetargetSubUnit targetUnit = null;

						if (ret_LayerUnit._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform)
						{
							linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform;
							srcTransformID = ret_LayerUnit._transformID;

							targetUnit = ret_Transforms.Find(delegate (apRetargetSubUnit a)
							{
								return a._type == apRetargetSubUnit.TYPE.MeshTransform &&
										a._uniqueID == srcTransformID;
							});
						}
						else if (ret_LayerUnit._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform)
						{
							linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform;
							srcTransformID = ret_LayerUnit._transformID;

							targetUnit = ret_Transforms.Find(delegate (apRetargetSubUnit a)
							{
								return a._type == apRetargetSubUnit.TYPE.MeshGroupTransform &&
										a._uniqueID == srcTransformID;
							});
						}
						else if (ret_LayerUnit._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.Bone)
						{
							linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.Bone;
							srcBoneID = ret_LayerUnit._boneID;

							targetUnit = ret_Bones.Find(delegate (apRetargetSubUnit a)
							{
								return a._type == apRetargetSubUnit.TYPE.Bone &&
										a._uniqueID == srcBoneID;
							});
						}
						else
						{
							Debug.LogError("Wrong Link Mod Type");
							continue;
						}


						//여기에 해당하는 실제 SubUnit을 찾자


						if (targetUnit == null)
						{
							//엥.. 매칭된게 없네요.
							//Import가 안되었나.
							//Debug.LogError("No Imported Transform/Bone : " + ret_LayerUnit._displayName);
							continue;
						}

						//레이어에 연결된 Transform/Bone
						apTransform_Mesh targetMeshTransform = null;
						apTransform_MeshGroup targetMeshGroupTransform = null;
						apBone targetBone = null;

						apAnimTimelineLayer targetTimelineLayer = null;

						switch (linkModType)
						{
							case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
								targetMeshTransform = targetUnit._targetMeshTransform;
								targetTimelineLayer = targetTimeline.GetTimelineLayer(targetMeshTransform);
								break;

							case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
								targetMeshGroupTransform = targetUnit._targetMeshGroupTransform;
								targetTimelineLayer = targetTimeline.GetTimelineLayer(targetMeshGroupTransform);
								break;

							case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
								targetBone = targetUnit._targetBone;
								targetTimelineLayer = targetTimeline.GetTimelineLayer(targetBone);
								break;
						}


						//기존의 레이어가 있는가
						if (targetTimelineLayer != null)
						{
							//<1> 기존 레이어가 있다.
							if (!isMerge)
							{
								//만약 Merge가 아닌 Replace라면 기존의 키프레임을 모두 지워야함
								List<apAnimKeyframe> prevKeyframes = new List<apAnimKeyframe>();
								for (int iKey = 0; iKey < targetTimelineLayer._keyframes.Count; iKey++)
								{
									prevKeyframes.Add(targetTimelineLayer._keyframes[iKey]);
								}
								RemoveKeyframes(prevKeyframes, false);
							}
						}
						else
						{
							//<2> 레이어를 새로 만들어야 한다.
							switch (linkModType)
							{
								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
									targetTimelineLayer = AddAnimTimelineLayer(targetMeshTransform, targetTimeline, false);
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
									targetTimelineLayer = AddAnimTimelineLayer(targetMeshGroupTransform, targetTimeline, false);
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
									targetTimelineLayer = AddAnimTimelineLayer(targetBone, targetTimeline, false);
									break;
							}



							if (targetTimelineLayer == null)
							{
								Debug.LogError("TImelineLayer Add Error");
								continue;
							}
						}

						//타임라인 정보를 리셋
						Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

						//AnimClip이 추가되었다.
						//추가 21.1.32 : Rule 가시성 동기화 초기화
						ResetVisibilityPresetSync();


						Editor.OnAnyObjectAddedOrRemoved();
						Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));

						//타겟 타임라인 레이어에 Keyframe을 채워넣어야 한다.
						List<apRetargetKeyframeUnit> ret_Keyframes = ret_LayerUnit._keyframeUnits;
						for (int iKey = 0; iKey < ret_Keyframes.Count; iKey++)
						{
							//키프레임을 추가한다.
							apRetargetKeyframeUnit keyUnit = ret_Keyframes[iKey];

							int frameIndex = keyUnit._frameIndex;

							//만약 겹치는 Keyframe이 있다면 삭제
							apAnimKeyframe overlapKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(frameIndex);
							if (overlapKeyframe != null)
							{
								RemoveKeyframe(overlapKeyframe, false);
							}

							apAnimKeyframe addedKeyframe = AddAnimKeyframe(frameIndex, targetTimelineLayer, false, false, false, false);
							if (addedKeyframe == null)
							{
								//Debug.LogError("Keyframe 생성 실패");
								continue;
							}

							//설정값을 복사하자
							addedKeyframe._isKeyValueSet = keyUnit._isKeyValueSet;
							addedKeyframe._isActive = keyUnit._isActive;

							addedKeyframe._isLoopAsStart = keyUnit._isLoopAsStart;
							addedKeyframe._isLoopAsEnd = keyUnit._isLoopAsEnd;
							addedKeyframe._loopFrameIndex = keyUnit._loopFrameIndex;

							addedKeyframe._activeFrameIndexMin = keyUnit._activeFrameIndexMin;
							addedKeyframe._activeFrameIndexMax = keyUnit._activeFrameIndexMax;

							addedKeyframe._activeFrameIndexMin_Dummy = keyUnit._activeFrameIndexMin_Dummy;
							addedKeyframe._activeFrameIndexMax_Dummy = keyUnit._activeFrameIndexMax_Dummy;

							if (addedKeyframe._curveKey != null)
							{
								addedKeyframe._curveKey = new apAnimCurve();
							}
							addedKeyframe._curveKey._prevTangentType = keyUnit._curve_PrevTangentType;
							addedKeyframe._curveKey._prevSmoothX = keyUnit._curve_PrevSmoothX;
							addedKeyframe._curveKey._prevSmoothY = keyUnit._curve_PrevSmoothY;
							addedKeyframe._curveKey._nextTangentType = keyUnit._curve_NextTangentType;
							addedKeyframe._curveKey._nextSmoothX = keyUnit._curve_NextSmoothX;
							addedKeyframe._curveKey._nextSmoothY = keyUnit._curve_NextSmoothY;

							addedKeyframe._conSyncValue_Int = keyUnit._conSyncValue_Int;
							addedKeyframe._conSyncValue_Float = keyUnit._conSyncValue_Float;
							addedKeyframe._conSyncValue_Vector2 = keyUnit._conSyncValue_Vector2;


							//ModMesh/ModBone 값을 넣어주자
							switch (linkModType)
							{
								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
									if (addedKeyframe._linkedModMesh_Editor != null)
									{
										addedKeyframe._linkedModMesh_Editor._transformMatrix.SetMatrix(keyUnit._modTransformMatrix, true);
										addedKeyframe._linkedModMesh_Editor._meshColor = keyUnit._modMeshColor;
										addedKeyframe._linkedModMesh_Editor._isVisible = keyUnit._modVisible;
									}
									else
									{
										Debug.LogError("No LinkedModMesh");
									}
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
									if (addedKeyframe._linkedModBone_Editor != null)
									{
										addedKeyframe._linkedModBone_Editor._transformMatrix.SetMatrix(keyUnit._modTransformMatrix, true);
									}
									else
									{
										Debug.LogError("No LinkedModBone");
									}
									break;
							}
						}
					}
				}

			}

			//이벤트를 넣자
			//만약 Replace라면 기존 이벤트 삭제
			if (!isMerge)
			{
				targetAnimClip._animEvents.Clear();
			}
			else
			{
				//추가하기 전에
				//동일한 이름 + 동일한 프레임이 있다면 삭제하자 (Merge라고 하더라도)

				for (int i = 0; i < ret_Events.Count; i++)
				{
					apRetargetAnimEvent ret_Event = ret_Events[i];

					targetAnimClip._animEvents.RemoveAll(delegate (apAnimEvent a)
					{
						return string.Equals(a._eventName, ret_Event._eventName)
								&& a._frameIndex == ret_Event._frameIndex
								&& a._callType == ret_Event._callType;
					});
				}

			}

			for (int i = 0; i < ret_Events.Count; i++)
			{
				apRetargetAnimEvent ret_Event = ret_Events[i];

				apAnimEvent newEvent = new apAnimEvent();

				newEvent._frameIndex = ret_Event._frameIndex;
				newEvent._frameIndex_End = ret_Event._frameIndex_End;
				newEvent._eventName = ret_Event._eventName;

				newEvent._callType = ret_Event._callType;

				if (newEvent._subParams == null)
				{
					newEvent._subParams = new List<apAnimEvent.SubParameter>();
				}

				if (ret_Event._subParams != null && ret_Event._subParams.Count > 0)
				{
					for (int iSubParam = 0; iSubParam < ret_Event._subParams.Count; iSubParam++)
					{
						apAnimEvent.SubParameter newSubParam = new apAnimEvent.SubParameter();
						apRetargetAnimEvent.SubParameter ret_SubParam = ret_Event._subParams[iSubParam];

						newSubParam._paramType = ret_SubParam._paramType;

						newSubParam._boolValue = ret_SubParam._boolValue;
						newSubParam._intValue = ret_SubParam._intValue;
						newSubParam._floatValue = ret_SubParam._floatValue;
						newSubParam._vec2Value = ret_SubParam._vec2Value;
						newSubParam._strValue = ret_SubParam._strValue;

						newSubParam._intValue_End = ret_SubParam._intValue_End;
						newSubParam._floatValue_End = ret_SubParam._floatValue_End;
						newSubParam._vec2Value_End = ret_SubParam._vec2Value_End;

						newEvent._subParams.Add(newSubParam);
					}
				}


				targetAnimClip._animEvents.Add(newEvent);
			}

			//이벤트 정렬도 하자
			targetAnimClip._animEvents.Sort(delegate (apAnimEvent a, apAnimEvent b)
			{
				if (a._frameIndex == b._frameIndex)
				{
					return string.Compare(a._eventName, b._eventName);
				}
				return a._frameIndex - b._frameIndex;
			});


			//타임라인 정보를 리셋
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));//다시 리셋
			Editor.RefreshControllerAndHierarchy(false);


			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			bool isWorkKeyframeChanged = false;//<<딱히 사용되지는 않는다. FFD는 위에서 Revert한 상태
			Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);


			//완료되었쩌여
			EditorUtility.DisplayDialog(Editor.GetText(TEXT.Retarget_ImportAnimComplete_Title),
															Editor.GetText(TEXT.Retarget_ImportAnimComplete_Body),
															Editor.GetText(TEXT.Close));

		}




		public apAnimTimeline AddAnimTimeline(apAnimClip.LINK_TYPE linkType, int modifierUniqueID, apAnimClip targetAnimClip, bool errorMsg = true, bool isSetRecordAndRefresh = true)
		{
			if (targetAnimClip == null)
			{
				return null;
			}

			//Timeline을 추가해야한다.
			if (isSetRecordAndRefresh)
			{
				apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_AddTimeline, 
																			Editor, 
																			Editor._portrait, 
																			targetAnimClip._targetMeshGroup, 
																			//null, 
																			false,
																			apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			int nextTimelineID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimeline);
			if (nextTimelineID < 0)
			{
				if (errorMsg)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Adding Failed", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineAddFailed_Title),
													Editor.GetText(TEXT.AnimTimelineAddFailed_Body),
													Editor.GetText(TEXT.Close));
				}
				return null;
			}



			apAnimTimeline newTimeline = new apAnimTimeline();
			newTimeline.Init(linkType, nextTimelineID, modifierUniqueID, targetAnimClip);

			targetAnimClip._timelines.Add(newTimeline);

			newTimeline.Link(targetAnimClip);

			if (isSetRecordAndRefresh)
			{
				//바로 Timeline을 선택한다.
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);//<<추가 : 타임라인 정보를 리셋

				Editor.Select.SelectAnimTimeline(newTimeline, true);
				Editor.RefreshControllerAndHierarchy(false);

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);//이전
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newTimeline;
		}

		public void RemoveAnimTimeline(apAnimTimeline animTimeline)
		{
			if (animTimeline == null)
			{
				return;
			}
			//Undo - Remove AnimTimeline
			//apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_RemoveTimeline, 
			//													Editor, 
			//													Editor._portrait, 
			//													animTimeline._parentAnimClip._targetMeshGroup, 
			//													animTimeline._linkedModifier, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Timeline");

			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimTimeline, animTimeline._uniqueID);


			//선택중이면 제외
			Editor.Select.CancelAnimEditing();
			Editor.Select.SelectAnimTimeline(null, true);
			if (Editor.Select.AnimTimeline != null)
			{
				//Debug.LogError("Error!!! : AnimTimeline 해제가 안되었다!!");
			}

			apAnimClip parentAnimClip = animTimeline._parentAnimClip;
			//apMeshGroup targetMeshGroup = null;
			if (parentAnimClip == null)
			{
				//?? 없네요.. 에러가..
				//Debug.LogError("Error : AnimClip이 연결되지 않은 Timeline 제거");
			}
			else
			{
				animTimeline._linkedModifier = null;
				animTimeline._modifierUniqueID = -1;
				//뭔가 더 있어야하지 않으려나..
				//targetMeshGroup = parentAnimClip._targetMeshGroup;

				parentAnimClip._timelines.Remove(animTimeline);

				//자동 삭제도 수행한다.
				parentAnimClip.RemoveUnlinkedTimeline();
			}

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//객체가 추가/삭제시 호출
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);//<<추가 : 타임라인 정보를 리셋

			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentAnimClip));
			Editor.RefreshControllerAndHierarchy(false);

			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

		}



		public apAnimTimelineLayer AddAnimTimelineLayer(object targetObject, apAnimTimeline parentTimeline, bool isRecordAndRefresh = true)
		{
			if (targetObject == null || parentTimeline == null)
			{
				return null;
			}

			
			//이미 추가되었으면 리턴
			if (parentTimeline.IsObjectAddedInLayers(targetObject))
			{
				return null;
			}

			if (isRecordAndRefresh)
			{
				//Undo - Add TimelineLayer
				//apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_AddTimelineLayer, Editor, Editor._portrait, parentTimeline._parentAnimClip._targetMeshGroup, null, false);
				if(parentTimeline._linkedModifier != null)
				{
					//추가 21.4.16
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_AddTimelineLayer,
																		Editor,
																		Editor._portrait,
																		parentTimeline._parentAnimClip._targetMeshGroup,
																		parentTimeline._linkedModifier,
																		//null,
																		false,
																		apEditorUtil.UNDO_STRUCT.StructChanged
																		);
				}
				else
				{
					apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_AddTimelineLayer, 
																				Editor, 
																				Editor._portrait, 
																				parentTimeline._parentAnimClip._targetMeshGroup, 
																				//null, 
																				false,
																				apEditorUtil.UNDO_STRUCT.StructChanged);
				}
				
			}


			int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
			if (nextLayerID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Title),
												Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}





			apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
			newLayer.Link(parentTimeline._parentAnimClip, parentTimeline);

			bool isEditorOpt_VividColor = Editor._animUIOption_DefaultTimelineColor == apEditor.TIMELINE_DEFAULT_COLOR.Vivid;


			switch (parentTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						int transformID = -1;
						bool isMeshTransform = false;
						if (targetObject is apTransform_Mesh)
						{
							apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
							transformID = meshTransform._transformUniqueID;
							isMeshTransform = true;

							newLayer.Init_TransformOfModifier(	parentTimeline,
																nextLayerID,
																transformID,
																isMeshTransform,
																isEditorOpt_VividColor);


						}
						else if (targetObject is apTransform_MeshGroup)
						{
							apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
							transformID = meshGroupTransform._transformUniqueID;
							isMeshTransform = false;

							newLayer.Init_TransformOfModifier(	parentTimeline,
																nextLayerID,
																transformID,
																isMeshTransform,
																isEditorOpt_VividColor);
						}
						else if (targetObject is apBone)
						{
							apBone bone = targetObject as apBone;
							newLayer.Init_Bone(	parentTimeline,
												nextLayerID,
												bone._uniqueID,
												bone,
												isEditorOpt_VividColor);
						}
						else
						{
							//?
							Debug.LogError(">> [Unknown Type]");
						}
					}
					break;


				case apAnimClip.LINK_TYPE.ControlParam:
					{
						int controlParamID = -1;
						if (targetObject is apControlParam)
						{
							apControlParam controlParam = targetObject as apControlParam;
							controlParamID = controlParam._uniqueID;
						}
						newLayer.Init_ControlParam(	parentTimeline,
													nextLayerID,
													controlParamID,
													isEditorOpt_VividColor);
					}
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입의 Layer 추가 코드 필요[" + parentTimeline._linkType + "]");
					break;
			}

			parentTimeline._layers.Add(newLayer);

			//전체 Refresh를 해야한다.
			if (parentTimeline._parentAnimClip._targetMeshGroup != null)
			{
				apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip);

				parentTimeline._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);
				parentTimeline._parentAnimClip._targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
				parentTimeline._parentAnimClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();


				//추가 : ExMode에 추가한다.
				//Editor.Select.RefreshMeshGroupExEditingFlags(
				//				parentTimeline._parentAnimClip._targetMeshGroup,
				//				parentTimeline._linkedModifier,
				//				null,
				//				parentTimeline._parentAnimClip,
				//				true);

				////변경 21.2.17
				//if (parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				//{
				//	//Modifier에 연동되는 타입이라면
				//	//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				//	AddAndSyncAnimClipToModifier(parentTimeline._parentAnimClip);
				//}
				//Editor.Select.RefreshMeshGroupExEditingFlags(true);
				
				
			}

			if (isRecordAndRefresh)
			{
				//이전
				//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋

				//이후 19.5.21 : 새로운 레이어를 위주로 갱신
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, newLayer, null);

				//추가 21.1.32 : Rule 가시성 동기화 초기화
				ResetVisibilityPresetSync();

				//4.1 추가된 데이터가 있으면 일단 호출한다.
				Editor.OnAnyObjectAddedOrRemoved();


				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip));
				Editor.RefreshControllerAndHierarchy(false);

				if (parentTimeline._linkedModifier != null)
				{
					parentTimeline._linkedModifier.RefreshParamSet(apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip));
				}

				Editor.Select.SelectAnimTimelineLayer(newLayer, apSelection.MULTI_SELECT.Main, true);
				Editor.Select.AutoSelectAnimTimelineLayer(true, false);//<<타임라인 자동 스크롤 선택


				////추가 21.2.14 : 모디파이어 연결 갱신
				//Editor.Select.ModLinkInfo.LinkRefresh();

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

				bool isWorkKeyframeChanged = false;
				Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//<<isWorkKeyframeChanged 값은 사용되지 않는다.

				////추가 21.2.17 : 여기서 호출
				//Editor.Select.RefreshMeshGroupExEditingFlags(true);

				//변경 21.2.17
				if (parentTimeline != null 
					&& parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					//Modifier에 연동되는 타입이라면
					//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
					AddAndSyncAnimClipToModifier(parentTimeline._parentAnimClip);
				}
				Editor.Select.RefreshMeshGroupExEditingFlags(true);
			}

			return newLayer;
		}




		//추가 20.6.19 : 선택된 다수의 오브젝트들을 타임라인 레이어로 등록한다.
		//다중 선택은 불가능하지만 컨트롤 파라미터도 처리한다.
		public List<apAnimTimelineLayer> AddAnimTimelineLayersForMultipleSelection(apAnimTimeline parentTimeline, bool isRecordAndRefresh = true)
		{
			if (Editor == null 
				|| parentTimeline == null
				|| Editor.Select == null)
			{
				return null;
			}

			
			apAnimClip animClip = parentTimeline._parentAnimClip;
			apMeshGroup targetMeshGroup = animClip._targetMeshGroup;

			//선택된 모든 오브젝트들을 찾자
			List<object> selectedObjects = new List<object>();
			int nMeshTF = 0;
			int nMeshGroupTF = 0;
			int nBone = 0;

			bool isEditorOpt_VividColor = Editor._animUIOption_DefaultTimelineColor == apEditor.TIMELINE_DEFAULT_COLOR.Vivid;

			if (parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				if (parentTimeline._linkedModifier != null)
				{
					nMeshTF = parentTimeline._linkedModifier.IsTarget_MeshTransform ? Editor.Select.SubObjects.NumMeshTF : 0;
					nMeshGroupTF = parentTimeline._linkedModifier.IsTarget_MeshGroupTransform ? Editor.Select.SubObjects.NumMeshGroupTF : 0;
					nBone = parentTimeline._linkedModifier.IsTarget_Bone ? Editor.Select.SubObjects.NumBone : 0;
				}
				if (nMeshTF > 0)
				{
					for (int i = 0; i < nMeshTF; i++)
					{
						selectedObjects.Add(Editor.Select.SubObjects.AllMeshTFs[i]);
					}
				}
				if (nMeshGroupTF > 0)
				{
					for (int i = 0; i < nMeshGroupTF; i++)
					{
						selectedObjects.Add(Editor.Select.SubObjects.AllMeshGroupTFs[i]);
					}
				}
				if (nBone > 0)
				{
					for (int i = 0; i < nBone; i++)
					{
						selectedObjects.Add(Editor.Select.SubObjects.AllBones[i]);
					}
				}
			}
			else
			{
				//컨트롤 파라미터 타임라인의 경우				
				//이전
				//if(Editor.Select.SubObjects.ControlParamForAnim != null)
				//{
				//	selectedObjects.Add(Editor.Select.SubObjects.ControlParamForAnim);
				//}

				//변경 v1.5.0 : 다중 추가 지원
				int nCP = Editor.Select.SubObjects.NumControlParam;
				if(nCP > 0)
				{
					for (int i = 0; i < nCP; i++)
					{
						selectedObjects.Add(Editor.Select.SubObjects.AllControlParamsForAnim[i]);
					}
				}
			}

			if(selectedObjects.Count == 0)
			{
				return null;
			}


			if (isRecordAndRefresh)
			{
				//Undo - Add TimelineLayer
				apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_AddTimelineLayer, 
																			Editor, 
																			Editor._portrait, 
																			parentTimeline._parentAnimClip._targetMeshGroup, 
																			//null, 
																			false,
																			apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			List<apAnimTimelineLayer> resultLayers = new List<apAnimTimelineLayer>();

			//이제 하나씩 추가한다.
			object curObj = null;
			for (int iObj = 0; iObj < selectedObjects.Count; iObj++)
			{
				curObj = selectedObjects[iObj];

				bool isSuccess = false;


				//이미 추가되었으면 패스
				if (parentTimeline.IsObjectAddedInLayers(curObj))
				{
					continue;
				}

				//추가될 수 없어도 패스
				if (!parentTimeline.IsLayerAddableType(curObj))
				{
					continue;
				}


				//추가 20.9.8 : 리깅된 자식 메시는 TF 타임라인에는 추가되지 않는다.
				//조건을 모두 체크하자
				//1. TF 모디파이어인 타임라인
				//2. 메시 타입
				//3. 리깅된 자식 메시 그룹의 메시
				if(parentTimeline._linkedModifier != null &&
					parentTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedTF)
				{
					if(curObj is apTransform_Mesh)
					{
						apTransform_Mesh curMeshTF = curObj as apTransform_Mesh;
						if(curMeshTF != null && curMeshTF.IsRiggedChildMeshTF(targetMeshGroup))
						{
							//리깅된 자식 메시이다.
							//이 조건에서는 타임라인 레이어로 등록할 수 없다.
							//Debug.LogError("리깅된 자식 메시는 타임라인 레이어로 등록할 수 없다.");
							continue;
						}
					}
				}


				int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
				if (nextLayerID < 0)
				{
					//실패하면 리턴..이 아니라 패스.
					//경고 문구는 없다.
					continue;
				}

				//레이어 추가
				apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
				newLayer.Link(parentTimeline._parentAnimClip, parentTimeline);

				

				switch (parentTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						{
							int transformID = -1;
							bool isMeshTransform = false;
							if (curObj is apTransform_Mesh)
							{
								apTransform_Mesh meshTransform = curObj as apTransform_Mesh;
								transformID = meshTransform._transformUniqueID;
								isMeshTransform = true;

								newLayer.Init_TransformOfModifier(	parentTimeline,
																	nextLayerID,
																	transformID,
																	isMeshTransform,
																	isEditorOpt_VividColor);
								isSuccess = true;

							}
							else if (curObj is apTransform_MeshGroup)
							{
								apTransform_MeshGroup meshGroupTransform = curObj as apTransform_MeshGroup;
								transformID = meshGroupTransform._transformUniqueID;
								isMeshTransform = false;

								newLayer.Init_TransformOfModifier(	parentTimeline,
																	nextLayerID,
																	transformID,
																	isMeshTransform,
																	isEditorOpt_VividColor);
								isSuccess = true;
							}
							else if (curObj is apBone)
							{
								apBone bone = curObj as apBone;
								newLayer.Init_Bone(	parentTimeline,
													nextLayerID,
													bone._uniqueID,
													bone,
													isEditorOpt_VividColor);
								isSuccess = true;
							}
							else
							{
								break;
							}
						}
						break;


					case apAnimClip.LINK_TYPE.ControlParam:
						{
							int controlParamID = -1;
							if (curObj is apControlParam)
							{
								apControlParam controlParam = curObj as apControlParam;
								controlParamID = controlParam._uniqueID;
							}
							newLayer.Init_ControlParam(	parentTimeline,
														nextLayerID,
														controlParamID,
														isEditorOpt_VividColor);
							isSuccess = true;
						}
						break;

					default:
						//Debug.LogError("TODO : 정의되지 않은 타입의 Layer 추가 코드 필요[" + parentTimeline._linkType + "]");
						break;
				}


				if(!isSuccess)
				{
					continue;
				}

				parentTimeline._layers.Add(newLayer);
				resultLayers.Add(newLayer);
			}

			if(resultLayers.Count == 0)
			{
				return null;
			}

			//전체 Refresh를 해야한다.
			if (parentTimeline._parentAnimClip._targetMeshGroup != null)
			{
				apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip);

				parentTimeline._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);
				parentTimeline._parentAnimClip._targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
				parentTimeline._parentAnimClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();


				//추가 : ExMode에 추가한다.
				//Editor.Select.RefreshMeshGroupExEditingFlags(
				//				parentTimeline._parentAnimClip._targetMeshGroup,
				//				parentTimeline._linkedModifier,
				//				null,
				//				parentTimeline._parentAnimClip,
				//				true);

				////변경 21.2.17
				//if (parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				//{
				//	//Modifier에 연동되는 타입이라면
				//	//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				//	AddAndSyncAnimClipToModifier(parentTimeline._parentAnimClip);
				//}
				//Editor.Select.RefreshMeshGroupExEditingFlags(true);
			}

			if (isRecordAndRefresh)
			{
				//추가된 레이어들 갱신[다중 처리]
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, resultLayers);

				//추가 21.1.32 : Rule 가시성 동기화 초기화
				ResetVisibilityPresetSync();

				//추가된 데이터가 있으면 일단 호출한다.
				Editor.OnAnyObjectAddedOrRemoved();


				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip));
				Editor.RefreshControllerAndHierarchy(false);

				if (parentTimeline._linkedModifier != null)
				{
					parentTimeline._linkedModifier.RefreshParamSet(apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip));
				}

				Editor.Select.SelectAnimTimelineLayer(resultLayers[0], apSelection.MULTI_SELECT.Main, true);//생성된 첫번째 레이어를 메인으로 설정
				if(resultLayers.Count > 1)
				{
					for (int iLayers = 1; iLayers < resultLayers.Count; iLayers++)
					{
						//나머지 레이어들 추가
						Editor.Select.SelectAnimTimelineLayer(resultLayers[iLayers], apSelection.MULTI_SELECT.AddOrSubtract, true);
					}
				}
				Editor.Select.AutoSelectAnimTimelineLayer(true, false);//<<타임라인 자동 스크롤 선택

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);//이전
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

				bool isWorkKeyframeChanged = false;
				Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				//FFD가 켜졌다면 Revert를 하자 (Adapt 여부를 물어보고자 한다면 아예 앞에서 물어봐야 한다.)
				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//여기서는 강제로 Revert
					Editor.Gizmos.RevertFFDTransformForce();
				}

				////추가 21.2.14 : 모디파이어 연결 갱신
				//Editor.Select.ModLinkInfo.LinkRefresh();

				////추가 21.2.17 : 여기서 호출
				//Editor.Select.RefreshMeshGroupExEditingFlags(true);

				//변경 21.2.17
				if (parentTimeline != null 
					&& parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					//Modifier에 연동되는 타입이라면
					//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
					AddAndSyncAnimClipToModifier(parentTimeline._parentAnimClip);
				}
				Editor.Select.RefreshMeshGroupExEditingFlags(true);
			}

			return resultLayers;
		}








		public apAnimTimelineLayer AddAnimTimelineLayerForAllTransformObject(apMeshGroup parentMeshGroup, bool isTargetTransform, bool isAddChildTransformAddable, apAnimTimeline parentTimeline)
		{
			if (parentMeshGroup == null || parentTimeline == null)
			{
				return null;
			}

			//Undo - Add TimelineLayer
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_AddTimelineLayer, 
																		Editor, 
																		Editor._portrait, 
																		parentMeshGroup, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

			List<object> targetObjects = new List<object>();

			//목표를 리스트로 찾자
			if(parentTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				//컨트롤 파라미터라면..
				if(Editor._portrait != null
					&& Editor._portrait._controller != null
					&& Editor._portrait._controller._controlParams != null
					&& Editor._portrait._controller._controlParams.Count > 0)
				{
					for (int iParam = 0; iParam < Editor._portrait._controller._controlParams.Count; iParam++)
					{
						targetObjects.Add(Editor._portrait._controller._controlParams[iParam]);
					}
				}
				
			}
			else
			{
				//메시나 본을 찾는다면..
				FindChildTransformsOrBones(parentMeshGroup, parentMeshGroup._rootMeshGroupTransform, isTargetTransform, targetObjects, isAddChildTransformAddable);

				//추가 21.3.17 : 지금은 무작위로 가져온것이고, Hierarchy와 유사하게 정렬을 하자
				//Debug.Log("정렬을 하자");
				List<object> sortedTargetList = GetSortedSubObjectsAsHierarchy(parentMeshGroup, isTargetTransform, !isTargetTransform);

				//Sorted List와 비슷하게 정렬하자
				targetObjects.Sort(delegate(object a, object b)
				{
					return sortedTargetList.IndexOf(a) - sortedTargetList.IndexOf(b);
				});
			}
			
			

			apAnimTimelineLayer firstLayer = null;
			int startFrame = parentTimeline._parentAnimClip.StartFrame;

			//추가 대상을 계산하자
			bool isAddable_MeshTF = false;
			bool isAddable_RiggedChildMeshTFAddable = false;
			bool isAddable_ChildMeshTFAddable = false;
			bool isAddable_MeshGroupTF = false;

			bool isAddable_Bone = false;
			bool isAddable_ControlParam = false;

			if(parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				if(parentTimeline._linkedModifier != null)
				{
					#region [미사용 코드] 이전 코드
					//if((int)(parentTimeline._linkedModifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0)
					//{
					//	//Morph타입인 경우
					//	isTFAddable = true;
					//}
					//else
					//{
					//	//그 외의 경우
					//	isTFAddable = true;
					//	isAddable_Bone = true;
					//} 
					#endregion

					//변경 v1.4.2 : 지원하는 대상을 좀더 자세히 분류하자
					if(parentTimeline._linkedModifier.IsTarget_MeshTransform)
					{
						isAddable_MeshTF = true;
					}
					if(parentTimeline._linkedModifier.IsTarget_ChildMeshTransform)
					{
						isAddable_ChildMeshTFAddable = true;
						isAddable_RiggedChildMeshTFAddable = true;

						if(parentTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedTF
							|| parentTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.TF)
						{
							//TF 모디파이어는 리깅된 자식 MeshTF를 지원하지 않는다.
							isAddable_RiggedChildMeshTFAddable = false;

						}
					}
					if(parentTimeline._linkedModifier.IsTarget_MeshGroupTransform)
					{
						isAddable_MeshGroupTF = true;
					}
					if(parentTimeline._linkedModifier.IsTarget_Bone)
					{
						isAddable_Bone = true;
					}
				}
			}
			else
			{
				isAddable_ControlParam = true;
			}

			List<apAnimTimelineLayer> addedLayers = new List<apAnimTimelineLayer>();
			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			apBone bone = null;
			apControlParam controlParam = null;

			bool isEditorOpt_VividColor = Editor._animUIOption_DefaultTimelineColor == apEditor.TIMELINE_DEFAULT_COLOR.Vivid;

			for (int iTargetObjects = 0; iTargetObjects < targetObjects.Count; iTargetObjects++)
			{
				object targetObject = targetObjects[iTargetObjects];

				//이미 추가되었으면 리턴
				if (parentTimeline.IsObjectAddedInLayers(targetObject))
				{
					continue;
				}

				//20.7.5 추가할 수 없으면 리턴
				meshTransform = null;
				meshGroupTransform = null;
				bone = null;
				controlParam = null;
				if (targetObject is apTransform_Mesh)
				{	
					meshTransform = targetObject as apTransform_Mesh;

					if(!isAddable_MeshTF)
					{
						//MeshTF를 지원하지 않는 모디파이어라면
						continue;
					}

					bool isChildMeshTF = false;
					bool isRiggedMeshTF = false;
					
					if(!parentMeshGroup.IsContainMeshTransform(meshTransform))
					{
						//메인 MeshGroup의 메시가 아니라면
						isChildMeshTF = true;

						//리깅된 자식 메시 그룹의 메시인지 여부
						isRiggedMeshTF = meshTransform.IsRiggedChildMeshTF(parentMeshGroup);
					}

					if(!isAddable_ChildMeshTFAddable && isChildMeshTF)
					{
						//자식 MeshTF를 지원하지 않는다면
						continue;
					}

					if(!isAddable_RiggedChildMeshTFAddable && isRiggedMeshTF)
					{
						//자식 메시 그룹의 메시 + 리깅된 상태라면
						continue;
					}
				}
				else if (targetObject is apTransform_MeshGroup)
				{	
					meshGroupTransform = targetObject as apTransform_MeshGroup;

					if(!isAddable_MeshGroupTF)
					{
						//자식 메시 그룹을 지원하지 않는다면
						continue;
					}
				}
				else if (targetObject is apBone)
				{	
					bone = targetObject as apBone;

					if(!isAddable_Bone)
					{
						//본을 지원하지 않는다면
						continue;
					}
				}
				if (targetObject is apControlParam)
				{	
					controlParam = targetObject as apControlParam;
					if(!isAddable_ControlParam)
					{
						//컨트롤 파라미터를 지원하지 않는다면
						continue;
					}
				}


				//레이어를 만들자.
				int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
				if (nextLayerID < 0)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Title),
													Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Body),
													Editor.GetText(TEXT.Close));

					return null;
				}





				apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
				newLayer.Link(parentTimeline._parentAnimClip, parentTimeline);

				if (firstLayer == null)
				{
					firstLayer = newLayer;
				}

				switch (parentTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						{
							int transformID = -1;
							bool isMeshTransform = false;
							if (meshTransform != null)
							{
								transformID = meshTransform._transformUniqueID;
								isMeshTransform = true;

								newLayer.Init_TransformOfModifier(	parentTimeline,
																	nextLayerID,
																	transformID,
																	isMeshTransform,
																	isEditorOpt_VividColor);
							}
							else if (meshGroupTransform != null)
							{
								transformID = meshGroupTransform._transformUniqueID;
								isMeshTransform = false;

								newLayer.Init_TransformOfModifier(	parentTimeline,
																	nextLayerID,
																	transformID,
																	isMeshTransform,
																	isEditorOpt_VividColor);
							}
							else if (bone != null)
							{
								newLayer.Init_Bone(	parentTimeline,
													nextLayerID,
													bone._uniqueID,
													bone,
													isEditorOpt_VividColor);
							}
							else
							{
								//?
								Debug.LogError(">> [Unknown Type]");
							}
						}
						break;


					case apAnimClip.LINK_TYPE.ControlParam:
						{
							int controlParamID = -1;
							if (controlParam != null)
							{
								controlParamID = controlParam._uniqueID;
							}
							newLayer.Init_ControlParam(	parentTimeline,
														nextLayerID,
														controlParamID,
														isEditorOpt_VividColor);
						}
						break;

					default:
						Debug.LogError("TODO : 정의되지 않은 타입의 Layer 추가 코드 필요[" + parentTimeline._linkType + "]");
						break;
				}

				parentTimeline._layers.Add(newLayer);


				//시작 프레임에 Keyframe을 추가하자
				//AddAnimKeyframe(startFrame, newLayer, false, false, false, false);
				//>> 이걸 Refresh 후로 미루자
				addedLayers.Add(newLayer);
			}


			//전체 Refresh를 해야한다.
			if (parentTimeline._parentAnimClip._targetMeshGroup != null)
			{
				apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip);

				parentTimeline._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);
				parentTimeline._parentAnimClip._targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

				parentTimeline._parentAnimClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();

				//추가 : ExMode에 추가한다.
				//Editor.Select.RefreshMeshGroupExEditingFlags(
				//				parentTimeline._parentAnimClip._targetMeshGroup,
				//				parentTimeline._linkedModifier,
				//				null,
				//				parentTimeline._parentAnimClip,
				//				true);

				////변경 21.2.17
				//if (parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				//{
				//	//Modifier에 연동되는 타입이라면
				//	//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				//	AddAndSyncAnimClipToModifier(parentTimeline._parentAnimClip);
				//}
				//Editor.Select.RefreshMeshGroupExEditingFlags(true);
			}

			//전체 타임라인 갱신
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip));
			Editor.RefreshControllerAndHierarchy(false);

			//if(parentTimeline._linkedModifier != null)
			//{
			//	Debug.Log("AnimLayer Add -> RefreshParamSet");
			//	parentTimeline._linkedModifier.RefreshParamSet();
			//}

			for (int i = 0; i < addedLayers.Count; i++)
			{
				AddAnimKeyframe(startFrame, addedLayers[i], false, false, false, false);
			}


			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//다시 Refresh
			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentTimeline._parentAnimClip));
			Editor.RefreshControllerAndHierarchy(true);//<<true : Timeline도 모두 갱신한다.

			Editor.Select.SelectAnimTimelineLayer(firstLayer, apSelection.MULTI_SELECT.Main, true);

			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			bool isWorkKeyframeChanged = false;
			Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

			//FFD가 켜져있다면 Revert 한다. (이 함수의 외부에서 이미 FFD 처리 여부를 물어봤을 것이다.)
			if(Editor.Gizmos.IsFFDMode)
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();

			//변경 21.2.17
			if (parentTimeline != null 
				&& parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				AddAndSyncAnimClipToModifier(parentTimeline._parentAnimClip);
			}
			Editor.Select.RefreshMeshGroupExEditingFlags(true);


			return firstLayer;
		}

		/// <summary>
		/// AnimTimelineLayer에 오브젝트를 추가하기 위해 "모든 Mesh/MeshGroup Transform"을 찾거나 "Bone"을 찾는 함수
		/// </summary>
		public void FindChildTransformsOrBones(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform, bool isTargetTransform, List<object> resultList, bool isChildTransformSupport)
		{
			if (isTargetTransform)
			{
				if (meshGroup != meshGroupTransform._meshGroup)
				{
					resultList.Add(meshGroupTransform);
				}


				for (int i = 0; i < meshGroupTransform._meshGroup._childMeshTransforms.Count; i++)
				{
					resultList.Add(meshGroupTransform._meshGroup._childMeshTransforms[i]);
				}
			}
			else
			{
				//<BONE_EDIT> Recursive로 모든 오브젝트를 찾기 때문에 Bone은 자기 자신만 체크하면 된다.
				List<apBone> bones = new List<apBone>();
				for (int i = 0; i < meshGroupTransform._meshGroup._boneList_Root.Count; i++)
				{
					MakeRecursiveList(meshGroupTransform._meshGroup._boneList_Root[i], bones);
				}

				for (int i = 0; i < bones.Count; i++)
				{
					resultList.Add(bones[i]);
				}

			}

			if (isChildTransformSupport)
			{
				for (int i = 0; i < meshGroupTransform._meshGroup._childMeshGroupTransforms.Count; i++)
				{
					apTransform_MeshGroup childMeshGroup = meshGroupTransform._meshGroup._childMeshGroupTransforms[i];
					FindChildTransformsOrBones(meshGroup, childMeshGroup, isTargetTransform, resultList, isChildTransformSupport);
				}
			}
		}

		private void MakeRecursiveList(apBone targetBone, List<apBone> resultList)
		{
			resultList.Add(targetBone);
			if (targetBone._childBones != null)
			{
				for (int i = 0; i < targetBone._childBones.Count; i++)
				{
					MakeRecursiveList(targetBone._childBones[i], resultList);
				}
			}
		}


		public void RemoveAnimTimelineLayer(apAnimTimelineLayer animTimelineLayer)
		{
			if (animTimelineLayer == null)
			{
				return;
			}
			//Undo - Remove Anim Timeline Layer
			//apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_RemoveTimelineLayer, 
			//													Editor, 
			//													Editor._portrait, 
			//													animTimelineLayer._parentAnimClip._targetMeshGroup, 
			//													animTimelineLayer._parentTimeline._linkedModifier, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Timeline Layer");

			//ID 반납
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimTimelineLayer, animTimelineLayer._uniqueID);



			//선택중이면 제외
			//if (Editor.Select.AnimTimelineLayer == animTimelineLayer)//이전
			if (Editor.Select.NumAnimTimelineLayers > 0 && Editor.Select.AnimTimelineLayers_All.Contains(animTimelineLayer))//변경 20.6.17
			{
				Editor.Select.SelectAnimTimelineLayer(null, apSelection.MULTI_SELECT.Main, true);
			}

			//apMeshGroup targetMeshGroup = null;
			apAnimTimeline parentTimeline = animTimelineLayer._parentTimeline;
			apAnimClip parentAnimClip = null;
			if (parentTimeline != null)
			{
				parentAnimClip = parentTimeline._parentAnimClip;
				parentTimeline._layers.Remove(animTimelineLayer);

				animTimelineLayer._transformID = -1;
				animTimelineLayer._boneID = -1;
				animTimelineLayer._controlParamID = -1;

				//자동 삭제도 해준다.
				parentTimeline.RemoveUnlinkedLayer();

				//if (parentTimeline._parentAnimClip != null)
				//{
				//	targetMeshGroup = parentTimeline._parentAnimClip._targetMeshGroup;
				//}
			}
			else
			{
				Debug.LogError("Error : Parent Timeline이 없는 Layer 제거 시도");
			}

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//객체가 추가/삭제시 호출
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

			//Debug.LogError("Remove AnimTimeline Layer");
			


			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentAnimClip));
			Editor.RefreshControllerAndHierarchy(false);

			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();

			//변경 21.2.17
			if (parentTimeline != null 
				&& parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				AddAndSyncAnimClipToModifier(parentTimeline._parentAnimClip);
			}
			Editor.Select.RefreshMeshGroupExEditingFlags(true);


			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();
		}





		//추가 20.7.17
		/// <summary>
		/// 타임라인 레이어를 여러개 삭제한다.
		/// </summary>
		/// <param name="animTimelineLayer"></param>
		public void RemoveAnimTimelineLayers(List<apAnimTimelineLayer> animTimelineLayers, apAnimClip parentAnimClip)
		{
			if (animTimelineLayers == null
				|| animTimelineLayers.Count == 0
				|| parentAnimClip == null)
			{
				return;
			}

			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Timeline Layers");

			apAnimTimelineLayer curLayer = null;
			for (int i = 0; i < animTimelineLayers.Count; i++)
			{
				curLayer = animTimelineLayers[i];
				if(curLayer == null)
				{
					continue;
				}

				//ID 반납
				Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimTimelineLayer, curLayer._uniqueID);

				//선택중이면 제외
				if (Editor.Select.NumAnimTimelineLayers > 0 && Editor.Select.AnimTimelineLayers_All.Contains(curLayer))//변경 20.6.17
				{
					Editor.Select.SelectAnimTimelineLayer(null, apSelection.MULTI_SELECT.Main, true);
				}

				apAnimTimeline parentTimeline = curLayer._parentTimeline;
				if (parentTimeline != null)
				{
					parentTimeline._layers.Remove(curLayer);

					curLayer._transformID = -1;
					curLayer._boneID = -1;
					curLayer._controlParamID = -1;

					//자동 삭제도 해준다.
					parentTimeline.RemoveUnlinkedLayer();
				}
				else
				{
					Debug.LogError("Error : Parent Timeline이 없는 Layer 제거 시도");
				}
			}

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//객체가 추가/삭제시 호출
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

			//Debug.LogError("Remove AnimTimeline Layer");

			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentAnimClip));
			Editor.RefreshControllerAndHierarchy(false);

			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();
			
			//변경 21.2.17			
			AddAndSyncAnimClipToModifier(parentAnimClip);
			Editor.Select.RefreshMeshGroupExEditingFlags(true);


			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();
		}






		//애니메이션 키프레임 추가하기
		public apAnimKeyframe AddAnimKeyframe(int targetFrame, apAnimTimelineLayer parentLayer, bool isMakeCurrentBlendData, bool isErrorMsg = true, bool isSetRecord = true, bool isRefresh = true)
		{
			if (parentLayer == null)
			{
				return null;
			}

			if (isSetRecord)
			{
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_AddKeyframe, Editor,
																	Editor._portrait,
																	parentLayer._parentAnimClip._targetMeshGroup,
																	parentLayer._parentTimeline._linkedModifier, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly
																	);
			}

			apAnimKeyframe existFrame = parentLayer.GetKeyframeByFrameIndex(targetFrame);

			if (existFrame != null)
			{
				//이미 해당 프레임에 값이 있다.
				//삭제 21.4.19 : 경고 메시지가 나오지 않도록 만든다.
				//EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimKeyframeAddFailed_Title),
				//								Editor.GetText(TEXT.AnimKeyframeAddFailed_Body_Already),
				//								Editor.GetText(TEXT.Close));

				return null;
			}

			int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
			if (nextKeyframeID < 0)
			{
				if (isErrorMsg)
				{
					//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimKeyframeAddFailed_Title),
													Editor.GetText(TEXT.AnimKeyframeAddFailed_Body_Error),
													Editor.GetText(TEXT.Close));

				}
				return null;
			}


			bool isIntControlParamLayer = (parentLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
												&& (parentLayer._linkedControlParam != null && parentLayer._linkedControlParam._valueType == apControlParam.TYPE.Int);


			apAnimKeyframe newKeyframe = new apAnimKeyframe();
			//이전
			//newKeyframe.Init(nextKeyframeID, targetFrame, isIntControlParamLayer);

			//변경 22.1.8 : 컨트롤 파라미터의 기본값을 넣어야 한다. (블렌드는 아래에서 수행)
			if(parentLayer._linkType == apAnimClip.LINK_TYPE.ControlParam && parentLayer._linkedControlParam != null)
			{
				newKeyframe.Init_ControlParam(nextKeyframeID, targetFrame, isIntControlParamLayer, parentLayer._linkedControlParam);
			}
			else
			{
				newKeyframe.Init_Modifier(nextKeyframeID, targetFrame, isIntControlParamLayer);
			}

			

			newKeyframe.Link(parentLayer);


			parentLayer._keyframes.Add(newKeyframe);

			
			if (parentLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				AddAndSyncAnimClipToModifier(parentLayer._parentTimeline._parentAnimClip);
			}

			if (isMakeCurrentBlendData)
			{
				MakeBlendModifiedDataAnimated(newKeyframe);
			}


			//전체 Refresh를 해야한다.
			if (isRefresh)
			{
				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentLayer._parentAnimClip));

				Editor.RefreshControllerAndHierarchy(false);

				Editor.RefreshTimelineLayers(	apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, 
												parentLayer, null);

				//중요 : LinkAndRefreshInEditor 이후에는 다음 함수들을 꼭 호출해야한다.
				//Debug.LogError("Add Keyframe + Refresh Link");
				//parentLayer._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits();
				//parentLayer._parentAnimClip._targetMeshGroup.RefreshModifierLink();
				//parentLayer._parentAnimClip._targetMeshGroup._modifierStack.RefreshAndSort(true);

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);//이전
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();

			return newKeyframe;
		}


		//추가 20.6.12 : 애니메이션 키프레임"들"을 추가하기
		//다중 편집을 위한 처리
		public List<apAnimKeyframe> AddAnimKeyframes(int targetFrame, apAnimClip animClip, List<apAnimTimelineLayer> parentLayers, bool isMakeCurrentBlendData, bool isSetRecord = true, bool isRefresh = true)
		{
			if (parentLayers == null 
				|| parentLayers.Count == 0
				|| animClip == null)
			{
				return null;
			}

			apAnimTimelineLayer curLayer = null;

			if (isSetRecord)
			{
				//저장하기 전에, 모든 레이어가 공통의 타임라인을 공유하는가
				apAnimTimeline commonTimeline = null;
				for (int i = 0; i < parentLayers.Count; i++)
				{
					curLayer = parentLayers[i];
					if(i == 0)
					{
						commonTimeline = curLayer._parentTimeline;
					}
					else
					{
						if(commonTimeline != curLayer._parentTimeline)
						{
							//동일하지 않다.
							commonTimeline = null;
							break;
						}
					}
				}

				if (commonTimeline == null)
				{
					//모든 타임라인 체크
					apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_AddKeyframe, Editor,
																	Editor._portrait,
																	animClip._targetMeshGroup, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					//공통의 타임라인만 체크
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_AddKeyframe, Editor,
																	Editor._portrait,
																	animClip._targetMeshGroup,
																	commonTimeline._linkedModifier, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				
			}

			List<apAnimKeyframe> resultKeyframes = new List<apAnimKeyframe>();

			
			for (int iTL = 0; iTL < parentLayers.Count; iTL++)
			{
				curLayer = parentLayers[iTL];
				if (curLayer == null)
				{
					continue;
				}

				apAnimKeyframe existFrame = curLayer.GetKeyframeByFrameIndex(targetFrame);
				if (existFrame != null)
				{
					//이미 해당 프레임에 값이 있다.
					//> 패스
					continue;
				}

				int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
				if (nextKeyframeID < 0)
				{	
					continue;
				}


				bool isIntControlParamLayer = (curLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
													&& (curLayer._linkedControlParam != null && curLayer._linkedControlParam._valueType == apControlParam.TYPE.Int);


				apAnimKeyframe newKeyframe = new apAnimKeyframe();
				//이전
				//newKeyframe.Init(nextKeyframeID, targetFrame, isIntControlParamLayer);

				//변경 22.1.8 : 컨트롤 파라미터의 기본값을 넣어야 한다. (블렌드는 아래에서 수행)
				if(curLayer._linkType == apAnimClip.LINK_TYPE.ControlParam && curLayer._linkedControlParam != null)
				{
					newKeyframe.Init_ControlParam(nextKeyframeID, targetFrame, isIntControlParamLayer, curLayer._linkedControlParam);
				}
				else
				{
					newKeyframe.Init_Modifier(nextKeyframeID, targetFrame, isIntControlParamLayer);
				}

				newKeyframe.Link(curLayer);


				curLayer._keyframes.Add(newKeyframe);
				resultKeyframes.Add(newKeyframe);

				if (curLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					//Modifier에 연동되는 타입이라면
					//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
					AddAndSyncAnimClipToModifier(curLayer._parentTimeline._parentAnimClip);
				}

				if (isMakeCurrentBlendData)
				{
					MakeBlendModifiedDataAnimated(newKeyframe);
				}
			}

			


			//전체 Refresh를 해야한다.
			if (isRefresh)
			{
				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(animClip));
				Editor.RefreshControllerAndHierarchy(false);

				Editor.RefreshTimelineLayers(	apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, 
												null, null);//null을 넣어서 전체 리프레시

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);//이전
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();

			return resultKeyframes;
		}




		public apAnimKeyframe AddCopiedAnimKeyframe(int targetFrameIndex, apAnimTimelineLayer parentLayer, bool isMakeCurrentBlendData, apAnimKeyframe srcKeyframe, bool isRefresh, bool isRecord)
		{
			if (parentLayer == null)
			{
				return null;
			}

			//Undo - 키프레임 복사
			if (isRecord)
			{
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_DupKeyframe, Editor,
																	Editor._portrait,
																	parentLayer._parentAnimClip._targetMeshGroup,
																	parentLayer._parentTimeline._linkedModifier,
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.StructChanged);
			}

			int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
			if (nextKeyframeID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimKeyframeAddFailed_Title),
												Editor.GetText(TEXT.AnimKeyframeAddFailed_Body_Error),
												Editor.GetText(TEXT.Close));
				return null;
			}

			bool isIntControlParamLayer = (parentLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
												&& (parentLayer._linkedControlParam != null && parentLayer._linkedControlParam._valueType == apControlParam.TYPE.Int);


			apAnimKeyframe newKeyframe = new apAnimKeyframe();			

			//이전
			//newKeyframe.Init(nextKeyframeID, targetFrameIndex, isIntControlParamLayer);

			//변경 22.1.8 : 컨트롤 파라미터의 기본값을 넣어야 한다. (블렌드는 아래에서 수행)
			if(parentLayer._linkType == apAnimClip.LINK_TYPE.ControlParam && parentLayer._linkedControlParam != null)
			{
				newKeyframe.Init_ControlParam(nextKeyframeID, targetFrameIndex, isIntControlParamLayer, parentLayer._linkedControlParam);
			}
			else
			{
				newKeyframe.Init_Modifier(nextKeyframeID, targetFrameIndex, isIntControlParamLayer);
			}

			newKeyframe.Link(parentLayer);

			if (isMakeCurrentBlendData)
			{
				//Debug.LogError("TODO : Set Key -> isMakeCurrentBlendData");
			}


			parentLayer._keyframes.Add(newKeyframe);

			if (parentLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				//Debug.Log("Start Sync To Animated Modifier");
				AddAndSyncAnimClipToModifier(parentLayer._parentTimeline._parentAnimClip);
			}

			//이전
			//Editor.RefreshTimelineLayers(false);

			//변경 19.5.21 : 복제된 키프레임의 타임라인 레이어만 갱신한다.
			Editor.RefreshTimelineLayers(	apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, 
											parentLayer, null);


			//값을 넣어서 복사하자
			if (srcKeyframe != null)
			{
				newKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, newKeyframe._frameIndex);
				newKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;

				//newKeyframe._conSyncValue_Bool = srcKeyframe._conSyncValue_Bool;
				newKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
				newKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
				newKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;
				//newKeyframe._conSyncValue_Vector3 = srcKeyframe._conSyncValue_Vector3;
				//newKeyframe._conSyncValue_Color = srcKeyframe._conSyncValue_Color;


				if (newKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//Mod Mesh 값을 복사하자
					apModifiedMesh srcLinkedModMesh = srcKeyframe._linkedModMesh_Editor;
					apModifiedMesh newLinkedModMesh = newKeyframe._linkedModMesh_Editor;

					List<apModifiedVertex> srcVertList =	srcLinkedModMesh._vertices;
					apMatrix srcTransformMatrix =			srcLinkedModMesh._transformMatrix;
					Color srcMeshColor =					srcLinkedModMesh._meshColor;
					bool isVisible =						srcLinkedModMesh._isVisible;

					//추가 22.3.20 [v1.4.0] Pin 복사
					List<apModifiedPin> srcPinList =		srcLinkedModMesh._pins;

					newLinkedModMesh._transformMatrix.SetMatrix(srcTransformMatrix, true);
					newLinkedModMesh._meshColor = srcMeshColor;
					newLinkedModMesh._isVisible = isVisible;

					apModifiedVertex srcModVert = null;
					apModifiedVertex dstModVert = null;

					for (int i = 0; i < srcVertList.Count; i++)
					{
						srcModVert = srcVertList[i];
						dstModVert = newLinkedModMesh._vertices[i];

						if (dstModVert._vertexUniqueID != srcModVert._vertexUniqueID)
						{
							dstModVert = newLinkedModMesh._vertices.Find(delegate (apModifiedVertex a)
							{
								return a._vertexUniqueID == srcModVert._vertexUniqueID;
							});
						}

						if (dstModVert != null)
						{
							dstModVert._deltaPos = srcModVert._deltaPos;
						}
					}

					//추가 22.3.20 [v1.4.0] Pin 복사
					if(srcPinList != null && srcPinList.Count > 0)
					{
						apModifiedPin srcModPin = null;
						apModifiedPin dstModPin = null;
						for (int i = 0; i < srcPinList.Count; i++)
						{
							srcModPin = srcPinList[i];
							dstModPin = newLinkedModMesh._pins[i];

							if(dstModPin._pinUniqueID != srcModPin._pinUniqueID)
							{
								dstModPin = newLinkedModMesh._pins.Find(delegate(apModifiedPin a)
								{
									return a._pinUniqueID == srcModPin._pinUniqueID;
								});
							}

							if(dstModPin != null)
							{
								dstModPin._deltaPos = srcModPin._deltaPos;
							}
						}
					}
					
				}

				if (newKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//ModBone도 복사하자
					if (newKeyframe._linkedModBone_Editor._transformMatrix == null)
					{
						newKeyframe._linkedModBone_Editor._transformMatrix = new apMatrix();
					}
					newKeyframe._linkedModBone_Editor._transformMatrix.SetMatrix(srcKeyframe._linkedModBone_Editor._transformMatrix, true);
				}

				//else
				//{
				//	//만약 Src만 있다면 체크해볼 필요가 있다. 연동이 안된 상태에서 복사를 시도했기 때문
				//	if (srcKeyframe._linkedModMesh_Editor != null)
				//	{
				//		Debug.LogError("Copy Keyframe Error : No Linked ModMesh");
				//	}
				//}
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentLayer._parentAnimClip));
			if (isRefresh)
			{
				Editor.RefreshControllerAndHierarchy(false);
				Editor.RefreshTimelineLayers(	apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, 
												parentLayer, null);

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);//이전
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();

			return newKeyframe;
		}


		/// <summary>
		/// AnimClip의 모든 레이어에 대해 Keyframe을 일괄적으로 생성한다.
		/// </summary>
		public List<apAnimKeyframe> AddAnimKeyframeToAllLayer(int targetFrame, apAnimClip animClip, bool isMakeCurrentBlendData)
		{
			if (animClip == null)
			{
				return null;
			}
			if (animClip._targetMeshGroup == null)
			{
				return null;
			}

			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_AddKeyframe,
																	Editor,
																	Editor._portrait,
																	animClip._targetMeshGroup, 
																	//null,
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);


			List<apAnimKeyframe> resultKeyframes = new List<apAnimKeyframe>();

			apAnimTimeline timeline = null;
			apAnimTimelineLayer timelineLayer = null;
			for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
			{
				timeline = animClip._timelines[iTimeline];
				for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
				{
					//각 레이어마다 Keyframe을 추가해주자
					timelineLayer = timeline._layers[iLayer];

					apAnimKeyframe existFrame = timelineLayer.GetKeyframeByFrameIndex(targetFrame);
					if (existFrame != null)
					{
						//이미 해당 프레임에 값이 있다.
						//패스
						continue;
					}

					int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
					if (nextKeyframeID < 0)
					{
						//... ID가 안나온다. 패스
						continue;
					}

					bool isIntControlParamLayer = (timelineLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
												&& (timelineLayer._linkedControlParam != null && timelineLayer._linkedControlParam._valueType == apControlParam.TYPE.Int);

					apAnimKeyframe newKeyframe = new apAnimKeyframe();

					//이전
					//newKeyframe.Init(nextKeyframeID, targetFrame, isIntControlParamLayer);

					//변경 22.1.8 : 컨트롤 파라미터의 기본값을 넣어야 한다. (블렌드는 아래에서 수행)
					if(timelineLayer._linkType == apAnimClip.LINK_TYPE.ControlParam && timelineLayer._linkedControlParam != null)
					{
						newKeyframe.Init_ControlParam(nextKeyframeID, targetFrame, isIntControlParamLayer, timelineLayer._linkedControlParam);
					}
					else
					{
						newKeyframe.Init_Modifier(nextKeyframeID, targetFrame, isIntControlParamLayer);
					}
					
					newKeyframe.Link(timelineLayer);




					timelineLayer._keyframes.Add(newKeyframe);

					if (timelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						//Modifier에 연동되는 타입이라면
						//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
						AddAndSyncAnimClipToModifier(timelineLayer._parentTimeline._parentAnimClip);
					}

					if (isMakeCurrentBlendData)
					{
						if (apEditor.IS_DEBUG_DETAILED)
						{
							Debug.LogError("TODO : Set Key -> isMakeCurrentBlendData 현재 중간 값을 이용해서 ModMesh 값을 세팅한다.");

						}
						MakeBlendModifiedDataAnimated(newKeyframe);
					}

					//Result 에 추가
					resultKeyframes.Add(newKeyframe);
				}
			}




			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(animClip));
			Editor.RefreshControllerAndHierarchy(true);

			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();

			return resultKeyframes;
		}


		public void RemoveKeyframe(apAnimKeyframe animKeyframe, bool isSetRecordAndRefresh = true)
		{
			if (animKeyframe == null)
			{
				return;
			}

			apMeshGroup targetMeshGroup = null;
			apAnimClip parentAnimClip = animKeyframe._parentTimelineLayer._parentAnimClip;
			
			targetMeshGroup = parentAnimClip._targetMeshGroup;
			
			if (isSetRecordAndRefresh)
			{
				//Undo - Remove Keyframe
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_RemoveKeyframe,
																	Editor,
																	Editor._portrait,
																	targetMeshGroup,
																	animKeyframe._parentTimelineLayer._parentTimeline._linkedModifier, 
																	//animKeyframe, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//ID 반탑
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimKeyFrame, animKeyframe._uniqueID);



			//선택중이면 제외
			if (Editor.Select.AnimKeyframe == animKeyframe || Editor.Select.AnimKeyframes.Contains(animKeyframe))
			{
				Editor.Select.SelectAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
			}

			//Timeline Layer에서 삭제
			apAnimTimelineLayer parentLayer = animKeyframe._parentTimelineLayer;
			if (parentLayer != null)
			{
				parentLayer._keyframes.Remove(animKeyframe);
			}


			if (isSetRecordAndRefresh)
			{
				//전체 Refresh를 해야한다.
				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentAnimClip));
				Editor.RefreshControllerAndHierarchy(false);
				//Editor.RefreshTimelineLayers(false);//이전

				Editor.RefreshTimelineLayers(	apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, 
												parentLayer, null);

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);//이전
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();
		}

		public void RemoveKeyframes(List<apAnimKeyframe> animKeyframes, bool isSetRecordAndRefresh = true)
		{
			if (animKeyframes == null || animKeyframes.Count == 0)
			{
				return;
			}

			apMeshGroup targetMeshGroup = null;
			apAnimClip parentAnimClip = animKeyframes[0]._parentTimelineLayer._parentAnimClip;
			
			targetMeshGroup = parentAnimClip._targetMeshGroup;

			if (isSetRecordAndRefresh)
			{
				//Undo - Remove Keyframes : 여러개를 동시에 삭제하지만 Multiple은 아니고 리스트값을 넣어주자
				apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_RemoveKeyframe,
																	Editor,
																	Editor._portrait,
																	targetMeshGroup,
																	//null,
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			List<apAnimKeyframe> targetKeyframes = new List<apAnimKeyframe>();
			for (int i = 0; i < animKeyframes.Count; i++)
			{
				targetKeyframes.Add(animKeyframes[i]);
			}



			for (int i = 0; i < targetKeyframes.Count; i++)
			{
				apAnimKeyframe animKeyframe = targetKeyframes[i];

				//ID 반납
				Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimKeyFrame, animKeyframe._uniqueID);

				//선택중이면 제외
				if (Editor.Select.AnimKeyframe == animKeyframe || Editor.Select.AnimKeyframes.Contains(animKeyframe))
				{
					Editor.Select.SelectAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}

				//Timeline Layer에서 삭제
				apAnimTimelineLayer parentLayer = animKeyframe._parentTimelineLayer;
				if (parentLayer != null)
				{
					parentLayer._keyframes.Remove(animKeyframe);
				}
			}

			if (isSetRecordAndRefresh)
			{
				//전체 Refresh를 해야한다.
				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(parentAnimClip));
				Editor.RefreshControllerAndHierarchy(true);
				//Editor.RefreshTimelineLayers(false);

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);//이전
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();
		}



		//[v1.5.0] 키프레임들을 비율에 따라 재배치하기
		private class RescaleKeyframeInfo
		{
			public apAnimKeyframe _keyframe = null;
			public int _prevFrameIndex = 0;
			public int _nextFrameIndex = 0;
			public RescaleKeyframeInfo(	apAnimKeyframe keyframe,
										int nextFrameIndex)
			{
				_keyframe = keyframe;
				_prevFrameIndex = _keyframe._frameIndex;
				_nextFrameIndex = nextFrameIndex;
			}
		}

		/// <summary>
		/// 키프레임들의 위치를 비율과 함께 일괄 변경한다. 
		/// </summary>
		/// <param name="srcFrame_Start">기존의 키프레임들의 범위 (Min)</param>
		/// <param name="srcFrame_Last">기존의 키프레임들의 범위 (Max)</param>
		/// <param name="dstFrame_Start">변경할 키프레임들의 범위 (Min)</param>
		/// <param name="dstFrame_Last">변경할 키프레임들의 범위 (Max)</param>
		public void RescaleKeyframes(apAnimClip animClip, List<apAnimKeyframe> keyframes, int srcFrame_Start, int srcFrame_Last, int dstFrame_Start, int dstFrame_Last)
		{
			//이동한게 없으면 바로 종료
			if(srcFrame_Start == dstFrame_Start && srcFrame_Last == dstFrame_Last)
			{
				return;
			}

			int nTargetKeyframes = keyframes != null ? keyframes.Count : 0;
			if(nTargetKeyframes == 0) { return; }


			//이제 키프레임을 이동시키자
			//Src, Dst의 비율을 이용하자
			int srcLength = srcFrame_Last - srcFrame_Start;
			//int dstLength = dstFrame_Last - dstFrame_Start;

			//Src Length가 1 이상일때만 비율 계산을 한다.
			//그렇지 않다면 dstStart로 다 통일한다.
			bool isSrcValidLength = srcLength > 0;
			
			//키프레임 리스트를 복사한 후 오름차순으로 정렬한다.
			List<RescaleKeyframeInfo> keyMoveInfos = new List<RescaleKeyframeInfo>();//전체 변환 정보

			apAnimKeyframe curKey = null;
			for (int i = 0; i < nTargetKeyframes; i++)
			{
				curKey = keyframes[i];
				if(curKey == null)
				{
					continue;
				}

				//프레임 변환 정보를 넣자
				int prevFrame = curKey._frameIndex;
				int nextFrame = prevFrame;
				if(isSrcValidLength)
				{
					//Src Length가 유효할 때
					//선형 보간 비율을 이용해서 계산
					float srcLerp = Mathf.Clamp01((float)(prevFrame - srcFrame_Start) / (float)srcLength);
					nextFrame = Mathf.Clamp(Mathf.RoundToInt(((1.0f - srcLerp) * (float)dstFrame_Start) + (srcLerp * (float)dstFrame_Last)), dstFrame_Start, dstFrame_Last);
				}
				else
				{
					//Src Length가 0일때
					nextFrame = dstFrame_Start;
				}

				keyMoveInfos.Add(new RescaleKeyframeInfo(curKey, nextFrame));
			}

			if(keyMoveInfos.Count == 0)
			{
				return;
			}

			//정렬을 하자 (오름차순(
			keyMoveInfos.Sort(delegate(RescaleKeyframeInfo a, RescaleKeyframeInfo b)
			{
				return a._nextFrameIndex - b._nextFrameIndex;
			});

			RescaleKeyframeInfo curInfo = null;

			//단, 겹치는 정보는 미리 삭제를 해야한다. (해당 키프레임을 삭제해야한다.)
			List<RescaleKeyframeInfo> overlappedInfos = new List<RescaleKeyframeInfo>();
			for (int i = 0; i < keyMoveInfos.Count; i++)
			{
				curInfo = keyMoveInfos[i];
				if(overlappedInfos.Contains(curInfo))
				{
					//이미 삭제 대상
					continue;
				}

				List<RescaleKeyframeInfo> findOverlapped = keyMoveInfos.FindAll(delegate(RescaleKeyframeInfo a)
				{
					if(a == curInfo)
					{
						//본인은 제외
						return false;
					}

					if(a._nextFrameIndex != curInfo._nextFrameIndex)
					{
						//위치가 겹치지 않는다.
						return false;
					}

					if(a._keyframe._parentTimelineLayer != curInfo._keyframe._parentTimelineLayer)
					{
						//같은 타임라인 레이어가 아니다.
						return false;
					}

					//겹치넹?
					return true;
				});

				int nFind = findOverlapped != null ? findOverlapped.Count : 0;
				if(nFind == 0)
				{
					//겹친게 없다.
					continue;
				}

				for (int iFind = 0; iFind < nFind; iFind++)
				{
					RescaleKeyframeInfo findOverlapInfo = findOverlapped[iFind];//겹친건 삭제 리스트에 넣자
					if(!overlappedInfos.Contains(findOverlapInfo))
					{
						overlappedInfos.Add(findOverlapInfo);
					}
				}
			}

			//변경된 타임라인 레이어와 키프레임들
			List<apAnimTimelineLayer> changedLayers = new List<apAnimTimelineLayer>();
			List<apAnimKeyframe> changedKeyframes = new List<apAnimKeyframe>();
			
			apAnimTimelineLayer curLayer = null;

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_MoveKeyframe, 
														_editor, 
														_editor._portrait, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

			if(overlappedInfos.Count > 0)
			{
				//겹친 키프레임 정보는 미리 제거한다.
				for (int i = 0; i < overlappedInfos.Count; i++)
				{
					curInfo = overlappedInfos[i];

					//변경 대상 Info에서 제외하고
					keyMoveInfos.Remove(curInfo);

					//실제로 키프레임도 제거한다.
					curLayer = curInfo._keyframe._parentTimelineLayer;
					curLayer._keyframes.Remove(curInfo._keyframe);
					if(!changedLayers.Contains(curLayer))
					{
						changedLayers.Add(curLayer);
					}
				}
			}

			//변환 정보를 하나씩 정리하자
			//TimelineLayer당 리스트를 만들고, 변경될 프레임 인덱스를 계산한다.
			int nInfo = keyMoveInfos.Count;

			
			for (int i = 0; i < nInfo; i++)
			{
				curInfo = keyMoveInfos[i];

				//이동을 시킨다.
				curInfo._keyframe._frameIndex = curInfo._nextFrameIndex;

				//겹치는걸 제거하기 위해, "변경된 키프레임"들을 별도의 리스트로 만든다.
				changedKeyframes.Add(curInfo._keyframe);
				
				curLayer = curInfo._keyframe._parentTimelineLayer;
				if(!changedLayers.Contains(curLayer))
				{
					changedLayers.Add(curLayer);
				}
			}

			//프레임이 겹치는 키프레임들을 삭제한다.

			for (int i = 0; i < changedKeyframes.Count; i++)
			{
				curKey = changedKeyframes[i];
				curLayer = curKey._parentTimelineLayer;

				//이 레이어에서 겹치는 모든 키프레임들을 삭제한다.
				if(!curLayer._keyframes.Contains(curKey))
				{
					continue;
				}

				curLayer._keyframes.RemoveAll(delegate(apAnimKeyframe a)
				{
					if(a == curKey)
					{
						//동일하면 패스
						return false;
					}

					return (a._frameIndex == curKey._frameIndex);
				});
			}

			//타임라인 레이어들을 갱신한다.
			int nLayers = changedLayers.Count;
			for (int i = 0; i < nLayers; i++)
			{
				curLayer = changedLayers[i];
				curLayer.SortAndRefreshKeyframes();
			}

			//갱신을 한다.
			_editor.SetMeshGroupChanged();

			//선택을 갱신한다.
			_editor.Select.SelectAnimMultipleKeyframes(changedKeyframes, apGizmos.SELECT_TYPE.New, false);
		}



		//AnimClip / Timeline / TimelineLayer / Keyframe과 Modifier 연동
		/// <summary>
		/// 이미 생성된 Timeline/Timelinelayer에 대해서 Modifier 내부의 ParamSetGroup/ParamSet 까지 만들어서
		/// ModMesh를 생성하게 한다. > 수정) 링크된 값에 따라 ModBone을 만든다.
		/// (중복을 체크하여 자동으로 만드므로 Refresh에 가깝다)
		/// </summary>
		/// <param name="animClip"></param>
		public void AddAndSyncAnimClipToModifier(apAnimClip animClip)
		{
			apMeshGroup targetMeshGroup = animClip._targetMeshGroup;
			if (targetMeshGroup == null)
			{
				//if (apEditor.IS_DEBUG_DETAILED)
				//{
				//	Debug.LogError("AddAndSyncAnimClipToModifier Error : Target Mesh Group이 없다. [" + animClip._name + "]");
				//}
				return;
			}
			apModifierStack modStack = targetMeshGroup._modifierStack;
			List<apAnimTimeline> timelines = animClip._timelines;

			apAnimTimeline curTimeline = null;
			apAnimTimelineLayer curLayer = null;
			apModifierParamSetGroup paramSetGroup = null;

			int nTimelines = timelines != null ? timelines.Count : 0;
			if (nTimelines > 0)
			{
				for (int iTimeline = 0; iTimeline < nTimelines; iTimeline++)
				{
					curTimeline = timelines[iTimeline];

					//if(isPrintLog)
					//{
					//	Debug.Log("Check Timeline Link [" + curTimeline.DisplayName + "]");
					//}

					if (curTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						//Modifier 연동 타입일 때
						apModifierBase modifier = curTimeline._linkedModifier;//링크가 되어있어야 한다.
						if (!modStack._modifiers.Contains(modifier)
							|| modifier == null)
						{
							//Modifier가 없는데요?
							continue;
						}

						List<apModifierParamSetGroup> paramSetGroupList = modifier._paramSetGroup_controller;
						int nModPSGList = paramSetGroupList != null ? paramSetGroupList.Count : 0;

						int nLayers = curTimeline._layers != null ? curTimeline._layers.Count : 0;

						if(nLayers == 0)
						{
							continue;
						}

						
						for (int iLayer = 0; iLayer < nLayers; iLayer++)
						{
							curLayer = curTimeline._layers[iLayer];

							//연결된 paramSetGroup을 찾자
							paramSetGroup = curLayer._targetParamSetGroup;

							//이 PSG가 유효한지 체크한다. (v1.5.0)
							bool isValidPSG = false;
							if(paramSetGroup != null)
							{
								if(paramSetGroup._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame
									&& paramSetGroup._keyAnimClipID == animClip._uniqueID
									&& paramSetGroup._keyAnimTimelineID == curTimeline._uniqueID
									&& paramSetGroup._keyAnimTimelineLayerID == curLayer._uniqueID)
								{
									if(nModPSGList > 0)
									{
										if(paramSetGroupList.Contains(paramSetGroup))
										{
											isValidPSG = true;
										}
									}
								}
							}

							//유효하지 않다면 다시 찾아서 연결한다.
							if (!isValidPSG)
							{
								paramSetGroup = null;

								if (nModPSGList > 0)
								{
									//이전 (GC 발생)
									//paramSetGroup = paramSetGroupList.Find(delegate (apModifierParamSetGroup a)
									//{
									//	if (a._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
									//	{
									//		if (a._keyAnimClipID == animClip._uniqueID &&
									//			a._keyAnimTimelineID == curTimeline._uniqueID &&
									//			a._keyAnimTimelineLayerID == curLayer._uniqueID)
									//		{
									//			//ID가 모두 동일하다 (링크가 될 paramSetGroup이 이미 만들어졌다.
									//			return true;
									//		}
									//	}

									//	return false;
									//});

									//변경 v1.5.0
									s_FindAnimMatchModPSG_AnimClipID = animClip._uniqueID;
									s_FindAnimMatchModPSG_TimelineID = curTimeline._uniqueID;
									s_FindAnimMatchModPSG_LayerID = curLayer._uniqueID;

									paramSetGroup = paramSetGroupList.Find(s_FindAnimMatchModPSG_Func);
								}
								
							}
							

							bool isAddParamSetGroup = false;
							if (paramSetGroup == null)
							{
								isAddParamSetGroup = true;
								
								paramSetGroup = new apModifierParamSetGroup(Editor._portrait, modifier, modifier.GetNextParamSetLayerIndex());
								paramSetGroup.SetTimeline(animClip, curTimeline, curLayer);
								paramSetGroup.LinkPortrait(Editor._portrait, modifier);

								modifier._paramSetGroup_controller.Add(paramSetGroup);
								//여기서는 ParamSet을 등록하진 않는다.

								//추가 v1.5.0 : 이게 왜 빠졌지
								curLayer.LinkParamSetGroup(paramSetGroup);
							}
							else
							{
								//Debug.Log("Link ParamSetGroup <- " + curLayer.DisplayName);
								//연동을 한번 더 해주자
								paramSetGroup._keyAnimClip = animClip;
								paramSetGroup._keyAnimTimeline = curTimeline;
								paramSetGroup._keyAnimTimelineLayer = curLayer;
								curLayer.LinkParamSetGroup(paramSetGroup);
							}

							//연동될 Tranform이 있는지 확인하자
							int linkedTransformID = -1;
							apTransform_Mesh linkedMeshTransform = null;
							apTransform_MeshGroup linkedMeshGroupTransform = null;
							apBone linkedBone = null;
							int linkedBoneID = -1;

							if (curLayer._linkedMeshTransform != null)
							{
								linkedMeshTransform = curLayer._linkedMeshTransform;
								linkedTransformID = linkedMeshTransform._transformUniqueID;
							}
							else if (curLayer._linkedMeshGroupTransform != null)
							{
								linkedMeshGroupTransform = curLayer._linkedMeshGroupTransform;
								linkedTransformID = linkedMeshGroupTransform._transformUniqueID;
							}
							else if (curLayer._linkedBone != null)
							{
								linkedBone = curLayer._linkedBone;
								linkedBoneID = linkedBone._uniqueID;
							}
							

							//Key를 추가해준다.
							List<apModifierParamSet> paramSetList = paramSetGroup._paramSetList;
							apAnimKeyframe curKeyframe = null;
							apModifierParamSet targetParamSet = null;

							int nKeyframes = curLayer._keyframes != null ? curLayer._keyframes.Count : 0;

							if (nKeyframes > 0)
							{
								for (int iKeyframe = 0; iKeyframe < nKeyframes; iKeyframe++)
								{
									curKeyframe = curLayer._keyframes[iKeyframe];

									targetParamSet = curKeyframe._linkedParamSet_Editor;
									bool isValidPS = false;
									if(targetParamSet != null)
									{
										if(targetParamSet.SyncKeyframe == curKeyframe
											&& paramSetList != null
											&& paramSetList.Contains(targetParamSet))
										{
											//유효한 ParamSet이다.
											isValidPS = true;
										}
									}

									if(!isValidPS)
									{
										//유효한 ParamSet이 아니라면
										targetParamSet = null;

										//이전 (GC 발생)
										//targetParamSet = paramSetList.Find(delegate (apModifierParamSet a)
										//{
										//	return a.SyncKeyframe == curKeyframe;
										//});

										//변경 v1.5.0
										s_FindAnimMatchModParamSet_Keyframe = curKeyframe;
										targetParamSet = paramSetList.Find(s_FindAnimMatchModParamSet_Func);
									}
									

									if (targetParamSet == null)
									{
										//없다면 생성
										targetParamSet = new apModifierParamSet();
										targetParamSet.LinkParamSetGroup(paramSetGroup);
										targetParamSet.LinkSyncKeyframe(curKeyframe);

										paramSetList.Add(targetParamSet);
									}
									else
									{
										//이미 있다면 서로 연결
										targetParamSet.LinkParamSetGroup(paramSetGroup);
										targetParamSet.LinkSyncKeyframe(curKeyframe);
									}


									if (linkedTransformID >= 0)
									{
										apModifiedMesh addedModMesh = null;
										//Modifier에 연동을 해주자
										if (linkedMeshTransform != null)
										{
											if (isAddParamSetGroup)
											{
												//Debug.Log("Add ModMesh > MeshTransform");
											}
											//ModeMesh는 추가하되, Refresh는 나중에 하자 (마지막 인자를 false로 둔다)
											addedModMesh = modifier.AddMeshTransform(targetMeshGroup, linkedMeshTransform, targetParamSet, true, true, false);
										}
										else if (linkedMeshGroupTransform != null)
										{
											if (isAddParamSetGroup)
											{
												//Debug.Log("Add ModMesh > MeshGroupTransform");
											}
											//ModeMesh는 추가하되, Refresh는 나중에 하자 (마지막 인자를 false로 둔다)
											addedModMesh = modifier.AddMeshGroupTransform(targetMeshGroup, linkedMeshGroupTransform, targetParamSet, true, true, false);
										}
										if (addedModMesh == null)
										{
											//Debug.LogError("Add Mod Mesh Failed");
											curKeyframe.LinkModMesh_Editor(targetParamSet, null);
										}
										else
										{
											curKeyframe.LinkModMesh_Editor(targetParamSet, addedModMesh);
										}
									}
									else if (linkedBoneID > 0)
									{
										//Bone 추가
										apModifiedBone addModBone = modifier.AddBone(linkedBone, targetParamSet, true, false);
										if (addModBone == null)
										{
											curKeyframe.LinkModBone_Editor(targetParamSet, null);
										}
										else
										{
											curKeyframe.LinkModBone_Editor(targetParamSet, addModBone);
										}
									}

								}
							}
							paramSetGroup.RefreshSync();
						}

					}
				}
			}
			
			//ModMesh 링크를 여기서 일괄적으로 처리
			targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_AnimClip(animClip));

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//				targetMeshGroup,
			//				null,
			//				null,
			//				animClip,
			//				true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);
		}


		private static int s_FindAnimMatchModPSG_AnimClipID = -1;
		private static int s_FindAnimMatchModPSG_TimelineID = -1;
		private static int s_FindAnimMatchModPSG_LayerID = -1;
		private static Predicate<apModifierParamSetGroup> s_FindAnimMatchModPSG_Func = FUNC_FindAnimMatchModPSG;
		private static bool FUNC_FindAnimMatchModPSG(apModifierParamSetGroup a)
		{
			if (a._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
			{
				if (a._keyAnimClipID == s_FindAnimMatchModPSG_AnimClipID &&
					a._keyAnimTimelineID == s_FindAnimMatchModPSG_TimelineID &&
					a._keyAnimTimelineLayerID == s_FindAnimMatchModPSG_LayerID)
				{
					//ID가 모두 동일하다 (링크가 될 paramSetGroup이 이미 만들어졌다.
					return true;
				}
			}

			return false;
		}

		private static apAnimKeyframe s_FindAnimMatchModParamSet_Keyframe = null;
		private static Predicate<apModifierParamSet> s_FindAnimMatchModParamSet_Func = FUNC_FindAnimMatchModParamSet;
		private static bool FUNC_FindAnimMatchModParamSet(apModifierParamSet a)
		{
			return a.SyncKeyframe == s_FindAnimMatchModParamSet_Keyframe//이게 Null이라도
				|| a._keyframeUniqueID == s_FindAnimMatchModParamSet_Keyframe._uniqueID;//ID도 체크
		}



		// ModMesh/ModBone을 만들때, 기본 값이 아니라 두개의 키프레임 도는 몇개의 ParamSet으로부터 보간하여 값을 만들때의 함수.
		//여기서는 일단 애니메이션 키프레임의 중간값을 가져오는 것만 생각한다.
		public void MakeBlendModifiedDataAnimated(apAnimKeyframe targetKeyframe)
		{
			apAnimTimelineLayer timelineLayer = targetKeyframe._parentTimelineLayer;
			apAnimTimeline timeline = timelineLayer._parentTimeline;
			apAnimClip animClip = timeline._parentAnimClip;
			//apMeshGroup targetMeshGroup = animClip._targetMeshGroup;

			int targetFrameIndex = targetKeyframe._frameIndex;

			//앞쪽 Keyframe과 뒤쪽 Keyframe을 가져온다.
			//1) 없는 경우 -> 리턴
			//2) 하나만 있는 경우 -> 그 키프레임의 값을 복사한다.
			//3) 두개가 있는 경우 -> Curve 보간을 이용하여 중간 값을 가져온다.

			//두개의 값을 가져올 때 -> Loop 타입 여부에 따라 키를 가져와야 한다.
			apAnimKeyframe prevKeyframe = null;
			apAnimKeyframe nextKeyframe = null;

			apAnimKeyframe minKeyframe = null;
			apAnimKeyframe maxKeyframe = null;

			bool isLoop = animClip.IsLoop;

			List<apAnimKeyframe> keyframes = timelineLayer._keyframes;

			//1. 일단 Loop 없이 키값을 가져온다.

			apAnimKeyframe curKeyframe = null;
			for (int i = 0; i < keyframes.Count; i++)
			{
				curKeyframe = keyframes[i];
				if (curKeyframe == targetKeyframe)
				{
					continue;
				}

				if (minKeyframe == null || curKeyframe._frameIndex < minKeyframe._frameIndex)
				{
					//가장 앞쪽의 프레임을 찾자
					minKeyframe = curKeyframe;
				}

				if (maxKeyframe == null || curKeyframe._frameIndex > maxKeyframe._frameIndex)
				{
					//가장 뒤쪽의 프레임을 찾자
					maxKeyframe = curKeyframe;
				}

				if (curKeyframe._frameIndex < targetFrameIndex)
				{
					//이전 프레임일때
					if (prevKeyframe == null || curKeyframe._frameIndex > prevKeyframe._frameIndex)
					{
						//이전 프레임 중 [최대 프레임]을 찾는다.
						prevKeyframe = curKeyframe;
					}
				}
				else if (curKeyframe._frameIndex > targetFrameIndex)
				{
					//다음 프레임일때
					if (nextKeyframe == null || curKeyframe._frameIndex < nextKeyframe._frameIndex)
					{
						//다음 프레임 중 [최소 프레임]을 찾는다.
						nextKeyframe = curKeyframe;
					}
				}
			}

			if (prevKeyframe == null && nextKeyframe == null)
			{
				//1) 둘다 없을 때 -> 리턴
				//Debug.LogError("Blend : No Keyframes");
				return;
			}

			//하나만 있다면 + Loop일때 반대편 Keyframe을 찾자
			if (prevKeyframe == null || nextKeyframe == null)
			{
				if (isLoop)
				{
					if (prevKeyframe == null)
					{
						//Prev가 Null이라면
						// (Loop Max) .... [Target] - [Next]
						//Max Keyframe을 검토하여 Prev로 넣어주자
						if (maxKeyframe != null && nextKeyframe != maxKeyframe)
						{
							prevKeyframe = maxKeyframe;
						}
					}
					else if (nextKeyframe == null)
					{
						//Next가 Null이라면
						// [Prev] - [Target] ...... (Loop Min)
						//Min Keyframe을 검토하여 Next로 넣어주자
						if (minKeyframe != null && prevKeyframe != minKeyframe)
						{
							nextKeyframe = minKeyframe;
						}
					}
				}
			}

			//3) 둘다 있을때를 먼저 검토한다.
			if (prevKeyframe != null && nextKeyframe != null)
			{
				//Debug.Log("Blend : 2 Keyframes");
				int prevFrameIndex = prevKeyframe._frameIndex;
				int nextFrameIndex = nextKeyframe._frameIndex;

				if (prevFrameIndex > targetFrameIndex && isLoop)
				{
					prevFrameIndex = prevKeyframe._loopFrameIndex;
				}

				if (nextFrameIndex < targetFrameIndex && isLoop)
				{
					nextFrameIndex = nextKeyframe._loopFrameIndex;
				}

				float itp = apAnimCurveResult.CalculateInterpolation_Float(
										(float)targetFrameIndex, targetFrameIndex,
										prevFrameIndex, nextFrameIndex,
										prevKeyframe._curveKey,
										nextKeyframe._curveKey);

				//itp = 0 : A값을 사용
				//itp = 1 : B값을 사용

				//1. 키프레임의 값을 보간하여 적용. CurveKey 포함
				targetKeyframe._curveKey = new apAnimCurve(prevKeyframe._curveKey, nextKeyframe._curveKey, targetFrameIndex);
				targetKeyframe._isKeyValueSet = prevKeyframe._isKeyValueSet;

				//Controller Param 값을 입력
				targetKeyframe._conSyncValue_Int = (int)((((float)(prevKeyframe._conSyncValue_Int) * (1 - itp)) + ((float)(nextKeyframe._conSyncValue_Int) * (itp))) + 0.5f);
				targetKeyframe._conSyncValue_Float = (prevKeyframe._conSyncValue_Float * (1 - itp)) + (nextKeyframe._conSyncValue_Float * itp);
				targetKeyframe._conSyncValue_Vector2 = (prevKeyframe._conSyncValue_Vector2 * (1 - itp)) + (nextKeyframe._conSyncValue_Vector2 * itp);

				//2. Linked ModMesh를 수정
				if (targetKeyframe._linkedModMesh_Editor != null &&
					prevKeyframe._linkedModMesh_Editor != null &&
					nextKeyframe._linkedModMesh_Editor != null)
				{
					List<apModifiedVertex> prevVertList = prevKeyframe._linkedModMesh_Editor._vertices;
					apMatrix prevTransformMatrix = prevKeyframe._linkedModMesh_Editor._transformMatrix;
					Color prevMeshColor = prevKeyframe._linkedModMesh_Editor._meshColor;
					bool prevIsVisible = prevKeyframe._linkedModMesh_Editor._isVisible;
					if (!prevIsVisible)
					{
						prevMeshColor.a = 0.0f;
					}
					List<apModifiedPin> prevPinList = prevKeyframe._linkedModMesh_Editor._pins;//추가 22.3.20

					List<apModifiedVertex> nextVertList = nextKeyframe._linkedModMesh_Editor._vertices;
					apMatrix nextTransformMatrix = nextKeyframe._linkedModMesh_Editor._transformMatrix;
					Color nextMeshColor = nextKeyframe._linkedModMesh_Editor._meshColor;
					bool nextIsVisible = nextKeyframe._linkedModMesh_Editor._isVisible;
					if (!nextIsVisible)
					{
						nextMeshColor.a = 0.0f;
					}
					List<apModifiedPin> nextPinList = nextKeyframe._linkedModMesh_Editor._pins;//추가 22.3.20

					targetKeyframe._linkedModMesh_Editor._transformMatrix.SetZero();
					targetKeyframe._linkedModMesh_Editor._transformMatrix._pos = (prevTransformMatrix._pos * (1 - itp)) + (nextTransformMatrix._pos * itp);
					targetKeyframe._linkedModMesh_Editor._transformMatrix._angleDeg = (prevTransformMatrix._angleDeg * (1 - itp)) + (nextTransformMatrix._angleDeg * itp);
					targetKeyframe._linkedModMesh_Editor._transformMatrix._scale = (prevTransformMatrix._scale * (1 - itp)) + (nextTransformMatrix._scale * itp);
					targetKeyframe._linkedModMesh_Editor._transformMatrix.MakeMatrix();


					targetKeyframe._linkedModMesh_Editor._meshColor = (prevMeshColor * (1 - itp)) + (nextMeshColor * itp);
					targetKeyframe._linkedModMesh_Editor._isVisible = prevIsVisible | nextIsVisible;//<<하나라도 true이면 오케이

					if (prevVertList != null && nextVertList != null && prevVertList.Count > 0)
					{
						apModifiedVertex prevModVert = null;
						apModifiedVertex nextModVert = null;
						apModifiedVertex dstModVert = null;

						for (int i = 0; i < prevVertList.Count; i++)
						{
							prevModVert = prevVertList[i];
							//같은 인덱스를 먼저 찾고, 다르면 일일이 Find
							nextModVert = nextVertList[i];

							if (nextModVert._vertexUniqueID != prevModVert._vertexUniqueID)
							{
								nextModVert = nextVertList.Find(delegate (apModifiedVertex a)
								{
									return a._vertexUniqueID == prevModVert._vertexUniqueID;
								});
							}

							dstModVert = targetKeyframe._linkedModMesh_Editor._vertices[i];

							if (dstModVert._vertexUniqueID != prevModVert._vertexUniqueID)
							{
								dstModVert = targetKeyframe._linkedModMesh_Editor._vertices.Find(delegate (apModifiedVertex a)
								{
									return a._vertexUniqueID == prevModVert._vertexUniqueID;
								});
							}

							//Vertex 보간
							if (dstModVert != null && nextModVert != null)
							{
								dstModVert._deltaPos = (prevModVert._deltaPos * (1 - itp)) + (nextModVert._deltaPos * itp);
							}
						}
					}

					//추가 22.3.20 [v1.4.0] Pin 복사
					
					List<apModifiedPin> targetPinList = targetKeyframe._linkedModMesh_Editor._pins;
					
					
					if(prevPinList != null && nextPinList != null && targetPinList != null)
					{
						apModifiedPin prevModPin = null;
						apModifiedPin nextModPin = null;
						apModifiedPin dstModPin = null;

						for (int i = 0; i < prevPinList.Count; i++)
						{
							prevModPin = prevPinList[i];
							nextModPin = nextPinList[i];

							if(nextModPin._pinUniqueID != prevModPin._pinUniqueID)
							{
								nextModPin = nextPinList.Find(delegate(apModifiedPin a)
								{
									return a._pinUniqueID == prevModPin._pinUniqueID;
								});
							}

							dstModPin = targetPinList[i];
							if(dstModPin._pinUniqueID != prevModPin._pinUniqueID)
							{
								dstModPin = targetPinList.Find(delegate(apModifiedPin a)
								{
									return a._pinUniqueID == prevModPin._pinUniqueID;
								});
							}

							if(dstModPin != null && nextModPin != null)
							{
								dstModPin._deltaPos = (prevModPin._deltaPos * (1 - itp)) + (nextModPin._deltaPos * itp);
							}
						}
					}

				}

				//3. Linked ModBone을 수정
				if (targetKeyframe._linkedModBone_Editor != null &&
					prevKeyframe._linkedModBone_Editor != null &&
					nextKeyframe._linkedModBone_Editor != null)
				{
					apMatrix prevTransformMatrix = prevKeyframe._linkedModBone_Editor._transformMatrix;
					apMatrix nextTransformMatrix = nextKeyframe._linkedModBone_Editor._transformMatrix;

					targetKeyframe._linkedModBone_Editor._transformMatrix.SetZero();
					targetKeyframe._linkedModBone_Editor._transformMatrix._pos = (prevTransformMatrix._pos * (1 - itp)) + (nextTransformMatrix._pos * itp);
					targetKeyframe._linkedModBone_Editor._transformMatrix._angleDeg = (prevTransformMatrix._angleDeg * (1 - itp)) + (nextTransformMatrix._angleDeg * itp);
					targetKeyframe._linkedModBone_Editor._transformMatrix._scale = (prevTransformMatrix._scale * (1 - itp)) + (nextTransformMatrix._scale * itp);
					targetKeyframe._linkedModBone_Editor._transformMatrix.MakeMatrix();
				}


			}
			else if (prevKeyframe != null || nextKeyframe != null)
			{
				//2) 한개만 유효한 경우 그냥 복사
				apAnimKeyframe srcKeyframe = null;
				if (prevKeyframe != null)
				{
					srcKeyframe = prevKeyframe;
				}
				else
				{
					srcKeyframe = nextKeyframe;
				}

				//Debug.Log("Blend : 1 Keyframe");


				targetKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, targetFrameIndex);
				targetKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;

				targetKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
				targetKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
				targetKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;


				if (targetKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//Mod Mesh 값을 복사하자
					List<apModifiedVertex> srcVertList = srcKeyframe._linkedModMesh_Editor._vertices;
					apMatrix srcTransformMatrix = srcKeyframe._linkedModMesh_Editor._transformMatrix;
					Color srcMeshColor = srcKeyframe._linkedModMesh_Editor._meshColor;
					bool isVisible = srcKeyframe._linkedModMesh_Editor._isVisible;

					List<apModifiedPin> srcPinList = srcKeyframe._linkedModMesh_Editor._pins;

					targetKeyframe._linkedModMesh_Editor._transformMatrix.SetMatrix(srcTransformMatrix, true);
					targetKeyframe._linkedModMesh_Editor._meshColor = srcMeshColor;
					targetKeyframe._linkedModMesh_Editor._isVisible = isVisible;

					apModifiedVertex srcModVert = null;
					apModifiedVertex dstModVert = null;
					for (int i = 0; i < srcVertList.Count; i++)
					{
						srcModVert = srcVertList[i];
						dstModVert = targetKeyframe._linkedModMesh_Editor._vertices[i];

						if (dstModVert._vertexUniqueID != srcModVert._vertexUniqueID)
						{
							dstModVert = targetKeyframe._linkedModMesh_Editor._vertices.Find(delegate (apModifiedVertex a)
							{
								return a._vertexUniqueID == srcModVert._vertexUniqueID;
							});
						}

						if (dstModVert != null)
						{
							dstModVert._deltaPos = srcModVert._deltaPos;
						}
					}


					//추가 22.3.20 [v1.4.0] Pin 복사
					
					List<apModifiedPin> targetPinList = targetKeyframe._linkedModMesh_Editor._pins;
					
					
					if(srcPinList != null && targetPinList != null)
					{
						apModifiedPin srcModPin = null;
						apModifiedPin dstModPin = null;

						for (int i = 0; i < srcPinList.Count; i++)
						{
							srcModPin = srcPinList[i];
							
							dstModPin = targetPinList[i];
							if(dstModPin._pinUniqueID != srcModPin._pinUniqueID)
							{
								dstModPin = targetPinList.Find(delegate(apModifiedPin a)
								{
									return a._pinUniqueID == srcModPin._pinUniqueID;
								});
							}

							if(dstModPin != null)
							{
								dstModPin._deltaPos = srcModPin._deltaPos;
							}
						}
					}
				}

				if (targetKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//ModBone도 복사하자
					if (targetKeyframe._linkedModBone_Editor._transformMatrix == null)
					{
						targetKeyframe._linkedModBone_Editor._transformMatrix = new apMatrix();
					}
					targetKeyframe._linkedModBone_Editor._transformMatrix.SetMatrix(srcKeyframe._linkedModBone_Editor._transformMatrix, true);
				}
			}
			else
			{
				//엥 둘다 없네요 이 무슨..
				//Debug.LogError("Blend : No Keyframes");
				return;
			}

		}







		public void CopyAnimCurveToAllKeyframes(apAnimCurveResult srcCurveResult, apAnimTimelineLayer animLayer, apAnimClip animClip)
		{
			if (srcCurveResult == null || animLayer == null || animClip == null)
			{
				Debug.LogError("CopyAnimCurveToAllKeyframes Failed : Case 1");
				return;
			}
			if (srcCurveResult._curveKeyA == null || srcCurveResult._curveKeyB == null)
			{
				Debug.LogError("CopyAnimCurveToAllKeyframes Failed : Case 2");
				return;
			}

			//변경 3.30 : 적용 범위 : 현재 타임라인 레이어만 / 모든 타임라인 레이어 / 취소
			int iBtn = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_Title),
													Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_Body),
													Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_SelectedLayer),
													Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_AllLayer),
													Editor.GetText(TEXT.Cancel));

			if (iBtn == 2)
			{
				//취소했다.
				return;
			}
			bool isCopyToAllLayers = (iBtn == 1);

			//이건 Modified를 수정하지 않으므로 Portrait만 수정하자
			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
												Editor, 
												Editor._portrait, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			if (isCopyToAllLayers)
			{
				//모든 레이어의 키프레임들에 복사 << 추가 3.31
				apAnimTimeline curTimeline = null;
				apAnimTimelineLayer curLayer = null;
				apAnimKeyframe keyframe = null;
				for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
				{
					curTimeline = animClip._timelines[iTimeline];
					for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
					{
						curLayer = curTimeline._layers[iLayer];

						for (int iKey = 0; iKey < curLayer._keyframes.Count; iKey++)
						{
							keyframe = curLayer._keyframes[iKey];

							if (keyframe._curveKey._prevCurveResult != srcCurveResult)
							{
								keyframe._curveKey._prevCurveResult.CopyCurve(srcCurveResult);
							}


							if (keyframe._curveKey._nextCurveResult != srcCurveResult)
							{
								keyframe._curveKey._nextCurveResult.CopyCurve(srcCurveResult);
							}
						}
					}
				}

			}
			else
			{
				//현재 레이어의 키프레임들에만 복사
				for (int i = 0; i < animLayer._keyframes.Count; i++)
				{
					apAnimKeyframe keyframe = animLayer._keyframes[i];
					if (keyframe._curveKey._prevCurveResult != srcCurveResult)
					{
						keyframe._curveKey._prevCurveResult.CopyCurve(srcCurveResult);
					}


					if (keyframe._curveKey._nextCurveResult != srcCurveResult)
					{
						keyframe._curveKey._nextCurveResult.CopyCurve(srcCurveResult);
					}
				}
			}



		}


		//추가 3.30 : 키프레임을 SnapShot에서 꺼내서 복사한다.
		public void CopyAnimKeyframeFromSnapShot(apAnimClip animClip, int frameIndex)
		{
			if (!apSnapShotManager.I.IsKeyframesPastableOnTimelineUI(animClip))
			{
				//추가 19.6.27 : 다른 AnimClip에 복사를 할 수 있다.
				//만약 AnimClip만 다른 것이라면?
				if (apSnapShotManager.I.IsKeyframesPastableOnTimelineUI_ToOtherAnimClip(animClip))
				{
					//다른 애니메이션 클립으로 키프레임을 복사하자.
					CopyAnimKeyframeFromSnapShotToOtherAnimClip(animClip, frameIndex);
				}
				return;
			}

			List<apSnapShotStackUnit> snapshotData = apSnapShotManager.I.GetKeyframesOnTimelineUI();
			int snapshotFrame = apSnapShotManager.I.StartFrameOfKeyframesOnTimelineUI;

			//얼마나 이동되어 복붙해야하는가
			int deltaFrame = frameIndex - snapshotFrame;

			//Record
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_CopyKeyframe, Editor,
																	Editor._portrait,
																	animClip._targetMeshGroup,
																	//animClip,
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

			apSnapShot_Keyframe curSnapshot = null;
			for (int iData = 0; iData < snapshotData.Count; iData++)
			{
				curSnapshot = snapshotData[iData]._snapShot as apSnapShot_Keyframe;
				if (curSnapshot == null || curSnapshot.KeyTimelineLayer == null)
				{
					continue;
				}

				//1. 목표 프레임을 결정한다.
				int dstFrameIndex = curSnapshot.SavedFrameIndex + deltaFrame;


				//2. 키프레임을 생성한다. 
				//버그 수정 19.6.27 : 기존의 키프레임이 있다면 그걸 이용, 그렇지 않다면 삭제하는 것으로..
				apAnimKeyframe targetKeyframe = curSnapshot.KeyTimelineLayer.GetKeyframeByFrameIndex(dstFrameIndex);
				if (targetKeyframe == null)
				{
					targetKeyframe = AddAnimKeyframe(dstFrameIndex, curSnapshot.KeyTimelineLayer, false, false, false, false);
				}
				//else
				//{
				//	Debug.Log("기존의 키프레임 [" + dstFrameIndex + "]에 복사했다");
				//}


				if (targetKeyframe != null)
				{
					//데이터를 복사한다.
					curSnapshot.Load(targetKeyframe);
				}


			}

			//Refresh
			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(animClip));
			Editor.RefreshControllerAndHierarchy(true);


			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

		}


		public void CopyAnimKeyframeFromSnapShotToOtherAnimClip(apAnimClip targetAnimClip, int frameIndex)
		{
			List<apSnapShotStackUnit> snapshotData = apSnapShotManager.I.GetKeyframesOnTimelineUI();
			int snapshotFrame = apSnapShotManager.I.StartFrameOfKeyframesOnTimelineUI;

			//두가지를 물어봐야한다.
			//1. (프레임이 다른 경우)
			//2. (타임라인이나 타임라인 레이어가 없는 경우)

			bool isCheck_NoTimelineLayer = false;//질문을 했는지 확인

			bool isCopyCreatingNewTimelineLayer = false;


			//얼마나 이동되어 복붙해야하는가
			int deltaFrame = frameIndex - snapshotFrame;

			if (deltaFrame != 0)
			{
				//1. (프레임이 다른 경우) "저장된 프레임과 복사하고자 하는 프레임이 같지 않습니다" > (1) 현재 프레임에 복사하기 (2) 저장된 프레임에 복사하기 (3) 취소
				//"Select a frame to copy"
				//"The current frame and the saved frame are not the same.\nWhere do you want to copy?\nCurrent : " + frameIndex + " / Saved : " + snapshotFrame
				//"Current Frame"
				//"Saved Frame"
				int resultBtn = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Title),
																	Editor.GetTextFormat(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Body, frameIndex, snapshotFrame),
																	Editor.GetText(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Current),
																	Editor.GetText(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Saved),
																	Editor.GetText(TEXT.Cancel));

				if (resultBtn == 0)
				{
					//현재 프레임에 저장한다. -> Delta Frame 유지하기
				}
				else if (resultBtn == 1)
				{
					//저장된 프레임에 저장한다. (deltaFrame을 0으로 고정)
					deltaFrame = 0;
				}
				else
				{
					//복사 취소
					return;
				}
			}


			bool isAnyAdded = false;

			//Record
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_CopyKeyframe, Editor,
																	Editor._portrait,
																	targetAnimClip._targetMeshGroup,
																	//targetAnimClip,
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

			apSnapShot_Keyframe curSnapshot = null;
			for (int iData = 0; iData < snapshotData.Count; iData++)
			{
				curSnapshot = snapshotData[iData]._snapShot as apSnapShot_Keyframe;
				if (curSnapshot == null || curSnapshot.KeyTimelineLayer == null
					)
				{
					continue;
				}

				//1. 목표 프레임을 결정한다.
				int dstFrameIndex = curSnapshot.SavedFrameIndex + deltaFrame;

				//2. <중요> 타겟이 될 TimelineLayer를 찾거나 연결하자.
				//서로 다른 AnimClip이므로, 최대한 유사한 TimelineLayer를 연결해야한다.
				apAnimTimelineLayer srcTimelineLayer = curSnapshot.KeyTimelineLayer;
				apAnimTimeline srcTimeline = srcTimelineLayer._parentTimeline;

				apAnimTimeline targetTimeline = null;
				apAnimTimelineLayer targetTimelineLayer = null;

				//복사 가능한 Src인지 확인한다.
				if (srcTimeline == null)
				{
					continue;
				}
				if (srcTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
				{
					if (srcTimelineLayer._linkedControlParam == null)
					{
						//유효한 ControlParam이 없다.
						continue;
					}
				}
				else
				{
					if (srcTimeline._linkedModifier == null)
					{
						//연결된 유효한 Modifier가 없다.
						continue;
					}

					if (srcTimelineLayer._linkedMeshTransform == null
						&& srcTimelineLayer._linkedMeshGroupTransform == null
						&& srcTimelineLayer._linkedBone == null)
					{
						//연결된 대상이 아무것도 없다.
						continue;
					}
				}

				//적절한 TimelineLayer를 찾자
				switch (srcTimeline._linkType)
				{
					//1) Timeline이 Control Param 타입일 때
					case apAnimClip.LINK_TYPE.ControlParam:
						{
							apControlParam linkedControlParam = srcTimelineLayer._linkedControlParam;

							targetTimeline = targetAnimClip._timelines.Find(delegate (apAnimTimeline a)
							{
								return a._linkType == apAnimClip.LINK_TYPE.ControlParam;
							});

							if (targetTimeline != null)
							{
								targetTimelineLayer = targetTimeline._layers.Find(delegate (apAnimTimelineLayer a)
								{
									return a._linkedControlParam == linkedControlParam;
								});
							}
						}
						break;

					//2) Timeline이 Modifier에 연결되는 타입일 때
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						{
							apModifierBase lnkedModifier = srcTimeline._linkedModifier;

							targetTimeline = targetAnimClip._timelines.Find(delegate (apAnimTimeline a)
							{
								return a._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
										&& a._linkedModifier == lnkedModifier;
							});

							if (targetTimeline != null)
							{
								if (srcTimelineLayer._linkedMeshTransform != null)
								{
									//MeshTransform과 연결된 경우
									targetTimelineLayer = targetTimeline._layers.Find(delegate (apAnimTimelineLayer a)
									{
										return a._linkedMeshTransform == srcTimelineLayer._linkedMeshTransform;
									});
								}

								if (srcTimelineLayer._linkedMeshGroupTransform != null)
								{
									//MeshGroupTransform과 연결된 경우
									targetTimelineLayer = targetTimeline._layers.Find(delegate (apAnimTimelineLayer a)
									{
										return a._linkedMeshGroupTransform == srcTimelineLayer._linkedMeshGroupTransform;
									});
								}

								if (srcTimelineLayer._linkedBone != null)
								{
									//Bone과 연결된 경우
									targetTimelineLayer = targetTimeline._layers.Find(delegate (apAnimTimelineLayer a)
									{
										return a._linkedBone == srcTimelineLayer._linkedBone;
									});
								}
							}
						}
						break;
				}

				//적절한TimelineLayer를 찾지 못한 경우
				if (targetTimelineLayer == null)
				{
					if (!isCheck_NoTimelineLayer)
					{
						//질문을 해봅시더
						//2. (타임라인이나 타임라인 레이어가 없는 경우) "키프레임이 복사될 적절한 타임라인 레이어가 없습니다. 타임라인 레이어를 새로 생성할까요?" > (1) 생성하고 복사하기 (2) 무시하기
						//"No Timeline Layer"
						//"There is no appropriate Timeline Layer to copy the keyframes. Create a new Timeline Layer?"
						//"Ignore"
						bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.DLG_NoTimelineLayerCopingKeyframes_Title),
																	Editor.GetText(TEXT.DLG_NoTimelineLayerCopingKeyframes_Body),
																	Editor.GetText(TEXT.Okay),
																	Editor.GetText(TEXT.Ignore));

						//일단 질문은 끝
						isCheck_NoTimelineLayer = true;

						if (result)
						{
							//Timeline과 Timeline Layer를 생성하면서 복사한다.
							isCopyCreatingNewTimelineLayer = true;
						}
						else
						{
							//존재하지 않는 Timeline Layer는 무시한다.
							isCopyCreatingNewTimelineLayer = false;
						}
					}

					//"무시하기"를 선택한 경우
					if (!isCopyCreatingNewTimelineLayer)
					{
						//없으면 Skip
						continue;
					}

					//타임라인 레이어를 새로 만들자

					if (targetTimeline == null)
					{
						//먼저, Timeline이 없다면 SrcTimeline의 정보를 이용해서 생성하자.
						targetTimeline = AddAnimTimeline(srcTimeline._linkType, srcTimeline._modifierUniqueID, targetAnimClip, false, false);
					}

					if (targetTimeline == null)
					{
						Debug.LogError("AnyPortrait : Creating a new Timeline while copying keyframes failed.");
						continue;
					}

					object targetObject = null;
					if (targetTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						if (srcTimelineLayer._linkedMeshTransform != null) { targetObject = srcTimelineLayer._linkedMeshTransform; }
						else if (srcTimelineLayer._linkedMeshGroupTransform != null) { targetObject = srcTimelineLayer._linkedMeshGroupTransform; }
						else if (srcTimelineLayer._linkedBone != null) { targetObject = srcTimelineLayer._linkedBone; }
					}
					else
					{
						if (srcTimelineLayer._linkedControlParam != null) { targetObject = srcTimelineLayer._linkedControlParam; }
					}

					targetTimelineLayer = AddAnimTimelineLayer(targetObject, targetTimeline, false);

					if (targetTimelineLayer == null)
					{
						Debug.LogError("AnyPortrait : Creating a new Timeline Layer while copying keyframes failed.");
						continue;
					}

					//타임라인 레이어가 추가될때마다 리프레시 
					isAnyAdded = true;
					Editor.RefreshTimelineLayers(	apEditor.REFRESH_TIMELINE_REQUEST.All, targetTimelineLayer,  null);

					Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));

					if (targetTimeline._linkedModifier != null)
					{
						targetTimeline._linkedModifier.RefreshParamSet(apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));
					}
				}

				//여기까지 하면 Timeline과 Timeline Layer 생성 or 검색이 모두 완료되었다.


				//2. 키프레임을 생성 또는 참조한다. 
				apAnimKeyframe targetKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(dstFrameIndex);
				if (targetKeyframe == null)
				{
					targetKeyframe = AddAnimKeyframe(dstFrameIndex, targetTimelineLayer, false, false, false, false);
					isAnyAdded = true;
				}


				if (targetKeyframe != null)
				{
					//데이터를 복사한다.
					curSnapshot.Load(targetKeyframe);
				}


			}

			//Refresh
			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));//Link Reset도 해야한다.
			Editor.RefreshControllerAndHierarchy(true);
			
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			if (isAnyAdded)
			{
				Editor.OnAnyObjectAddedOrRemoved();
			}
		}


		//추가 19.8.20 : 애니메이션의 메시 그룹을 변경할 때, 데이터를 이전할 수 있다.
		private int _requestedModType = 0;
		private int _requestedModValidationKey = -1;
		public void MigrateAnimClipToMeshGroup(apAnimClip animClip, apMeshGroup nextMeshGroup)
		{
			if (Editor == null || Editor._portrait == null || animClip == null)
			{
				return;
			}

			//Undo
			//이전 > 버그
			//apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_SetMeshGroup, Editor, Editor._portrait, nextMeshGroup, null, false);

			//변경 20.3.19 : 되돌아갈때를 위해서 원래 두개의 메시 그룹과 해당 모든 모디파이어를 저장해야하지만, 그냥 다 하자.
			apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_SetMeshGroup, 
																		Editor, 
																		Editor._portrait, 
																		//nextMeshGroup, 
																		false,
																		apEditorUtil.UNDO_STRUCT.StructChanged);

			//타임라인 데이터를 유지한 상태에서 소유권을 바꾼다.
			apMeshGroup prevMeshGroup = animClip._targetMeshGroup;
			if (prevMeshGroup == nextMeshGroup || prevMeshGroup == null || nextMeshGroup == null)
			{
				return;
			}

			//매핑할 데이터
			Dictionary<apModifierBase, apModifierBase> mapping_Modifiers = new Dictionary<apModifierBase, apModifierBase>();

			//일단 모디파이어가 있는지 확인 > 없다면 추가하자.
			//어떤 모디파이어가 필요한가 체크
			List<apAnimTimeline> timelines = animClip._timelines;
			apAnimTimeline curTimeline = null;
			apAnimTimelineLayer curTimelineLayer = null;
			apAnimKeyframe curKeyframe = null;

			Dictionary<apAnimKeyframe, apModifiedMesh> keyframe2PrevModMesh = new Dictionary<apAnimKeyframe, apModifiedMesh>();
			Dictionary<apAnimKeyframe, apModifiedBone> keyframe2PrevModBone = new Dictionary<apAnimKeyframe, apModifiedBone>();

			for (int i = 0; i < timelines.Count; i++)
			{
				curTimeline = null;
				curTimelineLayer = null;
				curKeyframe = null;

				curTimeline = timelines[i];

				if (curTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					continue;
				}

				apModifierBase prevModifier = curTimeline._linkedModifier;
				if (prevModifier == null)
				{
					continue;
				}

				//같은 타입의 모디파이어가 있는지 확인
				apModifierBase nextModifier = nextMeshGroup._modifierStack._modifiers.Find(delegate (apModifierBase a)
				{
					return a.ModifierType == prevModifier.ModifierType;
				});

				if (nextModifier == null)
				{
					//없다면 만들자
					_requestedModType = (int)prevModifier.ModifierType;
					_requestedModValidationKey = -1;
					apVersion.I.RequestAddableModifierTypes(OnModifierValidationKeyCheck);
					//모디파이어를 추가하기 위한 Key가 적용되었을 것

					nextModifier = AddModifierToMeshGroup(nextMeshGroup, prevModifier.ModifierType, _requestedModValidationKey, false, true, true);
					_requestedModType = -1;
					_requestedModValidationKey = -1;
				}

				//Mod > Mod 매핑
				if (nextModifier != null)
				{
					if (!mapping_Modifiers.ContainsKey(prevModifier))
					{
						mapping_Modifiers.Add(prevModifier, nextModifier);
					}
				}
				else
				{
					continue;
				}

				//기본 설정 몇가지를 복사하자. (단, False > True 방향으로만)
				if (prevModifier._isColorPropertyEnabled)
				{
					nextModifier._isColorPropertyEnabled = true;
				}

				if (prevModifier._isExtraPropertyEnabled)
				{
					nextModifier._isExtraPropertyEnabled = true;
				}


				//ParamSetGroup 변경부터 중요 >> AddTimeline 함수를 참고해보자.
				//- 현재 타임라인의 정보를 모두 바꾸자
				//- 타임라인 레이어의 정보를 모두 바꾸자
				//- 키프레임의 정보를 모두 바꾸자
				//- 링크 다시하면 어떻게 되지 않을까??? -ㅅ-?
				//- ModMesh/ModBone을 복사해야한다.

				curTimeline._linkedModifier = nextModifier;
				curTimeline._modifierUniqueID = nextModifier._uniqueID;

				for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
				{
					curTimelineLayer = curTimeline._layers[iLayer];

					#region [미사용 코드]
					////타임라인 레이어는 자동으로 되나?? 검사하자.
					//if(curTimelineLayer._linkedMeshTransform != null)
					//{
					//	apTransform_Mesh prevLinkedMeshTF = curTimelineLayer._linkedMeshTransform;

					//	//동일한게 있는지 찾자
					//	apTransform_Mesh nextLinkedMeshTF = nextMeshGroup.GetMeshTransformRecursive(prevLinkedMeshTF._transformUniqueID);
					//	//if(nextLinkedMeshTF != null)
					//	//{
					//	//	UnityEngine.Debug.Log(">> Find Next Mesh Transform!");
					//	//}
					//	//else
					//	//{
					//	//	UnityEngine.Debug.LogError(">> No Mesh Transform....");
					//	//}

					//}
					//else if(curTimelineLayer._linkedMeshGroupTransform != null)
					//{
					//	apTransform_MeshGroup prevLinkedMeshGroupTF = curTimelineLayer._linkedMeshGroupTransform;
					//	//UnityEngine.Debug.Log(">> MeshGroup Transform Type : " + prevLinkedMeshGroupTF._nickName + " (" + prevLinkedMeshGroupTF._transformUniqueID + ")");

					//	//동일한게 있는지 찾자
					//	apTransform_MeshGroup nextLinkedMeshGroupTF = nextMeshGroup.GetMeshGroupTransformRecursive(prevLinkedMeshGroupTF._transformUniqueID);
					//	//if(nextLinkedMeshGroupTF != null)
					//	//{
					//	//	UnityEngine.Debug.Log(">> Find Next MeshGroup Transform!");
					//	//}
					//	//else
					//	//{
					//	//	UnityEngine.Debug.LogError(">> No MeshGroup Transform....");
					//	//}
					//}
					//else if(curTimelineLayer._linkedBone != null)
					//{
					//	apBone prevLinkedBone = curTimelineLayer._linkedBone;
					//	//UnityEngine.Debug.Log(">> Bone Type : " + prevLinkedBone._name + " (" + prevLinkedBone._uniqueID + ")");

					//	//동일한게 있는지 찾자
					//	apBone nextLinkedBone = nextMeshGroup.GetBoneRecursive(prevLinkedBone._uniqueID);

					//	//if(nextLinkedBone != null)
					//	//{
					//	//	UnityEngine.Debug.Log(">> Find Next Bone!");
					//	//}
					//	//else
					//	//{
					//	//	UnityEngine.Debug.LogError(">> No Bone....");
					//	//}
					//}
					////else
					////{
					////	UnityEngine.Debug.LogError(">> No Linked Object..");
					////} 
					#endregion


					//키프레임을 검사하자
					//ModMesh/ModBone은 다시 링크하면 사라질 것 같으니 따로 저장하자.

					for (int iKeyframe = 0; iKeyframe < curTimelineLayer._keyframes.Count; iKeyframe++)
					{
						curKeyframe = curTimelineLayer._keyframes[iKeyframe];
						if (curKeyframe._linkedModMesh_Editor != null)
						{
							keyframe2PrevModMesh.Add(curKeyframe, curKeyframe._linkedModMesh_Editor);
						}
						if (curKeyframe._linkedModBone_Editor != null)
						{
							keyframe2PrevModBone.Add(curKeyframe, curKeyframe._linkedModBone_Editor);
						}
					}
				}


			}

			//타겟 변경
			animClip._targetMeshGroup = nextMeshGroup;
			animClip._targetMeshGroupID = nextMeshGroup._uniqueID;

			//일단 Link를 먼저 하자.
			animClip.LinkEditor(Editor._portrait);

			//이제 ParamSetGroup/ParamSet을 자동으로 생성
			AddAndSyncAnimClipToModifier(animClip);

			//다시 링크를 하자.
			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

			curTimeline = null;
			curTimelineLayer = null;
			curKeyframe = null;

			//ModMesh와 ModBone의 데이터를 복사해주자.
			for (int iTimeline = 0; iTimeline < timelines.Count; iTimeline++)
			{
				curTimeline = timelines[iTimeline];
				if (curTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					continue;
				}

				curTimelineLayer = null;
				curKeyframe = null;
				for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
				{
					curTimelineLayer = curTimeline._layers[iLayer];
					for (int iKeyframe = 0; iKeyframe < curTimelineLayer._keyframes.Count; iKeyframe++)
					{
						curKeyframe = curTimelineLayer._keyframes[iKeyframe];
						apModifiedMesh prevModMesh = null;
						apModifiedBone prevModBone = null;
						apModifiedMesh nextModMesh = curKeyframe._linkedModMesh_Editor;
						apModifiedBone nextModBone = curKeyframe._linkedModBone_Editor;

						if (keyframe2PrevModMesh.ContainsKey(curKeyframe))
						{
							prevModMesh = keyframe2PrevModMesh[curKeyframe];
						}
						if (keyframe2PrevModBone.ContainsKey(curKeyframe))
						{
							prevModBone = keyframe2PrevModBone[curKeyframe];
						}

						//ModifiedMesh / ModifiedBone 중에서 애니메이션되는 값만 복사를 하자.

						if (nextModMesh != null && prevModMesh != null)
						{
							//1) ModVert 복사
							if (nextModMesh._vertices != null && nextModMesh._vertices.Count > 0)
							{
								apModifiedVertex nextModVert = null;
								apModifiedVertex prevModVert = null;
								for (int iVert = 0; iVert < nextModMesh._vertices.Count; iVert++)
								{
									nextModVert = nextModMesh._vertices[iVert];

									if (nextModVert._vertexUniqueID < 0)
									{
										continue;
									}

									//값을 복사할 데이터를 찾자
									prevModVert = null;

									//데이터가 그대로 있는지확인
									if (prevModMesh._vertices != null && iVert < prevModMesh._vertices.Count)
									{
										prevModVert = prevModMesh._vertices[iVert];
										if (prevModVert._vertexUniqueID != nextModVert._vertexUniqueID)
										{
											//잉.. 다른 버텍스네..
											prevModVert = null;
										}
									}
									if (prevModVert == null)
									{
										prevModVert = prevModMesh._vertices.Find(delegate (apModifiedVertex a)
										{
											return a._vertexUniqueID == nextModVert._vertexUniqueID;
										});
									}
									if (prevModVert != null)
									{
										//값을 복사하자.
										nextModVert._deltaPos = prevModVert._deltaPos;
									}
								}
							}

							//1-2) ModPin 복사 (22.3.20)
							if (nextModMesh._pins != null && nextModMesh._pins.Count > 0
								&& prevModMesh._pins != null && prevModMesh._pins.Count > 0)
							{
								apModifiedPin nextModPin = null;
								apModifiedPin prevModPin = null;
								int nPins = nextModMesh._pins.Count;
								for (int iPin = 0; iPin < nPins; iPin++)
								{
									nextModPin = nextModMesh._pins[iPin];
									if(nextModPin == null || nextModPin._pinUniqueID < 0)
									{
										continue;
									}
									//복사하고자 하는 원본 소스를 찾자
									prevModPin = null;
									if(iPin < prevModMesh._pins.Count)
									{
										prevModPin = prevModMesh._pins[iPin];
										if(prevModPin._pinUniqueID != nextModPin._pinUniqueID)
										{
											prevModPin = null;
										}
									}

									if(prevModPin == null)
									{
										prevModPin = prevModMesh._pins.Find(delegate(apModifiedPin a)
										{
											return a._pinUniqueID == nextModPin._pinUniqueID;
										});
									}

									if(prevModPin != null)
									{
										//값을 복사하자
										nextModPin._deltaPos = prevModPin._deltaPos;
									}
									
								}
							}


							//2) Transform / Color 복사
							nextModMesh._transformMatrix.SetMatrix(prevModMesh._transformMatrix, true);
							nextModMesh._meshColor = prevModMesh._meshColor;
							nextModMesh._isVisible = prevModMesh._isVisible;

							//3) Extra Property 복사
							nextModMesh._isExtraValueEnabled = prevModMesh._isExtraValueEnabled;
							if (nextModMesh._extraValue == null)
							{
								nextModMesh._extraValue = new apModifiedMesh.ExtraValue();
								nextModMesh._extraValue.Init();
							}

							if (prevModMesh._isExtraValueEnabled)
							{
								if (prevModMesh._extraValue != null)
								{
									nextModMesh._extraValue._isDepthChanged = prevModMesh._extraValue._isDepthChanged;
									nextModMesh._extraValue._deltaDepth = prevModMesh._extraValue._deltaDepth;
									nextModMesh._extraValue._isTextureChanged = prevModMesh._extraValue._isTextureChanged;
									nextModMesh._extraValue._linkedTextureData = prevModMesh._extraValue._linkedTextureData;
									nextModMesh._extraValue._textureDataID = prevModMesh._extraValue._textureDataID;
									nextModMesh._extraValue._weightCutout = prevModMesh._extraValue._weightCutout;
									nextModMesh._extraValue._weightCutout_AnimPrev = prevModMesh._extraValue._weightCutout_AnimPrev;
									nextModMesh._extraValue._weightCutout_AnimNext = prevModMesh._extraValue._weightCutout_AnimNext;

									//UnityEngine.Debug.Log(">>> ExtraValue 복사 (Depth : " + nextModMesh._extraValue._isDepthChanged + " / Texture : " + nextModMesh._extraValue._isTextureChanged + ")");
								}
							}
						}

						if (nextModBone != null && prevModBone != null)
						{
							//ModBone은 Matrix 하나다.
							nextModBone._transformMatrix.SetMatrix(prevModBone._transformMatrix, true);
						}
					}
				}
			}

			//기존의 MeshGroup의 Modifier->ParamSetGroup에서 이 AnimClip은 제외하자.
			if (prevMeshGroup != null)
			{
				for (int i = 0; i < prevMeshGroup._modifierStack._modifiers.Count; i++)
				{
					apModifierBase prevModifier = prevMeshGroup._modifierStack._modifiers[i];
					if (!prevModifier.IsAnimated)
					{
						continue;
					}

					prevModifier._paramSetGroup_controller.RemoveAll(delegate (apModifierParamSetGroup a)
					{
						return a._keyAnimClip == animClip;
					});

					prevModifier._paramSetGroupAnimPacks.RemoveAll(delegate (apModifierParamSetGroupAnimPack a)
					{
						return a.LinkedAnimClip == animClip;
					});
				}
			}

			//Link를 다시 하자.
			animClip.LinkEditor(Editor._portrait);

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//객체가 추가/삭제시 호출
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);//<<추가 : 타임라인 정보를 리셋

			AddAndSyncAnimClipToModifier(animClip);

			//두개의 MeshGroup에 대해서 모두 갱신
			//Editor._portrait.LinkAndRefreshInEditor(false, animClip._targetMeshGroup);
			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AllObjects(animClip._targetMeshGroup));

			Editor.RefreshControllerAndHierarchy(false);

			Editor.Select.SelectAnimClip(animClip);

			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);//이전
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

		}

		private void OnModifierValidationKeyCheck(int[] modifierTypes, int[] validationKey, string[] names)
		{
			for (int i = 0; i < modifierTypes.Length; i++)
			{
				if (_requestedModType == modifierTypes[i])
				{
					_requestedModValidationKey = validationKey[i];
					return;
				}
			}
		}
		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------


		public apMeshGroup AddMeshGroup(bool isRecordUndo = true, bool isRefreshHierarchy = true)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			//연결할 GameObjectGroup을 체크하자
			CheckAndMakeObjectGroup();


			//Undo - Add Mesh Group
			if(isRecordUndo)
			{
				apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create MeshGroup");
			}
			

			//int nextID = Editor._portrait.MakeUniqueID_MeshGroup();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.MeshGroup);
			int nextRootTransformID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0 || nextRootTransformID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Mesh Add Group Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshGroupAddFailed_Title),
												Editor.GetText(TEXT.MeshGroupAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}


			int nMeshGroups = Editor._portrait._meshGroups.Count;

			//GameObject로 만드는 경우
			string newName = "New Mesh Group (" + nMeshGroups + ")";
			GameObject newGameObj = new GameObject(newName);
			newGameObj.transform.parent = Editor._portrait._subObjectGroup_MeshGroup.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			apMeshGroup newGroup = newGameObj.AddComponent<apMeshGroup>();

			//apMeshGroup newGroup = new apMeshGroup();

			newGroup._uniqueID = nextID;
			newGroup._presetType = apMeshGroup.PRESET_TYPE.Default;
			newGroup._name = newName;

			newGroup.MakeRootTransform(nextRootTransformID);//<<추가 : Root Transform 생성

			newGroup.Init(Editor._portrait);

			Editor._portrait._meshGroups.Add(newGroup);
			//Debug.Log("MeshGroup Added");

			//메시 그룹을 대상으로 하는 Link 갱신
			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(newGroup);

			newGroup.RefreshModifierLink(apUtil.LinkRefresh);
			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);

			//추가 : ExMode에 추가한다.
			//일단 삭제 21.2.15 : 현재 선택된 MeshGroup이 아니므로
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//				newGroup,
			//				null,
			//				null,
			//				null,
			//				true);



			if (isRefreshHierarchy)
			{
				Editor.Hierarchy.SetNeedReset();
				Editor.RefreshControllerAndHierarchy(false);

				//MeshGroup Hierarchy Filter를 활성화한다.
				Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.MeshGroup, true);
			}
			

			if (isRecordUndo)
			{
				//Undo - Create 추가
				apEditorUtil.SetRecordCreateMonoObject(newGroup, "Create MeshGroup");
			}


			////프리팹이었다면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newGroup;
		}




		public void RemoveMeshGroup(apMeshGroup meshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			bool isNeedToNone = Editor.Select.MeshGroup == meshGroup;

			//Undo - Remove MeshGroup
			//apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Main_RemoveMeshGroup, 
			//														Editor,
			//														Editor._portrait, 
			//														meshGroup, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Mesh Group");

			//int meshGroupID = meshGroup._uniqueID;
			////Editor._portrait.PushUniqueID_MeshGroup(meshGroupID);

			List<MonoBehaviour> removedObjects = new List<MonoBehaviour>();
			List<apRootUnit> removedRootUnits = new List<apRootUnit>();
			List<apAnimClip> removedAnimClips = new List<apAnimClip>();


			RemoveChildMeshGroupsRecursive(meshGroup, removedObjects, removedRootUnits, removedAnimClips);

			//removedObjects.Add(meshGroup);

			////meshGroup의 Modifier도 같이 삭제해야 한다.
			//for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			//{
			//	apModifierBase modifier = meshGroup._modifierStack._modifiers[iMod];

			//	Editor._portrait.PushUnusedID(apIDManager.TARGET.Modifier, modifier._uniqueID);


			//	//Undo.DestroyObjectImmediate(modifier.gameObject);//<< 나중에 한꺼번에
			//	removedObjects.Add(modifier);
			//}

			//Editor._portrait.PushUnusedID(apIDManager.TARGET.MeshGroup, meshGroupID);

			//Editor._portrait._meshGroups.Remove(meshGroup);

			//if (meshGroup != null)
			//{
			//	//추가 : MeshGroup이 포함된 AnimClip과 RootUnit을 삭제한다.
			//	Editor._portrait._rootUnits.RemoveAll(delegate (apRootUnit a)
			//	{
			//		return a._childMeshGroup != null && a._childMeshGroup == meshGroup;
			//	});

			//	Editor._portrait._animClips.RemoveAll(delegate(apAnimClip a)
			//	{
			//		return a._targetMeshGroup != null && a._targetMeshGroup == meshGroup;
			//	});
			//}

			for (int iRoot = 0; iRoot < removedRootUnits.Count; iRoot++)
			{
				Editor._portrait._rootUnits.Remove(removedRootUnits[iRoot]);
			}

			for (int iAnim = 0; iAnim < removedAnimClips.Count; iAnim++)
			{
				Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimClip, removedAnimClips[iAnim]._uniqueID);//ID 반환하고..
				Editor._portrait._animClips.Remove(removedAnimClips[iAnim]);
			}

			//MeshGroup + Modifier
			if (meshGroup != null)
			{
				//Undo.DestroyObjectImmediate(meshGroup.gameObject);
				apEditorUtil.SetRecordDestroyMonoObjects(removedObjects, "Remove MeshGroup");
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));//<<전체 링크 갱신

			if(isNeedToNone)
			{
				Editor.Select.SelectNone();
			}


			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);
			//Editor.Hierarchy.RefreshUnits();

			////프리팹이었다면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}





		/// <summary>
		/// 여러개의 메시 그룹들을 삭제한다. (21.10.10)
		/// </summary>
		/// <param name="meshGroup"></param>
		public void RemoveMeshGroups(List<apMeshGroup> meshGroups)
		{
			if (Editor._portrait == null || meshGroups == null || meshGroups.Count == 0)
			{
				return;
			}

			bool isNeedToNone = false;
			//None으로 변경해야하는 경우
			//RootUnit / MeshGroup / AnimClip
			switch (Editor.Select.SelectionType)
			{
				case apSelection.SELECTION_TYPE.MeshGroup:
					{
						if (Editor.Select.MeshGroup != null)
						{
							if(meshGroups.Contains(Editor.Select.MeshGroup))
							{
								isNeedToNone = true;
							}
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Animation:
					{
						if(Editor.Select.AnimClip != null
							&& Editor.Select.AnimClip._targetMeshGroup != null)
						{
							if(meshGroups.Contains(Editor.Select.AnimClip._targetMeshGroup))
							{
								isNeedToNone = true;
							}
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Overall:
					{
						if(Editor.Select.RootUnit != null
							&& Editor.Select.RootUnit._childMeshGroup != null)
						{
							if(meshGroups.Contains(Editor.Select.RootUnit._childMeshGroup))
							{
								isNeedToNone = true;
							}
						}
					}
					break;
			}

			//Undo - Remove MeshGroup
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Mesh Group");

			int nMeshGroups = meshGroups.Count;
			apMeshGroup curMeshGroup = null;

			for (int iMG = 0; iMG < nMeshGroups; iMG++)
			{
				curMeshGroup = meshGroups[iMG];
				//이미 삭제되었을 수 있다.
				if (curMeshGroup == null
					|| !Editor._portrait._meshGroups.Contains(curMeshGroup))
				{
					continue;
				}

				List<MonoBehaviour> removedObjects = new List<MonoBehaviour>();
				List<apRootUnit> removedRootUnits = new List<apRootUnit>();
				List<apAnimClip> removedAnimClips = new List<apAnimClip>();

				RemoveChildMeshGroupsRecursive(curMeshGroup, removedObjects, removedRootUnits, removedAnimClips);

				for (int iRoot = 0; iRoot < removedRootUnits.Count; iRoot++)
				{
					Editor._portrait._rootUnits.Remove(removedRootUnits[iRoot]);
				}

				for (int iAnim = 0; iAnim < removedAnimClips.Count; iAnim++)
				{
					Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimClip, removedAnimClips[iAnim]._uniqueID);//ID 반환하고..
					Editor._portrait._animClips.Remove(removedAnimClips[iAnim]);
				}

				//MeshGroup + Modifier
				if (curMeshGroup != null)
				{
					apEditorUtil.SetRecordDestroyMonoObjects(removedObjects, "Remove MeshGroup");
				}
			}
			

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));//<<전체 링크 갱신

			if(isNeedToNone)
			{
				Editor.Select.SelectNone();
			}

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(true);
		}






		/// <summary>
		/// Target과 기준으로 하위의 MeshGroup들을 삭제한다.
		/// 실제 삭제는 리스트를 받아서 하자
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		private void RemoveChildMeshGroupsRecursive(apMeshGroup targetMeshGroup,
														List<MonoBehaviour> removedObjects,
														List<apRootUnit> removedRootUnits,
														List<apAnimClip> removedAnimClips)
		{
			if (targetMeshGroup == null)
			{
				return;
			}


			//[Recursive]
			//Child MeshGroup부터 갔다온다.
			for (int iChild = 0; iChild < targetMeshGroup._childMeshGroupTransforms.Count; iChild++)
			{
				apTransform_MeshGroup childMeshGroupTransform = targetMeshGroup._childMeshGroupTransforms[iChild];
				RemoveChildMeshGroupsRecursive(childMeshGroupTransform._meshGroup, removedObjects, removedRootUnits, removedAnimClips);

			}


			int meshGroupID = targetMeshGroup._uniqueID;
			//Editor._portrait.PushUniqueID_MeshGroup(meshGroupID);

			removedObjects.Add(targetMeshGroup);

			//meshGroup의 Modifier도 같이 삭제해야 한다.
			for (int iMod = 0; iMod < targetMeshGroup._modifierStack._modifiers.Count; iMod++)
			{
				apModifierBase modifier = targetMeshGroup._modifierStack._modifiers[iMod];

				Editor._portrait.PushUnusedID(apIDManager.TARGET.Modifier, modifier._uniqueID);
				removedObjects.Add(modifier);
			}

			Editor._portrait.PushUnusedID(apIDManager.TARGET.MeshGroup, meshGroupID);//<<ID는 반납한다.
			Editor._portrait._meshGroups.Remove(targetMeshGroup);


			if (targetMeshGroup != null)
			{
				//같이 삭제되어야 하는 RootUnit과 AnimClip들
				for (int iRoot = 0; iRoot < Editor._portrait._rootUnits.Count; iRoot++)
				{
					apRootUnit rootUnit = Editor._portrait._rootUnits[iRoot];
					if (rootUnit._childMeshGroup != null && rootUnit._childMeshGroup == targetMeshGroup)
					{
						if (!removedRootUnits.Contains(rootUnit))
						{
							removedRootUnits.Add(rootUnit);
						}
					}
				}

				for (int iAnim = 0; iAnim < Editor._portrait._animClips.Count; iAnim++)
				{
					apAnimClip animClip = Editor._portrait._animClips[iAnim];
					if (animClip._targetMeshGroup != null && animClip._targetMeshGroup == targetMeshGroup)
					{
						if (!removedAnimClips.Contains(animClip))
						{
							removedAnimClips.Add(animClip);
						}
					}
				}
			}
		}


		//메시를 복제하기 위한 객체 정보(Src > New 변환 정보를 가진다.)
		public class MeshGroupDupcliateConvert
		{
			public Dictionary<apMeshGroup, apMeshGroup> Src2Dst_MeshGroup = null;
			public Dictionary<apTransform_Mesh, apTransform_Mesh> Src2Dst_MeshTransform = null;
			public Dictionary<apTransform_MeshGroup, apTransform_MeshGroup> Src2Dst_MeshGroupTransform = null;
			public Dictionary<apBone, apBone> Src2Dst_Bone = null;
			public Dictionary<apModifierBase, apModifierBase> Src2Dst_Modifier = null;
			public Dictionary<apModifierParamSetGroup, apModifierParamSetGroup> Src2Dst_ParamSetGroup = null;
			public Dictionary<apModifierParamSet, apModifierParamSet> Src2Dst_ParamSet = null;
			public Dictionary<apModifiedMesh, apModifiedMesh> Src2Dst_ModMesh = null;
			public Dictionary<apModifiedBone, apModifiedBone> Src2Dst_ModBone = null;
			public Dictionary<apAnimClip, apAnimClip> Src2Dst_AnimClip = null;
			public Dictionary<apAnimTimeline, apAnimTimeline> Src2Dst_Timeline = null;
			public Dictionary<apAnimTimelineLayer, apAnimTimelineLayer> Src2Dst_TimelineLayer = null;
			public Dictionary<apAnimKeyframe, apAnimKeyframe> Src2Dst_Keyframe = null;

			public MeshGroupDupcliateConvert()
			{
				Src2Dst_MeshGroup = new Dictionary<apMeshGroup, apMeshGroup>();
				Src2Dst_MeshTransform = new Dictionary<apTransform_Mesh, apTransform_Mesh>();
				Src2Dst_MeshGroupTransform = new Dictionary<apTransform_MeshGroup, apTransform_MeshGroup>();
				Src2Dst_Bone = new Dictionary<apBone, apBone>();
				Src2Dst_Modifier = new Dictionary<apModifierBase, apModifierBase>();
				Src2Dst_ParamSetGroup = new Dictionary<apModifierParamSetGroup, apModifierParamSetGroup>();
				Src2Dst_ParamSet = new Dictionary<apModifierParamSet, apModifierParamSet>();
				Src2Dst_ModMesh = new Dictionary<apModifiedMesh, apModifiedMesh>();
				Src2Dst_ModBone = new Dictionary<apModifiedBone, apModifiedBone>();
				Src2Dst_AnimClip = new Dictionary<apAnimClip, apAnimClip>();
				Src2Dst_Timeline = new Dictionary<apAnimTimeline, apAnimTimeline>();
				Src2Dst_TimelineLayer = new Dictionary<apAnimTimelineLayer, apAnimTimelineLayer>();
				Src2Dst_Keyframe = new Dictionary<apAnimKeyframe, apAnimKeyframe>();
				
			}

		}


		/// <summary>
		/// 추가 20.1.5 : 메시 그룹을 복제한다.
		/// </summary>
		/// <param name="srcMeshGroup"></param>
		/// <returns></returns>
		public apMeshGroup DuplicateMeshGroup(		apMeshGroup srcMeshGroup,
													MeshGroupDupcliateConvert convertInfo,
													bool isRoot,
													bool isDuplicateAnimClip,
													string undoName = null,
													bool isRefresh = true, bool isSkipUndoIncrement = false)
		{
			if (apVersion.I.IsDemo) //메시 그룹 복제 불가 (내부 코드)
			{
				Debug.LogError("AnyPortrait : The demo version does not support this feature.");
				return null;
			}

			if (srcMeshGroup == null
				|| Editor == null
				|| Editor.Select == null
				//|| Editor.Select.MeshGroup == null//이게 있으면 버그 v1.4.2
				|| Editor._portrait == null)
			{
				return null;
			}

			try
			{
				//int undoID = apEditorUtil.SetRecordBeforeCreateOrDestroyMultipleObjects(Editor._portrait, undoName == null ? "Duplicate Mesh Group" : undoName, isSkipUndoIncrement);
				apEditorUtil.SetRecordBeforeCreateOrDestroyMultipleObjects(Editor._portrait, string.IsNullOrEmpty(undoName) ? "Duplicate Mesh Group" : undoName, isSkipUndoIncrement);

				//자식 메시 그룹들도 복사해야한다.
				
				//변환 정보를 생성한다.
				if(convertInfo == null)
				{
					convertInfo = new MeshGroupDupcliateConvert();
				}


				apMeshGroup newMeshGroup = AddMeshGroup(false);
				if (newMeshGroup == null)
				{
					return null;
				}

				//리스트에 추가
				convertInfo.Src2Dst_MeshGroup.Add(srcMeshGroup, newMeshGroup);

				//Rott Transform이 만들어졌을테니 변환 리스트에 추가하자
				if(srcMeshGroup._rootMeshGroupTransform != null
					&& newMeshGroup._rootMeshGroupTransform != null)
				{
					convertInfo.Src2Dst_MeshGroupTransform.Add(srcMeshGroup._rootMeshGroupTransform, newMeshGroup._rootMeshGroupTransform);
				}
				


				//1. 이름 복사하기
				string newMGName = srcMeshGroup._name + " Copy";

				//중복되지 않은 이름을 찾는다.
				if (Editor._portrait._meshGroups.Exists(delegate (apMeshGroup a)
				{ return string.Equals(newMGName, a._name); })
					)
				{
					//오잉 똑같은 이름이 있네염...
					int copyIndex = -1;
					for (int iCopyIndex = 1; iCopyIndex < 1000; iCopyIndex++)
					{
						if (!Editor._portrait._meshGroups.Exists(delegate (apMeshGroup a)
						{ return string.Equals(newMGName + " (" + iCopyIndex + ")", a._name); }))
						{
							copyIndex = iCopyIndex;
							break;
						}
					}
					if (copyIndex < 0)
					{ newMGName += " (1000+)"; }
					else
					{ newMGName += " (" + copyIndex + ")"; }
				}
				newMeshGroup.gameObject.name = newMGName;
				newMeshGroup._name = newMGName;

				//2. 자식 MeshGroup이 있다면 이 단계에서 먼저 복사를 해야한다.
				if (srcMeshGroup._childMeshGroupTransforms != null &&
					srcMeshGroup._childMeshGroupTransforms.Count > 0)
				{
					apTransform_MeshGroup childMeshGroupTransform = null;
					apMeshGroup childMeshGroup = null;
					for (int i = 0; i < srcMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						childMeshGroupTransform = srcMeshGroup._childMeshGroupTransforms[i];
						if (childMeshGroupTransform == null)
						{
							continue;
						}
						childMeshGroup = childMeshGroupTransform._meshGroup;
						if (childMeshGroup == srcMeshGroup)
						{
							continue;
						}
						//재귀 함수를 호출한다.
						//- 리스트 전달 (이 이후로 srcMeshGroup2NewMeshGroup에는 모든 변환 정보가 저장된다.)
						//- 자식 객체는 AnimClip을 적용하지 않는다.
						DuplicateMeshGroup(childMeshGroup,
											convertInfo,
											false,
											false);
					}
				}

				//3. 기본 정보를 복사한다.
				//- 부모 연결
				apMeshGroup srcParentMeshGroup = srcMeshGroup._parentMeshGroup;
				if (srcParentMeshGroup == null && srcMeshGroup._parentMeshGroupID >= 0)
				{
					srcParentMeshGroup = Editor._portrait.GetMeshGroup(srcMeshGroup._parentMeshGroupID);
				}

				if (srcParentMeshGroup == null)
				{
					newMeshGroup._parentMeshGroup = null;
					newMeshGroup._parentMeshGroupID = -1;
				}
				else
				{
					//부모가 있다면 그에 대칭되는 부모 연결
					apMeshGroup newParentMeshGroup = null;
					if (convertInfo.Src2Dst_MeshGroup.ContainsKey(srcParentMeshGroup))
					{
						newParentMeshGroup = convertInfo.Src2Dst_MeshGroup[srcParentMeshGroup];
					}

					if (newParentMeshGroup == null)
					{
						//대칭되는게 없는데용?
						//Debug.LogError("AnyPortrait : No Duplicated Parent MeshGroup [" + newMeshGroup._name + "]");
						newMeshGroup._parentMeshGroup = null;
						newMeshGroup._parentMeshGroupID = -1;
					}
					else
					{
						//대칭되는 Parent MeshGroup을 입력
						newMeshGroup._parentMeshGroup = newParentMeshGroup;
						newMeshGroup._parentMeshGroupID = newParentMeshGroup._uniqueID;
					}
				}

				//4. 하위 객체들을 복사한다.
				//복사해야하는 정보
				//<기본 : 데이터를 먼저 복사하고 링크를 나중에 하자>
				//1단계 : 객체들 (Recursive)
				//	- Mesh/MeshGroup Transform <- 미리 자식 Mesh Group을 복사해둘것 (Recursive)
				//	- Bone
				//2단계 : Modifier : 일단 애니메이션 데이터를 제외한 모든 데이터를 복사한다.
				//3단계 : Animation Clip (선택사항-자식 MeshGroup 제외) : 애니메이션 데이터와 관련된 모디파이어 정보를 모두 복사한다.

				//>>
				//1단계 : 객체들
				//	- Mesh/MeshGroup Transform <- 미리 자식 Mesh Group을 복사해둘것 (이건 Recursive로 호출하면 안된다)
				//	- Bone (Recursive)
				int nMeshTransforms = srcMeshGroup._childMeshTransforms == null ? 0 : srcMeshGroup._childMeshTransforms.Count;
				int nMeshGroupTransforms = srcMeshGroup._childMeshGroupTransforms == null ? 0 : srcMeshGroup._childMeshGroupTransforms.Count;
				if (nMeshTransforms > 0)
				{
					//MeshTransform 복사하기
					apTransform_Mesh srcMeshTransform = null;
					apTransform_Mesh newMeshTransform = null;
					for (int iMT = 0; iMT < nMeshTransforms; iMT++)
					{
						srcMeshTransform = srcMeshGroup._childMeshTransforms[iMT];
						if (srcMeshTransform == null)
						{
							continue;
						}
						newMeshTransform = DuplicateMeshTransform(srcMeshTransform,
																	srcMeshGroup, newMeshGroup,
																	false,
																	false);
						if (newMeshTransform != null)
						{
							//srcMeshGroup에 있던 MeshTransform이 newMeshGroup으로 복제되었다.
							convertInfo.Src2Dst_MeshTransform.Add(srcMeshTransform, newMeshTransform);//Src > New에 추가
						}
					}
				}
				if (nMeshGroupTransforms > 0)
				{
					//MeshGroupTransform 복사하기
					apTransform_MeshGroup srcMeshGroupTransform = null;
					apTransform_MeshGroup newMeshGroupTransform = null;
					for (int iMGT = 0; iMGT < nMeshGroupTransforms; iMGT++)
					{
						srcMeshGroupTransform = srcMeshGroup._childMeshGroupTransforms[iMGT];
						if (srcMeshGroupTransform == null)
						{
							continue;
						}
						newMeshGroupTransform = DuplicateMeshGroupTransform(srcMeshGroupTransform,
																				srcMeshGroup,
																				newMeshGroup,
																				false,
																				false,
																				convertInfo.Src2Dst_MeshGroup);

						if (newMeshGroupTransform != null)
						{
							//srcMeshGroup에 있던 MeshGroupTransform이 newMeshGroup으로 복제되었다.
							//재귀 호출 하면 안된다. (이미 위에서 복제되었기 때문)
							convertInfo.Src2Dst_MeshGroupTransform.Add(srcMeshGroupTransform, newMeshGroupTransform);
						}
					}
				}


				//Bone 복사
				int nBones = srcMeshGroup._boneList_All == null ? 0 : srcMeshGroup._boneList_All.Count;
				if (nBones > 0)
				{
					//복사 함수를 호출
					DuplicateBonesToOtherMeshGroup(srcMeshGroup, newMeshGroup, convertInfo.Src2Dst_Bone, false, false);
				}


				//2단계 : Modifier : 일단 애니메이션 데이터를 제외한 모든 데이터를 복사한다.
				DuplicateModifiersToOtherMeshGroup(srcMeshGroup, newMeshGroup, convertInfo);

				//3단계 : Animation Clip (선택사항-자식 MeshGroup 제외) : 애니메이션 데이터와 관련된 모디파이어 정보를 모두 복사한다.
				if(isDuplicateAnimClip)
				{
					//복사할 애니메이션 클립 리스트를 만들자 (대상이 아닌 것도 있다.)
					
					int nAnimClips = Editor._portrait._animClips == null ? 0 : Editor._portrait._animClips.Count;
					List<apAnimClip> srcAnimClips = new List<apAnimClip>();
					apAnimClip curAnimClip = null;
					for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
					{
						curAnimClip = Editor._portrait._animClips[iAnimClip];
						if(curAnimClip == null)
						{
							continue;
						}
						if(curAnimClip._targetMeshGroup == srcMeshGroup)
						{
							//현재의 srcMeshGroup과 연결된 메시 그룹이라면 복사하자
							srcAnimClips.Add(curAnimClip);
						}
					}

					int nSrcAnimClips = srcAnimClips.Count;
					apAnimClip srcAnimClip;
					for (int iSrcAnimClip = 0; iSrcAnimClip < nSrcAnimClips; iSrcAnimClip++)
					{
						srcAnimClip = srcAnimClips[iSrcAnimClip];
						//복사하자
						DuplicateAnimClipWithOtherMeshGroup(srcAnimClip,
															srcMeshGroup,
															newMeshGroup,
															convertInfo);
					}


				}

				if(isRoot)
				{
					//이제 마지막으로 AnimClip과 모디파이어를 연결하자
					foreach (KeyValuePair<apModifierParamSetGroup, apModifierParamSetGroup> src2Dst_PSG in convertInfo.Src2Dst_ParamSetGroup)
					{
						apModifierParamSetGroup srcPSG = src2Dst_PSG.Key;
						apModifierParamSetGroup dstPSG = src2Dst_PSG.Value;

						if(srcPSG._syncTarget != apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
						{
							//애니메이션 타입이 아니라면 패스
							continue;
						}

						int dstKeyAnimClipID = -1;
						int dstKeyTimelineID = -1;
						int dstKeyTimelineLayerID = -1;

						apAnimClip dstLinkedAnimClip = null;
						apAnimTimeline dstLinkedTimeline = null;
						apAnimTimelineLayer dstLinkedTimelineLayer = null;

						if(srcPSG._keyAnimClip != null
							&& convertInfo.Src2Dst_AnimClip.ContainsKey(srcPSG._keyAnimClip))
						{
							dstLinkedAnimClip = convertInfo.Src2Dst_AnimClip[srcPSG._keyAnimClip];
						}
						dstKeyAnimClipID = dstLinkedAnimClip == null ? -1 : dstLinkedAnimClip._uniqueID;

						if(srcPSG._keyAnimTimeline != null
							&& convertInfo.Src2Dst_Timeline.ContainsKey(srcPSG._keyAnimTimeline))
						{
							dstLinkedTimeline = convertInfo.Src2Dst_Timeline[srcPSG._keyAnimTimeline];
						}
						dstKeyTimelineID = dstLinkedTimeline == null ? -1 : dstLinkedTimeline._uniqueID;

						if(srcPSG._keyAnimTimelineLayer != null
							&& convertInfo.Src2Dst_TimelineLayer.ContainsKey(srcPSG._keyAnimTimelineLayer))
						{
							dstLinkedTimelineLayer = convertInfo.Src2Dst_TimelineLayer[srcPSG._keyAnimTimelineLayer];
						}
						dstKeyTimelineLayerID = dstLinkedTimelineLayer == null ? -1 : dstLinkedTimelineLayer._uniqueID;

						if(dstKeyAnimClipID == -1 || dstKeyTimelineID == -1 || dstKeyTimelineLayerID == -1)
						{
							Debug.LogError("AnyPortrait : Duplicating Animation and Modifier is failed");
							Debug.Log(">> AnimClip : [Src] " 
								+ (srcPSG._keyAnimClip != null ? srcPSG._keyAnimClip._name : "<None>") 
								+ " / [Dst] " + (dstLinkedAnimClip != null ? dstLinkedAnimClip._name : "<None>"));
							
							Debug.Log(">> Timeline : [Src] " 
								+ (srcPSG._keyAnimTimeline != null ? srcPSG._keyAnimTimeline.DisplayName : "<None>") 
								+ " / [Dst] " + (dstLinkedTimeline != null ? dstLinkedTimeline.DisplayName : "<None>"));
							
							Debug.Log(">> TimelineLayer : [Src] " 
								+ (srcPSG._keyAnimTimelineLayer != null ? srcPSG._keyAnimTimelineLayer.DisplayName : "<None>") 
								+ " / [Dst] " + (dstLinkedTimelineLayer != null ? dstLinkedTimelineLayer.DisplayName : "<None>"));
						}
						dstPSG._keyAnimClipID = dstKeyAnimClipID;
						dstPSG._keyAnimTimelineID = dstKeyTimelineID;
						dstPSG._keyAnimTimelineLayerID = dstKeyTimelineLayerID;
					}

					foreach (KeyValuePair<apModifierParamSet, apModifierParamSet> src2Dst_PS in convertInfo.Src2Dst_ParamSet)
					{
						apModifierParamSet srcPS = src2Dst_PS.Key;
						apModifierParamSet dstPS = src2Dst_PS.Value;

						int dstKeyframeID = -1;
						apAnimKeyframe dstLinkedKeyframe = null;
						if (srcPS.SyncKeyframe != null
							&& convertInfo.Src2Dst_Keyframe.ContainsKey(srcPS.SyncKeyframe))
						{
							dstLinkedKeyframe = convertInfo.Src2Dst_Keyframe[srcPS.SyncKeyframe];
						}
						dstKeyframeID = dstLinkedKeyframe == null ? -1 : dstLinkedKeyframe._uniqueID;
						dstPS._keyframeUniqueID = dstKeyframeID;
					}
				}

				//Undo - Create 추가
				//apEditorUtil.SetRecordCreateMonoObject(newMeshGroup, "Duplicate MeshGroup");
				//여러개를 동시에 추가해야한다.
				//MeshGroup / Modifier를 추가하자
				List<MonoBehaviour> createdObjects = new List<MonoBehaviour>();
				foreach (KeyValuePair<apMeshGroup, apMeshGroup> src2Dst_MeshGroup in convertInfo.Src2Dst_MeshGroup)
				{
					createdObjects.Add(src2Dst_MeshGroup.Value);
				}

				foreach (KeyValuePair<apModifierBase, apModifierBase> src2Dst_Mod in convertInfo.Src2Dst_Modifier)
				{
					createdObjects.Add(src2Dst_Mod.Value);
				}
				//Undo에 넣자
				//apEditorUtil.SetRecordCreateMultipleMonoObjects(createdObjects, "Duplicate Mesh Group", true, undoID);
				apEditorUtil.SetRecordCreateMultipleMonoObjects(createdObjects, "Duplicate Mesh Group");//변경

				Editor.OnAnyObjectAddedOrRemoved();

				if (isRoot)
				{
					if (isRefresh)
					{

						Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(newMeshGroup));

						//Refresh 추가
						//Editor.Select.RefreshAnimEditing(true);//이전
						Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

						//메시 그룹 선택
						Editor.Select.SelectMeshGroup(newMeshGroup);

						////프리팹이었으면 Apply
						//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

						//v1.5.0 : 복제시 원본의 다음에 위치시키도록 하자 (Refresh Hierarchy보다 먼저 호출해야함)
						if(Editor._portrait._objectOrders == null)
						{
							Editor._portrait._objectOrders = new apObjectOrders();
						}
						Editor._portrait._objectOrders.Sync(Editor._portrait);
						Editor._portrait._objectOrders.SetOrderToNext(	Editor._portrait,
																		apObjectOrders.OBJECT_TYPE.MeshGroup,
																		newMeshGroup._uniqueID,//복제된 대상의 ID
																		srcMeshGroup._uniqueID);//원본의 ID

						Editor.ResetHierarchyAll();
						Editor.RefreshControllerAndHierarchy(true);
						Editor.Notification(srcMeshGroup._name + " > " + newMeshGroup._name + " Duplicated", true, false);

						apEditorUtil.ReleaseGUIFocus();
					}
				}


				return newMeshGroup;
				//리턴 > 상위의 MeshGroup으로 넘기거나 함수를 처음 호출한 곳으로 리턴
			}
			catch (Exception ex)
			{
				Debug.LogError("AnyPortrait : Duplicating the Mesh Group failed. (Exception : " + ex + ")");
				return null;
			}
		}


		//추가 20.1.17 : MeshTransform과 MeshGroupTransform을 복제하기 (내부에서)
		//중요 > 단순히 객체를 복사하는건 이미 있지만, Modifier / AnimClip과 연동하는게 필요해서 코드가 길어진다. 힝
		public apTransform_Mesh DuplicateMeshTransformInSameMeshGroup(apTransform_Mesh srcMeshTransform, bool isUndoAndRefresh = true)
		{
			if (apVersion.I.IsDemo)//서브 객체 복제 불가 (내부 코드)
			{
				Debug.LogError("AnyPortrait : The demo version does not support this feature.");
				return null;
			}

			
			if (srcMeshTransform == null
				|| Editor == null
				|| Editor.Select == null
				|| Editor.Select.MeshGroup == null
				|| Editor._portrait == null)
			{
				return null;
			}

			//이 MeshTransform을 포함하는 MeshGroup을 찾는다.
			apMeshGroup srcMeshGroup = Editor._portrait._meshGroups.Find(delegate(apMeshGroup a)
			{
				if(a._childMeshTransforms == null)
				{
					return false;
				}
				return a._childMeshTransforms.Contains(srcMeshTransform);
			});

			if(srcMeshGroup == null)
			{
				Debug.LogError("적절한 MeshGroup을 찾지 못했다.");
				return null;
			}
			
			//모든 상위 MeshGroup을 찾자
			//상위의 MeshGroup의 모디파이어나 AnimClip이 이 MeshTransform을 참조할 수 있다.
			//아이고
			List<apMeshGroup> srcMeshGroupsAll = new List<apMeshGroup>();
			srcMeshGroupsAll.Add(srcMeshGroup);
			if(srcMeshGroup._parentMeshGroup != null)
			{
				apMeshGroup curParentMeshGroup = srcMeshGroup._parentMeshGroup;
				while(true)
				{
					if(curParentMeshGroup == null) { break; }
					if(srcMeshGroupsAll.Contains(curParentMeshGroup)) { break; }
					
					srcMeshGroupsAll.Add(curParentMeshGroup);

					curParentMeshGroup = curParentMeshGroup._parentMeshGroup;
				}
			}
			

			try
			{
				//일단 Undo에 등록
				if (isUndoAndRefresh)
				{
					if (srcMeshGroupsAll.Count == 1)
					{
						//한개의 MeshGroup만 체크
						apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.MeshGroup_DuplicateMeshTransform, 
																					Editor, 
																					Editor._portrait, 
																					srcMeshGroup, 
																					//srcMeshTransform, 
																					false,
																					apEditorUtil.UNDO_STRUCT.StructChanged);
					}
					else
					{
						//전부 Undo에 등록
						apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.MeshGroup_DuplicateMeshTransform, 
																					Editor, 
																					Editor._portrait, 
																					//srcMeshTransform, 
																					false,
																					apEditorUtil.UNDO_STRUCT.StructChanged);
					}

					
				}

				//변환 정보도 하나 만들자
				MeshGroupDupcliateConvert convertInfo = new MeshGroupDupcliateConvert();

				//MeshTransform을 복사하자
				apTransform_Mesh dstMeshTransform = DuplicateMeshTransform(srcMeshTransform, srcMeshGroup, srcMeshGroup, false, false);
				if(dstMeshTransform == null)
				{
					Debug.LogError("AnyPortrait : Duplicating Mesh Transform is failed. Please try again.");
					return null;
				}

				convertInfo.Src2Dst_MeshTransform.Add(srcMeshTransform, dstMeshTransform);

				//Depth를 정하자
				//생성된 Depth는 기본적으로 +1
				//...순서는 알아서 바꿉시당
				int srcDepth = srcMeshTransform._depth;
				int nextDepth = srcDepth + 1;
				
				apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;

				int nMeshTransform = srcMeshGroup._childMeshTransforms == null ? 0 : srcMeshGroup._childMeshTransforms.Count;
				int nMeshGroupTransform = srcMeshGroup._childMeshGroupTransforms == null ? 0 : srcMeshGroup._childMeshGroupTransforms.Count;

				for (int iMeshTF = 0; iMeshTF < nMeshTransform; iMeshTF++)
				{
					curMeshTF = srcMeshGroup._childMeshTransforms[iMeshTF];
					if(curMeshTF == dstMeshTransform
						|| curMeshTF == srcMeshTransform)
					{
						//같은거면 패스
						continue;
					}
					if(curMeshTF._depth > srcDepth)
					{
						curMeshTF._depth += 100;//100 증가시켜서 위로 올리자
					}
				}

				for (int iMeshGroupTF = 0; iMeshGroupTF < nMeshGroupTransform; iMeshGroupTF++)
				{
					curMeshGroupTF = srcMeshGroup._childMeshGroupTransforms[iMeshGroupTF];
					if(curMeshGroupTF._depth > srcDepth)
					{
						curMeshGroupTF._depth += 100;//100 증가시켜서 위로 올리자
					}
				}

				dstMeshTransform._depth = nextDepth;//새로운 MeshTransform의 Depth 설정

				
				//srcMeshGroup.SetDirtyToSort();
				//srcMeshGroup.SortRenderUnits(true);
				//이전
				//Editor.Select.MeshGroup.SetDirtyToSort();
				//Editor.Select.MeshGroup.SortRenderUnits(true);

				//변경 [v1.4.2]
				Editor.Select.MeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//여기선 일단 Sort만 하자

				//이제 MeshGroup들을 돌면서 모디파이어, AnimClip들을 복사된 MeshTransform과 연결하자
				//ModMesh를 찾아서 필요한 경우 복사
				int nMeshGroups = srcMeshGroupsAll.Count;
				apMeshGroup curMeshGroup = null;
				for (int iMeshGroup = 0; iMeshGroup < nMeshGroups; iMeshGroup++)
				{
					curMeshGroup = srcMeshGroupsAll[iMeshGroup];
					if (curMeshGroup == null)
					{
						continue;
					}

					int nModifier = curMeshGroup._modifierStack._modifiers == null ? 0 : curMeshGroup._modifierStack._modifiers.Count;

					apModifierBase curModifier = null;
					List<apModifierParamSetGroup> addedPSG = new List<apModifierParamSetGroup>();//PSG에는 여기에 넣고 나중에 한꺼번에 추가하자

					for (int iModifier = 0; iModifier < nModifier; iModifier++)
					{
						curModifier = curMeshGroup._modifierStack._modifiers[iModifier];
						if (curModifier == null)
						{
							continue;
						}

						bool isAnimated = curModifier.IsAnimated;

						//애니메이션과 비 애니메이션의 차이가 있다.
						//- 애니메이션 타입
						//	: ParamSetGroup이 TimelineLayer로서, 만약 해당 TimelineLayer가 srcMeshTransform을 가지고 있는 경우,
						//	아예 ParamSetGroup + TimelineLayer를 복사해야한다.

						//- 비 애니메이션 타입
						//	: ParamSet 내부에 ModMesh를 찾아서 복사한뒤 ModMesh에 넣어주면 된다.

						int nPSG = curModifier._paramSetGroup_controller == null ? 0 : curModifier._paramSetGroup_controller.Count;
						apModifierParamSetGroup srcPSG = null;
						addedPSG.Clear();

						int maxPSGLayerIndex = -1;

						//미리 최대 인덱스를 계산하자
						for (int iPSG = 0; iPSG < nPSG; iPSG++)
						{
							srcPSG = curModifier._paramSetGroup_controller[iPSG];
							if(srcPSG._layerIndex > maxPSGLayerIndex)
							{
								maxPSGLayerIndex = srcPSG._layerIndex;
							}
						}

						for (int iPSG = 0; iPSG < nPSG; iPSG++)
						{
							srcPSG = curModifier._paramSetGroup_controller[iPSG];
							if (srcPSG == null)
							{
								continue;
							}

							if (isAnimated)
							{	
								//애니메이션 타입인 경우
								DuplicateModifierParamSetGroupAndTimelineLayer(curModifier, srcPSG, addedPSG, convertInfo, true, false, false);
							}
							else
							{
								//비 애니메이션 타입이면 ParamSet 내부의 ModMesh를 복사해야한다.
								DuplicateModMeshesAndBonesWhenNoAnimated(curModifier, srcPSG, convertInfo, true, false, false);
							}
						}

						//추가할 ParamSetGroup이 있다면 추가하기
						if(addedPSG.Count > 0)
						{
							//Debug.Log(addedPSG.Count + "개의 PSG가 " + curModifier.DisplayName + "에 추가됨");
							for (int iPSG = 0; iPSG < addedPSG.Count; iPSG++)
							{
								curModifier._paramSetGroup_controller.Add(addedPSG[iPSG]);
							}
							addedPSG.Clear();
						}
						
					}
				}


				if (isUndoAndRefresh)
				{
					Editor.OnAnyObjectAddedOrRemoved(true);

					apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(srcMeshGroup);

					Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);

					//Refresh 추가
					//Editor.Select.RefreshAnimEditing(true);
					Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

					//메시 그룹 선택
					//Editor.Select.SetMeshGroup(newMeshGroup);
					Editor.Select.MeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
					Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
					
					Editor.Select.MeshGroup.ResetRenderUnitsChildAndRoot();//Depth 할당 전에, Root>Child의 Render Unit들이 잘 생성되었는지 검토하자
					Editor.Select.MeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);//여기서 Depth를 처리하자

					//v1.5.0 버그 수정용 추가 : 여기서 완벽하게 한번 더 갱신을 해줘야 새로 추가된 RenderUnit <-> 모디파이어 연결이 완료된다.
					Editor.Select.MeshGroup.SetDirtyToReset();
					Editor.Select.MeshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);


					Editor.Select.SelectMeshTF(dstMeshTransform, apSelection.MULTI_SELECT.Main);

					////프리팹이었으면 Apply
					//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

					Editor.ResetHierarchyAll();
					Editor.RefreshControllerAndHierarchy(true);
					Editor.Notification(dstMeshTransform._nickName + " is Duplicated", true, false);

					

					apEditorUtil.ReleaseGUIFocus();
				}
				return dstMeshTransform;
			}
			catch (Exception ex)
			{
				Debug.LogError("AnyPortrait : Duplicating Mesh Transform is failed.\n(Exception : " + ex + ")");
			}

			return null;
		}



		//추가 20.1.18 : MeshGroupTransform 복제하기 (내부에서)
		public apTransform_MeshGroup DuplicateMeshGroupTransformInSameMeshGroup(apTransform_MeshGroup srcMeshGroupTransform, bool isUndoAndRefresh = true)
		{
			if (apVersion.I.IsDemo)//서브 객체 복제 불가 (내부 코드)
			{
				Debug.LogError("AnyPortrait : The demo version does not support this feature.");
				return null;
			}

			//Mesh Transform을 복사하는 것과 다르게, Mesh Group Transform은 실제로 일단 메시 그룹을 복제하고 시작한다.
			//- MGTF에 해당하는 메시 그룹을 먼저 복제. 단 애니메이션은 복제하지 않는다.
			//- MGTF를 복사하면서 복제된 메시 그룹을 연결한다.
			//- 모든 모디파이어, 타임라인을 검사하면서 값을 복사/연결한다.


			if (srcMeshGroupTransform == null
				|| Editor == null
				|| Editor.Select == null
				|| Editor.Select.MeshGroup == null
				|| Editor._portrait == null)
			{
				return null;
			}

			//이 MeshTransform을 포함하는 MeshGroup을 찾는다.
			//parentMeshGroup = 해당 Transform을 가지고 있는 메시 그룹 (복사 대상 아님)
			//srcLinkedMeshGroup = 해당 MeshGroup Transform에 해당하는 메시 그룹 (복사 대상)

			apMeshGroup parentMeshGroup = Editor._portrait._meshGroups.Find(delegate(apMeshGroup a)
			{
				if(a._childMeshGroupTransforms == null)
				{
					return false;
				}
				return a._childMeshGroupTransforms.Contains(srcMeshGroupTransform);
			});

			apMeshGroup srcLinkedMeshGroup = srcMeshGroupTransform._meshGroup;

			if(parentMeshGroup == null || srcLinkedMeshGroup == null)
			{
				Debug.LogError("적절한 MeshGroup을 찾지 못했다.");
				return null;
			}

			//모든 상위 MeshGroup을 찾자
			//상위의 MeshGroup의 모디파이어나 AnimClip이 이 MeshTransform을 참조할 수 있다.
			//아이고
			List<apMeshGroup> srcMeshGroupsAll = new List<apMeshGroup>();
			srcMeshGroupsAll.Add(parentMeshGroup);
			if(parentMeshGroup._parentMeshGroup != null)
			{
				apMeshGroup curParentMeshGroup = parentMeshGroup._parentMeshGroup;
				while(true)
				{
					if(curParentMeshGroup == null) { break; }
					if(srcMeshGroupsAll.Contains(curParentMeshGroup)) { break; }
					
					srcMeshGroupsAll.Add(curParentMeshGroup);

					curParentMeshGroup = curParentMeshGroup._parentMeshGroup;
				}
			}

			try
			{
				MeshGroupDupcliateConvert convertInfo = new MeshGroupDupcliateConvert();

				//여기서 Undo가 된다.
				bool isSkipUndoIncrement = isUndoAndRefresh ? false : true;//Undo를 요청하지 않았다면 MeshGroup을 Duplicate할 때 UndoGroup을 증가시키지 않는다.

				apMeshGroup dstLinkedMeshGroup = DuplicateMeshGroup(srcLinkedMeshGroup, convertInfo, true, false, "Duplicate MeshGroup Transform", false, isSkipUndoIncrement);
				if(dstLinkedMeshGroup == null)
				{
					//원본 메시 그룹의 복사 실패
					Debug.LogError("AnyPortrait : Duplicating Mesh Group Transform is failed.");
					return null;
				}

				if(!convertInfo.Src2Dst_MeshGroup.ContainsKey(srcLinkedMeshGroup))
				{
					convertInfo.Src2Dst_MeshGroup.Add(srcLinkedMeshGroup, dstLinkedMeshGroup);
				}

				//복사된 MeshGroup을 MeshGroup Transform으로 등록하자 (Depth는 최상위로 연결)
				apTransform_MeshGroup dstMeshGroupTransform = AddMeshGroupToMeshGroup(dstLinkedMeshGroup, parentMeshGroup, null, false);
				
				if(dstMeshGroupTransform == null)
				{
					Debug.LogError("AnyPortrait : Duplicating Mesh Transform is failed. Please try again.");
					return null;
				}

				if(!convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcMeshGroupTransform))
				{
					convertInfo.Src2Dst_MeshGroupTransform.Add(srcMeshGroupTransform, dstMeshGroupTransform);
				}

				//Matrix를 동일하게 설정한다.
				dstMeshGroupTransform._matrix.SetMatrix(srcMeshGroupTransform._matrix, true);

				//Depth를 설정하자.
				//동일 레벨의 Transform들의 Depth 중에서 "SrcDepth"보다 큰 값 중 "최소값"을 찾고, 그 자리에 넣자
				int srcDepth = srcMeshGroupTransform._depth;
				int minNextDepth = -1;

				apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;

				int nMeshTransform = parentMeshGroup._childMeshTransforms == null ? 0 : parentMeshGroup._childMeshTransforms.Count;
				int nMeshGroupTransform = parentMeshGroup._childMeshGroupTransforms == null ? 0 : parentMeshGroup._childMeshGroupTransforms.Count;

				for (int iMeshTF = 0; iMeshTF < nMeshTransform; iMeshTF++)
				{
					curMeshTF = parentMeshGroup._childMeshTransforms[iMeshTF];
					if (curMeshTF._depth > srcDepth)
					{
						if (minNextDepth == srcDepth || curMeshTF._depth < minNextDepth)
						{
							minNextDepth = curMeshTF._depth;
						}
					}
				}

				for (int iMeshGroupTF = 0; iMeshGroupTF < nMeshGroupTransform; iMeshGroupTF++)
				{
					curMeshGroupTF = parentMeshGroup._childMeshGroupTransforms[iMeshGroupTF];
					if(curMeshGroupTF == srcMeshGroupTransform
						|| curMeshGroupTF == dstMeshGroupTransform)
					{
						//같은 거면 패스
						continue;
					}
					if (curMeshGroupTF._depth > srcDepth)
					{
						if (minNextDepth == srcDepth || curMeshGroupTF._depth < minNextDepth)
						{
							minNextDepth = curMeshGroupTF._depth;
						}
					}
				}

				if(minNextDepth < srcDepth + 1)
				{
					minNextDepth = srcDepth + 1;
				}
				int moveOffset = (minNextDepth - srcDepth) + 100;


				//다시 모든 동일 레벨 MeshTransform과 MeshGroup Transform의 Depth를 올린다.
				for (int iMeshTF = 0; iMeshTF < nMeshTransform; iMeshTF++)
				{
					curMeshTF = parentMeshGroup._childMeshTransforms[iMeshTF];
					if (curMeshTF._depth > srcDepth)
					{
						curMeshTF._depth += moveOffset;
					}
				}

				for (int iMeshGroupTF = 0; iMeshGroupTF < nMeshGroupTransform; iMeshGroupTF++)
				{
					curMeshGroupTF = parentMeshGroup._childMeshGroupTransforms[iMeshGroupTF];
					if(curMeshGroupTF == srcMeshGroupTransform
						|| curMeshGroupTF == dstMeshGroupTransform)
					{
						//같은 거면 패스
						continue;
					}
					if (curMeshGroupTF._depth > srcDepth)
					{
						curMeshGroupTF._depth += moveOffset;
					}
				}

				dstMeshGroupTransform._depth = minNextDepth;
				
				//Editor.Select.MeshGroup.SetDirtyToSort();
				//Editor.Select.MeshGroup.SortRenderUnits(true);

				//변경 [v1.4.2]
				Editor.Select.MeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//여기서는 일단 Sorting만 하자



				//이제 연결을 하자
				//ModMesh / ModBone 모두 복사해야해서 난감..


				int nMeshGroups = srcMeshGroupsAll.Count;
				apMeshGroup curMeshGroup = null;
				for (int iMeshGroup = 0; iMeshGroup < nMeshGroups; iMeshGroup++)
				{
					curMeshGroup = srcMeshGroupsAll[iMeshGroup];
					if (curMeshGroup == null)
					{
						continue;
					}

					int nModifier = curMeshGroup._modifierStack._modifiers == null ? 0 : curMeshGroup._modifierStack._modifiers.Count;

					apModifierBase curModifier = null;
					List<apModifierParamSetGroup> addedPSG = new List<apModifierParamSetGroup>();//PSG에는 여기에 넣고 나중에 한꺼번에 추가하자

					for (int iModifier = 0; iModifier < nModifier; iModifier++)
					{
						curModifier = curMeshGroup._modifierStack._modifiers[iModifier];
						if (curModifier == null)
						{
							continue;
						}

						bool isAnimated = curModifier.IsAnimated;

						//Debug.Log("메시 그룹 : [" + curMeshGroup._name + "] / 모디파이어 [" + curModifier.DisplayName + "] (Animated : " + isAnimated + ")");

						//애니메이션과 비 애니메이션의 차이가 있다.
						//- 애니메이션 타입
						//	: ParamSetGroup이 TimelineLayer로서, 만약 해당 TimelineLayer가 srcMeshTransform을 가지고 있는 경우,
						//	아예 ParamSetGroup + TimelineLayer를 복사해야한다.
						//- 비 애니메이션 타입
						//	: ParamSet 내부에 ModMesh를 찾아서 복사한뒤 ModMesh에 넣어주면 된다.
						int nPSG = curModifier._paramSetGroup_controller == null ? 0 : curModifier._paramSetGroup_controller.Count;
						apModifierParamSetGroup srcPSG = null;
						addedPSG.Clear();

						//Debug.LogWarning(">>> Prev PSG : " + nPSG);
						//int maxPSGLayerIndex = -1;

						////미리 최대 인덱스를 계산하자
						//for (int iPSG = 0; iPSG < nPSG; iPSG++)
						//{
						//	srcPSG = curModifier._paramSetGroup_controller[iPSG];
						//	if (srcPSG._layerIndex > maxPSGLayerIndex)
						//	{
						//		maxPSGLayerIndex = srcPSG._layerIndex;
						//	}
						//}

						for (int iPSG = 0; iPSG < nPSG; iPSG++)
						{
							srcPSG = curModifier._paramSetGroup_controller[iPSG];
							if (srcPSG == null)
							{
								continue;
							}

							if (isAnimated)
							{
								DuplicateModifierParamSetGroupAndTimelineLayer(curModifier, srcPSG, addedPSG, convertInfo, true, true, true);
							}
							else
							{
								//비 애니메이션 타입이면 ParamSet 내부의 ModMesh/ModBone을 복사해야한다.
								DuplicateModMeshesAndBonesWhenNoAnimated(curModifier, srcPSG, convertInfo, true, true, true);
							}
						}

						//추가할 ParamSetGroup이 있다면 추가하기
						if(addedPSG.Count > 0)
						{
							for (int iPSG = 0; iPSG < addedPSG.Count; iPSG++)
							{
								curModifier._paramSetGroup_controller.Add(addedPSG[iPSG]);
							}
							addedPSG.Clear();
						}

						//Debug.LogWarning(">>> Next PSG : " + curModifier._paramSetGroup_controller.Count);
					}
				}

				if (isUndoAndRefresh)
				{
					Editor.OnAnyObjectAddedOrRemoved(true);

					//현재 선택된 메시 그룹(복사되는 서브 메시 그룹 말고)에 대해서 Link
					apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(Editor.Select.MeshGroup);

					Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);
					Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

					//Refresh 추가
					//Editor.Select.RefreshAnimEditing(true);
					Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

					//메시 그룹 선택
					//Editor.Select.SetMeshGroup(newMeshGroup);
					Editor.Select.MeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
					Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

					//여기서 Depth를 넣자
					Editor.Select.MeshGroup.ResetRenderUnitsChildAndRoot();//Root > Child 렌더 유닛 재검토를 먼저 하자
					Editor.Select.MeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);

					//v1.5.0 버그 수정용 추가 : 여기서 완벽하게 한번 더 갱신을 해줘야 새로 추가된 RenderUnit <-> 모디파이어 연결이 완료된다.
					Editor.Select.MeshGroup.SetDirtyToReset();
					Editor.Select.MeshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);

					Editor.Select.SelectMeshGroupTF(dstMeshGroupTransform, apSelection.MULTI_SELECT.Main);

					////프리팹이었으면 Apply
					//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

					Editor.ResetHierarchyAll();
					Editor.RefreshControllerAndHierarchy(true);
					Editor.Notification(dstMeshGroupTransform._nickName + " is Duplicated", true, false);

					apEditorUtil.ReleaseGUIFocus();
				}
				return dstMeshGroupTransform;

			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : Duplicating Mesh Group Transform is failed.\n(Exception : " + ex + ")");
			}

			return null;
		}


		/// <summary>
		/// 추가 21.6.21 : 다중 선택된 MeshTF와 MeshGroupTF
		/// </summary>
		/// <param name="meshTFs"></param>
		/// <param name="meshGroupTFs"></param>
		public void DuplicateMultipleTFsInSameMeshGroup(List<apTransform_Mesh> meshTFs, List<apTransform_MeshGroup> meshGroupTFs)
		{
			//실제로 복사할 수 있는 리스트를 만들자.
			//만약 meshTF나 meshGroupTF 중에서 상위 meshGroupTF가 선택된 상태라면 제외
			if (apVersion.I.IsDemo)//서브 객체 복제 불가 (내부 코드)
			{
				Debug.LogError("AnyPortrait : The demo version does not support this feature.");
				return;
			}
			int nSrcMeshTFs = meshTFs != null ? meshTFs.Count : 0;
			int nSrcMeshGroupTFs = meshGroupTFs != null ? meshGroupTFs.Count : 0;

			if(nSrcMeshTFs + nSrcMeshGroupTFs == 0
				|| Editor == null
				|| Editor.Select == null
				|| Editor.Select.MeshGroup == null
				|| Editor._portrait == null)
			{
				return;
			}

			apMeshGroup rootMeshGroup = Editor.Select.MeshGroup;
			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			List<apTransform_Mesh> targetMeshTFs = new List<apTransform_Mesh>();
			List<apTransform_MeshGroup> targetMeshGroupTFs = new List<apTransform_MeshGroup>();

			if(nSrcMeshGroupTFs > 0)
			{
				List<apMeshGroup> selectedMeshGroups = new List<apMeshGroup>();
				for (int iMeshGroupTF = 0; iMeshGroupTF < nSrcMeshGroupTFs; iMeshGroupTF++)
				{
					selectedMeshGroups.Add(meshGroupTFs[iMeshGroupTF]._meshGroup);
				}

				//MeshGroup TF를 선택했다면,
				//다른 TF들의 MeshGroup들을 확인하여, 루트가 아닌 부모중 하나라도 선택된 서브 MeshGroupTF에 속한다면 복제를 하지 않는다.
				if(nSrcMeshTFs > 0)
				{	
					for (int iMeshTF = 0; iMeshTF < nSrcMeshTFs; iMeshTF++)
					{
						curMeshTF = meshTFs[iMeshTF];
						apMeshGroup parentMeshGroup = Editor._portrait._meshGroups.Find(delegate(apMeshGroup a)
						{
							if(a._childMeshTransforms == null)
							{
								return false;
							}
							return a._childMeshTransforms.Contains(curMeshTF);
						});
						if(parentMeshGroup == null)
						{
							continue;
						}

						if(parentMeshGroup == rootMeshGroup)
						{
							//Root MeshGroup에 속해있으니 바로 복제 가능하다.
							targetMeshTFs.Add(curMeshTF);
						}
						else
						{
							//parentMeshGroup 또는 그 부모 메시 그룹들이
							//모두 meshGroupTFs에 속하면 안된다.
							//상위로 올라가면서 반복 탐색해야한다.
							bool isInSelectedMeshGroupTF = false;
							while(true)
							{
								if(selectedMeshGroups.Contains(parentMeshGroup))
								{
									//메시 그룹 TF에 속한 메시 TF다. > 복제 대상이 아님
									isInSelectedMeshGroupTF = true;
									break;
								}

								if(parentMeshGroup._parentMeshGroup == null || parentMeshGroup._parentMeshGroup == rootMeshGroup)
								{
									//더 탐색 불가
									break;
								}

								//위로 탐색
								parentMeshGroup = parentMeshGroup._parentMeshGroup;
							}

							if(!isInSelectedMeshGroupTF)
							{
								//복제되는 다른 MeshGroupTF에 속하지 않았다.
								//복제 가능
								targetMeshTFs.Add(curMeshTF);
							}
						}
					}
				}

				for (int iMeshGroupTF = 0; iMeshGroupTF < nSrcMeshGroupTFs; iMeshGroupTF++)
				{
					curMeshGroupTF = meshGroupTFs[iMeshGroupTF];
					if(curMeshGroupTF._meshGroup == null)
					{
						continue;
					}

					apMeshGroup parentMeshGroup = curMeshGroupTF._meshGroup._parentMeshGroup;
					if(parentMeshGroup == rootMeshGroup)
					{
						//루트 메시 그룹의 1차 자식이면 복제 가능
						targetMeshGroupTFs.Add(curMeshGroupTF);
						continue;
					}

					
					//이 메시 그룹의 부모가 (당사자 말고) 선택된 메시 그룹에 포함되어 있다면
					//복제 불가
					bool isInSelectedMeshGroupTF = false;
					while(true)
					{
						if(selectedMeshGroups.Contains(parentMeshGroup))
						{
							//메시 그룹 TF에 속한 메시 TF다. > 복제 대상이 아님
							isInSelectedMeshGroupTF = true;
							break;
						}

						if(parentMeshGroup._parentMeshGroup == null || parentMeshGroup._parentMeshGroup == rootMeshGroup)
						{
							//더 탐색 불가
							break;
						}

						//위로 탐색
						parentMeshGroup = parentMeshGroup._parentMeshGroup;
					}

					if(!isInSelectedMeshGroupTF)
					{
						//복제되는 다른 MeshGroupTF에 속하지 않았다.
						//복제 가능
						targetMeshGroupTFs.Add(curMeshGroupTF);
					}
				}
				
			}
			else
			{
				//그게 아니라면, 모두 복제 (MeshTF만 있는 경우)
				for (int i = 0; i < nSrcMeshTFs; i++)
				{
					targetMeshTFs.Add(meshTFs[i]);
				}
			}


			if(targetMeshTFs.Count == 0 && targetMeshGroupTFs.Count == 0)
			{
				//복제될게 없다.
				return;
			}

			//Undo 등록 (모든 MeshGroup복사)
			apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.MeshGroup_DuplicateMeshTransform, 
																		Editor, 
																		Editor._portrait, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.StructChanged);

			
			//복제 전에, MeshTF와 MeshGroupTF를 합쳐서 depth 순서대로 정렬하자. depth가 낮은것부터 복사를 하도록
			List<object> targetObjs = new List<object>();
			for (int i = 0; i < targetMeshTFs.Count; i++)
			{
				targetObjs.Add(targetMeshTFs[i]);
			}

			for (int i = 0; i < targetMeshGroupTFs.Count; i++)
			{
				targetObjs.Add(targetMeshGroupTFs[i]);
			}

			targetObjs.Sort(delegate(object a, object b)
			{
				int depthA = 0;
				int depthB = 0;

				if(a is apTransform_Mesh)
				{
					depthA = (a as apTransform_Mesh)._depth;
				}
				else if(a is apTransform_MeshGroup)
				{
					depthA = (a as apTransform_MeshGroup)._depth;
				}

				if(b is apTransform_Mesh)
				{
					depthB = (b as apTransform_Mesh)._depth;
				}
				else if(b is apTransform_MeshGroup)
				{
					depthB = (b as apTransform_MeshGroup)._depth;
				}

				return depthA - depthB;//오름차순
			});

			//이제 하나씩 복사를 하자
			List<object> duplicatedObjs = new List<object>();

			object curObj = null;
			for (int i = 0; i < targetObjs.Count; i++)
			{
				curObj = targetObjs[i];
				if(curObj is apTransform_Mesh)
				{
					curMeshTF = curObj as apTransform_Mesh;
					apTransform_Mesh dupMeshTF = DuplicateMeshTransformInSameMeshGroup(curMeshTF, false);
					if(dupMeshTF != null)
					{
						duplicatedObjs.Add(dupMeshTF);
					}
				}
				else if(curObj is apTransform_MeshGroup)
				{
					curMeshGroupTF = curObj as apTransform_MeshGroup;
					apTransform_MeshGroup dupMeshGroupTF = DuplicateMeshGroupTransformInSameMeshGroup(curMeshGroupTF, false);
					if(dupMeshGroupTF != null)
					{
						duplicatedObjs.Add(dupMeshGroupTF);
					}
				}

				//중간에 SortRenderUnit을 하자 (Depth 할당 없이)
				Editor.Select.MeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);
			}

			Editor.OnAnyObjectAddedOrRemoved(true);

			apUtil.LinkRefresh.Set_AllObjects(Editor.Select.MeshGroup);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);

			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			//메시 그룹 선택
			//Editor.Select.SetMeshGroup(newMeshGroup);
			Editor.Select.MeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
			Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

			//1.4.2 : 여기서 Depth를 일괄적으로 할당하자
			Editor.Select.MeshGroup.ResetRenderUnitsChildAndRoot();//Root > Child의 렌더 유닛들을 전체 검토
			Editor.Select.MeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);

			////프리팹이었으면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

			//복사된 것을 선택한다.
			Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);

			for (int i = 0; i < duplicatedObjs.Count; i++)
			{
				curObj = duplicatedObjs[i];
				if (curObj is apTransform_Mesh)
				{
					curMeshTF = curObj as apTransform_Mesh;
					Editor.Select.SelectMeshTF(curMeshTF, apSelection.MULTI_SELECT.AddOrSubtract);
				}
				else if (curObj is apTransform_MeshGroup)
				{
					curMeshGroupTF = curObj as apTransform_MeshGroup;
					Editor.Select.SelectMeshGroupTF(curMeshGroupTF, apSelection.MULTI_SELECT.AddOrSubtract);
				}
			}

			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(true);
			Editor.Notification(targetObjs.Count + " Objects are Duplicated", true, false);

			apEditorUtil.ReleaseGUIFocus();
		}








		/// <summary>
		/// Duplicate Mesh Transform과 Duplicate MeshGroup Transform 함수에서 사용되는 함수
		/// Modifier ParamSetGroup과 연결된 TimelineLayer를 찾아서 복제한 뒤 AnimClip에 넣는다.
		/// 복제된 PSG는 바로 Modifer에 추가하지 않고, addedPSG변수에 넣어서 리턴하니, 나중에 알아서 모디파이어에 추가하자
		/// </summary>
		private bool DuplicateModifierParamSetGroupAndTimelineLayer(	apModifierBase curModifier,
																		apModifierParamSetGroup srcPSG,
																		List<apModifierParamSetGroup> addedPSG,
																		MeshGroupDupcliateConvert convertInfo,
																		bool isTarget_MeshTransform,
																		bool isTarget_MeshGroupTransform,
																		bool isTarget_Bone)
		{
			if (srcPSG == null)
			{
				//Debug.LogError("No srcPSG");
				return false;
			}
			apAnimClip linkedAnimClip = srcPSG._keyAnimClip;
			apAnimTimeline linkedTimeline = srcPSG._keyAnimTimeline;
			apAnimTimelineLayer srcTimelineLayer = srcPSG._keyAnimTimelineLayer;

			if (linkedAnimClip == null
				|| linkedTimeline == null
				|| srcTimelineLayer == null)
			{
				//Debug.LogError("Null KeyAnim");
				return false;
			}

			if(srcTimelineLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				//ControlParam인 경우 복사 대상이 아니다.
				//Debug.LogError("Control Param Timeline Layer");
				return false;
			}

			if (convertInfo.Src2Dst_TimelineLayer.ContainsKey(srcTimelineLayer))
			{
				//이미 변환 정보가 등록되어 있다면 패스
				//Debug.LogError("이미 변환되어 복제된 타임라인 레이어 [" + srcTimelineLayer.DisplayName + "]");
				return false;
			}



			//이 타임라인이 유효한지 체크
			apTransform_Mesh srcLinkedMeshTransform = null;
			apTransform_MeshGroup srcLinkedMeshGroupTransform = null;
			apBone srcLinkedBone = null;

			apTransform_Mesh dstLinkedMeshTransform = null;
			apTransform_MeshGroup dstLinkedMeshGroupTransform = null;
			apBone dstLinkedBone = null;

			bool isUseModMesh = false;
			bool isUseModBone = false;

			switch (srcTimelineLayer._linkModType)
			{
				case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
					{
						if(!isTarget_MeshTransform)
						{
							return false;
						}

						srcLinkedMeshTransform = srcTimelineLayer._linkedMeshTransform;
						
						if(srcLinkedMeshTransform != null
							&& convertInfo.Src2Dst_MeshTransform.ContainsKey(srcLinkedMeshTransform))
						{
							dstLinkedMeshTransform = convertInfo.Src2Dst_MeshTransform[srcLinkedMeshTransform];
						}

						isUseModMesh = true;
					}
					break;

				case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
					{
						if(!isTarget_MeshGroupTransform)
						{
							return false;
						}

						srcLinkedMeshGroupTransform = srcTimelineLayer._linkedMeshGroupTransform;

						if(srcLinkedMeshGroupTransform != null
							&& convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcLinkedMeshGroupTransform))
						{
							dstLinkedMeshGroupTransform = convertInfo.Src2Dst_MeshGroupTransform[srcLinkedMeshGroupTransform];
						}

						isUseModMesh = true;
					}
					break;

				case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
					{
						if(!isTarget_Bone)
						{
							return false;
						}

						srcLinkedBone = srcTimelineLayer._linkedBone;

						if(srcLinkedBone != null
							&& convertInfo.Src2Dst_Bone.ContainsKey(srcLinkedBone))
						{
							dstLinkedBone = convertInfo.Src2Dst_Bone[srcLinkedBone];
						}

						isUseModBone = true;
					}
					break;
			}

			//변환 대상이 아니면 실패 (이 조건이 아니면 죄다 복사하게 된다)
			if(dstLinkedMeshTransform == null &&
				dstLinkedMeshGroupTransform == null &&
				dstLinkedBone == null)
			{
				//Debug.LogError("연결 정보 없음 [" + srcTimelineLayer.DisplayName + "] / " + srcTimelineLayer._linkModType);
				//switch (srcTimelineLayer._linkModType)
				//{
				//	case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
				//			Debug.LogWarning("  >> Mesh Transform : " 
				//		+ (srcLinkedMeshTransform == null ? "<None>" : srcLinkedMeshTransform._nickName) + " >> "
				//		+ (dstLinkedMeshTransform == null ? "<None>" : dstLinkedMeshTransform._nickName));
				//			break;

				//	case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
				//			Debug.LogWarning("  >> MeshGroup Transform : " 
				//		+ (srcLinkedMeshGroupTransform == null ? "<None>" : srcLinkedMeshGroupTransform._nickName) + " >> "
				//		+ (dstLinkedMeshGroupTransform == null ? "<None>" : dstLinkedMeshGroupTransform._nickName));
				//			break;

				//	case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
				//			Debug.LogWarning("  >> Bone : " 
				//		+ (srcLinkedBone == null ? "<None>" : srcLinkedBone._name) + " >> "
				//		+ (dstLinkedBone == null ? "<None>" : dstLinkedBone._name));
				//			break;
				//}

				return false;
			}
			

			//타임라인과 ParamSetGroup을 복사하자
			//>> 임시 리스트에 넣고, ParamSetGroup에 추가하는건 나중에 하자
			apModifierParamSetGroup dstPSG = new apModifierParamSetGroup(Editor._portrait, curModifier, curModifier._paramSetGroup_controller.Count + addedPSG.Count + 1);
			addedPSG.Add(dstPSG);
			convertInfo.Src2Dst_ParamSetGroup.Add(srcPSG, dstPSG);

			//기본 정보와 ParamSet + ModMesh를 복사하자
			dstPSG._parentModifier = curModifier;

			dstPSG._syncTarget = srcPSG._syncTarget;
			dstPSG._keyControlParamID = srcPSG._keyControlParamID;
			dstPSG._keyAnimClipID = srcPSG._keyAnimClipID;
			dstPSG._keyAnimTimelineID = srcPSG._keyAnimTimelineID;
			dstPSG._keyAnimTimelineLayerID = -1;//<<타임라인 레이어는 아래에서 적용

			//여기서 기본 링크도 해주자
			dstPSG._keyAnimClip = srcPSG._keyAnimClip;
			dstPSG._keyAnimTimeline = srcPSG._keyAnimTimeline;
			dstPSG._keyAnimTimelineLayer = null;//<<타임라인 레이어는 아래에서 적용

			//Debug.Log("대상 애니메이션 : [" + dstPSG._keyAnimClip._name + " / " + dstPSG._keyAnimTimeline.DisplayName + "]");

			dstPSG._isEnabled = srcPSG._isEnabled;

			dstPSG._layerWeight = srcPSG._layerWeight;
			dstPSG._blendMethod = srcPSG._blendMethod;

			dstPSG._isColorPropertyEnabled = srcPSG._isColorPropertyEnabled;

			//ParamSet을 복사하자
			int nParamSet = srcPSG._paramSetList == null ? 0 : srcPSG._paramSetList.Count;

			apModifierParamSet srcParamSet = null;
			apModifierParamSet dstParamSet = null;

			if (dstPSG._paramSetList == null)
			{
				dstPSG._paramSetList = new List<apModifierParamSet>();
			}

			for (int iPS = 0; iPS < nParamSet; iPS++)
			{
				srcParamSet = srcPSG._paramSetList[iPS];
				if (srcParamSet == null)
				{
					continue;
				}

				//ParamSet 만들고 등록 + 변환 정보
				dstParamSet = new apModifierParamSet();
				dstPSG._paramSetList.Add(dstParamSet);
				convertInfo.Src2Dst_ParamSet.Add(srcParamSet, dstParamSet);

				//기본 정보 복사
				dstParamSet._conSyncValue_Int = srcParamSet._conSyncValue_Int;
				dstParamSet._conSyncValue_Float = srcParamSet._conSyncValue_Float;
				dstParamSet._conSyncValue_Vector2 = srcParamSet._conSyncValue_Vector2;

				dstParamSet._conSyncValueRange_Under = srcParamSet._conSyncValueRange_Under;
				dstParamSet._conSyncValueRange_Over = srcParamSet._conSyncValueRange_Over;

				//키프레임은 일단 보류
				dstParamSet._overlapWeight = srcParamSet._overlapWeight;

				//ModMesh를 사용할지, ModBone을 사용할지 체크
				if (isUseModMesh)
				{
					//ModMesh를 복사하자
					int nModMesh = srcParamSet._meshData == null ? 0 : srcParamSet._meshData.Count;

					if (dstParamSet._meshData == null)
					{
						dstParamSet._meshData = new List<apModifiedMesh>();
					}

					apModifiedMesh srcModMesh = null;
					apModifiedMesh dstModMesh = null;

					//for문으로 작성하긴 하지만 사실 1개짜리 리스트여야 한다.
					for (int iSrcModMesh = 0; iSrcModMesh < nModMesh; iSrcModMesh++)
					{
						srcModMesh = srcParamSet._meshData[iSrcModMesh];
						if (srcModMesh == null)
						{
							continue;
						}

						dstModMesh = new apModifiedMesh();
						dstParamSet._meshData.Add(dstModMesh);//<<추가된 ModMesh를 리스트에 등록

						convertInfo.Src2Dst_ModMesh.Add(srcModMesh, dstModMesh);

						//기본 연결 정보 설정
						if(dstLinkedMeshTransform != null)
						{
							dstModMesh._transformUniqueID = dstLinkedMeshTransform._transformUniqueID;
							dstModMesh._meshUniqueID = dstLinkedMeshTransform._meshUniqueID;
							dstModMesh._isMeshTransform = true;
						}
						else if(dstLinkedMeshGroupTransform != null)
						{
							dstModMesh._transformUniqueID = dstLinkedMeshGroupTransform._transformUniqueID;
							dstModMesh._meshUniqueID = -1;
							dstModMesh._isMeshTransform = false;
						}
						else
						{
							//에러..
						}

						dstModMesh._isRecursiveChildTransform = srcModMesh._isRecursiveChildTransform;
						dstModMesh._meshGroupUniqueID_Transform = srcModMesh._meshGroupUniqueID_Transform;

						//값을 복사하자
						dstModMesh.CopyFromSrc(srcModMesh);
					}
				}
				
				if(isUseModBone)
				{
					//ModBone을 복사하자
					int nModBone = srcParamSet._boneData == null ? 0 : srcParamSet._boneData.Count;

					if(dstParamSet._boneData == null)
					{
						dstParamSet._boneData = new List<apModifiedBone>();
					}

					apModifiedBone srcModBone = null;
					apModifiedBone dstModBone = null;

					for (int iSrcModBone = 0; iSrcModBone < nModBone; iSrcModBone++)
					{
						srcModBone = srcParamSet._boneData[iSrcModBone];
						if (srcModBone == null)
						{
							continue;
						}

						dstModBone = new apModifiedBone();
						dstParamSet._boneData.Add(dstModBone);//<<추가된 ModBone을 리스트에 등록

						convertInfo.Src2Dst_ModBone.Add(srcModBone, dstModBone);

						//값을 복사하자
						apMeshGroup srcMeshGroupOfModifier = srcModBone._meshGroup_Modifier;
						apMeshGroup srcMeshGroupOfBone = srcModBone._meshGroup_Bone;
						apTransform_MeshGroup srcMGTF = srcModBone._meshGroupTransform;

						//meshGroup_Modifier은 그대로 유지,
						//meshGroup_Bone / meshGroupTransform은 변환 정보가 있어야 한다.

						apMeshGroup dstMeshGroupOfBone = null;
						apTransform_MeshGroup dstMGTF = null;

						if (srcMeshGroupOfBone != null
								&& convertInfo.Src2Dst_MeshGroup.ContainsKey(srcMeshGroupOfBone))
						{
							dstMeshGroupOfBone = convertInfo.Src2Dst_MeshGroup[srcMeshGroupOfBone];
						}

						if (srcMGTF != null
							&& convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcMGTF))
						{
							dstMGTF = convertInfo.Src2Dst_MeshGroupTransform[srcMGTF];
						}

						if (dstMeshGroupOfBone == null
								|| dstMGTF == null)
						{
							Debug.LogError("AnyPortrait : Duplicating Mod Bone is Failed > Target Missing");
							Debug.LogError("dstMeshGroupOfBone : " + (dstMeshGroupOfBone != null));
							Debug.LogError("dstMGTF : " + (dstMGTF != null));

							//if(dstMGTF == null && dstMeshGroupOfModifier != null && dstMeshGroupOfBone != null)
							//{
							//	Debug.Log(">> MeshGroup TF만 없다");
							//	Debug.Log(">> MeshGroup of Mod : " + dstMeshGroupOfModifier._name);
							//	Debug.Log(">> MeshGroup of Bone : " + dstMeshGroupOfBone._name);
							//	Debug.Log(">> srcMGTF : " + (srcMGTF != null ?  srcMGTF._nickName : "<None>"));
							//}
						}

						dstModBone._meshGroupUniqueID_Modifier = (srcMeshGroupOfModifier == null) ? -1 : srcMeshGroupOfModifier._uniqueID;
						dstModBone._meshGropuUniqueID_Bone = (dstMeshGroupOfBone == null) ? -1 : dstMeshGroupOfBone._uniqueID;
						dstModBone._transformUniqueID = (dstMGTF == null) ? -1 : dstMGTF._transformUniqueID;

						dstModBone._meshGroup_Modifier = srcMeshGroupOfModifier;
						dstModBone._meshGroup_Bone = dstMeshGroupOfBone;
						dstModBone._meshGroupTransform = dstMGTF;

						dstModBone._boneID = dstLinkedBone._uniqueID;
						dstModBone._bone = dstLinkedBone;

						dstModBone._transformMatrix.SetMatrix(srcModBone._transformMatrix, true);
					}
				}
			}


			//타임라인레이어도 만들고 복사하자
			int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
			if (nextLayerID < 0)
			{
				Debug.LogError("AnyPortrait : Duplicating Timeline Layer is failed. Please try again");
				return false;
			}
			apAnimTimelineLayer dstTimelineLayer = new apAnimTimelineLayer();
			linkedTimeline._layers.Add(dstTimelineLayer);//<<바로 추가
			convertInfo.Src2Dst_TimelineLayer.Add(srcTimelineLayer, dstTimelineLayer);

			//타임라인 레이어의 기본 정보를 복사하자
			dstTimelineLayer._uniqueID = nextLayerID;
			dstTimelineLayer._linkModType = srcTimelineLayer._linkModType;

			int dstTranformID = -1;
			int dstBoneID = -1;
			int dstControlParamID = -1;


			switch (dstTimelineLayer._linkModType)
			{
				case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
					{
						dstTranformID = dstLinkedMeshTransform == null ? -1 : dstLinkedMeshTransform._transformUniqueID;
					}
					break;

				case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
					{
						dstTranformID = dstLinkedMeshGroupTransform == null ? -1 : dstLinkedMeshGroupTransform._transformUniqueID;
					}
					break;

				case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
					{
						dstBoneID = dstLinkedBone == null ? -1 : dstLinkedBone._uniqueID;
					}
					break;
			}

			dstTimelineLayer._transformID = dstTranformID;
			dstTimelineLayer._boneID = dstBoneID;
			dstTimelineLayer._guiColor = srcTimelineLayer._guiColor;
			dstTimelineLayer._guiLayerVisible = srcTimelineLayer._guiLayerVisible;
			dstTimelineLayer._controlParamID = dstControlParamID;
			dstTimelineLayer._linkType = srcTimelineLayer._linkType;

			//키프레임을 복사하자
			int nSrcKeyframes = srcTimelineLayer._keyframes == null ? 0 : srcTimelineLayer._keyframes.Count;

			apAnimKeyframe srcKeyframe = null;
			apAnimKeyframe dstKeyframe = null;

			if (dstTimelineLayer._keyframes == null)
			{
				dstTimelineLayer._keyframes = new List<apAnimKeyframe>();
			}
			for (int iKeyframe = 0; iKeyframe < nSrcKeyframes; iKeyframe++)
			{
				srcKeyframe = srcTimelineLayer._keyframes[iKeyframe];
				if (srcKeyframe == null)
				{
					continue;
				}

				int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
				if (nextKeyframeID < 0)
				{
					Debug.LogError("AnyPortrait : Duplicating Keyframe is failed. Please try again");
					continue;
				}

				dstKeyframe = new apAnimKeyframe();

				dstKeyframe._uniqueID = nextKeyframeID;
				dstKeyframe._frameIndex = srcKeyframe._frameIndex;
				dstKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, dstKeyframe._frameIndex);
				dstKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;
				dstKeyframe._isActive = srcKeyframe._isActive;
				dstKeyframe._isLoopAsStart = srcKeyframe._isLoopAsStart;
				dstKeyframe._isLoopAsEnd = srcKeyframe._isLoopAsEnd;
				dstKeyframe._loopFrameIndex = srcKeyframe._loopFrameIndex;

				dstKeyframe._prevRotationBiasMode = srcKeyframe._prevRotationBiasMode;
				dstKeyframe._nextRotationBiasMode = srcKeyframe._nextRotationBiasMode;
				dstKeyframe._prevRotationBiasCount = srcKeyframe._prevRotationBiasCount;
				dstKeyframe._nextRotationBiasCount = srcKeyframe._nextRotationBiasCount;

				dstKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
				dstKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
				dstKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;


				//등록
				dstTimelineLayer._keyframes.Add(dstKeyframe);
				convertInfo.Src2Dst_Keyframe.Add(srcKeyframe, dstKeyframe);
			}

			//ParamSet과 키프레임이 모두 복사되었으니 연결하자
			//타임라인 레이어를 연결한다.
			dstPSG._keyAnimTimelineLayerID = dstTimelineLayer._uniqueID;
			dstPSG._keyAnimTimelineLayer = dstTimelineLayer;

			//Debug.Log(">>> 타임라인 레이어 생성 [" + dstTimelineLayer.DisplayName + " (" + dstTimelineLayer._uniqueID + ") ]");

			srcParamSet = null;
			dstParamSet = null;

			srcKeyframe = null;
			dstKeyframe = null;


			//키프레임을 연결한다.
			foreach (KeyValuePair<apModifierParamSet, apModifierParamSet> src2Dst_ParamSet in convertInfo.Src2Dst_ParamSet)
			{
				srcParamSet = src2Dst_ParamSet.Key;
				dstParamSet = src2Dst_ParamSet.Value;

				srcKeyframe = srcParamSet.SyncKeyframe;
				dstKeyframe = null;

				if (srcKeyframe != null && convertInfo.Src2Dst_Keyframe.ContainsKey(srcKeyframe))
				{
					dstKeyframe = convertInfo.Src2Dst_Keyframe[srcKeyframe];
				}
				if (dstKeyframe != null)
				{
					dstParamSet._keyframeUniqueID = dstKeyframe._uniqueID;
				}
			}

			return true;
		}


		/// <summary>
		/// Duplicate Mesh Transform과 Duplicate MeshGroup Transform 함수에서 사용되는 함수
		/// Modifier ParamSetGroup / ParamSet의 ModMesh나 ModBone을 복사한다.
		/// </summary>
		private bool DuplicateModMeshesAndBonesWhenNoAnimated(	apModifierBase curModifier,
																		apModifierParamSetGroup srcPSG,
																		MeshGroupDupcliateConvert convertInfo,
																		bool isTarget_MeshTransform,
																		bool isTarget_MeshGroupTransform,
																		bool isTarget_Bone
																		)
		{
			int nParamSet = srcPSG._paramSetList == null ? 0 : srcPSG._paramSetList.Count;

			//추가할 ModMesh와 ModBone을 리스트로 만들어 따로 저장한 뒤, 나중에 일괄적으로 추가한다.
			List<apModifiedMesh> addedModMeshes = new List<apModifiedMesh>();
			List<apModifiedBone> addedModBones = new List<apModifiedBone>();

			apModifierParamSet curParamSet = null;
			for (int iPS = 0; iPS < nParamSet; iPS++)
			{
				curParamSet = srcPSG._paramSetList[iPS];
				if (curParamSet == null)
				{
					continue;
				}

				addedModMeshes.Clear();
				addedModBones.Clear();

				//ModMesh 복사하기
				if (isTarget_MeshTransform || isTarget_MeshGroupTransform)
				{
					int nModMesh = curParamSet._meshData == null ? 0 : curParamSet._meshData.Count;

					apModifiedMesh srcModMesh = null;
					apModifiedMesh dstModMesh = null;

					apTransform_Mesh srcLinkedMeshTranform = null;
					apTransform_Mesh dstLinkedMeshTranform = null;
					apTransform_MeshGroup srcLinkedMeshGroupTranform = null;
					apTransform_MeshGroup dstLinkedMeshGroupTranform = null;

					for (int iSrcModMesh = 0; iSrcModMesh < nModMesh; iSrcModMesh++)
					{
						srcModMesh = curParamSet._meshData[iSrcModMesh];

						if (srcModMesh == null)
						{
							continue;
						}

						//타겟이 맞지 않은 경우인지 체크
						if (srcModMesh._isMeshTransform && !isTarget_MeshTransform)
						{
							continue;
						}
						if (!srcModMesh._isMeshTransform && !isTarget_MeshGroupTransform)
						{
							continue;
						}

						//연결되고 변환된 Mesh/MeshGroup Transform을 찾자
						srcLinkedMeshTranform = null;
						dstLinkedMeshTranform = null;
						srcLinkedMeshGroupTranform = null;
						dstLinkedMeshGroupTranform = null;

						if (srcModMesh._isMeshTransform &&
							srcModMesh._transform_Mesh != null)
						{
							//Mesh Transform
							srcLinkedMeshTranform = srcModMesh._transform_Mesh;

							if (srcLinkedMeshTranform != null
								&& convertInfo.Src2Dst_MeshTransform.ContainsKey(srcLinkedMeshTranform))
							{
								dstLinkedMeshTranform = convertInfo.Src2Dst_MeshTransform[srcLinkedMeshTranform];
							}
						}
						else if (!srcModMesh._isMeshTransform
							&& srcModMesh._transform_MeshGroup != null)
						{
							//MeshGroup Transform
							srcLinkedMeshGroupTranform = srcModMesh._transform_MeshGroup;

							if (srcLinkedMeshGroupTranform != null
								&& convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcLinkedMeshGroupTranform))
							{
								dstLinkedMeshGroupTranform = convertInfo.Src2Dst_MeshGroupTransform[srcLinkedMeshGroupTranform];
							}
						}

						if (dstLinkedMeshTranform == null && dstLinkedMeshGroupTranform == null)
						{
							//변환 정보가 없다.
							//이 ModMesh는 대상이 아니다.
							continue;
						}

						//ModMesh를 복사하자
						dstModMesh = new apModifiedMesh();
						//curParamSet._meshData.Add(dstModMesh);//<<추가된 ModMesh를 리스트에 등록
						//일단 리스트에 넣고 나중에 일괄 처리
						addedModMeshes.Add(dstModMesh);

						convertInfo.Src2Dst_ModMesh.Add(srcModMesh, dstModMesh);

						//기본 연결 정보 설정
						if(dstLinkedMeshTranform != null)
						{
							dstModMesh._transformUniqueID = dstLinkedMeshTranform._transformUniqueID;
							dstModMesh._meshUniqueID = dstLinkedMeshTranform._meshUniqueID;
							dstModMesh._isMeshTransform = true;
						}
						else if(dstLinkedMeshGroupTranform != null)
						{
							dstModMesh._transformUniqueID = dstLinkedMeshGroupTranform._transformUniqueID;
							dstModMesh._meshUniqueID = -1;
							dstModMesh._isMeshTransform = false;
						}
						else
						{
							//에러..
						}

						dstModMesh._isRecursiveChildTransform = srcModMesh._isRecursiveChildTransform;
						dstModMesh._meshGroupUniqueID_Transform = srcModMesh._meshGroupUniqueID_Transform;

						//값을 복사하자
						dstModMesh.CopyFromSrc(srcModMesh);

					}
				}

				//Mod Bone 복사하기
				if(isTarget_Bone)
				{
					int nModBone = curParamSet._boneData == null ? 0 : curParamSet._boneData.Count;

					apModifiedBone srcModBone = null;
					apModifiedBone dstModBone = null;

					apBone srcLinkedBone = null;
					apBone dstLinkedBone = null;

					for (int iSrcModBone = 0; iSrcModBone < nModBone; iSrcModBone++)
					{
						srcModBone = curParamSet._boneData[iSrcModBone];

						srcLinkedBone = null;
						dstLinkedBone = null;

						if (srcModBone == null)
						{
							continue;
						}

						srcLinkedBone = srcModBone._bone;
						if(srcLinkedBone != null
							&& convertInfo.Src2Dst_Bone.ContainsKey(srcLinkedBone))
						{
							dstLinkedBone = convertInfo.Src2Dst_Bone[srcLinkedBone];
						}

						if(dstLinkedBone == null)
						{
							//변환 정보가 없다. 대상이 아니다.
							continue;
						}

						dstModBone = new apModifiedBone();
						//dstParamSet._boneData.Add(dstModBone);//<<추가된 ModBone을 리스트에 등록
						//일단 리스트에 넣자
						addedModBones.Add(dstModBone);

						convertInfo.Src2Dst_ModBone.Add(srcModBone, dstModBone);

						//값을 복사하자
						apMeshGroup srcMeshGroupOfModifier = srcModBone._meshGroup_Modifier;
						apMeshGroup srcMeshGroupOfBone = srcModBone._meshGroup_Bone;
						apTransform_MeshGroup srcMGTF = srcModBone._meshGroupTransform;

						//meshGroup_Modifier은 그대로 유지,
						//meshGroup_Bone / meshGroupTransform은 변환 정보가 있어야 한다.

						apMeshGroup dstMeshGroupOfBone = null;
						apTransform_MeshGroup dstMGTF = null;

						if (srcMeshGroupOfBone != null
								&& convertInfo.Src2Dst_MeshGroup.ContainsKey(srcMeshGroupOfBone))
						{
							dstMeshGroupOfBone = convertInfo.Src2Dst_MeshGroup[srcMeshGroupOfBone];
						}

						if (srcMGTF != null
							&& convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcMGTF))
						{
							dstMGTF = convertInfo.Src2Dst_MeshGroupTransform[srcMGTF];
						}

						if (dstMeshGroupOfBone == null
								|| dstMGTF == null)
						{
							Debug.LogError("AnyPortrait : Duplicating Mod Bone is Failed > Target Missing");
							Debug.LogError("dstMeshGroupOfBone : " + (dstMeshGroupOfBone != null));
							Debug.LogError("dstMGTF : " + (dstMGTF != null));

							//if(dstMGTF == null && dstMeshGroupOfModifier != null && dstMeshGroupOfBone != null)
							//{
							//	Debug.Log(">> MeshGroup TF만 없다");
							//	Debug.Log(">> MeshGroup of Mod : " + dstMeshGroupOfModifier._name);
							//	Debug.Log(">> MeshGroup of Bone : " + dstMeshGroupOfBone._name);
							//	Debug.Log(">> srcMGTF : " + (srcMGTF != null ?  srcMGTF._nickName : "<None>"));
							//}
						}

						dstModBone._meshGroupUniqueID_Modifier = (srcMeshGroupOfModifier == null) ? -1 : srcMeshGroupOfModifier._uniqueID;
						dstModBone._meshGropuUniqueID_Bone = (dstMeshGroupOfBone == null) ? -1 : dstMeshGroupOfBone._uniqueID;
						dstModBone._transformUniqueID = (dstMGTF == null) ? -1 : dstMGTF._transformUniqueID;

						dstModBone._meshGroup_Modifier = srcMeshGroupOfModifier;
						dstModBone._meshGroup_Bone = dstMeshGroupOfBone;
						dstModBone._meshGroupTransform = dstMGTF;

						dstModBone._boneID = dstLinkedBone._uniqueID;
						dstModBone._bone = dstLinkedBone;

						dstModBone._transformMatrix.SetMatrix(srcModBone._transformMatrix, true);
					}
				}

				//이제 새로운 ModMesh와 ModBone을 추가하자
				if (addedModMeshes.Count > 0)
				{
					for (int iModMesh = 0; iModMesh < addedModMeshes.Count; iModMesh++)
					{
						curParamSet._meshData.Add(addedModMeshes[iModMesh]);
					}
					addedModMeshes.Clear();
				}

				if (addedModBones.Count > 0)
				{
					for (int iModBone = 0; iModBone < addedModBones.Count; iModBone++)
					{
						curParamSet._boneData.Add(addedModBones[iModBone]);
					}
					addedModBones.Clear();
				}
			}

			return true;
		}


		/// <summary>
		/// 선택된 MeshTransform을 다른 MeshGroup으로 옮깁니다.
		/// 단, 대상이 되는 MeshGroup은 srcMeshGroup의 부모나 자식이어야 하며, 부모로 옮기는 경우엔 일부 모디파이어의 정보가 삭제된다.
		/// </summary>
		/// <param name="targetMeshTransform"></param>
		/// <param name="srcMeshGroup"></param>
		/// <param name="dstMeshGroup"></param>
		/// <returns></returns>
		public bool MigrateMeshTransformToOtherMeshGroup(apTransform_Mesh targetMeshTransform, List<apTransform_Mesh> targetMeshTFs, apMeshGroup srcMeshGroup, apMeshGroup dstMeshGroup)
		{
			if (apVersion.I.IsDemo)//서브 객체 복제 불가 (내부 코드)
			{
				Debug.LogError("AnyPortrait : The demo version does not support this feature.");
				return false;
			}

			if (targetMeshTransform == null
				|| Editor == null
				|| Editor.Select == null
				|| Editor.Select.MeshGroup == null
				|| Editor._portrait == null
				|| srcMeshGroup == dstMeshGroup
				|| srcMeshGroup == null
				|| dstMeshGroup == null)
			{
				return false;
			}


			try
			{
				//대상이 유효한지 확인
				//1. srcMeshGroup이 srcMeshTransform을 가지고 있는가
				//2. dstMeshGroup이 srcMeshGroup의 부모이거나 자식인가 (두가지를 구분할 것) - 추가 : 형제인 경우도 포함
				if (!srcMeshGroup._childMeshTransforms.Contains(targetMeshTransform))
				{
					return false;
				}

				//추가 21.6.24 : 다중 마이그레이션인지 확인
				//아래의 처리는 무조건 리스트로 한다.
				List<apTransform_Mesh> srcTargetMeshTFs = new List<apTransform_Mesh>();
				
				srcTargetMeshTFs.Add(targetMeshTransform);//메인은 일단 넣고

				apTransform_Mesh curMeshTF = null;

				if(targetMeshTFs != null && targetMeshTFs.Count > 0)
				{	
					for (int i = 0; i < targetMeshTFs.Count ; i++)
					{
						curMeshTF = targetMeshTFs[i];
						if(srcTargetMeshTFs.Contains(curMeshTF))
						{
							//이미 있는건 패스
							continue;
						}

						//같은 MeshGroup의 자식이어야 한다.
						if(srcMeshGroup._childMeshTransforms.Contains(curMeshTF))
						{
							srcTargetMeshTFs.Add(curMeshTF);
						}
					}

					//정렬은 Depth의 오름차순
					srcTargetMeshTFs.Sort(delegate(apTransform_Mesh a, apTransform_Mesh b)
					{
						return a._depth - b._depth;
					});
				}


				bool isMultiple = srcTargetMeshTFs.Count > 1;



				//모든 자식/부모 메시 그룹을 일단 수집한다.
				bool isMoveToParent = false;
				
				List<apMeshGroup> parentMeshGroups_FromSrc = new List<apMeshGroup>();
				List<apMeshGroup> parentMeshGroups_FromDst = new List<apMeshGroup>();
				List<apMeshGroup> allMeshGroups = new List<apMeshGroup>();
				List<apMeshGroup> childAndSiblingsMeshGroups_FromDst = new List<apMeshGroup>();

				FindAllParentMeshGroups(srcMeshGroup, parentMeshGroups_FromSrc);
				FindAllParentMeshGroups(dstMeshGroup, parentMeshGroups_FromDst);

				//루트를 찾자
				apMeshGroup rootMeshGroup = srcMeshGroup;
				while(true)
				{
					if(rootMeshGroup._parentMeshGroup == null)
					{
						break;
					}
					rootMeshGroup = rootMeshGroup._parentMeshGroup;
				}

				//전체 자식 리스트를 돌면서, parent에 속하지 않은 모든 MeshGroup들을 수집하자
				FindAllChildMeshGroups(rootMeshGroup, allMeshGroups);
				allMeshGroups.Add(rootMeshGroup);

				apMeshGroup curCheckMeshGroup = null;
				for (int iMG = 0; iMG < allMeshGroups.Count; iMG++)
				{
					curCheckMeshGroup = allMeshGroups[iMG];
					if(!parentMeshGroups_FromDst.Contains(curCheckMeshGroup))
					{
						//Dst 기준: 부모에 없는 메시 그룹이면 자식/형제 메시 그룹이다.
						childAndSiblingsMeshGroups_FromDst.Add(curCheckMeshGroup);
					}
				}

				if (!allMeshGroups.Contains(dstMeshGroup))
				{
					//전체 메시 그룹 리스트에 포함되지 않았다면 잘못된 MeshGroup
					Debug.LogError("AnyPortrait : Migration is failed.");
					return false;
				}
				if (parentMeshGroups_FromSrc.Contains(dstMeshGroup))
				{
					//dstMeshGroup이 부모 메시그룹이다.
					isMoveToParent = true;
				}

				if(isMoveToParent)
				{
					//Parent로 이동하는 경우
					//원래의 자식 메시 그룹에서의 일부 모디파이어의 정보가 누락될 수 있음을 경고
					//"Migration Warning"
					//"Currently, you have chosen to transfer the target mesh to the parent mesh group. In this case, all data, which are related to this mesh, of modifiers included in the children of the selected mesh group are deleted. Do you want to continue?"
					bool isResult = EditorUtility.DisplayDialog(
						Editor.GetText(TEXT.DLG_MigrationMeshTranformWarning_Title), 
						Editor.GetText(TEXT.DLG_MigrationMeshTranformWarning_Body), 
						Editor.GetText(TEXT.Okay), 
						Editor.GetText(TEXT.Cancel)
						);

					//취소!
					if(!isResult)
					{
						Editor.Notification("Migrating data has been canceled.", false, false);
						return false;
					}
				}

				//Debug.Log("Undo > Migration 저장");

				//일단 모든 상태를 저장한다.
				apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.MeshGroup_MigrateMeshTransform, 
																			Editor, 
																			Editor._portrait, 
																			//srcMeshGroup, 
																			false,
																			apEditorUtil.UNDO_STRUCT.StructChanged);

				//1. Mesh Transform을 그대로 옮겨서 대상 메시 그룹에 넣자.
				//Depth는 최대값을 적용한다.
				//Dst에 넣고, Src에서 제외한다.
				int maxDepth = 0;
				//apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;
				for (int iMeshTF = 0; iMeshTF < dstMeshGroup._childMeshTransforms.Count; iMeshTF++)
				{
					curMeshTF = dstMeshGroup._childMeshTransforms[iMeshTF];
					if (curMeshTF._depth > maxDepth)
					{
						maxDepth = curMeshTF._depth;
					}
				}

				for (int iMGTF = 0; iMGTF < dstMeshGroup._childMeshGroupTransforms.Count; iMGTF++)
				{
					curMeshGroupTF = dstMeshGroup._childMeshGroupTransforms[iMGTF];
					if (curMeshGroupTF._depth > maxDepth)
					{
						maxDepth = curMeshGroupTF._depth;
					}
				}

				maxDepth += 100;//여유있게 많이 추가

				//변경 21.6.24 : 여러개의 MeshTF를 변경할 수 있다.
				for (int iMeshTF = 0; iMeshTF < srcTargetMeshTFs.Count; iMeshTF++)
				{
					curMeshTF = srcTargetMeshTFs[iMeshTF];
					maxDepth += 10;//매번 최대 Depth를 추가 (위에 붙도록)


					//대상 메시 그룹에 추가
					if (!dstMeshGroup._childMeshTransforms.Contains(curMeshTF))//<대상 변경>
					{
						dstMeshGroup._childMeshTransforms.Add(curMeshTF);//<대상 변경>
					}
					//기존 메시 그룹에서 제거
					srcMeshGroup._childMeshTransforms.Remove(curMeshTF);//<대상 변경>

					//MeshTransform의 기본 값 중 연결 정보 위주로 수정하자.
					//<대상 변경>
					curMeshTF._depth = maxDepth;
					//curMeshTF._level = maxDepth;//사용되지 않는 값

					//클리핑 설정은 모두 해제
					//<대상 변경>
					curMeshTF._isClipping_Child = false;
					curMeshTF._isClipping_Parent = false;
					curMeshTF._clipIndexFromParent = -1;
					curMeshTF._clipParentMeshTransform = null;
					curMeshTF._clipChildMeshes.Clear();

					curMeshTF._linkedRenderUnit = null;

					//중요 > Matrix를 바꾸어야 한다.
					//이전 메시 그룹/변경된 메시 그룹에서의 Local2World Matrix를 구하고
					//이전 메시 그룹에서의 World Matrix가 동일하도록 Local Matrix를 다시 재계산하자
					apMatrix prevLocal2WorldMatrix = new apMatrix();
					apMatrix nextLocal2WorldMatrix = new apMatrix();

					//반복문을 이용하자
					apMeshGroup curCalMeshGroup = null;
					apMeshGroup curCalParentMeshGroup = null;
					apTransform_MeshGroup curSubParentMGTF = null;

					//이전 행렬을 구하자
					prevLocal2WorldMatrix.SetIdentity();

					prevLocal2WorldMatrix.OnBeforeRMultiply();//추가 20.8.6 [RMultiply Scale 이슈]

					curCalMeshGroup = srcMeshGroup;
					curCalParentMeshGroup = null;
					while (true)
					{
						//부모가 없다면 루트이다. 굳이 루트의 Matrix까지 넣을 필욘 없을 듯
						if (curCalMeshGroup == null || curCalMeshGroup._parentMeshGroup == null)
						{
							break;
						}

						//부모가 있다면, 부모 기준에서 MGTF의 Matrix를 계산에 넣자
						curCalParentMeshGroup = curCalMeshGroup._parentMeshGroup;
						curSubParentMGTF = curCalParentMeshGroup._childMeshGroupTransforms.Find(delegate (apTransform_MeshGroup a)
						{
							return a._meshGroup == curCalMeshGroup;
						});

						if (curSubParentMGTF == null)
						{
							Debug.LogError("World Matrix Correction Error [Src]");
							break;
						}
						prevLocal2WorldMatrix.RMultiply(curSubParentMGTF._matrix, false);

						//한단계 위로 이동
						curCalMeshGroup = curCalMeshGroup._parentMeshGroup;
					}

					prevLocal2WorldMatrix.MakeMatrix();

					//이번엔 변환 이후의 행렬을 구하자
					nextLocal2WorldMatrix.SetIdentity();

					nextLocal2WorldMatrix.OnBeforeRMultiply();//추가 20.8.6 [RMultiply Scale 이슈]

					curCalMeshGroup = dstMeshGroup;
					curCalParentMeshGroup = null;
					while (true)
					{
						//부모가 없다면 루트이다. 굳이 루트의 Matrix까지 넣을 필욘 없을 듯
						if (curCalMeshGroup == null || curCalMeshGroup._parentMeshGroup == null)
						{
							break;
						}

						//부모가 있다면, 부모 기준에서 MGTF의 Matrix를 계산에 넣자
						curCalParentMeshGroup = curCalMeshGroup._parentMeshGroup;
						curSubParentMGTF = curCalParentMeshGroup._childMeshGroupTransforms.Find(delegate (apTransform_MeshGroup a)
						{
							return a._meshGroup == curCalMeshGroup;
						});

						if (curSubParentMGTF == null)
						{
							Debug.LogError("World Matrix Correction Error [Dst]");
							break;
						}
						nextLocal2WorldMatrix.RMultiply(curSubParentMGTF._matrix, false);

						//한단계 위로 이동
						curCalMeshGroup = curCalMeshGroup._parentMeshGroup;
					}

					nextLocal2WorldMatrix.MakeMatrix();

					//Local > PrevLocal2World > World
					apMatrix targetWorldMatrix = new apMatrix(curMeshTF._matrix);//<대상 변경>

					//추가 20.8.6. [RMultiply Scale 이슈]
					targetWorldMatrix.OnBeforeRMultiply();

					targetWorldMatrix.RMultiply(prevLocal2WorldMatrix, true);

					//World > NextLocal2World_Inv > NextLocal
					apMatrix nextLocalMatrix = new apMatrix(targetWorldMatrix);
					nextLocalMatrix.RInverse(nextLocal2WorldMatrix, true);

					//변환 결과를 저장하자
					curMeshTF._matrix.SetMatrix(nextLocalMatrix, true);//<대상 변경>


					//이제 모디파이어에서 ModMesh를 찾아서 값을 변경해야한다.
					//부모 메시 그룹에서 이 MeshTF를 참조하는 모든 ModMesh를 찾아서 연결 정보를 갱신해야한다.

					apMeshGroup curMeshGroup = null;
					apModifierBase curModifier = null;
					apModifierParamSetGroup curPSG = null;
					apModifierParamSet curParamSet = null;
					apModifiedMesh curModMesh = null;
					for (int iParentMeshGroup = 0; iParentMeshGroup < parentMeshGroups_FromDst.Count; iParentMeshGroup++)
					{
						curMeshGroup = parentMeshGroups_FromDst[iParentMeshGroup];
						if (curMeshGroup == null)
						{
							continue;
						}

						int nModifiers = curMeshGroup._modifierStack._modifiers == null ? 0 : curMeshGroup._modifierStack._modifiers.Count;
						for (int iModifier = 0; iModifier < nModifiers; iModifier++)
						{
							curModifier = curMeshGroup._modifierStack._modifiers[iModifier];
							if (curModifier == null)
							{
								continue;
							}

							bool isAnimated = curModifier.IsAnimated;

							int nPSG = curModifier._paramSetGroup_controller == null ? 0 : curModifier._paramSetGroup_controller.Count;
							for (int iPSG = 0; iPSG < nPSG; iPSG++)
							{
								curPSG = curModifier._paramSetGroup_controller[iPSG];

								//- 애니메이션 타입이면 연결된 타임라인을 찾고, 해당 타임라인이 이 MeshTransform을 참조하는지 확인하자
								//- 애니메이션 타입이 아니면 ModMesh까지 찾아야 한다.
								bool isTargetPSG = false;
								if (isAnimated)
								{
									apAnimTimelineLayer linkedTimelineLayer = curPSG._keyAnimTimelineLayer;
									if (linkedTimelineLayer != null)
									{
										if (linkedTimelineLayer._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
											&& linkedTimelineLayer._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform
											&& linkedTimelineLayer._linkedMeshTransform == curMeshTF)//<대상 변경>
										{
											//해당 타임라인 레이어가 대상 메시 트랜스폼과 연결된 경우
											isTargetPSG = true;
										}
									}
								}
								else
								{
									//애니메이션 타입이 아니면 ModMesh를 직접 보기 전까진 모른다.
									isTargetPSG = true;

								}
								if (!isTargetPSG)
								{
									//이 PSG는 안봐도 되겠당
									continue;
								}

								int nPS = curPSG._paramSetList == null ? 0 : curPSG._paramSetList.Count;
								for (int iParamSet = 0; iParamSet < nPS; iParamSet++)
								{
									curParamSet = curPSG._paramSetList[iParamSet];
									if (curParamSet == null)
									{
										continue;
									}

									if (!isAnimated)
									{
										//애니메이션 타입이 아니면 여기서 검사하자
										//메시 트랜스폼이 등록되지 않았다면 패스
										if (!curParamSet.IsContainMeshTransform(curMeshTF))//<대상 변경>
										{
											continue;
										}
									}

									int nModMesh = curParamSet._meshData == null ? 0 : curParamSet._meshData.Count;
									for (int iModMesh = 0; iModMesh < nModMesh; iModMesh++)
									{
										curModMesh = curParamSet._meshData[iModMesh];
										if (curModMesh == null)
										{
											continue;
										}

										if (!curModMesh._isMeshTransform
											|| curModMesh._transform_Mesh == null
											|| curModMesh._transform_Mesh != curMeshTF)//<대상 변경>
										{
											//대상이 아니라면 패스
											continue;
										}

										//이제 연결 정보를 수정하자
										curModMesh._meshGroupOfTransform = dstMeshGroup;//<<이제 srcMeshGroup에서 dstMeshGroup으로 바뀌어야 한다.
										curModMesh._isRecursiveChildTransform = (curMeshGroup != dstMeshGroup);//메시 그룹이 다르다는 것은 모디파이어가 자식 메시 그룹에 연결된 것이다.
										curModMesh._meshGroupUniqueID_Transform = dstMeshGroup._uniqueID;

										//나머지는 그대로
									}
								}
							}
						}
					}


					//부모 메시 그룹으로 이동하는 경우) srcMeshGroup을 포함한 자식 메시 그룸에 있는 ModMesh를 찾아서 모두 삭제한다,.
					//삭제된 ModMesh/PSG 중에서 애니메이션과 관련된 경우, 삭제할 TimelineLayer를 리스트로 모은 뒤, 다 삭제하자
					if (isMoveToParent)
					{
						if (!childAndSiblingsMeshGroups_FromDst.Contains(srcMeshGroup))
						{
							childAndSiblingsMeshGroups_FromDst.Add(srcMeshGroup);
						}

						int nMeshGroup = childAndSiblingsMeshGroups_FromDst.Count;

						curMeshGroup = null;
						curModifier = null;
						curPSG = null;
						curParamSet = null;
						curModMesh = null;

						List<apAnimTimelineLayer> removeTimelineLayers = new List<apAnimTimelineLayer>();
						List<apModifierParamSetGroup> removePSGs = new List<apModifierParamSetGroup>();

						for (int iMeshGroup = 0; iMeshGroup < nMeshGroup; iMeshGroup++)
						{
							curMeshGroup = childAndSiblingsMeshGroups_FromDst[iMeshGroup];
							if (curMeshGroup == null)
							{
								continue;
							}


							int nModifier = curMeshGroup._modifierStack._modifiers == null ? 0 : curMeshGroup._modifierStack._modifiers.Count;
							for (int iModifier = 0; iModifier < nModifier; iModifier++)
							{
								curModifier = curMeshGroup._modifierStack._modifiers[iModifier];
								if (curModifier == null)
								{
									continue;
								}

								bool isAnimated = curModifier.IsAnimated;

								removeTimelineLayers.Clear();
								removePSGs.Clear();

								//애니메이션이면 연결된 PSG 자체와 타임라인 자체를 제거할 필요가 있다.
								int nPSG = curModifier._paramSetGroup_controller == null ? 0 : curModifier._paramSetGroup_controller.Count;
								for (int iPSG = 0; iPSG < nPSG; iPSG++)
								{
									curPSG = curModifier._paramSetGroup_controller[iPSG];
									if (curPSG == null)
									{
										continue;
									}

									//대상이 되는 메시 트랜스폼과 연결된 PSG인가
									if (isAnimated)
									{
										apAnimTimelineLayer linkedTimelineLayer = curPSG._keyAnimTimelineLayer;
										if (linkedTimelineLayer != null)
										{
											if (linkedTimelineLayer._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
												&& linkedTimelineLayer._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform
												&& linkedTimelineLayer._linkedMeshTransform == curMeshTF)//<대상 변경>
											{
												//해당 타임라인 레이어가 대상 메시 트랜스폼과 연결된 경우
												//바로 이 PSG와 Timeline Layer는 문답무용으로 삭제해야한다.
												removePSGs.Add(curPSG);
												if (!removeTimelineLayers.Contains(linkedTimelineLayer))
												{
													removeTimelineLayers.Add(linkedTimelineLayer);
												}
											}
										}

										//애니메이션 타입은 더 처리할 필요가 없다.
										continue;
									}

									int nPS = curPSG._paramSetList == null ? 0 : curPSG._paramSetList.Count;
									for (int iParamSet = 0; iParamSet < nPS; iParamSet++)
									{
										curParamSet = curPSG._paramSetList[iParamSet];
										if (curParamSet == null)
										{
											continue;
										}

										if (!curParamSet.IsContainMeshTransform(curMeshTF))//<대상 변경>
										{
											//이 MeshTransform이 포함되지 않았습니다.
											continue;
										}

										//이제 조건에 맞는 모든 ModMesh를 삭제하자
										if (curParamSet._meshData != null &&
											curParamSet._meshData.Count > 0)
										{
											//대상 메시 트랜스폼에 대한 ModMesh를 모두 삭제한다.
											curParamSet._meshData.RemoveAll(delegate (apModifiedMesh a)
											{
												return a._isMeshTransform
												&& a._transform_Mesh != null
												&& a._transform_Mesh == curMeshTF;//<대상 변경>
											});
										}
									}
								}

								if (removeTimelineLayers.Count > 0)
								{
									//타임라인 레이어를 삭제하자
									apAnimTimelineLayer rmvTLL = null;
									for (int iTLL = 0; iTLL < removeTimelineLayers.Count; iTLL++)
									{
										rmvTLL = removeTimelineLayers[iTLL];
										if (rmvTLL._parentTimeline != null)
										{
											rmvTLL._parentTimeline._layers.Remove(rmvTLL);//이 레이어를 삭제하자
										}
									}
								}

								if (removePSGs.Count > 0)
								{
									//PSG를 삭제하자
									apModifierParamSetGroup rmvPSG = null;
									for (int iRemovePSG = 0; iRemovePSG < removePSGs.Count; iRemovePSG++)
									{
										rmvPSG = removePSGs[iRemovePSG];
										curModifier._paramSetGroup_controller.Remove(rmvPSG);
									}
								}
								removeTimelineLayers.Clear();
								removePSGs.Clear();
							}
						}


					}
				}
				
				

				//전체 Refresh
				Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(null));

				//Refresh 추가
				//Editor.Select.RefreshAnimEditing(true);
				Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15


				//현재 메시 그룹과 도착 메시 그룹이 모두 갱신이 되어야 한다.
				//- 현재 메시 그룹 : 해당 TF가 이동되어 없어졌기 때문에 Refresh
				//- 도착 메시 그룹 : 해당 TF가 도착해서 생성되었기 때문에 Refresh


				//<1> Root Mesh Group에 대해서 렌더 유닛 생성만 미리 요청한다.
				//이걸 호출해야 Sort가 문제없이 동작한다.
				//(두 메시 그룹은 부모 관계이므로 루트 메시 그룹은 같다.)
				Editor.Select.MeshGroup.ResetRenderUnitsChildAndRoot();//<<Child > Root 메시 그룹의 렌더 유닛을 모두 재검토하는 함수
				
				//<1> 현재 메시 그룹에 대한 Link/Refresh
				//현재 메시 그룹에 대해서 Link 요청
				apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(Editor.Select.MeshGroup);

				//메시 그룹 선택
				Editor.Select.MeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
				Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

				Editor.Select.MeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);//Depth를 할당한다. [1.4.2]

				//이전
				//Editor.Select.SelectMeshTF(targetMeshTransform, apSelection.MULTI_SELECT.Main);
				//if(isMultiple)
				//{
				//	//다중 선택이었다면, 추가로 다른 MeshTF들도 선택하자
				//	for (int i = 0; i < srcTargetMeshTFs.Count; i++)
				//	{
				//		curMeshTF = srcTargetMeshTFs[i];
				//		if(curMeshTF == targetMeshTransform)
				//		{
				//			continue;
				//		}

				//		Editor.Select.SelectMeshTF(curMeshTF, apSelection.MULTI_SELECT.AddOrSubtract);
				//	}
				//}
				
				//변경 v1.4.2 : 이미 이전되어서 사라졌는데 선택하는게 말이 될리가..
				Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);


				//<2> 도착 메시 그룹도 렌더 유닛 관련 Refresh를 미리 하자 (추가 v1.4.2)
				if(dstMeshGroup != Editor.Select.MeshGroup)
				{
					apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(dstMeshGroup);
					dstMeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
				}

				

				//추가 21.1.32 : Rule 가시성 동기화 초기화
				ResetVisibilityPresetSync();

				Editor.OnAnyObjectAddedOrRemoved(true);//소속이 변경되었지만, 내부에서는 객체가 사라지거나 추가된 효과라서 이 함수를 호출해야함

				////프리팹이었으면 Apply
				//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

				Editor.ResetHierarchyAll();
				Editor.RefreshControllerAndHierarchy(true);

				if(!isMultiple)
				{
					//한개만 이주되었다면
					Editor.Notification(targetMeshTransform._nickName + " is Migrated", true, false);
				}
				else
				{
					//여러개가 이주되었다면
					Editor.Notification(srcTargetMeshTFs.Count + " Sub Meshes are Migrated", true, false);
				}
				

				apEditorUtil.ReleaseGUIFocus();


				return true;

			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : Migrating Mesh Transform is failed.\n(Exception : " + ex + ")");
			}

			return false;


		}

		/// <summary>입력된 메시 그룹의 모든 부모 메시 그룹을 리스트에 넣는다. 대상 메시 그룹은 포함되지 않는다.</summary>
		public void FindAllParentMeshGroups(apMeshGroup srcMeshGroup, List<apMeshGroup> result)
		{
			if(srcMeshGroup == null
				|| srcMeshGroup._parentMeshGroup == null)
			{
				return;
			}

			apMeshGroup curParentMeshGroup = srcMeshGroup._parentMeshGroup;
			while(true)
			{
				//위로 하나씩 올라가면서 리스트에 넣는다.
				result.Add(curParentMeshGroup);

				curParentMeshGroup = curParentMeshGroup._parentMeshGroup;
				if(curParentMeshGroup == null)
				{
					break;
				}
			}
		}

		/// <summary>
		/// 입력된 메시 그룹의 자식 메시 그룹을 리스트에 넣는다. 대상 메시 그룹은 포함하지 않는다.
		/// </summary>
		/// <param name="srcMeshGroup"></param>
		/// <param name="result"></param>
		public void FindAllChildMeshGroups(apMeshGroup srcMeshGroup, List<apMeshGroup> result)
		{
			if(srcMeshGroup == null
				|| srcMeshGroup._childMeshGroupTransforms == null
				|| srcMeshGroup._childMeshGroupTransforms.Count == 0)
			{
				return;
			}

			for (int iChild = 0; iChild < srcMeshGroup._childMeshGroupTransforms.Count; iChild++)
			{
				apTransform_MeshGroup childMeshGroupTransform = srcMeshGroup._childMeshGroupTransforms[iChild];
				if(childMeshGroupTransform == null
					|| childMeshGroupTransform._meshGroup == null)
				{
					continue;
				}
				apMeshGroup childMeshGroup = childMeshGroupTransform._meshGroup;
				if(childMeshGroup == srcMeshGroup
					|| result.Contains(childMeshGroup))
				{
					continue;
				}

				result.Add(childMeshGroup);

				FindAllChildMeshGroups(childMeshGroup, result);
			}
		}

		/// <summary>
		/// 메시 그룹 관계상 가장 부모인 Root Mesh Group을 리턴한다.
		/// 자기 자신이 리턴될 수도 있다.
		/// </summary>
		/// <param name="srcMeshGroup"></param>
		/// <returns></returns>
		public apMeshGroup FindRootMeshGroup(apMeshGroup srcMeshGroup)
		{
			if(srcMeshGroup == null)
			{
				return null;
			}
			if(srcMeshGroup._parentMeshGroup == null)
			{
				//이게 루트 메시그룹이다.
				return srcMeshGroup;
			}
			//하나씩 위로 올라가지;
			apMeshGroup curMeshGroup = srcMeshGroup;
			while(true)
			{
				if(curMeshGroup._parentMeshGroup == null
					|| curMeshGroup._parentMeshGroup == srcMeshGroup)
				{
					//루트 메시 그룹을 찾았다. (루프 발생도 체크)
					return curMeshGroup;
				}

				//한칸 위로 올라가자
				curMeshGroup = curMeshGroup._parentMeshGroup;
			}
		}



		/// <summary>
		/// MeshTransform을 MeshGroup으로부터 분리
		/// </summary>
		/// <param name="targetMeshTransform"></param>
		/// <param name="parentMeshGroup"></param>
		public void DetachMeshTransform(apTransform_Mesh targetMeshTransform, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			//추가 : 이 Transform이 Child에 속하는 것인지, 아니면 Recursive에 속하는 것인지 확인해야한다.
			//Recursive인 경우 해당 MeshGroup을 찾아야 한다.
			apMeshGroup parentMeshGroupOfTransform = parentMeshGroup;
			if (parentMeshGroup.GetMeshTransform(targetMeshTransform._transformUniqueID) == null)
			{
				//Debug.LogError("<Detach : 해당 MeshTransform이 MeshGroup에 존재하지 않는다.");

				//Recursive에 존재하는지 확인
				parentMeshGroupOfTransform = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(targetMeshTransform, null);

				//못찾은 경우
				if (parentMeshGroupOfTransform == null)
				{
					Debug.LogError("AnyPortrait : Detach Failed. No Parent MeshGroup.");
					return;
				}
			}

			//Undo - Detach
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_DetachMesh, Editor, parentMeshGroup, targetMeshTransform, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Detach");

			int removedUniqueID = targetMeshTransform._transformUniqueID;
			//Editor._portrait.PushUniqueID_Transform(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);



			//parentMeshGroup._childMeshTransforms.Remove(targetMeshTransform);
			parentMeshGroupOfTransform._childMeshTransforms.Remove(targetMeshTransform);//<<삭제는 이쪽에서 

			//parentMeshGroup.ResetRenderUnits();
			//parentMeshGroup.RefreshModifierLink();
			//parentMeshGroup.SortRenderUnits(true);

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					parentMeshGroup,
			//					null,
			//					null,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			Editor.Hierarchy.SetNeedReset();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//추가 / 삭제시 요청한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//메시 그룹을 대상으로 Link
			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(parentMeshGroup);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);

			parentMeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
			parentMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			parentMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);//[1.4.2]

			parentMeshGroup.RefreshForce();

			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();
		}



		public void DetachMeshGroupTransform(apTransform_MeshGroup targetMeshGroupTransform, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			apMeshGroup parentMeshGroupOfTransform = parentMeshGroup;
			if (parentMeshGroup.GetMeshGroupTransform(targetMeshGroupTransform._transformUniqueID) == null)
			{
				//Debug.LogError("<Detach : 해당 MeshGroupTransform이 MeshGroup에 존재하지 않는다.");
				
				//Recursive에 존재하는지 확인
				parentMeshGroupOfTransform = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(null, targetMeshGroupTransform);

				//못찾은 경우
				if (parentMeshGroupOfTransform == null)
				{
					//Debug.LogError("<Parent MeshGroup>를 찾을 수 없다.");
					Debug.LogError("AnyPortrait : Detach Failed. No Parent MeshGroup.");
					return;
				}

				//자식의 자식 메시 그룹 Transform을 Detach하려고 했다. > 계속 진행
				//if (parentMeshGroupOfTeransform != parentMeshGroup)
				//{
				//	Debug.LogError("Recursive한 MeshTransform이다. Parent MeshGroup이 다르다.");
				//}
			}
			//Undo - Detach
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_DetachMeshGroup, 
			//												Editor, parentMeshGroup, targetMeshGroupTransform, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Detach");

			if (targetMeshGroupTransform._meshGroup != null)
			{
				targetMeshGroupTransform._meshGroup._parentMeshGroup = null;
				targetMeshGroupTransform._meshGroup._parentMeshGroupID = -1;

				//targetMeshGroupTransform._meshGroup.SortRenderUnits(true);
			}

			int removedUniqueID = targetMeshGroupTransform._transformUniqueID;
			//Editor._portrait.PushUniqueID_Transform(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);

			//parentMeshGroup._childMeshGroupTransforms.Remove(targetMeshGroupTransform);
			parentMeshGroupOfTransform._childMeshGroupTransforms.Remove(targetMeshGroupTransform);//<<변경

			//parentMeshGroup.ResetRenderUnits();
			//parentMeshGroup.RefreshModifierLink();
			//parentMeshGroup.SortRenderUnits(true);

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					parentMeshGroup,
			//					null,
			//					null,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			
			Editor.Hierarchy.SetNeedReset();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//추가 / 삭제시 요청한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//메시 그룹을 대상으로 Link
			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(parentMeshGroup);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);

			parentMeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
			parentMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			parentMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);//1.4.2
			if (parentMeshGroup._parentMeshGroup != null)
			{
				parentMeshGroup._parentMeshGroup.RefreshModifierLink(null);
			}

			parentMeshGroup.RefreshForce();

			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();
		}



		// 다중 선택 삭제

		/// <summary>
		/// MeshTransform을 MeshGroup으로부터 분리 (다중 선택)
		/// </summary>
		/// <param name="targetMeshTransform"></param>
		/// <param name="parentMeshGroup"></param>
		public void DetachMeshTransforms(List<apTransform_Mesh> targetMeshTransforms, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			if(targetMeshTransforms == null || targetMeshTransforms.Count == 0)
			{
				return;
			}

			
			//Undo - Detach
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Detach");

			//추가 : 이 Transform이 Child에 속하는 것인지, 아니면 Recursive에 속하는 것인지 확인해야한다.
			//Recursive인 경우 해당 MeshGroup을 찾아야 한다.
			apMeshGroup parentMeshGroupOfTeransform = null;
			apTransform_Mesh curMeshTF = null;
			for (int iMeshTF = 0; iMeshTF < targetMeshTransforms.Count; iMeshTF++)
			{
				curMeshTF = targetMeshTransforms[iMeshTF];

				parentMeshGroupOfTeransform = parentMeshGroup;//부모 메시 그룹은 일단 입력값을 기준으로 찾자

				if(curMeshTF == null)
				{
					continue;
				}

				if (parentMeshGroup.GetMeshTransform(curMeshTF._transformUniqueID) == null)
				{
					//Recursive에 존재하는지 확인
					parentMeshGroupOfTeransform = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(curMeshTF, null);

					//못찾은 경우
					if (parentMeshGroupOfTeransform == null)
					{
						Debug.LogError("AnyPortrait : Detach Failed. No Parent MeshGroup.");
						//return;
						
						continue;
					}
				}

				
				//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_DetachMesh, Editor, parentMeshGroup, targetMeshTransform, false);
				

				int removedUniqueID = curMeshTF._transformUniqueID;
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);

				parentMeshGroupOfTeransform._childMeshTransforms.Remove(curMeshTF);//<<삭제는 이쪽에서 
			}

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					parentMeshGroup,
			//					null,
			//					null,
			//					null,
			//					true);			

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			Editor.Hierarchy.SetNeedReset();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//추가 / 삭제시 요청한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//메시 그룹을 대상으로 Link
			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(parentMeshGroup);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);

			parentMeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
			parentMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			parentMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);//1.4.2

			parentMeshGroup.RefreshForce();

			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();
		}


		/// <summary>
		/// 서브 메시 그룹들을 제거한다.
		/// </summary>
		/// <param name="targetMeshGroupTransform"></param>
		/// <param name="parentMeshGroup"></param>
		public void DetachMeshGroupTransforms(List<apTransform_MeshGroup> targetMeshGroupTransforms, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			if(targetMeshGroupTransforms == null || targetMeshGroupTransforms.Count == 0)
			{
				return;
			}

			apMeshGroup parentMeshGroupOfTeransform = parentMeshGroup;
			apTransform_MeshGroup curMeshGroupTF = null;

			//Undo - Detach
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Detach");

			for (int iMeshGroupTF = 0; iMeshGroupTF < targetMeshGroupTransforms.Count; iMeshGroupTF++)
			{
				curMeshGroupTF = targetMeshGroupTransforms[iMeshGroupTF];
				if (curMeshGroupTF == null)
				{
					continue;
				}

				parentMeshGroupOfTeransform = parentMeshGroup;//입력값을 시작으로 부모 메시 그룹을 찾자

				if(curMeshGroupTF._meshGroup == parentMeshGroup)
				{
					//자기 자신이므로 패스
					continue;
				}

				if (parentMeshGroup.GetMeshGroupTransform(curMeshGroupTF._transformUniqueID) == null)
				{
					//Recursive에 존재하는지 확인
					parentMeshGroupOfTeransform = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(null, curMeshGroupTF);

					//못찾은 경우
					if (parentMeshGroupOfTeransform == null)
					{
						//Debug.LogError("<Parent MeshGroup>를 찾을 수 없다.");
						//return;

						continue;
					}
					//if (parentMeshGroupOfTeransform != parentMeshGroup)
					//{
					//	Debug.LogError("Recursive한 MeshTransform이다. Parent MeshGroup이 다르다.");
					//}
				}
				

				if (curMeshGroupTF._meshGroup != null)
				{
					curMeshGroupTF._meshGroup._parentMeshGroup = null;
					curMeshGroupTF._meshGroup._parentMeshGroupID = -1;
				}

				int removedUniqueID = curMeshGroupTF._transformUniqueID;
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);

				parentMeshGroupOfTeransform._childMeshGroupTransforms.Remove(curMeshGroupTF);
			}
			

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					parentMeshGroup,
			//					null,
			//					null,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);




			Editor.Hierarchy.SetNeedReset();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//추가 / 삭제시 요청한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//메시 그룹을 대상으로 Link
			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(parentMeshGroup);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);

			parentMeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
			parentMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			parentMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);//1.4.2
			if (parentMeshGroup._parentMeshGroup != null)
			{
				parentMeshGroup._parentMeshGroup.RefreshModifierLink(null);
			}

			parentMeshGroup.RefreshForce();

			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();
		}



		/// <summary>
		/// Mesh/MeshGroup Transform들을 한번에 분리한다.
		/// </summary>
		/// <param name="targetMeshTFs"></param>
		/// <param name="targetMeshGroupTFs"></param>
		/// <param name="parentMeshGroup"></param>
		public void DetachMultipleTransforms(List<apTransform_Mesh> targetMeshTFs,
											List<apTransform_MeshGroup> targetMeshGroupTFs,
											apMeshGroup parentMeshGroup)
		{
			//Detach Mesh Trasnfroms와 Detach MeshGroup Transforms를 동시에 수행한다.
			//대상 중에 재귀적인 부모 MeshGroupTF가 Detach 대상이 있다면 해당 객체는 Detach에서 제외된다. (부모가 대신 Detach되므로)
			//제거 대상을 모두 고르자
			int nInputMeshTFs = targetMeshTFs != null ? targetMeshTFs.Count : 0;
			int nInputMeshGroupTFs = targetMeshGroupTFs != null ? targetMeshGroupTFs.Count : 0;

			//실제로 Detach되는 TF와 그걸 실제로 보유한 Parent MeshGroup
			Dictionary<apTransform_Mesh, apMeshGroup> detachMeshTF2ParentMG = new Dictionary<apTransform_Mesh, apMeshGroup>();
			Dictionary<apTransform_MeshGroup, apMeshGroup> detachMeshGroupTF2ParentMG = new Dictionary<apTransform_MeshGroup, apMeshGroup>();

			if(nInputMeshTFs > 0)
			{
				apTransform_Mesh srcMeshTF = null;
				apMeshGroup srcParentMeshGroup = null;
				for (int i = 0; i < nInputMeshTFs; i++)
				{
					srcMeshTF = targetMeshTFs[i];
					if(srcMeshTF == null) { continue; }

					//이미 포함되었다면 스킵
					if(detachMeshTF2ParentMG.ContainsKey(srcMeshTF))
					{
						continue;
					}

					//부모 메시 그룹을 찾자
					if (parentMeshGroup.GetMeshTransform(srcMeshTF._transformUniqueID) == null)
					{
						//직속 TF은 아니므로 자식의 자식일 가능성이 높다.
						
						//Recursive에 존재하는지 확인
						srcParentMeshGroup = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(srcMeshTF, null);
					}
					else
					{
						//입력된 MeshGroup의 직속 TF다.
						srcParentMeshGroup = parentMeshGroup;
					}

					//못찾은 경우는 일단 제외
					if (srcParentMeshGroup == null)
					{
						continue;
					}

					//부모 MeshGroup이 제거 대상에 포함되어 있다면, 이건 제거 대상에서 제외한다.
					if(srcParentMeshGroup != parentMeshGroup && nInputMeshGroupTFs > 0)
					{
						bool isParentDetachable = targetMeshGroupTFs.Exists(delegate(apTransform_MeshGroup a)
						{
							return a._meshGroup == srcParentMeshGroup;
						});

						if(isParentDetachable)
						{
							//부모가 삭제 대상이라면 제외
							continue;
						}
					}

					//삭제 대상에 넣자
					detachMeshTF2ParentMG.Add(srcMeshTF, srcParentMeshGroup);
				}
			}

			//MeshGroupTF도 동일하게 처리
			if(nInputMeshGroupTFs > 0)
			{
				apTransform_MeshGroup srcMeshGroupTF = null;
				apMeshGroup srcParentMeshGroup = null;
				for (int i = 0; i < nInputMeshGroupTFs; i++)
				{
					srcMeshGroupTF = targetMeshGroupTFs[i];
					if(srcMeshGroupTF == null) { continue; }

					if(srcMeshGroupTF._meshGroup == null 
						|| srcMeshGroupTF._meshGroup == parentMeshGroup)
					{
						continue;
					}

					//이미 포함되었다면 스킵
					if(detachMeshGroupTF2ParentMG.ContainsKey(srcMeshGroupTF))
					{
						continue;
					}

					//부모 메시 그룹을 찾자
					if (parentMeshGroup.GetMeshGroupTransform(srcMeshGroupTF._transformUniqueID) == null)
					{
						//직속 TF은 아니므로 자식의 자식일 가능성이 높다.
						
						//Recursive에 존재하는지 확인
						srcParentMeshGroup = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(null, srcMeshGroupTF);
					}
					else
					{
						//입력된 MeshGroup의 직속 TF다.
						srcParentMeshGroup = parentMeshGroup;
					}

					//못찾은 경우는 일단 제외
					if (srcParentMeshGroup == null)
					{
						continue;
					}

					//부모 MeshGroup이 제거 대상에 포함되어 있다면, 이건 제거 대상에서 제외한다.
					if(srcParentMeshGroup != parentMeshGroup && nInputMeshGroupTFs > 0)
					{
						bool isParentDetachable = targetMeshGroupTFs.Exists(delegate(apTransform_MeshGroup a)
						{
							return a._meshGroup == srcParentMeshGroup 
							&& a != srcMeshGroupTF;//자기 자신은 제외
						});

						if(isParentDetachable)
						{
							//부모가 삭제 대상이라면 제외
							continue;
						}
					}

					//삭제 대상에 넣자
					detachMeshGroupTF2ParentMG.Add(srcMeshGroupTF, srcParentMeshGroup);
				}
			}

			//이제 하나씩 삭제하자

			//Undo 등록
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Detach Objects");


			int nDetachMeshTFs = detachMeshTF2ParentMG.Count;
			int nDetachMeshGroupTFs = detachMeshGroupTF2ParentMG.Count;
				
			//MeshTF부터 시작
			if(nDetachMeshTFs > 0)
			{
				apTransform_Mesh curMeshTF = null;
				apMeshGroup curParentMG = null;
				foreach (KeyValuePair<apTransform_Mesh, apMeshGroup> meshTFPair in detachMeshTF2ParentMG)
				{
					curMeshTF = meshTFPair.Key;
					curParentMG = meshTFPair.Value;

					if(curMeshTF == null || curParentMG == null)
					{
						continue;
					}

					//삭제 처리
					Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, curMeshTF._transformUniqueID);
					if(curParentMG._childMeshTransforms != null)
					{
						curParentMG._childMeshTransforms.Remove(curMeshTF);
					}
				}
			}

			//MeshGroupTF도 삭제
			if(nDetachMeshGroupTFs > 0)
			{
				apTransform_MeshGroup curMeshGroupTF = null;
				apMeshGroup curParentMG = null;
				foreach (KeyValuePair<apTransform_MeshGroup, apMeshGroup> meshGroupTFPair in detachMeshGroupTF2ParentMG)
				{
					curMeshGroupTF = meshGroupTFPair.Key;
					curParentMG = meshGroupTFPair.Value;

					if(curMeshGroupTF == null || curParentMG == null)
					{
						continue;
					}

					//일단 Parent 관계를 끊는다.
					if(curMeshGroupTF._meshGroup != null)
					{
						curMeshGroupTF._meshGroup._parentMeshGroup = null;
						curMeshGroupTF._meshGroup._parentMeshGroupID = -1;
					}

					//삭제 처리
					Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, curMeshGroupTF._transformUniqueID);
					if(curParentMG._childMeshGroupTransforms != null)
					{
						curParentMG._childMeshGroupTransforms.Remove(curMeshGroupTF);
					}
				}
			}


			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			Editor.Hierarchy.SetNeedReset();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//추가 / 삭제시 요청한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//메시 그룹을 대상으로 Link
			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(parentMeshGroup);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh);

			parentMeshGroup.ResetRenderUnits(apUtil.LinkRefresh);
			parentMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			parentMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);//1.4.2


			if (parentMeshGroup._parentMeshGroup != null)
			{
				parentMeshGroup._parentMeshGroup.RefreshModifierLink(null);
			}

			parentMeshGroup.RefreshForce();


			//삭제된 다른 메시 그룹들도 Depth를 생신해주자
			foreach (KeyValuePair<apTransform_MeshGroup, apMeshGroup> meshGroupTFPair in detachMeshGroupTF2ParentMG)
			{
				apMeshGroup detachedMeshGroup = meshGroupTFPair.Value;
				if(detachedMeshGroup != null)
				{
					detachedMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.AssignDepth);
				}
			}


			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}











		public void RefreshMeshGroups()
		{
			List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
			for (int i = 0; i < meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = meshGroups[i];

				List<apRenderUnit> removableRenderUnits = new List<apRenderUnit>();

				if (meshGroup._rootRenderUnit == null)
				{
					continue;
				}
				CheckRemovableRenderUnit(meshGroup, meshGroup._rootRenderUnit, removableRenderUnits);

				if (removableRenderUnits.Count > 0)
				{
					meshGroup._renderUnits_All.RemoveAll(delegate (apRenderUnit a)
					{
						return removableRenderUnits.Contains(a);
					});

					meshGroup.SetDirtyToReset();
					meshGroup.RefreshForce();
					meshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//1.4.2 : Depth 할당은 하지 않고 정렬만 한다.
				}

				//Bone Refresh도 여기서 하자
				RefreshBoneHierarchy(meshGroup);
				RefreshBoneChaining(meshGroup);
			}
		}




		private void CheckRemovableRenderUnit(apMeshGroup parentMeshGroup, apRenderUnit curRenderUnit, List<apRenderUnit> removableRenderUnits)
		{
			bool isRemovable = false;
			if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			{
				//메시가 존재하는가
				if (curRenderUnit._meshTransform == null)
				{
					isRemovable = true;
					//Debug.LogError("R 1");
				}
				else if (curRenderUnit._meshTransform._mesh == null)
				{
					isRemovable = true;
					//Debug.LogError("R 2");
				}
				else if (GetMesh(curRenderUnit._meshTransform._mesh._uniqueID) == null)
				{
					isRemovable = true;
					//Debug.LogError("R 3"); 
				}
			}
			else
			{
				//메시 그룹이 존재하는가?
				//if(curRenderUnit._meshGroupTransform == null)											{ isRemovable = true; Debug.LogError("R 4"); }
				if (curRenderUnit._meshGroup == null)
				{
					isRemovable = true;
					//Debug.LogError("R 5");
				}
				else if (GetMeshGroup(curRenderUnit._meshGroup._uniqueID) == null)
				{
					isRemovable = true;
					//Debug.LogError("R 6 - " + curRenderUnit._meshGroup._name);
				}
			}

			if (isRemovable)
			{
				//이후 모든 Child는 다 Remove한다.
				AddChildRenderUnitsToRemove(parentMeshGroup, curRenderUnit, removableRenderUnits);
			}
			else
			{
				for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
				{
					CheckRemovableRenderUnit(parentMeshGroup, curRenderUnit._childRenderUnits[i], removableRenderUnits);
				}
			}
		}

		private void AddChildRenderUnitsToRemove(apMeshGroup parentMeshGroup, apRenderUnit curRenderUnit, List<apRenderUnit> removableRenderUnits)
		{
			if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode)
			{
				if (curRenderUnit._meshGroup != null)
				{
					if (curRenderUnit._meshGroup._parentMeshGroup == parentMeshGroup)
					{
						//Debug.LogError("Removable Unit : " + curRenderUnit._meshGroup._name + " In " + parentMeshGroup._name);
						curRenderUnit._meshGroup._parentMeshGroup = null;
						curRenderUnit._meshGroup._parentMeshGroupID = -1;
					}
				}
			}
			removableRenderUnits.Add(curRenderUnit);

			for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
			{
				AddChildRenderUnitsToRemove(parentMeshGroup, curRenderUnit._childRenderUnits[i], removableRenderUnits);
			}
		}

		/// <summary>
		/// Mesh의 Vertex가 바뀌면 이 함수를 호출한다.
		/// 모든 Render Unit들의 Vertex Buffer를 다시 리셋하게 만든다.
		/// </summary>
		public void ResetAllRenderUnitsVertexIndex()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			for (int iMG = 0; iMG < Editor._portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[iMG];
				for (int iRU = 0; iRU < meshGroup._renderUnits_All.Count; iRU++)
				{
					apRenderUnit renderUnit = meshGroup._renderUnits_All[iRU];
					renderUnit.ResetVertexIndex();
				}
			}

			//통계 재계산 요청
			Editor.Select.SetStatisticsRefresh();
		}



		//추가 21.3.9 : 메시 그룹의 이름을 바꾼다.
		//단순한 것 같지만, 다른 메시 그룹의 자식으로 등록된 경우 MeshGroupTF의 이름도 같이 바꿔야한다.
		public void RenameMeshGroup(apMeshGroup targetMeshGroup, string newName)
		{
			if(Editor == null || Editor._portrait == null || targetMeshGroup == null)
			{
				return;
			}

			//다른 메시 그룹에 속해있다면 이름을 동기화한다. (단, 물어보고)
			if (targetMeshGroup._parentMeshGroup != null && targetMeshGroup._parentMeshGroup != targetMeshGroup)
			{
				bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.DLG_RenameSyncSubMeshGroupObject_Title),
															Editor.GetText(TEXT.DLG_RenameSyncSubMeshGroupObject_Body),
															Editor.GetText(TEXT.Okay),
															Editor.GetText(TEXT.Cancel)
															);
				if (result)
				{
					//부모 메시 그룹이 있다면 > 전체 저장
					apEditorUtil.SetRecord_PortraitAllMeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	Editor._portrait, 
																	//targetMeshGroup, 
																	false,
																	apEditorUtil.UNDO_STRUCT.StructChanged);
					targetMeshGroup._name = newName;

					//Sub MeshGroup을 찾자
					apMeshGroup parentMeshGroup = targetMeshGroup._parentMeshGroup;
					apTransform_MeshGroup targetMeshGroupTF = parentMeshGroup.FindChildMeshGroupTransform(targetMeshGroup);
					if (targetMeshGroupTF != null)
					{
						//연결된 MeshGroup Transform의 이름도 바꾸자
						targetMeshGroupTF._nickName = newName;
					}
				}
				else
				{
					//그냥 이것만 적용하자
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
														Editor, 
														targetMeshGroup, 
														//null, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					targetMeshGroup._name = newName;
				}
			}
			else
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
													Editor, 
													targetMeshGroup, 
													//null, 
													false, false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
				targetMeshGroup._name = newName;
			}
		}








		//추가 20.1.6 : MeshTransform / MeshGroupTransform 복제하기
		//동일한 MeshGroup 내에서 복제할지, 아니면 다른 MeshGroup으로 복제할지 결정할 수 있다. (이 경우 전환 테이블 필요)

		/// <summary>
		/// 추가 20.1.6 : 메시 트랜스폼을 복제한다.
		/// srcMeshGroup와 dstMeshGroup가 같다면 동일한 메시 그룹 내에서 복제한다.
		/// 동일한 메시 그룹이 아니라면, srcMeshGroup2NewMeshGroup와 srcMT2NewMT를 넣어야 한다.
		/// </summary>
		public apTransform_Mesh DuplicateMeshTransform(apTransform_Mesh srcMeshTransform,
														apMeshGroup srcMeshGroup,
														apMeshGroup dstMeshGroup,
														bool isRecordUndo,
														bool isRefresh
														)
		{
			if (Editor == null
				|| srcMeshTransform == null
				|| srcMeshGroup == null
				|| dstMeshGroup == null)
			{
				return null;
			}

			bool isSameMeshGroup = (srcMeshGroup == dstMeshGroup);

			if (isRecordUndo)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DuplicateMeshTransform, 
													Editor, 
													dstMeshGroup, 
													//null, 
													false, true,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}

			//ID 발급
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0)
			{
				Debug.LogError("AnyPortrait : Duplicating Mesh Transform is failed.");
				return null;
			}

			//메시 트랜스폼 생성
			apTransform_Mesh newMeshTransform = new apTransform_Mesh(nextID);

			//리스트에 넣자
			dstMeshGroup._childMeshTransforms.Add(newMeshTransform);


			//1. 이름
			string newNickName = srcMeshTransform._nickName;

			//만약 동일한 메시 그룹으로 복제하는 것이라면
			//이름이 동일하지 않게 만들자
			if (isSameMeshGroup)
			{
				newNickName += " Copy";
			}

			newMeshTransform._nickName = newNickName;

			//기본 정보 모두 복사
			newMeshTransform._meshUniqueID = srcMeshTransform._meshUniqueID;
			newMeshTransform._mesh = srcMeshTransform._mesh;
			newMeshTransform._matrix.SetMatrix(srcMeshTransform._matrix, true);
			newMeshTransform._meshColor2X_Default = srcMeshTransform._meshColor2X_Default;
			newMeshTransform._isVisible_Default = srcMeshTransform._isVisible_Default;

			//Depth는 상황마다 다름
			if (isSameMeshGroup)
			{
				//동일한 MeshGroup에서 복사시 > 마지막 Depth로 이동
				int maxDepth = srcMeshGroup.GetLastDepth();
				newMeshTransform._depth = maxDepth + 1;
				
			}
			else
			{
				//다른 MeshGroup으로 복사시 > Depth값 그대로 복사
				newMeshTransform._depth = srcMeshTransform._depth;
			}
			newMeshTransform._shaderType = srcMeshTransform._shaderType;
			newMeshTransform._isCustomShader = srcMeshTransform._isCustomShader;
			newMeshTransform._customShader = srcMeshTransform._customShader;
			newMeshTransform._renderTexSize = srcMeshTransform._renderTexSize;
			newMeshTransform._isSocket = srcMeshTransform._isSocket;

			//클리핑 마스크도 상황마다 다름
			if (isSameMeshGroup)
			{
				//동일한 메시 그룹 > 클리핑 마스크 해제
				newMeshTransform._isClipping_Parent = false;
				newMeshTransform._isClipping_Child = false;
			}
			else
			{
				//다른 메시 그룹 > 클리핑 마스크 유지
				newMeshTransform._isClipping_Parent = srcMeshTransform._isClipping_Parent;
				newMeshTransform._isClipping_Child = srcMeshTransform._isClipping_Child;
			}
			
			//클리핑 마스크 체크
			newMeshTransform._isAlways2Side = srcMeshTransform._isAlways2Side;
			newMeshTransform._isUsePortraitShadowOption = srcMeshTransform._isUsePortraitShadowOption;
			newMeshTransform._shadowCastingMode = srcMeshTransform._shadowCastingMode;
			newMeshTransform._receiveShadow = srcMeshTransform._receiveShadow;

			//Material Set 설정 복사
			newMeshTransform._materialSetID = srcMeshTransform._materialSetID;
			newMeshTransform._linkedMaterialSet = srcMeshTransform._linkedMaterialSet;
			newMeshTransform._isUseDefaultMaterialSet = srcMeshTransform._isUseDefaultMaterialSet;
			int nCustomMatProp = srcMeshTransform._customMaterialProperties == null ? 0 : srcMeshTransform._customMaterialProperties.Count;
			if (nCustomMatProp > 0)
			{
				if (newMeshTransform._customMaterialProperties == null)
				{
					newMeshTransform._customMaterialProperties = new List<apTransform_Mesh.CustomMaterialProperty>();
				}
				newMeshTransform._customMaterialProperties.Clear();

				apTransform_Mesh.CustomMaterialProperty srcProp = null;
				apTransform_Mesh.CustomMaterialProperty newProp = null;
				for (int iProp = 0; iProp < nCustomMatProp; iProp++)
				{
					srcProp = srcMeshTransform._customMaterialProperties[iProp];
					if (srcProp == null) { continue; }

					newProp = new apTransform_Mesh.CustomMaterialProperty();
					newProp.CopyFromSrc(srcProp);

					newMeshTransform._customMaterialProperties.Add(newProp);
				}
			}



			if (isRefresh)
			{
				dstMeshGroup.SetDirtyToReset();
				dstMeshGroup.RefreshForce();

				//추가 / 삭제시 요청한다.
				Editor.OnAnyObjectAddedOrRemoved();

				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			return newMeshTransform;

		}



		/// <summary>
		/// 추가 20.1.6 : 메시그룹 트랜스폼을 복제한다.
		/// srcMeshGroup와 dstMeshGroup가 같다면 동일한 메시 그룹 내에서 복제한다.
		/// 동일한 메시 그룹이 아니라면, srcMeshGroup2NewMeshGroup와 srcMT2NewMT를 넣어야 한다.
		/// srcMeshGroup2NewMeshGroup이 null이라면 srcMeshGroupTransform와 동일한 메시 그룹을 적용받는다.
		/// null이 아니라면 "변환된" MeshGroup을 찾아서 적용한다.
		/// </summary>
		public apTransform_MeshGroup DuplicateMeshGroupTransform(apTransform_MeshGroup srcMeshGroupTransform,
																	apMeshGroup srcMeshGroup,
																	apMeshGroup dstMeshGroup,
																	bool isRecordUndo,
																	bool isRefresh,
																	Dictionary<apMeshGroup, apMeshGroup> srcMeshGroup2NewMeshGroup)
		{
			if (Editor == null
				|| srcMeshGroupTransform == null
				|| srcMeshGroup == null
				|| dstMeshGroup == null)
			{
				return null;
			}

			bool isSameMeshGroup = (srcMeshGroup == dstMeshGroup);

			if (isRecordUndo)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DuplicateMeshGroupTransform, 
													Editor, 
													dstMeshGroup, 
													//null, 
													false, true,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}

			//ID 발급
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0)
			{
				Debug.LogError("AnyPortrait : Duplicating MeshGroup Transform is failed.");
				return null;
			}

			//메시 트랜스폼 생성
			apTransform_MeshGroup newMeshGroupTransform = new apTransform_MeshGroup(nextID);



			//리스트에 넣자
			dstMeshGroup._childMeshGroupTransforms.Add(newMeshGroupTransform);

			//1. 이름
			string newNickName = srcMeshGroupTransform._nickName;

			//만약 동일한 메시 그룹으로 복제하는 것이라면
			//이름이 동일하지 않게 만들자
			if (isSameMeshGroup)
			{
				//이름 + " (n+1)"
				newNickName += " Copy";
			}

			newMeshGroupTransform._nickName = newNickName;

			//기본 정보 모두 복사
			//Parent MeshGroup을 찾자
			//(MeshGroupTransform이 속한 메시 그룹이 아니라, 원본 데이터의 메시 그룹을 의미한다)
			apMeshGroup srcLinkedMeshGroup = srcMeshGroupTransform._meshGroup;
			apMeshGroup newLinkedMeshGroup = null;
			if (srcMeshGroup2NewMeshGroup != null && srcLinkedMeshGroup != null)
			{
				if (srcMeshGroup2NewMeshGroup.ContainsKey(srcLinkedMeshGroup))
				{
					newLinkedMeshGroup = srcMeshGroup2NewMeshGroup[srcLinkedMeshGroup];
				}
			}
			if (newLinkedMeshGroup == null)
			{
				//변환 정보가 없다.
				//그대로 적용
				newMeshGroupTransform._meshGroupUniqueID = srcMeshGroupTransform._meshGroupUniqueID;
				newMeshGroupTransform._meshGroup = srcMeshGroupTransform._meshGroup;
			}
			else
			{
				//변환 정보가 있다.
				//복제된 메시 그룹을 링크
				newMeshGroupTransform._meshGroupUniqueID = newLinkedMeshGroup._uniqueID;
				newMeshGroupTransform._meshGroup = newLinkedMeshGroup;
			}

			//나머지는 값 그대로 복사
			newMeshGroupTransform._matrix.SetMatrix(srcMeshGroupTransform._matrix, true);
			newMeshGroupTransform._meshColor2X_Default = srcMeshGroupTransform._meshColor2X_Default;
			newMeshGroupTransform._isVisible_Default = srcMeshGroupTransform._isVisible_Default;

			//Depth는 상황마다 다름
			if (isSameMeshGroup)
			{
				//동일한 MeshGroup에서 복사시 > 마지막 Depth로 이동
				int maxDepth = srcMeshGroup.GetLastDepth();
				newMeshGroupTransform._depth = maxDepth + 1;
			}
			else
			{
				//다른 MeshGroup으로 복사시 > Depth값 그대로 복사
				newMeshGroupTransform._depth = srcMeshGroupTransform._depth;
			}
			newMeshGroupTransform._isSocket = srcMeshGroupTransform._isSocket;

			if (isRefresh)
			{
				dstMeshGroup.SetDirtyToReset();
				dstMeshGroup.RefreshForce();

				//추가 / 삭제시 요청한다.
				Editor.OnAnyObjectAddedOrRemoved();

				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			return newMeshGroupTransform;

		}



		//------------------------------------------------------------------------------------------
		// 본
		//------------------------------------------------------------------------------------------

		/// <summary>
		/// 본을 생성하여 TargetMeshGroup에 추가한다.
		/// 만약 루트 본이 아닌 경우 : ParentBone에 값을 넣어주면 자동으로 Child에 포함된다.
		/// null을 넣으면 루트 본으로 설정되어 MeshGroup에서 따로 관리하도록 한다.
		/// 그외 설정은 리턴값을 받아서 처리하자
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="parentBone"></param>
		/// <returns></returns>
		public apBone AddBone(apMeshGroup targetMeshGroup, apBone parentBone, bool isRecordUndo = true)
		{
			if (Editor._portrait == null || targetMeshGroup == null)
			{
				return null;
			}
			if (isRecordUndo)
			{
				//Undo
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AddBone, 
													Editor, 
													targetMeshGroup, 
													//null, 
													false, false,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}

			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Bone);
			int meshGroupID = targetMeshGroup._uniqueID;

			if (nextID < 0 || meshGroupID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Adding Bone is Failed. Please Retry", "Close");

				EditorUtility.DisplayDialog(Editor.GetText(TEXT.BoneAddFailed_Title),
												Editor.GetText(TEXT.BoneAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}



			//이름을 지어주자
			string parentName = "Bone ";
			if (parentBone != null)
			{
				parentName = parentBone._name;
			}
			apEditorUtil.NameAndIndexPair nameIndexPair = apEditorUtil.ParseNumericName(parentName);

			int curIndex = nameIndexPair._index;
			//이제 index를 올려가면서 겹치는게 있는지 확인한다.

			curIndex++;
			string resultName = "";
			int nCnt = 0;
			while (true)
			{
				resultName = nameIndexPair.MakeNewName(curIndex);

				bool isAnySameName = false;
				if (parentBone == null)
				{
					for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
					{
						if (string.Equals(targetMeshGroup._boneList_Root[i]._name, resultName))
						{
							//같은 이름이 있다.
							isAnySameName = true;
							break;
						}
					}
				}
				else
				{
					for (int i = 0; i < parentBone._childBones.Count; i++)
					{
						if (string.Equals(parentBone._childBones[i]._name, resultName))
						{
							//같은 이름이 있다.
							isAnySameName = true;
							break;
						}
					}
				}
				if (isAnySameName)
				{
					//똑같은 이름이 있네염..
					curIndex++;

					nCnt++;
					if (nCnt > 100)
					{
						Debug.Log("Error");
						break;
					}
				}
				else
				{
					//다른 이름 찾음
					break;
				}
			}

			#region [미사용 코드]
			//string baseName = "Bone ";
			//int nameNumber = 0;

			//string resultName = baseName + nameNumber;
			//if(parentBone == null)
			//{
			//	//동일한 이름이 Root에 있는지 체크하자
			//	resultName = baseName + nameNumber;

			//	if (targetMeshGroup._boneList_Root.Count > 0)
			//	{
			//		while (true)
			//		{
			//			bool isAnySameName = false;
			//			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			//			{
			//				if(string.Equals(targetMeshGroup._boneList_Root[i]._name, resultName))
			//				{
			//					//같은 이름이 있다.
			//					isAnySameName = true;
			//					break;
			//				}
			//			}
			//			if(isAnySameName)
			//			{
			//				//똑같은 이름이 있네염..
			//				nameNumber++;
			//				resultName = baseName + nameNumber;
			//			}
			//			else
			//			{
			//				//다른 이름 찾음
			//				break;
			//			}
			//		}
			//	}

			//}
			//else
			//{
			//	baseName = parentBone._name + "";
			//	resultName = baseName + nameNumber;

			//	//동일한 이름이 Child에 있는지 체크하자
			//	if(parentBone._childBones.Count > 0)
			//	{
			//		while (true)
			//		{
			//			bool isAnySameName = false;
			//			for (int i = 0; i < parentBone._childBones.Count; i++)
			//			{
			//				if(string.Equals(parentBone._childBones[i]._name, resultName))
			//				{
			//					//같은 이름이 있다.
			//					isAnySameName = true;
			//					break;
			//				}
			//			}

			//			if(isAnySameName)
			//			{
			//				//똑같은 이름이 있네염..
			//				nameNumber++;
			//				resultName = baseName + nameNumber;
			//			}
			//			else
			//			{
			//				//다른 이름 찾음
			//				break;
			//			}
			//		}

			//	}
			//} 
			#endregion

			apBone newBone = new apBone(nextID, meshGroupID, resultName);

			if (parentBone != null)
			{
				//변경 19.8.13 : 옵션에 따라서
				//- 색이 일정하게 되거나
				//- 색이 아예 비슷하지 않게 되도록 만든다.

				Color boneGUIColor = Color.black;

				//if (Editor._rigOption_NewChildBoneColorIsLikeParent)//이전
				if (Editor._boneGUIOption_NewBoneColor == apEditor.NEW_BONE_COLOR.SimilarColor)//변경
				{
					//비슷한 색상
					boneGUIColor = apEditorUtil.GetSimilarColor(parentBone._color, 0.7f, 1.0f, 0.6f, 1.0f, true);
				}
				else
				{
					//완전히 다른 색상
					if (parentBone._parentBone != null)
					{
						//부모의 부모가 있는 경우
						boneGUIColor = apEditorUtil.GetDiffierentColor(parentBone._color, parentBone._parentBone._color, 0.8f, 1.0f, 0.9f, 1.0f);
					}
					else
					{
						boneGUIColor = apEditorUtil.GetDiffierentColor(parentBone._color, 0.8f, 1.0f, 0.9f, 1.0f);
					}


				}

				boneGUIColor.a = 1.0f;

				newBone._color = boneGUIColor;
			}
			else
			{
				//랜덤한 색상으로 정하자. (19.8.13)
				//기존과 달리 채도가 높고 밝은 색상으로 고른다.
				Color boneGUIColor = apEditorUtil.GetRandomColor(0.8f, 1.0f, 0.9f, 1.0f);
				boneGUIColor.a = 1.0f;
				newBone._color = boneGUIColor;
			}

			//버그 수정 10.2 : IK 설정 초기화가 안되어있었다.
			newBone._optionIK = apBone.OPTION_IK.Disabled;
			newBone._IKTargetBone = null;
			newBone._IKNextChainedBone = null;
			newBone._IKTargetBoneID = -1;
			newBone._IKNextChainedBoneID = -1;

			//ParentBone을 포함해서 Link를 한다.
			//ParentBone이 있다면 이 Bone이 ChildList로 자동으로 추가된다.
			newBone.Link(targetMeshGroup, parentBone, Editor._portrait);
			newBone.InitTransform(Editor._portrait);

			targetMeshGroup._boneList_All.Add(newBone);//<<새로운 Bone을 추가하자

			if (newBone._parentBone == null)
			{
				//Root Bone이라면
				targetMeshGroup._boneList_Root.Add(newBone);//Root List에도 추가한다.
			}

			//newBone.Link(targetMeshGroup, parentBone);//<<이걸 두번할 필요가 없는데..

			//targetMeshGroup.RefreshBoneGUIVisible();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newBone;
		}


		public void RemoveAllBones(apMeshGroup targetMeshGroup)
		{
			if (targetMeshGroup == null)
			{
				return;
			}
			//Undo
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_RemoveAllBones, Editor, targetMeshGroup, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove All Bones");

			//일단 ID 반납
			int nBones = targetMeshGroup._boneList_All.Count;
			for (int i = 0; i < nBones; i++)
			{
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, targetMeshGroup._boneList_All[i]._uniqueID);
			}

			targetMeshGroup._boneList_All.Clear();
			targetMeshGroup._boneList_Root.Clear();

			Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(targetMeshGroup));

			targetMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//v1.4.2 : 본만 삭제된 것이므로, Depth 할당은 하지 않는다.
			targetMeshGroup.RefreshForce();

			Editor.RefreshControllerAndHierarchy(false);

			Editor.Notification("All Bones of [" + targetMeshGroup._name + "] are removed", true, false);

			//targetMeshGroup.RefreshBoneGUIVisible();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();
		}


		public void RemoveBone(apBone bone, bool isRemoveChildren)
		{
			if (bone == null)
			{
				return;
			}

			//Debug.Log("Remove Bone : " + bone._name + " / Remove Children : " + isRemoveChildren);

			apMeshGroup meshGroup = bone._meshGroup;
			apBone parentBone = bone._parentBone;

			

			List<string> removedNames = new List<string>();

			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_RemoveBone, Editor, bone._meshGroup, bone, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Bone");

			if (!isRemoveChildren)
			{
				//< Children을 삭제하지 않을때 >
				//1. Parent에서 Bone을 삭제한다.
				//2. Bone의 Child를 Parent (또는 Null)에 연결한다.

				//Parent - [삭제할 Bone] - Child에서
				//Child를 삭제하지 않는다면
				//Parent <- Child로 연결한다.

				//3.연결할 때, 각각의 Child의 Matrix를 갱신한다.

				//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
				//5. Refresh

				removedNames.Add(bone._name);

				//1)
				if (parentBone != null)
				{
					parentBone._childBones.Remove(bone);
					parentBone._childBoneIDs.Remove(bone._uniqueID);
				}

				//2, 3)
				for (int i = 0; i < bone._childBones.Count; i++)
				{
					apBone childBone = bone._childBones[i];

					//아래에 래핑된 코드로 변경
					//apMatrix nextParent_Matrix = null;
					//apComplexMatrix nextParent_Matrix = null;//변경 20.8.13 : ComplexMatrix로 변경

					if (parentBone != null)
					{
						childBone._parentBone = parentBone;
						childBone._parentBoneID = parentBone._uniqueID;

						if (!parentBone._childBones.Contains(childBone))
						{
							parentBone._childBones.Add(childBone);
						}
						if (!parentBone._childBoneIDs.Contains(childBone._uniqueID))
						{
							parentBone._childBoneIDs.Add(childBone._uniqueID);
						}

						//nextParent_Matrix = parentBone._worldMatrix;//아래 래핑된 코드로 변경
					}
					else
					{
						childBone._parentBone = null;
						childBone._parentBoneID = -1;

						//아래 래핑된 코드로 변경
						//if (bone._renderUnit != null)
						//{
						//	//nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;//기존

						//	//변경 20.8.13
						//	nextParent_Matrix = apComplexMatrix.TempMatrix_1;
						//	nextParent_Matrix.SetMatrix_Step2(bone._renderUnit.WorldMatrixWrap, true);
						//}
						//else
						//{
						//	//nextParent_Matrix = new apMatrix();
						//	nextParent_Matrix = apComplexMatrix.TempMatrix_1;
						//}
					}


					apBoneWorldMatrix nextParent_Matrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
											0, Editor._portrait,
											(parentBone != null ? parentBone._worldMatrix : null),
											(bone._renderUnit != null ? bone._renderUnit.WorldMatrixWrap : null)
											);

					//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
					//apMatrix localBoneMatrix = apMatrix.RMultiply(childBone._defaultMatrix, childBone._localMatrix);

					//현재의 worldMatrix

					//기존
					//apMatrix worldBoneMatrix = childBone._worldMatrix;
					
					//변경 20.8.13 : ComplexMatrix로 변경 + Temp를 만든다. > BoneWorldMatrix로 변경
					apBoneWorldMatrix worldBoneMatrix = childBone._worldMatrix;
					worldBoneMatrix.MakeMatrix(true);



					//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
					//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

					// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
					// Default

					//--------------------------------
					// 기존 방식
					//--------------------------------
					//apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix, nextParent_Matrix), childBone._localMatrix);
					//newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
					//childBone._defaultMatrix.SetMatrix(newDefaultMatrix);

					//--------------------------------
					// 변경된 방식 (20.8.13) ComplexMatrix 이용 + Temp
					//--------------------------------
					//apComplexMatrix newDefaultMatrix = apComplexMatrix.TempMatrix_2;
					//newDefaultMatrix.CopyFromComplexMatrix(worldBoneMatrix);
					//newDefaultMatrix.Inverse(nextParent_Matrix);
					//newDefaultMatrix.RInverse(childBone._localMatrix);
					//newDefaultMatrix._angleDeg_Step1 = apUtil.AngleTo180(newDefaultMatrix._angleDeg_Step1);

					//childBone._defaultMatrix.SetTRS(	newDefaultMatrix._pos_Step1, 
					//									newDefaultMatrix._angleDeg_Step1, 
					//									newDefaultMatrix._scale_Step1);

					//--------------------------------
					// 다시 변경된 방식 (20.8.18) 래핑된 BoneWorldMatrix 이용
					//--------------------------------
					apBoneWorldMatrix newDefaultMatrix = apBoneWorldMatrix.MakeDefaultMatrixFromWorld(
															1, Editor._portrait,
															worldBoneMatrix, 
															nextParent_Matrix, 
															childBone._localMatrix);

					childBone._defaultMatrix.SetTRS(	newDefaultMatrix.Pos,
														apUtil.AngleTo180(newDefaultMatrix.Angle),
														newDefaultMatrix.Scale,
														true);

				}


				//IK Option은 바꾸지 않는다.

				//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
				//혹시 모를 연동을 위해 에러를 발생하도록 하자
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, bone._uniqueID);

				bone._parentBone = null;
				bone._parentBoneID = -1;
				bone._meshGroup = null;
				bone._meshGroupID = -1;
				bone._childBones.Clear();
				bone._childBoneIDs.Clear();

				meshGroup._boneList_All.Remove(bone);
				meshGroup._boneList_Root.Remove(bone);

			}
			else
			{
				//< 모든 Children을 삭제한다. >
				//Parent에서 bone 연결 끊고 삭제하면 되므로 간단.
				if (parentBone != null)
				{
					parentBone._childBones.Remove(bone);
					parentBone._childBoneIDs.Remove(bone._uniqueID);
				}

				//재귀적으로 삭제를 해주자
				RemoveBoneWithChildrenRecursive(bone, meshGroup, removedNames);
			}

			Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(meshGroup));//전체 리셋

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);

			if (meshGroup != null)
			{
				meshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);
				meshGroup.RefreshForce();
			}

			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.Hierarchy_AnimClip.ResetSubUnits();
			Editor.RefreshControllerAndHierarchy(false);

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();

			if (removedNames.Count == 1)
			{
				Editor.Notification("[" + removedNames[0] + "] is removed", true, false);
			}
			else
			{
				string strRemoved = "";
				int nNames = removedNames.Count;
				if (nNames > 3)
				{
					nNames = 3;
				}
				for (int i = 0; i < nNames; i++)
				{
					if (i != 0)
					{
						strRemoved += ", ";
					}
					strRemoved += removedNames[i];

				}
				if (removedNames.Count > 3)
				{
					strRemoved += "...";
				}

				Editor.Notification("[" + strRemoved + "] are removed", true, false);
			}
		}

		private void RemoveBoneWithChildrenRecursive(apBone bone, apMeshGroup meshGroup, List<string> removedNames)
		{
			int nChildBones = bone._childBones != null ? bone._childBones.Count : 0;
			if(nChildBones > 0)
			{
				for (int i = 0; i < nChildBones; i++)
				{
					RemoveBoneWithChildrenRecursive(bone._childBones[i], meshGroup, removedNames);
				}
			}
			

			Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, bone._uniqueID);

			meshGroup._boneList_All.Remove(bone);
			meshGroup._boneList_Root.Remove(bone);

			bone._parentBone = null;
			bone._parentBoneID = -1;
			bone._meshGroup = null;
			bone._meshGroupID = -1;
			bone._childBones.Clear();
			bone._childBoneIDs.Clear();

			removedNames.Add(bone._name);
		}



		public void RemoveBones(List<apBone> bones, apMeshGroup parentMeshGroup, bool isRemoveChildren)
		{
			int nInputBones = bones != null ? bones.Count : 0;

			if (nInputBones == 0)
			{
				return;
			}

			

			apBone curBone = null;
			List<string> removedNames = new List<string>();

			//실제로 삭제될 본들은 따로 리스트를 만든다.
			//자식 본을 삭제하는 옵션의 경우, 부모가 삭제되는 경우엔 일단 기본 루프에서는 제외해야하기 때문.
			List<apBone> targetBones = new List<apBone>();

			apBone inputBone = null;
			for (int i = 0; i < nInputBones; i++)
			{
				inputBone = bones[i];
				if(isRemoveChildren)
				{
					//자식들도 삭제하는 옵션이 있다면
					//부모 중에 하나라도 삭제 대상이라면 기본 루프에서는 제외해야한다.

					bool isParentRemovable = false;
					if(inputBone._parentBone != null)
					{
						apBone curParentBone = inputBone._parentBone;
						while(true)
						{
							if(curParentBone == null
								|| curParentBone == inputBone)
							{
								break;
							}

							if(bones.Contains(curParentBone))
							{
								//부모 본이 삭제 대상이다.
								isParentRemovable = true;
								break;
							}

							//하나 위로 이동
							curParentBone = curParentBone._parentBone;
						}
					}

					if(!isParentRemovable)
					{
						//부모가 삭제 대상에 포함되지 않는 경우에만 루프에 포함시킨다.
						targetBones.Add(inputBone);
					}
				}
				else
				{
					//선택된 걸 삭제한다면 그냥 입력
					targetBones.Add(inputBone);
				}
			}


			int nTargetBones = targetBones.Count;
			if(nTargetBones == 0)
			{
				//삭제할 게 없다.
				return;
			}


			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Bones");


			for (int iBone = 0; iBone < nTargetBones; iBone++)
			{
				//curBone = bones[iBone];
				curBone = targetBones[iBone];

				apMeshGroup meshGroup = curBone._meshGroup;
				apBone parentBone = curBone._parentBone;



				if (!isRemoveChildren)
				{
					//< Children을 삭제하지 않을때 >
					//1. Parent에서 Bone을 삭제한다.
					//2. Bone의 Child를 Parent (또는 Null)에 연결한다.

					//Parent - [삭제할 Bone] - Child에서
					//Child를 삭제하지 않는다면
					//Parent <- Child로 연결한다.

					//3.연결할 때, 각각의 Child의 Matrix를 갱신한다.

					//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
					//5. Refresh

					removedNames.Add(curBone._name);

					//1)
					if (parentBone != null)
					{
						parentBone._childBones.Remove(curBone);
						parentBone._childBoneIDs.Remove(curBone._uniqueID);
					}

					//2, 3)
					int nChildBones = curBone._childBones != null ? curBone._childBones.Count : 0;
					if (nChildBones > 0)
					{
						for (int iChild = 0; iChild < nChildBones; iChild++)
						{
							apBone childBone = curBone._childBones[iChild];

							//아래에 래핑된 코드로 변경
							//apMatrix nextParent_Matrix = null;
							//apComplexMatrix nextParent_Matrix = null;//변경 20.8.13 : ComplexMatrix로 변경

							if (parentBone != null)
							{
								childBone._parentBone = parentBone;
								childBone._parentBoneID = parentBone._uniqueID;

								if (!parentBone._childBones.Contains(childBone))
								{
									parentBone._childBones.Add(childBone);
								}
								if (!parentBone._childBoneIDs.Contains(childBone._uniqueID))
								{
									parentBone._childBoneIDs.Add(childBone._uniqueID);
								}
							}
							else
							{
								childBone._parentBone = null;
								childBone._parentBoneID = -1;
							}


							apBoneWorldMatrix nextParent_Matrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
													0, Editor._portrait,
													(parentBone != null ? parentBone._worldMatrix : null),
													(curBone._renderUnit != null ? curBone._renderUnit.WorldMatrixWrap : null)
													);

							//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
							//apMatrix localBoneMatrix = apMatrix.RMultiply(childBone._defaultMatrix, childBone._localMatrix);

							apBoneWorldMatrix worldBoneMatrix = childBone._worldMatrix;
							worldBoneMatrix.MakeMatrix(true);

							//--------------------------------
							// 다시 변경된 방식 (20.8.18) 래핑된 BoneWorldMatrix 이용
							//--------------------------------
							apBoneWorldMatrix newDefaultMatrix = apBoneWorldMatrix.MakeDefaultMatrixFromWorld(
																	1, Editor._portrait,
																	worldBoneMatrix,
																	nextParent_Matrix,
																	childBone._localMatrix);

							childBone._defaultMatrix.SetTRS(newDefaultMatrix.Pos,
																apUtil.AngleTo180(newDefaultMatrix.Angle),
																newDefaultMatrix.Scale,
																true);

						}
					}
					


					//IK Option은 바꾸지 않는다.

					//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
					//혹시 모를 연동을 위해 에러를 발생하도록 하자
					Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, curBone._uniqueID);

					curBone._parentBone = null;
					curBone._parentBoneID = -1;
					curBone._childBones.Clear();
					curBone._childBoneIDs.Clear();

					meshGroup._boneList_All.Remove(curBone);
					meshGroup._boneList_Root.Remove(curBone);

					//MeshGroup 연결을 끊는다.
					curBone._meshGroup = null;
					curBone._meshGroupID = -1;

				}
				else
				{
					//< 모든 Children을 삭제한다. >
					//Parent에서 bone 연결 끊고 삭제하면 되므로 간단.
					if (parentBone != null)
					{
						parentBone._childBones.Remove(curBone);
						parentBone._childBoneIDs.Remove(curBone._uniqueID);
					}

					//재귀적으로 삭제를 해주자
					RemoveBoneWithChildrenRecursive(curBone, meshGroup, removedNames);
				}
			}
			

			Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(parentMeshGroup));//전체 리셋

			RefreshBoneHierarchy(parentMeshGroup);
			RefreshBoneChaining(parentMeshGroup);

			if (parentMeshGroup != null)
			{
				parentMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);
				parentMeshGroup.RefreshForce();
			}

			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.Hierarchy_AnimClip.ResetSubUnits();
			Editor.RefreshControllerAndHierarchy(false);

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();


			if (removedNames.Count == 1)
			{
				Editor.Notification("[" + removedNames[0] + "] is removed", true, false);
			}
			else
			{
				string strRemoved = "";
				int nNames = removedNames.Count;
				if (nNames > 3)
				{
					nNames = 3;
				}
				for (int i = 0; i < nNames; i++)
				{
					if (i != 0)
					{
						strRemoved += ", ";
					}
					strRemoved += removedNames[i];

				}
				if (removedNames.Count > 3)
				{
					strRemoved += "...";
				}

				Editor.Notification("[" + strRemoved + "] are removed", true, false);
			}
		}











		public void AttachBoneToChild(apBone bone, apBone attachedBone)
		{
			if (bone == null || attachedBone == null)
			{
				return;
			}


			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AttachBoneToChild, 
												Editor, 
												bone._meshGroup, 
												//bone, 
												false, false,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			if (bone.GetParentRecursive(attachedBone._uniqueID) != null)
			{
				//Parent가 오면 Loop가 발생한다.
				return;
			}
			apMeshGroup targetMeshGroup = bone._meshGroup;


			//Child로 추가한다.
			//Child의 Parent를 연결한다.
			//기존의 Child의 Parent에서는 Child를 제외한다.
			//IK가 Disabled -> Single로 가능하면 Single로 만들어준다.
			//Child의 Default Matrix를 보정해준다.

			targetMeshGroup.RefreshForce();
			
			//기존
			//apMatrix worldBoneMatrix_Prev = new apMatrix(attachedBone._worldMatrix_NonModified);

			//변경 20.8.13 : ComplexMatrix로 변경 + Temp
			//apComplexMatrix worldBoneMatrix_Prev = apComplexMatrix.TempMatrix_1;
			//worldBoneMatrix_Prev.CopyFromComplexMatrix(attachedBone._worldMatrix_NonModified);

			//다시 변경 20.8.18 : 래핑된 BoneWorldMatrix로 변경 + Temp
			apBoneWorldMatrix worldBoneMatrix_Prev = apBoneWorldMatrix.GetTemp(0, Editor._portrait);
			worldBoneMatrix_Prev.CopyFromMatrix(attachedBone._worldMatrix_NonModified);



			if (!bone._childBones.Contains(attachedBone))
			{
				bone._childBones.Add(attachedBone);
			}
			if (!bone._childBoneIDs.Contains(attachedBone._uniqueID))
			{
				bone._childBoneIDs.Add(attachedBone._uniqueID);
			}

			attachedBone._parentBone = bone;
			attachedBone._parentBoneID = bone._uniqueID;

			apBone prevParentBoneOffAttachedBone = attachedBone._parentBone;
			if (prevParentBoneOffAttachedBone != null)
			{
				prevParentBoneOffAttachedBone._childBones.Remove(attachedBone);
				prevParentBoneOffAttachedBone._childBoneIDs.Remove(attachedBone._uniqueID);
			}

			if (bone._childBones.Count == 1 && bone._optionIK == apBone.OPTION_IK.Disabled)
			{
				bone._optionIK = apBone.OPTION_IK.IKSingle;
				bone._IKTargetBone = attachedBone;
				bone._IKTargetBoneID = attachedBone._uniqueID;

				bone._IKNextChainedBone = attachedBone;
				bone._IKNextChainedBoneID = attachedBone._uniqueID;
			}

			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);

			//Attached Bone의 Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			//apMatrix localBoneMatrix = apMatrix.RMultiply(attachedBone._defaultMatrix, attachedBone._localMatrix);

			//현재의 worldMatrix
			//apMatrix worldBoneMatrix = attachedBone._worldMatrix;


			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)


			//기존
			//apMatrix nextParent_Matrix = null;

			//변경 20.8.13 : Complex Matrix로 변경 + Temp2
			//apComplexMatrix nextParent_Matrix = apComplexMatrix.TempMatrix_2;
			//if (bone._renderUnit != null)
			//{
			//	//nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;//기존
			//	nextParent_Matrix.SetMatrix_Step2(bone._renderUnit.WorldMatrixWrap, true);
			//}
			//else
			//{
			//	//nextParent_Matrix = new apMatrix();//기존
			//}
			//nextParent_Matrix = bone._worldMatrix;

			//다시 변경 20.8.18
			apBoneWorldMatrix nextParent_Matrix = bone._worldMatrix;





			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default

			//--------------------------
			// 기존 방식
			//--------------------------
			//apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix_Prev, nextParent_Matrix), attachedBone._localMatrix);
			//newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
			//attachedBone._defaultMatrix.SetMatrix(newDefaultMatrix);//<<여기서 적용. 일단 빼보자


			//--------------------------
			// 변경된 방식 20.8.13 : ComplexMatrix + Temp3 이용
			//--------------------------
			//apComplexMatrix newDefaultMatrix = apComplexMatrix.TempMatrix_3;
			//newDefaultMatrix.CopyFromComplexMatrix(worldBoneMatrix_Prev);
			//newDefaultMatrix.Inverse(nextParent_Matrix);
			//newDefaultMatrix.RInverse(attachedBone._localMatrix);
			//newDefaultMatrix._angleDeg_Step1 = apUtil.AngleTo180(newDefaultMatrix._angleDeg_Step1);
			
			//attachedBone._defaultMatrix.SetTRS(newDefaultMatrix._pos_Step1, newDefaultMatrix._angleDeg_Step1, newDefaultMatrix._scale_Step1);

			//--------------------------
			// 다시 변경된 방식 20.8.18 : 래핑된 BoneWorldMatrix + Temp 이용
			//--------------------------
			apBoneWorldMatrix newDefaultMatrix = apBoneWorldMatrix.MakeDefaultMatrixFromWorld(
														1, Editor._portrait,
														worldBoneMatrix_Prev,
														nextParent_Matrix, 
														attachedBone._localMatrix);

			attachedBone._defaultMatrix.SetTRS(		newDefaultMatrix.Pos, 
													apUtil.AngleTo180(newDefaultMatrix.Angle), 
													newDefaultMatrix.Scale,
													true);




			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(targetMeshGroup));

			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);
			targetMeshGroup.UpdateBonesWorldMatrix();



			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = targetMeshGroup._boneListSets[iSet];

				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					boneSet._bones_Root[iRoot].MakeWorldMatrix(true);
					boneSet._bones_Root[iRoot].GUIUpdate(true);
				}
			}


			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy(false);

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			Editor.OnAnyObjectAddedOrRemoved();
			Editor.SetRepaint();
		}

		#region [미사용 코드] 정상적으로 동작하지 않는다. 대신 SetBoneAsParent에 parentBone을 null로 입력해서 사용하자
		//public void DetachBoneFromChild(apBone bone, apBone detachedBone)
		//{
		//	if (bone == null || detachedBone == null)
		//	{
		//		return;
		//	}
		//	//Undo
		//	apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DetachBoneFromChild, 
		//										Editor, 
		//										bone._meshGroup, 
		//										//bone, 
		//										false, false,
		//										apEditorUtil.UNDO_STRUCT.StructChanged);

		//	apMeshGroup targetMeshGroup = bone._meshGroup;

		//	//Child를 제거한다.
		//	//Child는 Parent가 없어졌으므로 Root가 된다.
		//	//Default Matrix 보존해줄 것
		//	targetMeshGroup.RefreshForce();


		//	//기존 방식
		//	//apMatrix worldBoneMatrix_Prev = new apMatrix(detachedBone._worldMatrix);//<<변경 : 값을 복사해야한다.

		//	//변경된 방식 20.8.13 : Complex Matrix + Temp1 이용
		//	//apComplexMatrix worldBoneMatrix_Prev = apComplexMatrix.TempMatrix_1;
		//	//worldBoneMatrix_Prev.CopyFromComplexMatrix(detachedBone._worldMatrix);

		//	//다시 변경된 방식 20.8.18 : BoneWorldMatrix
		//	//apBoneWorldMatrix worldBoneMatrix_Prev = detachedBone._worldMatrix;
		//	apBoneWorldMatrix worldBoneMatrix_Prev = apBoneWorldMatrix.MakeTempWorldMatrix(0, Editor._portrait, detachedBone._worldMatrix_NonModified);



		//	bone._childBones.Remove(detachedBone);
		//	bone._childBoneIDs.Remove(detachedBone._uniqueID);

		//	detachedBone._parentBone = null;
		//	detachedBone._parentBoneID = -1;

		//	//DetachedBone의 Parent가 없으므로 IK는 해제되고 Root에 들어가야 한다.
		//	if (!targetMeshGroup._boneList_Root.Contains(detachedBone))
		//	{
		//		targetMeshGroup._boneList_Root.Add(detachedBone);
		//	}




		//	targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
		//	targetMeshGroup.RefreshForce(true);

		//	//Detached Bone의 Default Matrix를 갱신하자

		//	//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
		//	//apMatrix localBoneMatrix = apMatrix.RMultiply(detachedBone._defaultMatrix, detachedBone._localMatrix);

		//	//현재의 worldMatrix


		//	//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
		//	//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

		//	//--------------------------------
		//	// 기존 방식
		//	//--------------------------------
		//	//apMatrix nextParent_Matrix = null;
		//	//if (bone._renderUnit != null)
		//	//{
		//	//	nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
		//	//}
		//	//else
		//	//{
		//	//	nextParent_Matrix = new apMatrix();
		//	//}

		//	//--------------------------------
		//	// 변경된 방식 (20.8.13) : ComplexMatrix + Temp2
		//	//--------------------------------
		//	//apComplexMatrix nextParent_Matrix = apComplexMatrix.TempMatrix_2;
		//	//if (bone._renderUnit != null)
		//	//{
		//	//	nextParent_Matrix.SetMatrix_Step2(bone._renderUnit.WorldMatrixWrap, true);
		//	//}

		//	//--------------------------------
		//	// 다시 변경된 방식 (20.8.13) : ComplexMatrix + Temp2
		//	//--------------------------------
		//	//RenderUnit이 있는 경우에만 할당
		//	//부모 본은 없어진 상태
		//	apBoneWorldMatrix nextParent_Matrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
		//											0, Editor._portrait,
		//											null, 
		//											((bone._renderUnit != null) ? bone._renderUnit.WorldMatrixWrap : null)
		//											);


		//	// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
		//	// Default
		//	//--------------------------------
		//	// 기존 방식
		//	//--------------------------------
		//	//apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix_Prev, nextParent_Matrix), detachedBone._localMatrix);
		//	//newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
		//	//detachedBone._defaultMatrix.SetMatrix(newDefaultMatrix);

		//	//--------------------------------
		//	// 변경된 방식 (20.8.13) : ComplexMatrix + Temp3
		//	//--------------------------------
		//	//apComplexMatrix newDefaultMatrix = apComplexMatrix.TempMatrix_3;
		//	//newDefaultMatrix.CopyFromComplexMatrix(worldBoneMatrix_Prev);
		//	//newDefaultMatrix.Inverse(nextParent_Matrix);
		//	//newDefaultMatrix.RInverse(detachedBone._localMatrix);
		//	//newDefaultMatrix._angleDeg_Step1 = apUtil.AngleTo180(newDefaultMatrix._angleDeg_Step1);
		//	//detachedBone._defaultMatrix.SetTRS(newDefaultMatrix._pos_Step1, newDefaultMatrix._angleDeg_Step1, newDefaultMatrix._scale_Step1);


		//	//--------------------------
		//	// 다시 변경된 방식 20.8.18 : 래핑된 BoneWorldMatrix + Temp 이용
		//	//--------------------------
		//	apBoneWorldMatrix newDefaultMatrix = apBoneWorldMatrix.MakeDefaultMatrixFromWorld(
		//												1, Editor._portrait,
		//												worldBoneMatrix_Prev,
		//												nextParent_Matrix, 
		//												detachedBone._localMatrix);

		//	detachedBone._defaultMatrix.SetTRS(		newDefaultMatrix.Pos, 
		//											apUtil.AngleTo180(newDefaultMatrix.Angle), 
		//											newDefaultMatrix.Scale,
		//											true);


		//	targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();

		//	//targetMeshGroup.RefreshForce();
		//	Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(targetMeshGroup));
		//	targetMeshGroup.RefreshForce();

		//	targetMeshGroup.UpdateBonesWorldMatrix();

		//	apMeshGroup.BoneListSet boneSet = null;
		//	for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
		//	{
		//		boneSet = targetMeshGroup._boneListSets[iSet];

		//		for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
		//		{
		//			boneSet._bones_Root[iRoot].MakeWorldMatrix(true);
		//			boneSet._bones_Root[iRoot].GUIUpdate(true);
		//		}
		//	}

		//	RefreshBoneHierarchy(targetMeshGroup);
		//	RefreshBoneChaining(targetMeshGroup);


		//	Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
		//	Editor.RefreshControllerAndHierarchy(false);
		//} 
		#endregion


		public void SetBoneAsParent(apBone bone, apBone parentBone)
		{
			if (bone == null)
			{
				return;
			}

			if (parentBone == bone._parentBone)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_SetBoneAsParent, 
												Editor, 
												bone._meshGroup, 
												//bone, 
												false, false,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			//Parent Bone은 Null이 될 수 있다. (Root가 되는 경우)
			apMeshGroup targetMeshGroup = bone._meshGroup;

			targetMeshGroup.RefreshForce();

			//이전
			//apMatrix worldBoneMatrix_Prev = new apMatrix(bone._worldMatrix_NonModified);

			//변경 20.8.19 : 래핑된 코드
			apBoneWorldMatrix worldBoneMatrix_Prev = apBoneWorldMatrix.MakeTempWorldMatrix(0, Editor._portrait, bone._worldMatrix_NonModified);

			//1. 기존의 Parent에서 지금 Bone을 Child에서 뺀다.
			//2. 새로운 Parent에서 지금 Bone을 추가한다.
			//3. 새로운 Parent를 지금 Bone의 Parent로 등록한다.
			//4. Refresh
			//중요 > WorldMatrix를 보존해야한다.
			//apMatrix prevParent_Matrix = null;//<<기존
			//apMatrix nextParent_Matrix = null;


			apBone prevParent = bone._parentBone;
			if (prevParent != null)
			{
				prevParent._childBones.Remove(bone);
				prevParent._childBoneIDs.Remove(bone._uniqueID);

				//prevParent_Matrix = prevParent._worldMatrix;
			}
			//else
			//{
			//	if (bone._renderUnit != null)
			//	{
			//		prevParent_Matrix = bone._renderUnit.WorldMatrixWrap;
			//	}
			//	else
			//	{
			//		prevParent_Matrix = new apMatrix();
			//	}
			//}

			//변경 20.8.19 : 래핑된 코드 > 이게 사용되지 않나..
			//apBoneWorldMatrix prevParent_Matrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
			//											0, Editor._portrait,
			//											(prevParent != null ? prevParent._worldMatrix : null),
			//											(bone._renderUnit != null ? bone._renderUnit.WorldMatrixWrap : null));


			if (parentBone != null)
			{
				parentBone._childBones.Add(bone);
				parentBone._childBoneIDs.Add(bone._uniqueID);

				bone._parentBone = parentBone;
				bone._parentBoneID = parentBone._uniqueID;

				if (parentBone._optionIK == apBone.OPTION_IK.Disabled && parentBone._childBones.Count == 1)
				{
					//처음 들어간거라면 자동으로 IK를 설정해주자.
					parentBone._optionIK = apBone.OPTION_IK.IKSingle;
					parentBone._IKTargetBone = parentBone._childBones[0];
					parentBone._IKTargetBoneID = parentBone._childBones[0]._uniqueID;

					parentBone._IKNextChainedBone = parentBone._childBones[0];
					parentBone._IKNextChainedBoneID = parentBone._childBones[0]._uniqueID;
				}

				//nextParent_Matrix = parentBone._worldMatrix;
			}
			else
			{
				bone._parentBone = null;
				bone._parentBoneID = -1;

				//if (bone._renderUnit != null)
				//{
				//	nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
				//}
				//else
				//{
				//	nextParent_Matrix = new apMatrix();
				//}
			}


			//변경 20.8.19 : 래핑된 코드
			apBoneWorldMatrix nextParent_Matrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
														1, Editor._portrait,
														(parentBone != null ? parentBone._worldMatrix : null),
														(bone._renderUnit != null ? bone._renderUnit.WorldMatrixWrap : null));


			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);

			//Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			//apMatrix localBoneMatrix = apMatrix.RMultiply(bone._defaultMatrix, bone._localMatrix);

			//현재의 worldMatrix
			//apMatrix worldBoneMatrix = bone._worldMatrix;

			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

			
			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default
			//이전
			//apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix_Prev, nextParent_Matrix), bone._localMatrix);
			//newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
			//bone._defaultMatrix.SetMatrix(newDefaultMatrix);

			//변경 20.8.19 : 래핑된 코드 (+180도 각도 제한은 아래의 함수에 포함된다.)
			apBoneWorldMatrix newDefaultMatrix = apBoneWorldMatrix.MakeDefaultMatrixFromWorld(
																	2, Editor._portrait,
																	worldBoneMatrix_Prev, 
																	nextParent_Matrix, 
																	bone._localMatrix);
				
			bone._defaultMatrix.SetTRS(		newDefaultMatrix.Pos,
											newDefaultMatrix.Angle,
											newDefaultMatrix.Scale,
											true);



			//bone._meshGroup.RefreshForce();
			//Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(targetMeshGroup));
			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();

			targetMeshGroup.RefreshForce(true);
			targetMeshGroup.UpdateBonesWorldMatrix();

			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = targetMeshGroup._boneListSets[iSet];

				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					boneSet._bones_Root[iRoot].MakeWorldMatrix(true);
					boneSet._bones_Root[iRoot].GUIUpdate(true);
				}
			}


			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy(false);

			Editor.OnAnyObjectAddedOrRemoved();
			Editor.SetRepaint();
		}


		public void SetBoneAsIKTarget(apBone bone, apBone IKTargetBone)
		{
			if (bone == null || IKTargetBone == null)
			{
				return;
			}

			if (bone.GetChildBoneRecursive(IKTargetBone._uniqueID) == null)
			{
				return;
			}

			apBone nextChainedBone = bone.FindNextChainedBone(IKTargetBone._uniqueID);
			if (nextChainedBone == null)
			{
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_SetBoneAsIKTarget, 
												Editor, 
												bone._meshGroup, 
												//bone, 
												false, false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			//string prevIKTargetBoneName = "<None>";
			//if (bone._IKTargetBone != null)
			//{
			//	prevIKTargetBoneName = bone._IKTargetBone._name;
			//}

			//Debug.Log("Set Bone As IK Target : [" + prevIKTargetBoneName + " >> " + IKTargetBone._name + "] (" + bone._name + ")");



			bone._IKTargetBone = IKTargetBone;
			bone._IKTargetBoneID = IKTargetBone._uniqueID;

			bone._IKNextChainedBone = nextChainedBone;
			bone._IKNextChainedBoneID = nextChainedBone._uniqueID;


			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);

			Editor.RefreshControllerAndHierarchy(false);

			//string curIKTargetBoneName = "<None>";
			//if (bone._IKTargetBone != null)
			//{
			//	curIKTargetBoneName = bone._IKTargetBone._name;
			//}

			//Debug.Log("Set Bone As IK Target : Refresh Finined [ Cur IK Target : " + curIKTargetBoneName + " / Request IK Target : " + IKTargetBone._name + "]");
		}

		public void SetBoneAsIKPositionControllerEffector(apBone bone, apBone effectorBone)
		{
			if (bone == null)
			{
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, 
												Editor, 
												bone._meshGroup, 
												//bone, 
												false, false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			if (bone._IKController == null)
			{
				bone._IKController = new apBoneIKController();
				bone._IKController.Link(bone, bone._meshGroup, Editor._portrait);
			}

			bone._IKController._controllerType = apBoneIKController.CONTROLLER_TYPE.Position;
			bone._IKController._effectorBone = effectorBone;
			if (effectorBone == null)
			{
				bone._IKController._effectorBoneID = -1;
			}
			else
			{
				bone._IKController._effectorBoneID = effectorBone._uniqueID;
			}

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);
		}

		public void SetBoneAsIKLookAtControllerEffectorOrStartBone(apBone bone, apBone effectorOrStartBone, bool isEffectBone)
		{
			if (bone == null)
			{
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, 
												Editor, 
												bone._meshGroup, 
												//bone, 
												false, false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			if (bone._IKController == null)
			{
				bone._IKController = new apBoneIKController();
				bone._IKController.Link(bone, bone._meshGroup, Editor._portrait);
			}

			bone._IKController._controllerType = apBoneIKController.CONTROLLER_TYPE.LookAt;
			if (isEffectBone)
			{
				bone._IKController._effectorBone = effectorOrStartBone;
				if (effectorOrStartBone == null)
				{
					bone._IKController._effectorBoneID = -1;
				}
				else
				{
					bone._IKController._effectorBoneID = effectorOrStartBone._uniqueID;
				}
			}
			else
			{
				//bone._IKController._startBone = effectorOrStartBone;
				//if (effectorOrStartBone == null)
				//{
				//	bone._IKController._startBoneID = -1;
				//}
				//else
				//{
				//	bone._IKController._startBoneID = effectorOrStartBone._uniqueID;
				//}
			}

			////SubChain을 Refresh한다.
			//bone._IKController.RefreshSubChainedBones();

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);
		}


		public void SetBoneAsMirror(apBone bone, apBone mirror)
		{
			if (bone == null)
			{
				return;
			}
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneMirrorChanged, 
												Editor, 
												bone._meshGroup, 
												//bone, 
												false, false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apBone prevMirrorBone = bone._mirrorBone;//기존의 Mirror Bone
			if (prevMirrorBone != null
				&& prevMirrorBone != mirror)
			{
				if (prevMirrorBone._mirrorBone == bone)
				{
					//Mirror가 바뀌었다면
					//기존 Mirror는 연결을 끊는다.
					prevMirrorBone._mirrorBone = null;
					prevMirrorBone._mirrorBoneID = -1;
				}
			}

			bone._mirrorBone = mirror;
			if (bone._mirrorBone != null)
			{
				bone._mirrorBoneID = bone._mirrorBone._uniqueID;
			}
			else
			{
				bone._mirrorBoneID = -1;
			}

			//Mirror도 서로 연결해준다.
			if (mirror != null)
			{
				mirror._mirrorBone = bone;
				mirror._mirrorBoneID = bone._uniqueID;
			}
		}


		/// <summary>
		/// 새로운 Mirror Bone을 만든다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="bone"></param>
		public void MakeNewMirrorBone(apMeshGroup meshGroup, apBone bone)
		{
			if (meshGroup == null || bone == null)
			{
				return;
			}

			//경고 문구
			//1. 이미 Mirror가 있는 경우
			//2. Child를 모두 Mirror할 것인지
			if (bone._mirrorBone != null)
			{
				bool isResult = EditorUtility.DisplayDialog(
					Editor.GetText(TEXT.DLG_MirrorBoneWarning_Title),//"Creating a Mirror Bone", 
					Editor.GetText(TEXT.DLG_MirrorBoneWarning_Body_Aleady),//"Mirror Bone already exists. Create a new Mirror Bone?",
					Editor.GetText(TEXT.Okay), Editor.GetText(TEXT.Cancel));
				if (!isResult)
				{
					return;
				}
			}

			int createWithChild = EditorUtility.DisplayDialogComplex(
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Title),//"Creating a mirror bone",
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Body_Children),//"Do you also want to create the mirror bones for the child Bones? If any of the child Bones have a Mirror Bone, they will be disconnected.",
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Btn1_WithAllChildBones),//"with all Child Bones", 
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Btn2_OnlySelectedBone),//"Only selected bone", 
				Editor.GetText(TEXT.Cancel));

			//Debug.Log("Create Mirror [" + createWithChild + "]");
			if (createWithChild == 2)
			{
				//Cancel
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AddBone, 
												Editor, 
												meshGroup, 
												//null, 
												false, false,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			bool isCreateWithChild = (createWithChild == 0);

			//첫 Bone을 제외하면 Recursive하게 해야한다.
			//첫 Bone은 Offset을 적용, 그 외에는 링크에 맞게 동일
			//처리는 Recursive하게 해야한다.
			Dictionary<apBone, apBone> src2Mirror = new Dictionary<apBone, apBone>();
			Dictionary<apBone, apBone> mirror2Src = new Dictionary<apBone, apBone>();

			MakeNewMirrorBoneHierarchyRecursive(meshGroup,
														bone, bone._parentBone,
														bone,
														isCreateWithChild,
														src2Mirror,
														mirror2Src);


			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(meshGroup));

			meshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			meshGroup.RefreshForce(true);
			meshGroup.UpdateBonesWorldMatrix();

			RefreshBoneHierarchy(meshGroup);

			//IK 연결을 갱신한다.
			if (bone._mirrorBone != null)
			{
				SetMirrorBonesLinks(bone._mirrorBone, src2Mirror, mirror2Src);
			}

			//RefreshBoneChainingUnit(meshGroup, bone);
			RefreshBoneChainingUnit(bone);//<<MeshGroup 빠짐
			if (bone._mirrorBone != null)
			{
				//RefreshBoneChainingUnit(meshGroup, bone._mirrorBone);
				RefreshBoneChainingUnit(bone._mirrorBone);//<<MeshGroup 빠짐
			}

			//[1.5.0] 메시 그룹의 IK 업데이트를 위한 IKRunNode를 갱신한다.
			meshGroup.RefreshBoneIKRunNodes();

			Editor.RefreshControllerAndHierarchy(false);
		}

		/// <summary>
		/// Mirror Bone을 만든다.
		/// 이 단계에서는 Mirror Bone 생성, 계층 연결, 기본 속성 복사만 한다.
		/// IK
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="srcBone"></param>
		/// <param name="mirrorParentBone"></param>
		/// <param name="rootSrcBone"></param>
		/// <param name="isRecursive"></param>
		private void MakeNewMirrorBoneHierarchyRecursive(apMeshGroup meshGroup,
														apBone srcBone, apBone mirrorParentBone,
														apBone rootSrcBone,
														bool isRecursive,
														Dictionary<apBone, apBone> src2Mirror,
														Dictionary<apBone, apBone> mirror2Src)
		{
			//새로운 Bone을 만들고
			apBone newBone = AddBone(meshGroup, mirrorParentBone, false);

			src2Mirror.Add(srcBone, newBone);//Pair에 추가
			mirror2Src.Add(newBone, srcBone);

			//1. 이름을 맞춘다.
			//" L " <-> " R "
			string boneName = srcBone._name;
			if (boneName.Contains(" L ")) { boneName = boneName.Replace(" L ", " R "); }
			else if (boneName.Contains(" R ")) { boneName = boneName.Replace(" R ", " L "); }
			else if (boneName.EndsWith(" L")) { boneName = boneName.Replace(" L", " R"); }
			else if (boneName.EndsWith(" R")) { boneName = boneName.Replace(" R", " L"); }
			else if (boneName.StartsWith("L ")) { boneName = boneName.Replace("L ", "R "); }
			else if (boneName.StartsWith("R ")) { boneName = boneName.Replace("R ", "L "); }
			else if (boneName.Contains(" Left")) { boneName = boneName.Replace(" Left", " Right"); }
			else if (boneName.Contains(" Right")) { boneName = boneName.Replace(" Right", " Left"); }
			else if (boneName.Contains("Left ")) { boneName = boneName.Replace("Left ", "Right "); }
			else if (boneName.Contains("Right ")) { boneName = boneName.Replace("Right ", "Left "); }
			else
			{
				if (rootSrcBone._mirrorOption == apBone.MIRROR_OPTION.X) { boneName = boneName + " (Mirror X)"; }
				else { boneName = boneName + " (Mirror Y)"; }
			}

			newBone._name = boneName;

			//2. 기본 속성 복사
			newBone._renderUnit = srcBone._renderUnit;

			//3. Parent와 연결
			if (mirrorParentBone != null)
			{
				newBone._parentBoneID = mirrorParentBone._uniqueID;
				newBone._parentBone = mirrorParentBone;
			}
			else
			{
				newBone._parentBoneID = -1;
				newBone._parentBone = null;
			}

			newBone._childBoneIDs.Clear();
			newBone._childBones.Clear();

			Vector2 mirrorPos = Vector2.zero;
			float mirrorAngle = 0.0f;
			if (srcBone._parentBone == null)
			{
				//Root Bone이라면
				if (srcBone._mirrorOption == apBone.MIRROR_OPTION.X)
				{
					//X 반전 + Offset
					mirrorPos.x = -1 * (srcBone._defaultMatrix._pos.x - srcBone._mirrorCenterOffset) + srcBone._mirrorCenterOffset;
					mirrorPos.y = srcBone._defaultMatrix._pos.y;
					mirrorAngle = -srcBone._defaultMatrix._angleDeg;
				}
				else
				{
					//Y 반전 + Offset
					mirrorPos.y = -1 * (srcBone._defaultMatrix._pos.y - srcBone._mirrorCenterOffset) + srcBone._mirrorCenterOffset;
					mirrorPos.x = srcBone._defaultMatrix._pos.x;
					float srcAngle = apUtil.AngleTo180(srcBone._defaultMatrix._angleDeg);
					if (srcAngle > 0.0f)
					{
						mirrorAngle = apUtil.AngleTo180(180.0f - srcAngle);
					}
					else
					{
						mirrorAngle = apUtil.AngleTo180(-180.0f - srcAngle);
					}

				}
			}
			else
			{
				mirrorPos = new Vector2(-srcBone._defaultMatrix._pos.x, srcBone._defaultMatrix._pos.y);
				mirrorAngle = -srcBone._defaultMatrix._angleDeg;
			}

			newBone._defaultMatrix.SetTRS(mirrorPos,
											mirrorAngle,
											srcBone._defaultMatrix._scale,
											true);

			newBone._color = srcBone._color +
				new Color(UnityEngine.Random.Range(-0.2f, 0.2f),
							UnityEngine.Random.Range(-0.2f, 0.2f),
							UnityEngine.Random.Range(-0.2f, 0.2f),
							0.0f);
			newBone._shapeWidth = srcBone._shapeWidth;
			newBone._shapeLength = srcBone._shapeLength;
			newBone._shapeTaper = srcBone._shapeTaper;
			newBone._shapeHelper = srcBone._shapeHelper;

			newBone._optionLocalMove = srcBone._optionLocalMove;
			newBone._optionIK = srcBone._optionIK;
			newBone._isIKTail = srcBone._isIKTail;

			newBone._isIKAngleRange = srcBone._isIKAngleRange;

			//대칭인 경우 이 값이 바뀔 것
			float mirrorRange_A = -srcBone._IKAngleRange_Lower;
			float mirrorRange_B = -srcBone._IKAngleRange_Upper;
			newBone._IKAngleRange_Lower = Mathf.Min(mirrorRange_A, mirrorRange_B);
			newBone._IKAngleRange_Upper = Mathf.Max(mirrorRange_A, mirrorRange_B);
			newBone._IKAnglePreferred = -srcBone._IKAnglePreferred;

			//TODO IK 어쩔..
			//.......
			if (newBone._IKController == null)
			{
				newBone._IKController = new apBoneIKController();
			}
			if (srcBone._IKController == null)
			{
				srcBone._IKController = new apBoneIKController();
			}
			newBone._IKController._controllerType = srcBone._IKController._controllerType;
			newBone._IKController._defaultMixWeight = srcBone._IKController._defaultMixWeight;
			newBone._IKController._isWeightByControlParam = srcBone._IKController._isWeightByControlParam;
			newBone._IKController._weightControlParamID = srcBone._IKController._weightControlParamID;
			newBone._IKController._weightControlParam = srcBone._IKController._weightControlParam;

			newBone._isSocketEnabled = srcBone._isSocketEnabled;

			//MirrorBone 서로 연결
			newBone._mirrorBoneID = srcBone._uniqueID;
			newBone._mirrorBone = srcBone;
			srcBone._mirrorBoneID = newBone._uniqueID;
			srcBone._mirrorBone = newBone;

			//Mirror 옵션은 Root의 것을 이용
			srcBone._mirrorOption = rootSrcBone._mirrorOption;
			newBone._mirrorOption = rootSrcBone._mirrorOption;

			//TODO : _mirrorCenterOffset 계산하기
			//- Root Bone이라면 값 반전
			//- Root Bone이 아니라면 다시 Default 확인해서 다시 계산한다.
			newBone._mirrorCenterOffset = -(srcBone._mirrorCenterOffset);

			if (!isRecursive)
			{
				return;
			}

			//자식 본에 대해서도 작성을 한다.
			if (srcBone._childBones != null && srcBone._childBones.Count > 0)
			{
				for (int i = 0; i < srcBone._childBones.Count; i++)
				{
					apBone childBone = srcBone._childBones[i];

					MakeNewMirrorBoneHierarchyRecursive(meshGroup,
														childBone, newBone,
														rootSrcBone,
														true,
														src2Mirror,
														mirror2Src);
				}
			}

		}

		/// <summary>
		/// Mirror된 Bone 연결 (특히 IK)을 갱신한다.
		/// Mirror가 되면 연결이 바뀐다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="mirrorBone"></param>
		/// <param name="mirrorPair"></param>
		/// <param name="srcBones"></param>
		/// <param name="mirrorBones"></param>
		private void SetMirrorBonesLinks(apBone mirrorBone,
											Dictionary<apBone, apBone> src2Mirror,
											Dictionary<apBone, apBone> mirror2Src)
		{
			apBone srcBone = mirror2Src[mirrorBone];

			//IK Target Bone
			mirrorBone._IKTargetBone = GetMirrorBone(srcBone._IKTargetBone, src2Mirror);
			mirrorBone._IKTargetBoneID = (mirrorBone._IKTargetBone != null ? mirrorBone._IKTargetBone._uniqueID : -1);

			//IK Chained Bone
			mirrorBone._IKNextChainedBone = GetMirrorBone(srcBone._IKNextChainedBone, src2Mirror);
			mirrorBone._IKNextChainedBoneID = (mirrorBone._IKNextChainedBone != null ? mirrorBone._IKNextChainedBone._uniqueID : -1);

			//IK Header Bone
			mirrorBone._IKHeaderBone = GetMirrorBone(srcBone._IKHeaderBone, src2Mirror);
			mirrorBone._IKHeaderBoneID = (mirrorBone._IKHeaderBone != null ? mirrorBone._IKHeaderBone._uniqueID : -1);

			//IK Controller
			if (srcBone._IKController == null)
			{
				srcBone._IKController = new apBoneIKController();
			}
			if (mirrorBone._IKController == null)
			{
				mirrorBone._IKController = new apBoneIKController();
			}
			mirrorBone._IKController._effectorBone = GetMirrorBone(srcBone._IKController._effectorBone, src2Mirror);
			mirrorBone._IKController._effectorBoneID = (mirrorBone._IKController._effectorBone != null ? mirrorBone._IKController._effectorBone._uniqueID : -1);

			if (mirrorBone._childBones != null && mirrorBone._childBones.Count > 0)
			{
				for (int i = 0; i < mirrorBone._childBones.Count; i++)
				{
					SetMirrorBonesLinks(mirrorBone._childBones[i],
											src2Mirror,
											mirror2Src);
				}
			}
		}

		private apBone GetMirrorBone(apBone srcBone, Dictionary<apBone, apBone> src2Mirror)
		{
			if (srcBone == null) { return null; }
			if (src2Mirror.ContainsKey(srcBone)) { return src2Mirror[srcBone]; }
			return null;
		}



		public void DuplicateBone(apMeshGroup meshGroup, apBone srcBone, float offsetX, float offsetY, bool isDuplicateChildren)
		{
			if (meshGroup == null || srcBone == null)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AddBone, 
												Editor, 
												meshGroup, 
												//null, 
												false, false,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			//1. 일단 본들을 생성하고, Src>Copied를 연결한다.
			//2. IK 설정도 연결한다.

			//World Matrix에서 위치를 보정할 것이므로, 필요한 행렬 변수를 가져오자
			apBone parentBone = srcBone._parentBone;
			//apMatrix default2WorldMatrix = null;//<< 이전 방식
			if (parentBone != null)
			{
				parentBone.MakeWorldMatrix(true);
				//default2WorldMatrix = new apMatrix(parentBone._worldMatrix_NonModified);//<< 이전 방식
			}
			else
			{
				srcBone.MakeWorldMatrix(true);
			}



			Dictionary<apBone, apBone> Src2Dst = new Dictionary<apBone, apBone>();
			Dictionary<apBone, apBone> Dst2Src = new Dictionary<apBone, apBone>();


			//기본 한개의 본을 만들고, 옵션에 따라 Recursive하게 만든다.
			//일단 만들고 속성 복사
			//- IK 설정은 이후에 일괄적으로 연결한다.
			//- Mirror 속성은 복사되지 않는다.
			apBone copiedBone = AddBone(meshGroup, parentBone, false);

			copiedBone._name = srcBone._name + " (Copied)";

			//위치는 World 좌표계로 Offset을 줘야한다.
			
			//이전 방식
			//apMatrix targetMatrix = new apMatrix(srcBone._worldMatrix_NonModified);
			//targetMatrix._pos.x += offsetX;
			//targetMatrix._pos.y += offsetY;
			//targetMatrix.RInverse(default2WorldMatrix);


			//변경 20.8.19 : 래핑된 코드
			apBoneWorldMatrix parentMatrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
											0, Editor._portrait,
											(parentBone != null ? parentBone._worldMatrix_NonModified : null),
											(srcBone._renderUnit != null ? srcBone._renderUnit.WorldMatrixWrapWithoutModified : null));

			apBoneWorldMatrix newDefaultMatrix = apBoneWorldMatrix.MakeTempWorldMatrix(
											1, Editor._portrait,
											srcBone._worldMatrix_NonModified);
			newDefaultMatrix.MoveAsResult(new Vector2(offsetX, offsetY));
			newDefaultMatrix.SetWorld2Default(parentMatrix);


			//copiedBone._defaultMatrix = new apMatrix(targetMatrix);//<< 이전
			//변경 20.8.19
			copiedBone._defaultMatrix = apMatrix.TRS(newDefaultMatrix.Pos, newDefaultMatrix.Angle, newDefaultMatrix.Scale);
			copiedBone._defaultMatrix.MakeMatrix();

			copiedBone._color = srcBone._color;
			copiedBone._shapeWidth = srcBone._shapeWidth;
			copiedBone._shapeLength = srcBone._shapeLength;
			copiedBone._shapeTaper = srcBone._shapeTaper;

			copiedBone._shapeHelper = srcBone._shapeHelper;

			copiedBone._isSocketEnabled = srcBone._isSocketEnabled;

			//[v1.5.1] 복사되지 않은 속성들 복사
			//지글본
			copiedBone._isJiggle = srcBone._isJiggle;
			copiedBone._jiggle_Mass = srcBone._jiggle_Mass;
			copiedBone._jiggle_K = srcBone._jiggle_K;
			copiedBone._jiggle_Drag = srcBone._jiggle_Drag;
			copiedBone._jiggle_Damping = srcBone._jiggle_Damping;
			copiedBone._isJiggleAngleConstraint = srcBone._isJiggleAngleConstraint;
			copiedBone._jiggle_AngleLimit_Min = srcBone._jiggle_AngleLimit_Min;
			copiedBone._jiggle_AngleLimit_Max = srcBone._jiggle_AngleLimit_Max;
			copiedBone._jiggle_IsControlParamWeight = srcBone._jiggle_IsControlParamWeight;
			copiedBone._jiggle_WeightControlParamID = srcBone._jiggle_WeightControlParamID;
			copiedBone._linkedJiggleControlParam = srcBone._linkedJiggleControlParam;



			//매핑
			Src2Dst.Add(srcBone, copiedBone);
			Dst2Src.Add(copiedBone, srcBone);


			if (isDuplicateChildren)
			{
				//자식들도 복사해주자
				DuplicateBonesRecursive(copiedBone, srcBone, meshGroup, /*offsetX, offsetY, */Src2Dst, Dst2Src);
			}

			//모두 복사가 끝났다면, IK설정을 복사한다.
			foreach (KeyValuePair<apBone, apBone> Src2DstPair in Src2Dst)
			{
				apBone src = Src2DstPair.Key;
				apBone dst = Src2DstPair.Value;

				//IK 설정을 복사하자.
				dst._optionLocalMove = src._optionLocalMove;
				dst._optionIK = src._optionIK;

				dst._isIKTail = src._isIKTail;

				dst._isIKAngleRange = src._isIKAngleRange;
				dst._IKAngleRange_Lower = src._IKAngleRange_Lower;
				dst._IKAngleRange_Upper = src._IKAngleRange_Upper;
				dst._IKAnglePreferred = src._IKAnglePreferred;

				//[v1.5.1] IK 추가 설정 복사
				dst._IKDepth = src._IKDepth;
				dst._isIKSoftAngleLimit = src._isIKSoftAngleLimit;


				apBone dstIKTargetBone = null;
				if (src._IKTargetBoneID >= 0 && src._IKTargetBone != null)
				{
					//IK 설정이 복사가 될지 확인하자.
					dstIKTargetBone = null;
					if (Src2Dst.ContainsKey(src._IKTargetBone))
					{
						//IK Target과 대칭되는 복제된 본
						dstIKTargetBone = Src2Dst[src._IKTargetBone];
					}
				}

				if (dstIKTargetBone != null)
				{
					//같이 복제된 IK 타겟이 있다.
					dst._IKTargetBoneID = dstIKTargetBone._uniqueID;
					dst._IKTargetBone = dstIKTargetBone;

					//그 외의 본도 복사하자. (없으면 일단 생략)
					apBone dstIKNextChainedBone = null;
					apBone dstIKHeaderBone = null;

					if (src._IKNextChainedBone != null)
					{
						if (Src2Dst.ContainsKey(src._IKNextChainedBone))
						{
							dstIKNextChainedBone = Src2Dst[src._IKNextChainedBone];
						}
					}

					if (src._IKHeaderBone != null)
					{
						if (Src2Dst.ContainsKey(src._IKHeaderBone))
						{
							dstIKHeaderBone = Src2Dst[src._IKHeaderBone];
						}
					}

					if (dstIKNextChainedBone != null)
					{
						dst._IKNextChainedBoneID = dstIKNextChainedBone._uniqueID;
						dst._IKNextChainedBone = dstIKNextChainedBone;
					}
					else
					{
						dst._IKNextChainedBoneID = -1;
						dst._IKNextChainedBone = null;
					}

					if (dstIKHeaderBone != null)
					{
						dst._IKHeaderBoneID = dstIKHeaderBone._uniqueID;
						dst._IKHeaderBone = dstIKHeaderBone;
					}
					else
					{
						dst._IKHeaderBoneID = -1;
						dst._IKHeaderBone = null;
					}
				}
				else
				{
					//연결된 IK 타겟이 없다.
					dst._IKTargetBoneID = -1;
					dst._IKTargetBone = null;

					dst._IKNextChainedBoneID = -1;
					dst._IKNextChainedBone = null;

					dst._IKHeaderBoneID = -1;
					dst._IKHeaderBone = null;
				}

				//IK Controller에 대해서도 설정하자
				//이건 복제된게 없다면 그대로 가도 상관없다.
				if (dst._IKController == null)
				{
					dst._IKController = new apBoneIKController();
				}
				if (src._IKController != null)
				{
					apBoneIKController srcIKCont = src._IKController;
					apBoneIKController dstIKCont = dst._IKController;

					dstIKCont._controllerType = srcIKCont._controllerType;
					dstIKCont._parentBone = dst;

					dstIKCont._effectorBoneID = srcIKCont._effectorBoneID;
					dstIKCont._effectorBone = srcIKCont._effectorBone;

					dstIKCont._defaultMixWeight = srcIKCont._defaultMixWeight;

					dstIKCont._isWeightByControlParam = srcIKCont._isWeightByControlParam;
					dstIKCont._weightControlParamID = srcIKCont._weightControlParamID;
					dstIKCont._weightControlParam = srcIKCont._weightControlParam;
				}
			}

			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(meshGroup));

			meshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			meshGroup.RefreshForce(true);
			meshGroup.UpdateBonesWorldMatrix();
			//meshGroup.RefreshBoneGUIVisible();

			RefreshBoneChaining(meshGroup);
			RefreshBoneHierarchy(meshGroup);

			Editor.RefreshControllerAndHierarchy(false);
		}


		private void DuplicateBonesRecursive(apBone parentCopiedBone, apBone parentSrcBone,
											apMeshGroup meshGroup,
											//float offsetX, float offsetY, 
											Dictionary<apBone, apBone> Src2Dst, Dictionary<apBone, apBone> Dst2Src)
		{
			//연결시킬 자식 본을 하나 꺼내자
			if (parentSrcBone._childBones == null && parentSrcBone._childBones.Count == 0)
			{
				return;
			}

			for (int iChild = 0; iChild < parentSrcBone._childBones.Count; iChild++)
			{
				apBone childSrcBone = parentSrcBone._childBones[iChild];

				//자식에 대한 복제 본을 만들자
				apBone childCopiedBone = AddBone(meshGroup, parentCopiedBone, false);

				childCopiedBone._name = childSrcBone._name + " (Copied)";
				childCopiedBone._defaultMatrix = new apMatrix(childSrcBone._defaultMatrix);
				//childCopiedBone._defaultMatrix._pos += new Vector2(offsetX, offsetY);

				childCopiedBone._color = childSrcBone._color;
				childCopiedBone._shapeWidth = childSrcBone._shapeWidth;
				childCopiedBone._shapeLength = childSrcBone._shapeLength;
				childCopiedBone._shapeTaper = childSrcBone._shapeTaper;

				childCopiedBone._shapeHelper = childSrcBone._shapeHelper;

				childCopiedBone._isSocketEnabled = childSrcBone._isSocketEnabled;


				//[v1.5.1] 복사되지 않은 속성들 복사
				//지글본
				childCopiedBone._isJiggle = childSrcBone._isJiggle;
				childCopiedBone._jiggle_Mass = childSrcBone._jiggle_Mass;
				childCopiedBone._jiggle_K = childSrcBone._jiggle_K;
				childCopiedBone._jiggle_Drag = childSrcBone._jiggle_Drag;
				childCopiedBone._jiggle_Damping = childSrcBone._jiggle_Damping;
				childCopiedBone._isJiggleAngleConstraint = childSrcBone._isJiggleAngleConstraint;
				childCopiedBone._jiggle_AngleLimit_Min = childSrcBone._jiggle_AngleLimit_Min;
				childCopiedBone._jiggle_AngleLimit_Max = childSrcBone._jiggle_AngleLimit_Max;
				childCopiedBone._jiggle_IsControlParamWeight = childSrcBone._jiggle_IsControlParamWeight;
				childCopiedBone._jiggle_WeightControlParamID = childSrcBone._jiggle_WeightControlParamID;
				childCopiedBone._linkedJiggleControlParam = childSrcBone._linkedJiggleControlParam;




				//매핑
				Src2Dst.Add(childSrcBone, childCopiedBone);
				Dst2Src.Add(childCopiedBone, childSrcBone);

				//새로운 본에 대한 자식 본을 계속 만들자
				DuplicateBonesRecursive(childCopiedBone, childSrcBone, meshGroup, /*offsetX, offsetY, */Src2Dst, Dst2Src);
			}
		}

		/// <summary>
		/// 추가 20.1.7 : 다른 메시 그룹으로 본들을 복제하는 함수
		/// </summary>
		/// <param name="srcMeshGroup"></param>
		/// <param name="dstMeshGroup"></param>
		/// <param name="isDuplicateChildren"></param>
		public void DuplicateBonesToOtherMeshGroup(	apMeshGroup srcMeshGroup, 
													apMeshGroup dstMeshGroup, 
													Dictionary<apBone, apBone> srcBone2NewBone,
													bool isRecordUndo, bool isRefresh)
		{
			if (Editor == null
				|| Editor.Select == null
				|| Editor._portrait == null
				|| srcMeshGroup == null
				|| dstMeshGroup == null)
			{
				return;
			}


			//Undo
			if(isRecordUndo)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DuplicateMeshGroupTransform, 
													Editor, 
													dstMeshGroup, 
													//null, 
													false, false,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}
			

			//1. 일단 본들을 생성하고, Src>Copied를 연결한다.
			//2. IK 설정도 연결한다.

			//1. Root Bone들을 기준으로 복제를 한다.(Recursive)
			//이 단계에서는 연결이 되지 않는다.
			int nRootBones = srcMeshGroup._boneList_Root == null ? 0 : srcMeshGroup._boneList_Root.Count;
			if(nRootBones == 0)
			{
				//Root Bone이 없네염?
				return;
			}
			
			//Dictionary<apBone, apBone> Src2Dst = new Dictionary<apBone, apBone>();
			Dictionary<apBone, apBone> dstBone2SrcBone = new Dictionary<apBone, apBone>();
			List<apBone> duplicatedBones = new List<apBone>();

			apBone rootBone = null;
			for (int iRootBone = 0; iRootBone < nRootBones; iRootBone++)
			{
				rootBone = srcMeshGroup._boneList_Root[iRootBone];
				DuplicateBonesToOtherMeshGroupRecursive(rootBone, null, srcMeshGroup, dstMeshGroup, srcBone2NewBone, dstBone2SrcBone, duplicatedBones);
			}

			//이제 복사된 본의 연결 정보를 설정하자
			int nDuplicatedBones = duplicatedBones.Count;
			apBone dstBone = null;
			apBone srcBone = null;
			apBone dstLinkedBone = null;
			for (int iBone = 0; iBone < nDuplicatedBones; iBone++)
			{
				dstBone = duplicatedBones[iBone];
				srcBone = null;
				if(!dstBone2SrcBone.ContainsKey(dstBone))
				{
					//키가 등록되지 않았다 > 본간의 연결을 설정할 수 없다.
					continue;
				}
				srcBone = dstBone2SrcBone[dstBone];
				if(srcBone == null)
				{
					//으잉 값이 없넹
					continue;
				}

				//IK 값을 연결해주자
				//1. _IKTargetBone
				dstLinkedBone = null;
				if(srcBone._IKTargetBoneID >= 0 && srcBone._IKTargetBone != null)
				{
					if(srcBone2NewBone.ContainsKey(srcBone._IKTargetBone))
					{
						dstLinkedBone = srcBone2NewBone[srcBone._IKTargetBone];
					}
				}
				if(dstLinkedBone != null)
				{
					dstBone._IKTargetBone = dstLinkedBone;
					dstBone._IKTargetBoneID = dstLinkedBone._uniqueID;
				}
				else
				{
					dstBone._IKTargetBone = null;
					dstBone._IKTargetBoneID = -1;
				}


				//2. _IKNextChainedBone
				dstLinkedBone = null;
				if(srcBone._IKNextChainedBoneID >= 0 && srcBone._IKNextChainedBone != null)
				{
					if(srcBone2NewBone.ContainsKey(srcBone._IKNextChainedBone))
					{
						dstLinkedBone = srcBone2NewBone[srcBone._IKNextChainedBone];
					}
				}
				if(dstLinkedBone != null)
				{
					dstBone._IKNextChainedBone = dstLinkedBone;
					dstBone._IKNextChainedBoneID = dstLinkedBone._uniqueID;
				}
				else
				{
					dstBone._IKNextChainedBone = null;
					dstBone._IKNextChainedBoneID = -1;
				}

				//3. _IKHeaderBone
				dstLinkedBone = null;
				if(srcBone._IKHeaderBoneID >= 0 && srcBone._IKHeaderBone != null)
				{
					if(srcBone2NewBone.ContainsKey(srcBone._IKHeaderBone))
					{
						dstLinkedBone = srcBone2NewBone[srcBone._IKHeaderBone];
					}
				}
				if(dstLinkedBone != null)
				{
					dstBone._IKHeaderBone = dstLinkedBone;
					dstBone._IKHeaderBoneID = dstLinkedBone._uniqueID;
				}
				else
				{
					dstBone._IKHeaderBone = null;
					dstBone._IKHeaderBoneID = -1;
				}

				//4. _mirrorBone
				dstLinkedBone = null;
				if(srcBone._mirrorBoneID >= 0 && srcBone._mirrorBone != null)
				{
					if(srcBone2NewBone.ContainsKey(srcBone._mirrorBone))
					{
						dstLinkedBone = srcBone2NewBone[srcBone._mirrorBone];
					}
				}
				if(dstLinkedBone != null)
				{
					dstBone._mirrorBone = dstLinkedBone;
					dstBone._mirrorBoneID = dstLinkedBone._uniqueID;
				}
				else
				{
					dstBone._mirrorBone = null;
					dstBone._mirrorBoneID = -1;
				}

				//5. IK Controller도 복사하자 (기본 데이터는 위에서 생성되었을 것)
				if (dstBone._IKController != null && srcBone._IKController != null)
				{
					apBoneIKController srcIKController = srcBone._IKController;
					apBoneIKController dstIKController = dstBone._IKController;

					//5-1. Effector Bone
					dstLinkedBone = null;
					if (srcIKController._effectorBoneID >= 0 && srcIKController._effectorBone != null)
					{
						if (srcBone2NewBone.ContainsKey(srcIKController._effectorBone))
						{
							dstLinkedBone = srcBone2NewBone[srcIKController._effectorBone];
						}
					}
					if (dstLinkedBone != null)
					{
						dstIKController._effectorBone = dstLinkedBone;
						dstIKController._effectorBoneID = dstLinkedBone._uniqueID;
					}
					else
					{
						dstIKController._effectorBone = null;
						dstIKController._effectorBoneID = -1;
					}
				}

				
			}

			

			if (isRefresh)
			{
				Editor.Select.SelectMeshGroup(dstMeshGroup);
				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(dstMeshGroup));

				dstMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
				dstMeshGroup.RefreshForce(true);
				dstMeshGroup.UpdateBonesWorldMatrix();
				//dstMeshGroup.RefreshBoneGUIVisible();

				RefreshBoneChaining(dstMeshGroup);
				RefreshBoneHierarchy(dstMeshGroup);
			}
			

			Editor.RefreshControllerAndHierarchy(false);

		}
		private void DuplicateBonesToOtherMeshGroupRecursive(
											apBone srcBone,
											apBone parentNewBone,
											apMeshGroup srcMeshGroup, apMeshGroup dstMeshGroup,
											Dictionary<apBone, apBone> Src2Dst, 
											Dictionary<apBone, apBone> Dst2Src,
											List<apBone> duplicatedBones)
		{
			//현재 본을 복사해서 값을 복사하자
			if(srcBone == null)
			{
				return;
			}

			//새로운 본 생성하고 복사하기
			apBone newBone = AddBone(dstMeshGroup, parentNewBone, false);

			if(newBone == null)
			{
				//실패
				return;
			}

			Src2Dst.Add(srcBone, newBone);
			Dst2Src.Add(newBone, srcBone);
			duplicatedBones.Add(newBone);

			//값을 복사하자
			//연결 정보는 이 단계에서 생략
			newBone._name = srcBone._name;
			newBone._meshGroup = dstMeshGroup;//소속되는 MeshGroup은 Dst로
			newBone._meshGroupID = dstMeshGroup._uniqueID;

			if(parentNewBone != null)
			{
				//부모 본 연결
				newBone._parentBone = parentNewBone;
				newBone._parentBoneID = parentNewBone._uniqueID;

				//부모의 자식으로도 등록
				parentNewBone._childBones.Add(newBone);
				parentNewBone._childBoneIDs.Add(newBone._uniqueID);
			}
			else
			{
				//부모 본이 없다
				newBone._parentBone = null;
				newBone._parentBoneID = -1;
			}
			//기본 정보
			newBone._level = srcBone._level;
			newBone._recursiveIndex = srcBone._recursiveIndex;

			newBone._defaultMatrix.SetMatrix(srcBone._defaultMatrix, true);
			newBone._depth = srcBone._depth;

			//출력 설정
			newBone._color = srcBone._color;
			newBone._shapeWidth = srcBone._shapeWidth;
			newBone._shapeLength = srcBone._shapeLength;
			newBone._shapeTaper = srcBone._shapeTaper;
			newBone._shapeHelper = srcBone._shapeHelper;

			//IK 설정
			newBone._optionLocalMove = srcBone._optionLocalMove;
			newBone._optionIK = srcBone._optionIK;
			newBone._isIKTail = srcBone._isIKTail;
			//> _IKTargetBoneID, _IKTargetBone, _IKNextChainedBoneID, _IKNextChainedBone, _IKHeaderBoneID, _IKHeaderBone는 나중에 설정

			newBone._isIKAngleRange = srcBone._isIKAngleRange;
			newBone._IKAngleRange_Lower = srcBone._IKAngleRange_Lower;
			newBone._IKAngleRange_Upper = srcBone._IKAngleRange_Upper;
			newBone._IKAnglePreferred = srcBone._IKAnglePreferred;

			//[v1.5.1] IK 추가 설정 복사
			newBone._IKDepth = srcBone._IKDepth;
			newBone._isIKSoftAngleLimit = srcBone._isIKSoftAngleLimit;



			//기타 정보
			newBone._isRigTestPosing = false;
			newBone._isSocketEnabled = srcBone._isSocketEnabled;


			//[v1.5.1] 복사되지 않은 속성들 복사
			//지글본
			newBone._isJiggle = srcBone._isJiggle;
			newBone._jiggle_Mass = srcBone._jiggle_Mass;
			newBone._jiggle_K = srcBone._jiggle_K;
			newBone._jiggle_Drag = srcBone._jiggle_Drag;
			newBone._jiggle_Damping = srcBone._jiggle_Damping;
			newBone._isJiggleAngleConstraint = srcBone._isJiggleAngleConstraint;
			newBone._jiggle_AngleLimit_Min = srcBone._jiggle_AngleLimit_Min;
			newBone._jiggle_AngleLimit_Max = srcBone._jiggle_AngleLimit_Max;
			newBone._jiggle_IsControlParamWeight = srcBone._jiggle_IsControlParamWeight;
			newBone._jiggle_WeightControlParamID = srcBone._jiggle_WeightControlParamID;
			newBone._linkedJiggleControlParam = srcBone._linkedJiggleControlParam;



			//IK 컨트롤러 (연결 정보를 제외하고 복사)
			if(newBone._IKController == null)
			{
				newBone._IKController = new apBoneIKController();
			}
			if(srcBone._IKController != null)
			{
				apBoneIKController srcIKController = srcBone._IKController;
				apBoneIKController newIKController = newBone._IKController;

				newIKController._controllerType = srcIKController._controllerType;
				//연결 정보는 나중에..
				newIKController._defaultMixWeight = srcIKController._defaultMixWeight;
				newIKController._isWeightByControlParam = srcIKController._isWeightByControlParam;
				newIKController._weightControlParamID = srcIKController._weightControlParamID;
				newIKController._weightControlParam = srcIKController._weightControlParam;
			}

			//미러 본 설정 (연결 정보 나중에)
			newBone._mirrorOption = srcBone._mirrorOption;
			newBone._mirrorCenterOffset = srcBone._mirrorCenterOffset;
			
			
			//srcBone의 자식 본이 있다면 하나씩 연결하자
			int nChildBones = srcBone._childBones == null ? 0 : srcBone._childBones.Count;
			if(nChildBones > 0)
			{
				apBone srcChildBone = null;
				for (int iChildBone = 0; iChildBone < nChildBones; iChildBone++)
				{
					srcChildBone = srcBone._childBones[iChildBone];
					if(srcChildBone == null)
					{
						continue;
					}

					//자식 본도 복사
					DuplicateBonesToOtherMeshGroupRecursive(
											srcChildBone,
											newBone,
											srcMeshGroup, dstMeshGroup,
											Src2Dst, Dst2Src, duplicatedBones);
				}
			}

		}




		/// <summary>
		/// 자식 본으로 스냅하는 함수
		/// </summary>
		/// <param name="srcBone"></param>
		/// <param name="childBone"></param>
		/// <param name="meshGroup"></param>
		public void SnapBoneEndToChildBone(apBone srcBone, apBone childBone, apMeshGroup meshGroup)
		{
			if(meshGroup == null || srcBone == null || childBone == null || srcBone == childBone)
			{
				return;
			}

			if(srcBone._childBones == null)
			{
				return;
			}
			if(!srcBone._childBones.Contains(childBone))
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
												Editor, 
												meshGroup, 
												//null, 
												false, false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			//World Matrix에서 위치를 보정할 것이므로, 필요한 행렬 변수를 가져오자
			apBone parentBone = srcBone._parentBone;
			
			//apMatrix default2WorldMatrix = null;//이전. 아래의 코드에서 래핑됨
			if(parentBone != null)
			{
				parentBone.MakeWorldMatrix(true);
				//default2WorldMatrix = new apMatrix(parentBone._worldMatrix_NonModified);//<이전. 아래의 코드에서 래핑됨
			}
			else
			{
				srcBone.MakeWorldMatrix(true);
				//이전. 아래 코드에서 래핑됨.
				//if(srcBone._renderUnit != null)
				//{
				//	default2WorldMatrix = new apMatrix(srcBone._renderUnit.WorldMatrixWrapWithoutModified);
				//}
				//else
				//{
				//	default2WorldMatrix = new apMatrix();
				//	default2WorldMatrix.SetIdentity();
				//}
			}

			//회전과 ShapeLegth를 조절하자.
			//이전
			//apMatrix srcWorldMatrix_Prev = new apMatrix(srcBone._worldMatrix_NonModified);
			//apMatrix targetChildWorldMatrix = childBone._worldMatrix_NonModified;

			//변경 20.8.19 : 래핑된 코드
			apBoneWorldMatrix srcWorldMatrix_Prev = apBoneWorldMatrix.MakeTempWorldMatrix(0, Editor._portrait, srcBone._worldMatrix_NonModified);
			apBoneWorldMatrix targetChildWorldMatrix = apBoneWorldMatrix.MakeTempWorldMatrix(1, Editor._portrait, childBone._worldMatrix_NonModified);

			apBoneWorldMatrix parentMatrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(2, Editor._portrait,
															(parentBone != null ? parentBone._worldMatrix_NonModified : null),
															(srcBone._renderUnit != null ? srcBone._renderUnit.WorldMatrixWrapWithoutModified : null));

			float dist2Target = Vector2.Distance(targetChildWorldMatrix.Pos, srcWorldMatrix_Prev.Pos);

			//일단 길이부터 바꾸자.
			srcBone._shapeLength = (int)(dist2Target + 0.5f);
			
			//길이가 0 이상이라면
			if(srcBone._shapeLength > 0)
			{
				//각도도 바꾸고, 다른 Child의 Default Matrix도 모두 바꿔야 한다.
				
				//이전
				//float angle2Child = Mathf.Atan2(targetChildWorldMatrix.Pos.y - srcWorldMatrix_Prev.Pos.y, targetChildWorldMatrix.Pos.x - srcWorldMatrix_Prev.Pos.x);

				//변경 20.8.29 : 상대 위치는 IKSpace에서 바꾸어야 한다.
				Vector2 targetChildPos_IKSpace = srcWorldMatrix_Prev.ConvertForIK(targetChildWorldMatrix.Pos);
				Vector2 srcPos_IKSpace = srcWorldMatrix_Prev.Pos_IKSpace;
				float angle2Child = Mathf.Atan2(targetChildPos_IKSpace.y - srcPos_IKSpace.y, targetChildPos_IKSpace.x - srcPos_IKSpace.x);


				angle2Child *= Mathf.Rad2Deg;
				angle2Child -= 90.0f;
				
				while(angle2Child < -180.0f)
				{
					angle2Child += 360.0f;
				}

				while(angle2Child > 180.0f)
				{
					angle2Child -= 360.0f;
				}

				//이전
				//apMatrix srcWorldMatrix_Next = new apMatrix(srcWorldMatrix_Prev);
				//srcWorldMatrix_Next._angleDeg = angle2Child;
				//srcWorldMatrix_Next.MakeMatrix(true);

				////World(next) > Default로 변환하자.
				//apMatrix srcDefaultMatrix_Next = new apMatrix(srcWorldMatrix_Next);
				//srcDefaultMatrix_Next.RInverse(default2WorldMatrix);

				
				//변경 20.8.19 : 래핑된 코드
				apBoneWorldMatrix srcWorldMatrix_Next = apBoneWorldMatrix.MakeTempWorldMatrix(3, Editor._portrait, srcWorldMatrix_Prev);
				srcWorldMatrix_Next.SetAngleAsStep1(angle2Child, true);


				//World(next) > Default로 변환하자.
				apBoneWorldMatrix srcDefaultMatrix_Next = apBoneWorldMatrix.MakeTempWorldMatrix(4, Editor._portrait, srcWorldMatrix_Next);
				srcDefaultMatrix_Next.SetWorld2Default(parentMatrix);

				

				//자식 본들의 Default Matrix도 바꾸자
				//apMatrix childMatrix_Next = new apMatrix();//이전
				apBoneWorldMatrix childMatrix_Next = apBoneWorldMatrix.GetTemp(5, Editor._portrait);

				for (int iChild = 0; iChild < srcBone._childBones.Count; iChild++)
				{
					apBone curChildBone = srcBone._childBones[iChild];

					//Child의 Default->World Matrix는 Parent인 srcBone의 World Matrix이다.

					//이전
					//childMatrix_Next.SetMatrix(curChildBone._worldMatrix_NonModified);//World는 유지
					//childMatrix_Next.RInverse(srcWorldMatrix_Next);//Default2World가 바뀐 것 (Next로)

					//변경 20.8.19 : 래핑된 코드
					childMatrix_Next.CopyFromMatrix(curChildBone._worldMatrix_NonModified);//World는 유지
					childMatrix_Next.SetWorld2Default(srcWorldMatrix_Next);//Default2World가 바뀐 것 (Next로)



					//바뀐 Default 적용
					//curChildBone._defaultMatrix.SetMatrix(childMatrix_Next);//이전
					curChildBone._defaultMatrix.SetTRS(childMatrix_Next.Pos, childMatrix_Next.Angle, childMatrix_Next.Scale, true);//<<변경 20.8.19
				}

				//변경된 Default Matrix를 적용
				//srcBone._defaultMatrix.SetMatrix(srcDefaultMatrix_Next);
				srcBone._defaultMatrix.SetTRS(srcDefaultMatrix_Next.Pos, srcDefaultMatrix_Next.Angle, srcDefaultMatrix_Next.Scale, true);//변경 20.8.19

				//다시 연산
				srcBone.MakeWorldMatrix(true);
				
			}
			
			srcBone.GUIUpdate(true, false);

			meshGroup.RefreshForce(true);
			meshGroup.UpdateBonesWorldMatrix();
			
			RefreshBoneChaining(meshGroup);
		}
		


		//20.3.24 : 본의 색상을 프리셋을 이용하여 바꾸기
		public void ChangeBoneColorWithPreset(apMeshGroup meshGroup, apBone bone, Color presetColor)
		{
			if(meshGroup == null || bone == null)
			{
				return;
			}
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
												Editor, 
												meshGroup, 
												//bone, 
												false, false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			//프리셋 색상으로부터 범위를 정한다.
			Color newBoneColor = Color.black;

			//이전
			//if (bone._parentBone != null)
			//{
			//	//부모 본이 있다면, 아무리 프리셋을 적용한다고 해도 부모 본의 색상과는 다르게 만들자
			//	newBoneColor = apEditorUtil.GetSimilarColorButDiff(presetColor, bone._parentBone._color, 0.7f, 1.0f, 0.6f, 1.0f);
			//}
			//else
			//{
			//	//부모 본이 없다면 프리셋 색상과 유사한 아무 색상이나 오케이
			//	newBoneColor = apEditorUtil.GetSimilarColor(presetColor, 0.7f, 1.0f, 0.6f, 1.0f, false);
			//}

			//변경 v1.4.7
			float hue = 0.0f;
			float sat = 0.0f;
			float val = 0.0f;

			Color.RGBToHSV(presetColor, out hue, out sat, out val);

			//채도, 밝기는 자동으로 설정한다.
			sat = UnityEngine.Random.Range(0.7f, 1.4f);//일부러 범위를 오버시킴
			val = UnityEngine.Random.Range(0.6f, 1.4f);

			if(sat > 1.0f)
			{
				sat = 1.0f;
			}
			if(val > 1.0f)
			{
				val = 1.0f;
			}
			
			//Hue는 약간의 바리에이션
			//총 6개의 프리셋이 있으므로
			//각각의 간격은 0.167, 즉, +- 0.083 만큼 랜덤 범위를 가진다.
			//범위는 조금 더 줄여서 중간 영역에서 색을 만들지 말자
			float randHue = UnityEngine.Random.Range(-0.07f, 0.07f);
			hue += randHue;
			if(hue < 0.0f)
			{
				hue += 1.0f;
			}
			else if(hue > 1.0f)
			{
				hue -= 1.0f;
			}
			newBoneColor = Color.HSVToRGB(hue, sat, val);

			bone._color = newBoneColor;
		}

		//---------------------------------------------------------------------------------------


		/// <summary>
		/// 해당 MeshGroup의 본들의 계층 연결 관계를 다시 갱신한다.
		/// IK Chaining은 호출되지 않으므로 별도로 (RefreshBoneChaining)를 호출하자
		/// 이 함수는 Link 이후에 호출해주자
		/// </summary>
		/// <param name="meshGroup"></param>
		public void RefreshBoneHierarchy(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//<BONE_EDIT>
			//for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			//{
			//	//Root 부터 재귀적으로 호출한다.
			//	RefreshBoneHierarchyUnit(meshGroup, meshGroup._boneList_Root[i], null);
			//}

			//if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
			//{
			//	for (int i = 0; i < meshGroup._childMeshGroupTransformsWithBones.Count; i++)
			//	{
			//		apTransform_MeshGroup meshGroupTransform = meshGroup._childMeshGroupTransformsWithBones[i];
			//		if (meshGroupTransform._meshGroup != null)
			//		{
			//			RefreshBoneHierarchy(meshGroupTransform._meshGroup);
			//		}
			//	}
			//}

			//>> Bone Set으로 변경
			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
			{
				boneSet = meshGroup._boneListSets[iSet];
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					RefreshBoneHierarchyUnit(boneSet._bones_Root[iRoot], null);
				}
			}

			//meshGroup.RefreshBoneGUIVisible();
		}


		//private void RefreshBoneHierarchyUnit(apMeshGroup meshGroup, apBone bone, apBone parentBone)
		private void RefreshBoneHierarchyUnit(apBone bone, apBone parentBone)//<<MeshGroup 필요 없음
		{
			if (bone == null)
			{
				return;
			}
			if (parentBone == null)
			{
				bone._parentBone = null;
				bone._parentBoneID = -1;
			}
			else
			{
				bone._parentBone = parentBone;
				bone._parentBoneID = parentBone._uniqueID;
			}

			bone._childBoneIDs.Clear();//ID 리스트는 일단 Clear
			bone._childBones.RemoveAll(delegate (apBone a)
			{
				return a == null;
			});

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				apBone childBone = bone._childBones[i];
				//ID 연결해주고..
				bone._childBoneIDs.Add(childBone._uniqueID);

				//계층적으로 호출하며 이어가자
				//RefreshBoneHierarchyUnit(meshGroup, childBone, bone);
				RefreshBoneHierarchyUnit(childBone, bone);

			}
		}


		/// <summary>
		/// 해당 본의 IK를 포함한 Chain 갱신을 한다.
		/// 기본 Link이후에 수행해야한다.
		/// IK 설정을 변경하면 한번씩 호출하자
		/// 초기화 이후에도 호출
		/// </summary>
		/// <param name="meshGroup"></param>
		public void RefreshBoneChaining(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//>> Bone Set으로 통합
			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
			{
				boneSet = meshGroup._boneListSets[iSet];
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					//Root 부터 재귀적으로 호출한다.
					RefreshBoneChainingUnit(boneSet._bones_Root[iRoot]);
				}

				//내부적으로도 BoneChaining을 다시 연결해주자
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					boneSet._bones_Root[iRoot].LinkBoneChaining();
				}

				//추가 : Validation도 체크한다.
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					RefreshBoneIKControllerValidationRecursive(boneSet._meshGroup, boneSet._bones_Root[iRoot]);
				}
			}

			//[추가 v1.5.0]
			//IK 업데이트(Calculate IK 함수)에서는 BoneListSet이 아니라
			//IKRunNode를 이용하는 것으로 변경되었다.
			//그래서 메시 그룹 내의 IK Run Node를 갱신해야한다.
			meshGroup.RefreshBoneIKRunNodes();
		}

		//private void RefreshBoneChainingUnit(apMeshGroup meshGroup, apBone bone)
		private void RefreshBoneChainingUnit(apBone bone)//<<MeshGroup이 필요없다.
		{
			if (bone == null)
			{
				return;
			}

			//1. Parent의 값에 따라서 IK Tail / IK Chained 처리를 갱신한다.
			bool isLocalMovable = false;
			if (bone._parentBone != null)
			{
				//Parent의 IK 옵션에 따라서 Tail 처리를 한다.
				switch (bone._parentBone._optionIK)
				{
					case apBone.OPTION_IK.Disabled:
						//Parent의 IK가 꺼져있다.
						//Chained라면 해제해준다.
						if (bone._optionIK == apBone.OPTION_IK.IKChained)
						{
							bone._optionIK = apBone.OPTION_IK.IKSingle;
							bone._IKTargetBone = null;
							bone._IKTargetBoneID = -1;

							bone._IKNextChainedBone = null;
							bone._IKNextChainedBoneID = -1;
						}

						bone._isIKTail = false;
						bone._IKHeaderBone = null;
						bone._IKHeaderBoneID = -1;
						isLocalMovable = true;
						break;

					case apBone.OPTION_IK.IKHead:
					case apBone.OPTION_IK.IKSingle:
					case apBone.OPTION_IK.IKChained:
						{
							//1) Parent가 자신을 타겟으로 삼고 있다면 Tail 처리
							//2) Parent가 자신의 자식을 타겟으로 삼고 있다면 Chained + Tail 처리
							//3) Parent가 자신 또는 자신의 자식을 타겟으로 삼고있지 않다면 IK 타겟이 아니다.
							int IKTargetBoneID = bone._parentBone._IKTargetBoneID;
							apBone IKTargetBone = bone._parentBone._IKTargetBone;
							int IKNextChainedBoneID = bone._parentBone._IKNextChainedBoneID;

							if (bone._uniqueID == IKTargetBoneID)
							{
								//1) 자신을 타겟으로 삼고 있는 경우
								//자신이 Chained였다면 이를 풀어줘야 한다.
								if (bone._optionIK == apBone.OPTION_IK.IKChained)
								{
									bone._optionIK = apBone.OPTION_IK.IKSingle;
									bone._IKTargetBone = null;
									bone._IKTargetBoneID = -1;

									bone._IKNextChainedBone = null;
									bone._IKNextChainedBoneID = -1;
								}

								bone._isIKTail = true;

								//bone._IKHeaderBone = bone._parentBone;
								//bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								if (bone._parentBone._optionIK == apBone.OPTION_IK.IKHead
									|| bone._parentBone._optionIK == apBone.OPTION_IK.IKSingle)
								{
									bone._IKHeaderBone = bone._parentBone;
									bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								}
								else
								{
									bone._IKHeaderBone = bone._parentBone._IKHeaderBone;
									bone._IKHeaderBoneID = bone._parentBone._IKHeaderBoneID;
								}
							}
							else if (bone._uniqueID == IKNextChainedBoneID)
							{
								//2) Parent가 자신의 자식을 타겟으로 삼고 있다면 Chained + Tail 처리
								bone._optionIK = apBone.OPTION_IK.IKChained;
								bone._isIKTail = true;

								//Parent가 Header로 삼고있는 Bone을 Header로 연결하여 공유한다.
								if (bone._parentBone._optionIK == apBone.OPTION_IK.IKHead
									|| bone._parentBone._optionIK == apBone.OPTION_IK.IKSingle)
								{
									bone._IKHeaderBone = bone._parentBone;
									bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								}
								else
								{
									bone._IKHeaderBone = bone._parentBone._IKHeaderBone;
									bone._IKHeaderBoneID = bone._parentBone._IKHeaderBoneID;
								}


								if (bone._IKHeaderBone == null)
								{
									Debug.LogError("Bone Chaining Error : Header를 찾을 수 없다. [" + bone._parentBone._name + " -> " + bone._name + "]");
								}
								else
								{
									//Debug.Log("Chained : " + bone._IKHeaderBone._name + " >> " + bone._parentBone._name + " >> " + bone._name);
								}


								bone._IKNextChainedBone = bone.FindNextChainedBone(IKTargetBoneID);
								if (bone._IKNextChainedBone == null)
								{
									bone._IKNextChainedBoneID = -1;
									Debug.LogError("Bone Chaining Error : IK Chained가 이어지지 않았다. [" + bone._parentBone._name + " -> " + bone._name + " -> (끊김) -> " + IKTargetBone._name);
								}
								else
								{
									bone._IKNextChainedBoneID = bone._IKNextChainedBone._uniqueID;
								}

								//타겟을 공유한다.
								bone._IKTargetBone = IKTargetBone;
								bone._IKTargetBoneID = IKTargetBoneID;


							}
							else
							{
								//3) Parent가 자신 또는 자신의 자식을 타겟으로 삼고있지 않다면 IK 타겟이 아니다.
								//IK Chain이 끊겼다.

								if (bone._optionIK == apBone.OPTION_IK.IKChained)
								{
									bone._optionIK = apBone.OPTION_IK.IKSingle;
									bone._IKTargetBone = null;
									bone._IKTargetBoneID = -1;

									bone._IKNextChainedBone = null;
									bone._IKNextChainedBoneID = -1;
								}

								bone._isIKTail = false;
								bone._IKHeaderBone = null;
								bone._IKHeaderBoneID = -1;
								isLocalMovable = true;
							}
						}
						break;
				}
			}
			else
			{
				bone._isIKTail = false;
				bone._IKHeaderBone = null;
				bone._IKHeaderBoneID = -1;

				isLocalMovable = true;
			}


			//2. Child로의 IK 설정에 따라서 이어지는 Chain 처리를 한다.

			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.Disabled:
					{
						//IK가 꺼져있으니 값을 날리자
						bone._IKTargetBoneID = -1;
						bone._IKTargetBone = null;

						bone._IKNextChainedBoneID = -1;
						bone._IKNextChainedBone = null;
					}
					break;

				case apBone.OPTION_IK.IKChained:
					//Chain 처리는 위의 Parent 처리에서 연동해서 이미 수행했다.
					break;

				case apBone.OPTION_IK.IKHead:
					{
						int targetIKBoneID = bone._IKTargetBoneID;
						int nextChainedBoneID = bone._IKNextChainedBoneID;

						//갱신 작업이 필요한지 체크
						bool isRefreshNeed = false;
						if (bone._IKTargetBone == null || bone._IKNextChainedBone == null)
						{
							//ID는 있는데 연결이 안되었네요
							//다시 연결 필요
							isRefreshNeed = true;
						}
						else
						{
							//검색속도가 빠른 -> Parent로의 함수를 이용하여 유효한 링크인지 판단한다.
							if (bone._IKTargetBone.GetParentRecursive(bone._uniqueID) == null
								|| bone._IKNextChainedBone.GetParentRecursive(bone._uniqueID) == null)
							{
								isRefreshNeed = true;
							}
						}

						if (isRefreshNeed)
						{
							//Target을 기준으로 ID와 레퍼런스 연동을 하자
							apBone targetBone = bone.GetChildBoneRecursive(targetIKBoneID);
							apBone nextChainedBone = bone.FindNextChainedBone(nextChainedBoneID);

							if (targetBone == null || nextChainedBone == null)
							{
								//못찾았네요...
								//Debug.LogError("Bone Chaining Error : IK Header가 적절한 타겟을 찾지 못했다. [" + bone._name + "] > IK 해제됨");

								//에러로 인해 초기화 할때는
								//Child Bone이 1개라면 Single로 초기화
								//Child Bone이 여러개라면 Disabled
								if (bone._childBones.Count == 1 && bone._childBones[0] != null)
								{
									//IKSingle로 초기화하자
									apBone childBone = bone._childBones[0];

									bone._IKTargetBoneID = childBone._uniqueID;
									bone._IKTargetBone = childBone;

									bone._IKNextChainedBoneID = childBone._uniqueID;
									bone._IKNextChainedBone = childBone;

									bone._optionIK = apBone.OPTION_IK.IKSingle;
								}
								else
								{
									//Disabled로 초기화하자
									bone._IKTargetBoneID = -1;
									bone._IKTargetBone = null;

									bone._IKNextChainedBoneID = -1;
									bone._IKNextChainedBone = null;

									bone._optionIK = apBone.OPTION_IK.Disabled;
								}
							}
							else
							{
								//타겟이 있다. 마저 연결하자
								bone._IKTargetBoneID = targetIKBoneID;
								bone._IKTargetBone = targetBone;

								bone._IKNextChainedBoneID = nextChainedBoneID;
								bone._IKNextChainedBone = nextChainedBone;
							}
						}
					}
					break;

				case apBone.OPTION_IK.IKSingle:
					{
						//연결이 유효하면 -> 지속
						//연결이 유효하지 않으면 무조건 Disabled로 바꾼다.
						//자동 연결은 하지 말자

						int targetIKBoneID = bone._IKTargetBoneID;
						int nextChainedBoneID = bone._IKNextChainedBoneID;

						//갱신 작업이 필요한지 체크
						bool isRefreshNeed = false;
						if (bone._IKTargetBone == null || bone._IKNextChainedBone == null)
						{
							//ID는 있는데 연결이 안되었네요
							//다시 연결 필요
							isRefreshNeed = true;
						}
						else
						{
							//Parent/Child 연결 관계가 유효한가
							if (bone._IKTargetBone._parentBone != bone
								|| bone._IKNextChainedBone._parentBone != bone)
							{
								//직접 연결이 안되어있다.
								isRefreshNeed = true;
							}
						}

						if (isRefreshNeed)
						{
							apBone targetBone = bone.GetChildBone(targetIKBoneID);
							apBone nextChainedBone = bone.GetChildBone(targetIKBoneID);

							bool isInvalid = false;
							if (targetBone == null || nextChainedBone == null || targetBone != nextChainedBone)
							{
								isInvalid = true;
							}

							if (!isInvalid)
							{
								//유효한 연결
								bone._IKTargetBoneID = targetIKBoneID;
								bone._IKTargetBone = targetBone;

								bone._IKNextChainedBoneID = nextChainedBoneID;
								bone._IKNextChainedBone = nextChainedBone;
							}
							else
							{
								//유효하지 않은 연결
								//Disabled로 바꾸자
								bone._IKTargetBoneID = -1;
								bone._IKTargetBone = null;

								bone._IKNextChainedBoneID = -1;
								bone._IKNextChainedBone = null;

								bone._optionIK = apBone.OPTION_IK.Disabled;
							}

						}
					}
					break;
			}

			//3. IK 값에 따라서 Local Move 처리를 확인한다.
			if (!isLocalMovable)
			{
				if (bone._optionLocalMove == apBone.OPTION_LOCAL_MOVE.Enabled)
				{
					//IK Tail로 세팅된 상태라면 LocalMove는 불가능하다
					bone._optionLocalMove = apBone.OPTION_LOCAL_MOVE.Disabled;
				}
			}

			//4. Child도 Bone 체크를 하자
			for (int i = 0; i < bone._childBones.Count; i++)
			{
				//RefreshBoneChainingUnit(meshGroup, bone._childBones[i]);
				RefreshBoneChainingUnit(bone._childBones[i]);
			}

		}

		/// <summary>
		/// Bone Chaining 체크할때 같이 실행되는 함수
		/// Position/LookAt Controller가 설정되어 있을 때, 각각의 Bone의 설정들이 유효한지 확인한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="bone"></param>
		private void RefreshBoneIKControllerValidationRecursive(apMeshGroup meshGroup, apBone bone)
		{
			if (bone == null)
			{
				return;
			}

			if (bone._IKController == null)
			{
				bone._IKController = new apBoneIKController();
				bone._IKController.Link(bone, meshGroup, Editor._portrait);
			}

			bone._IKController.CheckValidation();

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				RefreshBoneIKControllerValidationRecursive(meshGroup, bone._childBones[i]);
			}
		}

		//-----------------------------------------------------------------------------
		// 본 리타겟
		//-----------------------------------------------------------------------------
		public void ImportBonesFromRetargetBaseFile(apMeshGroup targetMeshGroup, apRetarget retarget)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.MeshGroup != targetMeshGroup
				|| !retarget.IsBaseFileLoaded)
			{
				return;
			}

			List<apRetargetBoneUnit> importBoneUnits = new List<apRetargetBoneUnit>();
			//Import되는 것만 가져오자
			for (int i = 0; i < retarget.BaseBoneUnits.Count; i++)
			{
				apRetargetBoneUnit boneUnit = retarget.BaseBoneUnits[i];
				if (boneUnit._isImportEnabled)
				{
					importBoneUnits.Add(boneUnit);
				}
			}

			if (importBoneUnits.Count == 0)
			{
				//Import 할게 없네용?
				return;
			}

			float importScale = retarget._importScale;

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AddBone, 
												Editor, 
												targetMeshGroup, 
												//null, 
												false, false,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			//Unit ID -> Bone ID(새로 생성) 으로 연결하는 Map을 만들자
			Dictionary<int, int> unitID2BoneID = new Dictionary<int, int>();

			for (int i = 0; i < importBoneUnits.Count; i++)
			{
				apRetargetBoneUnit boneUnit = importBoneUnits[i];
				int newUniqueBoneID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Bone);
				if (newUniqueBoneID < 0)
				{
					//ID 발급에 실패했다.
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.BoneAddFailed_Title),
												Editor.GetText(TEXT.BoneAddFailed_Body),
												Editor.GetText(TEXT.Close));
					return;
				}
				unitID2BoneID.Add(boneUnit._unitID, newUniqueBoneID);
				boneUnit._boneUniqueID = newUniqueBoneID;//<<여기에 새로 발급한 ID를 직접 넣어주자
			}

			//Bone을 일일이 만들어주자
			//Link는 몰아서 나중에 하자
			List<apBone> addedBoneList = new List<apBone>();

			for (int i = 0; i < importBoneUnits.Count; i++)
			{
				apRetargetBoneUnit boneUnit = importBoneUnits[i];
				apBone newBone = new apBone(boneUnit._boneUniqueID, targetMeshGroup._uniqueID, boneUnit._name);

				newBone.InitTransform(Editor._portrait);

				newBone._parentBoneID = (unitID2BoneID.ContainsKey(boneUnit._parentUnitID) ? unitID2BoneID[boneUnit._parentUnitID] : -1);
				newBone._level = boneUnit._level;

				newBone._childBoneIDs.Clear();

				if (boneUnit._childUnitID != null && boneUnit._childUnitID.Count > 0)
				{
					for (int iChild = 0; iChild < boneUnit._childUnitID.Count; iChild++)
					{
						int childUnitID = boneUnit._childUnitID[iChild];
						int childBoneID = unitID2BoneID.ContainsKey(childUnitID) ? unitID2BoneID[childUnitID] : -1;
						if (childBoneID >= 0)
						{
							newBone._childBoneIDs.Add(childBoneID);
						}
					}
				}

				newBone._defaultMatrix.SetMatrix(boneUnit._defaultMatrix, true);
				newBone._defaultMatrix._pos *= importScale;
				newBone._defaultMatrix._angleDeg = apUtil.AngleTo180(newBone._defaultMatrix._angleDeg);
				newBone._defaultMatrix.MakeMatrix();

				if (boneUnit._isShapeEnabled)
				{
					//Shape를 적용한다면..
					newBone._color = boneUnit._color;
					newBone._shapeWidth = (int)(boneUnit._shapeWidth * importScale + 0.5f);
					newBone._shapeLength = (int)(boneUnit._shapeLength * importScale + 0.5f);
					newBone._shapeTaper = boneUnit._shapeTaper;
				}

				if (boneUnit._isIKEnabled)
				{
					//IK를 적용한다면
					newBone._optionIK = boneUnit._optionIK;
					newBone._isIKTail = boneUnit._isIKTail;
					newBone._IKTargetBoneID = unitID2BoneID.ContainsKey(boneUnit._IKTargetBoneUnitID) ? unitID2BoneID[boneUnit._IKTargetBoneUnitID] : -1;
					newBone._IKNextChainedBoneID = unitID2BoneID.ContainsKey(boneUnit._IKNextChainedBoneUnitID) ? unitID2BoneID[boneUnit._IKNextChainedBoneUnitID] : -1;
					newBone._IKHeaderBoneID = unitID2BoneID.ContainsKey(boneUnit._IKHeaderBoneUnitID) ? unitID2BoneID[boneUnit._IKHeaderBoneUnitID] : -1;

					newBone._isIKAngleRange = boneUnit._isIKAngleRange;
					newBone._IKAngleRange_Lower = boneUnit._IKAngleRange_Lower;
					newBone._IKAngleRange_Upper = boneUnit._IKAngleRange_Upper;
					newBone._IKAnglePreferred = boneUnit._IKAnglePreferred;
				}

				newBone._isSocketEnabled = boneUnit._isSocketEnabled;

				//추가 21.3.7 : 지글본 정보도 있다면 가져오자
				if(boneUnit._isJigglePropertyImported)
				{
					newBone._isJiggle = boneUnit._isJiggle;
					newBone._jiggle_Mass = boneUnit._jiggle_Mass;
					newBone._jiggle_K = boneUnit._jiggle_K;
					newBone._jiggle_Drag = boneUnit._jiggle_Drag;
					newBone._jiggle_Damping = boneUnit._jiggle_Damping;
					newBone._isJiggleAngleConstraint = boneUnit._isJiggleAngleConstraint;
					newBone._jiggle_AngleLimit_Min = boneUnit._jiggle_AngleLimit_Min;
					newBone._jiggle_AngleLimit_Max = boneUnit._jiggle_AngleLimit_Max;
				}


				//일단 전체 리스트에 넣자
				targetMeshGroup._boneList_All.Add(newBone);

				addedBoneList.Add(newBone);
			}

			//추가되었던 Bone을 Link한다.
			for (int i = 0; i < addedBoneList.Count; i++)
			{
				apBone bone = addedBoneList[i];

				apBone parentBone = targetMeshGroup.GetBone(bone._parentBoneID);
				if (parentBone == null)
				{
					//Parent가 없으면 Root에 추가
					targetMeshGroup._boneList_Root.Add(bone);
				}

				bone.Link(targetMeshGroup, parentBone, Editor._portrait);
				bone.MakeWorldMatrix(false);
				bone.GUIUpdate();

			}

			//Bone IK를 갱신한다.
			RefreshBoneChaining(targetMeshGroup);

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(targetMeshGroup));

			targetMeshGroup.RefreshForce(true, 0.0f, null);

			//GUI를 업데이트한다.
			targetMeshGroup.UpdateBonesWorldMatrix();//<<변경
			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			{
				//targetMeshGroup._boneList_Root[i].MakeWorldMatrix(true);//이전
				targetMeshGroup._boneList_Root[i].GUIUpdate(true);
			}
			

			Editor.RefreshControllerAndHierarchy(false);

			Editor.Notification("Bones are loaded from file", true, false);

		}



		/// <summary>
		/// Retarget 파일로부터 "단일 포즈"를 Import한다.
		/// 대상은 Modifier
		/// </summary>
		public void ImportBonePoseFromRetargetSinglePoseFileToModifier(apMeshGroup targetMeshGroup, apRetarget retarget, apModifierBase targetModifier, apModifierParamSet paramSet,
																		apDialog_RetargetSinglePoseImport.IMPORT_METHOD importMethod)
		{
			if (Editor._portrait == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.MeshGroup != targetMeshGroup
				|| Editor.Select.Modifier != targetModifier
				|| Editor.Select.ParamSetOfMod != paramSet)
			{
				return;
			}


			//TODO.. 
			Dictionary<apBone, apRetargetBonePoseUnit> validBonePoseUnits = new Dictionary<apBone, apRetargetBonePoseUnit>();
			for (int i = 0; i < retarget.SinglePoseFile._bones.Count; i++)
			{
				apRetargetBonePoseUnit boneUnit = retarget.SinglePoseFile._bones[i];

				//동일한 Bone이 존재하는가
				//<BONE_EDIT>
				//apBone bone = targetMeshGroup.GetBone(boneUnit._uniqueID);

				//>>Bone Set 이용
				apBone bone = targetMeshGroup.GetBoneRecursive(boneUnit._uniqueID);


				if (bone != null)
				{
					validBonePoseUnits.Add(bone, boneUnit);
				}
			}
			if (validBonePoseUnits.Count == 0)
			{
				return;
			}

			bool isXAnyMirror = true;
			if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Mirror)
			{
				//Mirror인 경우
				//X 대칭인지, Y 대칭인지 확인해야한다.
				//Bone 리스트에서 Root 본(한개만) 찾고, 그 Root 본이 Y 대칭이면 Y 대칭으로 계산
				//그렇지 않다면 기본적으로 X 대칭이다.
				foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
				{
					apBone srcBone = posePair.Key;
					if (srcBone._parentBone == null && srcBone._mirrorBone != null)
					{
						if (srcBone._mirrorOption == apBone.MIRROR_OPTION.Y)
						{
							//Y대칭을 해야한다.
							isXAnyMirror = false;
							break;
						}
					}
				}
			}

			apEditorUtil.SetRecord_MeshGroupAndModifier(	apUndoGroupData.ACTION.Retarget_ImportSinglePoseToMod, 
															Editor, 
															targetMeshGroup, 
															targetModifier, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.StructChanged);

			//Dict
			foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
			{
				apBone srcBone = posePair.Key;
				apRetargetBonePoseUnit poseData = posePair.Value;

				apBone mirrorBone = srcBone._mirrorBone;

				apModifiedBone modBone = null;

				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal
					|| mirrorBone == null)
				{
					//1. 기본 방식으로 Import
					modBone = paramSet._boneData.Find(delegate (apModifiedBone a)
					{
						return a._bone == srcBone;
					});

					//잉.. ModBone이 없군요. 등록해드리겠습니다.
					if (modBone == null)
					{
						modBone = targetModifier.AddBone(srcBone, paramSet);

					}
				}
				else
				{
					modBone = paramSet._boneData.Find(delegate (apModifiedBone a)
					{
						return a._bone == mirrorBone;
					});

					//잉.. ModBone이 없군요. 등록해드리겠습니다.
					if (modBone == null)
					{
						modBone = targetModifier.AddBone(mirrorBone, paramSet);

					}
				}


				if (modBone == null)
				{
					//처리 후에도 안된다면 스킵
					continue;
				}

				//Matrix를 대입해주자
				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal)
				{
					//1. Normal
					modBone._transformMatrix.SetMatrix(poseData._localMatrix, true);
				}
				else
				{
					//2. Mirror
					bool isXInverse = true;
					if (mirrorBone == null)
					{
						isXInverse = isXAnyMirror;
					}
					else
					{
						isXInverse = (srcBone._parentBone != null || srcBone._mirrorOption == apBone.MIRROR_OPTION.X);
					}
					if (isXInverse)
					{
						modBone._transformMatrix.SetTRS(
								new Vector2(-poseData._localMatrix._pos.x, poseData._localMatrix._pos.y),//X 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale,
												true
											);
					}
					else
					{
						modBone._transformMatrix.SetTRS(
								new Vector2(poseData._localMatrix._pos.x, -poseData._localMatrix._pos.y),//Y 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale,
												true
											);
					}
				}
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			if (isChanged)
			{
				apUtil.LinkRefresh.Set_MeshGroup_Modifier(targetMeshGroup, targetModifier);

				Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			}
			Editor.Select.AutoSelectModMeshOrModBone();


			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();


			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					Editor.Select.MeshGroup,
			//					targetModifier,
			//					Editor.Select.SubEditedParamSetGroup,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			Editor.RefreshControllerAndHierarchy(false);

			Editor.Select.MeshGroup.RefreshForce(true);

			Editor.SetRepaint();

		}

		/// <summary>
		/// Retarget 파일로부터 "단일 포즈"를 Import한다.
		/// 대상은 AnimClip
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="retarget"></param>
		/// <param name="targetAnimClip"></param>
		/// <param name="targetFrame"></param>
		public void ImportBonePoseFromRetargetSinglePoseFileToAnimClip(apMeshGroup targetMeshGroup, apRetarget retarget, apAnimClip targetAnimClip, apAnimTimeline targetTimeline, int targetFrame,
																		apDialog_RetargetSinglePoseImport.IMPORT_METHOD importMethod)
		{
			if (Editor._portrait == null
				|| Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip._targetMeshGroup == null
				|| Editor.Select.AnimClip != targetAnimClip
				|| Editor.Select.AnimClip._targetMeshGroup != targetMeshGroup
				|| targetTimeline == null
				|| targetTimeline._linkedModifier == null
				|| !targetTimeline._linkedModifier.IsTarget_Bone)
			{
				return;
			}


			//TODO...>>>

			Dictionary<apBone, apRetargetBonePoseUnit> validBonePoseUnits = new Dictionary<apBone, apRetargetBonePoseUnit>();
			for (int i = 0; i < retarget.SinglePoseFile._bones.Count; i++)
			{
				apRetargetBonePoseUnit boneUnit = retarget.SinglePoseFile._bones[i];
				//동일한 Bone이 존재하는가
				//<BONE_EDIT>
				//apBone bone = targetMeshGroup.GetBone(boneUnit._uniqueID);

				//>> Recursive로 변경
				apBone bone = targetMeshGroup.GetBoneRecursive(boneUnit._uniqueID);
				if (bone != null)
				{
					validBonePoseUnits.Add(bone, boneUnit);
				}
			}

			if (validBonePoseUnits.Count == 0)
			{
				return;
			}

			bool isXAnyMirror = true;
			if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Mirror)
			{
				//Mirror인 경우
				//X 대칭인지, Y 대칭인지 확인해야한다.
				//Bone 리스트에서 Root 본(한개만) 찾고, 그 Root 본이 Y 대칭이면 Y 대칭으로 계산
				//그렇지 않다면 기본적으로 X 대칭이다.
				foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
				{
					apBone srcBone = posePair.Key;
					if (srcBone._parentBone == null && srcBone._mirrorBone != null)
					{
						if (srcBone._mirrorOption == apBone.MIRROR_OPTION.Y)
						{
							//Y대칭을 해야한다.
							isXAnyMirror = false;
							break;
						}
					}
				}
			}



			apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Retarget_ImportSinglePoseToAnim, 
																Editor, 
																Editor._portrait, 
																targetMeshGroup, 
																targetTimeline._linkedModifier, 
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.StructChanged);

			bool isAnyTimelinelayerCreated = false;

			bool isEditorOpt_VividColor = Editor._animUIOption_DefaultTimelineColor == apEditor.TIMELINE_DEFAULT_COLOR.Vivid;

			//Dictionary 돌면서 Timelinelayer 여부 체크하고 Keyframe을 체크한 뒤, ModBone을 넣어주자
			//두번 도는데, 일단 TimelineLayer를 추가하고 처리하자
			foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
			{
				apBone srcBone = posePair.Key;
				apRetargetBonePoseUnit poseData = posePair.Value;

				apBone mirrorBone = srcBone._mirrorBone;

				//1. Timelinelayer가 존재하는가
				apAnimTimelineLayer timelineLayer = null;
				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal
					|| mirrorBone == null)
				{
					timelineLayer = targetTimeline.GetTimelineLayer(srcBone);
				}
				else
				{
					timelineLayer = targetTimeline.GetTimelineLayer(mirrorBone);
				}

				if (timelineLayer == null)
				{
					//새로 만들자
					int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
					if (nextLayerID < 0)
					{
						//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Title),
														Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Body),
														Editor.GetText(TEXT.Close));
						return;
					}

					timelineLayer = new apAnimTimelineLayer();
					timelineLayer.Link(targetAnimClip, targetTimeline);

					if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal || mirrorBone == null)
					{
						timelineLayer.Init_Bone(	targetTimeline,
													nextLayerID,
													srcBone._uniqueID,
													srcBone,
													isEditorOpt_VividColor);
					}
					else
					{
						timelineLayer.Init_Bone(	targetTimeline,
													nextLayerID,
													mirrorBone._uniqueID,
													mirrorBone,
													isEditorOpt_VividColor);
					}

					targetTimeline._layers.Add(timelineLayer);

					isAnyTimelinelayerCreated = true;//<타임라인 레이어가 추가되었다.
				}



				//현재 프레임에 Keyframe이 있는가
				apAnimKeyframe keyframe = timelineLayer.GetKeyframeByFrameIndex(targetFrame);
				if (keyframe == null)
				{
					//키프레임이 없다. 추가해주자
					AddAnimKeyframe(targetFrame, timelineLayer, false, false, false, false);
				}
			}

			//Refresh 하자
			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));

			if (isAnyTimelinelayerCreated)
			{
				//Sync를 한번 해두자
				AddAndSyncAnimClipToModifier(Editor.Select.AnimClip);
			}


			apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip);

			targetMeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);
			targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

			targetMeshGroup._modifierStack.InitModifierCalculatedValues();

			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);


			


			//다시 돌자
			foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
			{
				apBone srcBone = posePair.Key;
				apRetargetBonePoseUnit poseData = posePair.Value;

				apBone mirrorBone = srcBone._mirrorBone;

				//로드할 본에 대해 타임라인 레이어를 찾는다 => 위에서 생성을 했다.
				//타임라인 레이어에 대해서 현재 프레임에 해당하는 키프레임이 있는가 => 없을 수 없다. 없으면 만들었을 테니까
				//붙여넣기할 키프레임에 대해 LinkedModBone_Editor가 있는가 => 에엥? (이건 에러 맞다.. 잘 못 판단함)
				apAnimTimelineLayer timelineLayer = null;

				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal || mirrorBone == null)
				{
					timelineLayer = targetTimeline.GetTimelineLayer(srcBone);
				}
				else
				{
					timelineLayer = targetTimeline.GetTimelineLayer(mirrorBone);
				}

				if (timelineLayer == null)
				{
					Debug.Log("AnyPortrait : Importing Pose | No TimelineLayer to load data.");//생성한 타임라인 레이어가 
					continue;
				}

				apAnimKeyframe keyframe = timelineLayer.GetKeyframeByFrameIndex(targetFrame);
				if (keyframe == null)
				{
					//키프레임이 없으면 안된다. 위 코드에서 생성했기 때문
					Debug.Log("AnyPortrait : Importing Pose | No Keyframe to load data.");
					continue;
				}

				if (keyframe._linkedModBone_Editor == null)
				{
					//
					Debug.Log("AnyPortrait : Importing Pose | No Bone Data to load data.");
					continue;
				}

				//Matrix를 대입해주자
				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal)
				{
					//1. Normal
					keyframe._linkedModBone_Editor._transformMatrix.SetMatrix(poseData._localMatrix, true);
				}
				else
				{
					//2. Mirror
					bool isXInverse = true;
					if (mirrorBone == null)
					{
						isXInverse = isXAnyMirror;
					}
					else
					{
						isXInverse = (srcBone._parentBone != null || srcBone._mirrorOption == apBone.MIRROR_OPTION.X);
					}

					if (isXInverse)
					{
						keyframe._linkedModBone_Editor._transformMatrix.SetTRS(
								new Vector2(-poseData._localMatrix._pos.x, poseData._localMatrix._pos.y),//X 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale,
												true
											);
					}
					else
					{
						keyframe._linkedModBone_Editor._transformMatrix.SetTRS(
								new Vector2(poseData._localMatrix._pos.x, -poseData._localMatrix._pos.y),//Y 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale,
												true
											);
					}
				}
			}

			//다시 Refresh
			targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));
			targetMeshGroup._modifierStack.InitModifierCalculatedValues();

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					targetMeshGroup,
			//					targetTimeline._linkedModifier,
			//					null,
			//					targetAnimClip,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);


			Editor.RefreshControllerAndHierarchy(true);//TimelineLayer도 갱신

			bool isWorkKeyframeChanged = false;
			Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
			if(Editor.Gizmos.IsFFDMode)
			{
				//여기서는 키프레임 변경 여부 상관없이 FFD를 강제로 취소한다.
				Editor.Gizmos.RevertFFDTransformForce();
			}
			
			//Refresh 추가
			//Editor.Select.RefreshAnimEditing(true);
			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15


			////추가 21.2.14 : Timeline과 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();

			//targetMeshGroup.RefreshForce(true);
			//targetAnimClip.Update_Editor(0.0f, true, true)
			Editor.SetRepaint();
		}



		//-----------------------------------------------------------------------------
		// 본의 스케일 옵션 변경
		//-----------------------------------------------------------------------------
		public void RefreshBoneScaleMethod_Editor()
		{
			//추가 20.8.14 : 루트 본의 스케일 옵션을 적용하고,
			//본의 모든 WorldMatrix를 갱신한다.
			if(Editor == null 
				|| Editor.Select == null
				|| Editor._portrait == null)
			{
				return;
			}

			//이 기능은 MeshGroup, Animation, RootUnit 메뉴에서 발휘된다.
			apMeshGroup targetMeshGroup = null;
			if(Editor.Select.SelectionType == apSelection.SELECTION_TYPE.Overall)
			{
				if(Editor.Select.RootUnit != null)
				{
					targetMeshGroup = Editor.Select.RootUnit._childMeshGroup;
				}
			}
			else if(Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				if(Editor.Select.MeshGroup != null)
				{
					targetMeshGroup = Editor.Select.MeshGroup;
				}
			}
			else if(Editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				if(Editor.Select.AnimClip != null && Editor.Select.AnimClip._targetMeshGroup != null)
				{
					targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
				}
			}
			if(targetMeshGroup == null)
			{
				return;
			}

			//BoneListSet을 돌면서 모든 본의 모드를 변경한다.
			apPortrait.ROOT_BONE_SCALE_METHOD nextBoneScaleMode = Editor._portrait._rootBoneScaleMethod;

			apMeshGroup.BoneListSet bls = null;
			apBone bone = null;
			for (int iBLS = 0; iBLS < targetMeshGroup._boneListSets.Count; iBLS++)
			{
				bls = targetMeshGroup._boneListSets[iBLS];

				for (int iBone = 0; iBone < bls._bones_All.Count; iBone++)
				{
					bone = bls._bones_All[iBone];
					//다음의 WorldMatrix들의 모드를 변경한다.
					//bone._worldMatrix;
					//bone._worldMatrix_IK;
					//bone._worldMatrix_NonModified;
					bone.SetWorldMatrixScaleMode(nextBoneScaleMode);
				}
			}
			
			
		}


		//-----------------------------------------------------------------------------
		// 본 리깅
		//-----------------------------------------------------------------------------
		/// <summary>
		/// 현재 선택한 ModMesh의 ModVertRig의 Weight 리스트에 "현재 선택한 Bone"을 선택한 Weight와 함께 추가한다.
		/// 만약 선택한 ModVertRig(1개 이상)가 없고 Bone을 선택하지 않았다면 패스한다.
		/// Bone이 등록되지 않았다면 자동으로 등록하며, AutoNormalize가 되어있다면 같이 수행한다.
		/// </summary>
		/// <param name="calculateType">연산 타입. 0 : 대입, 1 : 더하기, 2 : 곱하기</param>
		/// <param name="weight"></param>
		/// <param name="isSetOtherRigValue0or1">이 값이 true이면 0이나 1일 때에 Normalize를 하면 다른 RigData에 값을 할당한다. (이전에는 안되며 false가 기본값)</param>
		public void SetBoneWeight(float weight, int calculateType, bool isRecord = true, bool isSetOtherRigValue0or1 = false)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			if (isRecord)
			{
				//Undo - 연속 입력 가능
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetBoneWeight, 
													Editor, 
													Editor.Select.Modifier, 
													//null, 
													true,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			bool isAutoNormalize = Editor.Select._rigEdit_isAutoNormalize;

			apModifiedVertexRig vertRig = null;
			apModifiedVertexRig.WeightPair targetWeightPair = null;

			bool isAnyRigDataAdded = false;

			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];



				//Bone이 있는가?
				targetWeightPair = null;
				for (int iPair = 0; iPair < vertRig._weightPairs.Count; iPair++)
				{
					if (vertRig._weightPairs[iPair]._bone == bone)
					{
						targetWeightPair = vertRig._weightPairs[iPair];
						break;
					}
				}
				//없으면 추가
				if (targetWeightPair == null)
				{
					targetWeightPair = new apModifiedVertexRig.WeightPair(bone);
					vertRig._weightPairs.Add(targetWeightPair);
					
					isAnyRigDataAdded = true;
				}
				switch (calculateType)
				{
					case 0://대입
						targetWeightPair._weight = weight;
						break;

					case 1://더하기
						targetWeightPair._weight += weight;
						break;

					case 2://곱하기
						targetWeightPair._weight *= weight;
						break;
				}
				//if (isMultiply)
				//{
				//	targetWeightPair._weight *= weight;
				//}
				//else
				//{
				//	targetWeightPair._weight = weight;
				//}

				vertRig.CalculateTotalWeight();

				if (isAutoNormalize)
				{
					//Normalize를 하자
					vertRig.NormalizeExceptPair(targetWeightPair, isSetOtherRigValue0or1);
				}
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);

			//RigData 추가/삭제시 호출할 것
			if(isAnyRigDataAdded)
			{
				Editor.Select.CheckLinkedToModMeshBones(true);
			}
		}


		/// <summary>
		/// Bone Weight를 Normalize한다.
		/// </summary>
		public void SetBoneWeightNormalize()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				//|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetBoneWeight, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];

				vertRig.Normalize();
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}




		/// <summary>
		/// Bone Weight를 Prune한다.
		/// </summary>
		public void SetBoneWeightPrune()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetBoneWeight, 
												Editor, 
												Editor.Select.Modifier, 
												//false, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apModifiedVertexRig vertRig = null;
			bool isAnyRigDataRemoved = false;

			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];

				bool isRemoved = vertRig.Prune();
				if(isRemoved)
				{
					isAnyRigDataRemoved = true;
				}
			}


			//RigData 추가/삭제시 호출할 것
			if(isAnyRigDataRemoved)
			{
				Editor.Select.CheckLinkedToModMeshBones(true);
			}


			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}



		/// <summary>
		/// Bone Weight를 Blend한다.
		/// Blend시 주의점
		/// Blend는 내부의 Weight가 아니라 "주변의 Weight를 비교"하여 Blend한다.
		/// Mesh를 참조하여 "연결된 Vertex"를 가지는 "VertRigs"를 모두 검색한 뒤,
		/// "주변 Weight의 평균값" 10% + "자신의 Weight" + 90%를 적용한다.
		/// 연산 결과가 요청된 다른 Vertex에 영향을 주지 않도록 결과를 따로 저장했다가 일괄 적용한다.
		/// </summary>
		public void SetBoneWeightBlend()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				
				//|| Editor.Select.ModMeshOfMod == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.17

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			//apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;//이전
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;//변경 20.6.27

			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}


			bool isAnyRigDataRemoved = false;

			//Undo
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetBoneWeight, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apBone curSelectedBone = Editor.Select.Bone;

			//요청된 VertRig와 연결된 외부의 Weight 평균값
			Dictionary<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>> linkedWeightAvgs = new Dictionary<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>>();

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				//각 Vertex별로 검사 시작
				vertRig = vertRigs[iVertRig];

				List<apModifiedVertexRig.WeightPair> curWeightPairs = vertRig._weightPairs;

				//연결된 Vertex
				List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, true);

				if (linkedVerts == null)
				{
					continue;
				}

				//연결된 VerRigs를 가져오자
				List<apModifiedVertexRig> linkedVertRigs = modMesh._vertRigs.FindAll(delegate (apModifiedVertexRig a)
				{
					return a != vertRig && linkedVerts.Contains(a._vertex);
				});

				if (linkedVertRigs.Count == 0)
				{
					continue;
				}


				//평균값을 내자
				//Bone+Weight 조합으로 다 더한 뒤
				//연결된 VertRig 개수만큼 나누자
				List<apModifiedVertexRig.WeightPair> weightPairList = new List<apModifiedVertexRig.WeightPair>();
				int nLinkedVertRigs = linkedVertRigs.Count;
				for (int iLink = 0; iLink < nLinkedVertRigs; iLink++)
				{
					apModifiedVertexRig linkedVertRig = linkedVertRigs[iLink];
					for (int iWP = 0; iWP < linkedVertRig._weightPairs.Count; iWP++)
					{
						apModifiedVertexRig.WeightPair linkedWeightPair = linkedVertRig._weightPairs[iWP];
						
						bool isExistBoneInTable = curWeightPairs.Exists(delegate (apModifiedVertexRig.WeightPair a)
						{
							return a._bone == linkedWeightPair._bone;
						});

						bool isSelectedBone = curSelectedBone != null && curSelectedBone == linkedWeightPair._bone;

						if (!isExistBoneInTable && !isSelectedBone)
						{
							//테이블에 Bone이 없다면 Blend 타겟에 넣지 않는다.
							//+ 선택된 Bone도 아니라면
							continue;
						}

						////추가 19.8.25 : 선택되지 본의 Lock 설정이 있다면, 값은 "현재의 값"으로 대체해야한다.
						//bool isLock = linkedWeightPair._bone._isRigLock && linkedWeightPair._bone != curSelectedBone;

						apModifiedVertexRig.WeightPair existPair = weightPairList.Find(delegate (apModifiedVertexRig.WeightPair a)
						{
							return a._bone == linkedWeightPair._bone;
						});

						if (existPair != null)
						{
							//이미 존재하는 Bone이다.
							//Weight만 추가하자.
							existPair._weight += linkedWeightPair._weight;
						}
						else
						{
							//등록되지 않은 Bone이다.
							//새로 추가하자
							apModifiedVertexRig.WeightPair newPair = new apModifiedVertexRig.WeightPair(linkedWeightPair._bone);
							newPair._weight = linkedWeightPair._weight;
							weightPairList.Add(newPair);

							isAnyRigDataRemoved = true;//<<새로운 본이 추가되었으므로
						}
					}
				}
				//평균값을 내자
				for (int iWP = 0; iWP < weightPairList.Count; iWP++)
				{
					weightPairList[iWP]._weight /= nLinkedVertRigs;
				}

				//연산 결과에 등록[요청 VertRig + 주변의 Rig 데이터]
				linkedWeightAvgs.Add(vertRig, weightPairList);
			}

			//값을 넣어주자
			//비율은 20% + 80%
			float ratio_Src = 0.8f;
			float ratio_Link = 0.2f;

			List<apModifiedVertexRig> modVertRigList = new List<apModifiedVertexRig>();
			Dictionary<apModifiedVertexRig, float> modVertRig_2_PrevTotalWeight = new Dictionary<apModifiedVertexRig, float>();
			Dictionary<apModifiedVertexRig, float> modVertRig_2_LockedWeight = new Dictionary<apModifiedVertexRig, float>();
			Dictionary<apModifiedVertexRig, bool> modVertRig_2_IsAnyLocked = new Dictionary<apModifiedVertexRig, bool>();
			

			foreach (KeyValuePair<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>> rigPair in linkedWeightAvgs)
			{
				apModifiedVertexRig targetVertRig = rigPair.Key;
				List<apModifiedVertexRig.WeightPair> linkedWeightPairs = rigPair.Value;

				//Total Weight를 계산하자 + 나중을 위해 Pair로 기록.
				targetVertRig.CalculateTotalWeight();

				float prevTotalWeight = 0.0f;
				float prevLockedWeight = 0.0f;
				bool isAnyLocked = false;
				for (int iWP = 0; iWP < targetVertRig._weightPairs.Count; iWP++)
				{
					apModifiedVertexRig.WeightPair weightPair = targetVertRig._weightPairs[iWP];
					prevTotalWeight += weightPair._weight;
					if(weightPair._bone._isRigLock && weightPair._bone != curSelectedBone)
					{
						//RigLock이 걸린 상태인가?
						isAnyLocked = true;
						prevLockedWeight += weightPair._weight;
					}
				}

				//변경 19.8.27 : 이건 아래의 보정 코드에서 체크하는 걸로
				//float prevTotalWeight = targetVertRig._totalWeight;
				//if (Editor.Select._rigEdit_isAutoNormalize || Mathf.Approximately(prevTotalWeight, 1.0f))
				//{
				//	//Prev Weight가 되어야 하는 값 (Auto Normalize 옵션이거나 1 근처일 때)
				//	prevTotalWeight = 1.0f;
				//}

				//List/Dictionary에 필요한 값을 추가하자.
				if(!modVertRigList.Contains(targetVertRig))
				{
					modVertRigList.Add(targetVertRig);
				}

				if (!modVertRig_2_PrevTotalWeight.ContainsKey(targetVertRig))
				{
					modVertRig_2_PrevTotalWeight.Add(targetVertRig, prevTotalWeight);
				}

				if(!modVertRig_2_LockedWeight.ContainsKey(targetVertRig))
				{
					modVertRig_2_LockedWeight.Add(targetVertRig, prevLockedWeight);
				}

				if(!modVertRig_2_IsAnyLocked.ContainsKey(targetVertRig))
				{
					modVertRig_2_IsAnyLocked.Add(targetVertRig, isAnyLocked);
				}




				//1) Bone이 없다면 추가해준다.
				//2) targetVertRig 기준으로 : 80% 20% 비율로 계산

				//3) 추가 : Lock걸린 경우 제외하고 변경해야한다.

				for (int i = 0; i < linkedWeightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair linkedPair = linkedWeightPairs[i];

					if (!targetVertRig._weightPairs.Exists(delegate (apModifiedVertexRig.WeightPair a)
					 {
						 return a._bone == linkedPair._bone;
					 }))
					{
						//새로 추가해준다.
						apModifiedVertexRig.WeightPair newPair = new apModifiedVertexRig.WeightPair(linkedPair._bone);
						newPair._weight = 0.0f;
						targetVertRig._weightPairs.Add(newPair);

						isAnyRigDataRemoved = true;//새로운 본 추가
					}
				}

				for (int i = 0; i < targetVertRig._weightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair targetWeight = targetVertRig._weightPairs[i];
					apModifiedVertexRig.WeightPair linkedWeight = linkedWeightPairs.Find(delegate (apModifiedVertexRig.WeightPair a)
					{
						return a._bone == targetWeight._bone;
					});

					//추가 19.8.25 : 만약 Lock 상태이며 현재 선택된 본이 아니라면 기존의 값을 유지한다.
					bool isRigLock = (targetWeight._bone._isRigLock && targetWeight._bone != curSelectedBone);

					if (!isRigLock)
					{
						if (linkedWeight != null)
						{
							targetWeight._weight = targetWeight._weight * ratio_Src + linkedWeight._weight * ratio_Link;
						}
						else
						{
							targetWeight._weight = targetWeight._weight * ratio_Src;
						}
					}
				}
			}

			//Blend 이후에 Weight가 바뀌는 버그가 있다.
			//Weight값을 보정해주자
			//참고 : 보정일 때는 RigLock이 적용이 안된다.
			//foreach (KeyValuePair<apModifiedVertexRig, float> prevWeightPair in modVertRig_2_PrevTotalWeight)
			for (int iModVert = 0; iModVert < modVertRigList.Count; iModVert++)
			{
				apModifiedVertexRig modVertRig = modVertRigList[iModVert];
				float prevTotalWeight = modVertRig_2_PrevTotalWeight[modVertRig];
				float prevLockedWeight = modVertRig_2_LockedWeight[modVertRig];
				bool isAnyLocked = modVertRig_2_IsAnyLocked[modVertRig];
				//apModifiedVertexRig modRig = prevWeightPair.Key;
				//float prevTotalWeight = prevWeightPair.Value;

				if (Editor.Select._rigEdit_isAutoNormalize || Mathf.Approximately(prevTotalWeight, 1.0f))
				{
					//TotalWeight를 1로 만들어야 한다.
					float correctRatio = 0.0f;
					if(prevTotalWeight > 0.0f)
					{
						correctRatio = 1.0f / prevTotalWeight;
					}
					else
					{
						correctRatio = 1.0f;
					}
					prevTotalWeight = 1.0f;
					prevLockedWeight *= correctRatio;
				}

				//Total Weight를 다시 계산하자
				modVertRig.CalculateTotalWeight();
				
				float nextTotalWeight = modVertRig._totalWeight;
				float nextLockedWeight = 0.0f;
				


				//변경 19.8.27
				//1) RigLock이 없다면 > 그냥 바로 보정
				//2) RigLock이 하나라도 있다면 > Unlocked Weight만 계산해서 PrevUnlockedWeight를 만들어야 한다.
				//단, Unlocked가 0이거나 음수라면, 1)의 경우로 계산한다.

				bool isCheckOnlyUnlocked = false;//이게 False면 1), True면 2)로 계산한다.
				float prevUnlockedWeight = prevTotalWeight - prevLockedWeight;
				float nextUnlockedWeight = 0.0f;

				if (isAnyLocked)
				{
					for (int iWP = 0; iWP < modVertRig._weightPairs.Count; iWP++)
					{
						apModifiedVertexRig.WeightPair weightPair = modVertRig._weightPairs[iWP];
						if (weightPair._bone._isRigLock && weightPair._bone != curSelectedBone)
						{
							//RigLock이 걸린 상태인가?
							nextLockedWeight += weightPair._weight;
						}
					}
					nextUnlockedWeight = nextTotalWeight - nextLockedWeight;
					if(nextUnlockedWeight > 0.0f && prevUnlockedWeight > 0.0f)
					{
						//Unlocked만 체크해봅시다.
						isCheckOnlyUnlocked = true;
					}
				}
				if (!isCheckOnlyUnlocked)
				{
					//1) 모든 Weight에 대하여 보정 (totalWeight가 0 이상일때)
					if (!Mathf.Approximately(prevTotalWeight, nextTotalWeight) && nextTotalWeight > 0.0f)
					{
						//Debug.LogError("Bone Weight Blend 후 Weight 변경 : [" + prevTotalWeight + "] > [" + nextTotalWeight + "]");

						//다시 Prev에 맞추어야 한다.
						float correctToPrevRatio = prevTotalWeight / nextTotalWeight;

						apModifiedVertexRig.WeightPair curPair = null;
						for (int i = 0; i < modVertRig._weightPairs.Count; i++)
						{
							curPair = modVertRig._weightPairs[i];
							curPair._weight *= correctToPrevRatio;
						}

						//마지막 계산
						modVertRig.CalculateTotalWeight();

						//Debug.Log("보정 후 가중치 : " + modRig._totalWeight);
					}
				}
				else
				{
					//2) Unlocked에 대하여만 보정
					float correctToPrevRatio = prevUnlockedWeight / nextUnlockedWeight;

					apModifiedVertexRig.WeightPair curPair = null;
					for (int i = 0; i < modVertRig._weightPairs.Count; i++)
					{
						curPair = modVertRig._weightPairs[i];
						if (!curPair._bone._isRigLock || curPair._bone == curSelectedBone)
						{
							//RigLock이 걸리지 않은 경우 (조건문 반대임)
							curPair._weight *= correctToPrevRatio;
						}
					}

					modVertRig.CalculateTotalWeight();
				}
			}



			//RigData 추가/삭제시 호출할 것
			if(isAnyRigDataRemoved)
			{
				Editor.Select.CheckLinkedToModMeshBones(true);
			}


			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}



		/// <summary>
		/// 선택한 Vertex Rig에 대해서 Grow 또는 Select 선택을 한다.
		/// </summary>
		/// <param name="isGrow"></param>
		public void SelectVertexRigGrowOrShrink(bool isGrow)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null

				//|| Editor.Select.ModMeshOfMod == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.27

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			//apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;//이전
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;//변경 20.6.17

			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//apEditorUtil.SetRecord("Select Vertex Rig Grow Or Shrink", Editor._portrait);


			apModifiedVertexRig vertRig = null;
			if (isGrow)
			{
				//Grow인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져오고, 기존의 리스트에 없는 Vertex를 선택해준다.
				List<apModifiedVertexRig> addVertRigs = new List<apModifiedVertexRig>();
				
				for (int iVR = 0; iVR < vertRigs.Count; iVR++)
				{
					vertRig = vertRigs[iVR];

					List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, false);
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertRigs.Exists(delegate (apModifiedVertexRig a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (isExist)
						{
							//하나라도 이미 선택된 거네요.
							//패스
							continue;
						}

						apVertex addVertex = linkedVerts[iVert];
						//하나도 선택되지 않은 Vertex라면
						//새로 추가해주자
						apModifiedVertexRig addVertRig = modMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
						{
							return a._vertex == addVertex;
						});
						if (addVertRig != null)
						{
							if (!addVertRigs.Contains(addVertRig))
							{
								addVertRigs.Add(addVertRig);
							}
						}
					}
				}
				//일괄적으로 추가해주자
				//이전
				//for (int i = 0; i < addVertRigs.Count; i++)
				//{
				//	Editor.Select.AddModVertexOfModifier(null, addVertRigs[i], null, addVertRigs[i]._renderVertex);
				//}

				//변경 20.6.26 : 향상된 ModVert 선택하기
				Editor.Select.AddModVertRigsOfModifier(addVertRigs);
			}
			else
			{
				//Shirink인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져온다.
				//1) Linked Vert가 없으면 : 삭제 리스트에 넣는다.
				//2) Linked Vert 중에서 "지금 선택 중인 Vert"에 해당하지 않는 Vert가 하나라도 있으면 : 삭제 리스트에 넣는다.
				//3) 모든 Linked Vert가 "지금 선택중인 Vert"에 해당된다면 유지
				List<apModifiedVertexRig> removeVertRigs = new List<apModifiedVertexRig>();
				for (int iVR = 0; iVR < vertRigs.Count; iVR++)
				{
					vertRig = vertRigs[iVR];

					List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, false);
					if (linkedVerts == null || linkedVerts.Count == 0)
					{
						//1) 연결된게 없으면 삭제
						removeVertRigs.Add(vertRig);
						continue;
					}

					//모든 Vertex가 현재 선택된 상태인지 확인하자
					bool isAllSelected = true;
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertRigs.Exists(delegate (apModifiedVertexRig a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (!isExist)
						{
							//선택되지 않은 Vertex를 발견!
							isAllSelected = false;
							break;
						}
					}
					if (!isAllSelected)
					{
						//2) 하나라도 선택되지 않은 Link Vertex가 발견되었다면 이건 삭제 대상이다.
						if (!removeVertRigs.Contains(vertRig))
						{
							removeVertRigs.Add(vertRig);
						}
					}
					else
					{
						//추가)
						//만약 외곽선에 위치한 Vertex라면 우선순위에 포함된다.
						bool isOutlineVertex = vertRig._mesh.IsOutlineVertex(vertRig._vertex);
						if (isOutlineVertex)
						{
							if (!removeVertRigs.Contains(vertRig))
							{
								removeVertRigs.Add(vertRig);
							}
						}
					}
				}

				if (removeVertRigs.Count > 0)
				{
					//이전
					////하나씩 삭제하자
					//for (int i = 0; i < removeVertRigs.Count; i++)
					//{
					//	vertRig = removeVertRigs[i];
					//	Editor.Select.RemoveModVertexOfModifier(null, vertRig, null, vertRig._renderVertex);
					//}

					//변경 20.6.26 : 향상된 "선택된 ModVertRig 해제하기"
					Editor.Select.RemoveModVertRigs(removeVertRigs);

				}

			}

		}



		public void SelectVerticesOfTheBone()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				
				//|| Editor.Select.ModMeshOfMod == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.27
				
				|| Editor.Select.Bone == null)
			{
				return;
			}

			apBone targetBone = Editor.Select.Bone;
			
			//apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;//이전
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;//변경 20.6.17

			if(modMesh._vertRigs == null || modMesh._vertRigs.Count == 0)
			{
				return;
			}

			bool isCtrl = false;
#if UNITY_EDITOR_OSX
			isCtrl = Event.current.command;
#else
			isCtrl = Event.current.control;
#endif
			if(!isCtrl)
			{
				//선택하기 전에 기존 버텍스 선택 취소
				//Editor.Select.SetModVertexOfModifier(null, null, null, null);
				Editor.Select.SelectModRenderVertOfModifier(null);//변경 20.6.28 : 변경된 MRV
			}

			List<apModifiedVertexRig> addVertRigs = new List<apModifiedVertexRig>();

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < modMesh._vertRigs.Count; iVertRig++)
			{
				vertRig = modMesh._vertRigs[iVertRig];
				if(vertRig == null)
				{
					continue;
				}
				if(vertRig._weightPairs.Exists(delegate(apModifiedVertexRig.WeightPair a)
				{
					return a._bone == targetBone;
				}))
				{
					//선택된 본과 연결되어 있다면
					addVertRigs.Add(vertRig);
				}
			}

			//일괄적으로 추가해주자
			if (addVertRigs.Count > 0)
			{
				//이전
				//for (int i = 0; i < addVertRigs.Count; i++)
				//{
				//	Editor.Select.AddModVertexOfModifier(null, addVertRigs[i], null, addVertRigs[i]._renderVertex);
				//}

				//변경 20.6.26 : 향상된 ModVertRig 선택하기
				Editor.Select.AddModVertRigsOfModifier(addVertRigs);
			}
		}



		public void SetBoneAutoRig(List<apBone> targetBoneList = null)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				
				//|| Editor.Select.ModMeshOfMod == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.17
				
				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//1. 대상이 되는 VertRig를 리스트로 만든다.
			//apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;//이전
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;//변경 20.6.17

			//No-Mod World Matrix를 계산하고 적용하기
			modMesh._renderUnit.CalculateRenverVertsWorldPosNoMod(apRenderUnit.CALCULATE_NO_MOD_POS_REQUEST.RefreshWorldMatrix_Single);

			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apModifiedVertexRig> vertRigs_All = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
			

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					if(!vertRigs.Contains(modRenderVert._modVertRig))
					{
						vertRigs.Add(modRenderVert._modVertRig);
						vertRigs_All.Add(modRenderVert._modVertRig);
					}
				}
			}

			if (vertRigs.Count == 0)
			{
				return;
			}

			//전체 VertRig도 가져오자
			apModifiedVertexRig curVertRig = null;
			for (int i = 0; i < modMesh._vertRigs.Count; i++)
			{
				curVertRig = modMesh._vertRigs[i];
				if (!vertRigs_All.Contains(curVertRig))
				{
					vertRigs_All.Add(curVertRig);
				}
			}


			//전체 본을 대상으로 계산
			List<apBone> bones = meshGroup._boneList_All;

			//추가 19.12.30 : 지정된 본 리스트가 있다면 그걸 사용
			if(targetBoneList != null && targetBoneList.Count > 0)
			{
				bones = targetBoneList;
			}
			

			//Undo
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetBoneWeight, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apModifiedVertexRig vertRig = null;

			//2. 버텍스마다 "가장 가까운 Bone"을 찾자.
			//- 데이터는 Dictionary<Bone + Vert List> 형식으로 저장
			//- 만약 이미 Rig 데이터가 있다면, 가장 큰 값의 Bone을 선택한다. (여러개일 수 있음)

			Dictionary<apBone, apAutoRigUtil.EnvelopeInfo> bone2Envelope = new Dictionary<apBone, apAutoRigUtil.EnvelopeInfo>();
			Dictionary<apModifiedVertexRig,apAutoRigUtil.EnvelopeInfo> vertRig2NearestEnvelope = new Dictionary<apModifiedVertexRig, apAutoRigUtil.EnvelopeInfo>();
			Vector2 vertPos = Vector2.zero;
			apBone curBone = null;
			apBone nearestBone = null;
			apModifiedVertexRig.WeightPair curWeightPair = null;

			
			//for (int iVR = 0; iVR < vertRigs.Count; iVR++)
			for (int iVR = 0; iVR < vertRigs_All.Count; iVR++)
			{	
				vertRig = vertRigs_All[iVR];
				vertPos = vertRig._renderVertex._pos_World_NoMod;

				bool isSelectedVertRig = vertRigs.Contains(vertRig);

				curBone = null;
				nearestBone = null;
				
				if(vertRig._weightPairs != null && vertRig._weightPairs.Count > 0)
				{
					//등록된 Bone이 있다면, 일단 여기서 먼저 체크한다.
					float maxWeight = 0.0f;
					for (int iPair = 0; iPair < vertRig._weightPairs.Count; iPair++)
					{
						 curWeightPair = vertRig._weightPairs[iPair];
						if(curWeightPair == null || curWeightPair._bone == null)
						{
							continue;
						}

						if(curWeightPair._weight > 0.0f)
						{
							if(curWeightPair._weight > maxWeight || nearestBone == null)
							{
								//가장 Weight가 큰 것을 선택한다.
								nearestBone = curWeightPair._bone;
								maxWeight = curWeightPair._weight;
							}
						}
					}
				}

				if(nearestBone == null)
				{
					//등록된 Bone이 없거나 유효한 Weight의 Bone이 없었다면
					//> 전체 검색
					float minDist = 0.0f;
					float dst = 0.0f;
					for (int iBone = 0; iBone < bones.Count; iBone++)
					{
						curBone = bones[iBone];
						dst = apAutoRigUtil.GetDistanceToBone(vertPos, curBone);
						if(dst < minDist || nearestBone == null)
						{
							//가장 가까운 Bone
							minDist = dst;
							nearestBone = curBone;
						}
					}
				}

				if(nearestBone == null)
				{
					//가장 가까운 Bone이 없다 > 에러
					continue;
				}

				//Bone > Vert 매핑 데이터에 넣자
				apAutoRigUtil.EnvelopeInfo nearestEnvelope = null;

				if(!bone2Envelope.ContainsKey(nearestBone))
				{
					nearestEnvelope = new apAutoRigUtil.EnvelopeInfo(nearestBone);
					bone2Envelope.Add(nearestBone, nearestEnvelope);
				}
				else
				{
					nearestEnvelope = bone2Envelope[nearestBone];
				}
				nearestEnvelope.AddNearVertRig(vertRig, !isSelectedVertRig);
				vertRig2NearestEnvelope.Add(vertRig, nearestEnvelope);
			}

			//3. Bone마다 가장 멀리 있는 Vertex를 기준으로 Envelope Size를 구한다.
			foreach (KeyValuePair<apBone, apAutoRigUtil.EnvelopeInfo> envelopePair in bone2Envelope)
			{
				envelopePair.Value.CalculateSize();
			}

			//3-2. Envelope가 생성되지 못한 Bone에 대해서도 Envelope 크기를 만든다.
			//(1) 인접한 본 (Parent 또는 Child) 중에 Envelope가 만들어진 본이 있다면, 그 크기의 70%를 크기로 이용한다.
			//(2) 그렇지 않다면, 본 길이 의 20%를 크기로 이용한다.
			//(3) 헬퍼인 경우 아주 작은 0.1의 값을 가진다.
			//(4) 실제 크기는 (1), (2), (3) 중에서 가장 큰 값을 이용한다.
			List<apAutoRigUtil.EnvelopeInfo> otherEnvelopes = new List<apAutoRigUtil.EnvelopeInfo>();
			for (int iBone = 0; iBone < bones.Count; iBone++)
			{
				curBone = bones[iBone];
				if(bone2Envelope.ContainsKey(curBone))
				{
					//이미 등록된 본이면 생략
					continue;
				}

				//함수를 이용하여 EnvelopeInfo를 만든다.
				apAutoRigUtil.EnvelopeInfo newEnvInfo = apAutoRigUtil.MakeEnvelopeWithoutVertRigs(curBone, bone2Envelope);
				otherEnvelopes.Add(newEnvInfo);
			}

			//이제 이걸 모두 리스트에 합친다.
			for (int i = 0; i < otherEnvelopes.Count; i++)
			{
				apAutoRigUtil.EnvelopeInfo envInfo = otherEnvelopes[i];
				bone2Envelope.Add(envInfo._bone, envInfo);
			}

			//Debug.Log("----------------------------------------");
			//연결된 본과 EnvelopeInfo 단위로 연결해야한다.
			foreach (KeyValuePair<apBone, apAutoRigUtil.EnvelopeInfo> envelopePair in bone2Envelope)
			{
				envelopePair.Value.LinkEnvelopeInfo(bone2Envelope);
			}
			//Debug.Log("----------------------------------------");
			//4. Envelope > VertRig를 모두 검사하여 Weight를 계산한다.
			//- VertRig의 Weight를 모두 초기화
			//- VertRig를 기준으로 Envelope를 모두 검사하여 Dist 기준으로 Weight를 넣는다.
			//- Normalize를 한다.

			for (int iVR = 0; iVR < vertRigs.Count; iVR++)
			{
				vertRig = vertRigs[iVR];
				//Debug.Log("Vert Rig [" + iVR + "]--------");

				vertPos = vertRig._renderVertex._pos_World_NoMod;

				if(vertRig._weightPairs == null)
				{
					vertRig._weightPairs = new List<apModifiedVertexRig.WeightPair>();
				}
				vertRig._weightPairs.Clear();//<<아예 초기화

				//원래 가장 가까웠던 EnvInfo를 찾자
				apAutoRigUtil.EnvelopeInfo nearestEnvInfo = null;
				if(vertRig2NearestEnvelope.ContainsKey(vertRig))
				{
					nearestEnvInfo = vertRig2NearestEnvelope[vertRig];
				}

				//Envelope를 모두 검사하여 Bone+Weight를 추가한다.
				apAutoRigUtil.EnvelopeInfo envInfo = null;
				foreach (KeyValuePair<apBone, apAutoRigUtil.EnvelopeInfo> envelopePair in bone2Envelope)
				{
					envInfo = envelopePair.Value;

					
					
					float weight = apAutoRigUtil.GetVertRigWeight(vertRig, envInfo, nearestEnvInfo);
					if(weight < 0.001f)
					{
						continue;
					}

					//WeightPair 값을 만들어서 넣자
					apModifiedVertexRig.WeightPair newWeightPair = new apModifiedVertexRig.WeightPair(envInfo._bone);
					newWeightPair._weight = weight;

					vertRig._weightPairs.Add(newWeightPair);
				}
				
				//Normalize
				vertRig.Normalize();

				//Debug.Log(">>");
			}


			//RigData 추가/삭제시 호출할 것
			Editor.Select.CheckLinkedToModMeshBones(true);
			
		}


		/// <summary>
		/// 추가 19.7.27 : Rigging 편집을 위한 Lock을 모두 해제한다.
		/// 메시 그룹을 선택했거나, Rigging Binding 편집을 켜거나 끌 때 호출하자.
		/// </summary>
		public void ResetBoneRigLock(apMeshGroup meshGroup)
		{
			if(meshGroup == null)
			{
				return;
			}

			if(meshGroup._boneList_All == null || meshGroup._boneList_All.Count == 0)
			{
				return;
			}

			apBone bone = null;
			for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
			{
				bone = meshGroup._boneList_All[iBone];
				bone._isRigLock = false;
			}

			
			//자식 메시그룹에 대해서도
			if(meshGroup._childMeshGroupTransforms != null && meshGroup._childMeshGroupTransforms.Count > 0)
			{
				apMeshGroup childMeshGroup = null;
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					childMeshGroup = meshGroup._childMeshGroupTransforms[iChild]._meshGroup;
					if(childMeshGroup != meshGroup)
					{
						ResetBoneRigLock(childMeshGroup);
					}
				}
			}
		}


		public void RemoveVertRigData(List<apSelection.ModRenderVert> selectedVerts, apBone targetBone)
		{
			//Undo
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_RemoveBoneWeight, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			bool isAutoNormalize = Editor.Select._rigEdit_isAutoNormalize;

			for (int iVert = 0; iVert < selectedVerts.Count; iVert++)
			{
				apSelection.ModRenderVert modRenderVert = selectedVerts[iVert];
				if (modRenderVert._modVertRig != null)
				{
					modRenderVert._modVertRig._weightPairs.RemoveAll(delegate (apModifiedVertexRig.WeightPair a)
					{
						return a._bone == targetBone;
					});

					if (isAutoNormalize)
					{
						modRenderVert._modVertRig.Normalize();
					}

					modRenderVert._modVertRig.CalculateTotalWeight();
				}
			}

			//RigData 추가/삭제시 호출할 것
			Editor.Select.CheckLinkedToModMeshBones(true);

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}




		//Rigging과 유사하게 Physic/Volume Weight도 처리하자
		/// <summary>
		/// Physic / Volume Modifier의 Vertex Weight를 지정한다.
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="calculateType">연산 타입. 0 : 대입, 1 : 더하기, 2 : 곱하기</param>
		public void SetPhyVolWeight(float weight, int calculateType)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				
				//|| Editor.Select.ModMeshOfMod == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.17

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				//Debug.LogError("Failed..");
				return;
			}

			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			//Undo - 연속 입력 가능
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetPhysicsWeight, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);


			for (int i = 0; i < vertWeights.Count; i++)
			{
				float curWeight = vertWeights[i]._weight;
				switch (calculateType)
				{
					case 0://대입
						curWeight = weight;
						break;

					case 1://더하기
						curWeight += weight;
						break;

					case 2://곱하기
						curWeight *= weight;
						break;
				}
				curWeight = Mathf.Clamp01(curWeight);
				vertWeights[i]._weight = curWeight;
			}

			//Weight Refresh
			//이전
			//Editor.Select.ModMeshOfMod.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);
			//변경 20.6.17
			Editor.Select.ModMesh_Main.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}




		public void SetPhyVolWeightBlend()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None

				//이전
				//|| Editor.Select.SubMeshTransform == null
				//|| Editor.Select.ModMeshOfMod == null

				//변경 20.6.17
				|| Editor.Select.MeshTF_Main == null
				|| Editor.Select.ModMesh_Main == null
				)
			{
				//Debug.LogError("Failed..");
				return;
			}

			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
			
			//apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;//이전
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;//변경 20.6.17

			//apMesh targetMesh = Editor.Select.SubMeshTransform._mesh;//이전
			apMesh targetMesh = Editor.Select.MeshTF_Main._mesh;//변경 20.6.17

			if (targetMesh == null)
			{
				Debug.LogError("Mesh is Null");
				return;
			}



			//평균값 로직은 문제가 많다.
			//모든 VertWeight를 대상으로
			//해당 Mesh에서 연결된 1Level Vert를 일일이 검색한뒤,
			//검색된 Vert의 ModVertWeight를 구하고,
			//그 ModVertWeight의 Weight의 평균을 구해서 Dictionary로 상태로 저장한다.

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			//Undo - 연속 입력 가능
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetPhysicsWeight, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);


			Dictionary<apModifiedVertexWeight, float> avgWeights = new Dictionary<apModifiedVertexWeight, float>();

			for (int iModVert = 0; iModVert < vertWeights.Count; iModVert++)
			{
				apModifiedVertexWeight modVertWeight = vertWeights[iModVert];



				float totalWeight = 0.0f;
				int nWeight = 0;

				//자기 자신도 추가
				totalWeight += modVertWeight._weight;
				nWeight++;

				List<apVertex> linkedVert = targetMesh.GetLinkedVertex(modVertWeight._vertex, true);
				for (int iLV = 0; iLV < linkedVert.Count; iLV++)
				{
					apModifiedVertexWeight linkedModVertWeight = modMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
					{
						return a._vertex == linkedVert[iLV];
					});
					if (linkedModVertWeight != null && linkedModVertWeight != modVertWeight)
					{
						totalWeight += linkedModVertWeight._weight;
						nWeight++;
					}
					else
					{
						Debug.LogError("Link Vert에 해당하는 ModVert를 찾을 수 없다.");
					}
				}
				if (nWeight > 0)
				{
					totalWeight /= nWeight;
					avgWeights.Add(modVertWeight, totalWeight);
				}
			}
			//계산된 평균값을 넣어주자
			float ratio_Src = 0.8f;
			float ratio_Avg = 0.2f;
			foreach (KeyValuePair<apModifiedVertexWeight, float> vertWeightPair in avgWeights)
			{
				vertWeightPair.Key._weight = vertWeightPair.Key._weight * ratio_Src + vertWeightPair.Value * ratio_Avg;
			}


			//평균값을 두고, 기존 80%, 평균 20%로 넣어주자
			//float avgWeight = 0.0f;
			//for (int i = 0; i < vertWeights.Count; i++)
			//{
			//	avgWeight += vertWeights[i]._weight;
			//}
			//avgWeight /= vertWeights.Count;


			//for (int i = 0; i < vertWeights.Count; i++)
			//{
			//	vertWeights[i]._weight = (vertWeights[i]._weight * ratio_Src) + (avgWeight * ratio_Avg);
			//}

			//Weight Refresh
			//이전
			//Editor.Select.ModMeshOfMod.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);

			//변경
			Editor.Select.ModMesh_Main.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}


		public void SelectVertexWeightGrowOrShrink(bool isGrow)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				
				//|| Editor.Select.ModMeshOfMod == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.17
				)
			{
				//Debug.LogError("Failed..");
				return;
			}

			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;

			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			apModifiedVertexWeight vertWeight = null;
			if (isGrow)
			{
				//Grow인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져오고, 기존의 리스트에 없는 Vertex를 선택해준다.
				List<apModifiedVertexWeight> addVertWeights = new List<apModifiedVertexWeight>();
				for (int iVW = 0; iVW < vertWeights.Count; iVW++)
				{
					vertWeight = vertWeights[iVW];

					List<apVertex> linkedVerts = vertWeight._mesh.GetLinkedVertex(vertWeight._vertex, false);
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertWeights.Exists(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (isExist)
						{
							//하나라도 이미 선택된 거네요.
							//패스
							continue;
						}

						apVertex addVertex = linkedVerts[iVert];
						//하나도 선택되지 않은 Vertex라면
						//새로 추가해주자
						apModifiedVertexWeight addVertWeight = modMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == addVertex;
						});
						if (addVertWeight != null)
						{
							if (!addVertWeights.Contains(addVertWeight))
							{
								addVertWeights.Add(addVertWeight);
							}
						}
					}
				}
				//일괄적으로 추가해주자
				//이전
				//for (int i = 0; i < addVertWeights.Count; i++)
				//{
				//	Editor.Select.AddModVertexOfModifier(null, null, addVertWeights[i], addVertWeights[i]._renderVertex);
				//}

				//변경 20.6.26 : 개선된 선택하기
				Editor.Select.AddModVertWeights(addVertWeights);
			}
			else
			{
				//Shirink인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져온다.
				//1) Linked Vert가 없으면 : 삭제 리스트에 넣는다.
				//2) Linked Vert 중에서 "지금 선택 중인 Vert"에 해당하지 않는 Vert가 하나라도 있으면 : 삭제 리스트에 넣는다.
				//3) 모든 Linked Vert가 "지금 선택중인 Vert"에 해당된다면 유지
				List<apModifiedVertexWeight> removeVertWeights = new List<apModifiedVertexWeight>();
				for (int iVW = 0; iVW < vertWeights.Count; iVW++)
				{
					vertWeight = vertWeights[iVW];

					List<apVertex> linkedVerts = vertWeight._mesh.GetLinkedVertex(vertWeight._vertex, false);
					if (linkedVerts == null || linkedVerts.Count == 0)
					{
						//1) 연결된게 없으면 삭제
						removeVertWeights.Add(vertWeight);
						continue;
					}

					//모든 Vertex가 현재 선택된 상태인지 확인하자
					bool isAllSelected = true;
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertWeights.Exists(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (!isExist)
						{
							//선택되지 않은 Vertex를 발견!
							isAllSelected = false;
							break;
						}
					}
					if (!isAllSelected)
					{
						//2) 하나라도 선택되지 않은 Link Vertex가 발견되었다면 이건 삭제 대상이다.
						if (!removeVertWeights.Contains(vertWeight))
						{
							removeVertWeights.Add(vertWeight);
						}
					}
					else
					{
						//추가)
						//만약 외곽선에 위치한 Vertex라면 우선순위에 포함된다.
						bool isOutlineVertex = vertWeight._mesh.IsOutlineVertex(vertWeight._vertex);
						if (isOutlineVertex)
						{
							if (!removeVertWeights.Contains(vertWeight))
							{
								removeVertWeights.Add(vertWeight);
							}
						}
					}
				}

				if (removeVertWeights.Count > 0)
				{
					//이전
					////하나씩 삭제하자
					//for (int i = 0; i < removeVertWeights.Count; i++)
					//{
					//	vertWeight = removeVertWeights[i];
					//	Editor.Select.RemoveModVertexOfModifier(null, null, vertWeight, vertWeight._renderVertex);
					//}

					//변경 20.6.26 : 향상된 MRV 선택취소하기
					Editor.Select.RemoveModVertWeights(removeVertWeights);
				}

			}
		}

		public void SetPhysicsViscostyGroupID(int iViscosityID, bool isViscosityAdd)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				|| Editor.Select.ModMesh_Main == null)
			{
				//Debug.LogError("Failed..");
				return;
			}

			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetPhysicsProperty, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apModifiedVertexWeight vertWeight = null;
			for (int i = 0; i < vertWeights.Count; i++)
			{
				vertWeight = vertWeights[i];
				if (iViscosityID == 0)
				{
					//0으로 초기화
					vertWeight._physicParam._viscosityGroupID = 0;
				}
				else
				{
					if (isViscosityAdd)
					{
						//추가한다.
						vertWeight._physicParam._viscosityGroupID |= iViscosityID;
					}
					else
					{
						//삭제한다.
						vertWeight._physicParam._viscosityGroupID &= ~iViscosityID;

					}
				}

			}
		}

		public void ResetPhysicsValues()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			for (int iMG = 0; iMG < Editor._portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[iMG];

				List<apModifierBase> modifiers = meshGroup._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apModifierBase mod = modifiers[iMod];

					if (!mod.IsPhysics)
					{
						continue;
					}

					for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
					{
						apModifierParamSetGroup paramSetGroup = mod._paramSetGroup_controller[iPSG];

						for (int iPS = 0; iPS < paramSetGroup._paramSetList.Count; iPS++)
						{
							apModifierParamSet paramSet = paramSetGroup._paramSetList[iPS];

							for (int iModMesh = 0; iModMesh < paramSet._meshData.Count; iModMesh++)
							{
								apModifiedMesh modMesh = paramSet._meshData[iModMesh];

								List<apModifiedVertexWeight> vertWeights = modMesh._vertWeights;
								for (int iVW = 0; iVW < vertWeights.Count; iVW++)
								{
									apModifiedVertexWeight vertWeight = vertWeights[iVW];
									if (vertWeight == null)
									{
										continue;
									}

									vertWeight._calculatedDeltaPos = Vector2.zero;
									vertWeight.DampPhysicVertex();
								}
							}
						}
					}

				}
			}
		}


		//---------------------------------------------------
		// 4. 메시 작업
		//--------------------------------------------------
		public void StartMeshEdgeWork()
		{
			if (Editor.Select.Mesh == null)
			{
				return;
			}

			Editor.Select.Mesh.StartNewEdgeWork();

		}

		public void CheckMeshEdgeWorkRemained()
		{
			Editor.VertController.StopEdgeWire();
			if (Editor.Select.Mesh == null)
			{
				return;
			}

			if (Editor.Select.Mesh.IsEdgeWorking() && Editor.Select.Mesh.IsAnyWorkedEdge())
			{
				//Undo
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MakeEdges, 
												Editor, 
												Editor.Select.Mesh, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				//bool isResult = EditorUtility.DisplayDialog("Confirm Edges", "Edge working is not completed.\nDo you want to complete it?", "Make Edges", "Remove");
				bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.MeshEditChanged_Title),
																_editor.GetText(TEXT.MeshEditChanged_Body),
																_editor.GetText(TEXT.MeshEditChanged_Okay),
																_editor.GetText(TEXT.Cancel)
																);
				if (isResult)
				{
					//Editor._selection.Mesh.MakeEdgesToIndexBuffer();
					Editor.Select.Mesh.MakeEdgesToPolygonAndIndexBuffer();
					Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
				}
				else
				{
					//Editor._selection.Mesh.CancelEdgeWork();
				}
			}
		}

		/// <summary>
		/// [Deprecated]
		/// </summary>
		/// <param name="volumeValue"></param>
		public void PaintVolumeValue(float volumeValue)
		{
			//이 코드는 사용하지 않습니다.
			//if(Editor.Select.Mesh == null)
			//{
			//	return;
			//}
			//apEditorUtil.SetRecord("Paint Volume Value", Editor._portrait);

			//List<apVertex> vertices = Editor.Select.Mesh._vertexData;
			//for (int i = 0; i < vertices.Count; i++)
			//{
			//	vertices[i]._volumeWeight = volumeValue / 100.0f;
			//}

			////Editor.Repaint();
			//Editor.SetRepaint();
		}

		public float GetBrushValue(float dist, float brushRadius, float value, float hardness)
		{
			if (dist > brushRadius)
			{
				return 0.0f;
			}
			if (dist < 0.0f)
			{
				dist = 0.0f;
			}

			value *= 0.01f;
			hardness *= 0.01f;

			hardness = Mathf.Clamp01(hardness);

			float softValue = 1.0f * (brushRadius - dist) / brushRadius;

			float resultValue = 1.0f * (hardness) + softValue * (1.0f - hardness);
			return resultValue * value;

		}

		//public int GetNearestBrushSizeIndex(float brushSize)
		//{
		//	int iNearest = -1;
		//	float minDiff = float.MaxValue;

		//	for (int i = 0; i < Editor._brushPreset_Size.Length; i++)
		//	{
		//		float diff = Mathf.Abs(Editor._brushPreset_Size[i] - brushSize);
		//		if (iNearest < 0 || diff < minDiff)
		//		{
		//			minDiff = diff;
		//			iNearest = i;
		//		}
		//	}
		//	return iNearest;
		//}

		//public float GetNextBrushSize(float brushSize, bool isIncrease)
		//{
		//	int iNearest = GetNearestBrushSizeIndex(brushSize);
		//	if (isIncrease)
		//	{
		//		iNearest++;
		//	}
		//	else
		//	{
		//		iNearest--;
		//	}

		//	if (iNearest >= Editor._brushPreset_Size.Length)
		//	{
		//		iNearest = Editor._brushPreset_Size.Length - 1;
		//	}
		//	else if (iNearest < 0)
		//	{
		//		iNearest = 0;
		//	}
		//	return Editor._brushPreset_Size[iNearest];
		//}

		public apHotKey.HotKeyResult RemoveSelectedMeshPolygon(object paramObject)
		{
			if (Editor._portrait == null)
			{
				return null;
			}
			if (Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh ||
				Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AddTools ||
				Editor._meshEditeMode_MakeMesh_AddTool != apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon)
			{
				return null;
			}
			if (Editor.Select.Mesh == null || Editor.VertController.Polygon == null)
			{
				return null;
			}

			apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditPolygons, 
											Editor, 
											Editor.Select.Mesh, 
											//null, 
											false,
											apEditorUtil.UNDO_STRUCT.ValueOnly);

			Editor.Select.Mesh._polygons.Remove(Editor.VertController.Polygon);
			Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
			Editor.VertController.UnselectVertex();

			Editor.Controller.ResetAllRenderUnitsVertexIndex();

			return apHotKey.HotKeyResult.MakeResult();
		}

		//--------------------------------------------------
		// 5. 메시 그룹 작업
		//--------------------------------------------------
		public apTransform_Mesh AddMeshToMeshGroup(apMesh addedMesh, bool isRecordAndRefresh = true)
		{
			if (Editor == null)
			{
				//EditorUtility.DisplayDialog("Error", "Adding Mesh is failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshAttachFailed_Title),
												Editor.GetText(TEXT.MeshAttachFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}
			return AddMeshToMeshGroup(addedMesh, Editor.Select.MeshGroup, isRecordAndRefresh);
		}
		public apTransform_Mesh AddMeshToMeshGroup(apMesh addedMesh, apMeshGroup targetMeshGroup, bool isRecordAndRefresh = true)
		{
			if (Editor == null || targetMeshGroup == null || addedMesh == null)
			{
				return null;
			}

			if (isRecordAndRefresh)
			{
				//Undo
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AttachMesh, 
													Editor, 
													targetMeshGroup, 
													//addedMesh, 
													false, 
													true,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}

			int nSameMesh = targetMeshGroup._childMeshTransforms.FindAll(delegate (apTransform_Mesh a)
			{
				return a._meshUniqueID == addedMesh._uniqueID;
			}).Count;

			string newNickName = addedMesh._name;
			if (nSameMesh > 0)
			{
				newNickName += " (" + (nSameMesh + 1) + ")";
			}

			//int nextID = Editor._portrait.MakeUniqueID_Transform();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0)
			{
				return null;
			}
			apTransform_Mesh newMeshTransform = new apTransform_Mesh(nextID);

			newMeshTransform._meshUniqueID = addedMesh._uniqueID;
			newMeshTransform._nickName = newNickName;
			newMeshTransform._mesh = addedMesh;
			newMeshTransform._matrix = new apMatrix();
			newMeshTransform._isVisible_Default = true;

			//Depth는 가장 큰 값으로 들어간다.
			int maxDepth = targetMeshGroup.GetLastDepth();
			newMeshTransform._depth = maxDepth + 1;

			//추가 19.6.12 : Material Set 초기화
			newMeshTransform._linkedMaterialSet = Editor._portrait.GetDefaultMaterialSet();
			if(newMeshTransform._linkedMaterialSet != null)
			{
				newMeshTransform._materialSetID = newMeshTransform._linkedMaterialSet._uniqueID;
			}
			else
			{
				newMeshTransform._materialSetID = -1;
			}

			newMeshTransform._isUseDefaultMaterialSet = true;

			newMeshTransform._isCustomShader = false;
			if(newMeshTransform._customMaterialProperties == null)
			{
				newMeshTransform._customMaterialProperties = new List<apTransform_Mesh.CustomMaterialProperty>();
			}
			else
			{
				newMeshTransform._customMaterialProperties.Clear();
			}
			
			targetMeshGroup._childMeshTransforms.Add(newMeshTransform);


			//추가 : 20.3.30 : ModMesh 링크를 다시 하자.
			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();

			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(targetMeshGroup);

			targetMeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);
			targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);


			if (isRecordAndRefresh)
			{
				targetMeshGroup.SetDirtyToReset();
				targetMeshGroup.RefreshForce();

				//추가 21.1.32 : Rule 가시성 동기화 초기화
				ResetVisibilityPresetSync();

				//추가 / 삭제시 요청한다.
				Editor.OnAnyObjectAddedOrRemoved();

				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}
			

			return newMeshTransform;
		}

		public class AttachMeshGroupError
		{
			public bool _isError = false;
			public int _nError = 0;
			public List<apMeshGroup> _meshGroups = new List<apMeshGroup>();
			public AttachMeshGroupError()
			{
				_isError = false;
				_nError = 0;
				if(_meshGroups == null)
				{
					_meshGroups = new List<apMeshGroup>();
				}
				_meshGroups.Clear();
			}
		}

		public apTransform_MeshGroup AddMeshGroupToMeshGroup(apMeshGroup addedMeshGroup, AttachMeshGroupError error, bool isRecordAndRefresh = true)
		{
			if (Editor == null)
			{
				return null;
			}
			return AddMeshGroupToMeshGroup(addedMeshGroup, Editor.Select.MeshGroup, error, isRecordAndRefresh);
		}

		public apTransform_MeshGroup AddMeshGroupToMeshGroup(apMeshGroup addedMeshGroup, apMeshGroup targetMeshGroup, AttachMeshGroupError error, bool isRecordAndRefresh = true)
		{
			if (Editor == null || targetMeshGroup == null || addedMeshGroup == null)
			{
				return null;
			}


			bool isExist = targetMeshGroup._childMeshGroupTransforms.Exists(delegate (apTransform_MeshGroup a)
			{
				return a._meshGroupUniqueID == addedMeshGroup._uniqueID;
			});

			if (isExist)
			{
				return null;
			}

			if (isRecordAndRefresh)
			{
				//Undo
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AttachMeshGroup, 
													Editor, 
													targetMeshGroup, 
													//addedMeshGroup, 
													false, true,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}


			string newNickName = addedMeshGroup._name;

			//int nextID = Editor._portrait.MakeUniqueID_Transform();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);
			if (nextID < 0)
			{
				return null;
			}

			apTransform_MeshGroup newMeshGroupTransform = new apTransform_MeshGroup(nextID);

			newMeshGroupTransform._meshGroupUniqueID = addedMeshGroup._uniqueID;
			newMeshGroupTransform._nickName = newNickName;
			newMeshGroupTransform._meshGroup = addedMeshGroup;
			newMeshGroupTransform._matrix = new apMatrix();
			newMeshGroupTransform._isVisible_Default = true;

			//Depth는 가장 큰 값으로 들어간다.
			int maxDepth = targetMeshGroup.GetLastDepth();
			newMeshGroupTransform._depth = maxDepth + 1;

			targetMeshGroup._childMeshGroupTransforms.Add(newMeshGroupTransform);

			newMeshGroupTransform._meshGroup._parentMeshGroup = targetMeshGroup;
			newMeshGroupTransform._meshGroup._parentMeshGroupID = targetMeshGroup._uniqueID;


			//추가 19.8.20 : 추가되는 MeshGroup에 (Animation) 모디파이어가 있다면 경고 문구를 띄워야 한다.
			if (error != null)
			{
				if (addedMeshGroup._modifierStack != null && addedMeshGroup._modifierStack._modifiers != null)
				{
					bool isAnyAnimModifier = false;
					for (int iMod = 0; iMod < addedMeshGroup._modifierStack._modifiers.Count; iMod++)
					{
						apModifierBase subModifier = addedMeshGroup._modifierStack._modifiers[iMod];
						if (subModifier == null)
						{
							continue;
						}

						if(subModifier.IsAnimated)
						{
							isAnyAnimModifier = true;
							break;
						}
					}
					if(isAnyAnimModifier)
					{
						error._isError = true;
						error._nError++;
						error._meshGroups.Add(addedMeshGroup);
					}
				}
			}

			//버그 수정 19.8.20 : 추가된 MeshGroup이 루트 유닛인 경우 > RootUnit에서 삭제
			apRootUnit linkedRootUnit = Editor._portrait._rootUnits.Find(delegate(apRootUnit a)
			{
				return a._childMeshGroup == addedMeshGroup;
			});
			if(linkedRootUnit != null)
			{
				Editor._portrait._mainMeshGroupIDList.Remove(addedMeshGroup._uniqueID);
				Editor._portrait._mainMeshGroupList.Remove(addedMeshGroup);
				Editor._portrait._rootUnits.Remove(linkedRootUnit);
			}

			//추가 : 20.3.30 : ModMesh 링크를 다시 하자.
			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();

			apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(targetMeshGroup);

			targetMeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);
			targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			

			if (isRecordAndRefresh)
			{
				targetMeshGroup.SetDirtyToReset();
				//targetMeshGroup.SetAllRenderUnitForceUpdate();
				targetMeshGroup.RefreshForce();

				//추가 21.1.32 : Rule 가시성 동기화 초기화
				ResetVisibilityPresetSync();

				//추가 / 삭제시 요청한다.
				Editor.OnAnyObjectAddedOrRemoved();

				Editor.Hierarchy.SetNeedReset();
				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			return newMeshGroupTransform;
		}




		public bool AddClippingMeshTransform(	apMeshGroup meshGroup,
												apTransform_Mesh meshTransform,
												bool isShowErrorDialog,
												bool isRecordAndRefresh,
												bool isSortMeshGroupAndRefreshHierarchy)
		{
			if (meshGroup == null || meshTransform == null)
			{
				return false;
			}

			//Parent도 Clip을 지정한 뒤 -> Refresh만 잘 하면 된다.
			if (meshGroup.GetMeshTransform(meshTransform._transformUniqueID) == null)
			{
				//해당 메시 그룹에 존재하지 않는 트랜스폼이다.

				if (meshGroup.GetMeshTransformRecursive(meshTransform._transformUniqueID) == null)
				{
					//Debug.LogError("해당 메시 그룹에 존재하지 않는 트랜스폼이다. -> Child 에도 없다. 끝");
					return false;
				}
			}

			if (isRecordAndRefresh)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_ClippingChanged, 
													Editor, 
													meshGroup, 
													//null, 
													false, true,
													apEditorUtil.UNDO_STRUCT.StructChanged);
			}


			//이미 Clip 상태이면 패스
			//if(meshTransform._isClipping_Child || meshTransform._isClipping_Parent)
			//{
			//	return false;
			//}

			//속성 바꾸고 자동으로 Sort 및 Clipping 연결
			meshTransform._isClipping_Child = true;
			meshGroup.SetDirtyToSort();

			//변경 v1.4.2 : 무조건 Clipping 나올때마다 Sort/Hierarchy 갱신을 하진 않는다.
			//요청에 의해서만 갱신할 것 (PSD 임포트시에 이 함수가 레이어마다 호출되기 때문)
			if (isSortMeshGroupAndRefreshHierarchy)
			{
				meshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//1.4.2 : Depth는 변경되지 않았으므로

				meshGroup.RefreshForce();
				Editor.Hierarchy_MeshGroup.RefreshUnits();
			}
			
			return true;
		}



		public void ReleaseClippingMeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform)
		{
			if (meshGroup == null || meshTransform == null)
			{
				return;
			}

			if (meshGroup.GetMeshTransform(meshTransform._transformUniqueID) == null)
			{
				//해당 메시 그룹에 존재하지 않는 트랜스폼이다.
				if (meshGroup.GetMeshTransformRecursive(meshTransform._transformUniqueID) == null)
				{
					return;
				}
			}

			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_ClippingChanged, 
												Editor, 
												meshGroup, 
												//null, 
												false, true,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			meshTransform._isClipping_Child = false;
			meshTransform._clipIndexFromParent = -1;
			meshTransform._clipParentMeshTransform = null;

			meshGroup.SetDirtyToSort();
			meshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//1.4.2 : Depth는 변경되지 않았다.
			meshGroup.RefreshForce();
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}



		public enum RESET_VISIBLE_TARGET
		{
			RenderUnits,
			Bones,
			RenderUnitsAndBones
		}

		/// <summary>
		/// 추가 21.1.31 : Tmp Visibility와 별개로 Rule에 의한건 이 함수로 초기화하자
		/// </summary>
		/// <param name="meshGroup"></param>
		public void ResetMeshGroupRuleVisibility(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//렌더 유닛 초기화
			for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
			{
				meshGroup._renderUnits_All[i]._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None;
			}

			//본 초기화
			if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
			{
				List<apBone> boneList = null;
				for (int iBontSet = 0; iBontSet < meshGroup._boneListSets.Count; iBontSet++)
				{	
					boneList = meshGroup._boneListSets[iBontSet]._bones_All;
					if (boneList != null && boneList.Count > 0)
					{
						for (int iBone = 0; iBone < boneList.Count; iBone++)
						{
							boneList[iBone].SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.None);
						}
					}
				}
			}
		}

		/// <summary>
		/// 추가 21.1.31 : Visibility Preset의 동기화를 해제한다.
		/// </summary>
		public void ResetVisibilityPresetSync()
		{
			if(Editor._portrait == null
				|| Editor._portrait.VisiblePreset == null)
			{
				return;
			}

			Editor._portrait.VisiblePreset.ClearSync();
		}


		public enum RESET_VISIBLE_ACTION
		{
			/// <summary>옵션에 관계없이 Default값으로 초기화 후 동기화 (초기화 버튼을 Hierarchy에서 누를 때)</summary>
			ResetForce,
			/// <summary>옵션에 관계없이 Default 값으로 초기화를 하지만 동기화는 하지 않는다. (루트 유닛 등을 선택할 때)</summary>
			ResetForceAndNoSync,
			/// <summary>옵션이 On인 경우 Default 값으로 초기화 + 동기화 / Off인 경우 저장된 Visible값을 로드하여 적용한다. (메시 그룹, 애니메이션을 선택할 때)</summary>
			RestoreByOption,
			/// <summary>옵션이 On인 경우 Default 값으로 초기화 후 동기화 / Off인 경우 Refresh만 한다. (작업이 시작/종료될 때)</summary>
			OnlyRefreshIfOptionIsOff
		}

		/// <summary>
		/// 작업시 TmpWorkVisible이 각 Transform에 저장되어있다. 이값을 초기화(true)한다.
		/// 에디터 옵션에 따라서 이 함수는 무시될 수 있다. (인자에 false입력시..)
		/// </summary>
		/// <param name="meshGroup"></param>
		//public void SetMeshGroupTmpWorkVisibleReset(apMeshGroup meshGroup, bool isForceWOEditorOption, bool isTargetMeshes, bool isTargetBones)//이전
		public void SetMeshGroupTmpWorkVisibleReset(apMeshGroup meshGroup, RESET_VISIBLE_ACTION actionType, RESET_VISIBLE_TARGET targetType)//변경 20.4.13
		{
			if (meshGroup == null)
			{
				return;
			}

			bool isResetForce = false;
			bool isRestoreFromVisibilityContoller = false;
			bool isSyncAfterReset = false;
			
			switch (actionType)
			{
				case RESET_VISIBLE_ACTION.ResetForce:
					//강제로 초기화 한다. + 동기화
					isResetForce = true;
					isSyncAfterReset = true;
					break;

				case RESET_VISIBLE_ACTION.ResetForceAndNoSync:
					//강제로 초기화는 하지만 동기화는 하지 않는다.
					isResetForce = true;
					break;

				case RESET_VISIBLE_ACTION.RestoreByOption:
					{

						if (Editor._isRestoreTempMeshVisibilityWhenTaskEnded)
						{
							//옵션이 ON > 초기화 + 동기화
							isResetForce = true;
							isSyncAfterReset = true;
						}
						else
						{
							//옵션이 OFF > Visible 값으로부터 복구
							isRestoreFromVisibilityContoller = true;
						}
					}
					break;
				case RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff:
					{
						if (Editor._isRestoreTempMeshVisibilityWhenTaskEnded)
						{
							//옵션이 ON > 초기화 + 동기화
							isResetForce = true;
							isSyncAfterReset = true;
						}
						//옵션이 OFF > Refresh만 한다.
					}
					break;
			}


			if(isResetForce)
			{
				// <처리 1> 초기값으로 전환 후 동기화
				if(targetType == RESET_VISIBLE_TARGET.RenderUnits ||
					targetType == RESET_VISIBLE_TARGET.RenderUnitsAndBones)
				{
					//렌더 유닛 초기화
					for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
					{
						meshGroup._renderUnits_All[i].ResetTmpWorkVisible(false);
					}
				}

				if(targetType == RESET_VISIBLE_TARGET.Bones ||
					targetType == RESET_VISIBLE_TARGET.RenderUnitsAndBones)
				{
					//본 초기화
					if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
					{
						List<apBone> boneRootList = null;
						for (int iBontSet = 0; iBontSet < meshGroup._boneListSets.Count; iBontSet++)
						{
							boneRootList = meshGroup._boneListSets[iBontSet]._bones_Root;
							if (boneRootList != null && boneRootList.Count > 0)
							{
								for (int iBone = 0; iBone < boneRootList.Count; iBone++)
								{
									boneRootList[iBone].ResetGUIVisibleRecursive(false);
								}
							}
						}
					}
				}

				//TmpWorkVisible 이 초기화되었다.
				if (targetType == RESET_VISIBLE_TARGET.RenderUnitsAndBones)
				{
					//모두 초기화가 되었으므로, 변경 사항은 없다.
					Editor.Select.SetTmpWorkVisibleChanged(false, false);
				}
				else
				{
					//일부만 되었다면 검사를 하자
					CheckTmpWorkVisible(meshGroup);
				}
				meshGroup.RefreshForce();

				if (isSyncAfterReset)
				{
					//동기화를 하자 (초기화 형태로)
					Editor.VisiblityController.SaveAll(meshGroup);
				}
				
			}
			else if(isRestoreFromVisibilityContoller)
			{
				// <처리 2> 저장된 값으로부터 복구 (동기화 없음)
				Editor.VisiblityController.LoadAll(meshGroup);

				meshGroup.RefreshForce();

				//일부만 되었다면 검사를 하자
				CheckTmpWorkVisible(meshGroup);
			}
			else
			{
				// <처리 3> 단순히 Refresh 후 체크
				meshGroup.RefreshForce();//<<RefeshForce만 실행하자

				//일단 검사를 하자
				CheckTmpWorkVisible(meshGroup);
			}

			//이전 코드
			#region [미사용 코드]
			//if(!isForceWOEditorOption && !Editor._isRestoreTempMeshVisibilityWhenTaskEnded)
			//{
			//	//Visibility 복구 옵션이 false인 경우
			//	meshGroup.RefreshForce();//<<RefeshForce만 실행하자

			//	//일단 검사를 하자
			//	CheckTmpWorkVisible(meshGroup);

			//	return;
			//}

			//if (isTargetMeshes)
			//{
			//	for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
			//	{
			//		meshGroup._renderUnits_All[i].ResetTmpWorkVisible();
			//	}
			//}

			//if (isTargetBones)
			//{
			//	if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
			//	{
			//		List<apBone> boneRootList = null;
			//		for (int iBontSet = 0; iBontSet < meshGroup._boneListSets.Count; iBontSet++)
			//		{
			//			boneRootList = meshGroup._boneListSets[iBontSet]._bones_Root;
			//			if (boneRootList != null && boneRootList.Count > 0)
			//			{
			//				for (int iBone = 0; iBone < boneRootList.Count; iBone++)
			//				{
			//					boneRootList[iBone].ResetGUIVisibleRecursive();
			//				}
			//			}
			//		}
			//	}
			//}

			////TmpWorkVisible 이 초기화되었다.
			//if(isTargetMeshes && isTargetBones)
			//{
			//	Editor.Select.SetTmpWorkVisibleChanged(false, false);
			//}
			//else
			//{
			//	//일부만 되었다면 검사를 하자
			//	CheckTmpWorkVisible(meshGroup);
			//}


			////SetMeshGoupTransformTmpWorkVisibleReset(meshGroup._rootMeshGroupTransform);
			//meshGroup.RefreshForce(); 
			#endregion
		}

		/// <summary>
		/// 작업시 TmpWorkVisible을 일괄 적용한다.
		/// 옵션으로 "제외 대상"을 하나 받을 수 있다.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void SetMeshGroupTmpWorkVisibleAll(apMeshGroup meshGroup, bool isVisibleTmpWork, apRenderUnit exceptTarget)
		{
			if (meshGroup == null)
			{
				return;
			}

			apRenderUnit renderUnit = null;
			for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
			{
				renderUnit = meshGroup._renderUnits_All[i];
				if (renderUnit == exceptTarget)
				{
					continue;
				}
				if (renderUnit == meshGroup._rootRenderUnit)
				{
					continue;
				}
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode)
				{
					//GroupNode는 토글되지 않는다.
					continue;
				}
				if (isVisibleTmpWork == renderUnit._isVisible_WithoutParent)
				{
					//현재 Visible이 같으면 토글하지 않는다.
					continue;
				}

				//의도한 Visible 값이 아니다.
				//토글하자
				if (renderUnit._isVisible_WithoutParent == renderUnit._isVisibleCalculated)
				{
					if (isVisibleTmpWork)
					{
						//Hide -> Show
						//이전
						//renderUnit._isVisibleWorkToggle_Hide2Show = true;
						//renderUnit._isVisibleWorkToggle_Show2Hide = false;

						//변경 21.1.28
						renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;
					}
					else
					{
						//Show -> Hide
						//이전
						//renderUnit._isVisibleWorkToggle_Hide2Show = false;
						//renderUnit._isVisibleWorkToggle_Show2Hide = true;

						//변경 21.1.28
						renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
					}
				}
				else
				{
					//이전
					//renderUnit._isVisibleWorkToggle_Hide2Show = false;
					//renderUnit._isVisibleWorkToggle_Show2Hide = false;

					//변경 21.1.28
					renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.None;
				}
			}

			meshGroup.RefreshForce();


			//추가 20.4.13 : Visibility Controller와 동기화를 하자
			Editor.VisiblityController.Save_AllRenderUnits(meshGroup);

			//TmpWorkVisible 여부 체크 > 이 함수가 호출될 때 외부에서 같이 호출되더라
			//CheckTmpWorkVisible(meshGroup);
		}

		//[v1.5.0] 대상 TF의 Tmp Work Visible을 변경한다.
		public void SetMeshGroupTmpWorkVisibleTargetTFs(	apMeshGroup meshGroup,
															List<apTransform_Mesh> targetMeshTFs,
															List<apTransform_MeshGroup> targetMeshGroupTFs,
															bool isVisibleTmpWork,
															apRenderUnit exceptRenderUnit)
		{
			int nMeshTFs = targetMeshTFs != null ? targetMeshTFs.Count : 0;
			int nMeshGroupTFs = targetMeshGroupTFs != null ? targetMeshGroupTFs.Count : 0;

			if (meshGroup == null || (nMeshTFs == 0 && nMeshGroupTFs == 0))
			{
				return;
			}

			apRenderUnit renderUnit = null;

			if(nMeshTFs > 0)
			{
				apTransform_Mesh meshTF = null;
				for (int i = 0; i < nMeshTFs; i++)
				{
					meshTF = targetMeshTFs[i];
					if(meshTF == null) { continue; }

					renderUnit = meshTF._linkedRenderUnit;
					if(renderUnit == null) { continue; }

					//Root 렌더 유닛은 생략한다.
					if (renderUnit == meshGroup._rootRenderUnit) { continue; }

					//일부 렌더 유닛도 생략한다.
					if(renderUnit == exceptRenderUnit) { continue; }

					//Visible이 같으면 토글하지 않는다.
					if (isVisibleTmpWork == renderUnit._isVisible_WithoutParent)
					{
						//현재 Visible이 같으면 토글하지 않는다.
						continue;
					}
					//의도한 Visible 값이 아니라면 토글하자
					if (renderUnit._isVisible_WithoutParent == renderUnit._isVisibleCalculated)
					{
						//값이 같다면 다르도록 변경
						if (isVisibleTmpWork)
						{
							//Hide -> Show
							renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;
						}
						else
						{
							//Show -> Hide
							renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
						}
					}
					else
					{
						//값이 다르다면 동일해지도록 토글
						//변경 21.1.28
						renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.None;
					}
				}
			}

			if(nMeshGroupTFs > 0)
			{
				apTransform_MeshGroup meshGroupTF = null;
				for (int i = 0; i < nMeshGroupTFs; i++)
				{
					meshGroupTF = targetMeshGroupTFs[i];
					if(meshGroupTF == null) { continue; }

					renderUnit = meshGroupTF._linkedRenderUnit;
					if(renderUnit == null) { continue; }

					//Root 렌더 유닛은 생략한다.
					if (renderUnit == meshGroup._rootRenderUnit) { continue; }

					//일부 렌더 유닛도 생략한다.
					if(renderUnit == exceptRenderUnit) { continue; }

					//Visible이 같으면 토글하지 않는다.
					if (isVisibleTmpWork == renderUnit._isVisible_WithoutParent)
					{
						//현재 Visible이 같으면 토글하지 않는다.
						continue;
					}

					//의도한 Visible 값이 아니라면 토글하자
					if (renderUnit._isVisible_WithoutParent == renderUnit._isVisibleCalculated)
					{
						//값이 같다면 다르도록 변경
						if (isVisibleTmpWork)
						{
							//Hide -> Show
							renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;
						}
						else
						{
							//Show -> Hide
							renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
						}
					}
					else
					{
						//값이 다르다면 동일해지도록 토글
						//변경 21.1.28
						renderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.None;
					}
				}
			}

			meshGroup.RefreshForce();


			//추가 20.4.13 : Visibility Controller와 동기화를 하자
			Editor.VisiblityController.Save_AllRenderUnits(meshGroup);
		}


		/// <summary>
		/// MeshGroup작업시 TmpWorkVisible이 변경된 것(Mesh, Bone)이 있는지 확인한다.
		/// TmpWorkVisible을 변경할 경우 이 함수를 꼭 호출해주자.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void CheckTmpWorkVisible(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			bool isAnyMeshChanged = false;
			bool isAnyBoneChanged = false;

			apRenderUnit renderUnit = null;
			apBone bone = null;

			for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
			{
				renderUnit = meshGroup._renderUnits_All[i];
				//이전
				//if(renderUnit._isVisibleWorkToggle_Hide2Show ||
				//	renderUnit._isVisibleWorkToggle_Show2Hide)

				//변경 21.1.28
				if(renderUnit._workVisible_Tmp != apRenderUnit.WORK_VISIBLE_TYPE.None)
				{
					//변경된 메시가 있다.
					isAnyMeshChanged = true;
					break;
				}
			}

			if(meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
			{
				List<apBone> boneList = null;
				for (int iBontSet = 0; iBontSet < meshGroup._boneListSets.Count; iBontSet++)
				{
					boneList = meshGroup._boneListSets[iBontSet]._bones_All;
					if(boneList != null && boneList.Count > 0)
					{
						for (int iBone = 0; iBone < boneList.Count; iBone++)
						{
							bone = boneList[iBone];

							//이전
							//if(!bone.IsGUIVisible)

							//변경 21.1.28
							if(bone.VisibleType_Tmp != apBone.GUI_VISIBLE_TYPE.None)
							{
								//보여지지 않는 본이 있다.
								isAnyBoneChanged = true;
								break;
							}
						}
					}

					if(isAnyBoneChanged)
					{
						break;
					}

				}
			}

			Editor.Select.SetTmpWorkVisibleChanged(isAnyMeshChanged, isAnyBoneChanged);
		}



		//----------------------------------------------------------
		// 메시 그룹의 오브젝트들을 "정렬된 순서로 받고자 할 때"
		//----------------------------------------------------------
		public class SortedObjectSubUnit
		{
			public object target;
			public List<SortedObjectSubUnit> childUnits = null;

			public SortedObjectSubUnit(object targetObject)
			{
				target = targetObject;
			}

			public void AddChild(SortedObjectSubUnit childUnit)
			{
				if(childUnits == null)
				{
					childUnits = new List<SortedObjectSubUnit>();
				}
				childUnits.Add(childUnit);
			}

		}
		//추가 21.3.16 : 메시 그룹의 하위 오브젝트들을 정렬된 상태로 리턴한다.
		//가능한 Hierarchy의 순서와 동일하게 정렬했다.
		//전체 생성, 타임라인 레이어 순서 변경 등을 할 땐 이걸 이용해서 정렬 기준으로 삼자
		public List<object> GetSortedSubObjectsAsHierarchy(apMeshGroup targetMeshGroup, bool isTF, bool isBone)
		{
			List<object> result = new List<object>();

			List<SortedObjectSubUnit> rootUnits_Meshes = new List<SortedObjectSubUnit>();
			List<SortedObjectSubUnit> rootUnits_Bones_Main = new List<SortedObjectSubUnit>();
			List<SortedObjectSubUnit> rootUnits_Bones_Sub = new List<SortedObjectSubUnit>();

			//1. 일단 요청한 값들을 넣자 (재귀적으로)
			//2. 요청한 재귀 리스트들을 정렬한다.
			//3. 재귀 리스트들을 Linear하게 넣자
			//4. Linear하게 넣은 순서대로 Index를 넣어서 완성하자
			if(isTF)
			{
				AddMeshGroupTFToList_Recv(targetMeshGroup, targetMeshGroup, null, rootUnits_Meshes);
				
				//재귀적으로 Sorting을 하자
				SortUnitsRecv(rootUnits_Meshes, false);
			}
			if(isBone)
			{
				//Bone은 BoneListSet이 있어서 조금 낫다.
				int nBoneSets = targetMeshGroup._boneListSets != null ? targetMeshGroup._boneListSets.Count : 0;
				if(nBoneSets > 0)
				{
					apMeshGroup.BoneListSet curBoneSet = null;
					for (int iBoneSet = 0; iBoneSet < nBoneSets; iBoneSet++)
					{
						curBoneSet = targetMeshGroup._boneListSets[iBoneSet];
						if(curBoneSet._isRootMeshGroup)
						{
							//이건 메인에 넣자
							int nRootBones = curBoneSet._bones_Root != null ? curBoneSet._bones_Root.Count : 0;
							if(nRootBones > 0)
							{
								for (int iRootBone = 0; iRootBone < nRootBones; iRootBone++)
								{
									AddBoneToList_Recv(curBoneSet._bones_Root[iRootBone], null, rootUnits_Bones_Main);
								}
							}
							
						}
						else if(curBoneSet._meshGroupTransform != null)
						{
							//이건 서브에 넣자
							//MeshGroupTF부터 넣기
							SortedObjectSubUnit newRootUnit = new SortedObjectSubUnit(curBoneSet._meshGroupTransform);
							rootUnits_Bones_Sub.Add(newRootUnit);

							int nRootBones = curBoneSet._bones_Root != null ? curBoneSet._bones_Root.Count : 0;
							if (nRootBones > 0)
							{
								for (int iSubRootBones = 0; iSubRootBones < nRootBones; iSubRootBones++)
								{
									AddBoneToList_Recv(curBoneSet._bones_Root[iSubRootBones], newRootUnit, rootUnits_Bones_Sub);
								}
							}
						}
					}
				}

				//재귀적으로 Sorting을 하자
				SortUnitsRecv(rootUnits_Bones_Main, true);
				SortUnitsRecv(rootUnits_Bones_Sub, true);
			}

			//이제 정렬된 순서대로 재귀적으로 리스트에 하나씩 넣자
			if(isTF)
			{
				AddSortedUnitToLinearListRecv(rootUnits_Meshes, result, false);
			}
			if(isBone)
			{
				AddSortedUnitToLinearListRecv(rootUnits_Bones_Main, result, true);
				AddSortedUnitToLinearListRecv(rootUnits_Bones_Sub, result, true);
			}
			
			return result;
		}

		private void AddMeshGroupTFToList_Recv(apMeshGroup curMeshGroup, apMeshGroup rootMeshGroup, SortedObjectSubUnit parentUnit, List<SortedObjectSubUnit> rootList)
		{
			int nMeshTF = curMeshGroup._childMeshTransforms != null ? curMeshGroup._childMeshTransforms.Count : 0;
			int nMeshGroupTF = curMeshGroup._childMeshGroupTransforms != null ? curMeshGroup._childMeshGroupTransforms.Count : 0;
			if (nMeshTF > 0)
			{
				apTransform_Mesh curMeshTF = null;
				for (int i = 0; i < nMeshTF; i++)
				{
					curMeshTF = curMeshGroup._childMeshTransforms[i];
					SortedObjectSubUnit newUnit = new SortedObjectSubUnit(curMeshTF);
					if(parentUnit != null)
					{
						parentUnit.AddChild(newUnit);
					}
					else
					{
						rootList.Add(newUnit);
					}
				}
			}

			if(nMeshGroupTF > 0)
			{
				apTransform_MeshGroup curMeshGroupTF = null;
				for (int i = 0; i < nMeshGroupTF; i++)
				{
					curMeshGroupTF = curMeshGroup._childMeshGroupTransforms[i];
					SortedObjectSubUnit newUnit = new SortedObjectSubUnit(curMeshGroupTF);
					if(parentUnit != null)
					{
						parentUnit.AddChild(newUnit);
					}
					else
					{
						rootList.Add(newUnit);
					}

					if(curMeshGroupTF._meshGroup != null 
						&& curMeshGroupTF._meshGroup != curMeshGroup 
						&& curMeshGroupTF._meshGroup != rootMeshGroup)
					{
						//재귀 호출한다.
						AddMeshGroupTFToList_Recv(curMeshGroupTF._meshGroup, rootMeshGroup, newUnit, rootList);
					}
				}
			}
		}

		private void AddBoneToList_Recv(apBone curBone, SortedObjectSubUnit parentUnit, List<SortedObjectSubUnit> rootList)
		{
			SortedObjectSubUnit newUnit = new SortedObjectSubUnit(curBone);
			if(parentUnit != null)
			{
				parentUnit.AddChild(newUnit);
			}
			else
			{
				rootList.Add(newUnit);
			}
			int nChildBones = curBone._childBones != null ? curBone._childBones.Count : 0;
			if (nChildBones > 0)
			{
				for (int iChildBone = 0; iChildBone < nChildBones; iChildBone++)
				{
					AddBoneToList_Recv(curBone._childBones[iChildBone], newUnit, rootList);
				}
			}
		}

		private void SortUnitsRecv(List<SortedObjectSubUnit> curUnits, bool isBoneList)
		{
			if(!isBoneList)
			{
				//Mesh 리스트라면
				curUnits.Sort(delegate(SortedObjectSubUnit a, SortedObjectSubUnit b)
				{
					int depthA = -1;
					int depthB = -1;
					if(a.target is apTransform_Mesh)
					{
						apTransform_Mesh meshTF = a.target as apTransform_Mesh;
						depthA = meshTF._depth;
					}
					else if(a.target is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroupTF = a.target as apTransform_MeshGroup;
						depthA = meshGroupTF._depth;
					}

					if(b.target is apTransform_Mesh)
					{
						apTransform_Mesh meshTF = b.target as apTransform_Mesh;
						depthB = meshTF._depth;
					}
					else if(b.target is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroupTF = b.target as apTransform_MeshGroup;
						depthB = meshGroupTF._depth;
					}

					return depthB - depthA;
				});
			}
			else
			{
				//Bone 리스트라면
				curUnits.Sort(delegate(SortedObjectSubUnit a, SortedObjectSubUnit b)
				{
					
					if(a.target is apTransform_MeshGroup && b.target is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroupTF_A = a.target as apTransform_MeshGroup;
						apTransform_MeshGroup meshGroupTF_B = b.target as apTransform_MeshGroup;
						return meshGroupTF_B._depth - meshGroupTF_A._depth;
					}

					int depthA = -1;
					int depthB = -1;

					apBone boneA = null;
					apBone boneB = null;
					if(a.target is apBone)
					{
						boneA = a.target as apBone;
						depthA = boneA._depth;
					}
					if(b.target is apBone)
					{
						boneB = b.target as apBone;
						depthB = boneB._depth;
					}

					if(depthA == depthB)
					{
						if(boneA != null && boneB != null)
						{
							//이름으로 순서를 매긴다.
							return string.Compare(boneA._name, boneB._name);
						}
					}

					return depthB - depthA;
				});
			}

			//자식 리스트도 동일하게
			SortedObjectSubUnit curNextUnit = null;
			for (int i = 0; i < curUnits.Count; i++)
			{
				curNextUnit = curUnits[i];
				if(curNextUnit.childUnits != null
					&& curNextUnit.childUnits.Count > 0)
				{
					SortUnitsRecv(curNextUnit.childUnits, isBoneList);
				}
			}
			
		}

		private void AddSortedUnitToLinearListRecv(List<SortedObjectSubUnit> subUnitList, List<object> result, bool isBoneList)
		{
			if(subUnitList == null || subUnitList.Count == 0)
			{
				return;
			}

			SortedObjectSubUnit curUnit = null;
			if(!isBoneList)
			{
				//타입 상관없이 넣자
				for (int i = 0; i < subUnitList.Count; i++)
				{	
					curUnit = subUnitList[i];
					result.Add(curUnit.target);

					if(curUnit.childUnits != null && curUnit.childUnits.Count > 0)
					{
						//자식이 있다면 그걸 먼저 넣어야 한다.
						AddSortedUnitToLinearListRecv(curUnit.childUnits, result, isBoneList);
					}
				}
			}
			else
			{
				//Bone만 넣자
				for (int i = 0; i < subUnitList.Count; i++)
				{	
					curUnit = subUnitList[i];
					if(curUnit.target is apBone)
					{
						result.Add(curUnit.target);
					}
					

					if(curUnit.childUnits != null && curUnit.childUnits.Count > 0)
					{
						//자식이 있다면 그걸 먼저 넣어야 한다.
						AddSortedUnitToLinearListRecv(curUnit.childUnits, result, isBoneList);
					}
				}
			}
		}


		//--------------------------------------------------
		// 메시 그룹의 Modifier 작업
		//--------------------------------------------------
		public void AddModifier(apModifierBase.MODIFIER_TYPE _type, int validationKey)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//ObjectGroup을 체크하여 만들어주자
			CheckAndMakeObjectGroup();


			//Undo
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_AddModifier, Editor, Editor.Select.MeshGroup, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create Modifier");

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;
			int newID = modStack.GetNewModifierID((int)_type, validationKey);
			if (newID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Modifier Adding Failed. Please Retry", "Close");

				EditorUtility.DisplayDialog(Editor.GetText(TEXT.ModifierAddFailed_Title),
												Editor.GetText(TEXT.ModifierAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return;
			}

			//GameObject로 만드는 경우
			GameObject newGameObj = null;



			int newLayer = modStack.GetLastLayer() + 1;
			apModifierBase newModifier = null;
			switch (_type)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					newGameObj = new GameObject("Modifier - Base");
					//newModifier = new apModifierBase();//<<이건 처리하지 않습니다... 사실은;
					newModifier = newGameObj.AddComponent<apModifierBase>();
					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					newGameObj = new GameObject("Modifier - Volume");
					//newModifier = new apModifier_Volume();
					newModifier = newGameObj.AddComponent<apModifier_Volume>();
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					newGameObj = new GameObject("Modifier - Morph");
					//newModifier = new apModifier_Morph();
					newModifier = newGameObj.AddComponent<apModifier_Morph>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					newGameObj = new GameObject("Modifier - AnimatedMorph");
					//newModifier = new apModifier_AnimatedMorph();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedMorph>();
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					newGameObj = new GameObject("Modifier - Rigging");
					//newModifier = new apModifier_Rigging();
					newModifier = newGameObj.AddComponent<apModifier_Rigging>();
					break;
				case apModifierBase.MODIFIER_TYPE.Physic:
					newGameObj = new GameObject("Modifier - Physic");
					//newModifier = new apModifier_Physic();
					newModifier = newGameObj.AddComponent<apModifier_Physic>();
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					newGameObj = new GameObject("Modifier - TF");
					//newModifier = new apModifier_TF();
					newModifier = newGameObj.AddComponent<apModifier_TF>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					newGameObj = new GameObject("Modifier - AnimatedTF");
					//newModifier = new apModifier_AnimatedTF();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedTF>();
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					newGameObj = new GameObject("Modifier - FFD");
					//newModifier = new apModifier_FFD();
					newModifier = newGameObj.AddComponent<apModifier_FFD>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					newGameObj = new GameObject("Modifier - AnimatedFFD");
					//newModifier = new apModifier_AnimatedFFD();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedFFD>();
					break;

				//추가 21.7.20 : Color Only Modifier 추가
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
					newGameObj = new GameObject("Modifier - ColorOnly");
					newModifier = newGameObj.AddComponent<apModifier_ColorOnly>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					newGameObj = new GameObject("Modifier - AnimatedColorOnly");
					newModifier = newGameObj.AddComponent<apModifier_AnimatedColorOnly>();
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + _type + "]");
					break;
			}


			newGameObj.transform.parent = Editor._portrait._subObjectGroup_Modifier.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;




			newModifier.LinkPortrait(Editor._portrait);
			newModifier.SetInitSetting(newID, newLayer, Editor.Select.MeshGroup._uniqueID, Editor.Select.MeshGroup);

			//추가 : 보간 방식 기본값을 Interpolation에서 Additive로 변경
			newModifier._blendMethod = apModifierBase.BLEND_METHOD.Additive;

			modStack.AddModifier(newModifier, _type);

			//Sort
			//modStack.RefreshAndSort(true);//이전
			modStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
										apModifierStack.REFRESH_OPTION_REMOVE.Ignore);//[1.4.2] 변경 22.12.13


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();

			Editor.Select.SelectModifier(newModifier);
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();

			//Undo - Create 추가
			apEditorUtil.SetRecordCreateMonoObject(newModifier, "Create Modifier");

			////프리팹이었다면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}



		public apModifierBase AddModifierToMeshGroup(	apMeshGroup targetMeshGroup, 
														apModifierBase.MODIFIER_TYPE _type, 
														int validationKey, 
														bool isRecord, bool isRefresh, bool isErrorPopup)
		{
			if (Editor._portrait == null || targetMeshGroup == null)
			{
				return null;
			}

			//ObjectGroup을 체크하여 만들어주자
			CheckAndMakeObjectGroup();


			//Undo
			if (isRecord)
			{
				apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create Modifier");
			}

			apModifierStack modStack = targetMeshGroup._modifierStack;
			int newID = modStack.GetNewModifierID((int)_type, validationKey);
			if (newID < 0)
			{
				if (isErrorPopup)
				{
					//EditorUtility.DisplayDialog("Error", "Modifier Adding Failed. Please Retry", "Close");

					EditorUtility.DisplayDialog(Editor.GetText(TEXT.ModifierAddFailed_Title),
													Editor.GetText(TEXT.ModifierAddFailed_Body),
													Editor.GetText(TEXT.Close));
				}
				return null;
			}

			//GameObject로 만드는 경우
			GameObject newGameObj = null;



			int newLayer = modStack.GetLastLayer() + 1;
			apModifierBase newModifier = null;
			switch (_type)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					newGameObj = new GameObject("Modifier - Base");
					//newModifier = new apModifierBase();//<<이건 처리하지 않습니다... 사실은;
					newModifier = newGameObj.AddComponent<apModifierBase>();
					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					newGameObj = new GameObject("Modifier - Volume");
					//newModifier = new apModifier_Volume();
					newModifier = newGameObj.AddComponent<apModifier_Volume>();
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					newGameObj = new GameObject("Modifier - Morph");
					//newModifier = new apModifier_Morph();
					newModifier = newGameObj.AddComponent<apModifier_Morph>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					newGameObj = new GameObject("Modifier - AnimatedMorph");
					//newModifier = new apModifier_AnimatedMorph();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedMorph>();
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					newGameObj = new GameObject("Modifier - Rigging");
					//newModifier = new apModifier_Rigging();
					newModifier = newGameObj.AddComponent<apModifier_Rigging>();
					break;
				case apModifierBase.MODIFIER_TYPE.Physic:
					newGameObj = new GameObject("Modifier - Physic");
					//newModifier = new apModifier_Physic();
					newModifier = newGameObj.AddComponent<apModifier_Physic>();
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					newGameObj = new GameObject("Modifier - TF");
					//newModifier = new apModifier_TF();
					newModifier = newGameObj.AddComponent<apModifier_TF>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					newGameObj = new GameObject("Modifier - AnimatedTF");
					//newModifier = new apModifier_AnimatedTF();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedTF>();
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					newGameObj = new GameObject("Modifier - FFD");
					//newModifier = new apModifier_FFD();
					newModifier = newGameObj.AddComponent<apModifier_FFD>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					newGameObj = new GameObject("Modifier - AnimatedFFD");
					//newModifier = new apModifier_AnimatedFFD();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedFFD>();
					break;

					//추가 21.7.20
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
					newGameObj = new GameObject("Modifier - ColorOnly");
					newModifier = newGameObj.AddComponent<apModifier_ColorOnly>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					newGameObj = new GameObject("Modifier - AnimatedColorOnly");
					newModifier = newGameObj.AddComponent<apModifier_AnimatedColorOnly>();
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + _type + "]");
					break;
			}


			newGameObj.transform.parent = Editor._portrait._subObjectGroup_Modifier.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			

			newModifier.LinkPortrait(Editor._portrait);
			newModifier.SetInitSetting(newID, newLayer, targetMeshGroup._uniqueID, targetMeshGroup);

			//추가 : 보간 방식 기본값을 Interpolation에서 Additive로 변경
			newModifier._blendMethod = apModifierBase.BLEND_METHOD.Additive;

			modStack.AddModifier(newModifier, _type);

			//modStack.RefreshAndSort(true);//이전
			modStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
										apModifierStack.REFRESH_OPTION_REMOVE.Ignore);//[1.4.2] 변경 22.12.13

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			if (isRecord)
			{
				//Undo - Create 추가
				apEditorUtil.SetRecordCreateMonoObject(newModifier, "Create Modifier");
			}

			//if (isRefresh)
			//{
			//	//프리팹이었다면 Apply
			//	apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
			//}

			return newModifier;
		}


		/// <summary>
		/// 현재의 Modifier에 선택한 Transform을 ModMesh로 등록한다.
		/// </summary>
		public apModifiedMesh AddModMesh_WithSubMeshOrSubMeshGroup(bool isUndo = true)
		{
			if (Editor.Select.SubEditedParamSetGroup == null || Editor.Select.Modifier == null || Editor.Select.MeshGroup == null)
			{
				return null;
			}

			if(isUndo)
			{
				//Undo
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_AddModMeshToParamSet, 
												Editor, 
												Editor.Select.Modifier, 
												//Editor.Select.MeshTF_Main != null ? (object)Editor.Select.MeshTF_Main : (object)Editor.Select.MeshGroupTF_Main, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//apModifiedMesh addedModMesh = null;
			List<apModifierParamSet> modParamSetList = Editor.Select.SubEditedParamSetGroup._paramSetList;

			bool isAnyAdded = false;
			apModifiedMesh newModMesh = null;
			if (Editor.Select.MeshTF_Main != null)
			{
				for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
				{
					//addedModMesh = Editor.Select.Modifier.AddMeshTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshInGroup, modParamSetList[iParam]);
					newModMesh = Editor.Select.Modifier.AddMeshTransform(	Editor.Select.MeshGroup,
																			Editor.Select.MeshTF_Main,
																			modParamSetList[iParam],
																			false,
																			Editor.Select.Modifier.IsTarget_ChildMeshTransform,
																			true);

					if(newModMesh != null)
					{
						isAnyAdded = true;
					}
				}
			}
			else if (Editor.Select.MeshGroupTF_Main != null)
			{
				for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
				{
					//addedModMesh = Editor.Select.Modifier.AddMeshGroupTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshGroupInGroup, modParamSetList[iParam]);
					newModMesh = Editor.Select.Modifier.AddMeshGroupTransform(	Editor.Select.MeshGroup, 
																				Editor.Select.MeshGroupTF_Main, 
																				modParamSetList[iParam], 
																				false, 
																				true);

					if(newModMesh != null)
					{
						isAnyAdded = true;
					}
				}
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			
			if (isChanged || isAnyAdded)
			{
				Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier));//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier));
			}

			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();


			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor.Select.AutoSelectModMeshOrModBone();

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					Editor.Select.MeshGroup,
			//					Editor.Select.Modifier,
			//					Editor.Select.SubEditedParamSetGroup,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			return newModMesh;
		}

		/// <summary>
		/// 현재의 Modifier와 선택한 Bone을 연동하여 ModBone을 생성하여 추가한다.
		/// </summary>
		public void AddModBone_WithSelectedBone()
		{
			if (Editor.Select.SubEditedParamSetGroup == null || Editor.Select.Modifier == null || Editor.Select.MeshGroup == null)
			{
				return;
			}
			if (Editor.Select.Bone == null)
			{
				return;
			}

			//현재 모디파이어에 본을 추가할 수 있는지 확인
			if(!Editor.Select.Modifier.IsTarget_Bone)
			{
				//본을 추가할 수 없다.
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_AddModMeshToParamSet, 
											Editor, 
											Editor.Select.Modifier, 
											//Editor.Select.Bone, 
											false,
											apEditorUtil.UNDO_STRUCT.ValueOnly);

			apBone bone = Editor.Select.Bone;

			List<apModifierParamSet> modParamSetList = Editor.Select.SubEditedParamSetGroup._paramSetList;

			for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
			{
				Editor.Select.Modifier.AddBone(bone, modParamSetList[iParam], false, true);
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			if (isChanged)
			{
				apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier);

				Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			}


			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();


			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//TODO : 원래는 AutoSelectModMesh가 들어와야 하지만 -> 여기서는 ModBone이 선택되어야 한다.
			//ModBone이 apSelection에 존재하도록 설정할 것
			//Editor.Select.AutoSelectModMesh();
			Editor.Select.AutoSelectModMeshOrModBone();

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					Editor.Select.MeshGroup,
			//					Editor.Select.Modifier,
			//					Editor.Select.SubEditedParamSetGroup,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);
		}


		//추가 20.6.4 : 다중 선택된 객체들을 대상으로 한번에 ModMesh, ModBone으로 등록한다.		
		public void AddModMeshesBones_WithMultipleSelected(	bool isUndo = true,
															bool isAddModMesh = true,
															bool isAddModBone = true)
		{
			if (Editor.Select.SubEditedParamSetGroup == null || Editor.Select.Modifier == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//Undo
			if(isUndo)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_AddModMeshToParamSet, 
												Editor, 
												Editor.Select.Modifier, 
												//null,
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			apModifierParamSetGroup targetParamSetGroup = Editor.Select.SubEditedParamSetGroup;
			apMeshGroup targetMeshGroup = Editor.Select.MeshGroup;
			apModifierBase modifier = Editor.Select.Modifier;

			//apModifiedMesh addedModMesh = null;
			List<apModifierParamSet> modParamSetList = targetParamSetGroup._paramSetList;

			bool isAnyAdded = false;

			apModifiedMesh newModMesh = null;
			apModifiedBone newModBone = null;

			bool isTarget_Bone = modifier.IsTarget_Bone;
			bool isTarget_MeshTransform = modifier.IsTarget_MeshTransform;
			bool isTarget_MeshGroupTransform = modifier.IsTarget_MeshGroupTransform;
			bool isTarget_ChildMeshTransform = modifier.IsTarget_ChildMeshTransform;

			int nMeshTF = isTarget_MeshTransform ? Editor.Select.SubObjects.NumMeshTF : 0;
			int nMeshGroupTF = isTarget_MeshGroupTransform ? Editor.Select.SubObjects.NumMeshGroupTF : 0;
			int nBones = isTarget_Bone ? Editor.Select.SubObjects.NumBone : 0;

			if(nMeshTF > 0 && isAddModMesh)
			{
				List<apTransform_Mesh> meshTFs = Editor.Select.SubObjects.AllMeshTFs;
				apTransform_Mesh curMeshTF = null;
				apRenderUnit targetRenderUnit = null;
				for (int iMeshTF = 0; iMeshTF < nMeshTF; iMeshTF++)
				{
					curMeshTF = meshTFs[iMeshTF];
					if(curMeshTF == null) { continue; }

					if(targetParamSetGroup.IsMeshTransformContain(curMeshTF))
					{
						//이미 등록되었다.
						continue;
					}

					//Child Mesh 허용여부에 따라 RenderUnit을 가져오는게 다르다.
					if (isTarget_ChildMeshTransform)	{ targetRenderUnit = targetMeshGroup.GetRenderUnit(curMeshTF); }//Child 허용
					else								{ targetRenderUnit = targetMeshGroup.GetRenderUnit_NoRecursive(curMeshTF); }

					if (targetRenderUnit == null)
					{
						//추가할 수 없는 MeshTF이다.
						continue;
					}

					//추가 20.9.8 : "리깅된 자식 메시그룹의 메시가 리깅된 상태"에서는 TF 모디파이어에 등록할 수 없다.
					if(modifier.ModifierType == apModifierBase.MODIFIER_TYPE.TF ||
						modifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedTF)
					{
						if(curMeshTF.IsRiggedChildMeshTF(targetMeshGroup))
						{
							//조건에 맞으면 모디파이어에 등록 불가
							//Debug.LogError("리깅된 자식 본은 추가안된다.");
							continue;
						}
					}

					for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
					{
						newModMesh = modifier.AddMeshTransform(	targetMeshGroup,
																curMeshTF,
																modParamSetList[iParam],
																false,
																modifier.IsTarget_ChildMeshTransform,
																true);

						if (newModMesh != null)
						{
							isAnyAdded = true;
						}
					}
				}
			}

			if(nMeshGroupTF > 0 && isAddModMesh)
			{
				List<apTransform_MeshGroup> meshGroupTFs = Editor.Select.SubObjects.AllMeshGroupTFs;
				apTransform_MeshGroup curMeshGroupTF = null;

				for (int iMeshGroupTF = 0; iMeshGroupTF < nMeshGroupTF; iMeshGroupTF++)
				{
					curMeshGroupTF = meshGroupTFs[iMeshGroupTF];
					if (curMeshGroupTF == null) { continue; }

					if(targetParamSetGroup.IsMeshGroupTransformContain(curMeshGroupTF))
					{
						//이미 등록되었다.
						continue;
					}

					for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
					{
						newModMesh = modifier.AddMeshGroupTransform(	targetMeshGroup,
																		curMeshGroupTF,
																		modParamSetList[iParam],
																		false,
																		true);
						if (newModMesh != null)
						{
							isAnyAdded = true;
						}
					}
				}
			}

			if (nBones > 0 && isAddModBone)
			{
				List<apBone> bones = Editor.Select.SubObjects.AllBones;
				apBone curBone = null;
				for (int iBone = 0; iBone < nBones; iBone++)
				{
					curBone = bones[iBone];
					if (curBone == null) { continue; }
					if(targetParamSetGroup.IsBoneContain(curBone))
					{
						//이미 등록되었다.
						continue;
					}

					for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
					{
						newModBone = modifier.AddBone(curBone, modParamSetList[iParam], false, true);
						if(newModBone != null)
						{
							isAnyAdded = true;
						}
					}
				}
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			
			if (isChanged || isAnyAdded)
			{
				Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier));//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier));
			}


			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();


			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor.Select.AutoSelectModMeshOrModBone();
			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					targetMeshGroup,
			//					modifier,
			//					targetParamSetGroup,
			//					null,
			//					true);


			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);
		}







		public void AddControlParamToModifier(apControlParam controlParam, bool isUseCurrentKey = true, bool isAddCurrentMeshToParamSet = true)
		{
			//이 ControlParam에 해당하는 ParamSetGroup이 있는지 체크한다.
			if (Editor.Select.Modifier == null)
			{
				Debug.LogError("AddControlParamToModifier -> No Modifier");
				return;
			}

			apModifierBase modifier = Editor.Select.Modifier;

			//Undo
			apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Modifier_LinkControlParam, 
														Editor, 
														modifier._meshGroup, 
														modifier,
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);


			apModifierParamSetGroup paramSetGroup = modifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
			{
				return a._syncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller &&
						a._keyControlParam == controlParam;
			});


			apModifierParamSet targetParamSet = null;

			if (paramSetGroup == null)
			{
				//없다면 만들어주자
				paramSetGroup = new apModifierParamSetGroup(Editor._portrait, modifier, modifier.GetNextParamSetLayerIndex());
				paramSetGroup.SetController(controlParam);

				modifier._paramSetGroup_controller.Add(paramSetGroup);
				Editor.Select.SelectParamSetGroupOfModifier(paramSetGroup);
			}
			else
			{
				//추가 3.22
				//있다면 >> 만약 Key를 추가할 때 목표 Key와 동일한 Key가 이미 있다면 불가
				if (paramSetGroup._paramSetList != null && paramSetGroup._paramSetList.Count > 0)
				{
					//"현재"와 동일한 키가 있는지 확인하자
					//또는 기본값과 동일한지 체크
					apModifierParamSet existedParamSet = null;
					for (int i = 0; i < paramSetGroup._paramSetList.Count; i++)
					{
						existedParamSet = paramSetGroup._paramSetList[i];
						//ParamSet으로 새로 추가하고자 하는 컨트롤러의 목표 값(현재 또는 기본값)이 이미 등록된 ParamSet의 값과 같다면
						//이 값으로 ParamSet으로 새로 추가하면 안된다.
						bool isSameKey = false;
						float bias = 0.001f;

						switch (controlParam._valueType)
						{
							case apControlParam.TYPE.Int:
								if (isUseCurrentKey)
								{
									isSameKey = existedParamSet._conSyncValue_Int == controlParam._int_Cur;
								}
								else
								{
									isSameKey = existedParamSet._conSyncValue_Int == controlParam._int_Def;
								}
								break;
							case apControlParam.TYPE.Float:
								if (controlParam._snapSize > 0)
								{
									//Bias를 결정
									bias = Mathf.Min(bias, Mathf.Abs((controlParam._float_Max - controlParam._float_Min) / controlParam._snapSize) * 0.2f);
								}

								if (isUseCurrentKey)
								{
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Float - controlParam._float_Cur) < bias;
								}
								else
								{
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Float - controlParam._float_Def) < bias;
								}
								break;
							case apControlParam.TYPE.Vector2:
								if (controlParam._snapSize > 0)
								{
									//Bias를 결정
									bias = Mathf.Min(bias,
										Mathf.Abs((controlParam._vec2_Max.x - controlParam._vec2_Min.x) / controlParam._snapSize) * 0.2f,
										Mathf.Abs((controlParam._vec2_Max.y - controlParam._vec2_Min.y) / controlParam._snapSize) * 0.2f
										);
								}

								if (isUseCurrentKey)
								{
									//X, Y 좌표가 모두 같아야함
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Vector2.x - controlParam._vec2_Cur.x) < bias
												&& Mathf.Abs(existedParamSet._conSyncValue_Vector2.y - controlParam._vec2_Cur.y) < bias;
								}
								else
								{
									//X, Y 좌표가 모두 같아야함
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Vector2.x - controlParam._vec2_Def.x) < bias
												&& Mathf.Abs(existedParamSet._conSyncValue_Vector2.y - controlParam._vec2_Def.y) < bias;
								}
								break;
						}

						if (isSameKey)
						{
							//키가 같다 => 새로 ParamSet을 추가할 필요가 없다.
							targetParamSet = existedParamSet;
							//Debug.LogError("동일한 값의 Key가 이미 추가되어 있다! 3.22");
							break;
						}
					}

				}
			}

			//추가 3.22 : 무조건 Key (ParamSet)를 생성하지 말고, 기존과 겹친다면 패스
			if (targetParamSet == null)
			{
				//Key를 추가하자

				apModifierParamSet newParamSet = new apModifierParamSet();

				newParamSet.LinkParamSetGroup(paramSetGroup);//Link도 해준다.

				
															 
				switch (controlParam._valueType)
				{
					case apControlParam.TYPE.Int:
						if (isUseCurrentKey)
						{
							newParamSet._conSyncValue_Int = controlParam._int_Cur;
						}
						else
						{
							//추가 3.22 : 기본값으로 생성하는 기능
							newParamSet._conSyncValue_Int = controlParam._int_Def;
						}

						break;

					case apControlParam.TYPE.Float:
						if (isUseCurrentKey)
						{
							newParamSet._conSyncValue_Float = controlParam._float_Cur;
						}
						else
						{
							//추가 3.22 : 기본값으로 생성하는 기능
							newParamSet._conSyncValue_Float = controlParam._float_Def;
						}

						break;

					case apControlParam.TYPE.Vector2:
						if (isUseCurrentKey)
						{
							newParamSet._conSyncValue_Vector2 = controlParam._vec2_Cur;
						}
						else
						{
							//추가 3.22 : 기본값으로 생성하는 기능
							newParamSet._conSyncValue_Vector2 = controlParam._vec2_Def;
						}

						break;
				}


				paramSetGroup._paramSetList.Add(newParamSet);

				//[v1.5.0]
				//중간 생성 Mod Mesh/Bone의 보간을 위해서
				//새로 생성되는 ParamSet 주위의 Key들의 가중치를 구하자
				//이 단계에서는 Control Param 보간 영역 (CalculatedLerpPoint)가 갱신되지 않는다.
				//모디파이어 로직의 Calculate의 코드(apCalculatedResultParamSubList > CalculateWeiht_ControlParam)를 참조하자
				//CalResultParam은 불안정해서, 가능하면 보간 로직을 여기서 재현하자. (근데 Vector2 타입은???)
				int nPSList = paramSetGroup._paramSetList.Count;
				
				//현재 컨트롤 파라미터와의 거리를 바탕으로 ParamSet-Weight를 구한다.
				Dictionary<apModifierParamSet, float> adjacentParamSets = GetSimulatedWeightParamSetsWhenParamSetAdded(	modifier,
																														paramSetGroup,
																														newParamSet,
																														controlParam);

				//paramSetGroup.RefreshSync();//이전
				Dictionary<apModifierParamSet, List<apModifiedMesh>> addedModMeshes = new Dictionary<apModifierParamSet, List<apModifiedMesh>>();
				Dictionary<apModifierParamSet, List<apModifiedBone>> addedModBones = new Dictionary<apModifierParamSet, List<apModifiedBone>>();
				
				paramSetGroup.RefreshSyncWithNewMod(addedModMeshes, addedModBones);//변경
				

				//v1.5.0 : 새로운 ParamSet + Mod Mesh/Bone에 대해서 앞뒤 보간을 한다.
				//에디터 옵션도 활성화 되어 있어야 한다.
				if(Editor._option_NewControlParamModMeshBoneBlended//에디터 옵션
					&& (addedModMeshes.Count > 0 || addedModBones.Count > 0)
					&& adjacentParamSets.Count > 0)
				{
					//추가된 ParamSet을 기준으로 인접한 ParamSet으로 부터의 가중치를 이용해서 새로운 ModMesh/Bone의 값을 보간한다.
					MakeInterpolatedNewModMeshesAndBones(	modifier, paramSetGroup, newParamSet,
															addedModMeshes, addedModBones,
															adjacentParamSets);
				}


				targetParamSet = newParamSet;
			}


			//만약, 현재 선택중인 Mesh나 MeshGroup이 ModMesh로서 ParamSetList에 없다면 추가해주는 것도 좋을 것 같다.
			//추가 3.22 : 현재는 무조건 추가지만, isAddCurrentMeshToParamSet의 값이 true인 경우에 한해서만..
			//True : 컨트롤 파라미터를 등록하면서 선택된 메시/본들"도" 같이 등록하기
			//False : 컨트롤 파라미터"만" 등록하기
			if (isAddCurrentMeshToParamSet)
			{	
				//변경 20.7.15 : 다중 선택 지원
				int nMeshTF = Editor.Select.SubObjects.NumMeshTF;
				int nMeshGroupTF = Editor.Select.SubObjects.NumMeshGroupTF;
				int nBone = Editor.Select.Modifier.IsTarget_Bone ? Editor.Select.SubObjects.NumBone : 0;

				if(nMeshTF > 0 || nMeshGroupTF > 0 || nBone > 0)
				{
					//하나라도 선택 되었다면 추가 후 리프레시를 하자
					//여기서는 보간을 하지 않아도 된다.
					AddModMeshesBones_WithMultipleSelected();
					paramSetGroup.RefreshSync();
				}
			}

			Editor.Select.SelectParamSetGroupOfModifier(paramSetGroup);
			Editor.Select.SelectParamSetOfModifier(targetParamSet);

			//Link를 하자
			apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier);

			Editor.Select.Modifier.RefreshParamSet(apUtil.LinkRefresh);

			

			Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);//<<Link 전에 이걸 먼저 선언한다.
			Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);//<<여기서 Mod + Control Param Lerp Area가 갱신된다.


			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();


			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					Editor.Select.MeshGroup,
			//					Editor.Select.Modifier,
			//					paramSetGroup,
			//					null,
			//					true);


			//컨트롤 파라미터 > PSG 매핑 갱신
			Editor.Select.RefreshControlParam2PSGMapping();

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);



			Editor.SetRepaint();
		}


		//[v1.5.0 추가]
		//컨트롤 파라미터 키를 추가하여 ParamSet을 생성할 때, 주변의 ParamSet으로 부터 ModMesh/Bone에 보간된 값을 넣기 위한 함수.
		//추가된 ParamSet이 주변의 어떤 ParamSet들로 부터 어떤 Weight를 가진 상태로 보간되는지 시뮬레이션한다.
		//apCalculatedResyltParamSubList의 Calculate를 시뮬레이션한 함수다.
		private Dictionary<apModifierParamSet, float> GetSimulatedWeightParamSetsWhenParamSetAdded(	apModifierBase modifier,
																									apModifierParamSetGroup paramSetGroup,
																									apModifierParamSet newParamSet,
																									apControlParam controlParam)
		{
			//시뮬레이터를 이용해서 보간 코드 작성
			Dictionary<apModifierParamSet, float> result = apParamSetLerpSimulator.GetSimulatedWeightParamSets(	modifier,
																												paramSetGroup,
																												newParamSet,
																												controlParam);

			return result;
		}

		//위 정보를 바탕으로 새로운 Mod Mesh/Bone들에 보간된 정보를 입력한다.
		private void MakeInterpolatedNewModMeshesAndBones(	apModifierBase modifier,
															apModifierParamSetGroup paramSetGroup,
															apModifierParamSet newParamSet,
															Dictionary<apModifierParamSet, List<apModifiedMesh>> addedModMeshes,
															Dictionary<apModifierParamSet, List<apModifiedBone>> addedModBones,
															Dictionary<apModifierParamSet, float> adjacentParamSets
															)
		{
			//새로 추가된 Mod Mesh / Bone들에 대해서만 보간을 하자 (대상 ParamSet의 값이어야 한다.)
			List<apModifiedMesh> newModMeshes = null;
			List<apModifiedBone> newModBones = null;
			if(addedModMeshes != null)
			{
				addedModMeshes.TryGetValue(newParamSet, out newModMeshes);
			}
			if(addedModBones != null)
			{
				addedModBones.TryGetValue(newParamSet, out newModBones);
			}
			
			int nNewModMeshes = newModMeshes != null ? newModMeshes.Count : 0;
			int nNewModBones = newModBones != null ? newModBones.Count : 0;

			if(nNewModMeshes == 0 && nNewModBones == 0)
			{
				return;
			}

			//Matrix 계산은 이걸 이용한다. (apModifierBase 참조)
			apMatrixCal tmpMatrix = new apMatrixCal();
			apMatrixCal tmpMatrix_Step2 = new apMatrixCal();


			//1. Vert + Pin (Morph)
			//2. Transform Matrix
			//3. Color
			//(Extra Option은 보간되지 않는다.)
			bool isTarget_VertPin = false;
			bool isTarget_TF = false;
			bool isTarget_Color = false;
			bool isShowHideToggle = false;
			//if(modifier.ModifiedValueType.HasFlag(apModifiedMesh.MOD_VALUE_TYPE.VertexPosList))
			if((int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0)
			{
				isTarget_VertPin = true;
			}

			//if(modifier.ModifiedValueType.HasFlag(apModifiedMesh.MOD_VALUE_TYPE.TransformMatrix))
			if((int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.TransformMatrix) != 0)
			{
				isTarget_TF = true;
			}

			//if(modifier.ModifiedValueType.HasFlag(apModifiedMesh.MOD_VALUE_TYPE.Color))
			if((int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.Color) != 0)
			{
				isTarget_Color = true;
				if(paramSetGroup._isToggleShowHideWithoutBlend)
				{
					isShowHideToggle = true;
				}
			}

			//추가 21.9.1 : <회전 보정>
			bool isRotation180Correction = paramSetGroup._tfRotationLerpMethod == apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.RotationByVector;
			float correct_DeltaAngle = 0.0f;
			Vector2 correct_SumVector = Vector2.zero;

			//Show/Hide 토글 변수
			Color tmpColor = Color.clear;
			bool tmpIsVisible = false;
			bool tv_IsAny_Shown = false;
			float tv_TotalWeight_Shown = 0.0f;
			float tv_MaxWeight_Shown = 0.0f;
			float tv_KeyIndex_Shown = 0.0f;
			bool tv_IsAny_Hidden = false;
			float tv_MaxWeight_Hidden = 0.0f;
			float tv_KeyIndex_Hidden = 0.0f;
			


			//1. Mod Mesh
			if(nNewModMeshes > 0)
			{
				//Mod Mesh의 경우 다음의 항목이 보간 대상이다.
				for (int iModMesh = 0; iModMesh < nNewModMeshes; iModMesh++)
				{
					apModifiedMesh modMesh = newModMeshes[iModMesh];
					
					apRenderUnit targetRenderUnit = modMesh._renderUnit;
					if(targetRenderUnit == null)
					{
						continue;
					}

					//기본 정보 및 초기화
					int nVerts = modMesh._vertices != null ? modMesh._vertices.Count : 0;
					int nPins = modMesh._pins != null ? modMesh._pins.Count : 0;

					//TF용 Matrix
					apMatrix defaultMatrixOfRenderUnit = null;
					if(targetRenderUnit._meshTransform != null)
					{
						defaultMatrixOfRenderUnit = targetRenderUnit._meshTransform._matrix_TF_ToParent;
					}
					else if(targetRenderUnit._meshGroupTransform != null)
					{
						defaultMatrixOfRenderUnit = targetRenderUnit._meshGroupTransform._matrix_TF_ToParent;
					}

					//1. Vertices / Pin (Morph) 초기화
					if(isTarget_VertPin)
					{
						apModifiedVertex curModVert = null;
						apModifiedPin curModPin = null;
						if(nVerts > 0)
						{
							for (int iVert = 0; iVert < nVerts; iVert++)
							{
								curModVert = modMesh._vertices[iVert];
								curModVert._deltaPos = Vector2.zero;//초기화
							}
						}

						if(nPins > 0)
						{
							for (int iPin = 0; iPin < nPins; iPin++)
							{
								curModPin = modMesh._pins[iPin];
								curModPin._deltaPos = Vector2.zero;
							}
						}
					}

					//2. TF 초기화
					if(isTarget_TF)
					{
						modMesh._transformMatrix.SetZero();
						tmpMatrix.SetZero();
						tmpMatrix_Step2.SetZero();

						//회전 보정 옵션 (Vector 타입의 각도 보정)
						correct_DeltaAngle = 0.0f;
						correct_SumVector = Vector2.zero;
					}

					//3. Color 초기화
					if(isTarget_Color)
					{
						//modMesh._isVisible = false;//일단 안보이는 것 부터 시작 (하나라도 보이면 Show)
						//modMesh._meshColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

						//바로 설정하지 않고 Tmp 이용
						tmpColor = Color.clear;
						tmpIsVisible = false;

						tv_IsAny_Shown = false;
						tv_TotalWeight_Shown = 0.0f;
						tv_MaxWeight_Shown = 0.0f;
						tv_KeyIndex_Shown = 0.0f;
						tv_IsAny_Hidden = false;
						tv_MaxWeight_Hidden = 0.0f;
						tv_KeyIndex_Hidden = 0.0f;
					}

					foreach (KeyValuePair<apModifierParamSet, float> PSPair in adjacentParamSets)
					{
						apModifierParamSet adjPS = PSPair.Key;
						float weight = PSPair.Value;

						List<apModifiedMesh> adjModMeshes = adjPS._meshData;

						if(adjModMeshes == null) { continue; }

						if(weight <= 0.0f)
						{
							continue;
						}

						//이 RenderUnit에 해당하는 인접한 ModMesh를 찾자
						s_FindAdjacentModMesh_RenderUnit = modMesh._renderUnit;
						apModifiedMesh adjModMesh = adjModMeshes.Find(s_FindAdjacentModMeshByRenderUnit_Func);

						if(adjModMesh == null)
						{
							continue;
						}

						//1. Vert / Pin
						if(isTarget_VertPin)
						{
							int nAdjVerts = adjModMesh._vertices != null ? adjModMesh._vertices.Count : 0;
							int nAdjPins = adjModMesh._pins != null ? adjModMesh._pins.Count : 0;

							//Vertex/Pin이 다를 수 있으므로, 일일이 찾아서 할당하자
							if(nVerts > 0 && nAdjVerts > 0)
							{
								apModifiedVertex curModVert = null;
								apModifiedVertex adjModVert = null;
								for (int iVert = 0; iVert < nVerts; iVert++)
								{
									curModVert = modMesh._vertices[iVert];
									adjModVert = adjModMesh._vertices.Find(delegate(apModifiedVertex a)
									{
										return a._vertex == curModVert._vertex;
									});

									if(adjModVert != null)
									{
										curModVert._deltaPos += adjModVert._deltaPos * weight;//Vertex(*Weight) 더하기
									}
								}
							}

							if(nPins > 0 && nAdjPins > 0)
							{
								apModifiedPin curModPin = null;
								apModifiedPin adjModPin = null;

								for (int iPin = 0; iPin < nPins; iPin++)
								{
									curModPin = modMesh._pins[iPin];
									adjModPin = adjModMesh._pins.Find(delegate(apModifiedPin a)
									{
										return a._pin == curModPin._pin;
									});

									if(adjModPin != null)
									{
										curModPin._deltaPos += adjModPin._deltaPos * weight;//Pin(*Weight) 더하기
									}
								}
							}
						}

						//2. TF
						if(isTarget_TF)
						{
							tmpMatrix.AddMatrixParallel_ModMesh(	adjModMesh._transformMatrix, 
																	defaultMatrixOfRenderUnit, 
																	weight);

							//회전 보정 처리도 해야한다.
							if(isRotation180Correction)
							{
								//회전 보정 옵션 (Vector 타입의 각도 보정)
								float curAngle = adjModMesh._transformMatrix._angleDeg * Mathf.Deg2Rad;
								Vector2 curVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));

								//Weight가 추가된 벡터합
								//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
								correct_SumVector += curVector * weight * 10.0f;
							}
						}

						//3. Color
						if(isTarget_Color)
						{	
							Color adjColor = adjModMesh._meshColor;
							bool adjVisible = adjModMesh._isVisible;
							if(!isShowHideToggle)
							{
								// 일반 방식의 Color/Visible 계산
								//하나만 Visible이면 True								
								if(adjVisible)
								{
									tmpIsVisible = true;
								}
								else
								{
									adjColor.a = 0.0f;
								}
								tmpColor += adjColor * weight;
							}
							else
							{
								//토글 방식의 Color / Visible 계산
								if(adjVisible)
								{
									//Show
									tmpColor += adjColor * weight;
									tmpIsVisible = true;

									//토글용 처리
									float keyIndex = adjPS.ComparableIndex;
									if(!tv_IsAny_Shown)
									{
										tv_KeyIndex_Shown = keyIndex;
									}
									else
									{
										tv_KeyIndex_Shown = Mathf.Min(keyIndex, tv_KeyIndex_Shown);
									}

									tv_IsAny_Shown = true;
									tv_TotalWeight_Shown += weight;
									tv_MaxWeight_Shown = Mathf.Max(tv_MaxWeight_Shown, weight);
								}
								else
								{
									//Hide
									//토글용 처리
									float keyIndex = adjPS.ComparableIndex;
									if(!tv_IsAny_Hidden)
									{
										tv_KeyIndex_Hidden = keyIndex;
									}
									else
									{
										tv_KeyIndex_Hidden = Mathf.Max(keyIndex, tv_KeyIndex_Hidden);
									}
									tv_IsAny_Hidden = true;
									tv_MaxWeight_Hidden = Mathf.Max(weight, tv_MaxWeight_Hidden);
								}
							}
							
							//modMesh._meshColor += adjColor * weight;
						}
					}


					//색상을 적용한다.
					if(isTarget_Color)
					{	
						Color resultColor = tmpColor;
						bool resultVisible = tmpIsVisible;

						//만약 토글 방식이라면
						if(isShowHideToggle)
						{
							if(tv_IsAny_Shown && tv_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (tv_MaxWeight_Shown > tv_MaxWeight_Hidden)
								{
									//Show가 더 크다
									resultVisible = true;
								}
								else if (tv_MaxWeight_Shown < tv_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									resultVisible = false;
									resultColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (tv_KeyIndex_Shown > tv_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										resultVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										resultVisible = false;
										resultColor = Color.clear;
									}
								}
							}
							else if (tv_IsAny_Shown && !tv_IsAny_Hidden)
							{
								//Show만 있다면
								resultVisible = true;
							}
							else if (!tv_IsAny_Shown && tv_IsAny_Hidden)
							{
								//Hide만 있다면
								resultVisible = false;
								resultColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								resultVisible = false;
								resultColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (resultVisible && tv_TotalWeight_Shown > 0.0f)
							{
								resultColor.r = Mathf.Clamp01(resultColor.r / tv_TotalWeight_Shown);
								resultColor.g = Mathf.Clamp01(resultColor.g / tv_TotalWeight_Shown);
								resultColor.b = Mathf.Clamp01(resultColor.b / tv_TotalWeight_Shown);
								resultColor.a = Mathf.Clamp01(resultColor.a / tv_TotalWeight_Shown);
							}
						}

						modMesh._meshColor = resultColor;
						modMesh._isVisible = resultVisible;
					}

					//TF의 경우엔 추가 보간 처리가 필요하다.
					if(isTarget_TF)
					{
						if(isRotation180Correction)
						{
							correct_DeltaAngle = 0.0f;

							//회전 보정 옵션 (Vector 타입의 각도 보정)
							if(correct_SumVector.sqrMagnitude > 0.001f)
							{
								correct_SumVector *= 10.0f;
								correct_DeltaAngle = Mathf.Atan2(correct_SumVector.y, correct_SumVector.x) * Mathf.Rad2Deg;
							}
							tmpMatrix._angleDeg = correct_DeltaAngle;
						}

						tmpMatrix.CalculateScale_FromAdd();
						tmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit);
						tmpMatrix_Step2.SetTRSForLerp(tmpMatrix);
						tmpMatrix_Step2.CalculateScale_FromLerp();

						modMesh._transformMatrix.LerpMartixCal(tmpMatrix_Step2, 1.0f);
					}

					
				}
			}

			//2. Mod Bone
			if(nNewModBones > 0 && isTarget_TF)
			{
				for (int iModBone = 0; iModBone < nNewModBones; iModBone++)
				{
					apModifiedBone modBone = newModBones[iModBone];

					apBone targetBone = modBone._bone;
					if(targetBone == null)
					{
						continue;
					}

					modBone._transformMatrix.SetZero();
					tmpMatrix.SetZero();
					tmpMatrix_Step2.SetZero();

					//회전 보정 옵션 (Vector 타입의 각도 보정)
					correct_DeltaAngle = 0.0f;
					correct_SumVector = Vector2.zero;
					//디버깅
					
					foreach (KeyValuePair<apModifierParamSet, float> PSPair in adjacentParamSets)
					{
						apModifierParamSet adjPS = PSPair.Key;
						float weight = PSPair.Value;

						List<apModifiedBone> adjModBones = adjPS._boneData;

						if(adjModBones == null) { continue; }

						//이 Bone에 해당하는 ModBone을 찾자
						s_FindAdjacentModBone_Bone = targetBone;
						apModifiedBone adjModBone = adjModBones.Find(s_FindAdjacentModBoneByBone_Func);

						if(adjModBone == null)
						{
							continue;
						}
					}

					foreach (KeyValuePair<apModifierParamSet, float> PSPair in adjacentParamSets)
					{
						apModifierParamSet adjPS = PSPair.Key;
						float weight = PSPair.Value;

						List<apModifiedBone> adjModBones = adjPS._boneData;

						if(adjModBones == null) { continue; }

						//이 Bone에 해당하는 ModBone을 찾자
						s_FindAdjacentModBone_Bone = targetBone;
						apModifiedBone adjModBone = adjModBones.Find(s_FindAdjacentModBoneByBone_Func);

						if(adjModBone == null)
						{
							continue;
						}

						tmpMatrix.AddMatrixParallel_ModBone(adjModBone._transformMatrix, weight);

						//회전 보정 처리도 해야한다.
						if(isRotation180Correction)
						{
							//회전 보정 옵션 (Vector 타입의 각도 보정)
							float curAngle = adjModBone._transformMatrix._angleDeg * Mathf.Deg2Rad;
							Vector2 curVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));
							//Weight가 추가된 벡터합
							//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
							correct_SumVector += curVector * weight * 10.0f;
						}
					}

					if(isRotation180Correction)
					{
						correct_DeltaAngle = 0.0f;

						//회전 보정 옵션 (Vector 타입의 각도 보정)
						if(correct_SumVector.sqrMagnitude > 0.001f)
						{
							correct_SumVector *= 10.0f;
							correct_DeltaAngle = Mathf.Atan2(correct_SumVector.y, correct_SumVector.x) * Mathf.Rad2Deg;
						}
						tmpMatrix._angleDeg = correct_DeltaAngle;
					}

					tmpMatrix.CalculateScale_FromAdd();
					tmpMatrix_Step2.SetTRSForLerp(tmpMatrix);
					tmpMatrix_Step2.CalculateScale_FromLerp();

					modBone._transformMatrix.LerpMartixCal(tmpMatrix_Step2, 1.0f);


				}
			}
		}
		
		private static apRenderUnit s_FindAdjacentModMesh_RenderUnit = null;
		private Predicate<apModifiedMesh> s_FindAdjacentModMeshByRenderUnit_Func = FUNC_FindAdjacentModMeshByRenderUnit;
		private static bool FUNC_FindAdjacentModMeshByRenderUnit(apModifiedMesh a)
		{
			return a._renderUnit == s_FindAdjacentModMesh_RenderUnit;
		}

		private static apBone s_FindAdjacentModBone_Bone = null;
		private Predicate<apModifiedBone> s_FindAdjacentModBoneByBone_Func = FUNC_FindAdjacentModBoneByBone;
		private static bool FUNC_FindAdjacentModBoneByBone(apModifiedBone a)
		{
			return a._bone == s_FindAdjacentModBone_Bone;
		}





		/// <summary>
		/// Static 타입 (아무런 입력 연동값이 없음)의 ParamSetGroup을 Modifier에 등록한다.
		/// Static 타입은 ParamSetGroup(연동 오브젝트) 1개와 ParamSet(키값) 1개만 가진다. (그 아래 ModifiedMesh를 여러개 가진다)
		/// </summary>
		public void AddStaticParamSetGroupToModifier()
		{
			//이 ControlParam에 해당하는 ParamSetGroup이 있는지 체크한다.
			if (Editor.Select.Modifier == null)
			{
				Debug.LogError("AddStaticParamSetGroupToModifier -> No Modifier");
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroupAndModifier(	apUndoGroupData.ACTION.Modifier_AddStaticParamSetGroup, 
															Editor, 
															Editor.Select.Modifier._meshGroup, 
															Editor.Select.Modifier, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);


			if (Editor.Select.Modifier._paramSetGroup_controller.Count > 0)
			{
				//Static 타입은 한개의 ParamSetGroup만 적용한다.
				return;
			}

			apModifierParamSetGroup paramSetGroup = new apModifierParamSetGroup(Editor._portrait, Editor.Select.Modifier, Editor.Select.Modifier.GetNextParamSetLayerIndex());
			paramSetGroup.SetStatic();//<Static 타입

			Editor.Select.Modifier._paramSetGroup_controller.Add(paramSetGroup);
			Editor.Select.SelectParamSetGroupOfModifier(paramSetGroup);



			//Static 타입은 한개의 ParamSet을 가진다.

			apModifierParamSet newParamSet = new apModifierParamSet();

			newParamSet.LinkParamSetGroup(paramSetGroup);//Link도 해준다.
			paramSetGroup._paramSetList.Add(newParamSet);
			paramSetGroup.RefreshSync();


			Editor.Select.SelectParamSetGroupOfModifier(paramSetGroup);
			Editor.Select.SelectParamSetOfModifier(newParamSet);

			//Link를 하자
			apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier);

			Editor.Select.Modifier.RefreshParamSet(apUtil.LinkRefresh);

			

			Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);//<<Link 전에 이걸 먼저 선언한다.
			Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					Editor.Select.MeshGroup,
			//					Editor.Select.Modifier,
			//					paramSetGroup,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);


			////추가 21.2.14 : 모디파이어 연결 갱신
			//Editor.Select.ModLinkInfo.LinkRefresh();

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor.SetRepaint();
		}


		//추가 20.1.13
		public void DuplicateModifiersToOtherMeshGroup(	apMeshGroup srcMeshGroup, 
														apMeshGroup dstMeshGroup, 
														MeshGroupDupcliateConvert convertInfo)
		{
			if(srcMeshGroup == null || dstMeshGroup == null || convertInfo == null)
			{
				return;
			}

			List<apModifierBase> srcModifiers = srcMeshGroup._modifierStack._modifiers;
			int nSrcModifiers = srcModifiers.Count;
			apModifierBase srcModifier = null;
			apModifierBase dstModifier = null;
			for (int iSrcMod = 0; iSrcMod < nSrcModifiers; iSrcMod++)
			{
				srcModifier = srcModifiers[iSrcMod];
				if(srcModifier == null)
				{
					continue;
				}

				//없다면 만들자
				_requestedModType = (int)srcModifier.ModifierType;
				_requestedModValidationKey = -1;
				apVersion.I.RequestAddableModifierTypes(OnModifierValidationKeyCheck);
				//모디파이어를 추가하기 위한 Key가 적용되었을 것

				dstModifier = AddModifierToMeshGroup(dstMeshGroup, srcModifier.ModifierType, _requestedModValidationKey, false, false, false);
				if(dstModifier == null)
				{
					Debug.LogError("AnyPortrait : Duplicating Modifier is failed. Please try again.");
					continue;
				}

				convertInfo.Src2Dst_Modifier.Add(srcModifier, dstModifier);//변환 리스트에 입력

				//1. 기본 속성을 직접 복사하기
				dstModifier._portrait = srcModifier._portrait;
				dstModifier._layer = srcModifier._layer;
				dstModifier._layerWeight = srcModifier._layerWeight;
				dstModifier._blendMethod = srcModifier._blendMethod;

				dstModifier._isColorPropertyEnabled = srcModifier._isColorPropertyEnabled;
				dstModifier._isExtraPropertyEnabled = srcModifier._isExtraPropertyEnabled;

				//2. ParamSetGroup을 추가하고 복사한다. 단 AnimClip은 나중에
				int nParamSetGroups = srcModifier._paramSetGroup_controller == null ? 0 : srcModifier._paramSetGroup_controller.Count;

				apModifierParamSetGroup srcPSG = null;
				apModifierParamSetGroup dstPSG = null;
				for (int iSrcPSG = 0; iSrcPSG < nParamSetGroups; iSrcPSG++)
				{
					srcPSG = srcModifier._paramSetGroup_controller[iSrcPSG];
					if(srcPSG == null)
					{
						continue;
					}

					dstPSG = new apModifierParamSetGroup(Editor._portrait, dstModifier, srcPSG._layerIndex);
					if(!dstModifier._paramSetGroup_controller.Contains(dstPSG))
					{
						dstModifier._paramSetGroup_controller.Add(dstPSG);
					}

					//ParamSetGroup을 생성하고 변환 리스트에 넣자
					convertInfo.Src2Dst_ParamSetGroup.Add(srcPSG, dstPSG);//변환 리스트에 입력

					//ParamSetGroup의 정보를 복사하자
					dstPSG._syncTarget = srcPSG._syncTarget;
					dstPSG._keyControlParamID = srcPSG._keyControlParamID;
					dstPSG._keyControlParam = srcPSG._keyControlParam;

					//애니메이션 정보는 일단 생략

					dstPSG._isEnabled = srcPSG._isEnabled;
					dstPSG._layerIndex = srcPSG._layerIndex;
					dstPSG._layerWeight = srcPSG._layerWeight;
					dstPSG._blendMethod = srcPSG._blendMethod;
					dstPSG._isColorPropertyEnabled = srcPSG._isColorPropertyEnabled;

					//ParamSet을 설정하자
					int nSrcParamSets = srcPSG._paramSetList == null ? 0 : srcPSG._paramSetList.Count;

					apModifierParamSet srcParamSet = null;
					apModifierParamSet dstParamSet = null;
					for (int iSrcPS = 0; iSrcPS < nSrcParamSets; iSrcPS++)
					{
						srcParamSet = srcPSG._paramSetList[iSrcPS];
						if(srcParamSet == null)
						{
							continue;
						}

						//ParamSet을 생성하고 변환 리스트에 넣자
						dstParamSet = new apModifierParamSet();
						dstPSG._paramSetList.Add(dstParamSet);

						convertInfo.Src2Dst_ParamSet.Add(srcParamSet, dstParamSet);//변환 리스트에 입력

						dstParamSet._conSyncValue_Int = srcParamSet._conSyncValue_Int;
						dstParamSet._conSyncValue_Float = srcParamSet._conSyncValue_Float;
						dstParamSet._conSyncValue_Vector2 = srcParamSet._conSyncValue_Vector2;

						dstParamSet._conSyncValueRange_Under = srcParamSet._conSyncValueRange_Under;
						dstParamSet._conSyncValueRange_Over = srcParamSet._conSyncValueRange_Over;

						//키프레임은 일단 생략

						dstParamSet._overlapWeight = srcParamSet._overlapWeight;

						//Mesh Data와 Bone Data를 복사하자
						int nSrcModMeshes = srcParamSet._meshData == null ? 0 : srcParamSet._meshData.Count;
						int nSrcModBones = srcParamSet._boneData == null ? 0 : srcParamSet._boneData.Count;

						apModifiedMesh srcModMesh = null;
						apModifiedMesh dstModMesh = null;

						apTransform_Mesh dstModTarget_MeshTF = null;
						apTransform_MeshGroup dstModTarget_MeshGroupTF = null;

						// Mesh Data 복사
						for (int iSrcModMesh = 0; iSrcModMesh < nSrcModMeshes; iSrcModMesh++)
						{
							srcModMesh = srcParamSet._meshData[iSrcModMesh];
							if (srcModMesh == null)
							{
								continue;
							}

							//연결되는 대상 Mesh/MeshGroup Transform
							dstModTarget_MeshTF = null;
							dstModTarget_MeshGroupTF = null;

							if (srcModMesh._transform_Mesh != null
								&& convertInfo.Src2Dst_MeshTransform.ContainsKey(srcModMesh._transform_Mesh))
							{
								dstModTarget_MeshTF = convertInfo.Src2Dst_MeshTransform[srcModMesh._transform_Mesh];
							}
							else if (srcModMesh._transform_MeshGroup != null
								&& convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcModMesh._transform_MeshGroup))
							{
								dstModTarget_MeshGroupTF = convertInfo.Src2Dst_MeshGroupTransform[srcModMesh._transform_MeshGroup];
							}

							//만약 연결 대상이 없다면 패스
							if (dstModTarget_MeshTF == null && dstModTarget_MeshGroupTF == null)
							{
								continue;
							}

							//ModMesh를 생성하고 등록하자
							dstModMesh = new apModifiedMesh();
							if (dstParamSet._meshData == null)
							{
								dstParamSet._meshData = new List<apModifiedMesh>();
							}
							dstParamSet._meshData.Add(dstModMesh);
							convertInfo.Src2Dst_ModMesh.Add(srcModMesh, dstModMesh);//변환 리스트에도 입력

							//값을 복사하자
							dstModMesh._meshGroupUniqueID_Modifier = dstMeshGroup._uniqueID;

							//연결 대상 설정
							if (dstModTarget_MeshTF != null)
							{
								//Mesh Transform을 타겟으로 하는 경우
								dstModMesh._transformUniqueID = dstModTarget_MeshTF._transformUniqueID;
								dstModMesh._meshUniqueID = dstModTarget_MeshTF._meshUniqueID;
								dstModMesh._isMeshTransform = true;
							}
							else
							{
								//MeshGroup Transform을 타겟으로 하는 경우
								dstModMesh._transformUniqueID = dstModTarget_MeshGroupTF._transformUniqueID;
								dstModMesh._meshUniqueID = -1;
								dstModMesh._isMeshTransform = false;
							}

							//메시를 복사할 수 있는가
							//- MeshTransform을 대상 + 동일한 메시를 대상
							bool isMeshDataDuplicatable = dstModMesh._isMeshTransform && dstModMesh._meshUniqueID == srcModMesh._meshUniqueID;

							//그 외의 값을 복사하자
							dstModMesh._isUsePhysicParam = srcModMesh._isUsePhysicParam;

							apPhysicsMeshParam srcPhysicsParam = srcModMesh.PhysicParam;
							apPhysicsMeshParam dstPhysicsParam = dstModMesh.PhysicParam;

							if (srcModMesh._isMeshTransform
								&& srcModMesh._isUsePhysicParam
								&& srcPhysicsParam != null
								&& dstPhysicsParam != null
								&& isMeshDataDuplicatable)
							{
								//물리 파라미터도 복사하자..으아앙
								dstPhysicsParam.CopyFromSrc(srcPhysicsParam);
							}
							dstModMesh._modValueType = srcModMesh._modValueType;


							//대상 Transform의 MeshGroup이 자식 메시 그룹이라면?
							//자식 MeshGroupTransform을 모두 찾아서 해당 Src 및 변환 정보를 찾자
							if (srcModMesh._isRecursiveChildTransform
								&& srcModMesh._meshGroupOfTransform != null)
							{	
								//>> 이거 제대로 작동하는지 테스트하자
								apMeshGroup dstTargetMeshGroupOfRecursive = null;
								if(convertInfo.Src2Dst_MeshGroup.ContainsKey(srcModMesh._meshGroupOfTransform))
								{
									//전체 변환 정보에서 찾는다.
									dstTargetMeshGroupOfRecursive = convertInfo.Src2Dst_MeshGroup[srcModMesh._meshGroupOfTransform];
								}
								if(dstTargetMeshGroupOfRecursive != null)
								{
									dstModMesh._isRecursiveChildTransform = true;
									dstModMesh._meshGroupUniqueID_Transform = dstTargetMeshGroupOfRecursive._uniqueID;
								}
								else
								{
									dstModMesh._isRecursiveChildTransform = false;
									dstModMesh._meshGroupUniqueID_Transform = -1;
								}
								
							}
							else
							{
								dstModMesh._isRecursiveChildTransform = false;
								dstModMesh._meshGroupUniqueID_Transform = -1;
							}

							//Vertex Morph 정보를 복사하자
							if(isMeshDataDuplicatable)
							{
								int nModVerts = srcModMesh._vertices == null ? 0 : srcModMesh._vertices.Count;
								apModifiedVertex srcModVert = null;
								apModifiedVertex dstModVert = null;

								if(nModVerts > 0 && dstModMesh._vertices == null)
								{
									dstModMesh._vertices = new List<apModifiedVertex>();
								}

								for (int iSrcModVert = 0; iSrcModVert < nModVerts; iSrcModVert++)
								{
									srcModVert = srcModMesh._vertices[iSrcModVert];
									dstModVert = new apModifiedVertex();
									dstModMesh._vertices.Add(dstModVert);
									

									//값을 복사하자
									//동일한 메시를 참조하는 것일테므로, 별도로 검사하지는 말자
									dstModVert._vertexUniqueID = srcModVert._vertexUniqueID;
									dstModVert._vertIndex = srcModVert._vertIndex;
									dstModVert._deltaPos = srcModVert._deltaPos;
									dstModVert._overlapWeight = srcModVert._overlapWeight;
								}

								// 추가 22.3.20 : Pin 복사 [v1.4.0]
								int nModPins = srcModMesh._pins != null ? srcModMesh._pins.Count : 0;
								apModifiedPin srcModPin = null;
								apModifiedPin dstModPin = null;

								if(nModPins > 0)
								{
									if (dstModMesh._pins == null)
									{
										dstModMesh._pins = new List<apModifiedPin>();
									}
									dstModMesh._pins.Clear();

									for (int iSrcModPin = 0; iSrcModPin < nModPins; iSrcModPin++)
									{
										srcModPin = srcModMesh._pins[iSrcModPin];
										dstModPin = new apModifiedPin();

										//값을 복사하자
										dstModPin._pinUniqueID = srcModPin._pinUniqueID;
										dstModPin._deltaPos = srcModPin._deltaPos;

										dstModMesh._pins.Add(dstModPin);
									}
								}


							}

							//나머지 정보를 복사하자
							dstModMesh._transformMatrix.SetMatrix(srcModMesh._transformMatrix, true);
							dstModMesh._meshColor = srcModMesh._meshColor;
							dstModMesh._isVisible = srcModMesh._isVisible;
							

							//VertRigging을 복사하자
							if(isMeshDataDuplicatable)
							{
								int nModVertRig = srcModMesh._vertRigs == null ? 0 : srcModMesh._vertRigs.Count;
								if(nModVertRig > 0 && dstModMesh._vertRigs == null)
								{
									dstModMesh._vertRigs = new List<apModifiedVertexRig>();
								}

								apModifiedVertexRig srcModVertRig = null;
								apModifiedVertexRig dstModVertRig = null;

								for (int iSrcModVertRig = 0; iSrcModVertRig < nModVertRig; iSrcModVertRig++)
								{
									srcModVertRig = srcModMesh._vertRigs[iSrcModVertRig];
									dstModVertRig = new apModifiedVertexRig();
									dstModMesh._vertRigs.Add(dstModVertRig);

									//값을 복사하자.
									//본 정보는 참조를 해야함
									dstModVertRig._vertexUniqueID = srcModVertRig._vertexUniqueID;
									dstModVertRig._vertIndex = srcModVertRig._vertIndex;
									dstModVertRig._totalWeight = srcModVertRig._totalWeight;

									//Weight Pair를 복사하자
									int nWeightPair = srcModVertRig._weightPairs == null ? 0 : srcModVertRig._weightPairs.Count;
									if(nWeightPair > 0 && dstModVertRig._weightPairs == null)
									{
										dstModVertRig._weightPairs = new List<apModifiedVertexRig.WeightPair>();
									}

									apModifiedVertexRig.WeightPair srcWeightPair = null;
									apModifiedVertexRig.WeightPair dstWeightPair = null;
									for (int iWeightPair = 0; iWeightPair < nWeightPair; iWeightPair++)
									{
										srcWeightPair = srcModVertRig._weightPairs[iWeightPair];
										if(srcWeightPair == null)
										{
											continue;
										}
										//본을 찾아서 유효한지 보자
										if(srcWeightPair._bone != null 
											&& convertInfo.Src2Dst_Bone.ContainsKey(srcWeightPair._bone)
											)
										{
											//모든 변환 정보가 있는가
											apBone dstLinkedBone = convertInfo.Src2Dst_Bone[srcWeightPair._bone];
											
											dstWeightPair = new apModifiedVertexRig.WeightPair(dstLinkedBone);
											dstWeightPair._weight = srcWeightPair._weight;

											dstModVertRig._weightPairs.Add(dstWeightPair);
										}
									}
								}
							}

							//VertWeight를 복사하자
							if(isMeshDataDuplicatable)
							{
								int nModVertWeight = srcModMesh._vertWeights == null ? 0 : srcModMesh._vertWeights.Count;
								if (nModVertWeight > 0 && dstModMesh._vertWeights == null)
								{
									dstModMesh._vertWeights = new List<apModifiedVertexWeight>();
								}

								apModifiedVertexWeight srcModVertWeight = null;
								apModifiedVertexWeight dstModVertWeight = null;

								for (int iModVertWeight = 0; iModVertWeight < nModVertWeight; iModVertWeight++)
								{
									srcModVertWeight = srcModMesh._vertWeights[iModVertWeight];
									dstModVertWeight = new apModifiedVertexWeight();

									dstModVertWeight._vertexUniqueID = srcModVertWeight._vertexUniqueID;
									dstModVertWeight._vertIndex = srcModVertWeight._vertIndex;

									dstModVertWeight._isEnabled = srcModVertWeight._isEnabled;
									dstModVertWeight._weight = srcModVertWeight._weight;
									dstModVertWeight._isPhysics = srcModVertWeight._isPhysics;
									dstModVertWeight._isVolume = srcModVertWeight._isVolume;
									//dstModVertWeight._pos_World_NoMod = srcModVertWeight._pos_World_NoMod;//삭제 v1.4.4

									dstModVertWeight._deltaPosRadius_Free = srcModVertWeight._deltaPosRadius_Free;
									dstModVertWeight._deltaPosRadius_Max = srcModVertWeight._deltaPosRadius_Max;

									if(dstModVertWeight._physicParam != null)
									{
										dstModVertWeight._physicParam = new apPhysicsVertParam();
									}
									if(srcModVertWeight._physicParam != null)
									{
										dstModVertWeight._physicParam.CopyFromSrc(srcModVertWeight._physicParam);
									}

									dstModMesh._vertWeights.Add(dstModVertWeight);
								}
							}

							//Extra Value를 복사하자
							dstModMesh._isExtraValueEnabled = srcModMesh._isExtraValueEnabled;

							if(dstModMesh._extraValue == null)
							{
								dstModMesh._extraValue = new apModifiedMesh.ExtraValue();
							}
							if(srcModMesh._extraValue != null)
							{
								dstModMesh._extraValue.CopyFromSrc(srcModMesh._extraValue);
							}
						}

						apModifiedBone srcModBone = null;
						apModifiedBone dstModBone = null;

						// Bone Data 복사
						for (int iSrcModBone = 0; iSrcModBone < nSrcModBones; iSrcModBone++)
						{
							srcModBone = srcParamSet._boneData[iSrcModBone];
							if(srcModBone == null)
							{
								continue;
							}

							if(srcModBone._bone == null)
							{
								//Bone이 읍넹?
								continue;
							}

							if(!convertInfo.Src2Dst_Bone.ContainsKey(srcModBone._bone))
							{
								//연결된 Bone이 없는데요?
								Debug.LogError("AnyPortrait : Duplicating Mod Bone is failed > No Converted Bone");
								continue;
							}
							apBone dstLinkedBone = convertInfo.Src2Dst_Bone[srcModBone._bone];

							//ModBone을 생성하고 등록하자
							dstModBone = new apModifiedBone();
							if(dstParamSet._boneData == null)
							{
								dstParamSet._boneData = new List<apModifiedBone>();
							}
							dstParamSet._boneData.Add(dstModBone);
							convertInfo.Src2Dst_ModBone.Add(srcModBone, dstModBone);//변환 리스트에도 입력

							//값을 복사하자
							apMeshGroup srcMeshGroupOfModifier = srcModBone._meshGroup_Modifier;
							apMeshGroup srcMeshGroupOfBone = srcModBone._meshGroup_Bone;
							apTransform_MeshGroup srcMGTF = srcModBone._meshGroupTransform;
							

							//ModBone의 연결 정보 설정
							apMeshGroup dstMeshGroupOfModifier = null;
							apMeshGroup dstMeshGroupOfBone = null;
							apTransform_MeshGroup dstMGTF = null;
							
							
							if(srcMeshGroupOfModifier != null
								&& convertInfo.Src2Dst_MeshGroup.ContainsKey(srcMeshGroupOfModifier))
							{
								dstMeshGroupOfModifier = convertInfo.Src2Dst_MeshGroup[srcMeshGroupOfModifier];
							}

							if(srcMeshGroupOfBone != null
								&& convertInfo.Src2Dst_MeshGroup.ContainsKey(srcMeshGroupOfBone))
							{
								dstMeshGroupOfBone = convertInfo.Src2Dst_MeshGroup[srcMeshGroupOfBone];
							}

							if(srcMGTF != null
								&& convertInfo.Src2Dst_MeshGroupTransform.ContainsKey(srcMGTF))
							{
								dstMGTF = convertInfo.Src2Dst_MeshGroupTransform[srcMGTF];
							}


							if(dstMeshGroupOfModifier == null
								|| dstMeshGroupOfBone == null
								|| dstMGTF == null)
							{
								Debug.LogError("AnyPortrait : Duplicating Mod Bone is Failed > Target Missing");
								Debug.LogError("dstMeshGroupOfModifier : " + (dstMeshGroupOfModifier != null));
								Debug.LogError("dstMeshGroupOfBone : " + (dstMeshGroupOfBone != null));
								Debug.LogError("dstMGTF : " + (dstMGTF != null));

								//if(dstMGTF == null && dstMeshGroupOfModifier != null && dstMeshGroupOfBone != null)
								//{
								//	Debug.Log(">> MeshGroup TF만 없다");
								//	Debug.Log(">> MeshGroup of Mod : " + dstMeshGroupOfModifier._name);
								//	Debug.Log(">> MeshGroup of Bone : " + dstMeshGroupOfBone._name);
								//	Debug.Log(">> srcMGTF : " + (srcMGTF != null ?  srcMGTF._nickName : "<None>"));
								//}
							}

							dstModBone._meshGroupUniqueID_Modifier = (dstMeshGroupOfModifier == null) ? -1 : dstMeshGroupOfModifier._uniqueID;
							dstModBone._meshGropuUniqueID_Bone = (dstMeshGroupOfBone == null) ? -1 : dstMeshGroupOfBone._uniqueID;
							dstModBone._transformUniqueID = (dstMGTF == null) ? -1 : dstMGTF._transformUniqueID;

							dstModBone._meshGroup_Modifier = dstMeshGroupOfModifier;
							dstModBone._meshGroup_Bone = dstMeshGroupOfBone;
							dstModBone._meshGroupTransform = dstMGTF;

							dstModBone._boneID = dstLinkedBone._uniqueID;
							dstModBone._bone = dstLinkedBone;

							dstModBone._transformMatrix.SetMatrix(srcModBone._transformMatrix, true);
						}

					}
				}
				
			}
		}









		public void LayerChange(apModifierBase modifier, bool isLayerUp)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{ return; }

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;

			if (!modStack._modifiers.Contains(modifier))
			{ return; }


			//Undo를 기록하자
			apEditorUtil.SetRecord_MeshGroupAllModifiers(	apUndoGroupData.ACTION.Modifier_LayerChanged, 
															Editor, 
															Editor.Select.MeshGroup, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.StructChanged);


			//modStack.RefreshAndSort(false);//이전
			modStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.Keep,
										apModifierStack.REFRESH_OPTION_REMOVE.Ignore);//[1.4.2] 변경 22.12.13

			int prevIndex = modStack._modifiers.IndexOf(modifier);
			int nextIndex = prevIndex;
			if (isLayerUp)
			{
				nextIndex++;

				if (nextIndex >= modStack._modifiers.Count)
				{ return; }
			}
			else
			{
				nextIndex--;

				if (nextIndex < 0)
				{ return; }
			}

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();

			//순서를 바꿀 모디파이어다.
			apModifierBase swapMod = modStack._modifiers[nextIndex];

			//인덱스를 서로 바꾸자
			swapMod._layer = prevIndex;
			modifier._layer = nextIndex;

			//modStack.RefreshAndSort(false);
			modStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.Keep,
										apModifierStack.REFRESH_OPTION_REMOVE.Ignore);//[1.4.2] 변경 22.12.13

			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}



		public void RemoveModifier(apModifierBase modifier)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;

			if (!modStack._modifiers.Contains(modifier))
			{
				return;
			}

			//Undo
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_RemoveModifier, Editor, Editor.Select.MeshGroup, modifier, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Modifier");

			apMeshGroup targetMeshGroup = Editor.Select.MeshGroup;

			int modifierID = modifier._uniqueID;
			//Editor._portrait.PushUniqueID_Modifier(modifierID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Modifier, modifierID);

			modStack.RemoveModifier(modifier);
			
			//modStack.RefreshAndSort(true);
			modStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
										apModifierStack.REFRESH_OPTION_REMOVE.RemoveNullModifiers);//[1.4.2] 변경 22.12.13

			//추가
			if (modifier != null)
			{
				//Undo.DestroyObjectImmediate(modifier.gameObject);
				apEditorUtil.SetRecordDestroyMonoObject(modifier, "Remove Modifier");
			}

			//다시 연결
			targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(targetMeshGroup));

			Editor.Select.SelectModifier(null);

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					targetMeshGroup,
			//					null,
			//					null,
			//					null,
			//					true);

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AllObjects(targetMeshGroup));//<<전체 갱신

			targetMeshGroup.SetDirtyToReset();
			targetMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//1.4.2
			targetMeshGroup.RefreshForce(true);

			Editor.RefreshControllerAndHierarchy(false);

			////프리팹이었다면 Apply
			//apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}




		//추가 5.21
		//만약 더이상 링크가 되지 않는 Modifier / Modifier ParamSetGroup / Modifier ParamSet / Modified Mesh/Bone이 있다면
		//자동으로 삭제를 해야한다.
		//삭제 시점은 
		//- Mesh/MeshGroup/AnimClip/ControlParam이 삭제 되었을 때
		//- 타임라인/타임라인 레이어가 삭제되었을 때
		//- AnimClip의 대상이 바뀌었을 때 (이건 물어봐야함)
		//- 에디터 첫 링크할때
		public void CheckAndRemoveUnusedModifierData(apPortrait portrait, bool isShowDialog, bool isSaveToUndo)
		{
			//1. 일단 체크하자
			if (portrait == null)
			{
				return;
			}

			//삭제되어야 하는 객체들
			//List<apModifierBase> _rmvModifiers = new List<apModifierBase>();
			//List<apModifierParamSetGroup> _rmvParamSetGroups = new List<apModifierParamSetGroup>();
			//List<apModifierParamSet> _rmvParamSets = new List<apModifierParamSet>();
			//List<apModifiedMesh> _rmvModMeshes = new List<apModifiedMesh>();
			//List<apModifiedBone> _rmvModBones = new List<apModifiedBone>();

			//1) MeshGroup -> 체크
			//2) Modifier -> 체크
			//3) Timeline / TimelineLayer -> 체크
			apMeshGroup meshGroup = null;
			apModifierBase modifier = null;
			apModifierParamSetGroup paramSetGroup = null;
			apModifierParamSet paramSet = null;
			for (int iMeshGroup = 0; iMeshGroup < portrait._meshGroups.Count; iMeshGroup++)
			{
				meshGroup = portrait._meshGroups[iMeshGroup];
				for (int iModifier = 0; iModifier < meshGroup._modifierStack._modifiers.Count; iModifier++)
				{
					modifier = meshGroup._modifierStack._modifiers[iModifier];

					for (int iPSG = 0; iPSG < modifier._paramSetGroup_controller.Count; iPSG++)
					{
						paramSetGroup = modifier._paramSetGroup_controller[iPSG];

						apAnimClip animClip = null;
						apAnimTimeline timeline = null;
						apAnimTimelineLayer timelineLayer = null;

						//체크한다.
						bool isRemovablePSG = false;
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey:
							case apModifierParamSetGroup.SYNC_TARGET.Bones:
								//사용하지 않는 값
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								{
									//Control Param과 연결되어야 한다.
									//없으면 검색
									if (paramSetGroup._keyControlParam == null)
									{
										if (portrait._controller.FindParam(paramSetGroup._keyControlParamID) == null)
										{
											isRemovablePSG = true;
										}
									}
								}
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								{
									//KeyAnim이 있어야 한다.
									//없으면 검색
									animClip = paramSetGroup._keyAnimClip;
									if (animClip == null)
									{
										animClip = portrait.GetAnimClip(paramSetGroup._keyAnimClipID);
										if (animClip == null)
										{
											isRemovablePSG = true;
										}

									}

									if (animClip != null)
									{
										//반대로, AnimClip에 이 ParamSetGroup이 속한게 아니라면 문제
										if (animClip._targetMeshGroup != meshGroup)
										{
											//MeshGroup이 다르다.
											isRemovablePSG = true;
										}
									}


									if (!isRemovablePSG && animClip != null)
									{
										//타임라인이 있는지 검사하자
										timeline = paramSetGroup._keyAnimTimeline;
										if (timeline == null)
										{
											timeline = animClip.GetTimeline(paramSetGroup._keyAnimTimelineID);
											if (timeline == null)
											{
												//타임라인이 없네용
												isRemovablePSG = true;
											}
										}
										if (timeline != null)
										{
											//반대로, 해당 Timeline에 paramSetGroup에 포함이 안된다면 그것도 문제

										}
									}

									if (!isRemovablePSG && animClip != null && timeline != null)
									{
										//타임라인 레이어가 있는지도 검사하자
										timelineLayer = paramSetGroup._keyAnimTimelineLayer;
										if (timelineLayer == null)
										{
											timelineLayer = timeline.GetTimelineLayer(paramSetGroup._keyAnimTimelineLayerID);
											if (timelineLayer == null)
											{
												//타임라인 레이어가 없네용
												isRemovablePSG = true;
											}
										}
									}
								}
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Static:
								//Static은 기본적으로 삭제되는 요건이 없다.
								break;


						}

						if (isRemovablePSG)
						{
							//>> 삭제되어야 하는 ParamSetGroup
							//Debug.LogError("삭제되어야 하는 ParamSetGroup");
							//_rmvParamSetGroups.Add(paramSetGroup);
							continue;
						}

						//삭제가 안되도 된다면
						//ParamSet을 분석하자
						for (int iPS = 0; iPS < paramSetGroup._paramSetList.Count; iPS++)
						{
							paramSet = paramSetGroup._paramSetList[iPS];

							//- 연결된 Control Param이 없다면 ParamSetGroup이 이미 삭제 되었을 것이다.
							//- Anim타입일 때 Keyframe이 없다면 삭제되어야 한다.
							//- Modified Mesh가 유효한지 테스트한다.
							//- Modified Bone이 유효한지 테스트한다.
							bool isRemovablePS = false;
							if (paramSetGroup._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
							{
								if (timelineLayer == null)
								{
									//Debug.LogError("??? 위에서 ParamSetGroup이 삭제 안되었는데 정작 timelineLayer가 null이다.");
								}
								else
								{
									apAnimKeyframe animKeyframe = timelineLayer.GetKeyframeByID(paramSet._keyframeUniqueID);
									if (animKeyframe == null)
									{
										//키프레임이 없어졌다. >> 삭제
										isRemovablePS = true;
										//Debug.LogError("삭제되어야 하는 ParamSet");
									}
								}
							}
							if (!isRemovablePS)
							{
								//Modified Mesh / Bone을 테스트하자
								if (paramSet._meshData != null && paramSet._meshData.Count > 0)
								{
									apModifiedMesh modMesh = null;
									for (int iModMesh = 0; iModMesh < paramSet._meshData.Count; iModMesh++)
									{
										modMesh = paramSet._meshData[iModMesh];

										if (modMesh._meshGroupOfModifier == null &&
													modMesh._meshGroupOfTransform == null)
										{
											//둘다 Null이면 뭐지.. Link 전인가
											Debug.LogError("Link MeshGroup Error");
										}


										//if(modMesh._isMeshTransform)
										//{
										//	if(modMesh._transform_Mesh == null)
										//	{
										//		Debug.LogError("MeshTransform이 없는 ModMesh");
										//	}
										//	else if(modMesh._renderUnit == null)
										//	{
										//		Debug.LogError("RenderUnit이 없다;");
										//	}
										//}
										//else
										//{
										//	if(modMesh._transform_MeshGroup == null)
										//	{
										//		Debug.LogError("MeshGroupTransform이 없는 ModMesh");
										//	}
										//	else if(modMesh._renderUnit == null)
										//	{
										//		Debug.LogError("RenderUnit이 없다;");
										//	}
										//}
									}
								}

								if (paramSet._boneData != null && paramSet._boneData.Count > 0)
								{
									apModifiedBone modBone = null;
									for (int iModBone = 0; iModBone < paramSet._boneData.Count; iModBone++)
									{
										modBone = paramSet._boneData[iModBone];
										//if(modBone._bone == null)
										//{
										//	Debug.LogError("Bone없는 ModBone");
										//}
									}
								}
							}


						}
					}
				}

			}
		}

		//----------------------------------------------------------------------------------
		// Bake
		//----------------------------------------------------------------------------------
		/// <summary>
		/// 현재 Portrait를 실행가능한 버전으로 Bake하자
		/// </summary>
		public apBakeResult Bake()
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			apPortrait targetPortrait = Editor._portrait;

			//추가 20.11.7
			//이미지가 설정되지 않은 메시가 있다면 에러가 발생한다.
			//미리 안내를 하자
			if(!CheckIfAnyNoImageMesh(targetPortrait))
			{
				//에러가 발생해서 Bake 취소
				return null;
			}

			apEditorUtil.SetDirty(_editor);

			apBakeResult bakeResult = new apBakeResult();


			//추가 19.5.26 : v1.1.7의 용량 최적화가 적용되었는가 (=modMeshSet을 이용하도록 설정되었는가)
			bool isSizeOptimizedV117 = true;

			//bool isSizeOptimizedV117 = false;//<<테스트

			//추가 19.8.5
			//bool isUseSRP = Editor._isUseSRP;//이전
			bool isUseSRP = Editor.ProjectSettingData.Project_IsUseSRP;//변경 [v1.4.2]
			bool isBakeGammaColorSpace = Editor.ProjectSettingData.Project_IsColorSpaceGamma;//추가 [v1.4.2]



			//추가 10.26 : Bake에서는 빌보드가 꺼져야 한다.
			//임시로 껐다가 마지막에 다시 복구
			apPortrait.BILLBOARD_TYPE billboardType = targetPortrait._billboardType;
			targetPortrait._billboardType = apPortrait.BILLBOARD_TYPE.None;//임시로 끄자




			//추가 21.3.11
			// Scale 이슈가 있다.
			// Bake 전에 이미 Scale이 음수인 경우, Bake 직후나 Link후 플레이시 메시가 거꾸로 보이게 된다.
			//따라서 portrait부터 시작해서 상위의 모든 GameObject의 Sca;e을 저장했다가 복원해야한다.
			Dictionary<Transform, Vector3> prevTransformScales = new Dictionary<Transform, Vector3>();
			Transform curScaleCheckTransform = targetPortrait.transform;
			while(true)
			{
				prevTransformScales.Add(curScaleCheckTransform, curScaleCheckTransform.localScale);
				curScaleCheckTransform.localScale = Vector3.one;//일단 기본으로 강제 적용
				if(curScaleCheckTransform.parent == null)
				{
					break;
				}
				curScaleCheckTransform = curScaleCheckTransform.parent;
			}
			






			//Bake 방식 변경
			//일단 숨겨진 GameObject를 제외한 모든 객체를 리스트로 저장한다.
			//LinkParam 형태로 저장을 한다.
			//LinkParam으로 저장하면서 <apOpt 객체>와 <그렇지 않은 객체>를 구분한다.
			//"apOpt 객체"는 나중에 (1)재활용 할지 (2) 삭제 할지 결정한다.
			//"그렇지 않은 GameObject"는 Hierarchy 정보를 가진채 (1) 링크를 유지할 지(재활용되는 경우) (2) Unlink Group에 넣을지 결정한다.
			//만약 재활용되지 않는 (apOpt GameObject)에서 알수 없는 Component가 발견된 경우 -> 이건 삭제 예외 대상에 넣는다.

			//분류를 위한 그룹
			//1. ReadyToRecycle
			// : 기존에 RootUnit과 그 하위에 있었던 GameObject들이다. 분류 전에 일단 여기로 들어간다.
			// : 분류 후에는 원칙적으로 하위에 어떤 객체도 남아선 안된다.

			//2. RemoveTargets
			// : apOpt를 가진 GameObject 그룹 중에서 사용되지 않았던 그룹이다. 
			// : 처리 후에는 이 GameObject를 통째로 삭제한다.

			//3. UnlinkedObjects
			// : apOpt를 가지지 않은 GameObject중에서 재활용되지 않은 객체들


			GameObject groupObj_1_ReadyToRecycle = new GameObject("__Baking_1_ReadyToRecycle");
			GameObject groupObj_2_RemoveTargets = new GameObject("__Baking_2_RemoveTargets");


			GameObject groupObj_3_UnlinkedObjects = null;
			if (targetPortrait._bakeUnlinkedGroup == null)
			{
				groupObj_3_UnlinkedObjects = new GameObject("__UnlinkedObjects");
				targetPortrait._bakeUnlinkedGroup = groupObj_3_UnlinkedObjects;
			}
			else
			{
				groupObj_3_UnlinkedObjects = targetPortrait._bakeUnlinkedGroup;
				groupObj_3_UnlinkedObjects.name = "__UnlinkedObjects";
			}




			groupObj_1_ReadyToRecycle.transform.parent = targetPortrait.transform;
			groupObj_2_RemoveTargets.transform.parent = targetPortrait.transform;
			groupObj_3_UnlinkedObjects.transform.parent = targetPortrait.transform;

			groupObj_1_ReadyToRecycle.transform.localPosition = Vector3.zero;
			groupObj_2_RemoveTargets.transform.localPosition = Vector3.zero;
			groupObj_3_UnlinkedObjects.transform.localPosition = Vector3.zero;

			groupObj_1_ReadyToRecycle.transform.localRotation = Quaternion.identity;
			groupObj_2_RemoveTargets.transform.localRotation = Quaternion.identity;
			groupObj_3_UnlinkedObjects.transform.localRotation = Quaternion.identity;

			groupObj_1_ReadyToRecycle.transform.localScale = Vector3.one;
			groupObj_2_RemoveTargets.transform.localScale = Vector3.one;
			groupObj_3_UnlinkedObjects.transform.localScale = Vector3.one;


			//2. 기존 RootUnit을 Recycle로 옮긴다.
			//옮기면서 "Prev List"를 만들어야 한다. Recycle을 하기 위함
			List<apOptRootUnit> prevOptRootUnits = new List<apOptRootUnit>();
			if (targetPortrait._optRootUnitList != null)
			{
				for (int i = 0; i < targetPortrait._optRootUnitList.Count; i++)
				{
					apOptRootUnit optRootUnit = targetPortrait._optRootUnitList[i];
					if (optRootUnit != null)
					{
						optRootUnit.transform.parent = groupObj_1_ReadyToRecycle.transform;

						prevOptRootUnits.Add(optRootUnit);
					}
				}
			}



			//삭제하는 코드
			//일단 이 코드는 사용하지 않습니다.
			//if (targetPortrait._optRootUnitList != null)
			//{
			//	for (int i = 0; i < targetPortrait._optRootUnitList.Count; i++)
			//	{
			//		apOptRootUnit optRootUnit = targetPortrait._optRootUnitList[i];
			//		if (optRootUnit != null && optRootUnit.gameObject != null)
			//		{
			//			GameObject.DestroyImmediate(optRootUnit.gameObject);
			//		}
			//	}
			//}
			//else
			//{
			//	targetPortrait._optRootUnitList = new List<apOptRootUnit>();
			//}

			//RootUnit 리스트를 초기화한다.
			if (targetPortrait._optRootUnitList == null)
			{
				targetPortrait._optRootUnitList = new List<apOptRootUnit>();
			}

			targetPortrait._optRootUnitList.Clear();
			targetPortrait._curPlayingOptRootUnit = null;
			//if(targetPortrait._optRootUnit != null)
			//{
			//	GameObject.DestroyImmediate(targetPortrait._optRootUnit.gameObject);
			//}

			if (targetPortrait._optTransforms == null) { targetPortrait._optTransforms = new List<apOptTransform>(); }
			if (targetPortrait._optMeshes == null) { targetPortrait._optMeshes = new List<apOptMesh>(); }
			//if (targetPortrait._optMaskedMeshes == null)		{ targetPortrait._optMaskedMeshes = new List<apOptMesh>(); }
			//if (targetPortrait._optClippedMeshes == null)		{ targetPortrait._optClippedMeshes = new List<apOptMesh>(); }
			if (targetPortrait._optTextureData == null) { targetPortrait._optTextureData = new List<apOptTextureData>(); }//<<텍스쳐 데이터 추가



			targetPortrait._optTransforms.Clear();
			targetPortrait._optMeshes.Clear();
			//targetPortrait._optMaskedMeshes.Clear();
			//targetPortrait._optClippedMeshes.Clear();
			targetPortrait._optTextureData.Clear();
			//targetPortrait._isAnyMaskedMeshes = false;

			//추가
			//Batched Matrial 관리 객체가 생겼다.
			if (targetPortrait._optBatchedMaterial == null)
			{
				targetPortrait._optBatchedMaterial = new apOptBatchedMaterial();
			}
			else
			{
				targetPortrait._optBatchedMaterial.Clear(true);//<<이미 생성되어 있다면 초기화
			}

			////추가 11.6 : LWRP Shader를 사용하는지 체크하고, 필요한 경우 생성해야한다.
			//CheckAndCreateLWRPShader();


			//3. 텍스쳐 데이터를 먼저 만들자.
			for (int i = 0; i < targetPortrait._textureData.Count; i++)
			{
				apTextureData textureData = targetPortrait._textureData[i];
				apOptTextureData newOptTexData = new apOptTextureData();

				newOptTexData.Bake(i, textureData);
				targetPortrait._optTextureData.Add(newOptTexData);
			}

			//추가 20.1.28 : Color Space가 동일하도록 묻고 변경
			CheckAndChangeTextureDataColorSpace(targetPortrait);



			//4. 추가 : Reset
			//TODO : 이 함수를 호출한 이후에, 현재 Mesh Group에 대해서 추가 처리 필요
			//이 함수를 호출하면 계층적인 MeshGroup 내부늬 Modifier 연결이 풀린다.
			//이 코드 두개가 포함되어야 한다.
			//meshGroup.LinkModMeshRenderUnits();
			//meshGroup.RefreshModifierLink();
			targetPortrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AllObjects(null));


			//추가 : 사용되지 않는 Monobehaviour는 삭제해야한다.
			CheckAndRemoveUnusedMonobehaviours(targetPortrait);

			//이름을 갱신한다.
			CheckAndRefreshGameObjectNames(targetPortrait);


			//4. OptTransform을 만들자 (RootUnit부터)

			for (int i = 0; i < targetPortrait._rootUnits.Count; i++)
			{
				apRootUnit rootUnit = targetPortrait._rootUnits[i];

				//업데이트를 한번 해주자

				//추가 : 계층구조의 MeshGroup인 경우 이 코드가 추가되어야 한다.
				if (rootUnit._childMeshGroup != null)
				{
					rootUnit._childMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//렌더 유닛의 Depth를 다시 계산해야한다. <<
					rootUnit._childMeshGroup.LinkModMeshRenderUnits(null);
					rootUnit._childMeshGroup.RefreshModifierLink(null);
				}

				rootUnit.Update(0.0f, false, false);


				apOptRootUnit optRootUnit = null;

				//1. Root Unit
				//재활용 가능한지 판단한다.


				bool isRecycledRootUnit = false;
				apOptRootUnit recycledOptRootUnit = GetRecycledRootUnit(rootUnit, prevOptRootUnits);

				if (recycledOptRootUnit != null)
				{

					//재활용이 된다.
					optRootUnit = recycledOptRootUnit;

					//일부 값은 다시 리셋
					optRootUnit.name = "Root Unit " + i;
					optRootUnit._portrait = targetPortrait;
					optRootUnit._transform = optRootUnit.transform;

					optRootUnit.transform.parent = targetPortrait.transform;
					optRootUnit.transform.localPosition = Vector3.zero;
					optRootUnit.transform.localRotation = Quaternion.identity;
					optRootUnit.transform.localScale = Vector3.one;

					//재활용에 성공했으니 OptUnit은 제외한다.
					prevOptRootUnits.Remove(recycledOptRootUnit);
					isRecycledRootUnit = true;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}
				else
				{
					//새로운 RootUnit이다.
					optRootUnit = AddGameObject<apOptRootUnit>("Root Unit " + i, targetPortrait.transform);

					optRootUnit._portrait = targetPortrait;
					optRootUnit._rootOptTransform = null;
					optRootUnit._transform = optRootUnit.transform;

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}

				optRootUnit.ClearChildLinks();//Child Link를 초기화한다.

				//추가 12.6 : SortedRenderBuffer에 관련한 Bake 코드 <<
				optRootUnit.BakeSortedRenderBuffer(targetPortrait, rootUnit);


				targetPortrait._optRootUnitList.Add(optRootUnit);



				//재활용에 성공했다면
				//기존의 GameObject + Bake 여부를 재귀적 리스트로 작성한다.
				apBakeLinkManager bakeLinkManager = null;
				if (isRecycledRootUnit)
				{
					bakeLinkManager = new apBakeLinkManager();

					//파싱하자.
					bakeLinkManager.Parse(optRootUnit._rootOptTransform.gameObject, recycledOptRootUnit.gameObject);
				}

				apMeshGroup childMainMeshGroup = rootUnit._childMeshGroup;

				//0. 추가
				//일부 Modified Mesh를 갱신해야한다.
				if (childMainMeshGroup != null && rootUnit._childMeshGroupTransform != null)
				{
					//Refresh를 한번 해주자
					childMainMeshGroup.RefreshForce();

					List<apModifierBase> modifiers = childMainMeshGroup._modifierStack._modifiers;
					for (int iMod = 0; iMod < modifiers.Count; iMod++)
					{
						apModifierBase mod = modifiers[iMod];
						if (mod._paramSetGroup_controller != null)
						{
							for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
							{
								apModifierParamSetGroup psg = mod._paramSetGroup_controller[iPSG];
								for (int iPS = 0; iPS < psg._paramSetList.Count; iPS++)
								{
									apModifierParamSet ps = psg._paramSetList[iPS];
									ps.UpdateBeforeBake(targetPortrait, childMainMeshGroup, rootUnit._childMeshGroupTransform);
								}
							}
						}
					}
				}

				//1. 1차 Bake : GameObject 만들기
				//List<apMeshGroup> meshGroups = targetPortrait._meshGroups;
				if (childMainMeshGroup != null && rootUnit._childMeshGroupTransform != null)
				{
					//정렬 한번 해주고
					childMainMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);

					apRenderUnit rootRenderUnit = childMainMeshGroup._rootRenderUnit;
					//apRenderUnit rootRenderUnit = targetPortrait._rootUnit._renderUnit;
					if (rootRenderUnit != null)
					{
						//apTransform_MeshGroup meshGroupTransform = targetPortrait._rootUnit._childMeshGroupTransform;
						apTransform_MeshGroup meshGroupTransform = rootRenderUnit._meshGroupTransform;

						if (meshGroupTransform == null)
						{
							Debug.LogError("Bake Error : MeshGroupTransform Not Found [" + childMainMeshGroup._name + "]");
						}
						else
						{
							MakeMeshGroupToOptTransform(	rootRenderUnit,
															meshGroupTransform, optRootUnit.transform,
															null,
															optRootUnit,
															bakeLinkManager, bakeResult,
															targetPortrait._bakeZSize,

															//<<감마 색상 공간으로 Bake할 것인가
															//Editor._isBakeColorSpaceToGamma,//<<감마 색상 공간으로 Bake할 것인가
															isBakeGammaColorSpace,//로컬 변수로 변경 v1.4.2

															//Editor._isUseSRP,//LWRP Shader를 사용할 것인가 > 삭제 (SRP로 변경)
															targetPortrait,
															childMainMeshGroup,
															isSizeOptimizedV117,
															isUseSRP);
							//MakeMeshGroupToOptTransform(null, meshGroupTransform, targetPortrait._optRootUnit.transform, null);
						}
					}
					else
					{
						Debug.LogError("Bake Error : RootMeshGroup Not Found [" + childMainMeshGroup._name + "]");
					}
				}



				//optRootUnit.transform.localScale = Vector3.one * 0.01f;
				optRootUnit.transform.localScale = Vector3.one * targetPortrait._bakeScale;


				// 이전에 Bake 했던 정보에서 가져왔다면
				//만약 "재활용되지 않은 GameObject"를 찾아서 별도의 처리를 해야한다.
				if (isRecycledRootUnit && bakeLinkManager != null)
				{
					bakeLinkManager.SetHierarchyNotRecycledObjects(groupObj_1_ReadyToRecycle, groupObj_2_RemoveTargets, groupObj_3_UnlinkedObjects, bakeResult);
				}

				//추가 v1.4.8 : 루트 모션 설정을 입력하자
				optRootUnit._rootMotionBoneID = -1;
				if(childMainMeshGroup != null)
				{
					//루트 모션용 본이 존재하는지 확인하자
					apBone rootMotionBone = childMainMeshGroup.GetBone(childMainMeshGroup._rootMotionBoneID);
					if(rootMotionBone != null)
					{
						//루트 모션 본이 존재한다면 ID를 할당한다.
						optRootUnit._rootMotionBoneID = childMainMeshGroup._rootMotionBoneID;
					}
				}


				//추가 12.6 : Bake 함수 추가 <<
				optRootUnit.BakeComplete();

			}



			if (prevOptRootUnits.Count > 0)
			{
				//이 유닛들은 Remove Target으로 이동해야 한다.
				apOptRootUnit curPrevoptRootUnit = null;
				for (int i = 0; i < prevOptRootUnits.Count; i++)
				{
					curPrevoptRootUnit = prevOptRootUnits[i];//변경 1.4.5

					//[v1.4.5] 오류 검출
					if(curPrevoptRootUnit == null
						|| curPrevoptRootUnit.transform == null)
					{
						Debug.LogWarning("AnyPortrait : Bake warning. Since the previous root unit is null, some objects may not be created or deleted properly.");
						continue;
					}

					curPrevoptRootUnit.transform.parent = groupObj_2_RemoveTargets.transform;

					//[v1.4.5] 연결이 해제된 상태에서 Bake를 다시 실행할 때 Null 체크
					if (curPrevoptRootUnit._rootOptTransform == null)
					{	
						Debug.LogWarning("AnyPortrait : Bake warning. Some subobjects of the unused Root Unit have already been deleted, so moving them to the Unlinked group for preservation failed.");
						continue;
					}


					//만약 여기서 알수없는 GameObject나 Compnent에 대해서는 Remove가 아니라 Unlink로 옮겨야 한다.
					apBakeLinkManager prevBakeManager = new apBakeLinkManager();
					prevBakeManager.Parse(curPrevoptRootUnit._rootOptTransform.gameObject, null);

					prevBakeManager.SetHierarchyToUnlink(groupObj_3_UnlinkedObjects, bakeResult);
				}
			}


			//TODO: 이제 그룹을 삭제하던가 경고 다이얼로그를 띄워주던가 하자
			UnityEngine.Object.DestroyImmediate(groupObj_1_ReadyToRecycle);
			UnityEngine.Object.DestroyImmediate(groupObj_2_RemoveTargets);

			if (groupObj_3_UnlinkedObjects.transform.childCount == 0)
			{
				UnityEngine.Object.DestroyImmediate(groupObj_3_UnlinkedObjects);

				targetPortrait._bakeUnlinkedGroup = null;
			}


			//1-2. Masked Mesh 연결해주기
			//if (targetPortrait._optMaskedMeshes.Count > 0 || targetPortrait._optClippedMeshes.Count > 0)
			//{
			//	targetPortrait._isAnyMaskedMeshes = true;
			//}

			for (int i = 0; i < targetPortrait._optMeshes.Count; i++)
			{
				apOptMesh optMesh = targetPortrait._optMeshes[i];
				if (optMesh._isMaskParent)
				{
					//Parent라면..
					//apOptMesh[] childMeshes = new apOptMesh[3];
					//for (int iChild = 0; iChild < 3; iChild++)
					//{
					//	childMeshes[iChild] = null;
					//	if(optMesh._clipChildIDs[iChild] >= 0)
					//	{
					//		apOptTransform optTransform = targetPortrait.GetOptTransform(optMesh._clipChildIDs[iChild]);
					//		if(optTransform != null && optTransform._childMesh != null)
					//		{
					//			childMeshes[iChild] = optTransform._childMesh;
					//		}

					//	}
					//}
					//optMesh.LinkAsMaskParent(childMeshes);//<<이거 사용 안합니더
				}
				else if (optMesh._isMaskChild)
				{
					apOptTransform optTransform = targetPortrait.GetOptTransform(optMesh._clipParentID);
					apOptMesh parentMesh = null;
					if (optTransform != null && optTransform._childMesh != null)
					{
						parentMesh = optTransform._childMesh;
					}
					optMesh.LinkAsMaskChild(parentMesh);
				}
			}

			//2. 2차 Bake : Modifier 만들기
			List<apOptTransform> optTransforms = targetPortrait._optTransforms;
			for (int i = 0; i < optTransforms.Count; i++)
			{
				apOptTransform optTransform = optTransforms[i];

				apMeshGroup srcMeshGroup = targetPortrait.GetMeshGroup(optTransform._meshGroupUniqueID);
				optTransform.BakeModifier(targetPortrait, srcMeshGroup, isSizeOptimizedV117);
			}





			//3. 3차 Bake : ControlParam/KeyFrame ~~> Modifier <- [Calculated Param] -> OptTrasform + Mesh
			targetPortrait.SetFirstInitializeAfterBake();//이게 호출되어야 Initialize가 제대로 동작한다.
			targetPortrait.Initialize();

			//추가 20.8.10 [Flipped Scale 문제]
			//3.1 : 리깅 본 정보를 Initialize 직후에 Bake한다. (다만 옵션이 설정된 경우에 한해서)
			//Debug.LogError("Flipped Option : " + targetPortrait._flippedMeshOption);
			if (targetPortrait._flippedMeshOption == apPortrait.FLIPPED_MESH_CHECK.All)
			{
				for (int i = 0; i < optTransforms.Count; i++)
				{
					apOptTransform optTransform = optTransforms[i];

					//리깅이 된 optTransform은 연결된 본들을 입력해주자
					if (optTransform._childMesh != null && optTransform._isIgnoreParentModWorldMatrixByRigging)
					{
						SetRiggingOptBonesToOptTransform(optTransform);
					}
				}
			}



			//4. 첫번째 OptRoot만 보여주도록 하자
			if (targetPortrait._optRootUnitList.Count > 0)
			{
				targetPortrait.ShowRootUnitWhenBake(targetPortrait._optRootUnitList[0]);
			}

			//5. AnimClip의 데이터를 받아서 AnimPlay 데이터로 만들자
			if (targetPortrait._animPlayManager == null)
			{
				targetPortrait._animPlayManager = new apAnimPlayManager();
			}

			targetPortrait._animPlayManager.InitAndLink();
			targetPortrait._animPlayManager._animPlayDataList.Clear();

			for (int i = 0; i < targetPortrait._animClips.Count; i++)
			{
				apAnimClip animClip = targetPortrait._animClips[i];
				int animClipID = animClip._uniqueID;
				string animClipName = animClip._name;
				int targetMeshGroupID = animClip._targetMeshGroupID;

				apAnimPlayData animPlayData = new apAnimPlayData(animClipID, targetMeshGroupID, animClipName);
				targetPortrait._animPlayManager._animPlayDataList.Add(animPlayData);

			}

			//6. 한번 업데이트를 하자 (소켓들이 갱신된다)
			if (targetPortrait._optRootUnitList.Count > 0)
			{
				apOptRootUnit optRootUnit = null;
				for (int i = 0; i < targetPortrait._optRootUnitList.Count; i++)
				{
					//이전 : 함수가 너무 반복되어 래핑되었다. 함수를 제거한닷
					//targetPortrait._optRootUnitList[i].RemoveAllCalculateResultParams();

					//변경
					optRootUnit = targetPortrait._optRootUnitList[i];
					if (optRootUnit._rootOptTransform != null)
					{
						optRootUnit._rootOptTransform.ClearResultParams(true);
						optRootUnit._rootOptTransform.ResetCalculateStackForBake(true);
					}
					else
					{
						Debug.LogError("AnyPortrait : No Root Opt Transform on RootUnit");
					}
				}

				//이 코드는 위에 추가되었다. "optRootUnit._rootOptTransform.ResetCalculateStackForBake(true);"
				//추가 3.22 : Bake후 메시가 변경되었을 경우에 다시 리셋할 필요가 있다.
				//for (int i = 0; i < targetPortrait._optRootUnitList.Count; i++)
				//{
				//	targetPortrait._optRootUnitList[i].ResetCalculateStackForBake();
				//}


				for (int i = 0; i < targetPortrait._optRootUnitList.Count; i++)
				{
					//업데이트
					targetPortrait._optRootUnitList[i].UpdateTransforms(0.0f, true, null);
					
				}

				////디버그를 해보자
				//for (int i = 0; i < targetPortrait._optRootUnitList.Count; i++)
				//{	
				//	targetPortrait._optRootUnitList[i].DebugBoneMatrix();
				//}
				//Debug.LogError("------------------------------------------");

			}



			//6. Mask 메시 한번 더 갱신
			//if(targetPortrait._optMaskedMeshes.Count > 0)
			//{
			//	for (int i = 0; i < targetPortrait._optMaskedMeshes.Count; i++)
			//	{
			//		targetPortrait._optMaskedMeshes[i].RefreshMaskedMesh();
			//	}
			//}
			//> 변경 : Child 위주로 변경
			//if (targetPortrait._optClippedMeshes.Count > 0)
			//{
			//	for (int i = 0; i < targetPortrait._optClippedMeshes.Count; i++)
			//	{
			//		targetPortrait._optClippedMeshes[i].RefreshClippedMesh();
			//	}
			//}


			//추가 3.22 
			//6-2. LayerOrder 갱신하자
			string sortingLayerName = "";
			bool isValidSortingLayer = false;
			if (SortingLayer.IsValid(targetPortrait._sortingLayerID))
			{
				sortingLayerName = SortingLayer.IDToName(targetPortrait._sortingLayerID);
				isValidSortingLayer = true;
			}
			else
			{
				if (SortingLayer.layers.Length > 0)
				{
					sortingLayerName = SortingLayer.layers[0].name;
					isValidSortingLayer = true;
				}
				else
				{
					isValidSortingLayer = false;
				}
			}
			if (isValidSortingLayer)
			{
				targetPortrait.SetSortingLayer(sortingLayerName);
			}
			//변경 19.8.19 : 옵션이 적용되는 경우에 한해서
			if (targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
			{
				targetPortrait.SetSortingOrder(targetPortrait._sortingOrder);
			}


			//추가 19.5.26
			//6-3. 최적화 옵션으로 Bake 되었는지 체크
			targetPortrait._isSizeOptimizedV117 = isSizeOptimizedV117;



			//7. 기본 GameObject 타입 (Mesh, MeshGroup, Modifier) 중에서 사용되지 않는 객체는 삭제해주자
			List<apMesh> usingMeshes = new List<apMesh>();
			List<apMeshGroup> usingMeshGroups = new List<apMeshGroup>();
			List<apModifierBase> usingModifiers = new List<apModifierBase>();

			for (int i = 0; i < targetPortrait._meshes.Count; i++)
			{
				targetPortrait._meshes[i].gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

				usingMeshes.Add(targetPortrait._meshes[i]);
			}

			for (int i = 0; i < targetPortrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = targetPortrait._meshGroups[i];
				meshGroup.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

				usingMeshGroups.Add(meshGroup);

				for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
				{
					meshGroup._modifierStack._modifiers[iMod].gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

					usingModifiers.Add(meshGroup._modifierStack._modifiers[iMod]);
				}
			}

			CheckAndMakeObjectGroup();

			//각 서브 오브젝트 하위의 그룹들을 체크하여 유효하지 않는게 있는지 체크한다.

			List<GameObject> unusedMeshGameObjects = new List<GameObject>();
			List<GameObject> unusedMeshGroupGameObjects = new List<GameObject>();
			List<GameObject> unusedModifierGameObjects = new List<GameObject>();

			for (int iMesh = 0; iMesh < targetPortrait._subObjectGroup_Mesh.transform.childCount; iMesh++)
			{
				Transform meshTF = targetPortrait._subObjectGroup_Mesh.transform.GetChild(iMesh);
				apMesh targetMesh = meshTF.GetComponent<apMesh>();

				if (targetMesh == null)
				{
					//Mesh가 없는 GameObject 발견
					Debug.Log("No Mesh GameObject : " + meshTF.gameObject.name);

					unusedMeshGameObjects.Add(meshTF.gameObject);
				}
				else if (!usingMeshes.Contains(targetMesh))
				{
					//사용되지 않는 Mesh 발견
					Debug.Log("Unused Mesh Found : " + targetMesh._name);

					unusedMeshGameObjects.Add(meshTF.gameObject);
				}
			}

			for (int iMeshGroup = 0; iMeshGroup < targetPortrait._subObjectGroup_MeshGroup.transform.childCount; iMeshGroup++)
			{
				Transform meshGroupTF = targetPortrait._subObjectGroup_MeshGroup.transform.GetChild(iMeshGroup);
				apMeshGroup targetMeshGroup = meshGroupTF.GetComponent<apMeshGroup>();

				if (targetMeshGroup == null)
				{
					//MeshGroup이 없는 GameObject 발견
					//Debug.Log("No MeshGroup GameObject : " + meshGroupTF.gameObject.name);

					unusedMeshGroupGameObjects.Add(meshGroupTF.gameObject);
				}
				else if (!usingMeshGroups.Contains(targetMeshGroup))
				{
					//사용되지 않는 MeshGroup 발견
					//Debug.Log("Unused MeshGroup Found : " + targetMeshGroup._name);

					unusedMeshGroupGameObjects.Add(meshGroupTF.gameObject);
				}
			}

			for (int iMod = 0; iMod < targetPortrait._subObjectGroup_Modifier.transform.childCount; iMod++)
			{
				Transform modTF = targetPortrait._subObjectGroup_Modifier.transform.GetChild(iMod);
				apModifierBase targetMod = modTF.GetComponent<apModifierBase>();

				if (targetMod == null)
				{
					//Modifier가 없는 GameObject 발견
					//Debug.Log("No Modifier GameObject : " + modTF.gameObject.name);

					unusedModifierGameObjects.Add(modTF.gameObject);
				}
				else if (!usingModifiers.Contains(targetMod))
				{
					//사용되지 않는 Modifier 발견
					//Debug.Log("Unused Modifier Found : " + targetMod.DisplayName);

					unusedModifierGameObjects.Add(modTF.gameObject);
				}
			}

			//참조되지 않은건 삭제하자
			for (int i = 0; i < unusedMeshGameObjects.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(unusedMeshGameObjects[i]);
			}
			for (int i = 0; i < unusedMeshGroupGameObjects.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(unusedMeshGroupGameObjects[i]);
			}
			for (int i = 0; i < unusedModifierGameObjects.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(unusedModifierGameObjects[i]);
			}

			//여기서 Opt 업뎃을 하나 할까..
			//targetPortrait.Hide();
			//targetPortrait.Show();
			//targetPortrait.UpdateForce();

			//추가3.22
			//Portrait가 Prefab이라면
			//Bake와 동시에 Apply를 해야한다.
			//if(apEditorUtil.IsPrefab(targetPortrait.gameObject))
			//{
			//	apEditorUtil.ApplyPrefab(targetPortrait.gameObject, true);
			//	//그리고 다시 Apply를 해제
			//	apEditorUtil.DisconnectPrefab(targetPortrait);
			//}

			//메카님 옵션이 켜져 있다면
			//1. Animation Clip들을 리소스로 생성한다.
			//2. Animator 컴포넌트를 추가한다.


			if (targetPortrait._isUsingMecanim)
			{
				//추가 3.22 : animClip 경로가 절대 경로인 경우, 여러 작업자가 공유해서 쓸 수 없다.
				//상대 경로로 바꾸는 작업을 해야한다.
				CheckAnimationsBasePathForV116(targetPortrait);

				CreateAnimationsWithMecanim(targetPortrait, targetPortrait._mecanimAnimClipResourcePath);
			}


			//추가 21.9.25 : 유니티 이벤트 (UnityEvent)를 사용한다면 Bake를 하자
			if(targetPortrait._unityEventWrapper == null)
			{
				targetPortrait._unityEventWrapper = new apUnityEventWrapper();
			}
			targetPortrait._unityEventWrapper.Bake(targetPortrait);


			apEditorUtil.SetDirty(_editor);

			//추가. Bake 후 처리
			ProcessAfterBake();

			//추가 19.10.26 : 빌보드 설정을 다시 복구
			targetPortrait._billboardType = billboardType;


			//추가 21.3.11
			// Scale 이슈가 있어서 저장된 값의 Scale로 복원
			foreach (KeyValuePair<Transform, Vector3> transform2Scale in prevTransformScales)
			{
				if(transform2Scale.Key != null)
				{
					transform2Scale.Key.localScale = transform2Scale.Value;
				}
			}



			//버그 수정 : 첫번째 루트 유닛만 보여야 하는데 그렇지 않은 경우 문제 해결
			//추가 22.1.9 : 루트 유닛이 여러개 있는 경우엔 첫번째 루트 유닛을 출력하자
			int nOptRootUnits = targetPortrait._optRootUnitList != null ? targetPortrait._optRootUnitList.Count : 0;
			if (nOptRootUnits > 1)
			{
				targetPortrait.ShowRootUnitWhenBake(targetPortrait._optRootUnitList[0]);
			}


			//Bake 후에는 Initialize를 하지 않은 상태로 되돌린다. (v1.4.3)
			targetPortrait.SetFirstInitializeAfterBake();


			return bakeResult;
		}

		//객체를 생성하기 전에 이전에 Bake된 것을 재활용하기 위한 함수

		private apOptRootUnit GetRecycledRootUnit(apRootUnit srcRootUnit, List<apOptRootUnit> prevObjects)
		{
			//Debug.Log("RootUnit 재활용 찾기");
			if (srcRootUnit._childMeshGroup != null && srcRootUnit._childMeshGroup._rootRenderUnit != null && srcRootUnit._childMeshGroup._rootRenderUnit._meshGroupTransform != null)
			{
				apTransform_MeshGroup rootMGTransform = srcRootUnit._childMeshGroup._rootRenderUnit._meshGroupTransform;

				apOptRootUnit prevOptRootUnit = null;
				for (int i = 0; i < prevObjects.Count; i++)
				{
					prevOptRootUnit = prevObjects[i];


					if (prevOptRootUnit._rootOptTransform != null)
					{

						//동일한 OptTransform을 가진다면 복사 가능함
						if (IsOptTransformRecyclable(prevOptRootUnit._rootOptTransform, null, rootMGTransform))
						{
							return prevOptRootUnit;
						}
					}
				}
			}

			return null;
		}

		private bool IsOptTransformRecyclable(apOptTransform prevOptTransform, apTransform_Mesh meshTransform, apTransform_MeshGroup meshGroupTransform)
		{
			if (meshTransform != null)
			{
				if (prevOptTransform._unitType == apOptTransform.UNIT_TYPE.Mesh)
				{
					return prevOptTransform._transformID == meshTransform._transformUniqueID;
				}
			}
			else if (meshGroupTransform != null)
			{
				if (prevOptTransform._unitType == apOptTransform.UNIT_TYPE.Group)
				{
					return prevOptTransform._transformID == meshGroupTransform._transformUniqueID;
				}
			}

			return false;
		}





		private T AddGameObject<T>(string name, Transform parent) where T : MonoBehaviour
		{
			GameObject newGameObject = new GameObject(name);
			newGameObject.transform.parent = parent;
			newGameObject.transform.localPosition = Vector3.zero;
			newGameObject.transform.localRotation = Quaternion.identity;
			newGameObject.transform.localScale = Vector3.one;

			return newGameObject.AddComponent<T>();
		}



		private void MakeMeshGroupToOptTransform(apRenderUnit renderUnit,
													apTransform_MeshGroup meshGroupTransform,
													Transform parent, apOptTransform parentTransform,
													apOptRootUnit targetOptRootUnit,
													apBakeLinkManager bakeLinkManager,
													apBakeResult bakeResult,
													float bakeZScale,
													bool isGammaColorSpace,
													//bool isLWRPShader,//삭제
													apPortrait targetOptPortrait,
													apMeshGroup rootMeshGroup,
													bool isSizeOptimizedV117,
													bool isUseSRP													
													)
		{
			string objectName = meshGroupTransform._nickName;
			int meshGroupUniqueID = -1;
			if (meshGroupTransform._meshGroup != null)
			{
				objectName = meshGroupTransform._meshGroup._name;
				meshGroupUniqueID = meshGroupTransform._meshGroup._uniqueID;
			}

			apMeshGroup meshGroup = meshGroupTransform._meshGroup;

			//if(meshGroupTransform._nickName.Length == 0)
			//{
			//	Debug.LogWarning("Empy Name : " + meshGroupTransform._meshGroup._name);
			//}

			apOptTransform optTransform = null;
			if (bakeLinkManager != null)
			{
				optTransform = bakeLinkManager.FindOptTransform(null, meshGroupTransform);
				if (optTransform != null)
				{
					//재활용에 성공했다.
					optTransform.gameObject.name = objectName;
					optTransform.transform.parent = parent;

					optTransform.transform.localPosition = Vector3.zero;
					optTransform.transform.localRotation = Quaternion.identity;
					optTransform.transform.localScale = Vector3.one;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}
			}

			if (optTransform == null)
			{
				//재활용에 실패했다면 생성
				optTransform = AddGameObject<apOptTransform>(objectName, parent);

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}

			//OptTransform을 설정하자
			#region [미사용 코드] SetBasicSetting 함수로 대체
			//optTransform._transformID = meshGroupTransform._transformUniqueID;
			//optTransform._transform = optTransform.transform;

			//optTransform._depth = meshGroupTransform._depth;
			//optTransform._defaultMatrix = new apMatrix(meshGroupTransform._matrix);

			////optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth * 0.1f);
			//optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth);
			//optTransform._transform.localRotation = Quaternion.Euler(0.0f, 0.0f, optTransform._defaultMatrix._angleDeg);
			//optTransform._transform.localScale = optTransform._defaultMatrix._scale; 
			#endregion

			int renderUnitLevel = -1;
			if (renderUnit != null)
			{
				renderUnitLevel = renderUnit._level;
			}
			optTransform.Bake(targetOptPortrait,//meshGroup, 
								parentTransform,
								targetOptRootUnit,
								meshGroupTransform._nickName,
								meshGroupTransform._transformUniqueID,
								meshGroupUniqueID,
								meshGroupTransform._matrix,
								false,
								renderUnitLevel, meshGroupTransform._depth,
								meshGroupTransform._isVisible_Default,
								meshGroupTransform._meshColor2X_Default,
								bakeZScale,
								isSizeOptimizedV117,
								false,//리깅 옵션. MeshGroupTF는 리깅이 적용되지 않는다.
								false
								);


			


			//첫 초기화 Matrix(No-Mod)를 만들어주자 - Mesh Bake에서 사용된다.
			if (optTransform._matrix_TF_ToParent == null) { optTransform._matrix_TF_ToParent = new apMatrix(); }
			if (optTransform._matrix_TF_ParentWorld_NonModified == null) { optTransform._matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (optTransform._matrix_TFResult_WorldWithoutMod == null) { optTransform._matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			optTransform._matrix_TF_ToParent.SetMatrix(optTransform._defaultMatrix, true);
			optTransform._matrix_TF_ParentWorld_NonModified.SetIdentity();
			if (parentTransform != null)
			{
				optTransform._matrix_TF_ParentWorld_NonModified.SetMatrix(parentTransform._matrix_TFResult_WorldWithoutMod, true);
			}
			optTransform._matrix_TFResult_WorldWithoutMod.SetIdentity();

			//추가 20.8.6. [RMultiply Scale 이슈]
			optTransform._matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();


			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ToParent, false);
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ParentWorld_NonModified, true);


			//RootUnit에 등록하자
			targetOptRootUnit.AddChildTransform(optTransform, rootMeshGroup.SortedBuffer.GetBufferData(renderUnit));


			//apBone을 추가해주자
			if (meshGroup._boneList_All.Count > 0)
			{
				MakeOptBone(meshGroup, optTransform, targetOptRootUnit, bakeLinkManager, bakeResult);
			}
			else
			{
				optTransform._boneList_All = null;
				optTransform._boneList_Root = null;
				optTransform._isBoneUpdatable = false;
			}




			//추가
			//소켓을 붙이자
			if (meshGroupTransform._isSocket)
			{
				apOptNode socketNode = null;
				if (bakeLinkManager != null)
				{
					socketNode = bakeLinkManager.FindOptTransformSocket(optTransform);
					if (socketNode != null)
					{
						socketNode.gameObject.name = meshGroupTransform._nickName + " Socket";
						socketNode.transform.parent = optTransform.transform;
						socketNode.transform.localPosition = Vector3.zero;
						socketNode.transform.localRotation = Quaternion.identity;
						socketNode.transform.localScale = Vector3.one;

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();
					}

				}

				if (socketNode == null)
				{
					socketNode = AddGameObject<apOptNode>(meshGroupTransform._nickName + " Socket", optTransform.transform);

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}
				optTransform._socketTransform = socketNode.transform;
			}
			else
			{
				optTransform._socketTransform = null;
			}


			if (parentTransform != null)
			{
				parentTransform.AddChildTransforms(optTransform);
			}

			//만약 Root라면 ->
			if (parentTransform == null)
			{
				targetOptRootUnit._rootOptTransform = optTransform;
			}
			targetOptPortrait._optTransforms.Add(optTransform);


			if (renderUnit != null)
			{
				for (int i = 0; i < renderUnit._childRenderUnits.Count; i++)
				{
					apRenderUnit childRenderUnit = renderUnit._childRenderUnits[i];

					apTransform_MeshGroup childTransform_MeshGroup = childRenderUnit._meshGroupTransform;
					apTransform_Mesh childTransform_Mesh = childRenderUnit._meshTransform;

					if (childTransform_MeshGroup != null)
					{
						MakeMeshGroupToOptTransform(	childRenderUnit, childTransform_MeshGroup, optTransform.transform, optTransform, targetOptRootUnit, 
														bakeLinkManager, bakeResult, bakeZScale, 
														isGammaColorSpace,
														//isLWRPShader, //삭제
														targetOptPortrait, rootMeshGroup, 
														isSizeOptimizedV117, isUseSRP);
					}
					else if (childTransform_Mesh != null)
					{
						MakeMeshToOptTransform(	childRenderUnit, childTransform_Mesh, meshGroup, optTransform.transform, optTransform, targetOptRootUnit, 
												bakeLinkManager, bakeResult, bakeZScale, 
												isGammaColorSpace,
												//isLWRPShader, //삭제
												targetOptPortrait, rootMeshGroup, 
												isSizeOptimizedV117, isUseSRP);
					}
					else
					{
						Debug.LogError("Empty Render Unit");
					}
				}
			}
			else
			{
				Debug.LogError("No RenderUnit");
			}

			#region [미사용 코드] Child 등록 코드 (RenderUnit 없음)
			//apMeshGroup meshGroup = meshGroupTransform._meshGroup;
			////Child를 연결하자
			//if (meshGroup != null)
			//{

			//	// Child Mesh를 등록한다.
			//	if (meshGroup._childMeshTransforms.Count > 0)
			//	{
			//		for (int i = 0; i < meshGroup._childMeshTransforms.Count; i++)
			//		{
			//			apTransform_Mesh childMeshTransform = meshGroup._childMeshTransforms[i];
			//			MakeMeshToOptTransform(childMeshTransform, meshGroup, optTransform.transform);
			//		}
			//	}

			//	//Child MeshGroup을 등록한다.
			//	if(meshGroup._childMeshGroupTransforms.Count > 0)
			//	{
			//		for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
			//		{
			//			apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[i];
			//			MakeMeshGroupToOptTransform(childMeshGroupTransform, optTransform.transform);
			//		}
			//	}
			//} 
			#endregion
		}

		private void MakeMeshToOptTransform(apRenderUnit renderUnit,
												apTransform_Mesh meshTransform,
												apMeshGroup parentMeshGroup,
												Transform parent,
												apOptTransform parentTransform,
												apOptRootUnit targetOptRootUnit,
												apBakeLinkManager bakeLinkManager,
												apBakeResult bakeResult,
												float bakeZScale,
												bool isGammaColorSpace,
												//bool isLWRPShader,//삭제
												apPortrait targetOptPortrait,
												apMeshGroup rootMeshGroup,
												bool isSizeOptimizedV117,
												bool isUseSRP)
		{
			apOptTransform optTransform = null;
			if (bakeLinkManager != null)
			{
				optTransform = bakeLinkManager.FindOptTransform(meshTransform, null);
				if (optTransform != null)
				{
					//재활용에 성공했다.
					optTransform.gameObject.name = meshTransform._nickName;
					optTransform.transform.parent = parent;

					optTransform.transform.localPosition = Vector3.zero;
					optTransform.transform.localRotation = Quaternion.identity;
					optTransform.transform.localScale = Vector3.one;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}

			}

			if (optTransform == null)
			{
				//재활용에 실패했다면 생성
				optTransform = AddGameObject<apOptTransform>(meshTransform._nickName, parent);

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}

			



			//OptTransform을 설정하자
			#region [미사용 코드] SetBasicSetting 함수로 대체
			//optTransform._transformID = meshTransform._transformUniqueID;
			//optTransform._transform = optTransform.transform;

			//optTransform._depth = meshTransform._depth;
			//optTransform._defaultMatrix = new apMatrix(meshTransform._matrix);

			////optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth * 0.1f);
			//optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth);
			//optTransform._transform.localRotation = Quaternion.Euler(0.0f, 0.0f, optTransform._defaultMatrix._angleDeg);
			//optTransform._transform.localScale = optTransform._defaultMatrix._scale; 
			#endregion

			optTransform.Bake(targetOptPortrait, //null, 
								parentTransform,
								targetOptRootUnit,
								meshTransform._nickName,
								meshTransform._transformUniqueID,
								-1,
								meshTransform._matrix,
								true,
								renderUnit._level, meshTransform._depth,
								meshTransform._isVisible_Default,
								meshTransform._meshColor2X_Default,
								bakeZScale,
								isSizeOptimizedV117,
								renderUnit._calculatedStack.IsRigging,//추가 20.8.10 : 리깅 여부를 Bake에 미리 넣는다.
								(targetOptPortrait._flippedMeshOption == apPortrait.FLIPPED_MESH_CHECK.All)//Flip 여부를 리깅본으로 부터 체크할지를 portrait 옵션에서 확인
								);


			//Debug.Log("Mesh OptTransform Bake [" + optTransform.name + "] Pivot : " + meshTransform._matrix._pos);
			//첫 초기화 Matrix(No-Mod)를 만들어주자 - Mesh Bake에서 사용된다.
			if (optTransform._matrix_TF_ToParent == null) { optTransform._matrix_TF_ToParent = new apMatrix(); }
			if (optTransform._matrix_TF_ParentWorld_NonModified == null) { optTransform._matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (optTransform._matrix_TFResult_WorldWithoutMod == null) { optTransform._matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			optTransform._matrix_TF_ToParent.SetMatrix(optTransform._defaultMatrix, true);
			optTransform._matrix_TF_ParentWorld_NonModified.SetIdentity();
			if (parentTransform != null)
			{
				optTransform._matrix_TF_ParentWorld_NonModified.SetMatrix(parentTransform._matrix_TFResult_WorldWithoutMod, true);
			}
			optTransform._matrix_TFResult_WorldWithoutMod.SetIdentity();

			//추가 20.8.6. [RMultiply Scale 이슈]
			optTransform._matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();

			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ToParent, false);
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ParentWorld_NonModified, true);


			//추가
			//소켓을 붙이자
			if (meshTransform._isSocket)
			{
				apOptNode socketNode = null;
				if (bakeLinkManager != null)
				{

					socketNode = bakeLinkManager.FindOptTransformSocket(optTransform);
					if (socketNode != null)
					{
						socketNode.gameObject.name = meshTransform._nickName + " Socket";
						socketNode.transform.parent = optTransform.transform;
						socketNode.transform.localPosition = Vector3.zero;
						socketNode.transform.localRotation = Quaternion.identity;
						socketNode.transform.localScale = Vector3.one;

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();
					}

				}

				if (socketNode == null)
				{
					socketNode = AddGameObject<apOptNode>(meshTransform._nickName + " Socket", optTransform.transform);

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}
				optTransform._socketTransform = socketNode.transform;
			}
			else
			{
				optTransform._socketTransform = null;
			}

			if (parentTransform != null)
			{
				parentTransform.AddChildTransforms(optTransform);
			}

			targetOptPortrait._optTransforms.Add(optTransform);

			//RootUnit에 등록하자
			targetOptRootUnit.AddChildTransform(optTransform, rootMeshGroup.SortedBuffer.GetBufferData(renderUnit));


			//하위에 OptMesh를 만들자
			apMesh mesh = meshTransform._mesh;
			if (mesh != null)
			{
				apOptMesh optMesh = null;

				if (bakeLinkManager != null)
				{
					optMesh = bakeLinkManager.FindOptMesh(optTransform);
					if (optMesh != null)
					{
						optMesh.gameObject.name = meshTransform._nickName + "_Mesh";
						optMesh.transform.parent = optTransform.transform;
						optMesh.transform.localPosition = Vector3.zero;
						optMesh.transform.localRotation = Quaternion.identity;
						optMesh.transform.localScale = Vector3.one;

						//필수 컴포넌트가 비었는지도 확인
						if (optMesh.GetComponent<MeshFilter>() == null)
						{
							optMesh.gameObject.AddComponent<MeshFilter>();
						}
						if (optMesh.GetComponent<MeshRenderer>() == null)
						{
							optMesh.gameObject.AddComponent<MeshRenderer>();
						}

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();

					}
				}
				if (optMesh == null)
				{
					//재활용이 안되었으니 직접 만들자
					optMesh = AddGameObject<apOptMesh>(meshTransform._nickName + "_Mesh", optTransform.transform);
					optMesh.gameObject.AddComponent<MeshFilter>();
					optMesh.gameObject.AddComponent<MeshRenderer>();

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}


				List<apVertex> verts = mesh._vertexData;

				List<Vector3> posList = new List<Vector3>();
				List<Vector2> UVList = new List<Vector2>();
				List<int> IDList = new List<int>();
				List<int> triList = new List<int>();
				List<float> zDepthList = new List<float>();

				apVertex vert = null;
				for (int i = 0; i < verts.Count; i++)
				{
					vert = verts[i];
					posList.Add(vert._pos);
					UVList.Add(vert._uv);
					IDList.Add(vert._uniqueID);
					zDepthList.Add(vert._zDepth);
				}

				for (int i = 0; i < mesh._indexBuffer.Count; i++)
				{
					triList.Add(mesh._indexBuffer[i]);
				}

				Texture2D texture = null;
				apOptTextureData optTextureData = null;//<<연결될 OptTextureData

				//이전 코드
				//if (mesh._textureData != null)
				//{
				//	texture = mesh._textureData._image;
				//	optTextureData = targetOptPortrait._optTextureData.Find(delegate (apOptTextureData a)
				//	{
				//		return a._srcUniqueID == mesh._textureData._uniqueID;
				//	});
				//}

				//변경 코드 4.1
				if (mesh.LinkedTextureData != null)
				{
					texture = mesh.LinkedTextureData._image;
					optTextureData = targetOptPortrait._optTextureData.Find(delegate (apOptTextureData a)
					{
						return a._srcUniqueID == mesh.LinkedTextureData._uniqueID;
					});
				}

				//Mesh Bake를 하자
				optMesh._portrait = targetOptPortrait;
				optMesh._uniqueID = meshTransform._transformUniqueID;

				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

				//이전 : MaterialSet / Material Library를 사용하지 않는 경우
				////Shader 설정
				//Shader shaderNormal = GetOptMeshShader(meshTransform._shaderType, false, isGammaColorSpace, isLWRPShader);
				//Shader shaderMask = GetOptMeshShader(meshTransform._shaderType, true, isGammaColorSpace, isLWRPShader);
				//Shader shaderAlphaMask = GetOptAlphaMaskShader(isLWRPShader);
				//if (meshTransform._isCustomShader && meshTransform._customShader != null)
				//{
				//	shaderNormal = meshTransform._customShader;
				//	shaderMask = meshTransform._customShader;
				//}


				////통합 재질을 찾자
				//int batchedMatID = -1;
				//if (texture != null && optTextureData != null && !meshTransform._isClipping_Child)
				//{
				//	apOptBatchedMaterial.MaterialUnit batchedMatUnit = targetOptPortrait._optBatchedMaterial.MakeBatchedMaterial_Prev(texture, optTextureData._textureID, shaderNormal);
				//	if (batchedMatUnit != null)
				//	{
				//		batchedMatID = batchedMatUnit._uniqueID;
				//	}
				//}

				//변경 19.6.15 : Material Set / Material Library를 사용하는 경우
				//Mat Info 만들기 전에 다시 Mat Set 다시 설정
				if(meshTransform._isUseDefaultMaterialSet)
				{
					meshTransform._linkedMaterialSet = targetOptPortrait.GetDefaultMaterialSet();
					if(meshTransform._linkedMaterialSet != null)
					{
						meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
					}
				}
				else
				{
					if(meshTransform._materialSetID >= 0)
					{
						meshTransform._linkedMaterialSet = targetOptPortrait.GetMaterialSet(meshTransform._materialSetID);
						if (meshTransform._linkedMaterialSet == null)
						{
							//연결될 MatSet이 없다면.. > 기본값
							meshTransform._linkedMaterialSet = targetOptPortrait.GetDefaultMaterialSet();
							if (meshTransform._linkedMaterialSet != null)
							{
								meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
							}
							else
							{
								meshTransform._materialSetID = -1;
							}
						}
					}
					else
					{
						meshTransform._linkedMaterialSet = null;
					}
				}


				apOptMaterialInfo matInfo = new apOptMaterialInfo();
				int textureDataID = -1;
				int linkedSrcTextureDataID = -1;
				if(meshTransform._mesh != null)
				{
					//기존 방식 (SrcUniqueID : 에디터용을 사용했다.)
					textureDataID = meshTransform._mesh.LinkedTextureDataID;
					linkedSrcTextureDataID = meshTransform._mesh.LinkedTextureDataID;

					apOptTextureData optTexData = targetOptPortrait._optTextureData.Find(delegate(apOptTextureData a)
					{
						return a._srcUniqueID == meshTransform._mesh.LinkedTextureDataID;
					});
					if(optTexData != null)
					{
						//Debug.Log("optTexData를 MatInfo로 저장 : " + optTexData._name + "(" + optTexData._srcUniqueID + ") : " + optTexData._textureID);
						textureDataID = optTexData._textureID;
					}
					else
					{
						//Debug.LogError("실패 : optTexData를 찾지 못했다. : " + meshTransform._mesh.LinkedTextureDataID);
						textureDataID = meshTransform._mesh.LinkedTextureDataID;
					}
					
				}
				matInfo.Bake(meshTransform, targetOptPortrait, !isGammaColorSpace, textureDataID, linkedSrcTextureDataID, Editor.MaterialLibrary);

				Shader shader_AlphaMask = null;
				if(meshTransform._linkedMaterialSet != null)
				{
					shader_AlphaMask = meshTransform._linkedMaterialSet._shader_AlphaMask;
				}
				else
				{
					shader_AlphaMask = targetOptPortrait.GetDefaultMaterialSet()._shader_AlphaMask;
				}

				//Debug.LogError("Bake Mesh : " + meshTransform._nickName);

				//Material Info를 이용하여 BatchedMatID를 찾자
				int batchedMatID = -1;
				if (texture != null && optTextureData != null && !meshTransform._isClipping_Child)
				{
					apOptBatchedMaterial.MaterialUnit batchedMatUnit = targetOptPortrait._optBatchedMaterial.MakeBatchedMaterial_MatInfo(matInfo);
					if (batchedMatUnit != null)
					{
						batchedMatID = batchedMatUnit._uniqueID;
					}
				}
				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

				//Render Texture 크기
				int maskRenderTextureSize = 0;
				switch (meshTransform._renderTexSize)
				{
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_64:		maskRenderTextureSize = 64;		break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_128:	maskRenderTextureSize = 128;	break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_256:	maskRenderTextureSize = 256;	break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_512:	maskRenderTextureSize = 512;	break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_1024:	maskRenderTextureSize = 1024;	break;
					default:
						maskRenderTextureSize = 64;
						Debug.LogError("Unknown RenderTexture Size [" + meshTransform._renderTexSize + "]");
						break;
				}

				bool isVisibleDefault = true;

				if (!meshTransform._isVisible_Default)
				{
					isVisibleDefault = false;
				}
				else
				{
					//Parent로 올라가면서 VisibleDefault가 하나라도 false이면 false
					apRenderUnit curRenderUnit = renderUnit;
					while (true)
					{
						if (curRenderUnit == null) { break; }

						if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
						{
							if (curRenderUnit._meshTransform != null)
							{
								if (!curRenderUnit._meshTransform._isVisible_Default)
								{
									isVisibleDefault = false;
									break;
								}
							}
							else
							{
								break;
							}
						}
						else if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode)
						{
							if (curRenderUnit._meshGroupTransform != null)
							{
								if (!curRenderUnit._meshGroupTransform._isVisible_Default)
								{
									isVisibleDefault = false;
									break;
								}
							}
							else
							{
								break;
							}
						}
						//위로 이동
						curRenderUnit = curRenderUnit._parentRenderUnit;
					}
				}

				//추가 : 그림자 설정
				apPortrait.SHADOW_CASTING_MODE shadowCastMode = targetOptPortrait._meshShadowCastingMode;
				bool receiveShadow = targetOptPortrait._meshReceiveShadow;
				if (!meshTransform._isUsePortraitShadowOption)
				{
					shadowCastMode = meshTransform._shadowCastingMode;
					receiveShadow = meshTransform._receiveShadow;
				}

				//추가 v1.5.0 : Light Probe 설정
				apPortrait.LIGHT_PROBE_USAGE lightProbeUsage = targetOptPortrait._meshLightProbeUsage;
				apPortrait.REFLECTION_PROBE_USAGE reflectionProbeUsage = targetOptPortrait._meshReflectionProbeUsage;

				//변경 21.5.27 : 여기로 이동
				//Parent Transform에 등록하자
				optTransform.SetChildMesh(optMesh);

				//이전 버전의 BakeMesh
				optMesh.BakeMesh(posList.ToArray(),
									UVList.ToArray(),
									IDList.ToArray(),
									triList.ToArray(),
									zDepthList.ToArray(),
									mesh._offsetPos,
									optTransform,
									texture,
									//<텍스쳐 ID가 들어가야 한다.
									(optTextureData != null ? optTextureData._textureID : -1),
									meshTransform._shaderType,

									//이전
									//shaderNormal,
									//shaderMask,
									//shaderAlphaMask,

									//변경 : 19.6.15 : Material Info 이용
									matInfo,
									shader_AlphaMask,

									maskRenderTextureSize,
									isVisibleDefault,
									meshTransform._isClipping_Parent,
									meshTransform._isClipping_Child,
									batchedMatID,
									//batchedMaterial,
									meshTransform._isAlways2Side,
									shadowCastMode,
									receiveShadow,
									lightProbeUsage,
									reflectionProbeUsage,
									isUseSRP
									);

				//역으로 OptTextureData에도 OptMesh를 등록
				if (optTextureData != null)
				{
					optTextureData.AddLinkOptMesh(optMesh);
				}

				//Clipping의 기본 정보를 넣고, 나중에 연결하자
				if (meshTransform._isClipping_Parent)
				{
					List<int> clipIDs = new List<int>();
					for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
					{
						clipIDs.Add(meshTransform._clipChildMeshes[iClip]._transformID);
					}

					optMesh.SetMaskBasicSetting_Parent(clipIDs);
					//optMesh.SetMaskBasicSetting_Parent(meshTransform._clipChildMeshTransformIDs);

					//따로 관리할 마스크 메시에 넣는다.
					//마스크 메시에 추가하는 건 생략한다.
					//Editor._portrait._optMaskedMeshes.Add(optMesh);
				}
				else if (meshTransform._isClipping_Child)
				{
					optMesh.SetMaskBasicSetting_Child(meshTransform._clipParentMeshTransform._transformUniqueID);

					//마스크 메시에 추가하는 건 생략한다.
					//Editor._portrait._optClippedMeshes.Add(optMesh);
				}

				//이전
				////Parent Transform에 등록하자
				//optTransform.SetChildMesh(optMesh);

				targetOptPortrait._optMeshes.Add(optMesh);
			}
		}


		//private Shader GetOptMeshShader(apPortrait.SHADER_TYPE shaderType, bool isClipping, bool isGammaColorSpace, bool isLightweightRenderPipeline)
		//{
		//	string folderPath = null;
		//	string fileName = null;
		//	if (isGammaColorSpace)
		//	{
		//		if (!isLightweightRenderPipeline)
		//		{
		//			//Gamma + Default
		//			folderPath = apShaderGenerator.ShaderPath;

		//			switch (shaderType)
		//			{
		//				case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_ClippedWithMask" : "apShader_Transparent"); break;
		//				case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_ClippedWithMask_Additive" : "apShader_Transparent_Additive"); break;
		//				case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_ClippedWithMask_Multiplicative" : "apShader_Transparent_Multiplicative"); break;
		//				case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_ClippedWithMask_SoftAdditive" : "apShader_Transparent_SoftAdditive"); break;
		//			}
		//		}
		//		else
		//		{
		//			//Gamma + LWRP
		//			folderPath = apShaderGenerator.ShaderPath_LWRP;

		//			switch (shaderType)
		//			{
		//				case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask" : "apShader_LWRP_Transparent"); break;
		//				case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask_Additive" : "apShader_LWRP_Transparent_Additive"); break;
		//				case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask_Multiplicative" : "apShader_LWRP_Transparent_Multiplicative"); break;
		//				case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask_SoftAdditive" : "apShader_LWRP_Transparent_SoftAdditive"); break;
		//			}
		//		}
		//	}
		//	else
		//	{
		//		if (!isLightweightRenderPipeline)
		//		{
		//			//Linear + Default
		//			folderPath = apShaderGenerator.ShaderPath_Linear;

		//			switch (shaderType)
		//			{
		//				case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_L_ClippedWithMask" : "apShader_L_Transparent"); break;
		//				case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_L_ClippedWithMask_Additive" : "apShader_L_Transparent_Additive"); break;
		//				case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_L_ClippedWithMask_Multiplicative" : "apShader_L_Transparent_Multiplicative"); break;
		//				case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_L_ClippedWithMask_SoftAdditive" : "apShader_L_Transparent_SoftAdditive"); break;
		//			}
		//		}
		//		else
		//		{
		//			//Linear + LWRP
		//			folderPath = apShaderGenerator.ShaderPath_Linear_LWRP;

		//			switch (shaderType)
		//			{
		//				case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask" : "apShader_LWRP_L_Transparent"); break;
		//				case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask_Additive" : "apShader_LWRP_L_Transparent_Additive"); break;
		//				case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask_Multiplicative" : "apShader_LWRP_L_Transparent_Multiplicative"); break;
		//				case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask_SoftAdditive" : "apShader_LWRP_L_Transparent_SoftAdditive"); break;
		//			}
		//		}
		//	}

		//	return AssetDatabase.LoadAssetAtPath<Shader>(folderPath + "/" + fileName + ".shader");


		//	#region [미사용 코드] :Material에서 Shader를 추출하는 구식 방법
		//	//Material targetMat = null;
		//	//string materialAssetName = "";
		//	//switch (shaderType)
		//	//{
		//	//	case apPortrait.SHADER_TYPE.AlphaBlend:
		//	//		if (isGammaColorSpace)
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal"; }
		//	//			else				{ materialAssetName = "apMat_Opt_Clipped"; }
		//	//		}
		//	//		else
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal"; }
		//	//			else				{ materialAssetName = "apMat_L_Opt_Clipped"; }
		//	//		}
		//	//		break;

		//	//	case apPortrait.SHADER_TYPE.Additive:
		//	//		if (isGammaColorSpace)
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal Additive"; }
		//	//			else				{ materialAssetName = "apMat_Opt_Clipped Additive"; }
		//	//		}
		//	//		else
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal Additive"; }
		//	//			else				{ materialAssetName = "apMat_L_Opt_Clipped Additive"; }
		//	//		}

		//	//		break;

		//	//	case apPortrait.SHADER_TYPE.SoftAdditive:
		//	//		if(isGammaColorSpace)
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal SoftAdditive"; }
		//	//			else				{ materialAssetName = "apMat_Opt_Clipped SoftAdditive"; }
		//	//		}
		//	//		else
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal SoftAdditive"; }
		//	//			else				{ materialAssetName = "apMat_L_Opt_Clipped SoftAdditive"; }
		//	//		}

		//	//		break;

		//	//	case apPortrait.SHADER_TYPE.Multiplicative:
		//	//		if(isGammaColorSpace)
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal Multiplicative"; }
		//	//			else				{ materialAssetName = "apMat_Opt_Clipped Multiplicative"; }
		//	//		}
		//	//		else
		//	//		{
		//	//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal Multiplicative"; }
		//	//			else				{ materialAssetName = "apMat_L_Opt_Clipped Multiplicative"; }
		//	//		}

		//	//		break;
		//	//}
		//	//if (string.IsNullOrEmpty(materialAssetName))
		//	//{
		//	//	return null;
		//	//}
		//	////경로 변경 : "Assets/Editor/AnyPortraitTool/" => apEditorUtil.ResourcePath_Material
		//	//if (isGammaColorSpace)
		//	//{
		//	//	targetMat = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + materialAssetName + ".mat");
		//	//}
		//	//else
		//	//{
		//	//	//Linear Color Space인 경우 저장된 위치가 다르다
		//	//	targetMat = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/" + materialAssetName + ".mat");
		//	//}
		//	//if (targetMat == null)
		//	//{
		//	//	Debug.LogError("Error : Invalid Shader [" + materialAssetName + "]");
		//	//	return null;
		//	//}

		//	//return targetMat.shader; 
		//	#endregion
		//}

		//private Shader GetOptAlphaMaskShader(bool isLightweightRenderPipeline)
		//{
		//	string assetPath = null;
		//	if (!isLightweightRenderPipeline)
		//	{
		//		//Default
		//		assetPath = apShaderGenerator.ShaderPath + "/" + "apShader_AlphaMask.shader";
		//	}
		//	else
		//	{
		//		//LWRP
		//		assetPath = apShaderGenerator.ShaderPath_LWRP + "/" + "apShader_LWRP_AlphaMask.shader";
		//	}

		//	return AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

		//	#region [미사용 코드]
		//	//Material targetMat = null;
		//	//string materialAssetName = "apMat_Opt_AlphaMask";
		//	////경로 변경 : "Assets/Editor/AnyPortraitTool/" => apEditorUtil.ResourcePath_Material
		//	//targetMat = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + materialAssetName + ".mat");
		//	//if (targetMat == null)
		//	//{
		//	//	Debug.LogError("Error : Invalid Shader [" + materialAssetName + "]");
		//	//	return null;
		//	//}

		//	//return targetMat.shader; 
		//	#endregion
		//}

		private void MakeOptBone(apMeshGroup srcMeshGroup,
									apOptTransform targetOptTransform,
									apOptRootUnit targetOptRootUnit,
									apBakeLinkManager bakeLinkManager,
									apBakeResult bakeResult)
		{
			//1. Bone Group을 만들고
			//2. Bone을 계층적으로 추가하자 (재귀 함수 필요)

			apOptNode boneGroupNode = null;
			if (bakeLinkManager != null)
			{
				boneGroupNode = bakeLinkManager.FindOptBoneGroupNode();
				if (boneGroupNode != null)
				{
					boneGroupNode.gameObject.name = "__Bone Group";
					boneGroupNode.transform.parent = targetOptTransform.transform;
					boneGroupNode.transform.localPosition = Vector3.zero;
					boneGroupNode.transform.localRotation = Quaternion.identity;
					boneGroupNode.transform.localScale = Vector3.one;

					boneGroupNode._param = 100;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();

				}
			}
			if (boneGroupNode == null)
			{
				boneGroupNode = AddGameObject<apOptNode>("__Bone Group", targetOptTransform.transform);
				boneGroupNode._param = 100;//<<Bone Group의 Param은 100이다.

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}


			targetOptTransform._boneGroup = boneGroupNode.transform;
			targetOptTransform._boneList_All = null;
			targetOptTransform._boneList_Root = null;
			targetOptTransform._isBoneUpdatable = true;

			List<apBone> rootBones = srcMeshGroup._boneList_Root;
			List<apOptBone> totalOptBones = new List<apOptBone>();
			for (int i = 0; i < rootBones.Count; i++)
			{
				apOptBone newRootBone = MakeOptBoneRecursive(	srcMeshGroup, rootBones[i], null, 
																targetOptTransform, targetOptRootUnit, totalOptBones, 
																bakeLinkManager, bakeResult);
				targetOptTransform._boneList_Root = apEditorUtil.AddItemToArray<apOptBone>(newRootBone, targetOptTransform._boneList_Root);
			}

			targetOptTransform._boneList_All = totalOptBones.ToArray();



			int nBones = totalOptBones.Count;
			//이제 전체 Bone을 돌면서 링크를 해주자
			for (int i = 0; i < totalOptBones.Count; i++)
			{
				totalOptBones[i].LinkOnBake(targetOptTransform);
			}
			//Root에서부터 LinkChaining을 실행하자
			for (int i = 0; i < targetOptTransform._boneList_Root.Length; i++)
			{
				targetOptTransform._boneList_Root[i].LinkBoneChaining();
			}
		}

		private apOptBone MakeOptBoneRecursive(apMeshGroup srcMeshGroup,
												apBone srcBone,
												apOptBone parentOptBone,
												apOptTransform targetOptTransform,
												apOptRootUnit targetOptRootUnit,
												List<apOptBone> resultOptBones,
												apBakeLinkManager bakeLinkManager,
												apBakeResult bakeResult)
		{
			Transform parentTransform = targetOptTransform._boneGroup;
			if (parentOptBone != null)
			{
				parentTransform = parentOptBone.transform;
			}
			apOptBone newBone = null;

			if (bakeLinkManager != null)
			{
				newBone = bakeLinkManager.FindOptBone(srcBone);
				if (newBone != null)
				{
					newBone.gameObject.name = srcBone._name;
					newBone.transform.parent = parentTransform;
					newBone.transform.localPosition = Vector3.zero;
					newBone.transform.localRotation = Quaternion.identity;
					newBone.transform.localScale = Vector3.one;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}

			}
			if (newBone == null)
			{
				newBone = AddGameObject<apOptBone>(srcBone._name, parentTransform);

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}

			srcBone.GUIUpdate(false);

			//Link를 제외한 Bake를 먼저 하자.
			//Link는 ID를 이용하여 일괄적으로 처리
			newBone.Bake(srcBone);

			
			//RootUnit에 등록하자
			targetOptRootUnit.AddChildBone(newBone);


			if (srcBone._isSocketEnabled)
			{
				//소켓을 붙여주자
				apOptNode socketNode = null;
				if (bakeLinkManager != null)
				{
					socketNode = bakeLinkManager.FindOptBoneSocket(newBone);
					if (socketNode != null)
					{
						socketNode.gameObject.name = srcBone._name + " Socket";
						socketNode.transform.parent = newBone.transform;
						socketNode.transform.localPosition = Vector3.zero;
						socketNode.transform.localRotation = Quaternion.identity;
						socketNode.transform.localScale = Vector3.one;

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();
					}

				}

				if (socketNode == null)
				{
					socketNode = AddGameObject<apOptNode>(srcBone._name + " Socket", newBone.transform);

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}
				newBone._socketTransform = socketNode.transform;
			}

			if (parentOptBone != null)
			{
				newBone._parentBone = parentOptBone;
				parentOptBone._childBones = apEditorUtil.AddItemToArray<apOptBone>(newBone, parentOptBone._childBones);
			}
			else
			{
				//[v1.4.2 버그] TODO : 만약에 Root Bone이 없다면 여기서 해제하는 것이 중요하다.
				//null로 만드는 코드가 없다면 편집하여 부모 본을 해제했을 때 반영이 안되서 에러가 발생한다.
				//if(newBone._parentBone != null)
				//{
				//	Debug.Log("버그 확인");
				//}
				newBone._parentBone = null;//<<중요
			}

			resultOptBones.Add(newBone);
			//하위 Child Bone에 대해서도 반복

			for (int i = 0; i < srcBone._childBones.Count; i++)
			{
				MakeOptBoneRecursive(srcMeshGroup,
										srcBone._childBones[i],
										newBone,
										targetOptTransform,
										targetOptRootUnit,
										resultOptBones,
										bakeLinkManager,
										bakeResult);
			}


			return newBone;
		}

		//추가 20.8.11
		//리깅된 메시 Transform은 미리 모든 버텍스의 리깅 정보를 참고하여
		//리깅된 본들을 리스트>배열로 가진다.
		private void SetRiggingOptBonesToOptTransform(apOptTransform targetOptTransform)
		{
			//Debug.Log("Set RiggingOptBones [" + targetOptTransform.gameObject.name + "]");
			targetOptTransform._riggingBones = null;
			if(targetOptTransform.CalculatedStack == null)
			{
				//Debug.LogError("No CalculateStack");
				return;
			}

			//TODO : 이 코드가 종종 null을 리턴한다..

			List<apOptBone> riggingBones = targetOptTransform.CalculatedStack.GetRiggingBonesForBake();
			if(riggingBones == null || riggingBones.Count == 0)
			{
				//Debug.LogError("No RiggingBones");
				return;
			}

			//리깅된 본을 저장하자
			targetOptTransform.BakeRiggingBones(riggingBones);
		}






		/// <summary>
		/// 만약 사용하지 않는 Monobehaviour 객체가 있는 경우 삭제를 해야한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void CheckAndRemoveUnusedMonobehaviours(apPortrait portrait)
		{
			if (portrait == null)
			{
				return;
			}
			//Monobehaiour는 Mesh, MeshGroup, Modifier이다.
			if (portrait._subObjectGroup_Mesh == null ||
				portrait._subObjectGroup_MeshGroup == null ||
				portrait._subObjectGroup_Modifier == null)
			{
				return;
			}
			//실제로 존재하는 데이터를 정리한다.
			List<GameObject> meshObjects = new List<GameObject>();
			List<GameObject> meshGroupObjects = new List<GameObject>();
			List<GameObject> modifierObjects = new List<GameObject>();

			apMesh mesh = null;
			apMeshGroup meshGroup = null;
			apModifierBase modifier = null;

			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				mesh = portrait._meshes[i];
				if (mesh == null) { continue; }

				meshObjects.Add(mesh.gameObject);
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				meshGroup = portrait._meshGroups[i];
				if (meshGroup == null) { continue; }

				meshGroupObjects.Add(meshGroup.gameObject);

				for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
				{
					modifier = meshGroup._modifierStack._modifiers[iMod];
					if (modifier == null) { continue; }

					modifierObjects.Add(modifier.gameObject);
				}
			}

			//이제 Child GameObject를 확인하자
			int nChild_Mesh = portrait._subObjectGroup_Mesh.transform.childCount;
			int nChild_MeshGroup = portrait._subObjectGroup_MeshGroup.transform.childCount;
			int nChild_Modifier = portrait._subObjectGroup_Modifier.transform.childCount;
			List<GameObject> unusedGameObjects = new List<GameObject>();

			GameObject curGameObject = null;

			//1. Mesh
			for (int i = 0; i < nChild_Mesh; i++)
			{
				curGameObject = portrait._subObjectGroup_Mesh.transform.GetChild(i).gameObject;
				if (!meshObjects.Contains(curGameObject))
				{
					//안쓰는게 나왔다.
					unusedGameObjects.Add(curGameObject);
				}
			}

			//2. MeshGroup
			for (int i = 0; i < nChild_MeshGroup; i++)
			{
				curGameObject = portrait._subObjectGroup_MeshGroup.transform.GetChild(i).gameObject;
				if (!meshGroupObjects.Contains(curGameObject))
				{
					//안쓰는게 나왔다.
					unusedGameObjects.Add(curGameObject);
				}
			}

			//3. Modifier
			for (int i = 0; i < nChild_Modifier; i++)
			{
				curGameObject = portrait._subObjectGroup_Modifier.transform.GetChild(i).gameObject;
				if (!modifierObjects.Contains(curGameObject))
				{
					//안쓰는게 나왔다.
					unusedGameObjects.Add(curGameObject);
				}
			}

			if (unusedGameObjects.Count > 0)
			{
				//Debug.LogError("삭제되어야 하는 게임 오브젝트가 나왔다.");
				for (int i = 0; i < unusedGameObjects.Count; i++)
				{
					//Debug.LogError("[" + i + "] " + unusedGameObjects[i].name);
					Undo.DestroyObjectImmediate(unusedGameObjects[i]);
				}
			}
		}


		/// <summary>
		/// GameObject들의 이름을 갱신하자
		/// Mesh, MeshGroup이 그 대상
		/// </summary>
		/// <param name="portrait"></param>
		public void CheckAndRefreshGameObjectNames(apPortrait portrait)
		{
			//숨어있는 GameObject들의 이름을 갱신한다.
			if (portrait == null)
			{
				return;
			}
			if (portrait._subObjectGroup_Mesh == null ||
				portrait._subObjectGroup_MeshGroup == null ||
				portrait._subObjectGroup_Modifier == null)
			{
				return;
			}
			apMesh mesh = null;
			apMeshGroup meshGroup = null;

			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				mesh = portrait._meshes[i];
				if (mesh == null) { continue; }

				mesh.gameObject.name = mesh._name;
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				meshGroup = portrait._meshGroups[i];
				if (meshGroup == null) { continue; }

				meshGroup.gameObject.name = meshGroup._name;
			}
		}

		//추가

		//버전 1.1.6에서 애니메이션 경로가 "절대 경로"에서 "상대 경로"로 바뀌었다.
		//절대 경로인지 확인하여 상대 경로로 전환한다.
		private void CheckAnimationsBasePathForV116(apPortrait targetPortrait)
		{
			string basePath = apUtil.ConvertEscapeToPlainText(targetPortrait._mecanimAnimClipResourcePath);//변경 21.7.3 (Escape (%20)과 같은 문자)

			bool isUndoRecorded = false;
			if(!string.Equals(basePath, targetPortrait._mecanimAnimClipResourcePath))
			{
				Debug.Log("Escape 문자 발견 [" + targetPortrait._mecanimAnimClipResourcePath + "]");

				isUndoRecorded = true;
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_BakeOptionChanged, 
																			_editor, 
																			targetPortrait, 
																			//targetPortrait, 
																			false,
																			apEditorUtil.UNDO_STRUCT.ValueOnly);

				targetPortrait._mecanimAnimClipResourcePath = basePath;//Escape 문자 삭제
			}

			if (!string.IsNullOrEmpty(basePath))
			{
				//경로를 체크하자
				apEditorUtil.PATH_INFO_TYPE pathInfo = apEditorUtil.GetPathInfo(basePath);
				switch (pathInfo)
				{
					case apEditorUtil.PATH_INFO_TYPE.Absolute_InAssetFolder:
						{
							//Asset 안의 절대 경로 >> 메시지 없이 바로 상대 경로로 바꾼다.
							if (!isUndoRecorded)//위에서 Undo Record가 없었다면
							{
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged,
																	_editor,
																	targetPortrait,
																	//targetPortrait, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
							}

							targetPortrait._mecanimAnimClipResourcePath = apEditorUtil.AbsolutePath2RelativePath(basePath);
						}
						break;

					case apEditorUtil.PATH_INFO_TYPE.Absolute_OutAssetFolder:
					case apEditorUtil.PATH_INFO_TYPE.NotValid:
					case apEditorUtil.PATH_INFO_TYPE.Relative_OutAssetFolder:
						{
							//잘못된 경로이므로 다시 지정하라고 안내
							//1. 일단 안내 메시지를 띄운다 > 
							//2. Okay인 경우 > Save Panel 을 띄운다.
							//3. Save Panel에서 유효한 Path를 리턴 받은 경우 검사
							//4. 유효한 경로라면 저장, 아니라면 다시 경고 메시지 (이때는 저장 불가)
							bool isReset = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Title),
								_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Body),
								_editor.GetText(TEXT.Okay),
								_editor.GetText(TEXT.Cancel));

							if (isReset)
							{
								string nextPath = EditorUtility.SaveFolderPanel("Select to export animation clips", "", "");

								if (!string.IsNullOrEmpty(nextPath))
								{
									//이스케이프 삭제
									nextPath = apUtil.ConvertEscapeToPlainText(nextPath);

									if (apEditorUtil.IsInAssetsFolder(nextPath))
									{
										//유효한 폴더인 경우
										//중요 : 경로가 절대 경로로 찍힌다.
										//상대 경로로 바꾸자
										apEditorUtil.PATH_INFO_TYPE pathInfoType = apEditorUtil.GetPathInfo(nextPath);
										if (pathInfoType == apEditorUtil.PATH_INFO_TYPE.Absolute_InAssetFolder)
										{
											//절대 경로 + Asset 폴더 안쪽이라면
											nextPath = apEditorUtil.AbsolutePath2RelativePath(nextPath);

										}

										if (!isUndoRecorded)//위에서 Undo Record가 없었다면
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged,
																				_editor,
																				targetPortrait,
																				//targetPortrait, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);
										}

										targetPortrait._mecanimAnimClipResourcePath = nextPath;
									}
									else
									{
										//유효한 폴더가 아닌 경우
										EditorUtility.DisplayDialog(
													_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Title),
													_editor.GetText(TEXT.DLG_AnimClipSavePathResetError_Body),
													_editor.GetText(TEXT.Close));
									}
								}
							}
						}
						break;

					case apEditorUtil.PATH_INFO_TYPE.Relative_InAssetFolder:
						//Asset 안의 상대 경로 >> 그대로 둔다. >> 근데 %20이 포함되어 있다면?
						if(basePath.Contains("%20"))
						{
							string nextPath = apEditorUtil.DecodeURLEmptyWord(basePath);

							if (!isUndoRecorded)//위에서 Undo Record가 없었다면
							{
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged,
																	_editor,
																	targetPortrait,
																	//targetPortrait, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
							}

							targetPortrait._mecanimAnimClipResourcePath = nextPath;
						}

						break;
				}
			}
		}


		private bool CreateAnimationsWithMecanim(apPortrait targetPortrait, string basePath)
		{
			if (targetPortrait == null)
			{
				return false;
			}
			if (!targetPortrait._isUsingMecanim)
			{
				return false;
			}

			if(string.IsNullOrEmpty(basePath))
			{
				Debug.LogError("AnyPortrait : The path where animation clip assets are saved is not specified.");
				return false;
			}
			//Debug.Log("Base Path : " + basePath);

			//1. 경로 체크
			if (!basePath.EndsWith("/"))
			{
				basePath += "/";
			}

			basePath = basePath.Replace("\\", "/");

			//추가 21.7.3 : 경로 문제 수정
			basePath = apUtil.ConvertEscapeToPlainText(basePath);


			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(basePath);

			//변경 3.24 : basePath가 절대 경로에서 상대 경로(Assets로 시작되는..)로 바뀌었다.
			//보통은 그래도 경로가 인식이 되는데, 경로 인식이 안된다면 Asset 폴더의 절대 경로를 한번 더 붙여주자
			if (!di.Exists)
			{
				if (basePath.StartsWith("Assets"))
				{
					//상대 경로로서 충분하다면
					string projectRootPath = Application.dataPath;
					//뒤의 Assets을 빼자 (6글자 빼자)
					projectRootPath = projectRootPath.Substring(0, projectRootPath.Length - 6);

					//루트 + / 로 되어 있을 것
					string absPath = projectRootPath + basePath;

					di = new System.IO.DirectoryInfo(absPath);
				}
			}

			if (!di.Exists)
			{
				Debug.LogError("AnyPortrait : Wrong Animation Clip Destination Folder [" + basePath + "]");
				return false;
			}

			string fullPath = di.FullName;

			//AssetDataBase는 Assets 부터 시작해야한다.
			string projectPath = Application.dataPath + "/";

			//Debug.Log("DataPath : " + projectPath);
			//Debug.Log("BasePath : " + basePath);

			System.Uri uri_dataPath = new Uri(projectPath);
			//System.Uri uri_basePath = new Uri(basePath);
			System.Uri uri_basePath = new Uri(fullPath);

			if (!apEditorUtil.IsInAssetsFolder(fullPath))
			{
				Debug.LogError("AnyPortrait : Wrong Animation Clip Destination Folder [" + fullPath + "]");
				return false;
			}

			//string relativePath = "Assets/" + uri_dataPath.MakeRelativeUri(uri_basePath).ToString();
			string relativePath = apUtil.ConvertEscapeToPlainText(uri_dataPath.MakeRelativeUri(uri_basePath).ToString());//변경 21.7.11 : 이스케이프 문자 삭제

			if (!relativePath.StartsWith("Assets/"))
			{
				relativePath = "Assets/" + relativePath;
			}
			if (!relativePath.EndsWith("/"))
			{
				relativePath += "/";
			}
			//Debug.Log("AnimClip Result Path : " + relativePath);

			//2. Animator 체크
			if (targetPortrait._animator == null)
			{
				targetPortrait._animator = targetPortrait.gameObject.AddComponent<Animator>();

				//추가 v1.4.8
				//Root Motion 옵션을 켜서 오히려 위치가 (0, 0, 0)으로 강제되는 것을 막자
				//원래는 false여야 위치가 강제되지 않는데, 여기서는 FBX가 아닌 애니메이션 키값에 원점 위치가 있어서 그걸 무시하기 위해서 true를 입력
				targetPortrait._animator.applyRootMotion = true;
			}

			//3. AnimatorController 있는지 체크 > 없다면 만든다. 다만 있을 경우엔 더이상 수정하지 않는다.
			UnityEditor.Animations.AnimatorController newAnimController = null;
			UnityEditor.Animations.AnimatorController runtimeAnimController = null;
			if (targetPortrait._animator.runtimeAnimatorController == null)
			{
				//AnimatorController는 파일 덮어쓰기는 아예 안되고, 새로 만드는 것만 가능
				//새로 만들자

				string animControllerPath = relativePath + targetPortrait.name + "-AnimController";
				string animControllerExp = "controller";
				string animControllerFullPath = GetNewUniqueAssetName(animControllerPath, animControllerExp, typeof(UnityEditor.Animations.AnimatorController));

				newAnimController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(animControllerFullPath);
				targetPortrait._animator.runtimeAnimatorController = newAnimController;
			}
			else
			{
				runtimeAnimController = targetPortrait._animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			}

			//4. 애니메이션 클립 체크
			//이미 AnimationClip이 있다면 덮어씌운다.
			//없다면 새로 생성한다. 이때 에셋 이름은 "충돌되지 않게" 만든다.

			for (int iAnim = 0; iAnim < targetPortrait._animClips.Count; iAnim++)
			{
				AnimationClip createdAnimClipAsset = CreateAnimationClipAsset(targetPortrait, targetPortrait._animClips[iAnim], relativePath);
				if (createdAnimClipAsset != null)
				{
					//데이터를 저장하자
					//targetPortrait._animClipAssetPairs.Add(new apAnimMecanimData_AssetPair(targetPortrait._animClips[iAnim]._uniqueID, createdAnimClipAsset));
					targetPortrait._animClips[iAnim]._animationClipForMecanim = createdAnimClipAsset;

					if (newAnimController != null)
					{
						//자동으로 생성된 AnimController가 있는 경우
						if (newAnimController.layers.Length > 0)
						{
							//animController.layers[0].stateMachine.AddStateMachineBehaviour()
							UnityEditor.Animations.AnimatorState newMotionState = newAnimController.AddMotion(createdAnimClipAsset, 0);
							newMotionState.motion = createdAnimClipAsset;
							newMotionState.name = targetPortrait._animClips[iAnim]._name;

						}
					}
				}

				EditorUtility.SetDirty(createdAnimClipAsset);
			}

			//추가 : "비어있는 애니메이션 클립"을 만든다.
			AnimationClip emptyAnimClipAsset = CreateEmptyAnimationClipAsset(targetPortrait, relativePath);
			targetPortrait._emptyAnimClipForMecanim = emptyAnimClipAsset;

			if (newAnimController != null)
			{
				//자동으로 생성된 AnimController가 있는 경우
				if (newAnimController.layers.Length > 0)
				{
					//animController.layers[0].stateMachine.AddStateMachineBehaviour()
					UnityEditor.Animations.AnimatorState newMotionState = newAnimController.AddMotion(emptyAnimClipAsset, 0);
					newMotionState.motion = emptyAnimClipAsset;
					newMotionState.name = "Empty";
				}
			}

			EditorUtility.SetDirty(emptyAnimClipAsset);



			//4. 1차적으로 레이어 Refresh
			//이름으로 비교하여 없으면 추가, 있으면 넣기 방식으로 갱신한다.
			List<apAnimMecanimData_Layer> mecanimLayers = new List<apAnimMecanimData_Layer>();

			if (newAnimController != null || runtimeAnimController != null)
			{
				UnityEditor.Animations.AnimatorController curAnimController = null;
				if (newAnimController != null)
				{
					curAnimController = newAnimController;
				}
				else
				{
					curAnimController = runtimeAnimController;
				}

				if (curAnimController.layers != null && curAnimController.layers.Length > 0)
				{
					for (int iLayer = 0; iLayer < curAnimController.layers.Length; iLayer++)
					{
						apAnimMecanimData_Layer newLayerData = new apAnimMecanimData_Layer();
						newLayerData._layerIndex = iLayer;
						newLayerData._layerName = curAnimController.layers[iLayer].name;
						newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Unknown;
						switch (curAnimController.layers[iLayer].blendingMode)
						{
							case UnityEditor.Animations.AnimatorLayerBlendingMode.Override:
								newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Override;
								break;

							case UnityEditor.Animations.AnimatorLayerBlendingMode.Additive:
								newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Additive;
								break;
						}
						mecanimLayers.Add(newLayerData);
					}
				}

				targetPortrait._animatorLayerBakedData.Clear();
				for (int i = 0; i < mecanimLayers.Count; i++)
				{
					targetPortrait._animatorLayerBakedData.Add(new apAnimMecanimData_Layer(mecanimLayers[i]));
				}
			}



			apEditorUtil.SetDirty(_editor);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return true;
		}

		//Animation Clip 만들기
		private AnimationClip CreateAnimationClipAsset(apPortrait targetPortrait, apAnimClip targetAnimClip, string basePath)
		{
			//이미 AnimationClip이 있다면 덮어씌운다.
			//없다면 새로 생성한다. 이때 에셋 이름은 "충돌되지 않게" 만든다.

			float timeLength = targetAnimClip.TimeLength;

			AnimationClip resultAnimClip = null;



			string animClipAssetPath = "";
			bool isCreate = false;
			if (targetAnimClip._animationClipForMecanim != null)
			{
				//1. 이미 저장된 AnimationClip이 있는 경우
				//> 저장된 에셋 Path와 이름을 공유한다. 
				//> 해당 에셋을 덮어씌운다.
				//수정 : 덮어씌우지 말고 이걸 그냥 수정할 순 없을까
				resultAnimClip = targetAnimClip._animationClipForMecanim;
				animClipAssetPath = AssetDatabase.GetAssetPath(targetAnimClip._animationClipForMecanim);
				isCreate = false;
			}
			else
			{
				isCreate = true;
			}

			if (string.IsNullOrEmpty(animClipAssetPath))
			{
				//2. 새로 만들어야 하는 경우 or Asset 경로를 찾지 못했을 경우
				//> "겹치지 않는 이름"으로 생성한다.
				resultAnimClip = new AnimationClip();
				resultAnimClip.name = targetPortrait.name + "-" + targetAnimClip._name;
				animClipAssetPath = GetNewUniqueAssetName(basePath + resultAnimClip.name, "anim", typeof(AnimationClip));
				isCreate = true;
			}

			resultAnimClip.legacy = false;
			if (targetAnimClip.IsLoop)
			{
				resultAnimClip.wrapMode = WrapMode.Loop;
			}
			else
			{
				resultAnimClip.wrapMode = WrapMode.Once;
			}
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));

			if (isCreate)
			{
				AssetDatabase.CreateAsset(resultAnimClip, animClipAssetPath);
			}


			return AssetDatabase.LoadAssetAtPath<AnimationClip>(animClipAssetPath);

		}



		private string GetNewUniqueAssetName(string assetPathWOExtension, string extension, System.Type type)
		{
			if (AssetDatabase.LoadAssetAtPath(assetPathWOExtension + "." + extension, type) != null)
			{
				//에셋이 이미 존재한다.
				//이름을 바꾼다.
				int newNameIndex = 1;
				string newName = "";
				while (true)
				{
					newName = assetPathWOExtension + " (" + newNameIndex + ")." + extension;

					if (AssetDatabase.LoadAssetAtPath(newName, type) == null)
					{
						//겹치는게 없다.
						return newName;//새로운 이름을 찾았다.
					}

					newNameIndex++;
				}
			}
			else
			{
				//에셋이 없다. 그대로 사용하자
				return assetPathWOExtension + "." + extension;
			}
		}



		private AnimationClip CreateEmptyAnimationClipAsset(apPortrait targetPortrait, string basePath)
		{
			//이미 AnimationClip이 있다면 덮어씌운다.
			//없다면 새로 생성한다. 이때 에셋 이름은 "충돌되지 않게" 만든다.

			float timeLength = 1.0f;

			if (targetPortrait._emptyAnimClipForMecanim != null)
			{
				return targetPortrait._emptyAnimClipForMecanim;
			}
			AnimationClip resultAnimClip = new AnimationClip();
			resultAnimClip.name = targetPortrait.name + "-Empty";
			string animClipAssetPath = GetNewUniqueAssetName(basePath + resultAnimClip.name, "anim", typeof(AnimationClip));

			resultAnimClip.legacy = false;
			resultAnimClip.wrapMode = WrapMode.Loop;

			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));

			AssetDatabase.CreateAsset(resultAnimClip, animClipAssetPath);
			return AssetDatabase.LoadAssetAtPath<AnimationClip>(animClipAssetPath);

		}


		//19.8.5 : LWRP 옵션은 있어도 Material Library
		//		//추가 11.6 : 만약 LWRP Shader를 사용한다면, 
		//		private void CheckAndCreateLWRPShader()
		//		{
		//			if (_editor == null)
		//			{
		//				return;
		//			}
		//#if UNITY_2018_1_OR_NEWER
		//#else
		//			//하위버전에서는 설정이 False로 강제된다.
		//			_editor._isUseLWRPShader = false;
		//#endif
		//			if (!_editor._isUseLWRPShader)
		//			{
		//				return;
		//			}

		//			//LWRP Shader를 사용한다.
		//			apShaderGenerator shaderGen = new apShaderGenerator();
		//			if (!shaderGen.IsAnyMissingLWRPShader)
		//			{
		//				//모두 로드되었다고 합니다.
		//				return;
		//			}

		//			//LWRP Shader를 만들자.
		//			shaderGen.GenerateLWRPShaders();
		//		} 

		//추가 20.1.28 : Bake시 Color Space에 맞추어서 모든 TextureData를 확인하여 일괄 변환할지 물어보고 처리하자
		private void CheckAndChangeTextureDataColorSpace(apPortrait targetPortrait)
		{
			if(targetPortrait == null)
			{
				return;
			}


			//True이면 Gamma, False면 Linear
			//bool isGammaSpace = Editor._isBakeColorSpaceToGamma;//이전
			bool isGammaSpace = Editor.ProjectSettingData.Project_IsColorSpaceGamma;//변경 22.12.19 [v1.4.2]


			int nTextureData = targetPortrait._textureData == null ? 0 : targetPortrait._textureData.Count;
			
			bool isAllSameColorSpace = true;//모두 같은 ColorSpace인가
			apTextureData curTexData = null;
			TextureImporter textureImporter = null;
			
			for (int iTex = 0; iTex < nTextureData; iTex++)
			{
				curTexData = targetPortrait._textureData[iTex];
				if(curTexData._image == null)
				{
					continue;
				}
				string path = AssetDatabase.GetAssetPath(curTexData._image);
				textureImporter = (TextureImporter)TextureImporter.GetAtPath(path);

				if(textureImporter != null)
				{
					bool isGammaTexture = textureImporter.sRGBTexture;
					textureImporter = null;

					if(isGammaSpace != isGammaTexture)
					{
						//하나라도 다르다면
						isAllSameColorSpace = false;
						break;
					}
				}
			}

			if(isAllSameColorSpace)
			{
				//모두 Color Space가 같네염
				return;
			}

			//물어봅시다.
			//apStringFactory.I.Gamma
			//"Color Space Correction"
			//"The value of the Color Space of some Images differs from the current setting.\nDo you want to change the Color Space of all the Images to [] to be the same as the current setting?"
			bool result = EditorUtility.DisplayDialog(
				Editor.GetText(TEXT.DLG_CorrectionImageColorSpace_Title), 
				Editor.GetTextFormat(TEXT.DLG_CorrectionImageColorSpace_Body, isGammaSpace ? apStringFactory.I.Gamma : apStringFactory.I.Linear),
				Editor.GetText(TEXT.Okay), 
				Editor.GetText(TEXT.Cancel));

			if(!result)
			{
				return;
			}

			//모든 텍스쳐의 Color Space
			for (int iTex = 0; iTex < nTextureData; iTex++)
			{
				curTexData = targetPortrait._textureData[iTex];
				if (curTexData._image == null)
				{
					continue;
				}
				string path = AssetDatabase.GetAssetPath(curTexData._image);
				textureImporter = (TextureImporter)TextureImporter.GetAtPath(path);
				if(textureImporter != null)
				{
					bool isGammaTexture = textureImporter.sRGBTexture;

					if(isGammaSpace != isGammaTexture)
					{
						textureImporter.sRGBTexture = isGammaSpace;
						textureImporter.SaveAndReimport();
					}

					textureImporter = null;
				}
			}

			AssetDatabase.Refresh();
		}


		/// <summary>
		/// 추가 20.11.7
		/// 이미지가 지정되지 않은 메시가 있다면 Bake를 할 수 없다.
		/// </summary>
		/// <returns>문제가 되는 메시가 없어서 Bake를 할 수 있는 상황이면 True, 없으면 False</returns>
		private bool CheckIfAnyNoImageMesh(apPortrait targetPortrait)
		{
			int nMeshes = targetPortrait._meshes.Count;
			apMesh curMesh = null;
			
			List<apMesh> wrongMeshes = new List<apMesh>();
			bool isAnyNoImageMesh = false;

			for (int i = 0; i < nMeshes; i++)
			{
				curMesh = targetPortrait._meshes[i];

				//이미지가 없는 메시가 있다.
				if(curMesh._textureData_Linked == null)
				{
					wrongMeshes.Add(curMesh);
					isAnyNoImageMesh = true;
				}
				else if(curMesh._textureData_Linked._image == null)
				{
					//Image는 연결되었지만 텍스쳐 에셋이 없다..
					wrongMeshes.Add(curMesh);
					isAnyNoImageMesh = true;
				}
			}
			
			if(isAnyNoImageMesh)
			{
				//문제가 있는 메시가 있다.
				//메시지를 보여주자
				apStringWrapper strMeshes = new apStringWrapper(128);
				if(wrongMeshes.Count == 1)
				{
					strMeshes.Append(wrongMeshes[0]._name, false);
				}
				else
				{
					for (int i = 0; i < wrongMeshes.Count; i++)
					{
						if(i > 3)
						{
							//개수가 너무 많다.
							strMeshes.Append(apStringFactory.I.Dot3, false);
							strMeshes.Append(apStringFactory.I.Return, false);
							break;
						}

						strMeshes.Append(wrongMeshes[i]._name, false);
						if(i < wrongMeshes.Count - 1)
						{
							strMeshes.Append(apStringFactory.I.Return, false);
						}
					}
				}
				strMeshes.MakeString();

				bool result = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_NoImageMesh_Title),
															Editor.GetTextFormat(TEXT.DLG_NoImageMesh_Body, strMeshes.ToString()),
															Editor.GetText(TEXT.Okay),
															Editor.GetText(TEXT.Ignore));

				if(result)
				{
					//에러가 발생했고 Bake는 중지
					return false;
				}
				else
				{
					//에러가 발생했지만 무시
					return true;
				}				
			}
			
			//에러가 없다.
			return true;
		}


		//------------------------------------------------------------------------------------
		// Optimized Bake
		//------------------------------------------------------------------------------------
		/// <summary>
		/// 현재 Portrait를 실행가능한 버전으로 Bake하자
		/// </summary>
		public apBakeResult Bake_Optimized(apPortrait srcPortrait, apPortrait targetOptPortrait)
		{
			if (srcPortrait == null)
			{
				return null;
			}


			//추가 20.11.7
			//이미지가 설정되지 않은 메시가 있다면 에러가 발생한다.
			//미리 안내를 하자
			if(!CheckIfAnyNoImageMesh(srcPortrait))
			{
				//에러가 발생해서 Bake 취소
				return null;
			}


			//추가 19.5.26 : v1.1.7에 추가된 "용량 최적화 옵션"이 적용되어 Bake를 하는가?
			bool isSizeOptimizedV117 = true;
			//bool isSizeOptimizedV117 = false;//테스트

			//추가 19.8.5
			//bool isUseSRP = Editor._isUseSRP;//이전
			bool isUseSRP = Editor.ProjectSettingData.Project_IsUseSRP;//변경 [v1.4.2]
			bool isBakeGammaColorSpace = Editor.ProjectSettingData.Project_IsColorSpaceGamma;//추가 [v1.4.2]

			//apEditorUtil.SetEditorDirty();
			EditorUtility.SetDirty(srcPortrait);

			apBakeResult bakeResult = new apBakeResult();

			//Optimized에서 타겟이 되는 Portrait가 없다면 새로 만들어준다.
			if (targetOptPortrait == null)
			{
				GameObject dstPortraitGameObj = new GameObject(srcPortrait.gameObject.name + " (Optimized)");
				dstPortraitGameObj.transform.parent = srcPortrait.transform.parent;
				dstPortraitGameObj.transform.localPosition = srcPortrait.transform.localPosition;
				dstPortraitGameObj.transform.localRotation = srcPortrait.transform.localRotation;
				dstPortraitGameObj.transform.localScale = srcPortrait.transform.localScale;

				dstPortraitGameObj.layer = srcPortrait.gameObject.layer;

				targetOptPortrait = dstPortraitGameObj.AddComponent<apPortrait>();
			}

			//추가 20.9.14 : 만약 targetOptPortrait가 Prefab으로 만들어진 상태라면, 연결을 끊어야 한다. (안그러면 에러가 난다.)
			//갱신 > 조회 > 안내 > Disconnect 순서
			apEditorUtil.CheckAndRefreshPrefabInfo(targetOptPortrait);

			if (apEditorUtil.IsPrefabConnected(targetOptPortrait.gameObject))
			{
				//Prefab 해제 안내
				if (EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_PrefabDisconn_Title),
													Editor.GetText(TEXT.DLG_PrefabDisconn_Body),
													Editor.GetText(TEXT.Okay)))
				{	
					apEditorUtil.DisconnectPrefab(targetOptPortrait);
				}
			}



			//< Optimized Bake와 일반 Bake의 차이 >
			//- 순서는 일반 Bake와 동일하게 처리된다. (참조 에러를 막기 위해 Instantiate 등의 방법을 제외한다)
			//- 생성/제거되는 GameObject는 모두 taretOptPortrait에 속한다.
			//- 데이터는 srcPortrait에서 가져온다.
			//- 이 코드내에 Editor._portrait는 한번도 등장해선 안된다.

			//< 일단 Bake 했으니 초기 정보를 연결해준다. >
			//0. Bake 했다는 기본 정보 복사
			targetOptPortrait._isOptimizedPortrait = true;
			targetOptPortrait._bakeSrcEditablePortrait = srcPortrait;

			srcPortrait._bakeTargetOptPortrait = targetOptPortrait;

			//Editable GameObject로 저장되는 정보를 제외하고 모두 복사한다.
			//1. Controller 복사
			targetOptPortrait._controller._portrait = targetOptPortrait;
			targetOptPortrait._controller._controlParams.Clear();

			for (int iCP = 0; iCP < srcPortrait._controller._controlParams.Count; iCP++)
			{
				apControlParam srcParam = srcPortrait._controller._controlParams[iCP];

				apControlParam newParam = new apControlParam();
				newParam._portrait = targetOptPortrait;
				newParam.CopyFromControlParam(srcParam);//<<복사하자

				//리스트에 추가
				targetOptPortrait._controller._controlParams.Add(newParam);
			}

			//2. AnimClip 복사 (링크정보에 관한건 제외하고)
			// (AnimPlayManager는 나중에 Link하면 자동으로 연결됨)

			//추가 10.5 : 기존에 생성되었던 Animation Clip Asset은 없어지면 안된다.
			Dictionary<int, AnimationClip> animID2AnimAssets = new Dictionary<int, AnimationClip>();
			if (targetOptPortrait._animClips != null && targetOptPortrait._animClips.Count > 0)
			{
				for (int i = 0; i < targetOptPortrait._animClips.Count; i++)
				{
					apAnimClip beforeAnimClip = targetOptPortrait._animClips[i];
					if (beforeAnimClip != null && beforeAnimClip._animationClipForMecanim != null)
					{
						if (!animID2AnimAssets.ContainsKey(beforeAnimClip._uniqueID))
						{
							animID2AnimAssets.Add(beforeAnimClip._uniqueID, beforeAnimClip._animationClipForMecanim);
						}
					}
				}
			}
			targetOptPortrait._animClips.Clear();

			for (int iAnim = 0; iAnim < srcPortrait._animClips.Count; iAnim++)
			{
				apAnimClip srcAnimClip = srcPortrait._animClips[iAnim];

				//AnimClip을 Src로 부터 복사해서 넣자
				apAnimClip newAnimClip = new apAnimClip();
				newAnimClip.CopyFromAnimClip(srcAnimClip);

				if (animID2AnimAssets.ContainsKey(newAnimClip._uniqueID))
				{
					//추가 : Mecanim에 사용된 AnimAsset을 재활용해야한다.
					newAnimClip._animationClipForMecanim = animID2AnimAssets[newAnimClip._uniqueID];
				}

				targetOptPortrait._animClips.Add(newAnimClip);
			}

			//3. MainMeshGroup ID 복사
			targetOptPortrait._mainMeshGroupIDList.Clear();
			for (int iMainMG = 0; iMainMG < srcPortrait._mainMeshGroupIDList.Count; iMainMG++)
			{
				//ID(int) 복사
				targetOptPortrait._mainMeshGroupIDList.Add(srcPortrait._mainMeshGroupIDList[iMainMG]);
			}

			//4. 다른 정보들 복사
			targetOptPortrait._FPS = srcPortrait._FPS;

			targetOptPortrait._bakeScale = srcPortrait._bakeScale;
			targetOptPortrait._bakeZSize = srcPortrait._bakeZSize;

			targetOptPortrait._imageFilePath_Thumbnail = srcPortrait._imageFilePath_Thumbnail;

			targetOptPortrait._isImportant = srcPortrait._isImportant;
			targetOptPortrait._autoPlayAnimClipID = srcPortrait._autoPlayAnimClipID;

			targetOptPortrait._sortingLayerID = srcPortrait._sortingLayerID;
			targetOptPortrait._sortingOrder = srcPortrait._sortingOrder;

			targetOptPortrait._isUsingMecanim = srcPortrait._isUsingMecanim;
			targetOptPortrait._mecanimAnimClipResourcePath = srcPortrait._mecanimAnimClipResourcePath;

			targetOptPortrait._billboardType = srcPortrait._billboardType;
			targetOptPortrait._meshShadowCastingMode = srcPortrait._meshShadowCastingMode;
			targetOptPortrait._meshReceiveShadow = srcPortrait._meshReceiveShadow;

			//[v1.5.0 추가]
			targetOptPortrait._billboardParentRotation = srcPortrait._billboardParentRotation;

			//[v1.5.0 추가]
			targetOptPortrait._meshLightProbeUsage = srcPortrait._meshLightProbeUsage;
			targetOptPortrait._meshReflectionProbeUsage = srcPortrait._meshReflectionProbeUsage;

			targetOptPortrait._vrRenderTextureSize = srcPortrait._vrRenderTextureSize;
			targetOptPortrait._vrSupportMode = srcPortrait._vrSupportMode;
			targetOptPortrait._flippedMeshOption = srcPortrait._flippedMeshOption;
			targetOptPortrait._rootBoneScaleMethod = srcPortrait._rootBoneScaleMethod;//<<이것도 추가

			//추가 [v1.4.0]
			targetOptPortrait._isTeleportCorrectionOption = srcPortrait._isTeleportCorrectionOption;
			targetOptPortrait._teleportMovementDist = srcPortrait._teleportMovementDist;

			//추가 [v1.5.0] : 텔레포트 옵션 추가
			targetOptPortrait._teleportRotationOffset = srcPortrait._teleportRotationOffset;
			targetOptPortrait._teleportScaleOffset = srcPortrait._teleportScaleOffset;
			targetOptPortrait._teleportPositionEnabled = srcPortrait._teleportPositionEnabled;
			targetOptPortrait._teleportRotationEnabled = srcPortrait._teleportRotationEnabled;
			targetOptPortrait._teleportScaleEnabled = srcPortrait._teleportScaleEnabled;

			targetOptPortrait._unspecifiedAnimControlParamOption = srcPortrait._unspecifiedAnimControlParamOption;


			//추가 [v1.4.8] 옵션 복사
			targetOptPortrait._meshRefreshRateOption = srcPortrait._meshRefreshRateOption;
			targetOptPortrait._meshRefreshRateFPS = srcPortrait._meshRefreshRateFPS;
			targetOptPortrait._mainProcessEvent = srcPortrait._mainProcessEvent;

			//추가 [v1.4.9]
			targetOptPortrait._meshRefreshFPSScaleOption = srcPortrait._meshRefreshFPSScaleOption;

			//추가 [v1.4.8]
			targetOptPortrait._rootMotionModeOption = srcPortrait._rootMotionModeOption;			
			targetOptPortrait._rootMotionAxisOption_X = srcPortrait._rootMotionAxisOption_X;
			targetOptPortrait._rootMotionAxisOption_Y = srcPortrait._rootMotionAxisOption_Y;
			targetOptPortrait._rootMotionTargetTransformType = srcPortrait._rootMotionTargetTransformType;

			//주의 : 루트 모션 중 "지정된 Parent Transform 객체"는 복사하면 안된다.
			//targetOptPortrait._rootMotionSpecifiedParentTransform = srcPortrait._rootMotionSpecifiedParentTransform;//<<이거 주석 풀지 말것.

			//추가 [v1.5.1]
			targetOptPortrait._invisibleMeshUpdate = srcPortrait._invisibleMeshUpdate;
			targetOptPortrait._clippingMeshUpdate = srcPortrait._clippingMeshUpdate;


			//4-2. Material Set 복사
			targetOptPortrait._materialSets.Clear();
			for (int i = 0; i < srcPortrait._materialSets.Count; i++)
			{
				apMaterialSet srcMatSet = srcPortrait._materialSets[i];
				apMaterialSet copiedMatSet = new apMaterialSet();
				copiedMatSet.CopyFromSrc(srcMatSet, srcMatSet._uniqueID, false, false, srcMatSet._isDefault);
				targetOptPortrait._materialSets.Add(copiedMatSet);
			}


			//추가 10.26 : Bake에서는 빌보드가 꺼져야 한다.
			//임시로 껐다가 마지막에 다시 복구
			apPortrait.BILLBOARD_TYPE billboardType = targetOptPortrait._billboardType;
			targetOptPortrait._billboardType = apPortrait.BILLBOARD_TYPE.None;//임시로 끄자
			




			//추가 21.3.11
			// Scale 이슈가 있다.
			// Bake 전에 이미 Scale이 음수인 경우, Bake 직후나 Link후 플레이시 메시가 거꾸로 보이게 된다.
			//따라서 portrait부터 시작해서 상위의 모든 GameObject의 Sca;e을 저장했다가 복원해야한다.
			Dictionary<Transform, Vector3> prevTransformScales = new Dictionary<Transform, Vector3>();
			Transform curScaleCheckTransform = targetOptPortrait.transform;
			while(true)
			{
				prevTransformScales.Add(curScaleCheckTransform, curScaleCheckTransform.localScale);
				curScaleCheckTransform.localScale = Vector3.one;//일단 기본으로 강제 적용
				if(curScaleCheckTransform.parent == null)
				{
					break;
				}
				curScaleCheckTransform = curScaleCheckTransform.parent;
			}




			// 지금부터는 일반 Bake처럼 진행이 된다.
			// 1. Editor._portrait대신 targetOptPortrait를 사용한다.
			// 2. 데이터는 Mesh, MeshGroup, Modifier 정보는 srcPortrait 정보를 사용한다.


			//Bake 방식 변경
			//일단 숨겨진 GameObject를 제외한 모든 객체를 리스트로 저장한다.
			//LinkParam 형태로 저장을 한다.
			//LinkParam으로 저장하면서 <apOpt 객체>와 <그렇지 않은 객체>를 구분한다.
			//"apOpt 객체"는 나중에 (1)재활용 할지 (2) 삭제 할지 결정한다.
			//"그렇지 않은 GameObject"는 Hierarchy 정보를 가진채 (1) 링크를 유지할 지(재활용되는 경우) (2) Unlink Group에 넣을지 결정한다.
			//만약 재활용되지 않는 (apOpt GameObject)에서 알수 없는 Component가 발견된 경우 -> 이건 삭제 예외 대상에 넣는다.

			//분류를 위한 그룹
			//1. ReadyToRecycle
			// : 기존에 RootUnit과 그 하위에 있었던 GameObject들이다. 분류 전에 일단 여기로 들어간다.
			// : 분류 후에는 원칙적으로 하위에 어떤 객체도 남아선 안된다.

			//2. RemoveTargets
			// : apOpt를 가진 GameObject 그룹 중에서 사용되지 않았던 그룹이다. 
			// : 처리 후에는 이 GameObject를 통째로 삭제한다.

			//3. UnlinkedObjects
			// : apOpt를 가지지 않은 GameObject중에서 재활용되지 않은 객체들


			GameObject groupObj_1_ReadyToRecycle = new GameObject("__Baking_1_ReadyToRecycle");
			GameObject groupObj_2_RemoveTargets = new GameObject("__Baking_2_RemoveTargets");


			GameObject groupObj_3_UnlinkedObjects = null;
			if (targetOptPortrait._bakeUnlinkedGroup == null)
			{
				groupObj_3_UnlinkedObjects = new GameObject("__UnlinkedObjects");
				targetOptPortrait._bakeUnlinkedGroup = groupObj_3_UnlinkedObjects;
			}
			else
			{
				groupObj_3_UnlinkedObjects = targetOptPortrait._bakeUnlinkedGroup;
				groupObj_3_UnlinkedObjects.name = "__UnlinkedObjects";
			}




			groupObj_1_ReadyToRecycle.transform.parent = targetOptPortrait.transform;
			groupObj_2_RemoveTargets.transform.parent = targetOptPortrait.transform;
			groupObj_3_UnlinkedObjects.transform.parent = targetOptPortrait.transform;

			groupObj_1_ReadyToRecycle.transform.localPosition = Vector3.zero;
			groupObj_2_RemoveTargets.transform.localPosition = Vector3.zero;
			groupObj_3_UnlinkedObjects.transform.localPosition = Vector3.zero;

			groupObj_1_ReadyToRecycle.transform.localRotation = Quaternion.identity;
			groupObj_2_RemoveTargets.transform.localRotation = Quaternion.identity;
			groupObj_3_UnlinkedObjects.transform.localRotation = Quaternion.identity;

			groupObj_1_ReadyToRecycle.transform.localScale = Vector3.one;
			groupObj_2_RemoveTargets.transform.localScale = Vector3.one;
			groupObj_3_UnlinkedObjects.transform.localScale = Vector3.one;


			//2. 기존 RootUnit을 Recycle로 옮긴다.
			//옮기면서 "Prev List"를 만들어야 한다. Recycle을 하기 위함
			List<apOptRootUnit> prevOptRootUnits = new List<apOptRootUnit>();
			if (targetOptPortrait._optRootUnitList != null)
			{
				for (int i = 0; i < targetOptPortrait._optRootUnitList.Count; i++)
				{
					apOptRootUnit optRootUnit = targetOptPortrait._optRootUnitList[i];
					if (optRootUnit != null)
					{
						optRootUnit.transform.parent = groupObj_1_ReadyToRecycle.transform;

						prevOptRootUnits.Add(optRootUnit);
					}
				}
			}


			//RootUnit 리스트를 초기화한다.
			if (targetOptPortrait._optRootUnitList == null)
			{
				targetOptPortrait._optRootUnitList = new List<apOptRootUnit>();
			}

			targetOptPortrait._optRootUnitList.Clear();
			targetOptPortrait._curPlayingOptRootUnit = null;

			if (targetOptPortrait._optTransforms == null) { targetOptPortrait._optTransforms = new List<apOptTransform>(); }
			if (targetOptPortrait._optMeshes == null) { targetOptPortrait._optMeshes = new List<apOptMesh>(); }
			if (targetOptPortrait._optTextureData == null) { targetOptPortrait._optTextureData = new List<apOptTextureData>(); }//<<텍스쳐 데이터 추가

			targetOptPortrait._optTransforms.Clear();
			targetOptPortrait._optMeshes.Clear();
			targetOptPortrait._optTextureData.Clear();

			//추가
			//Batched Matrial 관리 객체가 생겼다.
			if (targetOptPortrait._optBatchedMaterial == null)
			{
				targetOptPortrait._optBatchedMaterial = new apOptBatchedMaterial();
			}
			else
			{
				targetOptPortrait._optBatchedMaterial.Clear(true);//<<이미 생성되어 있다면 초기화
			}

			////추가 11.6 : LWRP Shader를 사용하는지 체크하고, 필요한 경우 생성해야한다.
			//CheckAndCreateLWRPShader();


			// srcPortrait로 부터 가져온 데이터는 앞에 src를 붙인다.

			//3. 텍스쳐 데이터를 먼저 만들자.
			// Src -> Target
			for (int i = 0; i < srcPortrait._textureData.Count; i++)
			{
				apTextureData srcTextureData = srcPortrait._textureData[i];
				apOptTextureData newOptTexData = new apOptTextureData();

				newOptTexData.Bake(i, srcTextureData);
				targetOptPortrait._optTextureData.Add(newOptTexData);
			}


			//추가 20.1.28 : Color Space가 동일하도록 묻고 변경
			CheckAndChangeTextureDataColorSpace(srcPortrait);

			//4. 추가 : Reset
			srcPortrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AllObjects(null)); // Source를 먼저 준비





			//4. OptTransform을 만들자 (RootUnit부터)
			// Src -> Taret
			for (int i = 0; i < srcPortrait._rootUnits.Count; i++)
			{
				apRootUnit srcRootUnit = srcPortrait._rootUnits[i];

				//추가 : 계층구조의 MeshGroup인 경우 이 코드가 추가되어야 한다.
				if (srcRootUnit._childMeshGroup != null)
				{
					srcRootUnit._childMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);//렌더 유닛의 Depth를 다시 계산해야한다. <<
					srcRootUnit._childMeshGroup.LinkModMeshRenderUnits(null);
					srcRootUnit._childMeshGroup.RefreshModifierLink(null);
				}

				//업데이트를 한번 해주자
				srcRootUnit.Update(0.0f, false, false);

				apOptRootUnit optRootUnit = null;

				//1. Root Unit
				//재활용 가능한지 판단한다.
				bool isRecycledRootUnit = false;
				apOptRootUnit recycledOptRootUnit = GetRecycledRootUnit(srcRootUnit, prevOptRootUnits);

				if (recycledOptRootUnit != null)
				{

					//재활용이 된다.
					optRootUnit = recycledOptRootUnit;

					//일부 값은 다시 리셋
					optRootUnit.name = "Root Portrait " + i;
					optRootUnit._portrait = targetOptPortrait;
					optRootUnit._transform = optRootUnit.transform;

					optRootUnit.transform.parent = targetOptPortrait.transform;
					optRootUnit.transform.localPosition = Vector3.zero;
					optRootUnit.transform.localRotation = Quaternion.identity;
					optRootUnit.transform.localScale = Vector3.one;

					//재활용에 성공했으니 OptUnit은 제외한다.
					prevOptRootUnits.Remove(recycledOptRootUnit);
					isRecycledRootUnit = true;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}
				else
				{
					//새로운 RootUnit이다.
					optRootUnit = AddGameObject<apOptRootUnit>("Root Portrait " + i, targetOptPortrait.transform);

					optRootUnit._portrait = targetOptPortrait;
					optRootUnit._rootOptTransform = null;
					optRootUnit._transform = optRootUnit.transform;

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}

				optRootUnit.ClearChildLinks();//Child Link를 초기화한다.

				//추가 12.6 : SortedRenderBuffer에 관련한 Bake 코드 <<
				optRootUnit.BakeSortedRenderBuffer(targetOptPortrait, srcRootUnit);

				targetOptPortrait._optRootUnitList.Add(optRootUnit);



				//재활용에 성공했다면
				//기존의 GameObject + Bake 여부를 재귀적 리스트로 작성한다.
				apBakeLinkManager bakeLinkManager = null;
				if (isRecycledRootUnit)
				{
					bakeLinkManager = new apBakeLinkManager();

					//파싱하자.
					bakeLinkManager.Parse(optRootUnit._rootOptTransform.gameObject, recycledOptRootUnit.gameObject);
				}

				apMeshGroup srcChildMainMeshGroup = srcRootUnit._childMeshGroup;

				//0. 추가
				//일부 Modified Mesh를 갱신해야한다.
				if (srcChildMainMeshGroup != null && srcRootUnit._childMeshGroupTransform != null)
				{
					//Refresh를 한번 해주자
					srcChildMainMeshGroup.RefreshForce();

					List<apModifierBase> srcModifiers = srcChildMainMeshGroup._modifierStack._modifiers;
					for (int iMod = 0; iMod < srcModifiers.Count; iMod++)
					{
						apModifierBase mod = srcModifiers[iMod];
						if (mod._paramSetGroup_controller != null)
						{
							for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
							{
								apModifierParamSetGroup psg = mod._paramSetGroup_controller[iPSG];
								for (int iPS = 0; iPS < psg._paramSetList.Count; iPS++)
								{
									apModifierParamSet ps = psg._paramSetList[iPS];
									ps.UpdateBeforeBake(srcPortrait, srcChildMainMeshGroup, srcRootUnit._childMeshGroupTransform);
								}
							}
						}
					}
				}

				//1. 1차 Bake : GameObject 만들기
				//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
				if (srcChildMainMeshGroup != null && srcRootUnit._childMeshGroupTransform != null)
				{
					//정렬 한번 해주고
					srcChildMainMeshGroup.SortRenderUnits(true, apMeshGroup.DEPTH_ASSIGN.OnlySort);

					apRenderUnit srcRootRenderUnit = srcChildMainMeshGroup._rootRenderUnit;
					//apRenderUnit rootRenderUnit = Editor._portrait._rootUnit._renderUnit;
					if (srcRootRenderUnit != null)
					{
						//apTransform_MeshGroup meshGroupTransform = Editor._portrait._rootUnit._childMeshGroupTransform;
						apTransform_MeshGroup srcMeshGroupTransform = srcRootRenderUnit._meshGroupTransform;

						if (srcMeshGroupTransform == null)
						{
							Debug.LogError("Bake Error : MeshGroupTransform Not Found [" + srcChildMainMeshGroup._name + "]");
						}
						else
						{
							MakeMeshGroupToOptTransform(srcRootRenderUnit,
															srcMeshGroupTransform,
															optRootUnit.transform,
															null,
															optRootUnit,
															bakeLinkManager,
															bakeResult,
															targetOptPortrait._bakeZSize,
															
															//Editor._isBakeColorSpaceToGamma,//<<감마 색상 공간으로 Bake할 것인가
															isBakeGammaColorSpace,//로컬 변수로 변경 v1.4.2
															
															//삭제
															//Editor._isUseSRP,//LWRP Shader를 사용할 것인가

															targetOptPortrait,
															srcChildMainMeshGroup,
															isSizeOptimizedV117,
															isUseSRP
															);
							//MakeMeshGroupToOptTransform(null, meshGroupTransform, Editor._portrait._optRootUnit.transform, null);
						}
					}
					else
					{
						Debug.LogError("Bake Error : RootMeshGroup Not Found [" + srcChildMainMeshGroup._name + "]");
					}
				}



				//optRootUnit.transform.localScale = Vector3.one * 0.01f;
				optRootUnit.transform.localScale = Vector3.one * targetOptPortrait._bakeScale;


				// 이전에 Bake 했던 정보에서 가져왔다면
				//만약 "재활용되지 않은 GameObject"를 찾아서 별도의 처리를 해야한다.
				if (isRecycledRootUnit && bakeLinkManager != null)
				{
					bakeLinkManager.SetHierarchyNotRecycledObjects(groupObj_1_ReadyToRecycle, groupObj_2_RemoveTargets, groupObj_3_UnlinkedObjects, bakeResult);

				}


				//추가 v1.4.8 : 루트 모션 설정을 입력하자
				optRootUnit._rootMotionBoneID = -1;
				if(srcChildMainMeshGroup != null)
				{
					//루트 모션용 본이 존재하는지 확인하자
					apBone rootMotionBone = srcChildMainMeshGroup.GetBone(srcChildMainMeshGroup._rootMotionBoneID);
					if(rootMotionBone != null)
					{
						//루트 모션 본이 존재한다면 ID를 할당한다.
						optRootUnit._rootMotionBoneID = srcChildMainMeshGroup._rootMotionBoneID;
					}
				}

				//추가 12.6 : Bake 함수 추가 <<
				optRootUnit.BakeComplete();


			}


			if (prevOptRootUnits.Count > 0)
			{
				//이 유닛들은 Remove Target으로 이동해야 한다.
				apOptRootUnit curPrevoptRootUnit = null;
				for (int i = 0; i < prevOptRootUnits.Count; i++)
				{
					curPrevoptRootUnit = prevOptRootUnits[i];//변경 1.4.5

					//[v1.4.5] 오류 검출
					if(curPrevoptRootUnit == null
						|| curPrevoptRootUnit.transform == null)
					{
						Debug.LogWarning("AnyPortrait : Bake warning. Since the previous root unit is null, some objects may not be created or deleted properly.");
						continue;
					}

					curPrevoptRootUnit.transform.parent = groupObj_2_RemoveTargets.transform;

					//[v1.4.5] 연결이 해제된 상태에서 Bake를 다시 실행할 때 Null 체크
					if (curPrevoptRootUnit._rootOptTransform == null)
					{	
						Debug.LogWarning("AnyPortrait : Bake warning. Some subobjects of the unused Root Unit have already been deleted, so moving them to the Unlinked group for preservation failed.");
						continue;
					}

					//만약 여기서 알수없는 GameObject나 Compnent에 대해서는 Remove가 아니라 Unlink로 옮겨야 한다.
					apBakeLinkManager prevBakeManager = new apBakeLinkManager();
					prevBakeManager.Parse(curPrevoptRootUnit._rootOptTransform.gameObject, null);

					prevBakeManager.SetHierarchyToUnlink(groupObj_3_UnlinkedObjects, bakeResult);

				}
			}


			//TODO: 이제 그룹을 삭제하던가 경고 다이얼로그를 띄워주던가 하자
			UnityEngine.Object.DestroyImmediate(groupObj_1_ReadyToRecycle);
			UnityEngine.Object.DestroyImmediate(groupObj_2_RemoveTargets);

			if (groupObj_3_UnlinkedObjects.transform.childCount == 0)
			{
				UnityEngine.Object.DestroyImmediate(groupObj_3_UnlinkedObjects);

				targetOptPortrait._bakeUnlinkedGroup = null;
			}


			for (int i = 0; i < targetOptPortrait._optMeshes.Count; i++)
			{
				apOptMesh optMesh = targetOptPortrait._optMeshes[i];
				if (optMesh._isMaskChild)
				{
					apOptTransform optTransform = targetOptPortrait.GetOptTransform(optMesh._clipParentID);
					apOptMesh parentMesh = null;
					if (optTransform != null && optTransform._childMesh != null)
					{
						parentMesh = optTransform._childMesh;
					}
					optMesh.LinkAsMaskChild(parentMesh);
				}
			}

			//2. 2차 Bake : Modifier 만들기
			List<apOptTransform> optTransforms = targetOptPortrait._optTransforms;
			for (int i = 0; i < optTransforms.Count; i++)
			{
				apOptTransform optTransform = optTransforms[i];

				apMeshGroup srcMeshGroup = srcPortrait.GetMeshGroup(optTransform._meshGroupUniqueID);
				optTransform.BakeModifier(targetOptPortrait, srcMeshGroup, isSizeOptimizedV117);
			}


			//3. 3차 Bake : ControlParam/KeyFrame ~~> Modifier <- [Calculated Param] -> OptTrasform + Mesh
			targetOptPortrait.SetFirstInitializeAfterBake();
			targetOptPortrait.Initialize();

			//추가 20.8.10 [Flipped Scale 문제]
			//3.1 : 리깅 본 정보를 Initialize 직후에 Bake한다.
			if (targetOptPortrait._flippedMeshOption == apPortrait.FLIPPED_MESH_CHECK.All)
			{
				for (int i = 0; i < optTransforms.Count; i++)
				{
					apOptTransform optTransform = optTransforms[i];

					//리깅이 된 optTransform은 연결된 본들을 입력해주자
					if (optTransform._childMesh != null && optTransform._isIgnoreParentModWorldMatrixByRigging)
					{
						SetRiggingOptBonesToOptTransform(optTransform);
					}
				}
			}



			//4. 첫번째 OptRoot만 보여주도록 하자
			if (targetOptPortrait._optRootUnitList.Count > 0)
			{
				targetOptPortrait.ShowRootUnitWhenBake(targetOptPortrait._optRootUnitList[0]);
			}


			//5. AnimClip의 데이터를 받아서 AnimPlay 데이터로 만들자
			if (targetOptPortrait._animPlayManager == null)
			{
				targetOptPortrait._animPlayManager = new apAnimPlayManager();
			}

			targetOptPortrait._animPlayManager.InitAndLink();
			targetOptPortrait._animPlayManager._animPlayDataList.Clear();

			for (int i = 0; i < targetOptPortrait._animClips.Count; i++)
			{
				apAnimClip animClip = targetOptPortrait._animClips[i];
				int animClipID = animClip._uniqueID;
				string animClipName = animClip._name;
				int targetMeshGroupID = animClip._targetMeshGroupID;

				apAnimPlayData animPlayData = new apAnimPlayData(animClipID, targetMeshGroupID, animClipName);
				targetOptPortrait._animPlayManager._animPlayDataList.Add(animPlayData);

			}


			//6. 한번 업데이트를 하자 (소켓들이 갱신된다)
			if (targetOptPortrait._optRootUnitList.Count > 0)
			{
				apOptRootUnit optRootUnit = null;
				for (int i = 0; i < targetOptPortrait._optRootUnitList.Count; i++)
				{
					//이전
					//taretOptPortrait._optRootUnitList[i].RemoveAllCalculateResultParams();

					//변경
					optRootUnit = targetOptPortrait._optRootUnitList[i];
					if (optRootUnit._rootOptTransform != null)
					{
						optRootUnit._rootOptTransform.ClearResultParams(true);
						optRootUnit._rootOptTransform.ResetCalculateStackForBake(true);
					}
					else
					{
						Debug.LogError("AnyPortrait : No Root Opt Transform on RootUnit (OptBake)");
					}
				}

				//추가 3.22 : Bake후 메시가 변경되었을 경우에 다시 리셋할 필요가 있다.
				//for (int i = 0; i < taretOptPortrait._optRootUnitList.Count; i++)
				//{
				//	taretOptPortrait._optRootUnitList[i].ResetCalculateStackForBake();
				//}

				for (int i = 0; i < targetOptPortrait._optRootUnitList.Count; i++)
				{
					targetOptPortrait._optRootUnitList[i].UpdateTransforms(0.0f, true, null);
				}
			}
			//taretOptPortrait.ResetMeshesCommandBuffers(false);

			//taretOptPortrait.UpdateForce();

			// 원래는 "사용하지 않는 Mesh, MeshGroup 등을 삭제하는 코드"가 있는데,
			// Opt에서는 필요가 없다.
			//추가 3.22 
			//6-2. LayerOrder 갱신하자
			string sortingLayerName = "";
			bool isValidSortingLayer = false;
			if (SortingLayer.IsValid(Editor._portrait._sortingLayerID))
			{
				sortingLayerName = SortingLayer.IDToName(Editor._portrait._sortingLayerID);
				isValidSortingLayer = true;
			}
			else
			{
				if (SortingLayer.layers.Length > 0)
				{
					sortingLayerName = SortingLayer.layers[0].name;
					isValidSortingLayer = true;
				}
				else
				{
					isValidSortingLayer = false;
				}
			}
			if (isValidSortingLayer)
			{
				targetOptPortrait.SetSortingLayer(sortingLayerName);
			}
			//변경 19.8.19 : 옵션이 적용되는 경우에 한해서
			if (Editor._portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
			{
				targetOptPortrait.SetSortingOrder(Editor._portrait._sortingOrder);
			}


			//추가 19.5.26
			//6-3. 최적화 옵션으로 Bake 되었는지 체크
			targetOptPortrait._isSizeOptimizedV117 = isSizeOptimizedV117;



			//추가3.22
			//Portrait가 Prefab이라면
			//Bake와 동시에 Apply를 해야한다.
			//if(apEditorUtil.IsPrefab(taretOptPortrait.gameObject))
			//{
			//	apEditorUtil.ApplyPrefab(taretOptPortrait.gameObject);
			//}

			//추가 4.26
			//메카님 옵션이 켜져 있다면
			//1. Animation Clip들을 리소스로 생성한다.
			//2. Animator 컴포넌트를 추가한다.
			//TODO : > Optimized에서도
			if (targetOptPortrait._isUsingMecanim)
			{
				//추가 3.22 : animClip 경로가 절대 경로인 경우, 여러 작업자가 공유해서 쓸 수 없다.
				//상대 경로로 바꾸는 작업을 해야한다.
				CheckAnimationsBasePathForV116(targetOptPortrait);

				CreateAnimationsWithMecanim(targetOptPortrait, targetOptPortrait._mecanimAnimClipResourcePath);
				targetOptPortrait.SetFirstInitializeAfterBake();
				targetOptPortrait.Initialize();
			}


			//추가 21.9.25 : 유니티 이벤트 (UnityEvent)를 사용한다면 Bake를 하자
			if(targetOptPortrait._unityEventWrapper == null)
			{
				targetOptPortrait._unityEventWrapper = new apUnityEventWrapper();
			}
			targetOptPortrait._unityEventWrapper.Bake(targetOptPortrait);

			EditorUtility.SetDirty(targetOptPortrait);


			//추가. Bake 후 처리
			ProcessAfterBake();

			//추가 19.10.26 : 빌보드 설정을 다시 복구
			targetOptPortrait._billboardType = billboardType;

			//추가 21.3.11
			// Scale 이슈가 있어서 저장된 값의 Scale로 복원
			foreach (KeyValuePair<Transform, Vector3> transform2Scale in prevTransformScales)
			{
				if(transform2Scale.Key != null)
				{
					transform2Scale.Key.localScale = transform2Scale.Value;
				}
			}

			//버그 수정 : 첫번째 루트 유닛만 보여야 하는데 그렇지 않은 경우 문제 해결
			//추가 22.1.9 : 루트 유닛이 여러개 있는 경우엔 첫번째 루트 유닛을 출력하자
			int nOptRootUnits = targetOptPortrait._optRootUnitList != null ? targetOptPortrait._optRootUnitList.Count : 0;
			if (nOptRootUnits > 1)
			{
				targetOptPortrait.ShowRootUnitWhenBake(targetOptPortrait._optRootUnitList[0]);
			}



			//Bake 후에는 Initialize를 하지 않은 상태로 되돌린다. (v1.4.3)
			targetOptPortrait.SetFirstInitializeAfterBake();

			return bakeResult;
		}


		/// <summary>
		/// Bake / OptimizedBake 이후에 호출해야하는 함수.
		/// 현재 편집되는 것에 따라서 Link를 다시 해야한다.
		/// </summary>
		private void ProcessAfterBake()
		{
			apPortrait portrait = Editor.Select.Portrait;
			if (portrait == null)
			{
				return;
			}
			apMeshGroup meshGroup = null;
			switch (Editor.Select.SelectionType)
			{
				case apSelection.SELECTION_TYPE.Overall:
					if (Editor.Select.RootUnit != null)
					{
						meshGroup = Editor.Select.RootUnit._childMeshGroup;
					}
					break;


				case apSelection.SELECTION_TYPE.MeshGroup:
					meshGroup = Editor.Select.MeshGroup;
					break;

				case apSelection.SELECTION_TYPE.Animation:
					if (Editor.Select.AnimClip != null)
					{
						meshGroup = Editor.Select.AnimClip._targetMeshGroup;
					}

					break;
			}
			if (meshGroup != null)
			{
				//현재 작업 중인 MeshGroup을 찾아서 Link를 다시 한다.
				portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(meshGroup));
			}

		}

		// 추가 19.6.3 : MaterialSet에 관련된 함수들
		//-------------------------------------------------------------------------------
		public void LinkMaterialSets(apPortrait portrait)
		{
			if(portrait == null || _editor == null)
			{
				return;
			}

			//apPortrait portrait = _editor._portrait;
			if(portrait._materialSets == null)
			{
				portrait._materialSets = new List<apMaterialSet>();
			}
			List<apMaterialSet> matSets = portrait._materialSets;

			apMaterialLibrary matLibrary = _editor.MaterialLibrary;

			//만약 Preset이 없다면 Load.
			if(matLibrary.Presets.Count == 0)
			{
				matLibrary.Load();
			}

			//Debug.Log("Material Set 로드");

			//1. 만약 MatSet이 아무것도 없다면, 기본값인 Unlit을 연결해줘야 한다.
			if(matSets.Count == 0)
			{
				//이전
				//AddMaterialSet(matLibrary.Presets[0], true, true, false);

				//변경 v1.4.7 : 기존과 다른 Unlit v2를 넣자.
				AddMaterialSet(matLibrary.GetDefaultPreset(), true, true, false);

			}


			//2. Shader, Texture Asset들을 연결하자 (Portrait의 데이터와 Library 포함)
			for (int iMat = 0; iMat < portrait._materialSets.Count; iMat++)
			{
				LinkMaterialSetAssets(portrait._materialSets[iMat], false, portrait);
			}

			for (int iMat = 0; iMat < matLibrary.Presets.Count; iMat++)
			{
				LinkMaterialSetAssets(matLibrary.Presets[iMat], true, portrait);
			}

			//3. Default가 1개여야 한다. 되도록 설정하자.
			int nDefault = 0;
			for (int i = 0; i < matSets.Count; i++)
			{
				apMaterialSet matSet = matSets[i];
				if(matSet._isDefault)
				{
					nDefault++;
				}
			}

			if(nDefault == 0)
			{
				//Default가 없다면?
				//첫번째 항목을 Default로 설정
				matSets[0]._isDefault = true;
			}
			else if(nDefault > 1)
			{
				//Default가 1개를 넘었다면?
				//맨 앞의 Default 외의 Default는 해제
				bool isDefaultFound = false;
				for (int i = 0; i < matSets.Count; i++)
				{
					apMaterialSet matSet = matSets[i];
					if (matSet._isDefault)
					{
						if (!isDefaultFound)
						{
							//유지
							isDefaultFound = true;
						}
						else
						{
							//Default 해제
							matSet._isDefault = false;
						}
					}
					
				}
			}

		}
		


		public void LinkMaterialSetAssets(apMaterialSet matSet, bool isPreset, apPortrait portrait)
		{
			//1. Shader 연결
			MaterialSetLoadResult result_Shader_Normal_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_AlphaBlend,		matSet._shaderPath_Normal_AlphaBlend);
			MaterialSetLoadResult result_Shader_Normal_Additive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_Additive,			matSet._shaderPath_Normal_Additive);
			MaterialSetLoadResult result_Shader_Normal_SoftAdditive =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_SoftAdditive,		matSet._shaderPath_Normal_SoftAdditive);
			MaterialSetLoadResult result_Shader_Normal_Multiplicative = LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_Multiplicative,	matSet._shaderPath_Normal_Multiplicative);

			MaterialSetLoadResult result_Shader_Clipped_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_AlphaBlend,		matSet._shaderPath_Clipped_AlphaBlend);
			MaterialSetLoadResult result_Shader_Clipped_Additive =			LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_Additive,		matSet._shaderPath_Clipped_Additive);
			MaterialSetLoadResult result_Shader_Clipped_SoftAdditive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_SoftAdditive,	matSet._shaderPath_Clipped_SoftAdditive);
			MaterialSetLoadResult result_Shader_Clipped_Multiplicative =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_Multiplicative,	matSet._shaderPath_Clipped_Multiplicative);

			MaterialSetLoadResult result_Shader_L_Normal_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_AlphaBlend,		matSet._shaderPath_L_Normal_AlphaBlend);
			MaterialSetLoadResult result_Shader_L_Normal_Additive =			LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_Additive,		matSet._shaderPath_L_Normal_Additive);
			MaterialSetLoadResult result_Shader_L_Normal_SoftAdditive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_SoftAdditive,	matSet._shaderPath_L_Normal_SoftAdditive);
			MaterialSetLoadResult result_Shader_L_Normal_Multiplicative =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_Multiplicative,	matSet._shaderPath_L_Normal_Multiplicative);

			MaterialSetLoadResult result_Shader_L_Clipped_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_AlphaBlend,		matSet._shaderPath_L_Clipped_AlphaBlend);
			MaterialSetLoadResult result_Shader_L_Clipped_Additive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_Additive,			matSet._shaderPath_L_Clipped_Additive);
			MaterialSetLoadResult result_Shader_L_Clipped_SoftAdditive =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_SoftAdditive,		matSet._shaderPath_L_Clipped_SoftAdditive);
			MaterialSetLoadResult result_Shader_L_Clipped_Multiplicative =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_Multiplicative,	matSet._shaderPath_L_Clipped_Multiplicative);

			MaterialSetLoadResult result_Shader_AlphaMask =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_AlphaMask,	matSet._shaderPath_AlphaMask);


			//Normal
			matSet._shader_Normal_AlphaBlend =		result_Shader_Normal_AlphaBlend.asset as Shader;
			matSet._shaderPath_Normal_AlphaBlend =	result_Shader_Normal_AlphaBlend.path;

			matSet._shader_Normal_Additive =		result_Shader_Normal_Additive.asset as Shader;
			matSet._shaderPath_Normal_Additive =	result_Shader_Normal_Additive.path;

			matSet._shader_Normal_SoftAdditive =		result_Shader_Normal_SoftAdditive.asset as Shader;
			matSet._shaderPath_Normal_SoftAdditive =	result_Shader_Normal_SoftAdditive.path;

			matSet._shader_Normal_Multiplicative =		result_Shader_Normal_Multiplicative.asset as Shader;
			matSet._shaderPath_Normal_Multiplicative =	result_Shader_Normal_Multiplicative.path;

			//Clipped
			matSet._shader_Clipped_AlphaBlend =		result_Shader_Clipped_AlphaBlend.asset as Shader;
			matSet._shaderPath_Clipped_AlphaBlend =	result_Shader_Clipped_AlphaBlend.path;

			matSet._shader_Clipped_Additive =		result_Shader_Clipped_Additive.asset as Shader;
			matSet._shaderPath_Clipped_Additive =	result_Shader_Clipped_Additive.path;

			matSet._shader_Clipped_SoftAdditive =		result_Shader_Clipped_SoftAdditive.asset as Shader;
			matSet._shaderPath_Clipped_SoftAdditive =	result_Shader_Clipped_SoftAdditive.path;

			matSet._shader_Clipped_Multiplicative =		result_Shader_Clipped_Multiplicative.asset as Shader;
			matSet._shaderPath_Clipped_Multiplicative =	result_Shader_Clipped_Multiplicative.path;

			//Normal (Linear)
			matSet._shader_L_Normal_AlphaBlend =		result_Shader_L_Normal_AlphaBlend.asset as Shader;
			matSet._shaderPath_L_Normal_AlphaBlend =	result_Shader_L_Normal_AlphaBlend.path;

			matSet._shader_L_Normal_Additive =		result_Shader_L_Normal_Additive.asset as Shader;
			matSet._shaderPath_L_Normal_Additive =	result_Shader_L_Normal_Additive.path;

			matSet._shader_L_Normal_SoftAdditive =		result_Shader_L_Normal_SoftAdditive.asset as Shader;
			matSet._shaderPath_L_Normal_SoftAdditive =	result_Shader_L_Normal_SoftAdditive.path;

			matSet._shader_L_Normal_Multiplicative =		result_Shader_L_Normal_Multiplicative.asset as Shader;
			matSet._shaderPath_L_Normal_Multiplicative =	result_Shader_L_Normal_Multiplicative.path;

			//Clipped (Linear)
			matSet._shader_L_Clipped_AlphaBlend =		result_Shader_L_Clipped_AlphaBlend.asset as Shader;
			matSet._shaderPath_L_Clipped_AlphaBlend =	result_Shader_L_Clipped_AlphaBlend.path;

			matSet._shader_L_Clipped_Additive =		result_Shader_L_Clipped_Additive.asset as Shader;
			matSet._shaderPath_L_Clipped_Additive =	result_Shader_L_Clipped_Additive.path;

			matSet._shader_L_Clipped_SoftAdditive =		result_Shader_L_Clipped_SoftAdditive.asset as Shader;
			matSet._shaderPath_L_Clipped_SoftAdditive =	result_Shader_L_Clipped_SoftAdditive.path;

			matSet._shader_L_Clipped_Multiplicative =		result_Shader_L_Clipped_Multiplicative.asset as Shader;
			matSet._shaderPath_L_Clipped_Multiplicative =	result_Shader_L_Clipped_Multiplicative.path;

			//Alpha Mask
			matSet._shader_AlphaMask =		result_Shader_AlphaMask.asset as Shader;
			matSet._shaderPath_AlphaMask =	result_Shader_AlphaMask.path;



			if(!isPreset)
			{
				matSet._linkedPresetMaterial = _editor.MaterialLibrary.GetPresetUnit(matSet._linkedPresetID);
				if(matSet._linkedPresetMaterial == null)
				{
					matSet._linkedPresetID = -1;
				}
			}
			else
			{
				matSet._linkedPresetMaterial = null;
				matSet._linkedPresetID = -1;
			}
			
			//추가 v1.5.1 : 레퍼 재질
			MaterialSetLoadResult result_RefMaterial =	LoadAssetOfMaterialSet<Material>(	matSet._referenceMat,	matSet._referenceMaterialPath);
			matSet._referenceMat = result_RefMaterial.asset as Material;
			matSet._referenceMaterialPath = result_RefMaterial.path;

			//2. 텍스쳐 연결
			//연결될 텍스쳐 데이터들
			List<apTextureData> srcImages = portrait._textureData;

			apMaterialSet.PropertySet propSet = null;
			for (int iProp = 0; iProp < matSet._propertySets.Count; iProp++)
			{
				propSet = matSet._propertySets[iProp];

				if (propSet._propType != apMaterialSet.SHADER_PROP_TYPE.Texture)
				{
					//텍스쳐 타입이 아니라면, 데이터를 삭제하자.
					propSet._value_CommonTexture = null;
					propSet._commonTexturePath = "";
					propSet._imageTexturePairs.Clear();
				}
				else
				{
					//Common Texture를 사용하는 경우
					MaterialSetLoadResult commonTexResult = LoadAssetOfMaterialSet<Texture>(propSet._value_CommonTexture, propSet._commonTexturePath);

					propSet._value_CommonTexture = commonTexResult.asset as Texture;
					propSet._commonTexturePath = commonTexResult.path;

					
					//텍스쳐 <-> 이미지 (TextureData) 연결
					//- 연결은 자동. 
					//1) 일단 기존 데이터에서 연결을 하자
					//적당한 Image가 없다면 > 연결 해제
					apMaterialSet.PropertySet.ImageTexturePair imgTexPair = null;
					for (int iPair = 0; iPair < propSet._imageTexturePairs.Count; iPair++)
					{
						imgTexPair = propSet._imageTexturePairs[iPair];

						//일단 연결
						imgTexPair._targetTextureData = portrait.GetTexture(imgTexPair._textureDataID);
					}

					propSet._imageTexturePairs.RemoveAll(delegate (apMaterialSet.PropertySet.ImageTexturePair a)
					{
						return a._targetTextureData == null;//<<Link에 실패한 경우
						});

					//2) TextureData에서 존재하지 않는 데이터가 있다면 자동으로 추가
					for (int iSrc = 0; iSrc < srcImages.Count; iSrc++)
					{
						apTextureData srcImage = srcImages[iSrc];

						if (!propSet._imageTexturePairs.Exists(delegate (apMaterialSet.PropertySet.ImageTexturePair a)
						 {
							 return a._targetTextureData == srcImage;
						 }))
						{
							//아직 등록되지 않은 TextureData이다.
							apMaterialSet.PropertySet.ImageTexturePair newPair = new apMaterialSet.PropertySet.ImageTexturePair();
							newPair._textureDataID = srcImage._uniqueID;
							newPair._targetTextureData = srcImage;
							newPair._textureAsset = null;
							newPair._textureAssetPath = "";

							//Pair 리스트에 추가하자.
							propSet._imageTexturePairs.Add(newPair);
						}
					}



					for (int iPair = 0; iPair < propSet._imageTexturePairs.Count; iPair++)
					{
						imgTexPair = propSet._imageTexturePairs[iPair];

						//이제 텍스쳐 에셋과 연결
						MaterialSetLoadResult textureLoadResult = LoadAssetOfMaterialSet<Texture>(imgTexPair._textureAsset, imgTexPair._textureAssetPath);

						imgTexPair._textureAsset = textureLoadResult.asset as Texture;
						imgTexPair._textureAssetPath = textureLoadResult.path;
					}

					//끝으로, 이름순으로 정렬
					propSet._imageTexturePairs.Sort(
					delegate (apMaterialSet.PropertySet.ImageTexturePair a, apMaterialSet.PropertySet.ImageTexturePair b)
					{
						return string.Compare(a._targetTextureData._name, b._targetTextureData._name);
					});
				}
				
			}
		}


		private MaterialSetLoadResult LoadAssetOfMaterialSet<T>(T asset, string assetPath) where T : UnityEngine.Object
		{
			 MaterialSetLoadResult result = new MaterialSetLoadResult();

			if(asset != null)
			{
				//Shader가 이미 있다면 > Path를 변경하자
				string pathOfAsset = AssetDatabase.GetAssetPath(asset);
				result.asset = asset;
				result.path = pathOfAsset;
			}
			else
			{
				//Shader가 없다면 > Path에서 에셋을 열자
				T assetFromPath = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if(assetFromPath != null)
				{
					result.asset = assetFromPath;
					result.path = assetPath;
				}
				else
				{
					//로드 실패
					result.asset = null;
					result.path = "";
				}
			}

			return result;
			
		}

		private struct MaterialSetLoadResult
		{
			public UnityEngine.Object asset;
			public string path;
		}
		

		



		/// <summary>
		/// Src가 되는 MaterialSet으로 부터 새로운 MaterialSet을 생성한다.
		/// Preset이라면 연동이 되어야 하며, 그렇지 않다면 값만 복사된다.
		/// Src는 꼭 넣어주자.
		/// </summary>
		/// <param name="srcPreset"></param>
		/// <returns></returns>
		public apMaterialSet AddMaterialSet(apMaterialSet srcMaterialSet, bool isFromPreset, bool isDefault, bool linkMaterialSetAfterAdd = true)
		{
			if(_editor == null || _editor._portrait == null)
			{
				return null;
			}
			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetAdded, 
												_editor, 
												_editor._portrait, 
												//_editor._portrait, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apPortrait portrait = _editor._portrait;
			if(portrait._materialSets == null)
			{
				portrait._materialSets = new List<apMaterialSet>();
			}
			//List<apMaterialSet> matSets = portrait._materialSets;
			apMaterialLibrary matLibrary = _editor.MaterialLibrary;

			//새로운 ID를 만들자
			int newUniqueID = -1;
			int cnt = 0;
			while(true)
			{
				int nextID = UnityEngine.Random.Range(2000, 99999);
				apMaterialSet existMatSet = portrait.GetMaterialSet(nextID);
				if(existMatSet == null)
				{
					//이 값은 사용할 수 있겠다.
					newUniqueID = nextID;
					break;
				}

				cnt++;
				if(cnt > 100)
				{
					//100번이 넘도록 실패했다면?
					break;
				}
			}
			if(newUniqueID < 0)
			{
				//ID 할당에 
				//1000~1999 사이의 값을 할당하자.
				for (int iNextID = 1000; iNextID < 2000; iNextID++)
				{
					if(portrait.GetMaterialSet(iNextID) == null)
					{
						newUniqueID = iNextID;
						break;
					}
				}
			}

			if(newUniqueID < 0)
			{
				Debug.LogError("AnyPortrait : Failed to create new Material Set. please try again.");
				return null;
			}

			apMaterialSet newMatSet = new apMaterialSet();
			newMatSet.Init();
			if(srcMaterialSet != null)
			{
				newMatSet.CopyFromSrc(srcMaterialSet, newUniqueID, isFromPreset, false, false);
			}
			else
			{
				newMatSet._uniqueID = newUniqueID;
				newMatSet._name = "<No Name>";

				//새로운 MatSet에 기본 프로퍼티는 넣어야지
				newMatSet.AddProperty("_Color", true, apMaterialSet.SHADER_PROP_TYPE.Color);
				newMatSet.AddProperty("_MainTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				newMatSet.AddProperty("_MaskTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				newMatSet.AddProperty("_MaskScreenSpaceOffset", true, apMaterialSet.SHADER_PROP_TYPE.Vector);
			}
			
			newMatSet._isDefault = isDefault;

			portrait._materialSets.Add(newMatSet);


			//만약, isDefault = true였다면, 다른 MaterialSet들 중에 Default가 있는걸 없애야함
			if(isDefault)
			{
				apMaterialSet curMatSet = null;
				for (int i = 0; i < portrait._materialSets.Count; i++)
				{
					curMatSet = portrait._materialSets[i];
					if(curMatSet._isDefault && curMatSet != newMatSet)
					{
						curMatSet._isDefault = false;//<<Default 속성을 해제한다.
					}
				}
			}

			if (linkMaterialSetAfterAdd)
			{
				//연결
				LinkMaterialSets(portrait);
			}

			return newMatSet;
		}


		/// <summary>
		/// Portrait의 MaterialSet을 연결된 Preset의 값으로 복구한다.
		/// Texture Per Image 속성의 Texture 프로퍼티는 값을 유지한다.
		/// </summary>
		/// <param name="matSet"></param>
		public bool RestoreMaterialSetToPreset(apMaterialSet matSet)
		{
			if(matSet == null || matSet._linkedPresetMaterial == null || !_editor._portrait._materialSets.Contains(matSet))
			{
				return false;
			}

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
												_editor, 
												_editor._portrait, 
												//_editor._portrait, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apMaterialSet preset = matSet._linkedPresetMaterial;

			//값을 복사하자.
			matSet._icon = preset._icon;

			matSet._shaderPath_Normal_AlphaBlend =			preset._shaderPath_Normal_AlphaBlend;
			matSet._shaderPath_Normal_Additive =			preset._shaderPath_Normal_Additive;
			matSet._shaderPath_Normal_SoftAdditive =		preset._shaderPath_Normal_SoftAdditive;
			matSet._shaderPath_Normal_Multiplicative =		preset._shaderPath_Normal_Multiplicative;
			matSet._shaderPath_Clipped_AlphaBlend =			preset._shaderPath_Clipped_AlphaBlend;
			matSet._shaderPath_Clipped_Additive =			preset._shaderPath_Clipped_Additive;
			matSet._shaderPath_Clipped_SoftAdditive =		preset._shaderPath_Clipped_SoftAdditive;
			matSet._shaderPath_Clipped_Multiplicative =		preset._shaderPath_Clipped_Multiplicative;
			matSet._shaderPath_L_Normal_AlphaBlend =		preset._shaderPath_L_Normal_AlphaBlend;
			matSet._shaderPath_L_Normal_Additive =			preset._shaderPath_L_Normal_Additive;
			matSet._shaderPath_L_Normal_SoftAdditive =		preset._shaderPath_L_Normal_SoftAdditive;
			matSet._shaderPath_L_Normal_Multiplicative =	preset._shaderPath_L_Normal_Multiplicative;
			matSet._shaderPath_L_Clipped_AlphaBlend =		preset._shaderPath_L_Clipped_AlphaBlend;
			matSet._shaderPath_L_Clipped_Additive =			preset._shaderPath_L_Clipped_Additive;
			matSet._shaderPath_L_Clipped_SoftAdditive =		preset._shaderPath_L_Clipped_SoftAdditive;
			matSet._shaderPath_L_Clipped_Multiplicative =	preset._shaderPath_L_Clipped_Multiplicative;
			matSet._shaderPath_AlphaMask =					preset._shaderPath_AlphaMask;

			matSet._shader_Normal_AlphaBlend =			preset._shader_Normal_AlphaBlend;
			matSet._shader_Normal_Additive =			preset._shader_Normal_Additive;
			matSet._shader_Normal_SoftAdditive =		preset._shader_Normal_SoftAdditive;
			matSet._shader_Normal_Multiplicative =		preset._shader_Normal_Multiplicative;
			matSet._shader_Clipped_AlphaBlend =			preset._shader_Clipped_AlphaBlend;
			matSet._shader_Clipped_Additive =			preset._shader_Clipped_Additive;
			matSet._shader_Clipped_SoftAdditive =		preset._shader_Clipped_SoftAdditive;
			matSet._shader_Clipped_Multiplicative =		preset._shader_Clipped_Multiplicative;
			matSet._shader_L_Normal_AlphaBlend =		preset._shader_L_Normal_AlphaBlend;
			matSet._shader_L_Normal_Additive =			preset._shader_L_Normal_Additive;
			matSet._shader_L_Normal_SoftAdditive =		preset._shader_L_Normal_SoftAdditive;
			matSet._shader_L_Normal_Multiplicative =	preset._shader_L_Normal_Multiplicative;
			matSet._shader_L_Clipped_AlphaBlend =		preset._shader_L_Clipped_AlphaBlend;
			matSet._shader_L_Clipped_Additive =			preset._shader_L_Clipped_Additive;
			matSet._shader_L_Clipped_SoftAdditive =		preset._shader_L_Clipped_SoftAdditive;
			matSet._shader_L_Clipped_Multiplicative =	preset._shader_L_Clipped_Multiplicative;
			matSet._shader_AlphaMask =					preset._shader_AlphaMask;


			matSet._isNeedToSetBlackColoredAmbient = preset._isNeedToSetBlackColoredAmbient;

			//프로퍼티 복사
			//- 일단 기존 프로퍼티를 복사
			//- 프로퍼티를 복사하면서 Texture인 경우에만 보존
			List<apMaterialSet.PropertySet> prevPropertySets = matSet._propertySets;

			matSet._propertySets = new List<apMaterialSet.PropertySet>();//<<새롭게 만들자


			apMaterialSet.PropertySet presetPropSet = null;
			apMaterialSet.PropertySet dstPropSet = null;
			int nPresetProps = preset._propertySets != null ? preset._propertySets.Count : 0;

			for (int i = 0; i < nPresetProps; i++)
			{
				presetPropSet = preset._propertySets[i];
				dstPropSet = null;

				if(presetPropSet._propType == apMaterialSet.SHADER_PROP_TYPE.Texture
					&& !presetPropSet._isReserved)
				{
					//Reserved가 아닌 Texture타입의 PropSet이라면
					//기존값이 있는지 확인하자.
					apMaterialSet.PropertySet existPropSet = prevPropertySets.Find(delegate(apMaterialSet.PropertySet a)
					{
						return string.Equals(a._name, presetPropSet._name) 
						&& !a._isReserved 
						&& a._propType == apMaterialSet.SHADER_PROP_TYPE.Texture
						&& !a._isCommonTexture;//<<Common Texture가 아닌 경우에 한해서
					});

					if(existPropSet != null)
					{
						//대체 가능한 PropSet이 있다면
						dstPropSet = existPropSet;
					}
				}

				if(dstPropSet == null)
				{
					//새로 복사해서 만들어야 하는 경우
					dstPropSet = new apMaterialSet.PropertySet();
					dstPropSet.CopyFromSrc(presetPropSet);
				}

				//이전
				//matSet._propertySets.Add(dstPropSet);

				//변경 v1.5.1 : 중복 체크 후 추가
				matSet.AddProperty(dstPropSet);
			}
			
			LinkMaterialSetAssets(matSet, false, _editor._portrait);


			return true;

		}


		public apMaterialSet DuplicateMaterialSet(apMaterialSet srcMatSet)
		{
			if(_editor == null || _editor._portrait == null || srcMatSet == null)
			{
				//처리 불가
				return null;
			}

			bool isPreset = false;

			if(_editor._portrait._materialSets.Contains(srcMatSet))
			{
				//Portrait의 데이터이다.
				isPreset = false;
			}
			else if(_editor.MaterialLibrary.Presets.Contains(srcMatSet))
			{
				//프리셋이다.
				isPreset = true;
			}
			else
			{
				//???? 알수 없는 데이터
				return null;
			}

			apMaterialSet newMatSet = null;
			if(isPreset)
			{
				//프리셋이라면
				newMatSet = _editor.MaterialLibrary.AddNewPreset(srcMatSet, true, srcMatSet._name + " Copy");
			}
			else
			{
				

				//프리셋이 아니라면
				newMatSet = AddMaterialSet(srcMatSet, false, false);
				if(newMatSet == null)
				{
					return null;
				}
				newMatSet._name = newMatSet._name + " Copy";
			}

			return newMatSet;
		}


		public void RemoveMaterialSet(apMaterialSet matSet)
		{
			if(_editor == null || _editor._portrait == null || matSet == null)
			{
				//처리 불가
				return;
			}

			bool isPreset = false;

			if(_editor._portrait._materialSets.Contains(matSet))
			{
				//Portrait의 데이터이다.
				isPreset = false;
			}
			else if(_editor.MaterialLibrary.Presets.Contains(matSet))
			{
				//프리셋이다.
				isPreset = true;
			}
			else
			{
				//???? 알수 없는 데이터
				return;
			}

			if(isPreset)
			{
				//삭제 가능한지 체크한다.
				apMaterialLibrary.PRESET_TYPE presetType = _editor.MaterialLibrary.GetPresetType(matSet);

				if(presetType == apMaterialLibrary.PRESET_TYPE.Reserved_NotRemovable)
				{
					//삭제 불가능한 타입이다.
					return;
				}

				_editor.MaterialLibrary.Presets.Remove(matSet);
				_editor.MaterialLibrary.Save();
			}
			else
			{
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetRemoved, 
													_editor, 
													_editor._portrait, 
													//_editor._portrait, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_editor._portrait._materialSets.Remove(matSet);
			}
		}
		//-------------------------------------------------------------------------------------------
		

		//-------------------------------------------------------------------------------------------
		
	}
}