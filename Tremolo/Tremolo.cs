using NAudio.Wave;
using PluginFramework;
using System;

namespace Tremolo
{
    public class Tremolo : SynthEffectPlugin
    {
        private float _phase;
        private float _freq;
        private float _amp;
        private Func<float, float> _lfoFunc;

        public Tremolo(ISampleProvider input) : base(input)
        {
            Name = "Tremolo";
            _lfoFunc = x => (float)Math.Cos(x * 2 * Math.PI);
            Properties = new Property[]
            {
                new SliderProperty()
                {
                    Name="Freq",
                    Min=0, Max=20,
                    ValueString = "{0:0.0}Hz",
                    onChange = (x) => _freq = (float)x,
                },
                new SliderProperty()
                {
                    Name="Amp",
                    Min=0, Max=1,
                    ValueString = "{0:0.0}",
                    onChange = (x) => _amp =(float)x,
                },
                new DropDownProperty()
                {
                    Name="WaveForm",
                    Items = new string[]{"Sine","Sawtooth","Square","Triangle"},
                    onChange = (s) =>
                    {
                        switch (s)
                        {
                            case "Sine":
                                _lfoFunc = x => (float)Math.Cos(x * 2 * Math.PI);
                                break;
                            case "Sawtooth":
                                _lfoFunc = x => 2 * x - 1;
                                break;
                            case "Square":
                                _lfoFunc = x => x > 0.5 ? 1 : -1;
                                break;
                            case "Triangle":
                                _lfoFunc = x => x < 0.5 ? x * 4 - 1 : 3 - 4 * x;
                                break;
                            default:
                                _lfoFunc = x => (float)Math.Cos(x * 2 * Math.PI);
                                break;
                        }
                    },
                }
            };
        }

        public override float Apply(float sample)
        {
            _phase = (_phase + _freq / WaveFormat.SampleRate) % 1;

            if (_amp > 0.0f)
            {
                var lfoSample = _lfoFunc(_phase) * _amp;
                sample += sample * lfoSample;
            }

            return sample;
        }
    }
}
