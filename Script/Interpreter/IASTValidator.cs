using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IASTValidator <T> where T : ASTNode
{
    void Validate(T node, SemanticContext context);
}
public class SemanticContext {
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

 public class FirstSpawnNodeValidator : IASTValidator<ProgramNode>
 {
    public void Validate(ProgramNode node, SemanticContext context)
    {
        if (node.Instructions.Count == 0|| !(node.Instructions[0] is (SpawnNode)))
        {
            context.AddError("El codigo no inicia con la instruccion Spawn");  
        }
        else if (context.HasSpawn) 
        {
            context.AddError("Spawn solo puede usarse una vez.");
        }
        context.HasSpawn = true;
    }
 }
public class SpawnValidator : IASTValidator<SpawnNode> 
{
    private int _canvasSize; 

    public SpawnValidator(int canvasSize) 
    {
        _canvasSize = canvasSize;
    }

    public void Validate(SpawnNode node, SemanticContext context) 
    {
        // Regla 1: X e Y deben ser no negativos
        if (node.X < 0 || node.Y < 0) 
        {
            context.AddError($"Coordenadas inválidas: ({node.X}, {node.Y}). Deben ser ≥ 0.");
        }

        // Regla 2: X e Y deben estar dentro del canvas
        if (node.X >= _canvasSize || node.Y >= _canvasSize) 
        {
            context.AddError($"Coordenadas fuera del canvas ({_canvasSize}x{_canvasSize}): ({node.X}, {node.Y}).");
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
        if(!validColors.Contains(node.Color)){
            context.AddError("Color inválido "+node.Color);

        }
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

        if (node.Size > CanvasSize) {
            context.AddError($"El tamaño ({node.Size}) es mayor que el tamaño del canvas ({CanvasSize}).");
        }
        if(node.Size%2==0){
            int adjustedSize = node.Size - 1;
            context.AddWarning($"Tamaño ajustado a {adjustedSize}. Use números impares.");
        }
    }
}

    /* public class DrawLineValidator : IASTValidator<DrawLineNode> 
    {
        public void Validate(DrawLineNode node, SemanticContext context) 
        {
            // Validar direcciones
            if (node.DirX < -1 || node.DirX > 1 || node.DirY < -1 || node.DirY > 1) 
            {
                context.AddError($"Dirección inválida en DrawLine: ({node.DirX}, {node.DirY}). Deben ser -1, 0, o 1.");
            }


            // Validar distancia
            if (node.Distance <= 0) 
            {
                context.AddError($"Distancia inválida en DrawLine: {node.Distance}. Debe ser mayor que cero.");
            }
        }
    }
public class DrawCircleValidator : IASTValidator<DrawCircleNode> 
{
    public void Validate(DrawCircleNode node, SemanticContext context) 
    {
        // Validar direcciones
        if (node.DirX < -1 || node.DirX > 1 || node.DirY < -1 || node.DirY > 1) 
        {
            context.AddError($"Dirección inválida en DrawCircle: ({node.DirX}, {node.DirY}). Deben ser -1, 0, o 1.");
        }

        // Validar radio
        if (node.Radius <= 0) 
        {
            context.AddError($"Radio inválido en DrawCircle: {node.Radius}. Debe ser mayor que cero.");
        }
    }
}
public class DrawRectangleValidator : IASTValidator<DrawRectangleNode> 
{
    public void Validate(DrawRectangleNode node, SemanticContext context) 
    {
        // Validar direcciones
        if (node.DirX < -1 || node.DirX > 1 || node.DirY < -1 || node.DirY > 1) 
        {
            context.AddError($"Dirección inválida en DrawRectangle: ({node.DirX}, {node.DirY}). Deben ser -1, 0, o 1.");
        }

        // Validar dimensiones del rectángulo
        if (node.Width <= 0 || node.Height <= 0) 
        {
            context.AddError($"Dimensiones inválidas en DrawRectangle: Ancho={node.Width}, Alto={node.Height}. Deben ser mayores que cero.");
        }

        // Validar distancia
        if (node.Distance <= 0) 
        {
            context.AddError($"Distancia inválida en DrawRectangle: {node.Distance}. Debe ser mayor que cero.");
        } 
    
     }
}

    */
   
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

        // Obtener especificaciones de la función
        var spec = _validFunctions[node.Name];
        
        // Validar cantidad de parámetros
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
            if (expectedType == typeof(string) && !(param is StringLiteralNode || param is VariableNode)) {
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

    // Método auxiliar para detectar nodos numéricos
    private bool IsNumericNode(ASTNode node)
    {
        return node is NumberNode || node is VariableNode || node is BinaryOpNode;
    }

    // Clase interna para especificaciones de funciones
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

