using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IASTValidator<T> where T : ASTNode
{
    void Validate(T node, SemanticContext context);
}

public class SemanticContext {
    public BrushState Brush { get; } = new BrushState(); 
    public HashSet<string> Variables { get; } = new HashSet<string>();
    public Dictionary<string, System.Type> VariableTypes { get; } = new Dictionary<string, System.Type>();
    public HashSet<string> Labels { get; } = new HashSet<string>();
    public List<string> Errors { get; } = new List<string>();
    public string CurrentBrushColor { get; set; }
    public bool HasSpawn { get; set; } = false;
    public int CanvasSize { get; set; } 

    public void AddError(string message) => Errors.Add(message);
    public void AddWarning(string message) => Errors.Add($"[Advertencia] {message}");
}


public class SpawnValidator : IASTValidator<SpawnNode>,IASTValidator<ProgramNode>
{
    private int _canvasSize;

    public SpawnValidator(int canvasSize)
    {
        _canvasSize = canvasSize;
    }

    public void Validate(ProgramNode programNode, SemanticContext context)
    {
         if (programNode.Instructions.Count == 0|| !(programNode.Instructions[0] is (SpawnNode)))
        {
            context.AddError("El codigo no inicia con la instruccion Spawn");  
        }
        else if (context.HasSpawn) 
        {
            context.AddError("Spawn solo puede usarse una vez.");
        }
        context.HasSpawn = true;
    }
    public void Validate(SpawnNode node, SemanticContext context)
    {

        // Regla 1: X e Y deben ser no negativos
        if (node.X < 0 || node.Y < 0 || node.X >= _canvasSize || node.Y >= _canvasSize)
        {
            context.AddError($"Coordenadas inválidas: ({node.X}, {node.Y}). Deben estar dentro del canvas");
        }
        if (context.Errors.Count == 0)
        {
            context.Brush.CurrentX = node.X;
            context.Brush.CurrentY = node.Y;
        }
    }
    
}
public class ColorValidator : IASTValidator<ColorNode> 
{
     private static readonly HashSet<string> validColors = new HashSet<string>{
         "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Black", "White", "Transparent" 
     };

    public void Validate(ColorNode node, SemanticContext context)
    {
        if (!validColors.Contains(node.Color))
        {
            context.AddError("Color inválido " + node.Color);

        }
        context.Brush.Color = node.Color;
    }
} 
public class SizeValidator : IASTValidator<SizeNode> 
{
    private int CanvasSize;

    public SizeValidator(int canvasSize) 
    {
        CanvasSize = canvasSize;
    }

    public void Validate(SizeNode node, SemanticContext context)
    {
        int adjustedSize = node.Size;
        if (node.Size <= 0)
        {
            context.AddError($"El tamaño debe ser un entero positivo: {node.Size}.");
        }

        if (node.Size > CanvasSize)
        {
            context.AddError($"El tamaño ({node.Size}) es mayor que el tamaño del canvas ({CanvasSize}).");
        }
        if (node.Size % 2 == 0)
        {
            adjustedSize = Math.Max(node.Size - 1,1);
            context.AddWarning($"Tamaño ajustado a {adjustedSize}. Use números impares.");
        }
        context.Brush.Size = adjustedSize;
    }
}

public class DrawCommandValidator : IASTValidator<DrawCommandNode>
{
    public void Validate(DrawCommandNode node, SemanticContext context)
    {
        switch (node.Name)
        {
            case "DrawLine":
                ValidateDrawLine(node, context);
                break;
            case "DrawCircle":
                ValidateDrawCircle(node, context);
                break;
            case "DrawRectangle":
                ValidateDrawRectangle(node, context);
                break;
            default:
                context.AddError($"Comando de dibujo no reconocido: '{node.Name}'.");
                break;
        }
    }
     public void ValidateCoordenadas(int dirX,int dirY,SemanticContext context)
    {
        if (dirX < -1 || dirX >1 || dirY > 1 || dirY< -1)
        {
            context.AddError($"Dirección inválida en DrawLine: ({dirX}, {dirY}). Deben ser -1, 0, o 1.");
        }
    }
    
