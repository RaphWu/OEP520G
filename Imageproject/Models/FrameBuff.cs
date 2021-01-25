using Imageproject.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TIS.Imaging;

namespace Imageproject.Models
{
    [Serializable]
    public class FrameBuff
    {
        /// <summary>
        /// 此影像所屬的物件ID
        /// </summary>
        public EImageTargetId ObjectId { get; set; }

        /// <summary>
        /// 相機截取的影像
        /// </summary>
        public Bitmap Frame { get; set; }

        /// <summary>
        /// 此影像是否需求解?
        /// </summary>
        public bool NeedSolve { get; set; }
    }
}
