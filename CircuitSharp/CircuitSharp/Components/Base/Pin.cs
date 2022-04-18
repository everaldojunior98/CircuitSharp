namespace CircuitSharp.Components.Base
{
    public class Pin
    {
        #region Properties

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

        public PinMode Mode;
        public PinType Type;

        public double DutyCycle;
        public double Current;

        public int VoltSourceId;

        #endregion

        #region Fields

        private readonly string name;
        private readonly double maxVoltage;

        #endregion

        #region Constructor

        public Pin(string name, double maxVoltage)
        {
            this.name = name;
            this.maxVoltage = maxVoltage;
        }

        #endregion

        #region Public Methods

        public string GetName()
        {
            return name;
        }

        public double GetVoltage()
        {
            return DutyCycle * maxVoltage;
        }

        #endregion
    }
}