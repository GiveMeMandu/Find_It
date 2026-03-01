using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ETFXPEL
{

public class UICanvasManager : MonoBehaviour {
	public static UICanvasManager GlobalAccess;
	void Awake () {
		GlobalAccess = this;
	}

	public bool MouseOverButton = false;
	public Text PENameText;
	public Text ToolTipText;

	// Use this for initialization
	void Start () {
		if (PENameText != null)
			PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
	}
	
	// Update is called once per frame
	void Update () {
	
		// Mouse Click - Check if mouse over button to prevent spawning particle effects while hovering or using UI buttons.
		if (!MouseOverButton) {
			// Left Button Click (new Input System guarded, fallback to legacy)
			#if ENABLE_INPUT_SYSTEM
			if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) {
				SpawnCurrentParticleEffect();
			}
			#else
			if (Input.GetMouseButtonUp (0)) {
				SpawnCurrentParticleEffect();
			}
			#endif
		}
		// Keyboard shortcuts (A/D)
		#if ENABLE_INPUT_SYSTEM
		if (Keyboard.current != null) {
			if (Keyboard.current.aKey.wasReleasedThisFrame) SelectPreviousPE();
			if (Keyboard.current.dKey.wasReleasedThisFrame) SelectNextPE();
		}
		#else
		if (Input.GetKeyUp (KeyCode.A)) {
			SelectPreviousPE ();
		}
		if (Input.GetKeyUp (KeyCode.D)) {
			SelectNextPE ();
		}
		#endif
	}

	public void UpdateToolTip(ButtonTypes toolTipType) {
		if (ToolTipText != null) {
			if (toolTipType == ButtonTypes.Previous) {
				ToolTipText.text = "Select Previous Particle Effect";
			}
			else if (toolTipType == ButtonTypes.Next) {
				ToolTipText.text = "Select Next Particle Effect";
			}
		}
	}
	public void ClearToolTip() {
		if (ToolTipText != null) {
			ToolTipText.text = "";
		}
	}

	private void SelectPreviousPE() {
		// Previous
		ParticleEffectsLibrary.GlobalAccess.PreviousParticleEffect();
		if (PENameText != null)
			PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
	}
	private void SelectNextPE() {
		// Next
		ParticleEffectsLibrary.GlobalAccess.NextParticleEffect();
		if (PENameText != null)
			PENameText.text = ParticleEffectsLibrary.GlobalAccess.GetCurrentPENameString();
	}

	private RaycastHit rayHit;
	private void SpawnCurrentParticleEffect() {
		// Spawn Particle Effect
		#if ENABLE_INPUT_SYSTEM
		Vector3 screenPos;
		if (Mouse.current != null) {
			Vector2 mpos = Mouse.current.position.ReadValue();
			screenPos = new Vector3(mpos.x, mpos.y, 0f);
		} else {
			screenPos = Input.mousePosition;
		}
		#else
		Vector3 screenPos = Input.mousePosition;
		#endif
		Ray mouseRay = Camera.main.ScreenPointToRay(screenPos);
		if (Physics.Raycast (mouseRay, out rayHit)) {
			ParticleEffectsLibrary.GlobalAccess.SpawnParticleEffect (rayHit.point);
		}
	}

	/// <summary>
	/// User interfaces the button click.
	/// </summary>
	/// <param name="buttonTypeClicked">Button type clicked.</param>
	public void UIButtonClick(ButtonTypes buttonTypeClicked) {
		switch (buttonTypeClicked) {
		case ButtonTypes.Previous:
			// Select Previous Prefab
			SelectPreviousPE();
			break;
		case ButtonTypes.Next:
			// Select Next Prefab
			SelectNextPE();
			break;
		default:
			// Nothing
			break;
		}
	}
}
}