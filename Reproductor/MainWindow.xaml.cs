﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using System.Windows.Threading;

namespace Reproductor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        //Lector de archivos
        AudioFileReader reader;
        //Comunicacion con la tarjeta de audio exclusiva
        WaveOut output;

        bool dragging = false;
        //VolumeSampleProvider volume;
        EfectoVolumen efectoVolumen;
        FadeIn fadeIn;
        FadeOut fadeOut;
        EfectoDelay efectoDelay;

        public MainWindow()
        {
            InitializeComponent();
            ListarDispositivosSalida();
            btnReproducir.IsEnabled = false;
            btnDetener.IsEnabled = false;
            btnPausa.IsEnabled = false;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(fadeIn != null)
            {
                //lblMuestras.Text = fadeIn.segundosTranscurridos.ToString();
            }

            lblTiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);

            //para que recorra el slider
            if (!dragging)
            {
                sldTiempo.Value = reader.CurrentTime.TotalSeconds;
            }
        }

        void ListarDispositivosSalida()
        {
            cbDispositivoSalida.Items.Clear();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                WaveOutCapabilities capacidades = WaveOut.GetCapabilities(i);
                cbDispositivoSalida.Items.Add(capacidades.ProductName);
            }
            cbDispositivoSalida.SelectedIndex = 0;
        }
        
        private void BtnExaminar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txtRutaArchivo.Text = openFileDialog.FileName;
                btnReproducir.IsEnabled = true;
            }
        }

        private void BtnReproducir_Click(object sender, RoutedEventArgs e)
        {
            if(output != null && output.PlaybackState == PlaybackState.Paused)
            {
                //Retomo reproduccion
                output.Play();
                btnReproducir.IsEnabled = false;
                btnPausa.IsEnabled = true;
                btnDetener.IsEnabled = true;
            } else
            {
                if (txtRutaArchivo.Text != null && txtRutaArchivo.Text != string.Empty)
                {
                    reader = new AudioFileReader(txtRutaArchivo.Text);

                    /*volume = new VolumeSampleProvider(reader);
                    volume.Volume = (float)(sldVolumen.Value);*/

                    efectoDelay = new EfectoDelay(reader, (int)(sldOffsetDelay.Value));

                    float duracionFadeOut = float.Parse(txtDuracionFadeOut.Text);
                    float inicio = float.Parse(txtInicio.Text);
                    fadeOut = new FadeOut(reader, duracionFadeOut, inicio);

                    float duracionFadeIn = float.Parse(txtDuracion.Text);
                    fadeIn = new FadeIn(efectoDelay, duracionFadeIn);

                    efectoVolumen = new EfectoVolumen(fadeOut);
                    efectoVolumen.Volumen = (float)(sldVolumen.Value);

                    output = new WaveOut();
                    output.DeviceNumber = cbDispositivoSalida.SelectedIndex;

                    output.PlaybackStopped += Output_PlaybackStopped;
                    output.Init(efectoVolumen);
                    output.Play();

                   //Cambiar el volumen del output
                   // output.Volume = (float)(sldVolumen.Value);

                    btnReproducir.IsEnabled = false;
                    btnPausa.IsEnabled = true;
                    btnDetener.IsEnabled = true;

                    lblTiempoTotal.Text = reader.TotalTime.ToString().Substring(0, 8);

                    lblTiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);

                    //para que recorra el slider
                    sldTiempo.Maximum = reader.TotalTime.TotalSeconds;
                    sldTiempo.Value = reader.CurrentTime.TotalSeconds;

                    timer.Start();
                }
            }
        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            timer.Stop();

            reader.Dispose();
            output.Dispose();
        }

        private void BtnDetener_Click(object sender, RoutedEventArgs e)
        {
            if(output != null)
            {
                output.Stop();
                btnReproducir.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnDetener.IsEnabled = false;
            }
        }

        private void BtnPausa_Click(object sender, RoutedEventArgs e)
        {
            if(output != null)
            {
                output.Pause();
                btnReproducir.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnDetener.IsEnabled = true;
            }
        }

        private void SldTiempo_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragging = true;
        }

        private void SldTiempo_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            dragging = false;
            if(reader != null && output != null && output.PlaybackState != PlaybackState.Stopped)
            {
                reader.CurrentTime = TimeSpan.FromSeconds(sldTiempo.Value);
            }
        }

        private void SldVolumen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(output != null && output.PlaybackState != PlaybackState.Stopped)
            {
                efectoVolumen.Volumen = (float)(sldVolumen.Value);

                //output.Volume = (float)(sldVolumen.Value);
                //volume.Volume = (float)(sldVolumen.Value);
            }
        }

        private void SldOffsetDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblOffsetDelay.Text = ((int)(sldOffsetDelay.Value)).ToString(); 
            if (efectoDelay != null)
            {
                efectoDelay.OffsetMilisegundos = (int)(sldOffsetDelay.Value);
            }
        }

        private void SldGanancia_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(lblGananciaDelay != null)
            {
                lblGananciaDelay.Text = sldGanancia.Value.ToString("N");
            }
        }
    }
}
