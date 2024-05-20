using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Importer.Core.Common
{
    public static class Extensions
    {
        public static bool HasAny<T>(this IEnumerable<T> source)
        {
            return (source?.Any() ?? false);
        }
        
        public static string GetaAllMessages(this Exception exp)
        {
            return exp.ToString();
        }
    }
}