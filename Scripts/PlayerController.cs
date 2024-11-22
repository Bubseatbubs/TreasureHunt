using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;

public class PlayerController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float moveSpeed;
    public Rigidbody2D rb2d;
    public Vector2 forceToApply;
    public float forceDamping;
    int playerID;
    Vector2 PlayerInput;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkController.Instance().GetID() == playerID)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            virtualCamera.Follow = rb2d.transform;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (NetworkController.Instance().GetID() == playerID)
        {
            PlayerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }
        Move(PlayerInput);
    }

    public void Move(Vector2 input)
    {
        Vector2 moveForce = input * moveSpeed;
        ApplyForce(moveForce);
    }

    void ApplyForce(Vector2 moveForce)
    {
        moveForce += forceToApply;
        forceToApply /= forceDamping;

        if (Mathf.Abs(forceToApply.x) <= 0.01f && Mathf.Abs(forceToApply.y) <= 0.01f)
        {
            forceToApply = Vector2.zero;
        }

        rb2d.velocity = moveForce;
    }

    public void AssignID(int id)
    {
        playerID = id;
    }

    public int GetID()
    {
        return playerID;
    }

    public float GetXPosition()
    {
        return rb2d.position.x;
    }

    public float GetYPosition()
    {
        return rb2d.position.y;
    }

    public Vector2 GetPosition()
    {
        return rb2d.position;
    }

    public void SetPosition(Vector2 pos)
    {
        rb2d.position = pos;
    }

    public Vector2 GetInput()
    {
        return PlayerInput;
    }

    public void SetInput(Vector2 Input)
    {
        PlayerInput = Input;
    }
}
