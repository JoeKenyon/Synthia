
using System;

namespace PluginFramework
{

    /*
     * Interface for creating own source signal
     * 
     *  Name: name of source signal, will 
     *        be displayed in UI.
     *      
     *  GetSample: Returns sample from your custom waveform
     *             given a phase.      
     */
    public abstract class SynthSourcePlugin : IFormattable
    {
        public abstract string Name { get; }
        public abstract Func<float, float> GetSample { get; }
        public string ToString(string format, IFormatProvider formatProvider) => Name;
    }
}
