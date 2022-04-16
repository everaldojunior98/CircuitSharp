using System;
using CircuitSharp.Components.Chips;
using CLanguage;
using static CircuitSharp.Components.Base.Chip.Pin;

namespace CircuitSharp.Machines
{
    public class ATmega328PMachineInfo : MachineInfo
    {
        #region Fields

        private readonly ATmega328P aTmega328P;
        private const int AnalogReadResolution = 10;
        private const int AnalogWriteResolution = 8;
        private const int HighVoltage = 5;
        private const int LowVoltage = 0;
        private const int MinVoltageToBeValid = HighVoltage / 2;

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
                var value = aTmega328P.GetPin(pin).Voltage > MinVoltageToBeValid;
                interpreter.Push(value);
            });

            AddInternalFunction("void digitalWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = interpreter.ReadArg(1).Int16Value;
                aTmega328P.GetPin(pin).Voltage = value > 0 ? HighVoltage : LowVoltage;
            });

            AddInternalFunction("int analogRead (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var maxInputValue = Math.Pow(2, AnalogReadResolution) - 1;
                var value = (int) (aTmega328P.GetPin(pin).Voltage * maxInputValue / HighVoltage);
                interpreter.Push(value);
            });

            AddInternalFunction("void analogWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = Math.Min(Math.Max((int) interpreter.ReadArg(1).Int16Value, LowVoltage),
                    Math.Pow(2, AnalogWriteResolution) - 1);


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