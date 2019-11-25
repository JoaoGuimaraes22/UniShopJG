using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UStoreBot.Models
{
    public class Discount
    {
        public int DiscountId { get; set; }

        public double DiscountPercentValue { get; set; }

        public Ingredient Ingredient { get; set; }
    }
}
