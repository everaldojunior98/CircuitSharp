using System;
using CircuitSharp.Components.Chips;
using CLanguage;
using static CircuitSharp.Components.Base.Pin;

namespace CircuitSharp.Machines
{
    public class ATmega328PMachineInfo : MachineInfo
    {
        #region Fields

        private readonly ATmega328P aTmega328P;

        #endregion

        #region Constructor

        public ATmega328PMachineInfo(ATmega328P chip)
        {
            aTmega328P = chip;
            CharSize = 1;
            ShortIntSize = 2;
            IntSize = 2;
            LongIntSize = 4;
            LongLongIntSize = 8;
            FloatSize = 4;
            DoubleSize = 8;
            LongDoubleSize = 8;
            PointerSize = 2;
            HeaderCode = @"
                #define HIGH 1
                #define LOW 0
                #define INPUT 0
                #define INPUT_PULLUP 2
                #define OUTPUT 1
                #define A0 0
                #define A1 1
                #define A2 2
                #define A3 3
                #define A4 4
                #define A5 5
                #define DEC 10
                #define HEX 16
                #define OCT 8
                #define BIN 2
                #define bitRead(x, n) ((x & (1 << n)) != 0)
                typedef bool boolean;
                typedef unsigned char byte;
                typedef unsigned short word;
                struct SerialClass
                {
                    void begin(int baud);
                    void print(const char *value);
                    void println(int value, int bas);
                    void println(int value);
                    void println(const char *value);
                };
                struct SerialClass Serial;
                ";

            AddInternalFunction("void pinMode (int pin, int mode)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var mode = interpreter.ReadArg(1).Int16Value;
                aTmega328P.GetPin(pin).Mode = (PinMode) mode;
            });

            AddInternalFunction("int digitalRead (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = aTmega328P.GetPin(pin).GetVoltage() > ATmega328P.MaxVoltage / 2;
                interpreter.Push(value);
            });

            AddInternalFunction("void digitalWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = interpreter.ReadArg(1).Int16Value;
                aTmega328P.GetPin(pin).DutyCycle = value > 0 ? 1 : 0;
            });

            AddInternalFunction("int analogRead (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var maxInputValue = Math.Pow(2, ATmega328P.AnalogReadResolution) - 1;
                var value = (int) (aTmega328P.GetPin(pin).GetVoltage() * maxInputValue / ATmega328P.MaxVoltage);
                interpreter.Push(value);
            });

            AddInternalFunction("void analogWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = Math.Min(Math.Max((int) interpreter.ReadArg(1).Int16Value, ATmega328P.MinVoltage),
                    Math.Pow(2, ATmega328P.AnalogWriteResolution) - 1);
                var dutyCycle = Math.Round(value / (Math.Pow(2, ATmega328P.AnalogWriteResolution) - 1), 2);
                aTmega328P.GetPin(pin).DutyCycle = dutyCycle;
            });

            AddInternalFunction("void delay (unsigned long ms)", interpreter =>
            {
                var value = (int) interpreter.ReadArg(0).UInt64Value;
                aTmega328P.Sleep(value);
            });
        }

        #endregion

        #region Private Methods

        private double Map(double x, double inMin, double inMax, double outMin, double outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        #endregion
    }
}