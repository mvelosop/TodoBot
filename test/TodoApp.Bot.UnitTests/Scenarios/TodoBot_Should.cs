using FluentAssertions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Domain.Model;
using TodoApp.Services;
using Xunit;

namespace TodoApp.Bot.UnitTests.Scenarios
{
    public class TodoBot_Should
    {
        readonly List<TodoTask> _tasks = new List<TodoTask>();

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
                .Send("/add")
                .AssertReply("Enter the task name")
                .Send("foo")
                .AssertReply("What's the due date?")
                .Send("Tomorrow")
                .AssertReply(activity =>
                    activity.AsMessageActivity().Text.Should().StartWith(@"Added ""foo"" due on"));

            var expectedDueDate = DateTime.Today.AddDays(1).Date;

            // Act / Assert ------------
            await testFlow.StartTestAsync();

            _tasks.Should().BeEquivalentTo(new[] { new TodoTask { Name = "foo", DueDate = expectedDueDate } });
        }

        [Fact]
        public async Task ListTasks_WhenListCommand()
        {
            _tasks.Add(new TodoTask { Name = "foo", DueDate = new DateTime(2018, 01, 01) });
            _tasks.Add(new TodoTask { Name = "bar", DueDate = new DateTime(2018, 02, 01) });

            // Arrange -----------------
            var testFlow = CreateTestFlow()
                .Send("/list")
                .AssertReply("- foo (2018-01-01)\n- bar (2018-02-01)");

            // Act / Assert ------------
            await testFlow.StartTestAsync();
        }

        private TestFlow CreateTestFlow()
        {
            var storage = new MemoryStorage();
            var conversationState = new ConversationState(storage);
            var adapter = new TestAdapter().Use(new AutoSaveStateMiddleware(conversationState));

            var accessors = new TodoBotAccessors(conversationState)
            {
                DialogState = conversationState.CreateProperty<DialogState>(TodoBotAccessors.DialogStateKey)
            };

            var loggerFactoryMock = CreateLoggerFactoryMock();
            var servicesMock = CreateServicesMock();

            var bot = new TodoBot(accessors, loggerFactoryMock, servicesMock);
            var testFlow = new TestFlow(adapter, bot.OnTurnAsync);

            return testFlow;
        }

        private ITodoTaskServices CreateServicesMock()
        {
            var mock = new Mock<ITodoTaskServices>();

            mock.Setup(m => m.AddTaskAsync(It.IsAny<TodoTask>()))
                .Callback<TodoTask>(t => _tasks.Add(t))
                .Returns(Task.CompletedTask);

            mock.Setup(m => m.GetTasksAsync()).ReturnsAsync(_tasks);

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
