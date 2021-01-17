using System;
using NAudio.Wave;
using System.Collections.Generic;
using PluginFramework;
using System.Reflection;
using System.IO;
using SynthiaTools;

namespace Synthia
{
    public class Synth : ISampleProvider
    {
        private List<Oscillator> _oscillators;
        private ISampleProvider _lastInput;
        private Mixer _mixer;

        public Synth(int sampleRate, int maxOsc)
        {
            /* Synth only uses 1 channel as it doesnt matter in my eyes */
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

            /* for mixing oscillators input */
            _mixer = new Mixer(WaveFormat);

            /* Hold any plugins we find, so we can reference them in the UI */
            Effects = new List<SynthEffectPlugin>();
            Sources  = new List<SynthSourcePlugin>();

            /* gotta keep track of last input just incase we add effects. */
            _lastInput = _mixer;

            _oscillators = new List<Oscillator>(maxOsc);
            for (var i = 0; i < maxOsc; ++i)
                _oscillators.Add(new Oscillator(WaveFormat));

            /* default values, these don't really matter */
            FilterWet = 0;
            FilterEnvelopeOctaves = 0;
            WaveFormOsc1 = new BaseSources.Sine();
            WaveFormOsc2 = new BaseSources.Sine();
            WaveFormOsc3 = new BaseSources.Sine();
            AmplitudeOsc1 = 0.0f;
            AmplitudeOsc2 = 0.0f;
            AmplitudeOsc3 = 0.0f;
            Attack = 0.01f;
            Decay = 0.01f;
            Sustain = 1.0f;
            Release = 0.01f;
            
            /* When finished playing we want to remove mixer input */
            _oscillators.ForEach(osc => osc.FinishedPlaying = (s, e) =>
            {
                _mixer.RemoveInput(osc);
            });

        }

        public WaveFormat WaveFormat { get; set; }
        public List<SynthEffectPlugin> Effects { get; set; }
        public List<SynthSourcePlugin> Sources { get; set; }


        #region Synth properties

        public int SemitonesOsc1
        {
            set{ _oscillators.ForEach(osc => osc.SemitonesOsc1 = value); }
        }

        public int SemitonesOsc2
        {
            set{ _oscillators.ForEach(osc => osc.SemitonesOsc2 = value); }
        }

        public int SemitonesOsc3
        {
            set { _oscillators.ForEach(osc => osc.SemitonesOsc3 = value); }
        }

        public int OctaveOsc1
        {
            set { _oscillators.ForEach(osc => osc.OctaveOsc1 = value); }
        }

        public int OctaveOsc2
        {
            set { _oscillators.ForEach(osc => osc.OctaveOsc2 = value); }
        }

        public int OctaveOsc3
        {
            set { _oscillators.ForEach(osc => osc.OctaveOsc3 = value); }
        }

        public SynthSourcePlugin WaveFormOsc1
        {
           set { _oscillators.ForEach(osc => osc.FunctionOsc1 = value.GetSample); }
        }

        public SynthSourcePlugin WaveFormOsc2
        {
            set { _oscillators.ForEach(osc => osc.FunctionOsc2 = value.GetSample); }
        }

        public SynthSourcePlugin WaveFormOsc3
        {
            set { _oscillators.ForEach(osc => osc.FunctionOsc3 = value.GetSample); }
        }

        public double LFOFrequency
        {
            get => _oscillators[0].LFOFrequency;
            set { _oscillators.ForEach(osc => osc.LFOFrequency = value); }
        }

        public double LFOAmplitude
        { 
            get => _oscillators[0].LFOAmplitude;
            set { _oscillators.ForEach(osc => osc.LFOAmplitude = value); }
        }

        public bool UseLFO
        {
            get => _oscillators[0].UseLFO;
            set { _oscillators.ForEach(osc => osc.UseLFO = value); }
        }

        public float MasterAmplitude{ get; set; }

        public float AmplitudeOsc1
        {
            get => _oscillators[0].AmplitudeOsc1;
            set { _oscillators.ForEach(osc => osc.AmplitudeOsc1 = value); }
        }

        public float AmplitudeOsc2
        {
            get => _oscillators[0].AmplitudeOsc2;
            set { _oscillators.ForEach(osc => osc.AmplitudeOsc2 = value); }
        }

        public float AmplitudeOsc3
        {
            get => _oscillators[0].AmplitudeOsc3;
            set { _oscillators.ForEach(osc => osc.AmplitudeOsc3 = value); }
        }

        public float Attack
        {
            get => _oscillators[0].Attack;
            set { _oscillators.ForEach(osc => osc.Attack = value); }
        }

        public float Decay
        {
            get => _oscillators[0].Decay;
            set { _oscillators.ForEach(osc => osc.Decay = value); }
        }

        public float Sustain
        {
            get => _oscillators[0].Sustain;
            set { _oscillators.ForEach(osc => osc.Sustain = value); }
        }

