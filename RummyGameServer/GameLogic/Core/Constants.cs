namespace RummyGameServer
{
    public class Constants
    {
        public const int GameWaitTime = 10; //Seconds
        public static int PlayerReconnectionTimeout = 30; //Seconds
        public static int TurnTime = 10; //Seconds
        
        //Client Events
        public const string JoinGame = "JoinGame";
        public const string WaitingForStart = "WaitingForStart";
        public const string StartGame = "StartGame";
        public const string TurnSync = "TurnSync";
        public const string DealCards = "CardDistribute";
        public const string TurnDecide = "StartTurn";
        public const string ExitGame = "ExitGame";
        public const string PickOrDropCard = "PickOrDropCard";
    }
}