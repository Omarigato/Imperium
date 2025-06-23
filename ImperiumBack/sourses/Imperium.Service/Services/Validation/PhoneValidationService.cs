using System.Text.RegularExpressions;
using Imperium.Core;

namespace Imperium.Service.Services.Validation
{
    public class PhoneValidationService : IPhoneValidationService
    {
        private static readonly Regex KazakhstanPhoneRegex = new Regex(
            @"^\+7\s?\(?(70[1257]|708|747|750|77[1567-8])\)?\s?\d{3}\s?\d{2}\s?\d{2}$",
            RegexOptions.Compiled);

        private static readonly Regex PhoneFormatRegex = new Regex(
            @"^\+?[1-9]\d{1,14}$",
            RegexOptions.Compiled);

        public bool IsValidKazakhstanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Удаляем все пробелы, скобки и дефисы для проверки
            var cleanNumber = phoneNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");

            // Проверяем, начинается ли с допустимых префиксов
            foreach (var prefix in Constants.KAZAKHSTAN_PHONE_PREFIXES)
            {
                if (cleanNumber.StartsWith(prefix) && cleanNumber.Length == 12) // +7 + 10 digits
                {
                    return true;
                }
            }

            return false;
        }

        public string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            // Удаляем все символы кроме цифр и +
            var cleanNumber = Regex.Replace(phoneNumber, @"[^\d+]", "");

            // Если начинается с 8, заменяем на +7
            if (cleanNumber.StartsWith("8") && cleanNumber.Length == 11)
            {
                cleanNumber = "+7" + cleanNumber.Substring(1);
            }
            // Если начинается с 7, добавляем +
            else if (cleanNumber.StartsWith("7") && cleanNumber.Length == 11)
            {
                cleanNumber = "+" + cleanNumber;
            }

            return cleanNumber;
        }

        public bool IsPhoneNumberFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var cleanInput = input.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
            return PhoneFormatRegex.IsMatch(cleanInput);
        }
    }
}
