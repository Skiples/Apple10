using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UISetting : MonoBehaviour
{
    enum EnumSettingState
    {
        None,
        Sound,
        Graphic,
        Particle,
        Debug,
        Credit,
        Report
    }
    const float PER = 1 / 20f;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] UITextSlider masterSlider, bgmSlider, sfxSlider;
    [SerializeField] Button closeBtn, restartBtn, exitBtn;
    [SerializeField] Toggle hintToggle, particleToggle;

    private void Awake()
    {
        restartBtn.onClick.AddListener(OnClickRestart);
        masterSlider.onValueChanged = (x) => SetVolume("Master", x);
        bgmSlider.onValueChanged = (x) => SetVolume("BGM", x);
        sfxSlider.onValueChanged = (x) => SetVolume("SFX", x);
        closeBtn.onClick.AddListener(CloseWindow);
        exitBtn.onClick.AddListener(Application.Quit);
        hintToggle.onValueChanged.AddListener((x) => {
            int b = x ? 1 : 0;
            SaveManager.I.SetData(EnumData.Hint, b);
            GameManager.I.SetHintToggle(x);
        });
        particleToggle.onValueChanged.AddListener((x) => {
            int b = x ? 1 : 0;
            SaveManager.I.SetData(EnumData.Particle, b);
            GameManager.I.SetParticleToggle(x);
        });
    }
    private void Start()
    {
        masterSlider.SetValue(SaveManager.I.GetData(EnumData.MasterVolume, 10));
        bgmSlider.SetValue(SaveManager.I.GetData(EnumData.BGM, 10));
        sfxSlider.SetValue(SaveManager.I.GetData(EnumData.SFX, 10));
        hintToggle.isOn = SaveManager.I.GetData(EnumData.Hint, 0) == 1;
        particleToggle.isOn = SaveManager.I.GetData(EnumData.Particle, 0) == 1;
        CloseWindow();
    }
    void SetVolume(string _type, float _x)
    {
        float volume = Mathf.Clamp(_x * PER, 0.0001f, 1f);
        audioMixer.SetFloat(_type, Mathf.Log10(volume) * 20);
    }

    void OnClickRestart()
    {
        CloseWindow();
        GameManager.I.PlayGame();
    }

    public void CloseWindow()
    {
        SaveData();
        GameManager.I.SetPause(false);
        GameManager.I.CheckAllowHint();
        gameObject.SetActive(false);
    }
    void SaveData()
    {
        SaveManager.I.SetData(EnumData.MasterVolume, (int)masterSlider.value);
        SaveManager.I.SetData(EnumData.BGM, (int)bgmSlider.value);
        SaveManager.I.SetData(EnumData.SFX, (int)sfxSlider.value);
        SaveManager.I.SetData(EnumData.Hint, hintToggle.isOn ? 1 : 0);
        SaveManager.I.SetData(EnumData.Particle, particleToggle.isOn ? 1 : 0);
    }
}

