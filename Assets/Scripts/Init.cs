using Core;
using UnityEngine;
using Logger = Core.Logger;

public class Init : MonoBehaviour
{
    public GlobalConfig globalConfig;
    private void Start()
    {
        Game.AddSingleton<GlobalOptions>().globalConfig = globalConfig;
        Game.AddSingleton<Logger>().ILog = new UnityLogger();
        // Game.AddSingleton<TimeInfo>();
        Game.AddSingleton<ObjectPool>();;
    }

    private void Update()
    {
        Game.Update();
    }

    private void LateUpdate()
    {
        Game.LateUpdate();
    }

    private void OnApplicationQuit()
    {
        Game.Close();
    }
}
