using UnityEngine;

public class BackgroundAnimator : MonoBehaviour
{
    public float panSpeed = 5f;       
    public float zoomSpeed = 0.05f;   
    public bool rotateBg = false;     

    private RectTransform rectTransform;
    private Vector2 startPos;
    private float timeOffset;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
        timeOffset = Random.Range(0f, 100f); 
    }

    void Update()
    {
        float t = Time.time + timeOffset;

        float newY = startPos.y + Mathf.Sin(t * 0.5f) * panSpeed;
        rectTransform.anchoredPosition = new Vector2(startPos.x, newY);

        float scale = 1f + (Mathf.Sin(t * 0.3f) * zoomSpeed);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (rotateBg)
        {
            transform.Rotate(0, 0, 0.5f * Time.deltaTime);
        }
    }
}
