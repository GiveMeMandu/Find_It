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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//#if UNITY_EDITOR
//using UnityEngine.Profiling;
//#endif
using System;

using AnyPortrait;


namespace AnyPortrait
{
	//Important를 끈 객체가 간헐적인 업데이트를 할 때, 
	public class apOptUpdateChecker
	{
		// Const
		//---------------------------------------------------
		//리뉴얼
		//토큰의 FPS 범위
		private const int MAX_TOKEN_FPS = 60;
		private const int MIN_TOKEN_FPS = 2;

		//이 카운트동안 Update 호출이 되지 않았다면, 이 토큰은 더이상 사용되지 않는 것
		private const int MAX_TOKEN_UNUSED_COUNT = 10;
		private const int MAX_TOKENLIST_UNUSED_COUNT = 60;

		// SubClass
		//---------------------------------------------------
		public class UpdateToken
		{
			//리뉴얼 코드
			private int _FPS = -1;
			private int _unusedCount = 0;

			//LateUpdate에서 업데이트 가능한지 확인하기 위한 슬롯 정보

			private int _slotIndex = 0;
			private float _elapsedTime = 0.0f;
			private float _resultElapsedTime = 0.0f;

			private TokenList _parentTokenList = null;
			private bool _isUpdatable = false;




			public UpdateToken(int fps)
			{
				//이전


				//리뉴얼
				_FPS = fps;
				_unusedCount = 0;
				_slotIndex = -1;
				_elapsedTime = 0.0f;
				_resultElapsedTime = 0.0f;
				_parentTokenList = null;

				_isUpdatable = false;
			}

			public bool SetFPS(int fps)
			{
				bool isChanged = (_FPS != fps);
				_FPS = fps;
				return isChanged;
			}

			public void SetParentTokenList(TokenList tokenList)
			{
				_parentTokenList = tokenList;
			}

			public void Update(float deltaTime, bool isUpdatableFrame)
			{
				//_elapsedTime += deltaTime;
				_elapsedTime += deltaTime;
				_unusedCount = 0;//업데이트가 되었다면 카운트를 0으로 초기화

				_isUpdatable = isUpdatableFrame;
				if (_isUpdatable)
				{
					//업데이트가 될 것이라면 경과 시간을 갱신한다.
					_resultElapsedTime = _elapsedTime;
					_elapsedTime = 0.0f;
				}
			}

			public void ReadyToUpdate()
			{
				_unusedCount++;//일단 카운트 1 올린다.
				_isUpdatable = false;
			}

			public void SetSlotIndex(int nextSlotIndex)
			{
				_slotIndex = nextSlotIndex;
			}

			//public void SetUpdatable(bool isUpdatable)
			//{
			//	_isUpdatable = isUpdatable;
			//	if(_isUpdatable)
			//	{
			//		//업데이트가 될 것이라면 경과 시간을 갱신한다.
			//		_resultElapsedTime = _elapsedTime;
			//		_elapsedTime = 0.0f;
			//	}
			//}


			//Get
			public int SlotIndex { get { return _slotIndex; } }
			public TokenList ParentTokenList { get { return _parentTokenList; } }
			public float ResultElapsedTime { get { return _resultElapsedTime; } }

			//연속으로 업데이트 요청이 없었다면, 성능을 위해서 토큰을 리스트에서 삭제하자.
			public bool IsRemovable { get { return _unusedCount > MAX_TOKEN_UNUSED_COUNT; } }
			public bool IsUpdatable { get { return _isUpdatable; } }

		}

		public class TokenList
		{
			public int _FPS = -1;//키값이 되는 FPS


			//전체 토큰 리스트
			private List<UpdateToken> _tokens_All = new List<UpdateToken>();

			//<슬롯 인덱스, 토큰 리스트>
			private Dictionary<int, List<UpdateToken>> _tokens_PerSlots = new Dictionary<int, List<UpdateToken>>();


			//게임의 FPS에 따라서 슬롯의 크기를 정한다.
			private int _slotSize = 0;

			//이것도 업데이트 호출이 안된 상태로 몇차례 시간이 지나면 자동으로 폐기한다.
			private int _unusedCount = 0;
			private List<UpdateToken> _removableTokens = new List<UpdateToken>();

