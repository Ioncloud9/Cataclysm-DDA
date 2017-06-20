using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using UnityEditor.VersionControl;
using ThreadState = System.Threading.ThreadState;

namespace Assets
{
    public delegate void ReceivedMessageHandler(string messageType, string message);

    public class zMQThread : IDisposable
    {
        private static readonly object _locker = new object();
        private SubscriberSocket _subscriber;
        private bool _stopping = false;
        private Thread _thread;

        public event ReceivedMessageHandler OnMessageReceived;

        public zMQThread()
        {
            Running = false;
        }

        public void Stop()
        {
            if (!Running || _stopping) return;
            lock (_locker)
            {
                if (!Running || _stopping) return;
                _stopping = true;
                //give things a chance to stop...
                _thread.Join(1000);
                if (_thread.ThreadState != ThreadState.Stopped)
                {
                    try
                    {
                        _thread.Abort();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("zMQThread did not exit gracefully and was terminated");
                    }
                }

            }
        }
        
        public void Start()
        {
            if (Running) return;
            lock (_locker)
            {
                if (Running) return;
                Running = true;
                _stopping = false;
                _subscriber = new SubscriberSocket("tcp://localhost:3332");
                _thread = new Thread(Main);
                _thread.Start();
            }
        }

        public bool Running { get; private set; }

        private void Main()
        {
            while (!_stopping)
            {
                _subscriber.SubscribeToAnyTopic();
                string msg;
                while (!_subscriber.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out msg) && !_stopping)
                {
                }
                if (_stopping) break;
                if (OnMessageReceived != null)
                {
                    OnMessageReceived("MapData", msg);
                }
            }
            _subscriber.Dispose();
            _subscriber = null;
            Running = false;
            _stopping = false;
        }

        public void Dispose()
        {
            Stop();
            if(_subscriber != null) _subscriber.Dispose();
        }
    }
}
