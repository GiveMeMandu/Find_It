using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InteractiveWind2D
{
    public class ShaderInstancer : MonoBehaviour
    {
        void Awake()
        {
            Renderer renderer = GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material = renderer.material;
            }
            else
            {
                //Check for Graphic:
                Graphic graphic = GetComponent<Graphic>();

                if (graphic.material != null)
                {
                    graphic.material = Instantiate<Material>(graphic.material);
                }
            }
        }
    }
}