			//"현재 업데이트될 슬롯 인덱스" > 커서
			//LateUpdate에서 하나씩 증가하며, 이 커서와 같은 경우에만 토큰이 업데이트 될 수 있다.
			private int _updateCursor = 0;


			public TokenList(int fps)
			{
				_FPS = fps;

				if (_tokens_All == null)
				{
					_tokens_All = new List<UpdateToken>();
				}
				if (_tokens_PerSlots == null)
				{
					_tokens_PerSlots = new Dictionary<int, List<UpdateToken>>();
				}
				if (_removableTokens == null)
				{
					_removableTokens = new List<UpdateToken>();
				}
				_slotSize = 0;
				_unusedCount = 0;
				_updateCursor = 0;
			}

			/// <summary>
			/// Update의 첫 계산에서 호출된다.
			/// </summary>
			public void ReadyToUpdate()
			{
				UpdateToken curToken = null;
				for (int i = 0; i < _tokens_All.Count; i++)
				{
					curToken = _tokens_All[i];
					if (curToken == null)
					{
						//null이면 나중에 삭제를 해야한다.
						continue;
					}
					curToken.ReadyToUpdate();
				}
				_unusedCount++;//일단 카운트 1 올린다.
			}

			/// <summary>
			/// 게임의 FPS를 입력 받고 슬롯 크기를 계산한다.
			/// 기존의 슬롯 크기와 다르다면 슬롯을 초기화한다.
			/// </summary>
			/// <param name="gameFPS"></param>
			public void SetGameFPS(int gameFPS)
			{
				//슬롯 크기 계산하기
				//- 기본적으로 (게임 프레임 / 캐릭터 고정 프레임)으로 슬롯을 만들면 딱 맞는다.
				//- 슬롯이 많으면 애니메이션의 FPS가 줄어들며, 슬롯이 적으면 분산이 덜 되서 게임 성능에 영향을 더 준다.
				//- 게임 프레임이 60 밑으로 떨어진다면, 슬롯을 늘려서 성능을 보전해야한다.
				float gameOptParam = 1.0f;
				if (gameFPS < 60)
				{
					//게임 프레임이 60밑으로 떨어지면 강제로 슬롯을 늘려서 처리를 분산시키고 애니메이션 FPS를 줄인다.
					gameOptParam = 60.0f / (float)gameFPS;
				}

				int nextSlotSize = (int)(((float)gameFPS / (float)_FPS) + 0.5f);


				if (nextSlotSize < 2)
				{
					nextSlotSize = 2;//슬롯 최소 개수는 2이다. (최소 한번의 스킵은 있어야 한다.)
				}

				//게임 FPS가 떨어지면 강제로 애니메이션 FPS를 낮춰야 한다.
				nextSlotSize = (int)((nextSlotSize * gameOptParam) + 0.5f);

				//Debug.Log("Set Game FPS : " + gameFPS + " / Token FPS : " + _FPS + " / Next Slot Size : " + nextSlotSize);

				if (nextSlotSize != _slotSize)
				{
					//초기화
					//Debug.LogError("Slot Size Changed : " + _slotSize + " > " + nextSlotSize);

					_tokens_PerSlots.Clear();
					_slotSize = nextSlotSize;
					for (int i = 0; i < _slotSize; i++)
					{
						_tokens_PerSlots.Add(i, new List<UpdateToken>());
					}

					//< 슬롯 인덱스 재할당 >
					//존재하는 모든 토큰들의 슬롯 인덱스를 다시 설정해준다.
					//앞의 토큰부터 하나씩 0, 1, 2..를 할당한다.
					int curSlotIndex = 0;
					UpdateToken curToken = null;

					for (int i = 0; i < _tokens_All.Count; i++)
					{
						curToken = _tokens_All[i];

						if (curToken == null)
						{
							//null인 토큰은 나중에 삭제를 하자
							continue;
						}

						curToken.SetSlotIndex(curSlotIndex);//인덱스 변경
						_tokens_PerSlots[curSlotIndex].Add(curToken);//변경된 인덱스에 따른 슬롯 리스트에 재할당

						//슬롯 인덱스는 0, 1, 2... 0, 1, 2..식으로 반복된다.
						curSlotIndex++;
						if (curSlotIndex >= _slotSize)
						{
							curSlotIndex = 0;
						}
					}

					//커서가 슬롯 사이즈보다 크면 다시 0으로 초기화
					if (_updateCursor >= _slotSize)
					{
						_updateCursor = 0;
					}
				}
			}


