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
		//------------------------------------------------------------
		// 입력 관련 함수
		//------------------------------------------------------------
		public void CheckInputEvent()
		{
			apMouseSet mouse = _editor.Mouse;
			mouse.InitMetaData();//추가 20.3.31

			Event currentEvent = Event.current;

			if(currentEvent == null)
			{
				return;
			}

			if (currentEvent.rawType == EventType.Used)
			{	
				return;
			}

			//주의 : _mouseBtn => _mouseSet (Mouse)로 변경되었다.
			//모든 변경 점에 주석을 단다.

			//Event.current.rawType으로 해야 Editor 외부에서의 MouseUp 이벤트를 가져올 수 있다.

			bool isMouseEvent = false;

			switch (currentEvent.rawType)
			{
				case EventType.ScrollWheel:
				case EventType.MouseDown:
				case EventType.MouseDrag:
				case EventType.MouseMove:
				case EventType.MouseUp:
					isMouseEvent = true;
					break;
			}

			Vector2 mousePos = Vector2.zero;

			//처리 순서
			//- 위치를 먼저 넣어주고
			//- 이벤트를 받는데 > 당장 이 프레임에 마우스 이벤트가 없을 수 있다.
			

			if (isMouseEvent || currentEvent.type == EventType.Repaint)
			{
				mousePos = currentEvent.mousePosition - new Vector2(_editor._mainGUIRect.x, _editor._mainGUIRect.y);
				mouse.SetMousePos(mousePos, currentEvent.mousePosition);
			}

			if (isMouseEvent)
			{
				mouse.ReadyToUpdate((int)_editor.position.width, (int)_editor.position.height);//>>변경

				if (currentEvent.rawType == EventType.ScrollWheel)
				{
					Vector2 deltaValue = currentEvent.delta;
					mouse.Update_Wheel((int)(deltaValue.y * 10.0f));
				}
				else//if (Event.current.isMouse)
				{
					int iMouse = -1;
					switch (currentEvent.button)
					{
						case 0://Left
							iMouse = 0;
							break;

						case 1://Right
							iMouse = 1;
							break;

						case 2://Middle
							iMouse = 2;
							break;
					}
					if (iMouse >= 0)
					{
						//>>변경
						bool isMouseInGUI = false;
						if (currentEvent.rawType == EventType.MouseDown)
						{
							//Down 이벤트일때만 영역 체크
							isMouseInGUI = IsMouseInGUI(mousePos);
						}
						mouse.Update_Button(currentEvent.rawType, iMouse, isMouseInGUI);
					}
				}
			}
			else if(currentEvent.type == EventType.Repaint)
			{
				//추가 20.3.31 : 마우스 이벤트가 아니면 Up->Released로 넘어가는 처리가 실행되지 않는다.
				//(두번의 Up이벤트는 없을 것이므로)
				//따라서 UpEvent에 한해서 Released로 전환하는 처리를 추가한다.
				mouse.Update_NoEvent();
			}

			if (currentEvent.rawType == EventType.KeyDown)
			{
#if UNITY_EDITOR_OSX
				_editor.OnHotKeyDown(currentEvent.keyCode, currentEvent.command, currentEvent.alt, currentEvent.shift);
#else
				_editor.OnHotKeyDown(currentEvent.keyCode, currentEvent.control, currentEvent.alt, currentEvent.shift);
#endif
			}
			else if (currentEvent.rawType == EventType.KeyUp)
			{	
				_editor.OnHotKeyUp();
			}
		}


		//추가 20.4.6 : 로딩 팝업이 있을 때는 마우스 입력을 할 수 없다.
		public void LockInputWhenPopupShown()
		{
			_editor.Mouse.InitMetaData();
			_editor.Mouse.Update_NoEvent();
			_editor.Mouse.UseWheel();
		}




		public bool IsMouseInGUI(Vector2 mousePos)
		{
			if (mousePos.x < 0 || mousePos.x > _editor._mainGUIRect.width)
			{
				return false;
			}

			if (mousePos.y < 0 || mousePos.y > _editor._mainGUIRect.height)
			{
				return false;
			}
			return true;
		}



		//------------------------------------------------------------
		// GUI 렌더링/업데이트 함수
		//------------------------------------------------------------
		/// <summary>
		/// 작업 공간 클릭을 했다면 다른 UI의 포커스를 해제한다.
		/// </summary>
		public void GUI_Input_CheckClickInCenter()
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatusWithoutActionID(apMouseSet.Button.Left);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatusWithoutActionID(apMouseSet.Button.Right);
			apMouse.MouseBtnStatus middleBtnStatus = _editor.Mouse.GetStatusWithoutActionID(apMouseSet.Button.Middle);


			if (leftBtnStatus == apMouse.MouseBtnStatus.Down
				|| rightBtnStatus == apMouse.MouseBtnStatus.Down
				|| middleBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				if (IsMouseInGUI(_editor.Mouse.Pos))
				{
					apEditorUtil.ReleaseGUIFocus();
				}
			}
		}


		/// <summary>
		/// 마우스 줌인/줌아웃
		/// </summary>
		/// <param name="isCtrl"></param>
		/// <param name="isShift"></param>
		/// <param name="isAlt"></param>
		/// <returns></returns>
		public bool GUI_Input_ZoomAndScroll(bool isCtrl, bool isShift, bool isAlt)
		{
			apMouseSet mouse = _editor.Mouse;
			
			if (mouse.Wheel != 0)//>>변경
			{
				if (IsMouseInGUI(mouse.PosLast))//>>변경
				{
					//현재 위치에서 마우스의 World 좌표를 구한다.
					float zoomPrev = _editor._zoomListX100[_editor._iZoomX100] * 0.01f;

					if (mouse.Wheel > 0)//>>이후
					{
						//줌 아웃 = 인덱스 감소
						_editor._iZoomX100--;
						if (_editor._iZoomX100 < 0)
						{
							_editor._iZoomX100 = 0;
						}
					}
					else if (mouse.Wheel < 0)//>>이후
					{
						//줌 인 = 인덱스 증가
						_editor._iZoomX100++;
						if (_editor._iZoomX100 >= _editor._zoomListX100.Length)
						{
							_editor._iZoomX100 = _editor._zoomListX100.Length - 1;
						}
					}
					//마우스의 World 좌표는 같아야 한다.
					float zoomNext = _editor._zoomListX100[_editor._iZoomX100] * 0.01f;

					//중심점의 위치를 구하자 (Editor GL 기준)
					Vector2 scroll = new Vector2(_editor._scroll_CenterWorkSpace.x * 0.1f * apGL.WindowSize.x,
													_editor._scroll_CenterWorkSpace.y * 0.1f * apGL.WindowSize.y);
					Vector2 guiCenterPos = apGL.WindowSizeHalf - scroll;

					Vector2 deltaMousePos = mouse.PosLast - guiCenterPos;//>>이후
					Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

					//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자
					//Delta = Mouse - GUICenter
					//GUICenter = Mouse - Delta
					//WindowSizeHalf - Scroll = Mouse - Delta
					//Scroll - WindowSizeHalf = Delta - Mouse
					//Scroll = (Delta - Mouse) + WindowSizeHalf
					//ScrollCenter * 0.1f * SizeXY = (Delta - Mouse) + WindowSizeHalf
					//ScrollCenter = ((Delta - Mouse) + WindowSizeHalf) / (0.1f * SizeXY)
					
					//>>변경
					float nextScrollX = ((nextDeltaMousePos.x - mouse.PosLast.x) + apGL.WindowSizeHalf.x) / (0.1f * apGL.WindowSize.x);
					float nextScrollY = ((nextDeltaMousePos.y - mouse.PosLast.y) + apGL.WindowSizeHalf.y) / (0.1f * apGL.WindowSize.y);

					nextScrollX = Mathf.Clamp(nextScrollX, -500.0f, 500.0f);
					nextScrollY = Mathf.Clamp(nextScrollY, -500.0f, 500.0f);

					_editor._scroll_CenterWorkSpace.x = nextScrollX;
					_editor._scroll_CenterWorkSpace.y = nextScrollY;

					_editor.SetRepaint();

					mouse.UseWheel();//변경

					return true;
				}

				//추가 22.7.13 : 작업 공간 외부에서 휠을 스크롤한 경우에
				//이걸 호출 안하면 휠값이 그대로 유지되어 나중에 이상하게 반영된다.
				mouse.UseWheel();
			}

			

			if (mouse.IsPressed(apMouseSet.Button.Middle, apMouseSet.ACTION.ScreenMove_MidBtn))
			{
				if (IsMouseInGUI(mouse.PosLast))//변경
				{
					Vector2 moveDelta = mouse.GetPosDelta(apMouseSet.Button.Middle);//이후
																						   //RealX = scroll * windowWidth * 0.1

					Vector2 sensative = new Vector2(
						1.0f / (_editor._mainGUIRect.width * 0.1f),
						1.0f / (_editor._mainGUIRect.height * 0.1f));

					_editor._scroll_CenterWorkSpace.x -= moveDelta.x * sensative.x;
					_editor._scroll_CenterWorkSpace.y -= moveDelta.y * sensative.y;

					_editor.SetRepaint();

					mouse.UseMouseDrag(apMouseSet.Button.Middle, apMouseSet.ACTION.ScreenMove_MidBtn);//이후

					//추가 : Pan 커서 모양이 나타나도록 하자
					apGL.AddCursorRectDelayed(mouse.PosLast, MouseCursor.Pan);
					return true;
				}
			}

			//추가 : Ctrl + Alt를 누르면 
			//- Left Button이 Middle Button의 역할을 한다.
			//- Right Button이 Wheel 역할을 한다.
			if (isCtrl && isAlt)
			{
				if (mouse.IsPressed(apMouseSet.Button.Left, apMouseSet.ACTION.ScreenMove_MidBtn))//변경
				{
					if (IsMouseInGUI(mouse.PosLast))//변경
					{
						Vector2 moveDelta = mouse.GetPosDelta(apMouseSet.Button.Left);//이후

						Vector2 sensative = new Vector2(
							1.0f / (_editor._mainGUIRect.width * 0.1f),
							1.0f / (_editor._mainGUIRect.height * 0.1f));

						_editor._scroll_CenterWorkSpace.x -= moveDelta.x * sensative.x;
						_editor._scroll_CenterWorkSpace.y -= moveDelta.y * sensative.y;

						_editor.SetRepaint();

						mouse.UseMouseDrag(apMouseSet.Button.Left, apMouseSet.ACTION.ScreenMove_MidBtn);//이후

						//추가 : Pan 커서 모양이 나타나도록 하자
						apGL.AddCursorRectDelayed(mouse.PosLast, MouseCursor.Pan);

						return true;
					}
				}

				if (mouse.IsPressed(apMouseSet.Button.Right, apMouseSet.ACTION.ScreenMove_MidBtn))//변경
				{
					if (IsMouseInGUI(mouse.PosLast))//변경
					{
						Vector2 moveDelta = mouse.GetPosDelta(apMouseSet.Button.Right);//이후

						float wheelOffset = 0.0f;
						if (Mathf.Abs(moveDelta.x) * 1.5f > Mathf.Abs(moveDelta.y))
						{
							wheelOffset = moveDelta.x;
						}
						else
						{
							wheelOffset = moveDelta.y;
						}

						//현재 위치에서 마우스의 World 좌표를 구한다.
						float zoomPrev = _editor._zoomListX100[Editor._iZoomX100] * 0.01f;

						if (wheelOffset < -1.3f)
						{
							//줌 아웃 = 인덱스 감소
							_editor._iZoomX100--;
							if (_editor._iZoomX100 < 0)
							{
								_editor._iZoomX100 = 0;
							}
						}
						else if (wheelOffset > 1.3f)
						{
							//줌 인 = 인덱스 증가
							_editor._iZoomX100++;
							if (_editor._iZoomX100 >= _editor._zoomListX100.Length)
							{
								_editor._iZoomX100 = _editor._zoomListX100.Length - 1;
							}
						}
						//마우스의 World 좌표는 같아야 한다.
						float zoomNext = _editor._zoomListX100[_editor._iZoomX100] * 0.01f;

						//중심점의 위치를 구하자 (Editor GL 기준)
						Vector2 scroll = new Vector2(_editor._scroll_CenterWorkSpace.x * 0.1f * apGL.WindowSize.x,
														_editor._scroll_CenterWorkSpace.y * 0.1f * apGL.WindowSize.y);
						Vector2 guiCenterPos = apGL.WindowSizeHalf - scroll;

						Vector2 deltaMousePos = mouse.PosLast - guiCenterPos;//>>이후
						Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

						//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자

						//>>변경
						float nextScrollX = ((nextDeltaMousePos.x - mouse.PosLast.x) + apGL.WindowSizeHalf.x) / (0.1f * apGL.WindowSize.x);
						float nextScrollY = ((nextDeltaMousePos.y - mouse.PosLast.y) + apGL.WindowSizeHalf.y) / (0.1f * apGL.WindowSize.y);

						nextScrollX = Mathf.Clamp(nextScrollX, -500.0f, 500.0f);
						nextScrollY = Mathf.Clamp(nextScrollY, -500.0f, 500.0f);

						_editor._scroll_CenterWorkSpace.x = nextScrollX;
						_editor._scroll_CenterWorkSpace.y = nextScrollY;

						_editor.SetRepaint();


						mouse.UseMouseDrag(apMouseSet.Button.Right, apMouseSet.ACTION.ScreenMove_MidBtn);//이후

						//추가 : Zoom 커서 모양이 나타나도록 하자
						apGL.AddCursorRectDelayed(mouse.PosLast, MouseCursor.Zoom);

						return true;
					}
				}
			}

			return false;
		}


		//----------------------------------------------------------------------------
		// 상황별 GizmoController 실행
		//----------------------------------------------------------------------------
		/// <summary>
		/// Mesh Modify 메뉴에서의 GUI 입력 처리
		/// </summary>
		public void GUI_Input_Mesh_Modify(float tDelta, bool isIgnoredUp)
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_Modify);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_Modify);
			Vector2 mousePos = _editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif
			_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);

		}

		/// <summary>
		/// Mesh - Make Mesh - TRS에서의 GUI 입력 처리
		/// </summary>
		public void GUI_Input_MakeMesh_TRS(float tDelta, bool isIgnoredUp)
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_AutoGen);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_AutoGen);
			Vector2 mousePos = _editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);
		}

		/// <summary>
		/// Mesh - Make Mesh - Atlas 편집 상태에서의 GUI 입력 처리
		/// </summary>
		public void GUI_Input_MakeMesh_AtlasAreaEdit()
		{
			if (Event.current.type == EventType.Used)
			{
				return;
			}

			apMesh selectedMesh = Editor.Select.Mesh;

			if(selectedMesh == null 
				|| selectedMesh._textureData_Linked == null
				|| selectedMesh._textureData_Linked._image == null
				)
			{
				return;
			}

			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_AutoGen);
			Vector2 mousePos = _editor.Mouse.Pos;
			apTextureData linkedTextureData = selectedMesh._textureData_Linked;

			Vector2 prevAtlas_LT = selectedMesh._atlasFromPSD_LT;
			Vector2 prevAtlas_RB = selectedMesh._atlasFromPSD_RB;

			Vector2 posLT = apGL.World2GL(new Vector2(selectedMesh._atlasFromPSD_LT.x - selectedMesh._offsetPos.x, selectedMesh._atlasFromPSD_LT.y - selectedMesh._offsetPos.y));
			Vector2 posRT = apGL.World2GL(new Vector2(selectedMesh._atlasFromPSD_RB.x - selectedMesh._offsetPos.x, selectedMesh._atlasFromPSD_LT.y - selectedMesh._offsetPos.y));
			Vector2 posLB = apGL.World2GL(new Vector2(selectedMesh._atlasFromPSD_LT.x - selectedMesh._offsetPos.x, selectedMesh._atlasFromPSD_RB.y - selectedMesh._offsetPos.y));
			Vector2 posRB = apGL.World2GL(new Vector2(selectedMesh._atlasFromPSD_RB.x - selectedMesh._offsetPos.x, selectedMesh._atlasFromPSD_RB.y - selectedMesh._offsetPos.y));
			
			//Debug.Log("Editing.. : " + leftBtnStatus + " / " + Event.current.isMouse);
			
			float clickSize = 26 + 5.0f;
			bool isUpdateArea = false;
			bool isFirstMove = false;
			if (Event.current.isMouse)
			{
				//Debug.Log("마우스 이벤트 : " + leftBtnStatus);
				if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					if (IsMouseInGUI(mousePos))
					{
						//가장 가까운 점을 찾자
						_editor.Select._meshAreaPointEditType = apSelection.MESH_AREA_POINT_EDIT.NotSelected;

						//Debug.Log("Mouse Pos : " + mousePos + " / LT : " + posLT + " / RT : " + posRT + " / LB : " + posLB + " / RB : " + posRB);
						//Debug.Log("World LT : " + selectedMesh._atlasFromPSD_LT);
						//Debug.Log("World LT (Offset) : " + (selectedMesh._atlasFromPSD_LT - selectedMesh._offsetPos));
						float minDist = 0.0f;
						int selectType = -1;
						
						//LT, RT, LB, RB 순으로 확인
						float dist = Vector2.Distance(posLT, mousePos);
						if(dist < clickSize)
						{
							selectType = 0;//LT
							minDist = dist;
						}

						dist = Vector2.Distance(posRT, mousePos);
						if(dist < clickSize && (selectType < 0 || dist < minDist))
						{
							selectType = 1;//RT
							minDist = dist;
						}

						dist = Vector2.Distance(posLB, mousePos);
						if(dist < clickSize && (selectType < 0 || dist < minDist))
						{
							selectType = 2;//LB
							minDist = dist;
						}

						dist = Vector2.Distance(posRB, mousePos);
						if(dist < clickSize && (selectType < 0 || dist < minDist))
						{
							selectType = 3;//LB
							minDist = dist;
						}

						//Debug.LogError("Down > Select : " + selectType);

						switch (selectType)
						{
							case 0: _editor.Select._meshAreaPointEditType = apSelection.MESH_AREA_POINT_EDIT.LT; break;
							case 1: _editor.Select._meshAreaPointEditType = apSelection.MESH_AREA_POINT_EDIT.RT; break;
							case 2: _editor.Select._meshAreaPointEditType = apSelection.MESH_AREA_POINT_EDIT.LB; break;
							case 3: _editor.Select._meshAreaPointEditType = apSelection.MESH_AREA_POINT_EDIT.RB; break;
						}

						if(selectType >= 0)
						{
							isFirstMove = true;
							isUpdateArea = true;
						}
					}
					//else
					//{
					//	Debug.Log("Down > 외부에서 클릭");
					//}
				}
				else if(leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					if(_editor.Select._meshAreaPointEditType != apSelection.MESH_AREA_POINT_EDIT.NotSelected)
					{
						isUpdateArea = true;
						//Debug.Log("크기 변동중...");
					}
				}
				else
				{
					if(_editor.Select._meshAreaPointEditType != apSelection.MESH_AREA_POINT_EDIT.NotSelected)
					{
						_editor.Select._meshAreaPointEditType = apSelection.MESH_AREA_POINT_EDIT.NotSelected;
						//Debug.LogError("이동 종료");
					}
				}

				if(isUpdateArea)
				{
					if(isFirstMove)
					{
						apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_AtlasChanged, 
														Editor, 
														Editor.Select.Mesh, 
														//Editor.Select.Mesh, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					}

					Vector2 inputPosW = apGL.GL2World(mousePos) + selectedMesh._offsetPos;
					
					//바로 값을 적용하지 말고, 다른쪽 영역을 침범하지 못하게 제한
					Vector2 nextPosLT = prevAtlas_LT;
					Vector2 nextPosRB = prevAtlas_RB;
					float bias = 5.0f;
					switch (_editor.Select._meshAreaPointEditType)
					{
						case apSelection.MESH_AREA_POINT_EDIT.LT:
							{
								nextPosLT.x = inputPosW.x;
								nextPosLT.y = inputPosW.y;
								if(nextPosLT.x > nextPosRB.x - bias)
								{
									nextPosLT.x = nextPosRB.x - bias;
								}
								if(nextPosLT.y < nextPosRB.y + bias)
								{
									nextPosLT.y = nextPosRB.y + bias;
								}
							}
							break;

						case apSelection.MESH_AREA_POINT_EDIT.RT:
							{
								nextPosRB.x = inputPosW.x;
								nextPosLT.y = inputPosW.y;
								if(nextPosRB.x < nextPosLT.x + bias)
								{
									nextPosRB.x = nextPosLT.x + bias;
								}
								if(nextPosLT.y < nextPosRB.y + bias)
								{
									nextPosLT.y = nextPosRB.y + bias;
								}
							}
							break;

						case apSelection.MESH_AREA_POINT_EDIT.LB:
							{
								nextPosLT.x = inputPosW.x;
								nextPosRB.y = inputPosW.y;
								if(nextPosLT.x > nextPosRB.x - bias)
								{
									nextPosLT.x = nextPosRB.x - bias;
								}
								if(nextPosRB.y > nextPosLT.y - bias)
								{
									nextPosRB.y = nextPosLT.y - bias;
								}
							}
							break;

						case apSelection.MESH_AREA_POINT_EDIT.RB:
							{
								nextPosRB.x = inputPosW.x;
								nextPosRB.y = inputPosW.y;
								if(nextPosRB.x < nextPosLT.x + bias)
								{
									nextPosRB.x = nextPosLT.x + bias;
								}
								if(nextPosRB.y > nextPosLT.y - bias)
								{
									nextPosRB.y = nextPosLT.y - bias;
								}
							}
							break;
					}

					float halfWidth = (float)linkedTextureData._width * 0.5f;
					float halfHeight = (float)linkedTextureData._height * 0.5f;
					
					nextPosLT.x = Mathf.Clamp(nextPosLT.x, -halfWidth, halfWidth);
					nextPosRB.x = Mathf.Clamp(nextPosRB.x, -halfWidth, halfWidth);

					nextPosLT.y = Mathf.Clamp(nextPosLT.y, -halfHeight, halfHeight);
					nextPosRB.y = Mathf.Clamp(nextPosRB.y, -halfHeight, halfHeight);

					selectedMesh._atlasFromPSD_LT = nextPosLT;
					selectedMesh._atlasFromPSD_RB = nextPosRB;

					//Debug.Log(">> 크기 갱신");
				}
			}
		}


		// Make Mesh의 GUI Input 함수
		private bool _isAnyVertexMoved = false;
		private bool _isHiddenEdgeTurnable = false;
		private bool _isMeshVertMovable = false;//< Vertex를 이동할 수 있는 조건 (1. null -> 새로 클릭 / 2. 기존꺼 다시 클릭) ~ 불가 조건 (기존꺼 -> 다른거 클릭)


		/// <summary>
		/// Mesh - Make Mesh의 GUI 입력
		/// </summary>
		public void GUI_Input_MakeMesh(apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS makeMeshMode)
		{
			if (Event.current.type == EventType.Used)
			{
				return;
			}

			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_Make);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_Make);
			Vector2 mousePos = _editor.Mouse.Pos;

			bool isShift = Event.current.shift;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			//추가
			//Ctrl을 누르면 Vertex를 선택하는게 "범위 이내"에서 "가장 가까운"으로 바뀐다. (Snap)
			//Shift를 누르면 1개가 아닌 여러개의 충돌 점을 검색하고, Edge를 만들때 아예 충돌점에 Vertex를 추가하며 강제로 만든다.


			bool isNearestVertexCheckable = false;


			bool isVertEdgeRemovalble = false;

			if (_editor.VertController.Vertex == null)
			{
				isVertEdgeRemovalble = true;//<<이전에 선택한게 없으면 다음 선택시 삭제 가능
			}


			apMesh mesh = _editor.Select.Mesh;
			if(mesh == null)
			{
				return;
			}

			if (_editor.VertController.Mesh == null || mesh != _editor.VertController.Mesh)
			{
				_editor.VertController.SetMesh(mesh);
			}


			if (Event.current.isMouse)
			{
				if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					_isMeshVertMovable = false;//일단 이동 불가


					if (IsMouseInGUI(mousePos))
					{
						//추가: 일단 Mirror 설정 중 Move 값 초기화
						if (_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
						{
							_editor.MirrorSet.ClearMovedVertex();
						}

						if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon)
						{
							apVertex prevSelectedVert = _editor.VertController.Vertex;

							if (prevSelectedVert == null)
							{
								_isMeshVertMovable = true;//새로 선택하면 -> 다음에 Vert 이동 가능 (1)
							}

							bool isAnySelect = false;
							bool isAnyAddVertOrMesh = false;
							//Ctrl을 누르는 경우 -> 가장 가까운 Vertex를 선택한다. (즉, Vertex 추가는 안된다.)
							if (isCtrl &&
								(
									makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge
									|| makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly)
								)
							{
								apVertex nearestVert = null;
								float minDistToVert = 0.0f;

								int nVerts = mesh._vertexData != null ? mesh._vertexData.Count : 0;
								apVertex vertex = null;
								if (nVerts > 0)
								{
									for (int i = 0; i < nVerts; i++)
									{
										vertex = mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);
										float distToMouse = Vector2.Distance(posGL, mousePos);
										if (nearestVert == null || distToMouse < minDistToVert)
										{
											nearestVert = vertex;
											minDistToVert = distToMouse;
										}
									}
								}
								
								if (nearestVert != null)
								{
									//가장 가까운 Vert를 찾았다.
									if (prevSelectedVert == nearestVert)
									{
										//같은걸 선택했다.
										//이동 가능
										_isMeshVertMovable = true;

									}
									else
									{
										//추가 4.8 : Ctrl을 누르고 선택한 경우 이동 불가
										_isMeshVertMovable = false;
									}

									_editor.VertController.SelectVertex(nearestVert);
									isAnySelect = true;

									//추가 : 이전 버텍스에서 새로운 버텍스로 자동으로 Edge를 생성해주자
									if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly)
									{
										if (nearestVert != prevSelectedVert
											&& prevSelectedVert != null
											&& nearestVert != null)
										{
											//Debug.Log("Ctrl");
											//Undo - Add Edge
											apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddEdge,
																			Editor,
																			Editor.Select.Mesh,
																			//null, 
																			false,
																			apEditorUtil.UNDO_STRUCT.ValueOnly);

											mesh.MakeNewEdge(prevSelectedVert, nearestVert, isShift);
											isAnyAddVertOrMesh = true;

											//추가 9.12 : 미러 옵션이 있다면 반대쪽도 만들어야 한다.
											if (_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
											{
												_editor.MirrorSet.RefreshMeshWork(mesh, _editor.VertController);
												_editor.MirrorSet.AddMirrorVertex(prevSelectedVert, nearestVert, mesh, true, isShift, false, false);

											}
										}
									}
								}
							}
							else
							{
								int nVertCount = mesh._vertexData != null ? mesh._vertexData.Count : 0;
								//버텍스 선택 로직 변경 (10.22)
								// - 일단 좁은 범위의 버텍스를 찾는다 > 이걸 찾으면 무조건 오케이 + 루프 종료
								// - 조금 더 넓은 범위의 버텍스도 찾는다. 이건 거리를 체크한다. > 더 가까운 걸 선택한다.
								// - 좁은 범위의 버텍스를 찾지 못했다면 넓은 범위의 버텍스를 선택한다.
								apVertex clickedVert_Default = null;
								apVertex clickedVert_Wide = null;

								float minSqrDistClickedVertWide = -1.0f;
								VERTEX_CLICK_RESULT curClickResult = VERTEX_CLICK_RESULT.None;
								float curSqrDist = 0.0f;

								if (nVertCount > 0)
								{
									apVertex vertex = null;
									for (int i = 0; i < nVertCount; i++)
									{
										vertex = mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);

										//어떤 버텍스를 클릭할 수 있는지 체크
										curClickResult = IsVertexClickable(ref posGL, ref mousePos, ref curSqrDist);

										if (curClickResult == VERTEX_CLICK_RESULT.None)
										{
											continue;
										}

										if (curClickResult == VERTEX_CLICK_RESULT.DirectClick)
										{
											//정확하게 클릭했다면
											clickedVert_Default = vertex;
											break;
										}
										//근처에서 클릭했다면 > 거리계산
										if (clickedVert_Wide == null || curSqrDist < minSqrDistClickedVertWide)
										{
											//가깝거나 없거나
											clickedVert_Wide = vertex;
											minSqrDistClickedVertWide = curSqrDist;
										}
									}
								}
								

								//클릭된 버텍스를 찾자
								apVertex resultClickedVert = null;
								if (clickedVert_Default != null)
								{
									resultClickedVert = clickedVert_Default;
								}
								else if (clickedVert_Wide != null)
								{
									resultClickedVert = clickedVert_Wide;
								}

								//클릭한게 있다.
								if (resultClickedVert != null)
								{
									if (prevSelectedVert == resultClickedVert)
									{
										//같은걸 선택했다.
										//이동 가능
										_isMeshVertMovable = true;
									}

									_editor.VertController.SelectVertex(resultClickedVert);
									isAnySelect = true;

									//추가 : 이전 버텍스에서 새로운 버텍스로 자동으로 Edge를 생성해주자
									if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly)
									{
										if (_editor.VertController.Vertex != prevSelectedVert
											&& prevSelectedVert != null
											&& _editor.VertController.Vertex != null)
										{
											//Undo - Add Edge
											apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddEdge,
																			_editor,
																			mesh,
																			//null, 
																			false,
																			apEditorUtil.UNDO_STRUCT.ValueOnly);

											mesh.MakeNewEdge(prevSelectedVert, _editor.VertController.Vertex, isShift);
											isAnyAddVertOrMesh = true;


											//추가 9.12 : 미러 옵션이 있다면 반대쪽도 만들어야 한다.
											if (_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
											{
												_editor.MirrorSet.RefreshMeshWork(mesh, _editor.VertController);
												_editor.MirrorSet.AddMirrorVertex(prevSelectedVert, _editor.VertController.Vertex, mesh, true, isShift, false, false);

											}
										}
									}
								}
							}


							if (!isAnySelect)
							{
								_editor.VertController.UnselectVertex();

								//아무 버텍스를 선택하지 않았다.
								//새로 추가한다. => Vertex 모드일 때
								if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly ||
									makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge)
								{
									if (_editor.VertController.Vertex == null)
									{
										//Undo - Vertex 추가
										apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddVertex,
																		Editor,
																		Editor.Select.Mesh,
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

										// <<< 버텍스 추가 >>>
										Vector2 mouseWorld = apGL.GL2World(mousePos) + mesh._offsetPos;

										//변경 9.14 : Shift를 누른 상태에서 근처에 Edge가 있다면->그 Edge를 분할한다. (Edge상에 버텍스를 추가하는 효과)
										apVertex addedVert = null;
										bool isAddVertexWithSplitEdge = false;

										if (isShift && !isCtrl)
										{
											//Debug.Log("새로운 버텍스 + Shift [" + makeMeshMode + "]");
											//Shift를 눌렀고, 이전에 선택된 Vert가 없다면..

											//추가 v1.4.2
											float distBiasSplitEdge = _editor.GUIRenderSettings.EdgeSelectBaseRange * 0.7f;//기본 Edge 선택 거리보다 짧다. (기본 값인 5 > 3으로 줄어듦)

											apMeshEdge nearestEdge = GetMeshNearestEdge(mousePos, mesh, distBiasSplitEdge);

											if (nearestEdge != null)
											{
												Vector2 splitPos = nearestEdge.GetNearestPosOnEdge(apGL.GL2World(mousePos) + mesh._offsetPos);
												if (Mathf.Abs(splitPos.x - nearestEdge._vert1._pos.x) < 1 && Mathf.Abs(splitPos.y - nearestEdge._vert1._pos.y) < 1)
												{
													//Vert1과 겹친다.
													addedVert = nearestEdge._vert1;
												}
												else if (Mathf.Abs(splitPos.x - nearestEdge._vert2._pos.x) < 1 && Mathf.Abs(splitPos.y - nearestEdge._vert2._pos.y) < 1)
												{
													//Vert2와 겹친다.
													addedVert = nearestEdge._vert2;
												}
												else
												{
													//겹치는게 없다.
													addedVert = mesh.SplitEdge(nearestEdge, splitPos);
													isAddVertexWithSplitEdge = true;
												}
											}
											else
											{
												//그냥 새로 생성
												addedVert = mesh.AddVertexAutoUV(mouseWorld);
											}
										}
										else
										{
											//그냥 새로 생성
											addedVert = mesh.AddVertexAutoUV(mouseWorld);
										}

										if (addedVert != null)
										{
											_editor.VertController.SelectVertex(addedVert);
										}


										bool isMakeEdge = false;
										if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge)
										{
											//삭제 20.7.6
											//apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor, Editor.Select.Mesh, null, false);

											//만약 이전에 선택한 버텍스가 있다면
											//Edge를 연결하자
											if (prevSelectedVert != null)
											{
												// <<< 버텍스 -> 선 추가 >>>
												isMakeEdge = true;
												mesh.MakeNewEdge(prevSelectedVert, addedVert, isShift);
											}
										}

										//추가 9.12 : 미러 옵션이 있다면 반대쪽도 만들어야 한다.
										if (_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
										{
											_editor.MirrorSet.RefreshMeshWork(mesh, _editor.VertController);

											//추가
											_editor.MirrorSet.AddMirrorVertex(prevSelectedVert, addedVert, mesh, isMakeEdge, isShift, true, isAddVertexWithSplitEdge);

										}


										//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
										//if(undo != null) { undo.Refresh(); }
										//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
									}
								}
								else
								{
									//Edge 선택 모드에서
									//만약 HiddenEdge를 선택한다면
									//Turn을 하자
									_editor.VertController.UnselectVertex();
									_editor.VertController.UnselectNextVertex();

									if (_isHiddenEdgeTurnable)
									{
										//Undo - Vertex 추가
										apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditEdge,
																		Editor,
																		Editor.Select.Mesh,
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

										//아무것도 선택하지 않았다면..
										//Hidden Edge의 Trun을 한번 해보자
										apMeshEdge minHiddenEdge = null;
										float minDist = float.MaxValue;
										apMeshPolygon minPoly = null;


										//v1.4.2 : Edge를 클릭하는 거리값은 옵션에 의해 결정된다. (기존 5 고정)
										float edgeClickRange = _editor.GUIRenderSettings.EdgeSelectBaseRange;

										List<apMeshPolygon> polygons = mesh._polygons;

										int nPolygons = polygons != null ? polygons.Count : 0;
										if (nPolygons > 0)
										{
											apMeshPolygon curPoly = null;
											for (int iPoly = 0; iPoly < nPolygons; iPoly++)
											{
												curPoly = polygons[iPoly];

												List<apMeshEdge> hiddenEdges = curPoly._hidddenEdges;
												int nHiddenEdges = hiddenEdges != null ? hiddenEdges.Count : 0;
												if(nHiddenEdges == 0)
												{
													continue;
												}

												apMeshEdge hiddenEdge = null;
												for (int iHide = 0; iHide < nHiddenEdges; iHide++)
												{
													hiddenEdge = hiddenEdges[iHide];

													Vector2 vPos1 = new Vector2(hiddenEdge._vert1._pos.x, hiddenEdge._vert1._pos.y) - mesh._offsetPos;
													Vector2 vPos2 = new Vector2(hiddenEdge._vert2._pos.x, hiddenEdge._vert2._pos.y) - mesh._offsetPos;

													float distEdge = apEditorUtil.DistanceFromLine(
																		apGL.World2GL(vPos1),
																		apGL.World2GL(vPos2),
																		mousePos);

													if (distEdge < edgeClickRange)
													{
														if (minHiddenEdge == null || distEdge < minDist)
														{
															minDist = distEdge;
															minHiddenEdge = hiddenEdge;
															minPoly = curPoly;
														}
													}
												}
											}
										}

										

										if (minHiddenEdge != null)
										{
											//Debug.Log("Try Hidden Edge Turn");
											if (minPoly.TurnHiddenEdge(minHiddenEdge))
											{
												mesh.RefreshPolygonsToIndexBuffer();
											}
										}

										_isHiddenEdgeTurnable = false;
									}
								}
							}
							else
							{
								if (!isAnyAddVertOrMesh)
								{
									//Debug.Log("Start Vertex Edit");
									//Undo - MeshEdit Vertex Pos Changed
									apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditVertex,
																	Editor,
																	Editor.Select.Mesh,
																	//Editor.VertController.Vertex, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
								}
							}
						}
						else
						{
							//추가 : Polygon 모드
							//apMeshPolygon prevPolygon = Editor.VertController.Polygon;
							List<apMeshPolygon> polygons = Editor.Select.Mesh._polygons;
							int nPolygons = polygons != null ? polygons.Count : 0;

							Vector2 meshOffsetPos = mesh._offsetPos;
							bool isAnyPolygonSelect = false;

							if (nPolygons > 0)
							{
								apMeshPolygon polygon = null;
								for (int iPoly = 0; iPoly < nPolygons; iPoly++)
								{
									polygon = polygons[iPoly];
									if (IsPolygonClickable(polygon, meshOffsetPos, mousePos))
									{
										Editor.VertController.SelectPolygon(polygon);
										isAnyPolygonSelect = true;
										break;
									}
								}
							}
							
							if (!isAnyPolygonSelect)
							{
								_editor.VertController.UnselectVertex();//<<이걸 호출하면 Polygon도 선택 해제됨
							}
						}
						_editor.SetRepaint();

						//통계 재계산 요청
						_editor.Select.SetStatisticsRefresh();
					}
				}
				else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly &&
						makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon)
					{
						if (_editor.VertController.Vertex != null)
						{
							if (IsMouseInGUI(mousePos))
							{
								if (_isMeshVertMovable)
								{
									if (!_isAnyVertexMoved)
									{
										//?
									}

									//버텍스 이동
									//Undo - MeshEdit Vertex Pos Changed
									//apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
									//								Editor, 
									//								Editor.Select.Mesh, 
									//								//Editor.VertController.Vertex, 
									//								true,
									//								apEditorUtil.UNDO_STRUCT.ValueOnly);

									Vector2 prevPos = _editor.VertController.Vertex._pos;
									Vector2 nextPos = apGL.GL2World(mousePos) + mesh._offsetPos;

									if (_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
									{
										//만약 Mirror가 켜졌다면
										if (_editor._meshTRSOption_MirrorSnapVertOnRuler)
										{
											//Snap 옵션이 켜졌을 때
											if (_editor.MirrorSet.IsOnAxisByMesh(nextPos, mesh))
											{
												//Ruler로 위치를 보정할 필요가 있다.
												nextPos = _editor.MirrorSet.GetAxisPosToSnap(nextPos, mesh);
											}
										}
									}
									_editor.VertController.Vertex._pos = nextPos;
									_editor.VertController.Mesh.RefreshVertexAutoUV(Editor.VertController.Vertex);

									_isAnyVertexMoved = true;

									//추가 : 미러 옵션이 켜진 경우 : 맞은편 버텍스를 찾아서 움직여보자
									if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
									{
										_editor.MirrorSet.MoveMirrorVertex(_editor.VertController.Vertex, prevPos, _editor.VertController.Vertex._pos, mesh);
									}
								}
							}
							else
							{
								_editor.VertController.UnselectVertex();
							}

							Editor.SetRepaint();
						}
					}
				}
				else if (leftBtnStatus == apMouse.MouseBtnStatus.Up ||
						leftBtnStatus == apMouse.MouseBtnStatus.Released)
				{
					_editor.VertController.StopEdgeWire();

					_isHiddenEdgeTurnable = true;

					if (_isAnyVertexMoved)
					{
						//apEditorUtil.SetRecord("Vertex Pos Change", Editor._selection.Mesh);
						_isAnyVertexMoved = false;
					}
				}

				//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Pos;

				if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					_editor.VertController.UnselectVertex();

					if (IsMouseInGUI(mousePos))
					{
						bool isAnyRemoved = false;

						if (isVertEdgeRemovalble)
						{
							if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly ||
								makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge)
							{
								//1. 버텍스 제거
								apVertex clickedVert_Direct = null;
								apVertex clickedVert_Wide = null;
								float minWideClickDist = 0.0f;
								float curWideClickDist = 0.0f;
								VERTEX_CLICK_RESULT clickResult = VERTEX_CLICK_RESULT.None;

								int nVerts = mesh._vertexData != null ? mesh._vertexData.Count : 0;
								if (nVerts > 0)
								{
									apVertex vertex = null;
									for (int i = 0; i < nVerts; i++)
									{
										vertex = mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);

										curWideClickDist = 0.0f;

										//버텍스를 클릭하자
										clickResult = IsVertexClickable(ref posGL, ref mousePos, ref curWideClickDist);

										if (clickResult == VERTEX_CLICK_RESULT.None)
										{
											continue;
										}

										if (clickResult == VERTEX_CLICK_RESULT.DirectClick)
										{
											//정확하게 클릭
											clickedVert_Direct = vertex;
											break;
										}

										//근처에서 클릭 > 거리 비교
										if (clickedVert_Wide == null || curWideClickDist < minWideClickDist)
										{
											clickedVert_Wide = vertex;
											minWideClickDist = curWideClickDist;
										}
									}
								}
								

								apVertex resultClickedVert = null;
								if (clickedVert_Direct != null)
								{
									resultClickedVert = clickedVert_Direct;
								}
								else if (clickedVert_Wide != null)
								{
									resultClickedVert = clickedVert_Wide;
								}


								if (resultClickedVert != null)
								{
									// Undo - MeshEdit_VertexRemoved
									apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemoveVertex,
																	Editor,
																	Editor.Select.Mesh,
																	//vertex, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);


									mesh.RemoveVertex(resultClickedVert, isShift);

									//추가 9.13 : Mirror Vertex 삭제
									if (_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror
										&& _editor._meshTRSOption_MirrorRemoved)
									{
										//반대편 버텍스도 삭제한다.
										Editor.MirrorSet.RemoveMirrorVertex(resultClickedVert, mesh, isShift);
									}

									_editor.SetRepaint();
									isAnyRemoved = true;
									isVertEdgeRemovalble = false;
								}
							}


						}

						if (isVertEdgeRemovalble)
						{
							if (!isAnyRemoved)
							{
								if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge ||
									makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly)
								{
									//2. Edge 제거
									apMeshEdge selectEdge = null;

									//변경 : 함수 하나로 합침

									float distEdgeClick = _editor.GUIRenderSettings.EdgeSelectBaseRange;//추가 v1.4.2 : 옵션에 의한 Edge 선택 범위 (기본값은 5 그대로)

									selectEdge = GetMeshNearestEdge(mousePos, mesh, distEdgeClick);

									if (selectEdge != null)
									{
										//삭제합시더
										// Undo - MeshEdit_EdgeRemoved
										apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemoveEdge,
																		Editor,
																		Editor.Select.Mesh,
																		//selectEdge, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);


										mesh.RemoveEdge(selectEdge);
										isVertEdgeRemovalble = false;

										//추가 9.13 : Mirror Edge 삭제
										if (_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror
											&& _editor._meshTRSOption_MirrorRemoved)
										{
											//반대편 Edge도 삭제한다.
											_editor.MirrorSet.RemoveMirrorEdge(selectEdge, mesh);
										}
									}

									_editor.VertController.UnselectVertex();
									_editor.VertController.UnselectNextVertex();
									_editor.SetRepaint();

									//통계 재계산 요청
									_editor.Select.SetStatisticsRefresh();
								}
							}
						}
					}

				}
			}

			isNearestVertexCheckable = false;

			if (_editor.VertController.Vertex != null)
			{
				if (IsMouseInGUI(mousePos) && makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon)
				{
					if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge ||
						makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly)
					{
						if (rightBtnStatus == apMouse.MouseBtnStatus.Up ||
							rightBtnStatus == apMouse.MouseBtnStatus.Released)
						{
							if (IsMouseInGUI(mousePos))
							{
								isNearestVertexCheckable = true;
							}
						}
					}
				}

				_editor.VertController.UpdateEdgeWire(mousePos, isShift, isCtrl);
				_editor.VertController.UnselectNextVertex();

				//마우스에서 가까운 Vertex를 찾는다.
				//Ctrl을 누르면 : 가장 가까운거 무조건
				//기본 : Vertex 영역안에 있는거
				if (isNearestVertexCheckable)
				{
					if (isCtrl)
					{
						apVertex nearestVert = null;
						float minDistToVert = 0.0f;

						int nVerts = mesh._vertexData != null ? mesh._vertexData.Count : 0;
						if (nVerts > 0)
						{
							apVertex vertex = null;
							for (int i = 0; i < nVerts; i++)
							{
								vertex = mesh._vertexData[i];
								Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - mesh._offsetPos;

								Vector2 posGL = apGL.World2GL(vPos);
								float distToMouse = Vector2.Distance(posGL, mousePos);
								if (nearestVert == null || distToMouse < minDistToVert)
								{
									nearestVert = vertex;
									minDistToVert = distToMouse;
								}
							}
						}
						
						if (nearestVert != null)
						{
							_editor.VertController.SelectNextVertex(nearestVert);

							Vector2 vPos = new Vector2(nearestVert._pos.x, nearestVert._pos.y) - mesh._offsetPos;
							Vector2 posGL = apGL.World2GL(vPos);


							_editor.VertController.UpdateEdgeWire(posGL, isShift, isCtrl);
						}
					}
					else
					{
						apVertex clickedVert_Direct = null;
						apVertex clickedVert_Wide = null;
						
						float minWideClickDist = 0.0f;
						float curWideClickDist = 0.0f;
						VERTEX_CLICK_RESULT clickResult = VERTEX_CLICK_RESULT.None;

						int nVerts = mesh._vertexData != null ? mesh._vertexData.Count : 0;
						if (nVerts > 0)
						{
							apVertex vertex = null;
							for (int i = 0; i < nVerts; i++)
							{
								vertex = mesh._vertexData[i];
								Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - mesh._offsetPos;

								Vector2 posGL = apGL.World2GL(vPos);

								//어떤 버텍스를 선택했다.
								clickResult = IsVertexClickable(ref posGL, ref mousePos, ref curWideClickDist);

								if (clickResult == VERTEX_CLICK_RESULT.None)
								{
									continue;
								}

								if (clickResult == VERTEX_CLICK_RESULT.DirectClick)
								{
									//직접 선택
									clickedVert_Direct = vertex;
									break;
								}

								//근처에서 선택 : 거리 비교
								if (clickedVert_Wide == null || curWideClickDist < minWideClickDist)
								{
									clickedVert_Wide = vertex;
									minWideClickDist = curWideClickDist;
								}
							}
						}
						

						apVertex clickedVert = null;
						if(clickedVert_Direct != null)
						{
							clickedVert = clickedVert_Direct;
						}
						else if(clickedVert_Wide != null)
						{
							clickedVert = clickedVert_Wide;
						}

						if (clickedVert != null)
						{
							_editor.VertController.SelectNextVertex(clickedVert);

							Vector2 posGL = apGL.World2GL(new Vector2(clickedVert._pos.x, clickedVert._pos.y) - Editor.Select.Mesh._offsetPos);
							_editor.VertController.UpdateEdgeWire(posGL, isShift, isCtrl);
						}
						
					}
				}

				//Edge로의 스냅 거리는 기본 3에 해상도 보정값을 곱한다. [v1.4.2]
				float edgeSnapDist = 3.0f * _editor.GUIRenderSettings.ClickRangeCorrectionByResolution;

				_editor.VertController.UpdateSnapEdgeGUIOnly(mousePos, isShift, isCtrl, leftBtnStatus == apMouse.MouseBtnStatus.Pressed, edgeSnapDist);
			}
			else
			{
				_editor.VertController.StopEdgeWire();
				_editor.VertController.UnselectNextVertex();

				if (isCtrl)
				{
					apVertex nearestVert = null;
					float minDistToVert = 0.0f;

					int nVerts = mesh._vertexData != null ? mesh._vertexData.Count : 0;
					if (nVerts > 0)
					{
						apVertex vertex = null;
						for (int i = 0; i < nVerts; i++)
						{
							vertex = mesh._vertexData[i];
							Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - mesh._offsetPos;

							Vector2 posGL = apGL.World2GL(vPos);
							float distToMouse = Vector2.Distance(posGL, mousePos);
							if (nearestVert == null || distToMouse < minDistToVert)
							{
								nearestVert = vertex;
								minDistToVert = distToMouse;
							}
						}
					}

					
					if (nearestVert != null)
					{
						//Ctrl을 누르고 있을 때 Next Vertex만은 다시 계산
						_editor.VertController.SelectNextVertex(nearestVert);
					}
				}

				//Edge로의 스냅 거리는 기본 3에 해상도 보정값을 곱한다. [v1.4.2]
				float edgeSnapDist = 3.0f * _editor.GUIRenderSettings.ClickRangeCorrectionByResolution;

				_editor.VertController.UpdateSnapEdgeGUIOnly(mousePos, isShift, isCtrl, leftBtnStatus == apMouse.MouseBtnStatus.Pressed, edgeSnapDist);
			}
		}


		public apMeshEdge GetMeshNearestEdge(Vector2 posGL, apMesh mesh, float offsetGL)
		{
			apMeshEdge curEdge = null;

			//Vector2 posW = apGL.GL2World(posGL) + mesh._offsetPos;

			Vector2 vPos1GL = Vector2.zero;
			Vector2 vPos2GL = Vector2.zero;
			float minX = 0.0f;
			float maxX = 0.0f;
			float minY = 0.0f;
			float maxY = 0.0f;
			float curDist = 0.0f;

			float minDist = 0.0f;
			apMeshEdge minEdge = null;

			int nEdges = mesh._edges != null ? mesh._edges.Count : 0;
			
			if(nEdges == 0)
			{
				return null;
			}

			//추가 v1.4.2 : AABB가 너무 타이트하게 들어가서 거리 비교가 아예 불가능한 버그 해결
			float aabbBias = (offsetGL * 1.5f) + 5.0f;

			for (int i = 0; i < nEdges; i++)
			{
				curEdge = mesh._edges[i];

				if (curEdge._vert1 == null || curEdge._vert2 == null)
				{
					continue;
				}

				//기본 사각 범위안에 있는지 확인
				vPos1GL = apGL.World2GL(curEdge._vert1._pos - mesh._offsetPos);
				vPos2GL = apGL.World2GL(curEdge._vert2._pos - mesh._offsetPos);

				minX = Mathf.Min(vPos1GL.x, vPos2GL.x);
				maxX = Mathf.Max(vPos1GL.x, vPos2GL.x);
				minY = Mathf.Min(vPos1GL.y, vPos2GL.y);
				maxY = Mathf.Max(vPos1GL.y, vPos2GL.y);


				//이전 : 버그 (수직 수평선의 경우 X축 또는 Y축의 범위가 0에 수렴하여 OffsetGL과 비교하기도 전에 연산을 포기한다)
				//if (posGL.x < minX || maxX < posGL.x ||
				//	posGL.y < minY || maxY < posGL.y)
				//{
				//	continue;
				//}

				//수정 : 적절히 Bias를 둬서 AABB 체크가 빡세게 들어가지 않게 한다.
				if(posGL.x < (minX - aabbBias) || (maxX + aabbBias) < posGL.x ||
					posGL.y < (minY - aabbBias) || (maxY + aabbBias) < posGL.y)
				{
					continue;
				}

				curDist = apEditorUtil.DistanceFromLine(	vPos1GL,
															vPos2GL,
															posGL);

				
				if (curDist < offsetGL)
				{
					if (minEdge == null || curDist < minDist)
					{
						minDist = curDist;
						minEdge = curEdge;
					}
				}

			}
			return minEdge;
		}



		
		private bool _isMeshPivotEdit_Moved = false;
		private Vector2 _mouseDownPos_PivotEdit = Vector2.zero;

		/// <summary>
		/// Mesh - Pin 편집 화면에서의 GUI 입력
		/// </summary>
		/// <param name="tDelta"></param>
		/// <param name="isIgnoredUp"></param>
		public void GUI_Input_PivotEdit(float tDelta, bool isIgnoredUp)
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_Pivot);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_Pivot);
			Vector2 mousePos = _editor.Mouse.Pos;

			apMesh mesh = _editor.Select.Mesh;
			if(mesh == null)
			{
				_isMeshPivotEdit_Moved = false;
				return;
			}

			if (_editor.VertController.Mesh == null || mesh != _editor.VertController.Mesh)
			{
				_editor.VertController.SetMesh(mesh);
			}

			//순서는 Gizmo Transform -> Mouse 위치 체크 -> GizmoUpdate -> 결과 봐서 나머지 처리
			if (!IsMouseInGUI(mousePos))
			{
				_isMeshPivotEdit_Moved = false;
				return;
			}

