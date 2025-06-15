using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
public class Evaluator
{
    private CanvasManager canvasManager;
    public Brush brush;
    private Dictionary<string, object> variables;
    private Dictionary<string, int> labels;
    private ProgramNode program;
    private int currentIndex;
    private SemanticContext context;
    private DrawingEngine drawingEngine;
    private Color[,] lastValidState;


    public Evaluator(CanvasManager canvasManager, DrawingEngine drawingEngine)
    {
        this.canvasManager = canvasManager ?? throw new ArgumentNullException(nameof(canvasManager)); this.brush = new Brush();
        this.variables = new Dictionary<string, object>();
        this.labels = new Dictionary<string, int>();
        this.drawingEngine = drawingEngine ?? throw new ArgumentNullException(nameof(drawingEngine));
        this.context = new SemanticContext();

    }

   public void Evaluate(ProgramNode program)
    {
        this.program = program;
        PreprocessLabels();
        lastValidState = drawingEngine.CloneCanvasState();
        Color[,] preInstructionState = null;

        currentIndex = 0;
        while (currentIndex < program.Instructions.Count)
        {
            try
            {
                preInstructionState = drawingEngine.CloneCanvasState();
                EvaluateInstruction(program.Instructions[currentIndex]);
            }
            catch (Exception ex)
            {
                drawingEngine.RestoreCanvasState(preInstructionState);
                drawingEngine.UpdateTexture(canvasManager.canvasTexture);
                throw new Exception($"Error en línea {currentIndex + 1}: {ex.Message}");
            }
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
#region Instructions
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
                EvaluateAssignment(assign);
                break;
            case FunctionNode func:
                EvaluateFunction(func);
                break;
            case DrawCommandNode drawCmd:
                EvaluateDrawCommand(drawCmd);
                break;
            case FillNode fillNode:
                EvaluateFill(fillNode);
                break;
            case GoToNode gotoNode:
                EvaluateGoTo(gotoNode);
                break;
        }
    }
    private void EvaluateAssignment(AssignNode assign)
    {
        try
        {
            object value = EvaluateExpression(assign.Expression);

            value = ProcessValueForAssignment(value);

            variables[assign.Variable] = value;

        }
        catch (Exception ex)
        {
            throw new Exception($"Error en asignación '{assign.Variable}': {ex.Message}");
        }
    }
    private object ProcessValueForAssignment(object value)
    {
        if (value is double d)
        {
            return Convert.ToInt32(d);
        }
        return value;
    }

    private object EvaluateExpression(ASTNode node)
    {

        switch (node)
        {
            case NumberNode num: return Convert.ToInt32(num.Number);;

            case StringLiteralNode str:
                if (context.validColors.Contains(str.Value))
                    return str.Value;
                throw new Exception($"Color no válido: {str.Value}");
            case VariableNode var:
                if (variables.ContainsKey(var.Variable))
                    return variables[var.Variable];
                throw new Exception($"Variable no definida: {var.Variable}");

            case BinaryOpNode binOp:

                return EvaluateBinaryOperation(binOp);

            case BooleanOpNode boolOp:

                return EvaluateBooleanOperation(boolOp);

            case FunctionNode func:
                return EvaluateFunction(func);

            default:
                throw new Exception($"Unsupported expression type: {node.GetType().Name}");
        }
    }
    private object EvaluateBinaryOperation(BinaryOpNode binOp)
    {
        object left = EvaluateExpression(binOp.Left);
        object right = EvaluateExpression(binOp.Right);


        if (IsArithmeticOperator(binOp.Operator))
        {
            int leftInt = Convert.ToInt32(left);
            int rightInt = Convert.ToInt32(right);

            return binOp.Operator switch
            {
                "+" => leftInt + rightInt,
                "-" => leftInt - rightInt,
                "*" => leftInt * rightInt,
                "/" => rightInt != 0 ? leftInt / rightInt : throw new Exception("División por cero"),
                "%" => rightInt != 0 ? leftInt % rightInt : throw new Exception("División por cero"),
                "**" => (int)Math.Pow(leftInt, rightInt),
                _ => throw new Exception($"Operador aritmético no soportado: '{binOp.Operator}'")
            };
        }
        else
            throw new Exception($"Tipos incompatibles para operación aritmética: {left?.GetType().Name} y {right?.GetType().Name}");

    }

    private bool IsArithmeticOperator(string op)
    {
        return op switch
        {
            "+" or "-" or "*" or "/" or "%" or "**" => true,
            _ => false
        };
    }

