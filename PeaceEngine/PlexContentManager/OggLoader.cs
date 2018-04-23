using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Plex.Objects;
using System.Diagnostics;

namespace Plex.Engine.PlexContentManager
{
    public class OggLoader : ILoader<SoundEffect>
    {
        public IEnumerable<string> Extensions => new[] { ".OGG" };

        const double sc16 = 0x7FFF + 0.4999999999999999;

        public SoundEffect Load(Stream fobj)
        {
            using (var stream = new NVorbis.VorbisReader(fobj, false))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logger.Log($"Reading Vorbis file...");
                AudioChannels channels;
                switch (stream.Channels)
                {
                    case 1:
                        channels = AudioChannels.Mono;
                        break;
                    case 2:
                        channels = AudioChannels.Stereo;
                        break;
                    default:
                        throw new InvalidDataException($"Unsupported channel count {stream.Channels} (must be mono or stereo).");
                }
                var samps = new float[stream.Channels * stream.SampleRate / 5];
                Logger.Log($"TotalSamples = {stream.TotalSamples}");
                using (var ms = new MemoryStream())
                {
                    using (var write = new BinaryWriter(ms))
                    {
                        int cnt = 0;
                        while((cnt = stream.ReadSamples(samps, 0, samps.Length)) != 0)
                            foreach(var samp in samps)
                                write.Write((short)(samp * sc16)); // convert to S16 int PCM
                    }
                    sw.Stop();
                    Logger.Log($"Done: {sw.Elapsed}");
                    return new SoundEffect(ms.ToArray(), stream.SampleRate, channels);
                }
            }
        }
    }
}
