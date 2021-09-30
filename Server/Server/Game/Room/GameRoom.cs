﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Server.Data;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomID { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();

        public void Init(int mapID)
        {
            Map.LoadMap(mapID);
        }

        public void Update()
        {
            lock (_lock)
            {
                foreach (Projectile projectile in _projectiles.Values)
                {
                    projectile.Update();
                }
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if (null == gameObject)
                return;

            GameObjectType type = ObjectManager.GetObjectType(gameObject.ID);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player newPlayer = gameObject as Player;
                    _players.Add(newPlayer.Info.ObjectID, newPlayer);
                    newPlayer.Room = this;

                    {
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = newPlayer.Info;
                        newPlayer.Session.Send(enterPacket);

                        S_Spawn spawnPacket = new S_Spawn();
                        foreach (Player player in _players.Values)
                        {
                            if (newPlayer != player)
                                spawnPacket.Objects.Add(player.Info);
                        }
                        newPlayer.Session.Send(spawnPacket);
                    }
                }

                else if (type == GameObjectType.Monster)
                {
                    Monster newMonster = gameObject as Monster;
                    _monsters.Add(newMonster.Info.ObjectID, newMonster);
                    newMonster.Room = this;
                }
                else if (type == GameObjectType.Projectile)
                {
                    Projectile newProjectile = gameObject as Projectile;
                    _projectiles.Add(newProjectile.Info.ObjectID, newProjectile);
                    newProjectile.Room = this;
                }

                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(gameObject.Info);
                    foreach (Player player in _players.Values)
                    {
                        if (player.ID != gameObject.ID)
                            player.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void LeaveGame(int objectID)
        {
            GameObjectType type = ObjectManager.GetObjectType(objectID);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = null;

                    if (_players.Remove(objectID, out player) == false)
                        return;

                    player.Room = null;
                    Map.ApplyLeave(player);

                    {
                        S_LeaveGame leavePacket = new S_LeaveGame();
                        player.Session.Send(leavePacket);
                    }
                }

                else if (type == GameObjectType.Monster)
                {
                    Monster monster = null;

                    if (_monsters.Remove(objectID, out monster) == false)
                        return;

                    monster.Room = null;
                    Map.ApplyLeave(monster);
                }

                else if (type == GameObjectType.Projectile)
                {
                    Projectile projectile = null;

                    if (_projectiles.Remove(objectID, out projectile) == false)
                        return;

                    projectile.Room = null;
                }

                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectIDs.Add(objectID);
                    foreach (Player p in _players.Values)
                    {
                        if (p.ID != objectID)
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
                ObjectInfo info = player.Info;

                if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                        return;
                }

                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectID = player.Info.ObjectID;
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
                ObjectInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                info.PosInfo.State = CreatureState.Skill;

                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectID = info.ObjectID;
                skill.Info.SkillID = skillPacket.Info.SkillID;
                Broadcast(skill);

                Data.Skill skillData = null;
                if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillID, out skillData) == false)
                    return;

                switch (skillData.skillType)
                {
                    case SkillType.SkillAuto:
                        {
                            Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                            GameObject target = Map.Find(skillPos);
                            if (target != null)
                            {

                            }
                        }
                        break;
                    case SkillType.SkillProjectile:
                        {
                            Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                            if (null == arrow)
                                return;

                            arrow.Owner = player;
                            arrow.Data = skillData;

                            arrow.PosInfo.State = CreatureState.Moving;
                            arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                            arrow.PosInfo.PosX = player.PosInfo.PosX;
                            arrow.PosInfo.PosY = player.PosInfo.PosY;
                            arrow.Speed = skillData.projectile.speed;

                            EnterGame(arrow);
                        }
                        break;
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
