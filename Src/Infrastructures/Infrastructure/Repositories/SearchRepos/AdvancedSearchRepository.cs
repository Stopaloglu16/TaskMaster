using Application.Aggregates.SearchAggregate.Queries;
using Application.Repositories;
using Domain.Entities.SearchEntities;
using Domain.Enums;
using Infrastructure.Data;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.SearchRepos
{
    public class AdvancedSearchRepository : EfCoreRepository<AdvancedSearch, int>, IAdvancedSearchRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public AdvancedSearchRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<AdvancedSearchResponseDto> RunAdvancedSearchAsync(AdvancedSearchRequestDto request,
                                                                             CancellationToken cancellationToken)
        {
            int stepNo = 0;
            try
            {

                stepNo = 1;
                // 1️⃣ Load report metadata
                var report = await _dbContext.AdvancedSearches
                    .Include(x => x.MainTable)
                    .Include(x => x.AdvancedSearchColumns)
                        .ThenInclude(rc => rc.ColumnDefinition)
                            .ThenInclude(cd => cd.Table)
                    .FirstAsync(x => x.Id == request.AdvancedSearchId);

                var columnMap = report.AdvancedSearchColumns
                    .ToDictionary(x => x.Id);

                var operatorMap = await _dbContext.Operators
                    .ToDictionaryAsync(x => x.Code);


                stepNo = 2;
                // 2️⃣ Determine required tables
                var requiredTables = CollectRequiredTables(request, columnMap);

                stepNo = 3;
                // 3️⃣ Build SQL
                var selectSql = BuildSelect(request, columnMap);
                var fromSql = BuildFrom(report.MainTable);
                var joinSql = BuildJoins(report.MainTable, requiredTables);
                var (whereSql, parameters) = new WhereBuilder()
                    .Build(request.Filters, columnMap, operatorMap);
                var orderSql = BuildOrder(request, columnMap);
                var pagingSql = BuildPaging(request);

                var dataSql = $@"
        {selectSql}
        {fromSql}
        {joinSql}
        {whereSql}
        {orderSql}
        {pagingSql}
    ";

                var countSql = $@"
        SELECT COUNT(1)
        {fromSql}
        {joinSql}
        {whereSql}
    ";

                stepNo = 4;
                // 4️⃣ Execute count
                var total = await _dbContext.Database
                    .SqlQueryRaw<int>(countSql, parameters.ToArray())
                    .FirstAsync();

                stepNo = 5;
                // 5️⃣ Execute data
                var rows = await _dbContext.Database
                    .SqlQueryRaw<object[]>(dataSql, parameters.ToArray())
                    .ToListAsync();

                stepNo = 6;
                // 6️⃣ Build response columns (ORDER MATTERS)
                var responseColumns = request.SelectedColumnIds //SelectedReportColumnIds
                    .Select(id =>
                    {
                        var col = columnMap[id];
                        return new AdvancedSearchColumnDto
                        {
                            Id = col.Id,
                            Name = col.ColumnDefinition.DisplayName
                        };
                    })
                    .ToList();

                return new AdvancedSearchResponseDto
                {
                    Columns = responseColumns,
                    Rows = rows,
                    TotalCount = total
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(stepNo);

                return new AdvancedSearchResponseDto();

            }

        }

        private string BuildSelect(AdvancedSearchRequestDto request,
                                   Dictionary<int, AdvancedSearchColumn> columnMap)
        {
            var columns = request.SelectedColumnIds
                .Select(id =>
                {
                    var col = columnMap[id];
                    var tableAlias = col.ColumnDefinition.Table.TableAlias;
                    var columnName = col.ColumnDefinition.ColumnName;

                    return $"{tableAlias}.{columnName}";
                });

            return "SELECT " + string.Join(", ", columns);
        }


        private string BuildFrom(AdvancedSearchTable mainTable)
        {
            return $"FROM {mainTable.TableName} {mainTable.TableAlias}";
        }


        private string BuildJoins(AdvancedSearchTable mainTable,
                                  HashSet<int> requiredTables)
        {
            var joins = new List<string>();
            var visited = new HashSet<int>();

            foreach (var tableId in requiredTables)
            {
                var current = _dbContext.AdvancedSearchTables
                    .Include(t => t.ParentTable)
                    .First(t => t.Id == tableId);

                while (current.ParentTableId != null &&
                       current.Id != mainTable.Id)
                {
                    if (!visited.Contains(current.Id))
                    {
                        var parent = current.ParentTable;

                        joins.Add($@" LEFT JOIN {parent.TableName} {parent.TableAlias}
                                           ON {current.TableAlias}.{current.ForeignKeyColumn}
                                            = {parent.TableAlias}.{parent.ParentKeyColumn}
                                            ");

                        visited.Add(current.Id);
                    }

                    current = current.ParentTable;
                }
            }

            return string.Join("\n", joins);
        }

        public class WhereBuilder
        {
            private readonly List<NpgsqlParameter> _parameters = new();
            private int _parameterIndex = 0;

            public (string sql, List<NpgsqlParameter> parameters) Build(
                FilterGroupDto filterGroup,
                Dictionary<int, AdvancedSearchColumn> columnMap,
                Dictionary<string, Operator> operatorMap)
            {
                if (filterGroup == null || !filterGroup.Items.Any())
                    return (string.Empty, _parameters);

                var conditions = new List<string>();

                foreach (var item in filterGroup.Items)
                {
                    if (item.Group != null && item.Group.Any())
                    {
                        var groupSql = BuildGroup(item, columnMap, operatorMap);
                        conditions.Add($"({groupSql})");
                    }
                    else
                    {
                        var singleSql = BuildSingle(item, columnMap, operatorMap);
                        conditions.Add(singleSql);
                    }
                }

                var joined = string.Join(
                    filterGroup.Logic == LogicalOperator.And ? " AND " : " OR ",
                    conditions);

                return ($"WHERE ({joined})", _parameters);
            }

           


            private string BuildSingle(FilterItemDto item,
                                       Dictionary<int, AdvancedSearchColumn> columnMap,
                                       Dictionary<string, Operator> operatorMap)
            {
                var column = columnMap[item.ColumnId.Value];
                var op = operatorMap[item.OperatorCode];

                var tableAlias = column.ColumnDefinition.Table.TableAlias;
                var columnName = column.ColumnDefinition.ColumnName;

                var fullColumn = $"{tableAlias}.{columnName}";

                if (op.ValueMode == OperatorValueMode.Single)
                {
                    var paramName = $"@p{_parameterIndex++}";
                    _parameters.Add(new NpgsqlParameter(paramName, item.Value ?? DBNull.Value));

                    return string.Format(op.SqlTemplate, fullColumn, paramName);
                }

                if (op.ValueMode == OperatorValueMode.Range)
                {
                    var paramFrom = $"@p{_parameterIndex++}";
                    var paramTo = $"@p{_parameterIndex++}";

                    _parameters.Add(new NpgsqlParameter(paramFrom, item.Value ?? DBNull.Value));
                    _parameters.Add(new NpgsqlParameter(paramTo, item.ValueTo ?? DBNull.Value));

                    return string.Format(op.SqlTemplate, fullColumn, paramFrom, paramTo);
                }

                throw new NotSupportedException("Unsupported ValueMode");
            }

            private string BuildGroup(FilterItemDto groupItem,
                                 Dictionary<int, AdvancedSearchColumn> columnMap,
                                 Dictionary<string, Operator> operatorMap)
            {
                var innerConditions = new List<string>();

                foreach (var item in groupItem.Group)
                {
                    innerConditions.Add(
                        BuildSingle(item, columnMap, operatorMap));
                }

                var logic = groupItem.GroupLogic == LogicalOperator.And
                    ? " AND "
                    : " OR ";

                return string.Join(logic, innerConditions);
            }
        }

       

        private string BuildOrder(AdvancedSearchRequestDto request,
                                 Dictionary<int, AdvancedSearchColumn> columnMap)
        {
            if (!request.Sorts.Any())
                return "";

            var orders = request.Sorts.Select(sort =>
            {
                var col = columnMap[sort.ReportColumnId];
                var tableAlias = col.ColumnDefinition.Table.TableAlias;
                var columnName = col.ColumnDefinition.ColumnName;

                return $"{tableAlias}.{columnName} " +
                       (sort.Descending ? "DESC" : "ASC");
            });

            return "ORDER BY " + string.Join(", ", orders);
        }
        private string BuildPaging(AdvancedSearchRequestDto request)
        {
            var skip = (request.Page - 1) * request.PageSize;

            return $@"
                        OFFSET {skip} ROWS
                        FETCH NEXT {request.PageSize} ROWS ONLY";
        }

        private HashSet<int> CollectRequiredTables(AdvancedSearchRequestDto request,
                                                   Dictionary<int, AdvancedSearchColumn> columnMap)
        {
            var tables = new HashSet<int>();

            foreach (var id in request.SelectedColumnIds)
                tables.Add(columnMap[id].ColumnDefinition.TableId);

            if (request.Filters != null)
            {
                foreach (var item in request.Filters.Items)
                {
                    if (item.ColumnId.HasValue)
                        tables.Add(columnMap[item.ColumnId.Value]
                            .ColumnDefinition.TableId);

                    if (item.Group != null)
                    {
                        foreach (var g in item.Group)
                            tables.Add(columnMap[g.ColumnId.Value]
                                .ColumnDefinition.TableId);
                    }
                }
            }

            return tables;
        }


    }
}
