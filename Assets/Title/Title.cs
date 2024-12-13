using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    public static void ResetDefaultWorld()
    {
        var defaultWorld = World.DefaultGameObjectInjectionWorld;
        defaultWorld.EntityManager.CompleteAllTrackedJobs();
        foreach (var system in defaultWorld.Systems)
        {
            system.Enabled = false;
        }

        defaultWorld.Dispose();
        DefaultWorldInitialization.Initialize("Default World", false);
    }

    public void OnClick1()
    {
        ResetDefaultWorld();
        SceneManager.LoadScene("SimpleCollision", LoadSceneMode.Single);
    }
}
