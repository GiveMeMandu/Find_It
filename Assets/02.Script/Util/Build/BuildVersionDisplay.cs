using UnityEngine;
using TMPro;

/// <summary>
/// 시작 시점에 대상 TMP 텍스트를 현재 앱 버전(Application.version)으로 갱신합니다.
/// </summary>
[DisallowMultipleComponent]
public class BuildVersionDisplay : MonoBehaviour
{
	[Header("Target")]
	[SerializeField] private TMP_Text target;

	[Header("Formatting")]
	[SerializeField] private string prefix = "v";    // 예: "v" -> v1.2.3
	[SerializeField] private string suffix = "";      // 예: "" 또는 " (beta)"

	[Header("Lifecycle")]
	[SerializeField] private bool updateOnStart = true; // Start에서 자동 반영 여부

	private void Reset()
	{
		// 동일 객체에 TMP_Text가 있으면 자동 할당
		if (target == null)
			target = GetComponent<TMP_Text>();
	}

	private void Start()
	{
		if (updateOnStart)
			UpdateText();
	}

	/// <summary>
	/// 수동으로 텍스트를 갱신합니다. 우클릭 컨텍스트 메뉴에서도 호출 가능.
	/// </summary>
	[ContextMenu("Refresh Version Text")]
	public void Refresh() => UpdateText();

	/// <summary>
	/// Application.version을 읽어 대상 TMP 텍스트에 반영합니다.
	/// </summary>
	public void UpdateText()
	{
		if (target == null)
			target = GetComponent<TMP_Text>();

		if (target == null)
		{
			Debug.LogWarning($"[BuildVersionDisplay] TMP_Text 대상이 없습니다. 오브젝트 '{name}'에 TMP_Text를 추가하거나 'target'에 할당하세요.");
			return;
		}

		string version = Application.version; // Player Settings > Version
		target.text = string.Concat(prefix ?? string.Empty, version, suffix ?? string.Empty);
	}
}

