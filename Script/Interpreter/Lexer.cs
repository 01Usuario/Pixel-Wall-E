using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Lexer 
{
   private readonly string source;
   private int position;
   private int line;
   private SemanticContext Context;
   
   
   public Lexer(string source)
    {
        this.source = source;
        this.Context = new SemanticContext();
        position = 0;
        line = 1;
    }
     private readonly HashSet<string> KeywordsList = new HashSet<string>
    {
        "Spawn","Color","Size","GoTo","Fill"
    };
    private readonly HashSet<char> PunctuationList = new HashSet<char>
    {
        '(',')',',','[',']'
    };
    private readonly HashSet<string> ArithmeticOperators = new HashSet<string> {

        "+" ,"-", "*" ,"/", "%","**"
    };
    private readonly HashSet<string> BooleanOperators = new HashSet<string> {

        "&&", "||", "==", ">=", "<=","<",">","=","&", "|"  
    };
    private readonly HashSet<string> AssignOperators = new HashSet<string> {
        "<-"
    };

    private readonly HashSet<string> FunctionsList = new HashSet<string> {
    "GetActualX", "GetActualY", "GetCanvasSize", "GetColorCount", "IsBrushColor", "IsBrushSize", "IsCanvasColor"
    };
    private readonly HashSet<string> DrawingCommands = new HashSet<string> {
    "DrawLine", "DrawCircle", "DrawRectangle"
    };  
    private readonly HashSet<string> identifiersDeclared = new HashSet<string>();
    private readonly HashSet<string> labelDeclared = new HashSet<string> ();

    private int openParentheses = 0;
    private int openCorchetes = 0;

   
    public List<Token> Tokenize()
    {

        List<Token> tokens = new List<Token>();
        while(position < source.Length){
            SkipWhiteSpaces();
            if(position >= source.Length){
                break;
            }
            
            char currentChar = source[position];
                if (source[position] == '-')
                {
                    tokens.Add(ProcessMinus());
                    continue;
                }
            if(char.IsLetter(currentChar)){
                tokens.Add(ClassifyToken());
            }
            else if(char.IsDigit(currentChar))
            {
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
            
        }
        
        if (openCorchetes != 0)
                Context.AddError($"Corchetes desbalanceados");
            // Verificar si hay parentesis desbalanceados.
            if (openParentheses != 0)
                Context.AddError($"Parentesis desbalanceados");

        return tokens;

}

    private void SkipWhiteSpaces()
    {
        while (position < source.Length && char.IsWhiteSpace(source[position]))
        {
            if (source[position] == '\n')
            {   
                line++;
                if(source[position+1]=='-'){
                    throw new System.Exception($"Identificador no valido: {source[position]} en la linea {line}, posicion {position}");
                }
                if (char.IsDigit(source[position+1])) {
                    throw new System.Exception($"Identificador no puede comenzar con número (línea {line})");
                }
            }
            position++;
        }
    }

    // Metodo que identifica si una secuencia de caracteres es un identificador o una palabra clave.
    private Token ClassifyToken() 
    {
        // Guardar la posicion inicial.
        int startPos = position;
        if(source[position]=='-'){
            throw new System.Exception($"Identificador no valido: {source[position]} en la linea {line}, posicion {position}");
        }
        if (char.IsDigit(source[startPos])) {
        throw new System.Exception($"Identificador no puede comenzar con número (línea {line})");
    }
        // Mientras no se haya alcanzado el final del codigo fuente y el caracter actual sea una letra, digito o guion
        while (position < source.Length &&( char.IsLetterOrDigit(source[position]) || source[position] == '_'))
        {
            position++;
        }
        string value = source.Substring(startPos, position - startPos);
           if(value[0]=='-') 
           throw new System.Exception($"Identificador no valido: {value} en la linea {line}, posicion {position}");


            if (KeywordsList.Contains(value))
                return new Token(TokenType.Keyword, value,line);
            else if (FunctionsList.Contains(value))
                return new Token(TokenType.Function, value,line);
        
             if(position + 2 < source.Length &&source[position+1]=='<'&& source[position+2]=='-'&&!labelDeclared.Contains(value)){
                identifiersDeclared.Add(value);
                 return new Token(TokenType.Identifier, value,line);
            }
            else if(DrawingCommands.Contains(value)){
                return new Token(TokenType.DrawingCommand, value,line);
            }
            else {
                if(!identifiersDeclared.Contains(value))
                {
                    return new Token(TokenType.Label, value,line);
                }
                else{
                    labelDeclared.Add(value);
                    return new Token(TokenType.Identifier, value,line);
                }

            }
            
            //Buscar en la lista contraria para evitar confuciones de que sea declarado como una cosa y luego como otra

    
    }
    private Token ProcessMinus()
    {
        if (IsNegativeNumber())
        {
            return Number();
        }
        else {
            position++;
            return new Token(TokenType.ArithmeticOperator, "-", line);
        }

    }
    private bool IsNegativeNumber() {
        // Si no hay un dígito después del '-', no es un número negativo
        if (position + 1 >= source.Length || !char.IsDigit(source[position + 1])) {
            return false;
        }
        
         if (position == 0)
        {
            return true;
        }
        // Caso 2: Si el '-' está precedido por un número, variable o ')', es operador
        char prevChar = source[position - 1];
        if (char.IsDigit(prevChar) || char.IsLetter(prevChar) || prevChar == ')')
        {
            return false;
        }
        return true;
    }

    private Token Number()
    {
        int startPos = position;
        if (position < source.Length && source[position] == '-')
        {
            position++;
        }
        while (position < source.Length && char.IsDigit(source[position]))
        {
            position++;
        }
        string value = source.Substring(startPos, position - startPos);
        return new Token(TokenType.Number, value, line);
    }

    // Metodo que tokeniza una cadena de texto.
    private Token String()
    {
        // Guardar la posicion inicial.
        int startPos = position;
        position++;
        while (position < source.Length && source[position] != '"')
        {
            position++;
        }
        if (position >= source.Length)
            throw new System.Exception ($"Literal de cadena sin cerrar en la linea {line}, posicion {position}");
        string value = source.Substring(startPos + 1, position - startPos - 1);
        position++;
        return new Token(TokenType.StringLiteral, value,line);
    }

    // Metodo que tokeniza un operador.
    private Token Operator()
    {
        string twoCharOp = position + 1 < source.Length ? source.Substring(position, 2) : null;
        if (twoCharOp != null)
        {
            if(AssignOperators.Contains(twoCharOp))
            {
                position += 2;
                return new Token(TokenType.AssignmentOperator, twoCharOp,line);
            }
            else if(ArithmeticOperators.Contains(twoCharOp))
            {
                position += 2;
                return new Token(TokenType.ArithmeticOperator, twoCharOp,line);
            }
            else if(BooleanOperators.Contains(twoCharOp))
            {
                position += 2;
                return new Token(TokenType.BooleanOperator, twoCharOp,line);
            }
            else
            {
                char simpleOperator = source[position];
                if(ArithmeticOperators.Contains(simpleOperator.ToString())){
                    position += 1;
                    return new Token(TokenType.ArithmeticOperator, simpleOperator.ToString(),line);
                }
                else if(BooleanOperators.Contains(simpleOperator.ToString())){
                    position += 1;
                    return new Token(TokenType.BooleanOperator, simpleOperator.ToString(),line);
                }

            }
        }
        throw new System.Exception($"Operador no reconocido en la linea {line}, posicion {position}, encontrado {source[position]}");
        
    }

    // Metodo que verifica si un caracter es un operador.
    private bool IsOperator(char character)
    {
        // Verificar si el caracter esta en la lista de operadores.
        return ArithmeticOperators.Contains(character.ToString())|| BooleanOperators.Contains(character.ToString());
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
        return new Token(TokenType.Punctuation, currentChar.ToString(),line);
    }

   
}
