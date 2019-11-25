using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using UStoreBot.Dialogs.Recipe.Resources;

namespace UStoreBot.Dialogs.Recipe
{
    public class RecipeResponses : TemplateManager
    {
        public RecipeResponses()
        {
            Register(new DictionaryRenderer(_responseTemplates));
        }

        public class ReceitasRespondeIds
        {
            public const string AvailableRecipes = "availableRecipes";
            public const string NecessaryIngredients = "necessaryIngredients";
            public const string RecipePrompt = "recipePrompt";
            public const string RecipeIngredientsNotFound = "recipeIngredientsNotFound";
            public const string OrderNecessaryIngredients = "orderNecessaryIngredients";
            public const string RecipeNotAvailable = "recipeNotAvailable";
            public const string ConfirmAddStorageItemsPrompt = "confirmAddStorageItemsPrompt";
            public const string AvailabeStorageIngredients = "availabeStorageIngredients";
            public const string NoAvailableStorageIngredients = "noAvailableStorageIngredients";
            public const string NumberOfPeopleForDinner = "numberOfPeopleForDinner";
            public const string EnoughIngredientsStorage = "enoughIngredientsStorage";
            public const string ConfirmTotalPrice = "confirmTotalPrice";
            public const string EndDialog = "endDialog";
            public const string EndPurchase = "endPurchase";
            public const string InitialDialog = "initialDialog";
            public const string RecipeOfTheWeekDialog = "recipeOfTheWeekDialog";
            public const string FollowingIngredientsInMind = "followingIngredientsInMind";
            public const string SugestedRecipes = "sugestedRecipes";
            public const string UpdateCartLaterUse = "updateCartLaterUse";
            public const string ProceedLogin = "proceedLogin";
            public const string OrderPizza = "orderPizza";
            public const string OnlyMeetingRequest = "onlyMeetingRequest";
            public const string AskDinnerDate = "askDinnerDate";
            public const string ConfirmDinnerDate = "confirmDinnerDate";
            public const string AppointmentConfirmation = "appointmentConfirmation";
            public const string AskForUserInputIngredient = "askForUserInputIngredient";
            public const string AskUserOrderPizza = "askUserOrderPizza";
            public const string WhichRecipePreferes = "whichRecipePreferes";
        }

