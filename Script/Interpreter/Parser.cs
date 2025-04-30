using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parser : MonoBehaviour
{
    private List<Token> tokens;
    private int currentTokenIndex;

    public void Parse(List<Token> tokens)
    {
        this.tokens = tokens;
        this.currentTokenIndex = 0;
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
        Token currentToken = tokens[currentTokenIndex];
        
        return null;
    }

}
