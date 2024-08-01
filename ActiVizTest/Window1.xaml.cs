using Kitware.VTK;
using System;
using System.Windows;

namespace ActiVizTest
{
    public partial class Window1 : Window
    {
        #region Değişkenler
        private int[] extent = new int[6];
        private double[] spacing = new double[3];
        private double[] origin = new double[3];
        private double[] center = new double[3];
        private double[] spa = new double[9];
        private vtkImageReslice[] reslices = new vtkImageReslice[3];
        private string path;
        private int[] indexNum = new int[3];
        private bool isStart = false;
        private vtkRenderer[] renderers = new vtkRenderer[3];
        private vtkLineSource[] lines = new vtkLineSource[6];
        private int[,,] linePos = new int[6,2,3];
        private int[] lastSizes = new int[6];
        private vtkTextActor[] numSlides = new vtkTextActor[3];
        private vtkImageMapper[] resliceColors = new vtkImageMapper[3];
        #endregion

        public Window1(string path)
        {
            this.path = path;
            InitializeComponent();
            Init(path);
            this.SizeChanged += MainWindow_SizeChanged;
        }

        #region Ekran Boyutu Değişimi
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int width = (int)Y_Slice.ActualWidth;
            int height = (int)Y_Slice.ActualHeight;
            int min = Math.Min(width, height);


            if (width > height)
            {
                int offset = (width - height) / 2;
                int ext = (extent[1] * min) / extent[5];
                reslices[0].SetOutputExtent(offset, ext + offset, 0, min, 0, 0);
                // x
                spa[0] = spacing[0]; // x
                spa[1] = spacing[1] * ext / extent[3]; // y
                spa[2] = spacing[2] * min / extent[5]; // z

                ext = (extent[3] * min) / extent[5];
                reslices[1].SetOutputExtent(offset, ext + offset, 0, min, 0, 0);
                // y
                spa[3] = spacing[0] / ext * extent[1]; // x
                spa[4] = spacing[1]; // y
                spa[5] = spacing[2] / min * extent[5]; // z

                ext = (extent[1] * min) / extent[3];
                reslices[2].SetOutputExtent(offset, ext + offset, 0, min, 0, 0);
                // z
                spa[6] = spacing[0] / ext * extent[1]; // x
                spa[7] = spacing[1] / min * extent[3]; // y
                spa[8] = spacing[2]; // z
            }

            reslices[0].SetOutputSpacing(spa[3], spa[5], 0);
            
            reslices[1].SetOutputSpacing(spa[1], spa[2], 0);
            
            reslices[2].SetOutputSpacing(spa[6], spa[7], 0);
            if (isStart)
            {
                for (int i = 0; i < 3; i++)
                {
                    linePos[i * 2, 0, 1] = linePos[i * 2, 0, 1] / (lastSizes[i * 2] / 2) * (height / 2);
                    linePos[i * 2, 1, 1] = linePos[i * 2, 1, 1] / (lastSizes[i * 2] / 2) * (height / 2);
                    if (width >= height)
                    {
                        int offset = (width - height) / 2;
                        lines[i * 2].SetPoint1(offset, linePos[i * 2, 0, 1], 0);
                        lines[i * 2].SetPoint2(offset + min, linePos[i * 2, 1, 1], 0);
                        linePos[i * 2, 0, 0] = offset;
                        linePos[i * 2, 1, 0] = offset + min;
                    }
                    else
                    {
                        int offset = (height - width) / 2;
                        lines[i * 2].SetPoint1(0, linePos[i * 2, 0, 1], 0);
                        lines[i * 2].SetPoint2(min, linePos[i * 2, 1, 1], 0);
                        linePos[i * 2, 0, 0] = 0;
                        linePos[i * 2, 1, 0] = min;
                    }
                    lines[i * 2].Update();
                    lastSizes[i * 2] = height;
                }
                for (int i = 0; i < 3; i++)
                {
                    linePos[i * 2 + 1, 0, 0] = linePos[i * 2 + 1, 0, 0] / (lastSizes[i * 2 + 1] / 2) * (width / 2);
                    linePos[i * 2 + 1, 1, 0] = linePos[i * 2 + 1, 1, 0] / (lastSizes[i * 2 + 1] / 2) * (width / 2);
                    if (width >= height)
                    {
                        int offset = (width - height) / 2;
                        lines[i * 2 + 1].SetPoint1(linePos[i * 2 + 1, 0, 0], 0, 0);
                        lines[i * 2 + 1].SetPoint2(linePos[i * 2 + 1, 1, 0], min, 0);
                        linePos[i * 2 + 1, 0, 1] = 0;
                        linePos[i * 2 + 1, 1, 1] = min;
                    }
                    else
                    {
                        int offset = (height - width) / 2;
                        lines[i * 2 + 1].SetPoint1(linePos[i * 2 + 1, 0, 0], offset, 0);
                        lines[i * 2 + 1].SetPoint2(linePos[i * 2 + 1, 1, 0], offset + min, 0);
                        linePos[i * 2 + 1, 0, 1] = offset;
                        linePos[i * 2 + 1, 1, 1] = offset + min;
                    }
                    lines[i * 2 + 1].Update();
                    lastSizes[i * 2 + 1] = width;
                }
            }
            else
            {
                isStart = true;
                InitAxesLines(width, height);
            }
        }
        #endregion