#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			//Gizmo 업데이트
			_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				if (apEditorUtil.IsMouseInMesh(mousePos, mesh))
				{
					_isMeshPivotEdit_Moved = true;
					_mouseDownPos_PivotEdit = mousePos;
				}
			}
			else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				if (_isMeshPivotEdit_Moved)
				{
					Vector2 posDownW = apGL.GL2World(_mouseDownPos_PivotEdit);
					Vector2 posCurW = apGL.GL2World(_editor.Mouse.Pos);

					Vector2 newPivotPos = mesh._offsetPos - (posCurW - posDownW);
					SetMeshPivot(mesh, newPivotPos);

					_mouseDownPos_PivotEdit = mousePos;
				}
			}
			else
			{
				_isMeshPivotEdit_Moved = false;
			}
		}



		public void SetMeshPivot(apMesh mesh, Vector2 nextOffsetPos)
		{
			//이 mesh를 포함하는 모든 MeshGroup을 찾는다.
			if (Editor == null
				|| Editor._portrait == null
				|| mesh == null)
			{
				return;
			}

			List<apMeshGroup> linkedMeshGroups = new List<apMeshGroup>();

			int nMeshGroups = _editor._portrait._meshGroups != null ? _editor._portrait._meshGroups.Count : 0;

			if (nMeshGroups > 0)
			{
				apMeshGroup meshGroup = null;
				for (int i = 0; i < nMeshGroups; i++)
				{
					meshGroup = _editor._portrait._meshGroups[i];

					//bool isExistMeshTF = meshGroup._childMeshTransforms.Exists(delegate (apTransform_Mesh a)
					//		 {
					//			 return a._mesh == mesh;
					//		 });

					//변경 v1.5.0
					s_FindMeshTF_Mesh = mesh;
					bool isExistMeshTF = meshGroup._childMeshTransforms.Exists(s_FindMeshTFByMesh_Func);

					if (isExistMeshTF)
					{
						//이 Mesh를 사용하는 MeshGroup을 추가한다.
						linkedMeshGroups.Add(meshGroup);
					}
				}
			}
			
			apEditorUtil.SetRecord_MeshAndMeshGroups(	apUndoGroupData.ACTION.MeshEdit_SetPivot, 
														Editor, 
														mesh, 
														linkedMeshGroups, 
														//mesh, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

			Vector2 prevOffset = mesh._offsetPos;
			apMatrix3x3 prevOffsetMatrix = apMatrix3x3.TRS(new Vector2(-prevOffset.x, -prevOffset.y), 0, Vector2.one);
			apMatrix3x3 nextOffsetMatrix = apMatrix3x3.TRS(new Vector2(-nextOffsetPos.x, -nextOffsetPos.y), 0, Vector2.one);


			apMatrix3x3 prevDefaultMatrix = apMatrix3x3.identity;
			//apMatrix nextDefaultMatrix = new apMatrix();

			for (int iMG = 0; iMG < linkedMeshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = linkedMeshGroups[iMG];
				List<apTransform_Mesh> meshTransforms = meshGroup._childMeshTransforms.FindAll(delegate (apTransform_Mesh a)
				{
					return a._mesh == mesh;
				});

				//Mesh를 참조하는 MeshTransform

				for (int iMesh = 0; iMesh < meshTransforms.Count; iMesh++)
				{
					apTransform_Mesh meshTF = meshTransforms[iMesh];
					//prevDefaultMatrix.SetMatrix(meshTF._matrix.MtrxToSpace);
					prevDefaultMatrix.SetMatrix(ref meshTF._matrix._mtrxToSpace);

					float newPosX = prevDefaultMatrix._m00 * (prevOffsetMatrix._m02 - nextOffsetMatrix._m02)
									+ prevDefaultMatrix._m01 * (prevOffsetMatrix._m12 - nextOffsetMatrix._m12)
									+ prevDefaultMatrix._m02;
					float newPosY = prevDefaultMatrix._m10 * (prevOffsetMatrix._m02 - nextOffsetMatrix._m02)
									+ prevDefaultMatrix._m11 * (prevOffsetMatrix._m12 - nextOffsetMatrix._m12)
									+ prevDefaultMatrix._m12;

					//회전, 크기 값은 동일한다.

					//Debug.Log("[" + meshTF._nickName + "] Pos Changed : " + meshTF._matrix._pos + " >> " + newPosX + ", " + newPosY);

					meshTF._matrix.SetPos(newPosX, newPosY, true);

				}

				meshGroup.RefreshForce(true);
			}

			mesh._offsetPos = nextOffsetPos;
			mesh.MakeOffsetPosMatrix();


		}


		private static apMesh s_FindMeshTF_Mesh = null;
		private static Predicate<apTransform_Mesh> s_FindMeshTFByMesh_Func = FUNC_FindMeshTFByMesh;
		private static bool FUNC_FindMeshTFByMesh(apTransform_Mesh a)
		{
			return a._mesh == s_FindMeshTF_Mesh;
		}



		/// <summary>
		/// Mesh의 Pin 편집 화면에서의 GUI 입력 (모드에 따라 분기가 있음)
		/// </summary>
		public void GUI_Input_MeshPinEdit(float tDelta, bool isIgnoredUp)
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_Pin);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_Pin);
			Vector2 mousePos = _editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			//핀 편집 중이라면
			switch (_editor._meshEditMode_Pin_ToolMode)
			{
				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Select:
					{
						//선택 툴 : 기즈모 업데이트
						_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);
					}
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Add:						
					{
						//추가 툴 : 별도의 처리 GUI
						GUI_Input_MeshPinEdit_AddTool(leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.alt);
					}
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Link:
					{
						//연결 툴 : 별도의 처리 GUI
						GUI_Input_MeshPinEdit_LinkTool(leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.alt);
					}
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Test:
					{
						//테스트 툴 : 기즈모 업데이트
						_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);
					}
					break;
			}
		}

		private bool _isMeshPinMovable = false;

		/// <summary>
		/// 메시의 Pin 툴 중 "추가"
		/// </summary>
		private void GUI_Input_MeshPinEdit_AddTool(apMouse.MouseBtnStatus leftBtnStatus, apMouse.MouseBtnStatus rightBtnStatus, Vector2 mousePos, bool isCtrl, bool isAlt)
		{
			if (Event.current.type == EventType.Used)
			{
				return;
			}

			if(Editor.Select.Mesh == null)
			{
				return;
			}

			
			bool isMouseEditable = Event.current.isMouse
									&& IsMouseInGUI(mousePos);

			

			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;

			apMeshPin prevSelectedPin = Editor.Select.MeshPin;
			apMeshPin curPin = null;

			int nPins = 0;
			if (pinGroup != null && pinGroup.NumPins > 0)
			{
				nPins = pinGroup.NumPins;
			}

			
			
			//Mesh 내에서의 좌표
			Vector2 mousePosOnMeshWorld = apGL.GL2World(mousePos) + Editor.Select.Mesh._offsetPos;

			//와이어를 그린다.
			Editor.Select.UpdatePinEditWire(mousePosOnMeshWorld);

			//클릭과 상관없이 Ctrl을 누르면 가장 가까운 걸 UI로 표시한다.
			if (isCtrl)
			{
				//가장 가까운걸 찾자
				float minDist = -1.0f;
				apMeshPin nearestPin = null;

				if (nPins > 0)
				{
					for (int iPin = 0; iPin < nPins; iPin++)
					{
						curPin = pinGroup._pins_All[iPin];
						float distToPin = Vector2.Distance(mousePosOnMeshWorld, curPin._defaultPos);
						if (nearestPin == null || distToPin < minDist)
						{
							nearestPin = curPin;
							minDist = distToPin;
						}
					}
				}

				if (nearestPin != null)
				{
					if (Editor.Select.SnapPin == null || Editor.Select.SnapPin != nearestPin)
					{
						Editor.Select.SelectSnapPin(nearestPin);
					}
				}
				else
				{
					if (Editor.Select.SnapPin != null)
					{
						Editor.Select.UnselectSnapPin();
					}
				}

				Editor.SetRepaint();
			}
			else
			{
				if(Editor.Select.SnapPin != null)
				{
					//스냅되는 핀 숨기기
					Editor.Select.UnselectSnapPin();
					Editor.SetRepaint();
				}
			}


			if(!isMouseEditable)
			{
				return;
			}

			bool isAnyChanged = false;

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				//Debug.Log("IsCtrl : " + isCtrl + " | isAlt : " + isAlt);

				//일단 Down 직후엔 Pressed시 이동이 안될 수 있다.
				//같은거 클릭시 또는 새로 생성시에만 Pressed에서 이동 가능
				_isMeshPinMovable = false;

				//- Ctrl을 누른채로 클릭하면 가까운 Pin을 선택한다.
				//- Ctrl+Alt를 누른채로 클릭하면 Pin의 Tangent를 변경하고 선택한다.
				//- 그냥 클릭하면 위치를 체크해서 Pin을 선택한다.
				//- 선택된게 없으면 Pin을 추가한다. (단, Alt를 누른 상태가 아니어야 한다.)

				bool isAnySelected = false;

				
					

				if (isCtrl)
				{
					//Ctrl을 누른 경우

					//가장 가까운걸 찾자
					float minDist = -1.0f;
					apMeshPin nearestPin = null;

					for (int iPin = 0; iPin < nPins; iPin++)
					{
						curPin = pinGroup._pins_All[iPin];
						float distToPin = Vector2.Distance(mousePosOnMeshWorld, curPin._defaultPos);
						if (nearestPin == null || distToPin < minDist)
						{
							nearestPin = curPin;
							minDist = distToPin;
						}
					}

					if (nearestPin != null)
					{
						//가까운 핀을 선택한다.
						Editor.Select.SelectMeshPin(nearestPin, apGizmos.SELECT_TYPE.New);

						//Alt를 누르고 있다면 Tangent 변경
						if (isAlt)
						{
							apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
														Editor,
														Editor.Select.Mesh,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

							if (nearestPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth)
							{
								nearestPin._tangentType = apMeshPin.TANGENT_TYPE.Sharp;
							}
							else
							{
								nearestPin._tangentType = apMeshPin.TANGENT_TYPE.Smooth;
							}

							pinGroup.Default_UpdateCurves();
							
							isAnyChanged = true;
									
						}
						else
						{
							if (prevSelectedPin != null)
							{
								//Alt를 누르고 있지 않은 상태일때
								//이전 Pin이 선택되어 있었다면,
								//연결을 시도하자

								//링크 가능한지 체크부터 하고
								if (pinGroup.IsPinLinkable(prevSelectedPin, nearestPin))
								{
									apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
												Editor,
												Editor.Select.Mesh,
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

									pinGroup.LinkPins(prevSelectedPin, nearestPin);

									isAnyChanged = true;
								}
							}
						}

						if (prevSelectedPin == nearestPin)
						{
							//똑같은걸 선택시 > Pressed에서 이동 가능
							_isMeshPinMovable = true;
						}
						else
						{
							//다른것 새로 선택시 > Pressed에서 이동 불가
							_isMeshPinMovable = false;
						}

						Editor.SetRepaint();
						isAnySelected = true;
					}
				}				
				else
				{
					//Ctrl을 누르지 않았다면 직접 선택한다.
					apMeshPin clickedPin_Default = null;
					apMeshPin clickedPin_Wide = null;
					float minClickedWideDist = 0.0f;
					float curClickedWideDist = 0.0f;

					VERTEX_CLICK_RESULT clickResult = VERTEX_CLICK_RESULT.None;

					for (int iPin = 0; iPin < nPins; iPin++)
					{
						curPin = pinGroup._pins_All[iPin];
						Vector2 pinPosGL = apGL.World2GL(curPin._defaultPos - Editor.Select.Mesh._offsetPos);

								
						//클릭 체크를 하자
						clickResult = IsPinClickable(ref pinPosGL, ref mousePos, ref curClickedWideDist);
						if(clickResult == VERTEX_CLICK_RESULT.None)
						{
							continue;
						}

						if(clickResult == VERTEX_CLICK_RESULT.DirectClick)
						{
							//정확하게 클릭했다.
							clickedPin_Default = curPin;
						}

						// Wide 범위에서 클릭했다.
						if(clickedPin_Wide == null || curClickedWideDist < minClickedWideDist)
						{
							clickedPin_Wide = curPin;
							minClickedWideDist = curClickedWideDist;
						}
					}
					//클릭한 결과를 반영한다.
					apMeshPin clickedPin = null;
					if(clickedPin_Default != null)		{ clickedPin = clickedPin_Default; }
					else if(clickedPin_Wide != null)	{ clickedPin = clickedPin_Wide; }
					
					if (clickedPin != null)
					{
						Editor.Select.SelectMeshPin(clickedPin, apGizmos.SELECT_TYPE.New);
						isAnySelected = true;

						//Alt를 눌렀다면 Tangent 변환
						if (isAlt)
						{
							apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
													Editor,
													Editor.Select.Mesh,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

							if(clickedPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
							{
								clickedPin._tangentType = apMeshPin.TANGENT_TYPE.Smooth;
							}
							else
							{
								clickedPin._tangentType = apMeshPin.TANGENT_TYPE.Sharp;
							}
						}
						else
						{
							if (prevSelectedPin != null)
							{
								//Alt를 누르고 있지 않은 상태일때
								//이전 Pin이 선택되어 있었다면,
								//연결을 시도하자

								//링크 가능한지 체크부터 하고
								if (pinGroup.IsPinLinkable(prevSelectedPin, clickedPin))
								{
									apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
												Editor,
												Editor.Select.Mesh,
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

									pinGroup.LinkPins(prevSelectedPin, clickedPin);

									isAnyChanged = true;
								}
							}
						}

						isAnyChanged = true;
					}


					if (isAnySelected && !isAlt)
					{
						if (prevSelectedPin == null)
						{
							//선택된게 없었다가 선택하면 바로 이동 가능
							_isMeshPinMovable = true;
						}
						else if (Editor.Select.MeshPin == prevSelectedPin)
						{
							//동일한걸 선택시 Pressed에서 이동 가능
							_isMeshPinMovable = true;
						}
						else
						{
							//다른걸 선택시 Pressed에서 이동 불가
							_isMeshPinMovable = false;
						}
					}
							
					//else
					//{
					//	////빈곳을 클릭했다면
					//	//Editor.Select.UnselectMeshPins();
					//}
					Editor.SetRepaint();
				}


				if (!isAnySelected && !isAlt)
				{
					//선택된게 없다면
					//핀을 추가하자
					int nextUniqueID = Editor.Select.Portrait.MakeUniqueID(apIDManager.TARGET.MeshPin);
					if (nextUniqueID >= 0)
					{
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddPin,
													Editor,
													Editor.Select.Mesh,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.StructChanged);

						if (pinGroup == null)
						{
							pinGroup = new apMeshPinGroup();
							pinGroup._parentMesh = Editor.Select.Mesh;
							Editor.Select.Mesh._pinGroup = pinGroup;
						}

						//추천 크기를 구하자
						float recommendedSize = Editor.Select.Mesh.GetVerticesMaxRange() * 0.4f;
						if(recommendedSize < 100.0f)
						{
							recommendedSize = 100.0f;
						}
						//Size > Range + Fade로 만들자
						float recommendedRange = recommendedSize * 0.6f;
						float recommendedFade = recommendedSize * 0.4f;
						
						//int형으로 만들자 (10단위로 설정하게)
						int intRange = (((int)(recommendedRange + 0.5f)) / 10) * 10;
						int intFade = (((int)(recommendedFade + 0.5f)) / 10) * 10;


						apMeshPin newPin = pinGroup.AddMeshPin(nextUniqueID, mousePosOnMeshWorld, prevSelectedPin, intRange, intFade);
						Editor.Select.SelectMeshPin(newPin, apGizmos.SELECT_TYPE.New);

						pinGroup.Default_UpdateCurves();

						Editor.SetRepaint();
						Editor.OnAnyObjectAddedOrRemoved();

						//새로 추가한건 바로 이동 가능
						//_isMeshPinMovable = true;

						//아니다 바로 이동 불가
						_isMeshPinMovable = false;

						isAnyChanged = true;
					}
				}
					
			}
			else if(leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				//드래그를 하면 선택된 Pin의 위치를 변경한다.
				if(prevSelectedPin != null && _isMeshPinMovable)
				{
					apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin,
													Editor,
													Editor.Select.Mesh,
													//null, 
													true,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					prevSelectedPin._defaultPos = mousePosOnMeshWorld;

					pinGroup.Default_UpdateCurves();

					Editor.SetRepaint();

					isAnyChanged = true;
				}
			}
				
				
			if(rightBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				//우클릭을 누르면
				//- 선택된게 있을 때 : 선택을 해제한다.
				//- 선택된게 없을 때 : 삭제를 하자
					
				if(prevSelectedPin != null)
				{
					//선택된게 있다. > 해제
					Editor.Select.UnselectMeshPins();
				}
				else
				{
					if (pinGroup != null && pinGroup.NumPins > 0)
					{
						nPins = pinGroup.NumPins;

						//선택된게 없다 > 삭제할게 있는지 확인하자
						//1. 점을 먼저 체크한다 (좁은 범위만 + Ctrl 없이)
						bool isAnyPinRemoved = false;
						apMeshPin clickedPin_Removed = null;
						float clickDst = 0.0f;

						VERTEX_CLICK_RESULT clickResult = VERTEX_CLICK_RESULT.None;

						for (int iPin = 0; iPin < nPins; iPin++)
						{
							curPin = pinGroup._pins_All[iPin];
							Vector2 pinPosGL = apGL.World2GL(curPin._defaultPos - Editor.Select.Mesh._offsetPos);

							//클릭 체크를 하자
							clickResult = IsPinClickable(ref pinPosGL, ref mousePos, ref clickDst);
							if (clickResult != VERTEX_CLICK_RESULT.None)
							{
								clickedPin_Removed = curPin;
								break;
							}
						}
						//클릭한 결과를 반영하여 삭제한다.
						if (clickedPin_Removed != null)
						{
							//삭제하자
							pinGroup.RemovePin(clickedPin_Removed);

							//ID를 반납한다. (22.7.12)
							Editor.Select.Portrait.PushUnusedID(apIDManager.TARGET.MeshPin, clickedPin_Removed._uniqueID);


							if (clickedPin_Removed == Editor.Select.MeshPin)
							{
								Editor.Select.UnselectMeshPins();
							}

							isAnyPinRemoved = true;

							isAnyChanged = true;
						}

						//점을 삭제하지 않았다면
						if(!isAnyPinRemoved)
						{
							apMeshPinCurve removedCurve = null;

							//커브를 검색한 후 삭제한다.
							if(pinGroup.NumCurves > 0)
							{
								int nCurve = pinGroup.NumCurves;
								apMeshPinCurve curCurve = null;

								for (int iCurve = 0; iCurve < nCurve; iCurve++)
								{
									curCurve = pinGroup._curves_All[iCurve];

									bool isCurveSelected = IsPinCurveClickable(curCurve, mousePos, Editor.Select.Mesh._offsetPos);
									if(isCurveSelected)
									{
										removedCurve = curCurve;
										break;
									}
								}
							}

							if(removedCurve != null)
							{
								//커브 연결을 삭제하자
								apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
													Editor,
													Editor.Select.Mesh,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

								apMeshPin prevPin = removedCurve._prevPin;
								apMeshPin nextPin = removedCurve._nextPin;

								//연결을 끊고 Refresh
								prevPin._nextPin = null;
								prevPin._nextPinID = -1;
								prevPin._nextCurve = null;

								nextPin._prevPin = null;
								nextPin._prevPinID = -1;
								nextPin._prevCurve = null;

								//전체적으로 다시 계산
								//pinGroup.RefreshPinLinksAll();//연결 다시
								//pinGroup.Default_UpdateCurves();
								pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);

								isAnyChanged = true;
							}
						}
					}
				}
					
					
				Editor.SetRepaint();
			}

			if(isAnyChanged)
			{
				//옵션에 따른 핀-버텍스 가중치 재계산
				Editor.Select.RecalculatePinWeightByOption();
				Editor.SetRepaint();
			}
		}

		
		//개선된 방식의 버텍스 클릭 방식을 활용한다.
		//GL상 핀 크기가 20

		//삭제 v1.4.2 : 옵션에 의해서 선택 범위가 변할 수 있다.
		//private const int PIN_SELECT_RANGE_DEFAULT = 12;
		//private const int PIN_SELECT_RANGE_WIDE = 15;

		/// <summary>
		/// 메시의 핀을 클릭하여 선택할 수 있는지 리턴하는 함수
		/// </summary>
		public VERTEX_CLICK_RESULT IsPinClickable(ref Vector2 pinPos, ref Vector2 mousePos, ref float curWideDist)
		{
			curWideDist = 0.0f;

			if (!IsMouseInGUI(pinPos))
			{
				return VERTEX_CLICK_RESULT.None;
			}

			Vector2 difPos = mousePos - pinPos;
			int difX = (int)(Mathf.Abs(difPos.x) + 0.5f);
			int difY = (int)(Mathf.Abs(difPos.y) + 0.5f);



			//이전
			//if(difX <= PIN_SELECT_RANGE_DEFAULT && difY <= PIN_SELECT_RANGE_DEFAULT)

			//변경 v1.4.2
			float selectRange_Normal = Editor.GUIRenderSettings.PinSelectionRange_Normal;
			float selectRange_Wide = Editor.GUIRenderSettings.PinSelectionRange_Wide;

			
			if(difX <= selectRange_Normal && difY <= selectRange_Normal)
			{
				//좁은 범위에서 클릭했다.
				return VERTEX_CLICK_RESULT.DirectClick;
			}

			//if(difX <= PIN_SELECT_RANGE_WIDE && difY <= PIN_SELECT_RANGE_WIDE)//이전
			if(difX <= selectRange_Wide && difY <= selectRange_Wide)//변경
			{
				//Wide 범위 안에서 클릭했다.
				curWideDist = difPos.sqrMagnitude;
				return VERTEX_CLICK_RESULT.WideClick;	
			}

			return VERTEX_CLICK_RESULT.None;
		}


		private bool IsPinCurveClickable(apMeshPinCurve curve, Vector2 mousePos, Vector2 meshOffsetPos)
		{
			//이전 : 고정값
			//float distBias = 5.0f;
			float distBias = Editor.GUIRenderSettings.EdgeSelectBaseRange;//v1.4.2 : 옵션에 따른 클릭 범위

			if (curve.IsLinear())
			{
				//Linear 타입이라면
				//두개의 선분을 잇는 점을 구한 후 선분과의 거리를 체크한다.
				Vector2 posA_GL = apGL.World2GL(curve._prevPin._defaultPos - meshOffsetPos);
				Vector2 posB_GL = apGL.World2GL(curve._nextPin._defaultPos - meshOffsetPos);

				//AABB 안에 들어가는가
				Vector2 posMin_GL = new Vector2(Mathf.Min(posA_GL.x, posB_GL.x), Mathf.Min(posA_GL.y, posB_GL.y));
				Vector2 posMax_GL = new Vector2(Mathf.Max(posA_GL.x, posB_GL.x), Mathf.Max(posA_GL.y, posB_GL.y));

				if (mousePos.x < posMin_GL.x - distBias || posMax_GL.x + distBias < mousePos.x
					|| mousePos.y < posMin_GL.y - distBias || posMax_GL.y + distBias < mousePos.y)
				{
					//AABB 밖에 있다.
					return false;
				}

				float dist = apEditorUtil.DistanceFromLine(posA_GL, posB_GL, mousePos);

				return (dist < distBias);
			}
			else
			{
				//Linear 타입이 아니라면
				//AABB가 복잡할 수 있다.
				//1. 주요 포인트를 잡아서 AABB를 만든다.
				Vector2 AABBPosW_Min = Vector2.zero;
				Vector2 AABBPosW_Max = Vector2.zero;

				curve.GetCurveAABBPos_Default(ref AABBPosW_Min, ref AABBPosW_Max);

				Vector2 AABBPosGL_1 = apGL.World2GL(AABBPosW_Min - meshOffsetPos);
				Vector2 AABBPosGL_2 = apGL.World2GL(AABBPosW_Max - meshOffsetPos);
				
				Vector2 minPosGL = new Vector2(Mathf.Min(AABBPosGL_1.x, AABBPosGL_2.x), Mathf.Min(AABBPosGL_1.y, AABBPosGL_2.y));
				Vector2 maxPosGL = new Vector2(Mathf.Max(AABBPosGL_1.x, AABBPosGL_2.x), Mathf.Max(AABBPosGL_1.y, AABBPosGL_2.y));

				//Debug.Log("AABB 체크 : 마우스 위치 : " + mousePos + " / Min : " + minPosGL + " ~ Max : " + maxPosGL);

				if (mousePos.x < minPosGL.x - distBias || maxPosGL.x + distBias < mousePos.x
						|| mousePos.y < minPosGL.y - distBias || maxPosGL.y + distBias < mousePos.y)
				{
					//AABB 밖에 있다.
					return false;
				}

				//Debug.Log("AABB안에 들어왔다.");

				//이건 10등분해서 거리 체크를 하자
				int nCheck = 10;
				for (int iCheck = 0; iCheck < nCheck; iCheck++)
				{
					float lerp = (float)iCheck / (float)nCheck;
					float lerpNext = (float)(iCheck + 1) / (float)nCheck;

					Vector2 pos1_GL = apGL.World2GL(curve.GetCurvePos_Default(lerp) - meshOffsetPos);
					Vector2 pos2_GL = apGL.World2GL(curve.GetCurvePos_Default(lerpNext) - meshOffsetPos);

					
					float curDist = apEditorUtil.DistanceFromLine(pos1_GL, pos2_GL, mousePos);

					//Debug.Log("[" +iCheck + "] " + pos1_GL + " > " + pos2_GL + " | Dist : " + curDist);
					if (curDist < distBias)
					{
						return true;
					}
				}
			}
			return false;
		}



		/// <summary>
		/// 메시의 Pin 툴 중 "링크".
		/// Add Tool과 유사하지만, "추가", "삭제", "이동"이 빠진다. 오직 연결만 한다. Tangent 변경은 허용한다.
		/// </summary>
		private void GUI_Input_MeshPinEdit_LinkTool(apMouse.MouseBtnStatus leftBtnStatus, apMouse.MouseBtnStatus rightBtnStatus, Vector2 mousePos, bool isCtrl, bool isAlt)
		{
			if (Event.current.type == EventType.Used)
			{
				return;
			}

			if(Editor.Select.Mesh == null)
			{
				return;
			}

			
			bool isMouseEditable = Event.current.isMouse
									&& IsMouseInGUI(mousePos);

			

			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;

			apMeshPin prevSelectedPin = Editor.Select.MeshPin;
			apMeshPin curPin = null;

			int nPins = 0;
			if (pinGroup != null && pinGroup.NumPins > 0)
			{
				nPins = pinGroup.NumPins;
			}

			
			
			//Mesh 내에서의 좌표
			Vector2 mousePosOnMeshWorld = apGL.GL2World(mousePos) + Editor.Select.Mesh._offsetPos;

			//와이어를 그린다.
			Editor.Select.UpdatePinEditWire(mousePosOnMeshWorld);

			//클릭과 상관없이 Ctrl을 누르면 가장 가까운 걸 UI로 표시한다.
			if (isCtrl)
			{
				//가장 가까운걸 찾자
				float minDist = -1.0f;
				apMeshPin nearestPin = null;

				if (nPins > 0)
				{
					for (int iPin = 0; iPin < nPins; iPin++)
					{
						curPin = pinGroup._pins_All[iPin];
						float distToPin = Vector2.Distance(mousePosOnMeshWorld, curPin._defaultPos);
						if (nearestPin == null || distToPin < minDist)
						{
							nearestPin = curPin;
							minDist = distToPin;
						}
					}
				}

				if (nearestPin != null)
				{
					if (Editor.Select.SnapPin == null || Editor.Select.SnapPin != nearestPin)
					{
						Editor.Select.SelectSnapPin(nearestPin);
					}
				}
				else
				{
					if (Editor.Select.SnapPin != null)
					{
						Editor.Select.UnselectSnapPin();
					}
				}

				Editor.SetRepaint();
			}
			else
			{
				if(Editor.Select.SnapPin != null)
				{
					//스냅되는 핀 숨기기
					Editor.Select.UnselectSnapPin();
					Editor.SetRepaint();
				}
			}


			if(!isMouseEditable)
			{
				return;
			}

			bool isAnyChanged = false;

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				//일단 Down 직후엔 Pressed시 이동이 안될 수 있다.
				//같은거 클릭시 또는 새로 생성시에만 Pressed에서 이동 가능
				_isMeshPinMovable = false;

				//- Ctrl을 누른채로 클릭하면 가까운 Pin을 선택한다.
				//- Ctrl+Alt를 누른채로 클릭하면 Pin의 Tangent를 변경하고 선택한다.
				//- 그냥 클릭하면 위치를 체크해서 Pin을 선택한다.
				//- 선택된게 없으면 Pin을 추가한다. (단, Alt를 누른 상태가 아니어야 한다.)

				
				if (isCtrl)
				{
					//Ctrl을 누른 경우

					//가장 가까운걸 찾자
					float minDist = -1.0f;
					apMeshPin nearestPin = null;

					for (int iPin = 0; iPin < nPins; iPin++)
					{
						curPin = pinGroup._pins_All[iPin];
						float distToPin = Vector2.Distance(mousePosOnMeshWorld, curPin._defaultPos);
						if (nearestPin == null || distToPin < minDist)
						{
							nearestPin = curPin;
							minDist = distToPin;
						}
					}

					if (nearestPin != null)
					{
						//가까운 핀을 선택한다.
						Editor.Select.SelectMeshPin(nearestPin, apGizmos.SELECT_TYPE.New);

						//Alt를 누르고 있다면 Tangent 변경
						if (isAlt)
						{
							apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
														Editor,
														Editor.Select.Mesh,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

							if (nearestPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth)
							{
								nearestPin._tangentType = apMeshPin.TANGENT_TYPE.Sharp;
							}
							else
							{
								nearestPin._tangentType = apMeshPin.TANGENT_TYPE.Smooth;
							}

							pinGroup.Default_UpdateCurves();
							
							isAnyChanged = true;
									
						}
						else
						{
							if (prevSelectedPin != null)
							{
								//Alt를 누르고 있지 않은 상태일때
								//이전 Pin이 선택되어 있었다면,
								//연결을 시도하자

								//링크 가능한지 체크부터 하고
								if (pinGroup.IsPinLinkable(prevSelectedPin, nearestPin))
								{
									apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
													Editor,
													Editor.Select.Mesh,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

									pinGroup.LinkPins(prevSelectedPin, nearestPin);

									isAnyChanged = true;
								}
							}
						}

						Editor.SetRepaint();
					}
				}				
				else
				{
					//Ctrl을 누르지 않았다면 직접 선택한다.
					apMeshPin clickedPin_Default = null;
					apMeshPin clickedPin_Wide = null;
					float minClickedWideDist = 0.0f;
					float curClickedWideDist = 0.0f;

					VERTEX_CLICK_RESULT clickResult = VERTEX_CLICK_RESULT.None;

					for (int iPin = 0; iPin < nPins; iPin++)
					{
						curPin = pinGroup._pins_All[iPin];
						Vector2 pinPosGL = apGL.World2GL(curPin._defaultPos - Editor.Select.Mesh._offsetPos);

								
						//클릭 체크를 하자
						clickResult = IsPinClickable(ref pinPosGL, ref mousePos, ref curClickedWideDist);
						if(clickResult == VERTEX_CLICK_RESULT.None)
						{
							continue;
						}

						if(clickResult == VERTEX_CLICK_RESULT.DirectClick)
						{
							//정확하게 클릭했다.
							clickedPin_Default = curPin;
						}

						// Wide 범위에서 클릭했다.
						if(clickedPin_Wide == null || curClickedWideDist < minClickedWideDist)
						{
							clickedPin_Wide = curPin;
							minClickedWideDist = curClickedWideDist;
						}
					}

					//클릭한 결과를 반영한다.
					apMeshPin clickedPin = null;
					if(clickedPin_Default != null)		{ clickedPin = clickedPin_Default; }
					else if(clickedPin_Wide != null)	{ clickedPin = clickedPin_Wide; }
					
					if (clickedPin != null)
					{
						Editor.Select.SelectMeshPin(clickedPin, apGizmos.SELECT_TYPE.New);
						
						//Alt를 눌렀다면 Tangent 변환
						if (isAlt)
						{
							apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
													Editor,
													Editor.Select.Mesh,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

							if(clickedPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
							{
								clickedPin._tangentType = apMeshPin.TANGENT_TYPE.Smooth;
							}
							else
							{
								clickedPin._tangentType = apMeshPin.TANGENT_TYPE.Sharp;
							}
						}
						else
						{
							//Alt를 누르지 않았다면
							if (prevSelectedPin != null)
							{
								//Alt를 누르고 있지 않은 상태일때
								//이전 Pin이 선택되어 있었다면,
								//연결을 시도하자

								//링크 가능한지 체크부터 하고
								if (pinGroup.IsPinLinkable(prevSelectedPin, clickedPin))
								{
									apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
													Editor,
													Editor.Select.Mesh,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

									pinGroup.LinkPins(prevSelectedPin, clickedPin);

									isAnyChanged = true;
								}
							}
						}

						isAnyChanged = true;
					}

					
					Editor.SetRepaint();
				}	
			}


			if(rightBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				//우클릭을 누르면
				//- 선택된게 있을 때 : 선택을 해제한다.
				//- 선택된게 없을 때 : Curve만 제거한다.
					
				if(prevSelectedPin != null)
				{
					//선택된게 있다. > 해제
					Editor.Select.UnselectMeshPins();
				}
				else
				{
					if (pinGroup != null && pinGroup.NumPins > 0)
					{
						apMeshPinCurve removedCurve = null;

						//커브를 검색한 후 삭제한다.
						if(pinGroup.NumCurves > 0)
						{
							int nCurve = pinGroup.NumCurves;
							apMeshPinCurve curCurve = null;

							for (int iCurve = 0; iCurve < nCurve; iCurve++)
							{
								curCurve = pinGroup._curves_All[iCurve];

								bool isCurveSelected = IsPinCurveClickable(curCurve, mousePos, Editor.Select.Mesh._offsetPos);
								if(isCurveSelected)
								{
									removedCurve = curCurve;
									break;
								}
							}
						}

						if(removedCurve != null)
						{
							//커브 연결을 삭제하자
							apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
												Editor,
												Editor.Select.Mesh,
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

							apMeshPin prevPin = removedCurve._prevPin;
							apMeshPin nextPin = removedCurve._nextPin;

							//연결을 끊고 Refresh
							prevPin._nextPin = null;
							prevPin._nextPinID = -1;
							prevPin._nextCurve = null;

							nextPin._prevPin = null;
							nextPin._prevPinID = -1;
							nextPin._prevCurve = null;

							//전체적으로 다시 계산
							//pinGroup.RefreshPinLinksAll();//연결 다시
							//pinGroup.Default_UpdateCurves();
							pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);

							isAnyChanged = true;
						}
					}
				}	
				Editor.SetRepaint();
			}
			

			if(isAnyChanged)
			{
				//옵션에 따른 핀-버텍스 가중치 재계산
				Editor.Select.RecalculatePinWeightByOption();
				Editor.SetRepaint();
			}
		}



		// GUI 중 Brush 커서를 그리는 경우의 함수
		public void GUI_PrintBrushCursor(apGizmos gizmo)
		{
			float radius = gizmo.BrushRadius;
			apGizmos.BRUSH_COLOR_MODE colorMode = gizmo.BrushColorMode;
			Texture2D image = gizmo.BrushImage;
			float lerp = gizmo.BrushColorLerp;


			//이후
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.Brush);
			//apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.Brush);
			apMouse.MouseBtnStatus midBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Middle, apMouseSet.ACTION.Brush);
			Vector2 mousePos = Editor.Mouse.Pos;

			if (!IsMouseInGUI(mousePos))
			{
				//Editor.Repaint();
				Editor.SetRepaint();
				return;
			}

			if (midBtnStatus == apMouse.MouseBtnStatus.Down ||
				midBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				//Editor.Repaint();
				Editor.SetRepaint();
				return;
			}

			Color colorRelease = Color.black;
			Color colorClick = Color.black;

			//색상은 총 4개(이동 A, B / 좌클릭 A, B)
			//B는 A보다 조금 더 밝은 색
			//등급이 높아질 수록 색상이 진해짐

			//기본 : 노란색 / 주황색
			//증가 : 연두색>녹색 / 하늘색>푸른색
			//감소 : 푸른톤의 보라색>진분홍 / 주황색>붉은색

			switch (colorMode)
			{
				case apGizmos.BRUSH_COLOR_MODE.Default:
					colorRelease = new Color(0.8f, 0.8f, 0.0f, 1.0f);
					colorClick = new Color(1.0f, 0.5f, 0.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Increase_Lv1:
					colorRelease = new Color(0.5f, 1.0f, 0.0f, 1.0f);
					colorClick = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Increase_Lv2:
					colorRelease = new Color(0.3f, 1.0f, 0.0f, 1.0f);
					colorClick = new Color(0.0f, 0.8f, 1.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Increase_Lv3:
					colorRelease = new Color(0.0f, 1.0f, 0.0f, 1.0f);
					colorClick = new Color(0.0f, 0.5f, 1.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Decrease_Lv1:
					colorRelease = new Color(0.8f, 0.0f, 1.0f, 1.0f);
					colorClick = new Color(1.0f, 0.4f, 0.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Decrease_Lv2:
					colorRelease = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					colorClick = new Color(1.0f, 0.2f, 0.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Decrease_Lv3:
					colorRelease = new Color(1.0f, 0.0f, 0.8f, 1.0f);
					colorClick = new Color(1.0f, 0.0f, 0.0f, 1.0f);
					break;

			}

			Color color = Color.yellow;
			float lineWidth = (2.0f * lerp) + (4.0f * (1.0f - lerp));

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down
				|| leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				color = colorClick * (1.0f - lerp) + ((colorClick * 2.0f + Color.white * 0.2f) * lerp);
			}
			else
			{
				color = colorRelease * (1.0f - lerp) + ((colorRelease * 2.0f + Color.white * 0.2f) * lerp);
			}

			color.a = 1.0f;

			//apGL.DrawCircle(apGL.GL2World(mousePos), radius, color, true);
			apGL.DrawBoldCircleGL(mousePos, radius, lineWidth, color, true);
			if (image != null)
			{
				float sqrt2 = 1.0f / 1.414f;
				apGL.DrawTextureGL(image, mousePos + new Vector2(radius * sqrt2 + 16, radius * sqrt2 + 16), 32, 32, Color.grey, 0.0f);
			}

			//Editor.Repaint();
			Editor.SetRepaint();
		}




		//----------------------------------------------------------
		// 버텍스/폴리곤 클릭 체크 함수들
		//----------------------------------------------------------
		
		//추가  20.3.31 : 리깅용 버텍스 클릭 조건은 별도 (크기가 다르다)
		public bool IsVertexClickable_Rigging(apRenderVertex renderVert, Vector2 mousePos)
		{
			Vector2 vertPos = renderVert._pos_GL;
			float clickSize_Half = 6.0f;

			if (_editor._rigViewOption_CircleVert)
			{
				if (renderVert._renderRigWeightParam != null)
				{
					if (renderVert._renderRigWeightParam._nParam == 0)
					{
						//리깅값이 없다.
						clickSize_Half = apGL.RIG_CIRCLE_SIZE_NORIG_CLICK_SIZE * 0.5f;
					}
					else
					{
						clickSize_Half = apGL.RigCircleSize_Clickable * 0.5f;
					}
				}
			}

			if (!IsMouseInGUI(vertPos))
			{
				return false;
			}

			Vector2 difPos = mousePos - vertPos;
			if (Mathf.Abs(difPos.x) < clickSize_Half && Mathf.Abs(difPos.y) < clickSize_Half)
			{
				return true;
			}
			return false;
		}

		//변경 21.10.22 : 좁은 범위/넓은 범위로 버텍스를 선택할 수 있다.
		//삭제 v1.4.2 : 고정값이 아닌 옵션에 따라 결정된다.
		//private const int VERTEX_SELECT_RANGE_DEFAULT = 6;
		//private const int VERTEX_SELECT_RANGE_WIDE = 10;

		/// <summary>버텍스 클릭 결과</summary>
		public enum VERTEX_CLICK_RESULT
		{
			/// <summary>클릭할 수 없다.</summary>
			None,
			/// <summary>바로 버텍스를 클릭했다.</summary>
			DirectClick,
			/// <summary>버텍스를 근처에서 클릭했다. 거리 변수를 이용해서 체크하자</summary>
			WideClick
		}

		public VERTEX_CLICK_RESULT IsVertexClickable(ref Vector2 vertPosGL, ref Vector2 mousePosGL, ref float sqrDist)
		{
			sqrDist = 0.0f;

			if (!IsMouseInGUI(vertPosGL))
			{
				return VERTEX_CLICK_RESULT.None;
			}

			Vector2 difPos = mousePosGL - vertPosGL;
			int difX = (int)(Mathf.Abs(difPos.x) + 0.5f);
			int difY = (int)(Mathf.Abs(difPos.y) + 0.5f);

			//변경 v1.4.2 : 고정값이 아닌 옵션에 따른 선택 범위를 적용한다.
			float selectRange_Normal = _editor.GUIRenderSettings.VertexSelectionRange_Normal;
			float selectRange_Wide = _editor.GUIRenderSettings.VertexSelectionRange_Wide;

			//if (difX > VERTEX_SELECT_RANGE_DEFAULT || difY > VERTEX_SELECT_RANGE_DEFAULT)//이전
			if (difX > selectRange_Normal || difY > selectRange_Normal)//변경
			{
				//기본 거리의 밖에 위치한다.

				//if (difX > VERTEX_SELECT_RANGE_WIDE || difY > VERTEX_SELECT_RANGE_WIDE)//이전
				if (difX > selectRange_Wide || difY > selectRange_Wide)
				{
					//가까이 클릭하지도 못했다.
					sqrDist = 0.0f;
					return VERTEX_CLICK_RESULT.None;//<<클릭 못함
				}
				sqrDist = difPos.sqrMagnitude;

				return VERTEX_CLICK_RESULT.WideClick;//넓은 범위에서 간접 클릭
			}
			
			//정확히 클릭함
			return VERTEX_CLICK_RESULT.DirectClick;
		}




		public bool IsPolygonClickable(apMeshPolygon polygon, Vector2 meshOffsetPos, Vector2 mousePos)
		{
			//Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;
			//Vector2 posGL = apGL.World2GL(vPos);
			//Tri 체크를 해보자
			int nPolygonTris = polygon._tris != null ? polygon._tris.Count : 0;
			if(nPolygonTris == 0)
			{
				return false;
			}

			apMeshTri tri = null;
			apVertex vert0 = null;
			apVertex vert1 = null;
			apVertex vert2 = null;

			for (int iTri = 0; iTri < nPolygonTris; iTri++)
			{
				tri = polygon._tris[iTri];
				vert0 = tri._verts[0];
				vert1 = tri._verts[1];
				vert2 = tri._verts[2];
				Vector2 vPos0 = apGL.World2GL(new Vector2(vert0._pos.x, vert0._pos.y) - meshOffsetPos);
				Vector2 vPos1 = apGL.World2GL(new Vector2(vert1._pos.x, vert1._pos.y) - meshOffsetPos);
				Vector2 vPos2 = apGL.World2GL(new Vector2(vert2._pos.x, vert2._pos.y) - meshOffsetPos);

				if (apEditorUtil.IsPointInTri(mousePos, vPos0, vPos1, vPos2))
				{
					return true;
				}
			}
			return false;

		}

		
		//-----------------------------------------------------------------------------
		// Mesh Group 화면에서의 GUI 편집 함수들
		//-----------------------------------------------------------------------------
		/// <summary>
		/// 메시 그룹의 Setting 탭에서의 GUI 입력
		/// </summary>
		public void GUI_Input_MeshGroup_Setting(float tDelta, bool isIgnoredUp)
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Setting);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Setting);
			Vector2 mousePos = _editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);

		}



		// Bone 탭에서의 GUI 입력 (기즈모 또는 Add Bone 분기 포함)

		//Bone Edit : 이전에 클릭했던 위치
		private bool _boneEdit_isFirstState = true;
		private bool _boneEdit_isMouseClickable = false;
		private bool _boneEdit_isDrawBoneGhost = false;//GUI에서 표시할 Ghost상태의 임시 Bone
		private Vector2 _boneEdit_PrevClickPosW = Vector2.zero;
		private Vector2 _boneEdit_NextGhostBonePosW = Vector2.zero;
		private apBone _boneEdit_PrevSelectedBone = null;



		public bool IsBoneEditGhostBoneDraw { get { return _boneEdit_isDrawBoneGhost; } }
		public Vector2 BoneEditGhostBonePosW_Start { get { return _boneEdit_PrevClickPosW; } }
		public Vector2 BoneEditGhostBonePosW_End { get { return _boneEdit_NextGhostBonePosW; } }
		public apBone BoneEditRollOverBone { get { return _boneEdit_rollOverBone; } }

		private apBone _boneEdit_rollOverBone = null;
		private Vector2 _boneEdit_PrevMousePosWToCheck = Vector2.zero;

		/// <summary>
		/// GUI Input에서 Bone 편집을 하기 전에 모드가 바뀌면 호출해야하는 함수.
		/// 몇가지 변수가 초기화된다.
		/// </summary>
		public void SetBoneEditInit()
		{
			_boneEdit_isFirstState = true;
			_boneEdit_isMouseClickable = false;
			_boneEdit_isDrawBoneGhost = false;
			_boneEdit_PrevClickPosW = Vector2.zero;
			_boneEdit_NextGhostBonePosW = Vector2.zero;

			_boneEdit_rollOverBone = null;
			_boneEdit_PrevMousePosWToCheck = Vector2.zero;

			_boneEdit_PrevSelectedBone = null;
		}

		public void GUI_Input_MeshGroup_Bone(float tDelta, bool isIgnoredUp)
		{
			//본 작업을 하자
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Bone);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Bone);
			Vector2 mousePos = _editor.Mouse.Pos;

			if (!_boneEdit_isMouseClickable)
			{
				if ((leftBtnStatus == apMouse.MouseBtnStatus.Up ||
					leftBtnStatus == apMouse.MouseBtnStatus.Released)
					&&
					(rightBtnStatus == apMouse.MouseBtnStatus.Up ||
					rightBtnStatus == apMouse.MouseBtnStatus.Released))
				{
					_boneEdit_isMouseClickable = true;
				}
			}

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif
			

			apSelection.BONE_EDIT_MODE boneEditMode = _editor.Select.BoneEditMode;
			switch (boneEditMode)
			{
				case apSelection.BONE_EDIT_MODE.None:
					//아무것도 안합니더
					break;

				case apSelection.BONE_EDIT_MODE.SelectOnly:
				case apSelection.BONE_EDIT_MODE.SelectAndTRS:
					//선택 + TRS
					if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						if (_boneEdit_isMouseClickable)
						{
							_editor.GizmoController._isBoneSelect_MovePosReset = true;//클릭시에는 리셋을 해주자
							_boneEdit_isMouseClickable = false;
						}

					}
					_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);
					if (rightBtnStatus == apMouse.MouseBtnStatus.Down && IsMouseInGUI(mousePos))
					{
						if (_boneEdit_isMouseClickable)
						{
							_editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
							_boneEdit_isMouseClickable = false;
						}
					}
					break;

				case apSelection.BONE_EDIT_MODE.Add:
					//"선택된 Bone"을 Parent로 하여 추가하기.
					{
						apBone curSelectedBone = _editor.Select.Bone;
						apMeshGroup curMeshGroup = _editor.Select.MeshGroup;

						if (curMeshGroup == null)
						{
							break;
						}

						bool isMouseInGUI = IsMouseInGUI(mousePos);


						//1) 처음 Add할때 : 클릭한 곳이 Start 포인트 (Parent 여부는 상관없음)
						//2) 2+ Add할때 : 클릭한 곳이 End 포인트. Start -> End로 Bone을 생성하고, End를 Start로 교체.
						//우클릭하면 (1) 상태로 돌아간다.
						//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.


						//처음 추가할때에는 선택된 본을 Parent으로 한다. (Select 모드에서 선택해야함)
						//추가한 이후에는 추가된 본을 Parent로 하여 계속 수행한다.

						//- Add는 여기서 직접 처리하자
						//- "생성 중"일때는 Ghost 본을 GUI에 출력하자

						if (_boneEdit_isFirstState)
						{
							//_boneEdit_isDrawBoneGhost = false;
							//_boneEdit_isDrawBoneGhostOnMouseMove = false;

							//만약 마우스 입력 없이
							//외부에 의해서 Bone을 바꾸었다면
							//-> Parent를 바꾸려고 한 것.
							//Parent가 바뀌었으면 위치를 자동으로 잡아주자.
							if (curSelectedBone != _boneEdit_PrevSelectedBone)
							{
								_boneEdit_PrevSelectedBone = curSelectedBone;
								_boneEdit_isFirstState = false;
								_boneEdit_isMouseClickable = false;

								curSelectedBone.MakeWorldMatrix(false);//<<이건 IK 적용 전이므로 바로 적용 가능
								curSelectedBone.GUIUpdate();
								//Vector3 endPosW = curSelectedBone._shapePointV1_End;
								Vector3 endPosW = curSelectedBone._shapePoint_Calculated_End;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								//_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
								_boneEdit_PrevClickPosW = endPosW;
								_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
							}
							else
							{
								if (_boneEdit_isMouseClickable && isMouseInGUI)
								{

									if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
									{
										//좌클릭
										//1) 처음 Add할때 : 클릭한 곳이 Start 포인트 (Parent 여부는 상관없음)
										//_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
										_boneEdit_isFirstState = false;//두번째 스테이트로 바꾼다.
										_boneEdit_isMouseClickable = false;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										_boneEdit_PrevClickPosW = apGL.GL2World(mousePos);
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
									else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
									{
										//우클릭
										//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.
										Editor.Select.SetBoneEditMode(apSelection.BONE_EDIT_MODE.SelectAndTRS, true);
										Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
										_boneEdit_isMouseClickable = false;
										_boneEdit_isDrawBoneGhost = false;


									}
								}
							}
						}
						else
						{
							_boneEdit_NextGhostBonePosW = apGL.GL2World(mousePos);


							if (curSelectedBone != _boneEdit_PrevSelectedBone)
							{
								_boneEdit_PrevSelectedBone = curSelectedBone;
								_boneEdit_isFirstState = false;
								_boneEdit_isMouseClickable = false;

								curSelectedBone.MakeWorldMatrix(false);
								curSelectedBone.GUIUpdate();
								//Vector3 endPosW = curSelectedBone._shapePointV1_End;
								Vector3 endPosW = curSelectedBone._shapePoint_Calculated_End;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								//_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
								_boneEdit_PrevClickPosW = endPosW;
								_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
							}

							//만약 Ctrl키를 누른다면 각도가 제한된다.
							if (isCtrl)
							{
								if ((_boneEdit_NextGhostBonePosW - _boneEdit_PrevClickPosW).sqrMagnitude > 0.0001f)
								{
									float lineAnlge = Mathf.Atan2(_boneEdit_NextGhostBonePosW.y - _boneEdit_PrevClickPosW.y,
										_boneEdit_NextGhostBonePosW.x - _boneEdit_PrevClickPosW.x) * Mathf.Rad2Deg;

									//Debug.Log("Bone Angle Ctrl : " + lineAnlge);
									float dist = (_boneEdit_NextGhostBonePosW - _boneEdit_PrevClickPosW).magnitude;
									float revSqrt2 = 1.0f / 1.414141f;
									if (lineAnlge < -180 + 22.5f)//Left
									{
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y;
									}
									else if (lineAnlge < -135 + 22.5f)//LB
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (-revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (-revSqrt2 * dist);
									}
									else if (lineAnlge < -90 + 22.5f)//B
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x;
									}
									else if (lineAnlge < -45 + 22.5f)//RB
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (-revSqrt2 * dist);
									}
									else if (lineAnlge < 0 + 22.5f)//R
									{
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y;
									}
									else if (lineAnlge < 45 + 22.5f)//RT
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (revSqrt2 * dist);
									}
									else if (lineAnlge < 90 + 22.5f)//T
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x;
									}
									else if (lineAnlge < 135 + 22.5f)//LT
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (-revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (revSqrt2 * dist);
									}
									else//Left
									{
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y;
									}

									mousePos = apGL.World2GL(_boneEdit_NextGhostBonePosW);

								}
							}



							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//좌클릭
									//2) 2+ Add할때 : 클릭한 곳이 End 포인트. Start -> End로 Bone을 생성하고, End를 Start로 교체.
									Vector2 startPosW = _boneEdit_PrevClickPosW;
									Vector2 endPosW = _boneEdit_NextGhostBonePosW;


									//Distance가 0이면 안된다.
									if (Vector2.Distance(startPosW, endPosW) > 0.00001f)
									{

										apBone newBone = AddBone(curMeshGroup, curSelectedBone);
										
										//설정을 복사하자
										if (curSelectedBone != null)
										{
											newBone._shapeWidth = curSelectedBone._shapeWidth;
											newBone._shapeTaper = curSelectedBone._shapeTaper;
										}
										else
										{
											//마지막으로 편집된 Bone Width를 적용
											if (_editor.Select._isLastBoneShapeWidthChanged)
											{
												newBone._shapeWidth = _editor.Select._lastBoneShapeWidth;
												newBone._shapeTaper = Mathf.Clamp(_editor.Select._lastBoneShapeTaper, 0, 100);
											}
										}

										//변경 20.8.17 : 래핑된 BoneWorldMatrix를 이용한다.
										apBoneWorldMatrix parentWorldMatrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
																					0, Editor._portrait,
																					(curSelectedBone != null ? curSelectedBone._worldMatrix : null),
																					(curMeshGroup._rootRenderUnit != null ? curMeshGroup._rootRenderUnit.WorldMatrixWrap : null)
																					);

										
										//Parent 기준으로 로컬 좌표계를 구한다.
										//변경 v1.4.4 : Ref를 이용
										Vector2 startPosL = Vector2.zero;
										Vector2 endPosL = Vector2.zero;
										parentWorldMatrix.InvMulPoint2(ref startPosL, ref startPosW);
										parentWorldMatrix.InvMulPoint2(ref endPosL, ref endPosW);

										float length = (endPosL - startPosL).magnitude;
										float angle = 0.0f;
										//start -> pos를 +Y로 삼도록 각도를 설정한다.
										if (Vector2.Distance(startPosL, endPosL) == 0.0f)
										{
											angle = -0.0f;
										}
										else
										{
											angle = Mathf.Atan2(endPosL.y - startPosL.y, endPosL.x - startPosL.x) * Mathf.Rad2Deg;
											angle += 90.0f;
										}

										angle += 180.0f;
										angle = apUtil.AngleTo180(angle);


										if (curSelectedBone != null)
										{
											curSelectedBone.LinkRecursive(curSelectedBone._level);

											//현재 본에 Child가 추가되었으므로
											//IK를 설정해주자
											//bool isIKConnectable = false;
											if (curSelectedBone._childBones.Count > 0 && curSelectedBone._optionIK == apBone.OPTION_IK.Disabled)
											{
												curSelectedBone._optionIK = apBone.OPTION_IK.IKSingle;
												curSelectedBone._IKTargetBone = curSelectedBone._childBones[0];
												curSelectedBone._IKNextChainedBone = curSelectedBone._childBones[0];

												curSelectedBone._IKTargetBoneID = curSelectedBone._IKTargetBone._uniqueID;
												curSelectedBone._IKNextChainedBoneID = curSelectedBone._IKNextChainedBone._uniqueID;
											}
										}
										else
										{
											newBone.Link(curMeshGroup, null, Editor._portrait);
										}

										newBone.InitTransform(Editor._portrait);

										newBone._shapeLength = (int)length;

										newBone._defaultMatrix.SetIdentity();
										newBone._defaultMatrix.SetTRS(startPosL, angle, Vector2.one, true);

										newBone.MakeWorldMatrix(false);
										newBone.GUIUpdate(false);


										//Select 에 선택해주자
										Editor.Select.SelectBone(newBone, apSelection.MULTI_SELECT.Main);

										RefreshBoneHierarchy(curMeshGroup);
										RefreshBoneChaining(curMeshGroup);

										Editor.Select.SelectBone(newBone, apSelection.MULTI_SELECT.Main);

										_boneEdit_PrevSelectedBone = Editor.Select.Bone;

										//본 미리보기를 활용하기 위해 생성된 본의 Width를 기록하자 [v1.4.2]
										_editor.Select._lastBoneShapeWidth = newBone._shapeWidth;
										_editor.Select._lastBoneShapeTaper = newBone._shapeTaper;
										_editor.Select._isLastBoneShapeWidthChanged = true;



										Editor.RefreshControllerAndHierarchy(false);
										curMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
										curMeshGroup.RefreshForce();

										//GUI가 바로 출력되면 에러가 있다.
										//다음 Layout까지 출력하지 말도록 제한하자
										Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Meshes, false);
										Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Bones, false);
									}
									//다음을 위해서 마우스 위치 갱신
									//_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
									_boneEdit_PrevClickPosW = apGL.GL2World(mousePos);
									_boneEdit_isMouseClickable = false;

									_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

									Editor.SetRepaint();
								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//우클릭
									//우클릭하면 (1) 상태로 돌아간다.
									_boneEdit_isFirstState = true;
									_boneEdit_isMouseClickable = false;

									_boneEdit_isDrawBoneGhost = false;//Ghost Draw 종료

									Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
									_boneEdit_PrevSelectedBone = Editor.Select.Bone;
									Editor.RefreshControllerAndHierarchy(false);
								}
							}
						}

						//Editor.SetRepaint();
						//Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.
					}


					break;

				case apSelection.BONE_EDIT_MODE.Link:
					//선택 + 2번째 선택으로 Parent 연결
					//(Child -> Parent)
					//연결한 후에는 연결 해제
					//우클릭으로 선택 해제
					{
						apBone curSelectedBone = Editor.Select.Bone;
						apMeshGroup curMeshGroup = Editor.Select.MeshGroup;

						if (curMeshGroup == null)
						{
							break;
						}

						bool isMouseInGUI = IsMouseInGUI(mousePos);

						//1) (현재 선택한 Bone 없이) 처음 Bone을 선택할 때 : 클릭한 Bone의 World Matrix + EndPos의 중점을 시작점으로 삼는다.
						//2) 다음 Bone을 선택할 때 : 
						//            이전에 선택한 Bone -> 지금 선택한 Bone으로 Parent 연결을 시도해본다. 
						//            (실패시 Noti)
						//            본 자체 선택을 Null로 지정

						if (curSelectedBone != _boneEdit_PrevSelectedBone)
						{
							_boneEdit_PrevSelectedBone = curSelectedBone;
							_boneEdit_isFirstState = false;
							_boneEdit_isMouseClickable = false;

							if (curSelectedBone != null)
							{
								curSelectedBone.MakeWorldMatrix(false);
								curSelectedBone.GUIUpdate();
								//Vector2 midPosW = (curSelectedBone._shapePoint_Calculated_End + curSelectedBone._worldMatrix._pos) * 0.5f;
								Vector2 midPosW = (curSelectedBone._shapePoint_Calculated_End + curSelectedBone._worldMatrix.Pos) * 0.5f;//20.8.17 : 래핑

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								//_boneEdit_PrevClickPos = apGL.World2GL(midPosW);
								_boneEdit_PrevClickPosW = midPosW;
								_boneEdit_NextGhostBonePosW = midPosW;
							}
							else
							{
								_boneEdit_isDrawBoneGhost = false;
							}
						}

						if (curSelectedBone == null)
						{
							//이전에 선택한 Bone이 없다.
							//새로 선택을 하자

							_boneEdit_isDrawBoneGhost = false;
							_boneEdit_rollOverBone = null;

							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//좌클릭
									//Bone을 선택할게 있는가
									//<BONE_EDIT> Bone 기본 설정은 자기 자신의 본만 선택할 수 있다. 변경 없음
									List<apBone> boneList = curMeshGroup._boneList_All;
									apBone bone = null;
									for (int i = 0; i < boneList.Count; i++)
									{
										bone = boneList[i];
										if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode, Editor.Select.IsBoneIKRenderable, Editor._boneGUIOption_RenderType))
										{
											//Debug.Log("Selected : " + bone._name);
											Editor.Select.SelectBone(bone, apSelection.MULTI_SELECT.Main);
											break;
										}
									}

									if (Editor.Select.Bone != null)
									{
										//새로 선택을 했다.
										curSelectedBone = Editor.Select.Bone;

										Editor.RefreshControllerAndHierarchy(false);

										//_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
										_boneEdit_isMouseClickable = false;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										//Vector2 midPosW = (curSelectedBone._shapePoint_Calculated_End + curSelectedBone._worldMatrix._pos) * 0.5f;
										Vector2 midPosW = (curSelectedBone._shapePoint_Calculated_End + curSelectedBone._worldMatrix.Pos) * 0.5f;//20.8.17 : 래핑

										_boneEdit_PrevClickPosW = midPosW;
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//우클릭
									//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.
									Editor.Select.SetBoneEditMode(apSelection.BONE_EDIT_MODE.SelectAndTRS, true);
									Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
									_boneEdit_isMouseClickable = false;
									_boneEdit_isDrawBoneGhost = false;
								}
							}
						}
						else
						{
							//이전에 선택한 Bone이 있다.
							//다른 Bone을 선택한 후 Parent 연결을 시도하자.
							//연결 후에는 Link를 종료. (Link 여러번 할게 있나?)

							_boneEdit_isDrawBoneGhost = true;
							Vector2 curMousePosW = apGL.GL2World(mousePos);
							float deltaMousePos = Vector2.Distance(curMousePosW, _boneEdit_PrevMousePosWToCheck);
							_boneEdit_NextGhostBonePosW = curMousePosW;

							if (deltaMousePos > 2.0f)
							{
								_boneEdit_PrevMousePosWToCheck = curMousePosW;

								//다시 "가까운 롤오버된 Bone 찾기"
								List<apBone> boneList = curMeshGroup._boneList_All;
								apBone bone = null;
								_boneEdit_rollOverBone = null;
								for (int i = 0; i < boneList.Count; i++)
								{
									bone = boneList[i];
									if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode, Editor.Select.IsBoneIKRenderable, Editor._boneGUIOption_RenderType))
									{
										_boneEdit_rollOverBone = bone;
										break;
									}
								}
							}

							//여기서 클릭을 하면 Parent를 바꾸고 -> CurSelectBone을 교체하자
							//우클릭시 단순히 선택 Bone 해제
							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//Parent로 이을 Bone을 검색하자
									List<apBone> boneList = curMeshGroup._boneList_All;
									apBone bone = null;
									apBone targetBone = null;
									for (int i = 0; i < boneList.Count; i++)
									{
										bone = boneList[i];
										if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode, Editor.Select.IsBoneIKRenderable, Editor._boneGUIOption_RenderType))
										{
											targetBone = bone;
											break;
										}
									}
									if (targetBone != null)
									{
										//TODO : 가능한지 체크하자
										//Parent를 바꿀때에는
										//targetBone이 재귀적인 Child이면 안된다
										bool isChangeAvailable = true;
										if (curSelectedBone == targetBone)
										{
											isChangeAvailable = false;
										}
										else if (curSelectedBone._parentBone == targetBone)
										{
											isChangeAvailable = false;
										}
										else if (curSelectedBone.GetChildBoneRecursive(targetBone._uniqueID) != null)
										{
											isChangeAvailable = false;
										}

										//가능 여부에 따라서 처리
										if (isChangeAvailable)
										{
											//교체한다.
											SetBoneAsParent(curSelectedBone, targetBone);
											Editor.Notification(curSelectedBone._name + " became a child of " + targetBone._name, true, false);
										}
										else
										{
											//안된다고 에디터 노티로 띄워주자
											Editor.Notification("A Bone that can not be selected as a Parent. Detach first.", true, false);
										}

									}

									if (targetBone != null)
									{
										//처리가 끝났으면 Bone 교체
										Editor.Select.SelectBone(targetBone, apSelection.MULTI_SELECT.Main);

										curSelectedBone.MakeWorldMatrix(false);
										curSelectedBone.GUIUpdate();
										//Vector3 endPosW = curSelectedBone._shapePointV1_End;
										Vector3 endPosW = curSelectedBone._shapePoint_Calculated_End;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										//_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
										_boneEdit_PrevClickPosW = endPosW;
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
									else
									{
										//우클릭 한것과 동일하게 작동
										Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
										_boneEdit_isDrawBoneGhost = false;
										_boneEdit_rollOverBone = null;
									}

									_boneEdit_isMouseClickable = false;

								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									_boneEdit_isMouseClickable = false;
									_boneEdit_isDrawBoneGhost = false;

									Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
									_boneEdit_rollOverBone = null;
								}
							}
						}

						//Editor.SetRepaint();
						//Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.

						//우클릭을 한번 하면 선택 취소.
						//선택 취소된 상태에서 누르면 모드 취소
					}
					break;
			}
		}

		//-----------------------------------------------------------------------------------
		// 모디파이어 입력
		//-----------------------------------------------------------------------------------
		public void GUI_Input_MeshGroup_Modifier(apModifierBase.MODIFIER_TYPE modifierType, float tDelta, bool isIgnoredUp)
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Modifier);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Modifier);
			Vector2 mousePos = _editor.Mouse.Pos;

			if (modifierType == apModifierBase.MODIFIER_TYPE.Base)
			{
				return;
			}

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);
		}



		//-----------------------------------------------------------------------------------
		// 애니메이션 입력
		//-----------------------------------------------------------------------------------
		public void GUI_Input_Animation(float tDelta, bool isIgnoredUp)
		{
			apMouse.MouseBtnStatus leftBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Animation);
			apMouse.MouseBtnStatus rightBtnStatus = _editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Animation);
			Vector2 mousePos = _editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			_editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt, isIgnoredUp);
		}



		//------------------------------------------------------------
		// 왼쪽 GUI (컨트롤 파라미터)
		//------------------------------------------------------------
		//------------------------------------------------------------------------------
		public int GUI_Controller_Upper(int width)
		{
			if (Editor == null)
			{
				return 0;
			}
			if (Editor._portrait == null)
			{
				return 0;
			}

			//레이아웃 변경 [v1.5.0]
			int width_Category_Label = 75;
			int width_Category_Dropdown = width - (width_Category_Label + 7);
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
			GUILayout.Space(4);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Category), apGUILOFactory.I.Width(width_Category_Label));//"Category"

			EditorGUI.BeginChangeCheck();
