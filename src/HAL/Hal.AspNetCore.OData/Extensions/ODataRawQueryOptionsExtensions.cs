using System;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Contains extension methods for <see cref="ODataRawQueryOptions"/> to set its values..
    /// </summary>
    public static class ODataRawQueryOptionsExtensions
    {
        private static readonly Action<ODataRawQueryOptions, string> _setFilter;
        private static readonly Action<ODataRawQueryOptions, string> _setApply;
        private static readonly Action<ODataRawQueryOptions, string> _setCompute;
        private static readonly Action<ODataRawQueryOptions, string> _setSearch;
        private static readonly Action<ODataRawQueryOptions, string> _setOrderBy;
        private static readonly Action<ODataRawQueryOptions, string> _setTop;
        private static readonly Action<ODataRawQueryOptions, string> _setSkip;
        private static readonly Action<ODataRawQueryOptions, string> _setSelect;
        private static readonly Action<ODataRawQueryOptions, string> _setExpand;
        private static readonly Action<ODataRawQueryOptions, string> _setCount;
        private static readonly Action<ODataRawQueryOptions, string> _setFormat;
        private static readonly Action<ODataRawQueryOptions, string> _setSkipToken;
        private static readonly Action<ODataRawQueryOptions, string> _setDeltaToken;

        static ODataRawQueryOptionsExtensions()
        {
            var type = typeof(ODataRawQueryOptions);

            _setFilter = type.GetProperty(nameof(ODataRawQueryOptions.Filter))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Filter));
            _setApply = type.GetProperty(nameof(ODataRawQueryOptions.Apply))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Apply));
            _setCompute = type.GetProperty(nameof(ODataRawQueryOptions.Compute))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Compute));
            _setSearch = type.GetProperty(nameof(ODataRawQueryOptions.Search))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Search));
            _setOrderBy = type.GetProperty(nameof(ODataRawQueryOptions.OrderBy))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.OrderBy));
            _setTop = type.GetProperty(nameof(ODataRawQueryOptions.Top))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Top));
            _setSkip = type.GetProperty(nameof(ODataRawQueryOptions.Skip))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Skip));
            _setSelect = type.GetProperty(nameof(ODataRawQueryOptions.Select))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Select));
            _setExpand = type.GetProperty(nameof(ODataRawQueryOptions.Expand))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Expand));
            _setCount = type.GetProperty(nameof(ODataRawQueryOptions.Count))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Count));
            _setFormat = type.GetProperty(nameof(ODataRawQueryOptions.Format))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.Format));
            _setSkipToken = type.GetProperty(nameof(ODataRawQueryOptions.SkipToken))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.SkipToken));
            _setDeltaToken = type.GetProperty(nameof(ODataRawQueryOptions.DeltaToken))?.SetMethod?.CreateDelegate<Action<ODataRawQueryOptions, string>>() ?? throw new MissingMemberException(nameof(ODataRawQueryOptions), nameof(ODataRawQueryOptions.DeltaToken));
        }

        /// <summary>
        /// Sets the raw $filter query value.
        /// </summary>
        public static void SetFilter(this ODataRawQueryOptions options, string value) => _setFilter(options, value);

        /// <summary>
        /// Sets the raw $apply query value.
        /// </summary>
        public static void SetApply(this ODataRawQueryOptions options, string value) => _setApply(options, value);

        /// <summary>
        /// Sets the raw $compute query value.
        /// </summary>
        public static void SetCompute(this ODataRawQueryOptions options, string value) => _setCompute(options, value);

        /// <summary>
        /// Sets the raw $search query value.
        /// </summary>
        public static void SetSearch(this ODataRawQueryOptions options, string value) => _setSearch(options, value);

        /// <summary>
        /// Sets the raw $orderby query value.
        /// </summary>
        public static void SetOrderBy(this ODataRawQueryOptions options, string value) => _setOrderBy(options, value);

        /// <summary>
        /// Sets the raw $top query value.
        /// </summary>
        public static void SetTop(this ODataRawQueryOptions options, string value) => _setTop(options, value);

        /// <summary>
        /// Sets the raw $skip query value.
        /// </summary>
        public static void SetSkip(this ODataRawQueryOptions options, string value) => _setSkip(options, value);

        /// <summary>
        /// Sets the raw $select query value.
        /// </summary>
        public static void SetSelect(this ODataRawQueryOptions options, string value) => _setSelect(options, value);

        /// <summary>
        /// Sets the raw $expand query value.
        /// </summary>
        public static void SetExpand(this ODataRawQueryOptions options, string value) => _setExpand(options, value);

        /// <summary>
        /// Sets the raw $count query value.
        /// </summary>
        public static void SetCount(this ODataRawQueryOptions options, string value) => _setCount(options, value);

        /// <summary>
        /// Sets the raw $format query value.
        /// </summary>
        public static void SetFormat(this ODataRawQueryOptions options, string value) => _setFormat(options, value);

        /// <summary>
        /// Sets the raw $skiptoken query value.
        /// </summary>
        public static void SetSkipToken(this ODataRawQueryOptions options, string value) => _setSkipToken(options, value);

        /// <summary>
        /// Sets the raw $deltatoken query value.
        /// </summary>
        public static void SetDeltaToken(this ODataRawQueryOptions options, string value) => _setDeltaToken(options, value);
    }
}
