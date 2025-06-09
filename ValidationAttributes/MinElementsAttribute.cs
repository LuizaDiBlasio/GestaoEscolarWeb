using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.ValidationAttributes
{
    //classe para personalizar uma DataAnotation que valida uma lista com um valor mínimo de elementos
    public class MinElementsAttribute : ValidationAttribute //Herda da classe ValidationAttribute responsável pelas validações em DataAnotations
    {
        private readonly int _minElements;

        public MinElementsAttribute(int minElements) //receber por construtor o parâmentro de mínimo de elementos 
        {
            _minElements = minElements; 
        }

        /// <summary>
        /// Override de IsValid
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>Objeto do tipo ValidationResult com mensagem de erro, ou status de sucesso </returns>
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