#if UNITY_2017_3_OR_NEWER
			Editor._curParamCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumFlagsField(apGUIContentWrapper.Empty.Content, Editor._curParamCategory, apGUILOFactory.I.Width(width_Category_Dropdown));
#else
			Editor._curParamCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumMaskPopup(apGUIContentWrapper.Empty.Content, Editor._curParamCategory, apGUILOFactory.I.Width(width_Category_Dropdown));
#endif
			if(EditorGUI.EndChangeCheck())
			{
				apEditorUtil.ReleaseGUIFocus();
				Editor.Mouse.Update_ReleaseForce();//필터 변경시 마우스 Up 이벤트를 강제로 발생시켜야 컨트롤러 UI를 바로 클릭할 수 있다. (22.7.7)
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(2);
			//56 - 15 = 41

			int width_Button = (width - 4) / 2;

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
			GUILayout.Space(4);
			//리셋 기능은 뺀다.

			if (GUILayout.Button(Editor.GetUIWord(UIWORD.SetDefaultAll), apGUILOFactory.I.Width(width_Button)))//"Set Default All"
			{
				//bool isResult = EditorUtility.DisplayDialog("Reset", "Really Set Default Value?", "Set Default All", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamDefaultAll_Title),
																Editor.GetText(TEXT.ControlParamDefaultAll_Body),
																Editor.GetText(TEXT.ControlParamDefaultAll_Okay),
																Editor.GetText(TEXT.Cancel));

				if (isResult)
				{
					List<apControlParam> cParams = Editor.ParamControl._controlParams;
					int nParams = cParams != null ? cParams.Count : 0;
					if(nParams > 0)
					{
						for (int i = 0; i < nParams; i++)
						{
							cParams[i].SetDefault();
						}
					}
					
				}
				Editor.RefreshControllerAndHierarchy(false);
			}
			
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.Snapshot), apGUILOFactory.I.Width(width_Button)))
			{
				apDialog_ControlParamSnapshot.ShowDialog(_editor);
			}
			EditorGUILayout.EndHorizontal();

			//56 + 60 => 41 + 25 = 
			//return 56 + 60;
			//return 41 + 25;//이전
			return 25 + 2 + 25 + 2;//v1.5.0
		}

		public void SetDefaultAllControlParams()
		{
			if (Editor == null || Editor._portrait == null)
			{
				return;
			}

			List<apControlParam> cParams = Editor.ParamControl._controlParams;

			for (int i = 0; i < cParams.Count; i++)
			{
				cParams[i].SetDefault();
			}
		}


		/// <summary>
		/// 추가 v1.5.0 : 컨트롤 파라미터 스냅샷을 적용한다.
		/// </summary>
		public void OnControlParamSnapshotAdapt(	bool isSuccess,
													apPortrait portrait,
													apControlParamValueSnapShot.SnapShotSet selectedSnapShot,
													apDialog_ControlParamSnapshot.SNAPSHOT_ADAPT_METHOD method)
		{
			if(!isSuccess
				|| portrait == null
				|| _editor == null
				|| _editor._portrait == null
				|| _editor._portrait != portrait
				|| selectedSnapShot == null)
			{
				return;
			}

			//FFD 상태 체크
			bool isExecutable = Editor.CheckModalAndExecutable();
			if (!isExecutable)
			{
				//적용 불가
				return;
			}

			//애니메이션 상태 + 키프레임 생성 방식으로 적용한다면
			bool isAnimAndSetKeyframe = false;
			apAnimClip animClip = null;
			apAnimTimeline timeline = null;
			apAnimTimeline curTimeline = null;
			if(method == apDialog_ControlParamSnapshot.SNAPSHOT_ADAPT_METHOD.AddAndSetKeyframe)
			{
				if(_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation
				&& _editor.Select.AnimClip != null)
				{
					animClip = _editor.Select.AnimClip;

					//이 애니메이션에 컨트롤 파라미터에 대한 타임라인이 있는지 여부
					int nTimelines = animClip._timelines != null ? animClip._timelines.Count : 0;
					if(nTimelines > 0)
					{
						for (int iTimeline = 0; iTimeline < nTimelines; iTimeline++)
						{
							curTimeline = animClip._timelines[iTimeline];
							if(curTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
							{
								//컨트롤 파라미터 대상의 타임라인이다.
								timeline = curTimeline;
								break;
							}
						}
					}
					if(timeline != null)
					{
						isAnimAndSetKeyframe = true;
					}
					
				}
			}

			int nCPValues = selectedSnapShot._valueList != null ? selectedSnapShot._valueList.Count : 0;
			if(nCPValues == 0)
			{
				return;
			}

			apControlParamValueSnapShot.ControlParamValue curCPValue = null;
			apControlParam targetCP = null;

			if(!isAnimAndSetKeyframe
				|| animClip == null
				|| timeline == null)
			{
				// [ 미리보기 ]
				for (int i = 0; i < nCPValues; i++)
				{
					curCPValue = selectedSnapShot._valueList[i];
					targetCP = _editor._portrait.GetControlParam(curCPValue._controlParamID);
					if(targetCP == null)
					{
						//컨트롤 파라미터가 없다.
						continue;
					}

					if(targetCP._valueType != curCPValue._savedType)
					{
						//저장된 타입이 다르다.
						continue;
					}

					switch(targetCP._valueType)
					{
						case apControlParam.TYPE.Int:
							targetCP._int_Cur = curCPValue._value_Int;
							break;

						case apControlParam.TYPE.Float:
							targetCP._float_Cur = curCPValue._value_Float;
							break;

						case apControlParam.TYPE.Vector2:
							targetCP._vec2_Cur = curCPValue._value_Vector;
							break;
					}
				}
			}
			else
			{
				// [ 애니메이션에 적용하기 ]
				//Undo를 하자
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_AddKeyframe, Editor,
																	Editor._portrait,
																	animClip._targetMeshGroup,
																	null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly
																	);

				int curFrame = animClip.CurFrame;

				List<apAnimTimelineLayer> refreshLayers = new List<apAnimTimelineLayer>();

				for (int i = 0; i < nCPValues; i++)
				{
					curCPValue = selectedSnapShot._valueList[i];
					targetCP = _editor._portrait.GetControlParam(curCPValue._controlParamID);
					if(targetCP == null)
					{
						//컨트롤 파라미터가 없다.
						continue;
					}

					if(targetCP._valueType != curCPValue._savedType)
					{
						//저장된 타입이 다르다.
						continue;
					}

					//현재 에디터에서 컨트롤 파라미터에 대한 "타임라인" + "타임라인 레이어"가 있다면
					apAnimTimelineLayer targetLayer = timeline.GetTimelineLayer(targetCP);
					if(targetLayer == null)
					{
						//레이어가 없다면 > 미리보기만 적용
						
					}
					else
					{
						//레이어를 찾았다.
						//키프레임을 생성하거나 가져와서 적용한다.
						apAnimKeyframe keyframe = targetLayer.GetKeyframeByFrameIndex(curFrame);
						if(keyframe == null)
						{
							keyframe = AddAnimKeyframe(curFrame, targetLayer, false, false, false, true);
						}
						if(keyframe == null)
						{
							//?? 못찾았당
							continue;
						}

						//키프레임에 스냅샷 값을 적용한다.
						switch (targetCP._valueType)
						{
							case apControlParam.TYPE.Int:
								keyframe._conSyncValue_Int = curCPValue._value_Int;
								break;

							case apControlParam.TYPE.Float:
								keyframe._conSyncValue_Float = curCPValue._value_Float;
								break;

							case apControlParam.TYPE.Vector2:
								keyframe._conSyncValue_Vector2 = curCPValue._value_Vector;
								break;
						}
						
						refreshLayers.Add(targetLayer);
					}

					//미리보기 혹은 실제 키프레임이든 일단 컨트롤 파라미터의 값을 갱신한다.
					switch(targetCP._valueType)
					{
						case apControlParam.TYPE.Int:
							targetCP._int_Cur = curCPValue._value_Int;
							break;

						case apControlParam.TYPE.Float:
							targetCP._float_Cur = curCPValue._value_Float;
							break;

						case apControlParam.TYPE.Vector2:
							targetCP._vec2_Cur = curCPValue._value_Vector;
							break;
					}
				}

				//데이터 갱신을 해보자
				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(animClip));
				Editor.RefreshControllerAndHierarchy(false);

				Editor.RefreshTimelineLayers(	apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, 
												null,
												refreshLayers);

				Editor.Select.AutoRefreshModifierExclusiveEditing();

				
			}

			//FFD 처리가 계속 되고 있었다면
			if(_editor.Gizmos.IsFFDMode && _editor._exModObjOption_UpdateByOtherMod)
			{	
				//FFD 모드 중일때 + 다른 모디파이어의 영향을 받는다면
				//> FFD를 종료한다.
				_editor.Gizmos.RevertFFDTransformForce();
			}


			//에디터 갱신
			_editor.SetRepaint();

		}



		public void GUI_Controller(int width, int height, int scrollY)
		{
			if (Editor == null)
			{
				return;
			}
			if (Editor._portrait == null)
			{
				return;
			}

			apModifierBase modifier = null;
			List<apModifierParamSet> modParamSetList = null;
			apModifierParamSetGroup modParamSetGroup = null;
			
			Dictionary<apControlParam, apModifierParamSetGroup> controlParam2PSGMapping = Editor.Select.ModControlParam2PSGMapping;

			//ControlParam 타입의 Timeline을 작업중인가.
			apAnimTimeline animTimeline = null;
			apAnimTimelineLayer animTimelineLayer = null;
			apAnimKeyframe animKeyframe = null;

			CONTROL_PARAM_UI_EDITING_MODE controlParamEditMode = CONTROL_PARAM_UI_EDITING_MODE.None;

			//현재 레코딩 중인지 체크
			if (Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				modifier = Editor.Select.Modifier;

				if (modifier != null)
				{	
					//[중요] 모디파이어중에서 ControlParam의 영향을 받을 경우 여기서 추가해서 키를 추가할 수 있게 세팅해야 한다.
					switch (modifier.SyncTarget)
					{
						case apModifierParamSetGroup.SYNC_TARGET.Controller:
							{
								controlParamEditMode = CONTROL_PARAM_UI_EDITING_MODE.ModifierEditing;

								modParamSetGroup = Editor.Select.SubEditedParamSetGroup;
								//modParamSetList = Editor.Select.Modifier._paramSetList;
								if (modParamSetGroup != null)
								{
									modParamSetList = modParamSetGroup._paramSetList;
								}
							}
							break;
					}
				}
			}
			else if (Editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				//추가 : Animation 상황에서도 레코딩이 가능하다. 단, isRecording(키 생성 방식)은 아니고
				//그 자체가 Keyframe의 값으로 치환되어야 한다.
				if (Editor.Select.AnimClip != null && Editor.Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
				{
					if (Editor.Select.AnimTimeline != null && Editor.Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						controlParamEditMode = CONTROL_PARAM_UI_EDITING_MODE.AnimEditing;

						animTimeline = Editor.Select.AnimTimeline;
						animTimelineLayer = Editor.Select.AnimTimelineLayer_Main;
						if (animTimelineLayer != null)
						{
							//기존 : 선택한 키프레임
							//if (Editor.Select.AnimKeyframes.Count == 1 && Editor.Select.AnimKeyframe != null)
							//{
							//	animKeyframe = Editor.Select.AnimKeyframe;
							//}

							//변경 : 현재의 키프레임
							//단, 현재 애니메이션 프레임과 같아야 한다.
							animKeyframe = Editor.Select.AnimWorkKeyframe_Main;
						}
					}
				}
			}

			//변경 20.3.19 > 컨트롤 파라미터도 Object Order의 영향으로 정렬되자
			apObjectOrders orders = Editor._portrait._objectOrders;
			int nControlParam_Orders = orders.ControlParams.Count;
			int nControlParam_RawData = Editor.ParamControl._controlParams.Count;
			
			apControlParam curParam = null;

			int iParamGUI = 0;
			if (nControlParam_Orders != nControlParam_RawData)
			{
				//개수가 다르면 문제가 있는 것. 기존 방식대로 하자.
				List<apControlParam> cParams = Editor.ParamControl._controlParams;
				for (int i = 0; i < cParams.Count; i++)
				{
					curParam = cParams[i];
					if ((byte)(curParam._category & Editor._curParamCategory) != 0)
					{	
						GUI_ControlParam(	iParamGUI, curParam, width, height, scrollY,
											controlParamEditMode,
											modifier, controlParam2PSGMapping, modParamSetGroup, modParamSetList,
											animTimelineLayer, animKeyframe);

						GUILayout.Space(10);

						iParamGUI++;
					}
				}
			}
			else
			{
				//개수가 같으면 Order 방식을 이용하자.
				for (int i = 0; i < nControlParam_Orders; i++)
				{
					curParam = orders.ControlParams[i]._linked_ControlParam;
					if(curParam == null)
					{
						continue;
					}
					if ((byte)(curParam._category & Editor._curParamCategory) != 0)
					{
						GUI_ControlParam(	iParamGUI, curParam, width, height, scrollY,
											controlParamEditMode,
											modifier, controlParam2PSGMapping, modParamSetGroup, modParamSetList,
											animTimelineLayer, animKeyframe);
						
						GUILayout.Space(10);

						iParamGUI++;
					}
				}
			}
			

			
		}


		//컨트롤 파라미터의 버튼 상태 <모디파이어를 대상으로 하는 경우>
		private enum CONTROL_PARAM_BTN_STATUS
		{
			/// <summary>현재 모디파이어에 등록되지 않았다. [Add and Make Key 버튼]</summary>
			NotRegistered,
			/// <summary>모디파이어에는 등록되었지만 지금 선택된 상태는 아니다. [Select 버튼]</summary>
			NotSelected,
			/// <summary>편집 중이지만 현재 키 위에 있지 않다. [Make Key 버튼]</summary>
			NotOnKey,
			/// <summary>편집 중이며 키 위에 있다. [Remove 버튼]</summary>
			OnKey,

		}

		private enum CONTROL_PARAM_UI_EDITING_MODE
		{
			None,
			ModifierEditing,
			AnimEditing
		}

		//에디터 좌측 탭의 컨트롤 파라미터 영역
		private void GUI_ControlParam(	int index,
										apControlParam controlParam, int width, int windowHeight, int windowScrollY,
										CONTROL_PARAM_UI_EDITING_MODE editMode,
										
										//MeshGroup-Controll Param Modifier인 경우
										apModifierBase modifier,
										Dictionary<apControlParam, apModifierParamSetGroup> cp2PSGMapping,
										apModifierParamSetGroup curParamSetGroup,
										List<apModifierParamSet> modParamSetList,
										
										//Animation인 경우
										apAnimTimelineLayer animTimelineLayer,
										apAnimKeyframe animKeyframe)
		{
			width -= 10;

			//변경 21.2.9 : 편집 버튼이 사라진 만큼 Label을 더 길게 만들 수 있다.			
			int rightBtnsWidth = editMode == CONTROL_PARAM_UI_EDITING_MODE.ModifierEditing ? 55 : 27;//50 + 5 / 25 + 2
			
			int recordBtnSize = 25;
			int presetIconSize = 32;
			int upperHeight = 42;
			int inputBoxHeight = 20;
			int labelHeight = upperHeight - (inputBoxHeight + 2);
			int labelNameWidth = width - (presetIconSize + 4 + rightBtnsWidth);

			bool isRepaint = false;

			apModifierParamSet recordedKey = null;
			apModifierParamSet prevRecordedKey = Editor.Select.ParamSetOfMod;
			
			bool isCurSelected = false;
			bool isRegistered = false;

			List<apModifierParamSet> recordedKeys = null;

			apModifierParamSetGroup linkedPSG = null;

			if (editMode == CONTROL_PARAM_UI_EDITING_MODE.ModifierEditing)
			{
				//모디파이어 편집 모드에서
				if (cp2PSGMapping != null)
				{
					//연결된 PSG (선택되지 않았어도) 가져오기
					cp2PSGMapping.TryGetValue(controlParam, out linkedPSG);
					if (linkedPSG != null)
					{
						//이 컨트롤 파라미터는 모디파이어에 등록되었다.
						isRegistered = true;
					}
				}

				if (curParamSetGroup != null)
				{
					if (curParamSetGroup._keyControlParam == controlParam)
					{
						isCurSelected = true;
						isRegistered = true;
						linkedPSG = curParamSetGroup;
					}
				}

				if(linkedPSG != null)
				{
					recordedKeys = linkedPSG._paramSetList;
				}
			}
			else if (editMode == CONTROL_PARAM_UI_EDITING_MODE.AnimEditing)
			{
				if (animTimelineLayer != null)
				{
					if (animTimelineLayer._linkedControlParam == controlParam)
					{
						isCurSelected = true;
					}
					else
					{
						animKeyframe = null;//현재 편집 중인 타임라인 레이어가 아니라면, 인자로 들어온 키프레임도 내꺼가 아니넹
					}
				}
			}
			
			apEditor.CONTROL_PARAM_UI_SIZE_OPTION vectorUISizeOption = Editor._controlParamUISizeOption;

			//변경 21.2.19 : 뒤 BG를 여기로 옮김
			int unitHeight = 24;
			int guiHeight = 0;
			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
				case apControlParam.TYPE.Float:
					{
						guiHeight += unitHeight * 3 + 20;
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						if(vectorUISizeOption == apEditor.CONTROL_PARAM_UI_SIZE_OPTION.Large)
						{
							//추가 22.5.19 : Vector UI가 커졌다 
							guiHeight += unitHeight * 9 + 18;
						}
						else
						{
							//기본 크기
							guiHeight += unitHeight * 6 + 18;
						}
						
					}
					break;
			}


			Rect lastRect = GUILayoutUtility.GetLastRect();

			if(index == 0)
			{
				//첫번째인 경우
				lastRect.y += 10;
			}

			lastRect.y += 5;

			Color prevColor = GUI.backgroundColor;

			if ((lastRect.y - windowScrollY) + guiHeight > -10 && lastRect.y - windowScrollY < windowHeight)
			{
				//영역 안에 있을때만 배경을 칠하자
				if (EditorGUIUtility.isProSkin)
				{
					if (isCurSelected)
					{
						GUI.backgroundColor = new Color(1.5f, 1.5f, 1.5f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
					}
				}
				else
				{
					if (isCurSelected)
					{
						GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
					}
				}


				GUI.Box(new Rect(lastRect.x, lastRect.y, width + 20, guiHeight), apStringFactory.I.None);
				GUI.backgroundColor = prevColor;
			}



			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(upperHeight));

			//아이콘
			GUILayout.Box(	Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(controlParam._iconPreset)), 
							apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, 
							apGUILOFactory.I.Width(presetIconSize), apGUILOFactory.I.Height(upperHeight));

			//변경 21.2.9 : 이름과 값을 한번에 표시한다.
			
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(labelNameWidth), apGUILOFactory.I.Height(upperHeight));

			//이름
			EditorGUILayout.LabelField(	controlParam._keyName, 
										apGUIStyleWrapper.I.Label_MiddleLeft,
										apGUILOFactory.I.Width(labelNameWidth), 
										apGUILOFactory.I.Height(labelHeight));

			
			//값 - 타입별로 다르게 만든다.
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(labelNameWidth), apGUILOFactory.I.Height(inputBoxHeight));
			GUILayout.Space(5);

			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					{
						int intNext = controlParam._int_Cur;
						
						intNext = EditorGUILayout.DelayedIntField(controlParam._int_Cur, apGUILOFactory.I.Width(labelNameWidth - 5));
						if (intNext != controlParam._int_Cur)
						{
							//FFD 모드 같은 모달 상태를 체크한다.
							bool isExecutable = Editor.CheckModalAndExecutable();
							if(isExecutable)
							{
								//모달 상태가 아닌 경우에만 컨트롤 파라미터 값을 변경한다.
								controlParam._int_Cur = intNext;
								isRepaint = true;
							}							
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float floatNext = controlParam._float_Cur;

						floatNext = EditorGUILayout.DelayedFloatField(controlParam._float_Cur, apGUILOFactory.I.Width(labelNameWidth - 5));
						floatNext = Mathf.Clamp(floatNext, controlParam._float_Min, controlParam._float_Max);
						if (Mathf.Abs(floatNext - controlParam._float_Cur) > 0.0001f)
						{
							//FFD 모드 같은 모달 상태를 체크한다.
							bool isExecutable = Editor.CheckModalAndExecutable();
							if (isExecutable)
							{
								//모달 상태가 아닌 경우에만 컨트롤 파라미터 값을 변경한다.
								controlParam._float_Cur = floatNext;
								isRepaint = true;
							}
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 vec2Next = controlParam._vec2_Cur;
						
						vec2Next.x = EditorGUILayout.DelayedFloatField(vec2Next.x, apGUILOFactory.I.Width((labelNameWidth - 5) / 2 - 2));
						vec2Next.y = EditorGUILayout.DelayedFloatField(vec2Next.y, apGUILOFactory.I.Width((labelNameWidth - 5) / 2 - 2));

						vec2Next.x = Mathf.Clamp(vec2Next.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						vec2Next.y = Mathf.Clamp(vec2Next.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);

						//여기서 1차로 한번 검사
						if (Mathf.Abs(vec2Next.x - controlParam._vec2_Cur.x) > 0.0001f || Mathf.Abs(vec2Next.y - controlParam._vec2_Cur.y) > 0.0001f)
						{
							//FFD 모드 같은 모달 상태를 체크한다.
							bool isExecutable = Editor.CheckModalAndExecutable();
							if (isExecutable)
							{
								//모달 상태가 아닌 경우에만 컨트롤 파라미터 값을 변경한다.
								controlParam._vec2_Cur = vec2Next;
								isRepaint = true;
							}
						}
					}
					break;
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();



			//오른쪽 버튼들
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(rightBtnsWidth), apGUILOFactory.I.Height(upperHeight));
			
			//여백
			GUILayout.Space(((upperHeight - recordBtnSize) / 2));

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(rightBtnsWidth), apGUILOFactory.I.Height(recordBtnSize));
			
			if (editMode == CONTROL_PARAM_UI_EDITING_MODE.ModifierEditing)
			{
				CONTROL_PARAM_BTN_STATUS btnStatus = CONTROL_PARAM_BTN_STATUS.NotRegistered;
				if(isRegistered)
				{
					//일단 등록은 되었다.
					//Not Registered > Not Selected
					btnStatus = CONTROL_PARAM_BTN_STATUS.NotSelected;

					if(isCurSelected)
					{
						//선택은 되었지만 키 위에 있는지는 모르는 상태
						//Not Selected > Not On Key
						btnStatus = CONTROL_PARAM_BTN_STATUS.NotOnKey;
					}
				}

				// 키 위에 있는지 확인하는 Bias (Int 제외)
				float snapBiasX = 0.0f;
				float snapBiasY = 0.0f;

				//bias는 기본 Snap 크기의 절반이다.
				switch (controlParam._valueType)
				{
					case apControlParam.TYPE.Float:
						snapBiasX = (Mathf.Abs(controlParam._float_Max - controlParam._float_Min) / (float)controlParam._snapSize) * 0.2f;
						break;

					case apControlParam.TYPE.Vector2:
						snapBiasX = (Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) / (float)controlParam._snapSize) * 0.2f;
						snapBiasY = (Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) / (float)controlParam._snapSize) * 0.2f;
						break;



				}

				int nModParamSets = modParamSetList != null ? modParamSetList.Count : 0;
				if (curParamSetGroup != null && nModParamSets > 0)
				{
					//현재 레코딩 키 위에 컨트롤러가 있는지 체크
					apModifierParamSet modParamSet = null;
					for (int iModParamSet = 0; iModParamSet < nModParamSets; iModParamSet++)
					{
						modParamSet = modParamSetList[iModParamSet];
						if (curParamSetGroup._syncTarget != apModifierParamSetGroup.SYNC_TARGET.Controller)
						{
							continue;
						}
						if (curParamSetGroup._keyControlParam != controlParam)
						{
							continue;
						}

						//현재 레코드 키 위에 있는지 체크하자
						if (recordedKey == null)
						{
							switch (controlParam._valueType)
							{
								case apControlParam.TYPE.Int:
									{
										if (controlParam._int_Cur == modParamSet._conSyncValue_Int)
										{
											btnStatus = CONTROL_PARAM_BTN_STATUS.OnKey;
										}
									}
									break;

								case apControlParam.TYPE.Float:
									{
										if (controlParam._float_Cur > modParamSet._conSyncValue_Float - snapBiasX &&
											controlParam._float_Cur < modParamSet._conSyncValue_Float + snapBiasX
											)
										{
											btnStatus = CONTROL_PARAM_BTN_STATUS.OnKey;
										}
									}
									break;

								case apControlParam.TYPE.Vector2:
									{
										//bias는 기본 Snap 크기의 절반이다.
										if (controlParam._vec2_Cur.x > modParamSet._conSyncValue_Vector2.x - snapBiasX &&
											controlParam._vec2_Cur.x < modParamSet._conSyncValue_Vector2.x + snapBiasX &&
											controlParam._vec2_Cur.y > modParamSet._conSyncValue_Vector2.y - snapBiasY &&
											controlParam._vec2_Cur.y < modParamSet._conSyncValue_Vector2.y + snapBiasY
											)
										{
											btnStatus = CONTROL_PARAM_BTN_STATUS.OnKey;
										}
									}
									break;



							}
						}

						if (btnStatus == CONTROL_PARAM_BTN_STATUS.OnKey)
						{
							recordedKey = modParamSet;
							break;
						}
					}
				}

				apModifierParamSetGroup modPSG = Editor.Select.SubEditedParamSetGroup;

				if (modPSG != null)
				{
					if(modPSG._keyControlParam == controlParam
						&& prevRecordedKey != recordedKey)
					{
						//편집 중인 PSG에서 Record Key가 변경되었다면
						if (recordedKey != null)
						{
							//자동으로 선택해주자
							if (modPSG._paramSetList.Contains(recordedKey))
							{
								//만약 현재 Modifier에서 Record 키 작업중이라면
								//현재 ParamSet을 Select에 지정하는 것도 좋겠다.
								Editor.Select.SelectParamSetOfModifier(recordedKey);
							}
						}
						else
						{
							Editor.Select.SelectParamSetOfModifier(null, true);//<<변경 3.31 : ExEdit 모드가 해제되지 않도록 만든다.
							Editor.Hierarchy_MeshGroup.RefreshUnits();
						}

						//string prevKey = prevRecordedKey != null ? prevRecordedKey._conSyncValue_Float.ToString() : "Null";
						//string nextKey = recordedKey != null ? recordedKey._conSyncValue_Float.ToString() : "Null";

						//Debug.Log("Key 변경됨 [" + controlParam._keyName + "] (" + prevKey + " > " + nextKey + ")");//<<TODO : 이게 왜 계속 발생하지??
						//Debug.Log("Ex Mode : " + Editor.Select.ExEditingMode);
						//isDebug = true;//여기서는 편집 모드가 풀리는 문제가 발생하지 않는다.
					}
				}

				//추가 19.12.23
				if(Editor._guiContent_EC_RemoveKey == null)
				{
					Editor._guiContent_EC_RemoveKey = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), "Remove Key");
				}

				//모디파이어에 등록된 상태에서 키를 추가하고자 할 때
				if(Editor._guiContent_EC_MakeKey_Editing == null)
				{
					Editor._guiContent_EC_MakeKey_Editing = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Controller_MakeRecordKey), "Make Key");
				}

				//모디파이어에 아직 등록되지 않은 상태에서 키를 추가하고자 할 때
				if(Editor._guiContent_EC_MakeKey_NotEdit == null)
				{
					Editor._guiContent_EC_MakeKey_NotEdit = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Controller_AddAndRecordKey), "Add to Modifier and Make Key");
				}

				//모디파이어에 등록되었지만 선택되지 않았을 때
				if(Editor._guiContent_EC_Select == null)
				{
					Editor._guiContent_EC_Select = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Controller_Select), "Select Control Parameter");
				}


				//상태에 따라 버튼이 모두 다르다.
				switch (btnStatus)
				{	
					case CONTROL_PARAM_BTN_STATUS.NotRegistered:
						{
							//현재 모디파이어에 등록되지 않았다.
							//[Add and Make Key 버튼]
							if (GUILayout.Button(	Editor._guiContent_EC_MakeKey_NotEdit.Content,
													apGUIStyleWrapper.I.Button_MarginHalf,
													apGUILOFactory.I.Width(recordBtnSize),
													apGUILOFactory.I.Height(recordBtnSize)))
							{
								AddControlParamToModifier(controlParam);

								//추가 : ExEdit 모드가 아니라면, Modifier에 추가할 때 자동으로 ExEdit 상태로 전환
								if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None && Editor.Select.IsExEditable(true))
								{
									Editor.Select.SetModifierExclusiveEditing(apSelection.EX_EDIT.ExOnly_Edit);
								}
							}
						}
						break;

					case CONTROL_PARAM_BTN_STATUS.NotSelected:
						{
							//모디파이어에는 등록되었지만 지금 선택된 상태는 아니다.
							//[Select 버튼]
							if (GUILayout.Button(	Editor._guiContent_EC_Select.Content,
													apGUIStyleWrapper.I.Button_MarginHalf,
													apGUILOFactory.I.Width(recordBtnSize),
													apGUILOFactory.I.Height(recordBtnSize)))
							{
								//해당 PSG를 찾자
								apModifierParamSetGroup targetPSG = null;
								apModifierParamSetGroup curPSG = null;
								if(modifier != null && modifier._paramSetGroup_controller != null)
								{
									if(modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller)
									{
										int nPSGs = modifier._paramSetGroup_controller.Count;
										for (int i = 0; i < nPSGs; i++)
										{
											curPSG = modifier._paramSetGroup_controller[i];
											if (curPSG._keyControlParam == controlParam)
											{
												targetPSG = curPSG;
												break;
											}
										}
									}
								}
								if(targetPSG != null)
								{
									Editor.Select.SelectParamSetGroupOfModifier(targetPSG);
								}
								
								//추가 : ExEdit 모드가 아니라면, Modifier에 추가할 때 자동으로 ExEdit 상태로 전환
								if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None && Editor.Select.IsExEditable(true))
								{
									Editor.Select.SetModifierExclusiveEditing(apSelection.EX_EDIT.ExOnly_Edit);
								}
							}
						}
						break;

					case CONTROL_PARAM_BTN_STATUS.NotOnKey:
						{
							//편집 중이지만 현재 키 위에 있지 않다.
							//[Make Key 버튼]
							if (GUILayout.Button(	Editor._guiContent_EC_MakeKey_Editing.Content,
													apGUIStyleWrapper.I.Button_MarginHalf,
													apGUILOFactory.I.Width(recordBtnSize),
													apGUILOFactory.I.Height(recordBtnSize)))
							{
								AddControlParamToModifier(controlParam);

								//추가 : ExEdit 모드가 아니라면, Modifier에 추가할 때 자동으로 ExEdit 상태로 전환
								if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None && Editor.Select.IsExEditable(true))
								{
									Editor.Select.SetModifierExclusiveEditing(apSelection.EX_EDIT.ExOnly_Edit);
								}
							}
						}
						break;

					case CONTROL_PARAM_BTN_STATUS.OnKey:
						{
							//편집 중이며 키 위에 있다.
							//[Remove 버튼]
							if (GUILayout.Button(	Editor._guiContent_EC_RemoveKey.Content, 
													apGUIStyleWrapper.I.Button_MarginHalf,
													apGUILOFactory.I.Width(recordBtnSize),
													apGUILOFactory.I.Height(recordBtnSize)))
							{
								bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveRecordKey_Title),
																				Editor.GetText(TEXT.RemoveRecordKey_Body),
																				Editor.GetText(TEXT.Remove),
																				Editor.GetText(TEXT.Cancel));
								if (isResult && recordedKey != null)
								{
									RemoveRecordKey(recordedKey);
								}
							}
						}
						break;
				}
			}

			

			//추가 19.12.23
			if(Editor._guiContent_EC_SetDefault == null)
			{
				Editor._guiContent_EC_SetDefault = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Controller_Default), "Set Default");
			}

			//삭제 21.2.9 : 편집 버튼은 삭제한다.
			//if(Editor._guiContent_EC_EditParameter == null)
			//{
			//	Editor._guiContent_EC_EditParameter = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Controller_Edit), "Edit Parameter");
			//}


			//Set Default 버튼
			if (GUILayout.Button(	Editor._guiContent_EC_SetDefault.Content, 
									apGUIStyleWrapper.I.Button_MarginHalf,
									apGUILOFactory.I.Width(recordBtnSize), apGUILOFactory.I.Height(recordBtnSize)))
			{
				controlParam.SetDefault();
				isRepaint = true;
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();//오른쪽 레이아웃의 Vertical 끝



			EditorGUILayout.EndHorizontal();


			Vector2 guiPos = new Vector2(lastRect.x, lastRect.y + 45);

			//변경 v1.4.2 : 컨트롤 파라미터가 변경되었을 경우 FFD를 종료시켜야 하는데,
			//바로 종료시키지 말고 Adapt할지 물어보고 종료시켜야 한다.
			//따라서 Control Param 값을 바로 변경하지 말고 FFD 적용여부 처리 후에 반영하자

			int guiPosYOffset = -3;
#if UNITY_2019_1_OR_NEWER
			guiPosYOffset = -1;
#endif

			

			//변경 21.2.9 : GUI와 Axis Label만 표시하고, 입력 박스는 위로 올라간다. 
			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					{
						int intNext = controlParam._int_Cur;
						
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, apGUILOFactory.I.Width(width / 2 - 5));
						EditorGUILayout.LabelField(controlParam._label_Max, Editor.GUIStyleWrapper.Label_UpperRight, apGUILOFactory.I.Width(width / 2 - 5));
						EditorGUILayout.EndHorizontal();

						//intNext = EditorGUILayout.IntSlider(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
						intNext = apControllerGL.DrawIntSlider(	guiPos + new Vector2(0, unitHeight + guiPosYOffset), width, 
																width + 5, unitHeight + 10,
																controlParam, recordedKeys, recordedKey, animKeyframe);
						intNext = Mathf.Clamp(intNext, controlParam._int_Min, controlParam._int_Max);

						GUILayout.Space(unitHeight);
						if (intNext != controlParam._int_Cur)
						{
							//FFD 모드 같은 모달 상태를 체크한다.
							bool isExecutable = Editor.CheckModalAndExecutable();
							if (isExecutable)
							{
								//모달 상태가 아닌 경우에만 컨트롤 파라미터 값을 변경한다.
								controlParam._int_Cur = intNext;
								isRepaint = true;
							}
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float floatNext = controlParam._float_Cur;
						
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, apGUILOFactory.I.Width(width / 2 - 5));
						EditorGUILayout.LabelField(controlParam._label_Max, Editor.GUIStyleWrapper.Label_UpperRight, apGUILOFactory.I.Width(width / 2 - 5));
						EditorGUILayout.EndHorizontal();

						floatNext = apControllerGL.DrawFloatSlider(	guiPos + new Vector2(0, unitHeight + guiPosYOffset), width, 
																	width + 5, unitHeight + 10,
																	controlParam, 
																	recordedKeys,
																	recordedKey, animKeyframe);

						GUILayout.Space(unitHeight);

						floatNext = Mathf.Clamp(floatNext, controlParam._float_Min, controlParam._float_Max);
						
						if (Mathf.Abs(floatNext - controlParam._float_Cur) > 0.0001f)
						{
							//FFD 모드 같은 모달 상태를 체크한다.
							bool isExecutable = Editor.CheckModalAndExecutable();
							if (isExecutable)
							{
								//모달 상태가 아닌 경우에만 컨트롤 파라미터 값을 변경한다.
								controlParam._float_Cur = floatNext;
								isRepaint = true;
							}
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 vec2Next = controlParam._vec2_Cur;
						
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Max, apGUILOFactory.I.Width(width));
						EditorGUILayout.EndHorizontal();

						//기본 높이값은 (unitHeight * 3)이었다.
						//크기가 커지면 (unitHeight * 6)을 사용한다.
						int uiBaseHeight = (vectorUISizeOption == apEditor.CONTROL_PARAM_UI_SIZE_OPTION.Default ? (unitHeight * 3) : (unitHeight * 6));

						vec2Next = apControllerGL.DrawVector2Slider(	guiPos + new Vector2(0, unitHeight + guiPosYOffset), width, uiBaseHeight, 
																		width + 5, uiBaseHeight + 20,
																		controlParam,
																		recordedKeys,
																		recordedKey, animKeyframe);

						vec2Next.x = Mathf.Clamp(vec2Next.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						vec2Next.y = Mathf.Clamp(vec2Next.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);


						GUILayout.Space(uiBaseHeight - 6);

						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, Editor.GUIStyleWrapper.Label_UpperRight, apGUILOFactory.I.Width(width));
						EditorGUILayout.EndHorizontal();

						GUILayout.Space(8);

						if (Mathf.Abs(vec2Next.x - controlParam._vec2_Cur.x) > 0.0001f || Mathf.Abs(vec2Next.y - controlParam._vec2_Cur.y) > 0.0001f)
						{
							//FFD 모드 같은 모달 상태를 체크한다.
							bool isExecutable = Editor.CheckModalAndExecutable();
							if (isExecutable)
							{
								//모달 상태가 아닌 경우에만 컨트롤 파라미터 값을 변경한다.
								controlParam._vec2_Cur = vec2Next;
								isRepaint = true;
							}
						}
					}
					break;
			}




			//애니메이션 작업 중이라면 => ControlParam의 값을 바로 keyframe
			if (isRepaint 
				&& editMode == CONTROL_PARAM_UI_EDITING_MODE.AnimEditing)
			{
				if (animTimelineLayer != null)
				{
					if (animTimelineLayer._linkedControlParam != controlParam)
					{
						animTimelineLayer = null;
						animKeyframe = null;
					}
				}
				if (animKeyframe == null)
				{
					//키프레임을 못찾은 경우엔 다음의 두가지 경우가 있다.
					//(1) 키프레임이나 타임라인 레이어가 등록되지 않은 경우
					//    > 자동 키 생성 옵션이 있다면 타임라인 레이어와 키프레임을 생성한다.

					//(2) 타임라인 레이어나 키프레임은 있는데 현재 선택된 타임라인이 아니어서 찾지 못했을 경우
					//    > 키프레임을 찾아야 한다.

					//즉, 일단 찾지 못한 타임라인 레이어/키프레임을 찾고, 만약 없다면 자동키 옵션시 생성을 한다.

					//자동 생성 옵션이 있고, 대상 키프레임이 없을 때
					//만약 키프레임/타임라인레이어 중 하나라도 없다면 자동으로 생성을 하자				
					//1. Timeline Layer가 있는가. (Timeline은 이미 등록이 되었어야 한다. 전환은 자동으로 해준다.)
					apAnimClip animClip = Editor.Select.AnimClip;
					apAnimTimeline cpTimeline = Editor.Select.AnimTimeline;

					//기본적으로 애니메이션 클립은 존재해야한다.
					if (animClip != null)
					{
						//컨트롤 파라미터를 선택한 후 자동으로 타임라인을 갱신한다.
						Editor.Select.SelectControlParam_ForAnimEdit(controlParam, true, true, apSelection.MULTI_SELECT.Main);
						
						//선택을 다시 하자
						cpTimeline = Editor.Select.AnimTimeline;
						animTimelineLayer = Editor.Select.AnimTimelineLayer_Main;
						animKeyframe = null;

						//1. Timeline이 유효한지 체크하고, 그렇지 않다면 전환한다.
						bool isTimelineChanged = false;
						if (cpTimeline != null)
						{
							if (cpTimeline._linkType != apAnimClip.LINK_TYPE.ControlParam)
							{
								//타입이 다르다 > 일단 무효
								cpTimeline = null;
							}
						}
						if(cpTimeline == null)
						{
							//유효한 타임라인을 찾자
							int nTimelines = animClip._timelines != null ? animClip._timelines.Count : 0;
							if(nTimelines > 0)
							{
								cpTimeline = animClip._timelines.Find(delegate(apAnimTimeline a)
								{
									return a._linkType == apAnimClip.LINK_TYPE.ControlParam;
								});
							}

							isTimelineChanged = true;
						}

						if(cpTimeline != null)
						{
							//찾았다면
							//일단 타임라인 전환을 하자
							if(isTimelineChanged)
							{
								Editor.Select.SelectAnimTimeline(cpTimeline, false, false, false);
							}

							//이 컨트롤 파라미터에 맞는 타임라인 레이어를 찾자
							if(animTimelineLayer == null)
							{
								//이 컨트롤 파라미터에 해당하는 타임라인을 다시 찾았다.
								animTimelineLayer = cpTimeline.GetTimelineLayer(controlParam);
								
								//타임라인 레이어를 전환한다.
								if(animTimelineLayer != null)
								{
									Editor.Select.SelectAnimTimelineLayer(animTimelineLayer, apSelection.MULTI_SELECT.Main, false, false, false);
								}
							}

							if(animTimelineLayer != null)
							{
								//키프레임 찾기
								animKeyframe = animTimelineLayer.GetKeyframeByFrameIndex(animClip.CurFrame);
								
								if(animKeyframe != null)
								{
									Editor.Select.SelectAnimKeyframe(animKeyframe, false, apGizmos.SELECT_TYPE.New);
								}
							}
						}

						//아직도 키프레임을 찾지 못했다면
						if(animKeyframe == null && Editor._isAnimAutoKey)
						{
							//[자동키 생성] 옵션이 켜진 경우
							if(cpTimeline != null)
							{
								//최소한 타임라인은 생성되어 있어야 한다.
								if(animTimelineLayer == null)
								{
									//1. 타임라인 레이어가 없다면 생성을 한다.
									animTimelineLayer = AddAnimTimelineLayer(controlParam, cpTimeline, true);
								}

								if(animTimelineLayer != null)
								{
									//정상적으로 생성이 되었다면
									//2. 키프레임을 생성한다.
									animKeyframe = AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, animTimelineLayer, true, false, true, true);

									//키프레임을 선택한다.
									if(animKeyframe != null)
									{
										Editor.Select.SelectAnimKeyframe(animKeyframe, false, apGizmos.SELECT_TYPE.New);
									}
								}
							}
						}
					}
				}


				//키프레임이 있다면 컨트롤 파라미터 편집을 한다.
				if (animKeyframe != null)
				{
					//편집이 유효한지 확인해야한다.
					bool isAnimKeyframeEditable = false;
					if(Editor.Select.AnimClip != null
						&& Editor.Select.AnimWorkKeyframe_Main == animKeyframe)
					{
						if(Editor.Select.AnimClip.IsLoop)
						{
							if(Editor.Select.AnimClip.CurFrame == animKeyframe._frameIndex
								|| Editor.Select.AnimClip.CurFrame == animKeyframe._loopFrameIndex)
							{
								isAnimKeyframeEditable = true;
							}
						}
						else
						{
							if(Editor.Select.AnimClip.CurFrame == animKeyframe._frameIndex)
							{
								isAnimKeyframeEditable = true;
							}
						}
					}

					if (isAnimKeyframeEditable)
					{
						//편집이 가능할 때만 변경된 컨트롤 파라미터 값이 적용된다.
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged,
														Editor,
														Editor._portrait,
														//animKeyframe, 
														true,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						switch (controlParam._valueType)
						{
							case apControlParam.TYPE.Int:
								animKeyframe._conSyncValue_Int = controlParam._int_Cur;
								break;

							case apControlParam.TYPE.Float:
								animKeyframe._conSyncValue_Float = controlParam._float_Cur;
								break;

							case apControlParam.TYPE.Vector2:
								animKeyframe._conSyncValue_Vector2 = controlParam._vec2_Cur;
								break;
						}
					}

				}
			}


			if(isRepaint && _editor.Gizmos.IsFFDMode && _editor._exModObjOption_UpdateByOtherMod)
			{
				//Debug.Log("컨트롤 파라미터를 움직여서 FFD 중단");
				//FFD 모드 중일때 + 다른 모디파이어의 영향을 받는다면
				//> FFD를 종료한다.
				_editor.Gizmos.RevertFFDTransformForce();
			}


			if (isRepaint)
			{
				//Editor.Repaint();
				Editor.SetRepaint();
			}
		}


		public void ResetControlParams()
		{
			Editor.ParamControl._controlParams.Clear();
			Editor.ParamControl.MakeReservedParams();
		}


		public void RemoveRecordKey(apModifierParamSet recordedKey)
		{
			bool isResetSelect_ParamSet = false;
			bool isResetSelect_ParamSetGroup = false;
			if (recordedKey == null)
			{
				return;
			}

			if (recordedKey == Editor.Select.ParamSetOfMod)
			{
				isResetSelect_ParamSet = true;
			}

			apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.MeshGroup_RemoveParamSet,
									Editor,
									Editor.Select.MeshGroup,
									Editor.Select.Modifier,
									//recordedKey,
									false,
									apEditorUtil.UNDO_STRUCT.ValueOnly);

			if (Editor.Select.Modifier != null && Editor.Select.SubEditedParamSetGroup != null)
			{
				apModifierParamSetGroup paramSetGroup = Editor.Select.SubEditedParamSetGroup;
				paramSetGroup._paramSetList.Remove(recordedKey);

				//Editor.Select.Modifier._paramSetList.Remove(recordedKey);

				//Link를 하자
				apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier);

				Editor.Select.Modifier.RefreshParamSet(apUtil.LinkRefresh);


				

				Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

				if (!Editor.Select.Modifier._paramSetGroup_controller.Contains(paramSetGroup))
				{
					//그사이에 ParamSetGroup이 사라졌다면
					isResetSelect_ParamSetGroup = true;
				}


			}

			//Select에서 선택중인게 삭제 되었다면..
			if (isResetSelect_ParamSet)
			{
				Editor.Select.SelectParamSetOfModifier(null);
			}
			if (isResetSelect_ParamSetGroup)
			{
				Editor.Select.SelectParamSetGroupOfModifier(null);

			}

			//추가 : ExMode에 추가한다.
			//Editor.Select.RefreshMeshGroupExEditingFlags(
			//					Editor.Select.MeshGroup,
			//					Editor.Select.Modifier,
			//					Editor.Select.SubEditedParamSetGroup,
			//					null,
			//					true);

			//컨트롤 파라미터 > PSG 매핑 갱신
			Editor.Select.RefreshControlParam2PSGMapping();

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();
		}




		/// <summary>
		/// 추가 22.6.10 : 모디파이어로부터 PSG를 삭제한다.
		/// </summary>
		/// <param name="targetParamSetGroup"></param>
		public void RemoveParamSetGroup(apModifierParamSetGroup targetParamSetGroup)
		{
			bool isResetSelect_ParamSetGroup = false;
			if (targetParamSetGroup == null)
			{
				return;
			}

			if (targetParamSetGroup == Editor.Select.SubEditedParamSetGroup)
			{
				isResetSelect_ParamSetGroup = true;
			}

			apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.MeshGroup_RemoveParamSetGroup,
									Editor,
									Editor.Select.MeshGroup,
									Editor.Select.Modifier,
									false,
									apEditorUtil.UNDO_STRUCT.ValueOnly);

			if (Editor.Select.Modifier != null)
			{
				apModifierBase modifier = Editor.Select.Modifier;
				if(modifier._paramSetGroup_controller != null)
				{
					modifier._paramSetGroup_controller.Remove(targetParamSetGroup);
				}

				//Link를 하자
				apUtil.LinkRefresh.Set_MeshGroup_Modifier(Editor.Select.MeshGroup, Editor.Select.Modifier);

				Editor.Select.Modifier.RefreshParamSet(apUtil.LinkRefresh);

				Editor.Select.MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
			}

			//Select에서 선택중인게 삭제 되었다면..
			if (isResetSelect_ParamSetGroup)
			{
				Editor.Select.SelectParamSetOfModifier(null);
				Editor.Select.SelectParamSetGroupOfModifier(null);
			}

			
			//컨트롤 파라미터 > PSG 매핑 갱신
			Editor.Select.RefreshControlParam2PSGMapping();

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(true);

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			ResetVisibilityPresetSync();
		}






		public apHotKey.HotKeyResult OnHotKeyEvent_GizmoSelect(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Select);
			
			return apHotKey.HotKeyResult.MakeResult();
		}
		public apHotKey.HotKeyResult OnHotKeyEvent_GizmoMove(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Move);

			return apHotKey.HotKeyResult.MakeResult();
		}
		public apHotKey.HotKeyResult OnHotKeyEvent_GizmoRotate(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Rotate);

			return apHotKey.HotKeyResult.MakeResult();
		}
		public apHotKey.HotKeyResult OnHotKeyEvent_GizmoScale(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Scale);

			return apHotKey.HotKeyResult.MakeResult();
		}

		public apHotKey.HotKeyResult OnHotKeyEvent_OnionVisibleToggle(object paramObject)
		{
			Editor.Onion.SetVisible(!Editor.Onion.IsVisible);

			//Onion 결과를 전달하자
			return apHotKey.HotKeyResult.MakeResult(Editor.Onion.IsVisible ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}

	}
}