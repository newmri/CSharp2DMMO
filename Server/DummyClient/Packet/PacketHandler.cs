using ServerCore;
using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;

class PacketHandler
{
	// Step4
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;
	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;
	}

	// Step1
	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		C_Login loginPacket = new C_Login();
		ServerSession serverSession = session as ServerSession;

		loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
		serverSession.Send(loginPacket);
	}

	// Step2
	// Login OK + Character List
	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = packet as S_Login;
		ServerSession serverSession = session as ServerSession;

		if (loginPacket.Players == null || loginPacket.Players.Count == 0)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player{serverSession.DummyId.ToString("0000")}";
			serverSession.Send(createPacket);
		}
		else
		{
			LobbyPlayerInfo info = loginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;
			serverSession.Send(enterGamePacket);
		}
	}

	// Step3
	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createPacket = packet as S_CreatePlayer;
		ServerSession serverSession = session as ServerSession;

		if (createPacket.Player == null)
		{
		}
		else
		{
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createPacket.Player.Name;
			serverSession.Send(enterGamePacket);
		}
	}

	public static void S_ItemListHandler(PacketSession session, IMessage packet)
	{
		S_ItemList itemList = packet as S_ItemList;
	}

	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem addItem = packet as S_AddItem;
	}

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItem = packet as S_EquipItem;
	}

	public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
	{
		S_ChangeStat changeStat = packet as S_ChangeStat;
	}

	public static void S_PingHandler(PacketSession session, IMessage packet)
	{
		C_Pong pongPacket = new C_Pong();
	}
}


