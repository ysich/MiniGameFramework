/*---------------------------------------------------------------------------------------
-- 负责人: onemt
-- 创建时间: 2024-09-10 15:17:03
-- 概述:
---------------------------------------------------------------------------------------*/

using Cysharp.Threading.Tasks;
using YooAsset;

namespace Core.Module.Resources
{
    public class ResourceMgr : Singleton<ResourceMgr>, ISingletonAwake
    {
        public void Awake()
        {
            YooAssets.Initialize();
        }

        public async UniTask CreatePackageAsync(string packageName, bool isDefault)
        {
            var package = YooAssets.CreatePackage(packageName);

            if (isDefault)
            {
                YooAssets.SetDefaultPackage(package);
            }

            EPlayMode ePlayMode = GlobalOptions.Instance.globalConfig.ePlayMode;
            switch (ePlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                    EditorSimulateModeParameters editorSimulateModeParameters =
                        new EditorSimulateModeParameters();
                    string simulateManifestFilePath =
                        EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.ScriptableBuildPipeline.ToString(),
                           packageName);
                    editorSimulateModeParameters.SimulateManifestFilePath = simulateManifestFilePath;
                    await package.InitializeAsync(editorSimulateModeParameters).Task;
                    break;
                case EPlayMode.OfflinePlayMode:
                    OfflinePlayModeParameters offlinePlayModeParameters = new OfflinePlayModeParameters();
                    // initParameters.DecryptionServices = new FileOffsetDecryption(); //需要补充这个
                    await package.InitializeAsync(offlinePlayModeParameters).Task;
                    break;
                case EPlayMode.HostPlayMode:
                    // string defaultHostServer = "https://static0.xesimg.com/project-yooasset/0130Offset";
                    // string fallbackHostServer = "https://static0.xesimg.com/project-yooasset/0130Offset";
                    HostPlayModeParameters hostPlayModeParameters = new HostPlayModeParameters();
                    // initParameters.BuildinQueryServices = new GameQueryServices(); //内置资源查询服务接口
                    // initParameters.DecryptionServices =
                    //     new FileOffsetDecryption(); //如果资源包在构建的时候有加密，需要提供实现IDecryptionServices接口的实例类。
                    // initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer); //远端服务器查询服务接口
                    await package.InitializeAsync(hostPlayModeParameters).Task;
                    break;
                case EPlayMode.WebPlayMode:
                    // string defaultHostServer = "http://127.0.0.1/CDN/WebGL/V1.0";
                    // string fallbackHostServer = "http://127.0.0.1/CDN/WebGL/V1.0";
                    WebPlayModeParameters webPlayModeParameters = new WebPlayModeParameters();
                    // initParameters.BuildinQueryServices = new GameQueryServices();
                    // initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    await package.InitializeAsync(webPlayModeParameters).Task;
                    break;
            }
        }
    }
}