        #region VTK Başlatma Kısmı
        private void Init(string path)
        {
            vtkDICOMImageReader reader = new vtkDICOMImageReader();
            reader.SetDirectoryName(path);
            reader.Update();

            extent = reader.GetDataExtent();
            spacing = reader.GetDataSpacing();
            origin = reader.GetDataOrigin();

            center = new double[]
            {
                origin[0] + spacing[0] * 0.5 * (extent[0] + extent[1]),
                origin[1] + spacing[1] * 0.5 * (extent[2] + extent[3]),
                origin[2] + spacing[2] * 0.5 * (extent[4] + extent[5])
            };

            InitYReslice(reader.GetOutputPort());
            InitXReslice(reader.GetOutputPort());
            InitZReslice(reader.GetOutputPort());
            Init3D(reader.GetOutputPort());

            int max1 = extent[1] - extent[0] + 1;
            int max2 = extent[3] - extent[2] + 1;
            int max3 = extent[5] - extent[4] + 1;

            Y_Slider.Maximum = max1 / 2;
            X_Slider.Maximum = max2 / 2;
            Z_Slider.Maximum = max3 / 2;
            Y_Slider.Minimum = max1 / 2 * -1;
            X_Slider.Minimum = max2 / 2 * -1;
            Z_Slider.Minimum = max3 / 2 * -1;
            Y_Slider.Value = 0;
            X_Slider.Value = 0;
            Z_Slider.Value = 0;
        }
        #endregion

        #region Kesitlerin Başlatılması
        private void InitYReslice(vtkAlgorithmOutput output)
        {
            var renderWindowControl = new Kitware.VTK.RenderWindowControl();

            renderWindowControl.Load += (sender, args) =>
            {
                vtkImageReslice reslice = new vtkImageReslice();
                reslice.SetInputConnection(output);
                reslice.SetOutputDimensionality(2);
                reslice.SetResliceAxesDirectionCosines(1, 0, 0, 0, 0, 1, 0, 1, 0);
                reslice.SetResliceAxesOrigin(center[0], center[1], center[2]);

                reslices[0] = reslice;

                reslice.GetResliceTransform();

                vtkImageMapper mapper = new vtkImageMapper();
                mapper.SetInputConnection(reslice.GetOutputPort());
                mapper.SetColorWindow(1000);
                mapper.SetColorLevel(500);
                resliceColors[0] = mapper;

                vtkActor2D actor = new vtkActor2D();
                actor.SetMapper(mapper);

                vtkRenderer renderer = new vtkRenderer();
                renderer.AddActor(actor);
                renderer.SetBackground(0.0, 1.0, 0.0);
                renderers[0] = renderer;

                renderWindowControl.RenderWindow.AddRenderer(renderer);
            };
            Y_Slice.Child = renderWindowControl;
        }

        private void InitXReslice(vtkAlgorithmOutput output)
        {
            var renderWindowControl = new Kitware.VTK.RenderWindowControl();

            renderWindowControl.Load += (sender, args) =>
            {
                vtkImageReslice reslice = new vtkImageReslice();
                reslice.SetInputConnection(output);
                reslice.SetOutputDimensionality(2);
                reslice.SetResliceAxesDirectionCosines(0, 1, 0, 0, 0, 1, 1, 0, 0);
                reslice.SetResliceAxesOrigin(center[0], center[1], center[2]);

                reslices[1] = reslice;

                reslice.GetResliceTransform();

                vtkImageMapper mapper = new vtkImageMapper();
                mapper.SetInputConnection(reslice.GetOutputPort());
                mapper.SetColorWindow(1000);
                mapper.SetColorLevel(500);
                resliceColors[1] = mapper;

                vtkActor2D actor = new vtkActor2D();
                actor.SetMapper(mapper);

                vtkRenderer renderer = new vtkRenderer();
                renderer.AddActor(actor);
                renderer.SetBackground(1.0, 0.0, 0.0);
                renderers[1] = renderer;

                renderWindowControl.RenderWindow.AddRenderer(renderer);
            };
            X_Slice.Child = renderWindowControl;
        }

