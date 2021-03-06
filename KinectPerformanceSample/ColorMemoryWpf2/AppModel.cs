﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using KLibrary.Labs.ObservableModel;
using Microsoft.Kinect;

namespace ColorMemoryWpf2
{
    public class AppModel
    {
        static readonly TimeSpan FramesInterval = TimeSpan.FromSeconds(1 / 30.0);
        static readonly KinectSensor Sensor = KinectSensor.GetDefault();
        static readonly BitmapInfo ColorBitmapInfo = BitmapInfo.ForColorBgra(Sensor.ColorFrameSource.FrameDescription);

        public IGetOnlyProperty<bool> IsAvailable { get; private set; }
        public IGetOnlyProperty<WriteableBitmap> ColorBitmap { get; private set; }

        public AppModel()
        {
            IsAvailable = Observable.FromEventPattern<IsAvailableChangedEventArgs>(Sensor, "IsAvailableChanged")
                .Select(_ => _.EventArgs.IsAvailable)
                .ToGetOnly(false);

            ColorBitmap = IsAvailable
                .ObserveOn(SynchronizationContext.Current)
                .Select(b => b ? ColorBitmapInfo.CreateBitmap() : null)
                .ToGetOnly(null);

            var colorReader = Sensor.ColorFrameSource.OpenReader();
            Sensor.Open();

            var data = new byte[ColorBitmapInfo.PixelBytesLength];
            Observable.Interval(FramesInterval)
                .Do(_ => colorReader.GetColorBgraData(data))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ => ColorBitmapInfo.WritePixels(ColorBitmap.Value, data));
        }
    }
}
