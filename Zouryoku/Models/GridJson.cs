namespace Zouryoku.Models
{
    public class GridJson<A>
    {
        public required IEnumerable<A> Data { get; set; }
        public required int ItemsCount { get; set; }
        public List<long> IdList { get; set; } = [];
    }
}
