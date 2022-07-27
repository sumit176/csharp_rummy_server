using CommonData.Enums;

namespace CommonData.Data.Request
{
    public sealed class PlayerActionData
    {
        public PlayerActionType ActionType { get; set; }
        public DeckType DeckType { get; set; }
        public string MoveId { get; set; }
    }
}