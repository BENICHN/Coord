using BenLib.Framework;
using BenLib.Standard;
using BenLib.WPF;
using Coord;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static BenLib.Framework.NumFramework;
using static Coord.VisualObjects;
using static System.Math;

namespace CoordSpec
{
    public class Watney : PhysicalObject
    {
        public override string Type => "Watney";

        /// <summary>
        /// Angle formé entre le corps de Mark et son bras (0 : bras collé à son corps, sens indirect) [rad]
        /// </summary>
        public double ArmAngle { get => (double)GetValue(ArmAngleProperty); set => SetValue(ArmAngleProperty, value); }
        public static readonly DependencyProperty ArmAngleProperty = CreateProperty<double>(true, true, "ArmAngle", typeof(Watney), (sender, e) => { if (sender is Watney owner) owner.SetArm(false); });

        /// <summary>
        /// Quantifie la résistance du bras au changement d'orientation par rapport au repère
        /// </summary>
        public double ArmResistance { get => (double)GetValue(ArmResistanceProperty); set => SetValue(ArmResistanceProperty, value); }
        public static readonly DependencyProperty ArmResistanceProperty = CreateProperty<double>(true, true, "ArmResistance", typeof(Watney));

        /// <summary>
        /// Pression à l'intérieur de la combinaison [PSI]
        /// </summary>
        public double Pressure { get => (double)GetValue(PressureProperty); set => SetValue(PressureProperty, value); }
        public static readonly DependencyProperty PressureProperty = CreateProperty<double>(true, true, "Pressure", typeof(Watney));

        /// <summary>
        /// Surface du trou de la combinaison [mm²]
        /// </summary>
        public double HoleArea { get => (double)GetValue(HoleAreaProperty); set => SetValue(HoleAreaProperty, value); }
        public static readonly DependencyProperty HoleAreaProperty = CreateProperty<double>(true, true, "HoleArea", typeof(Watney));

        /// <summary>
        /// Débit massique de l'air qui s'échappe de la combinaison [kg/s]
        /// </summary>
        public double MassRate { get => (double)GetValue(MassRateProperty); set => SetValue(MassRateProperty, value); }
        public static readonly DependencyProperty MassRateProperty = CreateProperty<double>(true, true, "MassRate", typeof(Watney));

