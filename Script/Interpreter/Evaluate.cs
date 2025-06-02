using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
public class Evaluator
{
    private CanvasManager canvasManager;
    private Brush brush;
    private Dictionary<string, object> variables;
    private Dictionary<string, int> labels;
    private ProgramNode program;
    private int currentIndex;
    private DrawingEngine drawingEngine;

    public Evaluator(CanvasManager canvasManager, DrawingEngine drawingEngine)
    {
        this.canvasManager = canvasManager;
        this.brush = new Brush();
        this.variables = new Dictionary<string, object>();
        this.labels = new Dictionary<string, int>();
        this.drawingEngine = drawingEngine;

    }

    public void Evaluate(ProgramNode program)
    {
        this.program = program;
        PreprocessLabels();

        currentIndex = 0;
        while (currentIndex < program.Instructions.Count)
        {
            EvaluateInstruction(program.Instructions[currentIndex]);
            currentIndex++;
        }
    }

    private void PreprocessLabels()
    {
        for (int i = 0; i < program.Instructions.Count; i++)
        {
            if (program.Instructions[i] is LabelNode labelNode)
            {
                labels[labelNode.Name] = i;
            }
        }
    }

    private void EvaluateInstruction(ASTNode node)
    {
        switch (node)
        {
            case SpawnNode spawn:
                brush.CurrentX = spawn.X;
                brush.CurrentY = spawn.Y;
                break;

            case ColorNode color:
                brush.Color = color.Color;
                break;

            case SizeNode size:
                brush.Size = size.Size;
                break;
            case AssignNode assign:
                variables[assign.Variable] = EvaluateExpression(assign.Expression);
                break;
            case FunctionNode func:
                Debug.Log("Evaluando Funcion " + func.Name);
                EvaluateFunction(func);
                break;
            case DrawCommandNode drawCmd:
                EvaluateDrawCommand(drawCmd);
                break;
        }
    }
    private object EvaluateExpression(ASTNode node)
    {

        switch (node)
        {
            case NumberNode num: return num.Number;

            case StringLiteralNode str: return str.Value;

            case VariableNode var:
                if (variables.ContainsKey(var.Variable))
                    return variables[var.Variable];
                throw new Exception($"Variable no definida: {var.Variable}");

            case BinaryOpNode binOp:
                var leftNode = EvaluateExpression(binOp.Left);
                var rightNode = EvaluateExpression(binOp.Right);
                return EvaluateBinaryOp(binOp.Operator, leftNode, rightNode);

            case BooleanOpNode boolOp:
                var left = Convert.ToBoolean(EvaluateExpression(boolOp.Left));
                var right = Convert.ToBoolean(EvaluateExpression(boolOp.Right));
                return EvaluateBooleanOp(boolOp.Operator, left, right);

            case FunctionNode func:
                return EvaluateFunction(func);

            default:
                throw new Exception($"Unsupported expression type: {node.GetType().Name}");
        }
    }


    private object EvaluateFunction(FunctionNode func)
    {
        List<object> evaluatedParams = new List<object>();
        foreach (var param in func.Parameters)
        {
            evaluatedParams.Add(EvaluateExpression(param));
        }
        object[] parameters = evaluatedParams.ToArray();

        switch (func.Name)
        {
            case "GetActualX":
                return brush.CurrentX;

            case "GetActualY":
                return brush.CurrentY;

            case "GetCanvasSize":
                return canvasManager.canvasSize;

            case "IsBrushColor":
                return (string)parameters[0] == brush.Color ? 1 : 0;

            case "IsCanvasColor":
                int checkX = brush.CurrentX + (int)parameters[1];
                int checkY = brush.CurrentY + (int)parameters[2];

                string pixelColor = canvasManager.GetPixelColor(checkX, checkY);

                return pixelColor == (string)parameters[0] ? 1 : 0;
            case "IsBrushSize":
                int size = brush.Size == (int)parameters[0] ? 1 : 0;
                return brush.Size == (int)parameters[0] ? 1 : 0;

            case "GetColorCount":
                int count = EvaluateGetColorCount((string)parameters[0], (int)parameters[1],
                                            (int)parameters[2], (int)parameters[3], (int)parameters[4]);
                Debug.Log("Color count " + count);
                return count;


            default:
                throw new Exception($"Función no reconocida: {func.Name}");
        }
    }
    private object EvaluateBinaryOp(string op, object left, object right)
    {

