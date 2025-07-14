using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.ValidationAttributes
{
    /// <summary>
    /// Minimum elements validation DataAnotation Atribute
    /// </summary>
    public class MinElementsAttribute : ValidationAttribute //Herda da classe ValidationAttribute responsável pelas validações em DataAnotations
    {
        private readonly int _minElements;

        public MinElementsAttribute(int minElements) //receber por construtor o parâmentro de mínimo de elementos 
        {
            _minElements = minElements; 
        }


        /// <summary>
        /// Validates that a collection of integers contains at least a specified minimum number of elements.
        /// <param name="value">The object to validate, expected to be an "ICollection{T}" of "int" type.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// Returns "ValidationResult.Success"if the collection's count is greater than or equal to the minimum required elements.
        /// Otherwise, returns a "ValidationResult" with the "ErrorMessage"/>.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) //Rebece como parametro um objeto qualquer e o contexto da validação
        {
            if (value is ICollection<int> list) //checar se o objeto recebido por parametro é uma lista de ints
            {
                if (list.Count >= _minElements) //se a contagem de elementos da lista >= ao minimo de elementos
                {
                    return ValidationResult.Success; //retornar sucesso
                }
            }

            //se não entro no if quer dizer que não cumpriu com a validação, retornar mensagem de erro
            return new ValidationResult(ErrorMessage); 
        }
    }
}

