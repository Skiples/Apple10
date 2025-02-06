using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class UIDragSelection : MonoBehaviour
{
    Dictionary<SelectionSlot, Vector2> selectableObjects = new Dictionary<SelectionSlot, Vector2>();
    Dictionary<SelectionSlot, Vector2> selectedObjects = new Dictionary<SelectionSlot, Vector2>();
    Action<int> onSelection;
    Action onClear;
    RectTransform selectionBox;
    RectTransform rtf;
    Vector2 startPos;
    Rect selectionRect;

    void Update()
    {
        Mouse();
    }

    public void Initialized(Action<int> _onSelection = null, Action _onClear = null)
    {
        onSelection = _onSelection;
        onClear = _onClear;
        rtf = transform as RectTransform;
        var slotParent = transform.Find("grid");
        var boxPrefab = Resources.Load<GameObject>("Prefabs/box");
        var slotPrefab = Resources.Load<GameObject>("Prefabs/slot");
        selectionBox = Instantiate(boxPrefab, transform).transform as RectTransform;
        selectionBox.gameObject.SetActive(false);

        selectableObjects.Clear();
        for (int i = 0; i < 170; i++)
        {
            var obj = Instantiate(slotPrefab, slotParent);
            obj.TryGetComponent(out SelectionSlot slot);
            slot.Initialized(UnityEngine.Random.Range(1, 10));
            LayoutRebuilder.ForceRebuildLayoutImmediate(slotParent as RectTransform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtf, obj.transform.position, null, out Vector2 pos);
            selectableObjects.Add(slot, pos);
        }
        slotParent.TryGetComponent(out GridLayoutGroup grid);
        Destroy(grid);
    }
    void Mouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtf, Input.mousePosition, null, out startPos);
            selectionBox.anchoredPosition = startPos;
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
        }
        else if (Input.GetMouseButton(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtf, Input.mousePosition, null, out Vector2 endPos);
            Vector2 size = endPos - startPos;
            size = new Vector2(Abs(size.x), Abs(size.y));
            Vector2 min = new Vector2(Mathf.Min(startPos.x, endPos.x), Mathf.Min(startPos.y, endPos.y));
            selectionRect = new Rect(min, size);
            selectionBox.sizeDelta = size;
            selectionBox.anchoredPosition = min + size * .5f;
            DetectObjectsInSelection();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selectionBox.gameObject.SetActive(false);
            SelectedObjectEvent();
            ObjectsToSelect(false);
        }
    }
    void DetectObjectsInSelection()
    {
        foreach (var obj in selectableObjects)
        {
            obj.Key.Selected(selectionRect.Contains(obj.Value));
        }
    }
    void ObjectsToSelect(bool _select)
    {
        foreach (var obj in selectableObjects)
            obj.Key.Selected(_select);
    }

    void SelectedObjectEvent()
    {
        selectedObjects.Clear();
        int total = 0;
        foreach (var dict in selectableObjects)
        {
            if (!selectionRect.Contains(dict.Value))
                continue;
            total += dict.Key.GetNumber;
            selectedObjects.Add(dict.Key, dict.Value);
        }

        if (total != 10) return;

        foreach (var dict in selectedObjects)
        {
            selectableObjects.Remove(dict.Key);
            dict.Key.Disable();
        }

        onSelection?.Invoke(selectedObjects.Count);

        if (onClear == null)
            return;
        foreach (var dict in selectableObjects)
            if (dict.Key.IsActive)
                return;
        

        onClear.Invoke();


    }

    float Abs(float x) => x > 0 ? x : -x;
}
