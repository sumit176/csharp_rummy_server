using System.Collections.Generic;

namespace CommonData.Data.Response
{
    public sealed class TurnDeciderData
    {
        public Dictionary<string, string> PlayerCardMap { get; set; } = new Dictionary<string, string>();
    }
}