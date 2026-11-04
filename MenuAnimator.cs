using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float delay = 0f;
    public float tapScale = 0.9f; 
    public float rotateSpeed = 10f;

    private Vector3 originalScale;
    private bool isAnimating = false;

    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero; 
        StartCoroutine(AnimatePopIn());
    }

    void Update()
    {
        if (!isAnimating)
        {
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        }
    }

    IEnumerator AnimatePopIn()
    {
        yield return new WaitForSeconds(delay);
        isAnimating = true;
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime / animationDuration;
            float curve = 1 - Mathf.Pow(1 - t, 3);
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, originalScale, curve);
            yield return null;
        }
        transform.localScale = originalScale;
        isAnimating = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isAnimating) transform.localScale = originalScale * tapScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isAnimating) transform.localScale = originalScale;
    }
}
