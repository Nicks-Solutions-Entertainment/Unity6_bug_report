using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHolder : MonoBehaviour
{
    static CoroutineHolder _instance;
    internal static CoroutineHolder Instance
    {
        get
        {
            if (_instance == null)
            {
                CoroutineHolder _i = new GameObject("CoroutineHolder").AddComponent<CoroutineHolder>();
                if (Application.isPlaying)
                    DontDestroyOnLoad(_i.gameObject);

                _i.gameObject.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
                _instance = _i;
            }
            return _instance;
        }
    }

    
}
