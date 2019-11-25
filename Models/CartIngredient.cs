using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UStoreBot.Models
{
    public class CartIngredient
    {
        public int CartIngredientId { get; set; }

        public int CartId { get; set; }

        public Cart Cart { get; set; }

        public int IngredientId { get; set; }

        public Ingredient Ingredient { get; set; }

        public int Quantity { get; set; }
    }
}
