using System;
using System.Threading;
using System.IO;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using OpenTK.Input;

namespace Toolbox.Core.Audio
{
    public class WavePlayerAL : IDisposable
    {
        int buffer;
        int source;
        AudioContext context;

        public void Load(string filePath) {
            Load(File.OpenRead(filePath));
        }

        public void Load(Stream stream)
        {
            context = new AudioContext();
            int state;

            buffer = AL.GenBuffer();
            source = AL.GenSource();

            WaveData waveFile = new WaveData(stream);
            AL.BufferData(buffer, waveFile.SoundFormat, waveFile.SoundData, waveFile.SoundData.Length, waveFile.SampleRate);

            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.SourcePlay(source);

            Console.WriteLine("Playing");

            // Query the source to find out when it stops playing.
            do
            {
                Thread.Sleep(250);
                Console.Write(".");
                AL.GetSource(source, ALGetSourcei.SourceState, out state);
            }
            while ((ALSourceState)state == ALSourceState.Playing);

            Console.WriteLine("FIN");

            AL.SourceStop(source);
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);

    /*        waveFile.dispose();

            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.Source(source, ALSourceb.Looping, true);
            AL.GenSources(source);

            Console.WriteLine($"context {AudioContext.AvailableDevices.Count > 0}");
            Console.WriteLine($"GENERATING WAV SRC {source} {waveFile.SoundFormat} {waveFile.SampleRate}");*/
        }

        public void Play() {
            AL.SourcePlay(source);
        }

        public void Stop() {
            AL.SourceStop(source);
        }

        public void Pause() {
            AL.SourcePause(source);
        }

        public void Dispose() {
            Destroy();
            context.Dispose();
        }

        private void Destroy()
        {
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);
        }
    }
}
