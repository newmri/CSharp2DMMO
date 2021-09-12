using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{

    public void Add(GameObject gameObject)
    {
        _objects.Add(gameObject);
    }

    public void Remove(GameObject gameObject)
    {
        _objects.Remove(gameObject);
    }

    public GameObject Find(Vector3Int cellPos)
    {
        foreach (GameObject gameObject in _objects)
        {
            CreatureController creatureController = gameObject.GetComponent<CreatureController>();
            if (null == creatureController)
                continue;

            if (creatureController.CellPos == cellPos)
                return gameObject;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject gameObject in _objects)
        {
            CreatureController creatureController = gameObject.GetComponent<CreatureController>();
            if (null == creatureController)
                continue;

            if (condition.Invoke(gameObject))
                return gameObject;
        }

        return null;
    }

    public void Clear()
    {
        _objects.Clear();
    }

    List<GameObject> _objects = new List<GameObject>();
}
