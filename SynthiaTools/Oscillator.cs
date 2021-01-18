using NAudio.Wave;
using System;
namespace SynthiaTools
{
    public class Oscillator : ISampleProvider
    {
        private readonly BiQuadFilter _filter;
        private readonly Envelope _amplitudeEnvelope;
        private readonly Envelope _filterEnvelope;
        private double[] _phase;
        private double _lfoPhase;
        private float _baseFrequency;
        private int _note;

        private readonly static double _twelfthRootOfTwo = Math.Pow(2, 1.0 / 12.0);

        public WaveFormat WaveFormat { get; set; }

        public EventHandler FinishedPlaying;
        public Oscillator(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;

            _filterEnvelope = new Envelope(WaveFormat.SampleRate);
            _amplitudeEnvelope = new Envelope(WaveFormat.SampleRate);
            _filter = new BiQuadFilter(waveFormat);

            _phase = new double[3] { 0.0, 0.0, 0.0};
            _lfoPhase = 0.0;

            IsPlaying = false;

            FilterCutoff = 1000;
            FilterEnvelopeWidth = 0;

            FilterAttack = .001f;
            FilterDecay = 2;
            FilterSustain = 0;
            FilterRelease = .001f;

            FilterWet = 0;

            Attack = .001f;
            Decay = 0;
            Sustain = 1;
            Release = .001f;

            AmplitudeOsc1 = 1;
            AmplitudeOsc2 = 0;
            AmplitudeOsc3 = 0;

            _baseFrequency = 440;
            _note = 57;
            OctaveOsc1 = 1;
            OctaveOsc2 = 1;
            OctaveOsc3 = 1;
            SemitonesOsc1 = 0;
            SemitonesOsc2 = 0;
            SemitonesOsc3 = 0;

            LFOAmplitude = 0.5d;
            LFOFrequency = 5d;
            UseLFO = false;         
        }


        public float Attack
        {
            get => _amplitudeEnvelope.AttackRate;
            set => _amplitudeEnvelope.AttackRate = value;
        }

        public float Decay
        {
            get => _amplitudeEnvelope.DecayRate;
            set => _amplitudeEnvelope.DecayRate = value;
        }

        public float Sustain
        {
            get => _amplitudeEnvelope.SustainLevel;
            set => _amplitudeEnvelope.SustainLevel = value;
        }

        public float Release
        {
            get => _amplitudeEnvelope.ReleaseRate;
            set => _amplitudeEnvelope.ReleaseRate = value;
        }

        public float FilterQ
        {
            get => _filter.Q;
            set => _filter.Q = value;
        }

        public Filter FilterType
        {
            get => _filter.FilterType;
            set => _filter.FilterType = value;
        }

        public float FilterAttack
        {
            get => _filterEnvelope.AttackRate;
            set => _filterEnvelope.AttackRate = value;
        }

        public float FilterDecay
        {
            get => _filterEnvelope.DecayRate;
            set => _filterEnvelope.DecayRate = value;
        }

        public float FilterSustain
        {
            get => _filterEnvelope.SustainLevel;
            set => _filterEnvelope.SustainLevel = value;
        }

        public float FilterRelease
        {
            get => _filterEnvelope.ReleaseRate;
            set => _filterEnvelope.ReleaseRate = value;
        }

        public float FilterWet
        {
            get => _filter.Wet;
            set => _filter.Wet = value;
        }

        public float FrequencyOsc1 { get; set; }
        public float FrequencyOsc2 { get; set; }
        public float FrequencyOsc3 { get; set; }
        public int SemitonesOsc1 { get; set; }
        public int SemitonesOsc2 { get; set; }
        public int SemitonesOsc3 { get; set; }
        public int OctaveOsc1 { get; set; }
        public int OctaveOsc2 { get; set; }
        public int OctaveOsc3 { get; set; }
        public float AmplitudeOsc1 { get; set; }
        public float AmplitudeOsc2 { get; set; }
        public float AmplitudeOsc3 { get; set; }
        public float FilterCutoff { get; set; }
        public int FilterEnvelopeWidth { get; set; }
        public double LFOFrequency { get; set; }
        public double LFOAmplitude { get; set; }
        public bool UseLFO { get; set; }
        public Func<float, float> FunctionOsc1 { get; set; }
        public Func<float, float> FunctionOsc2 { get; set; }
        public Func<float, float> FunctionOsc3 { get; set; }


