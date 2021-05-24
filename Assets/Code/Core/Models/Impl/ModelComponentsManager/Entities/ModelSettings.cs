using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager.Entities
{
    public class ModelSettings : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _modelScale;
        [SerializeField]
        private MonsterType _monsterType;
        
        public Vector3 ModelScale { get => _modelScale; }
        public MonsterType MonsterType { get => _monsterType; }
    }

    public enum MonsterType
    {
        Normal,
        Tribute,
        Fusion,
        Ritual,
        Syncro,
        XYZ,
        Pendulum,
        Link,
        PolymerizationSpellCard,
    }
}
