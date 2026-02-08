using System;
using UnityEngine;
using System.Collections;

namespace MustGames {
	
	public class DestroySelf : MonoBehaviour {
		
		[Serializable]
		public enum ModeType { Destroy, Disable }

		public ModeType Mode;
		public float DestroyTime = 2.0f;

		private float _timer;

		#region Mono Behaviour
		private void OnEnable () {
			_timer = 0.0f;
		}

		private void Update () {

			_timer += Time.unscaledDeltaTime;

			if (_timer > DestroyTime) {
				
				switch (Mode) {
				case ModeType.Destroy:
					Destroy(gameObject);
					break;
				case ModeType.Disable:
					gameObject.SetActive (false);
					break;
				}

				_timer = 0.0f;
			}
		}
		#endregion
	}
}

