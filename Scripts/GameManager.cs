using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    Transform resultWindow;
    TMP_Text scoreTxt;
    Timer timer;
    UIDragSelection dragSelection;
    int score;
    bool isPlaying;

    private void Start()
    {
        Initialized();
    }

    void Initialized()
    {
        transform.Find("ScoreTxt").TryGetComponent(out scoreTxt);
        transform.Find("Timer").TryGetComponent(out timer);
        transform.Find("DragSelection").TryGetComponent(out dragSelection);
        transform.Find("ResultWindow").TryGetComponent(out resultWindow);
        dragSelection.Initialized(AddScore, OnGameEnd);
        timer.Initialized(60, OnGameEnd);
        resultWindow.gameObject.SetActive(false);

        AddScore(score = 0);
        isPlaying = true;
    }


    void OnGameEnd()
    {
        if (!isPlaying) return;
        resultWindow.gameObject.SetActive(true);
        isPlaying = false;
    }


    void AddScore(int _score)
    {
        score += _score;
        scoreTxt.text = $"Score : {score}";
    }
    float Abs(float x) => x > 0 ? x : -x;

}
