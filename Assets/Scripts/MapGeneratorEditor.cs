using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (MapGeneration))] 
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()   //costumizar o inspector
    {
        MapGeneration mapGen = (MapGeneration)target; //o objeto a ser inspecionado que contém o script

        if (DrawDefaultInspector()){ //se ocorre qualquer mudança no inspector e o autoupdate for verdadeiro, a textura atualizará conforme mudanças forem feitas
            if (mapGen.autoUpdate)
            {
                mapGen.DrawInEditor();
            }
        }

        if(GUILayout.Button ("Generate")) //ao clicar no botão "generate", atualizará a textura.
        {
            mapGen.DrawInEditor();
        }
    }
}
