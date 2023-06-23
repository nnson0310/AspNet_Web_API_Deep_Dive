using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }

        public int TotalPages { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public bool HasNext
        {
            get => CurrentPage < TotalPages;
        }

        public bool HasPrevious
        {
            get => CurrentPage > 1;
        }

        private PagedList(List<T> items, int currentPage, int pageSize)
        {
            TotalCount = items.Count();
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            AddRange(items);
        }

        public static async Task<PagedList<T>> CreateListAsync(IQueryable<T> collection, int currentPage, int pageSize)
        {
            var items = await collection
                .Skip(pageSize * (currentPage - 1))
                .Take(pageSize).ToListAsync();

            return new PagedList<T>(items, currentPage, pageSize);
        }
    }
}
