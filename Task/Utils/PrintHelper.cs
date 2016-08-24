using System.Collections.Generic;

namespace Task.Utils
{
    public static class PrintHelper
    {
        public static void PrintItems<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                ObjectDumper.Write(item);
            }
        }
    }
}
