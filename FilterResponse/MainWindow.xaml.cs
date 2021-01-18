using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using OxyPlot;
using OxyPlot.Series;
using SynthiaTools;

namespace FilterResponse
{
    public partial class MainWindow : Window
    {
        private LineSeries _lineChart;
        private IList<DataPoint> _values;
        private BiQuadFilter _filter;
        private WaveFormat _waveFormat;
        private Dictionary<float,float[]> _waveTables;

        public MainWindow()
        {
            InitializeComponent();
            _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
            _values = new List<DataPoint>();
            _lineChart = new LineSeries();
            _filter = new BiQuadFilter(_waveFormat);
            _filter.FilterType = Filter.HighPass;
            PlotModel plt = new PlotModel { Title = "Example 1" };
            _lineChart.ItemsSource = _values;
            plt.Series.Add(_lineChart);
            plot.Model = plt;
            _waveTables = generateWaveTables();
            getFreqResponse();
        }

        private Dictionary<float, float[]> generateWaveTables()
        {
            Dictionary<float, float[]> ret = new Dictionary<float, float[]>();
            for (float freq = 0; freq < 5000f; freq++)
            {
                // gen sinewave
                float[] sinewave = new float[120];
                float phase = 0.0f;
                for (int i = 0; i < sinewave.Length; i++)
                {
                    phase = (float)(phase + (freq) / _waveFormat.SampleRate) % 1.0f;
                    sinewave[i] = (float)Math.Sin(2.0f * Math.PI * phase);
                }
                ret.Add(freq, sinewave);
            }
            return ret;
        }

        private void qValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_filter != null)
            {
                _filter.Q = ((float)e.NewValue);
                getFreqResponse();
            }
        }

        private void cutOffSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(_filter != null)
            {
                _filter.Cutoff = ((float)e.NewValue);
                getFreqResponse();
            }

        }

        private void getFreqResponse()
        {
            // frequncies
            _values.Clear();

            foreach (var waveTable in _waveTables)
            {
                float largestValue = 0.0f;

                for (int i = 0; i < waveTable.Value.Length; i++)
                {
                    var val = _filter.Apply(waveTable.Value[i]);
                    if (val > largestValue) largestValue = val;
                }
                _values.Add(new DataPoint(waveTable.Key, largestValue));
            }
            plot.Model.InvalidatePlot(true);
        }

    }
}
