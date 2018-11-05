using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Domain.Model;

namespace TodoApp.Services
{
    public interface ITodoTaskServices
    {
        Task AddTask(TodoTask task);

        Task<List<TodoTask>> GetTasks();
    }
}