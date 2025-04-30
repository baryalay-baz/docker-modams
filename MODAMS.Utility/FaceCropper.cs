using OpenCvSharp;
using System.IO;

namespace MODAMS.Utility
{
    public class FaceCropper
    {
        private readonly string _modelPath;

        public FaceCropper(string modelPath)
        {
            _modelPath = modelPath;
        }

        public string CropFace(string inputImagePath, string outputImagePath)
        {
            using var src = Cv2.ImRead(inputImagePath);

            var faceCascade = new CascadeClassifier(_modelPath);

            var faces = faceCascade.DetectMultiScale(image: src, scaleFactor: 1.05, minNeighbors: 3, flags: 0, minSize: new Size(30, 30));

            if (faces.Length == 0)
            {
                return "No face detected.";
            }

            // Pick the first face
            var faceRect = faces[0];

            // Add some margin (expand rectangle)
            int margin = 100; // you can adjust this value
            int x = Math.Max(faceRect.X - margin, 0);
            int y = Math.Max(faceRect.Y - margin, 0);
            int width = Math.Min(faceRect.Width + 2 * margin, src.Width - x);
            int height = Math.Min(faceRect.Height + 2 * margin, src.Height - y);

            var extendedFaceRect = new Rect(x, y, width, height);

            // Crop with extended rectangle
            var croppedFace = new Mat(src, extendedFaceRect);

            // Resize to 128x128
            var resizedFace = new Mat();
            Cv2.Resize(croppedFace, resizedFace, new Size(128, 128));

            // Save the resized face image
            Cv2.ImWrite(outputImagePath, resizedFace);

            return "Face cropped successfully.";
        }
    }
}
