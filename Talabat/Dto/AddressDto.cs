using System.ComponentModel.DataAnnotations.Schema;
using Talabat.Core.Entitys.Identity;

namespace Talabat.Dto
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
