using NAudio.Wave;
using PluginFramework;
using Synthia;
using SynthiaTools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthiaTest
{
    class TestSynthia
    {
        public class Test : Attribute { }
        static Random r = new Random();
        static WaveFormat WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

        class TestSourcePlugin : SynthSourcePlugin
        {
            public override string Name => "test";
            public override Func<float, float> GetSample => (x) => 1.0f;
        }

        class TestSampleProvider : ISampleProvider
        {
            public WaveFormat WaveFormat { get; set; }
            public TestSampleProvider(WaveFormat wf) { WaveFormat = wf; }
            public int Read(float[] buffer, int offset, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i + offset] = 3.00f;
                }
                return count;
            }
        }

        /*
        Using our testSource class with a simple GetSample method
        we can check if the synths .Read call will return the correct values.
        */
        [Test]
        public static bool SynthSourceTest()
        {

            Synth synth = new Synth(WaveFormat.SampleRate, 64);
            TestSourcePlugin t = new TestSourcePlugin();
            var val = t.GetSample(0);
            synth.WaveFormOsc1 = new TestSourcePlugin();
            synth.WaveFormOsc2 = new TestSourcePlugin();
            synth.WaveFormOsc3 = new TestSourcePlugin();
            synth.MasterAmplitude = 1.0f;
            synth.AmplitudeOsc1 = 1.0f;
            synth.AmplitudeOsc2 = 0.0f;
            synth.AmplitudeOsc3 = 0.0f;
            synth.Attack = 0.001f; // 44.1 samples
            synth.Decay = 0.001f;  // 44.1 samples
            synth.Sustain = 0.5f;

            synth.NoteOn(0);

            float[] buffer = new float[1000];
            synth.Read(buffer, 0, 500);

            int count = 0;
            float expectedSampleValue = synth.Sustain * val;
            int sampleStart = (int)(44.1 + 44.1);
            int expectedCount = 500 - sampleStart;


            // go through until we find sample value
            int i;
            for (i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == expectedSampleValue)
                    break;
            }

            // should be equal to sample start
            if (i == sampleStart)
            {
                // the rest should be expectedSampleValue
                // as note is still on
                for (i = sampleStart; i < buffer.Length; i++)
                {
                    if (buffer[i] != expectedSampleValue)
                        break;
                    count++;
                }
                if (count == expectedCount) return true;
            }


            return false;
        }

        /*
        Test are envelope using preset values
        */
        [Test]
        public static bool EnvelopeTest()
        {
            Func<float, float, float, float, bool> testEnvelope = (a, d, s, r) =>
            {
                Envelope envelope = new Envelope(WaveFormat.SampleRate);
                envelope.AttackRate = a;
                envelope.DecayRate = d;
                envelope.SustainLevel = s;
                envelope.ReleaseRate = r;

                int expectAttackSamples = (int)(envelope.AttackRate * WaveFormat.SampleRate);
                int expectDecaySamples = (int)(envelope.DecayRate * WaveFormat.SampleRate);
                float expectSustainSample = (envelope.SustainLevel);
                int expectReleaseSamples = (int)(envelope.ReleaseRate * WaveFormat.SampleRate);

                var aSamples = 0;
                var dSamples = 0;
                var sSamples = 0f;
                var rSamples = 0;

                envelope.State = Envelope.EnvState.Attack;

                for (int i = 0; i < expectAttackSamples + 10; i++)
                {
                    var val = envelope.Process();
                    if (envelope.State == Envelope.EnvState.Decay) { aSamples = i; break; }
                }

                for (int i = 1; i < expectDecaySamples + 10; i++)
                {
                    envelope.Process();
                    if (envelope.State == Envelope.EnvState.Sustain) { dSamples = i; break; }
                }

                sSamples = envelope.Process();
                envelope.State = Envelope.EnvState.Release;

                for (int i = 0; i < expectReleaseSamples + 10; i++)
                {
                    envelope.Process();
                    if (envelope.State == Envelope.EnvState.Idle) { rSamples = i; break; }
                }

                if (
                    (aSamples == expectAttackSamples) &&
                    (dSamples == expectDecaySamples) &&
                    (sSamples == expectSustainSample) &&
                    (rSamples == expectReleaseSamples)
                    )
                    return true;

                return false;
            };

            bool t1 = testEnvelope(0.01f, 0.001f, 0.76f, 0.01f);
            bool t2 = testEnvelope(0.01f, 0.001f, 0.50f, 0.01f);
            bool t3 = testEnvelope(1.04f, 0.001f, 1.0f, 0.1f);

            return t1 && t2 && t3;

        }

        /*
        This test will ensure the synth deals
        with too many keys pressed correctly.

        We check the synths mixer inputs after
        we return a false from a 
        synth noteOn Call.
        */
        [Test]
        public static bool SynthMaxOscillators()
        {
            Synth synth = new Synth(WaveFormat.SampleRate, 64);

            // should be false when noteson = 64
            // mixer count should also be 64
            int noteson;
            for (noteson = 0; noteson < 100; noteson++)
                if (!synth.NoteOn(r.Next(0, 127))) break;

            if (synth.MixerInputs == 64 && noteson == 64)
                return true;
            else
                return false;
        }

        /*
        Test to see if the mixer will actual mix input sources
        */
        [Test]
        public static bool MixerTypeTest()
        {
            Mixer m = new Mixer(WaveFormat);

            // all give same value
            TestSampleProvider src1 = new TestSampleProvider(WaveFormat);
            TestSampleProvider src2 = new TestSampleProvider(WaveFormat);
            TestSampleProvider src3 = new TestSampleProvider(WaveFormat);
            var readbuffer = new float[3];
            src1.Read(readbuffer, 0, 1);
            var srcSample = readbuffer[0];

            // add sources
            m.AddInput(src1);
            m.AddInput(src2);
            m.AddInput(src3);

            // testing additive mode
            // set mixer input mode
            m.Mode = MixerMode.Additive;
            // fill read buffer with 3 values
            m.Read(readbuffer, 0, 3);
            foreach (var val in readbuffer)
            {
                if (val != srcSample * m.InputCount)
                    return false;
            }

            // testing averaging mode
            // set mixer input mode
            m.Mode = MixerMode.Averaging;
            // fill read buffer with 3 values
            m.Read(readbuffer, 0, 3);
            foreach (var val in readbuffer)
            {
                if (val != (srcSample * m.InputCount) / m.InputCount)
                    return false;
            }

            return true;
        }

        /*
        This test will ensure that mixer input
        is succsfully removed.

        This is assumed by using the 
        mixer input count
        */
        [Test]
        public static bool MixerRemoveInput()
        {
            Mixer m = new Mixer(WaveFormat);

            List<Oscillator> oscs = new List<Oscillator>(64);

            for (int i = 0; i < oscs.Capacity; i++)
                oscs.Add(new Oscillator(WaveFormat));

            foreach (var o in oscs)
                m.AddInput(o);

            if (m.InputCount == 64)
            {
                foreach (var o in oscs)
                    m.RemoveInput(o);

                if (m.InputCount == 0)
                    return true;
            }
            return false;
        }


        /*
        The tests will begin from here
        */
        static void Main(string[] args)
        {
            foreach (var method in typeof(TestSynthia).GetMethods())
            {
                var m = method.CustomAttributes.ToList().Find(a => a.AttributeType == typeof(Test));
                if (m != default)
                    Console.WriteLine(method.Name
                    + (((bool)method.Invoke(null, null) == true) ? " Passed" : " Failed"));
            }
        }
    }
}
