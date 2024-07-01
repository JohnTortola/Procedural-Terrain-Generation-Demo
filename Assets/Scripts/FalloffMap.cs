using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalloffMap
{

    public static float[,] GenerateFalloffMap(int size, float offsetx, float offsety ,float a, float b, float falloffClamp)
    {

        float[,] map = new float[size, size]; //tamanho do mapa de altura

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - offsetx; //modifica o intervalo de 0 à 1 para -1 à 1
                float y = j / (float)size * 2 - offsety; //os offsets "movem" o centro do falloff

                float value = Mathf.Clamp01(Mathf.Max(Mathf.Abs(x), Mathf.Abs(y))); //obtém o valor máximo absoluto e desconsidera resultados além do intervalo 0 à 1
                map[i, j] = Mathf.Clamp(Evaluate(value, a, b), 0f , falloffClamp); //falloffClamp limita o número máximo possível, diminuindo o efeito do falloff no mapa 
            }
        }
        return map;
    }

    static float Evaluate(float value, float a, float b) //"a" manipula a intensidade do falloff, "b" manipula o tamanho do falloff
    {
        value = Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        return value;
    }
}
