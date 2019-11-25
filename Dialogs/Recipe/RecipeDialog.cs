using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UStoreBot.Models;

namespace UStoreBot.Dialogs.Recipe
{
    public class RecipeDialog : ComponentDialog
    {
        private const string FluxoEscolhaReceitas = "receitasDialog";
        private readonly UStoreDBContext _context;
        private static RecipeResponses _responder = new RecipeResponses();

        private static readonly string logicApp = "https://prod-24.northeurope.logic.azure.com:443/workflows/f079215777794f82ae16e08905e358ca/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=XCUIk90ftDgxA4gPxcO612tz571WW0OR8Jsl89RcuRM";

        private class DialogPromptIds
        {
            public const string OrderFoodPrompt = "orderFoodPrompt";
            public const string UserRecipeOfTheWeekPrompt = "userRecipeOfTheWeekPrompt";
            public const string RecipePrompt = "recipePrompt";
            public const string NumberOfPeopleForDinnerPrompt = "numberOfPeopleForDinnerPrompt";
            public const string ConfirmRecipePrompt = "confirmRecipePrompt";
            public const string ConfirmAddStorageItemsPrompt = "confirmAddStorageItemsPrompt";
            public const string ConfirmTotalPrice = "confirmTotalPrice";
            public const string SugestedRecipePrompt = "sugestedRecipePrompt";
            public const string AskForUserInputIngredient = "askForUserInputIngredient";
            public const string OrderPizzaPrompt = "orderPizzaPrompt";
            public const string DinnerDatePrompt = "dinnerDatePrompt";
            public const string ConfirmDatePrompt = "confirmDatePrompt";
            public const string AskUserOrderPizzaPrompt = "askUserOrderPizzaPrompt";
        }

        public RecipeDialog(IStatePropertyAccessor<RecipeState> userProfileStateAccessor, ILoggerFactory loggerFactory, UStoreDBContext context)
            : base(nameof(RecipeDialog))
        {
            _context = context;

            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            var waterfallSteps = new WaterfallStep[]
            {
                InitializeStateStepAsync, // dialogo de inicialização , verifica se já existe state
                CheckIfUserNeedsHelpWithMenu, // verificar se o utilizador quer mandar vir comida
                CheckPromotions, // verifica se existem ingredientes em promoção
                UseRecipeOfTheWeek, // dialogo em que sugerimos receita do mês mais 2 receitas random
                DisplayRecipeStepAsync, // dialogo que apresenta ao utilizador uma lista de recipes disponiveis
                NumberOfPersonForDinnerAsync, // pergunta quantas pessoas são para jantar
                DisplayNecessaryIngredientsListAsync, // dialogo que apresenta a lista de ingredientes necessários para uma determinada recipeLoop
                DisplayBillUser, // dialogo que apresenta conta final ao utilizador e pede confirmação
                DisplayPurchaseCompleted, // dialogo que apresenta confirmação ao utilizador que a compra foi terminada com sucesso
                PlanEventAsync, // pergunta data ao utilizador e marca evento
                ConfirmDateAsync, // confirma que a data foi bem inserida e marca o evento e envia o mail
            };

            AddDialog(new WaterfallDialog(FluxoEscolhaReceitas, waterfallSteps));
            AddDialog(new ConfirmPrompt(DialogPromptIds.OrderFoodPrompt, null));
            AddDialog(new ConfirmPrompt(DialogPromptIds.UserRecipeOfTheWeekPrompt, null));
            AddDialog(new TextPrompt(DialogPromptIds.RecipePrompt, ValidateRecipe));
            AddDialog(new TextPrompt(DialogPromptIds.SugestedRecipePrompt, ValidateOptionRecipe)); // ValidateOption;
            AddDialog(new TextPrompt(DialogPromptIds.AskForUserInputIngredient, null));
            AddDialog(new NumberPrompt<int>(DialogPromptIds.NumberOfPeopleForDinnerPrompt));
            AddDialog(new ConfirmPrompt(DialogPromptIds.ConfirmRecipePrompt, null)); // ValidateConfirmRecipePrompt));
            AddDialog(new ConfirmPrompt(DialogPromptIds.ConfirmAddStorageItemsPrompt, null));
            AddDialog(new ConfirmPrompt(DialogPromptIds.ConfirmTotalPrice, null));
            AddDialog(new ConfirmPrompt(DialogPromptIds.OrderPizzaPrompt, null));
            AddDialog(new DateTimePrompt(DialogPromptIds.DinnerDatePrompt, DateValidatorAsync));
            AddDialog(new ConfirmPrompt(DialogPromptIds.ConfirmDatePrompt, null));
            AddDialog(new ConfirmPrompt(DialogPromptIds.AskUserOrderPizzaPrompt, null));
        }

