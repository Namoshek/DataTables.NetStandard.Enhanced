using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataTables.NetStandard.Enhanced.Filters;
using DataTables.NetStandard.Enhanced.Util;
using DataTables.NetStandard.Extensions;
using MoreLinq.Extensions;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced
{
    public abstract class EnhancedDataTable<TEntity, TEntityViewModel> : DataTable<TEntity, TEntityViewModel>
    {
        protected DataTablesFilterConfiguration _filterConfiguration;

        public EnhancedDataTable()
        {
            _filterConfiguration = new DataTablesFilterConfiguration();
        }

        #region table_overrides
        /// <summary>
        /// Enhanced column definitions for this DataTable. Replaces normal column definitions.
        /// </summary>
        public abstract IList<EnhancedDataTablesColumn<TEntity, TEntityViewModel>> EnhancedColumns();

        /// <summary>
        /// We simply forward our enhanced column definitions as normal column definitions.
        /// This is possible because they still work the same except that they hold additional data.
        /// </summary>
        public override sealed IList<DataTablesColumn<TEntity, TEntityViewModel>> Columns()
        {
            return EnhancedColumns().Cast<DataTablesColumn<TEntity, TEntityViewModel>>().ToList();
        }

        /// <summary>
        /// Renders the results based on the given <see cref="DataTablesRequest{TEntity, TEntityViewModel}"/>
        /// and builds a response that can be returned immediately.
        /// </summary>
        /// <param name="query">The query.</param>
        public override DataTablesResponse<TEntity, TEntityViewModel> RenderResponse(string query)
        {
            Configure();

            var request = BuildRequest(query);
            var data = RenderResults(request);

            var columnFilters = GetColumnFilterOptions(request);
            var filterData = columnFilters.ToDictionary(f => $"yadcf_data_{f.ColumnNumber}", f => f.Data as dynamic);

            return new EnhancedDataTablesResponse<TEntity, TEntityViewModel>(data,
                Columns(),
                request.Draw,
                filterData);
        }

        /// <summary>
        /// Renders the script.
        /// </summary>
        /// <param name="url">The url of the data endpoint for the DataTable</param>
        /// <param name="method">The http method used for the data endpoint (get or post)</param>
        public override string RenderScript(string url, string method = "get")
        {
            Configure();

            var script = base.RenderScript(url, method);
            var columnFilters = GetColumnFilterOptions();

            var columnOptions = JsonConvert.SerializeObject(columnFilters);
            var globalOptions = JsonConvert.SerializeObject(_filterConfiguration.AdditionalFilterOptions);

            // We reference the DataTable instance with <code>dt_{tableIdentifier}</code>
            script += $"yadcf.init(dt_{GetTableIdentifier()}, {columnOptions}, {globalOptions});";

            return script;
        }
        #endregion

        #region filter_factories
        /// <summary>
        /// Creates a new text input filter based on the default configuration.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual TextInputFilter CreateTextInputFilter(Action<TextInputFilter> configure = null)
        {
            var filter = new TextInputFilter
            {
                PlaceholderValue = _filterConfiguration.DefaultTextInputPlaceholderValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(TextInputFilter)),
            };

            configure?.Invoke(filter);

            return filter;
        }

        /// <summary>
        /// Creates a new select filter based on the default configuration and the given key value selector.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="keyValueSelector"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual SelectFilter<TEntity> CreateSelectFilter(Expression<Func<TEntity, LabelValuePair>> keyValueSelector, 
            Action<SelectFilter<TEntity>> configure = null)
        {
            var filter = new SelectFilter<TEntity>(keyValueSelector)
            {
                EnableDefaultSelectionLabel = _filterConfiguration.EnableDefaultSelectionLabel,
                DefaultSelectionLabelValue = _filterConfiguration.DefaultSelectionLabelValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(SelectFilter<TEntity>)),
            };

            configure?.Invoke(filter);

            return filter;
        }

        /// <summary>
        /// Creates a new select filter based on the default configuration and the given filter options.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="filterOptions"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual SelectFilter<TEntity> CreateSelectFilter(IList<LabelValuePair> filterOptions, 
            Action<SelectFilter<TEntity>> configure = null)
        {
            var filter = new SelectFilter<TEntity>(filterOptions)
            {
                EnableDefaultSelectionLabel = _filterConfiguration.EnableDefaultSelectionLabel,
                DefaultSelectionLabelValue = _filterConfiguration.DefaultSelectionLabelValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(SelectFilter<TEntity>)),
            };

            configure?.Invoke(filter);

            return filter;
        }

        /// <summary>
        /// Returns a list of distinct column values that can be used for select filters.
        /// </summary>
        /// <param name="property"></param>
        public virtual IList<LabelValuePair> GetDistinctColumnValuesForSelect(Expression<Func<TEntity, LabelValuePair>> selector,
            DataTablesRequest<TEntity, TEntityViewModel> request)
        {
            var query = Query();

            if (request != null)
            {
                query = query.Apply(request);
            }

            // TODO: As soon as EFCore supports translation of .GroupBy(expr).Select(e => e.FirstOrDefault()),
            //       this method can be improved significantly by applying the group by instead of distinctby.
            return query.Select(selector.Compile())
                .DistinctBy(e => e.Value)
                .ToList();
        }

        /// <summary>
        /// Creates a new numeric range input filter based on the default configuration.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual NumericRangeFilter CreateNumericRangeFilter(Action<NumericRangeFilter> configure = null)
        {
            var filter = new NumericRangeFilter
            {
                PlaceholderValue = _filterConfiguration.DefaultNumericRangeInputPlaceholderValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(NumericRangeFilter)),
            };

            configure?.Invoke(filter);

            return filter;
        }

        /// <summary>
        /// Creates a new date range input filter based on the default configuration.
        /// Can be further configured by the given <paramref name="configure"/> action.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public virtual DateRangeFilter CreateDateRangeFilter(Action<DateRangeFilter> configure = null)
        {
            var filter = new DateRangeFilter
            {
                PlaceholderValue = _filterConfiguration.DefaultDateRangeInputPlaceholderValue,
                AdditionalOptions = _filterConfiguration.GetAdditionalColumnFilterOptions(typeof(DateRangeFilter)),
            };

            configure?.Invoke(filter);

            return filter;
        }
        #endregion

        #region configuration
        /// <summary>
        /// Allows to configure the DataTable instance.
        /// </summary>
        protected override void Configure()
        {
            if (_isConfigured)
            {
                return;
            }

            var columns = Columns();

            ConfigureColumns(_configuration, columns);
            ConfigureColumnOrdering(_configuration, columns);
            ConfigureAdditionalOptions(_configuration, columns);
            ConfigureFilters(_filterConfiguration);

            _isConfigured = true;
        }

        /// <summary>
        /// Allows to configure the DataTable filters.
        /// </summary>
        /// <param name="configuration"></param>
        protected virtual void ConfigureFilters(DataTablesFilterConfiguration configuration)
        {
            // We do not configure anything, but we provide a default implementation.
        }

        /// <summary>
        /// Returns a list of prefilled column filters with their corresponding options.
        /// </summary>
        protected virtual IList<FilterOptions> GetColumnFilterOptions(DataTablesRequest<TEntity, TEntityViewModel> request = null)
        {
            var columns = EnhancedColumns();

            columns.Where(c => c.ColumnFilter is IFilterWithSelectableData<TEntity> && (c.ColumnFilter as IFilterWithSelectableData<TEntity>).Data == null)
                .ToList()
                .ForEach(c =>
                {
                    var col = c.ColumnFilter as IFilterWithSelectableData<TEntity>;
                    if (col.KeyValueSelector() != null)
                    {
                        col.Data = GetDistinctColumnValuesForSelect(col.KeyValueSelector(), request).Cast<object>().ToList();
                    }
                });

            return columns.Where(c => c.ColumnFilter != null)
                .Select(c => c.ColumnFilter.GetFilterOptions(columns.IndexOf(c)))
                .ToList();
        }
        #endregion

        #region numeric_range_filters
        /// <summary>
        /// Returns a numeric range search predicate provider expression for the given <paramref name="propertySelector"/>.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        protected virtual Func<string, Expression<Func<TEntity, string, bool>>> CreateNumericRangeSearchPredicateProvider(Expression<Func<TEntity, long>> propertySelector, string delimiter = "-")
        {
            return (s) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return (e, s) => true;
                }

                if (!s.Contains(delimiter))
                {
                    if (long.TryParse(s, out long val))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, val, val);
                    }

                    return (e, s) => false;
                }

                var parts = s.Split(new string[] { delimiter }, StringSplitOptions.None);

                if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    if (long.TryParse(parts[0], out long min) && long.TryParse(parts[1], out long max))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, min, max);
                    }
                }

                if (!string.IsNullOrWhiteSpace(parts[0]))
                {
                    if (long.TryParse(parts[0], out long min))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, min, null);
                    }
                }

                if (!string.IsNullOrWhiteSpace(parts[1]))
                {
                    if (long.TryParse(parts[1], out long max))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, null, max);
                    }
                }

                return (e, s) => true;
            };
        }

        /// <summary>
        /// Builds a numeric range search expression using the given inputs. The expression will ignore borders set to <c>null</c>
        /// while filtering the data.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected virtual Expression<Func<TEntity, string, bool>> BuildNumericRangeSearchExpression(Expression<Func<TEntity, long>> propertySelector, long? min, long? max)
        {
            var entityParam = Expression.Parameter(typeof(TEntity), "e");
            var searchTermParam = Expression.Parameter(typeof(string), "s");

            var nullableMinConst = Expression.Constant(min, typeof(long?));
            var nullableMaxConst = Expression.Constant(max, typeof(long?));
            var minConst = Expression.Constant(min ?? 0, typeof(long));
            var maxConst = Expression.Constant(max ?? long.MaxValue, typeof(long));
            var nullConst = Expression.Constant(null, typeof(long?));

            return Expression.Lambda<Func<TEntity, string, bool>>(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.Equal(nullableMinConst, nullConst),
                        Expression.GreaterThanOrEqual(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            minConst)),
                    Expression.OrElse(
                        Expression.Equal(nullableMaxConst, nullConst),
                        Expression.LessThanOrEqual(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            maxConst))),
                entityParam,
                searchTermParam);
        }

        /// <summary>
        /// Returns a numeric range search predicate provider expression for the given <paramref name="propertySelector"/>.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        protected virtual Func<string, Expression<Func<TEntity, string, bool>>> CreateNumericRangeSearchPredicateProvider(Expression<Func<TEntity, int>> propertySelector, string delimiter = "-")
        {
            return (s) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return (e, s) => true;
                }

                if (!s.Contains(delimiter))
                {
                    if (int.TryParse(s, out int val))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, val, val);
                    }

                    return (e, s) => false;
                }

                var parts = s.Split(new string[] { delimiter }, StringSplitOptions.None);

                if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    if (int.TryParse(parts[0], out int min) && int.TryParse(parts[1], out int max))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, min, max);
                    }
                }

                if (!string.IsNullOrWhiteSpace(parts[0]))
                {
                    if (int.TryParse(parts[0], out int min))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, min, null);
                    }
                }

                if (!string.IsNullOrWhiteSpace(parts[1]))
                {
                    if (int.TryParse(parts[1], out int max))
                    {
                        return BuildNumericRangeSearchExpression(propertySelector, null, max);
                    }
                }

                return (e, s) => true;
            };
        }

        /// <summary>
        /// Builds a numeric range search expression using the given inputs. The expression will ignore borders set to <c>null</c>
        /// while filtering the data.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected virtual Expression<Func<TEntity, string, bool>> BuildNumericRangeSearchExpression(Expression<Func<TEntity, int>> propertySelector, int? min, int? max)
        {
            var entityParam = Expression.Parameter(typeof(TEntity), "e");
            var searchTermParam = Expression.Parameter(typeof(string), "s");

            var nullableMinConst = Expression.Constant(min, typeof(int?));
            var nullableMaxConst = Expression.Constant(max, typeof(int?));
            var minConst = Expression.Constant(min ?? 0, typeof(int));
            var maxConst = Expression.Constant(max ?? long.MaxValue, typeof(int));
            var nullConst = Expression.Constant(null, typeof(int?));

            return Expression.Lambda<Func<TEntity, string, bool>>(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.Equal(nullableMinConst, nullConst),
                        Expression.GreaterThanOrEqual(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            minConst)),
                    Expression.OrElse(
                        Expression.Equal(nullableMaxConst, nullConst),
                        Expression.LessThanOrEqual(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            maxConst))),
                entityParam,
                searchTermParam);
        }
        #endregion

        #region date_range_filters
        /// <summary>
        /// Returns a date range search predicate provider expression for the given <paramref name="propertySelector"/>.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="delimiter"></param>
        /// <param name="dateParseFunction"></param>
        /// <returns></returns>
        protected virtual Func<string, Expression<Func<TEntity, string, bool>>> CreateDateRangeSearchPredicateProvider(
            Expression<Func<TEntity, DateTimeOffset>> propertySelector, 
            string delimiter = "~",
            Func<string, DateTimeOffset?> dateParseFunction = null)
        {
            if (dateParseFunction == null)
            {
                dateParseFunction = (s) =>
                {
                    if (DateTimeOffset.TryParse(s, out DateTimeOffset val))
                    {
                        return val;
                    }

                    return null;
                };
            }

            return (s) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return (e, s) => true;
                }

                if (!s.Contains(delimiter))
                {
                    var val = dateParseFunction(s);

                    return val != null
                        ? BuilDateRangeSearchExpression(propertySelector, val, val)
                        : (e, s) => false;
                }

                var parts = s.Split(new string[] { delimiter }, StringSplitOptions.None);

                if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    var min = dateParseFunction(parts[0]);
                    var max = dateParseFunction(parts[1]);

                    return min != null && max != null
                        ? BuilDateRangeSearchExpression(propertySelector, min, max)
                        : (e, s) => false;
                }

                if (!string.IsNullOrWhiteSpace(parts[0]))
                {
                    var min = dateParseFunction(parts[0]);

                    return min != null
                        ? BuilDateRangeSearchExpression(propertySelector, min, null)
                        : (e, s) => false;
                }

                if (!string.IsNullOrWhiteSpace(parts[1]))
                {
                    var max = dateParseFunction(parts[1]);

                    return max != null
                        ? BuilDateRangeSearchExpression(propertySelector, null, max)
                        : (e, s) => false;
                }

                return (e, s) => true;
            };
        }

        /// <summary>
        /// Builds a date range search expression using the given inputs. The expression uses "inclusive" comparison for
        /// the lower border and "exclusive" comparison for the higher border.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected virtual Expression<Func<TEntity, string, bool>> BuilDateRangeSearchExpression(
            Expression<Func<TEntity, DateTimeOffset>> propertySelector, 
            DateTimeOffset? min, 
            DateTimeOffset? max)
        {
            var entityParam = Expression.Parameter(typeof(TEntity), "e");
            var searchTermParam = Expression.Parameter(typeof(string), "s");

            var nullableMinConst = Expression.Constant(min, typeof(DateTimeOffset?));
            var nullableMaxConst = Expression.Constant(max, typeof(DateTimeOffset?));
            var minConst = Expression.Constant(min ?? DateTimeOffset.MinValue, typeof(DateTimeOffset));
            var maxConst = Expression.Constant(max ?? DateTimeOffset.MaxValue, typeof(DateTimeOffset));
            var nullConst = Expression.Constant(null, typeof(DateTimeOffset?));

            if (min.HasValue && max.HasValue && min == max)
            {
                return Expression.Lambda<Func<TEntity, string, bool>>(
                    Expression.Equal(
                        Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                        minConst),
                    entityParam,
                    searchTermParam);
            }

            return Expression.Lambda<Func<TEntity, string, bool>>(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.Equal(nullableMinConst, nullConst),
                        Expression.GreaterThanOrEqual(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            minConst)),
                    Expression.OrElse(
                        Expression.Equal(nullableMaxConst, nullConst),
                        Expression.LessThan(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            maxConst))),
                entityParam,
                searchTermParam);
        }

        /// <summary>
        /// Returns a date range search predicate provider expression for the given <paramref name="propertySelector"/>.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="delimiter"></param>
        /// <param name="dateParseFunction"></param>
        /// <returns></returns>
        protected virtual Func<string, Expression<Func<TEntity, string, bool>>> CreateDateRangeSearchPredicateProvider(
            Expression<Func<TEntity, DateTime>> propertySelector, 
            string delimiter = "~",
            Func<string, DateTime?> dateParseFunction = null)
        {
            if (dateParseFunction == null)
            {
                dateParseFunction = (s) =>
                {
                    if (DateTime.TryParse(s, out DateTime val))
                    {
                        return val;
                    }

                    return null;
                };
            }

            return (s) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return (e, s) => true;
                }

                if (!s.Contains(delimiter))
                {
                    var val = dateParseFunction(s);

                    return val != null
                        ? BuilDateRangeSearchExpression(propertySelector, val, val)
                        : (e, s) => false;
                }

                var parts = s.Split(new string[] { delimiter }, StringSplitOptions.None);

                if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    var min = dateParseFunction(parts[0]);
                    var max = dateParseFunction(parts[1]);

                    return min != null && max != null
                        ? BuilDateRangeSearchExpression(propertySelector, min, max)
                        : (e, s) => false;
                }

                if (!string.IsNullOrWhiteSpace(parts[0]))
                {
                    var min = dateParseFunction(parts[0]);

                    return min != null
                        ? BuilDateRangeSearchExpression(propertySelector, min, null)
                        : (e, s) => false;
                }

                if (!string.IsNullOrWhiteSpace(parts[1]))
                {
                    var max = dateParseFunction(parts[1]);

                    return max != null
                        ? BuilDateRangeSearchExpression(propertySelector, null, max)
                        : (e, s) => false;
                }

                return (e, s) => true;
            };
        }

        /// <summary>
        /// Builds a date range search expression using the given inputs. The expression uses "inclusive" comparison for
        /// the lower border and "exclusive" comparison for the higher border.
        /// </summary>
        /// <param name="propertySelector"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected virtual Expression<Func<TEntity, string, bool>> BuilDateRangeSearchExpression(
            Expression<Func<TEntity, DateTime>> propertySelector,
            DateTime? min,
            DateTime? max)
        {
            var entityParam = Expression.Parameter(typeof(TEntity), "e");
            var searchTermParam = Expression.Parameter(typeof(string), "s");

            var nullableMinConst = Expression.Constant(min, typeof(DateTime?));
            var nullableMaxConst = Expression.Constant(max, typeof(DateTime?));
            var minConst = Expression.Constant(min ?? DateTime.MinValue, typeof(DateTime));
            var maxConst = Expression.Constant(max ?? DateTime.MaxValue, typeof(DateTime));
            var nullConst = Expression.Constant(null, typeof(DateTime?));

            if (min.HasValue && max.HasValue && min == max)
            {
                return Expression.Lambda<Func<TEntity, string, bool>>(
                    Expression.Equal(
                        Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                        minConst),
                    entityParam,
                    searchTermParam);
            }

            return Expression.Lambda<Func<TEntity, string, bool>>(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.Equal(nullableMinConst, nullConst),
                        Expression.GreaterThanOrEqual(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            minConst)),
                    Expression.OrElse(
                        Expression.Equal(nullableMaxConst, nullConst),
                        Expression.LessThan(
                            Expression.Property(entityParam, PropertyHelper<TEntity>.GetProperty(propertySelector)),
                            maxConst))),
                entityParam,
                searchTermParam);
        }
        #endregion
    }
}
