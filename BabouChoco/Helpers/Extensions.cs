using System;

namespace BabouChoco.Helpers
{
    public class Extensions
    {
        
    }
    public static class ArgumentValidationExtensions
    {
        public static T ThrowIfNullOrEmpty<T>(this T o, string paramName) where T : class
        {
            if (o == null)
                throw new ArgumentNullException(paramName);
            if(o is string  && string.IsNullOrWhiteSpace(Convert.ToString(o)))
                throw new ArgumentException($"{paramName} cannot be empty.");

            return o;
        }
    }
}