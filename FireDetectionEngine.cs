using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace GenetecEdwardsBridge
{
    public class FireDetectionEngine
    {
        // Adjustable parameters for site tuning
        private const double TriggerPercentage = 1.5; // Trigger alarm if more than 1.5% of the frame contains fire
        
        // Path tracking to the NJIT / Multicore / CUDA Python processor script
        private readonly string _pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fire_cuda_engine.py");

        /// <summary>
        /// Extracts raw pixel matrices and streams them into the CUDA-accelerated Numba Python pipeline.
        /// Returns true if a fire trigger threshold condition is mathematically satisfied.
        /// </summary>
        public unsafe bool AnalyzeFrameForFire(Bitmap frame, out double flameDensity)
        {
            flameDensity = 0.0;
            if (frame == null) return false;

            int width = frame.Width;
            int height = frame.Height;

            // 1. Lock bitmap bits into system RAM for swift array translation
            BitmapData bitmapData = frame.LockBits(
                new Rectangle(0, 0, width, height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format24bppRgb
            );

            // Calculate exact size and extract raw unmanaged memory bytes into a managed array container
            int totalByteCount = bitmapData.Stride * height;
            byte[] rawImageBytes = new byte[totalByteCount];
            Marshal.Copy(bitmapData.Scan0, rawImageBytes, 0, totalByteCount);
            
            // Crucial: Unlock bits immediately to maintain strict life-safety memory efficiency and prevent OS leaks
            frame.UnlockBits(bitmapData);

            try
            {
                // 2. Configure an ultra-low overhead Python process wrapper
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python.exe",
                    // Pass dimensions as command line flags so the Python side knows exactly how to slice the incoming byte stream
                    Arguments = $"`"{_pythonScriptPath}`" --width {width} --height {height}",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        // 3. Directly stream the binary byte matrix over the pipeline stdin channel to bypass overhead constraints
                        using (BinaryWriter streamWriter = new BinaryWriter(process.StandardInput.BaseStream))
                        {
                            streamWriter.Write(rawImageBytes);
                            streamWriter.Flush();
                        }

                        // 4. Capture the calculated density scalar passed back by Numba/CUDA via stdout
                        string outputResult = process.StandardOutput.ReadLine();
                        
                        if (!string.IsNullOrWhiteSpace(outputResult) && double.TryParse(outputResult, out double parsedDensity))
                        {
                            flameDensity = parsedDensity;
                        }
                        else
                        {
                            // Capture standard execution error logs if Python outputs execution structural issues
                            string operationalErrors = process.StandardError.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(operationalErrors))
                            {
                                Trace.WriteLine($"CUDA Python processing loop stderr warning: {operationalErrors}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Critical hardware acceleration handoff pipeline failure: {ex.Message}");
                return false;
            }

            // 5. Evaluate if density percentage crosses the plant engineering trigger threshold boundaries
            return flameDensity >= TriggerPercentage;
        }
    }
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GenetecEdwardsBridge
{
    public class FireDetectionEngine
    {
        private const double TriggerPercentage = 1.5;
        private readonly string _pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fire_cuda_engine.py");

        public unsafe bool AnalyzeFrameForFire(Bitmap frame, out double flameDensity)
        {
            flameDensity = 0.0;
            if (frame == null) return false;

            int width = frame.Width;
            int height = frame.Height;

            // Extract the raw image matrix array bytes out of memory
            BitmapData bitmapData = frame.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] rawBytes = new byte[bitmapData.Stride * height];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, rawBytes, 0, rawBytes.Length);
            frame.UnlockBits(bitmapData);

            try
            {
                // Call Python engine via an optimized execution shell pipeline
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python.exe",
                    Arguments = $"\"{_pythonScriptPath}\" --width {width} --height {height}",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        // Direct Binary Write stream injection to avoid standard IO encoding delays
                        using (BinaryWriter writer = new BinaryWriter(process.StandardInput.BaseStream))
                        {
                            writer.Write(rawBytes);
                            writer.Flush();
                        }

                        // Capture calculated result string
                        string output = process.StandardOutput.ReadLine();
                        if (double.TryParse(output, out double density))
                        {
                            flameDensity = density;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Hardware acceleration handoff pipeline failure: {ex.Message}");
                return false;
            }

            return flameDensity >= TriggerPercentage;
        }
    }
}
