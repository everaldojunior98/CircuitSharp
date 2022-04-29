using CircuitSharp.Components.Chips;
using CLanguage;

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
                #define A0 14
                #define A1 15
                #define A2 16
                #define A3 17
                #define A4 18
                #define A5 19
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
                aTmega328P.SetPinMode(pin, mode);
            });

            AddInternalFunction("int digitalRead (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = aTmega328P.ReadDigitalPin(pin);
                interpreter.Push(value);
            });

            AddInternalFunction("void digitalWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = interpreter.ReadArg(1).Int16Value;
                aTmega328P.WriteDigitalPin(pin, value);
            });

            AddInternalFunction("int analogRead (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = aTmega328P.ReadAnalogPin(pin);
                interpreter.Push(value);
            });

            AddInternalFunction("void analogWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = interpreter.ReadArg(1).Int16Value;
                aTmega328P.WriteAnalogPin(pin, value);
            });

            AddInternalFunction("void delay (unsigned long ms)", interpreter =>
            {
                var value = (int) interpreter.ReadArg(0).UInt64Value;
                aTmega328P.Sleep(value);
            });

            AddInternalFunction("long millis ()", interpreter =>
            {
                interpreter.Push(aTmega328P.Millis());
            });

            AddInternalFunction("void SerialClass::begin (int baud)", interpreter =>
            {
                var baud = interpreter.ReadArg(0).Int16Value;
                aTmega328P.SerialBegin(baud);
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