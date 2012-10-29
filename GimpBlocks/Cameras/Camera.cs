using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public interface ICamera
    {
        void Reset();

        void SetViewParameters(Vector3 location, float yaw, float pitch, float roll);

        void SetViewParameters(Vector3 location, Vector3 lookAt);

        void SetProjectionParameters(float fieldOfView, float zoomLevel, float aspectRatio, float nearPlane, float farPlane);

        void SetClippingPlanes(float nearPlane, float farPlane);

        float ZoomLevel { get; set; }

        float Yaw { get; set; }

        float Pitch { get; set; }

        float Roll { get; set; }

        Vector3 Location { get; set; }

        Vector3 Up { get; }

        Vector3 LookAt { get; }

        void ChangeYaw(float amount);

        void ChangePitch(float amount);

        void MoveForwardHorizontally(float distance);

        void MoveBackwardHorizontally(float distance);

        void MoveLeft(float distance);

        void MoveRight(float distance);

        void MoveUp(float distance);

        void MoveDown(float distance);

        Matrix OriginBasedViewTransformation { get; }

        Matrix ProjectionTransformation { get; }

        BoundingFrustum OriginBasedViewFrustum { get; }
    }

    public class Camera : ICamera
    {
        // This is a pretty normal six-axis free-flight camera.  Left and right motion are relative
        // to the current camera facing but forward, backward, up, and down are always axis-aligned.

        // Attributes for view matrix
        Vector3 cameraLocation;
        float cameraYaw;
        float cameraPitch;
        float cameraRoll;
        public Vector3 Up { get; private set; }
        public Vector3 LookAt { get; private set; }
        Matrix originBasedViewMatrix;
        public const float MaximumPitch = (float) Math.PI / 2 - 0.01f;
        public const float MinimumPitch = (float) -Math.PI / 2 + 0.01f;

        // Attributes for projection matrix
        float fieldOfView = MathHelper.Pi / 4;
        float zoomLevel = 1.0f;
        float aspectRatio = 640f / 480f;
        float nearPlane = 0.01f;
        float farPlane = Settings.Instance.FarClippingPlaneDistance;
        Matrix projectionMatrix;

        // View frustum
        readonly BoundingFrustum originBasedViewFrustum = new BoundingFrustum(Matrix.Identity);

        public Camera()
        {
            Reset();
        }

        public void Reset()
        {
            if (Settings.Instance.CameraStartingLocation != Settings.Instance.CameraStartingLookAt)
            {
                SetViewParameters(Settings.Instance.CameraStartingLocation, Settings.Instance.CameraStartingLookAt);
            }
            else
            {
                SetViewParameters(Settings.Instance.CameraStartingLocation, 0.0f, 0.0f, 0.0f);
            }
            zoomLevel = 1.0f;
            
            SetProjectionParameters(fieldOfView, zoomLevel, aspectRatio, nearPlane, Settings.Instance.FarClippingPlaneDistance);
        }

        public void SetViewParameters(Vector3 location, Vector3 lookAt)
        {
            cameraLocation = location;

            // To convert from a look-at vector to Euler angles, we'll go
            // via a rotation matrix.  There may be a shorter way, but
            // this works.

            Matrix rotationMatrix = CreateOriginBasedLookAt(location, lookAt);
            Vector3 lookDirection = lookAt - location;

            if (IsStraightUp(lookDirection))
            {
                // singularity at north pole
                cameraYaw = (float)Math.Atan2(rotationMatrix.M13, rotationMatrix.M33);
                cameraPitch = MaximumPitch;
                cameraRoll = 0;
            }
            else if (IsStraightDown(lookDirection))
            {
                // singularity at south pole
                cameraYaw = (float)Math.Atan2(rotationMatrix.M13, rotationMatrix.M33);
                cameraPitch = MinimumPitch;
                cameraRoll = 0;
            }
            else
            {
                // Normal conversion
                cameraYaw = (float)Math.Atan2(-rotationMatrix.M31, rotationMatrix.M11);
                cameraPitch = (float)Math.Atan2(-rotationMatrix.M23, rotationMatrix.M22);
                cameraRoll = (float)Math.Asin(rotationMatrix.M21);
            }

            ClampPitch();

            UpdateView();
        }

        bool IsStraightUp(Vector3 vector)
        {
            return (vector.X == 0 && vector.Z == 0 && vector.Y > 0);
        }

        bool IsStraightDown(Vector3 vector)
        {
            return (vector.X == 0 && vector.Z == 0 && vector.Y < 0);
        }

        Matrix CreateOriginBasedLookAt(Vector3 location, Vector3 lookAt)
        {
            Matrix rotationMatrix = Matrix.CreateLookAt(Vector3.Zero, lookAt - location, Vector3.Up);
            return rotationMatrix;
        }

        public void SetViewParameters(Vector3 location, float yaw, float pitch, float roll)
        {
            cameraLocation = location;
            cameraYaw = yaw;
            cameraPitch = pitch;
            cameraRoll = roll;

            UpdateView();
        }

        public void SetProjectionParameters(float fieldOfView, float zoomLevel, float aspectRatio, float nearPlane, float farPlane)
        {
            // Set attributes for the projection matrix
            this.fieldOfView = fieldOfView;
            this.zoomLevel = zoomLevel;
            this.aspectRatio = aspectRatio;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;

            CreateProjectionMatrix();
        }

        public void SetClippingPlanes(float nearPlane, float farPlane)
        {
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;

            CreateProjectionMatrix();
        }

        public float ZoomLevel
        {
            get { return zoomLevel; }
            set
            {
                if (fieldOfView / value < MathHelper.Pi)
                {
                    zoomLevel = value;
                    CreateProjectionMatrix();
                }
            }
        }

        public Vector3 Location
        {
            get
            {
                return cameraLocation;
            }

            set
            {
                cameraLocation = value;
                UpdateView();
            }
        }

        public float Yaw
        {
            get
            {
                return cameraYaw;
            }

            set
            {
                cameraYaw = value;
                UpdateView();
            }
        }

        public float Pitch
        {
            get
            {
                return cameraPitch;
            }

            set
            {
                cameraPitch = value;
                ClampPitch();
                UpdateView();
            }
        }

        public float Roll
        {
            get
            {
                return cameraRoll;
            }

            set
            {
                cameraRoll = value;
                UpdateView();
            }
        }

        // This is the view matrix built as though the camera were at the origin rather than at
        // its world location.  This is useful because it lets us translate meshes to be drawn
        // only as much as they need to be correct relative to the camera, not as much as they'd
        // need to be in their actual world locations (which might blow the floats they're based on).
        public Matrix OriginBasedViewTransformation
        {
            get { return originBasedViewMatrix; }
        }

        public Matrix ProjectionTransformation
        {
            get { return projectionMatrix; }
        }

        public BoundingFrustum OriginBasedViewFrustum
        {
            get { return originBasedViewFrustum; }
        }

        protected void CreateProjectionMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView / zoomLevel, aspectRatio, nearPlane, farPlane);

            originBasedViewFrustum.Matrix = Matrix.Multiply(OriginBasedViewTransformation, ProjectionTransformation);
        }

        protected void UpdateView()
        {
            CreateViewMatrix();
            EventAggregator.Instance.SendMessage(new CameraMoved());
        }

        void CreateViewMatrix()
        {
            Matrix yawMatrix = Matrix.CreateRotationY(cameraYaw);
            Matrix pitchMatrix = Matrix.CreateRotationX(cameraPitch);
            Matrix rollMatrix = Matrix.CreateRotationZ(cameraRoll);

            // In order to get the proper mouse-look behavior, where the
            // camera's up vector is always in the plane of the Y axis,
            // we need to apply pitch then yaw.
            Vector3 cameraRotation = Vector3.Transform(Vector3.Forward, pitchMatrix);
            cameraRotation = Vector3.Transform(cameraRotation, yawMatrix);
            LookAt = cameraRotation;
            Up = Vector3.Transform(Vector3.UnitY, rollMatrix);

            // Now that we have a vector pointing towards where we want the camera to look,
            // create a view matrix to represent the total transformation.
            originBasedViewMatrix = Matrix.CreateLookAt(Vector3.Zero, LookAt, Up);

            // Update the view frustum since we changed the camera transforms
            originBasedViewFrustum.Matrix = OriginBasedViewTransformation * ProjectionTransformation;
        }

        public void ChangeYaw(float amount)
        {
            Yaw += amount;
            UpdateView();
        }

        public void ChangePitch(float amount)
        {
            Pitch += amount;
            UpdateView();
        }

        private void ClampPitch()
        {
            // Constrain the pitch away from singularities at the poles
            cameraPitch = Math.Min(cameraPitch, MaximumPitch);
            cameraPitch = Math.Max(cameraPitch, MinimumPitch);
        }

        public void MoveForwardHorizontally(float distance)
        {
            distance = -distance;
            var translationVector = new Vector3((float)(distance * Math.Sin(cameraYaw)), 0.0f, (float)(distance * Math.Cos(cameraYaw)));
            cameraLocation += translationVector;
            UpdateView();
        }

        public void MoveBackwardHorizontally(float distance)
        {
            var translationVector = new Vector3((float)(distance * Math.Sin(cameraYaw)), 0.0f, (float)(distance * Math.Cos(cameraYaw)));
            cameraLocation += translationVector;
            UpdateView();
        }

        public void MoveLeft(float distance)
        {
            distance = -distance;
            var translationVector = new Vector3((float)(distance * Math.Sin(cameraYaw + (Math.PI / 2))), 0.0f, (float)(distance * Math.Cos(cameraYaw + (Math.PI / 2))));
            cameraLocation += translationVector;
            UpdateView();
        }

        public void MoveRight(float distance)
        {
            var translationVector = new Vector3((float)(distance * Math.Sin(cameraYaw + (Math.PI / 2))), 0.0f, (float)(distance * Math.Cos(cameraYaw + (Math.PI / 2))));
            cameraLocation += translationVector;
            UpdateView();
        }

        public void MoveUp(float distance)
        {
            var translationVector = new Vector3(0.0f, distance, 0.0f);
            cameraLocation += translationVector;
            UpdateView();
        }

        public void MoveDown(float distance)
        {
            distance = -distance;
            var translationVector = new Vector3(0.0f, distance, 0.0f);
            cameraLocation += translationVector;
            UpdateView();
        }
    }
}
