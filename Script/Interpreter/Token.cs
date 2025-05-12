using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    Keyword,Identifier,Label,Number,StringLiteral,Punctuation,WhiteSpace,ArithmeticOperator,
    BooleanOperator,AssignnmentOperator,Function,DrawingCommand,
}

public class Token 
{
   public TokenType Type;
   public string Value;

   public int Line;

   public Token(TokenType type,string value,int line){
        Type = type;
        Value = value;
        Line=line;
        
   }
}