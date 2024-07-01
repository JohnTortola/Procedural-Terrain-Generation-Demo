using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender; //usado para acessar e alterar a textura e tamanho do plano
    public MeshFilter meshFilter;   //acessa o mesh que será utilizada 
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture; //atualiza a textura no editor
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height); //atualiza o tamanho do plano para o tamanho da textura
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)  
    {
        meshFilter.sharedMesh = meshData.CreateMesh(); //aplica o mesh no editor
        meshRenderer.sharedMaterial.mainTexture = texture; //aplica a textura a textura
    }
}
