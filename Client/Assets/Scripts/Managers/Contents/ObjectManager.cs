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
			MyPlayer.PosInfo = info.PosInfo;
		}

		else
		{
			GameObject gameObject = Managers.Resource.Instantiate("Creature/Player");
			gameObject.name = info.Name;
			_objects.Add(info.PlayerID, gameObject);
			PlayerController playerController = gameObject.GetComponent<PlayerController>();
			playerController.ID = info.PlayerID;
			playerController.PosInfo = info.PosInfo;
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
		GameObject gameObject = Find(id);
		if (null == gameObject)
			return;

		_objects.Remove(id);
		Managers.Resource.Destroy(gameObject);
	}

	public GameObject Find(int id)
	{
		GameObject gameObject = null;
		_objects.TryGetValue(id, out gameObject);
		return gameObject;
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
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);

		_objects.Clear();
	}
}
