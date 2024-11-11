using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility
{
    public class MaxFileSizeAttribute(int _maxFileSize):ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as FormFile;
            if (file != null) 
            {
                if (file.Length > _maxFileSize*1024*1024) 
                {
                    return new ValidationResult("this photo size is greater than max file size!");
                }
            }
            return ValidationResult.Success;
        }
    }
}
