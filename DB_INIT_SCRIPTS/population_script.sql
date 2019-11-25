INSERT INTO Ingredients
	VALUES('Egg', 0.2, 30, null)
INSERT INTO Ingredients
	VALUES('Cod fish', 3, 10, 'Kg')
INSERT INTO Ingredients
	VALUES('Potato', 0.1, 100, 'Kg')
INSERT INTO Ingredients
	VALUES('Tomato', 0.2, 100, 'Kg')
INSERT INTO Ingredients
	VALUES('Coriander', 1.5, 100, 'Kg')
INSERT INTO Ingredients
	VALUES('Pizza', 5, 100, null)

INSERT INTO dbo.Recipes
	values('Cod fish', 0)
INSERT INTO dbo.Recipes
	values('Cod fish with Potatoes', 0)
INSERT INTO dbo.Recipes
	values('Avilez Style Cod fish', 1)
INSERT INTO dbo.Recipes
	values('Cod fish Gomes de Sá', 0)

--Codfish 
INSERT INTO dbo.RecipeIngredients
	VALUES (1, 3, 1)
INSERT INTO dbo.RecipeIngredients
	VALUES (2, 1, 1)

--Codfish with potatoes
INSERT INTO dbo.RecipeIngredients
	VALUES (1, 3, 2)
INSERT INTO dbo.RecipeIngredients
	VALUES (2, 1, 2)
INSERT INTO dbo.RecipeIngredients
	VALUES (3, 3, 2)

	SELECT * FROM Ingredients
SELECT * FROM Recipes


--Codfish Avillez
INSERT INTO dbo.RecipeIngredients
	VALUES (1, 3, 3)
INSERT INTO dbo.RecipeIngredients
	VALUES (2, 1, 3)
INSERT INTO dbo.RecipeIngredients
	VALUES (4, 4, 3)
INSERT INTO dbo.RecipeIngredients
	VALUES (5, 2, 3)

-- Despensa
INSERT INTO dbo.StoreRoomItems
	Values(1, 3)


INSERT INTO Discounts
	values (15, 1)

SELECT * FROM Ingredients
SELECT * FROM Recipes

select top 0 * from RecipeIngredients

SELECT recing.RecipeIngredientId, rec.Name as recipeName, ing.Name as ingName, recing.Quantity FROM RecipeIngredients as recing
INNER JOIN Recipes as rec on recing.RecipeId = rec.RecipeId
inner join Ingredients as ing on recing.IngredientId = ing.IngredientId

select * from Carts