        /// <summary>
        /// Taille du rectangle [m, m]
        /// </summary>
        public Size Size { get => (Size)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
        public static readonly DependencyProperty SizeProperty = CreateProperty<Size>(true, true, "Size", typeof(Watney), (sender, e) =>
        {
            if (sender is Watney owner)
            {
                var newValue = (Size)e.NewValue;
                var dimensions = new Vector(newValue.Width / 2, newValue.Height / 2);
                var dimensionsc = new Vector(newValue.Width / 2, -newValue.Height / 2);
                var location = owner.Location;
                owner.m_body = new Polygon(location - dimensionsc, location + dimensions, location + dimensionsc, location - dimensions);
                owner.ComputeAngularMass();
            }
        });

        /// <summary>
        /// Collection de <see cref="Character"/> à retourner par <see cref="GetCharacters"/> pour dessiner le corps
        /// </summary>
        public Character[] BodyGraphic { get => (Character[])GetValue(BodyGraphicProperty); set => SetValue(BodyGraphicProperty, value); }
        public static readonly DependencyProperty BodyGraphicProperty = CreateProperty<Character[]>(true, true, "BodyGraphic", typeof(Watney));

        /// <summary>
        /// Limites de <see cref="BodyGraphic"/> prises en compte
        /// </summary>
        public Rect BodyGraphicBounds { get => (Rect)GetValue(BodyGraphicBoundsProperty); set => SetValue(BodyGraphicBoundsProperty, value); }
        public static readonly DependencyProperty BodyGraphicBoundsProperty = CreateProperty<Rect>(true, true, "BodyGraphicBounds", typeof(Watney));

        /// <summary>
        /// Emplacement de la jonction du bras dans <see cref="BodyGraphicBounds"/>
        /// </summary>
        public RectPoint ArmLocation { get => (RectPoint)GetValue(ArmLocationProperty); set => SetValue(ArmLocationProperty, value); }
        public static readonly DependencyProperty ArmLocationProperty = CreateProperty<RectPoint>(true, true, "ArmLocation", typeof(Watney));

        /// <summary>
        /// Collection de <see cref="Character"/> à retourner par <see cref="GetCharacters"/> pour dessiner le bras
        /// </summary>
        public Character[] ArmGraphic { get => (Character[])GetValue(ArmGraphicProperty); set => SetValue(ArmGraphicProperty, value); }
        public static readonly DependencyProperty ArmGraphicProperty = CreateProperty<Character[]>(true, true, "ArmGraphic", typeof(Watney));

        /// <summary>
        /// Emplacement de la jonction du bras dans <see cref="ArmGraphic"/>.Geometry().Bounds
        /// </summary>
        public RectPoint ArmAnchor { get => (RectPoint)GetValue(ArmAnchorProperty); set => SetValue(ArmAnchorProperty, value); }
        public static readonly DependencyProperty ArmAnchorProperty = CreateProperty<RectPoint>(true, true, "ArmAnchor", typeof(Watney));

        /// <summary>
        /// <see langword="true"/> pour ne dessiner que le rectangle et la ligne; <see langword="false"/> pour dessiner <see cref="BodyGraphic"/> et <see cref="ArmGraphic"/>; <see langword="null"/> pour dessiner les deux
        /// </summary>
        public bool? Minimal { get => (bool?)GetValue(MinimalProperty); set => SetValue(MinimalProperty, value); }
        public static readonly DependencyProperty MinimalProperty = CreateProperty<bool?>(true, true, "Minimal", typeof(Watney));

        /// <summary>
        /// Corps subissant les transformations
        /// </summary>
        private Polygon m_body;

        /// <summary>
        /// Vecteur bras
        /// </summary>
        private Vector m_arm;

        protected override IReadOnlyCollection<Character> GetCharacters(ReadOnlyCoordinatesSystemManager coordinatesSystemManager)
        {
            var location = coordinatesSystemManager.ComputeOutCoordinates(Location);
            var speed = coordinatesSystemManager.ComputeOutCoordinates(Speed);
            var body = m_body.ComputeOutCoordinates(coordinatesSystemManager);

            IEnumerable<Character> MinimalCharacters()
            {
                var appl = coordinatesSystemManager.ComputeOutCoordinates(m_body[0] + m_body.VectorBetween(3, 0) * ArmLocation.YProgress);
                yield return body.ToGeometry().ToCharacter(Fill.EditFreezable(b => b.Opacity = 0.5), Stroke); //Corps
                yield return Character.Line(appl, appl + coordinatesSystemManager.ComputeOutCoordinates(m_arm)).Color(Fill, Stroke); //Bras
                yield return Character.Line(location, appl).Color(Fill, Stroke); //Dimensions
            }

            IEnumerable<Character> GraphicCharacters()
            {
                //Corps--------------------------------------------------------------------------------------------------------------------------------

                var bodyGraphic = BodyGraphic.CloneCharacters().ToArray();
                var bodyGraphicBounds = BodyGraphicBounds;
                var armLocation = ArmLocation.GetPoint(bodyGraphicBounds);
                var bodyGraphicTransform = Matrix.Identity;

                double scalar = body.VectorBetween(1, 2).Length / bodyGraphicBounds.Height;
                bodyGraphicTransform.ScaleAt(scalar, scalar, bodyGraphicBounds.X, bodyGraphicBounds.Y);

                double angle = -Angle * 180 / PI;
                bodyGraphicTransform.RotateAt(angle, bodyGraphicBounds.X, bodyGraphicBounds.Y);

                var offset = body[0] - bodyGraphicBounds.TopLeft;
                bodyGraphicTransform.Translate(offset.X, offset.Y);

                bodyGraphic.Transform(bodyGraphicTransform, false);

                //Bras---------------------------------------------------------------------------------------------------------------------------------

                var armGraphic = ArmGraphic.CloneCharacters().ToArray();
                var armGraphicBounds = armGraphic.Geometry().Bounds;
                var armAnchor = ArmAnchor.GetPoint(armGraphicBounds);
                var armGraphicTransform = Matrix.Identity;

                armGraphicTransform.ScaleAt(scalar, scalar, armAnchor.X, armAnchor.Y);
                armGraphicTransform.RotateAt(angle + ArmAngle * 180 / PI - 90 - 15, armAnchor.X, armAnchor.Y);

                var of = bodyGraphicTransform.Transform(armLocation) - armAnchor;
                armGraphicTransform.Translate(of.X, of.Y);

                armGraphic.Transform(armGraphicTransform, false);

                //-------------------------------------------------------------------------------------------------------------------------------------

                return new[] { bodyGraphic[0] }.Concat(armGraphic).Concat(bodyGraphic.Skip(1));
            }

            return (Minimal == true ? MinimalCharacters() : Minimal == null ? GraphicCharacters().Concat(MinimalCharacters()) : GraphicCharacters()).Concat(VectorVisualObject.GetCharacters(location, speed, new TriangleArrow { Length = 15, Width = 5 }, ArrowEnd.End, FlatBrushes.Nephritis, new PlanePen(FlatBrushes.Nephritis, 2)))/*.Concat(VectorVisualObject.GetCharacters(location, coordinatesSystemManager.ComputeOutCoordinates(Acceleration), new TriangleArrow(false, false, 15, 5), ArrowEnd.End, FlatBrushes.PeterRiver, new PlanePen(FlatBrushes.PeterRiver, 2)))*/.ToArray(); // Vecteur vitesse
        }

        public override void Update()
        {
            double pressure = 6894.76 * Pressure; //Pression [PSI -> Pa]
            double holeArea = 1E-6 * HoleArea; //Surface du trou [mm² -> m²]
            double density = pressure * 32E-3 / (8.314 * 293); //ρ=(PM)/(RT)
            double forceLength = pressure * holeArea + MassRate.Pow(2) / (holeArea * density); //Norme de la force (F=qm·ve+A·P=qm²/(A·ρ)+A·P) [N]
            var force = m_arm.ReLength(-forceLength); //Vecteur force (mise à l'échelle du vecteur bras)

            var dimensions = m_body[0] + m_body.VectorBetween(3, 0) * ArmLocation.YProgress - Location; //Vecteur ((Centre d'inertie) -> (Point d'application de la force))
            ApplyForce(force, dimensions);

            base.Update();

            m_body.Translate(Speed / FPS);
            m_body.RotateAt(AngularSpeed / FPS, Location);
            SetArm(true);

            NotifyChanged();
        }

        /// <summary>
        /// Calcule le vecteur bras à partir de <see cref="m_body"/> et de <see cref="ArmAngle"/>
        /// </summary>
        private void SetArm(bool resist)
        {
            if (resist)
            {
                var arm = m_body.VectorBetween(1, 0).ReLength(0.4).Rotate(-ArmAngle - PI / 2);
                double newAngle = PM(AngleBetweenVectors(new Vector(1, 0), arm));
                double oldAngle = PM(AngleBetweenVectors(new Vector(1, 0), m_arm));
                double angle = IA(oldAngle, newAngle, AngularSpeed, m_arm.Length == 0 ? 1 : 1 / ArmResistance);
                ArmAngle = (ArmAngle + newAngle - angle).Trim(0, PI);

                double PM(double agl)
                {
                    double result = agl % (2 * PI);
                    if (result < 0) result += 2 * PI;
                    return result;
                }

                double IA(double start, double end, double speed, double progress)
                {
                    if (speed < 0 && start < end) start += 2 * PI;
                    if (speed > 0 && start > end) end += 2 * PI;
                    return Num.Interpolate(start, end, progress);
                }
            }
            else m_arm = m_body.VectorBetween(1, 0).ReLength(0.4).Rotate(-ArmAngle - PI / 2);
        }

        /// <summary>
        /// Calcule le moment d'inertie de Mark
        /// </summary>
        public void ComputeAngularMass()
        {
            AngularMass = 0;
            double height = Size.Height;
            double radius = height / 2;
            for (double i = 0; i < height; i += 0.01) AngularMass += (i - radius).Pow(2);
            AngularMass *= Mass * 0.01 / height;
        }
    }
}
