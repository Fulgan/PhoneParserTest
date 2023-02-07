using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Humanizer;
using static Peppermint.ApplicationCore.Entities.PhoneNumber;

#nullable enable
namespace Peppermint.ApplicationCore.Entities
{
    // this class represents a phone number
    public class PhoneNumber
    {

        public enum FullNumberType {ForDialing, ForDisplay}
        public enum PhoneNumberType {Primary, Mobile, Personal, Other}

        [MaxLength(10)]
        public string? CountryCode { get; set; }
        [MaxLength(50)]
        public string? Number { get; set; }
        [MaxLength(10)]
        public string? Extension { get; set; }

        public static class DefaultFormats
        {
            public static PhoneNumberFormatProvider Dialing => new("+{0}", "{0}", "{0}");
            public static PhoneNumberFormatProvider Display => new("+{0}", " {0}", "Ext. ({0})");
            public static PhoneNumberFormatProvider Epp => new("{0}.", "{0}", "x{0}");

            public static PhoneNumberFormatProvider Raw => new(
                new("{0}", NumberFormatSpecifier.FormatOptions.KeepFullString),
                new("{0}", NumberFormatSpecifier.FormatOptions.KeepFullString), 
                new("{0}", NumberFormatSpecifier.FormatOptions.KeepFullString));
        }
    }

    public record NumberFormatSpecifier
    {
        public NumberFormatSpecifier(string formatSpecifier, FormatOptions options)
        {
            FormatSpecifier = formatSpecifier;
            Options = options;
        }

        public string FormatSpecifier { get; init; }
        public NumberFormatSpecifier.FormatOptions Options { get; init; } = NumberFormatSpecifier.FormatOptions.NumbersOnly;
        public enum FormatOptions {NumbersOnly, IgnoreIfAllZeros, KeepFullString}
        public override string? ToString() => FormatSpecifier;

        public static implicit operator NumberFormatSpecifier(string input)
        {
            return new(input, FormatOptions.NumbersOnly);
        }
        private static string NumbersOnly(string input) => string.Join(string.Empty, Regex.Matches(input, @"\d+").OfType<Match>().Select(m => m.Value));

        private static string? IgnoreIfAllZeros(string input)
        {
            var res = NumbersOnly(input); // only get the numbers
            return res.Any(t => t != '0') ? res : null; // only return the result if it is not made only of 0s
        }
        private static string FormatIfNotNullOrWhiteSpace(string? value, string format) => string.IsNullOrWhiteSpace(value) ? "" : string.Format(StringPadderFormatProvider.Default, format, NumbersOnly(value));
        public string Format(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";
            var res = Options switch
            {
                FormatOptions.NumbersOnly => FormatIfNotNullOrWhiteSpace(NumbersOnly(input), FormatSpecifier),
                FormatOptions.IgnoreIfAllZeros => FormatIfNotNullOrWhiteSpace(IgnoreIfAllZeros(input), FormatSpecifier),
                FormatOptions.KeepFullString => FormatIfNotNullOrWhiteSpace(input, FormatSpecifier),
                _ => throw new NotImplementedException(),
            };
            return string.IsNullOrWhiteSpace(res) ? "" : res;
        }
    }

    public record PhoneNumberFormatProvider(NumberFormatSpecifier CountryCode, NumberFormatSpecifier Number, NumberFormatSpecifier Extension);


    public static class PhoneNumberExtensions
    {

        public static string Format(this PhoneNumber instance, PhoneNumberFormatProvider format)
        {
            return format.CountryCode.Format(instance.CountryCode) +
                   format.Number.Format(instance.Number) +
                   format.Extension.Format(instance.Extension);
        }
        public static string FullNumber(this PhoneNumber instance, FullNumberType type = FullNumberType.ForDialing)
        {
            return type switch
            {
                FullNumberType.ForDialing => instance.Format(PhoneNumber.DefaultFormats.Dialing),
                FullNumberType.ForDisplay => instance.Format(PhoneNumber.DefaultFormats.Display),
                _ => throw new NotImplementedException(),
            };
        }

        public static PhoneNumber? ParsePhoneNumberFromWinEURFormat(this string rawValue)
        {
            var matchResult = Regex.Match(rawValue, @"^([0-9]{3})?[-. \/]?([0-9 ]{7,9})$");
            if (!matchResult.Success)
                return null;

            var areaCode = matchResult.Groups[1].Value;
            var number = matchResult.Groups[2].Value;

            return new PhoneNumber()
            {
                Number = number
            };
        }
    }

}
