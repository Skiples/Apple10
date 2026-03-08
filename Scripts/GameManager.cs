using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Theme;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] Apple10TextBox apple10;
    //[SerializeField] Transform resultWindow;
    [SerializeField] TMP_Text scoreTxt, highScoreTxt;
    [SerializeField] Button hintBtn, debugBtn;
    [SerializeField] Timer timer;
    [SerializeField] Animator guide;
    [SerializeField] UISetting setting;
    //Action onGameEnd;
    Coroutine scoreCoroutine;
    const string STATE = "State";
    public int score, addScore,highScore;
    public bool isPause, isSuffle;
    bool isTutorial;
    bool usedHint, allowHint;

    public override void Initialize()
    {
        hintBtn.onClick.AddListener(UseHint);
        debugBtn.onClick.AddListener(ShowSetting);
        isTutorial = SaveManager.I.GetData(EnumData.IsTutorialEnd, 0) == 0;
        PlayGame();
        setting.gameObject.SetActive(true);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ShowSetting();
    }

    public void PlayGame()
    {
        byte[,] arr = null;
        if (isTutorial)
        {
            arr = new byte[3, 3] {
            {1,1,2 },
            {1,8,1 },
            {4,9,3 } };
            scoreTxt.gameObject.SetActive(false);
            hintBtn.gameObject.SetActive(false);
            debugBtn.gameObject.SetActive(false);
            timer.gameObject.SetActive(false);
            guide.SetInteger(STATE, 0);
        }
        else
        {
            CheckAllowHint();
            scoreTxt.gameObject.SetActive(true);
            debugBtn.gameObject.SetActive(true);
            timer.gameObject.SetActive(true);
            timer.Initialized(120, OnGameEnd);
            AddScore(addScore = score = 0);
            highScore = SaveManager.I.GetData(EnumData.HighScore, 0);
        }
        guide.gameObject.SetActive(isTutorial);
        apple10.Initialized(arr);

        //resultWindow.gameObject.SetActive(false);

        isPause = false;
        usedHint = false;
    }
    public void CheckAllowHint()
    {
        allowHint = SaveManager.I.GetData(EnumData.Hint, 0) == 1;
        hintBtn.gameObject.SetActive(allowHint);
    }

    public void SetPause(bool _pause)
    {
        if (!timer.gameObject.activeSelf)
            return;
        var theme = ThemeManager.Instance.currentTheme;
        Color color = _pause ? theme.number : theme.textMain;
        timer.timerBar.color = timer.timerText.color = color;
        //Time.timeScale = _pause;
        timer.isPause = isSuffle || (isPause = _pause);
    }
    public void SetHintToggle(bool _b)
    {
        apple10.hintEnable = _b;
    }
    public void SetParticleToggle(bool _b)
    {
        apple10.particleOff = _b;
    }

    void OnGameEnd()
    {
        isPause = true;
        timer.enabled = false;
        //resultWindow.gameObject.SetActive(true);
        //resultWindow.transform.Find("slot/Text").TryGetComponent(out TMP_Text tmpTxt);
        //resultWindow.transform.Find("Button").TryGetComponent(out Button btn);
        //var clip = Resources.Load<AudioClip>("apple/end");
        //btn.onClick.AddListener(() => SceneManager.LoadScene(0));
        //tmpTxt.text = $"Score\n{score}";
        apple10.OnGameEnd();
    }
    public void OnClear()
    {
        if (isTutorial)
        {
            SaveManager.I.SetData(EnumData.IsTutorialEnd, 1);

            isTutorial = false;
            PlayGame();
        }
        SetPause(true);
    }


    public void AddScore(int _score)
    {
        addScore += _score;

        if (scoreCoroutine != null)
            StopCoroutine(scoreCoroutine);
        if (addScore + score > highScore)
            SaveManager.I.SetData(EnumData.HighScore, addScore + score);
        scoreCoroutine = StartCoroutine(AddScoreRutine());
        if (!isTutorial) return;
        int state = guide.GetInteger(STATE);
        guide.SetInteger(STATE, ++state);
        if (state > 2)
            guide.gameObject.SetActive(false);
        //hintTimer = 0;
    }

    public void UseHint()
    {
        scoreTxt.color = ThemeManager.Instance.currentTheme.accent;
        apple10.ShowHint();
        usedHint = true;
    }
    void ShowSetting()
    {
        if (isSuffle)
            return;
        SetPause(true);
        setting.gameObject.SetActive(true);
    }

    IEnumerator AddScoreRutine()
    {
        if(addScore == 0)
        {
            scoreTxt.SetText("{0}", score);
            yield break;
        }
        scoreTxt.SetText("{0}<sub>+{1}</sub>", score, addScore);
        yield return YieldCache.WaitForSeconds(.5f);

        while (addScore > 0)
        {
            yield return YieldCache.WaitForSeconds(.05f);
            scoreTxt.SetText("{0}<sub>+{1}</sub>", ++score, --addScore);
            if (score > highScore)
                highScoreTxt.SetText("{0}", highScore = score);
        }
        yield return YieldCache.WaitForSeconds(.25f);
        scoreTxt.SetText("{0}", score);
    }
}
