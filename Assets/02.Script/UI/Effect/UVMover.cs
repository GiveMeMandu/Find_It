using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UVMover : MonoBehaviour
{
    [SerializeField] private Image BackGround;
    [SerializeField] private bool RandomizeBGColor;
    [SerializeField] private bool RandomizeIconColor;
    private RawImage rawImage;
    private Rect uvRect;

    public float speed = 1.0f;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        if (RandomizeBGColor)
        {
            Color background = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));
            BackGround.color = background;
        }
        if (RandomizeIconColor)
        {
            Color ScrollColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));
            rawImage.color = ScrollColor;
        }
    }

    void Update()
    {
        uvRect = rawImage.uvRect;
        uvRect.x += Time.deltaTime * speed;
        uvRect.y += Time.deltaTime * speed;
        rawImage.uvRect = uvRect;
    }
}