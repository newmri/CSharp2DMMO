﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        // test
        Managers.Web.BaseUrl = "Https://localhost:5001/api";
        WebPacket.SendCreateAccount("test", "1234");

        Managers.Map.LoadMap(1);

        Screen.SetResolution(640, 480, false);

        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {
        
    }
}
