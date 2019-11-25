using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UStoreBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UStoreBot
{
    public class UStoreDBManagement
    {
        private readonly UStoreDBContext _context;

        public UStoreDBManagement(UStoreDBContext context)
        {
            _context = context;
        }

        public Recipe GetRecipe(string name)
        {
            var recipes = _context.Recipes.Where(r => r.Name == name || r.Name == name.ToLower())
                                          .FirstOrDefault();

            return recipes;
        }

        public List<Recipe> GetRecipes()
        {
            return _context.Recipes.ToList();
        }

        public List<Ingredient> GetIngredientsFromRecipe(string recipeName)
        {
            List<Ingredient> lstRecipes = null;

            var recipe = _context.Recipes.Where(r => r.Name == recipeName)
                                         .Include(r => r.RecipeIngredients)
                                         .FirstOrDefault();

            if (recipe != null && recipe.RecipeIngredients != null && recipe.RecipeIngredients.Count > 0)
            {
                lstRecipes = new List<Ingredient>();

                foreach (var recipeIngredient in recipe.RecipeIngredients)
                {
                    var ingredientAux = _context.Ingredients.Where(r => r.IngredientId == recipeIngredient.IngredientId).FirstOrDefault();

                    if (ingredientAux != null)
                    {
                        lstRecipes.Add(ingredientAux);
                    }
                }
            }

            return lstRecipes;
        }

        public List<Ingredient> GetIngredientsFromRecipe(int recipeId)
        {
            List<Ingredient> lstRecipes = null;

            var recipe = _context.Recipes.Where(r => r.RecipeId == recipeId)
                                         .Include(r => r.RecipeIngredients)
                                         .FirstOrDefault();

            if (recipe != null && recipe.RecipeIngredients != null && recipe.RecipeIngredients.Count > 0)
            {
                lstRecipes = new List<Ingredient>();

                foreach (var recipeIngredient in recipe.RecipeIngredients)
                {
                    var ingredientAux = _context.Ingredients.Where(r => r.IngredientId == recipeIngredient.IngredientId).FirstOrDefault();

                    if (ingredientAux != null)
                    {
                        lstRecipes.Add(ingredientAux);
                    }
                }
            }

            return lstRecipes;
        }

        public Recipe GetRecipeByAproxName(string name)
        {
            var recipe = from recipes in _context.Recipes
                         where EF.Functions.Like(recipes.Name, "%" + name + "%")
                         select recipes;

            if (recipe != null)
            {
                return recipe.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public int? GetNecessaryIngredientForRecipe(int recipeId, int ingredientId)
        {
            var recipeIngredient = _context.RecipeIngredients.Where(recpIng => recpIng.RecipeId == recipeId && recpIng.IngredientId == ingredientId).FirstOrDefault();

            if (recipeIngredient != null)
            {
                return recipeIngredient.Quantity;
            }
            else
            { return null; }
        }

        public List<StoreRoomItem> GetStoreRoomItems()
        {
            return _context.StoreRoomItems.Include(r => r.Ingredient).ToList();
        }

        public Ingredient GetIngredientById(int ingredientId)
        {
            return _context.Ingredients.Where(ing => ing.IngredientId == ingredientId).FirstOrDefault();
        }

        public double GetIngredientPricePerUnit(int ingredientId)
        {
            return _context.Ingredients.Where(ing => ing.IngredientId == ingredientId).FirstOrDefault().PricePerUnit;
        }

        public int CreateCart(Dictionary<int, int> orderList, double total)
        {
            Cart cart = new Cart();

            cart.Total = total;
            cart.IndActive = true;

            cart.CartIngredients = new List<CartIngredient>();

            foreach (var keyValuePairLoop in orderList)
            {
                CartIngredient cartIngredientLoop = new CartIngredient();
                cartIngredientLoop.IngredientId = keyValuePairLoop.Key;
                cartIngredientLoop.Quantity = keyValuePairLoop.Value;
                cart.CartIngredients.Add(cartIngredientLoop);
            }

            _context.Carts.Add(cart);
            _context.SaveChanges();

            return cart.CartId;
        }

        public Recipe GetRecipeOfTheWeek()
        {
            return _context.Recipes.Where(recipe => recipe.RecipeOfTheWeek).FirstOrDefault();
        }

        public List<Recipe> GetSugestedRecipes()
        {
            List<Recipe> resultRecipes = new List<Recipe>();

            resultRecipes.Add(GetRecipeOfTheWeek()); // adicionar receita do mês à lista de receitas na 1ª posição

            var recipes = _context.Recipes.Where(rcp => !rcp.RecipeOfTheWeek).ToList();

            if (recipes != null && recipes.Count > 0)
            {
                Random rdm = new Random();
                int r = rdm.Next(recipes.Count);
                resultRecipes.Add(recipes[r]);
                recipes.RemoveAt(r);
                r = rdm.Next(recipes.Count());
                resultRecipes.Add(recipes[r]);
            }

            return resultRecipes;
        }

        public Discount GetIngredientDiscount(int ingredientId)
        {
            var discount = _context.Discounts.
                Where(dscnt => dscnt.Ingredient.IngredientId == ingredientId).
                Include(d => d.Ingredient).
                FirstOrDefault();

            if (discount != null)
            {
                return discount;
            }
            else
            {
                return null;
            }
        }

        public void FinalizeCartTransaction(int cartId, bool transaction)
        {
            var cart = _context.Carts.Where(crt => crt.CartId == cartId).Include(crt => crt.CartIngredients).FirstOrDefault();

            if (cart != null)
            {
                cart.IndActive = false;

                if (transaction)
                {
                    cart.TransactionCompleted = transaction;
                    cart.TransactionTimeStamp = DateTime.Now;
                }
            }

            if (cart.CartIngredients != null && cart.CartIngredients.Count > 0)
            {
                foreach (var cartIngredientLoop in cart.CartIngredients)
                {
                    var ingredientLoop = _context.Ingredients.Where(ing => ing.IngredientId == cartIngredientLoop.IngredientId).FirstOrDefault();

                    if (ingredientLoop != null)
                    {
                        ingredientLoop.Quantity -= cartIngredientLoop.Quantity;
                    }
                }
            }

            _context.SaveChanges();
        }

        public Ingredient GetIngredientPizza()
        {
            return _context.Ingredients.Where(ing => ing.Name == "Pizza").FirstOrDefault();
        }
    }
}
