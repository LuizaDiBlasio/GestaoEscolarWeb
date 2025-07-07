using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GestaoEscolarWeb.ValidationAttributes
{
    public class NoCommaAttribute : ValidationAttribute
    {
        public NoCommaAttribute()
        {
            // Mensagem padrão, pode ser sobrescrita no ViewModel
            ErrorMessage = "Please use a dot (.) as decimal separator.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            // value.ToString() é seguro porque agora esperamos uma string ou algo que vira string.
            string stringValue = value.ToString();

            // 1. Verificar a vírgula
            if (stringValue.Contains(","))
            {
                return new ValidationResult(ErrorMessage);
            }

            // 2. Opcional, mas recomendado: Verificar se o que sobrou é de fato um número válido.
            // Isso pega casos como "abc" ou "1.2.3" que não têm vírgula, mas não são decimais válidos.
            if (!decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                // Usa a mensagem de erro padrão ou uma mais específica para formato inválido
                return new ValidationResult("Invalid number format. Please ensure it is a valid decimal number.");
            }

            return ValidationResult.Success;
        }

    }
}
