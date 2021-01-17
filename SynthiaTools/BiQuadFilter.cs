using NAudio.Wave;
using System;


namespace SynthiaTools
{
    public enum Filter { LowPass, HighPass, BandPass };

    public class BiQuadFilter
    {
        // coefficients
        private double b0;
        private double b1;
        private double b2;
        private double a0;
        private double a1;
        private double a2;

        // use to get 
        // delayed samples
        private float x1;
        private float x2;
        private float y1;
        private float y2;

        // filter properties
        private Filter _filterType;
        private float _cutoff;
        private float _q;

        public float Wet { get; set; }
        public float Cutoff
        {
            get => _cutoff;
            set
            {
                var old = _cutoff;
                _cutoff = Math.Max(10, Math.Min(value, 20000));
                if (Math.Abs(old - _cutoff) > float.Epsilon)
                    UpdateCoefs();
            }
        }

        public float Q
        {
            get => _q;
            set
            {
                _q = value;
                UpdateCoefs();
            }
        }

        public Filter FilterType
        {
            get => _filterType;
            set
            {
                _filterType = value;
                UpdateCoefs();
            }
        }

        public WaveFormat WaveFormat { get; set; }

        public BiQuadFilter(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
            _cutoff = 440;
            _q = (float)(Math.Sqrt(2) / 2);
            _filterType = Filter.LowPass;
            LowPass();
        }

        private void UpdateCoefs()
        {
            switch (_filterType)
            {
                case Filter.LowPass:
                    LowPass();
                    break;
                case Filter.HighPass:
                    HighPass();
                    break;
                case Filter.BandPass:
                    BandPass();
                    break;
                default:
                    LowPass(); 
                    break;
            }
        }

        // H(s) = 1 / (s^2 + s/Q + 1)
        public float Apply(float x0)
        {
             // direct form 1
             // compute result
             var result = 
                 (b0 / a0) * x0 +
                 (b1 / a0) * x1 +
                 (b2 / a0) * x2 -
                 (a1 / a0) * y1 -
                 (a2 / a0) * y2;

             // shift x1 to x2, sample to x1
             // shift y1 to y2, result to y1 
             // simulate delay
             // use previous values next time
             x2 = x1;
             x1 = x0;
             y2 = y1;
             y1 = (float)result;

            return y1;
        }

        // H(s) = 1 / (s^2 + s/Q + 1)
        private void LowPass()
        {
            var omega = 2 * Math.PI * _cutoff / WaveFormat.SampleRate;
            var cosomega = Math.Cos(omega);
            var alpha = Math.Sin(omega) / (2 * _q);

            b0 = (1 - cosomega) / 2;
            b1 = 1 - cosomega;
            b2 = (1 - cosomega) / 2;
            a0 = 1 + alpha;
            a1 = -2 * cosomega;
            a2 = 1 - alpha;
        }

        // H(s) = s^2 / (s^2 + s/Q + 1)
        private void HighPass()
        {
            var omega = 2 * Math.PI * _cutoff / WaveFormat.SampleRate;
            var cosomega = Math.Cos(omega);
            var alpha = Math.Sin(omega) / (2 * _q);

            b0 = (1 + cosomega) / 2;
            b1 = -(1 + cosomega);
            b2 = (1 + cosomega) / 2;
            a0 = 1 + alpha;
            a1 = -2 * cosomega;
            a2 = 1 - alpha;
        }

        // H(s) = (s/Q) / (s^2 + s/Q + 1)
        private void BandPass()
        {
            
            var omega = 2 * Math.PI * _cutoff / WaveFormat.SampleRate;
            var cosomega = Math.Cos(omega);
            var alpha = Math.Sin(omega) / (2 * _q);

            b0 = alpha;
            b1 = 0;
            b2 = -alpha;
            a0 = 1 + alpha;
            a1 = -2 * cosomega;
            a2 = 1 - alpha;
        }
    }
}
