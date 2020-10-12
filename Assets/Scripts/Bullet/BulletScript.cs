﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private CharacterAnimStateEnum charaShootAnimState;
    private Bullet bullet = new Bullet();
    private BulletAnimStateEnum animState = BulletAnimStateEnum.Bullet_Appear;


    //  Start
    void Start()
    {
        SetAnimation(BulletAnimStateEnum.Bullet_Appear);
        StartCoroutine(EndAnimationCoroutine(bullet.GetBulletAppearAnimationTime(), BulletAnimStateEnum.Bullet_Travel));
    }


    //  Getters
    public CharacterAnimStateEnum GetCharaShootAnimState()
    {
        return charaShootAnimState;
    }


    //  Setters
    public void SetCharaShootAnimState(CharacterAnimStateEnum arg_charaShootAnimState)
    {
        charaShootAnimState = arg_charaShootAnimState;
    }


    //  Update
    private void Update()
    {
        transform.Translate(new Vector3(1, 0, 0) * bullet.GetBulletMovementSpeed() * Time.deltaTime);
    }


    //  Coroutines
    IEnumerator EndAnimationCoroutine(float arg_time, BulletAnimStateEnum arg_animState)
    {
        yield return new WaitForSeconds(arg_time);
        SetAnimation(arg_animState);
    }

    //  Animation functions
    private void SetAnimation(BulletAnimStateEnum arg_bulletAnimStateEnum)
    {
        if (animState == arg_bulletAnimStateEnum)
        {
            return;
        }

        animator.Play(arg_bulletAnimStateEnum.ToString());
        animState = arg_bulletAnimStateEnum;
    }
}