			public bool AddAndUpdateToken(UpdateToken token)
			{
				//추가하거나 이미 있다면 인덱스 체크 후 갱신

				bool isNeedToAssignIndex = false;//슬롯 인덱스를 할당해야 하는가
				if (!_tokens_All.Contains(token))
				{
					//새로 추가했다면
					_tokens_All.Add(token);

					isNeedToAssignIndex = true;//슬롯 인덱스도 만들어줘야한다.
				}
				else
				{
					//이미 있다면
					//인덱스가 유효한지 확인하자
					int prevSlotIndex = token.SlotIndex;
					if (prevSlotIndex < 0 || prevSlotIndex >= _slotSize)
					{
						//기존의 슬롯 인덱스가 유효하지 않다.
						isNeedToAssignIndex = true;
					}
					else if (!_tokens_PerSlots[prevSlotIndex].Contains(token))
					{
						//슬롯의 리스트에 존재하지 않다면 다시 할당하자.
						isNeedToAssignIndex = true;
					}

					if (isNeedToAssignIndex)
					{
						//슬롯을 재할당하기 전에, 이전의 슬롯에서 이걸 제거해야한다.
						//"전체 리스트"에는 있었고, "슬롯 리스트에서는 찾지 못한" 상황 ( = 에러)
						for (int i = 0; i < _slotSize; i++)
						{
							_tokens_PerSlots[i].Remove(token);
						}
					}
				}
				if (isNeedToAssignIndex)
				{
					//중요!
					//최적의 슬롯 인덱스를 찾아서 토크에 할당하고, 슬롯 리스트에도 추가한다.
					int optSlotIndex = GetOptimizedSlotIndex();

					token.SetSlotIndex(optSlotIndex);
					_tokens_PerSlots[optSlotIndex].Add(token);

				}


				token.SetParentTokenList(this);//Parent로 등록하자

				//토큰이 입력되었으니 업데이트 카운트를 초기화하자.
				_unusedCount = 0;

				//리턴시에는 "현재 재생되는 슬롯의 인덱스"와 같은지 여부를 리턴한다.
				//같다면 이번 프레임에서 업데이트가 이루어진다.
				return (_updateCursor == token.SlotIndex);
			}

			public void RemoveToken(UpdateToken token)
			{
				if (!_tokens_All.Contains(token))
				{
					return;
				}

				//전체 리스트와 슬롯별 리스트에서 모두 삭제하자.
				_tokens_All.Remove(token);
				for (int i = 0; i < _slotSize; i++)
				{
					_tokens_PerSlots[i].Remove(token);
				}
			}


