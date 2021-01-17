using PluginFramework;
using System;


/*
 * Contains the default wave sources for our synth 
 */
namespace BaseSources
{

    public class Sine : SynthSourcePlugin
    {
        public override string Name => "Sine";
        public override Func<float, float> GetSample => (phase) =>
        {
            return (float)Math.Sin(Math.PI * 2 * phase);
        };
    }

    public class Sawtooth : SynthSourcePlugin
    {
        public override string Name => "Sawtooth";
        public override Func<float, float> GetSample => (phase) =>
        {
            return 2 * phase - 1;
        };
    }

    public class Square : SynthSourcePlugin
    {
        public override string Name => "Square";
        public override Func<float, float> GetSample => (phase) =>
        {
            return phase > 0.5 ? 1 : -1;
        };
    }

    public class Triangle : SynthSourcePlugin
    {
        public override string Name => "Triangle";
        public override Func<float, float> GetSample => (phase) =>
        {
            return phase < 0.5 ? phase * 4 - 1 : 3 - 4 * phase;
        };
    }

    public class Noise : SynthSourcePlugin
    {
        private readonly Random _random = new Random();
        public override string Name => "Noise";
        public override Func<float, float> GetSample => (phase) =>
        {
            return ((float)_random.NextDouble() * 2) - 1;
        };
    }


    /*
     * Sine wave that alters freq on the fly
     */
    public class SineSweep : SynthSourcePlugin
    {
        private float _lfoPhase = 0;
        private float LFOFrequncy = 2;
        private float LFOAmplitude = 0.5f;
        public override string Name => "SineSweep";
        public override Func<float, float> GetSample => (phase) =>
        {
            _lfoPhase = (_lfoPhase + LFOFrequncy / 44100) % 1;
            var lfo = Math.Sin(Math.PI * _lfoPhase * 2);
            var freq1 = (lfo * LFOAmplitude * phase) + phase;
            return (float)Math.Sin(Math.PI * 2 * (freq1));
        };
    }
}

