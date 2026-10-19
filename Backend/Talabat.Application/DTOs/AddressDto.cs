using System.ComponentModel.DataAnnotations.Schema;
using Talabat.Domain.Entities.Identity;

namespace Talabat.Application.DTOs
{
    public class AddressDto
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }

      
    }
}
