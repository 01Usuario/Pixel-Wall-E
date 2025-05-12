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
    public string Variable{get;}
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
public class DrawCommandNode : ASTNode
{
     public string Name{get;}
    public List<ASTNode> Parameters;

    public DrawCommandNode(string name, List<ASTNode> parameters) {
        Name = name;
        Parameters = parameters;
    }
}

public class FillNode : ASTNode {
   public FillNode(){}
}

public class AssignNode : ASTNode
{
    public string Variable;
    public ASTNode Expression;
    public AssignNode(string variable, ASTNode expresion)
    {
        Variable = variable;
        Expression = expresion;
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
    public string Name{get;}
    public List<ASTNode> Parameters;

    public FunctionNode(string name, List<ASTNode> parameters) {
        Name = name;
        Parameters = parameters;
    }
}