using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UILineDrag : MonoBehaviour
{
    [SerializeField] private RectTransform originPoint;  // Starting point
    [SerializeField] private RectTransform lineSegmentContainer;  // Parent for segments
    [SerializeField] private GameObject segmentPrefab;  // Line segment prefab
    [SerializeField] private CursorController cursor;

    public Vector3 currentStartPos;
    private List<RawImage> _currentLine = new List<RawImage>();
    [SerializeField] private RectTransform lineOriginPoint;

    public float chainSizeScale = 1;
    

    private void Start()
    {
        ResetLineStart();  // Set the initial start point to the origin
    }

    // Method for setting the next line segment's starting position
    public void SetStartPos(Vector3 pos)
    {
        currentStartPos = pos;
    }

    // Method for erasing all line segments and resetting the starting position
    public void ResetLineStart()
    {
        if (_currentLine == null) return;
        SetStartPos(originPoint.localPosition);

        // Destroy all existing segments
        foreach (RawImage segment in _currentLine)
        {
            Destroy(segment.gameObject);
        }
        _currentLine.Clear();
    }

    // Method for adding a new line segment
    public void AddNewSegment(Vector3 endPosition)
    {
        // localEndPos equals the last position of the cursor, usually at a skill slot or origin point
        Vector3 localEndPos = endPosition;
        
        // Instantiate new line segment prefab and get its rect transform to refer to later
        GameObject segment = Instantiate(segmentPrefab, lineSegmentContainer);
        RectTransform segmentRect = segment.GetComponent<RectTransform>();
        
        // Set its pivot to LEFT
        segmentRect.pivot = new Vector2(0f, 0.5f);
        Vector3 localStartPos;

        // If the cursor is currently outside a skill's radius, set the line segment's to the true origin point
        // Otherwise, set it to the last selected skill slot's position
        if (!cursor.snapped)
        {
            localStartPos = originPoint.localPosition;
        } else {
            localStartPos = currentStartPos;
        }
        
        // Set the line segment's anchor position (its starting point)
        segmentRect.anchoredPosition = localStartPos;

        // Calculate direction and length
        Vector2 direction = (localEndPos - localStartPos).normalized;
        float distance = Vector2.Distance(localStartPos, localEndPos);

        // Set correct size and orientation
        segmentRect.localScale = Vector3.one;
        segmentRect.sizeDelta = new Vector2(distance, segmentRect.sizeDelta.y);
        segmentRect.transform.right = new Vector3(direction.x, direction.y, 0);

        _currentLine.Add(segment.GetComponent<RawImage>());
        currentStartPos = localEndPos;
    }


    // Method for altering the current line segment (mouse tracking)
    public void EditCurrentSegment(Vector3 endPosition)
    {
        if (_currentLine == null || _currentLine.Count == 0) return;
        
        // Reference to the last segment in the list (currently tracking the mouse) and set its endpoint to the mouse's position
        RectTransform lastSegment = _currentLine[^1].rectTransform;
        Vector3 localEndPos = endPosition;
        
        // Set its starting position, can either be true origin point or last selected skill slot position
        Vector3 localStartPos = currentStartPos;
        
        // Calculate distance between the current line's start and end point
        Vector2 direction = (localEndPos - localStartPos).normalized;
        float distance = Vector2.Distance(localStartPos, localEndPos);

        // Update the current segment’s size and orientation
        lastSegment.sizeDelta = new Vector2(distance, lastSegment.sizeDelta.y);
        var uvWidth = distance / lastSegment.sizeDelta.y; //"normalization"
        _currentLine[^1].uvRect = new Rect(-uvWidth / chainSizeScale, 0, uvWidth / chainSizeScale, 1);
        lastSegment.transform.right = new Vector3(direction.x, direction.y, 0);
    }
}