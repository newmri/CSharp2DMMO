using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{
#if UNITY_EDITOR
    [MenuItem("Tools/GenerateMap %#g")]
    private static void GenerateMap()
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject gameObject in gameObjects)
        {
            Tilemap tileMap = Util.FindChild<Tilemap>(gameObject, "Tilemap_Collision", true);

            using (var writer = File.CreateText($"Assets/Resources/Map/{gameObject.name}.txt"))
            {
                writer.WriteLine(tileMap.cellBounds.xMin);
                writer.WriteLine(tileMap.cellBounds.xMax);
                writer.WriteLine(tileMap.cellBounds.yMin);
                writer.WriteLine(tileMap.cellBounds.yMax);

                for (int y = tileMap.cellBounds.yMax; y >= tileMap.cellBounds.yMin; --y)
                {
                    for (int x = tileMap.cellBounds.xMin; x <= tileMap.cellBounds.xMax; ++x)
                    {
                        TileBase tile = tileMap.GetTile(new Vector3Int(x, y, 0));
                        if (tile)
                            writer.Write("0");
                        else
                            writer.Write("1");
                    }
                    writer.WriteLine();
                }
            }
        }    
    }
#endif
}
