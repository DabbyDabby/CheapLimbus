using UnityEngine;

[RequireComponent(typeof(Unit))]
public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private FloatingText popupPrefab;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Color critColor   = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    Unit         unit;
    Camera       cam;

    void Awake()
    {
        unit = GetComponent<Unit>();
        cam  = Camera.main;

        unit.OnDamaged += Spawn;
    }

    void Spawn(Unit _, int amount)
    {
        Vector3 chest = unit.transform.position +
                        Vector3.up * unit.SpriteRenderer.bounds.extents.y * 0.4f;

        Vector2 screenPos = cam.WorldToScreenPoint(chest);
        var ft = Instantiate(popupPrefab, worldCanvas.transform);
        ft.Init("-" + amount, normalColor, screenPos, (RectTransform)worldCanvas.transform);
    }

    void OnDestroy() => unit.OnDamaged -= Spawn;
}