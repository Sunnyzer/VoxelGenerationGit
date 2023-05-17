using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance = null;
    public static T Instance => instance;

    protected virtual void Awake()
    {
        if(instance)
        {
            Destroy(this);
            return;
        }
        instance = this as T;
    }
}
