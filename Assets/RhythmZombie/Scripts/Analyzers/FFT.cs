using System;
using UnityEngine;

namespace RhythmZombie.Scripts.Analyzers
{
    public static class FFT
    {
        private static void BitReverse(Complex[] buffer)
        {
            var n = buffer.Length;
            for (int i = 1, j = 0; i < n; i++)
            {
                var bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                {
                    j ^= bit;
                }
                j ^= bit;

                if (i < j)
                {
                    (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
                }
            }
        }

        private static void FFTInterval(Complex[] buffer, bool inverse)
        {
            var n = buffer.Length;
            if ((n & (n - 1)) != 0)
            {
                Debug.LogError("Размер массива должен быть степенью двойки");
                return;
            }

            BitReverse(buffer);

            for (int s = 1; s <= Mathf.Log(n, 2); s++)
            {
                var m = 1 << s;
                var angle = (inverse ? 1f : -1f) * 2f * Mathf.PI / m;
                var wm = new Complex(Mathf.Cos(angle), Mathf.Sin(angle));

                for (int k = 0; k < n; k += m)
                {
                    var w = Complex.One;
                    for (int j = 0; j < m / 2; j++)
                    {
                        var t = w * buffer[k + j + m / 2];
                        var u = buffer[k + j];
                        buffer[k + j] = u + t;
                        buffer[k + j + m / 2] = u - t;
                        w *= wm;
                    }
                }
            }
        }

        public static void Compute(float[] samples, Complex[] output, bool inverse = false)
        {
            var n = samples.Length;
            if (output.Length != n)
            {
                Debug.LogError($"Размеры массивов должны совпадать! Input: {n}, Output: {output.Length}");
                return;
            }

            for (int i = 0; i < n; i++)
                output[i] = new Complex(samples[i], 0);
            
            FFTInterval(output, inverse);

            if (inverse)
            {
                for (int i = 0; i < n; i++)
                    output[i] /= n;
            }
        }
    }

    [System.Serializable]
    public struct Complex
    {
        private static Complex _one = new Complex(1, 0);
        
        public float Real;
        public float Imaginary;

        public Complex(float real, float imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public static Complex One => _one;

        public float Magnitude => Mathf.Sqrt(Real * Real + Imaginary * Imaginary);

        public static Complex operator *(Complex a, Complex b) => new(a.Real * b.Real - a.Imaginary * b.Imaginary,
            a.Real * b.Imaginary + a.Imaginary * b.Real);

        public static Complex operator +(Complex a, Complex b) => new(a.Real + b.Real, a.Imaginary + b.Imaginary);

        public static Complex operator -(Complex a, Complex b) => new(a.Real - b.Real, a.Imaginary - b.Imaginary);

        public static Complex operator /(Complex a, float divisor) => new(a.Real / divisor, a.Imaginary / divisor);
    }
}