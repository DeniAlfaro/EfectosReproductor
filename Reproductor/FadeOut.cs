using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Reproductor
{
    class FadeOut : ISampleProvider
    {
        private ISampleProvider fuente;
        private int muestrasLeidas = 0;
        private float segundosTranscurridos = 0;
        private float duracion;
        private float txtInicio;
        public WaveFormat WaveFormat
        {
            get
            {
                return fuente.WaveFormat;
            }
        }

        public FadeOut(ISampleProvider fuente, float duracion, float txtInicio)
        {
            this.fuente = fuente;
            this.duracion = duracion;
            this.txtInicio = txtInicio;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = fuente.Read(buffer, offset, count);
            muestrasLeidas += read;
            segundosTranscurridos = (float)muestrasLeidas / (float)fuente.WaveFormat.SampleRate / (float)fuente.WaveFormat.Channels;

            if (segundosTranscurridos <= txtInicio)
            {
                //Aplicar el efecto
                float factorEscala = 1- ((segundosTranscurridos - txtInicio) / duracion);
                for (int i = 1; i < read; i++)
                {
                    buffer[i + offset] *= factorEscala;
                }
            }
            return read;
        }
    }
}
