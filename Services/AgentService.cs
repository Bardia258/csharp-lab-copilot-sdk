using GitHub.Copilot;
using Microsoft.Extensions.AI;
using AI4SchoolAgent.Tools;

namespace AI4SchoolAgent.Services;

public class AgentService
{
    private readonly StudentsAgentTools _tools;

    public AgentService(StudentsAgentTools tools)
    {
        _tools = tools;
    }

    private CopilotClient CreateClient()
    {
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "GITHUB_TOKEN environment variable is missing.");
        }

        return new CopilotClient(new CopilotClientOptions
        {
            GitHubToken = token,
            UseLoggedInUser = false
        });
    }

    public async Task<List<string>> GetModelsAsync()
    {
        await using var client = CreateClient();
        await client.StartAsync();

        var models = await client.ListModelsAsync();

        Console.WriteLine("Available models:");

        foreach (var model in models)
        {
            Console.WriteLine($"{model.Id} - {model.Name}");
        }

        return models.Select(m => m.Id).ToList();
    }

    public async Task<string> AskAsync(
        string question, string model = "auto")
    {
        await using var client = CreateClient();
        await client.StartAsync();

        Console.WriteLine($"Selected model: {model}");
        Console.WriteLine("Using GitHub PAT authentication");

        await using var session = await client.CreateSessionAsync(
            new SessionConfig
            {
                Model = model,
                OnPermissionRequest = PermissionHandler.ApproveAll,

                AvailableTools = [
                    "get_students",
                    "get_students_by_school",
                    "get_student_count"
                ],

                Tools = [
                    AIFunctionFactory.Create(
                        _tools.GetStudentsAsync,
                        "get_students"),

                    AIFunctionFactory.Create(
                        _tools.GetStudentsBySchoolAsync,
                        "get_students_by_school"),

                    AIFunctionFactory.Create(
                        _tools.GetStudentCountAsync,
                        "get_student_count")
                ]
            });

        var response = await session.SendAndWaitAsync(
            new MessageOptions
            {
                Prompt = question
            });

        return response?.Data.Content ?? "No response received.";
    }
}