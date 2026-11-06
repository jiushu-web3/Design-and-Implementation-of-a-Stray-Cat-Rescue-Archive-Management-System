using System.Collections.Generic;

namespace StrayCat.Model
{
    public class PagedResult<T>
    {
        public PagedResult()
        {
            Items = new List<T>();
            PageIndex = 1;
            PageSize = 10;
        }

        public IList<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
