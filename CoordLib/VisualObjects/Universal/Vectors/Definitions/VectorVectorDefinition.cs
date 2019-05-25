using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un vecteur du plan grâce à un <see cref="Vector"/>
    /// </summary>
    public class VectorVectorDefinition : VectorDefinition
    {
        /// <summary>
        /// Vecteur du plan
        /// </summary>
        public new Vector InVector { get => (Vector)GetValue(InVectorProperty); set => SetValue(InVectorProperty, value); }
        public static readonly DependencyProperty InVectorProperty = CreateProperty<Vector>(true, true, "InVector", typeof(VectorVectorDefinition));

        protected override void OnChanged() => base.InVector = InVector;
    }
}
