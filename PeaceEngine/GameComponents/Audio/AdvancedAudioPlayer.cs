﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using NVorbis;
using Plex.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plex.Engine.GameComponents.Audio
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

        public float[] Samples => samps;

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
            samps = new float[aread.Channels * aread.SampleRate / 64];
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
            try
            {
                if (skipearly || aread.DecodedTime.TotalSeconds >= labels[cur].End)
                {
                    fade = null;
                    if (Next >= labels.Length)
                    {
                        Stop();
                        return;
                    }
                    double lastend = labels[cur].End;
                    var nextTime = TimeSpan.FromSeconds(labels[Next].Start + aread.DecodedTime.TotalSeconds - lastend);
                    if (nextTime < TimeSpan.Zero || nextTime > aread.TotalTime)
                    {
                        Stop();
                        return;
                    }
                    cur = Next;
                    aread.DecodedTime = nextTime;
                    Next = cur + (labels[cur].OneShot ? 1 : 0);
                }
            }
            catch (NullReferenceException)
            {
                Stop();
            }
            finally
            {
                data = null;
            }
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

        public TimeSpan Position => aread.DecodedTime;

        /// <summary>
        /// Resumes playback.
        /// </summary>
        public void Resume()
        {
            sfx.Resume();
        }

        public SoundState State => sfx.State;

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

        public string Album { get; private set; }
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string AlbumArtist { get; private set; }
        public string Composer { get; private set; }
        public uint Year { get; private set; }
        public Texture2D AlbumArt { get; private set; }

        /// <summary>
        /// Set up an AdvancedAudioPlayer from an Ogg Vorbis file alone.
        /// </summary>
        /// <param name="fname">The name of the song's Ogg Vorbis file.</param>
        /// <param name="loop">If set to <c>true</c>, the audio will loop.</param>
        public AdvancedAudioPlayer(string fname, bool loop)
        {
            var f = TagLib.File.Create(fname);

            var tag = f.Tag;
            Album = tag.Album;
            Title = tag.Title;

            Artist = tag.FirstPerformer;
            AlbumArtist = tag.FirstAlbumArtist;
            Composer = tag.FirstComposer;

            var art = tag.Pictures.FirstOrDefault(x => x.Type == TagLib.PictureType.FrontCover);
            if(art != null)
            {
                
            }

            Year = tag.Year;

            f.Dispose();

            construct(File.OpenRead(fname), null, true);
            labels = new[] { new Label(0, aread.TotalTime.TotalSeconds, !loop) };
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
}