    public void ValidateDrawLine(DrawCommandNode node, SemanticContext context)
    {
        if (node.Parameters.Count != 3)
        {
            context.AddError($"El comando DrawLine requiere 3 parámetros.");
        }
        if (node.Parameters[0] is NumberNode dirX && node.Parameters[1] is NumberNode dirY && node.Parameters[2] is NumberNode distance)
        {
            ValidateCoordenadas(dirX.Number, dirY.Number, context);
            if (distance.Number <= 0)
            {
                context.AddError($"Distancia inválida en DrawLine: {distance.Number}. Debe ser mayor que cero.");
            }
            int newX = context.Brush.CurrentX + distance.Number * dirX.Number;
            int newY = context.Brush.CurrentY + distance.Number * dirY.Number;
            if (newX < 0 || newX >= context.CanvasSize || newY < 0 || newY >= context.CanvasSize)
            {
                context.AddError($"DrawLine lleva a Wall-E fuera del canvas. Posición final: ({newX}, {newY})");
            }
            if (context.Errors.Count == 0)
            {
                context.Brush.CurrentX = newX;
                context.Brush.CurrentY = newY;
            }
        }
        else
            context.AddError($"Parametros Invalidos ");
    }
    public void ValidateDrawCircle(DrawCommandNode node, SemanticContext context)
    {
        if (node.Parameters.Count != 3)
        {
            context.AddError($"El comando DrawCircle requiere 3 parámetros.");
        }
        if (node.Parameters[0] is NumberNode dirX && node.Parameters[1] is NumberNode dirY && node.Parameters[2] is NumberNode radius) {
            ValidateCoordenadas(dirX.Number, dirY.Number, context);
            if (radius.Number <= 0 || radius.Number >= context.CanvasSize)
            {
                context.AddError($"Distancia inválida en DrawCircle: {radius.Number}. Debe ser mayor que cero.");
            }
        
            int newX = context.Brush.CurrentX + radius.Number * dirX.Number;
            int newY = context.Brush.CurrentY + radius.Number * dirY.Number;
            if (newX < 0 || newX >= context.CanvasSize || newY < 0 || newY >= context.CanvasSize)
            {
                context.AddError($"DrawLine lleva a Wall-E fuera del canvas. Posición final: ({newX}, {newY})");
            }
            if (context.Errors.Count == 0)
            {
                context.Brush.CurrentX = newX;
                context.Brush.CurrentY = newY;
            }
        }
        else
            context.AddError($"Parametros Invalidos ");
    }

