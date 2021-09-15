using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomID { get; set; }

        List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer)
        {
            if (null == newPlayer)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;

                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player player in _players)
                    {
                        if (newPlayer != player)
                            spawnPacket.Players.Add(player.Info);
                    }
                    newPlayer.Session.Send(spawnPacket);
                }

                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player player in _players)
                    {
                        if (newPlayer != player)
                            player.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void LeaveGame(int playerID)
        {
            lock (_lock)
            {
                Player player = _players.Find(p => p.Info.PlayerID == playerID);
                if (null == player)
                    return;

                _players.Remove(player);
                player.Room = null;

                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIDs.Add(player.Info.PlayerID);
                    foreach (Player p in _players)
                    {
                        if (player != p)
                            p.Session.Send(despawnPacket);
                    }
                }
            }
        }
    }
}
