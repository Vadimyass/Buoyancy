using UnityEngine;

namespace Gameplay
{
    public class ParticleFollower : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        private Vector3 _offset;
        private void Start()
        {
            _offset = transform.position - _target.position;
        }

        private void Update()
        {
            var newPosition = _target.position + _offset;
            transform.position = new Vector3(newPosition.x, transform.position.y,newPosition.z);
        }
    }
}