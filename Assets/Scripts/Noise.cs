using UnityEngine;
using System.Collections;
public static class Noise
{                         
    public enum NormalizationMode {Local, Global}   //a normalização local usará pontos máximos e mínimos dos chunks, enquanto a global utilizará o ponto mais alto possível.
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizationMode normalizationMode)   //método que utiliza os parâmetros para criar uma array 2D que armazenará o Perlin Noise
    {
        float[,] noiseMap = new float[mapWidth, mapHeight]; //cria uma array 2D com os parâmetros de altura e largura

        System.Random rng = new System.Random(seed);    //Cria números pseudo-aleatórios utilizando uma seed como referência. Uma seed gera sempre a mesma sequência de números.
        Vector2[] octavesOffsets = new Vector2[octaves]; //randomiza cada oitava

        float maxGlobalHeight = 0;
        float amplitude = 1; //intensidade do noise map. Quanto maior for, maior será o impacto no mapa. Por isso deve-se ficar entre 0 e 1
        float frequency; //número de ocorrências. Quanto maior for, maior será o número no mapa. Geralmente deve ser maior do que 1

        for (int i = 0; i < octaves; i++)        
        {
            float offsetX = rng.Next(-10000, 10000) - offset.x; // subtraído a um valor inserido manualmente somado a posição do chunk
            float offsetY = rng.Next(-10000, 10000) - offset.y; 
            octavesOffsets[i] = new Vector2(offsetX, offsetY);
            maxGlobalHeight += amplitude; //basicamente utiliza o mesmo mecanismo para obter a altura de um dado ponto, porém utilizando a altura máxima 1
            amplitude *= persistance;
        }

        if (scale <= 0)  //Impedir a divisão por 0 nas linhas 17 e 18
        {
            scale = 0.001f;
        }

        float maxNoiseHeight = float.MinValue; //variáveis para o maior e menor valor do noiseMap para normalização.
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;    //usado para fixar a escala do noise map no centro ao invés da ponta
        float halfHeight = mapHeight / 2f;
        
        for(int y=0; y < mapHeight; y++)    //duplo for para gerar a matriz
        {
            for(int x=0; x < mapWidth; x++)
            {
                amplitude = 1; 
                frequency = 1; 
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) { //gera as "oitavas". atuam como camadas de noise map para mais detalhamento
                    float tempX = (x- halfWidth + octavesOffsets[i].x) / scale * frequency;    //Os valores do Perlin Noise são iguais em números inteiros e a divisão por um valor atua como uma ferramenta de escala
                    float tempY = (y- halfHeight + octavesOffsets[i].y) / scale * frequency;    //O valor também é multiplicado pela frequência para influenciar no seu número de ocorrências e somado ao offset para “mover” o centro de geração.

                    float perlinValue = Mathf.PerlinNoise(tempX, tempY) * 2 - 1;    //Cada valor do Perlin Noise é gerado com cada coordenada da matriz.
                                                                                    //a multiplicação seguido de subtração aumenta o escopo dos valores para [-1;1] para maior diversidade
                    noiseHeight += perlinValue * amplitude; //Concatena todos os valores de todas as oitavas. a amplitude define o "peso" do PerlinValue no noise map

                    amplitude *= persistance; //Ordens de potência para definir a frequência e amplitude de cada "oitava" 
                    frequency *= lacunarity;

                }

                if(noiseHeight > maxNoiseHeight)    //procura pelos pontos máximos e minimos do noise map para normalização
                {
                    maxNoiseHeight = noiseHeight;
                } 
                if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight; //atribui o valor concatenado à coordenada do noise map.
            }
        }

        for (int y = 0; y < mapHeight; y++)    //normalização para o intervalo [0,1]
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizationMode == NormalizationMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxGlobalHeight * 2f/1.75f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }
                return noiseMap; //retorna o resultado
    }

}
