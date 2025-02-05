using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionSlot : MonoBehaviour
{
    Image img;
    TMP_Text txt;
    Color selectedTextColor = Color.white;
    Color defaulfTextColor = Color.black;
    Color selectedImageColor = new Color(0, 0, 0, .5f);
    Color defaulfImageColor = new Color(.5f, .5f, .5f, .5f);
    int number = 0;
    public int GetNumber => number;
    public void Initialized(int _num)
    {
        number = _num;
        TryGetComponent(out img);
        txt = GetComponentInChildren<TMP_Text>();
        txt.text = _num.ToString();
        Selected(false);
    }

    public void Selected(bool _isSelected)
    {
        img.color = _isSelected ? selectedImageColor : defaulfImageColor;
        txt.color = _isSelected ? selectedTextColor : defaulfTextColor;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
    public bool IsActive => gameObject.activeSelf;
}
