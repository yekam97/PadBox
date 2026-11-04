// GenSonidos: crea el pack de sonidos ORIGINALES "Drum Pad EDM" (126 BPM, Sol menor) en Sonidos\EDM.
// Todo se sintetiza aquí (osciladores, ruido y filtros); no usa ni copia sonidos de nadie.
// Compilar y ejecutar: csc /nologo /out:GenSonidos.exe GenSonidos.cs  &&  GenSonidos.exe
using System;
using System.Collections.Generic;
using System.IO;

static class Gen
{
    const int SR = 44100;
    const int Step = 5250;            // semicorchea a 126 BPM (44100 * 60 / 126 / 4)
    const int Bar = Step * 16, Loop = Bar * 2;
    static readonly Random Rnd = new Random(126);
    const double TwoPi = Math.PI * 2;

    static double Hz(double midi) { return 440 * Math.Pow(2, (midi - 69) / 12.0); }

    // Progresión de 2 compases (un acorde cada 2 tiempos): Mib - Sib - Fa - Solm
    static readonly int[][] Chords = { new[] { 63, 67, 70 }, new[] { 62, 65, 70 }, new[] { 60, 65, 69 }, new[] { 62, 67, 70 } };
    static readonly int[] Roots = { 39, 34, 41, 43 };   // Mib2 Sib1 Fa2 Sol2
    const int ChordLen = Step * 8;

    // Suma src en buf desde "at"; en los bucles la cola da la vuelta al inicio para que encajen sin cortes
    static void Mix(float[] buf, int at, float[] src, double gain, bool wrap = true)
    {
        for (int i = 0; i < src.Length; i++)
        {
            int j = at + i;
            if (wrap) j %= buf.Length; else if (j >= buf.Length) break;
            buf[j] += (float)(src[i] * gain);
        }
    }

    // ---------- filtros ----------
    class Svf   // filtro de estado variable (Chamberlin)
    {
        double low, band; readonly double q;
        public Svf(double resonance) { q = 1.0 / resonance; }
        public double Low, Band, High;
        public void Run(double x, double cutoff)
        {
            double f = 2 * Math.Sin(Math.PI * Math.Min(cutoff, SR / 6.0) / SR);
            low += f * band; High = x - low - q * band; band += f * High;
            Low = low; Band = band;
        }
    }