			/// <summary>
			/// 토큰들을 상대로 업데이트 되어야 할지, 삭제될지를 정하고, 토큰들에 기록을 해준다.
			/// - 업데이트 여부는 여기서 정하지 말고 삭제 여부만 정하자
			/// </summary>
			public void UpdateCursorAndRemoveInvalidTokens()
			{
				UpdateToken curToken = null;
				bool isAnyRemovable = false;
				bool isAnyNullToken = false;//<<하나라도 null이 있다면 검출해서 삭제하자.
				_removableTokens.Clear();

				int nAllTokens = _tokens_All.Count;
				for (int i = 0; i < nAllTokens; i++)
				{
					curToken = _tokens_All[i];
					if (curToken == null)
					{
						//null토큰이 있다면 삭제를 해야한다.
						isAnyNullToken = true;
						continue;
					}
					if (curToken.IsRemovable)
					{
						isAnyRemovable = true;
						_removableTokens.Add(curToken);
					}

					//이 부분 삭제 [v1.4.8]
					//else
					//{
					//	//커서와 슬롯 인덱스가 같을 때에만 업데이트 된다.
					//	curToken.SetUpdatable(_updateCursor == curToken.SlotIndex);
					//}
				}


				_updateCursor += 1;//커서 증가
				if (_updateCursor >= _slotSize)
				{
					_updateCursor = 0;
				}

				if (!isAnyRemovable && !isAnyNullToken)
				{
					return;
				}

				if (isAnyNullToken)
				{
					//Null 토큰을 삭제하자.
					//int nRemoved = _tokens_All.RemoveAll(delegate(UpdateToken a)
					_tokens_All.RemoveAll(delegate (UpdateToken a)
					{
						return a == null;
					});

					for (int iSlot = 0; iSlot < _slotSize; iSlot++)
					{
						_tokens_PerSlots[iSlot].RemoveAll(delegate (UpdateToken a)
						{
							return a == null;
						});
					}

					//Debug.LogError("Remove Null Tokens : " + nRemoved);
				}

				if (isAnyRemovable)
				{
					//수명이 다한 토큰을 삭제하자
					//int nRemovedUnusedTokens = _removableTokens.Count;
					for (int i = 0; i < _removableTokens.Count; i++)
					{
						curToken = _removableTokens[i];
						if (curToken == null)
						{
							continue;
						}
						_tokens_All.Remove(curToken);
						for (int iSlot = 0; iSlot < _slotSize; iSlot++)
						{
							_tokens_PerSlots[iSlot].Remove(curToken);
						}
					}

					_removableTokens.Clear();

					//Debug.LogError("Remove Unused Tokens : " + nRemovedUnusedTokens);
				}
			}

			//현재 "가장 적은 개수의 토큰을 보유하고 있는" 슬롯의 인덱스를 구한다.
			private int GetOptimizedSlotIndex()
			{
				int optSlotIndex = -1;
				int minNumTokens = -1;

				if (_slotSize == 0)
				{
					return -1;
				}

				int curTokens = 0;
				for (int i = 0; i < _slotSize; i++)
				{
					curTokens = _tokens_PerSlots[i].Count;
					if (minNumTokens < 0 || curTokens < minNumTokens)
					{
						//가장 적은 토큰 개수를 가진 슬롯의 인덱스를 반영한다.
						minNumTokens = curTokens;
						optSlotIndex = i;
					}
				}
				return optSlotIndex;
			}


			// Get
			//연속으로 업데이트가 없었다면 토큰 리스트 자체를 삭제한다.
			public bool IsRemovable { get { return _unusedCount > MAX_TOKENLIST_UNUSED_COUNT; } }


			// Debug Text
			//현재 상태를 출력하자
#if UNITY_EDITOR
			public string GetDebugText()
			{
				string result = "[" + _FPS + " ( " + _slotSize + " Slots ) ] : ";
				for (int i = 0; i < _slotSize; i++)
				{
					result += _tokens_PerSlots[i].Count + " ";
				}
				return result;
			}
#endif
		}


		// Members
		//---------------------------------------------------
		private static apOptUpdateChecker _instance = new apOptUpdateChecker();
		public static apOptUpdateChecker I { get { return _instance; } }

		private Dictionary<int, TokenList> _fps2Tokens = new Dictionary<int, TokenList>();

		private enum STATE
		{
			Ready, Update, LateUpdate
		}
		private STATE _state = STATE.Ready;

		//리뉴얼
		//게임의 FPS를 계산하자.
		//DeltaTime 기록 배열이 모두 찰때마다 평균값을 새로 계산한다.
		private const int INIT_GAME_FPS = 60;
		private const int NUM_FPS_RECORDS = 300;//60FPS 기준으로 5초마다 평균 계산

		//실제 게임의 FPS 범위
		private const int MAX_GAME_FPS = 150;
		private const int MIN_GAME_FPS = 15;

		private const float MAX_VALID_DELTA_TIME = 0.2f;//5 FPS보다 느리면 유효하지 않은 DeltaTime이다.

		private int _curGameFPS = INIT_GAME_FPS;
		private float[] _deltaTimeRecords = null;
		private int _iDeltaTimeRecord = 0;


