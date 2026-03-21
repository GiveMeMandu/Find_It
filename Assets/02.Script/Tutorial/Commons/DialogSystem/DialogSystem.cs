using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public enum Speaker { Rabbit = 0, Yomi = 1 }
public enum PortraitState { Idle, Happy, Sad, Angry, Surprised, Round, Disappear } // 초상화 표정 상태
public enum DialogType { Talk, Image } // 대화 타입 (일반 대화, 이미지 표시)

public class DialogSystem : MonoBehaviour
{
	[SerializeField]
	private Dialog[] dialogs;                       // 현재 분기의 대사 목록
	[SerializeField]
	private Image[] imageDialogs;                   // 대화창 Image UI
	[SerializeField]
	private TextMeshProUGUI[] textNames;                        // 현재 대사중인 캐릭터 이름 출력 Text UI
	[SerializeField]
	private TextMeshProUGUI[] textDialogues;                    // 현재 대사 출력 Text UI
	[SerializeField]
	private GameObject[] objectArrows;                  // 대사가 완료되었을 때 출력되는 커서 오브젝트
	[SerializeField]
	private Animator[] portraitAnimators;              // 캐릭터 초상화 애니메이터
	[SerializeField]
	private Image[] portraitImages;                 // 캐릭터 초상화 이미지
	[SerializeField]
	private Image illustrationImage;              // 일러스트 이미지 UI
	[SerializeField]
	private float imageFadeDuration = 0.5f;       // 이미지 페이드 인/아웃 시간
	[SerializeField]
	private float defaultAnimationDuration = 0.5f;  // 기본 애니메이션 전환 시간
	[SerializeField]
	private float typingSpeed;                  // 텍스트 타이핑 효과의 재생 속도
	[SerializeField]
	private KeyCode keyCodeSkip = KeyCode.Space;    // 타이핑 효과를 스킵하는 키

	private int currentIndex = -1;
	private bool isTypingEffect = false;            // 텍스트 타이핑 효과를 재생중인지
	private bool isPlayingAnimation = false;      // 초상화 애니메이션 재생중인지
	private bool isImageFading = false;          // 이미지 페이드 중인지
	private Speaker currentSpeaker = Speaker.Rabbit;

	public void Setup()
	{
		for (int i = 0; i < 2; ++i)
		{
			// 모든 대화 관련 게임오브젝트 비활성화
			InActiveObjects(i);
		}

		SetNextDialog();
	}

	public bool UpdateDialog()
	{
		if (Input.GetKeyDown(keyCodeSkip) || Input.GetMouseButtonDown(0))
		{
			// 애니메이션이나 페이드 효과 재생 중에는 스킵할 수 없음
			if (isPlayingAnimation || isImageFading)
			{
				return false;
			}

			// 텍스트 타이핑 효과를 재생중일때 마우스 왼쪽 클릭하면 타이핑 효과 종료
			if (isTypingEffect == true)
			{
				// 타이핑 효과를 중지하고, 현재 대사 전체를 출력한다
				StopCoroutine("TypingText");
				isTypingEffect = false;
				textDialogues[(int)currentSpeaker].text = dialogs[currentIndex].dialogue;
				// 대사가 완료되었을 때 출력되는 커서 활성화
				objectArrows[(int)currentSpeaker].SetActive(true);

				return false;
			}

			// 다음 대사 진행
			if (dialogs.Length > currentIndex + 1)
			{
				SetNextDialog();
			}
			// 대사가 더 이상 없을 경우 true 반환
			else
			{
				// 모든 캐릭터 이미지를 어둡게 설정
				for (int i = 0; i < 2; ++i)
				{
					// 모든 대화 관련 게임오브젝트 비활성화
					InActiveObjects(i);
				}

				return true;
			}
		}

		return false;
	}

