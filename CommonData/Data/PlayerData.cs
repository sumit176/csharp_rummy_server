using System;

namespace CommonData.Data
{
    public sealed class PlayerData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int AvatarId { get; set; }
        public string IconUrl { get; set; }
        public double AccountBalance { get; set; }
    }
}