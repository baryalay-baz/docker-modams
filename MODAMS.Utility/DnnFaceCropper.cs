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

        public string CropFace(string inputImagePath, string outputImagePath)
        {
            // Load DNN model
            var net = CvDnn.ReadNetFromCaffe(_prototxtPath, _caffeModelPath);

            using var src = Cv2.ImRead(inputImagePath);
            var blob = CvDnn.BlobFromImage(src, 1.0, new Size(300, 300), new Scalar(104, 177, 123), false, false);
            net.SetInput(blob);
            var detections = net.Forward();

            var cols = src.Cols;
            var rows = src.Rows;
            var confidenceThreshold = 0.5; // Confidence threshold

            // Loop through all detections
            for (int i = 0; i < detections.Size(2); i++)
            {
                float confidence = detections.At<float>(0, 0, i, 2);

                if (confidence > confidenceThreshold)
                {
                    int x1 = (int)(detections.At<float>(0, 0, i, 3) * cols);
                    int y1 = (int)(detections.At<float>(0, 0, i, 4) * rows);
                    int x2 = (int)(detections.At<float>(0, 0, i, 5) * cols);
                    int y2 = (int)(detections.At<float>(0, 0, i, 6) * rows);

                    // Expand box a bit
                    int margin = 100;
                    x1 = Math.Max(x1 - margin, 0);
                    y1 = Math.Max(y1 - margin, 0);
                    x2 = Math.Min(x2 + margin, src.Cols - 1);
                    y2 = Math.Min(y2 + margin, src.Rows - 1);

                    var rect = new Rect(x1, y1, x2 - x1, y2 - y1);
                    var croppedFace = new Mat(src, rect);

                    // Resize to 128x128
                    var resizedFace = new Mat();
                    Cv2.Resize(croppedFace, resizedFace, new Size(128, 128));

                    // Save the face
                    Cv2.ImWrite(outputImagePath, resizedFace);

                    return "Face cropped successfully.";
                }
            }

            return "No face detected.";
        }
    }
}