        if (left is FunctionNode leftFunc)
        {
            EvaluateFunction(leftFunc);

        }
        if (right is FunctionNode rightFunc)
        {
            EvaluateFunction(rightFunc);
        }
        if (left is int leftInt && right is int rightInt)
        {
            return op switch
            {
                "+" => leftInt + rightInt,
                "-" => leftInt - rightInt,
                "*" => leftInt * rightInt,
                "/" => leftInt / rightInt,
                "**" => Math.Pow(leftInt, rightInt),
                "%" => leftInt % rightInt,
                _ => throw new Exception($"Unsupported operator: {op}")
            };
        }
        if (right is BinaryOpNode binOp)
        {
            return EvaluateBinaryOp(op, left, EvaluateExpression(binOp.Left));
        }
        else
            throw new Exception("Operación binaria requiere operandos enteros");
    }
    private bool EvaluateBooleanOp(string op, object left, object right)
    {

        if (left is bool leftBool && right is bool rightBool)
        {
            return op switch
            {
                "&&" => leftBool && rightBool,
                "||" => leftBool || rightBool,
                "==" => leftBool == rightBool,
                _ => throw new Exception($"Unsupported boolean operator(Bool) : {op}")
            };
        }
        else if (left is int leftInt && right is int rightInt)
        {
            return op switch
            {
                "<" => leftInt < rightInt,
                ">" => leftInt > rightInt,
                "<=" => leftInt <= rightInt,
                ">=" => leftInt >= rightInt,
                _ => throw new Exception($"Unsupported boolean operator(Comparation): {op}")
            };
        }
        else
        {
            throw new Exception($"(Excepcion): {op}");
        }

    }
    private int EvaluateGetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        int canvasSize = canvasManager.canvasSize;


        int minX = Math.Min(x1, x2);
        int maxX = Math.Max(x1, x2);
        int minY = Math.Min(y1, y2);
        int maxY = Math.Max(y1, y2);

        if (minX >= canvasSize || maxX < 0 || minY >= canvasSize || maxY < 0)
        {
            return 0;
        }

        int count = 0;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (canvasManager.GetPixelColor(x, y) == color)
                {
                    count++;
                }
            }
        }
        return count;
    }
    private void EvaluateDrawCommand(DrawCommandNode drawCmd)
    {
        if (brush.Color == "Transparent")
            return;
        if (drawCmd.Name == "DrawLine")
            {
                int dirX = (int)EvaluateExpression(drawCmd.Parameters[0]);
                int dirY = (int)EvaluateExpression(drawCmd.Parameters[1]);
                int distance = (int)EvaluateExpression(drawCmd.Parameters[2]);

                int endX = brush.CurrentX + dirX * distance;
                int endY = brush.CurrentY + dirY * distance;

                drawingEngine.DrawLine(
                    brush.CurrentX,
                    brush.CurrentY,
                    endX,
                    endY,
                    brush.Color,
                    brush.Size
                );

                brush.CurrentX = endX;
                brush.CurrentY = endY;
                drawingEngine.UpdateTexture(canvasManager.canvasTexture);
            }
        if (drawCmd.Name == "DrawCircle")
        {
            int dirX = (int)EvaluateExpression(drawCmd.Parameters[0]);
            int dirY = (int)EvaluateExpression(drawCmd.Parameters[1]);
            int radio = (int)EvaluateExpression(drawCmd.Parameters[2]);
            int endX = brush.CurrentX + dirX * radio;
            int endY = brush.CurrentY + dirY * radio;
            drawingEngine.DrawCircle(
                brush.CurrentX,
                brush.CurrentY,
                radio,
                brush.Color,
                brush.Size
            );

            brush.CurrentX = endX;
            brush.CurrentY = endY;
            drawingEngine.UpdateTexture(canvasManager.canvasTexture);
        }
        if (drawCmd.Name == "DrawRectangle")
        {
            int dirX = (int)EvaluateExpression(drawCmd.Parameters[0]);
            int dirY = (int)EvaluateExpression(drawCmd.Parameters[1]);
            int distance = (int)EvaluateExpression(drawCmd.Parameters[2]);
            int width = (int)EvaluateExpression(drawCmd.Parameters[3]);
            int height = (int)EvaluateExpression(drawCmd.Parameters[4]);

            drawingEngine.DrawRectangle(
                brush.CurrentX,
                brush.CurrentY,
                dirX,
                dirY,
                distance,
                width,
                height,
                brush.Color,
                brush.Size
            );
            // Actualizar posición (centro del rectángulo)
            brush.CurrentX += dirX * distance;
            brush.CurrentY += dirY * distance;
            drawingEngine.UpdateTexture(canvasManager.canvasTexture);

        }
    }
}

        


