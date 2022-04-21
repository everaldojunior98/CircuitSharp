namespace CircuitSharp.Components.Base
{
    public class Pin
    {
        #region Properties

        public enum PinType
        {
            Analog,
            Digital,
            DigitalPwm
        }

        public enum PinMode
        {
            Input,
            Output
        }

        public PinMode Mode;

        public double DutyCycle;
        public double Current;

        public int VoltSourceId;

        public bool IsControlPin;

        #endregion

        #region Fields

        private readonly string name;
        private readonly double maxVoltage;
        private readonly PinType type;

        #endregion

        #region Constructor

        public Pin(string name, double maxVoltage, PinType type)
        {
            this.name = name;
            this.maxVoltage = maxVoltage;
            this.type = type;
        }

        #endregion

        #region Public Methods

        public string GetName()
        {
            return name;
        }

        public new PinType GetType()
        {
            return type;
        }

        public double GetVoltage()
        {
            return DutyCycle * maxVoltage;
        }

        #endregion
    }
}