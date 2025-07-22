namespace EventManagementSystem.API.Models
{
    public class PaginationParameters
    {
        private const int MaxPageSize = 100;
        private int pageSize = 20;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => this.pageSize;
            set => this.pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? SearchTerm { get; set; }

        public string? SortBy { get; set; }

        public bool Ascending { get; set; } = true;
    }
}
