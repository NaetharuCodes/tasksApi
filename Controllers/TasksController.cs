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
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks()
    {
        var tasks = await _context.Tasks.ToListAsync();
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
}
