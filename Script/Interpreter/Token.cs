using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    Keyword,Identifier,Label,Number,StringLiteral,Punctuation,WhiteSpace,Operator
    ,Function
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