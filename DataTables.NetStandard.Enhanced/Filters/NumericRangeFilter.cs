using System.Collections.Generic;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class NumericRangeFilter : BaseFilter, IColumnFilter
    {
        /// <summary>
        /// Defines the placeholder displayed on the input for the minimum value.
        /// </summary>
        public string PlaceholderMinValue { get; set; }

        /// <summary>
        /// Defines the placeholder displayed on the input for the maximum value.
        /// </summary>
        public string PlaceholderMaxValue { get; set; }

        /// <summary>
        /// Setting this to <c>true</c> enables a checkbox which allows filtering by <c>null</c>.
        /// </summary>
        public bool AllowFilteringByNull { get; set; }

        public override string FilterType => "range_number";

        internal NumericRangeFilter() { }

        public override FilterOptions GetFilterOptions(int columnIndex)
        {
            var options = base.GetFilterOptions(columnIndex, new Dictionary<string, dynamic>
            {
                { "filter_default_label", new string[] { PlaceholderMinValue, PlaceholderMaxValue } },
                { "null_check_box", AllowFilteringByNull },
            });

            return options;
        }
    }
}
