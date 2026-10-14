using System;

namespace AcVelocity.Models
{
    public class ServerInfo
    {
        public string Name { get; set; } = "Community Server";
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9600;
        public int HttpPort { get; set; } = 8081;
        public string Track { get; set; } = "Unknown Track";
        public int Clients { get; set; } = 0;
        public int MaxClients { get; set; } = 24;
        public int PingMs { get; set; } = 45;
        public bool IsOnline { get; set; } = true;
        public string Password { get; set; } = string.Empty;
        public string Country { get; set; } = "GLOBAL";

        public string PlayersDisplay => $"{Clients}/{MaxClients} PILOTS";
        public string AddressDisplay => $"{Ip}:{Port}";
        public bool HasSlots => Clients < MaxClients;
    }
}
