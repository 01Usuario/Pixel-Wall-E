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

        public void Execute()
        {
            string code = content.text;
            try
            {
                // 1. Tokenizar y Parsear
                Lexer lexer = new Lexer(code);
                List<Token> tokens = lexer.Tokenize();

                Parser parser = new Parser(tokens);
                ProgramNode program = parser.ParseProgram(tokens);

                // 2. Validación Semántica
                SemanticContext context = new SemanticContext
                {
                    CanvasSize = canvasManager.canvasSize // Obtener tamaño actual del canvas
                };

                ValidatorRunner validator = new ValidatorRunner(context.CanvasSize);
                validator.Validate(program, context); 

                // 3. Manejar errores
                if (context.Errors.Count > 0)
                {
                    errorText.text = string.Join("\n", context.Errors);
                    errorText.color = Color.red;
                    return; // Detener ejecución si hay errores
                }

                // 4. Ejecutar instrucciones (solo si no hay errores)
                //ExecuteProgram(program, context);
                errorText.text = "¡Ejecución exitosa!";
                errorText.color = Color.green;
            }
            catch (System.Exception e)
            {
                errorText.text = $"Error: {e.Message}";
                errorText.color = Color.red;
            }
        }

       /*  private void ExecuteProgram(ASTNode program, SemanticContext context)
        {
            // Reiniciar el canvas si es necesario
            canvasManager.ClearCanvas();

            // Ejecutar cada instrucción usando el estado del pincel
            foreach (var instruction in ((ProgramNode)program).Instructions)
            {
                if (instruction is SpawnNode spawn)
                {
                    canvasManager.SetPixel(spawn.X, spawn.Y, Color.black); // Ejemplo
                }
                else if (instruction is DrawCommandNode drawCmd)
                {
                    // Lógica para dibujar líneas/círculos
                }
            }
        } */
    
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
    