        private void InitZReslice(vtkAlgorithmOutput output)
        {
            var renderWindowControl = new Kitware.VTK.RenderWindowControl();

            renderWindowControl.Load += (sender, args) =>
            {
                vtkImageReslice reslice = new vtkImageReslice();
                reslice.SetInputConnection(output);
                reslice.SetOutputDimensionality(2);
                reslice.SetResliceAxesDirectionCosines(1, 0, 0, 0, 1, 0, 0, 0, 1);
                reslice.SetResliceAxesOrigin(center[0], center[1], center[2]);

                reslices[2] = reslice;

                reslice.GetResliceTransform();

                vtkImageMapper mapper = new vtkImageMapper();
                mapper.SetInputConnection(reslice.GetOutputPort());
                mapper.SetColorWindow(1000);
                mapper.SetColorLevel(500);
                resliceColors[2] = mapper;

                vtkActor2D actor = new vtkActor2D();
                actor.SetMapper(mapper);

                vtkRenderer renderer = new vtkRenderer();
                renderer.AddActor(actor);
                renderer.SetBackground(0.0, 0.0, 1.0);
                renderers[2] = renderer;

                renderWindowControl.RenderWindow.AddRenderer(renderer);

                
            };
            Z_Slice.Child = renderWindowControl;
        }
        #endregion

        #region 3D Başlatma
        private void Init3D(vtkAlgorithmOutput output)
        {
            var renderWindowControl = new Kitware.VTK.RenderWindowControl();

            renderWindowControl.Load += (sender, args) =>
            {
                vtkSmartVolumeMapper volumeMapper = new vtkSmartVolumeMapper();
                volumeMapper.SetInputConnection(output);

                vtkColorTransferFunction volumeColor = new vtkColorTransferFunction();
                volumeColor.AddRGBPoint(0, 0.0, 0.0, 0.0);
                volumeColor.AddRGBPoint(500, 0.5, 0.5, 1.0);
                volumeColor.AddRGBPoint(1000, 0.9, 0.5, 1.0);

                vtkPiecewiseFunction volumeOpacity = new vtkPiecewiseFunction();
                volumeOpacity.AddPoint(0, 0.00);
                volumeOpacity.AddPoint(200, 0.02);
                volumeOpacity.AddPoint(900, 0.05);
                volumeOpacity.AddPoint(1000, 1.0);

                vtkVolumeProperty volumeProperty = new vtkVolumeProperty();
                volumeProperty.SetColor(volumeColor);
                volumeProperty.SetScalarOpacity(volumeOpacity);
                volumeProperty.SetInterpolationTypeToLinear();
                volumeProperty.ShadeOn();
                volumeProperty.SetAmbient(0.4);
                volumeProperty.SetDiffuse(0.6);
                volumeProperty.SetSpecular(0.2);

                vtkVolume volume = new vtkVolume();
                volume.SetMapper(volumeMapper);
                volume.SetProperty(volumeProperty);

                vtkRenderer renderer = new vtkRenderer();
                renderer.AddVolume(volume);
                renderer.SetBackground(0.1, 0.2, 0.4);

                renderWindowControl.RenderWindow.AddRenderer(renderer);
                renderWindowControl.RenderWindow.Render();
            };
            ThreeD.Child = renderWindowControl;
        }
        #endregion

