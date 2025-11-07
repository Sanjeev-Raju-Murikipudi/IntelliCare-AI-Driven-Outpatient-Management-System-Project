using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.Helpers
{
    public static class PhoneFormatter
    {
        public static string FormatToE164(string rawNumber)
        {
            if (string.IsNullOrWhiteSpace(rawNumber))
                throw new ArgumentException("Mobile number is missing.");

            rawNumber = rawNumber.Trim();

            if (rawNumber.StartsWith("+91"))
                return rawNumber;

            if (rawNumber.StartsWith("91"))
                return "+" + rawNumber;

            if (rawNumber.Length == 10)
                return "+91" + rawNumber;

            throw new FormatException("Invalid mobile number format: " + rawNumber);
        }
    }
}
