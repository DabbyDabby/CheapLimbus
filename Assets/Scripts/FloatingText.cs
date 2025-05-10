using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float rise = 60f;
    [SerializeField] private float life = .6f;
    
    TMP_Text        _label;
    RectTransform   _rt;
    CanvasGroup     _cg;
    Vector2         _start;

    void Awake()
    {
        _label = GetComponent<TMP_Text>();
        _cg    = GetComponent<CanvasGroup>();
        _rt    = GetComponent<RectTransform>();
    }

    public void Init(string txt, Color col, Vector2 screenPos, RectTransform canvas)
    {
        _label.text  = txt;
        _label.color = col;
        _cg.alpha    = 1f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas, screenPos, null, out Vector2 local);
        _rt.anchoredPosition = _start = local;
        
        StartCoroutine(Life());
    }

    System.Collections.IEnumerator Life()
    {
        float t = 0;
        while (t < life)
        {
            t += Time.deltaTime;
            float k = t / life;
            _rt.anchoredPosition = _start + Vector2.up * (rise * k);
            _cg.alpha = 1 - k;
            yield return null;
        }
        Destroy(gameObject);
    }
}