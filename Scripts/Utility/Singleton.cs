using System;
using UnityEngine;
public interface ISingleton
{
    void Initialize();
}
public class Singleton<T> : MonoBehaviour, ISingleton where T : MonoBehaviour
{
    static T _instance;
    static object _lock = new();

    public static T I => Instance;
    public static T Instance
    {
        get
        {
            /*if (applicationIsQuitting)
            {
                UnityEngine.Debug.LogError($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }*/


            lock (_lock)
            {
                if (_instance)
                    return _instance;

                T[] objs = FindObjectsByType<T>(FindObjectsSortMode.InstanceID);

                if (objs.Length == 1)
                    _instance = objs[0];
                else if (objs.Length > 1)
                    UnityEngine.Debug.LogError($"There is more than one '{typeof(T)}' in the scene.");

                if (_instance)
                    return _instance;

                string className = typeof(T).ToString();
                var span = className.AsSpan();

                int lastDot = className.LastIndexOf('.');
                var objNameSpan = (lastDot >= 0) ? span.Slice(lastDot + 1) : span;
                string objName = objNameSpan.ToString();

                var obj = GameObject.Find(objName) ?? new GameObject(objName);
                _instance = obj.AddComponent<T>();
                obj.name = $"(S){objName}";

                if (!Application.isPlaying)
                    return _instance;
                DontDestroyOnLoad(obj);
            }

            return _instance;
        }
    }
    private void Awake()
    {
        //if (GameManager.Instance.isTest) Initialize();
    }
    public virtual void Initialize() { }


    /*static bool applicationIsQuitting;
    protected void OnDestroy()
    {
        applicationIsQuitting = true;
    }
    protected void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }*/
}
