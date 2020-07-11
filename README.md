# DataTables.NetStandard.Enhanced

This package provides an enhanced version of [DataTables.NetStandard](https://github.com/Namoshek/DataTables.NetStandard).

Because filters are an essential part of a DataTable, this is an extension package for `DataTables.NetStandard` which utilizes
the great [yadcf](https://github.com/vedmack/yadcf) library to add built-in support for filters on a per-column basis.
The extension package is written in a way that allows for easy configuration of the filters (although sensible defaults are used anyway).

To make this work, an abstract `EnhancedDataTable` base class is provided by this package, extending on the abstract `DataTable` base class.
This base class provides additional configuration options for filters and customizes the script rendering of the base package to add
script rendering for the `yadcf` filters defined on individual columns.

#### Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Supported Filters](#supported-filters)
  - [TextInput Filter](#textinput-filter)
  - [Select Filter](#select-filter)
- [Filter Configuration](#filter-configuration)
  - [Per-Table Configuration](#per-table-configuration)
  - [Per-Usage Configuration](#per-usage-configuration)
- [License](#license)

## Installation

The package can be found on [nuget.org](https://www.nuget.org/packages/DataTables.NetStandard.Enhanced/).
You can install the package with:

```pwsh
$> Install-Package DataTables.NetStandard.Enhanced
```

## Usage

To use the enhanced tables, you only need to base your tables on the `EnhancedDataTable` base class instead of `DataTable`.
You will also need to define `EnhancedColumns()` instead of `Columns()`:

```csharp
public class PersonDataTable : EnhancedDataTable<Person, PersonViewModel>
{
    public override IList<EnhancedDataTablesColumn<Person, PersonViewModel>> EnhancedColumns()
    {
        return new List<EnhancedDataTablesColumn<Person, PersonViewModel>>
        {
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
            // More columns ...
        };
    }
}
```

## Supported Filters

Currently, the following filters are supported by this package. You can implement your own filters though (and share them
by making a Pull Request :smile:).

### TextInput Filter

The most basic filter is the `TextInputFilter`. It provides a way to use free-text search on a per-column basis, just like the
global filter already supported by the base package. Usage is as simple as:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "name",
    DisplayName = "Name",
    PublicPropertyName = nameof(PersonViewModel.Name),
    PrivatePropertyName = nameof(Person.Name),
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = CreateTextInputFilter()
}
```

### Select Filter

For columns with a well-defined set of values (like enums) or colums with a finite set of values (like a `country` column),
this filter provides a way to display a select filter that contains these well-defined sets of values:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "country",
    DisplayName = "Country",
    PublicPropertyName = nameof(PersonViewModel.Country),
    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = CreateSelectFilter(p => new LabelValuePair(p.Location.Country))
}
```

The filter implements the `IFilterWithSelectableData` interface. For all filters of this type, the `EnhancedDataTable` will load
distinct values based on the given `LabelValuePair` when rendering the table or when returning an ajax response to update the filters
with the remaining set of possible values (cumulative search).
This will only happen if you pass a `Expression<Func<TEntity, LabelValuePair>>` to the filter constructor as seen in the example above.
Alternatively, you can also pass an `IList<LabelValuePair>` with the options to display. This is useful if you want to display
the localized options of an enum for example. Or you use some data from a repository:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "country",
    DisplayName = "Country",
    PublicPropertyName = nameof(PersonViewModel.Country),
    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = CreateSelectFilter(_countryRepository.GetAll())
}
```

Options of a select filter can also display a label different to the value they represent. This is especially useful if you want to
display an element of a foreign table using the values of the foreign table while searching with the foreign key:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "fullAddress",
    DisplayName = "Full Address",
    PublicPropertyName = nameof(PersonViewModel.FullAddress),
    PrivatePropertyName = nameof(Person.Location.Id),
    IsOrderable = true,
    IsSearchable = true,
    SearchPredicate = (p, s) => (p.Location.Street + " " + p.Location.HouseNumber).ToLower().Contains(s.ToLower()),
    ColumnSearchPredicate = (p, s) => p.Location.ToString() == s,
    ColumnFilter = CreateSelectFilter(p => new LabelValuePair(p.Location.FullAddress, p.Location.Id.ToString()))
}
```

_Please note that also the value of a `LabelValuePair` has always to be a string as the search of DataTables works with strings only._

### NumericRangeFilter Filter

Based on the `TextInputFilter`, the `NumericRangeFilter` allows searching for entities by entering a numeric range in one of the following forms:
- `42-45` to select entities matching the values 42, 43, 44 and 45
- `42-` to select entities matching the values 42 and above
- `-42` to select entities matching the value 42 and below
- `42` to select the entity matching the value 42
- any other input like `foo` (i.e. not a number) will return zero results to make the user aware of the wrong input

Defining the column filter alone will not be enough though. It only adds the filter to the table. In order to add the processing logic,
the `ColumnSearchPredicateProvider` should be set to `CreateNumericRangeSearchPredicateProvider(p => p.Id)` where `p => p.Id` is the predicate
selecting the column to filter on.

```csharp
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
}
```

The delimiter used to define a range when inputting numbers may also be changed from `-` to something else by passing a different delimiter
as second parameter to `CreateNumericRangeSearchPredicateProvider()`. This may be useful when searchability for negative numbers is required.

_Note: using this setup, the global search will find individual entries with the searched ID, while the column search may be used to search
for a range of IDs._

_Note: currently, this filter can be used for `int` and `long` columns. Other numeric types are currently not supported, since parsing them
is culture dependent._

## Filter Configuration

When configuring your filters with additional options, you can always choose between configuring only one instance of a filter
or all filters of your DataTable. By using a base table for all of your DataTable instances, you can also use a _global_ configuration.

### Per-Table Configuration

Configuring your filters is done in the `ConfigureFilters` method within your DataTable (or a base table, if you prefer):

```csharp
protected override void ConfigureFilters(DataTablesFilterConfiguration configuration)
{
    configuration.DefaultSelectionLabelValue = "Select something";
    configuration.DefaultTextInputPlaceholderValue = "Type to find";

    configuration.AdditionalFilterOptions.Add("filters_position", "footer");

    var selectFilterConfiguration = configuration.GetAdditionalColumnFilterOptions(typeof(SelectFilter<TEntity>));
    selectFilterConfiguration["select_type"] = "select2";
}
```

You can add additional options for the whole `yadcf` library via the `AdditionalFilterOptions` dictionary.
Additional options for individual filter types can be added by retrieving the corresponding dictionary with
`configuration.GetAdditionalColumnFilterOptions(type)` where `type` is the type of a filter class.

### Per-Usage Configuration

Alternatively, you can also configure your filters when defining your table columns and their filters:

```csharp
new EnhancedDataTablesColumn<Person, PersonViewModel>
{
    PublicName = "country",
    DisplayName = "Country",
    PublicPropertyName = nameof(PersonViewModel.Country),
    PrivatePropertyName = $"{nameof(Person.Location)}.{nameof(Location.Country)}",
    IsOrderable = true,
    IsSearchable = true,
    ColumnFilter = CreateSelectFilter(p => new LabelValuePair(p.Location.Country, p.Location.Country), p =>
    {
        p.DefaultSelectionLabelValue = "Choose something";
    })
}
```

## License

The code is licensed under the [MIT license](LICENSE.md).
