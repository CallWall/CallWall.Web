using System;
using PhoneNumbers;

namespace CallWall.Web.Domain
{
    public static class PhoneNumber
    {
        private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();
        private const string ItuPrefix = "00";
        private const string NanpaPrefix = "011";
        private const string JapanPrefix = "010";
        private const string AustraliaPrefix = "0011";

        public static string[] Parse(string input)
        {
            var result = ParseAsGsmFormat(input);
            if (result != null) return result;

            var normalizedNumber = PhoneNumberUtil.Normalize(input);

            result = ParseWithInternationalPrefix(normalizedNumber, ItuPrefix);
            if (result != null) return result;

            result = ParseWithInternationalPrefix(normalizedNumber, NanpaPrefix);
            if (result != null) return result;

            result = ParseWithInternationalPrefix(normalizedNumber, JapanPrefix);
            if (result != null) return result;

            result = ParseWithInternationalPrefix(normalizedNumber, AustraliaPrefix);
            if (result != null) return result;

            return new[] { normalizedNumber };
        }

        private static string[] ParseAsGsmFormat(string gmsFormatNumber)
        {
            try
            {
                var parsedNumber = PhoneUtil.Parse(gmsFormatNumber, RegionCode.ZZ);

                var internationalGsmFormat = PhoneUtil.FormatNumberForMobileDialing(parsedNumber, null, false);
                var nationalFormat = PhoneUtil.Format(parsedNumber, PhoneNumberFormat.NATIONAL);
                nationalFormat = PhoneNumberUtil.Normalize(nationalFormat);
                return new[] { internationalGsmFormat, nationalFormat };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string[] ParseWithInternationalPrefix(string input, string prefix)
        {
            if (input.StartsWith(prefix))
            {
                return ParseAsGsmFormat("+" + input.Substring(prefix.Length));
            }
            return null;
        }
    }
}