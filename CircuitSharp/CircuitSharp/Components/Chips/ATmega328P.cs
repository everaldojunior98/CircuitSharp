using System;
using System.Linq;
using CircuitSharp.Components.Base;
using CircuitSharp.Core;
using CircuitSharp.Machines;
using CLanguage;
using CLanguage.Interpreter;
using static CircuitSharp.Components.Base.Pin;

namespace CircuitSharp.Components.Chips
{
    public class ATmega328P : CircuitElement
    {
        #region Properties

        public const double MaxVoltage = 5;
        public const double MinVoltage = 0;

        public const double AnalogWriteResolution = 8;
        public const double AnalogReadResolution = 10;

        public const double PwmFrequency = 490;

        public new Lead Lead0 => new Lead(this, 0);
        public new Lead Lead1 => new Lead(this, 1);

        #endregion

        #region Fields

        private readonly double pwmPeriod;

        private const string InterpreterEntryPoint = "main";
        private readonly CInterpreter interpreter;
        
        private Pin[] pins;
        private double[] pinTimeOn;
        private double[] pinTotalTime;

        private double sleepTime;

        private double frequency;
        private double freqTimeZero;

        #endregion

        #region Constructor

        public ATmega328P(string code)
        {
            var machine = new ATmega328PMachineInfo(this);
            var fullCode = code + "\n\nvoid main() { __cinit(); setup(); while(1){loop();}}";
            interpreter = CLanguageService.CreateInterpreter(fullCode, machine);
            interpreter.CpuSpeed = 10 ^ 9;
            interpreter.Reset(InterpreterEntryPoint);

            frequency = PwmFrequency;
            pwmPeriod = 1 / PwmFrequency;
            SetupPins();
        }

        #endregion

        #region Public Methods

        public void Sleep(int value)
        {
            sleepTime = value * 1000;
        }

        public Pin GetPin(short pin)
        {
            return pins[pin];
        }

        #endregion

        #region Private Methods

        private void SetupPins()
        {
            pinTimeOn = new double[GetLeadCount()];
            pinTotalTime = new double[GetLeadCount()];
            pins = new Pin[GetLeadCount()];

            for (var i = 0; i != GetLeadCount(); i++)
                pins[i] = new Pin(i.ToString(), MaxVoltage);

            interpreter.Run();
            AllocLeads();
            Reset();
        }

        private void SetFrequency(double newFreq, double timeStep, double time)
        {
            var oldFreq = frequency;
            frequency = newFreq;
            var maxFreq = 1 / (8 * timeStep);
            if (frequency > maxFreq)
                frequency = maxFreq;
            freqTimeZero = time - oldFreq * (time - freqTimeZero) / frequency;
        }

        private double GetVoltage(Circuit circuit, Pin pin)
        {
            SetFrequency(frequency, circuit.GetTimeStep(), circuit.GetTime());
            var w = 2 * Pi * (circuit.GetTime() - freqTimeZero) * frequency;
            return w % (2 * Pi) > 2 * Pi * pin.DutyCycle ? 0 : MaxVoltage;
        }

        #endregion

        #region Overrides

        public override void SetCurrent(int lead, double current)
        {
            for (var i = 0; i != GetLeadCount(); i++)
                if (pins[i].Mode == PinMode.Output && pins[i].VoltSourceId == lead)
                    pins[i].Current = current;
        }

        public override void SetVoltageSource(int j, int vs)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Output && j-- == 0)
                    pin.VoltSourceId = vs;
            }
        }

        public override void Stamp(Circuit circuit)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Output)
                    circuit.StampVoltageSource(0, LeadNode[i], pin.VoltSourceId);
            }
        }

        public override void Step(Circuit circuit)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Input)
                {
                    pinTotalTime[i] += circuit.GetTimeStep();
                    if (LeadVolt[i] > MaxVoltage / 2)
                        pinTimeOn[i] += circuit.GetTimeStep();

                    if (pinTotalTime[i] >= pwmPeriod)
                    {
                        pin.DutyCycle = Math.Round(pinTimeOn[i] / pinTotalTime[i], 2);

                        pinTotalTime[i] = 0;
                        pinTimeOn[i] = 0;
                    }
                }
            }

            var tickInterval = circuit.GetTickTimeInterval();
            if (sleepTime <= 0)
                interpreter.Step((int) tickInterval);
            else
                sleepTime -= tickInterval;

            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Output)
                    circuit.UpdateVoltageSource(0, i, pin.VoltSourceId, GetVoltage(circuit, pin));
            }
        }

        public override void Reset()
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                pins[i].DutyCycle = 0;
                pins[i].Current = 0;

                pinTotalTime[i] = 0;
                pinTimeOn[i] = 0;

                LeadVolt[i] = 0;
            }

            interpreter.Reset(InterpreterEntryPoint);
            sleepTime = 0;
        }

        public override bool LeadsAreConnected(int lead1, int lead2)
        {
            return false;
        }

        public override bool LeadIsGround(int lead)
        {
            return pins[lead].Mode == PinMode.Output;
        }

        public override int GetVoltageSourceCount()
        {
            return pins.Count(pin => pin.Mode == PinMode.Output);
        }

        public override int GetLeadCount()
        {
            return 6;
        }

        #endregion
    }
}