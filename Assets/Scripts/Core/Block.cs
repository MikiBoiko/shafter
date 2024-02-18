using UnityEngine;
namespace Shafter
{
    public class Block : MonoBehaviour
    {
        private Material[,,] m_materials;
        public Material[,,] Materials => m_materials;

        public void SetMaterial(Material[,,] materials)
        {
            m_materials = materials;
        }
    }
}