using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
        _speed = Random.Range(5.0f, 15.0f);

        switch (_lastDir)
        {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                break;
            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, -180.0f);
                break;
            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                break;
            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
                break;
        }

        base.Init();
    }

    protected override void UpdateAnimation()
    {

    }

    protected override void UpdateIdle()
    {
        if (_dir != MoveDir.None)
        {
            Vector3Int destPos = CellPos;

            switch (_dir)
            {
                case MoveDir.Up:
                    destPos += Vector3Int.up;
                    break;
                case MoveDir.Down:
                    destPos += Vector3Int.down;
                    break;
                case MoveDir.Left:
                    destPos += Vector3Int.left;
                    break;
                case MoveDir.Right:
                    destPos += Vector3Int.right;
                    break;
            }

            State = CreatureState.Moving;

            if (Managers.Map.CanMove(destPos))
            {
                GameObject target = Managers.Object.Find(destPos);
                if (null == target)
                {
                    CellPos = destPos;
                }
                else
                {
                    Managers.Resource.Destroy(gameObject);
                }
            }
            else
            {
                Managers.Resource.Destroy(gameObject);
            }
        }
    }
}
