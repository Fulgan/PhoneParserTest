// See https://aka.ms/new-console-template for more information
using CsvHelper;
using CsvHelper.Configuration;
using Peppermint.ApplicationCore.Entities;
using PhoneNumbers;
using System.Globalization;
using System.Text.RegularExpressions;
namespace MLTest
{
    public class Program
    {

        public static void Main(string[] args)
        {
            void processEntry(string phoneNumber, Regex regex, ref int nonumber, ref int sucess, ref int failures, PhoneNumberUtil parser)
            {
                var sourceNumber = regex.Replace(phoneNumber, "");
                if (string.IsNullOrEmpty(sourceNumber))
                {
                    nonumber++;
                    return;
                }
                try
                {
                    var foundData = parser.Parse(sourceNumber, "CH");
                    var recognizedPhoneNumber = new Peppermint.ApplicationCore.Entities.PhoneNumber();
                    recognizedPhoneNumber.CountryCode = foundData.CountryCode.ToString();
                    recognizedPhoneNumber.Number = foundData.NationalNumber.ToString();
                    sucess++;
                    Console.WriteLine($"Data read -> \"{sourceNumber}\" result = {parser.Format(foundData, PhoneNumberFormat.E164)} Parsed: {recognizedPhoneNumber.Format(Peppermint.ApplicationCore.Entities.PhoneNumber.DefaultFormats.Display)}");
                }
                catch (NumberParseException ex)
                {
                    failures++;
                    Console.WriteLine($"Data read -> \"{sourceNumber}\" FAILED -> {ex.Message}");
                }
            }


            var lineCount = 0;
            var sucess = 0;
            var failures = 0;
            var nonumber = 0;
            const string dataFileName = @"Data/git-addresses.csv";
            const string phoneHeader = "Contact-Telephone";
            using var reader = new StreamReader(dataFileName, encoding: System.Text.Encoding.Latin1);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                Mode = CsvMode.NoEscape,
                BadDataFound = (data) =>
                Console.WriteLine($"Bad data: '{data.Field}' record : {data.RawRecord} ")
            });
            csv.Read();
            csv.ReadHeader();
            var parser = PhoneNumberUtil.GetInstance();
            var regex = new Regex(@"[^\x00-\x7F|\s]");
            while (csv.Read())
            {
                lineCount++;
                processEntry(csv.GetField(phoneHeader), regex,ref nonumber, ref sucess, ref failures, parser);
            }

            Console.WriteLine($"processed {lineCount} lines. {sucess} successfully parsed number, {failures} failures and {nonumber} lines with no number");
            var testPhoneNumber = parser.Parse("022 3463450x001", "CH");
        }
    }
}