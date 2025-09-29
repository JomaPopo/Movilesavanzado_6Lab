using UnityEngine;

public class PlayerSkin : MonoBehaviour
{
    
    public GameObject[] heads;
    public GameObject[] torsos;
    public GameObject[] eyes;
    public GameObject[] legs;
    
    /// <param name="skinIndex">El índice del conjunto de skin a mostrar.</param>
    public void ChangeSkin(int skinIndex)
    {
        // Cambia la cabeza
        ChangePart(heads, skinIndex);

        // Cambia el torso
        ChangePart(torsos, skinIndex);

        // Cambia los brazos
        ChangePart(eyes, skinIndex);

        // Cambia las piernas
        ChangePart(legs, skinIndex);
    }

    /// <param name="partsArray">El array de partes (ej: heads).</param>
    /// <param name="indexToShow">El índice que se debe activar.</param>
    private void ChangePart(GameObject[] partsArray, int indexToShow)
    {
        if (partsArray == null || partsArray.Length == 0)
        {
            return;
        }

        // Asegurarse de que el índice es válido
        if (indexToShow < 0 || indexToShow >= partsArray.Length)
        {
            Debug.LogError($"Índice de skin '{indexToShow}' fuera de rango para este array de partes.");
            return;
        }

        for (int i = 0; i < partsArray.Length; i++)
        {
            if (partsArray[i] != null)
            {
                partsArray[i].SetActive(i == indexToShow);
            }
        }
    }
}