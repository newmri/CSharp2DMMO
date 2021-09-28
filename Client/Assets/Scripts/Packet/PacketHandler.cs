using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;

		Managers.Object.Add(enterGamePacket.Player, true);
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame enterLeavePacket = packet as S_LeaveGame;

		Managers.Object.RemoveMyPlayer();
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;

		foreach (ObjectInfo player in spawnPacket.Objects)
		{
			Managers.Object.Add(player);
		}
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;

		foreach (int ID in despawnPacket.ObjectIDs)
		{
			Managers.Object.Remove(ID);
		}
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
		ServerSession serverSession = session as ServerSession;

		GameObject gameObject = Managers.Object.Find(movePacket.ObjectID);
		if (null == gameObject)
			return;

		CreatureController creatureController = gameObject.GetComponent<CreatureController>();
		if (null == creatureController)
			return;

		creatureController.PosInfo = movePacket.PosInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;

		GameObject gameObject = Managers.Object.Find(skillPacket.ObjectID);
		if (null == gameObject)
			return;

		PlayerController playerController = gameObject.GetComponent<PlayerController>();
		if (null == playerController)
			return;

		playerController.UseSkill(skillPacket.Info.SkillID);
	}
}
