using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthiaTools
{
    public class Mixer : ISampleProvider
    {
        private readonly List<ISampleProvider> _inputs;
        public int MaxInputs => 1024;
        public MixerMode Mode { get; set; }

        public Mixer(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
            _inputs = new List<ISampleProvider>(MaxInputs);
            Mode = MixerMode.Additive;
        }

        public Mixer(ISampleProvider firstInput) : this( firstInput.WaveFormat)
        {
            lock (_inputs)
                _inputs.Add(firstInput);
        }
        
        public void AddInput(ISampleProvider input)
        {
            lock (_inputs)
            {
                if (_inputs.Count >= MaxInputs)
                {
                    throw new InvalidOperationException("Mixer max input count reached");
                }
                else
                    _inputs.Add(input);
            }

            if (WaveFormat == null)
                WaveFormat = input.WaveFormat;
            else
                if (WaveFormat.SampleRate != input.WaveFormat.SampleRate || WaveFormat.Channels != input.WaveFormat.Channels)
                    throw new ArgumentException("All mixer inputs must have the same WaveFormat");
        }

        public void AddInputs(IEnumerable<ISampleProvider> inputs)
        {
            inputs.ToList().ForEach(AddInput);
        }

        public void RemoveInput(ISampleProvider input)
        {
            lock (_inputs)
            {
                _inputs.Remove(input);
            }
                
        }
        public void RemoveAllInputs()
        {
            lock (_inputs)
                _inputs.ToList().ForEach(RemoveInput);
        }

        public int InputCount => _inputs.Count;

        public WaveFormat WaveFormat { get; set; }

        public int Read(float[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
                buffer[offset + i] = 0;

            float[] temp = new float[count];
            foreach (var input in _inputs.ToList())
            {
                input.Read(temp, 0, count);

                for (int i = 0; i < count; i++)
                    buffer[offset + i] += temp[i];
            }

            if (Mode == MixerMode.Averaging && _inputs.Count != 0)
                    for (int i = 0; i < count; i++)
                        buffer[offset + i] /= _inputs.Count;

            return count;
        }
        
    }

    public enum MixerMode
    {
        Additive,
        Averaging
    }
}
