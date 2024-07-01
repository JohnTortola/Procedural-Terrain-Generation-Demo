using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using UnityEngine.UI;

public class MapGeneration : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColourMap, Mesh, Falloff};   //possibilita a escolha do tipo de mapa a ser renderizado
    public DrawMode drawMode;

    public Noise.NormalizationMode normalizationMode;

    public const int mapChunkSize = 241;   //Declaração das variáveis de tamanho, nível de detalhe e escala do plano que exibirá o Perlin Noise
    [Range(0, 6)]
    public int lodEditorAux; //nivel de detalhe do editor. quanto maior o nível de detalhe, menos subdivisões haverão na renderização do terreno
    public float noiseScale;

    public int octaves; //representa o número de ruidos a serem utilizados
    [Range(0,1)]
    public float persistance;   //representa a opacidade, ou seja, o quanto os menores detalhes afetam no formato original
    public float lacunarity;    //aumenta o nível de detalhamento do mapa

    public int seed;    //um número inicial para a randomização dos offsets. sua vantagem é a capacidade de revisitar gerações anteriores
    public Vector2 offsets; //essêncial para obter resultados diferentes. é utilizado como uma distância do ponto inicial do ruido

    public float heightMultiplier;  //multiplica a altura dos pontos do mapa 3D, visto que os valores iniciais variam apenas de 0 a 1.
    public AnimationCurve meshHeightCurve;  //uma curva usada para trazer valores que estão fora dela para dentro. usado por exemplo para planificar um oceano.

    [Range(0,10)]
    public float falloffIntensity, falloffSize;
    [Range(0.5f,1.5f)]
    public float centerCurveOffx, centerCurveOffy;
    [Range(0f, 1f)]
    public float falloffClamp;
    public bool falloffApply;
    public bool falloffRandomizer;

    public TerrainTypes[] regions;

    public bool autoUpdate; //Bool para ativar ou desativar a atualização em tempo real com a alteração de dados no inspector

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>(); //filas
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawInEditor()
    {
        MapData mapData = GenerateMap(Vector2.zero, centerCurveOffx, centerCurveOffy, falloffIntensity, falloffSize, 0); //obtém o mapa de cores e altura do GenerateMap()
        MapDisplay display = GetComponent<MapDisplay>(); //Acessa o script e executa o método para gerar a textura do Perlin Noise a partir da array enviada como parâmetro
        if (drawMode == DrawMode.NoiseMap)  //confecciona o mapa de altura preto e branco para visualização do ruído
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)   //confecciona o mapa de cores com as cores de cada região em suas camadas
        {
            display.DrawTexture(TextureGenerator.TextureApply(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)    //confecciona a mesh (mapa 3D)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, meshHeightCurve, lodEditorAux), TextureGenerator.TextureApply(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Falloff)  //confecciona o mapa falloff
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.falloffMap));
        }
    }
   
    public void RequestMapData(Action<MapData> callback, Vector2 chunkPos, float falloffOffsetX, float falloffOffsetY, float a, float b, int regionChoice)    //inicializa thread
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback, chunkPos, falloffOffsetX, falloffOffsetY, a, b, regionChoice);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback, Vector2 chunkPos, float falloffOffsetX, float falloffOffsetY, float a, float b, int regionChoice) //thread MapData
    {
        MapData mapData = GenerateMap(chunkPos, falloffOffsetX, falloffOffsetY, a, b, regionChoice); //gera dados do chunk
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData)); //insere na fila
        }
    }

    public void RequestMeshData(Action<MeshData> callback, MapData mapData, int lod) //inicializa a thread
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(callback, mapData, lod);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(Action<MeshData> callback, MapData mapData, int lod) //thread MeshData
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, meshHeightCurve, lod);    //gera os dados da mesh do chunk
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData)); //insere na fila
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();   //anda a fila 
                threadInfo.callback(threadInfo.parameter);  //execução
            }
        }

        if(meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue(); //anda a fila 
                threadInfo.callback(threadInfo.parameter);  //execução
            }
        }

    }

    public MapData GenerateMap(Vector2 chunkPos, float offsetFalloffX, float offsetFalloffY, float a, float b, int regionChoice)   //Esse método envia os parâmetros para geração de um Perlin Noise e gera uma matriz com o perlin noise com os parâmetros enviados no script Noise.cs
    {
 
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, chunkPos + offsets, normalizationMode);    
        float[,] falloffChunkMap = FalloffMap.GenerateFalloffMap(mapChunkSize, offsetFalloffX, offsetFalloffY, a, b, falloffClamp);
        Color[] colourMap = TextureGenerator.TextureFromColourMap(noiseMap, regions[regionChoice].terrainType, falloffChunkMap, falloffApply);
        return new MapData(noiseMap, falloffChunkMap, colourMap);
    }

    private void OnValidate()   //tratamento de exceções
    {
        if(lacunarity < 1)
        {
            lacunarity = 1;
        }
        if(octaves < 0)
        {
            octaves = 0;
        }
        if(noiseScale < 1)
        {
            noiseScale = 1;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter; //tipo genérico

        public MapThreadInfo(Action<T> callback, T parameter) //construtor
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType   //struct para criar as camadas de cores que representam um tipo de terreno
{
    public string name;
    public float height;
    public Color color;
}

[System.Serializable]
public struct TerrainTypes
{
    public string name;
    public TerrainType[] terrainType;
}

public struct MapData
{
    public readonly float[,] noiseMap;
    public readonly float[,] falloffMap;
    public readonly Color[] colourMap;


    public MapData(float[,] heightMap, float[,] falloffMap, Color[] colourMap)   //construtor
    {
        this.noiseMap = heightMap;
        this.falloffMap = falloffMap;
        this.colourMap = colourMap;


    }
}