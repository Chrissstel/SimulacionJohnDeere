using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    public ParticleSystem rainParticleSystem;

    void Update()
    {
        // Detectar cuando se presiona la tecla R
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Si el sistema de partículas está reproduciendo, pausarlo
            if (rainParticleSystem.isPlaying)
            {
                rainParticleSystem.Stop();
            }
            // Si no está reproduciendo, iniciarlo
            else
            {
                rainParticleSystem.Play();
            }
        }
    }
}
