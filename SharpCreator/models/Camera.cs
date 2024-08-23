using SharpDX;

namespace SharpCreator
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _target;
        private Vector3 _up;

        private float _yaw;   // Угол поворота вокруг оси Y (горизонтальный поворот)
        private float _pitch; // Угол поворота вокруг оси X (вертикальный поворот)

        private float _moveSpeed = 1; // Скорость перемещения камеры
        private float _rotationSpeed = 1f; // Скорость вращения камеры

        public Matrix ViewMatrix => Matrix.LookAtLH(_position, _target, _up);

        public Camera(Vector3 position, Vector3 target, Vector3 up)
        {
            _position = position;
            _target = target;
            _up = up;

            _yaw = 0;
            _pitch = 0;
        }

        // Вращение камеры на основе движения мыши
        public void Rotate(float deltaX, float deltaY)
        {
            _yaw += deltaX * _rotationSpeed;
            _pitch += deltaY * _rotationSpeed;

            // Ограничение вертикального поворота
            _pitch = MathUtil.Clamp(_pitch, -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

            // Обновление направления на основе углов вращения
            var rotation = Matrix.RotationYawPitchRoll(_yaw, _pitch, 0);
            var direction = Vector3.TransformCoordinate(new Vector3(0, 0, 1), rotation);

            _target = _position + direction;
        }

        // Перемещение камеры вперёд/назад
        public void MoveForward(float amount)
        {
            var forward = Vector3.Normalize(_target - _position);
            _position += forward * amount * _moveSpeed;
            _target += forward * amount * _moveSpeed;
        }

        // Перемещение камеры влево/вправо
        public void Strafe(float amount)
        {
            var forward = Vector3.Normalize(_target - _position);
            var right = Vector3.Cross(_up, forward); // Вычисление вектора вправо
            right.Normalize();
            _position += right * amount * _moveSpeed;
            _target += right * amount * _moveSpeed;
        }

        // Перемещение камеры вверх/вниз
        public void MoveUp(float amount)
        {
            _position += _up * amount * _moveSpeed;
            _target += _up * amount * _moveSpeed;
        }
    }
}
