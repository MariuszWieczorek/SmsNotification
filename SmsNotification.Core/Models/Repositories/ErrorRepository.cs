using SmsNotification.Core.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsNotification.Core.Models.Repositories
{
    public class ErrorRepository
    {
        public List<Error> GetLastErrors(int intervalInMinutes)
        {
            // TODO: Pobieramy z bazy

            return new List<Error>
            {
                new Error {Message = "błąd testowy 1", Date = DateTime.Now},
                new Error {Message = "błąd testowy 2", Date = DateTime.Now},
            };

        }
    }
}
