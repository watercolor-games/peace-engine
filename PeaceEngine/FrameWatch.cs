using System;
using Plex.Engine.Interfaces;
using System.Threading;
using System.Threading.Tasks;
namespace Plex.Engine
{
    /// <summary>
    /// I got my eyes on you.
    /// </summary>
    public class FrameWatch : IEngineComponent, IDisposable
    {
        [Dependency]
        Plexgate plexgate;

        EventWaitHandle updated;
        EventWaitHandle waite;

        volatile int waiting = 0;
        bool subscribed = false;
        
        void gameUpdated(object sender, EventArgs e)
        {
            updated?.Set();
        }

        /// <inheritdoc/>
        public void Initiate()
        {
            waiting = 0;
            updated = new ManualResetEvent(false);
            waite = new AutoResetEvent(true);
            if (!subscribed)
                plexgate.FrameDrawn += gameUpdated;
            subscribed = true;
            Task.Run(() =>
            {
                while (subscribed)
                {
                    while (waiting > 0)
                        waite?.WaitOne();
                    updated?.Reset();
                }
            });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            updated?.Set();
            waite?.Set();
            updated?.Dispose();
            updated = null;
            waite?.Dispose();
            waite = null;
            waiting = 0;
            if (subscribed)
                plexgate.FrameDrawn -= gameUpdated;
            subscribed = false;
        }

        /// <summary>
        /// Waits for the time given for the game to draw a frame.
        /// </summary>
        /// <returns><c>true</c> if a frame was drawn in time, <c>false</c> otherwise.</returns>
        /// <param name="max">The maximum time to wait for.</param>
        public bool WaitFor(TimeSpan max)
        {
            Interlocked.Increment(ref waiting);
            var ret = WaitHandle.WaitAll(new[] { updated }, max);
            Interlocked.Decrement(ref waiting);
            waite?.Set();
            return ret;
        }

        /// <summary>
        /// Calls callback on a different thread if a frame is not drawn in the time given.
        /// </summary>
        /// <param name="max">The maximum time to wait for.</param>
        /// <param name="callback">The action to be performed if no frame is drawn.</param>
        public void Alert(TimeSpan max, Action callback)
        {
            new Thread(() =>
            {
                if (!WaitFor(max))
                    callback();
            }).Start();
        }
    }
}