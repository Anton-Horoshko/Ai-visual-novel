using System.Collections;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour
{
    public float animationSpeed = 0.05f;
    public float scaleSize = 1.07f;

    private GameObject button;

    private void Awake()
    {
        button = gameObject;
    }

    public void PlayAnimation()
    {
        StartCoroutine(AnimateScale());
    }

    IEnumerator AnimateScale()
    {
        Vector3 originalScale = button.transform.localScale;
        Vector3 targetScale = originalScale * scaleSize;

        float timer = 0f;

        while (timer < animationSpeed)
        {
            button.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / animationSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        button.transform.localScale = targetScale;

        yield return new WaitForSeconds(0.05f);

        timer = 0f;
        while (timer < animationSpeed)
        {
            button.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / animationSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        button.transform.localScale = originalScale;
    }
}
