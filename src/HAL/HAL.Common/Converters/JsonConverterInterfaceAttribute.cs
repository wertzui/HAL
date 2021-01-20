namespace System.Text.Json.Serialization
{
    /// <summary>
    /// This is just a workaround because <see cref="JsonConverterAttribute"/> does not allow itself to be places on interfaces.
    /// If we would not use this one, The _embedded Collection could not use the IResource interface, but would have to use the Resource class.
    /// See https://github.com/dotnet/runtime/issues/33112 for more information.
    /// </summary>
    /// <seealso cref="JsonConverterAttribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class JsonInterfaceConverterAttribute : JsonConverterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonInterfaceConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">The type of the converter.</param>
        public JsonInterfaceConverterAttribute(Type converterType)
            : base(converterType)
        {
        }
    }
}