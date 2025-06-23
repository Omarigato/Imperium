namespace Imperium.Service.Services.Validation
{
    public interface IPhoneValidationService
    {
        bool IsValidKazakhstanPhoneNumber(string phoneNumber);
        string FormatPhoneNumber(string phoneNumber);
        bool IsPhoneNumberFormat(string input);
    }
}
