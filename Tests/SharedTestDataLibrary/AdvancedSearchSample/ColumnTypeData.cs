using Domain.Entities.SearchEntities;
using Domain.Enums;

namespace SharedTestDataLibrary.AdvancedSearchSample;

public class ColumnTypeData
{

    public static List<ColumnType> CreateColumnTypes()
    {
        return new List<ColumnType>()
        {
           new ColumnType()
           {
               Code = "text",
               Name = "Text",
               Operators = new List<Operator>()
               {
                   new Operator()
                   {
                       Code = "Equals",
                       DisplayName = "=",
                       SqlTemplate = "{0} = '{1}'",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.TextBox
                   },
                   new Operator()
                   {
                       Code = "Contains",
                       DisplayName = "contains",
                       SqlTemplate = "{col}  LIKE '%{@p1}%'",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.TextBox
                   }
               }
           },
           new ColumnType()
           {
               Code = "number",
               Name = "Number",
               Operators = new List<Operator>()
               {
                   new Operator()
                   {
                       Code = "Equals",
                       DisplayName = "=",
                       SqlTemplate = "{col} = {@p1}",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.NumberBox
                   },
                   new Operator()
                   {
                       Code = "Bigger",
                       DisplayName = "<",
                       SqlTemplate = "{col} < @p1",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.NumberBox
                   }
               }
           },
           new ColumnType()
           {
               Code = "date",
               Name = "Date",
               Operators = new List<Operator>()
               {
                   new Operator()
                   {
                       Code = "Equals",
                       DisplayName = "=",
                       SqlTemplate = "{col} = {@p1}",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.DatePicker
                   },
                   new Operator()
                   {
                       Code = "Between",
                       DisplayName = "Between",
                       SqlTemplate = "{col} between @p1 and @p2",
                       ValueMode = OperatorValueMode.Range,
                       InputControl = InputControlType.DateRangePicker
                   }
               }
           },
           new ColumnType()
           {
               Code = "list",
               Name = "List",
               Operators = new List<Operator>()
               {
                   new Operator()
                   {
                       Code = "Equals",
                       DisplayName = "=",
                       SqlTemplate = "{col} = {@p1}",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.Dropdown
                   }
               }
           },
           new ColumnType()
           {
               Code = "bool",
               Name = "TrueFalse",
               Operators = new List<Operator>()
               {
                   new Operator()
                   {
                       Code = "Equals",
                       DisplayName = "=",
                       SqlTemplate = "{col} = @p1",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.Checkbox
                   },
                   new Operator()
                   {
                       Code = "Not Equal",
                       DisplayName = "!=",
                       SqlTemplate = "{col} != @p1",
                       ValueMode = OperatorValueMode.Single,
                       InputControl = InputControlType.Checkbox
                   }
               }
           }
        };
    }
}
