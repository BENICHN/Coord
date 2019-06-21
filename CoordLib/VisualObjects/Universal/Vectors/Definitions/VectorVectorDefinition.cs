using System.Windows;

namespace Coord
{
    /// <summary>
    /// Détermine un vecteur du plan grâce à un <see cref="Vector"/>
    /// </summary>
    public class VectorVectorDefinition : VectorDefinition
    {
        protected override Freezable CreateInstanceCore() => new VectorVectorDefinition();

        /// <summary>
        /// Vecteur du plan
        /// </summary>
        public new Vector InVector { get => (Vector)GetValue(InVectorProperty); set => SetValue(InVectorProperty, value); }
        public static readonly DependencyProperty InVectorProperty = CreateProperty<VectorVectorDefinition, Vector>(true, true, true, "InVector");

        protected override void OnChanged() => base.InVector = InVector;
    }
}
