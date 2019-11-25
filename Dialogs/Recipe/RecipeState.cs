using System;
using System.Collections.Generic;
using UStoreBot.Models;

namespace UStoreBot.Dialogs.Recipe
{
    public class RecipeState
    {
        public int? RecipeId { get; set; }

        public string Name { get; set; }

        public int? PeopleForDinner { get; set; }

        public double? TotalPrice { get; set; }

        public bool UserConfirmRecipe { get; set; }

        public bool UseStoreRoomItems { get; set; }

        public bool? ConfirmOrder { get; set; }

        public bool? DoNotOrderFood { get; set; }

        public bool? UseRecipeOfTheWeek { get; set; }

        public bool? OrderPizza { get; set; }

        public int? CartId { get; set; }

        public DateTime? DinnerTime { get; set; }

        public List<Models.Recipe> SugestedRecipes { get; set; }

        // public Dictionary<int, int> IngredientsQuantities { get; set; }

        // public Dictionary<int, int> StoredIngredientsQuantities { get; set; }

        public Dictionary<int, int> OrderedIngredientsQuantities { get; set; }

        public void CleanState()
        {
            RecipeId = null;
            Name = null;
            PeopleForDinner = null;
            TotalPrice = null;
            UserConfirmRecipe = false;
            UseStoreRoomItems = false;
            ConfirmOrder = null;
            // IngredientsQuantities = null;
            // StoredIngredientsQuantities = null;
            SugestedRecipes = null;
            OrderedIngredientsQuantities = null;
            DoNotOrderFood = null;
            UseRecipeOfTheWeek = null;
            OrderPizza = null;
            CartId = null;
            DinnerTime = null;
        }
    }
}
