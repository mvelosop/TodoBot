// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using TodoApp.Domain.Model;
using TodoApp.Services;

namespace TodoApp.Bot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class TodoBot : IBot
    {
        private readonly TodoBotAccessors _accessors;
        private readonly ILogger _logger;
        private readonly ITodoTaskServices _services;
        private readonly DialogSet _dialogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public TodoBot(
            TodoBotAccessors accessors,
            ILoggerFactory loggerFactory,
            ITodoTaskServices services)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<TodoBot>();
            _logger.LogTrace("EchoBot turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _services = services;
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var helpText = "**TO-DO Commands**\nType:\n- **/add** to add a task\n- **/list** to list all tasks";
            var errorMessage = "I'm sorry I didn't understand that!";

            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context.
                var state = await _accessors.TodoState.GetAsync(turnContext, () => new TodoState());

                // Bump the turn count for this conversation.
                state.TurnCount++;

                // Set the property using the accessor.
                await _accessors.TodoState.SetAsync(turnContext, state);

                // Save the new turn count into the conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext);

                string responseMessage;

                if (turnContext.Activity.Text == "Hi")
                {
                    // Greet back the user by name.
                    responseMessage = $"Hi {turnContext.Activity.From.Name}";

                    await turnContext.SendActivityAsync(responseMessage);
                    await turnContext.SendActivityAsync(helpText);
                }
                else
                {
                    var input = turnContext.Activity.Text.Trim();

                    if (input.StartsWith("/add", StringComparison.OrdinalIgnoreCase))
                    {
                        var taskName = input.Substring(4).Trim();

                        _services.AddTask(new TodoTask { Name = taskName });

                        await turnContext.SendActivityAsync($@"Added task ""{taskName}"".");

                    }
                    else
                    {
                        await turnContext.SendActivityAsync(errorMessage);
                    }
                }

            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