    private object EvaluateBooleanOperation(BooleanOpNode boolOp)
    {
        // Evaluar ambos lados recursivamente
        object left = EvaluateExpression(boolOp.Left);
        object right = EvaluateExpression(boolOp.Right);

        // Determinar el tipo de operación booleana
        switch (boolOp.Operator)
        {
            // Operadores lógicos (&&, ||)
            case "&&":
            case "||":
                return EvaluateLogicalOperation(boolOp.Operator, left, right);

            // Operadores de comparación (==, !=, <, >, <=, >=)
            default:
                return EvaluateComparisonOperation(boolOp.Operator, left, right);
        }
    }

    private bool EvaluateLogicalOperation(string op, object left, object right)
    {
        // Convertir a booleanos
        bool leftBool = ConvertToBoolean(left);
        bool rightBool = ConvertToBoolean(right);

        return op switch
        {
            "&&" => leftBool && rightBool,
            "||" => leftBool || rightBool,
            _ => throw new Exception($"Operador lógico no soportado: '{op}'")
        };
    }

    private bool EvaluateComparisonOperation(string op, object left, object right)
    {
        // Caso especial: comparación de colores (strings)
        if (left is string leftStr && right is string rightStr)
        {
            return op switch
            {
                "==" => leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
                "!=" => !leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
                _ => throw new Exception($"Operador '{op}' no soportado para strings")
            };
        }

        // Comparación numérica
        if (TryConvertToNumber(left, out int leftNum) && TryConvertToNumber(right, out int rightNum))
        {
            return op switch
            {
                "==" => leftNum == rightNum,
                "!=" => leftNum != rightNum,
                "<" => leftNum < rightNum,
                ">" => leftNum > rightNum,
                "<=" => leftNum <= rightNum,
                ">=" => leftNum >= rightNum,
                _ => throw new Exception($"Operador de comparación no soportado: '{op}'")
            };
        }

        throw new Exception($"Tipos incompatibles para comparación: {left?.GetType().Name} y {right?.GetType().Name}");
    }

    private bool ConvertToBoolean(object value)
    {
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is string s) return !string.IsNullOrEmpty(s);

