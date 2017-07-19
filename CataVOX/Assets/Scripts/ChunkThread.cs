using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Assets.VOX;
using UnityEngine;

namespace Assets.Scripts
{
    public class ChunkThread
    {
        private object locker = new object();
        private Thread _th;

        public ChunkThread()
        {
            IsRunning = false;
            Result = null;
        }

        public bool IsRunning { get; private set; }
        public VOXChunk Result { get; private set; }
        public Stopwatch Timing { get; private set; }

        public static ChunkThread StartNew(IVector2 chunkLocation, VOXMap map, Vector3 chunkSize)
        {
            var thread = new ChunkThread();
            thread.Start(chunkLocation, map, chunkSize);
            return thread;
        }

        public void Start(IVector2 chunkLocation, VOXMap map, Vector3 chunkSize)
        {
            if (IsRunning) return;
            lock (locker)
            {
                if (IsRunning) return;
                _th = new Thread(Main);
                IsRunning = true;
                _th.Start(new ThreadStartArgs()
                {
                    location = chunkLocation,
                    map = map,
                    chunkSize = chunkSize
                });
            }
        }

        private class ThreadStartArgs
        {
            public Vector3 chunkSize;
            public IVector2 location;
            public VOXMap map;
        }
        private void Main(object args)
        {
            var sw = Stopwatch.StartNew();
            var tsArgs = (ThreadStartArgs) args;
            var chunk = new VOXChunk(tsArgs.map, tsArgs.location);
            chunk.Create(tsArgs.chunkSize);
            Result = chunk;
            IsRunning = false;
            sw.Stop();
            Timing = sw;
        }
    }
}
