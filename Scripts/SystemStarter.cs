using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemStarter : MonoBehaviour
{
    List<ISingleton> managers = new(16);
    private void Awake()
    {
        managers = new()
        {
            SaveManager.Instance,
            GameManager.Instance
        };
    }
    void Start()
    {
        foreach (var manager in managers)
            manager.Initialize();
    }
}
