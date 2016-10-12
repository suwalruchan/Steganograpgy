using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace fyp_stenography
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }


        //public values:
        string PathOfLoadedImage, PathOfloadedFile, Save2Image, PathOfDLoadImage, PathOfDSaveFile, password, confirmedpwd,key;
        int Image_Height, Image_Width;
        long SizeOfFile, SizeOfFileName, passwordlength;
        Image Loaded_Image, Decrypted_Image, After_Encryption;
        Bitmap Loaded_Bitmap, Decrypted_Bitmap;
        byte[] FileToBytes;


        private void Form1_Load(object sender, EventArgs e)
        {
            Form2 bijesh = new Form2();
            bijesh.Show();
        }


        private void BytesToBoolean(byte input, ref bool[] output)
        {
            if (input >= 0 && input <= 255)
                for (short a = 7; a >= 0; a--)
                {
                    if (input % 2 == 1)
                        output[a] = true;
                    else
                        output[a] = false;
                    input /= 2;
                }
            else
                throw new Exception("Illegal Input Number.");
        }


        private byte BooleanToBytes(bool[] input)
        {
            byte output = 0;
            for (short a = 7; a >= 0; a--)
            {
                if (input[a])
                    output += (byte)Math.Pow(2.0, (double)(7 - a));
            }
            return output;
        }


        private string FileNameToBytes(string path)
        {
            string output;
            int a;
            if (path.Length == 3)   // i.e: "C:\\"
                return path.Substring(0, 1);
            for (a = path.Length - 1; a > 0; a--)
                if (path[a] == '\\')
                    break;
            output = path.Substring(a + 1);
            return output;
        }


        private string BytesToFilename(string FileName)
        {
            string output;
            int a;
            for (a = FileName.Length - 1; a > 0; a--)
                if (FileName[a] == '.')
                    break;
            output = FileName.Substring(a + 1);
            return output;
        }



        private void Encryption()
        {
            MessageBox.Show("Please Wait.......", "Encrypting");
            Application.DoEvents();
            long FileSize = SizeOfFile;
            Bitmap Bitmap_Output = Loaded_Bitmap;
            int k = 0, l = 0;
            string key1 = FileNameToBytes(key);
            byte[] temppixel = new byte[3];
            for (l = 0; l * 3 < passwordlength; l++)
            {
                for (k = 0; k < 3 && (l * 3 + k) < passwordlength; k++)
                    temppixel[k] = (byte)key1[l * 3 + k];
                    Color result = Color.FromArgb(temppixel[0],temppixel[1],temppixel[2]);
                    Bitmap_Output.SetPixel(l + 3, 0, result);
                    Array.Clear(temppixel, 0, temppixel.Length);
                
            }            
            Bitmap Changed_Bitmap = Encryption(8, Bitmap_Output, 0, (Image_Height * (Image_Width / 3) * 3) / 3 - SizeOfFileName - 1, true);
            FileSize -= (Image_Height * (Image_Width / 3) * 3) / 3 - SizeOfFileName - 1;

            for (int a = 7; a >= 0 && FileSize > 0; a--)
            {
                Changed_Bitmap = Encryption(a, Changed_Bitmap, (((8 - a) * Image_Height * (Image_Width / 3) * 3) / 3 - SizeOfFileName - (8 - a)), (((9 - a) * Image_Height * (Image_Width / 3) * 3) / 3 - SizeOfFileName - (9 - a)), false);
                FileSize -= (Image_Height * (Image_Width / 3) * 3) / 3 - 1;
            }

            Changed_Bitmap.Save(Save2Image);
            MessageBox.Show("Done", "Encryption");
            After_Encryption = Image.FromFile(Save2Image);
            this.Invalidate();
        }


        private Bitmap Encryption(int layer, Bitmap Bitmap_Input, long Position_Start, long Position_End, bool WriteFN)
        {
            Bitmap Bitmap_Output = Bitmap_Input;
            layer--;
            int k = 0, l = 0;
            long FNSize = 0;
            bool[] f = new bool[8];
            bool[] r = new bool[8];
            bool[] b = new bool[8];
            bool[] g = new bool[8];
            Color pixel = new Color();
            byte rb, gb, bb;

            if (WriteFN)
            {
                FNSize = SizeOfFileName;
                string fileName = FileNameToBytes(PathOfloadedFile);

                //write fileName:
                for (k = 1; k < Image_Height && (k - 1) * (Image_Width / 3) < SizeOfFileName; k++)
                    for (l = 0; l < (Image_Width / 3) * 3 && (k-1) * (Image_Width / 3) + (l / 3) < SizeOfFileName; l++)
                    {
                        BytesToBoolean((byte)fileName[(k - 1) * (Image_Width / 3) + l / 3], ref f);
                        pixel = Bitmap_Input.GetPixel(l, k);
                        rb = pixel.R;
                        gb = pixel.G;
                        bb = pixel.B;
                        BytesToBoolean(rb, ref r);
                        BytesToBoolean(gb, ref b);
                        BytesToBoolean(bb, ref g);
                        if (l % 3 == 0)
                        {
                            r[7] = f[0];
                            b[7] = f[1];
                            g[7] = f[2];
                        }
                        else if (l % 3 == 1)
                        {
                            r[7] = f[3];
                            b[7] = f[4];
                            g[7] = f[5];
                        }
                        else
                        {
                            r[7] = f[6];
                            b[7] = f[7];
                        }
                        Color result = Color.FromArgb((int)BooleanToBytes(r), (int)BooleanToBytes(b), (int)BooleanToBytes(g));
                        Bitmap_Output.SetPixel(l, k, result);
                    }
                k--;
            }
            //write file (after file name):
            int JTemp = l;

            for (; k < Image_Height && (k - 1) * (Image_Width / 3) < Position_End - Position_Start + FNSize && Position_Start + (k - 1) * (Image_Width / 3) < SizeOfFile + FNSize; k++)
                for (l = 0; l < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (l / 3) < Position_End - Position_Start + FNSize && Position_Start + (k - 1) * (Image_Width / 3) + (l / 3) < SizeOfFile + FNSize; l++)
                {
                    if (JTemp != 0)
                    {
                        l = JTemp;
                        JTemp = 0;
                    }
                    BytesToBoolean((byte)FileToBytes[Position_Start + (k - 1) * (Image_Width / 3) + l / 3 - FNSize], ref f);
                    pixel = Bitmap_Input.GetPixel(l, k);
                    rb = pixel.R;
                    gb = pixel.G;
                    bb = pixel.B;
                    BytesToBoolean(rb, ref r);
                    BytesToBoolean(gb, ref b);
                    BytesToBoolean(bb, ref g);
                    if (l % 3 == 0)
                    {
                        r[layer] = f[0];
                        b[layer] = f[1];
                        g[layer] = f[2];
                    }
                    else if (l % 3 == 1)
                    {
                        r[layer] = f[3];
                        b[layer] = f[4];
                        g[layer] = f[5];
                    }
                    else
                    {
                        r[layer] = f[6];
                        b[layer] = f[7];
                    }
                    Color result = Color.FromArgb((int)BooleanToBytes(r), (int)BooleanToBytes(b), (int)BooleanToBytes(g));
                    Bitmap_Output.SetPixel(l, k, result);

                }

            long pwdlength = passwordlength;
            rb = (byte)(pwdlength % 255);
            pwdlength /= 255;
            gb = (byte)(pwdlength % 255);
            pwdlength /= 255;
            bb = (byte)(pwdlength % 255);
            Color pwdlengthColor = Color.FromArgb(rb, gb, bb);
            Bitmap_Output.SetPixel(2, 0, pwdlengthColor);
 
            long Temp_FileSize = SizeOfFile ;
            rb = (byte)(Temp_FileSize % 255);
            Temp_FileSize /= 255;
            gb = (byte)(Temp_FileSize % 255);
            Temp_FileSize /= 255;
            bb = (byte)(Temp_FileSize % 255);
            Color FilelengthColor = Color.FromArgb(rb, gb, bb);
            Bitmap_Output.SetPixel(0, 0, FilelengthColor);

            long Temp_FileNameSize = SizeOfFileName;
            rb = (byte)(Temp_FileNameSize % 255);
            Temp_FileNameSize /= 255;
            gb = (byte)(Temp_FileNameSize % 255);
            Temp_FileNameSize /= 255;
            bb = (byte)(Temp_FileNameSize % 255);
            Color FileNameLengthColor = Color.FromArgb(rb, gb, bb);
            Bitmap_Output.SetPixel(1, 0, FileNameLengthColor);

            return Bitmap_Output;
        }


        private void DecryptLayer()
        {
            MessageBox.Show("Please Wait.......", "Decrypting");
            Application.DoEvents();
            int k, j = 0;
            bool[] f = new bool[8];
            bool[] r = new bool[8];
            bool[] b = new bool[8];
            bool[] g = new bool[8];
            Color pixel = new Color();
            byte rb, gb, bb;
            pixel = Decrypted_Bitmap.GetPixel(2, 0);
            rb = pixel.R;
            gb = pixel.G;
            bb = pixel.B;
            long pwdlength = rb + gb * 255 + bb * 255 * 255;

            pixel = Decrypted_Bitmap.GetPixel(0, 0);
            rb = pixel.R;
            gb = pixel.G;
            bb = pixel.B;
            long FileSize = rb + gb * 255 + bb * 255 * 255;

            pixel = Decrypted_Bitmap.GetPixel(1, 0);
            rb = pixel.R;
            gb = pixel.G;
            bb = pixel.B;
            long FileNameSize = rb + gb * 255 + bb * 255 * 255;
            
            byte[] ResFile = new byte[FileSize];
            string ResFileName = "";
            string ResPwd = "";
            byte Virtual;

            for (k = 0; (k * 3) < pwdlength; k++)
            {
                pixel = Decrypted_Bitmap.GetPixel(k + 3, 0);
                rb = pixel.R;
                gb = pixel.G;
                bb = pixel.B;
                ResPwd += (char)rb ;
                ResPwd += (char)gb ;
                ResPwd += (char)bb ;
            }
            textBox7.Text = ResPwd;
            /*
            if (textBox7.Text.Equals(ResPwd, StringComparison.Ordinal))
            {
                //Read file name:
                for (k = 1; k < Image_Height && (k - 1) * (Image_Width / 3) < FileNameSize; k++)
                    for (j = 0; j < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (j / 3) < FileNameSize; j++)
                    {
                        pixel = Decrypted_Bitmap.GetPixel(j, k);
                        rb = pixel.R;
                        gb = pixel.G;
                        bb = pixel.B;
                        BytesToBoolean(rb, ref r);
                        BytesToBoolean(gb, ref b);
                        BytesToBoolean(bb, ref g);
                        if (j % 3 == 0)
                        {
                            f[0] = r[7];
                            f[1] = b[7];
                            f[2] = g[7];
                        }
                        else if (j % 3 == 1)
                        {
                            f[3] = r[7];
                            f[4] = b[7];
                            f[5] = g[7];
                        }
                        else
                        {
                            f[6] = r[7];
                            f[7] = b[7];
                            Virtual = BooleanToBytes(f);
                            ResFileName += (char)Virtual;
                        }
                    }

                //After file name read file on layer eight :
                int JTemp = j;
                k--;

                for (; k < Image_Height && (k - 1) * (Image_Width / 3) < FileSize + FileNameSize; k++)
                    for (j = 0; j < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (j / 3) < (Image_Height * (Image_Width / 3) * 3) / 3 - 1 && (k - 1) * (Image_Width / 3) + (j / 3) < FileSize + FileNameSize; j++)
                    {
                        if (JTemp != 0)
                        {
                            j = JTemp;
                            JTemp = 0;
                        }
                        pixel = Decrypted_Bitmap.GetPixel(j, k);
                        rb = pixel.R;
                        gb = pixel.G;
                        bb = pixel.B;
                        BytesToBoolean(rb, ref r);
                        BytesToBoolean(gb, ref b);
                        BytesToBoolean(bb, ref g);
                        if (j % 3 == 0)
                        {
                            f[0] = r[7];
                            f[1] = b[7];
                            f[2] = g[7];
                        }
                        else if (j % 3 == 1)
                        {
                            f[3] = r[7];
                            f[4] = b[7];
                            f[5] = g[7];
                        }
                        else
                        {
                            f[6] = r[7];
                            f[7] = b[7];
                            Virtual = BooleanToBytes(f);
                            ResFile[(k - 1) * (Image_Width / 3) + j / 3 - FileNameSize] = Virtual;
                        }
                    }

                //Read Bytes on remaining layer:
                long filebytesonlayer8 = (Image_Height * (Image_Width / 3) * 3) / 3 - FileNameSize - 1;

                for (int layer = 6; layer >= 0 && filebytesonlayer8 + (6 - layer) * ((Image_Height * (Image_Width / 3) * 3) / 3 - 1) < FileSize; layer--)
                    for (k = 1; k < Image_Height && (k - 1) * (Image_Width / 3) + filebytesonlayer8 + (6 - layer) * ((Image_Height * (Image_Width / 3) * 3) / 3 - 1) < FileSize; k++)
                        for (j = 0; j < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (j / 3) + filebytesonlayer8 + (6 - layer) * ((Image_Height * (Image_Width / 3) * 3) / 3 - 1) < FileSize; j++)
                        {
                            pixel = Decrypted_Bitmap.GetPixel(j, k);
                            rb = pixel.R;
                            gb = pixel.G;
                            bb = pixel.B;
                            BytesToBoolean(rb, ref r);
                            BytesToBoolean(gb, ref b);
                            BytesToBoolean(bb, ref g);
                            if (j % 3 == 0)
                            {
                                f[0] = r[layer];
                                f[1] = b[layer];
                                f[2] = g[layer];
                            }
                            else if (j % 3 == 1)
                            {
                                f[3] = r[layer];
                                f[4] = b[layer];
                                f[5] = g[layer];
                            }
                            else
                            {
                                f[6] = r[layer];
                                f[7] = b[layer];
                                Virtual = BooleanToBytes(f);
                                ResFile[(k - 1) * (Image_Width / 3) + j / 3 + (6 - layer) * ((Image_Height * (Image_Width / 3) * 3) / 3 - 1) + filebytesonlayer8] = Virtual;
                            }
                        }

                if (File.Exists(PathOfDSaveFile + "\\" + ResFileName))
                {
                    MessageBox.Show("File \"" + ResFileName + "\" Name already exists write another name", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                    return;
                }
                else
                    File.WriteAllBytes(PathOfDSaveFile + "\\" + ResFileName, ResFile);
                MessageBox.Show("Done", "Decryption");
                Application.DoEvents();
            }
            else
            {
                MessageBox.Show("Enter Correct Password","Invalid Password!!");
            }*/
        }



        private void button1_Click(object sender, EventArgs e)
        {
            #region cover Image
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                PathOfLoadedImage = openFileDialog1.FileName;
                textBox1.Text = PathOfLoadedImage;
                pictureBox1.Image = Image.FromFile(PathOfLoadedImage);
                Loaded_Image = Image.FromFile(PathOfLoadedImage);
                Image_Height = Loaded_Image.Height;
                Image_Width = Loaded_Image.Width;
                Loaded_Bitmap = new Bitmap(Loaded_Image);

                double SaveUpto = (Image_Height * Image_Width * 8) / (8 * 1024 * 1024);
                MessageBox.Show("The Image can save the file Upto " + SaveUpto.ToString() + " MB ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            #endregion
        }


        private void button2_Click(object sender, EventArgs e)
        {
            #region Load_File
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                PathOfloadedFile = openFileDialog2.FileName;
                textBox2.Text = PathOfloadedFile;
                FileInfo finfo = new FileInfo(PathOfloadedFile);
                SizeOfFile = finfo.Length;
                SizeOfFileName = FileNameToBytes(PathOfloadedFile).Length;
            }
            #endregion
        }


        private void button3_Click(object sender, EventArgs e)
        {
            #region Encryption

            if (textBox1.Text == String.Empty || textBox2.Text == String.Empty)
            {
                MessageBox.Show("Files are not loaded!\nPlease load them frist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //if (Image_Height * Image_Width * (3 * 3 - 1) < SizeOfFile + SizeOfFileName)
            //{
             //   MessageBox.Show("Large FileSize!\nPlease use a large size image to hide this file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
             //   return;
           // }
            password = textBox3.Text;
            confirmedpwd = textBox4.Text;
            if (password.Equals(confirmedpwd, StringComparison.Ordinal))
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Save2Image = saveFileDialog1.FileName;
                }
                else
                    return; 
                key = password;
                passwordlength = FileNameToBytes(key).Length;
                FileToBytes = File.ReadAllBytes(PathOfloadedFile);
                Encryption();
            }
            else
            {
                MessageBox.Show("Password Not Matched", "Re Type Password");
                textBox3.Text = "";
                textBox4.Text = "";
            }

            #endregion
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            #region ImageToDecrypt
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                    PathOfDLoadImage = openFileDialog1.FileName;
                    textBox5.Text = PathOfDLoadImage;
                    pictureBox1.Image = Image.FromFile(PathOfDLoadImage);
                    Decrypted_Image = Image.FromFile(PathOfDLoadImage);
                    Image_Height = Decrypted_Image.Height;
                    Image_Width = Decrypted_Image.Width;
                    Decrypted_Bitmap = new Bitmap(Decrypted_Image);
                 
             }
            #endregion
         }

        private void button6_Click(object sender, EventArgs e)
        {
            #region SaveToFile
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                PathOfDSaveFile = folderBrowserDialog1.SelectedPath;
                textBox6.Text = PathOfDSaveFile;
            }
            #endregion
        }


        private void button7_Click(object sender, EventArgs e)
        {
            #region Decryption
            if (textBox5.Text == String.Empty || textBox6.Text == String.Empty)
            {
                MessageBox.Show("Text boxes must not be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (System.IO.File.Exists(textBox5.Text) == false)
            {
                MessageBox.Show("Select image file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBox1.Focus();
                return;
            }

           

            DecryptLayer();

            #endregion
        }
    }
}
