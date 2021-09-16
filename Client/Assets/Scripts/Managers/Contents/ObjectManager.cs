using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	public MyPlayerController MyPlayer { get; set; }

	public void Add(PlayerInfo info, bool myPlayer = false)
	{
		if (myPlayer)
		{
			GameObject gameObject = Managers.Resource.Instantiate("Creature/MyPlayer");
			gameObject.name = info.Name;
			_objects.Add(info.PlayerID, gameObject);
			MyPlayer = gameObject.GetComponent<MyPlayerController>();
			MyPlayer.ID = info.PlayerID;
			MyPlayer.CellPos = new Vector3Int(info.PosX, info.PosY, 0);
		}

		else
		{
			GameObject gameObject = Managers.Resource.Instantiate("Creature/Player");
			gameObject.name = info.Name;
			_objects.Add(info.PlayerID, gameObject);
			PlayerController playerController = gameObject.GetComponent<PlayerController>();
			playerController.ID = info.PlayerID;
			playerController.CellPos = new Vector3Int(info.PosX, info.PosY, 0);
		}
	}

	public void Add(int id, GameObject go)
	{
		_objects.Add(id, go);
	}

	public void RemoveMyPlayer()
	{
		if (MyPlayer)
		{
			Remove(MyPlayer.ID);
			MyPlayer = null;
		}
	}

	public void Remove(int id)
	{
		_objects.Remove(id);
	}

	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		_objects.Clear();
	}
}
