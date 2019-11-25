IF OBJECT_ID(N'__EFMigrationsHistory') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [Carts] (
    [CartId] int NOT NULL IDENTITY,
    [IndActive] bit NOT NULL,
    [Total] float NOT NULL,
    [TransactionCompleted] bit NOT NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY ([CartId])
);

GO

CREATE TABLE [Recipes] (
    [RecipeId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Recipes] PRIMARY KEY ([RecipeId])
);

GO

CREATE TABLE [Ingredients] (
    [IngredientId] int NOT NULL IDENTITY,
    [CartId] int NULL,
    [Name] nvarchar(max) NULL,
    [Quantity] int NOT NULL,
    [RecipeId] int NULL,
    CONSTRAINT [PK_Ingredients] PRIMARY KEY ([IngredientId]),
    CONSTRAINT [FK_Ingredients_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([CartId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Ingredients_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Discounts] (
    [DiscountId] int NOT NULL IDENTITY,
    [DiscountPercentValue] float NOT NULL,
    [IngredientId] int NULL,
    CONSTRAINT [PK_Discounts] PRIMARY KEY ([DiscountId]),
    CONSTRAINT [FK_Discounts_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([IngredientId]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_Discounts_IngredientId] ON [Discounts] ([IngredientId]);

GO

CREATE INDEX [IX_Ingredients_CartId] ON [Ingredients] ([CartId]);

GO

CREATE INDEX [IX_Ingredients_RecipeId] ON [Ingredients] ([RecipeId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190116105406_InitialCreate', N'2.0.3-rtm-10026');

GO

