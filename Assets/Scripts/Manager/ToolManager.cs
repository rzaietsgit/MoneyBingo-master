using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager
{
    public static void SetMiddleAnchor(RectTransform left, RectTransform right, float midInterval = 20)
    {
        GameManager.Instance.StartCoroutine(DelaySetAnchor(left, right, midInterval));
    }
    private static IEnumerator DelaySetAnchor(RectTransform left, RectTransform right, float midInterval)
    {
        yield return null;
        float leftSizeX = left.sizeDelta.x;
        float rightSizeX = right.sizeDelta.x;
        float totalSizeX = leftSizeX + rightSizeX;
        left.localPosition = new Vector3(-totalSizeX / 2 + leftSizeX / 2 - midInterval / 2, left.localPosition.y);
        right.localPosition = new Vector3(totalSizeX / 2 - rightSizeX / 2 + midInterval / 2, right.localPosition.y);
    }
}
[System.Serializable]
public class Range
{
    [SerializeField]
    private int min;
    [SerializeField]
    private int max;
    public int Max { get { return max; } }
    public int Min { get { return min; } }
    public Range(int min, int max)
    {
        if (min < max)
        {
            this.min = min;
            this.max = max;
        }
        else
        {
            this.min = max;
            this.max = min;
        }
    }
    public int RandomIncludeMax()
    {
        return Random.Range(min, max + 1);
    }
    public int RandomExcludeMax()
    {
        return Random.Range(min, max);
    }
}
