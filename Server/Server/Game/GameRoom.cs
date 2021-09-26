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

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Map _map = new Map();

        public void Init(int mapID)
        {
            _map.LoadMap(mapID);
        }

        public void EnterGame(Player newPlayer)
        {
            if (null == newPlayer)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer.Info.PlayerID, newPlayer);
                newPlayer.Room = this;

                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player player in _players.Values)
                    {
                        if (newPlayer != player)
                            spawnPacket.Players.Add(player.Info);
                    }
                    newPlayer.Session.Send(spawnPacket);
                }

                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player player in _players.Values)
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
                Player player = null;

                if (_players.Remove(playerID, out player) == false)
                    return;

                player.Room = null;

                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIDs.Add(player.Info.PlayerID);
                    foreach (Player p in _players.Values)
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
                PositionInfo movePosInfo = movePacket.PosInfo;
                PlayerInfo info = player.Info;

                if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    if (_map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                        return;
                }

                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                _map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

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

                Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                Player target = _map.Find(skillPos);
                if (target != null)
                {

                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player player in _players.Values)
                {
                    player.Session.Send(packet);
                }
            }
        }
    }
}
