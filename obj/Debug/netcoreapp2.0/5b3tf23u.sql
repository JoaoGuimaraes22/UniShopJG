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

ALTER TABLE [Ingredients] DROP CONSTRAINT [FK_Ingredients_Recipes_RecipeId];

GO

DROP INDEX [IX_Ingredients_RecipeId] ON [Ingredients];

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'Ingredients') AND [c].[name] = N'RecipeId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Ingredients] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Ingredients] DROP COLUMN [RecipeId];

GO

CREATE TABLE [RecipeIngredients] (
    [RecipeIngredientId] int NOT NULL IDENTITY,
    [IngredientId] int NOT NULL,
    [IngredientName] nvarchar(max) NULL,
    [Quantity] int NOT NULL,
    [RecipeId] int NOT NULL,
    [RecipeName] nvarchar(max) NULL,
    CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY ([RecipeIngredientId]),
    CONSTRAINT [FK_RecipeIngredients_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([IngredientId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([RecipeId]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_RecipeIngredients_IngredientId] ON [RecipeIngredients] ([IngredientId]);

GO

CREATE INDEX [IX_RecipeIngredients_RecipeId] ON [RecipeIngredients] ([RecipeId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190116122327_RecipeIngredient', N'2.0.3-rtm-10026');

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'RecipeIngredients') AND [c].[name] = N'IngredientName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [RecipeIngredients] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [RecipeIngredients] DROP COLUMN [IngredientName];

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'RecipeIngredients') AND [c].[name] = N'RecipeName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RecipeIngredients] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [RecipeIngredients] DROP COLUMN [RecipeName];

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190116123154_RecipeIngredient2', N'2.0.3-rtm-10026');

GO

