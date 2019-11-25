using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UStoreBot.Models
{
    public class Ingredient
    {
        public int IngredientId { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public double PricePerUnit { get; set; }

        public string QuantityUnitMeasurement { get; set; }

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }

        public ICollection<CartIngredient> CartIngredients { get; set; }
    }
}
