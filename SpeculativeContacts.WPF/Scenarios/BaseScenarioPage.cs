using SpeculativeContacts.Code;
using SpeculativeContacts.Controls;
using SpeculativeContacts.Engine;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpeculativeContacts.Scenarios
{
    public abstract class BaseScenarioPage : Page, IDisposable
    {
        private readonly List<Contact> _contacts = new List<Contact>();
        private readonly List<RigidBody> _objects = new List<RigidBody>();
        private readonly List<RigidBody> _ghostObjs = new List<RigidBody>();
        private readonly List<Ellipse> _renderedContacts = new List<Ellipse>();
        private readonly List<System.Windows.Shapes.Rectangle> _renderedMotionBounds = new List<System.Windows.Shapes.Rectangle>();
        private readonly List<System.Windows.Shapes.Line> _renderedOrigins = new List<System.Windows.Shapes.Line>();

        public SolverType SolverKind
        {
            get => _solverType;
            set => _solverType = value;
        }
        private SolverType _solverType = SolverType.Discrete;

        public int NumIterations
        {
            get => _numIterations;
            set => _numIterations = value;
        }
        private int _numIterations = 3;

        public double TimeStepScale
        {
            get => _timeStepScale;
            set => _timeStepScale = value;
        }
        private double _timeStepScale = 1.0;

        public double MinTimeStepScale
        {
            get => _minTimeStepScale;
            set => _minTimeStepScale = value;
        }
        private double _minTimeStepScale = 1.0;

        public double MaxTimeStepScale
        {
            get => _maxTimeStepScale;
            set => _maxTimeStepScale = value;
        }
        private double _maxTimeStepScale = 1.0 / 100000.0;

        public bool IsShownMotionBoxes
        {
            get => _isShownMotionBoxes;
            set => _isShownMotionBoxes = value;
        }
        private bool _isShownMotionBoxes = false;

        public bool IsShownContacts
        {
            get => _isShownContacts;
            set => _isShownContacts = value;
        }
        private bool _isShownContacts = false;

        public bool IsShownOrigins
        {
            get => _isShownOrigins;
            set => _isShownOrigins = value;
        }
        private bool _isShownOrigins = false;

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (value)
                    Pause();
                else
                    Continue();
                Debug.Assert(_isPaused == value);
            }
        }
        private bool _isPaused = false;

        public bool IsStepping
        {
            get => _isStepping;
            set
            {
                if (value)
                    EnableStepping();
                else
                    DisableStepping();
                Debug.Assert(_isStepping == value);
            }
        }
        private bool _isStepping = false;

        private bool _nextStep = false;

        private readonly GameLoop _loop;

        private bool _isDragging = false;
        private RigidBody _dragObj = null;
        private bool _isDisposed;

        protected abstract IEnumerable<RigidBody> Bodies { get; }

        protected abstract Canvas RootCanvas { get; }

        protected BaseScenarioPage()
        {
            _loop = new GameLoop();
            _loop.UpdateEvent += new GameLoop.UpdateHandler(Update);

            MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseDown);
            MouseMove += new MouseEventHandler(OnMouseMove);
            MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseUp);
            Loaded += OnLoaded;
        }

        public void Pause()
        {
            _isPaused = true;
            _isStepping = false;
            _nextStep = false;
        }

        public void Continue()
        {
            _isPaused = false;
            _isStepping = false;
            _nextStep = false;
        }

        public void DisableStepping()
        {
            _isStepping = false;
            _nextStep = false;
        }

        public void EnableStepping()
        {
            _isStepping = true;
            _nextStep = true;
        }

        public void NextStep()
        {
            _nextStep = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            _objects.Clear();
            _ghostObjs.Clear();

            // get a list of all mapped rigid bodies
            RigidBody[] bodies = Bodies.ToArray();
            foreach (RigidBody rb in bodies)
            {
                _objects.Add(rb);

                // create a ghost for this object
                RigidBody ghost = rb switch
                {
                    BlueBall => new BlueBall(),
                    RedBall => new RedBall(),
                    GreenBall => new GreenBall(),
                    Box => new Box(),
                    _ => null
                };

                if (ghost is not null)
                {
                    ghost.Opacity = 0.5;
                    ghost.Width = rb.Width;
                    ghost.Height = rb.Height;
                    ghost.Initialise(1, new Vector2(), 0);
                    _ghostObjs.Add(ghost);
                    rb.Ghost = ghost;
                }
            }

            // initialise each RigidBody (Ball, Floor, Box, Plane)
            foreach (RigidBody rb in bodies)
            {
                if (rb is Ball ball) ball.Initialise(1, rb.DefaultVelocity, rb.DefaultAngularVelocity, rb.DefaultFriction);
                else if (rb is Floor floor) floor.Initialise(0, rb.DefaultVelocity, rb.DefaultAngularVelocity, rb.DefaultFriction);
                else if (rb is Box box) box.Initialise(1, rb.DefaultVelocity, rb.DefaultAngularVelocity, rb.DefaultFriction);
                else if (rb is Plane plane) plane.Initialise(0, rb.DefaultVelocity, rb.DefaultAngularVelocity, rb.DefaultFriction);
            }

            _loop.Start();
        }

        private void SetPos(RigidBody rb, MouseEventArgs e)
        {
            Point clickPos = e.GetPosition(this);
            _dragObj.Vel = new Vector2();
            _dragObj.AngularVel = 0;
            _dragObj.Position = new Vector2(clickPos.X, clickPos.Y);
        }

        public void Restart()
        {
            _contacts.Clear();

            foreach (RigidBody rb in _objects)
            {
                if (rb.Type == BodyType.Dynamic)
                {
                    rb.Vel = rb._mapVel;
                    rb.AngularVel = rb._mapAngularVel;
                    rb.Position = rb._mapPos;
                    rb.Angle = rb._mapAngle;
                }
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                SetPos(_dragObj, e);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(RootCanvas);

            RigidBody[] foundBodies = WPFUtils.FindElementsInHostCoordinates<RigidBody>(position, RootCanvas).ToArray();
            foreach (RigidBody body in foundBodies)
            {
                if (body.Type == BodyType.Static)
                    continue;
                if (body.Ghost is not null)
                {
                    _dragObj = body;
                    _isDragging = true;
                    break;
                }
            }

            if (_isDragging)
            {
                SetPos(_dragObj, e);
            }
        }

        private void RenderContacts(IEnumerable<Contact> contacts)
        {
            const double kRadius = 5;

            foreach (Contact c in contacts)
            {
                Ellipse ea = new Ellipse
                {
                    Width = kRadius * 2,
                    Height = kRadius * 2,
                    Fill = new SolidColorBrush(Colors.Red),
                    Stroke = null,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(c.PointA.X - kRadius, c.PointA.Y - kRadius, 0, 0),
                    IsHitTestVisible = false,
                };

                Ellipse eb = new Ellipse
                {
                    Width = kRadius * 2,
                    Height = kRadius * 2,
                    Fill = new SolidColorBrush(Colors.Green),
                    Stroke = null,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(c.PointB.X - kRadius, c.PointB.Y - kRadius, 0, 0),
                    IsHitTestVisible = false,
                };

                RootCanvas.Children.Add(ea);
                RootCanvas.Children.Add(eb);

                _renderedContacts.Add(ea);
                _renderedContacts.Add(eb);
            }
        }

        private void RenderMotionBounds(IEnumerable<RigidBody> bodies)
        {
            // render new ones
            foreach (RigidBody body in bodies)
            {
                AABB aabb = body.MotionBounds;

                if (aabb.HalfExtents.LenSquared == 0)
                    continue;

                var ext = aabb.HalfExtents;

                var p = aabb.Center;

                var r = new System.Windows.Shapes.Rectangle()
                {
                    Width = ext.X * 2,
                    Height = ext.Y * 2,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Yellow,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(p.X - ext.X, p.Y - ext.Y, 0, 0),
                    IsHitTestVisible = false,
                };

                RootCanvas.Children.Add(r);

                _renderedMotionBounds.Add(r);
            }
        }

        private void ClearShapes<T>(List<T> list) where T : Shape
        {
            foreach (T e in list)
                RootCanvas.Children.Remove(e);
            list.Clear();
        }

        private void RenderOrigins(IEnumerable<RigidBody> bodies)
        {
            const double kLineExt = 20;

            foreach (RigidBody body in bodies)
            {
                var p = body.Position;

                var normal = Vector2.FromAngle(body.Angle);
                var tangent = -normal.Perp();

                Vector2 nA = p;
                Vector2 nB = p + normal * kLineExt;

                Vector2 tA = p;
                Vector2 tB = p + tangent * kLineExt;

                var a = new System.Windows.Shapes.Line()
                {
                    X1 = nA.X,
                    Y1 = nA.Y,
                    X2 = nB.X,
                    Y2 = nB.Y,
                    Stroke = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    IsHitTestVisible = false,
                };

                var b = new System.Windows.Shapes.Line()
                {
                    X1 = tA.X,
                    Y1 = tA.Y,
                    X2 = tB.X,
                    Y2 = tB.Y,
                    Stroke = Brushes.Blue,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    IsHitTestVisible = false,
                };

                RootCanvas.Children.Add(a);
                RootCanvas.Children.Add(b);

                _renderedOrigins.Add(a);
                _renderedOrigins.Add(b);
            }
        }

        private void ApplyForces()
        {
            foreach (RigidBody rb in _objects)
            {
                if (rb.Type == BodyType.Dynamic)
                    rb.Vel += Constants.Gravity;
            }

            if (_isDragging)
            {
                _dragObj.Vel = new Vector2();
                _dragObj.AngularVel = 0;
            }
        }

        private void GenerateMotionBounds(double dt)
        {
            foreach (RigidBody rb in _objects)
            {
                rb.GenerateMotionAABB(dt);
            }
        }

        private void GenerateContacts()
        {
            _contacts.Clear();

            for (int i = 0; i < _objects.Count - 1; i++)
            {
                RigidBody A = _objects[i];

                for (int j = i + 1; j < _objects.Count; j++)
                {
                    RigidBody B = _objects[j];

                    if (A.Type == BodyType.Dynamic || B.Type == BodyType.Dynamic)
                    {
                        if (AABB.Overlap(A.MotionBounds, B.MotionBounds))
                        {
                            // generate contacts for this pair
                            List<Contact> lc = A.GetClosestPoints(B);

                            // add to main list
                            _contacts.AddRange(lc);
                        }
                    }
                }
            }
        }

        private void RenderDebug()
        {
            ClearShapes(_renderedOrigins);
            if (IsShownOrigins)
                RenderOrigins(_objects);

            ClearShapes(_renderedMotionBounds);
            if (IsShownMotionBoxes)
                RenderMotionBounds(_objects);

            ClearShapes(_renderedContacts);
            if (IsShownContacts)
                RenderContacts(_contacts);
        }

        private void Update(object sender, GameLoopUpdateEventArgs e)
        {
            bool simPause = _isPaused;

            if (_isStepping)
            {
                if (_nextStep)
                {
                    simPause = false;
                    _nextStep = false;
                }
            }

            double dt = Constants.FixedTimeStep * _timeStepScale;

            if (simPause)
            {
                if (!_nextStep || !_isStepping)
                {
                    GenerateMotionBounds(dt);
                    GenerateContacts();
                    RenderDebug();
                }
                return;
            }

            ApplyForces();

            GenerateMotionBounds(dt);

            GenerateContacts();

            RenderDebug();

            Solver.Solve(_contacts, _numIterations, _solverType, dt);

            foreach (RigidBody rb in _objects)
            {
                if (rb.Ghost != null)
                {
                    rb.Ghost.Position = rb.Position + rb.Vel * dt;
                    rb.Ghost.Angle = rb.Angle + rb.AngularVel * dt;
                }

                rb.Integrate(dt);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _loop.Stop();

                    MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseDown);
                    MouseMove -= new MouseEventHandler(OnMouseMove);
                    MouseLeftButtonUp -= new MouseButtonEventHandler(OnMouseUp);
                    Loaded -= OnLoaded;
                }
                _isDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BaseScenarioPage()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
