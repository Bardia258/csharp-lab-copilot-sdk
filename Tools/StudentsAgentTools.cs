using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using AI4SchoolAgent.Data;
using AI4SchoolAgent.Models;

namespace AI4SchoolAgent.Tools;

public class StudentsAgentTools
{
    private readonly SchoolDbContext _context;

    public StudentsAgentTools(SchoolDbContext context)
    {
        _context = context;
    }

    [Description("Gets all students from the database")]
    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _context.Students.ToListAsync();
    }

    [Description("Gets students belonging to a specific school")]
    public async Task<List<Student>> GetStudentsBySchoolAsync(
        string school)
    {
        return await _context.Students
            .Where(s => s.School == school)
            .ToListAsync();
    }

    [Description("Gets the total number of students")]
    public async Task<int> GetStudentCountAsync()
    {
        return await _context.Students.CountAsync();
    }
}