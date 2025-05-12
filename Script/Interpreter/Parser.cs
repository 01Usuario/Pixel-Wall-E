using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;

public class Parser : MonoBehaviour
{
    private List<Token> tokens;
    private int currentTokenIndex;
    private int line;
    public void Parse(List<Token> tokens)
    {
        this.tokens = tokens;
        this.currentTokenIndex = 0;
        this.line=1;

    }
     public ASTNode ParseProgram()
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
        switch (token.Value)
        {
            case "Spawn":
                return ParseSpawn();
            case "Color":
                return ParseColor();
            case "Size":
                return ParseSize();                   
        }
    return null;
    }
    
    private Token CurrentToken(){
        return tokens[currentTokenIndex];
    }

    private void Expect(TokenType type, string value) 
    {
        if (CurrentToken().Type != type || CurrentToken().Value != value) {
            throw new Exception($"Se esperaba {value} en línea {CurrentToken().Line}");
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
        if(CurrentToken().Type!=TokenType.Identifier )
        throw new Exception($"Se esperaba una variable en línea {CurrentToken().Line}");
        else{
            string variableName = CurrentToken().Value;
            ConsumeToken();
            return new VariableNode(variableName);
        }
    }    
    private ASTNode ParseExpression()
    {
        if (CurrentToken().Type == TokenType.Identifier)
        {
            return ParseVariable();
        }
        else if(CurrentToken().Type == TokenType.Number)
        {
            return ParseNumber();
        }
        else if(CurrentToken().Type == TokenType.Label)
        {
            return ParseLabel();
        }
        else if(CurrentToken().Value=="("){
            ConsumeToken();
            ASTNode expression = ParseExpression();
            Expect(TokenType.Punctuation, ")");
            return expression;
        }
        else
        {
            throw new Exception($"Se esperaba una expresión en línea {CurrentToken().Line}");
        }
        
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

        var validColors = new HashSet<string> { 
            "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Black", "White", "Transparent" 
        };

        if (!validColors.Contains(colorValue))
        {
            throw new Exception($"Error en línea {CurrentToken().Line}: Color inválido '{colorValue}'");
        }

        Expect(TokenType.Punctuation, ")");

        return new ColorNode(colorValue);
    }
    private SizeNode ParseSize()
    {
        if(CurrentToken().Value!="Size")
        throw new Exception($"Se esperaba 'Size' en línea {CurrentToken().Line}");

        ConsumeToken();//COnsumir Size
        Expect(TokenType.Punctuation, "(");
        if (CurrentToken().Type != TokenType.Number)
        {
            throw new Exception($"Error en línea {CurrentToken().Line}: Se esperaba un número entero");
        }

        NumberNode sizeValue = ParseNumber();
        ConsumeToken();
        Expect(TokenType.Punctuation, ")");
        return new SizeNode(sizeValue.Number);
    }
    private LabelNode ParseLabel()
    {
        var lebelName = CurrentToken().Value;
        ConsumeToken();
        return new LabelNode(lebelName);
    } 
    private DrawCommandNode ParseDrawCommand()
    {
        if(CurrentToken().Type!=TokenType.DrawingCommand){
            throw new Exception($"Se esperaba un comando de dibujo en línea {CurrentToken().Line}");
        }
        else
        {
            string commandName = CurrentToken().Value;
            Expect(TokenType.Punctuation, "(");

            List<ASTNode> parameters = new List<ASTNode>();

            while(CurrentToken().Value!=")"){
                parameters.Add(ParseExpression());
                if (CurrentToken().Value != ")")
                {
                    Expect(TokenType.Punctuation, ",");
                }
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

            while(CurrentToken().Value!=")"){
                parameters.Add(ParseExpression());
                if (CurrentToken().Value != ")")
                {
                    Expect(TokenType.Punctuation, ",");
                }
            }
           Expect(TokenType.Punctuation, ")");
            return new FunctionNode(functionName,parameters);
        }
    }
   
}


