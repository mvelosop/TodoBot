using System;

namespace TodoApp.Domain.Model
{
    public class TodoTask
    {
        public DateTime? DueDate { get; set; }

        public string Name { get; set; }

        public TodoTaskStatus Status { get; set; }
    }
}