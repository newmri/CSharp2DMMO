using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        _spriteRenderer.flipX = false;

        if (CreatureState.Idle == _state)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_BACK");
                    break;
                case MoveDir.Down:
                    _animator.Play("IDLE_FRONT");
                    break;
                case MoveDir.Left:
                    _animator.Play("IDLE_RIGHT");
                    _spriteRenderer.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("IDLE_RIGHT");
                    break;
            }
        }
        else if (CreatureState.Moving == _state)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    break;
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    _spriteRenderer.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    break;
            }
        }
        else if (CreatureState.Skill == _state)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
                    break;
                case MoveDir.Down:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
                    break;
                case MoveDir.Left:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    _spriteRenderer.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    break;
            }
        }
        else
        {

        }

    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                GetIdleInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        GetDirInput();
        base.UpdateController();
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10.0f);
    }

    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            Dir = MoveDir.None;
        }
    }

    void GetIdleInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            State = CreatureState.Skill;
            // _coroutineSkill = StartCoroutine("CoStartPunch");
            _coroutineSkill = StartCoroutine("CoStartShootArrow");

        }
    }

    IEnumerator CoStartPunch()
    {
        _rangedSkill = false;
        GameObject target = Managers.Object.Find(GetFrontCellPos());

        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Idle;
        _coroutineSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        _rangedSkill = true;
        GameObject arrow = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController ArrowController = arrow.GetComponent<ArrowController>();
        ArrowController.Dir = _lastDir;
        ArrowController.CellPos = CellPos;

        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coroutineSkill = null;
    }

    Coroutine _coroutineSkill;
    bool _rangedSkill = false;
}
