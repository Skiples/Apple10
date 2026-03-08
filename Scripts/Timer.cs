using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    Action onTimeOver;
    public Image timerBar;
    public TMP_Text timerText;
    float maxTime = 60, time = 0;
    float timePer;
    public bool isPause = false;

    void Update()
    {
        if(!isPause)
            TimeLoss(Time.deltaTime);
    }
    public void Initialized(float _time, Action _onTimeOver = null)
    {
        onTimeOver = _onTimeOver;
        time = maxTime = _time;
        timePer = 1 / maxTime;
        if (!transform.Find("bar").TryGetComponent(out timerBar))
            Debug.LogError("Fail TryGetComponent timerBar");
        if (!transform.Find("text").TryGetComponent(out timerText))
            Debug.LogError("Fail TryGetComponent timerText");
    }
    void TimeLoss(float _time)
    {
        if (time <= 0) return;
        time -= _time;
        timerBar.fillAmount = time * timePer;
        timerText.SetText("{0}s", (int)time);
        TimerCheck();
    }
    void TimerCheck()
    {
        if (time > 0) return;
        onTimeOver?.Invoke();
        onTimeOver = null;
    }
}
