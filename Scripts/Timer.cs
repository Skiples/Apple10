using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    Action onTimeOver;
    Image timerBar;
    float maxTime = 60;
    int score = 0;
    float time = 0;
    float timePer;

    void Update()
    {
        TimeLoss(Time.deltaTime);
    }
    public void Initialized(float _time, Action _onTimeOver = null)
    {
        onTimeOver = _onTimeOver;
        time = maxTime = _time;
        timePer = 1 / maxTime;
        if (!transform.Find("bar").TryGetComponent(out timerBar))
            Debug.LogError("Fail TryGetComponent timerBar");
    }
    void TimeLoss(float _time)
    {
        if (time <= 0) return;
        time -= _time;
        timerBar.fillAmount = time * timePer;
        TimerCheck();
    }
    void TimerCheck()
    {
        if (time > 0) return;
        onTimeOver?.Invoke();
        onTimeOver = null;
    }
}
