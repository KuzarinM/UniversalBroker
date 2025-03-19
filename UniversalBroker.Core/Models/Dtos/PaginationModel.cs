namespace UniversalBroker.Core.Models.Dtos
{
    public class PaginationModel<T>
    {
        public int CurrentPage { get; set; } 

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public List<T> Page {  get; set; }  
    }
}
