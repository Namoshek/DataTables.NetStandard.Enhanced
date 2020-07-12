using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class MultiSelectFilter<TEntity> : SelectFilter<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFilter{TEntity}"/> class.
        /// </summary>
        /// <param name="keyValueSelector">A selector used to load the filter options.</param>
        internal MultiSelectFilter(Expression<Func<TEntity, LabelValuePair>> keyValueSelector) : base(keyValueSelector)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFilter{TEntity}"/> class.
        /// </summary>
        /// <param name="filterOptions">The filter options.</param>
        internal MultiSelectFilter(IList<LabelValuePair> filterOptions) : base(filterOptions)
        {
        }

        public override string FilterType => "multi_select";
    }
}
