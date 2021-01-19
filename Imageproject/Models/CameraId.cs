using System;
using System.Collections.Generic;
using System.Text;

namespace Imageproject.Models
{
    public enum CameraId
    {
        FixCamera,
        MoveCamera,

        // 用於指定動作，表示同時處理全部相機，例如初始化
        None,
        All
    }
}
