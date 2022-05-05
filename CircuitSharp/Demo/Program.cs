using System;
using CircuitSharp.Components;
using CircuitSharp.Components.Chips;
using CircuitSharp.Core;

namespace Demo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var circuit = new Circuit(error =>
            {
                Console.WriteLine(error.Code);
            });

            /*var voltageInput = circuit.Create<VoltageInput>(Voltage.WaveType.Dc);
            voltageInput.SetMaxVoltage(10);
            var resistor = circuit.Create<Resistor>(100);
            var ground = circuit.Create<Ground>();

            circuit.Connect(voltageInput.LeadPos, resistor.LeadIn);
            circuit.Connect(resistor.LeadOut, ground.LeadIn);

            circuit.StartSimulation(() =>
            {
                Console.WriteLine($"{circuit.GetTime()}: {resistor.GetVoltageDelta()} {resistor.GetCurrent()}");
            });*/

            var blinkCode = @"
            void setup() 
            {
                pinMode(0, INPUT);
                pinMode(A1, OUTPUT);
                Serial.begin(9600);
                Serial.print(""abc"");
            }
            void loop()
            {
                Serial.print(""a"");
                //Serial.print(1.456);
                analogWrite(A1, 127);
                //digitalWrite(1, HIGH);
                delay(1000);
                analogWrite(A1, 0);
                //digitalWrite(1, LOW);
                delay(1000);
            }
            ";

            var print = new Action<byte>(b =>
            {
                Console.WriteLine(Math.Round(circuit.GetTime() * 1000) + " :: " + ((char) b));
            });

            var aTmega328 = circuit.Create<ATmega328P>(blinkCode, print);
            var resistor = circuit.Create<Resistor>(100);

            circuit.Connect(aTmega328.VCCLead, resistor.LeadIn);
            circuit.Connect(resistor.LeadOut, aTmega328.GNDLead);

            circuit.StartSimulation(() =>
            {
                Console.WriteLine(Math.Round(circuit.GetTime() * 1000) + " :: " + resistor.GetVoltageDelta() + " :: " + aTmega328.GetPinVoltage(ATmega328P.A1));
            });

            Console.ReadLine();
        }
    }
}