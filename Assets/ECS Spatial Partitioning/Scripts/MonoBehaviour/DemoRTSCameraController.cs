namespace ECS_Spatial_Partitioning.MonoBehaviour
{
    using UnityEngine;
    
    public class DemoRTSCameraController : MonoBehaviour
    {
        public new Camera camera;
        public float panSpeed = 20f;
        public float panBorderThickness = 20f;
        public float scrollSpeed = 20f;
        public float minZ = -250f;
        public float maxZ = -100f;
        public float zoomSpeedBase = 250f;
        public float zoomSpeedMultiplier = 10f;
        public int selectableMouseButton = 0; // 0: Left, 1: Right, 2: Middle

        // Boundaries of the playable area
        public float minX = -1000f;
        public float maxX = 1000f;
        public float minY = -1100f;
        public float maxY = 900f;


        private Vector3 _lastMousePos;
        private bool _isPanning = false;
        private float[] _deltaTimeBuffer = new float[5]; // Store the last few delta times
        private int _deltaIndex = 0;

        private void Start()
        {
            camera = Camera.main;
        }

        private void Update()
        {
            // Handle panning with mouse or touch input
            HandlePanning();

            // Handle arrow keys and WASD for movement
            HandleKeyboardMovement();

            // Handle zooming with the mouse wheel
            HandleZoom();

            // Clamp the camera position to playable area boundaries
            ClampCameraPosition();
        }

        private void HandlePanning()
        {
            if (Input.GetMouseButtonDown(selectableMouseButton))
            {
                _lastMousePos = Input.mousePosition;
                _isPanning = true;
            }

            if (Input.GetMouseButtonUp(selectableMouseButton)) _isPanning = false;

            if (!_isPanning)
                return;

            var delta = Input.mousePosition - _lastMousePos;

            // Calculate a smoothed delta time
            var smoothedDeltaTime = CalculateSmoothedDeltaTime(Time.deltaTime);

            var panX = -delta.x * CalculatePanSpeed() * smoothedDeltaTime;
            var panY = -delta.y * CalculatePanSpeed() * smoothedDeltaTime;

            // Apply panning considering the border thickness
            if (panX > 0 && Input.mousePosition.x < Screen.width - panBorderThickness)
                transform.Translate(Vector3.right * panX, Space.World);
            if (panX < 0 && Input.mousePosition.x > panBorderThickness)
                transform.Translate(Vector3.right * panX, Space.World);
            if (panY > 0 && Input.mousePosition.y < Screen.height - panBorderThickness)
                transform.Translate(Vector3.up * panY, Space.World);
            if (panY < 0 && Input.mousePosition.y > panBorderThickness)
                transform.Translate(Vector3.up * panY, Space.World);

            _lastMousePos = Input.mousePosition;
        }

        private float CalculateSmoothedDeltaTime(float currentDeltaTime)
        {
            // Store the current delta time in the buffer
            _deltaTimeBuffer[_deltaIndex] = currentDeltaTime;
            _deltaIndex = (_deltaIndex + 1) % _deltaTimeBuffer.Length;

            // Calculate the average of the last few delta times
            var sumDeltaTime = 0f;
            foreach (var t in _deltaTimeBuffer) sumDeltaTime += t;
            return sumDeltaTime / _deltaTimeBuffer.Length;
        }

        private float CalculatePanSpeed()
        {
            var zoomFactor = Mathf.InverseLerp(minZ, maxZ, transform.position.z);
            var adjustedPanSpeed = Mathf.Lerp(panSpeed, panSpeed * 0.25f, zoomFactor);
            return adjustedPanSpeed;
        }

        private void HandleKeyboardMovement()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            // Apply movement with arrow keys and WASD
            var direction = new Vector3(horizontal, vertical, 0);
            transform.Translate(direction * (panSpeed * Time.deltaTime), Space.World);
        }

        private void HandleZoom()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            var position = transform.position;
            var zoomSpeed = zoomSpeedBase + Mathf.Abs(position.z - minZ) * zoomSpeedMultiplier;

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            var zoomDistance = scroll * scrollSpeed * zoomSpeed * Time.deltaTime;
            var zoomDir = ray.direction * zoomDistance;

            var newPosition = position + zoomDir;

            //If we cannot zoom out or in, we don't want to change the camera position at all.
            if (newPosition.z > maxZ || newPosition.z < minZ)
            {
                newPosition.x = position.x;
                newPosition.y = position.y;
            }

            // Clamp the zoom to min and max values
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            // Apply zoom
            position = newPosition;
            transform.position = position;
            
            // Adjust orthographic size for orthographic cameras (fast approximation)
            if (camera.orthographic)
            {
                camera.orthographicSize = newPosition.z * -1 / 2;
            }
        }

        private void ClampCameraPosition()
        {
            var newPosition = transform.position;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = newPosition;
        }
    }
}
