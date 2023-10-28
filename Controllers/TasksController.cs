using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApi.Models;
using System.Threading.Tasks;
using TaskApi.DTOs;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(TaskCreateDto taskDto)
    {
        var taskItem = new TaskItem
        {
            Desc = taskDto.Desc,
            DueDate = taskDto.DueDate,
            Completed = false
        };

        _context.Tasks.Add(taskItem);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Task created with ID: {taskItem.Id}");

        return CreatedAtAction(nameof(GetTaskById), new { id = taskItem.Id }, taskItem);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks(string filter = "all")
    {
        IQueryable<TaskItem> query = _context.Tasks;

        DateTime currentDate = DateTime.Now.Date;

        switch (filter.ToLower())
        {
            case "open":
                query = query.Where(t => !t.Completed);
                break;
            case "closed":
                query = query.Where(t => t.Completed);
                break;
            case "overdue":
                query = query.Where(t => !t.Completed && t.DueDate < currentDate);
                break;
        }

        var tasks = await query.ToListAsync();
        return tasks;
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskById(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        return task;
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int id, TaskUpdateDto taskUpdateDto)
    {
        var task = await _context.Tasks.FindAsync(id);
        _logger.LogInformation($"Task id {id} not found");

        if (task == null)
        {
            return NotFound();
        }

        task.Completed = taskUpdateDto.Completed;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Task with ID: {id} status updated to : {taskUpdateDto.Completed}");

        return NoContent();
    }
}
