using MPCore;

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MultiLoader : MonoBehaviour
{
    public SceneField[] scenesToLoad;
    void Start()
    {
        foreach (SceneField scene in scenesToLoad)
            try
            {
                if(scene != null)
                    SceneManager.LoadScene(scene, LoadSceneMode.Additive);
            }
            catch (Exception) { }
    }
}
