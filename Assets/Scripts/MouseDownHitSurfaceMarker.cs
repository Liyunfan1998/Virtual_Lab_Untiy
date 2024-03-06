using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDownHitSurfaceMarker : MonoBehaviour
{
    public float lightIntensity = 2f; // Intensity of the light sphere
    public Color lightColor = Color.yellow; // Color of the light sphere
    private GameObject lightSphere;

    RaycastHit hit;
    Ray ray;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                ShowLightSphere(hit.point);
                //Debug.Log(hit.point);
            }
        }
        else
        {
            Destroy(lightSphere); // Destroy the light sphere after the specified duration
            lightSphere = null;
        }
    }

    void ShowLightSphere(Vector3 position)
    {
        if (lightSphere == null)
        {
            lightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Light light = lightSphere.AddComponent<Light>();
            light.color = lightColor;
            light.intensity = lightIntensity;
            light.range = 5f; // Adjust the range as desired
            lightSphere.transform.localScale = Vector3.one * 0.1f; // Adjust the scale as desired
        }

        lightSphere.transform.position = position;


    }
}
