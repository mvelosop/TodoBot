using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Domain.Model;

namespace TodoApp.Services
{
    public interface ITodoTaskServices
    {
        void AddTask(TodoTask task);

        Task<List<TodoTask>> GetTasks();
    }
}