using System;

namespace TcpipServer.Models
{
    [Serializable]
    public class ImageInfo
    {
        public int ObjectId { get; set; }
        public string Title { get; set; }
        public byte[] imgByte { get; set; }
    }
}
