﻿using System.Collections;
using UnityEngine;

public class CharacterScript : MonoBehaviour
{
    private Character character;

    //  Components
    private CharacterPhysicsManagerScript physicsManager;
    [SerializeField] private CharacterCollideCheckScript groundChecker;
    [SerializeField] private CharacterCollideCheckScript rightWallChecker;
    [SerializeField] private CharacterCollideCheckScript leftWallChecker;

    //  Animations
    private CharacterAnimStateEnum animState = CharacterAnimStateEnum.Idle;
    private int directionInt = 1;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;
    const string ANIM_IDLE = "Chara_Idle";
    const string ANIM_RUN = "Chara_Run";
    const string ANIM_SLIDE = "Chara_Slide";
    const string ANIM_JUMP = "Chara_Jump";
    const string ANIM_JUMP_FORWARD = "Chara_Jump_forward";
    const string ANIM_FALL_NORMAL = "Chara_Fall_normal";
    const string ANIM_FALL_FORWARD = "Chara_Fall_forward";
    const string ANIM_FALL_MAXSPEED = "Chara_Fall_maxspeed";
    const string ANIM_CRAWL_IDLE = "Chara_Crawl_idle";
    const string ANIM_CRAWL_MOVE = "Chara_Crawl_move";
    const string ANIM_WALLSLIDE = "Chara_Wallslide";
    const string ANIM_RUN_SLIDE = "Chara_Run_slide";
    const string ANIM_ONTHEGROUND = "Chara_Ontheground";
    const string ANIM_ONTHEGROUND_STANDUP = "Chara_Ontheground_standup";
    const string ANIM_MELEE_IDLE = "Chara_Melee_idle";

    //  Getters and Setters
    public Character GetCharacter()
    {
        return character;
    }

    public void SetCharacter(Character arg_character)
    {
        character = arg_character;
    }


    //  Awake Function
    private void Awake()
    {
        physicsManager = GetComponent<CharacterPhysicsManagerScript>();
    }


