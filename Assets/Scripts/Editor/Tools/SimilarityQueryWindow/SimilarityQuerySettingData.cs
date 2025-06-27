using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworkEditor.Tools.SimilarityQuery
{
    [Serializable]
    public class SimilarityQueryInfo
    {
        public string Name;
        public SimilarityQueryType QueryType = SimilarityQueryType.AssetSimilarityQuery;
        public AssetTypeFlag AssetTypeFlag = AssetTypeFlag.Texture;
        public string Path;
        public bool isSkipSameFolder;
    }
    public class SimilarityQuerySettingData:ScriptableObject
    {
        [SerializeField]
        public List<SimilarityQueryInfo> SimilarityQueryDatas;
    }
}