		private List<TokenList> _removableTokenLists = new List<TokenList>();//삭제 처리를 위한 리스트. 임시용이다.

#if UNITY_EDITOR
		private List<string> _debugTexts = new List<string>();
#endif

		// Init
		//---------------------------------------------------
		private apOptUpdateChecker()
		{
			Init();
		}

		public void Init()
		{
			//초기화
			_fps2Tokens.Clear();
			_removableTokenLists.Clear();

			_state = STATE.Ready;

			InitFPSRecords();
		}


#if UNITY_2020_1_OR_NEWER
		//[v1.5.0] Reload Domain 옵션 비활성화시 static 필드 초기화가 안되는데, 이 기능에 영향을 줘버린다.
		//Reload Domain 비활성화 + 게임 시작시 강제로 초기화를 호출해야한다.
		// 참조 : https://docs.unity3d.com/kr/2022.3/Manual/DomainReloading.html
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void InitDomain()
		{
			if(_instance != null)
			{
				_instance.Init();
			}
		}
#endif

		// Functions
		//---------------------------------------------------
		//Renew
		//프레임 계산
		private void InitFPSRecords()
		{
			_curGameFPS = INIT_GAME_FPS;
			if(_deltaTimeRecords == null)
			{
				_deltaTimeRecords = new float[NUM_FPS_RECORDS];
			}
			for (int i = 0; i < NUM_FPS_RECORDS; i++)
			{
				_deltaTimeRecords[i] = 0.0f;
			}
			_iDeltaTimeRecord = 0;
		}

		/// <summary>
		/// Update에서 "첫 업데이트"시 DeltaTime을 받아서 기록하거나 현재 게임의 FPS를 계산한다.
		/// FPS가 갱신되면 true를 리턴한다.
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		private bool CalculateFPSRecord(float deltaTime)
		{

			if(deltaTime > MAX_VALID_DELTA_TIME)
			{
				//너무 느린 프레임 (DeltaTime이 크다)이라면
				//Spike일 것이므로 생략한다.
				return false;
			}


			if(_iDeltaTimeRecord < NUM_FPS_RECORDS)
			{
				//기록을 하자
				_deltaTimeRecords[_iDeltaTimeRecord] = deltaTime;
				_iDeltaTimeRecord++;
				return false;
			}
			else
			{
				//평균을 내서 FPS를 갱신하자.
				float totalDeltaTime = 0.0f;
				for (int i = 0; i < NUM_FPS_RECORDS; i++)
				{
					totalDeltaTime += _deltaTimeRecords[i];//Delta Time을 저장하고
					//_deltaTimeRecords[i] = 0.0f;//기록은 초기화 > 안해도 된다.
				}
				_iDeltaTimeRecord = 0;

				//평균값으로 FPS 계산하기
				float avgDeltaTime = totalDeltaTime / NUM_FPS_RECORDS;
				int prevFPS = _curGameFPS;
				if(avgDeltaTime > 0.0f)
				{
					_curGameFPS = (int)(1.0f / avgDeltaTime);
				}

				return _curGameFPS != prevFPS;//값이 바뀌었다면 true 리턴
			}
		}



