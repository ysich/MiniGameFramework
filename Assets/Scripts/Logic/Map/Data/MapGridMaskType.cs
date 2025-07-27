namespace Logic.Map
{
    /// <summary>
    /// 地图格子掩码类型
    /// </summary>
    public enum MapGridMaskType
    {
        None = 0,
        GroundBuilding = 1 << 0,  // 地面建筑可放
        WaterBuilding = 1 << 1,   // 水面建筑可放
        All = GroundBuilding | WaterBuilding
    }
}
