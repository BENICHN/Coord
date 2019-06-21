using BenLib.Framework;
using Coord;
using System;
using System.Windows;
using static Coord.VisualObjects;

namespace CoordSpec
{
    public abstract class PhysicalObject : VisualObject
    {
        /// <summary>
        /// Position
        /// </summary>
        public Point Location { get => (Point)GetValue(LocationProperty); set => SetValue(LocationProperty, value); }
        public static readonly DependencyProperty LocationProperty = CreateProperty<PhysicalObject, Point>(false, false, true, "Location");

        /// <summary>
        /// Force
        /// </summary>
        public Vector Force { get => (Vector)GetValue(ForceProperty); set => SetValue(ForceProperty, value); }
        public static readonly DependencyProperty ForceProperty = CreateProperty<PhysicalObject, Vector>(false, false, true, "Force");

        /// <summary>
        /// Accélération
        /// </summary>
        public Vector Acceleration { get => (Vector)GetValue(AccelerationProperty); set => SetValue(AccelerationProperty, value); }
        public static readonly DependencyProperty AccelerationProperty = CreateProperty<PhysicalObject, Vector>(false, false, true, "Acceleration");

        /// <summary>
        /// Quantité de mouvement
        /// </summary>
        public Vector Momentum => Speed * Mass;

        /// <summary>
        /// Vitesse
        /// </summary>
        public Vector Speed { get => (Vector)GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }
        public static readonly DependencyProperty SpeedProperty = CreateProperty<PhysicalObject, Vector>(false, false, true, "Speed");

        /// <summary>
        /// Masse
        /// </summary>
        public double Mass { get => (double)GetValue(MassProperty); set => SetValue(MassProperty, value); }
        public static readonly DependencyProperty MassProperty = CreateProperty<PhysicalObject, double>(false, false, true, "Mass");

        /// <summary>
        /// Angle
        /// </summary>
        public double Angle { get => (double)GetValue(AngleProperty); set => SetValue(AngleProperty, value); }
        public static readonly DependencyProperty AngleProperty = CreateProperty<PhysicalObject, double>(false, false, true, "Angle");

        /// <summary>
        /// Moment des forces
        /// </summary>
        public double Torque { get => (double)GetValue(TorqueProperty); set => SetValue(TorqueProperty, value); }
        public static readonly DependencyProperty TorqueProperty = CreateProperty<PhysicalObject, double>(false, false, true, "Torque");

        /// <summary>
        /// Accélération angulaire
        /// </summary>
        public double AngularAcceleration { get => (double)GetValue(AngularAccelerationProperty); set => SetValue(AngularAccelerationProperty, value); }
        public static readonly DependencyProperty AngularAccelerationProperty = CreateProperty<PhysicalObject, double>(false, false, true, "AngularAcceleration");

        /// <summary>
        /// Moment cinétique
        /// </summary>
        public double AngularMoment => AngularSpeed * AngularMass;

        /// <summary>
        /// Vitesse angulaire
        /// </summary>
        public double AngularSpeed { get => (double)GetValue(AngularSpeedProperty); set => SetValue(AngularSpeedProperty, value); }
        public static readonly DependencyProperty AngularSpeedProperty = CreateProperty<PhysicalObject, double>(false, false, true, "AngularSpeed");

        /// <summary>
        /// Moment d'inertie
        /// </summary>
        public double AngularMass { get => (double)GetValue(AngularMassProperty); set => SetValue(AngularMassProperty, value); }
        public static readonly DependencyProperty AngularMassProperty = CreateProperty<PhysicalObject, double>(false, false, true, "AngularMass");

        public void ApplyForce(Vector force, Vector location)
        {
            var (d, p) = force.Decompose(location, new Vector(-location.Y, location.X)); //Décomposition : force = a * dimensions + b * perpendim = d + p
            Force += d; //Seule d déplace l'objet
            Torque += location.X * p.Y - location.Y * p.X; //Norme du moment de la force (produit vectoriel) ; seule p a un moment non nul, c'est le même que celui de force
        }

        public virtual void Update()
        {
            Acceleration = Force / Mass;
            Speed += Acceleration / FPS;
            Location += Speed / FPS;

            AngularAcceleration = Torque / AngularMass;
            AngularSpeed += AngularAcceleration / FPS;
            Angle = (Angle + AngularSpeed / FPS) % (2 * Math.PI);

            Force = default;
            Torque = default;
        }
    }
}
