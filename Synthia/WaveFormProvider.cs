using System;
using NAudio.Wave;

namespace Synthia
{
    public class WaveFormProvider : ISampleProvider
    {
        public Action<float[]> Update { get; set; }

        public ISampleProvider Input { get; }

        public EventHandler OnRead { get; set; }

        public float[] Data { get; set; }

        public WaveFormat WaveFormat { get; set; }

        public WaveFormProvider(ISampleProvider input)
        {
            WaveFormat = input.WaveFormat;
            Input = input;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            // the length of the wave form will determine
            // how many points per wave cycle
            // it seems that this is the perfect value
            float[] _data = new float[200];
            int samples = Input.Read(buffer, offset, count);
            int step = (500 / _data.Length);

            // down sample are wave so we can display
            // properly on our waveVisualizer.
            float acc = 0.0f;
            int samplesProc = 0;
            int steps = 0;
            for (int i = 0; i < 500; i++)
            {
                // add to our accumulator
                acc += buffer[i];
                samplesProc++;

                // take an average of the
                // samples accumulated
                if (samplesProc == step)
                {
                    float average = acc / samplesProc;
                    _data[steps++] = average;
                    if (steps == _data.Length)
                        break;
                    samplesProc = 0;
                    acc = 0.0F;
                }
            }


            // update the wave form
            // this will force the wave to be
            // re displayed
            Data = _data;
            OnRead?.Invoke(this, new EventArgs());

            return samples;
        }
    }
}

