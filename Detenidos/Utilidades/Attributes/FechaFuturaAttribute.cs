using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades.Validadores
{
    public class FechaFuturaAttribute: ValidationAttribute
	{
        public FechaFuturaAttribute()
        {
			this.ErrorMessage = "La fecha no puede ser mayor al día de hoy.";
        }
		protected override ValidationResult IsValid(object valor, ValidationContext contextoValidacion)
		{
			// Esta validación es porque no debemos repetir validaciones de atributos ya establecidos
			// Es decir, en esta clase no debemos programar la validación de [Required]
			if (valor == null || string.IsNullOrEmpty(valor.ToString()))
			{
				return ValidationResult.Success;
			}

            // Se realiza la validación correspondiente.
            DateTime? fecha = valor as DateTime?;

			// La fecha no debería ser mayor al día de hoy.
			if (fecha != null && fecha.Value.Date > DateTime.Today.AddDays(1).AddSeconds(-1))
			{
				return new ValidationResult(this.ErrorMessage);
			}

			return ValidationResult.Success;
		}
	}
}
