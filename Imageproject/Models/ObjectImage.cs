using Imageproject.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace Imageproject.Models
{
    public class ObjectImage
    {
        internal EImageTargetId ObjectId { get; set; }
        internal string Title { get; set; }
        internal BitmapImage Image { get; set; }
        internal double X { get; set; }
        internal double Y { get; set; }
        internal double A { get; set; }
    }
}
