using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imageproject.Models
{
    public class FixCameraQueued : PubSubEvent<CameraId> { }
    public class MoveCameraQueued : PubSubEvent<CameraId> { }
}
