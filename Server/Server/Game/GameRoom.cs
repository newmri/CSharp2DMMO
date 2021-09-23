using Google.Protobuf;
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

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (null == player)
                return;

            lock (_lock)
            {
                // TODO: Valid Check

                PlayerInfo info = player.Info;
                info.PosInfo = movePacket.PosInfo;

                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerID = player.Info.PlayerID;
                resMovePacket.PosInfo = movePacket.PosInfo;

                Broadcast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (null == player)
                return;

            lock (_lock)
            {
                PlayerInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                // TODO: 스킬 사용 가능 여부 체크
                info.PosInfo.State = CreatureState.Skill;

                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.PlayerID = info.PlayerID;
                skill.Info.SkillID = 1;
                Broadcast(skill);
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player player in _players)
                {
                    player.Session.Send(packet);
                }
            }
        }
    }
}
