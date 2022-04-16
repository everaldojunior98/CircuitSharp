using CircuitSharp.Core;
using static CircuitSharp.Components.Base.Chip.Pin;

namespace CircuitSharp.Components.Base
{
    public abstract class Chip : CircuitElement
    {
        #region Fields

        protected Pin[] Pins;

        #endregion

        #region Public Methods

        public virtual void SetupPins()
        {
        }

        protected virtual void Execute(Circuit circuit)
        {
        }

        public Pin GetPin(short pin)
        {
            return Pins[pin];
        }

        #endregion

        #region Overrides

        public override void SetVoltageSource(int j, int vs)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = Pins[i];
                if (pin.Mode == PinMode.Output && j-- == 0)
                    pin.VoltSourceId = vs;
            }
        }

        public override void Stamp(Circuit circuit)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = Pins[i];
                if (pin.Mode == PinMode.Output)
                    circuit.StampVoltageSource(0, LeadNode[i], pin.VoltSourceId);
            }
        }

        public override void Step(Circuit circuit)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = Pins[i];
                if (pin.Mode == PinMode.Input)
                    pin.Voltage = LeadVolt[i];
            }

            Execute(circuit);

            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = Pins[i];

                if (pin.Mode == PinMode.Output)
                    circuit.UpdateVoltageSource(0, i, pin.VoltSourceId, pin.Voltage);
            }
        }

        public override void Reset()
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                Pins[i].Voltage = 0;
                Pins[i].Current = 0;
                LeadVolt[i] = 0;
            }
        }

        public override void SetCurrent(int lead, double current)
        {
            for (var i = 0; i != GetLeadCount(); i++)
                if (Pins[i].Mode == PinMode.Output && Pins[i].VoltSourceId == lead)
                    Pins[i].Current = current;
        }

        public virtual string GetChipName()
        {
            return "Chip";
        }

        public override bool LeadsAreConnected(int lead1, int lead2)
        {
            return false;
        }

        public override bool LeadIsGround(int lead)
        {
            return Pins[lead].Mode == PinMode.Output;
        }

        #endregion

        public class Pin
        {
            public enum PinType
            {
                Analog,
                Digital
            }

            public enum PinMode
            {
                Input,
                Output
            }

            public string Name;
            public PinMode Mode;
            public PinType Type;

            public double Voltage;
            public double Current;

            public int VoltSourceId;

            public Pin(string name)
            {
                Name = name;
            }
        }
    }
}