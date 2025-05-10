using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EKG_UI : MonoBehaviour
{
    [SerializeField] private Image[] beats;         // assign 6 images
    [SerializeField] private TMP_Text dmgPopup;
    [SerializeField] private Vector2 offset = new(0, -40);
    [SerializeField] private bool debugShowBar; // inspector toggle
    [SerializeField] private FloatingText floatingTextPrefab;


    private Unit _target;
    private const float PopupFade = .6f;
    private Camera _cam;
    private RectTransform _canvasRect;
    private RectTransform _selfRT;

    public void Init(Unit u, Camera worldCam, RectTransform canvasR)
    {
        _target = u;
        _cam = worldCam;
        _canvasRect = canvasR;
        _selfRT = GetComponent<RectTransform>();
        
        u.OnDamaged += OnDmg;
        u.OnHealed  += OnHeal;
        u.OnDeath   += _ => OnDmg(u, 0);
        Refresh();
    }

    void LateUpdate()                          // only ONE LateUpdate
    {
        if (!_target) return;

        Vector3 screen = _cam.WorldToScreenPoint(_target.transform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screen, _cam, out Vector2 local);

        _selfRT.anchoredPosition = local + offset;   // UI pixels below sprite
    }

    /* ───────────────── events ───────────────── */
    private void OnDmg(Unit _, int amt)
    {
        if (amt > 0) SpawnFloatingText($"-{amt}", Color.red);
        Refresh();
    }

    private void OnHeal(Unit _, int amt)
    {
        if (amt > 0) SpawnFloatingText($"-{amt}", Color.red);
        Refresh();
    }

    /* ───────────────── helpers ───────────────── */
    private void Refresh()
    {
        float hpPct = _target.CurrentHp / (float)_target.MaxHp;
        int   lit   = Mathf.CeilToInt(hpPct * beats.Length);

        for (int i = 0; i < beats.Length; i++)
        {
            beats[i].enabled = debugShowBar || i < lit;
            beats[i].color   = hpPct switch
            {
                > .5f => Color.green,
                > .25f => new Color(1f, .8f, 0f),  // yellow
                _ => Color.red
            };
        }
    }
    private void SpawnFloatingText(string txt, Color col)
    {
        if (!floatingTextPrefab) return;

        // 1) compute chest position in screen coordinates
        float chestOffset = _target.SpriteRenderer.bounds.size.y * 0.4f;
        Vector3 chestWorld  = _target.transform.position + new Vector3(0, chestOffset, 0);
        Vector2 chestScreen = _cam.WorldToScreenPoint(chestWorld);

        // 2) instantiate and initialise
        FloatingText ft = Instantiate(floatingTextPrefab, _canvasRect);
        ft.Init(txt, col, chestScreen, _canvasRect);
    }

}
