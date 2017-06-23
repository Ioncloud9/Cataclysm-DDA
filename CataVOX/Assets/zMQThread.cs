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
        private RequestSocket _requestor;
        private ResponseSocket _subscriber;
        private bool _stopping = false;
        private Thread _thread;

        public event ReceivedMessageHandler OnMessageReceived;

        public zMQThread()
        {
            Running = false;

        }

        private class AsyncCommand
        {
            public string Command;
            public Action<string> Callback;
        }

        public void Stop()
        {
            if (!Running || _stopping) return;
            lock (_locker)
            {
                if (!Running || _stopping) return;
                _stopping = true;
                _requestor.Dispose();
                _subscriber.Dispose();
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

        public string SendCommand(string command)
        {
            _requestor.SendFrame(command);
            var msg = _requestor.ReceiveFrameString();
            var spMsg = msg.Split(':').ToList();
            if (spMsg[0] == "FAIL")
            {
                throw new SendCommandException(spMsg[1]);
            }
            spMsg.RemoveAt(0);
            var newMsg = string.Join(":", spMsg.ToArray());
            return newMsg;
        }

        private void _SendCommandAsync(object args)
        {
            var command = (AsyncCommand) args;
            var msg = SendCommand(command.Command);
            command.Callback(msg);
        }

        public void SendCommandAsync(string command, Action<string> callback)
        {
            var th = new Thread(_SendCommandAsync);
            th.Start(new AsyncCommand()
            {
                Command = command,
                Callback = callback
            });
        }
        
        public void Start()
        {
            if (Running) return;
            lock (_locker)
            {
                if (Running) return;
                Running = true;
                _stopping = false;
                _requestor = new RequestSocket(">tcp://localhost:3333");
                _subscriber = new ResponseSocket(">tcp://localhost:3332");
                _thread = new Thread(Main);
                _thread.Start();
            }
        }

        public bool Running { get; private set; }

        private void Main()
        {
            try
            {
                while (!_stopping)
                {
                    try
                    {
                        var msg = "";
                        while (!_subscriber.TryReceiveFrameString(TimeSpan.FromMilliseconds(1000), out msg) && !_stopping)
                        {
                        }
                        if (_stopping) break;
                        var msgSplit = msg.Split(':');
                        var command = msgSplit[0];
                        var data = "";
                        if (msgSplit.Length > 1)
                            data = msgSplit[1];
                        else
                        {
                            data = command;
                            command = "";
                        }
                        if (OnMessageReceived != null) OnMessageReceived(command, data);
                        _subscriber.SendFrame("OK");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    Thread.Sleep(100);
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
