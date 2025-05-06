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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	[Serializable]
	public class apModifierStack
	{
		// Members
		//----------------------------------------------------
		//저장되는 Modifier들
		//Serialize는 다형성 저장이 안되어서 타입별로 따로 만들고, 실행중에 부모 클래스 리스트에 합친다.
		[SerializeField]
		private List<apModifier_Volume> _modifiers_Volume = new List<apModifier_Volume>();

		[SerializeField]
		private List<apModifier_Morph> _modifiers_Morph = new List<apModifier_Morph>();

		[SerializeField]
		private List<apModifier_AnimatedMorph> _modifiers_AnimatedMorph = new List<apModifier_AnimatedMorph>();

		[SerializeField]
		private List<apModifier_Rigging> _modifiers_Rigging = new List<apModifier_Rigging>();

		[SerializeField]
		private List<apModifier_Physic> _modifiers_Physic = new List<apModifier_Physic>();

		[SerializeField]
		private List<apModifier_TF> _modifiers_TF = new List<apModifier_TF>();

		[SerializeField]
		private List<apModifier_AnimatedTF> _modifiers_AnimatedTF = new List<apModifier_AnimatedTF>();

		[SerializeField]
		private List<apModifier_FFD> _modifiers_FFD = new List<apModifier_FFD>();

		[SerializeField]
		private List<apModifier_AnimatedFFD> _modifiers_AnimatedFFD = new List<apModifier_AnimatedFFD>();

		//추가 21.7.20 : 색상만 다루는 모디파이어 추가
		[SerializeField]
		private List<apModifier_ColorOnly> _modifiers_ColorOnly = new List<apModifier_ColorOnly>();

		[SerializeField]
		private List<apModifier_AnimatedColorOnly> _modifiers_AnimatedColorOnly = new List<apModifier_AnimatedColorOnly>();


		//실제로 작동하는 Modifier 리스트 (Layer 순서에 맞게 Sort)
		[NonSerialized]
		public List<apModifierBase> _modifiers = new List<apModifierBase>();

		[NonSerialized]
		public apPortrait _parentPortrait = null;

		[NonSerialized]
		public apMeshGroup _parentMeshGroup = null;

		[NonSerialized]
		private bool _isSorted = false;


		// Init
		//----------------------------------------------------
		public apModifierStack()
		{

		}


		// Validate
		//-------------------------------------------------------
		#region [미사용 코드]
		///// <summary>
		///// 유효하지 않은 모디파이어들을 소스 리스트에서 삭제하고, 삭제된 개수를 리턴합니다.
		///// </summary>
		///// <returns></returns>
		//public int RemoveInvalidModifiers()
		//{
		//	int nRemoved = 0;


		//	if (_modifiers_Volume != null)
		//	{	
		//		int curRmv = _modifiers_Volume.RemoveAll(delegate (apModifier_Volume a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_Morph != null)
		//	{
		//		int curRmv = _modifiers_Morph.RemoveAll(delegate (apModifier_Morph a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_AnimatedMorph != null)
		//	{
		//		int curRmv = _modifiers_AnimatedMorph.RemoveAll(delegate (apModifier_AnimatedMorph a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_Rigging != null)
		//	{
		//		int curRmv = _modifiers_Rigging.RemoveAll(delegate (apModifier_Rigging a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_Physic != null)
		//	{
		//		int curRmv = _modifiers_Physic.RemoveAll(delegate (apModifier_Physic a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_TF != null)
		//	{
		//		int curRmv = _modifiers_TF.RemoveAll(delegate (apModifier_TF a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_AnimatedTF != null)
		//	{
		//		int curRmv = _modifiers_AnimatedTF.RemoveAll(delegate (apModifier_AnimatedTF a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_FFD != null)
		//	{
		//		int curRmv = _modifiers_FFD.RemoveAll(delegate (apModifier_FFD a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_AnimatedFFD != null)
		//	{
		//		int curRmv = _modifiers_AnimatedFFD.RemoveAll(delegate (apModifier_AnimatedFFD a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_ColorOnly != null)
		//	{
		//		int curRmv = _modifiers_ColorOnly.RemoveAll(delegate (apModifier_ColorOnly a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	if (_modifiers_AnimatedColorOnly != null)
		//	{
		//		int curRmv = _modifiers_AnimatedColorOnly.RemoveAll(delegate (apModifier_AnimatedColorOnly a) { return a == null; });
		//		nRemoved += curRmv > 0 ? curRmv : 0;
		//	}

		//	//만약, Runtime용 변수인 _modifiers에도 Null이 있다면 일단 여기서 없애자 (RefreshAndSort 함수가 더 적절하다)
		//	if (_modifiers != null)
		//	{
		//		_modifiers.RemoveAll(delegate(apModifierBase a)
		//		{
		//			return a == null;
		//		});
		//	}

		//	//if(nRemoved > 0)
		//	//{
		//	//	Debug.Log("Null 모디파이어 발견 (Invalid 체크에서)");
		//	//}

		//	return nRemoved;
		//} 
		#endregion




		public enum REFRESH_OPTION_ACTIVE
		{
			/// <summary>이 함수를 호출함과 함께 가능한 모디파이어를 Active한다. (기존의 true에 해당)</summary>
			ActiveAllModifierIfPossible,
			/// <summary>Active 여부를 그대로 둔다. (기존의 false에 해당)</summary>
			Keep,
		}

		public enum REFRESH_OPTION_REMOVE
		{
			/// <summary>Null인 모디파이어를 삭제한다. (Undo 체크 필요)</summary>
			RemoveNullModifiers,
			/// <summary>Null인 데이터를 그대로 둔다. (기본값)</summary>
			Ignore,
		}


		//public void RefreshAndSort(bool isSetActiveAllModifier)
		//변경 22.12.13 : 옵션이 하나 더 추가됨
		//(1) 함수 호출후 가능한 모든 모디파이어를 Active로 만들지 여부 (기존 bool 인자)
		//(2) Null 상태의 모디파이어를 소스 리스트에서 완전히 삭제할지 여부
		public void RefreshAndSort(REFRESH_OPTION_ACTIVE activeModOption, REFRESH_OPTION_REMOVE removeNullOption)
		{
			if(_modifiers == null)
			{
				_modifiers = new List<apModifierBase>();
			}
			_modifiers.Clear();

			apModifierBase curMod = null;

			bool isAnyNullMod = false;//Null 데이터가 있는지 감지하자


			int nMod_Volume = _modifiers_Volume != null ? _modifiers_Volume.Count : 0;
			if (nMod_Volume > 0)
			{
				for (int i = 0; i < nMod_Volume; i++)
				{
					curMod = _modifiers_Volume[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_Morph = _modifiers_Morph != null ? _modifiers_Morph.Count : 0;
			if (nMod_Morph > 0)
			{
				for (int i = 0; i < nMod_Morph; i++)
				{
					curMod = _modifiers_Morph[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_AnimMorph = _modifiers_AnimatedMorph != null ? _modifiers_AnimatedMorph.Count : 0;
			if (nMod_AnimMorph > 0)
			{
				for (int i = 0; i < nMod_AnimMorph; i++)
				{
					curMod = _modifiers_AnimatedMorph[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_Rigging = _modifiers_Rigging != null ? _modifiers_Rigging.Count : 0;
			if (nMod_Rigging > 0)
			{
				for (int i = 0; i < nMod_Rigging; i++)
				{
					curMod = _modifiers_Rigging[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_Physic = _modifiers_Physic != null ? _modifiers_Physic.Count : 0;
			if (nMod_Physic > 0)
			{
				for (int i = 0; i < nMod_Physic; i++)
				{
					curMod = _modifiers_Physic[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_TF = _modifiers_TF != null ? _modifiers_TF.Count : 0;
			if (nMod_TF > 0)
			{
				for (int i = 0; i < nMod_TF; i++)
				{
					curMod = _modifiers_TF[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_AnimTF = _modifiers_AnimatedTF != null ? _modifiers_AnimatedTF.Count : 0;
			if (nMod_AnimTF > 0)
			{
				for (int i = 0; i < nMod_AnimTF; i++)
				{
					curMod = _modifiers_AnimatedTF[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_FFD = _modifiers_FFD != null ? _modifiers_FFD.Count : 0;
			if (nMod_FFD > 0)
			{
				for (int i = 0; i < nMod_FFD; i++)
				{
					curMod = _modifiers_FFD[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_AnimFFD = _modifiers_AnimatedFFD != null ? _modifiers_AnimatedFFD.Count : 0;
			if (nMod_AnimFFD > 0)
			{
				for (int i = 0; i < nMod_AnimFFD; i++)
				{
					curMod = _modifiers_AnimatedFFD[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}


			//추가 21.7.20 : 색상 전용 모디파이어
			int nMod_ColorOnly = _modifiers_ColorOnly != null ? _modifiers_ColorOnly.Count : 0;
			if (nMod_ColorOnly > 0)
			{
				for (int i = 0; i < nMod_ColorOnly; i++)
				{
					curMod = _modifiers_ColorOnly[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			int nMod_AnimColorOnly = _modifiers_AnimatedColorOnly != null ? _modifiers_AnimatedColorOnly.Count : 0;
			if (nMod_AnimColorOnly > 0)
			{
				for (int i = 0; i < nMod_AnimColorOnly; i++)
				{
					curMod = _modifiers_AnimatedColorOnly[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}
			

			//Null 데이터가 발견되었다면 옵션에 따라 삭제하자
			if(isAnyNullMod 
				&& removeNullOption == REFRESH_OPTION_REMOVE.RemoveNullModifiers)
			{
				//이전 (GC 발생)
				//if(_modifiers_Volume != null)				{ _modifiers_Volume.RemoveAll(delegate(apModifier_Volume a) { return a == null; }); }
				//if(_modifiers_Morph != null)				{ _modifiers_Morph.RemoveAll(delegate(apModifier_Morph a) { return a == null; }); }
				//if(_modifiers_AnimatedMorph != null)		{ _modifiers_AnimatedMorph.RemoveAll(delegate(apModifier_AnimatedMorph a) { return a == null; }); }
				//if(_modifiers_Rigging != null)				{ _modifiers_Rigging.RemoveAll(delegate(apModifier_Rigging a) { return a == null; }); }
				//if(_modifiers_Physic != null)				{ _modifiers_Physic.RemoveAll(delegate(apModifier_Physic a) { return a == null; }); }
				//if(_modifiers_TF != null)					{ _modifiers_TF.RemoveAll(delegate(apModifier_TF a) { return a == null; }); }
				//if(_modifiers_AnimatedTF != null)			{ _modifiers_AnimatedTF.RemoveAll(delegate(apModifier_AnimatedTF a) { return a == null; }); }
				//if(_modifiers_FFD != null)					{ _modifiers_FFD.RemoveAll(delegate(apModifier_FFD a) { return a == null; }); }
				//if(_modifiers_AnimatedFFD != null)			{ _modifiers_AnimatedFFD.RemoveAll(delegate(apModifier_AnimatedFFD a) { return a == null; }); }
				//if(_modifiers_ColorOnly != null)			{ _modifiers_ColorOnly.RemoveAll(delegate(apModifier_ColorOnly a) { return a == null; }); }
				//if(_modifiers_AnimatedColorOnly != null)	{ _modifiers_AnimatedColorOnly.RemoveAll(delegate(apModifier_AnimatedColorOnly a) { return a == null; }); }

				//변경 v1.5.0
				if (_modifiers_Volume != null)				{ _modifiers_Volume.RemoveAll(s_FindNullMod_Volume_Func); }
				if(_modifiers_Morph != null)				{ _modifiers_Morph.RemoveAll(s_FindNullMod_Morph_Func); }
				if(_modifiers_AnimatedMorph != null)		{ _modifiers_AnimatedMorph.RemoveAll(s_FindNullMod_AnimMorph_Func); }
				if(_modifiers_Rigging != null)				{ _modifiers_Rigging.RemoveAll(s_FindNullMod_Rigging_Func); }
				if(_modifiers_Physic != null)				{ _modifiers_Physic.RemoveAll(s_FindNullMod_Physic_Func); }
				if(_modifiers_TF != null)					{ _modifiers_TF.RemoveAll(s_FindNullMod_TF_Func); }
				if(_modifiers_AnimatedTF != null)			{ _modifiers_AnimatedTF.RemoveAll(s_FindNullMod_AnimTF_Func); }
				if(_modifiers_FFD != null)					{ _modifiers_FFD.RemoveAll(s_FindNullMod_FFD_Func); }
				if(_modifiers_AnimatedFFD != null)			{ _modifiers_AnimatedFFD.RemoveAll(s_FindNullMod_AnimFFD_Func); }
				if(_modifiers_ColorOnly != null)			{ _modifiers_ColorOnly.RemoveAll(s_FindNullMod_ColorOnly_Func); }
				if(_modifiers_AnimatedColorOnly != null)	{ _modifiers_AnimatedColorOnly.RemoveAll(s_FindNullMod_AnimColorOnly_Func); }
			}


			int nMods = _modifiers.Count;
			if (nMods > 0)
			{
				//이전 (GC 발생)
				//_modifiers.Sort(delegate (apModifierBase a, apModifierBase b)
				//{
				//	return (a._layer * 10) - (b._layer * 10);
				//});

				//변경 v1.5.0
				_modifiers.Sort(s_SortModifiers_Func);


				for (int i = 0; i < nMods; i++)
				{
					_modifiers[i]._layer = i;
				}
			}
			
			_isSorted = true;

			if(activeModOption == REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible)
			{
				ActiveAllModifierFromExclusiveEditing();
			}
		}


		private static Predicate<apModifier_Volume>				s_FindNullMod_Volume_Func =		FUNC_FindNullMod_Volume;
		private static Predicate<apModifier_Morph>				s_FindNullMod_Morph_Func =		FUNC_FindNullMod_Morph;
		private static Predicate<apModifier_AnimatedMorph>		s_FindNullMod_AnimMorph_Func =	FUNC_FindNullMod_AnimMorph;
		private static Predicate<apModifier_Rigging>			s_FindNullMod_Rigging_Func =	FUNC_FindNullMod_Rigging;
		private static Predicate<apModifier_Physic>				s_FindNullMod_Physic_Func =		FUNC_FindNullMod_Physic;
		private static Predicate<apModifier_TF>					s_FindNullMod_TF_Func =			FUNC_FindNullMod_TF;
		private static Predicate<apModifier_AnimatedTF>			s_FindNullMod_AnimTF_Func =		FUNC_FindNullMod_AnimTF;
		private static Predicate<apModifier_FFD>				s_FindNullMod_FFD_Func =		FUNC_FindNullMod_FFD;
		private static Predicate<apModifier_AnimatedFFD>		s_FindNullMod_AnimFFD_Func =	FUNC_FindNullMod_AnimFFD;
		private static Predicate<apModifier_ColorOnly>			s_FindNullMod_ColorOnly_Func =	FUNC_FindNullMod_ColorOnly;
		private static Predicate<apModifier_AnimatedColorOnly>	s_FindNullMod_AnimColorOnly_Func = FUNC_FindNullMod_AnimColorOnly;

		private static bool FUNC_FindNullMod_Volume(apModifier_Volume a) { return a == null; }
		private static bool FUNC_FindNullMod_Morph(apModifier_Morph a) { return a == null; }
		private static bool FUNC_FindNullMod_AnimMorph(apModifier_AnimatedMorph a) { return a == null; }
		private static bool FUNC_FindNullMod_Rigging(apModifier_Rigging a) { return a == null; }
		private static bool FUNC_FindNullMod_Physic(apModifier_Physic a) { return a == null; }
		private static bool FUNC_FindNullMod_TF(apModifier_TF a) { return a == null; }
		private static bool FUNC_FindNullMod_AnimTF(apModifier_AnimatedTF a) { return a == null; }
		private static bool FUNC_FindNullMod_FFD(apModifier_FFD a) { return a == null; }
		private static bool FUNC_FindNullMod_AnimFFD(apModifier_AnimatedFFD a) { return a == null; }
		private static bool FUNC_FindNullMod_ColorOnly(apModifier_ColorOnly a) { return a == null; }
		private static bool FUNC_FindNullMod_AnimColorOnly(apModifier_AnimatedColorOnly a) { return a == null; }


		private static Comparison<apModifierBase> s_SortModifiers_Func = FUNC_SortModifiers;
		private static int FUNC_SortModifiers(apModifierBase a, apModifierBase b)
		{
			return (a._layer * 10) - (b._layer * 10);
		}



		// Functions - Validate
		//----------------------------------------------------
		#region [미사용 코드]
		///// <summary>
		///// 모디파이어가 이 모디파이어스택에 포함되어 있는지 확인하는 유효성 검사용 함수.
		///// 모디파이어 삭제/복구 과정에서 제대로 연결되었는지 체크한다.
		///// </summary>
		//public bool IsContain(apModifierBase modifier)
		//{
		//	if(modifier == null)
		//	{
		//		return false;
		//	}
		//	switch (modifier.ModifierType)
		//	{
		//		case apModifierBase.MODIFIER_TYPE.Volume:
		//			if(_modifiers_Volume != null && modifier is apModifier_Volume)
		//			{
		//				return _modifiers_Volume.Contains(modifier as apModifier_Volume);
		//			}
		//			break;

		//		case apModifierBase.MODIFIER_TYPE.Morph:

		//		case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
		//		case apModifierBase.MODIFIER_TYPE.Rigging:
		//		case apModifierBase.MODIFIER_TYPE.Physic:
		//		case apModifierBase.MODIFIER_TYPE.TF:
		//		case apModifierBase.MODIFIER_TYPE.AnimatedTF:
		//		case apModifierBase.MODIFIER_TYPE.FFD:
		//		case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
		//		case apModifierBase.MODIFIER_TYPE.ColorOnly:
		//		case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
		//			break;
		//	}
		//	return false;
		//} 
		#endregion




		// Functions - Update
		//----------------------------------------------------
		public void Update_Pre(float tDelta)
		{
			int nMod = _modifiers != null ? _modifiers.Count : 0;
			if (nMod == 0 && !_isSorted)
			{
				RefreshAndSort(REFRESH_OPTION_ACTIVE.Keep, REFRESH_OPTION_REMOVE.Ignore);//변경 22.12.13
				nMod = _modifiers != null ? _modifiers.Count : 0;//다시 계산
			}


			if(nMod == 0)
			{
				return;
			}

			apModifierBase curMod = null;

			for (int i = 0; i < nMod; i++)
			{
				curMod = _modifiers[i];
				if (!curMod.IsPreUpdate)
				{
					//Post-Update라면 패스
					continue;
				}
				if (curMod._isActive
#if UNITY_EDITOR
					&& curMod._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force//변경 21.2.14 : 편집 모드에 의한 값 헤분화
#endif
				)
				{
					curMod.Calculate(tDelta);
				}
				else
				{
					curMod.InitCalculate(tDelta);
				}
			}
		}



		/// <summary>
		/// 추가 21.5.14 : C++ DLL을 이용하여 업데이트를 한다.
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update_Pre_DLL(float tDelta)
		{
			int nMod = _modifiers != null ? _modifiers.Count : 0;
			if (nMod == 0 && !_isSorted)
			{
				RefreshAndSort(REFRESH_OPTION_ACTIVE.Keep, REFRESH_OPTION_REMOVE.Ignore);//변경 22.12.13
				nMod = _modifiers != null ? _modifiers.Count : 0;//다시 계산
			}

			if(nMod == 0)
			{
				return;
			}

			apModifierBase curMod = null;

			for (int i = 0; i < nMod; i++)
			{
				curMod = _modifiers[i];
				if (!curMod.IsPreUpdate)
				{
					//Post-Update라면 패스
					continue;
				}
				if (curMod._isActive
#if UNITY_EDITOR
					&& curMod._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force//변경 21.2.14 : 편집 모드에 의한 값 헤분화
#endif
				)
				{
					//curMod.Calculate(tDelta);//기본
					curMod.Calculate_DLL(tDelta);//C++ DLL
				}
				else
				{
					curMod.InitCalculate(tDelta);
				}
			}
		}





		public void Update_Post(float tDelta)
		{
			int nMod = _modifiers != null ? _modifiers.Count : 0;

			if(nMod == 0)
			{
				return;
			}

			apModifierBase curMod = null;

			for (int i = 0; i < nMod; i++)
			{
				curMod = _modifiers[i];
				if (curMod.IsPreUpdate)
				{
					//Pre-Update라면 패스
					continue;
				}
				if (curMod._isActive
#if UNITY_EDITOR
					&& curMod._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force
#endif
				)
				{
					curMod.Calculate(tDelta);
				}
				else
				{
					curMod.InitCalculate(tDelta);
				}
			}
		}


		/// <summary>
		/// 추가 21.5.14 : C++ DLL을 이용하여 업데이트를 한다.
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update_Post_DLL(float tDelta)
		{
			int nMod = _modifiers != null ? _modifiers.Count : 0;

			if(nMod == 0)
			{
				return;
			}

			apModifierBase curMod = null;
			
			for (int i = 0; i < nMod; i++)
			{
				curMod = _modifiers[i];
				if (curMod.IsPreUpdate)
				{
					//Pre-Update라면 패스
					continue;
				}
				if (curMod._isActive
#if UNITY_EDITOR
				&& curMod._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force
#endif
				)
				{
					//curMod.Calculate(tDelta);//기본
					curMod.Calculate_DLL(tDelta);//DLL을 이용하여 업데이트
				}
				else
				{
					curMod.InitCalculate(tDelta);
				}
			}
		}



		// 에디터 관련 코드
		//----------------------------------------------------
		public void ActiveAllModifierFromExclusiveEditing()
		{
			apModifierBase modifier = null;
			apModifierParamSetGroup modPSG = null;
			int nModifiers = _modifiers != null ? _modifiers.Count : 0;
			if (nModifiers > 0)
			{
				for (int iMod = 0; iMod < nModifiers; iMod++)
				{
					modifier = _modifiers[iMod];
					modifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run;

					int nPSGs = modifier._paramSetGroup_controller != null ? modifier._paramSetGroup_controller.Count : 0;
					if (nPSGs > 0)
					{
						for (int iPSG = 0; iPSG < nPSGs; iPSG++)
						{
							modPSG = modifier._paramSetGroup_controller[iPSG];
							modPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
							modPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
						}
					}
				}
			}
			

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				int nChilds = _parentMeshGroup._childMeshGroupTransforms != null ? _parentMeshGroup._childMeshGroupTransforms.Count : 0;
				if (nChilds > 0)
				{
					apTransform_MeshGroup meshGroupTransform = null;
					for (int i = 0; i < nChilds; i++)
					{
						meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						}
					}
				}
			}
		}

		/// <summary>
		/// 모든 모디파이어를 강제로 비활성화 한다. 편집용은 아니다.
		/// </summary>
		public void SetDisableForceAllModifier()
		{
			apModifierBase curModifier = null;
			apModifierParamSetGroup curParamSetGroup = null;

			int nMods = _modifiers != null ? _modifiers.Count : 0;
			if (nMods > 0)
			{
				for (int iMod = 0; iMod < nMods; iMod++)
				{
					curModifier = _modifiers[iMod];

					curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force;

					int nPSGs = curModifier._paramSetGroup_controller != null ? curModifier._paramSetGroup_controller.Count : 0;

					if (nPSGs > 0)
					{
						for (int iPSG = 0; iPSG < nPSGs; iPSG++)
						{
							curParamSetGroup = curModifier._paramSetGroup_controller[iPSG];
							curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
							curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
						}
					}
				}
			}
			

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				int nChilds = _parentMeshGroup._childMeshGroupTransforms != null ? _parentMeshGroup._childMeshGroupTransforms.Count : 0;
				if (nChilds > 0)
				{
					apTransform_MeshGroup meshGroupTransform = null;
					for (int i = 0; i < nChilds; i++)
					{
						meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.SetDisableForceAllModifier();
						}
					}
				}
			}
		}


		/// <summary>
		/// [v1.4.5] 모디파이어와 PSG의 모든 Active/Enable 값을 활성화한다. (자식 객체 포함)
		/// 메뉴가 바뀔때 호출하자.
		/// </summary>
		public void EnableAllModifierAndPSGs()
		{
			apModifierBase curModifier = null;
			apModifierParamSetGroup curParamSetGroup = null;

			int nMods = _modifiers != null ? _modifiers.Count : 0;
			if (nMods > 0)
			{
				for (int iMod = 0; iMod < nMods; iMod++)
				{
					curModifier = _modifiers[iMod];
					curModifier._isActive = true;

					int nPSGs = curModifier._paramSetGroup_controller != null ? curModifier._paramSetGroup_controller.Count : 0;

					if (nPSGs > 0)
					{
						for (int iPSG = 0; iPSG < nPSGs; iPSG++)
						{
							curParamSetGroup = curModifier._paramSetGroup_controller[iPSG];
							curParamSetGroup._isEnabled = true;
						}
					}
				}
			}
			

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				int nChilds = _parentMeshGroup._childMeshGroupTransforms != null ? _parentMeshGroup._childMeshGroupTransforms.Count : 0;
				if (nChilds > 0)
				{
					apTransform_MeshGroup meshGroupTransform = null;
					for (int i = 0; i < nChilds; i++)
					{
						meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.EnableAllModifierAndPSGs();
						}
					}
				}
			}

		}






		/// <summary>
		/// 선택되지 않는 다른 모디파이어/PSG가 어떻게 동작할 것인지 옵션 (함수 인자용)
		/// </summary>
		public enum OTHER_MOD_RUN_OPTION
		{
			/// <summary>선택되지 않는 모디파이어/PSG는 비활성화한다. (Background 설정 제외)</summary>
			Disabled,
			/// <summary>선택되지 않은 모디파이어/PSG는 비활성화하되, 미리보기 용으로 색상만 동작한다.</summary>
			ActiveColorOnly,
			/// <summary>충돌되지 않은 모든 경우에 대해서 선택되지 않은 모든 모디파이어/PSG가 동작한다.</summary>
			ActiveAllPossible
		}

		/// <summary>
		/// [선택한 Modifier]와 [선택한 ParamSetGroup]만 활성화하고 나머지는 비활성한다.
		/// 한개의 ParamSetGroup만 활성화하므로 "한개의 ControlParam만 작업할 때" 호출된다.
		/// </summary>
		/// <param name="targetModifier"></param>
		/// <param name="targetParamSetGroup"></param>
		public void SetExclusiveModifierInEditing(	apModifierBase targetModifier, apModifierParamSetGroup targetParamSetGroup, 

													//이전
													//bool isAllowColorCalculated, 
													//bool isEnablePSGEvenIfDisabledModifier//추가 21.2.17 : 이게 True라면 선택되지 않은 모디파이어의 PSG도 일단 활성화

													OTHER_MOD_RUN_OPTION multipleModType//변경 22.5.13. bool 변수는 너무 어렵다.
													)
		{
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;

			//추가 : isColorCalculated가 추가되었다.
			//isColorCalculated라면 Exclusive여서 처리가 안되는 경우라도 무조건 Color 계산은 할 수 있다.

			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			bool isRiggingAvailable = false;
			if (targetModifier != null 
				&& targetModifier.IsTarget_Bone 
				&& targetModifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				isRiggingAvailable = true;//Rigging은 허용하자
			}

			//추가 22.5.13 [v1.4.0]
			//선택된 모디파이어가 Transform을 관여하지 않는 타입이라면,
			//다른 모디파이어의 Transform을 막을 필요가 없다.

			//이전
			//bool isTransformUpdatableOnNotSelectedModifier = false;//이게 True라면 선택되지 않은 모디파이어도 Transform계열 연산을 지원한다.
			//if(targetModifier != null && 
			//	(targetModifier.ModifierType == apModifierBase.MODIFIER_TYPE.ColorOnly
			//	|| targetModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedColorOnly
			//	|| targetModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic))
			//{
			//	isTransformUpdatableOnNotSelectedModifier = true;
			//}

			//더 개선 v1.5.0
			//TF(Morph 포함 이동 데이터 처리)가 활성화되는지 여부.
			//"다른 모디파이어"의 "PSG"에 할당된다.
			bool isOtherModTFUpdatable_Morph = true;//Morph 타입의 업데이트 가능여부
			bool isOtherModTFUpdatable_TF = true;//TF 타입의 업데이트 가능 여부
			bool isOtherModTFUpdatable_Physics = true;

			if(targetModifier != null)
			{
				switch (targetModifier.ModifierType)
				{
					case apModifierBase.MODIFIER_TYPE.Morph:
					case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
						{
							// [ 선택된게 Morph 타입 ]
							//isOtherModTFUpdatable_Morph = false;
							isOtherModTFUpdatable_Physics = false;
						}
						break;

					case apModifierBase.MODIFIER_TYPE.TF:
					case apModifierBase.MODIFIER_TYPE.AnimatedTF:
						{
							// [ 선택된게 TF 타입 ]
							//다른 TF는 제한된다. (v1.5.0에서의 버그 해결)
							//Morph와는 다르게 TF-TF 모디파이어는 동시 실행 안됨
							isOtherModTFUpdatable_TF = false;
						}
						break;

					case apModifierBase.MODIFIER_TYPE.Rigging:
						{
							// [ 선택된게 Rigging 타입 ]
							//리깅에 방해되는 다른 모디파이어는 제한된다.
							isOtherModTFUpdatable_Morph = false;
							isOtherModTFUpdatable_TF = false;
							isOtherModTFUpdatable_Physics = false;
						}
						break;

						//나머지는 모든 TF 동시 실행 가능
				}
			}


			apModifierBase curModifier = null;
			apModifierParamSetGroup curPSG = null;

			int nMod = _modifiers != null ? _modifiers.Count : 0;

			if (nMod > 0)
			{
				for (int i = 0; i < nMod; i++)
				{
					curModifier = _modifiers[i];

					int nPSGs = curModifier._paramSetGroup_controller != null ? curModifier._paramSetGroup_controller.Count : 0;

					if (curModifier == targetModifier && targetModifier != null && targetParamSetGroup != null)
					{
						//동일한 Modifier이다. 
						// ParamSetGroup이 같은 경우 무조건 활성
						// 다를 경우 : Color 제외하고 무조건 비활성

						//curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;//이전
						curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit;//변경 21.2.14 : 편집 중인 모디파이어

						
						if (nPSGs > 0)
						{
							for (int iP = 0; iP < nPSGs; iP++)
							{
								curPSG = curModifier._paramSetGroup_controller[iP];
								if (targetParamSetGroup == curPSG)
								{
									//편집 중인 PSG
									curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
									curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								}
								else
								{
									curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

									//색상 미리보기는 지원하는 경우
									if (multipleModType == OTHER_MOD_RUN_OPTION.ActiveColorOnly
										|| multipleModType == OTHER_MOD_RUN_OPTION.ActiveAllPossible)
									{
										//색상은 분리해서 따로 Enable이 가능
										curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
									}
									else
									{
										curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
									}
								}
							}
						}
					}
					else if (isRiggingAvailable && curModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
					{
						//만약 Rigging 타입은 예외로 친다면..
						curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background;//변경 21.2.15

						if (nPSGs > 0)
						{
							for (int iP = 0; iP < nPSGs; iP++)
							{
								curPSG = curModifier._paramSetGroup_controller[iP];

								curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;//<<사실 색상은 상관 없는뎅
							}
						}
						
					}
					else
					{
						//Exclusive에서 다른 것들은 무조건 제외하되, 그 방식이 다르다.

						//v1.5.0 : 현재의 "다른 모디파이어"의 종류에 따라 TF 편집 가능 여부가 결정된다.
						bool isTFPSGUpdatable = true;
						if(multipleModType == OTHER_MOD_RUN_OPTION.ActiveAllPossible)
						{
							switch (curModifier.ModifierType)
							{
								case apModifierBase.MODIFIER_TYPE.Morph:
								case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
									{
										if(!isOtherModTFUpdatable_Morph)
										{
											//Morph 타입의 처리는 제한된다.
											isTFPSGUpdatable = false;
										}
									}
									break;

								case apModifierBase.MODIFIER_TYPE.TF:
								case apModifierBase.MODIFIER_TYPE.AnimatedTF:
									{
										if(!isOtherModTFUpdatable_TF)
										{
											//TF 타입의 처리는 제한된다.
											isTFPSGUpdatable = false;
										}
									}
									break;

								case apModifierBase.MODIFIER_TYPE.Physic:
									{
										if(!isOtherModTFUpdatable_Physics)
										{
											//Physics 타입의 처리는 제한된다.
											isTFPSGUpdatable = false;
										}
									}
									break;
							}
						}


						if (nPSGs > 0)
						{
							for (int iP = 0; iP < nPSGs; iP++)
							{
								curPSG = curModifier._paramSetGroup_controller[iP];

								switch (multipleModType)
								{
									case OTHER_MOD_RUN_OPTION.Disabled:
										{
											//선택되지 않은 모든 모디파이어의 PSG는 무조건 비활성화
											curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
											curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
										}
										break;

									case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
										{
											//색상 옵션에 한해서, 색상 처리는 허가한다.
											curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
											curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
										}
										break;

									case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
										{
											//가능한 다른 모디파이어의 PSG들도 실행하는 옵션이므로, PSG는 활성화
											curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
											curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;

											//v1.5.0 : 불가 여부에 따라 Transform(이동) 정보는 비활성화된다.
											if(!isTFPSGUpdatable)
											{
												curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
											}
										}
										break;
								}
							}
						}
						

						//변경 22.5.13 [1.4.0]
						switch (multipleModType)
						{
							case OTHER_MOD_RUN_OPTION.Disabled:
								{
									//선택되지 않은 모든 모디파이어는 무조건 비활성화
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit;
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
								{
									//색상 옵션에 한해서, 색상 처리는 허가한다.
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;//값 변경 21.2.15
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
								{
									//가능한 다른 모디파이어를 허가한다.
									//if (isTransformUpdatableOnNotSelectedModifier)
									if(isTFPSGUpdatable)
									{
										//Transform을 제어하지 않는 모디파이어가 선택된 상태에서는 Enabled이 가능하다
										curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run;
									}
									else
									{
										//색상만 허가한다.
										curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;
									}
								}
								break;
						}
					}
				}
			}
			

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				int nChildMeshGroupTFs = _parentMeshGroup._childMeshGroupTransforms != null ? _parentMeshGroup._childMeshGroupTransforms.Count : 0;
				if(nChildMeshGroupTFs > 0)
				{
					for (int i = 0; i < nChildMeshGroupTFs; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							//meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing(null, null, isColorCalculated);
							meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing(
																	targetModifier, null, 
																	//isAllowColorCalculated,
																	//isEnablePSGEvenIfDisabledModifier

																	//변경
																	multipleModType
																	);//변경 20.9.2 : 자식 메시 그룹의 모디파이어에 적용할 때, 현재 타겟 정보를 넘기자.(Rigging 때문에)
						}
					}
				}
			}
		}


		/// <summary>
		/// TODO : 이거 애니메이션 용으로 고쳐야 한다.
		/// </summary>
		/// <param name="modifier"></param>
		/// <param name="paramSetGroups"></param>
		/// <param name="isColorCalculated"></param>
		public void SetExclusiveModifierInEditing_Anim(apModifierBase modifier,
														List<apModifierParamSetGroup> paramSetGroups,
														//bool isColorCalculated,
														//bool isEnablePSGEvenIfDisabledModifier

														OTHER_MOD_RUN_OPTION multipleModType//변경 22.5.13. bool 변수는 너무 어렵다.
														)
		{
			
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			bool isRiggingAvailable = false;
			if (modifier != null
				&& modifier.IsTarget_Bone
				&& modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				isRiggingAvailable = true;//Rigging은 허용하자
			}

			//추가 22.5.13 [v1.4.0]
			//선택된 모디파이어가 Transform을 관여하지 않는 타입이라면,
			//다른 모디파이어의 Transform을 막을 필요가 없다.
			//추가 v1.5.0
			//단순히 선택된 모디파이어에 따라 다른 모디파이어는 일괄 가능/불가를 결정할게 아니라
			//선택된 모디파이어 - 다른 모디파이어의 타입을 비교하여 가능/불가를 결정하자
			//ExTF가 가능한 모디파이어 타입을 지정하자

			//이전
			//bool isTransformUpdatableOnNotSelectedModifier = false;//이게 True라면 선택되지 않은 모디파이어도 Transform계열 연산을 지원한다.
			//if(modifier != null && 
			//	(modifier.ModifierType == apModifierBase.MODIFIER_TYPE.ColorOnly
			//	|| modifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedColorOnly
			//	|| modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic))
			//{
			//	isTransformUpdatableOnNotSelectedModifier = true;
			//}

			//변경 v1.5.0
			//TF(Morph 포함 이동 데이터 처리)가 활성화되는지 여부.
			//"다른 모디파이어"의 "PSG"에 할당된다.
			bool isOtherModTFUpdatable_Morph = true;//Morph 타입의 업데이트 가능여부
			bool isOtherModTFUpdatable_TF = true;//TF 타입의 업데이트 가능 여부
			bool isOtherModTFUpdatable_Physics = true;
			
			if(modifier != null)
			{
				switch (modifier.ModifierType)
				{
					case apModifierBase.MODIFIER_TYPE.Morph:
					case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
						{
							// [ 선택된게 Morph 타입 ]
							//isOtherModTFUpdatable_Morph = false;
							isOtherModTFUpdatable_Physics = false;
						}
						break;

					case apModifierBase.MODIFIER_TYPE.TF:
					case apModifierBase.MODIFIER_TYPE.AnimatedTF:
						{
							// [ 선택된게 TF 타입 ]
							//다른 TF는 제한된다. (v1.5.0에서의 버그 해결)
							//Morph와는 다르게 TF-TF 모디파이어는 동시 실행 안됨
							isOtherModTFUpdatable_TF = false;
						}
						break;

					case apModifierBase.MODIFIER_TYPE.Rigging:
						{
							// [ 선택된게 Rigging 타입 ]
							//리깅에 방해되는 다른 모디파이어는 제한된다.
							isOtherModTFUpdatable_Morph = false;
							isOtherModTFUpdatable_TF = false;
							isOtherModTFUpdatable_Physics = false;
						}
						break;

						//나머지는 모든 TF 동시 실행 가능
				}
			}
			

			apModifierBase curModifier = null;
			int nModifiers = _modifiers != null ? _modifiers.Count : 0;

			apModifierParamSetGroup curPSG = null;

			if (nModifiers > 0)
			{
				for (int i = 0; i < nModifiers; i++)
				{
					curModifier = _modifiers[i];

					int nPSGs = curModifier._paramSetGroup_controller != null ? curModifier._paramSetGroup_controller.Count : 0;

					if (curModifier == modifier && modifier != null && paramSetGroups != null && paramSetGroups.Count > 0)
					{
						//편집중인 모디파이어이다.
						//허가된 PSG (TimelineLayer)는 Enable, 그렇지 않다면 Disable로 만들자

						//_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;
						curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit;

						if (nPSGs > 0)
						{
							for (int iP = 0; iP < nPSGs; iP++)
							{
								curPSG = curModifier._paramSetGroup_controller[iP];
								if (paramSetGroups.Contains(curPSG))
								{
									//허용되는 ParamSetGroup이다.
									//무조건 허용
									curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
									curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								}
								else
								{
									//허용되지 않는 ParamSetGroup이다.
									//색상은 따로 처리 가능하다.
									curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

									//색상 미리보기는 지원하는 경우
									if (multipleModType == OTHER_MOD_RUN_OPTION.ActiveColorOnly
										|| multipleModType == OTHER_MOD_RUN_OPTION.ActiveAllPossible)
									{
										curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
									}
									else
									{
										curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
									}
								}
							}
						}
						
					}
					else if (isRiggingAvailable && curModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
					{
						//만약 Rigging 타입은 예외로 친다면..
						curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background;

						if (nPSGs > 0)
						{
							for (int iP = 0; iP < nPSGs; iP++)
							{
								curPSG = curModifier._paramSetGroup_controller[iP];
								curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;//<<Rigging은 상관 없는뎅..
							}
						}
						
					}
					else					
					{
						//추가 21.2.17
						//애니메이션이 아닌 모디파이어라면, 모디파이어는 Disabled이지만, PSG는 다 켜보자 > 이 부분 수정됨 v1.5.0
						//비활성 객체를 대상으로 적용될 수 있다.

						//v1.5.0 : 현재의 "다른 모디파이어"의 종류에 따라 TF 편집 가능 여부가 결정된다.
						bool isTFPSGUpdatable = true;
						if(multipleModType == OTHER_MOD_RUN_OPTION.ActiveAllPossible)
						{
							switch (curModifier.ModifierType)
							{
								case apModifierBase.MODIFIER_TYPE.Morph:
								case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
									{
										if(!isOtherModTFUpdatable_Morph)
										{
											//Morph 타입의 처리는 제한된다.
											isTFPSGUpdatable = false;
										}
									}
									break;

								case apModifierBase.MODIFIER_TYPE.TF:
								case apModifierBase.MODIFIER_TYPE.AnimatedTF:
									{
										if(!isOtherModTFUpdatable_TF)
										{
											//TF 타입의 처리는 제한된다.
											isTFPSGUpdatable = false;
										}
									}
									break;

								case apModifierBase.MODIFIER_TYPE.Physic:
									{
										if(!isOtherModTFUpdatable_Physics)
										{
											//Physics 타입의 처리는 제한된다.
											isTFPSGUpdatable = false;
										}
									}
									break;
							}
						}

						

						if (nPSGs > 0)
						{
							for (int iP = 0; iP < nPSGs; iP++)
							{
								curPSG = curModifier._paramSetGroup_controller[iP];

								switch (multipleModType)
								{
									case OTHER_MOD_RUN_OPTION.Disabled:
										{
											//선택되지 않은 모든 모디파이어의 PSG는 무조건 비활성화
											curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
											curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
										}
										break;

									case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
										{
											//색상 옵션에 한해서, 색상 처리는 허가한다.
											curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
											curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
										}
										break;

									case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
										{
											//가능한 다른 모디파이어의 PSG들도 실행하는 옵션이므로, PSG는 활성화
											curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
											curPSG._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;

											//v1.5.0 : 불가 여부에 따라 Transform(이동) 정보는 비활성화된다.
											if(!isTFPSGUpdatable)
											{
												curPSG._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
											}
										}
										break;
								}
							}
						}
						

						switch (multipleModType)
						{
							case OTHER_MOD_RUN_OPTION.Disabled:
								{
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit;
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
								{
									//색상 옵션에 한해서, 색상 처리는 허가한다.
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
								{
									//가능한 다른 모디파이어를 허가한다.
									//if (isTransformUpdatableOnNotSelectedModifier)
									if(isTFPSGUpdatable)
									{
										//Transform을 제어하지 않는 모디파이어가 선택된 상태에서는 Enabled이 가능하다
										curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run;
									}
									else
									{
										//색상만 허가한다.
										curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;
									}
								}
								break;
						}

						//Debug.Log("- 비활성되어야 하는 모디파이어 : " + curModifier.DisplayName 
						//	+ " - " + curModifier._editorExclusiveActiveMod);
					}
				}
			}
			

			//Child MeshGroup에도 모두 적용하자 - False로.. << 이게 문제였네
			if (_parentMeshGroup != null)
			{
				int nChildMeshGroupTFs = _parentMeshGroup._childMeshGroupTransforms != null ? _parentMeshGroup._childMeshGroupTransforms.Count : 0;
				if(nChildMeshGroupTFs > 0)
				{
					apTransform_MeshGroup childMeshGroupTF = null;
					for (int i = 0; i < nChildMeshGroupTFs; i++)
					{
						childMeshGroupTF = _parentMeshGroup._childMeshGroupTransforms[i];
						if (childMeshGroupTF._meshGroup != null && childMeshGroupTF._meshGroup != _parentMeshGroup)
						{
							childMeshGroupTF._meshGroup._modifierStack.SetExclusiveModifierInEditing(modifier, null, multipleModType);
						}
					}
				}
			}
		}





		#region [미사용 코드] General Edit는 삭제되었다. 다만 코드는 확인할 것
		///// <summary>
		///// [선택한 Modifier] + [해당 Modifier가 허용하는 다른 Modifier]만 허용한다.
		///// 모든 ParamSetGroup을 허용하므로 에디팅이 조금 다를 수는 있다.
		///// Animation 버전은 따로 만들 것
		///// Mod Unlock 모드이다.
		///// </summary>
		///// <param name="modifier"></param>
		//public void SetExclusiveModifierInEditingGeneral(apModifierBase modifier, bool isColorCalculated, bool isOtherModCalcualte)
		//{

		//	//TODO : 이 내용이 SetExclusiveModifierInEditing 함수에 같이 포함되어야 한다.

		//	//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
		//	apModifierBase.MODIFIER_TYPE[] exGeneralTypes = modifier.GetGeneralExEditableModTypes();
		//	if (exGeneralTypes == null)
		//	{
		//		exGeneralTypes = new apModifierBase.MODIFIER_TYPE[] { modifier.ModifierType };
		//	}

		//	//추가
		//	//요청한 Modifier가 BoneTransform을 지원하는 경우
		//	//Rigging은 비활성화 되어서는 안된다.
		//	for (int i = 0; i < _modifiers.Count; i++)
		//	{
		//		bool isValidType = false;
		//		for (int iGT = 0; iGT < exGeneralTypes.Length; iGT++)
		//		{
		//			if (exGeneralTypes[iGT] == _modifiers[i].ModifierType)
		//			{
		//				isValidType = true;
		//				break;
		//			}
		//		}

		//		if (isValidType)
		//		{
		//			//_modifiers[i]._isActive_InEditorExclusive = true;
		//			_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

		//			for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//			{
		//				//ParamSetGroup도 모두다 허용
		//				//_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = true;
		//				_modifiers[i]._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//				_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//			}
		//		}
		//		else
		//		{
		//			//불가
		//			//다만, OtherMod 처리 가능시 실행할 수도 있다. < 추가 3.22
		//			if(isOtherModCalcualte)
		//			{
		//				_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled;

		//				//여기선 완전히 Disabled가 아니라 SubExEnabled로 처리한다.

		//				for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//				{
		//					//_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = false;//<<
		//					_modifiers[i]._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//					if(isColorCalculated)
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//					}
		//					else
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//					}
		//				}
		//			}
		//			else
		//			{
		//				_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

		//				bool isAnyColorUpdate = false;

		//				for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//				{
		//					//_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = false;//<<
		//					_modifiers[i]._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//					if(isColorCalculated)
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//						isAnyColorUpdate = true;
		//					}
		//					else
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//					}
		//				}

		//				if(isAnyColorUpdate)
		//				{
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled;
		//				}
		//			}




		//		}
		//	}

		//	//Child MeshGroup에도 모두 적용하자
		//	if (_parentMeshGroup != null)
		//	{
		//		if (_parentMeshGroup._childMeshGroupTransforms != null)
		//		{
		//			for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
		//			{
		//				apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
		//				if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
		//				{
		//					meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditingGeneral(modifier, isColorCalculated, isOtherModCalcualte);
		//				}
		//			}
		//		}
		//	}
		//}


		///// <summary>
		///// AnimTimeline을 선택하고, 그 안의 AnimTimeLayer를 모두 활성화한다.
		///// 일반적으로 [선택하지 않은 AnimTimeline]들을 모두 해제하는 반면에, 
		///// 여기서는 해당 ParamSetGroup에 연동된 AnimTimeline이 AnimClip에 포함된다면 모두 포함시킨다.
		///// </summary>
		///// <param name="modifier"></param>
		///// <param name="paramSetGroups"></param>
		//public void SetExclusiveModifierInEditing_MultipleParamSetGroup_General(apModifierBase modifier, apAnimClip targetAnimClip,
		//																		bool isColorCalculated, bool isOtherModCalcualte)
		//{
		//	//Debug.Log("---- SetExclusiveModifierInEditing_MultipleParamSetGroup_General (" + _parentMeshGroup.name + ")----");

		//	//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
		//	//추가
		//	//요청한 Modifier가 BoneTransform을 지원하는 경우
		//	//Rigging은 비활성화 되어서는 안된다.
		//	apModifierBase.MODIFIER_TYPE[] exGeneralTypes = modifier.GetGeneralExEditableModTypes();
		//	if (exGeneralTypes == null)
		//	{
		//		exGeneralTypes = new apModifierBase.MODIFIER_TYPE[] { modifier.ModifierType };
		//	}

		//	for (int i = 0; i < _modifiers.Count; i++)
		//	{
		//		bool isValidType = false;
		//		for (int iGT = 0; iGT < exGeneralTypes.Length; iGT++)
		//		{
		//			if (exGeneralTypes[iGT] == _modifiers[i].ModifierType)
		//			{
		//				isValidType = true;
		//				break;
		//			}
		//		}

		//		if (isValidType)
		//		{
		//			//AnimClip을 포함하는 ParamSetGroup에 한해서 
		//			_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

		//			for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//			{
		//				apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//				if (paramSetGroup._keyAnimClip == targetAnimClip)
		//				{
		//					//무조건 활성
		//					//paramSetGroup._isEnabledExclusive = true;
		//					paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//					paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//				}
		//				else
		//				{
		//					//이건 완전히 불가 (Color, Other Mod 상관없다)
		//					//다른 애니메이션이다.
		//					//paramSetGroup._isEnabledExclusive = false;
		//					paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//					paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//				}
		//			}
		//		}
		//		else
		//		{
		//			//지원하는 타입이 아니다.
		//			//모두 Disabled한다.
		//			//..> 변경
		//			//지원하는 타입이 아닐때 Other Mod가 켜진 상태 또는 Color라면 업데이트를 해야한다.
		//			//Color + Transform이 항상 Disabled인 경우
		//			//-> Animation Type이며 ParamSetGroup의 AnimClip이 다른 경우
		//			//그게 아니라면 Color까지 다 체크해봐야 한다.

		//			//- Animation 타입이 아닌 경우
		//			//- Animation 타입일 때, 지금 AnimClip에 해당하는 경우

		//			if(_modifiers[i].IsAnimated)
		//			{
		//				//애니메이션 타입이다.
		//				//ParamSetGroup의 AnimClip이 다르면 무조건 Disabled이다.
		//				if (isOtherModCalcualte)
		//				{
		//					//완전히 불가 -> SubEx
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						if (paramSetGroup._keyAnimClip == targetAnimClip)
		//						{
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;

		//							if (isColorCalculated)
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//							}
		//							else
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//							}
		//						}
		//						else
		//						{
		//							//AnimClip이 다르다면 얄짤없이 Disabled
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//						}
		//					}
		//				}
		//				else
		//				{
		//					//애니메이션 타입인데 동시에 실행이 안되는 타입
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

		//					bool isAnyColorUpdate = false;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						if (paramSetGroup._keyAnimClip == targetAnimClip)
		//						{
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

		//							if (isColorCalculated)
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//								isAnyColorUpdate = true;
		//							}
		//							else
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//							}
		//						}
		//						else
		//						{
		//							//이건 얄짤없이 Disabled
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//						}
		//					}

		//					if(isAnyColorUpdate)
		//					{
		//						//Color 업데이트가 존재한다.
		//						_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled;
		//					}
		//				}
		//			}
		//			else
		//			{
		//				//애니메이션 타입이 아니다.
		//				//무조건 Disabled인 경우는 없다.
		//				if (isOtherModCalcualte)
		//				{
		//					//완전히 불가 -> SubEx
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;

		//						if(isColorCalculated)
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//						}
		//						else
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//						}
		//					}
		//				}
		//				else
		//				{
		//					//완전히 불가
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

		//					bool isAnyColorUpdate = false;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

		//						if(isColorCalculated)
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//							isAnyColorUpdate = true;
		//						}
		//						else
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//						}
		//					}

		//					if(isAnyColorUpdate)
		//					{
		//						//Color 업데이트가 있다.
		//						_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled;
		//					}
		//				}
		//			}


		//		}
		//	}

		//	//Child MeshGroup에도 모두 적용하자
		//	if (_parentMeshGroup != null)
		//	{
		//		if (_parentMeshGroup._childMeshGroupTransforms != null)
		//		{
		//			for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
		//			{
		//				apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
		//				if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
		//				{
		//					meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(modifier, targetAnimClip, isColorCalculated, isOtherModCalcualte);
		//				}
		//			}
		//		}
		//	}
		//} 
		#endregion



		// Add / Remove
		//----------------------------------------------------
		public void AddModifier(apModifierBase modifier, apModifierBase.MODIFIER_TYPE modifierType)
		{
			switch (modifierType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:

					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					{
						if (_modifiers_Volume == null) { _modifiers_Volume = new List<apModifier_Volume>(); }
						_modifiers_Volume.Add((apModifier_Volume)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					{
						if(_modifiers_Morph == null) { _modifiers_Morph = new List<apModifier_Morph>(); }
						_modifiers_Morph.Add((apModifier_Morph)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					{
						if(_modifiers_AnimatedMorph == null) { _modifiers_AnimatedMorph = new List<apModifier_AnimatedMorph>(); }
						_modifiers_AnimatedMorph.Add((apModifier_AnimatedMorph)modifier);
					}
					
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					{
						if(_modifiers_Rigging == null) { _modifiers_Rigging = new List<apModifier_Rigging>(); }
						_modifiers_Rigging.Add((apModifier_Rigging)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.Physic:
					{
						if(_modifiers_Physic == null) { _modifiers_Physic = new List<apModifier_Physic>(); }
						_modifiers_Physic.Add((apModifier_Physic)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					{
						if(_modifiers_TF == null) { _modifiers_TF = new List<apModifier_TF>(); }
						_modifiers_TF.Add((apModifier_TF)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					{
						if(_modifiers_AnimatedTF == null) { _modifiers_AnimatedTF = new List<apModifier_AnimatedTF>(); }
						_modifiers_AnimatedTF.Add((apModifier_AnimatedTF)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					{
						if(_modifiers_FFD == null) { _modifiers_FFD = new List<apModifier_FFD>(); }
						_modifiers_FFD.Add((apModifier_FFD)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					{
						if(_modifiers_AnimatedFFD == null) { _modifiers_AnimatedFFD = new List<apModifier_AnimatedFFD>(); }
						_modifiers_AnimatedFFD.Add((apModifier_AnimatedFFD)modifier);
					}
					break;


					//추가 21.7.20 : 색상 모디파이어 추가
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
					{
						if(_modifiers_ColorOnly == null) { _modifiers_ColorOnly = new List<apModifier_ColorOnly>(); }
						_modifiers_ColorOnly.Add((apModifier_ColorOnly)modifier);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					{
						if(_modifiers_AnimatedColorOnly == null) { _modifiers_AnimatedColorOnly = new List<apModifier_AnimatedColorOnly>(); }
						_modifiers_AnimatedColorOnly.Add((apModifier_AnimatedColorOnly)modifier);
					}
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + modifier + "]");
					break;
			}
		}


		public void RemoveModifier(apModifierBase modifier)
		{
			apModifierBase.MODIFIER_TYPE modType = modifier.ModifierType;

			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:

					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					{
						if (_modifiers_Volume != null)
						{
							_modifiers_Volume.Remove((apModifier_Volume)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					{
						if(_modifiers_Morph != null)
						{
							_modifiers_Morph.Remove((apModifier_Morph)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					{
						if(_modifiers_AnimatedMorph != null)
						{
							_modifiers_AnimatedMorph.Remove((apModifier_AnimatedMorph)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					{
						if(_modifiers_Rigging != null)
						{
							_modifiers_Rigging.Remove((apModifier_Rigging)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.Physic:
					{
						if(_modifiers_Physic != null)
						{
							_modifiers_Physic.Remove((apModifier_Physic)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					{
						if(_modifiers_TF != null)
						{
							_modifiers_TF.Remove((apModifier_TF)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					{
						if(_modifiers_AnimatedTF != null)
						{
							_modifiers_AnimatedTF.Remove((apModifier_AnimatedTF)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					{
						if(_modifiers_FFD != null)
						{
							_modifiers_FFD.Remove((apModifier_FFD)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					{
						if(_modifiers_AnimatedFFD != null)
						{
							_modifiers_AnimatedFFD.Remove((apModifier_AnimatedFFD)modifier);
						}
					}
					break;

					//추가 21.7.20 : 색상 모디파이어
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
					{
						if(_modifiers_ColorOnly != null)
						{
							_modifiers_ColorOnly.Remove((apModifier_ColorOnly)modifier);
						}
					}
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					{
						if(_modifiers_AnimatedColorOnly != null)
						{
							_modifiers_AnimatedColorOnly.Remove((apModifier_AnimatedColorOnly)modifier);
						}
					}
					break;
			}
		}


		// Link
		//----------------------------------------------------
		public void ClearAllCalculateParams(apModifierBase targetSelectedModifier)
		{
			apModifierBase curMod = null;
			apRenderUnit renderUnit = null;

			int nMod = _modifiers != null ? _modifiers.Count : 0;
			int nRenderUnits = _parentMeshGroup._renderUnits_All != null ? _parentMeshGroup._renderUnits_All.Count : 0;

			if (targetSelectedModifier == null)
			{
				//모든 모디파이어의 ResultParam을 초기화
				if (nMod > 0)
				{
					for (int i = 0; i < nMod; i++)
					{
						curMod = _modifiers[i];
						curMod._calculatedResultParams.Clear();
					}
				}

				//렌더 유닛들의 Stack도 리셋한다.
				if (nRenderUnits > 0)
				{
					for (int i = 0; i < nRenderUnits; i++)
					{
						renderUnit = _parentMeshGroup._renderUnits_All[i];
						renderUnit._calculatedStack.ClearResultParams();
					}
				}
			}
			else
			{
				//특정 모디파이어에 대해서만 초기화하려고 한다면 (추가 20.4.21)
				if (nMod > 0)
				{
					for (int i = 0; i < nMod; i++)
					{
						curMod = _modifiers[i];
						if (curMod == targetSelectedModifier)
						{
							//같을때만 (없을 수도 있어서..)
							curMod._calculatedResultParams.Clear();
						}
					}
				}

				//렌더 유닛들의 Stack도 리셋한다.
				if (nRenderUnits > 0)
				{
					for (int i = 0; i < nRenderUnits; i++)
					{
						renderUnit = _parentMeshGroup._renderUnits_All[i];
						renderUnit._calculatedStack.ClearResultParamsOfModifier(targetSelectedModifier);
					}
				}
			}
		}

		
		public void LinkModifierStackToRenderUnitCalculateStack(bool isRoot = true, apMeshGroup rootMeshGroup = null, apUtil.LinkRefreshRequest linkRefreshRequest = null)
		{
			//전체 Modifier중에서 RenderUnit을 포함한 Modifer를 찾는다.
			//그 중, RenderUnit에 대한것만 처리할 CalculateResultParam을 만들고 연동한다.
			//ResultParam을 RenderUnit의 CalculateStack에 넣는다.

			
			//수정
			//각 ModMesh에서 계층적인 Link를 할 수 있도록 RenderUnit을 매번 바꾸어주자
			if (isRoot)
			{
				rootMeshGroup = _parentMeshGroup;

				//Modifier-ParamSetGroup-ParamSet + ModMesh가 "실제 RenderUnit"과 링크되지 않으므로
				//Calculate Param을 만들기 전에 이 링크를 먼저 해주어야 한다.
			}
			
			//최적화 20.4.4
			bool isOnlySelectedModifier = false;//선택한 모디파이어만 처리
			bool isSkipAllAnimModifiers = false;//모든 Anim 모디파이어 생략
			bool isSkipUnselectedAnimPSGs = false;//선택되지 않은 AnimClip에 대한 Anim-PSG 생략
			apAnimClip curSelectedAnimClip = null;
			apModifierBase curSelectedModifier = null;
			if(linkRefreshRequest != null)
			{
				if(linkRefreshRequest.Request_Modifier == apUtil.LR_REQUEST__MODIFIER.SelectedModifier)
				{
					//선택한 모디파이어만 처리
					isOnlySelectedModifier = true;
					curSelectedModifier = linkRefreshRequest.Modifier;
				}
				else if(linkRefreshRequest.Request_Modifier == apUtil.LR_REQUEST__MODIFIER.AllModifiers_ExceptAnimMods)
				{
					//애니메이션 모디파이어는 모두 생략
					isSkipAllAnimModifiers = true;
				}
				else
				{
					//모든 모디파이어 처리
					if(linkRefreshRequest.Request_PSG == apUtil.LR_REQUEST__PSG.SelectedAnimClipPSG_IfAnimModifier)
					{
						//Anim 모디파이어 중) 선택한 AnimClip에 해당하지 않는 PSG는 생략
						isSkipUnselectedAnimPSGs = true;
						curSelectedAnimClip = linkRefreshRequest.AnimClip;
					}
				}
			}


			//Modifier를 돌면서 ParamSet 데이터를 Calculated 데이터로 변환해서 옮긴다.
			int nMod = _modifiers != null ? _modifiers.Count : 0;
			if (nMod > 0)
			{
				apModifierBase modifier = null;
				for (int iMod = 0; iMod < nMod; iMod++)
				{
					//Modifier ->..
					modifier = _modifiers[iMod];
					
					if (isOnlySelectedModifier && modifier != curSelectedModifier)
					{
						//최적화 1) 선택되지 않은 모디파이어는 생략
						continue;
					}

					if (isSkipAllAnimModifiers && modifier.IsAnimated)
					{
						//최적화 2) 모든 Anim 모디파이어를 생략
						continue;
					}

					List<apModifierParamSetGroup> paramSetGroups = modifier._paramSetGroup_controller;

					int nPSGs = paramSetGroups != null ? paramSetGroups.Count : 0;
					if (nPSGs > 0)
					{
						for (int iGroup = 0; iGroup < nPSGs; iGroup++)
						{
							//Modifier -> ParamSetGroup ->..
							apModifierParamSetGroup paramSetGroup = paramSetGroups[iGroup];

							if (isSkipUnselectedAnimPSGs && modifier.IsAnimated)
							{
								if (paramSetGroup._keyAnimClip != curSelectedAnimClip)
								{
									//최적화 3) 만약 스킵할 수 있는 "애니메이션 모디파이어의 PSG"라면, 처리를 생략한다.
									continue;
								}
							}

							List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

							int nParamSets = paramSets != null ? paramSets.Count : 0;
							if (nParamSets > 0)
							{
								for (int iParam = 0; iParam < nParamSets; iParam++)
								{
									//Modifier -> ParamSetGroup -> ParamSet ->...
									apModifierParamSet paramSet = paramSets[iParam];

									List<apModifiedMesh> modMeshes = paramSet._meshData;
									List<apModifiedBone> modBones = paramSet._boneData;

									int nModMeshes = modMeshes != null ? modMeshes.Count : 0;
									int nModBones = modBones != null ? modBones.Count : 0;

									//1. Mod Mesh => Calculate Param으로 연결한다.
									if (nModMeshes > 0)
									{
										apModifiedMesh modMesh = null;
										for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
										{
											//[핵심]
											//Modifier -> ParamSetGroup -> ParamSet -> ModMeh 
											//이제 이 ModMesh와 타겟 Transform을 연결하자.
											//연결할땐 Calculated 오브젝트를 만들어서 연결
											modMesh = modMeshes[iModMesh];

											if (modMesh._renderUnit == null)
											{
												//>> 당장 링크가 안될 수도 있다. (선택한 MeshGroup이 아닐 경우)
												//이때는 걍 무시한다.
												continue;
											}

											//이미 만든 Calculate Param이 있는지 확인
											apCalculatedResultParam existParam = modifier.GetCalculatedResultParam(modMesh._renderUnit);


											if (existParam != null)
											{
												existParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, modMesh, null);
												existParam.RefreshResultVertices();

												// > 추가 12.03 <
												//ParamKeyValue가 추가될 때에도 CalculateStack을 갱신할 필요가 있다.
												modMesh._renderUnit._calculatedStack.OnParamKeyValueAddedOnCalculatedResultParam(existParam);
											}
											else
											{
												//새로 Calculate Param을 만들고..
												apCalculatedResultParam newCalParam = new apCalculatedResultParam(
													modifier.CalculatedValueType,
													modifier.CalculatedSpace,
													modifier,
													modMesh._renderUnit,
													modMesh._renderUnit,
													null//<Bone은 없으닝께..
														//weightedVertexData // << 19.5.20 : 삭제
													);

												newCalParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, modMesh, null);

												// Modifier에 등록하고
												modifier._calculatedResultParams.Add(newCalParam);

												//RenderUnit에도 등록을 하자
												//<< 여기가 호출되어야 하는데 안되는 것 같다 >>
												modMesh._renderUnit._calculatedStack.AddCalculatedResultParam(newCalParam, modMesh._renderUnit);
											}

										}
									}
									

									//변경 : ModBone의 계산을 위해서 모든 ModBone이 계산되는 RenderUnit은 "Root  MeshGroup"의 "Root RenderUnit"이다.
									apRenderUnit modBoneRenderUnit = rootMeshGroup._rootRenderUnit;
									if (modBoneRenderUnit != null && nModBones > 0)
									{
										apModifiedBone modBone = null;

										//2. Mod Bone => Calculate Param으로 연결한다.
										for (int iModBone = 0; iModBone < nModBones; iModBone++)
										{
											modBone = modBones[iModBone];

											if (modBone._bone == null || modBone._renderUnit == null)
											{
												//일단 무시하자. Stack에 넣을 필요가 없다는 것
												continue;
											}

											//apCalculatedResultParam existParam = modifier.GetCalculatedResultParam_Bone(modBone._renderUnit, modBone._bone);//이전
											apCalculatedResultParam existParam = modifier.GetCalculatedResultParam_Bone(modBoneRenderUnit, modBone._bone, modBone._renderUnit);//변경

											if (existParam != null)
											{
												//Debug.LogWarning(" < Add > : " + modBone._bone._name);

												//이미 있다면 ModBone만 추가해주자
												existParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, null, modBone);
												existParam.RefreshResultVertices();
											}
											else
											{
												apCalculatedResultParam newCalParam = new apCalculatedResultParam(
													modifier.CalculatedValueType,
													modifier.CalculatedSpace,
													modifier,
													modBoneRenderUnit,//<<변경
													modBone._renderUnit,
													modBone._bone
													//null//WeightedVertex // 19.5.20 : 삭제
													);

												newCalParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, null, modBone);

												// Modifier에 등록하고
												modifier._calculatedResultParams.Add(newCalParam);

												//RenderUnit에도 등록을 하자
												modBoneRenderUnit._calculatedStack.AddCalculatedResultParam(newCalParam, modBone._renderUnit);
											}
										}
									}
								}
							}
							
						}
					}
					


					//SubList를 한번 정렬하자
					int nCalResultParams = modifier._calculatedResultParams != null ? modifier._calculatedResultParams.Count : 0;
					if (nCalResultParams > 0)
					{
						for (int iCal = 0; iCal < nCalResultParams; iCal++)
						{
							modifier._calculatedResultParams[iCal].SortSubList();
						}
					}
				}
			}
			

			//추가>>
			//하위 객체에 대해서도 Link를 자동으로 수행한다.
			//다 끝나고 Sort
			List<apTransform_MeshGroup> childMeshGroupTransforms = _parentMeshGroup._childMeshGroupTransforms;

			apTransform_MeshGroup childMeshGroup = null;

			int nChildMeshGroups = childMeshGroupTransforms != null ? childMeshGroupTransforms.Count : 0;
			if (nChildMeshGroups > 0)
			{
				for (int i = 0; i < nChildMeshGroups; i++)
				{
					childMeshGroup = childMeshGroupTransforms[i];
					if (childMeshGroup._meshGroup != null && childMeshGroup._meshGroup != _parentMeshGroup)
					{
						//Debug.Log(">> Child MeshGroup Check : " + childMeshGroup._nickName);
						childMeshGroup._meshGroup._modifierStack.LinkModifierStackToRenderUnitCalculateStack(false, rootMeshGroup, linkRefreshRequest);//<<여기서도 같이 수행
					}
				}
			}

			if (isRoot)
			{
				//Debug.Log("Start Sort : " + _parentMeshGroup._name);
				//Root인 경우
				//RenderUnit들을 검사하면서 Calculated Stack에 대해서 Sort를 해주자
				List<apRenderUnit> renderUnits = _parentMeshGroup._renderUnits_All;
				int nRenderUnits = renderUnits != null ? renderUnits.Count : 0;
				if (nRenderUnits > 0)
				{
					apRenderUnit curRenderUnit = null;
					for (int i = 0; i < nRenderUnits; i++)
					{
						curRenderUnit = renderUnits[i];
						curRenderUnit._calculatedStack.Sort();
					}
				}
			}
		}


		/// <summary>
		/// Modifier들의 계산 값들을 초기화한다.
		/// </summary>
		public void InitModifierCalculatedValues()
		{
			int nModifiers = _modifiers != null ? _modifiers.Count : 0;
			if(nModifiers == 0)
			{
				return;
			}

			apModifierBase modifier = null;
			List<apModifierParamSetGroup> paramSetGroups = null;
			apModifierParamSetGroup paramSetGroup = null;
			List<apModifierParamSet> paramSets = null;
			apModifierParamSet paramSet = null;
			List<apModifiedMesh> modMeshes = null;
			//List<apModifiedBone> modBones = null;
			apModifiedMesh modMesh = null;

			for (int iMod = 0; iMod < nModifiers; iMod++)
			{
				//Modifier ->..
				modifier = _modifiers[iMod];

				paramSetGroups = modifier._paramSetGroup_controller;

				int nPSGs = paramSetGroups != null ? paramSetGroups.Count : 0;
				if(nPSGs == 0)
				{
					continue;
				}

				for (int iGroup = 0; iGroup < nPSGs; iGroup++)
				{
					//Modifier -> ParamSetGroup ->..
					paramSetGroup = paramSetGroups[iGroup];

					paramSets = paramSetGroup._paramSetList;

					int nParamSets = paramSets != null ? paramSets.Count : 0;
					if(nParamSets == 0)
					{
						continue;
					}

					for (int iParam = 0; iParam < nParamSets; iParam++)
					{
						//Modifier -> ParamSetGroup -> ParamSet ->...
						paramSet = paramSets[iParam];

						modMeshes = paramSet._meshData;
						//modBones = paramSet._boneData;//Mod Bone은 초기화할게 없어서 주석 처리

						int nModMeshes = modMeshes != null ? modMeshes.Count : 0;
						if (nModMeshes > 0)
						{
							for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
							{
								modMesh = modMeshes[iModMesh];


								//if (modMesh._vertices != null && modMesh._vertices.Count > 0)
								//{
								//	//ModVert 초기화 => 현재는 초기화 할게 없다.

								//}
								//if (modMesh._vertRigs != null && modMesh._vertRigs.Count > 0)
								//{
								//	//ModVertRig 초기화 => 현재는 초기화 할게 없다.
								//}

								//ModVertWeight는 초기화해야한다. (물리값)
								int nVertWeights = modMesh._vertWeights != null ? modMesh._vertWeights.Count : 0;
								if(nVertWeights > 0)
								{
									apModifiedVertexWeight vertWeight = null;
									for (int iVW = 0; iVW < nVertWeights; iVW++)
									{
										vertWeight = modMesh._vertWeights[iVW];
										vertWeight.InitCalculatedValue();//<<초기화를 하자. (여기서는 물리값)
									}
								}
							}
						}
						
						//int nModBones = modBones != null ? modBones.Count : 0;
						//if(nModBones > 0)
						//{
						//	for (int iModBone = 0; iModBone < nModBones; iModBone++)
						//	{
						//		apModifiedBone modBone = modBones[iModBone];
						//		//ModBone도 현재는 초기화 할게 없다.
						//	}
						//}
						
					}
				}
			}
		}


		// Get / Set
		//----------------------------------------------------
		public int GetNewModifierID(int modifierType, int validationKey)
		{
			return apVersion.I.GetNextModifierID(modifierType, validationKey, IsModifierExist);
		}

		public apModifierBase GetModifier(int uniqueID)
		{
			//이전 (GC 발생)
			//return _modifiers.Find(delegate (apModifierBase a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetModifier_ID = uniqueID;
			return _modifiers.Find(s_GetModifierByID_Func);
		}


		public bool IsModifierExist(int uniqueID)
		{
			//이전 (GC 발생)
			//return _modifiers.Exists(delegate (apModifierBase a)
			//{
			//	return a._uniqueID == uniqueID;
			//});

			//변경 v1.5.0
			s_GetModifier_ID = uniqueID;
			return _modifiers.Exists(s_GetModifierByID_Func);
		}


		private static int s_GetModifier_ID = -1;
		private static Predicate<apModifierBase> s_GetModifierByID_Func = FUNC_GetModifierByID;
		private static bool FUNC_GetModifierByID(apModifierBase a)
		{
			return a._uniqueID == s_GetModifier_ID;
		}




		public int GetLastLayer()
		{
			int maxLayer = -1;
			int nMod = _modifiers != null ? _modifiers.Count : 0;
			if (nMod > 0)
			{
				apModifierBase curMod = null;
				for (int i = 0; i < nMod; i++)
				{
					curMod = _modifiers[i];
					if (maxLayer < curMod._layer)
					{
						maxLayer = curMod._layer;
					}
				}
			}
			
			return maxLayer;

		}



	}
}