using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.ValidationAttributes
{

    /// <summary>
    /// Custom validation attribute that ensures a profile picture is required
    /// if the selected user role is "Student".
    /// </summary>
    public class RequiredIfRoleIsStudentAttribute : ValidationAttribute   
    {

        /// <summary>
        /// Validates whether a profile picture is provided when the selected role is "Student".
        /// <param name="value">The value of the property being validated (in this case, typically the ImageFile).</param>
        /// <param name="validationContext">The context information about the validation operation,
        /// which includes the instance of the model "Models.RegisterNewUserViewModel".</param>
        /// <returns>
        /// A "ValidationResult" indicating whether the validation was successful.
        /// Returns "ValidationResult" with an error message if the role is "Student" and no image file is provided.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (Models.RegisterNewUserViewModel)validationContext.ObjectInstance;

            // Se for "Student" E a imagem for nula, então é um erro.
            if (model.SelectedRole == "Student" && model.ImageFile == null)
            {
                //indica a mensagem de erro e qual propriedade a receberà (validationContext.MemberName), assim o html sabe onde mostrar a imagem
                return new ValidationResult(ErrorMessage ?? "All students must have a profile picture.", new[] { validationContext.MemberName });
            }

            // Outros roles são sempre Success
            return ValidationResult.Success;
        }
    }
}
