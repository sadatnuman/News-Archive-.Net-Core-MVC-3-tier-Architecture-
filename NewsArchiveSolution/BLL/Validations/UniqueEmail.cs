using DAL.Repos;
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

            var repo = (UserRepo?)validationContext.GetService(typeof(UserRepo));
            if (repo == null || repo.GetByEmail(email) == null)
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage);
        }
    }
}
