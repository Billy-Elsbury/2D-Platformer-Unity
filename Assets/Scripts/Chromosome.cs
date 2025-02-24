using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chromosome 
{
bool LeftPressed = false;
bool RightPressed = false;
bool JumpPressed = false;
    int leftIndex = 0;
    int rightIndex = 0;
    int jumpIndex = 0;
    float leftPassTime = 0;
    float rightPassTime = 0;
    float jumpPassTime = 0;
internal List<float> LeftTime = new List<float>();
internal List<float> RightTime = new List<float>();
internal List<float> JumpTime = new List<float>();

    internal float updateX(float gameTime)
    {
     if ((gameTime - leftPassTime) > LeftTime[leftIndex]) {LeftPressed = true;
           leftPassTime += LeftTime[leftIndex];
           leftIndex++;
           LeftPressed = !LeftPressed;
        }
        if ((gameTime - rightPassTime) > RightTime[rightIndex])
        {
            RightPressed = true;
            rightPassTime += RightTime[rightIndex];
            rightIndex++; RightPressed = !RightPressed;
        }

        return (LeftPressed ? -1 : 0) + (RightPressed ? 1 : 0);
    }
}
