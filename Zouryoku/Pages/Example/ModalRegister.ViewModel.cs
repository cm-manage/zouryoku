using Microsoft.AspNetCore.Mvc;

namespace Zouryoku.Pages.Example
{
    public partial class ModalRegisterModel
    {
        public class ViewModel
        {
            public List<Card> Cards { get; init; } = [];
        }

        public class Card
        {
            public required string Name { get; init; }
        }
    }
}
