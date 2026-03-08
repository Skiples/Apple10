using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextSlider : MonoBehaviour
{
    public Action<float> onValueChanged;
    Slider slider;
    public float value => slider.value;
    [SerializeField] int test;
    private void Awake()
    {
        if (!TryGetComponent(out TMP_Text txt))
            Debug.LogError($"No TMP_Text in {name}");
        if (!TryGetComponent(out slider))
            Debug.LogError($"No Slider in {name}");
        int maxCount = txt.text.Length;
        slider.maxValue = maxCount;
        slider.minValue = 0;
        slider.wholeNumbers = true;
        slider.onValueChanged.AddListener((f) =>
        {
            test = txt.maxVisibleCharacters = (int)f;
            onValueChanged?.Invoke(f);
        });
    }
    public void SetValue(float _value) => slider.value = _value;


}
