using CommonData.Enums;

namespace CommonData.Data.Response
{
    public class CardPickDropResponse
    {
        public string ClosedDeck { get; set; }
        public string OpenDeck { get; set; }
        public string CardId { get; set; }
        public string PlayerTurn { get; set; }
        public PlayerActionType PlayerAction { get; set; }
    }
}