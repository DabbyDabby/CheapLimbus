using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float rise = 60f;
    [SerializeField] private float life = .6f;
    
    TMP_Text        label;
    RectTransform   rt;
    CanvasGroup     cg;
    Vector2         start;

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
            canvas, screenPos, null, out Vector2 local);
        rt.anchoredPosition = start = local;
        
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
            cg.alpha = 1 - k;
            yield return null;
        }
        Destroy(gameObject);
    }
}