		//Update에서 호출하는 함수와
		//LateUpdate에서 호출하는 함수 2개로 나뉜다.
		/// <summary>
		/// 이 함수를 Update에서 호출하자
		/// 토큰이 없다면 null로 하되, 리턴값을 멤버로 가지고 있자
		/// 여기서 IsUpdatable도 처리한다.
		/// </summary>
		/// <param name="token"></param>
		/// <param name="fps"></param>
		/// <returns></returns>
		public UpdateToken OnUpdate(UpdateToken token, int fps, float deltaTime)
		{
			//상태가 바뀌면 초기화를 해야한다.
			if(_state == STATE.Ready)
			{
				//초기화 상태에서 토큰이 만들어졌다면 이상하다
				token = null;
			}

			if(_state != STATE.Update)
			{
				foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
				{
					keyValuePair.Value.ReadyToUpdate();
				}

				_state = STATE.Update;


				//리뉴얼 : FPS를 계산한다.
				bool isFPSChanged = CalculateFPSRecord(deltaTime);
				if(isFPSChanged)
				{
					//FPS가 바뀌었다면 토큰 리스트의 슬롯 개수를 변경해야한다.
					foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
					{
						keyValuePair.Value.SetGameFPS(_curGameFPS);
					}
				}
			}

			int tokenFPS = Mathf.Clamp(fps, MIN_TOKEN_FPS, MAX_TOKEN_FPS);
			bool isNewToken = false;
			if(token == null)
			{
				token = new UpdateToken(tokenFPS);
				isNewToken = true;//새로운 토큰 발생시에는 그 프레임에서는 무조건 업데이트
			}
			else
			{
				bool isChanged = token.SetFPS(tokenFPS);
				//이미 만들어진 토큰이다.
				//만약 FPS가 바뀌었다면, 기존에 슬롯에 저장되었다면, 해당 슬롯에서는 제거하자.
				if(isChanged && token.ParentTokenList != null)
				{
					//슬롯에서 제거하기 + 개수 변경하기
					token.ParentTokenList.RemoveToken(token);
				}
			}

			//입력될 토큰 리스트를 찾자
			bool isUpdatedFrame = false;
			if(_fps2Tokens.ContainsKey(tokenFPS))
			{
				isUpdatedFrame = _fps2Tokens[tokenFPS].AddAndUpdateToken(token);
			}
			else
			{
				//새로운 슬롯을 생성
				TokenList newTokenList = new TokenList(tokenFPS);
				newTokenList.SetGameFPS(_curGameFPS);//현재 기준으로 초기 크기도 지정해야한다.
				
				isUpdatedFrame = newTokenList.AddAndUpdateToken(token);

				_fps2Tokens.Add(tokenFPS, newTokenList);
			}

			//시간도 업데이트 해주고, 업데이트 여부도 지정하자 [v1.4.8]
			//첫 토큰 요청이거나 업데이트 프레임이면 업데이트가 이루어진다.
			token.Update(deltaTime, isUpdatedFrame || isNewToken);

			return token;
		}

		/// <summary>
		/// 이 함수를 LateUpdate에서 호출하자. 
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public void OnLateUpdate()
		{
			if(_state == STATE.LateUpdate)
			{
				return;
			}

			//Late Update의 첫 프레임에서는 "유효하지 않은 토큰"이나 "토큰 리스트"들을 삭제해야 한다.
			TokenList curTokenList = null;
			_removableTokenLists.Clear();
			bool isAnyRemoveTokenList = false;
			foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
			{
				curTokenList = keyValuePair.Value;
				if(curTokenList.IsRemovable)
				{
					//토큰 리스트가 삭제되어야 한다면
					isAnyRemoveTokenList = true;
					_removableTokenLists.Add(curTokenList);
				}
				else
				{
					//<중요!>
					//그렇지 않다면 토큰 리스트 내부의 유효하지 않은 토큰들을 찾아서 삭제하자
					//이 함수를 호출하면 토큰들의 "업데이트 여부"도 결정된다.
					curTokenList.UpdateCursorAndRemoveInvalidTokens();//<<이거 수정해야함 v1.4.8
				}
			}

			if(isAnyRemoveTokenList)
			{
				//삭제할 토큰리스트는 삭제하자
				int nRemovedList = _removableTokenLists.Count;
				for (int i = 0; i < _removableTokenLists.Count; i++)
				{
					_fps2Tokens.Remove(_removableTokenLists[i]._FPS);
				}

				_removableTokenLists.Clear();
			}

			_state = STATE.LateUpdate;
		}


		// Get / Set
		//---------------------------------------------------
#if UNITY_EDITOR
		public List<string> GetDebugTexts()
		{
			if (_debugTexts == null)
			{
				_debugTexts = new List<string>();
			}
			_debugTexts.Clear();

			if (_fps2Tokens == null)
			{
				return _debugTexts;
			}

			_debugTexts.Add("Token Lists : " + _fps2Tokens.Count);

			TokenList curTokenList = null;
			foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
			{
				curTokenList = keyValuePair.Value;
				_debugTexts.Add(curTokenList.GetDebugText());
			}

			return _debugTexts;
		}
#endif

	}
}