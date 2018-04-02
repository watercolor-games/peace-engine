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

namespace Plex.Engine
{
    public class AdvancedAudioPlayer : IDisposable
    {
        const int bufferSize = 1024; // in samples

        const double sc16 = 0x7FFF + 0.4999999999999999;

        DynamicSoundEffectInstance sfx;
        VorbisReader aread;


        int cur = 0;

        /// <summary>
        /// The audio section that will be played after the current one has finished.
        /// </summary>
        int Next { get; set; } = 1;

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
        float[] samps = new float[bufferSize];
        byte[] data = new byte[bufferSize * 2];

        public void Dispose()
        {
            aread?.Dispose();
            sfx?.Dispose();
            aread = null;
            sfx = null;
            labels = null;
            samps = null;
        }

        void construct(Stream audio, Stream labels, bool close)
        {
            if (labels != null)
                using (var read = new StreamReader(labels, Encoding.UTF8, true, 1024, !close))
                    this.labels = read.IterLines().Select(l => l.Split()).Select(s => new Label(double.Parse(s[0]), double.Parse(s[1]), s[2] == "oneshot")).ToArray();
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
            sfx = new DynamicSoundEffectInstance(aread.SampleRate, channels);
            sfx.BufferNeeded += update;
        }

        public void update(object sender, EventArgs e)
        {
            aread.ReadSamples(samps, 0, bufferSize);
            using (var ms = new MemoryStream(data))
            using (var write = new BinaryWriter(ms))
                foreach (var samp in samps)
                    write.Write((short)(samp * sc16)); // convert to S16 int PCM
            sfx.SubmitBuffer(data);
            if (aread.DecodedTime.TotalSeconds >= labels[cur].End)
            {
                if (Next >= labels.Length)
                {
                    Stop();
                    return;
                }
                cur = Next;
                aread.DecodedTime = TimeSpan.FromSeconds(labels[cur].Start + aread.DecodedTime.TotalSeconds - labels[cur].End);
                Next = cur + (labels[cur].OneShot ? 0 : 1);
            }
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
        void Pause()
        {
            sfx.Pause();
        }

        /// <summary>
        /// Starts or resumes playback.
        /// </summary>
        void Play()
        {
            sfx.Play();
        }

        /// <summary>
        /// Resumes playback.
        /// </summary>
        void Resume()
        {
            sfx.Resume();
        }

        SoundState State => sfx.State;

        /// <summary>
        /// Stops the player immediately.
        /// </summary>
        void Stop()
        {
            sfx.Stop();
        }

        /// <summary>
        /// Stops the player once the current section is over.
        /// </summary>
        void StopNext()
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
        public AdvancedAudioPlayer(Stream audio, bool loop = false, bool close = false)
        {
            construct(audio, null, close);
            labels = new[] { new Label(0, aread.TotalTime.TotalSeconds, !loop) };
        }

        /// <summary>
        /// Set up an AdvancedAudioPlayer from an Ogg Vorbis file alone.
        /// </summary>
        /// <param name="fname">The name of the song's Ogg Vorbis file.</param>
        /// <param name="loop">If set to <c>true</c>, the audio will loop.</param>
        public AdvancedAudioPlayer(string fname, bool loop = false) : this(File.OpenRead(fname), loop, true)
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
}
