using Core;
using Core.Module.Resources;
using Cysharp.Threading.Tasks;
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
        Game.AddSingleton<ObjectPool>();

        StartAsync();
    }

    private async UniTaskVoid StartAsync()
    {
        await Game.AddSingleton<ResourceMgr>().CreatePackageAsync("MainPackage",true);
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
