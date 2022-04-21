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
        #region Constants

        public const short A0 = 14;
        public const short A1 = 15;
        public const short A2 = 16;
        public const short A3 = 17;
        public const short A4 = 18;
        public const short A5 = 19;

        private const double MaxVoltage = 5;
        private const double MinVoltage = 0;

        private const double AnalogWriteResolution = 8;
        private const double AnalogReadResolution = 10;

        private const double PwmFrequency = 490;

        #endregion

        #region Properties

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

        public double GetPinVoltage(short pin)
        {
            if (IsValidPin(pin))
                return pins[pin].GetVoltage();
            return 0;
        }

        public void Sleep(int value)
        {
            sleepTime = value * 1000;
        }

        public void SetPinMode(short pin, int mode)
        {
            if (mode >= 0 && mode <= 2 && IsValidPin(pin))
                pins[pin].Mode = (PinMode) mode;
        }

        public bool ReadDigitalPin(short pin)
        {
            if (IsValidPin(pin))
                return pins[pin].GetVoltage() >= MaxVoltage / 2;

            return false;
        }

        public void WriteDigitalPin(short pin, short value)
        {
            if (IsValidPin(pin))
                pins[pin].DutyCycle = value > 0 ? 1 : 0;
        }

        public int ReadAnalogPin(short pin)
        {
            if (IsValidPin(pin) && pins[pin].GetType() == PinType.Analog)
            {
                var maxInputValue = Math.Pow(2, AnalogReadResolution) - 1;
                var value = (int) (pins[pin].GetVoltage() * maxInputValue / MaxVoltage);
                return value;
            }

            return 0;
        }

        public void WriteAnalogPin(short pin, short value)
        {
            if (IsValidPin(pin))
            {
                var analogValue = Math.Min(Math.Max(value, MinVoltage),
                    Math.Pow(2, AnalogWriteResolution) - 1);

                var dutyCycle = Math.Round(analogValue / (Math.Pow(2, AnalogWriteResolution) - 1), 2);
                if (pins[pin].GetType() == PinType.Digital)
                    dutyCycle = dutyCycle >= 0.5 ? 1 : 0;

                pins[pin].DutyCycle = dutyCycle;
            }
        }

        #endregion

        #region Private Methods

        private bool IsValidPin(short pin)
        {
            return pin >= 0 && pin < pins.Length && !pins[pin].IsControlPin;
        }

        private void SetupPins()
        {
            pinTimeOn = new double[GetLeadCount()];
            pinTotalTime = new double[GetLeadCount()];
            pins = new Pin[GetLeadCount()];
            
            //Digital Pins
            //0 - RXD
            pins[0] = new Pin("PD0", MaxVoltage, PinType.Digital);
            //1 - TXD
            pins[1] = new Pin("PD1", MaxVoltage, PinType.Digital);
            //2
            pins[2] = new Pin("PD2", MaxVoltage, PinType.Digital);
            //3
            pins[3] = new Pin("PD3", MaxVoltage, PinType.DigitalPwm);
            //4
            pins[4] = new Pin("PD4", MaxVoltage, PinType.DigitalPwm);
            //5
            pins[5] = new Pin("PD5", MaxVoltage, PinType.DigitalPwm);
            //6
            pins[6] = new Pin("PD6", MaxVoltage, PinType.DigitalPwm);
            //7
            pins[7] = new Pin("PD7", MaxVoltage, PinType.Digital);
            //8
            pins[8] = new Pin("PB0", MaxVoltage, PinType.Digital);
            //9
            pins[9] = new Pin("PB1", MaxVoltage, PinType.DigitalPwm);
            //10
            pins[10] = new Pin("PB2", MaxVoltage, PinType.DigitalPwm);
            //11
            pins[11] = new Pin("PB3", MaxVoltage, PinType.DigitalPwm);
            //12
            pins[12] = new Pin("PB4", MaxVoltage, PinType.Digital);
            //13
            pins[13] = new Pin("PB5", MaxVoltage, PinType.Digital);

            //Analog Pins
            //A0
            pins[14] = new Pin("PC0", MaxVoltage, PinType.Analog);
            //A1
            pins[15] = new Pin("PC1", MaxVoltage, PinType.Analog);
            //A2
            pins[16] = new Pin("PC2", MaxVoltage, PinType.Analog);
            //A3
            pins[17] = new Pin("PC3", MaxVoltage, PinType.Analog);
            //A4
            pins[18] = new Pin("PC4", MaxVoltage, PinType.Analog);
            //A5
            pins[19] = new Pin("PC5", MaxVoltage, PinType.Analog);

            //Control Pins
            //Reset
            pins[20] = new Pin("PC6", MaxVoltage, PinType.Analog)
            {
                Mode = PinMode.Input,
                IsControlPin = true
            };
            //VCC
            pins[21] = new Pin("VCC", MaxVoltage, PinType.Digital)
            {
                Mode = PinMode.Output,
                DutyCycle = 1,
                IsControlPin = true
            };
            //GND
            pins[22] = new Pin("GND", MaxVoltage, PinType.Digital)
            {
                Mode = PinMode.Output,
                IsControlPin = true
            };

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
            return 23;
        }

        #endregion
    }
}