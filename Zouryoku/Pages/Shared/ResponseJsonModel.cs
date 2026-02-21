using Model.Enums;

namespace Zouryoku.Pages.Shared
{
    public class ResponseJsonModel
    {
        public ResponseStatus Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}
