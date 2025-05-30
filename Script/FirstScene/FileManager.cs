using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class FileManager : MonoBehaviour
{
    public TMP_InputField content;
    public CanvasManager canvasManager;
    public TMP_Text errorText;
   public void LoadFile() {
        try {
            string path = GetFileBrowserPath(true);
            if (!string.IsNullOrEmpty(path)) {
                content.text = File.ReadAllText(path)+"";
            }
        } catch (System.Exception e) {
            Debug.LogError($"Error al cargar: {e.Message}");
            
        }
    }
    public void Save()
    {
        string path = GetFileBrowserPath(false);
        if(!string.IsNullOrEmpty(path))
        File.WriteAllText(path, content.text+" ");
    }

        public void Evaluate()
        {
            string code = content.text;
            try
            {
                // 1. Tokenizar y Parsear
                Lexer lexer = new Lexer(code);
                List<Token> tokens = lexer.Tokenize();
                foreach (Token token in tokens)
                {
                    Debug.Log("Valor: " + token.Value+", Tipo: " + token.Type);
                }
                Parser parser = new Parser(tokens);
                ProgramNode program = parser.ParseProgram(tokens);
                foreach (ASTNode instruction in program.Instructions)
                {
                    Debug.Log("Instrucción: " + instruction.GetType());
                }
                // 2. Validación Semántica
                SemanticContext context = new SemanticContext
                {
                    CanvasSize = canvasManager.canvasSize // Obtener tamaño actual del canvas
                };
                context.AddAllLabelsed(program, context.Labels);
                ValidatorRunner validator = new ValidatorRunner(context.CanvasSize);
                validator.Validate(program, context); 
           
                if (context.Errors.Count > 0)
                {
                    errorText.text = string.Join( "\n", context.Errors);
                    errorText.color = Color.red;
                    return; 
                }

            if (context.Warnings.Count > 0)
            {
                errorText.text = string.Join("\n", context.Warnings) + "\n!! Compilado con advertencias !!";
                errorText.color = Color.yellow;
            }
            else
            {
                errorText.text = "!! Compilado correctamente !!";
                errorText.color = Color.green;
                Evaluator evaluator = new Evaluator(canvasManager);
                evaluator.Evaluate(program);
            }

            }
            catch (System.Exception e)
            {
                errorText.text = $"Error: {e.Message}";
                errorText.color = Color.red;
            }
        }

    
     private string GetFileBrowserPath(bool isLoad) {
        #if UNITY_EDITOR
            if (isLoad) {
                return UnityEditor.EditorUtility.OpenFilePanel("Cargar código", "", "pw");
            } else {
                return UnityEditor.EditorUtility.SaveFilePanel("Guardar código", "", "nuevo_archivo", "pw");
            }
        #else
            Debug.LogError("Implementa un navegador de archivos para builds");
            return "";
        #endif
    }
}
    

