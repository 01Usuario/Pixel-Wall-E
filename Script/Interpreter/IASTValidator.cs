using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;

public interface IASTValidator<T> where T : ASTNode
{
    void Validate(T node, SemanticContext context);
}

public class SemanticContext
{
    public Brush Brush { get; } = new Brush();
    public readonly HashSet<string> validColors = new HashSet<string>{
         "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Black", "White", "Transparent"
     };
    public HashSet<string> Variables { get; } = new HashSet<string>();
    public Dictionary<string, System.Type> VariableTypes { get; } = new Dictionary<string, System.Type>();
    public HashSet<string> Labels { get; } = new HashSet<string>();
    public List<string> Errors { get; } = new List<string>();
    public List<string> Warnings { get; } = new List<string>();
    public string CurrentBrushColor { get; set; }
    public bool HasSpawn { get; set; } = false;
    public int CanvasSize { get; set; }

    public void AddError(string message) => Errors.Add(message);
    public void AddWarning(string message) => Warnings.Add($"[Advertencia] {message}");

    public void AddAllLabelsed(ProgramNode intructions, HashSet<string> labels)
    {
        foreach (var instruction in intructions.Instructions)
        {
            if (instruction is LabelNode labelNode)
            {
                labels.Add(labelNode.Name);
            }
        }
    }
   
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
        bool hasSpawn = false;
        if (programNode.Instructions.Count == 0 || !(programNode.Instructions[0] is (SpawnNode)))
        {
            context.AddError("El codigo no inicia con la instruccion Spawn");
        }
        foreach (var instruction in programNode.Instructions)
        {
            if (instruction is SpawnNode)
            {
                if (hasSpawn)
                {
                    context.AddError("Solo puede haber un comando Spawn");
                }
                hasSpawn = true;
            }
        }
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
     
    public void Validate(ColorNode node, SemanticContext context)
    {
        if (!context.validColors.Contains(node.Color))
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
            node.Size -= 1;
            context.AddWarning($"Tamaño ajustado a {node.Size}. Use números impares.");
        }
        context.Brush.Size = node.Size;
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

    public void ValidateDrawLine(DrawCommandNode node, SemanticContext context)
    {
        if (node.Parameters.Count != 3)
        {
            context.AddError($"El comando DrawLine requiere 3 parámetros.");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            if (!IsValidParameterType(node.Parameters[i]))
            {
                context.AddError($"Parámetro {i + 1} tipo inválido. Debe ser número, variable o expresión numérica");
                return;
            }
        }

        TryEvaluateParameters(node, context);
    }
     public void ValidateDrawCircle(DrawCommandNode node, SemanticContext context)
    {
        if (node.Parameters.Count != 3)
        {
            context.AddError($"El comando DrawCircle requiere 3 parámetros.");
        }
        for (int i = 0; i < 3; i++)
        {
            if (!IsValidParameterType(node.Parameters[i]))
            {
                context.AddError($"Parámetro {i + 1} tipo inválido. Debe ser número, variable o expresión numérica");
                return;
            }
        }

        TryEvaluateParameters(node, context);
    }

    public void ValidateDrawRectangle(DrawCommandNode node, SemanticContext context)
    {
        if (node.Parameters.Count != 5)
        {
            context.AddError($"El comando DrawRectangle requiere 5 parámetros.");
        }
         for (int i = 0; i < 3; i++)
        {
            if (!IsValidParameterType(node.Parameters[i]))
            {
                context.AddError($"Parámetro {i+1} tipo inválido. Debe ser número, variable o expresión numérica");
                return;
            }
        }
        TryEvaluateParameters(node, context);
    }

    private bool IsValidParameterType(ASTNode node)
    {
        // Acepta números, variables, operaciones binarias y funciones
        return node is NumberNode || 
            node is VariableNode || 
            node is BinaryOpNode || 
            node is FunctionNode;
    }

    private void TryEvaluateParameters(DrawCommandNode node, SemanticContext context)
    {
        try
        {
            bool allConstant = node.Parameters.All(p => p is NumberNode);
            
            if (allConstant)
            {
                int dirX = (node.Parameters[0] as NumberNode)?.Number ?? 0;
                int dirY = (node.Parameters[1] as NumberNode)?.Number ?? 0;
                int distance = (node.Parameters[2] as NumberNode)?.Number ?? 0;
                
                ValidateCoordinates(dirX, dirY, context);
                
                if (distance <= 0)
                {
                    context.AddError($"Distancia inválida: {distance}. Debe ser mayor que cero.");
                }
                
                int newX = context.Brush.CurrentX + distance * dirX;
                int newY = context.Brush.CurrentY + distance * dirY;
                
                if (newX < 0 || newX >= context.CanvasSize || 
                    newY < 0 || newY >= context.CanvasSize)
                {
                    context.AddError($"Posición final fuera del canvas: ({newX}, {newY})");
                }
                
                if (context.Errors.Count == 0)
                {
                    context.Brush.CurrentX = newX;
                    context.Brush.CurrentY = newY;
                }
            }
            else
            {
                // Para parámetros variables, solo podemos validar parcialmente
                ValidateParameterRanges(node, context);
            }
        }
        catch (Exception ex)
        {
            context.AddWarning($"No se pudo validar parámetros completamente: {ex.Message}");
        }
    }

