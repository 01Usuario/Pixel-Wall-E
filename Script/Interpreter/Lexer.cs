using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Lexer : MonoBehaviour
{
   private readonly string source;
   private int position;
   private int line;
   
   
   public Lexer(string source){
       this.source = source;
       position = 0;
       line = 1;
   }
     private readonly HashSet<string> KeywordsList = new HashSet<string>
    {
        "Spawn","Color","Size","DrawLine","DrawCircle","DrawRectangle","Fill",
        "GoTo",
    };
    private readonly HashSet<char> PunctuationList = new HashSet<char>
    {
        '(',')',',','[',']'
    };
    private readonly HashSet<char> simpleOperators = new HashSet<char> {

        '+', '-', '*' ,'/', '%','<','>','='
    };
    private readonly HashSet<string> multiCharOperators = new HashSet<string> { 
    "++", "--", "&&", "||", "==", ">=", "<=", "**", "%", "<-"
    };
    private readonly HashSet<string> FunctionsList = new HashSet<string> {
    "GetActualX", "GetActualY", "GetCanvasSize", "GetColorCount", "IsBrushColor", "IsBrushSize", "IsCanvasColor"
    };
    private readonly HashSet<string> identifiersDeclared = new HashSet<string> ();
    private readonly HashSet<string> labelDeclared = new HashSet<string> ();

    private int openParentheses = 0;
    private int openCorchetes = 0;

   
    public List<Token> Tokenize(){

    List<Token> tokens = new List<Token>();
    while(position < source.Length){

        SkipWhiteSpaces();
        if(position >= source.Length){
            break;
        }
        
        char currentChar = source[position];
        if(char.IsLetter(currentChar)){
            tokens.Add(IdentifierOrKeyword());
        }
        else if(char.IsDigit(currentChar)){
            tokens.Add(Number());
        }
        else if(currentChar == '"'){
            tokens.Add(String());
        }
        else if(IsOperator(currentChar)){
            tokens.Add(Operator());
        }
        else if(PunctuationList.Contains(currentChar)){
            tokens.Add(IsPunctuation());
        }
        
        else
        throw new System.Exception($"Token inesperado: {currentChar} en la linea {line}, posicion {position}");

        // Verificar si hay corchetes desbalanceadas.
        if (openCorchetes != 0 && position==source.Length-1)
            throw new System.Exception($"Corchetes desbalanceadas en la linea {line}, posicion {position}");
        // Verificar si hay parentesis desbalanceados.
        if (openParentheses != 0&&position==source.Length-1)
            throw new System.Exception($"Parentesis desbalanceados en la linea {line}, posicion {position}");

    }
    return tokens;

}

    private void SkipWhiteSpaces()
    {
        while (position < source.Length && char.IsWhiteSpace(source[position]))
        {
            if (source[position] == '\n')
            {
                line++;
            }
           
            position++;
        }
    }

    // Metodo que identifica si una secuencia de caracteres es un identificador o una palabra clave.
    private Token IdentifierOrKeyword()
    {
        // Guardar la posicion inicial.
        int startPos = position;
        if(source[position]=='-'){
            throw new System.Exception($"Identificador no valido: {source[position]} en la linea {line}, posicion {position}");
        }
        // Mientras no se haya alcanzado el final del codigo fuente y el caracter actual sea una letra, digito o guion
        while (position < source.Length &&( char.IsLetterOrDigit(source[position]) || source[position] == '-'))
        {
            position++;
        }
        string value = source.Substring(startPos, position - startPos);
           if(value[0]=='-') 
           throw new System.Exception($"Identificador no valido: {value} en la linea {line}, posicion {position}");


            if (KeywordsList.Contains(value))
                return new Token(TokenType.Keyword, value);
            else if (FunctionsList.Contains(value))
                return new Token(TokenType.Function, value);
        
             if(position + 2 < source.Length &&source[position+1]=='<'&& source[position+2]=='-'&&!labelDeclared.Contains(value)){
                identifiersDeclared.Add(value);
                 return new Token(TokenType.Identifier, value);
            }
            else {
                if(!identifiersDeclared.Contains(value))
                {
                    return new Token(TokenType.Label, value);
                }
                else{
                    labelDeclared.Add(value);
                    return new Token(TokenType.Identifier, value);
                }

            }
            
            //Buscar en la lista contraria para evitar confuciones de que sea declarado como una cosa y luego como otra

    
    }
    // Metodo que tokeniza un numero.
    private Token Number()
    {
        // Guardar la posicion inicial.
        int startPos = position;
        // Mientras no se haya alcanzado el final del codigo fuente y el caracter actual sea un digito.
        while (position < source.Length && char.IsDigit(source[position]))
        {
            position++;
        }
        // Obtener el valor del numero.
        string value = source.Substring(startPos, position - startPos);
        // Devolver un token de tipo numero.
        return new Token(TokenType.Number, value);
    }

    // Metodo que tokeniza una cadena de texto.
    private Token String()
    {
        // Guardar la posicion inicial.
        int startPos = position;
        // Incrementar la posicion y la columna para omitir la comilla inicial.
        position++;
        // Mientras no se haya alcanzado el final del codigo fuente y el caracter actual no sea una comilla.
        while (position < source.Length && source[position] != '"')
        {
            position++;
        }
        // Si se alcanza el final del codigo fuente sin encontrar una comilla de cierre, lanzar una excepcion.
        if (position >= source.Length)
            throw new System.Exception ($"Literal de cadena sin cerrar en la linea {line}, posicion {position}");
        // Obtener el valor de la cadena.
        string value = source.Substring(startPos + 1, position - startPos - 1);
        // Incrementar la posicion y la columna para omitir la comilla de cierre.
        position++;
        // Devolver un token de tipo cadena.
        return new Token(TokenType.StringLiteral, value);
    }

    // Metodo que tokeniza un operador.
    private Token Operator()
    {
        // Obtener un posible operador de dos caracteres.
        string twoCharOp = position + 1 < source.Length ? source.Substring(position, 2) : null;
        // Si el operador de dos caracteres es valido, devolver un token de tipo operador.
        if (twoCharOp != null && multiCharOperators.Contains(twoCharOp))
        {
            position += 2;
            return new Token(TokenType.Operator, twoCharOp);
        }

        // Obtener el caracter actual.
        char currentChar = source[position];
        // Incrementar la posicion y la columna.
        position++;
        // Devolver un token de tipo operador.
        return new Token(TokenType.Operator, currentChar.ToString());
    }

    // Metodo que verifica si un caracter es un operador.
    private bool IsOperator(char character)
    {
        // Verificar si el caracter esta en la lista de operadores.
        return simpleOperators.Contains(character);
    }

    // Metodo que tokeniza un caracter especial.
    private Token IsPunctuation()
    {
        // Obtener el caracter actual.
        char currentChar = source[position];
        // Incrementar la posicion 
        position++;
        if (currentChar == '(')
            openParentheses++;
        else if (currentChar == ')')
            openParentheses--;
        else if (currentChar == '[')
            openCorchetes++;
        else if (currentChar == ']')
            openCorchetes--;
        return new Token(TokenType.Punctuation, currentChar.ToString());
    }

   
}
