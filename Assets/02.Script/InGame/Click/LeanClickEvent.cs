using Lean.Common;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DeskCat.FindIt.Scripts.Core.Main.System;

public class LeanClickEvent : LeanSelectableByFinger
{
	public bool Enable = true;

	public UnityEvent OnMouseDownEvent;
	public UnityEvent OnMouseUpEvent;
	public UnityEvent OnClickEvent;

	private int _clickCount = 0;
	public int _maxClickCount = -1;

	protected override void OnEnable()
	{
		base.OnEnable();

		LeanTouch.OnFingerDown += HandleFingerDown;
		LeanTouch.OnFingerUp += HandleFingerUp;
		LeanTouch.OnFingerTap += HandleFingerTap;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		LeanTouch.OnFingerDown -= HandleFingerDown;
		LeanTouch.OnFingerUp -= HandleFingerUp;
		LeanTouch.OnFingerTap -= HandleFingerTap;
	}

	protected override void OnDestroy()
	{
		// Ensure static events are unsubscribed if object destroyed
		LeanTouch.OnFingerDown -= HandleFingerDown;
		LeanTouch.OnFingerUp -= HandleFingerUp;
		LeanTouch.OnFingerTap -= HandleFingerTap;

		base.OnDestroy();
	}

	private void HandleFingerDown(LeanFinger finger)
	{
		if (!Enable) return;

		if (IsFingerOverThis(finger) == false) return;

		// Hidden object priority check
		if (!CheckHiddenObjectPriority(finger.ScreenPosition)) return;

		// Check hierarchy priority - only allow top-most object to be clicked
		if (!CheckHierarchyPriority(finger.ScreenPosition)) return;

		// Fire down event
		OnMouseDownEvent?.Invoke();

		// Mark this selectable as selected by this finger
		SelectSelf(finger);
	}

	private void HandleFingerUp(LeanFinger finger)
	{
		if (!Enable) return;

		if (IsFingerOverThis(finger) == false) return;

		// Hidden object priority check
		if (!CheckHiddenObjectPriority(finger.ScreenPosition)) return;

		// Check hierarchy priority - only allow top-most object to be clicked
		if (!CheckHierarchyPriority(finger.ScreenPosition)) return;

		OnMouseUpEvent?.Invoke();
		// selection removal is handled by LeanSelectableByFinger.HandleFingerUp
	}

	private void HandleFingerTap(LeanFinger finger)
	{
		if (!Enable) return;

		if (IsFingerOverThis(finger) == false) return;

		// Prevent tap if max reached
		if (_maxClickCount != -1 && _clickCount >= _maxClickCount) return;

		// Hidden object priority check using EventSystem raycast
		if (!CheckHiddenObjectPriority(finger.ScreenPosition)) return;

		// Check hierarchy priority - only allow top-most object to be clicked
		if (!CheckHierarchyPriority(finger.ScreenPosition)) return;

		// Fire click event
		OnClickEvent?.Invoke();
		_clickCount++;

		Debug.Log("LeanClickEvent OnFingerTap " + gameObject.name + " _clickCount: " + _clickCount);
	}

	public void IsEnable(bool enable)
	{
		Enable = enable;
	}

	public void SetMaxClickCount(int maxCount)
	{
		_maxClickCount = maxCount;
	}

	public void ResetClickCount()
	{
		_clickCount = 0;
	}

	public int GetClickCount()
	{
		return _clickCount;
	}

