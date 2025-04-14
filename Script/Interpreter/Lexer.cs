using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        "GoTo","true","false"
    };
    private readonly HashSet<char> SpecialCharactersList = new HashSet<char>
    {
        '(',')',',',':',';','[',']','.'
    };
    private readonly HashSet<string> multiCharOperators = new HashSet<string> { 
    "++", "--", "&&", "||", "==", "!=", ">=", "<=", "**", "%", "<-"
    };
    private readonly HashSet<string> FunctionsList = new HashSet<string> {
    "GetActualX", "GetActualY", "GetCanvasSize", "GetColorCount", "IsBrushColor", "IsBrushSize", "IsCanvasColor"
    };
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
        else if(SpecialCharactersList.Contains(currentChar)){
            tokens.Add(IsPunctuation());
        }
        
        else
        throw new System.Exception($"Token inesperado: {currentChar} en la l�nea {line}, posicion {position}");

        // Verificar si hay corchetes desbalanceadas.
        if (openCorchetes != 0 && position==source.Length-1)
            throw new System.Exception($"Llaves desbalanceadas en la l�nea {line}, posicion {position}");
        // Verificar si hay par�ntesis desbalanceados.
        if (openParentheses != 0&&position==source.Length-1)
            throw new System.Exception($"Par�ntesis desbalanceados en la l�nea {line}, posicion {position}");

    }
    return tokens;

}

    private void SkipWhiteSpaces()
    {
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual sea un espacio en blanco.
        while (position < source.Length && char.IsWhiteSpace(source[position]))
        {
            // Si el car�cter actual es un salto de l�nea, incrementar la l�nea y reiniciar la columna.
            if (source[position] == '\n')
            {
                line++;
            }
           
            // Incrementar la posici�n.
            position++;
        }
    }

    // M�todo que identifica si una secuencia de caracteres es un identificador o una palabra clave.
    private Token IdentifierOrKeyword()
    {
        // Guardar la posici�n inicial.
        int startPos = position;
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual sea una letra, d�gito o guion bajo.
        while (position < source.Length && char.IsLetterOrDigit(source[position]) )
        {
            position++;
        }
        // Obtener el valor del identificador o palabra clave.
        string value = source.Substring(startPos, position - startPos);

        // Si el siguiente car�cter es un punto, tokenizarlo por separado.
        if (position < source.Length && source[position] == '.')
        {
            return new Token(TokenType.Identifier, value);
        }

       if (KeywordsList.Contains(value))
        return new Token(TokenType.KeyWord, value);
    else if (FunctionsList.Contains(value))
        return new Token(TokenType.Function, value);
    else if (value == "true" || value == "false")
        return new Token(TokenType.Boolean, value);
        //Esto tengo que hacer para que mejorarlo
        //TO-DO: Mejorar esto
    else if(source[position+1] == '\n')
            return new Token(TokenType.Label, value);
    else
        return new Token(TokenType.Identifier, value);
    }

    // M�todo que tokeniza un n�mero.
    private Token Number()
    {
        // Guardar la posici�n inicial.
        int startPos = position;
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual sea un d�gito.
        while (position < source.Length && char.IsDigit(source[position]))
        {
            position++;
        }
        // Obtener el valor del n�mero.
        string value = source.Substring(startPos, position - startPos);
        // Devolver un token de tipo n�mero.
        return new Token(TokenType.Number, value);
    }

    // M�todo que tokeniza una cadena de texto.
    private Token String()
    {
        // Guardar la posici�n inicial.
        int startPos = position;
        // Incrementar la posici�n y la columna para omitir la comilla inicial.
        position++;
        // Mientras no se haya alcanzado el final del c�digo fuente y el car�cter actual no sea una comilla.
        while (position < source.Length && source[position] != '"')
        {
            position++;
        }
        // Si se alcanz� el final del c�digo fuente sin encontrar una comilla de cierre, lanzar una excepci�n.
        if (position >= source.Length)
            throw new System.Exception ($"Literal de cadena sin cerrar en la l�nea {line}, posicion {position}");
        // Obtener el valor de la cadena.
        string value = source.Substring(startPos + 1, position - startPos - 1);
        // Incrementar la posici�n y la columna para omitir la comilla de cierre.
        position++;
        // Devolver un token de tipo cadena.
        return new Token(TokenType.StringLiteral, value);
    }

    // M�todo que tokeniza un operador.
    private Token Operator()
    {
        // Obtener un posible operador de dos caracteres.
        string twoCharOp = position + 1 < source.Length ? source.Substring(position, 2) : null;
        // Si el operador de dos caracteres es v�lido, devolver un token de tipo operador.
        if (twoCharOp != null && multiCharOperators.Contains(twoCharOp))
        {
            position += 2;
            return new Token(TokenType.Operator, twoCharOp);
        }

        // Obtener el car�cter actual.
        char currentChar = source[position];
        // Incrementar la posici�n y la columna.
        position++;
        // Devolver un token de tipo operador.
        return new Token(TokenType.Operator, currentChar.ToString());
    }

    // M�todo que verifica si un car�cter es un operador.
    private bool IsOperator(char character)
    {
        // Verificar si el car�cter est� en la lista de operadores.
        return "+-*/^<>=!&|@".Contains(character);
    }

    // M�todo que tokeniza un car�cter especial.
    private Token IsPunctuation()
    {
        // Obtener el car�cter actual.
        char currentChar = source[position];
        // Incrementar la posici�n y la columna.
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
