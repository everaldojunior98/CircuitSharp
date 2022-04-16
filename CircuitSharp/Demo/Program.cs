using System;
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
                pinMode(1, OUTPUT);
            }
            void loop()
            {
                digitalWrite(1, HIGH);
                delay(1000);
                digitalWrite(1, LOW);
                delay(1000);
            }
            ";
            var aTmega328 = circuit.Create<ATmega328P>(blinkCode);
            circuit.StartSimulation(() =>
            {
                Console.WriteLine(circuit.GetTime() + " :: " + aTmega328.GetPin(1).Voltage);
            });

            Console.ReadLine();
        }
    }
}