using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace neuroWeb_2._0
{
    public partial class Form1 : Form
    {
        public const int HiddenNeuronsCount = 23;
        public const int outputNeuronsCount = 10;
        public const int pictureWidth = 30;
        public const int pictureHeight = 50;
        public const double learnSpeed = 0.7;
        public const double moment = 0.3;
        public const double e = 2.71828182845;
        public double maxerror = 1;
        public double[,] input = new double[pictureWidth, pictureHeight];
        public double[,] publicSecondWeights = new double[outputNeuronsCount, HiddenNeuronsCount];
        public double[,,] publweights = new double[HiddenNeuronsCount, pictureHeight, pictureWidth];
        public double[] NeuronOutput = new double[HiddenNeuronsCount];
        public double[] outOutput = new double[outputNeuronsCount];
        public double[] error = new double[outputNeuronsCount];
        public double[] deltOut = new double[outputNeuronsCount];
        public double[] deltHidden = new double[HiddenNeuronsCount];
        public double[,] deltWHO = new double[outputNeuronsCount, HiddenNeuronsCount];
        public double[,,] deltWIH = new double[HiddenNeuronsCount, pictureHeight, pictureWidth];
        public double[,,] previousDeltWIH = new double[HiddenNeuronsCount, pictureHeight, pictureWidth];
        public double[,] previousDeltWHO = new double[outputNeuronsCount, HiddenNeuronsCount];
        public int[] ideal = new int[outputNeuronsCount];
        public int neuroCount = 1;
        public string pictureName;
        Bitmap bmp = new Bitmap(150, 250);
        bool paint = false;
        SolidBrush color;

        public Form1()
        {
            InitializeComponent();
            progressBar3.Visible = false;
        }

        void autolearning(int num,double[,] pixels)
        {


            Ideal(num);
            maxerror = 1;
            while (maxerror > 0.2)
            {


                neuronSum(pixels);
                outInput();
                Error();
                deltOutput();
                gradientOut();
                DeltHidden();
                gradientHidden(pixels);
                neuronSum(pixels);
                outInput();
                Error();
                Maximum();
            }




        }
        void Maximum()
        {
            maxerror = 0;
            for (int i = 0; i < outputNeuronsCount; i++)
            {
                if (Math.Abs(error[i]) > maxerror) maxerror = Math.Abs(error[i]);
            }
        }
        void writeWeights()
        {
            //writing first weights
            File.WriteAllText("weights.csv", "");
            string wght;
            progressBar3.Visible = true;

            progressBar3.Value = 1;
            progressBar3.Step = 1;
            progressBar3.Minimum = 1;
            progressBar3.Maximum = HiddenNeuronsCount;

            for (int i = 0; i <= HiddenNeuronsCount - 1; i++)
            {
                for (int j = 0; j <= pictureHeight - 1; j++)
                {

                    for (int m = 0; m <= pictureWidth - 1; m++)
                    {


                        wght = Convert.ToString(publweights[i, j, m]);

                        if (m == pictureWidth - 1)
                        {
                            if (j != pictureHeight - 1) File.AppendAllText("weights.csv", wght + ";");
                            else break;
                        }

                        File.AppendAllText("weights.csv", wght + ",");
                    }

                }
                File.AppendAllText("weights.csv", Environment.NewLine);
                progressBar3.PerformStep();
            }
            //writing second weights
            File.WriteAllText("secondWeights.csv", "");
            string weght;


            double[,] weights = new double[outputNeuronsCount, HiddenNeuronsCount];
            for (int j = 0; j < outputNeuronsCount; j++)
            {

                for (int m = 0; m < HiddenNeuronsCount; m++)
                {


                    weght = Convert.ToString(publicSecondWeights[j, m]);

                    if (m == HiddenNeuronsCount - 1)
                    {
                        File.AppendAllText("secondWeights.csv", weght);
                        break;
                    }


                    File.AppendAllText("secondWeights.csv", weght + ",");
                }
                File.AppendAllText("secondWeights.csv", Environment.NewLine);
            }
        }
        void secondRandomizeWeights()
        {
            File.WriteAllText("secondWeights.csv", "");
            string wght;
            Random randomWeight = new Random();
            double[,] weights = new double[outputNeuronsCount, HiddenNeuronsCount];
            for (int j = 0; j < outputNeuronsCount; j++)
            {

                for (int m = 0; m < HiddenNeuronsCount; m++)
                {
                    weights[j, m] = randomWeight.Next(0, 10) * 0.001;

                    wght = Convert.ToString(weights[j, m]);

                    if (m == HiddenNeuronsCount - 1)
                    {
                        File.AppendAllText("secondWeights.csv", wght);
                        break;
                    }


                    File.AppendAllText("secondWeights.csv", wght + ",");
                }
                File.AppendAllText("secondWeights.csv", Environment.NewLine);
            }


        }
        void randomizeWeights()
        {
            File.WriteAllText("weights.csv", "");
            string[] wght = new string[pictureHeight * pictureWidth * HiddenNeuronsCount];
            int z = 0;
            Random randomWeight = new Random();
            double[,,] weights = new double[HiddenNeuronsCount, pictureHeight, pictureWidth];
            for (int i = 0; i < HiddenNeuronsCount; i++)
            {
                for (int j = 0; j < pictureHeight; j++)
                {

                    for (int m = 0; m < pictureWidth; m++)
                    {
                        weights[i, j, m] = randomWeight.Next(0, 10) * 0.001;

                        wght[z] = Convert.ToString(weights[i, j, m]);

                        if (m == pictureWidth - 1)
                        {
                            if (j != pictureHeight - 1)
                            {
                                File.AppendAllText("weights.csv", wght[z] + ";");
                                break;
                            }
                            else
                            {
                                File.AppendAllText("weights.csv", wght[z]);
                                break;
                            }
                        }

                        File.AppendAllText("weights.csv", wght[z] + ",");
                    }

                }
                File.AppendAllText("weights.csv", Environment.NewLine);
            }
        }
        void readWeights()
        {
            double[,,] weights = new double[HiddenNeuronsCount, pictureHeight, pictureWidth];

            string[] tripleWeights = new string[pictureWidth];
            string[] doubleWeights = new string[pictureHeight];
            string[] readText = File.ReadAllLines("weights.csv");


            for (int i = 0; i <= HiddenNeuronsCount - 1; i++)
            {
                doubleWeights = readText[i].Split(new char[] { ';' });
                for (int j = 0; j <= pictureHeight - 1; j++)
                {

                    tripleWeights = doubleWeights[j].Split(new char[] { ',' });
                    for (int m = 0; m <= pictureWidth - 1; m++)
                    {

                        weights[i, j, m] = Convert.ToDouble(tripleWeights[m]);
                        publweights[i, j, m] = weights[i, j, m];

                    }
                }

            }
        }
        void neuronSum(double [,] pixels)
        {

            double[] NeuronInput = new double[HiddenNeuronsCount];
            for (int i = 0; i <= HiddenNeuronsCount - 1; i++)
            {
                for (int j = 0; j <= pictureHeight - 1; j++)
                {
                    for (int m = 0; m <= pictureWidth - 1; m++)
                    {
                        NeuronInput[i] = NeuronInput[i] + publweights[i, j, m] * pixels[m, j];

                    }

                }

                NeuronOutput[i] = sigmo(NeuronInput[i]);
            }

        }
        void readSecondWeights()
        {
            string[] readText = File.ReadAllLines("secondWeights.csv");
            string[] everyWeight = new string[HiddenNeuronsCount];

            for (int j = 0; j < outputNeuronsCount; j++)
            {

                everyWeight = readText[j].Split(new char[] { ',' });
                for (int m = 0; m < HiddenNeuronsCount; m++)
                {

                    publicSecondWeights[j, m] = Convert.ToDouble(everyWeight[m]);

                }
            }

        }
        void outInput()
        {
            double[] OutSum = new double[outputNeuronsCount];

            for (int j = 0; j < outputNeuronsCount; j++)
            {
                OutSum[0] = 0;
                for (int m = 0; m < HiddenNeuronsCount; m++)
                {
                    OutSum[j] = OutSum[j] + NeuronOutput[m] * publicSecondWeights[j, m];

                }
                outOutput[j] = sigmo(OutSum[j]);
            }


        }
        void Error()
        {
            for (int i = 0; i < outputNeuronsCount; i++)
            {
                error[i] = ideal[i] - outOutput[i];

            }

        }
        void deltOutput()
        {

            for (int i = 0; i < outputNeuronsCount; i++)
            {
                deltOut[i] = error[i] * (1 - outOutput[i]) * outOutput[i];
            }
        }
        void DeltHidden()
        {
            double sum = 0;
            for (int i = 0; i < HiddenNeuronsCount; i++)
            {
                for (int j = 0; j < outputNeuronsCount; j++)
                {
                    sum = sum + publicSecondWeights[j, i] * deltOut[j];
                }
                deltHidden[i] = ((1 - NeuronOutput[i]) * NeuronOutput[i]) * sum;
                sum = 0;
            }
        }
        void gradientHidden(double [,] pixels)
        {
            double[,,] gradientIH = new double[HiddenNeuronsCount, pictureHeight, pictureWidth];
            for (int i = 0; i < HiddenNeuronsCount; i++)
            {
                for (int j = 0; j < pictureHeight; j++)
                {
                    for (int m = 0; m < pictureWidth; m++)
                    {
                        gradientIH[i, j, m] = pixels[m, j] * deltHidden[i];
                        deltWIH[i, j, m] = learnSpeed * gradientIH[i, j, m] + (moment * previousDeltWIH[i, j, m]);
                        previousDeltWIH[i, j, m] = deltWIH[i, j, m];
                        publweights[i, j, m] = publweights[i, j, m] + deltWIH[i, j, m];
                    }

                }
            }
        }
        void gradientOut()
        {
            double[,] gradientHO = new double[outputNeuronsCount, HiddenNeuronsCount];
            for (int i = 0; i < outputNeuronsCount; i++)
            {
                for (int j = 0; j < HiddenNeuronsCount; j++)
                {
                    gradientHO[i, j] = NeuronOutput[j] * deltOut[i];
                    deltWHO[i, j] = learnSpeed * gradientHO[i, j] + (moment * previousDeltWHO[i, j]);
                    previousDeltWHO[i, j] = deltWHO[i, j];
                    publicSecondWeights[i, j] = publicSecondWeights[i, j] + deltWHO[i, j];
                }
            }

        }
        void Ideal(int num)
        {
            for (int i = 0; i < outputNeuronsCount; i++)
            {
                if (i == num) ideal[i] = 1;
                else ideal[i] = 0;
            }
        }

        double sigmo(double x)
        {
            double y;

            y = 1 / (1 + Math.Pow(e, -x));
            return y;
        }
        double Input(int x, int y)
        {
            Bitmap im = pictureBox1.Image as Bitmap;

            double n = (im.GetPixel(x, y).R)/255;
            

            input[x, y] = n;
            return input[x, y];


        }

        private void autolearningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = pictureBox1.Image as Bitmap;
            double[,] pixels = new double[pictureWidth, pictureHeight];
            
            int maximum=300;
            readWeights();
            readSecondWeights();
            progressBar1.Step = 1;
            progressBar1.Value = 1;
            progressBar2.Value = 1;
            progressBar1.Minimum = 1;
            progressBar1.Maximum = 30;
            progressBar2.Step = 1;
            progressBar2.Minimum = 1;
            progressBar2.Maximum = maximum;
            for (int k = 0; k < maximum-1; k++)
            {
                for (int j = 1; j < 4; j++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        pictureBox1.Image = Image.FromFile("pictures_for_learning\\" + i + "_" + j + ".bmp");
                        bitmap = pictureBox1.Image as Bitmap;
                        string filename = Path.GetFileName("pictures_for_learning\\" + i + "_" + j + ".bmp");
                        string number = filename.Substring(0, 1);
                        int num = Convert.ToInt32(number);
                        for (int p = 0; p < pictureWidth; p++)
                        {
                            for (int s = 0; s < pictureHeight; s++)
                            {
                                pixels[p, s] = (255.0 - (bitmap.GetPixel(p, s).R)) / 255.0;
                            }
                        }
                        autolearning(num,pixels);
                        progressBar1.PerformStep();
                    }
                }
                progressBar1.Value=1;
                progressBar2.PerformStep();
            }
            writeWeights();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = pictureBox1.Image as Bitmap;
            double[,] pixels = new double[pictureWidth, pictureHeight];
            for (int i = 0; i < pictureWidth; i++)
            {
                for (int j = 0; j < pictureHeight; j++)
                {
                    pixels[i, j] = (255.0 - (bitmap.GetPixel(i, j).R)) / 255.0;
                }
            }
            readWeights();
            readSecondWeights();
            neuronSum(pixels);
            outInput();
            double max = 0;
            int maxi = -1;
            richTextBox1.Text = "";
            for (int i = 0; i < outputNeuronsCount; i++)
            {
                richTextBox1.Text += outOutput[i];
                richTextBox1.Text += Environment.NewLine;
                if (outOutput[i] > max)
                {
                    max = outOutput[i];
                    maxi = i;
                }
            }

            richTextBox1.Text += maxi;
        }
        private void randomizeWeightsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("secondWeights.csv", "");
            string wght;
            Random randomWeight = new Random();
            double[,] weights = new double[outputNeuronsCount, HiddenNeuronsCount];
            for (int j = 0; j < outputNeuronsCount; j++)
            {

                for (int m = 0; m < HiddenNeuronsCount; m++)
                {
                    weights[j, m] = randomWeight.Next(0, 10) * 0.001;

                    wght = Convert.ToString(weights[j, m]);

                    if (m == HiddenNeuronsCount - 1)
                    {
                        File.AppendAllText("secondWeights.csv", wght);
                        break;
                    }


                    File.AppendAllText("secondWeights.csv", wght + ",");
                }
                File.AppendAllText("secondWeights.csv", Environment.NewLine);
            }
            File.WriteAllText("weights.csv", "");
            string[] wght2 = new string[pictureHeight * pictureWidth * HiddenNeuronsCount];
            int z = 0;
            Random randomWeight2 = new Random();
            double[,,] weights2 = new double[HiddenNeuronsCount, pictureHeight, pictureWidth];
            for (int i = 0; i < HiddenNeuronsCount; i++)
            {
                for (int j = 0; j < pictureHeight; j++)
                {

                    for (int m = 0; m < pictureWidth; m++)
                    {
                        weights2[i, j, m] = randomWeight2.Next(0, 10) * 0.001;

                        wght2[z] = Convert.ToString(weights2[i, j, m]);

                        if (m == pictureWidth - 1)
                        {
                            if (j != pictureHeight - 1)
                            {
                                File.AppendAllText("weights.csv", wght2[z] + ";");
                                break;
                            }
                            else
                            {
                                File.AppendAllText("weights.csv", wght2[z]);
                                break;
                            }
                        }

                        File.AppendAllText("weights.csv", wght2[z] + ",");
                    }

                }
                File.AppendAllText("weights.csv", Environment.NewLine);
            }
        }
        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint)
            {
                color = new SolidBrush(Color.Black);
                Bitmap Image2 = pictureBox2.Image as Bitmap;
                Graphics g = Graphics.FromImage(Image2);
                g.FillRectangle(color, e.X, e.Y, 10, 10);
                g.Dispose();
                pictureBox2.Image = Image2;
                Bitmap b = new Bitmap(Image2, 30, 50);
                pictureBox1.Image = (Image)b;
            }
        }
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true;
           
        }
        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false;
            
        }
        private void button3_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)Image.FromFile("white.bmp");
            pictureBox2.Image = Image.FromFile("white.bmp");
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = pictureBox1.Image as Bitmap;
            double[,] pixels = new double[pictureWidth, pictureHeight];
            for (int i = 0; i < pictureWidth; i++)
            {
                for (int j = 0; j < pictureHeight; j++)
                {
                    pixels[i, j] = (255.0 - (bitmap.GetPixel(i, j).R)) / 255.0;
                }
            }
            readWeights();
            readSecondWeights();
            neuronSum(pixels);
            outInput();
            double max = 0;
            int maxi = -1;
            richTextBox1.Text = "";
            for (int i = 0; i < outputNeuronsCount; i++)
            {
                richTextBox1.Text += outOutput[i];
                richTextBox1.Text += Environment.NewLine;
                if (outOutput[i] > max)
                {
                    max = outOutput[i];
                    maxi = i;
                }
            }

            richTextBox1.Text += maxi;
        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)Image.FromFile("white.bmp");
            pictureBox2.Image = Image.FromFile("white.bmp");
        }
    }
}
