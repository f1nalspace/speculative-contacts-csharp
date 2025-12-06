using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;

namespace SpeculativeContacts.Code
{
    public enum BodyType
    {
        Static = 0,
        Dynamic,
    }

    public abstract class RigidBody : ContentControl
    {
        public abstract BodyType Type { get; }

        public Vector2 Vel { get => _vel; set => _vel = value; }
        private Vector2 _vel;

        public double AngularVel { get => _angularVel; set => _angularVel = value; }
        private double _angularVel;

        public double InvMass { get => _invMass; protected set => _invMass = value; }
        private double _invMass;

        public double InvI { get => _invI; protected set => _invI = value; }
        private double _invI;

        public double Friction { get => _friction; protected set => _friction = value; }
        private double _friction;

        public RigidBody Ghost { get => _ghost; set => _ghost = value; }
        private RigidBody _ghost;

        public Vector2 Position
        {
            get => new Vector2(_translate.X + this.Width / 2, _translate.Y + this.Height / 2);
            set
            {
                _translate.X = value.X - this.Width / 2;
                _translate.Y = value.Y - this.Height / 2;

                // reform matrix
                _matrix = new Matrix23(Angle, Position);
            }
        }

        public double Angle
        {
            get => Scalar.DegToRad(_rotate.Angle);
            set
            {
                _rotate.Angle = Scalar.RadToDeg(value);

                // reform matrix
                _matrix = new Matrix23(Angle, Position);
            }
        }

        private TranslateTransform _translate;

        private RotateTransform _rotate;

        public Vector2 _mapPos;

        public Vector2 _mapVel;

        public double _mapAngularVel;

        public double _mapAngle;

        public double _mapFriction;

        private AABB _motionBounds;

        public Matrix23 Transformation => _matrix;
        private Matrix23 _matrix;

        public static readonly DependencyProperty DefaultVelocityProperty =
            DependencyProperty.RegisterAttached(nameof(DefaultVelocity), typeof(Vector2), typeof(RigidBody), new PropertyMetadata() { DefaultValue = new Vector2() });

        public Vector2 DefaultVelocity
        {
            get => (Vector2)GetValue(DefaultVelocityProperty);
            set => SetValue(DefaultVelocityProperty, value);
        }

        public static readonly DependencyProperty DefaultAngularVelocityProperty =
            DependencyProperty.RegisterAttached(nameof(DefaultAngularVelocity), typeof(double), typeof(RigidBody), new PropertyMetadata() { DefaultValue = 0.0 });

        public double DefaultAngularVelocity
        {
            get => (double)GetValue(DefaultAngularVelocityProperty);
            set => SetValue(DefaultAngularVelocityProperty, value);
        }

        public static readonly DependencyProperty DefaultFrictionProperty =
            DependencyProperty.RegisterAttached(nameof(DefaultFriction), typeof(double), typeof(RigidBody), new PropertyMetadata() { DefaultValue = 0.0 });

        public double DefaultFriction
        {
            get => (double)GetValue(DefaultFrictionProperty);
            set => SetValue(DefaultFrictionProperty, value);
        }

        public RigidBody() : base()
        {
        }

        public virtual void Initialise(double invMass, Vector2 initialVelocity, double initialAngulatVelocity, double initialFriction = 0.0)
        {
            TransformGroup tg = new TransformGroup();

            Matrix transformationMatrix;
            if (this.RenderTransform is MatrixTransform)
            {
                MatrixTransform mappedTranform = (MatrixTransform)this.RenderTransform;
                transformationMatrix = mappedTranform.Matrix;
            }
            else if (this.RenderTransform is TransformGroup)
            {
                TransformGroup mappedTransform = (TransformGroup)this.RenderTransform;
                transformationMatrix = mappedTransform.Value;
            }
            else
            {
                throw new Exception("error");
            }

            Vector2 right = new Vector2(transformationMatrix.M11, transformationMatrix.M12);
            double angle = Scalar.AngleFromVector(right);

            _translate = new TranslateTransform();
            _rotate = new RotateTransform { Angle = Scalar.RadToDeg(angle) };

            tg.Children.Add(_rotate);
            tg.Children.Add(_translate);

            this.RenderTransform = tg;

            _vel = initialVelocity;
            _angularVel = initialAngulatVelocity;
            _invMass = invMass;
            _friction = initialFriction;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                Debug.Assert(this.HorizontalAlignment == HorizontalAlignment.Left || this.HorizontalAlignment == HorizontalAlignment.Stretch);
                Debug.Assert(this.VerticalAlignment == VerticalAlignment.Top);
            }

            Position = new Vector2(transformationMatrix.OffsetX + Width / 2, transformationMatrix.OffsetY + Height / 2);

            _mapPos = Position;
            _mapAngle = Angle;
            _mapVel = initialVelocity;
            _mapAngularVel = initialAngulatVelocity;
            _mapFriction = initialFriction;

            // clear this out
            this.Margin = new Thickness(0, 0, 0, 0);
        }

        public abstract void GenerateMotionAABB(double dt);

        public abstract List<Contact> GetClosestPoints(RigidBody rb);

        public void Integrate(double dt)
        {
            Position += _vel * dt;
            Angle += _angularVel * dt;
        }

        public void ApplyImpulse(Vector2 impulse, Vector2 point)
        {
            Vel += InvMass * impulse;
            AngularVel += InvI * Vector2.Cross(point, impulse);
        }

        static public void CopyTransform(RigidBody from, RigidBody to)
        {
            to._rotate.Angle = from._rotate.Angle;
            to._rotate.CenterX = from.Width / 2;
            to._rotate.CenterY = from.Height / 2;

            to.Position = from.Position;
        }

        public AABB MotionBounds
        {
            get => _motionBounds;
            protected set => _motionBounds = value;
        }
    }
}
