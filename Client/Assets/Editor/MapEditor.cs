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
            Tilemap tileBase = Util.FindChild<Tilemap>(gameObject, "Tilemap_Base", true);
            Tilemap tileMap = Util.FindChild<Tilemap>(gameObject, "Tilemap_Collision", true);

            using (var writer = File.CreateText($"Assets/Resources/Map/{gameObject.name}.txt"))
            {
                writer.WriteLine(tileBase.cellBounds.xMin);
                writer.WriteLine(tileBase.cellBounds.xMax);
                writer.WriteLine(tileBase.cellBounds.yMin);
                writer.WriteLine(tileBase.cellBounds.yMax);

                for (int y = tileBase.cellBounds.yMax; y >= tileBase.cellBounds.yMin; --y)
                {
                    for (int x = tileBase.cellBounds.xMin; x <= tileBase.cellBounds.xMax; ++x)
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