    public void ValidateDrawRectangle(DrawCommandNode node, SemanticContext context)
    {
        if (node.Parameters.Count != 5)
        {
            context.AddError($"El comando DrawRectangle requiere 5 parámetros.");
        }
        if (node.Parameters[0] is NumberNode dirX && node.Parameters[1] is NumberNode dirY && node.Parameters[2] is NumberNode distance && node.Parameters[3] is NumberNode width && node.Parameters[4] is NumberNode height)
        {
            ValidateCoordenadas(dirX.Number, dirY.Number, context);
            if (width.Number <= 0 || height.Number <= 0 || width.Number >= context.CanvasSize || height.Number >= context.CanvasSize)
            {
                context.AddError($"Tamaño inválido en DrawRectangle: {width.Number} x {height.Number}. Deben ser mayores que cero.");
            }
            int newX = context.Brush.CurrentX + distance.Number * dirX.Number;
            int newY = context.Brush.CurrentY + distance.Number * dirY.Number;
            if (newX < 0 || newX >= context.CanvasSize || newY < 0 || newY >= context.CanvasSize)
            {
                context.AddError($"DrawLine lleva a Wall-E fuera del canvas. Posición final: ({newX}, {newY})");
            }
            if (context.Errors.Count == 0)
            {
                context.Brush.CurrentX = newX;
                context.Brush.CurrentY = newY;
            }
        }
    }
}

    public class FillValidator : IASTValidator<FillNode>
    {
        public void Validate(FillNode node, SemanticContext context)
        {
            if (context.CurrentBrushColor == "Transparent")
            {
                context.AddError("No se puede usar Fill con color Transparent.");
            }
        }
    }
    public class FunctionValidator : IASTValidator<FunctionNode>
    {
        // Lista de funciones válidas y sus especificaciones
        private readonly Dictionary<string, FunctionInspector> _validFunctions = new Dictionary<string, FunctionInspector>
    {
        // Funciones sin parámetros
        { "GetActualX", new FunctionInspector(0, new List<System.Type>()) },
        { "GetActualY", new FunctionInspector(0, new List<System.Type>()) },
        { "GetCanvasSize", new FunctionInspector(0, new List<System.Type>()) },

        // Funciones con parámetros
        { "GetColorCount", new FunctionInspector(5, new List<System.Type> { typeof(string), typeof(int), typeof(int), typeof(int), typeof(int) }) },
        { "IsBrushColor", new FunctionInspector(1, new List<System.Type> { typeof(string) }) },
        { "IsBrushSize", new FunctionInspector(1, new List<System.Type> { typeof(int) }) },
        { "IsCanvasColor", new FunctionInspector(3, new List<System.Type> { typeof(string), typeof(int), typeof(int) }) }
    };

        public void Validate(FunctionNode node, SemanticContext context)
        {
            if (!_validFunctions.ContainsKey(node.Name))
            {
                context.AddError($"Función no reconocida: '{node.Name}'.");
                return;
            }
            var spec = _validFunctions[node.Name];
            if (node.Parameters.Count != spec.ExpectedParamCount)
            {
                context.AddError(
                    $"{node.Name} requiere {spec.ExpectedParamCount} parámetros. " +
                    $"Se proporcionaron {node.Parameters.Count}."
                );
                return;
            }

            // Validar tipo de cada parámetro
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                ASTNode param = node.Parameters[i];
                System.Type expectedType = spec.ExpectedParamTypes[i];

                // Caso 1: Parámetro es un string (ej: color)
                if (expectedType == typeof(string) && !(param is VariableNode))
                {
                    context.AddError(
                        $"Parámetro {i + 1} de {node.Name} debe ser un string. " +
                        $"Se recibió: {param.GetType().Name}."
                    );
                }

                // Caso 2: Parámetro es un número (ej: coordenadas)
                if (expectedType == typeof(int) && !IsNumericNode(param))
                {
                    context.AddError(
                        $"Parámetro {i + 1} de {node.Name} debe ser numérico. " +
                        $"Se recibió: {param.GetType().Name}."
                    );
                }
                if (expectedType == typeof(string) && !(param is StringLiteralNode || param is VariableNode))
                {
                    context.AddError($"Parámetro {i + 1} debe ser un string literal o variable.");
                }
            }

            if (node.Name == "IsCanvasColor" && node.Parameters[0] is StringLiteralNode colorVar)
            {
                if (!context.Variables.Contains(colorVar.Value))
                {
                    context.AddError(
                        $"Color no declarado: '{colorVar.Value}'. " +
                        "Los colores deben ser variables predefinidas."
                    );
                }
            }
        }

        private bool IsNumericNode(ASTNode node)
        {
            return node is NumberNode || node is VariableNode || node is BinaryOpNode;
        }

        private class FunctionInspector
        {
            public int ExpectedParamCount { get; }
            public List<System.Type> ExpectedParamTypes { get; }

            public FunctionInspector(int count, List<System.Type> types)
            {
                ExpectedParamCount = count;
                ExpectedParamTypes = types;
            }
        }
    }

public class GoToValidator : IASTValidator<GoToNode>
{
    public void Validate(GoToNode node, SemanticContext context)
    {
        // Verificar si la etiqueta existe
        if (!context.Labels.Contains(node.Label))
        {
            context.AddError($"Etiqueta no declarada: '{node.Label}'.");
        }

        // Verificar que la condición sea booleana
        if (!IsBooleanExpression(node.Condition))
        {
            context.AddError($"La condición en GoTo debe ser una expresión booleana.");
        }
    }

    private bool IsBooleanExpression(ASTNode node)
    {
        return node is BooleanOpNode || node is VariableNode;
    }
   
}


