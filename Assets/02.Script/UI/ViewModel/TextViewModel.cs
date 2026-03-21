using TMPro;
using UI;
using UnityEngine;
using UnityWeld.Binding;

[Binding]
public class TextViewModel : BaseViewModel
{
    [Binding]
    public string Text
    {
        get => textComponent.text;
        set
        {
            if (textComponent.text != value)
            {
                textComponent.text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
    }
    public TextMeshProUGUI textComponent;

    protected override void Awake()
    {
        base.Awake();
        if(textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    private void OnTextChanged(Object obj)
    {
        if (obj == textComponent)
        {
            OnPropertyChanged(nameof(Text));
        }
    }
}
