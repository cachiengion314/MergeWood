using Unity.Mathematics;
using UnityEngine;

public class Utility
{
    public static float4 RandomColor(ref Unity.Mathematics.Random random)
    {
        var hue = (random.NextFloat() + 0.618034005f) % 1;
        return (Vector4)Color.HSVToRGB(hue, 1.0f, 1.0f);
    }

    /// <summary>
    /// BurstCompile not capable should using sparingly
    /// </summary>
    /// <param name="o"></param>
    public static void Print(in object o)
    {
        Debug.Log(o);
    }

    /// <summary>
    /// BurstCompile capable\
    /// 0: White
    /// 1: Blue
    /// 2: Red
    /// 3: Green
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public static void DrawLine(in float3 start, in float3 end, in int _color = 0)
    {
        var color = Color.white;
        if (_color == 1)
        {
            color = Color.blue;
        }
        else if (_color == 2)
        {
            color = Color.red;
        }
        else if (_color == 3)
        {
            color = Color.green;
        }

        Debug.DrawLine(start, end, color);
    }

    /// <summary>
    /// BurstCompile capable
    /// 0: White
    /// 1: Blue
    /// 2: Red
    /// 3: Green
    /// </summary>
    /// <param name="start"></param>
    /// <param name="dir"></param>
    public static void DrawRay(in float3 start, in float3 dir, in int _color = 0)
    {
        var color = Color.white;
        if (_color == 1)
        {
            color = Color.blue;
        }
        else if (_color == 2)
        {
            color = Color.red;
        }
        else if (_color == 3)
        {
            color = Color.green;
        }

        Debug.DrawRay(start, dir, color);
    }

    /// <summary>
    /// BurstCompile capable \n
    /// 0: White
    /// 1: Blue
    /// 2: Red
    /// 3: Green
    /// </summary>
    /// <param name="quadPos"></param>
    /// <param name="width"></param>
    /// <param name="_color"></param>
    /// <param name="_height"></param>
    public static void DrawQuad(in Vector2 quadPos, in float width, in int _color = 0, in float _height = .1f)
    {
        var color = Color.white;
        if (_color == 1)
        {
            color = Color.blue;
        }
        else if (_color == 2)
        {
            color = Color.red;
        }
        else if (_color == 3)
        {
            color = Color.green;
        }

        var Width = width / 2;

        var offset = new float3(quadPos.x, quadPos.y, _height);
        var pos1 = offset + new float3(-Width, -Width, _height);
        var pos2 = offset + new float3(-Width, Width, _height);
        var pos3 = offset + new float3(Width, Width, _height);
        var pos4 = offset + new float3(Width, -Width, _height);

        Debug.DrawLine(pos1, pos2, color);
        Debug.DrawLine(pos2, pos3, color);
        Debug.DrawLine(pos3, pos4, color);
        Debug.DrawLine(pos4, pos1, color);
    }
}