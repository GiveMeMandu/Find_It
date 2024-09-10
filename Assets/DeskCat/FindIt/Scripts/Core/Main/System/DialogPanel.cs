using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class DialogPanel : MonoBehaviour
    {
        [Header("Dialog UI Inspector")]
        public GameObject MainDialogPanel;
        public Image BlurBackgroundImage;
        public Image CharacterImage;
        public Animator DialogAnimator;
        public AudioSource DialogAudio;
        public Text DialogText;
        public Button DialogBtn;
        
        [Header("Dialog Panel Variable")]
        public bool IsPlayOnStart = true;
        public UnityEvent DialogStartEvent;
        public UnityEvent DialogEndEvent;
        
        [Header("Dialog Content")] [NonReorderable]
        public List<MultiDialogTextListModel> DialogContent = new();

        private Dictionary<string, List<DialogListModel>> DialogContentDic = new();

        private int CurrentDialogIndex = 0;

        private void Start()
        {
            BuildDictionary();
            DialogBtn.onClick.AddListener(PlayDialog);
            if (IsPlayOnStart)
            {
                MainDialogPanel.SetActive(true);
                PlayDialog();
            }
            else
            {
                MainDialogPanel.SetActive(false);
            }
        }
        
        public void PlayDialog()
        {
            MainDialogPanel.SetActive(true); 
            DialogAnimator.gameObject.SetActive(true);
            DialogAnimator.Play(0);
            DialogAudio.Play();
            
            if (CurrentDialogIndex >= DialogContentDic[GlobalSetting.CurrentLanguage].Count)
            {
                DialogEnd();
                return;
            }

            CharacterImage.sprite = DialogContentDic[GlobalSetting.CurrentLanguage][CurrentDialogIndex].CharacterSprite;
            DialogText.text = DialogContentDic[GlobalSetting.CurrentLanguage][CurrentDialogIndex].Value;
            DialogContentDic[GlobalSetting.CurrentLanguage][CurrentDialogIndex].Event?.Invoke();
            if (CurrentDialogIndex == 0) {
                DialogStartEvent?.Invoke();
            }
            CurrentDialogIndex++;
        }

        private void DialogEnd()
        {
            MainDialogPanel.SetActive(false);
            DialogText.text = "";
            DialogEndEvent?.Invoke();
        }
        

        private void BuildDictionary()
        {
            DialogContentDic = new Dictionary<string, List<DialogListModel>>();
            foreach (var dialogModel in DialogContent)
            {
                DialogContentDic.TryAdd(dialogModel.LanguageKey, dialogModel.Value);
            }
        }
    }
}