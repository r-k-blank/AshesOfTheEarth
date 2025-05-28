using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Entities; // Necesar pentru a urmări o entitate
using AshesOfTheEarth.Entities.Components; // Necesar pentru TransformComponent

namespace AshesOfTheEarth.Graphics
{
    public class Camera
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }
        public Viewport Viewport { get; private set; }

        private Matrix _transformMatrix;
        private bool _isDirty = true; // Recalculează matricea doar când e necesar

        // --- Urmărire Entitate ---
        public Entity Target { get; private set; }
        private TransformComponent _targetTransform;
        public float FollowLerpFactor { get; set; } = 0.8f; // Cât de lin urmărește camera

        public Camera(Viewport viewport)
        {
            Viewport = viewport;
            Position = Vector2.Zero; // Start la origine
            Zoom = 1.0f; // Fără zoom inițial
            Rotation = 0f;
            RecalculateTransformMatrix(); // Calculează matricea inițială
        }

        public void SetViewport(Viewport viewport)
        {
            Viewport = viewport;
            _isDirty = true;
        }

        public void Move(Vector2 amount)
        {
            Position += amount;
            _isDirty = true;
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
            _isDirty = true;
        }

        public void AdjustZoom(float amount)
        {
            Zoom += amount;
            if (Zoom < 0.1f) Zoom = 0.1f; // Limită minimă zoom
            _isDirty = true;
        }
        public void SetZoom(float zoomLevel)
        {
            Zoom = MathHelper.Max(zoomLevel, 0.1f); // Asigură zoom minim
            _isDirty = true;
        }


        public void Rotate(float amountRadians)
        {
            Rotation += amountRadians;
            _isDirty = true;
        }
        public void SetRotation(float radians)
        {
            Rotation = radians;
            _isDirty = true;
        }

        // Urmărește o entitate
        public void Follow(Entity target)
        {
            Target = target;
            _targetTransform = target?.GetComponent<TransformComponent>();
            if (_targetTransform == null && target != null)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Camera cannot follow Entity {target.Id} because it lacks a TransformComponent.");
                Target = null; // Oprește urmărirea dacă nu are transform
            }
        }

        public void StopFollowing()
        {
            Target = null;
            _targetTransform = null;
        }


        public void Update(GameTime gameTime)
        {
            if (Target != null && _targetTransform != null)
            {
                // Calculează poziția dorită a camerei (centrul ecranului pe target)
                Vector2 targetPosition = _targetTransform.Position;
                Vector2 desiredPosition = targetPosition - new Vector2(Viewport.Width / 10f, Viewport.Height / 10f) / Zoom; // Ajustat pentru zoom

                // Interpolează lin către poziția dorită
                Vector2 newPosition = Vector2.Lerp(Position, desiredPosition, FollowLerpFactor);

                // Verifică dacă poziția s-a schimbat suficient pentru a recalcula matricea
                if (Vector2.DistanceSquared(Position, newPosition) > 0.01f) // Evită recalculări inutile
                {
                    Position = newPosition;
                    _isDirty = true;
                }
            }

            // Recalculează matricea dacă e necesar
            if (_isDirty)
            {
                RecalculateTransformMatrix();
            }
        }


        // Calculează matricea de transformare a camerei
        private void RecalculateTransformMatrix()
        {
            // Centrul viewport-ului, ajustat pentru zoom și rotație
            Vector2 origin = new Vector2(Viewport.Width / 2f, Viewport.Height / 2f); // Nu împărți la zoom aici

            _transformMatrix = Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *       // Translație inversă
                               Matrix.CreateRotationZ(Rotation) *                             // Rotație în jurul originii
                               Matrix.CreateScale(Zoom, Zoom, 1f) *                           // Scalare (Zoom)
                               Matrix.CreateTranslation(origin.X, origin.Y, 0);               // Mută originea înapoi în centru

            _isDirty = false; // Matricea este actualizată
        }

        // Returnează matricea de transformare calculată
        public Matrix GetViewMatrix()
        {
            if (_isDirty) // Asigură-te că e recalculată dacă s-a schimbat ceva înainte de draw
            {
                RecalculateTransformMatrix();
            }
            return _transformMatrix;
        }

        // Convertește coordonatele ecranului în coordonatele lumii
        public Vector2 ScreenToWorld(Point screenPosition)
        {
            return Vector2.Transform(screenPosition.ToVector2(), Matrix.Invert(GetViewMatrix()));
        }

        // Convertește coordonatele lumii în coordonatele ecranului
        public Point WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, GetViewMatrix()).ToPoint();
        }
    }
}