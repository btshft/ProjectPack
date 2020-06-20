namespace PackProject.Tool.Services.CommandLine
{
    public interface ICommandLineArgsFilter
    {
        string GetProjectPath();

        string[] GetAll();

        string[] GetBypass();

        string[] GetParameters();
    }
}