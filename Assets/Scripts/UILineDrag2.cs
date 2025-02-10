using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UILineDrag2 : MonoBehaviour
{
    [SerializeField] private RectTransform originPoint;  // Starting point
    [SerializeField] private RectTransform lineSegmentContainer;  // Parent for segments
    [SerializeField] private GameObject segmentPrefab;  // Line segment prefab
    [SerializeField] private GameObject wholeCanvas;

    private Vector3 currentStartPos;
    private List<RawImage> currentLine = new List<RawImage>();
    [SerializeField] private RectTransform lineOriginPoint;

    public float chainSizeScale = 1;
    

    private void Start()
    {
        ResetLineStart();  // Set the initial start point to the origin
        RectTransform canvas = wholeCanvas.GetComponent<RectTransform>();
    }

    public void ResetLineStart()
    {
        currentStartPos = originPoint.localPosition;

        // Destroy all existing segments
        foreach (RawImage segment in currentLine)
        {
            Destroy(segment.gameObject);
        }
        currentLine.Clear();
    }

    public void AddNewSegment(Vector3 endPosition)
    {
        //Debug.Log($"🔹 Adding new segment: Start {currentStartPos} → End {endPosition}");

        if (segmentPrefab == null)
        {
            Debug.LogError("❌ segmentPrefab is missing! Assign it in the Inspector.");
            return;
        }

        // Use local space relative to LineSegmentContainer
        Vector3 localStartPos = originPoint.localPosition;
        Vector3 localEndPos = endPosition;

        GameObject segment = Instantiate(segmentPrefab, lineSegmentContainer);
        RectTransform segmentRect = segment.GetComponent<RectTransform>();

        segmentRect.pivot = new Vector2(0f, 0.5f);
        segmentRect.anchoredPosition = localStartPos;

        // Calculate direction and length
        Vector2 direction = (localEndPos - localStartPos).normalized;
        float distance = Vector2.Distance(localStartPos, localEndPos);

        // Set correct size and orientation
        segmentRect.localScale = Vector3.one;
        segmentRect.sizeDelta = new Vector2(distance, segmentRect.sizeDelta.y);
        segmentRect.transform.right = new Vector3(direction.x, direction.y, 0);

        currentLine.Add(segment.GetComponent<RawImage>());
        currentStartPos = localEndPos;
    }



    public void EditCurrentSegment(Vector3 endPosition)
    {
        if (currentLine == null || currentLine.Count == 0) return;

        RectTransform lastSegment = currentLine[^1].rectTransform;

        Vector3 localEndPos = endPosition;
        Vector3 localStartPos = currentStartPos;
        
        Vector2 direction = (localEndPos - localStartPos).normalized;
        float distance = Vector2.Distance(localStartPos, localEndPos);

        // Update the current segment’s size and orientation
        lastSegment.sizeDelta = new Vector2(distance, lastSegment.sizeDelta.y);
        var uvWidth = distance / lastSegment.sizeDelta.y; //"normalization"
        currentLine[^1].uvRect = new Rect(-uvWidth / chainSizeScale, 0, uvWidth / chainSizeScale, 1);
        lastSegment.transform.right = new Vector3(direction.x, direction.y, 0);
    }
}
