using System.Linq;
using CircuitSharp.Components.Base;
using CircuitSharp.Core;
using CircuitSharp.Machines;
using CLanguage;
using CLanguage.Interpreter;

namespace CircuitSharp.Components.Chips
{
    public class ATmega328P : Chip
    {
        #region Fields

        private const string InterpreterEntryPoint = "main";
        private readonly CInterpreter interpreter;

        private int sleepTime;

        #endregion

        #region Constructor

        public ATmega328P(string code)
        {
            var machine = new ATmega328PMachineInfo(this);
            var fullCode = code + "\n\nvoid main() { __cinit(); setup(); while(1){loop();}}";
            interpreter = CLanguageService.CreateInterpreter(fullCode, machine);
            interpreter.CpuSpeed = 10 ^ 9;
            interpreter.Reset(InterpreterEntryPoint);

            SetupPins();
        }

        #endregion

        #region Public Methods

        public void Sleep(int value)
        {
            sleepTime = value * 1000;
        }

        #endregion

        #region Overrides

        public override string GetChipName()
        {
            return "ATmega328P";
        }

        public sealed override void SetupPins()
        {
            Pins = new Pin[GetLeadCount()];
            for (var i = 0; i != GetLeadCount(); i++)
                Pins[i] = new Pin(i.ToString());

            interpreter.Run();
            AllocLeads();
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            interpreter.Reset(InterpreterEntryPoint);
            sleepTime = 0;
        }

        protected override void Execute(Circuit circuit)
        {
            var tickInterval = (int) circuit.GetTickTimeInterval();
            if (sleepTime <= 0)
                interpreter.Step(tickInterval);
            else
                sleepTime -= tickInterval;
        }

        public override int GetVoltageSourceCount()
        {
            return Pins.Count(pin => pin.Mode == Pin.PinMode.Output);
        }

        public override int GetLeadCount()
        {
            return 6;
        }

        #endregion
    }
}