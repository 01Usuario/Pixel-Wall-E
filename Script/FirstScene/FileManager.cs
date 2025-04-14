using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class FileManager : MonoBehaviour
{
    public TMP_InputField content;
   public void LoadFile() {
        try {
            string path = GetFileBrowserPath(true);
            if (!string.IsNullOrEmpty(path)) {
                content.text = File.ReadAllText(path);
            }
        } catch (System.Exception e) {
            Debug.LogError($"Error al cargar: {e.Message}");
            
        }
    }
    public void Save()
    {
        string path = GetFileBrowserPath(false);
        if(!string.IsNullOrEmpty(path))
        File.WriteAllText(path, content.text);
    }

    public void Execute(){
        DataManager.Instance.codeText = content.text;

        string code = content.text;

        try {
            // 1. Tokenizar el código
            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();
            foreach (Token token in tokens) {
            Debug.Log("Token: "+token.Value.ToString()+ "  |  Tipo: "+token.Type.ToString());
        }
            
        } catch (System.Exception e) {
            Debug.LogError($"Error: {e.Message}");
            // Mostrar error en la UI (ej: panel de errores)
        }
        

        SceneManager.LoadScene(1);
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
    

