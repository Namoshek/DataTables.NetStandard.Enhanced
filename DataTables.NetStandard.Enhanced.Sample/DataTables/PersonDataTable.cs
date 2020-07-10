﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DataTables.NetStandard.Enhanced.Filters;
using DataTables.NetStandard.Enhanced.Sample.DataTables.ViewModels;
using DataTables.NetStandard.Enhanced.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTables.NetStandard.Enhanced.Sample.DataTables
{
    public class PersonDataTable : BaseDataTable<Person, PersonViewModel>
    {
        protected SampleDbContext _dbContext;

        public PersonDataTable(IMapper mapper, SampleDbContext dbContext) : base(mapper)
        {
            _dbContext = dbContext;
        }

        public override IList<EnhancedDataTablesColumn<Person, PersonViewModel>> EnhancedColumns()
        {
            var columns = new List<EnhancedDataTablesColumn<Person, PersonViewModel>>
            {
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "id",
                    DisplayName = "ID",
                    PublicPropertyName = nameof(PersonViewModel.Id),
                    PrivatePropertyName = nameof(Person.Id),
                    IsOrderable = true,
                    IsSearchable = true,
                    SearchPredicate = (p, s) => s.Contains(p.Id.ToString()),
                    ColumnSearchPredicateProvider = CreateNumericRangeSearchPredicateProvider(p => p.Id),
                    ColumnFilter = CreateNumericRangeFilter()
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "name",
                    DisplayName = "Name",
                    PublicPropertyName = nameof(PersonViewModel.Name),
                    PrivatePropertyName = nameof(Person.Name),
                    IsOrderable = true,
                    IsSearchable = true,
                    ColumnFilter = CreateTextInputFilter()
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "email",
                    DisplayName = "Email",
                    PublicPropertyName = nameof(PersonViewModel.Email),
                    PrivatePropertyName = nameof(Person.Email),
                    IsOrderable = true,
                    IsSearchable = true,
                    ColumnFilter = CreateTextInputFilter()
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "dateOfBirth",
                    DisplayName = "Date of Birth",
                    PublicPropertyName = nameof(PersonViewModel.DateOfBirth),
                    PrivatePropertyName = nameof(Person.DateOfBirth),
                    IsOrderable = true,
                    IsSearchable = false
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "address",
                    DisplayName = "Address",
                    PublicPropertyName = nameof(PersonViewModel.Address),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Street)}",
                    IsOrderable = true,
                    IsSearchable = true,
                    SearchPredicate = (p, s) => (p.Location.Street + " " + p.Location.HouseNumber).ToLower().Contains(s.ToLower()),
                    ColumnFilter = CreateTextInputFilter()
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "postCode",
                    DisplayName = "Post Code",
                    PublicPropertyName = nameof(PersonViewModel.PostCode),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.PostCode)}",
                    IsOrderable = true,
                    IsSearchable = true,
                    ColumnFilter = CreateSelectFilter(p => new LabelValuePair(p.Location.PostCode))
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "city",
                    DisplayName = "City",
                    PublicPropertyName = nameof(PersonViewModel.City),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.City)}",
                    IsOrderable = true,
                    IsSearchable = true,
                    ColumnFilter = CreateSelectFilter(p => new LabelValuePair(p.Location.City))
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "country",
                    DisplayName = "Country",
                    PublicPropertyName = nameof(PersonViewModel.Country),
                    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
                    IsOrderable = true,
                    IsSearchable = true,
                    ColumnFilter = CreateSelectFilter(p => new LabelValuePair(p.Location.Country), p =>
                    {
                        p.DefaultSelectionLabelValue = "Choose something";
                    })
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "fullAddress",
                    DisplayName = "Full Address",
                    PublicPropertyName = nameof(PersonViewModel.FullAddress),
                    PrivatePropertyName = nameof(Person.Location.Id),
                    IsOrderable = true,
                    IsSearchable = true,
                    SearchPredicate = (p, s) => p.Location.Id.ToString() == s,
                    ColumnFilter = CreateSelectFilter(p => new LabelValuePair(p.Location.FullAddress, p.Location.Id.ToString()))
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "action",
                    DisplayName = "Action",
                    PublicPropertyName = nameof(PersonViewModel.Action),
                    PrivatePropertyName = null,
                    IsOrderable = false,
                    IsSearchable = false
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "action2",
                    DisplayName = "Action 2",
                    PublicPropertyName = nameof(PersonViewModel.Action2),
                    PrivatePropertyName = null,
                    IsOrderable = false,
                    IsSearchable = false
                },
                new EnhancedDataTablesColumn<Person, PersonViewModel>
                {
                    PublicName = "action3",
                    DisplayName = "Action 3",
                    PublicPropertyName = nameof(PersonViewModel.Action3),
                    PrivatePropertyName = null,
                    IsOrderable = false,
                    IsSearchable = false
                }
            };

            // We can also add additional options to a column
            columns.Last().AdditionalOptions.Add("className", "text-center");

            return columns;
        }

        public override IQueryable<Person> Query()
        {
            return _dbContext.Persons.Include(p => p.Location);
        }

        protected override void ConfigureFilters(DataTablesFilterConfiguration configuration)
        {
            base.ConfigureFilters(configuration);

            configuration.DefaultSelectionLabelValue = "Select anything";
        }

        protected override void ConfigureAdditionalOptions(DataTablesConfiguration configuration, IList<DataTablesColumn<Person, PersonViewModel>> columns)
        {
            base.ConfigureAdditionalOptions(configuration, columns);

            configuration.AdditionalOptions["stateSave"] = true;
        }
    }
}
