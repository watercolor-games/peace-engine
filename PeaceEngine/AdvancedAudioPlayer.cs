using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Plex.Engine.GraphicsSubsystem;
using Plex.Engine.Interfaces;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NVorbis;
using System.Collections.Concurrent;
using System.Threading;

namespace Plex.Engine
{
    public class AdvancedAudioPlayer : IDisposable
    {
        const double sc16 = 0x7FFF + 0.4999999999999999;

        internal static bool _reversed = false;

        public bool Reversed
        {
            get
            {
                return _reversed;
            }
            set
            {
                _reversed = true;
            }
        }

        DynamicSoundEffectInstance sfx;
        VorbisReader aread;

        ConcurrentQueue<byte[]> buf = new ConcurrentQueue<byte[]>();

        volatile bool disposed = false;

        EventWaitHandle bufUsed = new AutoResetEvent(true), bufSent = new AutoResetEvent(true);

        int cur = 0;

        /// <summary>
        /// The audio section that will be played after the current one has finished.
        /// </summary>
        public int Next { get; set; } = 1;

        struct Label
        {
            public readonly double Start, End;
            public readonly bool OneShot;
            public Label(double start, double end, bool oneshot)
            {
                Start = start;
                End = end;
                OneShot = oneshot;
            }
        }

        Label[] labels;
        float[] samps = null;
        byte[] data = null;

        long? fade = null;

        public TimeSpan Duration => aread.TotalTime;

        Thread readthread;

        public void Dispose()
        {
            aread?.Dispose();
            sfx?.Dispose();
            aread = null;
            sfx = null;
            labels = null;
            samps = null;
            buf = null;
            disposed = true;
            data = null;
            bufUsed.Set();
            readthread.Join();
            bufSent?.Dispose();
            bufUsed?.Dispose();
        }

        void construct(Stream audio, Stream labels, bool close)
        {
            if (labels != null)
                using (var read = new StreamReader(labels, Encoding.UTF8, true, 1024, !close))
                    this.labels = read.IterLines().Select(l => l.Split()).Where(s => s.Length == 3).Select(s => new Label(double.Parse(s[0]), double.Parse(s[1]), s[2] == "oneshot")).ToArray();
            aread = new VorbisReader(audio, close);
            AudioChannels channels;
            switch (aread.Channels)
            {
                case 1:
                    channels = AudioChannels.Mono;
                    break;
                case 2:
                    channels = AudioChannels.Stereo;
                    break;
                default:
                    throw new InvalidDataException($"Unsupported channel count {aread.Channels} (must be mono or stereo).");
            }
            if (this.labels?.Length > 0)
                aread.DecodedTime = TimeSpan.FromSeconds(this.labels[0].Start);
            samps = new float[aread.Channels * aread.SampleRate / 5];
            sfx = new DynamicSoundEffectInstance(aread.SampleRate, channels);
            sfx.BufferNeeded += update;
            readthread = new Thread(readthreadfun);
            readthread.Start();
        }

        void update(object sender, EventArgs e)
        {
            while (buf.Count > 0)
            {
                byte[] b = null;
                buf.TryDequeue(out b);
                sfx.SubmitBuffer(b);
            }
            bufUsed.Set();
        }

        void readbuffer()
        {
            if (samps == null)
                samps = new float[aread.Channels * aread.SampleRate / 5];
            data = new byte[samps.Length * sizeof(short)];
            aread.ReadSamples(samps, 0, samps.Length);
            bool skipearly = false;
            if (fade != null)
            {
                for (int i = 0; i < samps.Length; i++)
                {
                    var mul = MathHelper.Clamp(1 - ((long)fade - (aread.DecodedPosition - (samps.Length - i))) / ((float)aread.SampleRate * aread.Channels), 0, 1);
                    samps[i] *= mul;
                    if (mul <= 0)
                        skipearly = true;
                }
            }
            using (var ms = new MemoryStream(data))
            using (var write = new BinaryWriter(ms))
            {
                if (_reversed == true)
                {
                    foreach (var samp in samps)
                        write.Write((short)((samp * sc16) - 32767)); // convert to S16 int PCM
                }
                else
                {
                    foreach (var samp in samps)
                        write.Write((short)(samp * sc16)); // convert to S16 int PCM
                }
            }
            buf.Enqueue(data);
            if (skipearly || aread.DecodedTime.TotalSeconds >= labels[cur].End)
            {
                fade = null;
                if (Next >= labels.Length)
                {
                    Stop();
                    return;
                }
                double lastend = labels[cur].End;
                cur = Next;
                aread.DecodedTime = TimeSpan.FromSeconds(labels[cur].Start + aread.DecodedTime.TotalSeconds - lastend);
                Next = cur + (labels[cur].OneShot ? 0 : 1);
            }
            data = null;
        }

