using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class LimitationsManager
    {
        private int _concurrency;
        private int _quantity;
        private int _frequency;
        private bool _isHomeDevice;

        public LimitationsManager(int concurrency, int quantity, int frequency, bool isHomeDevice)
        {
            Concurrency = concurrency;
            Quantity = quantity;
            Frequency = frequency;
            IsHomeDevice = isHomeDevice;
        }

        public LimitationsManager(int concurrency, int quantity, int frequency)
        {
            Concurrency = concurrency;
            Quantity = quantity;
            Frequency = frequency;
            IsHomeDevice = false;
        }

        public LimitationsManager()
        {
            Concurrency = 5; 
            Quantity = 5;
            Frequency = 1;
            IsHomeDevice = false;
        }

        public int Concurrency
        {
            get
            {
                return _concurrency;
            }
            private set
            {
                _concurrency = value;
            }
        }

        public int Quantity
        {
            get
            {
                return _quantity;
            }
            private set
            {
                _quantity = value;
            }
        }

        public int Frequency
        {
            get
            {
                return _frequency;
            }
            private set
            {
                _frequency = value;
            }
        }

        public bool IsHomeDevice
        {
            get
            {
                return _isHomeDevice;
            }
            private set
            {
                _isHomeDevice = value;
            }
        }
    }
}
