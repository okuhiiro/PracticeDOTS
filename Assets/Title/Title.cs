using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    void Awake()
    {
        SceneParameter.Param1 = -1;
    }
    
    private static void ResetDefaultWorld()
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
    
    public void OnClickMonoPhysics()
    {
        SceneParameter.Param1 = (int)MonoMode.Physics;
        SceneManager.LoadScene("ManyCollisionMono", LoadSceneMode.Single);
    }
    
    public void OnClickMonoAll()
    {
        SceneParameter.Param1 = (int)MonoMode.All;
        SceneManager.LoadScene("ManyCollisionMono", LoadSceneMode.Single);
    }
    
    public void OnClickDOTSPhysics()
    {
        ResetDefaultWorld();
        SceneParameter.Param1 = (int)DOTSMode.Physics;
        SceneManager.LoadScene("ManyCollisionDOTS", LoadSceneMode.Single);
    }
    
    public void OnClickDOTSAllBurstCompiler()
    {
        ResetDefaultWorld();
        SceneParameter.Param1 = (int)DOTSMode.AllBustCompiler;
        SceneManager.LoadScene("ManyCollisionDOTS", LoadSceneMode.Single);
    }
    
    public void OnClickDOTSAllBurstCompilerJob()
    {
        ResetDefaultWorld();
        SceneParameter.Param1 = (int)DOTSMode.AllBustCompilerJob;
        SceneManager.LoadScene("ManyCollisionDOTS", LoadSceneMode.Single);
    }
    
    public void OnClickDOTSSpatial()
    {
        ResetDefaultWorld();
        SceneParameter.Param1 = (int)DOTSMode.Spatial;
        SceneManager.LoadScene("ManyCollisionDOTS", LoadSceneMode.Single);
    }
}