    private void ValidateParameterRanges(DrawCommandNode node, SemanticContext context)
    {
        // Validar que los parámetros de dirección podrían ser válidos
        if (node.Parameters[0] is NumberNode dirXNode)
        {
            int dirX = dirXNode.Number;
            if (dirX < -1 || dirX > 1)
            {
                context.AddError($"Valor dirX inválido: {dirX}. Debe ser -1, 0 o 1.");
            }
        }
        
        if (node.Parameters[1] is NumberNode dirYNode)
        {
            int dirY = dirYNode.Number;
            if (dirY < -1 || dirY > 1)
            {
                context.AddError($"Valor dirY inválido: {dirY}. Debe ser -1, 0 o 1.");
            }
        }
        
        // Validar que la distancia podría ser positiva
        if (node.Parameters[2] is NumberNode distanceNode)
        {
            if (distanceNode.Number <= 0)
            {
                context.AddError($"Distancia inválida: {distanceNode.Number}. Debe ser mayor que cero.");
            }
        }
    }

    private void ValidateCoordinates(int dirX, int dirY, SemanticContext context)
    {
        if (dirX < -1 || dirX > 1 || dirY < -1 || dirY > 1)
        {
            context.AddError($"Dirección inválida: ({dirX}, {dirY}). Deben ser valores entre -1 y 1.");
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
            if (context.CurrentBrushColor == context.Brush.Color)
            {
                context.Warnings.Add("El color de relleno coincide con el color de la casilla actual.");
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
        private readonly HashSet<string> _colorFunctions = new HashSet<string>
    {
        "IsBrushColor",
        "IsCanvasColor",
        "GetColorCount"
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
            if (_colorFunctions.Contains(node.Name) && node.Parameters.Count > 0)
            {
                ValidateColorParam(node.Parameters[0], context);
            }


            for (int i = 0; i < node.Parameters.Count; i++)
            {
                ASTNode param = node.Parameters[i];
                System.Type expectedType = spec.ExpectedParamTypes[i];



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

        }
        private bool IsNumericNode(ASTNode node)
        {
            return node is NumberNode || node is VariableNode || node is BinaryOpNode;
        }
        private void ValidateColorParam(ASTNode param, SemanticContext context)
        {
            if (param is StringLiteralNode strNode)
            {
                if (!context.validColors.Contains(strNode.Value))
                {
                    context.AddError($"Color no válido: '{strNode.Value}'. Use: {string.Join(", ", context.validColors)}");
                }
            }
            else if (param is VariableNode varNode)
            {
                if (context.VariableTypes.TryGetValue(varNode.Variable, out var type))
                {
                    if (type != typeof(string))
                    {
                        context.AddError($"La variable '{varNode.Variable}' debe ser de tipo string (color)");
                    }
                }
            }
        }



    }

    public class GoToValidator : IASTValidator<GoToNode>
    {
        private const int MAX_LOOPS = 1000;
        private int loopCount = 0;
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

            if (++loopCount > MAX_LOOPS)
            {
                context.AddError("Posible bucle infinito detectado");
            }
        }


        private bool IsBooleanExpression(ASTNode node)
        {
            return node is BooleanOpNode;
        }

    }

    public class VariableValidator : IASTValidator<VariableNode>
    {
        public void Validate(VariableNode node, SemanticContext context)
        {
            context.Variables.Add(node.Variable);
        }
    }
    public class LabelValidator : IASTValidator<LabelNode>
    {
        public void Validate(LabelNode node, SemanticContext context)
        {
            if (!context.Labels.Contains(node.Name))
            {
                context.Labels.Add(node.Name);

            }

        }
    }
    public class AssignValidator : IASTValidator<AssignNode>
    {
        public void Validate(AssignNode node, SemanticContext context)
        {
            if (node.Variable == null)
            {
                context.AddError("No se puede asignar a una variable nula.");
            }
            if (node.Expression == null)
            {
                context.AddError("No hay nada que asignar");
            }
            if (node.Expression is StringLiteralNode strNode && context.validColors.Contains(strNode.Value))
            {
                context.VariableTypes[node.Variable] = typeof(string);
            }

        }
    }


    public class FunctionInspector
    {
        public int ExpectedParamCount { get; }
        public List<System.Type> ExpectedParamTypes { get; }

        public FunctionInspector(int count, List<System.Type> types)
        {
            ExpectedParamCount = count;
            ExpectedParamTypes = types;
        }
    }
    public class CommandInspector
    {
        public int ExpectedParamCount { get; }
        public List<Type> ExpectedParamTypes { get; }
        public Action<DrawCommandNode, SemanticContext> Validator { get; }

        public CommandInspector(int count, List<Type> types, Action<DrawCommandNode, SemanticContext> validator)
        {
            ExpectedParamCount = count;
            ExpectedParamTypes = types;
            Validator = validator;
        }
    }


