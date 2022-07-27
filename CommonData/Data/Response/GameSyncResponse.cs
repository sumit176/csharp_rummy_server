namespace CommonData.Data.Response
{
    public class GameSyncResponse
    {
        public string ClosedDeck { get; set; }
        public string OpenDeck { get; set; }
        public CardsResponseData PlayerHand { get; set; }
        public string PlayerTurn { get; set; }
    }
}