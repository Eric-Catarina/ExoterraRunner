using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    public class CameraModManager : MonoBehaviour
    {
        public Volume volume; // Referência para o Volume onde está o override de Lens Distortion

        LensDistortion lensDistortion; // Referência para o componente LensDistortion
        public SmurfCatMovement playerScript;
        void Start()
        {
            if (volume == null)
            {
                Debug.LogError("Volume não atribuído ao script.");
                return;
            }
        

            // Tente obter o componente LensDistortion no Volume
            if (volume.profile.TryGet(out lensDistortion))
            {
                // Modifique o valor de Intensity conforme necessário
                lensDistortion.intensity.value = 0.5f; // Substitua 0.5f pelo valor desejado
            }
            else
            {
                Debug.LogError("LensDistortion não encontrado no Volume.");
            }
        }

        private void FixedUpdate()
        {
            if (playerScript.rb.linearVelocity.y < -5)
            {
                // Scale lens distortion intensity based on player's fall speed at a maximun of 0.5
                lensDistortion.intensity.value = Mathf.Clamp(-playerScript.rb.linearVelocity.y / 50, 0, 0.6f);
                
            }
            else
            {
                lensDistortion.intensity.value = 0;
            }
        }
    }
}
