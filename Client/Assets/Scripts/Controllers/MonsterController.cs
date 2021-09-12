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
        _rangedSkill = (Random.Range(0, 2)) == 0 ? true : false;

        if (_rangedSkill)
            _skillRange = Random.Range(15.0f, 20.0f);
        else
            _skillRange = 1.0f;
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

    [SerializeField]
    float _searchRange = 25.0f;
    [SerializeField]
    float _skillRange = 1.0f;
    [SerializeField]
    bool _rangedSkill = false;

    [SerializeField]
    GameObject _target;

    protected override void MoveToNextPos()
    {
        Vector3Int destPos = _destCellPos;
        if (_target)
        {
            destPos = _target.GetComponent<CreatureController>().CellPos;

            Vector3Int dir = destPos - CellPos;

            if (dir.magnitude <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                Dir = GetDirFromVec(dir);
                State = CreatureState.Skill;

                if(_rangedSkill)
                    _coroutineSkill = StartCoroutine("CoStartShootArrow");
                else
                    _coroutineSkill = StartCoroutine("CoStartPunch");

                return;
            }
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);
        if (path.Count < 2 || (_target && path.Count > 20))
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];

        Vector3Int moveCellDir = nextPos - CellPos;

        Dir = GetDirFromVec(moveCellDir);

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
    Coroutine _coroutineSkill;

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

    IEnumerator CoStartPunch()
    {
        GameObject target = Managers.Object.Find(GetFrontCellPos());
        if (target)
        {
            CreatureController creatrueController = target.GetComponent<CreatureController>();
            if (creatrueController)
                creatrueController.OnDamaged();
        }

        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Moving;
        _coroutineSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        GameObject arrow = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController ArrowController = arrow.GetComponent<ArrowController>();
        ArrowController.Dir = _lastDir;
        ArrowController.CellPos = CellPos;

        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Moving;
        _coroutineSkill = null;
    }
}
