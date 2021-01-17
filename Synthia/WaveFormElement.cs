using System;
using System.Windows;
using System.Windows.Media;

namespace Synthia
{

    // This is our custom framework element 
    // that will display waveforms of the output signal
    public class WaveFormElement : FrameworkElement
    {
        // our dependency property, we will use this to change how the element is rendered.
        // our render will be changed each time we change the value of 'Amplitudes'.
        static FrameworkPropertyMetadata meta = new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender);
        public static readonly DependencyProperty amplitudesProperty = DependencyProperty.Register("Amplitudes", typeof(float[]), typeof(WaveFormElement), meta);

        public float[] Amplitudes
        {
            get { return (float[])GetValue(amplitudesProperty); }
            set
            {
                SetValue(amplitudesProperty, value);
            }
        }

        // this function will tell the program how the to render the element
        // when instructed to.
        protected override void OnRender(DrawingContext drawingContext)
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);

            base.OnRender(drawingContext);
            
            // get height and width of element.
            var width = ActualWidth;
            var height = ActualHeight;

            // this will be our mid point
            // this is where the waveform will centre around.
            var midPoint = height * 0.5;

            // rectangle for background, change colour if you want idk
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

            // the pen we will use to draw the wave
            Pen pen = new Pen(Brushes.Black, 1);


            var wave = Amplitudes;

            // if we have a wave to draw
            // draw our wave, else draw just a line
            if (wave != null && wave.Length > 0)
            {
                // we want to space our wave our evenly yah
                var length = wave.Length;
                var step = width / length;
                for (uint i = 0; i < length - 1; ++i)
                {
                    // calculate y value of current sample and next sample
                    var y1 = midPoint * Math.Min(Math.Max(-1, wave[i]), 1) + midPoint;
                    var y2 = midPoint * Math.Min(Math.Max(-1, wave[i + 1]), 1) + midPoint;

                    // draw our line
                    Point p1 = new Point((i) * step + 1, y1 - 2);
                    Point p2 = new Point((i + 1) * step + 1, y2 - 2);
                    drawingContext.DrawLine(pen, p1, p2); ;
                }
            }
            else
            {
                Point p1 = new Point(0, midPoint - 2);
                Point p2 = new Point(width, midPoint - 2);
                drawingContext.DrawLine(pen, p1, p2);
            }
        }
    }
}

