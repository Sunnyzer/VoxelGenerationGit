using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance = null;
    public static T Instance => instance;

    protected void Awake()
    {
        if(instance)
        {
            Destroy(this);
            return;
        }
        instance = this as T;
    }
}
