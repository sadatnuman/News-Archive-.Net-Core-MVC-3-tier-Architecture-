using BLL.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace BLL.Validations
{
    public class UniqueEmail : ValidationAttribute
    {
        public UniqueEmail()
        {
            ErrorMessage = "Email already exists. Please use a different email address.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var email = value?.ToString();
            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Success;

            var users = validationContext.GetService(typeof(IUserService)) as IUserService;
            if (users == null || users.GetByEmail(email.Trim()) == null)
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage);
        }
    }
}
