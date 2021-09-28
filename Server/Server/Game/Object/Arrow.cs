﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        long _nextMoveTick = 0;

        public override void Update()
        {
            if (Owner == null || Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            _nextMoveTick = Environment.TickCount64 + 50;
            Vector2Int destPos = GetFrontCellPos();
            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectID = ID;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(movePacket);
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {

                }

                Room.LeaveGame(ID);
            }
        }
    }

}
