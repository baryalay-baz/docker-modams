using OpenCvSharp;
using OpenCvSharp.Dnn;
using System.IO;

namespace MODAMS.Utility
{
    public class DnnFaceCropper
    {
        private readonly string _prototxtPath;
        private readonly string _caffeModelPath;

        public DnnFaceCropper(string prototxtPath, string caffeModelPath)
        {
            _prototxtPath = prototxtPath;
            _caffeModelPath = caffeModelPath;
        }

        public string CropFace(string inputImagePath, string outputImagePath, int outputSize = 128)
        {
            try
            {
                var net = CvDnn.ReadNetFromCaffe(_prototxtPath, _caffeModelPath);
                using var src = Cv2.ImRead(inputImagePath);
                if (src.Empty()) return "Image not found or invalid.";

                var blob = CvDnn.BlobFromImage(src, 1.0, new Size(300, 300), new Scalar(104, 177, 123), false, false);
                net.SetInput(blob);
                var detections = net.Forward();

                var cols = src.Cols;
                var rows = src.Rows;
                var confidenceThreshold = 0.5;

                for (int i = 0; i < detections.Size(2); i++)
                {
                    float confidence = detections.At<float>(0, 0, i, 2);
                    if (confidence > confidenceThreshold)
                    {
                        int x1 = (int)(detections.At<float>(0, 0, i, 3) * cols);
                        int y1 = (int)(detections.At<float>(0, 0, i, 4) * rows);
                        int x2 = (int)(detections.At<float>(0, 0, i, 5) * cols);
                        int y2 = (int)(detections.At<float>(0, 0, i, 6) * rows);

                        // Dynamically expand bounding box for ears, hair, etc.
                        int faceWidth = x2 - x1;
                        int faceHeight = y2 - y1;
                        int marginX = (int)(faceWidth * 0.6);   // 40% width margin
                        int marginY = (int)(faceHeight * 0.6);  // 60% height margin

                        x1 = Math.Max(x1 - marginX, 0);
                        y1 = Math.Max(y1 - marginY, 0);
                        x2 = Math.Min(x2 + marginX, cols - 1);
                        y2 = Math.Min(y2 + marginY, rows - 1);

                        var rect = new Rect(x1, y1, x2 - x1, y2 - y1);
                        if (rect.Width <= 0 || rect.Height <= 0)
                            return "Invalid crop area.";

                        var croppedFace = new Mat(src, rect);

                        // Preserve aspect ratio
                        float aspectRatio = (float)croppedFace.Width / croppedFace.Height;
                        int newWidth = outputSize;
                        int newHeight = (int)(outputSize / aspectRatio);

                        if (newHeight > outputSize)
                        {
                            newHeight = outputSize;
                            newWidth = (int)(outputSize * aspectRatio);
                        }

                        var resizedFace = new Mat();
                        Cv2.Resize(croppedFace, resizedFace, new Size(newWidth, newHeight));

                        // Create final square image with white padding
                        var finalImage = new Mat(new Size(outputSize, outputSize), MatType.CV_8UC3, Scalar.All(255));
                        int xOffset = (outputSize - newWidth) / 2;
                        int yOffset = (outputSize - newHeight) / 2;

                        resizedFace.CopyTo(new Mat(finalImage, new Rect(xOffset, yOffset, newWidth, newHeight)));

                        Cv2.ImWrite(outputImagePath, finalImage);

                        return "Face cropped successfully.";
                    }
                }

                return "No face detected.";
            }
            catch (Exception ex)
            {
                return $"Error cropping face: {ex.Message}";
            }
        }
    }
}