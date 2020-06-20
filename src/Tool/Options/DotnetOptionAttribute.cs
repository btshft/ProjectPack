using System;

namespace PackProject.Tool.Options
{
    public class DotnetOptionAttribute : Attribute
    {
        public DotnetOptionAttribute(string @alias)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }
}