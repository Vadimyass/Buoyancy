using UnityEngine;

namespace Gameplay
{
    public class FoamTrailAnimator : MonoBehaviour
    {
        public float scrollSpeed = 0.5f;
        private Material mat;
        private float offset;

        void Start()
        {
            TrailRenderer trail = GetComponent<TrailRenderer>();
            mat = trail.material;
            mat = new Material(mat); 
            trail.material = mat;
            
        }

        void Update()
        {
            offset += Time.deltaTime * scrollSpeed;
            mat.mainTextureOffset = new Vector2(0, -offset);
        }
    }
}