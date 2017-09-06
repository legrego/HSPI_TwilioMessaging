using System;
using System.Text;

namespace Hspi
{
    using Hspi.Exceptions;

    internal static class ExceptionHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
        public static string GetFullMessage(this Exception ex)
        {
            switch (ex)
            {
                case AggregateException aggregationException:
                    var stb = new StringBuilder();

                    foreach (var innerException in aggregationException.InnerExceptions)
                    {
                        stb.AppendLine(GetFullMessage(innerException));
                    }

                    return stb.ToString();

               // case ApiKeyInvalidException apiKeyInvalidException:
               //     return Invariant($"Invalid API Key. Check Configuration.");

                default:
                    return ex.Message;
            }
        }
    };
}