    //  Update Function
    private void Update()
    {
        if (!Game.GetGamePaused())
        {
            //
            // Idle Actions & Events
            //
            if (animState.Equals(CharacterAnimStateEnum.Idle))
            {
                //  Run
                if (ReturnHorizontalInput() != 0)
                {
                    SetAnimation(ANIM_RUN, CharacterAnimStateEnum.Run);
                }

                //  Jump
                else if ((Input.GetButtonDown("Keyboard_Jump") || Input.GetButtonDown("Gamepad_Jump")) && groundChecker.GetIsColliding())
                {
                    SetAnimation(ANIM_JUMP, CharacterAnimStateEnum.Jump);
                    physicsManager.AddForceMethod(Vector2.up * character.GetIdleJumpVerticalForce());
                }

                //  Crawl
                else if (ReturnVerticalInput() < 0)
                {
                    SetAnimation(ANIM_CRAWL_IDLE, CharacterAnimStateEnum.Crawl_idle);
                }

                //  Fall
                if (!groundChecker.GetIsColliding())
                {
                    SetAnimation(ANIM_FALL_NORMAL, CharacterAnimStateEnum.Fall_normal);
                }

                // Melee
                if (Input.GetButtonDown("Gamepad_Melee"))
                {
                    SetAnimation(ANIM_MELEE_IDLE, CharacterAnimStateEnum.Melee_idle);
                    StartCoroutine("StopMelee");
                }
            }


            //
            // Run Actions & Events
            //
            else if (animState.Equals(CharacterAnimStateEnum.Run))
            {
                //  CanRunSlide Timer
                if (!character.GetRunSlideCanRunSlide())
                {
                    StartCoroutine("CanRunSlide");
                }

                //  Run
                if (ReturnHorizontalInput() != 0)
                {
                    int loc_directionInt = ReturnHorizontalInput();
                    SetDirection(loc_directionInt);
                    physicsManager.ChangeVelocityHorizontal(loc_directionInt * character.GetRunMovementSpeed());

                    //  Jump Forward
                    if ((Input.GetButtonDown("Keyboard_Jump") || Input.GetButtonDown("Gamepad_Jump")) && groundChecker.GetIsColliding())
                    {
                        //  Wallslide
                        if ((directionInt > 0 && rightWallChecker.GetIsColliding()) || (directionInt < 0 && leftWallChecker.GetIsColliding()))
                        {
                            //NOT IN FIXEDUPDATE
                            physicsManager.SetRigidBodyVelocity(new Vector2(physicsManager.GetRigidbody().velocity.x, 0));

                            physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                            SetAnimation(ANIM_WALLSLIDE, CharacterAnimStateEnum.Wallslide);
                        }

                        // Forward Jump
                        else
                        {
                            SetAnimation(ANIM_JUMP_FORWARD, CharacterAnimStateEnum.Jump_forward);
                            physicsManager.AddForceMethod(new Vector2(loc_directionInt * character.GetForwardJumpHorizontalForce(), character.GetIdleJumpVerticalForce()));
                        }
                    }

                    // Run Slide
                    else if (character.GetRunSlideCanRunSlide() && ReturnVerticalInput() < 0)
                    {
                        SetAnimation(ANIM_RUN_SLIDE, CharacterAnimStateEnum.Run_slide);
                        StartCoroutine("StopRunSlide");
                        physicsManager.AddForceMethod(new Vector2(loc_directionInt * character.GetRunSlideHorizontalForce(), 0));
                    }
                }

                //  Stop Slide
                else if (!Input.anyKey && ReturnHorizontalInput() == 0)
                {
                    SetAnimation(ANIM_SLIDE, CharacterAnimStateEnum.Slide);
                    StartCoroutine("StopSlide");
                }

                //  Fall
                if (!groundChecker.GetIsColliding())
                {
                    SetAnimation(ANIM_FALL_FORWARD, CharacterAnimStateEnum.Fall_forward);
                }
            }


            //
            //  Slide actions & Events
            //
            else if (animState.Equals(CharacterAnimStateEnum.Slide))
            {
                //  Fall
                if (!groundChecker.GetIsColliding())
                {
                    StopCoroutine("StopSlide");
                    SetAnimation(ANIM_FALL_NORMAL, CharacterAnimStateEnum.Fall_normal);
                }
            }


            //
            // Idle Jump actions & Events
            //
            else if (animState.Equals(CharacterAnimStateEnum.Jump))
            {
                physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[1]);
                IdleJumpMovement();

                //  Fall Animation
                if (physicsManager.GetRigidbody().velocity.y < 0)
                {
                    SetAnimation(ANIM_FALL_NORMAL, CharacterAnimStateEnum.Fall_normal);
                }

                //  Wallslide
                else if ((rightWallChecker.GetIsColliding() && ReturnHorizontalInput() > 0) || (leftWallChecker.GetIsColliding() && ReturnHorizontalInput() < 0))
                {
                    physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                    SetAnimation(ANIM_WALLSLIDE, CharacterAnimStateEnum.Wallslide);
                }
            }


            //
            // Idle Fall actions & Events
            //
            else if (animState.Equals(CharacterAnimStateEnum.Fall_normal))
            {
                physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[1]);
                IdleJumpMovement();

                //  Wallslide
                if ((rightWallChecker.GetIsColliding() && ReturnHorizontalInput() > 0) || (leftWallChecker.GetIsColliding() && ReturnHorizontalInput() < 0))
                {
                    physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                    SetAnimation(ANIM_WALLSLIDE, CharacterAnimStateEnum.Wallslide);
                }

                //  Touch Ground
                if (groundChecker.GetIsColliding())
                {
                    physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                    SetAnimation(ANIM_IDLE, CharacterAnimStateEnum.Idle);
                }

                //  Maximum Speed
                if (physicsManager.GetRigidbody().velocity.y < -character.GetFallMaxSpeedVelocityValue())
                {
                    SetAnimation(ANIM_FALL_MAXSPEED, CharacterAnimStateEnum.Fall_maxspeed);
                }
            }