        public IStatePropertyAccessor<RecipeState> UserProfileAccessor { get; }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var receitasState = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);

            if (receitasState == null)
            {
                if (stepContext.Options is RecipeState receitasStateOpt)
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, receitasStateOpt);
                }
                else
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, new RecipeState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> CheckIfUserNeedsHelpWithMenu(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(DialogPromptIds.OrderFoodPrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(stepContext.Context, stepContext.Context.Activity.Locale, RecipeResponses.ReceitasRespondeIds.InitialDialog),
            });
        }

        private async Task<DialogTurnResult> CheckPromotions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            // var resultConfirmationWantsToOrderFood = stepContext.Result as bool?;
            var resultConfirmationWantsToOrderFood = ParseSpokenBool(stepContext);

            if (resultConfirmationWantsToOrderFood.HasValue)
            {
                recipeState.DoNotOrderFood = !resultConfirmationWantsToOrderFood.Value;
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                if (!resultConfirmationWantsToOrderFood.Value)
                {
                    return await stepContext.NextAsync();
                }
            }

            var recipe = new UStoreDBManagement(_context).GetRecipeOfTheWeek();

            if (recipe != null)
            {
                var ingredientsOfRecipeOfTheWeek = new UStoreDBManagement(_context).GetIngredientsFromRecipe(recipe.RecipeId);

                if (ingredientsOfRecipeOfTheWeek != null &&
                    ingredientsOfRecipeOfTheWeek.Count > 0)
                {
                    Models.Discount ingredientDiscount = null;

                    foreach (var item in ingredientsOfRecipeOfTheWeek)
                    {
                        ingredientDiscount = new UStoreDBManagement(_context).GetIngredientDiscount(item.IngredientId);

                        if (ingredientDiscount != null)
                        {
                            break;
                        }
                    }

                    // var ingredientDiscount = new UStoreDBManagement(_context).GetIngredientDiscount(ingredientsOfRecipeOfTheWeek[0].IngredientId);

                    if (ingredientDiscount != null)
                    {
                        string ingredientName = ingredientDiscount.Ingredient.Name;
                        double discount = ingredientDiscount.DiscountPercentValue;

                        return await stepContext.PromptAsync(DialogPromptIds.UserRecipeOfTheWeekPrompt, new PromptOptions()
                        {
                            Prompt = await _responder.RenderTemplate(stepContext.Context, stepContext.Context.Activity.Locale, RecipeResponses.ReceitasRespondeIds.RecipeOfTheWeekDialog, new { recipe.Name, discount, ingredientName }),
                        });
                    }
                }
            }


            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> UseRecipeOfTheWeek(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (recipeState.DoNotOrderFood.HasValue &&
                recipeState.DoNotOrderFood.Value)
            {
                return await stepContext.NextAsync();
            }

            // var resultBlnUseRecipeOfTheWeek = stepContext.Result as bool?;
            var resultBlnUseRecipeOfTheWeek = ParseSpokenBool(stepContext);

            if (resultBlnUseRecipeOfTheWeek.HasValue)
            {
                recipeState.UseRecipeOfTheWeek = resultBlnUseRecipeOfTheWeek.Value;
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
            }

            if (resultBlnUseRecipeOfTheWeek.HasValue && resultBlnUseRecipeOfTheWeek.Value)
            {
                // se ele aceitou a sugestão dar a hipotese de escolher a receita do mês mais duas
                //passar para lista de receitas sugeridas e pedir para ele escolher por opção

                var sugestedrecipes = new UStoreDBManagement(_context).GetSugestedRecipes();

                await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.SugestedRecipes);

                if (sugestedrecipes != null && sugestedrecipes.Count > 0)
                {
                    // sugestedrecipes.ForEach(
                    //    async recipe => await stepContext.Context.SendActivityAsync("" + recipe.Name, recipe.Name, InputHints.IgnoringInput));

                    for (int i = 0; i < sugestedrecipes.Count; i++)
                    {
                        string strAux = string.Format("Option {0} - {1}", (i + 1).ToString(), sugestedrecipes[i].Name);
                        await stepContext.Context.SendActivityAsync(strAux, strAux, InputHints.IgnoringInput);
                    }

                    recipeState.SugestedRecipes = sugestedrecipes;
                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                    return await stepContext.PromptAsync(DialogPromptIds.SugestedRecipePrompt, new PromptOptions()
                    {
                        Prompt = await _responder.RenderTemplate(
                            stepContext.Context,
                            stepContext.Context.Activity.Locale,
                            RecipeResponses.ReceitasRespondeIds.WhichRecipePreferes),
                    });
                }
                else
                {
                    throw new Exception("não encontrou receitas para apresentar");
                }
            }
            else
            {
                // pedir ao utilizador para escolher um ingrediente
                // rever fluxo
                return await stepContext.PromptAsync(DialogPromptIds.AskForUserInputIngredient, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(
                        stepContext.Context,
                        stepContext.Context.Activity.Locale,
                        RecipeResponses.ReceitasRespondeIds.AskForUserInputIngredient),
                });
            }
        }


        private async Task<DialogTurnResult> DisplayRecipeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (recipeState.DoNotOrderFood.HasValue &&
                recipeState.DoNotOrderFood.Value)
            {
                return await stepContext.NextAsync();
            }

            var lowerCaseNome = stepContext.Result as string;

            if (!string.IsNullOrWhiteSpace(lowerCaseNome) &&
                recipeState.UseRecipeOfTheWeek.HasValue &&
                recipeState.UseRecipeOfTheWeek.Value)
            {
                // recipeState.Name = lowerCaseNome;

                // convert string to int e ir buscar a opção da receita
                if (int.TryParse(lowerCaseNome, out int aux))
                {
                    var recipeSelected = recipeState.SugestedRecipes.ElementAt(aux - 1);

                    if (recipeSelected != null)
                    {
                        recipeState.RecipeId = recipeSelected.RecipeId;
                        recipeState.Name = recipeSelected.Name;
                        await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                    }
                }
            }

            if (//!string.IsNullOrWhiteSpace(recipeState.Name) &&
                recipeState.RecipeId.HasValue &&
                recipeState.UseRecipeOfTheWeek.HasValue &&
                recipeState.UseRecipeOfTheWeek.Value)
            {
                // var ingredients = new UStoreDBManagement(_context).GetIngredientsFromRecipe(recipeState.Name);
                var ingredients = new UStoreDBManagement(_context).GetIngredientsFromRecipe(recipeState.RecipeId.Value);

                if (ingredients != null)
                {
                    await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.FollowingIngredientsInMind);

                    foreach (var ingredientLoop in ingredients)
                    {
                        await stepContext.Context.SendActivityAsync(ingredientLoop.Name, ingredientLoop.Name, InputHints.IgnoringInput);
                    }

                    return await stepContext.NextAsync();
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("I did not find that recipe.", "I did not find that recipe.", InputHints.IgnoringInput);
                    // return await stepContext.NextAsync();
                    recipeState.CleanState();
                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                    var evento = Activity.CreateEndOfConversationActivity();
                    await stepContext.Context.SendActivityAsync(evento);
                    return await stepContext.EndDialogAsync();
                }
            }
            else
            {
                // o utilizador não quis a receita do mês e optou por inserir um ingrediente. perguntar se ele quer antes encomendar pizza
                return await stepContext.PromptAsync(DialogPromptIds.AskUserOrderPizzaPrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(
                        stepContext.Context,
                        stepContext.Context.Activity.Locale,
                        RecipeResponses.ReceitasRespondeIds.AskUserOrderPizza),
                });
            }
        }

        private async Task<DialogTurnResult> NumberOfPersonForDinnerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (recipeState.DoNotOrderFood.HasValue &&
                recipeState.DoNotOrderFood.Value)
            {
                return await stepContext.NextAsync();
            }

            if (recipeState.UseRecipeOfTheWeek.HasValue && recipeState.UseRecipeOfTheWeek.Value)
            {
                // perguntar quantas pessoas o utilizador vai ter para jantar
                return await stepContext.PromptAsync(DialogPromptIds.NumberOfPeopleForDinnerPrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(
                    stepContext.Context,
                    stepContext.Context.Activity.Locale,
                    RecipeResponses.ReceitasRespondeIds.NumberOfPeopleForDinner),
                });
            }
            else
            {
                // ler resposta se o utilizador quer encomendar pizza
                var resultOrderPizza = ParseSpokenBool(stepContext);

                if (resultOrderPizza.HasValue)
                {
                    if (resultOrderPizza.Value)
                    {
                        recipeState.OrderPizza = resultOrderPizza.Value;

                        recipeState.UseStoreRoomItems = true;

                        // criar carrinho de compras com a pizza
                        var pizza = new UStoreDBManagement(_context).GetIngredientPizza();
                        if (pizza != null)
                        {
                            Dictionary<int, int> necessaryIngredientsToBuy = new Dictionary<int, int>
                            {
                                { pizza.IngredientId, 1 },
                            }; // colocar no dicionário o ingrediente pizza

                            recipeState.OrderedIngredientsQuantities = necessaryIngredientsToBuy;
                            recipeState.TotalPrice = pizza.PricePerUnit;
                            recipeState.CartId = new UStoreDBManagement(_context).CreateCart(necessaryIngredientsToBuy, pizza.PricePerUnit);
                        }
                        else
                        {
                            recipeState.CleanState();
                            await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                            throw new Exception("Não encontrou pizza na BD");
                        }

                        await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                        return await stepContext.NextAsync();
                    }
                    else
                    {
                        recipeState.DoNotOrderFood = true;
                        await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                        return await stepContext.NextAsync();
                    }
                }
            }

            return await stepContext.NextAsync();
            //}
        }

        private async Task<DialogTurnResult> DisplayNecessaryIngredientsListAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (recipeState.DoNotOrderFood.HasValue &&
                recipeState.DoNotOrderFood.Value)
            {
                return await stepContext.NextAsync();
            }

            if (recipeState.OrderPizza.HasValue && recipeState.OrderPizza.Value)
            {
                return await stepContext.NextAsync();
            }

            int? numPersons = stepContext.Result as int?;

            if (!recipeState.PeopleForDinner.HasValue && numPersons.HasValue)
            {
                recipeState.PeopleForDinner = numPersons.Value;
            }

            var context = stepContext.Context;

            var ingredients = new UStoreDBManagement(_context).GetIngredientsFromRecipe(recipeState.Name);
            var storeRoomItems = new UStoreDBManagement(_context).GetStoreRoomItems();

            if (ingredients != null && ingredients.Count > 0)
            {
                var recipe = new UStoreDBManagement(_context).GetRecipe(recipeState.Name);

                Dictionary<int, int> orderedIngredientsQuantities = new Dictionary<int, int>();

                await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.NecessaryIngredients);

                foreach (Ingredient ingredientLoop in ingredients)
                {
                    if (!string.IsNullOrEmpty(ingredientLoop.Name))
                    {
                        int? quantityPerPerson = new UStoreDBManagement(_context).GetNecessaryIngredientForRecipe(recipe.RecipeId, ingredientLoop.IngredientId);
                        int? quantity = null;
                        if (quantityPerPerson.HasValue)
                        {
                            quantity = quantityPerPerson.Value * recipeState.PeopleForDinner.Value;
                        }

                        // verificar se existe em despensa e subtrair à quantidade necessária
                        if (storeRoomItems != null)
                        {
                            var abc = storeRoomItems.Find(st => st.Ingredient != null &&
                                                          st.Ingredient.IngredientId == ingredientLoop.IngredientId);

                            if (abc != null && abc.Quantity > 0)
                            {
                                quantity -= abc.Quantity;
                            }
                        }

                        string strIngredientMsg = string.Empty;

                        if (quantity != null && quantity.HasValue && quantity > 0)
                        {
                            await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                            if (quantity.Value > 1)
                            {
                                strIngredientMsg = quantity.ToString() + " " + ingredientLoop.Name + "s";
                            }
                            else
                            {
                                strIngredientMsg = quantity.ToString() + " " + ingredientLoop.Name;
                            }

                            orderedIngredientsQuantities.Add(ingredientLoop.IngredientId, quantity.Value);

                            await stepContext.Context.SendActivityAsync(strIngredientMsg, strIngredientMsg, InputHints.IgnoringInput);
                        }
                        //else
                        //{
                        //    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                        //    strIngredientMsg = ingredientLoop.Name;
                        //}
                    }
                }

                if (orderedIngredientsQuantities.Count > 0)
                {
                    recipeState.OrderedIngredientsQuantities = orderedIngredientsQuantities;
                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                }

                return await stepContext.PromptAsync(DialogPromptIds.ConfirmRecipePrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(
                        stepContext.Context,
                        stepContext.Context.Activity.Locale,
                        RecipeResponses.ReceitasRespondeIds.OrderNecessaryIngredients),
                });
            }
            else
            {
                await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.RecipeIngredientsNotFound);

                recipeState.CleanState();
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                await stepContext.CancelAllDialogsAsync();
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> DisplayBillUser(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (recipeState.DoNotOrderFood.HasValue &&
                recipeState.DoNotOrderFood.Value)
            {
                return await stepContext.NextAsync();
            }

            bool? resultConfirmationUseStorageItemsDialog = null;

            if (recipeState.OrderPizza.HasValue &&
                recipeState.OrderPizza.Value)
            {
                resultConfirmationUseStorageItemsDialog = true;
                recipeState.UseStoreRoomItems = resultConfirmationUseStorageItemsDialog.Value;
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
            }
            else
            {
                resultConfirmationUseStorageItemsDialog = ParseSpokenBool(stepContext);
                recipeState.UseStoreRoomItems = resultConfirmationUseStorageItemsDialog.Value;
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
            }

            if (resultConfirmationUseStorageItemsDialog.HasValue &&
                resultConfirmationUseStorageItemsDialog.Value &&
                recipeState.OrderedIngredientsQuantities != null &&
                recipeState.OrderedIngredientsQuantities.Count > 0)
            {
                await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.ProceedLogin);

                // CALCULAR TOTAL E PERGUNTAR SE PODE COMMIT TRANSACCAO

                Dictionary<int, int> necessaryIngredientsToBuy = recipeState.OrderedIngredientsQuantities;

                // calcular preço final
                double total = 0;

                if (recipeState.OrderPizza.HasValue &&
                    recipeState.OrderPizza.Value &&
                    recipeState.TotalPrice.HasValue)
                {
                    total = recipeState.TotalPrice.Value;
                }
                else
                {
                    foreach (var itemLoop in necessaryIngredientsToBuy)
                    {
                        var pricePerUnit = new UStoreDBManagement(_context).GetIngredientPricePerUnit(itemLoop.Key);
                        total += pricePerUnit * itemLoop.Value;
                    }

                    recipeState.TotalPrice = total;
                    recipeState.OrderedIngredientsQuantities = necessaryIngredientsToBuy;

                    // abrir carrinho de compras e colocar lá pedido
                    recipeState.CartId = new UStoreDBManagement(_context).CreateCart(necessaryIngredientsToBuy, total);

                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                }

                if (total > 0)
                {
                    return await stepContext.PromptAsync(DialogPromptIds.ConfirmTotalPrice, new PromptOptions()
                    {
                        Prompt = await _responder.RenderTemplate(
                            stepContext.Context,
                            stepContext.Context.Activity.Locale,
                            RecipeResponses.ReceitasRespondeIds.ConfirmTotalPrice,
                            total.ToString()),
                    });
                }
                else
                {
                    // não é suposto passar por aqui
                    // mandar msg a dizer q não precisa de comprar nada e terminar dialogo
                    await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.EnoughIngredientsStorage);

                    recipeState.CleanState();
                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                    await stepContext.CancelAllDialogsAsync();
                    return await stepContext.EndDialogAsync();
                }
            }
            else if (recipeState.OrderPizza.HasValue &&
                recipeState.OrderPizza.Value)
            {
                // encomendou pizza
                await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.ProceedLogin);

                return await stepContext.PromptAsync(DialogPromptIds.ConfirmTotalPrice, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(
                        stepContext.Context,
                        stepContext.Context.Activity.Locale,
                        RecipeResponses.ReceitasRespondeIds.ConfirmTotalPrice,
                        recipeState.TotalPrice),
                });
            }
            else
            {
                await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.UpdateCartLaterUse);

                // finalizar conversa
                recipeState.CleanState();
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                var evento = Activity.CreateEndOfConversationActivity();
                await stepContext.Context.SendActivityAsync(evento);
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> DisplayPurchaseCompleted(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            bool? resultConfirmOrder = null;

            if (!(recipeState.DoNotOrderFood.HasValue &&
                recipeState.DoNotOrderFood.Value))
            {
                //return await stepContext.NextAsync();
                resultConfirmOrder = ParseSpokenBool(stepContext);
            }

            if (resultConfirmOrder.HasValue &&
                recipeState.CartId.HasValue)
            {
                recipeState.ConfirmOrder = resultConfirmOrder.Value;
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                new UStoreDBManagement(_context).FinalizeCartTransaction(recipeState.CartId.Value, resultConfirmOrder.Value);

                if (resultConfirmOrder.Value)
                {
                    // finalizar compra na BD
                    await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.EndPurchase);

                    recipeState.CartId = null;
                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                }
                else
                {
                    // marcar carrinho como inactivo
                    await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.EndDialog);
                }
            }

            // marcar evento
            return await stepContext.PromptAsync(DialogPromptIds.DinnerDatePrompt, new PromptOptions()
            {
                Prompt = await _responder.RenderTemplate(
                           stepContext.Context,
                           stepContext.Context.Activity.Locale,
                           RecipeResponses.ReceitasRespondeIds.AskDinnerDate),
            });
        }

        private async Task<DialogTurnResult> PlanEventAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            var resolution = (stepContext.Result as IList<DateTimeResolution>)?.FirstOrDefault();

            if (resolution != null)
            {
                var time = resolution.Value ?? resolution.Start;

                DateTime dt = DateTime.ParseExact(time, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                if (dt.Year < DateTime.Now.Year)
                {
                    dt = new DateTime(DateTime.Now.Year, dt.Month, dt.Day);
                }

                recipeState.DinnerTime = dt;
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                return await stepContext.PromptAsync(DialogPromptIds.ConfirmDatePrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(
                           stepContext.Context,
                           stepContext.Context.Activity.Locale,
                           RecipeResponses.ReceitasRespondeIds.ConfirmDinnerDate,
                           dt.ToShortDateString()),
                });
            }
            else
            {
                // supostamente não vamos passar aqui porque temos o método de validação que já faz este trabalho
                await stepContext.Context.SendActivityAsync("I didn't understand that date.", "I didn't understand that date.", InputHints.ExpectingInput);
                return await stepContext.NextAsync(); // aqui deviamos de retornar ao prompt anterior. rever código
            }
        }

        private async Task<DialogTurnResult> ConfirmDateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var resultConfirmDate = ParseSpokenBool(stepContext);
            var context = stepContext.Context;
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (resultConfirmDate.HasValue)
            {

                if (resultConfirmDate.Value &&
                    recipeState.DinnerTime.HasValue)
                {
                    await SendMailApointment(recipeState, recipeState.DinnerTime.Value);
                    await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.AppointmentConfirmation);

                    recipeState.CleanState();
                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                    //await stepContext.CancelAllDialogsAsync();
                    return await stepContext.EndDialogAsync();
                }
                else
                {
                    // ir para dialogo anterior e voltar a pedir data
                    stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 3;
                    recipeState.DinnerTime = null;
                    await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                    return await stepContext.NextAsync();
                }
            }

            await stepContext.RepromptDialogAsync(cancellationToken);
            return await stepContext.NextAsync();
        }

        private async Task<bool> ValidateRecipe(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var value = promptContext.Recognized.Value?.Trim() ?? string.Empty;
            var recipe = new UStoreDBManagement(_context).GetRecipe(value);

            if (recipe != null)
            {
                promptContext.Recognized.Value = recipe.Name;
                return true;
            }
            else
            {
                await _responder.ReplyWith(promptContext.Context, RecipeResponses.ReceitasRespondeIds.RecipeNotAvailable);
                return false;
            }
        }

        private async Task<bool> ValidateOptionRecipe(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            string msgError = "I did not understand that option. Please repeat.";
            var value = promptContext.Recognized.Value?.Trim() ?? string.Empty;
            Console.Write(value);
            string numberValue = Regex.Match(value, @"\d+").Value;
            Console.Write(numberValue);

            if (!string.IsNullOrEmpty(numberValue))
            {
                // try to get option
                if (int.TryParse(numberValue, out int x))
                {
                    if (x > 3)
                    {
                        await promptContext.Context.SendActivityAsync(msgError, msgError, InputHints.ExpectingInput);
                        return false;
                    }

                    promptContext.Recognized.Value = x.ToString();

                    Console.Write(x.ToString());
                    return true;
                }
            }

            if (value.Contains("one") || value.Contains("One"))
            {
                promptContext.Recognized.Value = "1";
                return true;
            }

            if (value.Contains("two") || value.Contains("Two"))
            {
                promptContext.Recognized.Value = "2";
                return true;
            }

            if (value.Contains("Three") || value.Contains("three"))
            {
                promptContext.Recognized.Value = "3";
                return true;
            }

            await promptContext.Context.SendActivityAsync(msgError, msgError, InputHints.ExpectingInput);
            return false;
        }

        private async Task<bool> DateValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check whether the input could be recognized as an integer.
            if (!promptContext.Recognized.Succeeded)
            {
                await promptContext.Context.SendActivityAsync(
                    "I'm sorry, I did not understand. Please enter the date or time for your reservation.",
                    "I'm sorry, I did not understand. Please enter the date or time for your reservation.",
                    InputHints.ExpectingInput);
                return false;
            }

            return true;
        }

        private async Task<bool> SendMailApointment(RecipeState recipeState, DateTime timeDinner)
        {
            HttpClient client = new HttpClient();

            string strEmailBody = string.Empty;

            if (recipeState.OrderedIngredientsQuantities != null &&
                recipeState.OrderedIngredientsQuantities.Count > 0)
            {
                strEmailBody = "Hello there!\n\n" +
                    "Thank you for choosing UniShop to help you with your dinner arrangements.\n";

                if (recipeState.PeopleForDinner.HasValue)
                {
                    strEmailBody = string.Concat(strEmailBody, string.Format("We ordered food for you and your {0} friends.\n", recipeState.PeopleForDinner.Value));
                }

                if (recipeState.OrderPizza.HasValue && recipeState.OrderPizza.Value)
                {
                    strEmailBody = string.Concat(strEmailBody, "You ordered pizza for the dinner.\n\n");
                }
                else
                {
                    string straux = string.Format("\nYou ordered the following ingredients to cook the dish '{0}':\n", recipeState.Name);
                    strEmailBody = string.Concat(strEmailBody, straux);
                    foreach (var keyValuePairLoop in recipeState.OrderedIngredientsQuantities)
                    {
                        var ingredient = new UStoreDBManagement(_context).GetIngredientById(keyValuePairLoop.Key);
                        if (ingredient != null)
                        {
                            if (keyValuePairLoop.Value > 1)
                            {
                                straux = string.Format("{0} {1}s\n", keyValuePairLoop.Value, ingredient.Name);
                                strEmailBody = string.Concat(strEmailBody, straux);
                            }
                            else
                            {
                                straux = string.Format("{0} {1}\n", keyValuePairLoop.Value, ingredient.Name);
                                strEmailBody = string.Concat(strEmailBody, straux);
                            }
                        }
                    }
                }

                strEmailBody = string.Concat(strEmailBody, "\nHope you have a nice dinner!\n\nUniShop");
            }

            DateTime dtaux = new DateTime(timeDinner.Year, timeDinner.Month, timeDinner.Day, 20, 0, 0);
            DateTime dtaux2 = new DateTime(timeDinner.Year, timeDinner.Month, timeDinner.Day, 23, 0, 0);

            string bodyAppointment = string.Empty;
            if (recipeState.PeopleForDinner.HasValue)
            {
                bodyAppointment = string.Format("Dinner with {0} friends.", recipeState.PeopleForDinner.Value.ToString());
            }

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "createEmail", !string.IsNullOrEmpty(strEmailBody) ? "true" : "false"},
                { "emailTo", "unipartnerbotdemo@outlook.com" },
                { "emailSubject", "UniShop - Order" },
                { "emailBody", strEmailBody },
                { "createAppointment", "true" },
                { "appointmentStartDate", dtaux.ToString("MM/dd/yyyy HH:mm:ss") },
                { "appointmentEndDate", dtaux2.ToString("MM/dd/yyyy  HH:mm:ss") },
                { "appointmentTitle", "Dinner with friends (by UniShop)" },
                { "appointmentBody", bodyAppointment },
            };

            string json = JsonConvert.SerializeObject(data);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpResponseMessage response = await client.PostAsync(logicApp, httpContent))
            {
            }

            return true;
        }

        private bool? ParseSpokenBool(WaterfallStepContext stepContext)
        {
            bool? blnResult = null;

            var result = stepContext.Result as bool?;

            if (result != null && result.HasValue)
            {
                blnResult = result.Value;
            }
            else
            {
                var result2 = stepContext.Result as string;
                result2.ToLower();

                if (!string.IsNullOrEmpty(result2))
                {
                    if (result2.Contains("yes") || result2.Contains("Yes") || result2.Contains("ok") || result2.Contains("yeah") || result2.Contains("cool"))
                    {
                        blnResult = true;
                    }
                    else if (result2.Contains("No") || result2.Contains("no") || result2.Contains("nope"))
                    {
                        blnResult = false;
                    }
                }
            }

            return blnResult;
        }
    }


    // comentar mais tarde
    /*
    private async Task<DialogTurnResult> DisplayStorageRoomItemsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var resultConfirmationOrderRecipeDialog = stepContext.Result as bool?;
        var context = stepContext.Context;

        if (resultConfirmationOrderRecipeDialog.HasValue)
        {
            var recipeState = await UserProfileAccessor.GetAsync(stepContext.Context);
            recipeState.UserConfirmRecipe = resultConfirmationOrderRecipeDialog.Value;
            await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

            if (recipeState.UserConfirmRecipe)
            {
                // se seleccionou sim apresentar produtos em dispensa e perguntar se quer utilizar algum
                var storeRoomItems = new UStoreDBManagement(_context).GetStoreRoomItems();

                if (storeRoomItems != null && storeRoomItems.Count > 0)
                {
                    // await context.SendActivityAsync("Tem os seguintes ingredientes na sua despensa que vão ser usados na recipeLoop seleccionada:");
                    await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.AvailabeStorageIngredients);

                    if (recipeState.StoredIngredientsQuantities == null)
                    {
                        recipeState.StoredIngredientsQuantities = new Dictionary<int, int>();
                        await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                    }

                    foreach (var storeRoomItemLoop in storeRoomItems)
                    {
                        var ingredientAux = new UStoreDBManagement(_context).GetIngredientById(storeRoomItemLoop.Ingredient.IngredientId);

                        recipeState.StoredIngredientsQuantities.Add(storeRoomItemLoop.Ingredient.IngredientId, storeRoomItemLoop.Quantity);
                        await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);

                        if (ingredientAux != null)
                        {
                            string strMsgAux = string.Empty;
                            if (storeRoomItemLoop.Quantity > 1)
                            {
                                strMsgAux = storeRoomItemLoop.Quantity + " " + ingredientAux.Name + "s";
                            }
                            else
                            {
                                strMsgAux = storeRoomItemLoop.Quantity + " " + ingredientAux.Name;
                            }

                            await context.SendActivityAsync(strMsgAux, strMsgAux, InputHints.IgnoringInput);
                        }
                    }
                }
                else
                {
                    await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.NoAvailableStorageIngredients);
                }

                return await stepContext.PromptAsync(DialogPromptIds.ConfirmRecipePrompt, new PromptOptions()
                {
                    Prompt = await _responder.RenderTemplate(
                        stepContext.Context,
                        stepContext.Context.Activity.Locale,
                        RecipeResponses.ReceitasRespondeIds.ConfirmAddStorageItemsPrompt),
                });
            }
            else
            {
                await _responder.ReplyWith(stepContext.Context, RecipeResponses.ReceitasRespondeIds.EndDialog);
                recipeState.CleanState();
                await UserProfileAccessor.SetAsync(stepContext.Context, recipeState);
                await stepContext.CancelAllDialogsAsync();
                return await stepContext.EndDialogAsync();
            }
        }
        else
        {
            throw new Exception("não recebemos resposta ao pedido de confirmação"); // penso q nunca devemos de passar por aqui
        }
    }
    */

}
