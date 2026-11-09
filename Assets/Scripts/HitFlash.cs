using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private float flashDuration = 0.15f;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Coroutine flashCoroutine;


    /*
    when hit, grabs every single material of the player --> turns it white 
    waits briefly (timer) --> turns back to original color 
    if hit again --> timer restarts 
    */



    //remembers normal material
    private void Awake()
    {
        // gets the renderer + children
        renderers = GetComponentsInChildren<Renderer>();

        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;
        }
    }

    //applies white + schedules timer for hit
    public void Flash()
    {
        if (whiteMaterial == null)
            return;

        // Restart the flash if the player gets hit again.
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    //turns every part of player white
    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] whiteMaterials =
                new Material[originalMaterials[i].Length];

            for (int j = 0; j < whiteMaterials.Length; j++)
            {
                whiteMaterials[j] = whiteMaterial;
            }

            renderers[i].sharedMaterials = whiteMaterials;
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreMaterials();
        flashCoroutine = null;
    }

    //restores everything back to white
    private void RestoreMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterials = originalMaterials[i];
        }
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        if (renderers != null)
            RestoreMaterials();

        flashCoroutine = null;
    }
}