using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthiaTools
{
    public class Envelope
    {
        private float _output;
        private float _coefficient;
        private EnvState _currentState;
        private int _samplesProcessed;
        private int _samplesUntilNextStage;
        private const float _minimumOutput = 0.0001f;
        private Dictionary<EnvState, float> _stateValues;

        public EnvState State
        {
            get => _currentState;
            set
            {
                _currentState = value;
                _samplesProcessed = 0;

                if (_currentState == EnvState.Idle || _currentState == EnvState.Sustain)
                    _samplesUntilNextStage = 0;
                else
                    _samplesUntilNextStage = (int)(_stateValues[_currentState] * SampleRate);

                switch (_currentState)
                {
                    case EnvState.Idle:
                        _output = 0.0f;
                        _coefficient = 0.0f;
                        break;

                    case EnvState.Attack:
                        _output = 0.0f;
                        calculateCoefficient(_output, 1.0f, _samplesUntilNextStage);
                        break;

                    case EnvState.Decay:
                        _output = 1.0f;
                        calculateCoefficient(_output, SustainLevel, _samplesUntilNextStage);
                        break;

                    case EnvState.Sustain:
                        _output = SustainLevel;
                        _coefficient = 0.0f;
                        break;

                    case EnvState.Release:
                        calculateCoefficient(_output, _minimumOutput, _samplesUntilNextStage);
                        break;

                    default:
                        break;
                }
            }
        }

        public float SampleRate
        {
            get; set;
        }

        public float IdleLevel
        {
            get => _stateValues[EnvState.Idle];
            set => _stateValues[EnvState.Idle] = Math.Max(value, _minimumOutput);
        }

        public float AttackRate
        {
            get => _stateValues[EnvState.Attack];
            set => _stateValues[EnvState.Attack] = Math.Max(value, _minimumOutput);
        }

        public float DecayRate
        {
            get => _stateValues[EnvState.Decay];
            set => _stateValues[EnvState.Decay] = Math.Max(value, _minimumOutput);
        }

        public float SustainLevel
        {
            get => _stateValues[EnvState.Sustain];
            set => _stateValues[EnvState.Sustain] = Math.Max(value, _minimumOutput);
        }

        public float ReleaseRate
        {
            get => _stateValues[EnvState.Release];
            set => _stateValues[EnvState.Release] = Math.Max(value, _minimumOutput);
        }

        public enum EnvState : int
        {
            Idle = 0,
            Attack = 1,
            Decay = 2,
            Sustain = 3,
            Release = 4,
        }

        public Envelope(int sampleRate)
        {
            SampleRate = sampleRate;

            _stateValues = new Dictionary<EnvState, float>
            {
                { EnvState.Idle,    0.00f},
                { EnvState.Attack,  0.01f},
                { EnvState.Decay,   0.01f},
                { EnvState.Sustain, 1.00f},
                { EnvState.Release, 0.01f},
            };

            State = EnvState.Idle;
            _output = _minimumOutput;
            _coefficient = 1.0f;
            _samplesProcessed = 0;
            _samplesUntilNextStage = 0;
        }

        private void calculateCoefficient(float start, float end, int length)
        {
            _coefficient = (end - start) / length;
        }

        public float Process()
        {
            if (State != EnvState.Idle &&
                State != EnvState.Sustain)
            {
                if (_samplesProcessed == _samplesUntilNextStage)
                    State = _stateValues.Keys.ElementAt(((int)State + 1) % 5);

                _output = _output + _coefficient;
                _samplesProcessed++;
            }
            return _output;
        }
    }
}
