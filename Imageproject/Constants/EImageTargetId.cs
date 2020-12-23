namespace Imageproject.Constants
{
    /// <summary>
    /// 拍照物件列表
    /// </summary>
    public enum EImageTargetId
    {
        /********************
         * 移動相機(上相機)
         ********************/
        // 機台基準點
        DatumPoint1,
        DatumPoint2,

        // Tray
        TrayComponent,      // Tray盤部品 - 宮格

        // 台車
        Barrel,             // 鏡筒 - 宮格
        Stage,              // 台車

        ProductBeforeGlue,  // 點膠前半成品

        /********************
         * 固定相機(下相機)
         ********************/
        // 吸嘴 - 宮格
        Nozzle01,
        Nozzle02,
        Nozzle03,
        Nozzle04,
        Nozzle05,
        Nozzle06,
        Nozzle07,
        Nozzle08,
        Nozzle09,
        Nozzle10,
        Nozzle11,

        // 吸嘴上有部品 - 宮格
        NozzleComponent01,
        NozzleComponent02,
        NozzleComponent03,
        NozzleComponent04,
        NozzleComponent05,
        NozzleComponent06,
        NozzleComponent07,
        NozzleComponent08,
        NozzleComponent09,
        NozzleComponent10,
        NozzleComponent11,

        Needle,         // 膠針

        /********************
         * 不需存照片 & 不需演算
         ********************/
        MovingCamera,   // 移動相機
        FixCamera,      // 固定相機
    }
}
