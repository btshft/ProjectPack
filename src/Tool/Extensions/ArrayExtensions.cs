using System;

namespace PackProject.Tool.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Extend<T>(this T[] source, T[] target)
        {
            var originalLength = source.Length;

            Array.Resize(ref source, source.Length + target.Length);
            Array.Copy(target, 0, source, originalLength, target.Length);

            return source;
        }
    }
}