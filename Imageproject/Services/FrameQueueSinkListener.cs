using System;
using System.Collections.Generic;
using System.Text;
using TIS.Imaging;

namespace Imageproject.Services
{
    internal class FrameQueueSinkListener : IFrameQueueSinkListener
    {
        public void FramesQueued(FrameQueueSink sink)
        {
            IFrameQueueBuffer[] buffers = sink.PopAllOutputQueueBuffers();
            foreach (IFrameQueueBuffer buf in buffers)
            {
                // call your function on buf
                // do_something( buf );
                // this sample expects do_something not to hold onto the buffer
                // because of this, we directly queue it back into the sink input queue
                sink.QueueBuffer(buf);
            }
        }

        public void SinkConnected(FrameQueueSink sink, FrameType frameType)
        {
            sink.AllocAndQueueBuffers(11);
        }

        public void SinkDisconnected(FrameQueueSink sink, IFrameQueueBuffer[] dequeuedBuffers)
        {
            foreach (IFrameQueueBuffer buf in dequeuedBuffers)
            {
                // this is practically sink.PopInputQueueBuffers
            }
            // these are the already copied buffers for which we were not called for/didn't already pop
            IFrameQueueBuffer[] outputBuffers = sink.PopAllOutputQueueBuffers();
        }
    }
}
