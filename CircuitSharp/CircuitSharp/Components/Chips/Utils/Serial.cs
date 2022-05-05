using System;
using CircularBuffer;

namespace CircuitSharp.Components.Chips.Utils
{
    public class Serial
    {
        #region Properties

        public Action<byte> OnArduinoSend;

        #endregion

        #region Fields

        private readonly int bufferSize;
        private readonly CircularBuffer<byte> rxBuffer;

        #endregion

        #region Constructor

        public Serial(int bufferSize)
        {
            this.bufferSize = bufferSize;
            rxBuffer = new CircularBuffer<byte>(this.bufferSize);
        }

        #endregion

        #region Public Methods

        public void Begin(int baud)
        {
        }

        public void End()
        {
        }

        public int Available()
        {
            return (bufferSize + rxBuffer.Size) % bufferSize;
        }

        public int Peek()
        {
            if (rxBuffer.Size == 0)
                return -1;
            return rxBuffer[rxBuffer.Size - 1];
        }

        public int Read()
        {
            if (rxBuffer.Size == 0)
                return -1;

            var c = rxBuffer[rxBuffer.Size - 1];
            rxBuffer.PopBack();
            return c;
        }

        public void Flush()
        {
        }

        public int Write(byte c)
        {
            OnArduinoSend?.Invoke(c);
            return 1;
        }

        public void WriteToArduino(byte c)
        {
            rxBuffer.PushFront(c);
        }

        #endregion
    }
}