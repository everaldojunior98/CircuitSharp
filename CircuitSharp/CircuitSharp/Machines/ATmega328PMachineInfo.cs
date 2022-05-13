using System;
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
                    void begin (int baud);
                    void end ();
                    int available ();
                    int peek ();
                    int read ();
                    void flush ();
                    int print (const char *value);
                    int print (int value);
                    int print (int value, int format);
                    int print (float value);
                    int print (float value, int format);
                    int println (const char *value);
                    int println (int value);
                    int println (int value, int format);
                    int println (float value);
                    int println (float value, int format);
                };
                struct SerialClass Serial;
                ";

            #region Digital / Analog I/O

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

            #endregion

            #region Time

            AddInternalFunction("void delay (unsigned long ms)", interpreter =>
            {
                var value = (int) interpreter.ReadArg(0).UInt64Value;
                aTmega328P.Delay(value);
            });

            AddInternalFunction("void delayMicroseconds (unsigned long us)", interpreter =>
            {
                var value = (int) interpreter.ReadArg(0).UInt64Value;
                aTmega328P.DelayMicroseconds(value);
            });

            AddInternalFunction("unsigned long micros ()", interpreter => { interpreter.Push(aTmega328P.Micros()); });

            AddInternalFunction("long millis ()", interpreter => { interpreter.Push(aTmega328P.Millis()); });

            #endregion

            #region Math

            #region Abs

            AddInternalFunction("char abs (char x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int8Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("short abs (short x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int8Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("int abs (int x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int32Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("long abs (long x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int32Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("long long abs (long long x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int64Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("long long int abs (long long int x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int64Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("float abs (float x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float32Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("double abs (double x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("long double abs (long double x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                interpreter.Push(Math.Abs(x));
            });

            #endregion

            #region Constrain

            AddInternalFunction("char constrain (char x, char a, char b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int8Value;
                var a = interpreter.ReadArg(1).Int8Value;
                var b = interpreter.ReadArg(2).Int8Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("short constrain (short x, short a, short b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int8Value;
                var a = interpreter.ReadArg(1).Int8Value;
                var b = interpreter.ReadArg(2).Int8Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("int constrain (int x, int a, int b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int32Value;
                var a = interpreter.ReadArg(1).Int32Value;
                var b = interpreter.ReadArg(2).Int32Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("long constrain (long x, long a, long b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int32Value;
                var a = interpreter.ReadArg(1).Int32Value;
                var b = interpreter.ReadArg(2).Int32Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("long long constrain (long long x, long long a, long long b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int64Value;
                var a = interpreter.ReadArg(1).Int64Value;
                var b = interpreter.ReadArg(2).Int64Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("long long int constrain (long long int x, long long int a, long long int b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Int64Value;
                var a = interpreter.ReadArg(1).Int64Value;
                var b = interpreter.ReadArg(2).Int64Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("float constrain (float x, float a, float b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float32Value;
                var a = interpreter.ReadArg(1).Float32Value;
                var b = interpreter.ReadArg(2).Float32Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("double constrain (double x, double a, double b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                var a = interpreter.ReadArg(1).Float64Value;
                var b = interpreter.ReadArg(2).Float64Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("long double constrain (long double x, long double a, long double b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                var a = interpreter.ReadArg(1).Float64Value;
                var b = interpreter.ReadArg(2).Float64Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            #endregion

            #endregion

            #region Serial

            AddInternalFunction("void SerialClass::begin (int baud)", interpreter =>
            {
                var baud = interpreter.ReadArg(0).Int16Value;
                aTmega328P.SerialBegin(baud);
            });

            AddInternalFunction("void SerialClass::end ()", interpreter => { aTmega328P.SerialEnd(); });

            AddInternalFunction("int SerialClass::available ()",
                interpreter => { interpreter.Push(aTmega328P.SerialAvailable()); });

            AddInternalFunction("int SerialClass::peek ()",
                interpreter => { interpreter.Push(aTmega328P.SerialPeek()); });

            AddInternalFunction("int SerialClass::read ()",
                interpreter => { interpreter.Push(aTmega328P.SerialRead()); });

            AddInternalFunction("void SerialClass::flush ()", interpreter => { aTmega328P.SerialFlush(); });

            AddInternalFunction("int SerialClass::print (const char *value)",
                interpreter =>
                {
                    interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadString(interpreter.ReadArg(0).PointerValue),
                        -1));
                });

            AddInternalFunction("int SerialClass::print (int value)",
                interpreter => { interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Int16Value, -1)); });

            AddInternalFunction("int SerialClass::print (int value, int format)",
                interpreter =>
                {
                    interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Int16Value,
                        interpreter.ReadArg(1).Int16Value));
                });

            AddInternalFunction("int SerialClass::print (float value)",
                interpreter => { interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Float32Value, -1)); });

            AddInternalFunction("int SerialClass::print (float value, int format)",
                interpreter =>
                {
                    interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Float32Value,
                        interpreter.ReadArg(1).Int16Value));
                });

            AddInternalFunction("int SerialClass::println (const char *value)",
                interpreter =>
                {
                    interpreter.Push(
                        aTmega328P.SerialPrintln(interpreter.ReadString(interpreter.ReadArg(0).PointerValue), -1));
                });

            AddInternalFunction("int SerialClass::println (int value)",
                interpreter => { interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Int16Value, -1)); });

            AddInternalFunction("int SerialClass::println (int value, int format)",
                interpreter =>
                {
                    interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Int16Value,
                        interpreter.ReadArg(1).Int16Value));
                });

            AddInternalFunction("int SerialClass::println (float value)",
                interpreter =>
                {
                    interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Float32Value, -1));
                });

            AddInternalFunction("int SerialClass::println (float value, int format)",
                interpreter =>
                {
                    interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Float32Value,
                        interpreter.ReadArg(1).Int16Value));
                });

            #endregion
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