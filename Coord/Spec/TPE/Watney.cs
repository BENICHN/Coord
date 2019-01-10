using BenLib;
using BenLib.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Coord.MainWindow;
using static System.Math;

namespace Coord
{
    public class Watney : PhysicalObject
    {
        private double m_armAngle;
        private double m_pressure;
        private double m_holeArea;
        private Size m_size;
        private IEnumerable<Character> m_bodyGraphic;
        private Rect m_bodyGraphicBounds;
        private IEnumerable<Character> m_armGraphic;
        private RectPoint m_armLocation;
        private RectPoint m_armAnchor;
        private bool? m_minimal;

        /// <summary>
        /// Angle formé entre le corps de Mark et son bras (0 : bras collé à son corps, sens indirect) [rad]
        /// </summary>
        public double ArmAngle
        {
            get => m_armAngle;
            set
            {
                m_armAngle = value;
                SetArm();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Pression à l'intérieur de la combinaison [PSI]
        /// </summary>
        public double Pressure
        {
            get => m_pressure;
            set
            {
                m_pressure = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Surface du trou de la combinaison [mm²]
        /// </summary>
        public double HoleArea
        {
            get => m_holeArea;
            set
            {
                m_holeArea = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Taille du rectangle [m, m]
        /// </summary>
        public Size Size
        {
            get => m_size;
            set
            {
                m_size = value;
                var dimensions = new Vector(value.Width / 2, value.Height / 2);
                var dimensionsc = new Vector(value.Width / 2, -value.Height / 2);
                m_body = new Polygon(Location - dimensionsc, Location + dimensions, Location + dimensionsc, Location - dimensions);
                Compute();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Collection de <see cref="Character"/> à retourner par <see cref="GetCharacters"/> pour dessiner le corps
        /// </summary>
        public IEnumerable<Character> BodyGraphic
        {
            get => m_bodyGraphic;
            set
            {
                m_bodyGraphic = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Limites de <see cref="BodyGraphic"/> prises en compte
        /// </summary>
        public Rect BodyGraphicBounds
        {
            get => m_bodyGraphicBounds;
            set
            {
                m_bodyGraphicBounds = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Emplacement de la jonction du bras dans <see cref="BodyGraphicBounds"/>
        /// </summary>
        public RectPoint ArmLocation
        {
            get => m_armLocation;
            set
            {
                m_armLocation = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Collection de <see cref="Character"/> à retourner par <see cref="GetCharacters"/> pour dessiner le bras
        /// </summary>
        public IEnumerable<Character> ArmGraphic
        {
            get => m_armGraphic;
            set
            {
                m_armGraphic = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Emplacement de la jonction du bras dans <see cref="ArmGraphic"/>.Geometry().Bounds
        /// </summary>
        public RectPoint ArmAnchor
        {
            get => m_armAnchor;
            set
            {
                m_armAnchor = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// <see langword="true"/> pour ne dessiner que le rectangle et la ligne; <see langword="false"/> pour dessiner <see cref="BodyGraphic"/> et <see cref="ArmGraphic"/>; <see langword="null"/> pour dessiner les deux
        /// </summary>
        public bool? Minimal
        {
            get => m_minimal;
            set
            {
                m_minimal = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Corps subissant les transformations
        /// </summary>
        private Polygon m_body;

        /// <summary>
        /// Vecteur bras
        /// </summary>
        private Vector m_arm;

        public override IReadOnlyCollection<Character> GetCharacters(CoordinatesSystemManager coordinatesSystemManager)
        {
            var location = coordinatesSystemManager.ComputeOutCoordinates(Location);
            var speed = coordinatesSystemManager.ComputeOutCoordinates(Speed);
            var body = m_body.ComputeOutCoordinates(coordinatesSystemManager);

            IEnumerable<Character> MinimalCharacters()
            {
                var appl = coordinatesSystemManager.ComputeOutCoordinates(m_body[0] + m_body.VectorBetween(3, 0) * ArmLocation.YProgress);
                yield return new Character(body.ToGeometry(), Fill.Edit(b => b.Opacity = 0.5), Stroke); //Corps
                yield return Character.Line(appl, appl + coordinatesSystemManager.ComputeOutCoordinates(m_arm), Fill, Stroke); //Bras
                yield return Character.Line(location, appl, Fill, Stroke); //Dimensions
            }

            IEnumerable<Character> GraphicCharacters()
            {
                //Corps--------------------------------------------------------------------------------------------------------------------------------

                var bodyGraphic = BodyGraphic.CloneCharacters().ToArray();
                var bodyGraphicBounds = BodyGraphicBounds;
                var armLocation = ArmLocation.GetPoint(bodyGraphicBounds);
                var bodyGraphicTransform = Matrix.Identity;

                var scalar = body.VectorBetween(1, 2).Length / bodyGraphicBounds.Height;
                bodyGraphicTransform.ScaleAt(scalar, scalar, bodyGraphicBounds.X, bodyGraphicBounds.Y);

                var angle = -Angle * 180 / PI;
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

            return (Minimal == true ? MinimalCharacters() : Minimal == null ? GraphicCharacters().Concat(MinimalCharacters()) : GraphicCharacters()).Concat(VectorVisualObject.GetCharacters(location, speed, new TriangleArrow(false, false, 15, 5), ArrowEnd.End, FlatBrushes.Nephritis, new Pen(FlatBrushes.Nephritis, 2))).ToArray(); // Vecteur vitesse
        }

        public override void Update()
        {
            double pressure = 6894.76 * Pressure; //Pression (PSI -> Pa)
            double holeArea = 1E-6 * HoleArea; //Surface du trou (mm² -> m²)
            double forceLength = pressure * holeArea; //Norme de la force (N)
            var force = m_arm.ReLength(-forceLength); //Vecteur force (mise à l'échelle du vecteur bras)

            var dimensions = m_body[0] + m_body.VectorBetween(3, 0) * ArmLocation.YProgress - Location; //Vecteur ((Centre d'inertie) -> (Point d'application de la force))
            ApplyForce(force, dimensions);

            base.Update();

            m_body.Translate(Speed / FPS);
            m_body.RotateAt(AngularSpeed / FPS, Location);
            SetArm();

            NotifyChanged();
        }

        /// <summary>
        /// Calcule le vecteur bras à partir de <see cref="m_body"/> et de <see cref="ArmAngle"/>
        /// </summary>
        private void SetArm() => m_arm = m_body.VectorBetween(1, 0).ReLength(0.4).Rotate(-ArmAngle - PI / 2);

        /// <summary>
        /// Calcule le moment d'inertie de Mark
        /// </summary>
        public void Compute()
        {
            AngularMass = 0;
            double height = Size.Height;
            double radius = height / 2;
            for (double i = 0; i < height; i += 0.01) AngularMass += (i - radius).Pow(2);
            AngularMass *= Mass * 0.01 / height;
        }
    }
}