            //
            //  Fall maxspeed Action & Events
            //
            else if (animState.Equals(CharacterAnimStateEnum.Fall_maxspeed))
            {
                IdleJumpMovement();

                //  Touch Ground
                if (groundChecker.GetIsColliding() && !character.GetOnTheGroundIsOntheGround())
                {
                    physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                    SetAnimation(ANIM_ONTHEGROUND, CharacterAnimStateEnum.Ontheground);
                    StartCoroutine("Ontheground");
                    character.SetOnTheGroundIsOntheGround(false);
                }
            }


            //
            //  Forward Jump actions & Events
            //
            else if (animState.Equals(CharacterAnimStateEnum.Jump_forward))
            {
                physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[1]);
                SwitchDirectionForwardJump();
                AirDrag();

                //  Wallslide
                if ((directionInt > 0 && rightWallChecker.GetIsColliding()) || (directionInt < 0 && leftWallChecker.GetIsColliding()))
                {
                    //NOT IN FIXEDUPDATE
                    physicsManager.SetRigidBodyVelocity(new Vector2(physicsManager.GetRigidbody().velocity.x, 0));

                    physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                    SetAnimation(ANIM_WALLSLIDE, CharacterAnimStateEnum.Wallslide);
                }

                //  Fall Animation
                else if (physicsManager.GetRigidbody().velocity.y < 0)
                {
                    SetAnimation(ANIM_JUMP_FORWARD, CharacterAnimStateEnum.Fall_forward);
                }
            }


            //
            // Forward Fall actions & Events
            //
            else if (animState.Equals(CharacterAnimStateEnum.Fall_forward))
            {
                SwitchDirectionForwardJump();
                AirDrag();

                //  Wallslide
                if ((directionInt > 0 && rightWallChecker.GetIsColliding()) || (directionInt < 0 && leftWallChecker.GetIsColliding()))
                {
                    physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                    SetAnimation(ANIM_WALLSLIDE, CharacterAnimStateEnum.Wallslide);
                }

                //  Touch Ground
                if (groundChecker.GetIsColliding())
                {
                    if (!Input.anyKey && ReturnHorizontalInput() == 0)
                    {
                        //physicsManager.AddForceMethod(new Vector2(directionInt * character.GetForwardJumpStopSlideForce(), 0));
                        SetAnimation(ANIM_SLIDE, CharacterAnimStateEnum.Slide);
                        StartCoroutine("StopSlide");
                    }

                    else if (ReturnHorizontalInput() > 0 || ReturnHorizontalInput() < 0)
                    {
                        SetAnimation(ANIM_RUN, CharacterAnimStateEnum.Run);
                    }

                    physicsManager.SetRigidBodyMaterial(physicsManager.GetColliderMaterialTable()[0]);
                }

                //  Maximum Speed
                if (physicsManager.GetRigidbody().velocity.y < -character.GetFallMaxSpeedVelocityValue())
                {
                    SetAnimation(ANIM_FALL_MAXSPEED, CharacterAnimStateEnum.Fall_maxspeed);
                }
            }


            //
            //  Crawl idle actions & Events
            //
            if (animState.Equals(CharacterAnimStateEnum.Crawl_idle))
            {
                //  Move
                if (ReturnHorizontalInput() > 0 || ReturnHorizontalInput() < 0)
                {
                    SetAnimation(ANIM_CRAWL_MOVE, CharacterAnimStateEnum.Crawl_move);
                }

                //  Stand Up
                else if (ReturnVerticalInput() >= 0)
                {
                    SetAnimation(ANIM_IDLE, CharacterAnimStateEnum.Idle);
                }

                //  Fall
                if (!groundChecker.GetIsColliding())
                {
                    SetAnimation(ANIM_FALL_NORMAL, CharacterAnimStateEnum.Fall_normal);
                }
            }


            //
            //  Crawl moving actions & Events
            //
            if (animState.Equals(CharacterAnimStateEnum.Crawl_move))
            {
                //  Move Crawl
                if (ReturnVerticalInput() < 0 && ReturnHorizontalInput() != 0)
                {
                    int loc_directionInt = ReturnHorizontalInput();
                    SetDirection(loc_directionInt);
                    physicsManager.ChangeVelocityHorizontal(loc_directionInt * character.GetCrawlMovementSpeed());
                }

                //  Stop
                else if (ReturnHorizontalInput() == 0)
                {
                    SetAnimation(ANIM_CRAWL_IDLE, CharacterAnimStateEnum.Crawl_idle);
                }

                //  Stand Up and Run
                else if (ReturnVerticalInput() >= 0 && ReturnHorizontalInput() != 0)
                {
                    SetAnimation(ANIM_RUN, CharacterAnimStateEnum.Run);
                }

                //  Fall
                if (!groundChecker.GetIsColliding())
                {
                    SetAnimation(ANIM_FALL_NORMAL, CharacterAnimStateEnum.Fall_normal);
                }
            }


            //
            //  Wallslide actions & Events
            //
            if (animState.Equals(CharacterAnimStateEnum.Wallslide))
            {
                //  Fall resistance
                if (ReturnHorizontalInput() == 0)
                {
                    //NOT IN FIXEDUPDATE
                    physicsManager.SetRigidBodyGravity(1);
                }
                else if ((ReturnHorizontalInput() > 0 && directionInt > 0) || (ReturnHorizontalInput() < 0 && directionInt < 0))
                {
                    //NOT IN FIXEDUPDATE
                    physicsManager.SetRigidBodyGravity(character.GetWallSlideHoldGravity());
                }

                //  Jump Left
                if (!character.GetWallJumpHasWallJumped() && (Input.GetButtonDown("Keyboard_Jump") || Input.GetButtonDown("Gamepad_Jump")) && ((ReturnHorizontalInput() <= 0 && directionInt > 0) || (ReturnHorizontalInput() >= 0 && directionInt < 0)))
                {
                    SetDirection(-directionInt);

                    //NOT IN FIXED UPDATE
                    physicsManager.SetRigidBodyGravity(1);
                    physicsManager.SetRigidBodyVelocity(new Vector2(physicsManager.GetRigidbody().velocity.x, 0));
                    physicsManager.AddForceMethod(new Vector2(directionInt * character.GetWallJumpHorizontalForce(), character.GetWallJumpVerticalForce()));
                    SetAnimation(ANIM_JUMP_FORWARD, CharacterAnimStateEnum.Jump_forward);
                    StartCoroutine("WallJumpTimer");
                }

                //  No more wall
                else if ((directionInt > 0 && !rightWallChecker.GetIsColliding()) || (directionInt < 0 && !leftWallChecker.GetIsColliding()))
                {
                    //NOT IN FIXED UPDATE
                    physicsManager.SetRigidBodyGravity(1);
                    SetAnimation(ANIM_FALL_NORMAL, CharacterAnimStateEnum.Fall_normal);
                }

                //  Switch Direction
                if ((directionInt > 0 && ReturnHorizontalInput() < 0) || (directionInt < 0 && ReturnHorizontalInput() > 0))
                {
                    //NOT IN FIXED UPDATE
                    physicsManager.SetRigidBodyGravity(1);

                    int loc_directionInt = ReturnHorizontalInput();
                    SetDirection(loc_directionInt);
                    SetAnimation(ANIM_FALL_NORMAL, CharacterAnimStateEnum.Fall_normal);
                }

                //  Touch Ground
                if (groundChecker.GetIsColliding())
                {
                    //NOT IN FIXED UPDATE
                    physicsManager.SetRigidBodyGravity(1);
                    SetAnimation(ANIM_IDLE, CharacterAnimStateEnum.Idle);
                }
            }
        }
    }


    //  Timer functions
    private IEnumerator StopSlide()
    {
        yield return new WaitForSeconds(character.GetRunStopSlideTime());
        SetAnimation(ANIM_IDLE, CharacterAnimStateEnum.Idle);
    }

    private IEnumerator CanRunSlide()
    {
        yield return new WaitForSeconds(character.GetRunSlideStartTime());
        character.SetRunSlideCanRunSlide(true);
    }

    private IEnumerator StopRunSlide()
    {
        yield return new WaitForSeconds(character.GetRunSlideDuration());
        SetAnimation(ANIM_CRAWL_MOVE, CharacterAnimStateEnum.Crawl_move);
        character.SetRunSlideCanRunSlide(false);
    }

    private IEnumerator WallJumpTimer()
    {
        character.SetWallJumpHasWallJumped(true);
        yield return new WaitForSeconds(character.GetWallJumpRestrainDuration());
        character.SetWallJumpHasWallJumped(false);
    }

    private IEnumerator Ontheground()
    {
        yield return new WaitForSeconds(character.GetOnTheGroundDuration());
        SetAnimation(ANIM_ONTHEGROUND_STANDUP, CharacterAnimStateEnum.Ontheground_standup);
        StartCoroutine("OnthegroundStandup");
    }

    private IEnumerator OnthegroundStandup()
    {
        yield return new WaitForSeconds(character.GetOnTheGroundStandUpTime());
        SetAnimation(ANIM_IDLE, CharacterAnimStateEnum.Idle);
        character.SetOnTheGroundIsOntheGround(false);
    }

    private IEnumerator StopMelee()
    {
        yield return new WaitForSeconds(character.GetMeleeStopTime());
        SetAnimation(ANIM_IDLE, CharacterAnimStateEnum.Idle);
    }


    //  Animation functions
    private void SetAnimation(string arg_animationName, CharacterAnimStateEnum arg_charaAnimStateEnum)
    {
        if (animState == arg_charaAnimStateEnum)
        {
            return;
        }

        animator.Play(arg_animationName);
        animState = arg_charaAnimStateEnum;
    }

    private void SetDirection(int arg_direction)
    {
        if (arg_direction > 0)
        {
            sprite.flipX = false;
        }
        else
        {
            sprite.flipX = true;
        }

        directionInt = arg_direction;
    }


    //  Player Input
    private int ReturnHorizontalInput()
    {
        int loc_horizonalInputValue;

        if (Input.GetAxisRaw("Keyboard_Horizontal") < 0 || Input.GetAxisRaw("Gamepad_Horizontal") < 0)
        {
            loc_horizonalInputValue = -1;
        }
        else if (Input.GetAxisRaw("Keyboard_Horizontal") > 0 || Input.GetAxisRaw("Gamepad_Horizontal") > 0)
        {
            loc_horizonalInputValue = 1;
        }
        else
        {
            loc_horizonalInputValue = 0;
        }

        return loc_horizonalInputValue;
    }

    private int ReturnVerticalInput()
    {
        int loc_verticalInputValue;

        if (Input.GetAxisRaw("Keyboard_Vertical") < 0 || Input.GetAxisRaw("Gamepad_Vertical") > 0)
        {
            loc_verticalInputValue = -1;
        }
        else if (Input.GetAxisRaw("Keyboard_Vertical") > 0 || Input.GetAxisRaw("Gamepad_Vertical") < 0)
        {
            loc_verticalInputValue = 1;
        }
        else
        {
            loc_verticalInputValue = 0;
        }

        return loc_verticalInputValue;
    }


    //  Physics & Movement
    private void IdleJumpMovement()
    {
        if (ReturnHorizontalInput() != 0)
        {
            int loc_directionInt = ReturnHorizontalInput();
            SetDirection(loc_directionInt);
            physicsManager.ChangeVelocityHorizontal(loc_directionInt * character.GetIdleJumpMovementSpeed());
        }
    }

    private void SwitchDirectionForwardJump()
    {
        if (ReturnHorizontalInput() != 0 && ReturnHorizontalInput() != directionInt)
        {
            SetDirection(-directionInt);
            physicsManager.SwitchHorizontalDirection();
        }
    }

    private void AirDrag()
    {
        if (!Input.anyKey && ReturnHorizontalInput() == 0)
        {
            physicsManager.HorizontalDrag(character.GetForwardJumpHorizontalAirDrag());
        }
    }
}