        #region Slider Kısmı
        private void Y_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            indexNum[0] = (int)e.NewValue;
            LayerSlices(0);
        }

        private void X_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            indexNum[1] = (int)e.NewValue;
            LayerSlices(1);
        }

        private void Z_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            indexNum[2] = (int)e.NewValue;
            LayerSlices(2);
        }

        private void LayerSlices(int option)
        {
            if (option == 0)
            {
                reslices[0].SetResliceAxesOrigin(center[0], center[1] + indexNum[0] * spa[4], center[2]); // buradaki spa değeri değiştirilecek
                reslices[0].Update();
                lines[4].SetPoint1(linePos[4, 0, 0], linePos[4, 0, 1] + indexNum[0] * spa[7], linePos[4, 0, 2]);
                lines[4].SetPoint2(linePos[4, 1, 0], linePos[4, 1, 1] + indexNum[0] * spa[7], linePos[4, 1, 2]);
                lines[4].Update();
                lines[3].SetPoint1(linePos[3, 0, 0] + indexNum[0] * spa[1], linePos[3, 0, 1], linePos[3, 0, 2]);
                lines[3].SetPoint2(linePos[3, 1, 0] + indexNum[0] * spa[1], linePos[3, 1, 1], linePos[3, 1, 2]);
                lines[3].Update();
            }
            if (option == 1)
            {
                reslices[1].SetResliceAxesOrigin(center[0] + indexNum[1] * spa[0], center[1], center[2]); // buradaki spa değeri değiştirilecek
                reslices[1].Update();
                lines[1].SetPoint1(linePos[1, 0, 0] + indexNum[1] * spa[3], linePos[1, 0, 1], linePos[1, 0, 2]); // buralar düzeltilecek
                lines[1].SetPoint2(linePos[1, 1, 0] + indexNum[1] * spa[3], linePos[1, 1, 1], linePos[1, 1, 2]); // burada kalındı
                lines[1].Update();
                lines[5].SetPoint1(linePos[5, 0, 0] + indexNum[1] * spa[6], linePos[5, 0, 1], linePos[5, 0, 2]);
                lines[5].SetPoint2(linePos[5, 1, 0] + indexNum[1] * spa[6], linePos[5, 1, 1], linePos[5, 1, 2]);
                lines[5].Update();
            }
            if (option == 2)
            {
                reslices[2].SetResliceAxesOrigin(center[0], center[1], center[2] + indexNum[2] * spa[8]); // buradaki spa değeri değiştirilecek
                reslices[2].Update();
                lines[0].SetPoint1(linePos[0, 0, 0], linePos[0, 0, 1] + indexNum[2] * spa[5], linePos[0, 0, 2]);
                lines[0].SetPoint2(linePos[0, 1, 0], linePos[0, 1, 1] + indexNum[2] * spa[5], linePos[0, 1, 2]);
                lines[0].Update();
                lines[2].SetPoint1(linePos[2, 0, 0], linePos[2, 0, 1] + indexNum[2] * spa[2], linePos[2, 0, 2]);
                lines[2].SetPoint2(linePos[2, 1, 0], linePos[2, 1, 1] + indexNum[2] * spa[2], linePos[2, 1, 2]);
                lines[2].Update();
            }
            Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child), null);
            X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child), null);
            Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child), null);
        }

        private void D_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)e.NewValue;
            resliceColors[0].SetColorWindow(val * 20);
            resliceColors[0].SetColorLevel(val * 10);
            resliceColors[1].SetColorWindow(val * 20);
            resliceColors[1].SetColorLevel(val * 10);
            resliceColors[2].SetColorWindow(val * 20);
            resliceColors[2].SetColorLevel(val * 10);
            Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Y_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Y_Slice.Child), null);
            X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(X_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(X_Slice.Child), null);
            Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child)?.GetType().GetMethod("Render")?.Invoke(Z_Slice.Child.GetType().GetProperty("RenderWindow")?.GetValue(Z_Slice.Child), null);
        }
        #endregion

        #region Kesitlerin Eksen Çizgileri Kısmı
        private void InitAxesLines(int width, int height)
        {
            int temp = Math.Min(width, height);
            // Y eksen çizgileri
            // Z Ekseni
            vtkLineSource yxLineSource = new vtkLineSource();
            if (width >= height)
            {
                int offset = (width - height) / 2;
                yxLineSource.SetPoint1(offset, height / 2, 0);
                yxLineSource.SetPoint2(offset + temp, height / 2, 0);
                linePos[0, 0, 0] = offset;
                linePos[0, 0, 1] = height / 2;
                linePos[0, 0, 2] = 0;
                linePos[0, 1, 0] = offset + temp;
                linePos[0, 1, 1] = height / 2;
                linePos[0, 1, 2] = 0;
            }
            else
            {
                int offset = (height - width) / 2;
                yxLineSource.SetPoint1(0, height / 2, 0);
                yxLineSource.SetPoint2(temp, height / 2, 0);
                linePos[0, 0, 0] = 0;
                linePos[0, 0, 1] = height / 2;
                linePos[0, 0, 2] = 0;
                linePos[0, 1, 0] = temp;
                linePos[0, 1, 1] = height / 2;
                linePos[0, 1, 2] = 0;
            }
            lastSizes[0] = height;

            vtkPolyDataMapper2D yxLineMapper = new vtkPolyDataMapper2D();
            yxLineMapper.SetInputConnection(yxLineSource.GetOutputPort());

            vtkActor2D yxLineActor = new vtkActor2D();
            yxLineActor.SetMapper(yxLineMapper);
            yxLineActor.GetProperty().SetColor(0.0, 0.0, 1.0);

            lines[0] = yxLineSource;
            renderers[0].AddActor(yxLineActor);

            // X Ekseni
            vtkLineSource yzLineSource = new vtkLineSource();
            if (width >= height)
            {
                int offset = (width - height) / 2;
                yzLineSource.SetPoint1(width / 2, 0, 0);
                yzLineSource.SetPoint2(width / 2, temp, 0);
                linePos[1, 0, 0] = width / 2;
                linePos[1, 0, 1] = 0;
                linePos[1, 0, 2] = 0;
                linePos[1, 1, 0] = width / 2;
                linePos[1, 1, 1] = temp;
                linePos[1, 1, 2] = 0;
            }
            else
            {
                int offset = (height - width) / 2;
                yzLineSource.SetPoint1(width / 2, offset, 0);
                yzLineSource.SetPoint2(width / 2, offset + temp, 0);
                linePos[1, 0, 0] = width / 2;
                linePos[1, 0, 1] = offset;
                linePos[1, 0, 2] = 0;
                linePos[1, 1, 0] = width / 2;
                linePos[1, 1, 1] = offset + temp;
                linePos[1, 1, 2] = 0;
            }
            lastSizes[1] = width;

            vtkPolyDataMapper2D yzLineMapper = new vtkPolyDataMapper2D();
            yzLineMapper.SetInputConnection(yzLineSource.GetOutputPort());

            vtkActor2D yzLineActor = new vtkActor2D();
            yzLineActor.SetMapper(yzLineMapper);
            yzLineActor.GetProperty().SetColor(1.0, 0.0, 0.0);

            lines[1] = yzLineSource;
            renderers[0].AddActor(yzLineActor);

            // X eksen çizgileri
            // Z Ekseni
            vtkLineSource xyLineSource = new vtkLineSource();
            if (width >= height)
            {
                int offset = (width - height) / 2;
                xyLineSource.SetPoint1(offset, height / 2, 0);
                xyLineSource.SetPoint2(offset + temp, height / 2, 0);
                linePos[2, 0, 0] = offset;
                linePos[2, 0, 1] = height / 2;
                linePos[2, 0, 2] = 0;
                linePos[2, 1, 0] = offset + temp;
                linePos[2, 1, 1] = height / 2;
                linePos[2, 1, 2] = 0;
            }
            else
            {
                int offset = (height - width) / 2;
                xyLineSource.SetPoint1(0, height / 2, 0);
                xyLineSource.SetPoint2(temp, height / 2, 0);
                linePos[2, 0, 0] = 0;
                linePos[2, 0, 1] = height / 2;
                linePos[2, 0, 2] = 0;
                linePos[2, 1, 0] = temp;
                linePos[2, 1, 1] = height / 2;
                linePos[2, 1, 2] = 0;
            }
            lastSizes[2] = height;

            vtkPolyDataMapper2D xyLineMapper = new vtkPolyDataMapper2D();
            xyLineMapper.SetInputConnection(xyLineSource.GetOutputPort());

            vtkActor2D xyLineActor = new vtkActor2D();
            xyLineActor.SetMapper(xyLineMapper);
            xyLineActor.GetProperty().SetColor(0.0, 0.0, 1.0);

            lines[2] = xyLineSource;
            renderers[1].AddActor(xyLineActor);

            // Y Ekseni
            vtkLineSource xzLineSource = new vtkLineSource();
            if (width >= height)
            {
                int offset = (width - height) / 2;
                xzLineSource.SetPoint1(width / 2, 0, 0);
                xzLineSource.SetPoint2(width / 2, temp, 0);
                linePos[3, 0, 0] = width / 2;
                linePos[3, 0, 1] = 0;
                linePos[3, 0, 2] = 0;
                linePos[3, 1, 0] = width / 2;
                linePos[3, 1, 1] = temp;
                linePos[3, 1, 2] = 0;
            }
            else
            {
                int offset = (height - width) / 2;
                xzLineSource.SetPoint1(width / 2, offset, 0);
                xzLineSource.SetPoint2(width / 2, offset + temp, 0);
                linePos[3, 0, 0] = width / 2;
                linePos[3, 0, 1] = offset;
                linePos[3, 0, 2] = 0;
                linePos[3, 1, 0] = width / 2;
                linePos[3, 1, 1] = offset + temp;
                linePos[3, 1, 2] = 0;
            }
            lastSizes[3] = width;

            vtkPolyDataMapper2D xzLineMapper = new vtkPolyDataMapper2D();
            xzLineMapper.SetInputConnection(xzLineSource.GetOutputPort());

            vtkActor2D xzLineActor = new vtkActor2D();
            xzLineActor.SetMapper(xzLineMapper);
            xzLineActor.GetProperty().SetColor(0.0, 1.0, 0.0);

            lines[3] = xzLineSource;
            renderers[1].AddActor(xzLineActor);

            // Z eksen çizgileri
            // Y Ekseni
            vtkLineSource zxLineSource = new vtkLineSource();
            if (width >= height)
            {
                int offset = (width - height) / 2;
                zxLineSource.SetPoint1(offset, height / 2, 0);
                zxLineSource.SetPoint2(offset + temp, height / 2, 0);
                linePos[4, 0, 0] = offset;
                linePos[4, 0, 1] = height / 2;
                linePos[4, 0, 2] = 0;
                linePos[4, 1, 0] = offset + temp;
                linePos[4, 1, 1] = height / 2;
                linePos[4, 1, 2] = 0;
            }
            else
            {
                int offset = (height - width) / 2;
                zxLineSource.SetPoint1(0, height / 2, 0);
                zxLineSource.SetPoint2(temp, height / 2, 0);
                linePos[4, 0, 0] = 0;
                linePos[4, 0, 1] = height / 2;
                linePos[4, 0, 2] = 0;
                linePos[4, 1, 0] = temp;
                linePos[4, 1, 1] = height / 2;
                linePos[4, 1, 2] = 0;
            }
            lastSizes[4] = height;

            vtkPolyDataMapper2D zxLineMapper = new vtkPolyDataMapper2D();
            zxLineMapper.SetInputConnection(zxLineSource.GetOutputPort());

            vtkActor2D zxLineActor = new vtkActor2D();
            zxLineActor.SetMapper(zxLineMapper);
            zxLineActor.GetProperty().SetColor(0.0, 1.0, 0.0);

            lines[4] = zxLineSource;
            renderers[2].AddActor(zxLineActor);

            // X Ekseni
            vtkLineSource zyLineSource = new vtkLineSource();
            if (width >= height)
            {
                int offset = (width - height) / 2;
                zyLineSource.SetPoint1(width / 2, 0, 0);
                zyLineSource.SetPoint2(width / 2, temp, 0);
                linePos[5, 0, 0] = width / 2;
                linePos[5, 0, 1] = 0;
                linePos[5, 0, 2] = 0;
                linePos[5, 1, 0] = width / 2;
                linePos[5, 1, 1] = temp;
                linePos[5, 1, 2] = 0;
            }
            else
            {
                int offset = (height - width) / 2;
                zyLineSource.SetPoint1(width / 2, offset, 0);
                zyLineSource.SetPoint2(width / 2, offset + temp, 0);
                linePos[5, 0, 0] = width / 2;
                linePos[5, 0, 1] = offset;
                linePos[5, 0, 2] = 0;
                linePos[5, 1, 0] = width / 2;
                linePos[5, 1, 1] = offset + temp;
                linePos[5, 1, 2] = 0;
            }
            lastSizes[5] = width;

            vtkPolyDataMapper2D zyLineMapper = new vtkPolyDataMapper2D();
            zyLineMapper.SetInputConnection(zyLineSource.GetOutputPort());

            vtkActor2D zyLineActor = new vtkActor2D();
            zyLineActor.SetMapper(zyLineMapper);
            zyLineActor.GetProperty().SetColor(1.0, 0.0, 0.0);

            lines[5] = zyLineSource;
            renderers[2].AddActor(zyLineActor);
        }
        #endregion

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}