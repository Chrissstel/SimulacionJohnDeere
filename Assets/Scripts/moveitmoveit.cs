using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveitmoveit : MonoBehaviour
{
    public float advanceDuration1 = 2f; // Tiempo para avanzar en la primera recta
    public float turnDuration = 1f;    // Tiempo que tarda en girar
    public float advanceDuration2 = 3f; // Tiempo para avanzar después del giro
    public float speed = 5f;           // Velocidad de avance
    public float turnAngle = 90f;      // Ángulo de giro en grados

    private float timer = 0f;
    private int state = 0; // 0: avanzando, 1: girando, 2: avanzando después de giro

    void Update()
    {
        timer += Time.deltaTime;

        switch (state)
        {
            case 0: // Avanzar
                if (timer <= advanceDuration1)
                {
                    transform.Translate(Vector3.forward * speed * Time.deltaTime);
                }
                else
                {
                    timer = 0f;
                    state = 1;
                }
                break;

            case 1: // Girar
                if (timer <= turnDuration)
                {
                    float rotationStep = (turnAngle / turnDuration) * Time.deltaTime;
                    transform.Rotate(Vector3.up, rotationStep);
                }
                else
                {
                    timer = 0f;
                    state = 2;
                }
                break;

            case 2: // Avanzar después de girar
                if (timer <= advanceDuration2)
                {
                    transform.Translate(Vector3.forward * speed * Time.deltaTime);
                }
                else
                {
                    // Aquí puedes reiniciar el proceso o detener el movimiento
                }
                break;
        }
    }
}
