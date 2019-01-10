using BenLib;
using System;
using System.Windows;
using static Coord.MainWindow;

namespace Coord
{
    public abstract class PhysicalObject : VisualObject
    {
        protected Point m_location;

        /// <summary>
        /// Position
        /// </summary>
        public Point Location
        {
            get => m_location;
            set
            {
                m_location = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Force
        /// </summary>
        public Vector Force { get; set; }

        /// <summary>
        /// Accélération
        /// </summary>
        public Vector Acceleration { get; set; }

        /// <summary>
        /// Quantité de mouvement
        /// </summary>
        public Vector Momentum => Speed * Mass;

        /// <summary>
        /// Vitesse
        /// </summary>
        public Vector Speed { get; set; }

        /// <summary>
        /// Masse
        /// </summary>
        public double Mass { get; set; }

        protected double m_angle;

        /// <summary>
        /// Angle
        /// </summary>
        public double Angle
        {
            get => m_angle;
            set
            {
                m_angle = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Moment des forces
        /// </summary>
        public double Torque { get; set; }

        /// <summary>
        /// Accélération angulaire
        /// </summary>
        public double AngularAcceleration { get; set; }

        /// <summary>
        /// Moment cinétique
        /// </summary>
        public double AngularMoment => AngularSpeed * AngularMass;

        /// <summary>
        /// Vitesse angulaire
        /// </summary>
        public double AngularSpeed { get; set; }

        /// <summary>
        /// Moment d'inertie
        /// </summary>
        public double AngularMass { get; set; }

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
            m_location += Speed / FPS;

            AngularAcceleration = Torque / AngularMass;
            AngularSpeed += AngularAcceleration / FPS;
            m_angle = (Angle + AngularSpeed / FPS) % (2 * Math.PI);

            Force = default;
            Torque = default;
        }
    }
}
