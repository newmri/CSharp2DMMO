using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        
        // [UNUSED(1)][TYPE(7)][ID(24)]
        int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                gameObject.ID = GenerateID(gameObject.ObjectType);

                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    _players.Add(gameObject.ID, gameObject as Player);
                }
            }

            return gameObject;
        }

        int GenerateID(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24) | (_counter++);
            }
        }

        public static GameObjectType GetObjectType(int ID)
        {
            int type = (ID >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public bool Remove(int objectID)
        {
            GameObjectType objectType = GetObjectType(objectID);

            lock (_lock)
            {
                if(objectType == GameObjectType.Player)
                    return _players.Remove(objectID);
            }

            return false;
        }

        public Player Find(int objectID)
        {
            GameObjectType objectType = GetObjectType(objectID);

            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.TryGetValue(objectID, out player))
                        return player;
                }
            }

            return null;
        }
    }
}
