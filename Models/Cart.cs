using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UStoreBot.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public double Total { get; set; }

        public bool IndActive { get; set; }

        public bool TransactionCompleted { get; set; }

        public DateTime TransactionTimeStamp { get; set; }

        public ICollection<CartIngredient> CartIngredients { get; set; }
    }
}
