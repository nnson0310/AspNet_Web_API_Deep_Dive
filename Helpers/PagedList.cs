using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }

        public int TotalPages { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public bool HasNext => CurrentPage < TotalPages;

        public bool HasPrevious => CurrentPage > 1 && CurrentPage <= TotalPages + 1;

        private PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            AddRange(items);
        }

        public static async Task<PagedList<T>> CreateListAsync(IQueryable<T> collection, int pageNumber, int pageSize)
        {
            var items = await collection
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize).ToListAsync();

            return new PagedList<T>(items, collection.Count(), pageNumber, pageSize);
        }
    }
}
