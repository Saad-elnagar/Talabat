using System.ComponentModel.DataAnnotations.Schema;

namespace Talabat.Core.Entitys.Identity
{
    public class Address
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }  

        public User User { get; set; }
    }
}