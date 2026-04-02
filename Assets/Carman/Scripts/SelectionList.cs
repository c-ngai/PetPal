using System.Collections.Generic;
using UnityEngine;

public class SelectionList : MonoBehaviour
{
    public List<Highlightable> items = new List<Highlightable>();

    public int currentIndex = 0;

    void Start()
    {
        UpdateHighlight();
    }

    public void MoveLeft()
    {
        currentIndex--;
        ClampIndex();
        UpdateHighlight();
        Debug.Log("Moved to:  "+ items[currentIndex].name);
    }

    public void MoveRight()
    {
        currentIndex++;
        ClampIndex();
        UpdateHighlight();
    }

    public void Confirm()
    {
        Debug.Log("Selected: " + items[currentIndex].name);
        OnItemSelected(currentIndex);
    }

    protected virtual void OnItemSelected(int index)
    {
        // Override in BuildingManager, RoomManager, etc.
    }

    private void ClampIndex()
    {
        if (items.Count == 0) return;
        currentIndex = Mathf.Clamp(currentIndex, 0, items.Count - 1);
    }

    public void UpdateHighlight()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetHighlight(i == currentIndex);
        }
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public int GetMaxIndex()
    {
        return items.Count - 1;
    }
}