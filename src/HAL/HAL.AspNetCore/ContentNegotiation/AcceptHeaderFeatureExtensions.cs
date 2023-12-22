using System.Linq;

namespace HAL.AspNetCore.ContentNegotiation
{
    /// <summary>
    /// Contains extension methods for the <see cref="IAcceptHeaderFeature"/> interface.
    /// </summary>
    public static class AcceptHeaderFeatureExtensions
    {
        /// <summary>
        /// Checks if the request accepts HAL.
        /// </summary>
        /// <param name="feature">The <see cref="IAcceptHeaderFeature"/> containing the Accept headers.</param>
        /// <returns><c>true</c> if the requests accepts HAL; <c>false</c> otherwise.</returns>
        public static bool AcceptsHal(this IAcceptHeaderFeature feature) => feature.AcceptHeaders.Any(h => h.IsHal());

        /// <summary>
        /// Checks if the request accepts HAL-Forms.
        /// </summary>
        /// <param name="feature">The <see cref="IAcceptHeaderFeature"/> containing the Accept headers.</param>
        /// <returns><c>true</c> if the requests accepts HAL-Forms; <c>false</c> otherwise.</returns>
        public static bool AcceptsHalForms(this IAcceptHeaderFeature feature) => feature.AcceptHeaders.Any(h => h.IsHalForms());

        /// <summary>
        /// Checks if the request accepts HAL-Forms over HAL.
        /// This means that if the request accepts both HAL and HAL-Forms, HAL-Forms is preferred.
        /// </summary>
        /// <param name="feature">The <see cref="IAcceptHeaderFeature"/> containing the Accept headers.</param>
        /// <returns><c>true</c> if the requests accepts HAL-Forms over HAL; <c>false</c> otherwise.</returns>
        public static bool AcceptsHalFormsOverHal(this IAcceptHeaderFeature feature)
        {
            foreach (var header in feature.AcceptHeaders)
            {
                if (header.IsHalForms())
                    return true;
                else if (header.IsHal())
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Checks if the request accepts HAL over HA-FormsL.
        /// This means that if the request accepts both HAL and HAL-Forms, HAL is preferred.
        /// </summary>
        /// <param name="feature">The <see cref="IAcceptHeaderFeature"/> containing the Accept headers.</param>
        /// <returns><c>true</c> if the requests accepts HAL over HAL-Forms; <c>false</c> otherwise.</returns>
        public static bool AcceptsHalOverHalForms(this IAcceptHeaderFeature feature)
        {
            foreach (var header in feature.AcceptHeaders)
            {
                if (header.IsHal())
                    return true;
                else if (header.IsHalForms())
                    return false;
            }

            return false;
        }
    }
}