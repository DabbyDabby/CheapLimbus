using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using Vector2 = System.Numerics.Vector2;
using System.Collections.Generic;

public class CursorController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform rectTransform, socketPos;
    //[SerializeField] private List<SkillSlot> selectedSkills = new List<SkillSlot>();
    
    private CanvasGroup canvasGroup;
    

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {   
        Debug.Log("OnPointerDown");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        //Allows the cursor to traverse through the socket
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        //rectTransform.anchoredPosition = transform.DOMove(defaultPos, 0.5f);
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
