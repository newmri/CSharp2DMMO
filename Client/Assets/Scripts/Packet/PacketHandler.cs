﻿using Google.Protobuf;
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
		Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
		Managers.Object.Clear();
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
		foreach (ObjectInfo obj in spawnPacket.Objects)
		{
			Managers.Object.Add(obj, myPlayer: false);
		}
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
		foreach (int id in despawnPacket.ObjectIds)
		{
			Managers.Object.Remove(id);
		}
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;

		GameObject go = Managers.Object.FindById(movePacket.ObjectId);
		if (go == null)
			return;

		if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
			return;

		BaseController bc = go.GetComponent<BaseController>();
		if (bc == null)
			return;

		bc.PosInfo = movePacket.PosInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;

		GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.UseSkill(skillPacket.Info.SkillId);
		}
	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;

		GameObject go = Managers.Object.FindById(changePacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.Hp = changePacket.Hp;
		}
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;

		GameObject go = Managers.Object.FindById(diePacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null)
		{
			cc.Hp = 0;
			cc.OnDead();
		}
	}

	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_ConnectedHandler");
		C_Login loginPacket = new C_Login();

		string path = Application.dataPath;
		loginPacket.UniqueId = path.GetHashCode().ToString();
		Managers.Network.Send(loginPacket);
	}

	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = packet as S_Login;
		Debug.Log($"LoginOK({loginPacket.LoginOk})");

		if (loginPacket.Players == null || loginPacket.Players.Count == 0)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createPacket);
		}
		else
		{
			LobbyPlayerInfo info = loginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;
			Managers.Network.Send(enterGamePacket);
		}
	}

	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createPacket = packet as S_CreatePlayer;
		if (createPacket.Player == null)
		{
			C_CreatePlayer createReqPacket = new C_CreatePlayer();
			createReqPacket.Name = $"Player{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createReqPacket);
		}
		else
		{
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createPacket.Player.Name;
			Managers.Network.Send(enterGamePacket);
		}
	}

	public static void S_ItemListHandler(PacketSession session, IMessage packet)
	{
		S_ItemList itemList = packet as S_ItemList;

		Managers.Inventory.Clear();

		foreach (ItemInfo itemInfo in itemList.Items)
		{
			Item item = Item.MakeItem(itemInfo);
			Managers.Inventory.Add(item);
		}

		if(Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem addItem = packet as S_AddItem;

		foreach (ItemInfo itemInfo in addItem.Items)
		{
			Item item = Item.MakeItem(itemInfo);
			Managers.Inventory.Add(item);
		}

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		gameSceneUI.StatUI.RefreshUI();

		if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItem = packet as S_EquipItem;

		Item item = Managers.Inventory.Get(equipItem.ItemDbId);
		if (item == null)
			return;

		item.Equipped = equipItem.Equipped;

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		gameSceneUI.StatUI.RefreshUI();

		if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionalStat();
	}

	public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
	{
		S_ChangeStat changeStat = packet as S_ChangeStat;
	}

	public static void S_PingHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("PingCheck");
		C_Pong pongPacket = new C_Pong();
		Managers.Network.Send(pongPacket);
	}
}


