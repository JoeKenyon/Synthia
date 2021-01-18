using NAudio.Wave;
using PluginFramework;
using SynthiaTools;
using System;

namespace WahWahEffect
{
    public class WahWah : SynthEffectPlugin
    {
        private float _phase;
        private BiQuadFilter _filter;

        public WahWah(ISampleProvider input) : base(input)
        {
            Name = "WahWah";
            _filter = new BiQuadFilter(input.WaveFormat);
            _filter.FilterType = Filter.BandPass;
            _filter.Wet = 1;
            _lfoFunc = x => (float)Math.Cos(x * 2 * Math.PI);

            Properties = new Property[]
            {
                new SliderProperty()
                {
                    Name="CentreFreq",
                    Min=20, Max=4000,
                    ValueString="{0:0.0}Hz",
                    onChange = (x) => _mainFreq = Convert.ToSingle(x),
                },
                new SliderProperty()
                {
                    Name="Freq",
                    Min=0,
                    Max=7,
                    ValueString="{0:0.0}Hz",
                    onChange = (x) => _freq = Convert.ToSingle(x),
                },
                new SliderProperty()
                {
                    Name="Amp",
                    Min=0, Max=1,
                    ValueString="{0:0.0}",
                    onChange = (x) => _amp = Convert.ToSingle(x),
                },
                new SliderProperty()
                {
                    Name="Q",
                    Min=-3,//707,
                    Max=20,//5000,
                    ValueString="{0:0.0}dB",
                    onChange = (x) => _filter.Q = (float)Math.Pow(10.0f, Convert.ToSingle(x)/20.0f),
                },
                new SliderProperty()
                {
                    Name="Wet",
                    Min=0, Max=1,
                    ValueString="{0:0.0}",
                    onChange = (x) => Wet = Convert.ToSingle(x),
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
                },
                new DropDownProperty()
                {
                    Name="FilterType",
                    Items = new string[]{"BandPass", "HighPass", "LowPass"},
                    onChange = (s) =>
                    {
                        switch (s)
                        {
                            case "BandPass":
                                _filter.FilterType = Filter.BandPass;
                                break;
                            case "HighPass":
                                _filter.FilterType = Filter.HighPass;
                                break;
                            case "LowPass":
                                _filter.FilterType = Filter.LowPass;
                                break;
                            default:
                                _filter.FilterType = Filter.LowPass;
                                break;
                        }
                    },
                }
            };
        }

        private float _mainFreq;
        private float _freq;
        private float _amp;
        private Func<float, float> _lfoFunc;

        public override float Apply(float sample)
        {
            _phase = (_phase + _freq / 44100) % 1;

            var lfo = _lfoFunc(_phase);

            _filter.Cutoff = (float)((lfo * _amp * _mainFreq) + _mainFreq);

            sample = _filter.Apply(sample);

            return sample;
        }
    }
}
