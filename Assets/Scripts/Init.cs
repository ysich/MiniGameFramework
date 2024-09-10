using Core;
using Core.Module.CodeLoader;
using Core.Module.Resources;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Core.Logger;

public class Init : MonoBehaviour
{
    public GlobalConfig globalConfig;
    private void Start()
    {
        StartAsync().Forget();
    }

    private async UniTaskVoid StartAsync()
    {
        DontDestroyOnLoad(gameObject);
        //放到这里面让逻辑看起来完整点
        Game.AddSingleton<GlobalOptions>().globalConfig = globalConfig;
        Game.AddSingleton<Logger>().ILog = new UnityLogger();
        // Game.AddSingleton<TimeInfo>();
        Game.AddSingleton<ObjectPool>();
        
        await Game.AddSingleton<ResourceMgr>().CreatePackageAsync("MainPackage",true);

        Game.AddSingleton<CodeLoader>().Start();
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
