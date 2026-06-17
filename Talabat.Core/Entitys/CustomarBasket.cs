using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entitys
{
    public class CustomarBasket
    {
       public string  Id { get; set; }
        public List<BasketIteam> items { get; set; } 
    }
}
