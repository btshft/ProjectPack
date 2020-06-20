namespace PackProject.Tool.Helpers
{
    public static class MsBuild
    {
        public static string Property(string name, string value)
        {
            return value != null ? $"/p:{name}={value}" : null;
        }

        public static string Target(params string[] targets)
        {
            return $"/t:{string.Join(',', targets)}";
        }
    }
}