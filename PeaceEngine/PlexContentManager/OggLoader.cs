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
                //Stopwatch is used to calculate load times in a debug build.
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Logger.Log($"Reading Vorbis file...");
#endif
                //We need to know whether this is a Mono or Stereo audio file. MonoGame only supports 1-channel or 2-channel audio data, so if it's anything else, throw an error.
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
                //This is our sample buffer. We don't wanna read the entire file all at once, that's a RAM killer. So we'll read chunks into a buffer this size.
                var samps = new float[stream.Channels * stream.SampleRate / 5];
#if DEBUG
                Logger.Log($"TotalSamples = {stream.TotalSamples}");
#endif
                //This is where we'll write the raw PCM data for MonoGame.
                using (var ms = new MemoryStream())
                {
                    //This is how we'll write the raw PCM data for MonoGame.
                    using (var write = new BinaryWriter(ms))
                    {
                        //Keep reading the Vorbis file until we've got nothing left to read.
                        while (stream.DecodedPosition<stream.TotalSamples-1)
                        {
                            //Read data from the Vorbis stream into our sample buffer. ReadSamples() returns how many samples were written to the buffer.
                            //We'll use that to know how many samples to convert to PCM instead of blindly writing to the PCM stream causing audible hitches at the end of the sound effect due to duplicate sample data being written.
                            int read = stream.ReadSamples(samps, 0, samps.Length);
                            for (int i = 0; i < read; i++)
                            {
                                write.Write((short)(samps[i] * sc16)); // convert to S16 int PCM
                            }
                        }
                    }
                    //Deallocate the read buffer
                    samps = null;
                    //Force garbage collection
                    GC.Collect();
#if DEBUG
                    sw.Stop();
                    Logger.Log($"Done: {sw.Elapsed}");
#endif
                    return new SoundEffect(ms.ToArray(), stream.SampleRate, channels);
                }
            }
        }
    }
}
