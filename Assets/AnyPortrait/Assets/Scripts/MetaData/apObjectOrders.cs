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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 에디터에서 기본 오브젝트 (RootUnit, Image, Mesh, MeshGroup, Animation, Control Param)의 출력 순서를 알려주는 클래스
	/// 기본적으로 ID+Order의 조합으로만 저장된다.
	/// 동기화 및 정렬이 자동으로 수행된다.
	/// </summary>
	[Serializable]
	public class apObjectOrders
	{
		//Members
		//----------------------------------------------------
		public enum OBJECT_TYPE
		{
			RootUnit = 0,
			Image = 1,
			Mesh = 2,
			MeshGroup = 3,//<<Root만 저장한다.
			AnimClip = 4,
			ControlParam = 5
		}
		[Serializable]
		public class OrderSet
		{
			[SerializeField, NonBackupField]//<백업은 안된다.
			public OBJECT_TYPE _objectType = OBJECT_TYPE.RootUnit;

			[SerializeField, NonBackupField]
			public int _ID = -1;

			[SerializeField, NonBackupField]
			public int _customOrder = -1;

			//실제로 연결된 데이터
			[NonSerialized]
			public int _regOrder = -1;

			[NonSerialized]
			public apRootUnit _linked_RootUnit = null;

			[NonSerialized]
			public apTextureData _linked_Image = null;

			[NonSerialized]
			public apMesh _linked_Mesh = null;

			[NonSerialized]
			public apMeshGroup _linked_MeshGroup = null;

			[NonSerialized]
			public apAnimClip _linked_AnimClip = null;

			[NonSerialized]
			public apControlParam _linked_ControlParam = null;


			[NonSerialized]
			public bool _isExist = false;

			public OrderSet()
			{

			}


			public void SetRootUnit(apRootUnit rootUnit, int regOrder)
			{
				SetData(OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, regOrder);
				_linked_RootUnit = rootUnit;
			}

			public void SetImage(apTextureData image, int regOrder)
			{
				SetData(OBJECT_TYPE.Image, image._uniqueID, regOrder);
				_linked_Image = image;
			}

			public void SetMesh(apMesh mesh, int regOrder)
			{
				SetData(OBJECT_TYPE.Mesh, mesh._uniqueID, regOrder);
				_linked_Mesh = mesh;
			}

			public void SetMeshGroup(apMeshGroup meshGroup, int regOrder)
			{
				SetData(OBJECT_TYPE.MeshGroup, meshGroup._uniqueID, regOrder);
				_linked_MeshGroup = meshGroup;
			}

			public void SetAnimClip(apAnimClip animClip, int regOrder)
			{
				SetData(OBJECT_TYPE.AnimClip, animClip._uniqueID, regOrder);
				_linked_AnimClip = animClip;
			}

			public void SetControlParam(apControlParam controlParam, int regOrder)
			{
				SetData(OBJECT_TYPE.ControlParam, controlParam._uniqueID, regOrder);
				_linked_ControlParam = controlParam;
			}


			private void SetData(OBJECT_TYPE objectType, int ID, int regOrder)
			{
				_objectType = objectType;
				_ID = ID;
				_regOrder = regOrder;
			}

			public void SetOrder(int order)
			{
				_customOrder = order;
			}

			public string Name
			{
				get
				{
					switch (_objectType)
					{
						case OBJECT_TYPE.RootUnit:
							return "RootUnit " + _regOrder;

						case OBJECT_TYPE.Image:
							return (_linked_Image != null) ? _linked_Image._name : "";

						case OBJECT_TYPE.Mesh:
							return (_linked_Mesh != null) ? _linked_Mesh._name : "";

						case OBJECT_TYPE.MeshGroup:
							return (_linked_MeshGroup != null) ? _linked_MeshGroup._name : "";

						case OBJECT_TYPE.AnimClip:
							return (_linked_AnimClip != null) ? _linked_AnimClip._name : "";

						case OBJECT_TYPE.ControlParam:
							return (_linked_ControlParam != null) ? _linked_ControlParam._keyName : "";
					}

					return "";
				}
			}
		}

		//Order 정보가 저장되는 직렬화되는 변수들

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_RootUnit = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_Image = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_Mesh = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_MeshGroup = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_AnimClip = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_ControlParam = new List<OrderSet>();

		[NonSerialized]
		private Dictionary<OBJECT_TYPE, List<OrderSet>> _orderSets = new Dictionary<OBJECT_TYPE, List<OrderSet>>();


		//Sync가 한번이라도 되었는가
		[NonSerialized]
		private bool _isSync = false;


		// Init
		//----------------------------------------------------
		public apObjectOrders()
		{
			if(_orderSets_RootUnit == null)
			{
				_orderSets_RootUnit = new List<OrderSet>();
			}

			if(_orderSets_Image == null)
			{
				_orderSets_Image = new List<OrderSet>();
			}

			if(_orderSets_Mesh == null)
			{
				_orderSets_Mesh = new List<OrderSet>();
			}

			if(_orderSets_MeshGroup == null)
			{
				_orderSets_MeshGroup = new List<OrderSet>();
			}

			if(_orderSets_AnimClip == null)
			{
				_orderSets_AnimClip = new List<OrderSet>();
			}

			if(_orderSets_ControlParam == null)
			{
				_orderSets_ControlParam = new List<OrderSet>();
			}

			if(_orderSets == null)
			{
				_orderSets = new Dictionary<OBJECT_TYPE, List<OrderSet>>();
			}
			_orderSets.Clear();

			_orderSets.Add(OBJECT_TYPE.RootUnit, _orderSets_RootUnit);
			_orderSets.Add(OBJECT_TYPE.Image, _orderSets_Image);
			_orderSets.Add(OBJECT_TYPE.Mesh, _orderSets_Mesh);
			_orderSets.Add(OBJECT_TYPE.MeshGroup, _orderSets_MeshGroup);
			_orderSets.Add(OBJECT_TYPE.AnimClip, _orderSets_AnimClip);
			_orderSets.Add(OBJECT_TYPE.ControlParam, _orderSets_ControlParam);

			_isSync = false;
		}

		public void Clear()
		{
			_orderSets_RootUnit.Clear();
			_orderSets_Image.Clear();
			_orderSets_Mesh.Clear();
			_orderSets_MeshGroup.Clear();
			_orderSets_AnimClip.Clear();
			_orderSets_ControlParam.Clear();

			_orderSets.Clear();

			_orderSets.Add(OBJECT_TYPE.RootUnit, _orderSets_RootUnit);
			_orderSets.Add(OBJECT_TYPE.Image, _orderSets_Image);
			_orderSets.Add(OBJECT_TYPE.Mesh, _orderSets_Mesh);
			_orderSets.Add(OBJECT_TYPE.MeshGroup, _orderSets_MeshGroup);
			_orderSets.Add(OBJECT_TYPE.AnimClip, _orderSets_AnimClip);
			_orderSets.Add(OBJECT_TYPE.ControlParam, _orderSets_ControlParam);

			_isSync = false;
		}

		// Functions
		//----------------------------------------------------
		public void Sync(apPortrait portrait)
		{
			//Debug.Log("Sync : " + portrait.name);
			List<apRootUnit> rootUnits = portrait._rootUnits;
			List<apTextureData> images = portrait._textureData;
			List<apMesh> meshes = portrait._meshes;
			List<apMeshGroup> meshGroups = portrait._meshGroups;
			List<apAnimClip> animClips = portrait._animClips;
			List<apControlParam> controlParams = portrait._controller._controlParams;


			//일단 모두 초기화
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				for (int i = 0; i < orderSets.Count; i++)
				{
					orderSets[i]._isExist = false;//<<플래그를 세운다. (응?)
				}
			}

			OrderSet existOrderSet = null;
			List<OrderSet> newOrderSets = new List<OrderSet>();

			//1. Root Unit
			apRootUnit curRootUnit = null;

			int nRootUnits = rootUnits != null ? rootUnits.Count : 0;

			if (nRootUnits > 0)
			{
				for (int i = 0; i < nRootUnits; i++)
				{
					curRootUnit = rootUnits[i];
					if (curRootUnit == null || curRootUnit._childMeshGroup == null)
					{
						continue;
					}

					//변경 v1.5.0
					s_FindOrderSet_ID = curRootUnit._childMeshGroup._uniqueID;
					existOrderSet = _orderSets_RootUnit.Find(s_FindOrderSet_Func);

					if (existOrderSet != null)
					{
						//이미 등록된 OrderSet이다.
						existOrderSet._isExist = true;
						existOrderSet._regOrder = i;
						existOrderSet._linked_RootUnit = curRootUnit;
					}
					else
					{
						//아직 등록되지 않은 OrderSet이다.
						OrderSet newOrderSet = new OrderSet();
						newOrderSet.SetRootUnit(curRootUnit, i);
						newOrderSets.Add(newOrderSet);
					}
				}
			}
			

			//2. Images
			apTextureData curImage = null;

			int nImages = images != null ? images.Count : 0;
			if (nImages > 0)
			{
				for (int i = 0; i < nImages; i++)
				{
					curImage = images[i];
					if (curImage == null)
					{
						continue;
					}

					//이전 (GC 발생)
					//existOrderSet = _orderSets_Image.Find(delegate(OrderSet a)
					//{
					//	return a._ID == curImage._uniqueID;
					//});

					//변경 v1.5.0
					s_FindOrderSet_ID = curImage._uniqueID;
					existOrderSet = _orderSets_Image.Find(s_FindOrderSet_Func);

					if (existOrderSet != null)
					{
						//이미 등록된 OrderSet이다.
						existOrderSet._isExist = true;
						existOrderSet._regOrder = i;
						existOrderSet._linked_Image = curImage;
					}
					else
					{
						//아직 등록되지 않은 OrderSet이다.
						OrderSet newOrderSet = new OrderSet();
						newOrderSet.SetImage(curImage, i);
						newOrderSets.Add(newOrderSet);
					}
				}
			}
			

			//3. Meshes
			apMesh curMesh = null;

			int nMeshes = meshes != null ? meshes.Count : 0;
			if (nMeshes > 0)
			{
				for (int i = 0; i < nMeshes; i++)
				{
					curMesh = meshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//이전 (GC 발생)
					//existOrderSet = _orderSets_Mesh.Find(delegate(OrderSet a)
					//{
					//	return a._ID == curMesh._uniqueID;
					//});

					//변경 v1.5.0
					s_FindOrderSet_ID = curMesh._uniqueID;
					existOrderSet = _orderSets_Mesh.Find(s_FindOrderSet_Func);


					if (existOrderSet != null)
					{
						//이미 등록된 OrderSet이다.
						existOrderSet._isExist = true;
						existOrderSet._regOrder = i;
						existOrderSet._linked_Mesh = curMesh;
					}
					else
					{
						//아직 등록되지 않은 OrderSet이다.
						OrderSet newOrderSet = new OrderSet();
						newOrderSet.SetMesh(curMesh, i);
						newOrderSets.Add(newOrderSet);
					}
				}
			}
			

			//4. MeshGroup
			apMeshGroup curMeshGroup = null;
			int nMeshGroups = meshGroups != null ? meshGroups.Count : 0;
			if (nMeshGroups > 0)
			{
				for (int i = 0; i < nMeshGroups; i++)
				{
					curMeshGroup = meshGroups[i];
					if (curMeshGroup == null)
					{
						continue;
					}
					//자식 MeshGroup인 경우 처리하지 않는다.
					if (curMeshGroup._parentMeshGroup != null)
					{
						continue;
					}

					//이전 (GC 발생)
					//existOrderSet = _orderSets_MeshGroup.Find(delegate(OrderSet a)
					//{
					//	return a._ID == curMeshGroup._uniqueID;
					//});

					// 변경 v1.5.0
					s_FindOrderSet_ID = curMeshGroup._uniqueID;
					existOrderSet = _orderSets_MeshGroup.Find(s_FindOrderSet_Func);



					if (existOrderSet != null)
					{
						//이미 등록된 OrderSet이다.
						existOrderSet._isExist = true;
						existOrderSet._regOrder = i;
						existOrderSet._linked_MeshGroup = curMeshGroup;
					}
					else
					{
						//아직 등록되지 않은 OrderSet이다.
						OrderSet newOrderSet = new OrderSet();
						newOrderSet.SetMeshGroup(curMeshGroup, i);
						newOrderSets.Add(newOrderSet);
					}
				}
			}
			

			//5. AnimClip
			apAnimClip curAnimClip = null;
			int nAnimClips = animClips != null ? animClips.Count : 0;
			if (nAnimClips > 0)
			{
				for (int i = 0; i < nAnimClips; i++)
				{
					curAnimClip = animClips[i];
					if (curAnimClip == null)
					{
						continue;
					}

					//이전 (GC 발생)
					//existOrderSet = _orderSets_AnimClip.Find(delegate(OrderSet a)
					//{
					//	return a._ID == curAnimClip._uniqueID;
					//});

					//변경 v1.5.0
					s_FindOrderSet_ID = curAnimClip._uniqueID;
					existOrderSet = _orderSets_AnimClip.Find(s_FindOrderSet_Func);




					if (existOrderSet != null)
					{
						//이미 등록된 OrderSet이다.
						existOrderSet._isExist = true;
						existOrderSet._regOrder = i;
						existOrderSet._linked_AnimClip = curAnimClip;
					}
					else
					{
						//아직 등록되지 않은 OrderSet이다.
						OrderSet newOrderSet = new OrderSet();
						newOrderSet.SetAnimClip(curAnimClip, i);
						newOrderSets.Add(newOrderSet);
					}
				}
			}
			

			//6. Control Param
			apControlParam curControlParam = null;
			int nControlParams = controlParams != null ? controlParams.Count : 0;
			if (nControlParams > 0)
			{
				for (int i = 0; i < nControlParams; i++)
				{
					curControlParam = controlParams[i];
					if (curControlParam == null)
					{
						continue;
					}

					//이전 (GC 발생)
					//existOrderSet = _orderSets_ControlParam.Find(delegate(OrderSet a)
					//{
					//	return a._ID == curControlParam._uniqueID;
					//});

					//변경 v1.5.0
					s_FindOrderSet_ID = curControlParam._uniqueID;
					existOrderSet = _orderSets_ControlParam.Find(s_FindOrderSet_Func);


					if (existOrderSet != null)
					{
						//이미 등록된 OrderSet이다.
						existOrderSet._isExist = true;
						existOrderSet._regOrder = i;
						existOrderSet._linked_ControlParam = curControlParam;
					}
					else
					{
						//아직 등록되지 않은 OrderSet이다.
						OrderSet newOrderSet = new OrderSet();
						newOrderSet.SetControlParam(curControlParam, i);
						newOrderSets.Add(newOrderSet);
					}
				}
			}
			

			bool isAnyChanged = false;

			// 연결이 안된 OrderSet 을 삭제
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				//이전 (GC 발생)
				//int nRemoved = orderSets.RemoveAll(delegate(OrderSet a)
				//{
				//	return !a._isExist;
				//});

				//변경 v1.5.0
				int nRemoved = orderSets.RemoveAll(s_RemoveInvalidOrderSet_Func);


				if(nRemoved > 0)
				{
					isAnyChanged = true;
				}
			}

			//새로 추가될 OrderSet을 추가한다. 이때, Order를 잘 체크하자
			if(newOrderSets.Count > 0)
			{
				isAnyChanged = true;

				OrderSet newOrderSet = null;
				List<OrderSet> targetList = null;
				for (int i = 0; i < newOrderSets.Count; i++)
				{
					newOrderSet = newOrderSets[i];
					targetList = _orderSets[newOrderSet._objectType];

					newOrderSet.SetOrder(targetList.Count);//리스트의 크기만큼의 Order값을 넣자

					targetList.Add(newOrderSet);//<<리스트에 추가!
				}
			}

			if(isAnyChanged)
			{
				//Sort를 하고 CustomOrder를 작성하자
				foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
				{
					List<OrderSet> orderSets = subOrderSet.Value;

					//이전 (GC 발생)
					//orderSets.Sort(delegate(OrderSet a, OrderSet b)
					//{
					//	return a._customOrder - b._customOrder;//오름차순
					//});

					//변경 v1.5.0
					orderSets.Sort(s_SortOrderSet_Func);

					for (int i = 0; i < orderSets.Count; i++)
					{
						orderSets[i]._customOrder = i;
					}
				}
			}

			//Debug.Log("Root Units : " + _orderSets[OBJECT_TYPE.RootUnit].Count);
			//Debug.Log("Images : " + _orderSets[OBJECT_TYPE.Image].Count);
			//Debug.Log("Meshes : " + _orderSets[OBJECT_TYPE.Mesh].Count);
			//Debug.Log("Mesh Groups : " + _orderSets[OBJECT_TYPE.MeshGroup].Count);
			//Debug.Log("AnimClips : " + _orderSets[OBJECT_TYPE.AnimClip].Count);
			//Debug.Log("Control Params : " + _orderSets[OBJECT_TYPE.ControlParam].Count);

			_isSync = true;
		}


		private static int s_FindOrderSet_ID = -1;
		private static Predicate<OrderSet> s_FindOrderSet_Func = FUNC_FindOrderSetByID;
		private static bool FUNC_FindOrderSetByID(OrderSet a)
		{
			return a._ID == s_FindOrderSet_ID;
		}

		private static Predicate<OrderSet> s_RemoveInvalidOrderSet_Func = FUNC_RemoveInvalidOrderSet;
		private static bool FUNC_RemoveInvalidOrderSet(OrderSet a)
		{
			return !a._isExist;
		}

		private static Comparison<OrderSet> s_SortOrderSet_Func = FUNC_SortOrderSet;
		private static int FUNC_SortOrderSet(OrderSet a, OrderSet b)
		{
			return a._customOrder - b._customOrder;//오름차순
		}

		private static int s_SwitchTarget_NextOrder = -1;
		private static OrderSet s_SwitchTarget_Target = null;
		private static Predicate<OrderSet> s_SwitchTarget_MeshGroup_Func = FUNC_SwitchTarget_MeshGroup;
		private static bool FUNC_SwitchTarget_MeshGroup(OrderSet a)
		{
			if(a._linked_MeshGroup == null)
			{
				return false;
			}
			if(a._linked_MeshGroup._parentMeshGroup != null)
			{
				return false;
			}
			//MeshGroup이 null이거나 하위 MeshGroup이면 패스

			return a._customOrder == s_SwitchTarget_NextOrder && a != s_SwitchTarget_Target;
		}

		private static Predicate<OrderSet> s_SwitchTarget_Normal_Func = FUNC_SwitchTarget_Normal;
		private static bool FUNC_SwitchTarget_Normal(OrderSet a)
		{
			return a._customOrder == s_SwitchTarget_NextOrder && a != s_SwitchTarget_Target;
		}


		private static Predicate<OrderSet> s_PullTargets_MeshGroup_Func = FUNC_PullTargets_MeshGroup;
		private static bool FUNC_PullTargets_MeshGroup(OrderSet a)
		{
			if(a._linked_MeshGroup == null)
			{
				return false;
			}
			if(a._linked_MeshGroup._parentMeshGroup != null)
			{
				return false;
			}
			//MeshGroup이 null이거나 하위 MeshGroup이면 패스

			//요청된 NextOrder보다 같거나 큰 값을 가졌다면 true 리턴 (밀어야 하므로)
			return a._customOrder >= s_SwitchTarget_NextOrder && a != s_SwitchTarget_Target;
		}


		private static Predicate<OrderSet> s_PullTargets_Normal_Func = FUNC_PullTargets_Normal;
		private static bool FUNC_PullTargets_Normal(OrderSet a)
		{
			//요청된 NextOrder보다 같거나 큰 값을 가졌다면 true 리턴 (밀어야 하므로)
			return a._customOrder >= s_SwitchTarget_NextOrder && a != s_SwitchTarget_Target;
		}

		// Sort
		//---------------------------------------------------------------------
		public void SortByRegOrder()
		{
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				orderSets.Sort(delegate (OrderSet a, OrderSet b)
				{
					return a._regOrder - b._regOrder;
				});
			}

			//Debug.Log("Sort By Reg Order");
		}
		
		public void SortByAlphaNumeric()
		{

			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				orderSets.Sort(delegate (OrderSet a, OrderSet b)
				{
					return string.Compare(a.Name, b.Name);
				});
			}

			//Debug.Log("Sort By AlphaNumeric");
		}

		public void SortByCustom()
		{
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				orderSets.Sort(delegate (OrderSet a, OrderSet b)
				{
					return a._customOrder - b._customOrder;
				});
			}

			//Debug.Log("Sort By Custom");
		}

		// Change Order
		//------------------------------------------------------------
		public bool ChangeOrder(apPortrait portrait, OBJECT_TYPE objectType, int ID, bool isOrderUp)
		{
			//Debug.Log("ChangeOrder : " + objectType + " / " + ID + " / Up : " + isOrderUp);

			//1. 타겟이 있는지 확인
			List<OrderSet> orderSets = _orderSets[objectType];

			//이전 (GC 발생)
			//OrderSet target = orderSets.Find(delegate(OrderSet a)
			//{
			//	return a._ID == ID;
			//});

			//변경 v1.5.0
			s_FindOrderSet_ID = ID;
			OrderSet target = orderSets.Find(s_FindOrderSet_Func);



			if(target == null)
			{
				return false;
			}
			if(objectType == OBJECT_TYPE.MeshGroup)
			{
				//MeshGroup이 없거나 자식 MeshGroup이면 순서를 바꿀 수 없다.
				if(target._linked_MeshGroup == null)
				{
					return false;
				}
				if(target._linked_MeshGroup._parentMeshGroup != null)
				{
					return false;
				}
			}
			
			//Order Up : order 값이 1 줄어든다.
			//Order Down : order 값이 1 증가한다.
			//자리가 바뀔 대상을 찾는다. 
			//단, MeshGroup은 Parent가 없는 것들이어야 한다.

			int prevOrder = target._customOrder;
			int nextOrder = isOrderUp ? (prevOrder - 1) : (prevOrder + 1);

			OrderSet switchTarget = null;
			if(objectType == OBJECT_TYPE.MeshGroup)
			{
				//이전 (GC 발생)
				//switchTarget = orderSets.Find(delegate(OrderSet a)
				//{
				//	if(a._linked_MeshGroup == null)
				//	{
				//		return false;
				//	}
				//	if(a._linked_MeshGroup._parentMeshGroup != null)
				//	{
				//		return false;
				//	}
				//	//MeshGroup이 null이거나 하위 MeshGroup이면 패스

				//	return a._customOrder == nextOrder && a != target;
				//});

				//변경 v1.5.0
				s_SwitchTarget_NextOrder = nextOrder;
				s_SwitchTarget_Target = target;
				switchTarget = orderSets.Find(s_SwitchTarget_MeshGroup_Func);
			}
			else
			{
				//이전 (GC 발생)
				//switchTarget = orderSets.Find(delegate(OrderSet a)
				//{
				//	return a._customOrder == nextOrder && a != target;
				//});

				//변경 v1.5.0
				s_SwitchTarget_NextOrder = nextOrder;
				s_SwitchTarget_Target = target;
				switchTarget = orderSets.Find(s_SwitchTarget_Normal_Func);
			}

			if(switchTarget != null)
			{
				//서로의 Order 값을 바꾼다.
				switchTarget._customOrder = prevOrder;
				target._customOrder = nextOrder;

				//Debug.Log("자리 바꾸기 : " + target.Name + " <-> " + switchTarget.Name);
				
				SortByCustom();

				//만약 RootUnit의 경우라면, Portrait에서의 RootUnit 인덱스를 교환할 필요도 있다.
				if(objectType == OBJECT_TYPE.RootUnit)
				{
					portrait._mainMeshGroupIDList.Clear();
					portrait._rootUnits.Clear();

					for (int i = 0; i < orderSets.Count; i++)
					{
						apRootUnit rootUnit = orderSets[i]._linked_RootUnit;
						portrait._mainMeshGroupIDList.Add(rootUnit._childMeshGroup._uniqueID);
						portrait._rootUnits.Add(rootUnit);
					}
					
					
				}
				return true;
			}
			//else
			//{
			//	Debug.LogError("자리 바꾸기 실패 : " + target.Name);
			//}

			return false;
			
		}



		/// <summary>
		/// 특정 대상(Target)의 순서를 다른 대상(PrevTarget)의 "다음"에 위치시킨다. 복제시 사용됨
		/// </summary>
		/// <returns>실패하면 false 리턴</returns>
		public bool SetOrderToNext(apPortrait portrait, OBJECT_TYPE objectType, int targetID, int prevTargetID)
		{
			//1. 타겟이 있는지 확인
			List<OrderSet> orderSets = _orderSets[objectType];

			s_FindOrderSet_ID = targetID;
			OrderSet target = orderSets.Find(s_FindOrderSet_Func);

			s_FindOrderSet_ID = prevTargetID;
			OrderSet prevTarget = orderSets.Find(s_FindOrderSet_Func);

			if(target == null || prevTarget == null)
			{
				//현재, 이전 대상을 찾지 못했다.
				return false;
			}

			if(objectType == OBJECT_TYPE.MeshGroup)
			{
				//MeshGroup이 없거나 자식 MeshGroup이면 순서를 바꿀 수 없다.
				if(target._linked_MeshGroup == null)
				{
					return false;
				}
				if(target._linked_MeshGroup._parentMeshGroup != null)
				{
					return false;
				}
			}
			
			//Order Up : order 값이 1 줄어든다.
			//Order Down : order 값이 1 증가한다.
			//자리가 바뀔 대상을 찾는다. 
			//단, MeshGroup은 Parent가 없는 것들이어야 한다.

			int prevOrder = prevTarget._customOrder;
			int nextOrder = prevOrder + 1;//Prev의 다음걸로 한다.

			//Next Order과 같거나 큰 Order들은 +1씩 더 증가시킨다.
			List<OrderSet> pullTargets = null;
			if(objectType == OBJECT_TYPE.MeshGroup)
			{
				s_SwitchTarget_NextOrder = nextOrder;
				s_SwitchTarget_Target = target;
				pullTargets = orderSets.FindAll(s_PullTargets_MeshGroup_Func);
			}
			else
			{
				s_SwitchTarget_NextOrder = nextOrder;
				s_SwitchTarget_Target = target;
				pullTargets = orderSets.FindAll(s_PullTargets_Normal_Func);
			}

			//1. Target의 Order 할당
			target._customOrder = nextOrder;

			//2. Pull Target의 Order들은 모두 +1
			int nPullTargets = pullTargets != null ? pullTargets.Count : 0; 
			if(nPullTargets > 0)
			{
				OrderSet orderSet = null;
				for (int i = 0; i < nPullTargets; i++)
				{
					orderSet = pullTargets[i];
					orderSet._customOrder += 1;//1 증가
				}
			}

			//전체 정렬을 한다.
			SortByCustom();

			//만약 RootUnit의 경우라면, Portrait에서의 RootUnit 인덱스를 교환할 필요도 있다.
			if(objectType == OBJECT_TYPE.RootUnit)
			{
				portrait._mainMeshGroupIDList.Clear();
				portrait._rootUnits.Clear();

				for (int i = 0; i < orderSets.Count; i++)
				{
					apRootUnit rootUnit = orderSets[i]._linked_RootUnit;
					portrait._mainMeshGroupIDList.Add(rootUnit._childMeshGroup._uniqueID);
					portrait._rootUnits.Add(rootUnit);
				}
					
					
			}
			return true;
			
		}

		// Get / Set
		//----------------------------------------------------
		public List<OrderSet> RootUnits
		{
			get {  return _orderSets[OBJECT_TYPE.RootUnit]; }
		}

		public List<OrderSet> Images
		{
			get {  return _orderSets[OBJECT_TYPE.Image]; }
		}

		public List<OrderSet> Meshes
		{
			get {  return _orderSets[OBJECT_TYPE.Mesh]; }
		}

		public List<OrderSet> MeshGroups
		{
			get {  return _orderSets[OBJECT_TYPE.MeshGroup]; }
		}

		public List<OrderSet> AnimClips
		{
			get {  return _orderSets[OBJECT_TYPE.AnimClip]; }
		}

		public List<OrderSet> ControlParams
		{
			get {  return _orderSets[OBJECT_TYPE.ControlParam]; }
		}

		public bool IsSync { get { return _isSync; } }
	}
}