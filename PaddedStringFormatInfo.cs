using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Peppermint.ApplicationCore
{
    public class StringPadderFormatProvider : IFormatProvider
    {
        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(ICustomFormatter) ? new PaddedStringFormatInfo() : null;
        }
        public static readonly IFormatProvider Default =
            new StringPadderFormatProvider();
    }

    public sealed class PaddedStringFormatInfo : IFormatProvider, ICustomFormatter
    {
        public object? GetFormat(Type? formatType)
        {
            return typeof(ICustomFormatter) == formatType ? this : null;
        }
        private string HandleOtherFormats(string? format, object? arg)
        {
            return arg switch
            {
                IFormattable fmt => fmt.ToString(format, CultureInfo.CurrentCulture),
                { } obj => obj.ToString()??string.Empty,
                _ => string.Empty,
            };
        }

        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrWhiteSpace(format))
                 return HandleOtherFormats(format, arg);

            string argAsString = arg?.ToString() ?? "";

            var args = format.Split(':');

            if (args.Length == 1)
                return string.Format("{0, " + format + "}", arg);

            

            if (!int.TryParse(args[0], out var padLength))
                throw new ArgumentException("Padding length should be an integer");
            var padChar = args[1][0];
            switch (args.Length)
            {
                case 2://Padded format
                    return (padLength > 0 ? argAsString.PadLeft(padLength, padChar) : argAsString.PadRight(padLength * -1, padChar))??"";
                default://Use default string.format
                    return string.Format("{0," + format + "}", argAsString);
            }
        }
    }
}
