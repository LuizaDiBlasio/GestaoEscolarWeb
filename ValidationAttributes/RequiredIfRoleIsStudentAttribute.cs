using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.ValidationAttributes
{
    public class RequiredIfRoleIsStudentAttribute : ValidationAttribute   
    {
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
