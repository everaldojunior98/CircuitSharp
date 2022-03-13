using System;
using CircuitSharp.Components;
using CircuitSharp.Components.Base;
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

            var voltageInput = circuit.Create<VoltageInput>(Voltage.WaveType.Dc);
            voltageInput.SetMaxVoltage(10);
            var resistor = circuit.Create<Resistor>(100);
            var ground = circuit.Create<Ground>();

            circuit.Connect(voltageInput.LeadPos, resistor.LeadIn);
            circuit.Connect(resistor.LeadOut, ground.LeadIn);

            circuit.StartSimulation(() =>
            {
                Console.WriteLine($"{resistor.GetVoltageDelta()} {resistor.GetCurrent()}");
            });

            Console.ReadLine();
        }
    }
}