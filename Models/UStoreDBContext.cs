using Microsoft.EntityFrameworkCore;

namespace UStoreBot.Models
{
    public class UStoreDBContext : DbContext
    {
        public UStoreDBContext(DbContextOptions<UStoreDBContext> options)
        : base(options)
        { }

        public virtual DbSet<Ingredient> Ingredients { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<Discount> Discounts { get; set; }
        public virtual DbSet<Recipe> Recipes { get; set; }
        public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        public virtual DbSet<StoreRoomItem> StoreRoomItems { get; set; }
    }
}
