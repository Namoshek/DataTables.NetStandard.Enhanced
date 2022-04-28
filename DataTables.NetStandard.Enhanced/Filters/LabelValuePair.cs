using System.Linq.Expressions;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public class LabelValuePair
    {
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// A default constructor to be used in expressions which are translated to SQL,
        /// e.g. <see cref="EnhancedDataTable{TEntity, TEntityViewModel}.CreateSelectFilter(Expression{System.Func{TEntity, LabelValuePair}}, System.Action{SelectFilter{TEntity}})"/>.
        /// </summary>
        public LabelValuePair()
        {
        }

        public LabelValuePair(string labelAndValue)
        {
            Label = labelAndValue;
            Value = labelAndValue;
        }

        public LabelValuePair(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}
