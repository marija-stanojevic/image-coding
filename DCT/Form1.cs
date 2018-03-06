using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace DCT
{
    public partial class Form1 : Form
    {
        double pi = 3.14;
        Bitmap bmp;
        byte[] data;
        sbyte[] sdata;
        double[,,,] cosm;
        int w, h;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "(*.bmp)|*.bmp";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName!=""){
                FileInfo fI=new FileInfo(openFileDialog1.FileName);
                bmp = new Bitmap(openFileDialog1.FileName);
                if (bmp.Height % 8 != 0) h = bmp.Height - bmp.Height % 8 + 8;
                else h=bmp.Height;
                if (bmp.Width % 8 != 0) w =bmp.Width - bmp.Width % 8 + 8;
                else w= bmp.Width;
                data = new byte[fI.Length];
                sdata = new sbyte[h*w];
                using (FileStream fs = fI.OpenRead())
                {
                    fs.Read(data, 0, data.Length);
                }
                for (int i = 0; i < h; i++)
                    for (int j = 0; j < w; j++)
                    {
                        if (i<bmp.Height && j<bmp.Width)
                        {
                            sdata[i*w+j] = (sbyte)(data[1078+i*bmp.Width+j] - 128);
                        }
                        else {
                            sdata[i*w+j]=0;
                        }                                               
                    }                       
                }
                pictureBox1.Image=bmp;
                bmp.Save("kodirano.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool[] kod= new bool[h * w * 8];
            int[] dct;
            Stopwatch sw=new Stopwatch();
            sw.Start();
            dct=new int[w*h];
            int pp = 0;
            try
            {
                pp = int.Parse(textBox1.Text);
            }
            catch {
                pp = 0;
            }
            cosm=new double[8,8,8,8];
            for (int ii = 0; ii < 8; ii++)
                for (int jj=0; jj<8; jj++)
                    for (int ik=0; ik<8; ik++)
                        for (int jk=0; jk<8; jk++)
                        {
                            cosm[ii, jj, ik, jk] = Math.Cos((2 * ik + 1) * ii * pi / 16.0) * Math.Cos((2 * jk + 1) * jj * pi / 16.0);
                        }
            for (int i = 0; i < w; i += 8)
                for (int j = 0; j < h; j += 8)
                    for (int ii = 0; ii < 8; ii++)
                        for (int jj = 0; jj < 8; jj++)
                        {
                            if (15 - ii - jj <= pp)
                            {
                                dct[(j + jj) * w + i + ii] = 0;
                            }
                            else
                            {
                                float c = 0;
                                double dd = 0;
                                if (ii == 0 && jj == 0) c = (float)0.5;
                                else if (ii != 0 && jj != 0) c = 1;
                                else c = (float)(1 / Math.Sqrt(2));
                                for (int ik = 0; ik < 8; ik++)
                                    for (int jk = 0; jk < 8; jk++)
                                    {
                                        dd += sdata[(j + jk) * w + i + ik] * cosm[ii, jj, ik, jk];
                                    }
                                dct[(j + jj) * w + i + ii] = (int)(c * dd / 4);
                            }
                        }
            for (int i = 0; i < 4; i++)
            {
                int t = pp;
                if (t % 2 == 0) kod[3 - i] = false;
                else kod[3 - i] = true;
                t /= 2;
            }
           int br=4;
           int nul = 0; 
           for (int i = 0; i < w; i += 8)
                for (int j = 0; j < h; j += 8)
                {
                    for (int ii = 0; ii < 8; ii++)
                        for (int jj = 0; jj < 8; jj++)
                        {
                            if (15 - ii - jj > pp)
                            {
                                if (ii + jj == 0)
                                {
                                    for (int k = 0; k < 10; k++)
                                    {
                                        int t = dct[(j + jj) * w + i + ii];
                                        if (t % 2 == 0) kod[11 + br - k] = false;
                                        else kod[11 + br - k] = true;
                                        t /= 2;
                                    }
                                    if (dct[(j + jj) * w + i + ii] > 0) kod[br] = false;
                                    else kod[br] = true;
                                    br += 11;
                                }
                                if (dct[(j + jj) * w + i + ii] == 0) nul++;
                                else
                                {
                                    if (nul > 0)
                                    {
                                        kod[br++] = false;
                                        for (int k = 0; k < 6; k++)
                                        {
                                            int t = nul;
                                            nul = 0;
                                            if (t % 2 == 0) kod[6 + br - k] = false;
                                            else kod[6 + br - k] = true;
                                            t /= 2;
                                        }
                                        br += 6;
                                        int dol = 0;
                                        int tt = dct[(j + jj) * w + i + ii];
                                        while (tt > 0)
                                        {
                                            dol++;
                                            tt /= 2;
                                        }
                                        dol++;
                                        for (int k = 0; k < dol-1; k++)
                                        {
                                            tt = dct[(j + jj) * w + i + ii];
                                            if (tt % 2 == 0) kod[dol + br - k] = false;
                                            else kod[dol + br - k] = true;
                                            tt /= 2;
                                        }
                                        if (dct[(j + jj) * w + i + ii] > 0) kod[br] = false;
                                        else kod[br] = true;
                                        br += dol;
                                    }
                                    else
                                    {
                                        kod[br++] = true;
                                        int dol = 0;
                                        int tt = dct[(j + jj) * w + i + ii];
                                        while (tt > 0)
                                        {
                                            dol++;
                                            tt /= 2;
                                        }
                                        dol++;
                                        for (int k = 0; k < dol; k++)
                                        {
                                            tt = dct[(j + jj) * w + i + ii];
                                            if (tt % 2 == 0) kod[dol + br - k] = false;
                                            else kod[dol + br - k] = true;
                                            tt /= 2;
                                        }
                                        if (dct[(j + jj) * w + i + ii] > 0) kod[br] = false;
                                        else kod[br] = true;
                                        br += dol;
                                    }
                                }
                            }
                        }
                    if (nul > 0)
                    {
                        kod[br++] = false;
                        for (int k = 0; k < 6; k++)
                        {
                            int t = nul;
                            nul = 0;
                            if (t % 2 == 0) kod[6 + br - k] = false;
                            else kod[6 + br - k] = true;
                            t /= 2;
                        }
                        br += 6;
                    }
                    nul = 0;
                }
            int tbr=br;
           if (br%8>0)
           {
               int t = br;
               for (int i=0; i<8-(t%8); i++)
               {
                   if (i==0) kod[br++]=true;
                   else kod[br++]=false;
               }
           }
           else {
               for (int i=0; i<8; i++)
               {
                   if (i==0) kod[br++]=true;
                   else kod[br++]=false;
               }           
           }
           byte[] kon = new byte[br / 8];
           for (int i = 0; i < br / 8; i++)
           {
                kon[i] = 0;
                for (int j = 0; j < 8; j++)
                {
                   kon[i] *= 2;
                   if (kod[i*8+j]) kon[i] += 1;
                }
            }
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string eT = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            richTextBox1.Text = "Čas kodiranja je: " + eT + "\n" + "Velikost originalne slike je: " + data.Length + " zlogov\n" + "Velikost shranjene slike je: " + kon.Length + " zlogov\n" + "Število bitov v kodiranoj slici je: " + tbr + "\n" + "Primerjava velikosti je: "+ 12.5*tbr/data.Length+" procentov";
            /* TypeConverter tr=TypeDescriptor.GetConverter(typeof(Bitmap));
            Bitmap bit1=(Bitmap)tr.ConvertFrom(kon);
            bit1.Save("kodirano.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            bit1.Save("kodirano.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);*/
            File.WriteAllBytes("kodirano.txt", kon);
        }
    }
}
