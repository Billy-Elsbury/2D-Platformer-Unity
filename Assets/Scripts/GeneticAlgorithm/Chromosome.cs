using System.Collections.Generic;
using UnityEngine;

public class Chromosome
{
    public List<float> LeftTime { get; set; } = new List<float>();
    public List<float> RightTime { get; set; } = new List<float>();
    public List<float> JumpTime { get; set; } = new List<float>();

    private int leftIndex = 0;
    private int rightIndex = 0;
    private int jumpIndex = 0;
    private float leftPassTime = 0;
    private float rightPassTime = 0;
    private float jumpPassTime = 0;

    bool leftPressed = false;
    bool rightPressed = false;
    bool jumpPressed = false;

    public float UpdateX(float gameTime)
    {
        // Check if it's time to move left
        if (leftIndex < LeftTime.Count && (gameTime - leftPassTime) > LeftTime[leftIndex])
        {
            leftPressed = !leftPressed;
            leftPassTime += LeftTime[leftIndex];
            leftIndex++;
        }

        // Check if it's time to move right
        if (rightIndex < RightTime.Count && (gameTime - rightPassTime) > RightTime[rightIndex])
        {
            rightPressed = !rightPressed;
            rightPassTime += RightTime[rightIndex];
            rightIndex++;
        }

        // Return the movement direction (-1 for left, 1 for right, 0 for no movement)
        return (leftPressed ? -1 : 0) + (rightPressed ? 1 : 0);
    }

    public bool ShouldJump(float gameTime)
    {
        // Check if it's time to jump
        if (jumpIndex < JumpTime.Count && (gameTime - jumpPassTime) > JumpTime[jumpIndex])
        {
            jumpPressed = !jumpPressed;
            jumpPassTime += JumpTime[jumpIndex];
            jumpIndex++;
        }
        return jumpPressed;
    }
}