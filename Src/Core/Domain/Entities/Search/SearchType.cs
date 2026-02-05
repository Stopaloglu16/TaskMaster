using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Search
{
    public class SearchType : BaseEntity<int>
    {
        [Column(TypeName = "varchar(50)")]
        public required string TypeName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public required string MainTable { get; set; }

        [Column(TypeName = "varchar(10)")]
        public required string MainTableAlias { get; set; }

        public virtual IList<SearchColumnFilter> SearchColumnFilters { get; private set; } = new List<SearchColumnFilter>();
    }
}