        throw new Exception($"No se puede convertir a booleano: {value?.GetType().Name}");
    }

    private bool TryConvertToNumber(object value, out int result)
    {
        result = 0;
        if (value == null) return false;

        if (value is int i)
        {
            result = i;
            return true;
        }

        if (value is double d)
        {
            result = (int)d;
            return true;
        }

        if (value is bool b)
        {
            result = b ? 1 : 0;
            return true;
        }

        return false;
    }
    #endregion
    #region Functions
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
            case "IsBrushColor":
            case "IsCanvasColor":
            case "GetColorCount":
                if (parameters[0] is string color && !context.validColors.Contains(color))
                {
                    throw new Exception($"Color inválido: '{color}'");
                }
                break;
        }
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
                return brush.Size == (int)parameters[0] ? 1 : 0;

            case "GetColorCount":
                int count = EvaluateGetColorCount((string)parameters[0], (int)parameters[1],
                                            (int)parameters[2], (int)parameters[3], (int)parameters[4]);
                return count;


            default:
                throw new Exception($"Función no reconocida: {func.Name}");
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

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x <maxX; x++)
            {
                if (canvasManager.GetPixelColor(x, y) == color)
                {
                    count++;
                }
            }
        }
        return count;
    }
    #endregion

    #region DrawCommands
    private void EvaluateDrawCommand(DrawCommandNode drawCmd)
    {

        if (drawCmd.Name == "DrawLine")
        {
            int dirY = (int)EvaluateExpression(drawCmd.Parameters[0]);
            int dirX = (int)EvaluateExpression(drawCmd.Parameters[1]);
            int distance = (int)EvaluateExpression(drawCmd.Parameters[2]);
            if (dirX > 1 || dirX < -1 || dirY > 1 || dirY < -1)
            {
                throw new Exception("Direcciones no válidas");
            }
            int endX = brush.CurrentX + dirX * distance;
            int endY = brush.CurrentY + dirY * distance;
            if (brush.Color != "Transparent")
            {
                drawingEngine.DrawLine(
                    brush.CurrentX,
                    brush.CurrentY,
                    endX,
                    endY,
                    brush.Color,
                    brush.Size
                );
            }

            brush.CurrentX = endX;
            brush.CurrentY = endY;
            drawingEngine.UpdateTexture(canvasManager.canvasTexture);
        }
        if (drawCmd.Name == "DrawCircle")
        {
            int dirY = (int)EvaluateExpression(drawCmd.Parameters[0]);
            int dirX = (int)EvaluateExpression(drawCmd.Parameters[1]);
            int radio = (int)EvaluateExpression(drawCmd.Parameters[2]);
            if (dirX > 1 || dirX < -1 || dirY > 1 || dirY < -1)
            {
                throw new Exception("Direcciones no válidas");
            }
            int endX = brush.CurrentX + dirX * radio;
            int endY = brush.CurrentY + dirY * radio;
            if (brush.Color != "Transparent")
            {
                drawingEngine.DrawCircle(
                    endX,
                    endY,
                    radio,
                    brush.Color,
                    brush.Size
            );
            }

            brush.CurrentX = endX;
            brush.CurrentY = endY;
            drawingEngine.UpdateTexture(canvasManager.canvasTexture);
        }
        if (drawCmd.Name == "DrawRectangle")
        {
            int dirY = (int)EvaluateExpression(drawCmd.Parameters[0]);
            int dirX = (int)EvaluateExpression(drawCmd.Parameters[1]);
            if (dirX > 1 || dirX < -1 || dirY > 1 || dirY < -1)
            {
                throw new Exception("Direcciones no válidas");
            }
            int distance = (int)EvaluateExpression(drawCmd.Parameters[2]);
            int width = (int)EvaluateExpression(drawCmd.Parameters[3]);
            int height = (int)EvaluateExpression(drawCmd.Parameters[4]);
            if (brush.Color != "Transparent")
            {
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
            }

            brush.CurrentX = brush.CurrentX + dirX * distance;
            brush.CurrentY = brush.CurrentY + dirY * distance;
            drawingEngine.UpdateTexture(canvasManager.canvasTexture);

        }
    }
    private void EvaluateFill(FillNode fillNode)
    {
        if (brush.Color == "Transparent")
        {
            throw new Exception("No se puede usar Fill con color Transparent");
        }

        string targetColor = canvasManager.GetPixelColor(brush.CurrentX, brush.CurrentY);

        if (targetColor == brush.Color)
        {

            return;
        }
        FloodFill(brush.CurrentX, brush.CurrentY, targetColor, brush.Color);
        drawingEngine.UpdateTexture(canvasManager.canvasTexture);

    }

    private void FloodFill(int startX, int startY, string targetColor, string fillColor)
    {

        Queue<(int x, int y)> pixels = new Queue<(int, int)>();
        pixels.Enqueue((startX, startY));

        HashSet<(int x, int y)> visited = new HashSet<(int, int)>();
        int canvasSize = canvasManager.canvasSize;

        while (pixels.Count > 0)
        {
            var (x, y) = pixels.Dequeue();

            if (visited.Contains((x, y))) continue;
            visited.Add((x, y));

            // Verificar límites
            if (x < 0 || x >= canvasSize || y < 0 || y >= canvasSize || targetColor == brush.Color)
                continue;

            // Obtener y comparar color
            string currentColor = canvasManager.GetPixelColor(x, y);
            if (!string.Equals(currentColor, targetColor, StringComparison.OrdinalIgnoreCase))
                continue;

            Color color = drawingEngine.colorMap[fillColor];
            drawingEngine.DrawBrushAt(x, y, color, brush.Size);
            pixels.Enqueue((x + 1, y));
            pixels.Enqueue((x - 1, y));
            pixels.Enqueue((x, y + 1));
            pixels.Enqueue((x, y - 1));
            /* pixels.Enqueue((x + 1, y - 1));
            pixels.Enqueue((x - 1, y - 1));
            pixels.Enqueue((x - 1, y + 1));
            pixels.Enqueue((x + 1, y + 1)); */


        }


    }
#endregion
    private void EvaluateGoTo(GoToNode gotoNode)
    {
    
        bool condition = Convert.ToBoolean(EvaluateExpression(gotoNode.Condition));
        if (condition)
        {
            if (labels.TryGetValue(gotoNode.Label, out int targetIndex))
            {
                // Saltar a la instrucción con la etiqueta
                currentIndex = targetIndex - 1; // -1 porque después se incrementa
            }
            else
            {
                throw new Exception($"Etiqueta no encontrada: {gotoNode.Label}");
            }
        }
    }
}

        


