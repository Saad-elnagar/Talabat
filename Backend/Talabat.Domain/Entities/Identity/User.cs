using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Domain.Entities.Identity
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; }
        public Address address { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get;  set; }
    }
}
