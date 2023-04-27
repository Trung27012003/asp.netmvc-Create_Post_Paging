namespace Poster.Models
{
    public class Paging
    {
        public int CurrentPage { get; set; }
        public int CountPage { get; set; }
        public Func<int?, string> GeneralUrl { get; set; }
    }
}
