namespace HAL.Common
{
    /// <summary>
    /// The page represents the state after a call to a list endpoint that supports paging.
    /// One or both properties may be null if they are unsupported by the endpoint.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Gets or sets the current page if the endpoint supports getting a current page.
        /// </summary>
        /// <value>
        /// The current page.
        /// </value>
        public long? CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages if the endpoint supports getting a total number of pages.
        /// </summary>
        /// <value>
        /// The total pages.
        /// </value>
        public long? TotalPages { get; set; }
    }
}