using System;
using System.Net;
using System.Net.Sockets;

namespace TVinciShared
{
    public class IPAddressRange
    {
        AddressFamily addressFamily;
        byte[] lowerBytes;
        byte[] upperBytes;

        public IPAddressRange()
        {

        }

        public IPAddressRange Init(string ipFrom, string ipTo)
        {
            var _from = System.Net.IPAddress.Parse(ipFrom);
            var _to = System.Net.IPAddress.Parse(ipTo);
            return Init(_from, _to);
        }

        public IPAddressRange Init(IPAddress lowerInclusive, IPAddress upperInclusive)
        {
            // Assert that lower.AddressFamily == upper.AddressFamily
            this.addressFamily = lowerInclusive.AddressFamily;
            this.lowerBytes = lowerInclusive.GetAddressBytes();
            this.upperBytes = upperInclusive.GetAddressBytes();
            return this;
        }

        public bool IsInRange(string address)
        {
            return IsInRange(IPAddress.Parse(address));
        }

        public bool IsValidIpV6(string address)
        {
            if (address.IsNullOrEmpty()) return false;
            var success = IPAddress.TryParse(address, out var _ipAddress);
            return success && _ipAddress.AddressFamily == AddressFamily.InterNetworkV6;
        }

        public bool IsInRange(IPAddress address)
        {
            if (address.AddressFamily != addressFamily)
            {
                return false;
            }

            byte[] addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (int i = 0; i < this.lowerBytes.Length &&
                (lowerBoundary || upperBoundary); i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }
    }
}