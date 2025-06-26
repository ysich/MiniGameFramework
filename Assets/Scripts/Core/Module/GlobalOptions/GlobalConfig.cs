using Core;
using UnityEngine;
using YooAsset;

[CreateAssetMenu(fileName = "GlobalConfig",menuName = "Settings/Create GlobalConfig",order = 0)]
public class GlobalConfig:ScriptableObject
{
    public string productName;
    public string version;
    public LogLevelType logLevelType;
    public BuildType buildType;
    public EPlayMode ePlayMode;
}