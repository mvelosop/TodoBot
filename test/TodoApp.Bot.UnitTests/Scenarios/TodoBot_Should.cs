using FluentAssertions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Domain.Model;
using TodoApp.Services;
using Xunit;

namespace TodoApp.Bot.UnitTests.Scenarios
{
    public class TodoBot_Should
    {
        List<TodoTask> _tasks = new List<TodoTask>();

        [Fact]
        public async Task GreetBackByName_WhenUserGreets()
        {
            // Arrange -----------------
            var testFlow = CreateTestFlow()
                .Send("Hi")
                .AssertReply("Hi User1");

            // Act / Assert ------------
            await testFlow.StartTestAsync();
        }

        [Fact]
        public async Task DisplayHelpText_AfterGreetingBackTheUser()
        {
            // Arrange -----------------
            var helpText = "**TO-DO Commands**\nType:\n- **/add** to add a task\n- **/list** to list all tasks";

            var testFlow = CreateTestFlow()
                .Send("Hi")
                .AssertReply("Hi User1")
                .AssertReply(helpText);

            // Act / Assert ------------
            await testFlow.StartTestAsync();
        }

        [Theory]
        [InlineData(1, "foo")]
        [InlineData(2, "bar")]
        public async Task DisplayErrorMessage_WhenUnknownCommand(int test, string command)
        {
            // Arrange -----------------
            var errorMessage = "I'm sorry I didn't understand that!";

            var testFlow = CreateTestFlow()
                .Send(command)
                .AssertReply(errorMessage);

            // Act / Assert ------------
            await testFlow.StartTestAsync();
        }

        [Fact]
        public async Task AddTask_WhenAddCommand()
        {
            // Arrange -----------------
            var testFlow = CreateTestFlow()
                .Send("/add Buy milk")
                .AssertReply($@"Added task ""Buy milk"".");

            // Act / Assert ------------
            await testFlow.StartTestAsync();

            _tasks.Should().BeEquivalentTo(new[] { new TodoTask { Name = "Buy milk" } });
        }

        private TestFlow CreateTestFlow()
        {
            var storage = new MemoryStorage();

            var loggerFactoryMock = CreateLoggerFactoryMock();
            var servicesMock = CreateServicesMock();
            var conversationState = new ConversationState(storage);
            var adapter = new TestAdapter();

            var accessors = new TodoBotAccessors(conversationState)
            {
                TodoState = conversationState.CreateProperty<TodoState>(TodoBotAccessors.TodoStateName)
            };

            var bot = new TodoBot(accessors, loggerFactoryMock, servicesMock);
            var testFlow = new TestFlow(adapter, bot.OnTurnAsync);

            return testFlow;
        }

        private ITodoTaskServices CreateServicesMock()
        {
            var mock = new Mock<ITodoTaskServices>();

            mock.Setup(m => m.AddTask(It.IsAny<TodoTask>()))
                .Callback<TodoTask>(t => _tasks.Add(t));

            return mock.Object;
        }

        private ILoggerFactory CreateLoggerFactoryMock()
        {
            var loggerMock = new Mock<ILogger<TodoBot>>().Object;

            var mock = new Mock<ILoggerFactory>();

            mock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock);

            return mock.Object;
        }

    }
}
