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

namespace Assets.Scripts
{
    public delegate void ReceivedMessageHandler(zMQResponse message);

    public class zMQThread : IDisposable
    {
        private static readonly object _locker = new object();
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
                if (_thread.ThreadState == ThreadState.Stopped) return;
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

        
        public zMQResponse SendCommand(zMQCommand command)
        {
            zMQResponse response;
            using (var requestor = new RequestSocket(voxConst.SENDER_URI))
            {
                requestor.SendFrame(command.ToString());
                string res = requestor.ReceiveFrameString();
                response = new zMQResponse(res);
            }

            return response;
        }
        
        public void Start()
        {
            if (Running) return;
            lock (_locker)
            {
                if (Running) return;
                Running = true;
                _stopping = false;

                _thread = new Thread(Main);
                _thread.Start();
            }
        }

        public bool Running { get; private set; }

        private void Main()
        {
            try
            {
                using (var listener = new ResponseSocket(voxConst.LISTENER_URI))
                {
                    while (!_stopping)
                    {
                        try
                        {
                            var msg = "";
                            while (!listener.TryReceiveFrameString(TimeSpan.FromMilliseconds(1000), out msg) && !_stopping) { }
                            if (_stopping) break;
                            zMQResponse response;
                            if (!zMQResponse.TryParse(msg, out response))
                            {
                                listener.SendFrame("OK");
                            }
                            else
                            {
                                listener.SendFrame(string.Format("{0}:OK", response.ID));
                                if (OnMessageReceived != null) OnMessageReceived(response);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Running = false;
                _stopping = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
