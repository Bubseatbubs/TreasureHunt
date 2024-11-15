using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb2d;
    public Vector2 forceToApply;
    public float forceDamping;
    int playerID;
    private PlayerManager playerManager; // Handles spawning in players
    Vector2 PlayerInput;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (NetworkController.instance.GetClientID() == playerID)
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

    public void SetPosition(float x, float y)
    {
        rb2d.position = new Vector2(x, y);
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
