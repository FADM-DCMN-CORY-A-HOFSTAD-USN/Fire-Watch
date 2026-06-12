using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GenetecEdwardsBridge
{
    public class FireDetectionEngine
    {
        // Adjustable parameters for site tuning
        private const int RedThreshold = 190;        // Minimum intensity for the Red channel (0-255)
        private const double TriggerPercentage = 1.5; // Trigger alarm if more than 1.5% of the frame contains fire

        /// <summary>
        /// Analyzes a raw bitmap frame using color spectrum equations.
        /// Returns true if a fire trigger threshold is crossed.
        /// </summary>
        public unsafe bool AnalyzeFrameForFire(Bitmap frame, out double flameDensity)
        {
            flameDensity = 0.0;
            if (frame == null) return false;

            int width = frame.Width;
            int height = frame.Height;
            int totalPixels = width * height;
            int firePixelCount = 0;

            // Lock bitmap bits into system RAM for lightning-fast direct pointer processing
            BitmapData bitmapData = frame.LockBits(
                new Rectangle(0, 0, width, height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format24bppRgb
            );

            try
            {
                byte* currentLine = (byte*)bitmapData.Scan0;
                int bytesPerPixel = 3; // 24bpp format has 3 bytes per pixel (BGR)

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Extract individual BGR channels via byte pointer manipulation
                        int b = currentLine[x * bytesPerPixel];
                        int g = currentLine[x * bytesPerPixel + 1];
                        int r = currentLine[x * bytesPerPixel + 2];

                        // --- EQUATION 1: Standard RGB Fire Spectrum Thresholds ---
                        // Rule: Red must be greater than Green, which must be greater than Blue.
                        bool passesRgbRule = (r > g) && (g > b) && (r > RedThreshold);

                        if (passesRgbRule)
                        {
                            // --- EQUATION 2: YCbCr Space Mathematical Conversion ---
                            // Convert RGB to standard ITU-R BT.601 YCbCr values
                            double yVal  = (0.299 * r) + (0.587 * g) + (0.114 * b);
                            double cbVal = (-0.1687 * r) - (0.3313 * g) + (0.5 * b) + 128;
                            double crVal = (0.5 * r) - (0.4187 * g) - (0.0813 * b) + 128;

                            // Rule: Fire has high luminance (Y) and strong red chrominance (Cr) relative to blue (Cb)
                            bool passesChrominanceRule = (yVal >= cbVal) && (crVal >= cbVal);

                            if (passesChrominanceRule)
                            {
                                firePixelCount++;
                            }
                        }
                    }
                    // Move pointer forward to the start of the next scan line entry row
                    currentLine += bitmapData.Stride;
                }
            }
            finally
            {
                // Crucial: Unlock bits immediately to prevent OS memory leaks
                frame.UnlockBits(bitmapData);
            }

            // Calculate the density percentage of fire in the frame
            flameDensity = ((double)firePixelCount / totalPixels) * 100.0;

            // Return true if the calculated density crosses the plant engineering trigger threshold
            return flameDensity >= TriggerPercentage;
        }
    }
}
