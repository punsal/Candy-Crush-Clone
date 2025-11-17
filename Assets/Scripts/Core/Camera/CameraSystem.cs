using Core.Camera.Abstract;
using Core.Camera.Provider.Interface;
using UnityEngine;

namespace Core.Camera
{
    /// <summary>
    /// Represents a camera system responsible for managing the positioning and sizing
    /// of the camera in relation to a game grid. Inherits foundational camera functionality
    /// from the <see cref="CameraSystemBase"/> class.
    /// </summary>
    public class CameraSystem : CameraSystemBase
    {
        public CameraSystem(ICameraProvider cameraProvider) : base(cameraProvider)
        {
            
        }
        
        /// <summary>
        /// Centers the camera on a grid based on the provided number of rows and columns.
        /// Adjusts the camera's position and orthographic size to fit the grid within the viewport.
        /// </summary>
        /// <param name="rows">The number of rows in the grid.</param>
        /// <param name="columns">The number of columns in the grid.</param>
        public override void CenterOnGrid(int rows, int columns)
        {
            if (Camera == null)
            {
                Debug.LogError("Camera is null, cannot center on grid");
                return;
            }
            
            var centerX = (columns - 1) / 2f;
            var centerY = -(rows - 1) / 2f;
        
            var cameraTransform = Camera.transform;
            cameraTransform.position = new Vector3(centerX, centerY, cameraTransform.position.z);
        
            var verticalSize = rows / 2f;
            var horizontalSize = columns / (2f * Camera.aspect);
        
            // Use the larger size and add padding (1 unit)
            Camera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) + 1f;
        }
    }
}