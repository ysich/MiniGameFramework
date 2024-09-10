/*---------------------------------------------------------------------------------------
-- 负责人: onemt
-- 创建时间: 2024-09-10 15:17:03
-- 概述:
---------------------------------------------------------------------------------------*/

using Cysharp.Threading.Tasks;
using YooAsset;

namespace Core.Module.Resources
{
    public class ResourceMgr:Singleton<ResourceMgr>,ISingletonAwake
    {
        public void Awake()
        {
            YooAssets.Initialize();
        }

        public static UniTaskVoid CreatePackageAsync(string packageName,bool isDefault)
        {
            var package = YooAssets.CreatePackage(packageName);

            if (isDefault)
            {
                YooAssets.SetDefaultPackage(package);
            }

            return new UniTaskVoid();
        }
    }
}