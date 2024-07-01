using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProceduralGenerator : MonoBehaviour
{
    static MapGeneration mapGeneration;
    const float scale = 5f;

    public Transform viewer;
    public static Vector2 viewerPosition;
    Vector2 viewerPostisionPrevious;
    const float viewerMoveUpdate = 25f * scale;
    const float sqrViewerMoveUpdate = viewerMoveUpdate * viewerMoveUpdate;

    public static float maxViewDist;
    public LODData[] detailLayers;  //array de structs com as distâncias e LODs

    Material chunkMaterial;
    int chunkSize;
    int chunksVisibleInViewDist;

    public Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>(); //armazena os chunks e suas coordenadas
    static List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();    //lista de chunks

    RngData rngData;
    System.Random rng;

    void Start()
    {
        mapGeneration = FindObjectOfType<MapGeneration>();
        rng = new System.Random(mapGeneration.seed);

        maxViewDist = detailLayers[detailLayers.Length - 1].dstLodLayer;
        chunkSize = MapGeneration.mapChunkSize - 1; //originalmente era 241
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);    //número de chunks visíveis
        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z)/scale;
        if ((viewerPostisionPrevious - viewerPosition).sqrMagnitude > sqrViewerMoveUpdate)
        {
            viewerPostisionPrevious = viewerPosition;
            UpdateVisibleChunks();            
        }

    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++)
        {
            terrainChunkVisibleLastUpdate[i].Visibility(false);
        }
        terrainChunkVisibleLastUpdate.Clear();  //remove os elementos da lista

        int currentChunkX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkX + xOffset, currentChunkY + yOffset);
                
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLayers, transform, chunkMaterial, RngDataGenerator(rngData)));
                }

            }
        }

    }

    RngData RngDataGenerator(RngData rngData)
    {
 
        int regionChoice = rng.Next(0, mapGeneration.regions.Length);

        float a = mapGeneration.falloffIntensity; //2.4 era o padrão
        float b = mapGeneration.falloffSize; //1.8 era o padrão
        
        float offsetFalloffX = mapGeneration.centerCurveOffx; //1 era o padrão para ambos
        float offsetFalloffY = mapGeneration.centerCurveOffy;

        if (mapGeneration.falloffRandomizer)
        {
            a = rng.Next(0, 100) * 0.05f;
            b = rng.Next(0, 100) * 0.05f;

            offsetFalloffX = rng.Next(90, 110) * 0.01f;
            offsetFalloffY = rng.Next(90, 110) * 0.01f;

            if (b > 4)
            {
                offsetFalloffX = rng.Next(90, 110) * 0.01f;
                offsetFalloffY = rng.Next(90, 110) * 0.01f;
            }
            else if (b > 3)
            {
                offsetFalloffX = rng.Next(80, 119) * 0.01f;
                offsetFalloffY = rng.Next(80, 119) * 0.01f;
            }
            else if (b > 2)
            {
                offsetFalloffX = rng.Next(75, 124) * 0.01f;
                offsetFalloffY = rng.Next(75, 124) * 0.01f;
            }
            else if (b >= 1)
            {
                offsetFalloffX = rng.Next(67, 132) * 0.01f;
                offsetFalloffY = rng.Next(67, 132) * 0.01f;
            }

            if (b < 1)
            {
                b = 0;
            }
            Debug.Log("regionchoice: " + regionChoice + " intensity: " + a + " size: " + b + " X: " + offsetFalloffX + " Y: " + offsetFalloffY);
        }

        rngData.regionChoice = regionChoice;
        rngData.a = a;
        rngData.b = b;
        rngData.offsetFalloffX = offsetFalloffX;
        rngData.offsetFalloffY = offsetFalloffY;
        return rngData;
    }

    public struct RngData
    {
    public int regionChoice;
    public float a;
    public float b;
    
    public float offsetFalloffX;
    public float offsetFalloffY;
    }
 
  
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODData[] detailLayers; //array de LODData a ser recebido
        LODMesh[] lodMeshes;    //array de LODMesh a receber cada LOD de detailLayers e UpdateTerrainChunk()

        MapData mapData;
        bool mapDataReceived;
        int previousLOD = -1;

        public RngData rngData = new RngData();

        public TerrainChunk(Vector2 coord, int size, LODData[] detailLayers, Transform parent, Material material, RngData rng)  //construtor
        {
            this.detailLayers = detailLayers;
            position = coord * size; //obtém a posição real
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y); //vale lembrar que deve-se usar o z e não o y do vetor.

            //meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            //meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject = new GameObject("Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            Visibility(false); //garante que apenas as funções de visibilidade decidem a visibilidade

            lodMeshes = new LODMesh[detailLayers.Length];   //instanciação de LODMesh para cada lod
            for(int i = 0; i < detailLayers.Length; i++){
                lodMeshes[i] = new LODMesh(detailLayers[i].lod, UpdateTerrainChunk);
            }

            rngData = rng;
            mapGeneration.RequestMapData(OnMapDataReceived, position, rngData.offsetFalloffX, rngData.offsetFalloffY, rngData.a, rngData.b, rngData.regionChoice); //requisita o MapData na posição em que o chunk se encontra
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureApply(mapData.colourMap, MapGeneration.mapChunkSize, MapGeneration.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }


        public void UpdateTerrainChunk()    //verifica se deve ou não ser visível
        {
            if (mapDataReceived) {
                float viewerDistFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition)); //precisa da distância normal e não a elevada ao quadrado
                bool visible = viewerDistFromEdge <= maxViewDist;

                if (visible)
                {
                    int lodIndex = 0;

                    for(int i = 0; i < detailLayers.Length - 1; i++) //tamanho -1 pois além disso a distância já ultrapassa a máxima
                    {
                        if(viewerDistFromEdge > detailLayers[i].dstLodLayer)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if(lodIndex != previousLOD)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLOD = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if(!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    terrainChunkVisibleLastUpdate.Add(this); //agora todas as chamadas a esse método incluem o chunk à lista para descarregar chunks não visíveis
                }

                Visibility(visible); 
            }
        }

        public void Visibility(bool visible)    //controla a visibilidade
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() //retorna um bool sobre a visibilidade
        {
            return meshObject.activeSelf;
        }
    }
    class LODMesh
    {
        public Mesh mesh;   //mesh que será criada, booleanos auxiliares e lod
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;   //método UpdateTerrainChunk
        public LODMesh(int lod, System.Action updateCallback) //construtor
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData)    //inicia thread
        {
            hasRequestedMesh = true;
            mapGeneration.RequestMeshData(OnMeshDataReceived, mapData, lod);
        }

        void OnMeshDataReceived(MeshData meshData)  //mesh recebida e criada
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();   //executa o callback (UpdateTerrainChunk)
        }
    }

    [System.Serializable]
    public struct LODData   //dados das distâncias no editor
    {
        public int lod;
        public float dstLodLayer;
    }
}
