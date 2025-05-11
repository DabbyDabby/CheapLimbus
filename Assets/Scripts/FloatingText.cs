using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] float life = .8f;    // seconds
    [SerializeField] float rise = 60f;    // pixels
    [SerializeField] AnimationCurve alphaCurve =
        AnimationCurve.EaseInOut(0,1,1,0);      // smoky fade

    TMP_Text      label;
    CanvasGroup   cg;
    RectTransform rt;

    Vector2 start;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        cg    = GetComponent<CanvasGroup>();
        rt    = GetComponent<RectTransform>();
    }

    public void Init(string txt, Color col, Vector2 screenPos, RectTransform canvas)
    {
        label.text  = txt;
        label.color = col;
        cg.alpha    = 1f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas, screenPos, null, out start);
        rt.anchoredPosition = start;

        StartCoroutine(Life());
    }

    System.Collections.IEnumerator Life()
    {
        float t = 0;
        while (t < life)
        {
            t += Time.deltaTime;
            float k = t / life;
            rt.anchoredPosition = start + Vector2.up * rise * k;
            cg.alpha = alphaCurve.Evaluate(k);
            yield return null;
        }
        Destroy(gameObject);
    }
}