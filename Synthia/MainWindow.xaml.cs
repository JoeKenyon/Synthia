using NAudio.Midi;
using NAudio.Wave;
using PluginFramework;
using SynthiaTools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace Synthia
{
    /* ___________________________________________________________
     *|  |   | |   |   |   |   | |   | |   |   |   |   | |   |   |
     *|  | S | | D |   |   | G | | H | | J |   |   | L | | ; |   |
     *|  |___| |___|   |   |___| |___| |___|   |   |___| |___|   |
     *|    |     |     |     |     |     |     |     |     |     |
     *| Z  |  X  |  C  |  V  |  B  |  N  |  M  |  ,  |  .  |  /  |
     *|____|_____|_____|_____|_____|_____|_____|_____|_____|_____|
     */
    public partial class MainWindow : Window
    {
        private Dictionary<string, int> _midiDevices;
        private WaveFormProvider _waveFormProvider;
        private Dictionary<Key, int> _keyboard;
        private MidiIn _currentMidiDevice;
        private bool _initialized = false;
        private List<Key> _pressedKeys;
        private Dispatcher _dispatcher;
        private IWavePlayer _player;
        int _midipressed = 0;
        private Synth _synth;
        
        public MainWindow()
        {
            InitializeSynthMaster();

            InitializeComponent();

            initializeUI();

            InitMidiDevices();

            _initialized = true;
        }

        #region Initialization methods

        private void InitializeSynthMaster()
        {
            /* Number of oscliattors 
             * 25, could be changed */
            _synth = new Synth(44100, 25);

            /* load effects from executing dir */
            _synth.IntializeEffects();
            _synth.IntializeSources();

            /* keys used for keyboard input */
            _keyboard = new Dictionary<Key, int>
            {
                { Key.Z, 48},
                { Key.S, 49},
                { Key.X, 50},
                { Key.D, 51},
                { Key.C, 52},
                { Key.V, 53},
                { Key.G, 54},
                { Key.B, 55},
                { Key.H, 56},
                { Key.N, 57}, // a4
                { Key.J, 58},
                { Key.M, 59},
                { Key.OemComma, 60},
                { Key.L, 61},
                { Key.OemPeriod, 62},
                { Key.OemSemicolon, 63},
                { Key.OemQuestion, 64},
            };

            /* To hold our key presses */
            _pressedKeys = new List<Key>();

            /* Get dispatcher of UI thread so
             * that we can update our wave renderer */
            _dispatcher = Dispatcher.CurrentDispatcher;
            _waveFormProvider = new WaveFormProvider(_synth);
            _waveFormProvider.OnRead = (s, e) =>
            {
                WaveFormProvider w = (WaveFormProvider)s;
                Action c = () =>
                {
                    this.waveFormElement.Amplitudes = w.Data;
                };
                _dispatcher.BeginInvoke(c);
            };

            /* 2 buffers seems like a good amount
             * could be changed? */
            _player = new WaveOutEvent()
            {
                NumberOfBuffers = 2,
                DesiredLatency = 100,
            };
            _player.Init(_waveFormProvider);
            _player.Play();
        }

        /*
         * Alot of this is just so we can 
         * automatically generate the UI
         * as im lazy and dont like writing
         * Xaml code.
         */
        private void initializeUI()
        {
            /* combo box of source signals */
            _synth.Sources.ForEach(s =>
            {
                this.source1Combo.Items.Add(s);
                this.source2Combo.Items.Add(s);
                this.source3Combo.Items.Add(s);
            });
            this.source1Combo.SelectedIndex = 0;

            /* filter types */
            foreach (Filter s in Enum.GetValues(typeof(Filter)))
                this.filterTypeCombo.Items.Add(s);
            this.filterTypeCombo.SelectedIndex = 0;

            /* no idea why i did this ------------V*/
            foreach (int s in new int[] { -4, -3, -2, -1, 0, 1, 2, 3, 4 })
            {
                /* Add values for octaves and semitones*/
                this.octavesCombo1.Items.Add(s);
                this.octavesCombo2.Items.Add(s);
                this.octavesCombo3.Items.Add(s);
                this.semitonesCombo1.Items.Add(s);
                this.semitonesCombo2.Items.Add(s);
                this.semitonesCombo3.Items.Add(s);
            }

            this.octavesCombo1.SelectedIndex = 4;
            this.octavesCombo2.SelectedIndex = 5;
            this.octavesCombo3.SelectedIndex = 6;
            this.semitonesCombo1.SelectedIndex = 4;
            this.semitonesCombo2.SelectedIndex = 4;
            this.semitonesCombo3.SelectedIndex = 4;


            /* Here we generate the UI for our effects */
            var tabControl = this.effectsControl;
            _synth.Effects.ForEach(e =>
            {
                Property[] prop = e.Properties;

                /* Tab item for every effect */
                TabItem tab = new TabItem
                {
                    Header = e.Name,
                    Name = e.Name + "TabItem"
                };

                /* default width height */
                int widgetHeight = 30;

                /* Scroller used if too many properties */
                ScrollViewer scroller = new ScrollViewer();

                /* everything is organised using a wrap panel */
                WrapPanel w = new WrapPanel();
                w.Width = tabControl.Width;
                w.Height = widgetHeight * (prop.Length + 1);

                CheckBox check = new CheckBox();
                check.Content = "ON";
                check.Height = widgetHeight;
                check.Width = tabControl.Width;
                check.Checked += (o, i) =>
                {
                    e.On = true;
                    foreach (UIElement ele in w.Children)
                    {
                        ele.IsEnabled = true;
                        check.IsEnabled = true;
                    }
                };
                check.Unchecked += (o, i) =>
                {
                    e.On = false;
                    foreach (UIElement ele in w.Children)
                    {
                        ele.IsEnabled = false;
                        check.IsEnabled = true;
                    }
                };
                w.Children.Add(check);

                foreach (Property p in prop)
                {
                    Label propName = new Label();
                    propName.Content = p.Name;
                    propName.Width = 100;
                    propName.Height = widgetHeight;

                    if (p.GetType() == typeof(SliderProperty))
                    {
                        SliderProperty sp = (SliderProperty)p;
                        Slider s = new Slider();
                        s.Minimum = sp.Min;
                        s.Maximum = sp.Max;
                        s.Width = tabControl.Width - 200;
                        s.Height = 20;
                        s.IsEnabled = false;
                        Label sliderValue = new Label();
                        sliderValue.Content = string.Format(sp.ValueString, s.Value);
                        sliderValue.Width = 100;
                        sliderValue.Height = widgetHeight;
                        s.ValueChanged += (o, i) =>
                        {
                            sp.onChange(s.Value);
                            sliderValue.Content = string.Format(sp.ValueString, s.Value);
                        };
                        w.Children.Add(propName);
                        w.Children.Add(s);
                        w.Children.Add(sliderValue);
                    }

                    if (p.GetType() == typeof(DropDownProperty))
                    {
                        DropDownProperty ddp = (DropDownProperty)p;
                        ComboBox combo = new ComboBox();
                        foreach (string item in ddp.Items)
                            combo.Items.Add(item);
                        combo.Width = tabControl.Width - 150;
                        combo.SelectionChanged += (o, i) => ddp.onChange(combo.SelectedItem);
                        combo.SelectedIndex = 0;
                        combo.Height = widgetHeight;
                        combo.IsEnabled = false;
                        w.Children.Add(propName);
                        w.Children.Add(combo);
                    }
                }
                w.VerticalAlignment = VerticalAlignment.Top;
                scroller.Content = w;
                tab.Content = scroller;
                tabControl.Items.Add(tab);
                tabControl.SelectedIndex = 0;
            });
        }
        #endregion

        #region MIDI
        /* 
         * Initialize out midi devices, if
         * we find any, we add them to the combo 
         * box. Use a dictionary to store device
         * numbers and the combobox string.
         */
        private void InitMidiDevices()
        {
            midiDevicesCombo.Items.Clear();

            _midiDevices = new Dictionary<string, int>();

            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                MidiInCapabilities info = MidiIn.DeviceInfo(i);
                string k = string.Format("{0}: {1} : {2}", i, info.ProductName, info.ProductId);
                _midiDevices.Add(k, i);
                midiDevicesCombo.Items.Add(k);
            }

            if (midiDevicesCombo.Items.Count > 0)
                midiDevicesCombo.SelectedIndex = 0;
        }

        /* Event handler, will fire every time we get a midi message */
        private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            /* split the data into the parts we need */
            int statusByte = e.RawMessage & 0xF0;
            int dataByte1 = (e.RawMessage >> 8) & 0xFF;
            int dataByte2 = (e.RawMessage >> 16) & 0xFF;

            /* 
             * We are only really looking for on and off event
             * Could add more in future but for now its not needed.
             *      note off = 0x80
             *      note on  = 0x90
             */
            switch (statusByte)
            {
                case 0x80:
                    _midipressed--;
                    _synth.NoteOff(dataByte1);
                    break;

                case 0x90:
                    if (_midipressed < 4)
                    {
                        _synth.NoteOn(dataByte1);
                        _midipressed++;
                    }
                    break;
            }
        }
        
        private void midiDevicesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /* If using a midi device already, dispose of it */
            if (_currentMidiDevice != default(MidiIn))
            {
                _currentMidiDevice.Stop();
                _currentMidiDevice.Dispose();
            }

            /* start the midi device, attach event handler */
            _currentMidiDevice = new MidiIn(_midiDevices[midiDevicesCombo.SelectedItem.ToString()]);
            _currentMidiDevice.MessageReceived += midiIn_MessageReceived;
            _currentMidiDevice.Start();
        }
        #endregion

        #region Main event handlers
        /* Dispose of our midi device and output audio device */
        private void onWindowClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_currentMidiDevice != null)
                _currentMidiDevice.Dispose();

            if (_currentMidiDevice != null)
            {
                _player.Stop();
                _player.Dispose();
            }
        }

        /*
         * Check if key is in our valid keyboard keys.
         * Check if we are not already pressing it.
         * Turn note on.
         */
        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (_initialized)
            {
                if (!_keyboard.ContainsKey(e.Key))
                    return;

                if (_pressedKeys.Find(k => k == e.Key) == default && _pressedKeys.Count < 5)
                {
                    _pressedKeys.Add(e.Key);
                    _synth.NoteOn(_keyboard[e.Key]);
                }
            }
            e.Handled = true;
        }

        /*
         * Check if key is in our valid keyboard keys.
         * Remove it from the pressed keys list
         * Turn note off.
         */
        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (!_keyboard.ContainsKey(e.Key))
                return;

            _pressedKeys.Remove(e.Key);
            _synth.NoteOff(_keyboard[e.Key]);
            e.Handled = true;

        }
        #endregion

        #region MainEventHandlers
        private void masterVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.MasterAmplitude = (float)(e.NewValue);
        }

        private void oscType_KeyDown(object sender, KeyEventArgs e)
        {
            /* 
             * This is used to stop the keyboard from messing with
             * osc type combo boxes.
             */
        }

        #endregion

        #region Source control
        private void source1Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.WaveFormOsc1 = ((SynthSourcePlugin)source1Combo.SelectedItem);
        }

        private void source2Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.WaveFormOsc2 = ((SynthSourcePlugin)source2Combo.SelectedItem);
        }

        private void source3Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.WaveFormOsc3 = ((SynthSourcePlugin)source3Combo.SelectedItem);
        }

        private void osc1VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.AmplitudeOsc1 = (float)(e.NewValue);
        }

        private void osc2VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.AmplitudeOsc2 = (float)(e.NewValue);
        }

        private void osc3VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.AmplitudeOsc3 = (float)(e.NewValue);
        }

        private void lfoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _synth.UseLFO = true;
            lfoFreqSlider.IsEnabled = true;
            lfoAmpSlider.IsEnabled = true;
            _synth.LFOFrequency = (float)lfoFreqSlider.Value;
            _synth.LFOAmplitude = (float)lfoAmpSlider.Value;
        }

        private void lfoCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _synth.UseLFO = false;
            lfoFreqSlider.IsEnabled = false;
            lfoAmpSlider.IsEnabled = false;
        }

        private void lfoFreqSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.LFOFrequency = (float)(e.NewValue);
            if (_initialized)
                lfoFreqLabel.Content = string.Format("{0:0.00}Hz", e.NewValue);
        }

        private void lfoAmpSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.LFOAmplitude = (float)(e.NewValue);
            if (_initialized)
                lfoAmpLabel.Content = string.Format("{0:0.00}", e.NewValue);
        }
        #endregion

        #region Amplitude evelope handlers
        private void amplitudeAttack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.Attack = (float)(e.NewValue);
            if (_initialized)
                ampAttackLabel.Content = string.Format("{0:0.0}s", e.NewValue);
        }

        private void amplitudeDecay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.Decay = (float)(e.NewValue);
            if (_initialized)
                ampDecayLabel.Content = string.Format("{0:0.0}s", e.NewValue);
        }

        private void amplitudeSustain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.Sustain = (float)(e.NewValue);
            if (_initialized)
                ampSustainLabel.Content = string.Format("{0:0.0}", e.NewValue);
        }

        private void amplitudeRelease_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.Release = (float)(e.NewValue);
            if (_initialized)
                ampReleaseLabel.Content = string.Format("{0:0.0}s", e.NewValue);
        }
        #endregion

        #region Filter envelope event handlers
        private void filterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _synth.UseFilterEnvelope = true;
        }

        private void filterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _synth.UseFilterEnvelope = false;
        }

        private void filterTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.FilterType = (Filter)filterTypeCombo.SelectedItem;
        }

        private float AmplitudeTodB(float amplitude)
        {
            return (float)(20.0f * Math.Log10(amplitude));
        }

        private void qValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterQ = (float)(e.NewValue / 1000f);
            if (_initialized)
                qValueLabel.Content = string.Format("{0:0.0}dB", AmplitudeTodB((float)(e.NewValue / 1000f)));
        }
        private void cutOffSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterCutoff = (float)(e.NewValue);
            if (_initialized)
                cutoffLabel.Content = String.Format("{0:0.0}Hz", e.NewValue);
        }

        private void envOctavesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterEnvelopeOctaves = (int)(e.NewValue / 100d);
            if (_initialized)
                envOctavesLabel.Content = ((int)(e.NewValue / 100d)).ToString();
        }

        private void filterWetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterWet = (float)(e.NewValue);
            if (_initialized)
                filterWetLabel.Content = String.Format("{0:0.0}", e.NewValue);
        }
        private void filterAttack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterAttack = (float)(e.NewValue);
            if (_initialized)
                filterAttackLabel.Content = string.Format("{0:0.0}s", e.NewValue);
        }

        private void filterDecay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterDecay = (float)(e.NewValue);
            if (_initialized)
                filterDecayLabel.Content = string.Format("{0:0.0}s", e.NewValue);
        }

        private void filterSustain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterSustain = (float)(e.NewValue);
            if (_initialized)
                filterSustainLabel.Content = string.Format("{0:0.0}", e.NewValue);
        }

        private void filterRelease_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _synth.FilterRelease = (float)(e.NewValue);
            if (_initialized)
                filterReleaseLabel.Content = string.Format("{0:0.0}s", e.NewValue);
        }
        #endregion

        #region Octaves and Semitones
        private void octavesCombo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.OctaveOsc1 = (int)((ComboBox)sender).SelectedItem;
        }

        private void octavesCombo2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.OctaveOsc2 = (int)((ComboBox)sender).SelectedItem;
        }

        private void octavesCombo3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.OctaveOsc3 = (int)((ComboBox)sender).SelectedItem;
        }

        private void semitonesCombo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.SemitonesOsc1 = (int)((ComboBox)sender).SelectedItem;
        }

        private void semitonesCombo2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.SemitonesOsc2 = (int)((ComboBox)sender).SelectedItem;
        }

        private void semitonesCombo3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _synth.SemitonesOsc3 = (int)((ComboBox)sender).SelectedItem;
        }
        #endregion
    }
}

