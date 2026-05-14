using DAL.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
            var db = (NewsArchiveDbContext)validationContext.GetService(typeof(NewsArchiveDbContext));

            var data = (from u in db.Users
                        where u.Email.Equals(value.ToString())
                        select u).SingleOrDefault();
            if (data == null)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
