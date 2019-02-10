﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataTables.NetCore.Builder
{
    public class DataTablesConfigurationBuilder
    {
        public static DataTablesConfiguration Configuration { get; } = new DataTablesConfiguration();

        public static string BuildGlobalConfigurationScript()
        {
            var output = JsonConvert.SerializeObject(Configuration, GetSerializerSettings());

            return $"$.extend(true, $.fn.dataTable.defaults, {output});";
        }

        public static string BuildDataTableConfigurationScript<TEntity, TEntityViewModel>(IDataTablesColumnsCollection<TEntity, TEntityViewModel> columns, string tableName, string url, string method)
        {
            var configuration = (DataTablesConfiguration)Configuration.Clone();
            configuration.Ajax = url;
            configuration.Method = method;

            foreach (var column in columns)
            {
                configuration.Columns.Add(new DataTablesConfiguration.DataTablesConfigurationColumn
                {
                    Data = column.PublicName,
                    Name = column.PublicName,
                    Title = column.DisplayName,
                    Searchable = column.IsSearchable,
                    Orderable = column.IsOrderable
                });
            }

            var output = JsonConvert.SerializeObject(configuration, GetSerializerSettings());

            return $"$(document).ready(function() {{ $('#{tableName}').DataTable({output}); }} );";
        }

        protected static JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}