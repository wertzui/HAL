namespace HAL.AspNetCore.Utils
{
    /// <summary>
    /// Helper for generation action method names.
    /// </summary>
    public class ActionHelper
    {
        /// <summary>
        /// Removes the Async suffix from the action method name if it exists.
        /// </summary>
        /// <param name="actionMethod">The name of the method with or without Async suffix.</param>
        /// <returns>The name of the method without Async suffix.</returns>
        public static string StripAsyncSuffix(string actionMethod) => actionMethod.EndsWith("Async", System.StringComparison.Ordinal) ? actionMethod[0..^5] : actionMethod;
    }
}
