// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using UStoreBot.Dialogs;
using UStoreBot.Dialogs.Recipe;
using UStoreBot.Models;

namespace UStoreBot
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class UStoreBot : IBot
    {
        // Supported LUIS Intents
        public const string GreetingIntent = "Greeting";
        public const string CancelIntent = "Cancel";
        public const string HelpIntent = "Help";
        public const string NoneIntent = "None";
        public const string RecipesIntent = "Recipe";
        private const string NoIntentMsg = "I didn't understand what you just said to me.";
        private const string CanceledActivityMsg = "Ok. I've canceled our last activity.";
        private const string NoActivityToCancelMsg = "I don't have anything to cancel.";
        // private const string TryProvideHelpMsg = "Let me try to provide some help.";
        // private const string UnderstandCommandsMsg = "I understand greetings, being asked for help, or being asked to cancel what I am doing.";

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisConfiguration = "UStoreBotLuisApplication";

        private readonly IStatePropertyAccessor<GreetingState> _greetingStateAccessor;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly IStatePropertyAccessor<RecipeState> _recipeStateAccessor;

        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;

        private readonly UStoreDBContext _context;
        /// <summary>
        /// Initializes a new instance of the <see cref="UStoreBot"/> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        public UStoreBot(BotServices services, UserState userState, ConversationState conversationState, ILoggerFactory loggerFactory, UStoreDBContext ucontext)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            _greetingStateAccessor = _userState.CreateProperty<GreetingState>(nameof(GreetingState));
            _recipeStateAccessor = _userState.CreateProperty<RecipeState>(nameof(RecipeState));
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _context = ucontext;

            // Verify LUIS configuration.
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

            Dialogs = new DialogSet(_dialogStateAccessor);
            Dialogs.Add(new GreetingDialog(_greetingStateAccessor, loggerFactory));
            Dialogs.Add(new RecipeDialog(_recipeStateAccessor, loggerFactory, _context));
        }

        private DialogSet Dialogs { get; set; }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // Create a dialog context
            var dc = await Dialogs.CreateContextAsync(turnContext);

            if (activity.Type == ActivityTypes.Message)
            {
                // if (!string.IsNullOrEmpty(activity.Text))
                //{
                // Perform a call to LUIS to retrieve results for the current activity message.
                var abc = _services.LuisServices[LuisConfiguration];



                var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(dc.Context, cancellationToken);

                // If any entities were updated, treat as interruption.
                // For example, "no my name is tony" will manifest as an update of the name to be "tony".
                var topScoringIntent = luisResults?.GetTopScoringIntent();

                var topIntent = topScoringIntent.Value.intent;

                // update greeting state with any entities captured
                await UpdateGreetingState(luisResults, dc.Context);
                await UpdateRecipeState(luisResults, dc.Context);
                // }

                // Handle conversation interrupts first.
                var interrupted = await IsTurnInterruptedAsync(dc, topIntent);
                if (interrupted)
                {
                    // Bypass the dialog.
                    // Save state before the next turn.
                    await _conversationState.SaveChangesAsync(turnContext);
                    await _userState.SaveChangesAsync(turnContext);
                    return;
                }

                // Continue the current dialog
                var dialogResult = await dc.ContinueDialogAsync();

                // if no one has responded,
                if (!dc.Context.Responded)
                {
                    // examine results from active dialog
                    switch (dialogResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                            switch (topIntent)
                            {
                                //case GreetingIntent:
                                //    await dc.BeginDialogAsync(nameof(GreetingDialog));
                                //    break;
                                case RecipesIntent:
                                    await dc.BeginDialogAsync(nameof(RecipeDialog));
                                    break;
                                case NoneIntent:
                                default:
                                    // Help or no intent identified, either way, let's provide some help.
                                    // to the user
                                    await dc.Context.SendActivityAsync(NoIntentMsg, NoIntentMsg, InputHints.IgnoringInput);
                                    break;
                            }

                            break;

                        case DialogTurnStatus.Waiting:
                            // The active dialog is waiting for a response from the user, so do nothing.
                            break;

                        case DialogTurnStatus.Complete:
                            await dc.EndDialogAsync();
                            break;

                        default:
                            await dc.CancelAllDialogsAsync();
                            break;
                    }
                }
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded != null)
                {
                    // Iterate over all new members added to the conversation.
                    foreach (var member in activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message.
                        // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                        if (member.Id != activity.Recipient.Id)
                        {
                            //var welcomeCard = CreateAdaptiveCardAttachment();
                            //var response = CreateResponse(activity, welcomeCard);
                            //await dc.Context.SendActivityAsync(response);
                            await dc.Context.SendActivityAsync("Hi");
                        }
                    }
                }
            }

            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        // Determine if an interruption has occurred before we dispatch to any active dialog.
        private async Task<bool> IsTurnInterruptedAsync(DialogContext dc, string topIntent)
        {
            // See if there are any conversation interrupts we need to handle.
            //if (topIntent.Equals(CancelIntent))
            //{
            //    if (dc.ActiveDialog != null)
            //    {
            //        await dc.CancelAllDialogsAsync();
            //        await dc.Context.SendActivityAsync(CanceledActivityMsg, CanceledActivityMsg, InputHints.IgnoringInput);
            //    }
            //    else
            //    {
            //        await dc.Context.SendActivityAsync(NoActivityToCancelMsg, NoActivityToCancelMsg, InputHints.IgnoringInput);
            //    }

            //    return true;        // Handled the interrupt.
            //}

            //if (topIntent.Equals(HelpIntent))
            //{
            //    await dc.Context.SendActivityAsync(TryProvideHelpMsg, TryProvideHelpMsg, InputHints.IgnoringInput);
            //    await dc.Context.SendActivityAsync(UnderstandCommandsMsg, UnderstandCommandsMsg, InputHints.IgnoringInput);
            //    if (dc.ActiveDialog != null)
            //    {
            //        await dc.RepromptDialogAsync();
            //    }

            //    return true;        // Handled the interrupt.
            //}

            return false;           // Did not handle the interrupt.
        }

        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        /*
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Dialogs\Welcome\Resources\welcomeCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        */

        /// <summary>
        /// Helper function to update greeting state with entities returned by LUIS.
        /// </summary>
        /// <param name="luisResult">LUIS recognizer <see cref="RecognizerResult"/>.</param>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task UpdateGreetingState(RecognizerResult luisResult, ITurnContext turnContext)
        {
            if (luisResult.Entities != null && luisResult.Entities.HasValues)
            {
                // Get latest GreetingState
                var greetingState = await _greetingStateAccessor.GetAsync(turnContext, () => new GreetingState());
                var entities = luisResult.Entities;

                // Supported LUIS Entities
                string[] userNameEntities = { "userName", "userName_patternAny" };
                string[] userLocationEntities = { "userLocation", "userLocation_patternAny" };

                // Update any entities
                // Note: Consider a confirm dialog, instead of just updating.
                foreach (var name in userNameEntities)
                {
                    // Check if we found valid slot values in entities returned from LUIS.
                    if (entities[name] != null)
                    {
                        // Capitalize and set new user name.
                        var newName = (string)entities[name][0];
                        greetingState.Name = char.ToUpper(newName[0]) + newName.Substring(1);
                        break;
                    }
                }

                foreach (var city in userLocationEntities)
                {
                    if (entities[city] != null)
                    {
                        // Capitalize and set new city.
                        var newCity = (string)entities[city][0];
                        greetingState.City = char.ToUpper(newCity[0]) + newCity.Substring(1);
                        break;
                    }
                }

                // Set the new values into state.
                await _greetingStateAccessor.SetAsync(turnContext, greetingState);
            }
        }

        private async Task UpdateRecipeState(RecognizerResult luisResult, ITurnContext turnContext)
        {
            if (luisResult.Entities != null && luisResult.Entities.HasValues)
            {
                var recipeState = await _recipeStateAccessor.GetAsync(turnContext, () => new RecipeState());
                var entities = luisResult.Entities;

                // Supported LUIS Entities
                if (entities["recipeName"] != null)
                {
                    var newRecipe = (string)entities["recipeName"][0];

                    // validar se esta receita existe na bd ou devolver a mais próxima
                    var recipe = new UStoreDBManagement(_context).GetRecipeByAproxName(newRecipe);

                    if (recipe != null)
                    {
                        recipeState.DoNotOrderFood = false;
                        recipeState.Name = newRecipe;
                    }
                }

                if (entities["numberOfPeopleForDinner"] != null)
                {
                    var numPeople = (int)entities["numberOfPeopleForDinner"][0];

                    // validar se esta receita existe na bd ou devolver a mais próxima
                    recipeState.PeopleForDinner = numPeople;
                }

                await _recipeStateAccessor.SetAsync(turnContext, recipeState);
            }
        }
    }
}