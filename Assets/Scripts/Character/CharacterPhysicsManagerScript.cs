﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysicsManagerScript : MonoBehaviour
{
    private Rigidbody2D rigidBody;

    private bool isTriggerHorizontalVelocity = false;
    private bool isTriggerVerticalVelocity = false;
    private bool isSwitchHorizontalDirection = false;
    private bool isTriggerHorizontalDrag = false;
    private bool isTriggerAddForce = false;

    private float xVelocityValue;
    private float yVelocityValue;
    private float horizontalDragMultiplier;
    private Vector2 addForceVector;

    [SerializeField] private PhysicsMaterial2D[] colliderMaterialTable = new PhysicsMaterial2D[2];

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //  Change Horizontal Velocity
        if (isTriggerHorizontalVelocity)
        {
            rigidBody.velocity = new Vector2(xVelocityValue, rigidBody.velocity.y);
            isTriggerHorizontalVelocity = false;
        }

        //  Change Vertical Velocity
        if (isTriggerVerticalVelocity)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, yVelocityValue);
            isTriggerVerticalVelocity = false;
        }

        //  Switch RigidBody Direction
        if (isSwitchHorizontalDirection)
        {
            rigidBody.velocity = new Vector2(-rigidBody.velocity.x, rigidBody.velocity.y);
            isSwitchHorizontalDirection = false;
        }

        //  Horizontal Drag
        if (isTriggerHorizontalDrag)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x * horizontalDragMultiplier, rigidBody.velocity.y);
            isTriggerHorizontalDrag = false;
        }

        //  AddForce
        if (isTriggerAddForce)
        {
            rigidBody.AddForce(addForceVector);
            isTriggerAddForce = false;
        }
    }


    //  Getters and Setters
    public Rigidbody2D GetRigidbody()
    {
        return rigidBody;
    }

    public PhysicsMaterial2D[] GetColliderMaterialTable()
    {
        return colliderMaterialTable;
    }

    public void SetRigidBodyGravity(float arg_gravityScale)
    {
        rigidBody.gravityScale = arg_gravityScale;
    }

    public void SetRigidBodyVelocity(Vector2 arg_velocityValue)
    {
        rigidBody.velocity = arg_velocityValue;
    }

    public void SetRigidBodyMaterial(PhysicsMaterial2D arg_physicsMaterial)
    {
        rigidBody.sharedMaterial = arg_physicsMaterial;
    }


    //  Velocity Methods
    public void ChangeVelocityHorizontal(float arg_XValue)
    {
        xVelocityValue = arg_XValue;
        isTriggerHorizontalVelocity = true;
    }

    public void ChangeVelocityVertical(float arg_YValue)
    {
        yVelocityValue = arg_YValue;
        isTriggerVerticalVelocity = true;
    }

    public void SwitchHorizontalDirection()
    {
        isSwitchHorizontalDirection = true;
    }

    public void HorizontalDrag(float arg_horizontalDragMultiplier)
    {
        horizontalDragMultiplier = arg_horizontalDragMultiplier;
        isTriggerHorizontalDrag = true;
    }


    //  AddForce Methods
    public void AddForceMethod(Vector2 arg_addForceVector)
    {
        addForceVector = arg_addForceVector;
        isTriggerAddForce = true;
    }
}