        private float NoteToFreq(int note, float tuning)
        {
            return (float)(tuning * Math.Pow(_twelfthRootOfTwo, note - 57));
        }

        public int Note
        {
            get => _note;
            set
            {
                _note = value;
                FrequencyOsc1 = NoteToFreq(_note + SemitonesOsc1 + (12 * OctaveOsc1), _baseFrequency);
                FrequencyOsc2 = NoteToFreq(_note + SemitonesOsc2 + (12 * OctaveOsc2), _baseFrequency);
                FrequencyOsc3 = NoteToFreq(_note + SemitonesOsc3 + (12 * OctaveOsc3), _baseFrequency);
            }
        }

        public bool IsPlaying { get; set; }

        public bool OnRelease => _amplitudeEnvelope.State == Envelope.EnvState.Release;
        public bool OnSustain => _amplitudeEnvelope.State == Envelope.EnvState.Sustain;

        public bool UseFilterEnvelope { get; set; }

        public void NoteOn()
        {
            _filterEnvelope.State = Envelope.EnvState.Attack;
            _amplitudeEnvelope.State = Envelope.EnvState.Attack;
            IsPlaying = true;
        }

        public void NoteOff()
        {
            _filterEnvelope.State = Envelope.EnvState.Release;
            _amplitudeEnvelope.State = Envelope.EnvState.Release;
        }

        public int Read(float[] buffer, int offset, int sampleCount)
        {
            for (var index = 0; index < sampleCount; index += WaveFormat.Channels)
            {
                if (_amplitudeEnvelope.State != Envelope.EnvState.Idle)
                {
                    _lfoPhase = (_lfoPhase + LFOFrequency / 44100) % 1;

                    if (UseLFO)
                    {
                        var lfo = Math.Sin(Math.PI * _lfoPhase * 2);
                        var freq1 = (lfo * LFOAmplitude * FrequencyOsc1) + FrequencyOsc1;
                        var freq2 = (lfo * LFOAmplitude * FrequencyOsc2) + FrequencyOsc2;
                        var freq3 = (lfo * LFOAmplitude * FrequencyOsc3) + FrequencyOsc3;
                        _phase[0] = (_phase[0] + freq1 / WaveFormat.SampleRate) % 1;
                        _phase[1] = (_phase[1] + freq2 / WaveFormat.SampleRate) % 1;
                        _phase[2] = (_phase[2] + freq3 / WaveFormat.SampleRate) % 1;
                    }
                    else
                    {
                        _phase[0] = (_phase[0] + FrequencyOsc1 / WaveFormat.SampleRate) % 1;
                        _phase[1] = (_phase[1] + FrequencyOsc2 / WaveFormat.SampleRate) % 1;
                        _phase[2] = (_phase[2] + FrequencyOsc3 / WaveFormat.SampleRate) % 1;
                    }

                    var sOsc1 = FunctionOsc1((float)_phase[0]) * (AmplitudeOsc1);
                    var sOsc2 = FunctionOsc2((float)_phase[1]) * (AmplitudeOsc2);
                    var sOsc3 = FunctionOsc3((float)_phase[2]) * (AmplitudeOsc3);

                    var sampleValue = (sOsc1 + sOsc2 + sOsc3);

                    buffer[offset + index] = sampleValue * _amplitudeEnvelope.Process();

                    for (var channel = 1; channel < WaveFormat.Channels; ++channel)
                        buffer[offset + index + channel] = buffer[offset + index];
                }
                else
                {
                    IsPlaying = false;
                    FinishedPlaying?.Invoke(this, new EventArgs());

                    for (var channel = 0; channel < WaveFormat.Channels; ++channel)
                        buffer[index + offset + channel] = 0;
                }
            }

            if (Math.Abs(_filter.Wet) < 0 || !UseFilterEnvelope)
                return sampleCount;

            for (var i = 0; i < sampleCount; ++i)
            {
                _filter.Cutoff = (float)(FilterCutoff * Math.Pow(2, FilterEnvelopeWidth * _filterEnvelope.Process()));
                buffer[offset + i] = _filter.Apply(buffer[offset + i]) * _filter.Wet + buffer[offset + i] * (1 - _filter.Wet);
            }

            return sampleCount;
        }
    }
}