    // ---------- batería ----------
    static float[] Kick()
    {
        int n = (int)(SR * 0.42); var o = new float[n]; double ph = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / SR;
            ph += TwoPi * (46 + 110 * Math.Exp(-t * 32)) / SR;
            o[i] = (float)(Math.Sin(ph) * Math.Exp(-t * 6.5) + (i < 240 ? (Rnd.NextDouble() * 2 - 1) * 0.25 * (1 - i / 240.0) : 0));
        }
        return o;
    }
    static float[] NoiseHit(double dur, double decay, double cutoff, bool high, double res = 0.8)
    {
        int n = (int)(SR * dur); var o = new float[n]; var f = new Svf(res);
        for (int i = 0; i < n; i++)
        {
            f.Run(Rnd.NextDouble() * 2 - 1, cutoff);
            o[i] = (float)((high ? f.High : f.Band) * Math.Exp(-(double)i / SR * decay));
        }
        return o;
    }
    static float[] Clap()
    {
        var o = new float[(int)(SR * 0.3)];
        foreach (var d in new[] { 0, 420, 880 }) Mix(o, d, NoiseHit(0.25, d == 880 ? 14 : 60, 1400, false, 1.5), 0.9, false);
        return o;
    }
    static float[] Snare()
    {
        var o = NoiseHit(0.22, 20, 2500, false, 0.9); double ph = 0;
        for (int i = 0; i < o.Length; i++) { ph += TwoPi * 190 / SR; o[i] += (float)(Math.Sin(ph) * 0.6 * Math.Exp(-(double)i / SR * 35)); }
        return o;
    }
    static float[] Tom(double f0)
    {
        int n = (int)(SR * 0.35); var o = new float[n]; double ph = 0;
        for (int i = 0; i < n; i++) { double t = (double)i / SR; ph += TwoPi * f0 * (1 + 0.6 * Math.Exp(-t * 25)) / SR; o[i] = (float)(Math.Sin(ph) * Math.Exp(-t * 9)); }
        return o;
    }

    static float[] Bateria()
    {
        var b = new float[Loop]; var kick = Kick(); var clap = Clap(); var oh = NoiseHit(0.2, 18, 7000, true);
        for (int s = 0; s < 32; s += 4) Mix(b, s * Step, kick, 1.0);
        for (int s = 4; s < 32; s += 8) Mix(b, s * Step, clap, 0.7);
        for (int s = 2; s < 32; s += 4) Mix(b, s * Step, oh, 0.35);
        return b;
    }
    static float[] Hats()
    {
        var b = new float[Loop]; var ch = NoiseHit(0.05, 70, 8000, true); var shaker = NoiseHit(0.08, 40, 5000, false, 0.6);
        for (int s = 0; s < 32; s++) Mix(b, s * Step, ch, s % 4 == 2 ? 0.55 : s % 2 == 1 ? 0.3 : 0.18);
        for (int s = 1; s < 32; s += 2) Mix(b, s * Step + Step / 2, shaker, 0.18);
        return b;
    }
    static float[] Percusion()
    {
        var b = new float[Loop]; var hi = Tom(220); var lo = Tom(140); var rim = NoiseHit(0.04, 90, 3200, false, 4);
        int[] his = { 3, 11, 14, 19, 27, 30 }, los = { 6, 22, 31 }, rims = { 7, 15, 23, 26, 29 };
        foreach (var s in his) Mix(b, s * Step, hi, 0.45);
        foreach (var s in los) Mix(b, s * Step, lo, 0.5);
        foreach (var s in rims) Mix(b, s * Step, rim, 0.6);
        return b;
    }

    // ---------- bajos ----------
    static float[] Bajo()
    {
        var b = new float[Loop]; var lp = new Svf(0.9);
        for (int c = 0; c < 4; c++)
        {
            double f = Hz(Roots[c]); double ph = 0;
            for (int i = 0; i < ChordLen; i++)
            {
                double t = (double)i / SR; ph += f / SR;
                double saw = 2 * (ph % 1) - 1, sub = Math.Sin(TwoPi * ph);
                lp.Run(saw, 380 + 300 * Math.Exp(-((i % (Step * 2)) / (double)SR) * 12));
                double env = Math.Min(1, i / 80.0) * Math.Min(1, (ChordLen - i) / 300.0);
                b[c * ChordLen + i] += (float)((sub * 0.8 + lp.Low * 0.35) * env * Pump(c * ChordLen + i));
            }
        }
        return b;
    }
    static float[] BajoWub()
    {
        var b = new float[Loop]; var lp = new Svf(3.5);
        for (int c = 0; c < 4; c++)
        {
            double f = Hz(Roots[c] + 12); double ph1 = 0, ph2 = 0;
            for (int i = 0; i < ChordLen; i++)
            {
                ph1 += f / SR; ph2 += f * 1.007 / SR;
                double saw = (2 * (ph1 % 1) - 1) + (2 * (ph2 % 1) - 1);
                double lfo = 0.5 - 0.5 * Math.Cos(TwoPi * i / (Step * 2.0));   // "wub" cada corchea
                lp.Run(saw * 0.5, 150 + 2200 * lfo * lfo);
                double env = Math.Min(1, i / 60.0) * Math.Min(1, (ChordLen - i) / 300.0);
                b[c * ChordLen + i] += (float)(Math.Tanh(lp.Low * 2.2) * env);
            }
        }
        return b;
    }

    // ---------- acordes, pad, melodías ----------
    // Bombeo tipo "sidechain": baja el volumen en cada tiempo y lo recupera
    static double Pump(int i)
    {
        double sinceBeat = (i % (Step * 4)) / (double)SR;
        return 0.25 + 0.75 * Math.Min(1, Math.Pow(sinceBeat / 0.22, 1.6));
    }
    static float[] Acordes()
    {
        var b = new float[Loop]; double[] det = { -0.11, -0.045, 0, 0.05, 0.12 };
        for (int c = 0; c < 4; c++)
        {
            var lp = new Svf(0.8);
            var phases = new double[Chords[c].Length * det.Length];
            for (int i = 0; i < ChordLen; i++)
            {
                double s = 0; int k = 0;
                foreach (var note in Chords[c])
                    foreach (var d in det) { phases[k] += Hz(note + d) / SR; s += 2 * (phases[k] % 1) - 1; k++; }
                lp.Run(s / phases.Length, 2600);
                double env = Math.Min(1, i / 220.0) * Math.Min(1, (ChordLen - i) / 900.0);
                b[c * ChordLen + i] += (float)(lp.Low * env * Pump(c * ChordLen + i));
            }
        }
        return b;
    }
    static float[] Pad()
    {
        var b = new float[Loop];
        for (int c = 0; c < 4; c++)
        {
            var notes = new List<int>(Chords[c]) { Chords[c][0] + 12 };
            for (int i = 0; i < ChordLen; i++)
            {
                double t = (double)i / SR, s = 0;
                foreach (var n in notes) { double ph = Hz(n) * t; s += 1 - 4 * Math.Abs((ph % 1) - 0.5) - 0.0 + 0.4 * Math.Sin(TwoPi * Hz(n) * 1.003 * t); }
                double env = Math.Min(1, t / 0.35) * Math.Min(1, (ChordLen - i) / (SR * 0.3));
                b[c * ChordLen + i] += (float)(s / notes.Count * env);
            }
        }
        Echo(b, (int)(Step * 3), 0.35);
        return b;
    }
    static void Echo(float[] b, int delay, double fb)
    {
        var copy = (float[])b.Clone();
        for (int pass = 1; pass <= 4; pass++)
            for (int i = 0; i < b.Length; i++) b[(i + delay * pass) % b.Length] += (float)(copy[i] * Math.Pow(fb, pass));
    }
    static float[] Pluck(double f, double dur, double bright)
    {
        int n = (int)(SR * dur); var o = new float[n]; var lp = new Svf(1.2); double ph = 0, ph2 = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / SR; ph += f / SR; ph2 += f * 1.006 / SR;
            double saw = (2 * (ph % 1) - 1) + 0.6 * (2 * (ph2 % 1) - 1);
            lp.Run(saw, 500 + bright * Math.Exp(-t * 18));
            o[i] = (float)(lp.Low * Math.Exp(-t * 6) * Math.Min(1, i / 40.0));
        }
        return o;
    }
    static float[] Melodia()
    {
        var b = new float[Loop];
        // {paso (semicorcheas), nota}: frase pegadiza en Sol menor pentatónica
        int[,] seq = { {0, 74}, {3, 72}, {6, 70}, {8, 67}, {10, 70}, {12, 72}, {14, 74},
                       {16, 77}, {19, 74}, {22, 72}, {24, 70}, {26, 72}, {28, 67}, {30, 70} };
        for (int k = 0; k < seq.GetLength(0); k++) Mix(b, seq[k, 0] * Step, Pluck(Hz(seq[k, 1]), 0.5, 4500), 0.8);
        Echo(b, Step * 3, 0.3);
        return b;
    }
    static float[] Arpegio()
    {
        var b = new float[Loop];
        for (int s = 0; s < 32; s++)
        {
            var ch = Chords[s / 8]; int note = ch[(s % 4 == 3) ? 1 : s % 3] + 12;
            int n = (int)(SR * 0.14); var o = new float[n]; double f = Hz(note), ph = 0;
            for (int i = 0; i < n; i++) { ph += f / SR; o[i] = (float)(((ph % 1) < 0.5 ? 0.6 : -0.6) * Math.Exp(-(double)i / SR * 22)); }
            Mix(b, s * Step, o, s % 4 == 0 ? 0.6 : 0.4);
        }
        return b;
    }
    // Voz cortada sintética: oscilador con vibrato pasado por "formantes" de la vocal (a / o)
    static float[] Vocal(double f, double dur, bool a)
    {
        int n = (int)(SR * dur); var o = new float[n];
        double[] fr = a ? new[] { 800.0, 1150, 2900 } : new[] { 450.0, 800, 2830 }; double[] g = { 1, 0.6, 0.25 };
        var fs = new[] { new Svf(9), new Svf(9), new Svf(9) }; double ph = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / SR; ph += f * (1 + 0.012 * Math.Sin(TwoPi * 5.5 * t)) / SR;
            double src = 2 * (ph % 1) - 1, s = 0;
            for (int k = 0; k < 3; k++) { fs[k].Run(src, fr[k]); s += fs[k].Band * g[k]; }
            double env = Math.Min(1, t / 0.01) * Math.Min(1, (dur - t) / 0.03);
            o[i] = (float)(s * env);
        }
        return o;
    }
    static float[] Voz()
    {
        var b = new float[Loop];
        int[,] seq = { {0, 70, 3}, {4, 74, 2}, {6, 72, 2}, {8, 70, 4}, {14, 67, 2}, {16, 77, 3}, {20, 74, 2}, {22, 72, 2}, {24, 70, 4}, {30, 74, 2} };
        for (int k = 0; k < seq.GetLength(0); k++)
            Mix(b, seq[k, 0] * Step, Vocal(Hz(seq[k, 1]), seq[k, 2] * Step / (double)SR * 0.9, k % 2 == 0), 0.8);
        Echo(b, Step * 2, 0.25);
        return b;
    }

    // ---------- efectos (un solo golpe) ----------
    static float[] Subida()
    {
        int n = Loop; var o = new float[n]; var bp = new Svf(2.5); double ph = 0;
        for (int i = 0; i < n; i++)
        {
            double x = (double)i / n, f = 300 * Math.Pow(30, x);
            bp.Run(Rnd.NextDouble() * 2 - 1, f);
            ph += (200 + 1000 * x * x) / SR;
            o[i] = (float)((bp.Band * 0.8 + (2 * (ph % 1) - 1) * 0.25) * x * x);
        }
        return o;
    }
    static float[] Impacto()
    {
        int n = (int)(SR * 2.6); var o = new float[n]; var lp = new Svf(0.7); double ph = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / SR; ph += TwoPi * (28 + 60 * Math.Exp(-t * 6)) / SR;
            lp.Run(Rnd.NextDouble() * 2 - 1, 900);
            o[i] = (float)(Math.Sin(ph) * Math.Exp(-t * 1.6) + lp.Low * Math.Exp(-t * 5) * 0.8);
        }
        return o;
    }
    static float[] Redoble()
    {
        var o = new float[Loop + SR / 2]; var sn = Snare();
        var hits = new List<int>();
        for (int s = 0; s < 16; s += 2) hits.Add(s * Step);                    // corcheas
        for (int s = 16; s < 24; s++) hits.Add(s * Step);                      // semicorcheas
        for (int k = 0; k < 16; k++) hits.Add(24 * Step + k * Step / 2);       // fusas
        foreach (var at in hits) Mix(o, at, sn, 0.25 + 0.75 * at / (double)Loop, false);
        return o;
    }

    // ---------- salida ----------
    static void Save(string dir, string name, float[] s, double peak)
    {
        double max = 1e-9;
        foreach (var v in s) max = Math.Max(max, Math.Abs(v));
        double g = peak / max;
        using (var w = new BinaryWriter(File.Create(Path.Combine(dir, name))))
        {
            int bytes = s.Length * 2;
            w.Write("RIFF".ToCharArray()); w.Write(36 + bytes); w.Write("WAVE".ToCharArray());
            w.Write("fmt ".ToCharArray()); w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(SR); w.Write(SR * 2); w.Write((short)2); w.Write((short)16);
            w.Write("data".ToCharArray()); w.Write(bytes);
            foreach (var v in s) w.Write((short)Math.Max(-32767, Math.Min(32767, v * g * 32767)));
        }
        Console.WriteLine("{0,-18} {1,6:0.00} s", name, s.Length / (double)SR);
    }

    static void Main(string[] args)
    {
        var dir = Path.Combine(args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory, "Sonidos", "EDM");
        Directory.CreateDirectory(dir);
        Save(dir, "01 Bateria.wav", Bateria(), 0.8);
        Save(dir, "02 Hats.wav", Hats(), 0.6);
        Save(dir, "03 Percusion.wav", Percusion(), 0.6);
        Save(dir, "04 Bajo.wav", Bajo(), 0.75);
        Save(dir, "05 Bajo wub.wav", BajoWub(), 0.7);
        Save(dir, "06 Acordes.wav", Acordes(), 0.6);
        Save(dir, "07 Pad.wav", Pad(), 0.5);
        Save(dir, "08 Melodia.wav", Melodia(), 0.6);
        Save(dir, "09 Arpegio.wav", Arpegio(), 0.45);
        Save(dir, "10 Voz.wav", Voz(), 0.6);
        Save(dir, "11 Subida.wav", Subida(), 0.7);
        Save(dir, "12 Impacto.wav", Impacto(), 0.85);
        Save(dir, "13 Redoble.wav", Redoble(), 0.7);
        Console.WriteLine("Listo: " + dir);
    }
}
