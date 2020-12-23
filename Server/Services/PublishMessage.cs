using Prism.Events;

namespace TcpipServer.Services
{
    // 負責送出演算結果(JSON字串)
    public class PublishSolve : PubSubEvent<string> { }

    //// 負責送出 Message 訊息
    //public class PublishMessage : PubSubEvent<string> { }

    //// 負責送出 Status 訊息
    //public class PublishStatus : PubSubEvent<string> { }
}
