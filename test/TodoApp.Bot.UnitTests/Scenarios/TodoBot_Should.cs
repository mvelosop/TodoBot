using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TodoApp.Bot.UnitTests.Scenarios
{
    public class TodoBot_Should
    {
        [Fact]
        public async Task GreetBackByName_WhenUserGreets()
        {
            // Arrange -----------------
            var fakeLoggerFactory = CreateFakeLoggerFactory();
            var conversationState = new ConversationState(new MemoryStorage());
            var adapter = new TestAdapter();

            var accessors = new TodoBotAccessors(conversationState)
            {
                TodoState = conversationState.CreateProperty<TodoState>(TodoBotAccessors.TodoStateName)
            };

            var bot = new TodoBot(accessors, fakeLoggerFactory);

            var testFlow = new TestFlow(adapter, bot.OnTurnAsync)
                .Send("Hi")
                .AssertReply("Hi User1");

            // Act / Assert ------------
            await testFlow.StartTestAsync();
        }

        [Fact]
        public async Task DisplayHelpText_AfterGreetingBackTheUser()
        {
            // Arrange -----------------
            var fakeLoggerFactory = CreateFakeLoggerFactory();
            var conversationState = new ConversationState(new MemoryStorage());
            var adapter = new TestAdapter();

            var accessors = new TodoBotAccessors(conversationState)
            {
                TodoState = conversationState.CreateProperty<TodoState>(TodoBotAccessors.TodoStateName)
            };

            var bot = new TodoBot(accessors, fakeLoggerFactory);
            var helpText = "**TO-DO Commands**\nType:\n**/add** to add a task\n**/list** to list all tasks"; 

            var testFlow = new TestFlow(adapter, bot.OnTurnAsync)
                .Send("Hi")
                .AssertReply("Hi User1")
                .AssertReply(helpText);

            // Act / Assert ------------
            await testFlow.StartTestAsync();
        }

        private ILoggerFactory CreateFakeLoggerFactory()
        {
            var fakeILogger = new Mock<ILogger<TodoBot>>().Object;

            var mock = new Mock<ILoggerFactory>();

            mock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(fakeILogger);

            return mock.Object;
        }
    }
}
