using Application.Aggregates.SearchAggregate.Queries;
using Domain.Entities.Search;
using Microsoft.Data.SqlClient;
using System.Text;

namespace ServiceLayer.Searches
{
    public class SearchService : ISearchService
    {

        public class SqlSearchBuilder
        {
            private int _paramIndex = 0;
            private readonly List<SqlParameter> _parameters = new();
            private readonly HashSet<string> _joins = new();

            public (string sql, List<SqlParameter> parameters)
                Build(SearchRequestDto request,
                      List<SearchColumnFilter> fields,
                      List<FilterOperator> operators)
            {
                SearchType searchType = new SearchType()
                {
                    MainTable = "TaskList",
                    MainTableAlias = "t",
                    IsDeleted = 0,
                    TypeName = "TaskListSearch"
                };


                var select = BuildSelect(searchType, request.SelectedColumnFieldIds, fields);
                var where = BuildWhere(searchType, request.Filters, fields, operators);
                var join = string.Join("\n", _joins);

                var sql = $@"
                                    SELECT {select}
                                    FROM TaskList t
                                    {join}
                                    WHERE 1=1
                                    {where}
                                    ";

                if (!request.IsExport)
                {
                    sql += @"
ORDER BY t.Id
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

                    _parameters.Add(new SqlParameter("@offset",
                        (request.Page - 1) * request.PageSize));
                    _parameters.Add(new SqlParameter("@pageSize", request.PageSize));
                }

                return (sql, _parameters);
            }


            //AI auto fill
            //private string BuildSelect(List<int> selectedFieldIds, List<SearchFieldMeta> fields)
            //{
            //    if (selectedFieldIds.Count == 0)
            //        return "t.Id"; // Default to Id if no fields selected
            //    var selectParts = new List<string>();
            //    foreach (var fieldId in selectedFieldIds)
            //    {
            //        var field = fields.Find(f => f.Id == fieldId);
            //        if (field != null)
            //        {
            //            if (field.TableAlias != "t" && !_joins.Contains(field.JoinClause))
            //            {
            //                _joins.Add(field.JoinClause);
            //            }
            //            selectParts.Add($"{field.TableAlias}.{field.ColumnName} AS {field.Alias}");
            //        }
            //    }
            //    return string.Join(", ", selectParts);

            //}

            private string BuildSelect(SearchType searchType,
                                        List<int> columnIds,
                                        List<SearchColumnFilter> fields)
            {
                var cols = fields
                    .Where(f => columnIds.Contains(f.Id))
                    .Select(f => $"{searchType.MainTableAlias}.{f.ColumnName} AS [{f.DisplayName}]")
                    .ToList();

                return cols.Any() ? string.Join(", ", cols) : "t.*";
            }

            private string BuildWhere(SearchType searchType,
                                       List<FilterRuleDto> filters,
                                       List<SearchColumnFilter> fields,
                                       List<FilterOperator> operators)
            {
                if (!filters.Any()) return "";

                var sb = new StringBuilder();
                sb.Append(" AND (");

                for (int i = 0; i < filters.Count; i++)
                {
                    var f = filters[i];
                    var field = fields.First(x => x.Id == f.FieldId);
                    var op = operators.First(x => x.Id == f.OperatorId);

                    //TODO join is multiple
                    //if (!string.IsNullOrEmpty(field.SearchTypeId))
                    //    _joins.Add(field.JoinSql);

                    var condition = BuildCondition(searchType, field, op, f.Values);

                    if (i > 0)
                        sb.Append($" {f.Logic} ");

                    sb.Append(condition);
                }

                sb.Append(")");
                return sb.ToString();
            }


            private string BuildCondition(SearchType searchType,
                                           SearchColumnFilter field,
                                           FilterOperator op,
                                           List<string> values)
            {
                var col = $"{searchType.MainTableAlias}.{field.ColumnName}";
                var sql = op.SqlTemplate.Replace("{col}", col);

                if (op.ValueCount == 0)
                    return sql;

                if (op.ValueCount == 1)
                {
                    var p = $"@p{_paramIndex++}";
                    _parameters.Add(new SqlParameter(p, values[0]));
                    return sql.Replace("@p{n}", p);
                }

                if (op.ValueCount == 2)
                {
                    var p1 = $"@p{_paramIndex++}";
                    var p2 = $"@p{_paramIndex++}";

                    _parameters.Add(new SqlParameter(p1, values[0]));
                    _parameters.Add(new SqlParameter(p2, values[1]));

                    return sql
                        .Replace("@p{n}", p1)
                        .Replace("@p{n2}", p2);
                }

                throw new NotSupportedException();
            }

        }

    }
}