	/// <summary>
	/// Checks whether the finger screen position overlaps this GameObject (UI/3D/2D).
	/// </summary>
	private bool IsFingerOverThis(LeanFinger finger)
	{
		// UI check
		if (EventSystem.current != null)
		{
			var ped = new PointerEventData(EventSystem.current)
			{
				position = finger.ScreenPosition
			};

			var raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(ped, raycastResults);

			foreach (var result in raycastResults)
			{
				if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
				{
					return true;
				}
			}
		}

		// 3D physics check
		var cam = Camera.main ?? Camera.current;
		if (cam != null)
		{
			var ray = cam.ScreenPointToRay(finger.ScreenPosition);

			if (Physics.Raycast(ray, out var hit))
			{
				if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
				{
					return true;
				}
			}

			// 2D check
			var wp = cam.ScreenToWorldPoint(new Vector3(finger.ScreenPosition.x, finger.ScreenPosition.y, cam.nearClipPlane));
			var hit2d = Physics2D.OverlapPoint(wp);
			if (hit2d != null && (hit2d.gameObject == gameObject || hit2d.transform.IsChildOf(transform)))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// HiddenObj 우선순위 검사: HiddenObj가 있고 찾아지지 않은 상태면 클릭을 막음
	/// 단, 현재 오브젝트가 HiddenObj라면 우선순위 검사 무시
	/// </summary>
	private bool CheckHiddenObjectPriority(Vector2 screenPosition)
	{
		// HiddenObject 레이어만 체크 (Layer 8)
		int hiddenObjectLayerMask = 1 << 8;

		Camera cam = Camera.main;
		if (cam == null) return true;

		Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
		Collider2D hiddenObjHit = Physics2D.OverlapPoint(worldPoint, hiddenObjectLayerMask);

		if (hiddenObjHit != null)
		{
			HiddenObj hiddenObj = hiddenObjHit.GetComponent<HiddenObj>();
			if (hiddenObj != null && !hiddenObj.IsFound)
			{
				Debug.Log($"HiddenObj {hiddenObj.gameObject.name} has priority, blocking other clicks");
				return false; // HiddenObj가 우선순위를 가짐
			}
		}

		return true; // 다른 오브젝트 클릭 허용
	}

	/// <summary>
	/// 하이어라키 우선순위 검사: 겹치는 객체들 중에서 하이어라키상 가장 위에 있는 객체만 클릭을 허용
	/// </summary>
	private bool CheckHierarchyPriority(Vector2 screenPosition)
	{
		Camera cam = Camera.main;
		if (cam == null) return true;

		Vector3 worldPoint = cam.ScreenToWorldPoint(screenPosition);
		
		// 모든 2D 콜라이더를 검사하여 겹치는 LeanClickEvent를 가진 객체들을 찾음
		Collider2D[] overlappingColliders = Physics2D.OverlapPointAll(worldPoint);
		
		List<LeanClickEvent> overlappingClickEvents = new List<LeanClickEvent>();
		
		foreach (var collider in overlappingColliders)
		{
			LeanClickEvent clickEvent = collider.GetComponent<LeanClickEvent>();
			if (clickEvent != null && clickEvent.Enable)
			{
				overlappingClickEvents.Add(clickEvent);
			}
		}
		
		// 겹치는 객체가 1개 이하면 그냥 허용
		if (overlappingClickEvents.Count <= 1)
		{
			return true;
		}
		
		// 하이어라키 순서로 정렬 (sibling index가 작을수록 위에 있음)
		overlappingClickEvents.Sort((a, b) => 
		{
			// 같은 부모를 가진 경우 sibling index로 비교
			if (a.transform.parent == b.transform.parent)
			{
				return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
			}
			
			// 다른 부모를 가진 경우, 루트까지 올라가서 비교
			Transform aRoot = a.transform;
			Transform bRoot = b.transform;
			
			while (aRoot.parent != null) aRoot = aRoot.parent;
			while (bRoot.parent != null) bRoot = bRoot.parent;
			
			return aRoot.GetSiblingIndex().CompareTo(bRoot.GetSiblingIndex());
		});
		
		// 가장 위에 있는(첫 번째) 객체만 클릭을 허용
		bool isTopMost = overlappingClickEvents[0] == this;
		
		if (!isTopMost)
		{
			Debug.Log($"Hierarchy priority: {overlappingClickEvents[0].gameObject.name} blocks {gameObject.name}");
		}
		
		return isTopMost;
	}
}
