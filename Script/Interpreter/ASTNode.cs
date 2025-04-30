using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ASTNode 
{
    
}
public class ProgramNode : ASTNode
{
    public List<ASTNode> Instructions;

}
public class VariableNode : ASTNode
{
    public string Variable;
    public VariableNode(string variable)
    {
        Variable = variable;
    }
}

//pa que los colores no se parseen como variables
public class StringLiteralNode : ASTNode
{
    public string Value;
    public StringLiteralNode(string value)
    {
        Value = value;
    }
}
public class NumberNode : ASTNode
{
    public int Number;
    public NumberNode(int number)
    {
        Number = number;
    }
}

public class LabelNode : ASTNode
{
    public string Name;
    public LabelNode(string name)
    {
        Name = name;
    }
}

public class SpawnNode : ASTNode
{
    public int X;
    public int Y;

    public SpawnNode(int x, int y)
    {
        X = x;
        Y = y;
    }
}
public class ColorNode : ASTNode
{
    public string Color;
    public ColorNode(string color)
    {
        Color = color;
    }
}
public class SizeNode : ASTNode
{
    public int Size;
    public SizeNode(int size)
    {
        Size = size;
    }
}
public class DrawLineNode : ASTNode
{
    public int DirX;
    public int DirY;
    public int Distance;
    public DrawLineNode(int dirX, int dirY, int distance)
    {
        DirX = dirX;
        DirY = dirY;
        Distance= distance;
    }

}

public class DrawCircleNode : ASTNode
{
    public int DirX;
    public int DirY;
    public int Radius;
    public DrawCircleNode(int dirX, int dirY, int radius)
    {
        DirX = dirX;
        DirY = dirY;
        Radius = radius;
    }
}

public class DrawRectangleNode : ASTNode
{
    public int DirX;
    public int DirY;
    public int Distance;
    public int Width;
    public int Height;
    public DrawRectangleNode(int dirX, int dirY, int width, int height,int distance)
    {
        DirX = dirX;
        DirY = dirY;
        Width = width;
        Height = height;
        Distance = distance;
    }
}
public class FillNode : ASTNode{

}

public class AssignNode : ASTNode
{
    public string Variable;
    public ASTNode Expresion;
    public AssignNode(string variable, ASTNode expresion)
    {
        Variable = variable;
        Expresion = expresion;
    }
}
public class BinaryOpNode : ASTNode {
    public string Operator;
    public ASTNode Left;
    public ASTNode Right;
    
    public BinaryOpNode(string op, ASTNode left, ASTNode right) {
        Operator = op;
        Left = left;
        Right = right;
    }
}

public class BooleanOpNode : ASTNode {
    public string Operator;
    public ASTNode Left;
    public ASTNode Right;
    
    public BooleanOpNode(string op, ASTNode left, ASTNode right) {
        Operator = op;
        Left = left;
        Right = right;
    }
}
public class GoToNode : ASTNode {
    public string Label;
    public ASTNode Condition; 
    
    public GoToNode(string label, ASTNode condition) {
        Label = label;
        Condition = condition;
    }
}
public class FunctionNode : ASTNode {
    public string Name;
    public List<ASTNode> Parameters;
    public ASTNode Body;

    public FunctionNode(string name, List<ASTNode> parameters, ASTNode body) {
        Name = name;
        Parameters = parameters;
        Body = body;
    }
}