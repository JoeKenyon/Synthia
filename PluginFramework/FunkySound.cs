using PluginFramework;
using System;

namespace FunkySound
{


    /*
     * Custom sound, just returns a sine wave with
     * a few harmonics.
     */
    public class FunkySound : SynthSourcePlugin
    {
        public override string Name => "FunkySound";

        public override Func<float, float> GetSample => (phase) =>
        {
            float output = (float)Math.Cos((phase) * (2 * Math.PI)); // 0 octave;            
            output += (float)Math.Cos((phase / 2) * (2 * Math.PI));  // -1 octave
            output += (float)Math.Cos((phase / 4) * (2 * Math.PI));  // -2 octave
            output += (float)Math.Cos((phase / 8) * (2 * Math.PI));  // -3 octave
            output += (float)Math.Cos((phase * 2) * (2 * Math.PI));  // +1  octave
            output += (float)Math.Cos((phase * 4) * (2 * Math.PI));  // +2  octave
            output += (float)Math.Cos((phase * 8) * (2 * Math.PI));  // +3  octave


            return output;
        };
    }
}

