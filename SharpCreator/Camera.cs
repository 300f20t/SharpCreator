using SharpDX;

namespace SharpCreator
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _target;
        private Vector3 _up;

        private Quaternion _rotation; // Используем Quaternion для вращения

        private float _moveSpeed = 1f; // Скорость перемещения камеры
        private float _rotationSpeed = 0.5f; // Скорость вращения камеры

        private Vector3 _forward; // Вектор направления вперед

        public Matrix ViewMatrix => Matrix.LookAtLH(_position, _target, _up);

        public Camera(Vector3 position, Vector3 target, Vector3 up)
        {
            _position = position;
            _target = target;
            _up = up;

            _rotation = Quaternion.Identity; // Изначально без вращения
            UpdateForwardVector();
        }

        private void UpdateForwardVector()
        {
            var rotationMatrix = Matrix.RotationQuaternion(_rotation);
            _forward = Vector3.TransformNormal(new Vector3(0, 0, 1), rotationMatrix);
        }

        // Вращение камеры на основе движения мыши
        public void Rotate(float deltaX, float deltaY)
        {
            var yawRotation = Quaternion.RotationAxis(_up, deltaX * _rotationSpeed);
            var pitchRotation = Quaternion.RotationAxis(Vector3.Cross(_forward, _up), -deltaY * _rotationSpeed);
            _rotation = yawRotation * _rotation;
            _rotation = pitchRotation * _rotation;

            // Ограничение вертикального поворота
            var pitch = (float)Math.Asin(MathUtil.Clamp((float)_rotation.Y, -1.0f, 1.0f));
            if (pitch < -MathF.PI / 2 + 0.01f || pitch > MathF.PI / 2 - 0.01f)
            {
                _rotation = Quaternion.RotationYawPitchRoll(_rotation.Y, pitch, 0);
            }

            UpdateForwardVector();
            _target = _position + _forward;
        }

        // Перемещение камеры вперёд/назад
        public void MoveForward(float amount)
        {
            var moveAmount = _forward * amount * _moveSpeed;
            _position += moveAmount;
            _target += moveAmount;
        }

        // Перемещение камеры влево/вправо
        public void Strafe(float amount)
        {
            var right = Vector3.Cross(_up, _forward); // Вычисление вектора вправо
            right.Normalize();
            var moveAmount = right * amount * _moveSpeed;
            _position += moveAmount;
            _target += moveAmount;
        }

        // Перемещение камеры вверх/вниз
        public void MoveUp(float amount)
        {
            var moveAmount = _up * amount * _moveSpeed;
            _position += moveAmount;
            _target += moveAmount;
        }

        // Установка скорости перемещения
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        // Установка скорости вращения
        public void SetRotationSpeed(float speed)
        {
            _rotationSpeed = speed;
        }
    }
}
