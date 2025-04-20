using UnityEngine;

namespace Gameplay
{
    public class BoatBalanceAim : MonoBehaviour
    {
        public Transform boatTransform;
        public Transform spineAimTarget;
        public float maxOffset = 0.5f;
        public float sensitivity = 30f;
        public float smoothTime = 0.2f;

        private Vector3 velocity;

        void Update()
        {
            float roll = boatTransform.localEulerAngles.z;
            if (roll > 180) roll -= 360;

            float leanOffset = Mathf.Clamp(-roll / sensitivity, -1f, 1f) * maxOffset;
            Vector3 targetLocalPosition = new Vector3(leanOffset, spineAimTarget.localPosition.y, spineAimTarget.localPosition.z);

            spineAimTarget.localPosition = Vector3.SmoothDamp(
                spineAimTarget.localPosition,
                targetLocalPosition,
                ref velocity,
                smoothTime
            );
        }
    }
}