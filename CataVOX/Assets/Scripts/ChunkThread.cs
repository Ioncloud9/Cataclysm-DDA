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
        public Chunk Result { get; private set; }
        public Stopwatch Timing { get; private set; }

        public static ChunkThread StartNew(IVector2 chunkLocation, VOX.Map map, IVector3 chunkSize)
        {
            var thread = new ChunkThread();
            thread.Start(chunkLocation, map, chunkSize);
            return thread;
        }

        public void Start(IVector2 chunkLocation, VOX.Map map, IVector3 chunkSize)
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
            public IVector3 chunkSize;
            public IVector2 location;
            public VOX.Map map;
        }
        private void Main(object args)
        {
            var sw = Stopwatch.StartNew();
            var tsArgs = (ThreadStartArgs) args;
            var chunk = new Chunk(tsArgs.map, tsArgs.location);
            chunk.Create(tsArgs.chunkSize);
            Result = chunk;
            IsRunning = false;
            sw.Stop();
            Timing = sw;
        }
    }
}
