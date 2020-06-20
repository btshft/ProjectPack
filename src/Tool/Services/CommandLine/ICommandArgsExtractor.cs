namespace PackProject.Tool.Services.CommandLine
{
    public interface ICommandArgsExtractor
    {
        string GetProjectPath();
        string[] GetParameters();
        string[] GetAll();
    }
}