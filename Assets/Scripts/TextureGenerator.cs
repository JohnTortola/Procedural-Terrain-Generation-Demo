using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureApply(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);   //cria uma textura com os dados de altura e largura do noise map
        texture.filterMode = FilterMode.Point;  //renderiza pixels mais nítidos
        texture.wrapMode = TextureWrapMode.Clamp;  //impede que a textura se repita 
        texture.SetPixels(colourMap); //efetua e aplica as alterações
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);  //Armazena a altura e largura da matriz que contém o noise map
        int height = noiseMap.GetLength(1);

        Color[] colourMap = new Color[width * height]; //array de cores

        for (int y = 0; y < height; y++) //matriz para repassar os valores do noise map
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]); //Converte a tabela em uma lista por se tratar de um array e interpola as cores
                                                                                                 //preto e branco para os valores 0 e 1 do noise map
            }
        }
        return TextureApply(colourMap, width, height);
    }
   
    public static Color[] TextureFromColourMap(float[,] noiseMap, TerrainType[] regions, float[,] falloffMap , bool falloffOn)
    {
        int width = noiseMap.GetLength(0);  //Armazena a altura e largura da matriz que contém o noise map
        int height = noiseMap.GetLength(1);

        Color[] colourMap = new Color[width * height]; //array de cores

        for (int y = 0; y < height; y++)   //percorre a matriz
        {
            for (int x = 0; x < width; x++)
            {
                if (falloffOn)
                {
                    noiseMap[x, y] = Mathf.Clamp(noiseMap[x, y] - falloffMap[x,y], 0, int.MaxValue);
                }
                float currentHeight = noiseMap[x, y];   //armazena o valor atual do noise map
                for (int i = 0; i < regions.Length; i++) //percorre as regiões de cores do struct TerrainType
                {
                    if (currentHeight >= regions[i].height)  //compara o valor atual do noise map com a altura do struct das regiões (se é maior ou igual)
                    {
                        colourMap[y * width + x] = regions[i].color;
                    }
                    else //irá encontrar a cor definitiva uma vez que a região sendo checada for mais alta que o ponto do mapa de altura atual
                    {
                        break; //break utilizado já que não é mais necessário efetuar a comparação com as outras regiões
                    }
                }
            }
        }

        return colourMap;
    }
}
