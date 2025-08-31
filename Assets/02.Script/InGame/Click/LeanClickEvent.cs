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

		// Fire down event
		OnMouseDownEvent?.Invoke();

		// Mark this selectable as selected by this finger
		SelectSelf(finger);
	}

	private void HandleFingerUp(LeanFinger finger)
	{
		if (!Enable) return;

		if (IsFingerOverThis(finger) == false) return;

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
	/// HiddenObj 우선순위 검사: EventSystem.RaycastAll 결과에 HiddenObj가 있고 IsFound==false면 클릭을 막음
	/// </summary>
	private bool CheckHiddenObjectPriority(Vector2 screenPosition)
	{
		if (EventSystem.current == null) return true;

		var ped = new PointerEventData(EventSystem.current)
		{
			position = screenPosition
		};

		var raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(ped, raycastResults);

		foreach (var result in raycastResults)
		{
			var hiddenObj = result.gameObject.GetComponent<HiddenObj>();
			if (hiddenObj != null && !hiddenObj.IsFound)
			{
				return false;
			}
		}

		return true;
	}
}
