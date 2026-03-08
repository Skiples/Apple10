using System;
using System.Collections.Generic;
using UnityEngine;
public enum EnumData
{
    IsTutorialEnd,
    HighScore,
    Hint,
    Particle,
    MasterVolume,
    BGM,
    SFX,
}

public class SaveManager : Singleton<SaveManager>
{
    private Dictionary<EnumData, string> keys = new(Enum.GetNames(typeof(EnumData)).Length);
    private string GetKey(EnumData enumValue)
    {
        if (!keys.TryGetValue(enumValue, out string key))
        {
            key = enumValue.ToString();
            keys[enumValue] = key;
        }
        return key;
    }
    #region Setting
    public bool HasSettingData() => PlayerPrefs.HasKey(GetKey(EnumData.HighScore));
    public void DeleteHighScore()
    {
        PlayerPrefs.DeleteKey(GetKey(EnumData.HighScore));
    }
    public void SetData(EnumData _key, int _data)
    {
        PlayerPrefs.SetInt(GetKey(_key), _data);
        PlayerPrefs.Save();
    }
    public int GetData(EnumData _key, int _default) => PlayerPrefs.GetInt(GetKey(_key), _default);
#endregion
}
