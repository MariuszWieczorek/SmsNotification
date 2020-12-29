using SmsNotification.Core.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsNotification.Core
{
    public class GenerateMessage
    {
        public string GenerateErrors(List<Error> errors, int interval)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            if (!errors.Any())
                return String.Empty;

            var message = $"Błędy z ostatnich {interval} minut.\n";
             
            foreach (var error in errors)
            {
                message +=
                $"{error.Message} {error.Date.ToString("dd-MM-yyyy HH:mm")}\n";
                
            }

            message += $@"Automatyczna wiadomość SMS wysłana z aplikacji";

            return message;
        }
    }
}
