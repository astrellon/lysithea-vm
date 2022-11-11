using UnityEngine;

namespace LysitheaVM.Unity
{
    public class AutoDestroyParticles : MonoBehaviour
    {
        private ParticleSystem ps;

        void Start()
        {
            this.ps = this.GetComponent<ParticleSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            if (this.ps && !this.ps.IsAlive())
            {
                Destroy(this.gameObject);
            }
        }
    }
}
