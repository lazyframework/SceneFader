using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AutoFitBackground : MonoBehaviour
{
    private Image img;
    private RectTransform rt;

    void Awake()
    {
        img = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        img.preserveAspect = true; 
    }

    void Update()
    {
        if (img.sprite == null) return;

        float spriteWidth = img.sprite.rect.width;
        float spriteHeight = img.sprite.rect.height;
        float spriteAspect = spriteWidth / spriteHeight;

        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRt = canvas.GetComponent<RectTransform>();
        float screenWidth = canvasRt.rect.width;
        float screenHeight = canvasRt.rect.height;
        float screenAspect = screenWidth / screenHeight;

        Vector2 newSize;
        if (screenAspect > spriteAspect)
        {
            newSize = new Vector2(screenWidth, screenWidth / spriteAspect);
        }
        else
        {
            newSize = new Vector2(screenHeight * spriteAspect, screenHeight);
        }

        float margin = 1.1f;
        rt.sizeDelta = newSize * margin;
    }
}
