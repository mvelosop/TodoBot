using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Domain.Model;

namespace TodoApp.Services
{
    public interface ITodoTaskServices
    {
        Task AddTaskAsync(TodoTask task);

        Task<List<TodoTask>> GetTasksAsync();
    }
}