        private static LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                { ReceitasRespondeIds.AvailableRecipes,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.AVAILABLE_RECIPES,
                        ssml: RecipeDialogStrings.AVAILABLE_RECIPES,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.RecipePrompt,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.RECIPE_PROMPT,
                        ssml: RecipeDialogStrings.RECIPE_PROMPT,
                        inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.NecessaryIngredients,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.NECESSARY_INGREDIENTS,
                        ssml: RecipeDialogStrings.NECESSARY_INGREDIENTS,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.RecipeIngredientsNotFound,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.INGREDIENTS_NOT_FOUND,
                        ssml: RecipeDialogStrings.INGREDIENTS_NOT_FOUND,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.OrderNecessaryIngredients,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.ORDER_NECESSARY_INGREDIENTS,
                        ssml: RecipeDialogStrings.ORDER_NECESSARY_INGREDIENTS,
                        inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.ConfirmAddStorageItemsPrompt,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.USE_STORAGE_INGREDIENTS,
                        ssml: RecipeDialogStrings.USE_STORAGE_INGREDIENTS,
                        inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.AvailabeStorageIngredients,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.AVAILABLE_STORAGE_INGREDIENTS,
                        ssml: RecipeDialogStrings.AVAILABLE_STORAGE_INGREDIENTS,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.NoAvailableStorageIngredients,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.NO_AVAILABLE_STORAGE_INGREDIENTS,
                        ssml: RecipeDialogStrings.NO_AVAILABLE_STORAGE_INGREDIENTS,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.RecipeNotAvailable,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.RECIPE_NOT_AVAILABLE,
                        ssml: RecipeDialogStrings.RECIPE_NOT_AVAILABLE,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.NumberOfPeopleForDinner,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.NUMBER_OF_PEOPLE,
                        ssml: RecipeDialogStrings.NUMBER_OF_PEOPLE,
                        inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.EnoughIngredientsStorage,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.ENOUGH_INGREDIENTS,
                        ssml: RecipeDialogStrings.ENOUGH_INGREDIENTS,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.ConfirmTotalPrice,
                    (context, data) =>
                    MessageFactory.Text(
                        text: string.Format(RecipeDialogStrings.CONFIRM_TOTAL_PRICE, data),
                        ssml: string.Format(RecipeDialogStrings.CONFIRM_TOTAL_PRICE, data),
                        inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.EndDialog,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.END_DIALOG,
                        ssml: RecipeDialogStrings.END_DIALOG,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.EndPurchase,
                    (context, data) =>
                    MessageFactory.Text(
                        text: RecipeDialogStrings.END_PURCHASE,
                        ssml: RecipeDialogStrings.END_PURCHASE,
                        inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.InitialDialog,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.INITIAL_DIALOG,
                       ssml: RecipeDialogStrings.INITIAL_DIALOG,
                       inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.RecipeOfTheWeekDialog,
                   (context, data) =>
                   MessageFactory.Text(
                       text: string.Format(RecipeDialogStrings.RECIPE_OF_THE_WEEK, data.Name, data.discount, data.ingredientName),
                       ssml: string.Format(RecipeDialogStrings.RECIPE_OF_THE_WEEK, data.Name, data.discount, data.ingredientName),
                       inputHint: InputHints.ExpectingInput)
               },
                { ReceitasRespondeIds.FollowingIngredientsInMind,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.FOLLOWING_INGREDIENTS,
                       ssml: RecipeDialogStrings.FOLLOWING_INGREDIENTS,
                       inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.SugestedRecipes,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.RECIPES_SUGESTED,
                       ssml: RecipeDialogStrings.RECIPES_SUGESTED,
                       inputHint: InputHints.IgnoringInput)
                },
                //{ ReceitasRespondeIds.SugestedRecipes,
                //   (context, data) =>
                //   MessageFactory.Text(
                //       text: string.Format(RecipeDialogStrings.RECIPES_SUGESTED, data.Name, data.Name2, data.Name3),
                //       ssml: string.Format(RecipeDialogStrings.RECIPES_SUGESTED, data.Name, data.Name2, data.Name3),
                //       inputHint: InputHints.ExpectingInput)
                //},
                { ReceitasRespondeIds.UpdateCartLaterUse,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.UPDATE_CART_LATER_USE,
                       ssml: RecipeDialogStrings.UPDATE_CART_LATER_USE,
                       inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.ProceedLogin,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.PROCEED_LOGIN,
                       ssml: RecipeDialogStrings.PROCEED_LOGIN,
                       inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.OrderPizza,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.ORDER_PIZZA,
                       ssml: RecipeDialogStrings.ORDER_PIZZA,
                       inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.OnlyMeetingRequest,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.ONLY_MEETING_REQUEST,
                       ssml: RecipeDialogStrings.ONLY_MEETING_REQUEST,
                       inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.AskDinnerDate,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.ASK_DINNER_DATE,
                       ssml: RecipeDialogStrings.ASK_DINNER_DATE,
                       inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.ConfirmDinnerDate,
                   (context, data) =>
                   MessageFactory.Text(
                       text: string.Format(RecipeDialogStrings.CONFIRM_DINNER_DATE, data),
                       ssml: string.Format(RecipeDialogStrings.CONFIRM_DINNER_DATE, data),
                       inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.AppointmentConfirmation,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.APPOINTMENT_CONFIRMATION,
                       ssml: RecipeDialogStrings.APPOINTMENT_CONFIRMATION,
                       inputHint: InputHints.IgnoringInput)
                },
                { ReceitasRespondeIds.AskForUserInputIngredient,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.ASK_USER_INPUT_INGREDIENT,
                       ssml: RecipeDialogStrings.ASK_USER_INPUT_INGREDIENT,
                       inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.AskUserOrderPizza,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.ASK_USER_ORDER_PIZZA,
                       ssml: RecipeDialogStrings.ASK_USER_ORDER_PIZZA,
                       inputHint: InputHints.ExpectingInput)
                },
                { ReceitasRespondeIds.WhichRecipePreferes,
                   (context, data) =>
                   MessageFactory.Text(
                       text: RecipeDialogStrings.WHICH_RECIPE_PREFERES,
                       ssml: RecipeDialogStrings.WHICH_RECIPE_PREFERES,
                       inputHint: InputHints.ExpectingInput)
                },
            },
        };
    }
}
