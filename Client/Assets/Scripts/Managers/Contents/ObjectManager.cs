using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	public MyPlayerController MyPlayer { get; set; }

	public static GameObjectType GetObjectType(int ID)
	{
		int type = (ID >> 24) & 0x7F;
		return (GameObjectType)type;
	}

	public void Add(ObjectInfo info, bool myPlayer = false)
	{
		GameObjectType objectType = GetObjectType(info.ObjectID);

		if (objectType == GameObjectType.Player)
		{
			if (myPlayer)
			{
				GameObject gameObject = Managers.Resource.Instantiate("Creature/MyPlayer");
				gameObject.name = info.Name;
				_objects.Add(info.ObjectID, gameObject);
				MyPlayer = gameObject.GetComponent<MyPlayerController>();
				MyPlayer.ID = info.ObjectID;
				MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.Stat = info.StatInfo;
				MyPlayer.SyncPos();
			}

			else
			{
				GameObject gameObject = Managers.Resource.Instantiate("Creature/Player");
				gameObject.name = info.Name;
				_objects.Add(info.ObjectID, gameObject);
				PlayerController playerController = gameObject.GetComponent<PlayerController>();
				playerController.ID = info.ObjectID;
				playerController.PosInfo = info.PosInfo;
				playerController.Stat = info.StatInfo;

				playerController.SyncPos();
			}
		}

		else if (objectType == GameObjectType.Monster)
		{

		}
		else if (objectType == GameObjectType.Projectile)
		{
			GameObject gameObject = Managers.Resource.Instantiate("Creature/Arrow");
			gameObject.name = "Arrow";
			_objects.Add(info.ObjectID, gameObject);

			ArrowController arrowController = gameObject.GetComponent<ArrowController>();
			arrowController.PosInfo = info.PosInfo;
			arrowController.Stat = info.StatInfo;
			arrowController.SyncPos();
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
