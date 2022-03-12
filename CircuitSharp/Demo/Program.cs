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
            var sim = new Circuit();

            var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.Dc);
            volt0.SetMaxVoltage(10);
            var res0 = sim.Create<Resistor>(100);
            var ground0 = sim.Create<Ground>();

            sim.Connect(volt0.LeadPos, res0.LeadIn);
            sim.Connect(res0.LeadOut, ground0.LeadIn);

            for (var x = 1; x <= 100; x++)
                sim.DoTick();

            Console.WriteLine($"{res0.GetVoltageDelta()} {res0.GetCurrent()}");
            Console.ReadLine();
        }
    }
}