        void readthreadfun()
        {
            while (!disposed)
            {
                while (!disposed && buf.Count < 2 * aread.Channels * aread.SampleRate / samps.Length) // This theoretically gives about 2 seconds of skip prevention
                {
                    readbuffer();
                    bufSent.Set();
                }
                bufUsed.WaitOne();
            }
        }

        /// <summary>
        /// Fades out the current section and then moves to the next one.
        /// </summary>
        /// <param name="duration">The time taken to fade out, in seconds.</param>
        public void FadeToNextSection(double duration = 1)
        {
            fade = aread.DecodedPosition + (long)(aread.SampleRate * duration * aread.Channels);
        }

        /// <summary>
        /// Read audio data and labels from streams you opened yourself.
        /// In most cases you'll be better off with the other constructors :^)
        /// </summary>
        /// <param name="audio">An open Ogg Vorbis file.</param>
        /// <param name="labels">An open Audacity labels file.</param>
        /// <param name="close">If true, the labels stream will be closed during construction and the audio stream will be closed on disposal.</param>
        public AdvancedAudioPlayer(Stream audio, Stream labels, bool close = false)
        {
            if (audio == null)
                throw new ArgumentNullException(nameof(audio));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));
            construct(audio, labels, close);
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public void Pause()
        {
            sfx.Pause();
        }

        /// <summary>
        /// Starts or resumes playback.
        /// </summary>
        public void Play()
        {
            sfx.Play();
        }

        /// <summary>
        /// Resumes playback.
        /// </summary>
        public void Resume()
        {
            sfx.Resume();
        }

        SoundState State => sfx.State;

        /// <summary>
        /// Stops the player immediately.
        /// </summary>
        public void Stop()
        {
            sfx.Stop();
        }

        /// <summary>
        /// Stops the player once the current section is over.
        /// </summary>
        public void StopNext()
        {
            Next = labels.Length;
        }

        /// <summary>
        /// Set up an AdvancedAudioPlayer with the given song.
        /// </summary>
        /// <param name="basename">The base name of the song's audio and labels files.</param>
        public AdvancedAudioPlayer(string basename) : this(File.OpenRead($"{basename}.ogg"), File.OpenRead($"{basename}.txt"), true)
        {
        }

        /// <summary>
        /// Set up an AdvancedAudioPlayer from an Ogg Vorbis file alone.
        /// </summary>
        /// <param name="audio">An open Ogg Vorbis file.</param>
        /// <param name="loop">If set to <c>true</c>, the audio will loop.</param>
        /// <param name="close">If true, the audio stream will be closed on disposal.</param>
        public AdvancedAudioPlayer(Stream audio, bool loop, bool close = false)
        {
            construct(audio, null, close);
            labels = new[] { new Label(0, aread.TotalTime.TotalSeconds, !loop) };
        }

        /// <summary>
        /// Set up an AdvancedAudioPlayer from an Ogg Vorbis file alone.
        /// </summary>
        /// <param name="fname">The name of the song's Ogg Vorbis file.</param>
        /// <param name="loop">If set to <c>true</c>, the audio will loop.</param>
        public AdvancedAudioPlayer(string fname, bool loop) : this(File.OpenRead(fname), loop, true)
        {
        }
    }

    public static class SRExtensions
    {
        public static IEnumerable<string> IterLines(this StreamReader src)
        {
            string line;
            while ((line = src.ReadLine()) != null)
                yield return line;
        }
    }

    public class CheatManager : IEngineComponent
    {
        [Dependency]
        private Plexgate _backend = null;

        public void Initiate()
        {
            _backend.GetLayer(LayerType.NoDraw).AddEntity(_backend.New<CheatEntity>());
        }

        class CheatEntity : IEntity
        {
            private readonly Keys[] _cheatSequence = new Keys[] { Keys.A, Keys.N, Keys.D, Keys.E, Keys.R, Keys.S, Keys.J, Keys.E, Keys.N, Keys.S, Keys.E, Keys.N };
            private List<Keys> _keys = new List<Keys>();

            public void Draw(GameTime time, GraphicsContext gfx)
            {
            }

            public void OnGameExit()
            {
            }

            public void OnKeyEvent(KeyboardEventArgs e)
            {
                _keys.Add(e.Key);
                if(_keys.Count == _cheatSequence.Length)
                {
                    if(_keys.ToArray().SequenceEqual(_cheatSequence))
                    {
                        AdvancedAudioPlayer._reversed = !AdvancedAudioPlayer._reversed;
                    }
                    _keys.Clear();
                }
            }

            public void OnMouseUpdate(MouseState mouse)
            {
            }

            public void Update(GameTime time)
            {
            }
        }
    }
}
