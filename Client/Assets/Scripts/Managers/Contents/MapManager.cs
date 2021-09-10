using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapManager
{
    public Grid CurrentGrid { get; private set; }

    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    bool[,] _collision;

    public void LoadMap(int mapID)
    {
        DestroyMap();

        string mapName = "Map_" + mapID.ToString("000");
        GameObject gameObject = Managers.Resource.Instantiate($"Map/{mapName}");
        gameObject.name = "Map";

        GameObject collision = Util.FindChild(gameObject, "Tilemap_Collision", true);
        if (collision)
            collision.SetActive(false);

        CurrentGrid = gameObject.GetComponent<Grid>();

        // Collision
        TextAsset text = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader reader = new StringReader(text.text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new bool[yCount, xCount];

        for (int y = 0; y < yCount; ++y)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; ++x)
            {
                _collision[y, x] = (line[x] == '1' ? true : false);
            }
        }
    }

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if (map)
        {
            GameObject.Destroy(map);
            CurrentGrid = null;
        }
    }

    public bool CanMove(Vector3Int cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return false;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return false;

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;

        return _collision[y, x];
    }
}
