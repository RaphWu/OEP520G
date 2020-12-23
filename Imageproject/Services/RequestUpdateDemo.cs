using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imageproject.Services
{
    // 負責送出結果
    public class RequestUpdateDemo : PubSubEvent<string> { }
}
