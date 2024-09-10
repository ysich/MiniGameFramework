/*---------------------------------------------------------------------------------------
-- 负责人: onemt
-- 创建时间: 2024-05-24 17:43:07
-- 概述:
---------------------------------------------------------------------------------------*/

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