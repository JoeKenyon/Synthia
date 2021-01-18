using NAudio.Wave;
using PluginFramework;
using System.Collections.Generic;

namespace Delay
{
    public class Delay : SynthEffectPlugin
    {
        List<float> buffer;
        int position = 0;
        float decay = 0.25f;
        int delayInSamples = (int)(44100 * 0.5f);

        public Delay(ISampleProvider input) : base(input)
        {
            Name = "Delay";
            buffer = new List<float>(WaveFormat.SampleRate * 10);
            for (int i = 0; i < buffer.Capacity; i++) buffer.Add(0);

            Properties = new Property[]
            {
                new SliderProperty()
                {
                    Name="Delay",
                    Min=0, Max=1,
                    ValueString = "{0:0.00}s",
                    onChange = (x) =>
                    {
                        delayInSamples = (int)((double)x * WaveFormat.SampleRate);
                        for (int i = 0; i < buffer.Capacity; i++) buffer[i] = 0;
                    },
                },

                new SliderProperty()
                {
                    Name="Decay",
                    Min=0, Max=0.5f,
                    ValueString = "{0:0.0}",
                    onChange = (x) => decay = (float)(double)x,
                },
            };
        }

        int mod(int dividend, int divisor)
        {
            if (dividend == 0) return 0;
            if ((dividend > 0) == (divisor > 0))
                return dividend % divisor;
            else
                return (dividend % divisor) + divisor;
        }

        public override float Apply(float sample)
        {
            buffer[position] = sample;

            int pos = mod((position - delayInSamples), buffer.Count);

            var delayedSample = buffer[pos];

            buffer[position] = (delayedSample * decay) + sample;

            position = ((position+1) % buffer.Count);

            return (delayedSample * decay) + sample;
        }
    }
}
