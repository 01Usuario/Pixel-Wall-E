using System.Collections.Generic;
using UnityEngine;
using System;


public class Parser
{
    private List<Token> tokens;
    private int currentTokenIndex;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        this.currentTokenIndex = 0;

    }
     public ASTNode ParseProgram(List<Token> tokens)
    {
        ProgramNode program = new ProgramNode();
        program.Instructions = new List<ASTNode>();

        while (currentTokenIndex < tokens.Count)
        {
            ASTNode instruction = ParseInstruction();
            program.Instructions.Add(instruction);
        }

        return program;
    }
    private ASTNode ParseInstruction()
{
    Token token = CurrentToken();
    
    // Primero verificamos el tipo de token
    switch (token.Type)
    {
        case TokenType.Keyword:
            return ParseKeywordInstruction();
            
        case TokenType.DrawingCommand:
            return ParseDrawCommand();
            
        case TokenType.Function:
            return ParseFunction();
            
        case TokenType.Identifier:
            if (PeekNextToken()?.Type == TokenType.AssignmentOperator)
                return ParseAssign();
            break;
                
        case TokenType.Label:
            return ParseLabel();
    }
    
    throw new Exception($"Instrucción no reconocida: {token.Value} (línea {token.Line})");
}

    private ASTNode ParseKeywordInstruction()
    {
        switch (CurrentToken().Value)
        {
            case "Spawn":
                return ParseSpawn();
            case "Color":
                return ParseColor();
            case "Size":
                return ParseSize();
            case "Fill":
                return ParseFill();
            case "GoTo":
                return ParseGoTo();
            default:
                throw new Exception($"Palabra clave no reconocida: {CurrentToken().Value} (línea {CurrentToken().Line})");
        }
    }

    private Token PeekNextToken()
    {
        return currentTokenIndex + 1 < tokens.Count ? tokens[currentTokenIndex + 1] : null;
    }
        
        private Token CurrentToken(){
            return tokens[currentTokenIndex];
        }

    private void Expect(TokenType type, string value) 
    {
        if (currentTokenIndex >= tokens.Count)
            throw new Exception($"Se esperaba {value} pero no hay más tokens.");

        Token current = CurrentToken();
        if (current.Type != type || current.Value != value)
        {
            throw new Exception($"Se esperaba {value} en línea {current.Line}, se obtuvo {current.Value}");
        }
        ConsumeToken();
    }
    private void ConsumeToken() {
        currentTokenIndex++;
    }
    private SpawnNode ParseSpawn()
    {
        if(CurrentToken().Value!="Spawn")
        throw new Exception($"Se esperaba 'Spawn' en línea {CurrentToken().Line}");
        ConsumeToken();
        Expect(TokenType.Punctuation, "(");
        NumberNode x = ParseNumber();
        Expect(TokenType.Punctuation, ",");
        NumberNode y = ParseNumber();
        Expect(TokenType.Punctuation, ")");
        return new SpawnNode(x.Number,y.Number);
    }
    private NumberNode ParseNumber()
{
    if (CurrentToken().Type != TokenType.Number)
    {
        throw new Exception($"Se esperaba un número en línea {CurrentToken().Line}");
    }

    if (!int.TryParse(CurrentToken().Value, out int numberValue))
    {
        throw new Exception($"Número inválido '{CurrentToken().Value}' en línea {CurrentToken().Line}");
    }
    NumberNode node = new NumberNode(numberValue);
    ConsumeToken(); 

    return node;
}
    private VariableNode ParseVariable()
    {
        if(CurrentToken().Type!=TokenType.Identifier && CurrentToken().Type!=TokenType.StringLiteral)    
        throw new Exception($"Se esperaba una variable en línea {CurrentToken().Line}, se obtuvo {CurrentToken().Value}, tipo: {CurrentToken().Type}");
        
        else
        {
            string variableName = CurrentToken().Value;
            ConsumeToken();
            return new VariableNode(variableName);
        }
    }   
    private LabelNode ParseLabel()
    {
        var lebelName = CurrentToken().Value;
        ConsumeToken();
        return new LabelNode(lebelName);
    }  
   private ASTNode ParsePrimary()
    {
        if (currentTokenIndex >= tokens.Count)
            throw new Exception("Expresión incompleta.");

        Token token = CurrentToken();

        

        if (CurrentToken().Type == TokenType.Identifier || CurrentToken().Type==TokenType.StringLiteral)
        {
            return ParseVariable();
        }
        else if (CurrentToken().Type == TokenType.Number)
        {
            return ParseNumber();
        }
        else if (CurrentToken().Type == TokenType.Label)
        {
            return ParseLabel();
        }
       
        else if (CurrentToken().Type == TokenType.Function)
        {
            return ParseFunction();
        }
        else if (token.Value == "(")
        {
            ConsumeToken();
            ASTNode expr = ParseExpression();
            if (currentTokenIndex >= tokens.Count || CurrentToken().Value != ")")
                throw new Exception($"Paréntesis no cerrado en línea {token.Line}.");
            ConsumeToken();
            return expr;
        }
        else
        {
            throw new Exception($"Se esperaba una expresión en línea {CurrentToken().Line}, se obtuvo {CurrentToken().Value}");
        }

    }
   private ASTNode ParseExpression(int minPrecedence = 0)
{
    ASTNode left = ParsePrimary();

    while (currentTokenIndex < tokens.Count)
    {
        Token opToken = CurrentToken();
        
        if (!IsOperator(opToken)) break;

        int precedence = GetOperatorPriorityLevel(opToken.Value);
        if (precedence < minPrecedence) break;

        ConsumeToken();
        ASTNode right = ParseExpression(precedence);
        left = new BinaryOpNode(opToken.Value, left, right);
    }

    return left;
}
    private bool IsOperator(Token token)
    {
        return token.Type == TokenType.ArithmeticOperator 
            || token.Type == TokenType.BooleanOperator;
    }
    private int GetOperatorPriorityLevel(string op)
    {
    return op switch
        {
            "+" or "-" => 1,
            "*" or "/" or "%" => 2,
            "**" => 3,
            "&&" => 4,
            "||" => 5,
            "==" or "!=" or ">" or "<" or ">=" or "<=" => 6,
            _ => 0,
        };
    }
    private AssignNode ParseAssign()
{
    if (currentTokenIndex >= tokens.Count)
        throw new Exception("Asignación incompleta.");

    string variable = CurrentToken().Value;
    ConsumeToken();

    // Verificar operador '<-'
    if (currentTokenIndex >= tokens.Count || CurrentToken().Value != "<-")
        throw new Exception($"Se esperaba '<-' después de '{variable}'.");

    ConsumeToken();

    ASTNode expr = ParseExpression();

    // Si hay tokens restantes que no son parte de otra instrucción, error
 /*    if (currentTokenIndex < tokens.Count && 
        CurrentToken().Type != TokenType.Keyword && 
        CurrentToken().Type != TokenType.DrawingCommand && 
        CurrentToken().Type != TokenType.Identifier)
    {
        throw new Exception($"Carácter inesperado: '{CurrentToken().Value}'");
    } */

    return new AssignNode(variable, expr);
} 
    private ColorNode ParseColor()
    {
        if (CurrentToken().Value != "Color" || CurrentToken().Type != TokenType.Keyword)
        {
            throw new Exception($"Error en línea {CurrentToken().Line}: Se esperaba 'Color'");
        }
        ConsumeToken();

        Expect(TokenType.Punctuation, "(");

        if (CurrentToken().Type != TokenType.StringLiteral)
        {
            throw new Exception($"Error en línea {CurrentToken().Line}: Se esperaba un color ");
        }

        string colorValue = CurrentToken().Value;
        ConsumeToken();
        Expect(TokenType.Punctuation, ")");

        return new ColorNode(colorValue);
    }
    private SizeNode ParseSize()
    {
        if(CurrentToken().Value!="Size")
        throw new Exception($"Se esperaba 'Size' en línea {CurrentToken().Line}");

        ConsumeToken();
        Expect(TokenType.Punctuation, "(");
        if (CurrentToken().Type != TokenType.Number)
        {
            throw new Exception($"Error en línea {CurrentToken().Line}: Se esperaba un número entero");
        }

        NumberNode sizeValue = ParseNumber();
        Expect(TokenType.Punctuation, ")");
        return new SizeNode(sizeValue.Number);
    }
    
    private DrawCommandNode ParseDrawCommand()
    {
        if(CurrentToken().Type!=TokenType.DrawingCommand){
            throw new Exception($"Se esperaba un comando de dibujo en línea {CurrentToken().Line}");
        }
        else
        {
            string commandName = CurrentToken().Value;
            ConsumeToken();
            Expect(TokenType.Punctuation, "(");

            List<ASTNode> parameters = new List<ASTNode>();

            while (CurrentToken().Value != ")")
            {
                parameters.Add(ParsePrimary());
                if (CurrentToken().Value == ")")
                {
                    ConsumeToken();
                    return new DrawCommandNode(commandName, parameters);
                }
                else
                    Expect(TokenType.Punctuation, ",");
            }
           Expect(TokenType.Punctuation, ")");
            return new DrawCommandNode(commandName,parameters);
        }
    } 
    private FillNode ParseFill()
    {
        if(CurrentToken().Value!="Fill")
        throw new Exception($"Se esperaba 'Fill' en línea {CurrentToken().Line}");
                ConsumeToken();

        Expect(TokenType.Punctuation, "(");
        Expect(TokenType.Punctuation, ")");
        return new FillNode();
    }
    private FunctionNode ParseFunction()
    {
        if(CurrentToken().Type!=TokenType.Function)
        throw new Exception($"Se esperaba una función en línea {CurrentToken().Line}");
        else
        {
            string functionName = CurrentToken().Value;
            ConsumeToken();
            Expect(TokenType.Punctuation, "(");

            List<ASTNode> parameters = new List<ASTNode>();

            while (CurrentToken().Value != ")")
            {
                parameters.Add(ParsePrimary());
                if (CurrentToken().Value == ")")
                {
                    ConsumeToken();
                    return new FunctionNode(functionName, parameters);
                }
                else
                    Expect(TokenType.Punctuation, ",");

            }
            Expect(TokenType.Punctuation, ")");
            return new FunctionNode(functionName, parameters);
        }
        
    } 
    

private GoToNode ParseGoTo()
{
    Expect(TokenType.Keyword, "GoTo");
    
    Expect(TokenType.Punctuation, "[");
    
    if (CurrentToken().Type != TokenType.Label)
    {
        throw new Exception($"Se esperaba una etiqueta en línea {CurrentToken().Line}");
    }
    string label = CurrentToken().Value; 
    ConsumeToken();                      
    Expect(TokenType.Punctuation, "]");
    Expect(TokenType.Punctuation, "(");
    ASTNode condition = ParseExpression();
    Expect(TokenType.Punctuation, ")");
    
    return new GoToNode(label, condition);
}

   
}


