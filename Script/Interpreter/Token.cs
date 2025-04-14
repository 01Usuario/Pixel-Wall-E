using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    KeyWord,Identifier,Label,Number,StringLiteral,Punctuation,WhiteSpace,Operator
    ,Function,Boolean
}

public class Token 
{
   public TokenType Type;
   public string Value;

   public Token(TokenType type,string value){
        Type = type;
        Value = value;
   }
}