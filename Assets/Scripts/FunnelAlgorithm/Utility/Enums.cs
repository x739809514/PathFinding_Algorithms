public enum FunnelShirkEnum
{
    None,
    /// <summary>
    /// 左向向左
    /// </summary>
    LeftToLeft,
    /// <summary>
    /// 左向向右，但未超过右极限
    /// </summary>
    LeftToCenter,
    /// <summary>
    /// 左向向右，且超过右极限
    /// </summary>
    LeftToRight,
    /// <summary>
    /// 右向向右
    /// </summary>
    RightToRight,
    /// <summary>
    /// 右向向左，但未超过左极限
    /// </summary>
    RightToCenter,
    /// <summary>
    /// 右向向左，且超过左极限
    /// </summary>
    RightToLeft
}
