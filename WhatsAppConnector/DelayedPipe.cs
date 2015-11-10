using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WhatsAppApi.Facades
{

    public class DelayedPipe
    {
        private int minDelay;
        private int maxDelay;
        private Queue<Message> queue = new Queue<Message>();
        private Thread pollingThread;

        public EventHandler<DelayedPipeMessageEventArgs> Dequeued;

        public DelayedPipe(int minDelay, int maxDelay)
        {
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
            pollingThread = new Thread(delegate()
            {
                while (true)
                {
                    Message msg = null;
                    lock (queue)
                    {
                        if (queue.Count > 0)
                        {
                            msg = queue.Dequeue();
                        }
                    }
                    if (msg != null)
                    {
                        OnDequeued(new DelayedPipeMessageEventArgs() { Message = msg });
                        msg = null;
                    }
                    int delay = (new Random()).Next(this.minDelay, this.maxDelay);
                    Thread.Sleep(delay);
                }
            });
            pollingThread.IsBackground = true;
            pollingThread.Start();
        }

        public void Enqueue(Message message)
        {
            lock (queue)
            {
                queue.Enqueue(message);
            }
        }


        protected virtual void OnDequeued(DelayedPipeMessageEventArgs args)
        {
            if (this.Dequeued != null)
            {
                var del = this.Dequeued;
                del(this, args);
            }
        }
    }

}