        public float Release
        {
            get => _oscillators[0].Release;
            set { _oscillators.ForEach(osc => osc.Release = value); }
        }

        public float FilterCutoff
        {
            get => _oscillators[0].FilterCutoff;
            set { _oscillators.ForEach(osc => osc.FilterCutoff = value); }
        }

        public float FilterQ
        {
            get => _oscillators[0].FilterQ;
            set { _oscillators.ForEach(osc => osc.FilterQ = value); }
        }

        public Filter FilterType
        {
            get => _oscillators[0].FilterType;
            set { _oscillators.ForEach(osc => osc.FilterType = value); }
        }

        public float FilterWet
        {
            get => _oscillators[0].FilterWet;
            set { _oscillators.ForEach(osc => osc.FilterWet = value); }
        }

        public float FilterAttack
        {
            get => _oscillators[0].FilterAttack;
            set { _oscillators.ForEach(osc => osc.FilterAttack = value); }
        }

        public float FilterSustain
        {
            get => _oscillators[0].FilterSustain;
            set { _oscillators.ForEach(osc => osc.FilterSustain = value); }
        }

        public float FilterDecay
        {
            get => _oscillators[0].FilterDecay;
            set { _oscillators.ForEach(osc => osc.FilterDecay = value); }
        }

        public float FilterRelease
        {
            get => _oscillators[0].FilterRelease;
            set { _oscillators.ForEach(osc => osc.FilterRelease = value); }
        }

        public int FilterEnvelopeOctaves
        {
            get => _oscillators[0].FilterEnvelopeWidth;
            set { _oscillators.ForEach(osc => osc.FilterEnvelopeWidth = value); }
        }

        public bool UseFilterEnvelope
        {
            get => _oscillators[0].UseFilterEnvelope; 
            set { _oscillators.ForEach(osc => osc.UseFilterEnvelope = value); }
        }

        public int MixerInputs
        {
            get => _mixer.InputCount;
        }

        #endregion

        /*
         * Try and find and oscillator thats not playing
         * start the envelope and play the note if we find one
         * 
         * If there's too many mixer inputs we'll catch the 
         * exception thats thrown as a result of this.
         */
        public bool NoteOn(int note)
        {
            Oscillator _osc = _oscillators.Find(osc => !osc.IsPlaying);

            if (_osc != default(Oscillator))
            {
                try
                {
                    _osc.Note = note;
                    _osc.NoteOn();
                    _mixer.AddInput(_osc);
                    return true;
                }
                catch(InvalidOperationException)
                {
                    _osc.Note = note;
                    _osc.NoteOff();
                }
            }
            return false;
                
        }

        public bool NoteOff(int note)
        {
            Oscillator _osc = _oscillators.Find(osc => osc.IsPlaying && osc.Note == note && !osc.OnRelease);
            if (_osc == default(Oscillator)) return false;
            _osc.NoteOff();
            return true;
        }


        public void IntializeEffects()
        {
            int effectsFound = 0;
            Effects.Clear();
            var assembly = Assembly.GetExecutingAssembly();
            var folder = Path.GetDirectoryName(assembly.Location);
            foreach (var dll in Directory.GetFiles(folder, "*.dll"))
            {
                var asm = Assembly.LoadFrom(dll);
                foreach (var type in asm.GetTypes())
                {
                    if (type.BaseType == typeof(SynthEffectPlugin))
                    {
                        if (effectsFound == 0)
                        {
                            var effect = Activator.CreateInstance(type, _mixer) as SynthEffectPlugin;
                            Effects.Add(effect);
                        }
                        else
                        {
                            var effect = Activator.CreateInstance(type, Effects[effectsFound - 1]) as SynthEffectPlugin;
                            Effects.Add(effect);
                        }
                        effectsFound++;
                    }
                }
            }

            if (effectsFound > 0)
                _lastInput = Effects[effectsFound - 1];
        }


        public void IntializeSources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var folder = Path.GetDirectoryName(assembly.Location);
            foreach (var dll in Directory.GetFiles(folder, "*.dll"))
            {
                var asm = Assembly.LoadFrom(dll);
                foreach (var type in asm.GetTypes())
                {
                    if (type.BaseType == typeof(SynthSourcePlugin))
                    {
                        var source = Activator.CreateInstance(type) as SynthSourcePlugin;
                        Sources.Add(source);
                    }
                }
            }
        }

        public void clearInputs()
        {
            _mixer.RemoveAllInputs();
        }

        public static float normalise(float inValue, float min, float max)
        {
            return (inValue - min) / (max - min);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samples = _lastInput.Read(buffer, offset, count);

            for (int i = 0; i < count; i++)
            {
                var sample = buffer[i + offset] * MasterAmplitude;
                buffer[i + offset] = sample;
            }

            return samples;
        }

    }
}
