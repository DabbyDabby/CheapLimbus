using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineDrag : MonoBehaviour
{
    public List<Image> currentLine;
    private Vector3 currentStartPos;
    private Vector3 firstStartPos;
    public void AddNewSegment(Vector3 endPosition)
    {
        //var segment = Instantiate() as GameObject;
        //segment.transform.SetParent(transform);
        //segment.transform.positon = //calculate it;
        //segment.transform.right = (position - currentStartPos).normalized;
        //remember the uv
    }
    
    public void RemoveCurrentSegment()
    {
        var segment = currentLine[^1];
        currentLine.Remove(segment);
        Destroy(segment.gameObject);
        
    }

    public void EditCurrentSegment(Vector3 endPosition)
    {
        //currentLine[^1].transform.position = //calculate it;
        //segment.transform.right = (position - currentStartPos).normalized;
        //remember the uv
        if (endPosition.x - currentStartPos.x > 200)
        {
            RemoveCurrentSegment();
        }
    }
}
