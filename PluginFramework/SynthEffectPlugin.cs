using NAudio.Wave;
using System;

namespace PluginFramework
{
    #region EffectClass
    /*
     * Create your own custom effect plugin that will appear
     * in the UI.
     */
    public abstract class SynthEffectPlugin : ISampleProvider
    {

        /*
         * This will be the title used to display your
         * effect onscreen
         */
        public string Name { get; protected set; }

        /*
         * Just incase your effect needs to do something after
         * the synth has finished applying your effect to a
         * buffer.
         */
        public EventHandler finishedProc;

        /*
         * If this is true, the effect will be applied.
         */
        public bool On { get; set; }

        /*
         * Must be implemented, this ensures your effects
         * properties can be modified, intialized these
         * in your ctor.
         */
        public Property[] Properties { get; set; }

        /*
         * Input source, the effect will be applied
         * to this source.
         */
        public ISampleProvider Input { get; set; }

        /*
         * How Wet the effect is. The higher
         * the wet, the more noticable it is.
         */
        public float Wet { get; set; }

        public WaveFormat WaveFormat { get; set; }

        public SynthEffectPlugin(ISampleProvider input) 
        {
            WaveFormat = input.WaveFormat;
            Input = input;
            Wet = 1;
            On = false;
        }

        /*
         * Read method will go through each sample and apply
         * the effect, this will use the Wet value if above 0.
         */
        public int Read(float[] buffer, int offset, int count)
        {
            var samples = Input?.Read(buffer, offset, count) ?? count;

            if (On)
            {
                if (Wet < 0) return samples;

                for (int i = 0; i < count; ++i)
                {
                    buffer[offset + i] = Apply(buffer[offset + i]) 
                        * Wet + buffer[offset + i] * (1 - Wet);
                }

                finishedProc?.Invoke(this, new EventArgs());
            }

            return samples;
        }

        public abstract float Apply(float sample);
    }
    #endregion

    #region Properties
    public abstract class Property
    {
        public string Name { get; set; }

        /*
         * The on change property acts as an event
         * handler. this will fire when the properties
         * value is changed. 
         * 
         * In practise you use this to alter the variables
         * you might have in your effect.
         * 
         * onChange(x) = x => effect.variable1 = x
         * onChange(x) = x => effect.variable2 = x
         */
        public Action<object> onChange { get; set; }
    }

    /*
     * Use if you want numerical properties of your 
     * effect changing.
     */
    public class SliderProperty : Property
    {
        /*
         * format string used when displaying the
         * properties value on screen.
         * 
         * "string.Format(ValueString, x);"
         */
        public string ValueString { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
    }

    public class DropDownProperty : Property
    {

        /*
         * This will hold all the items you
         * want to store in comboboxes.
         */
        public object[] Items { get; set; }
    }
    #endregion
}
