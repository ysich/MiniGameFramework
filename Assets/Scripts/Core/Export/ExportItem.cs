using UnityEngine;

namespace Framework_Export
{
    public class ExportItem:MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField]
        public string FieldName;
        [SerializeField]
        public string targetComponent;
        [SerializeField]
        public string assemblyQualifiedName;
#endif
    }
}