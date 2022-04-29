using System;
using System.Threading;
using CircularBuffer;

namespace CircuitSharp.Components.Chips.Utils
{
    public class Serial
    {
        #region Properties

        public Action<byte> OnArduinoSend;

        #endregion

        #region Fields

        private const int MaxBaudRate = 250000;
        private const int MinBaudRate = 2400;

        private readonly int bufferSize;

        private int baudRate;
        private float sendRateBySecond;

        private readonly CircularBuffer<byte> txBuffer;
        private readonly CircularBuffer<byte> rxBuffer;

        private CancellationTokenSource cancellationToken;
        private Thread sendThread;

        private double lastSendTime;
        private double currentTime;

        #endregion

        #region Constructor

        public Serial(int bufferSize)
        {
            this.bufferSize = bufferSize;
            txBuffer = new CircularBuffer<byte>(this.bufferSize);
            rxBuffer = new CircularBuffer<byte>(this.bufferSize);
        }

        #endregion

        #region Public Methods

        public void Update(double time)
        {
            currentTime = time;
        }

        public void Begin(int baud)
        {
            if (sendThread == null)
            {
                baudRate = Math.Min(Math.Max(baud, MinBaudRate), MaxBaudRate);
                sendRateBySecond = 1 / (baudRate / 8f);

                cancellationToken = new CancellationTokenSource();
                sendThread = new Thread(SendArduinoData);
                sendThread.Start();
            }
        }

        public void End()
        {
            while (txBuffer.Size > 0) ;
            rxBuffer.Clear();
            cancellationToken.Cancel();
            sendThread.Join();
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
            while (txBuffer.Size > 0) ;
        }

        public int Write(byte c)
        {
            while (txBuffer.IsFull) ;
            txBuffer.PushFront(c);
            return 1;
        }

        public void WriteToArduino(byte c)
        {
            while (rxBuffer.IsFull) ;
            rxBuffer.PushFront(c);
        }

        #endregion

        #region Private Methods

        private void SendArduinoData()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!txBuffer.IsEmpty)
                {
                    var c = txBuffer[txBuffer.Size - 1];
                    txBuffer.PopBack();
                    OnArduinoSend?.Invoke(c);
                    lastSendTime = currentTime;
                    while (currentTime - lastSendTime < sendRateBySecond) ;
                }
            }
        }

        #endregion
    }
}