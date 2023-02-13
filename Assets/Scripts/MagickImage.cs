using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ImageMagick;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Drawing;

public class Magick
{
    private readonly ConcurrentQueue<SelectedImageData> _requestQueue;
    private readonly Thread _thread;
    private readonly AutoResetEvent _resetEvent;

    public Magick()
    {
        _requestQueue = new ConcurrentQueue<SelectedImageData>();
        _resetEvent = new AutoResetEvent(false);
        _thread = new Thread(ProcessingThread);
        _thread.Start();
    }

    //public static byte[] Resize(byte[] textureByteArray, int width, int height)
    //{
    //    byte[] data;
    //    using (MagickImage image = new MagickImage(textureByteArray))
    //    {
    //        image.Resize(width, height);
    //        data = image.ToByteArray(MagickFormat.Png);
    //    }
    //    return data;
    //}

    public void Process(SelectedImageData data)
    {
        _requestQueue.Enqueue(data);
        _resetEvent.Set();
    }

    static byte[] Resize(SelectedImageData data, int selectionWidth, int selectionHeight, int xPosition, int yPosition)
    {
        byte[] outputData;
        //Bitmap b;

        using (MagickImage image = new MagickImage(data.RawTextureData))
        {
            image.Crop(new MagickGeometry(xPosition, yPosition, selectionWidth, selectionHeight));
            image.RePage();
            image.Resize(data.TargetWidth, data.TargetHeight);
            outputData = image.ToByteArray(MagickFormat.Png);
        }
        return outputData;
    }

    void ProcessingThread()
    {
        while (true)
        {
            _resetEvent.WaitOne();

            while (_requestQueue.TryDequeue(out SelectedImageData dataToProcess))
            {
                int width = Mathf.RoundToInt(dataToProcess.TargetWidth * dataToProcess.SelectionScale);
                int height = Mathf.RoundToInt(dataToProcess.TargetHeight * dataToProcess.SelectionScale);
                int xPosition = Mathf.RoundToInt(dataToProcess.SelectionPosition.x) - width / 2;
                int yPosition = Mathf.RoundToInt(dataToProcess.SelectionPosition.y) - height / 2;

                var outputImage = Resize(dataToProcess, width, height, xPosition, yPosition);
                FileManager.SaveAsPNG(outputImage);
            }
        }
    }

    public struct SelectedImageData
    {
        //public Texture2D Texture;
        public byte[] RawTextureData;
        public int TargetWidth; 
        public int TargetHeight;
        public float SelectionScale;
        public Vector2 SelectionPosition;

        public SelectedImageData(byte[] rawTextureData, int targetWidth, int targetHeight, float selectionScale, Vector2 selectionPosition)
        {
            //Texture = texture;
            RawTextureData = rawTextureData;
            TargetWidth = targetWidth;
            TargetHeight = targetHeight;
            SelectionScale = selectionScale;
            SelectionPosition = selectionPosition;
        }
    }
}
