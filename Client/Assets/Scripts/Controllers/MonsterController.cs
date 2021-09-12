using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        Dir = MoveDir.None;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (null == _coPatrol)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }

        if (null == _coSearch)
        {
            _coSearch = StartCoroutine("CoSearch");
        }
    }

    IEnumerator CoPatrol()
    {
        int waitForSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitForSeconds);

        for (int i = 0; i < 10; ++i)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange, 0);

            if (Managers.Map.CanMove(randPos) && null == Managers.Object.Find(randPos))
            {
                _destCellPos = randPos;
                State = CreatureState.Moving;
                yield break;
            }
        }

        State = CreatureState.Idle;
    }

    IEnumerator CoSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (_target)
                continue;

            _target = Managers.Object.Find((gameObject) =>
            {
                PlayerController playerController = gameObject.GetComponent<PlayerController>();
                if (null == playerController)
                    return false;

                Vector3Int dir = playerController.CellPos - CellPos;
                if (dir.magnitude > _searchRange)
                    return false;

                return true;
            });
        }
    }

    float _searchRange = 5.0f;
    [SerializeField]
    GameObject _target;

    protected override void MoveToNextPos()
    {
        Vector3Int destPos = _destCellPos;
        if (_target)
        {
            destPos = _target.GetComponent<CreatureController>().CellPos;
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);
        if (path.Count < 2 || (_target && path.Count > 10))
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];

        Vector3Int moveCellDir = nextPos - CellPos;

        if (moveCellDir.x > 0)
            Dir = MoveDir.Right;
        else if (moveCellDir.x < 0)
            Dir = MoveDir.Left;
        else if (moveCellDir.y > 0)
            Dir = MoveDir.Up;
        else if (moveCellDir.y < 0)
            Dir = MoveDir.Down;
        else
            Dir = MoveDir.None;

        State = CreatureState.Moving;

        if (Managers.Map.CanMove(nextPos) && null == Managers.Object.Find(nextPos))
        {
            CellPos = nextPos;
        }
        else
        {
            State = CreatureState.Idle;
        }
    }

    public override void OnDamaged()
    {
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f);

        Managers.Object.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }

    Coroutine _coPatrol;
    Coroutine _coSearch;

    [SerializeField]
    Vector3Int _destCellPos;

    public override CreatureState State
    {
        get { return _state; }
        set
        {
            if (value == _state)
                return;

            base.State = value;

            if (null != _coPatrol)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            if (null != _coSearch)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

}
