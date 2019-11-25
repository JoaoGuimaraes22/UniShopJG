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
    [TransactionTimeStamp] datetime2 NOT NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY ([CartId])
);

GO

CREATE TABLE [Ingredients] (
    [IngredientId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [PricePerUnit] float NOT NULL,
    [Quantity] int NOT NULL,
    [QuantityUnitMeasurement] nvarchar(max) NULL,
    CONSTRAINT [PK_Ingredients] PRIMARY KEY ([IngredientId])
);

GO

CREATE TABLE [Recipes] (
    [RecipeId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [RecipeOfTheWeek] bit NOT NULL,
    CONSTRAINT [PK_Recipes] PRIMARY KEY ([RecipeId])
);

GO

CREATE TABLE [CartIngredient] (
    [CartIngredientId] int NOT NULL IDENTITY,
    [CartId] int NOT NULL,
    [IngredientId] int NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_CartIngredient] PRIMARY KEY ([CartIngredientId]),
    CONSTRAINT [FK_CartIngredient_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([CartId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CartIngredient_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([IngredientId]) ON DELETE CASCADE
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

CREATE TABLE [StoreRoomItems] (
    [StoreRoomItemId] int NOT NULL IDENTITY,
    [IngredientId] int NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_StoreRoomItems] PRIMARY KEY ([StoreRoomItemId]),
    CONSTRAINT [FK_StoreRoomItems_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([IngredientId]) ON DELETE NO ACTION
);

GO

CREATE TABLE [RecipeIngredients] (
    [RecipeIngredientId] int NOT NULL IDENTITY,
    [IngredientId] int NOT NULL,
    [Quantity] int NOT NULL,
    [RecipeId] int NOT NULL,
    CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY ([RecipeIngredientId]),
    CONSTRAINT [FK_RecipeIngredients_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([IngredientId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_CartIngredient_CartId] ON [CartIngredient] ([CartId]);

GO

CREATE INDEX [IX_CartIngredient_IngredientId] ON [CartIngredient] ([IngredientId]);

GO

CREATE INDEX [IX_Discounts_IngredientId] ON [Discounts] ([IngredientId]);

GO

CREATE INDEX [IX_RecipeIngredients_IngredientId] ON [RecipeIngredients] ([IngredientId]);

GO

CREATE INDEX [IX_RecipeIngredients_RecipeId] ON [RecipeIngredients] ([RecipeId]);

GO

CREATE INDEX [IX_StoreRoomItems_IngredientId] ON [StoreRoomItems] ([IngredientId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190122183243_InitialCreate', N'2.0.3-rtm-10026');

GO

