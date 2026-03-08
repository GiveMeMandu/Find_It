using UnityEngine;
using UnityEngine.Events;

public class ToggleSpriteHelper : MonoBehaviour
{
    public Sprite spriteA;
    public Sprite spriteB;

    public UnityEvent OnToggleToA = new UnityEvent();
    public UnityEvent OnToggleToB = new UnityEvent();

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ToggleSprite()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        Sprite current = spriteRenderer.sprite;

        if (spriteA != null && spriteB != null)
        {
            if (current == spriteA)
            {
                spriteRenderer.sprite = spriteB;
                OnToggleToB.Invoke();
            }
            else
            {
                spriteRenderer.sprite = spriteA;
                OnToggleToA.Invoke();
            }
        }
        else if (spriteA != null)
        {
            if (current == spriteA)
            {
                spriteRenderer.sprite = null;
                OnToggleToB.Invoke();
            }
            else
            {
                spriteRenderer.sprite = spriteA;
                OnToggleToA.Invoke();
            }
        }
        else if (spriteB != null)
        {
            if (current == spriteB)
            {
                spriteRenderer.sprite = null;
                OnToggleToA.Invoke();
            }
            else
            {
                spriteRenderer.sprite = spriteB;
                OnToggleToB.Invoke();
            }
        }
    }

    public void SetToA()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteA == null) return;
        spriteRenderer.sprite = spriteA;
        OnToggleToA.Invoke();
    }

    public void SetToB()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteB == null) return;
        spriteRenderer.sprite = spriteB;
        OnToggleToB.Invoke();
    }
}