	private void SetNextDialog()
	{
		// 이전 화자의 대화 관련 오브젝트 비활성화
		InActiveObjects((int)currentSpeaker);

		currentIndex++;

		// 현재 대화 타입에 따라 처리
		if (dialogs[currentIndex].dialogType == DialogType.Image)
		{
			// 이미지 표시 처리
			ShowIllustration();
		}
		else
		{
			// 이미지가 표시중이었다면 페이드 아웃
			if (illustrationImage.gameObject.activeSelf)
			{
				StartCoroutine(FadeImage(false));
			}
			// 일반 대화 처리
			// 현재 화자 설정
			currentSpeaker = dialogs[currentIndex].speaker;

			// 대화창 활성화
			imageDialogs[(int)currentSpeaker].gameObject.SetActive(true);

			// 현재 화자 이름 텍스트 활성화 및 설정
			textNames[(int)currentSpeaker].gameObject.SetActive(true);
			textNames[(int)currentSpeaker].text = dialogs[currentIndex].speaker.ToString();

			// 초상화 상태 업데이트 및 애니메이션 재생
			UpdatePortrait();

			// 화자의 대사 텍스트 활성화 및 설정 (Typing Effect)
			textDialogues[(int)currentSpeaker].gameObject.SetActive(true);
			StartCoroutine(nameof(TypingText));
		}
	}

	private void UpdatePortrait()
	{
		// 초상화 이미지 활성화
		portraitImages[(int)currentSpeaker].gameObject.SetActive(true);

		// 애니메이션 파라미터 설정
		if (portraitAnimators[(int)currentSpeaker] != null)
		{
			portraitAnimators[(int)currentSpeaker].SetTrigger(dialogs[currentIndex].portraitState.ToString());
			StartCoroutine(PlayPortraitAnimation());
		}
	}

	private IEnumerator PlayPortraitAnimation()
	{
		isPlayingAnimation = true;
		// 애니메이션 재생 중 대기 (Dialog에 지정된 시간이 있으면 해당 시간 사용, 없으면 기본값 사용)
		float waitTime = dialogs[currentIndex].animationDuration > 0 ? dialogs[currentIndex].animationDuration : defaultAnimationDuration;
		yield return new WaitForSeconds(waitTime);
		isPlayingAnimation = false;
	}

	private void ShowIllustration()
	{
		if (dialogs[currentIndex].illustration != null)
		{
			illustrationImage.gameObject.SetActive(true);
			illustrationImage.sprite = dialogs[currentIndex].illustration;
			StartCoroutine(FadeImage(true));
		}
	}

	private IEnumerator FadeImage(bool fadeIn)
	{
		isImageFading = true;
		float elapsed = 0f;
		Color color = illustrationImage.color;

		while (elapsed < imageFadeDuration)
		{
			elapsed += Time.deltaTime;
			float alpha = fadeIn ?
				Mathf.Lerp(0f, 1f, elapsed / imageFadeDuration) :
				Mathf.Lerp(1f, 0f, elapsed / imageFadeDuration);

			illustrationImage.color = new Color(color.r, color.g, color.b, alpha);
			yield return null;
		}

		if (!fadeIn)
		{
			illustrationImage.gameObject.SetActive(false);
		}
		isImageFading = false;
	}

	private void InActiveObjects(int index)
	{
		imageDialogs[index].gameObject.SetActive(false);
		textNames[index].gameObject.SetActive(false);
		textDialogues[index].gameObject.SetActive(false);
		objectArrows[index].gameObject.SetActive(false);
		portraitImages[index].gameObject.SetActive(false);
	}

	private IEnumerator TypingText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		isTypingEffect = true;

		for (int i = 0; i < dialogs[currentIndex].dialogue.Length; i++)
		{
			stringBuilder.Append(dialogs[currentIndex].dialogue[i]);
			textDialogues[(int)currentSpeaker].text = stringBuilder.ToString();
			yield return new WaitForSeconds(typingSpeed);
		}

		isTypingEffect = false;
		objectArrows[(int)currentSpeaker].SetActive(true);
	}
}

[System.Serializable]
public struct Dialog
{
	public DialogType dialogType;      // 대화 타입 (일반 대화 or 이미지)
	public Speaker speaker;      // 화자
	public PortraitState portraitState;   // 초상화 표정 상태
	public float animationDuration; // 애니메이션 재생 시간 (0 이하면 기본값 사용)
	public Sprite illustration;     // 표시할 이미지
	[TextArea(3, 5)]
	public string dialogue;      // 대사 또는 이미지 설명
}

