using System.Collections.Generic;
using UnityEngine;

public class Evaluator
{
    private CanvasManager canvasManager;
    private Brush brush;
    private Dictionary<string, object> variables;
    private Dictionary<string, int> labels;
    private ProgramNode program;
    private int currentIndex;

    public Evaluator(CanvasManager canvasManager)
    {
        this.canvasManager = canvasManager;
        this.brush = new Brush();
        this.variables = new Dictionary<string, object>();
        this.labels = new Dictionary<string, int>();
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
                Debug.Log("Cooredenadas" + brush.CurrentX + "," + brush.CurrentY);
                break;

            case ColorNode color:
                brush.Color = color.Color;
                Debug.Log("Color " + brush.Color);
                break;

            case SizeNode size:
                brush.Size = size.Size;
                break;
        }
    }
    private void ExecuteDrawCommand(DrawCommandNode node)
    {
        switch (node.Name)
        {
            case "DrawLine":
                ExecuteDrawLine(
                    (int)EvaluateExpression(node.Parameters[0]),
                    (int)EvaluateExpression(node.Parameters[1]),
                    (int)EvaluateExpression(node.Parameters[2])
                );
                break;

                /*  case "DrawCircle":
                     ExecuteDrawCircle(
                         (int)EvaluateExpression(node.Parameters[0]),
                         (int)EvaluateExpression(node.Parameters[1]),
                         (int)EvaluateExpression(node.Parameters[2])
                     );
                     break;

                 case "DrawRectangle":
                     ExecuteDrawRectangle(
                         (int)EvaluateExpression(node.Parameters[0]),
                         (int)EvaluateExpression(node.Parameters[1]),
                         (int)EvaluateExpression(node.Parameters[2]),
                         (int)EvaluateExpression(node.Parameters[3]),
                         (int)EvaluateExpression(node.Parameters[4])
                     );
                     break; */
        }
    }

    private void ExecuteDrawLine(int dirX, int dirY, int distance)
    {
        int startX = brush
.CurrentX;
        int startY = brush
.CurrentY;
        int endX = startX + dirX * distance;
        int endY = startY + dirY * distance;

        DrawLine(startX, startY, endX, endY);

        // Actualizar posición del pincel
        brush
.CurrentX = endX;
        brush
.CurrentY = endY;
    }

    private void DrawLine(int x0, int y0, int x1, int y1)
    {
        // Implementación del algoritmo de Bresenham
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawPixelWithBrushSize(x0, y0);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private void DrawPixelWithBrushSize(int x, int y)
    {
        if (brush.Color == "Transparent") return;

        Color color = StringToColor(brush.Color);
        int halfSize = brush.Size / 2;

        for (int i = -halfSize; i <= halfSize; i++)
        {
            for (int j = -halfSize; j <= halfSize; j++)
            {
                int pixelX = x + i;
                int pixelY = y + j;

                if (pixelX >= 0 && pixelY >= 0 &&
                    pixelX < canvasManager.canvasSize &&
                    pixelY < canvasManager.canvasSize)
                {
                canvasManager.SetPixelDirect(pixelX, pixelY, color);
                }
            }
        }
    }
    private Color StringToColor(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red": return Color.red;
            case "blue": return Color.blue;
            case "green": return Color.green;
            case "yellow": return Color.yellow;
            case "orange": return new Color(1f, 0.5f, 0f);
            case "purple": return new Color(0.5f, 0f, 0.5f);
            case "black": return Color.black;
            case "white": return Color.white;
            case "transparent": return new Color(0, 0, 0, 0);
            default: return Color.magenta;
        }
    }

    private object EvaluateExpression(ASTNode node)
    {
        switch (node)
        {
            case NumberNode num:
                return num.Number;

            case VariableNode var:
                return variables.ContainsKey(var.Variable) ? variables[var.Variable] : 0;

            case BinaryOpNode binOp:
                dynamic left = EvaluateExpression(binOp.Left);
                dynamic right = EvaluateExpression(binOp.Right);

                switch (binOp.Operator)
                {
                    case "+": return left + right;
                    case "-": return left - right;
                    case "*": return left * right;
                    case "/": return left / right;
                    case "%": return left % right;
                    case "**": return Mathf.Pow(left, right);
                    default: return 0;
                }

            /* case FunctionNode func:
                return ExecuteFunction(func); */

            default:
                return 0;
        }
    }
    
}
                