using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace fyp_stenography
{
    public partial class Form2 : Form
    {
        Form oldform;
        public Form2(Form parent)
        {
            InitializeComponent();
            oldform = parent;
        }


        //public values:
        string PathOfLoadedImage, PathOfloadedFile, Save2Image, PathOfDLoadImage, PathOfDSaveFile, password, confirmedpwd, key, pathname;
        int Image_Height, Image_Width;
        long SizeOfFile, SizeOfFileName, passwordlength;
        Image Loaded_Image, Decrypted_Image, After_Encryption;
        Bitmap Loaded_Bitmap, Decrypted_Bitmap;
        byte[] FileToBytes;
        Thread t;

                       
        private void ToBoolean(byte input, ref bool[] output)
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


        private byte ToBytes(bool[] input)
        {
            byte output = 0;
            for (short a = 7; a >= 0; a--)
            {
                if (input[a])
                    output += (byte)Math.Pow(2.0, (double)(7 - a));
            }
            return output;
        }


        private string GetFileName(string path)
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


        private string SaveFileName(string FileName)
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
            toolStripStatusLabel1.Text = "Encrypting... Please wait";
            Application.DoEvents();
            long FileSize = SizeOfFile;
            Bitmap Bitmap_Output = Loaded_Bitmap;
            int k = 0, l = 0;
            string key1 = key.Substring(0);
            long tempdata1 = 0;

            byte[] temppixel = new byte[3];

            for (l = 0; l * 3 < key1.Length; l++)
            {
                for (k = 0; k < 3 && (l * 3 + k) < key1.Length; k++)
                    temppixel[k] = (byte)key1[l * 3 + k];
                Color result = Color.FromArgb(temppixel[0], temppixel[1], temppixel[2]);
                Bitmap_Output.SetPixel(l + 3, 0, result);
            }

            Bitmap Changed_Bitmap = Encryption(8, Bitmap_Output, 0, ((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - SizeOfFileName - 1, true);
            FileSize -= ((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - SizeOfFileName - 1;
            tempdata1 = ((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - SizeOfFileName - 1;

            for (int a = 7; a >= 0 && FileSize > 0; a--)
            {
                Changed_Bitmap = Encryption(a, Changed_Bitmap, tempdata1, tempdata1 + (((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1), false);
                FileSize -= ((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1;
                tempdata1 += ((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1;
            }

            Changed_Bitmap.Save(Save2Image);
            toolStripStatusLabel1.Text = "Encryption Completed!!";
            Application.DoEvents();
            After_Encryption = Image.FromFile(Save2Image);
            this.Invalidate();
        }


        private Bitmap Encryption(int layer, Bitmap Bitmap_Input, long Position_Start, long Position_End, bool WriteFN)
        {
            Bitmap Bitmap_Output = Bitmap_Input;
            layer--;
            int k = 1, l = 0;
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
                string fileName = GetFileName(PathOfloadedFile);

                //write fileName:
                for (k = 1; k < Image_Height && (k - 1) * (Image_Width / 3) < SizeOfFileName; k++)
                    for (l = 0; l < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (l / 3) < SizeOfFileName; l++)
                    {
                        ToBoolean((byte)fileName[(k - 1) * (Image_Width / 3) + l / 3], ref f);
                        pixel = Bitmap_Input.GetPixel(l, k);
                        rb = pixel.R;
                        gb = pixel.G;
                        bb = pixel.B;
                        ToBoolean(rb, ref r);
                        ToBoolean(gb, ref b);
                        ToBoolean(bb, ref g);
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
                        Color result = Color.FromArgb((int)ToBytes(r), (int)ToBytes(b), (int)ToBytes(g));
                        Bitmap_Output.SetPixel(l, k, result);
                    }
                k--;
            }
            //write file (after file name):
            int JTemp = l;
            k = 1;
            for (; k < Image_Height && (k - 1) * (Image_Width / 3) < Position_End - Position_Start + FNSize && Position_Start + (k - 1) * (Image_Width / 3) < SizeOfFile + FNSize; k++)
                for (l = 0; l < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (l / 3) < Position_End - Position_Start + FNSize && Position_Start + (k - 1) * (Image_Width / 3) + (l / 3) < SizeOfFile + FNSize; l++)
                {
                    if (JTemp != 0)
                    {
                        l = JTemp;
                        JTemp = 0;
                    }
                    ToBoolean((byte)FileToBytes[Position_Start + (k - 1) * (Image_Width / 3) + l / 3 - FNSize], ref f);
                    pixel = Bitmap_Input.GetPixel(l, k);
                    rb = pixel.R;
                    gb = pixel.G;
                    bb = pixel.B;
                    ToBoolean(rb, ref r);
                    ToBoolean(gb, ref b);
                    ToBoolean(bb, ref g);
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
                    Color result = Color.FromArgb((int)ToBytes(r), (int)ToBytes(b), (int)ToBytes(g));
                    Bitmap_Output.SetPixel(l, k, result);

                }

            //Color pwdlengthColor = Color.FromArgb((byte)(passwordlength), 0, 0);
            //Bitmap_Output.SetPixel(2, 0, pwdlengthColor);

            long Temp_FileSize = SizeOfFile;
            rb = (byte)(Temp_FileSize % 256);
            Temp_FileSize /= 256;
            gb = (byte)(Temp_FileSize % 256);
            Temp_FileSize /= 256;
            bb = (byte)(Temp_FileSize % 256);
            Temp_FileSize /= 256;
            Color FilelengthColor = Color.FromArgb(rb, gb, bb);
            Bitmap_Output.SetPixel(0, 0, FilelengthColor);
            byte gb1 = (byte)(Temp_FileSize % 256);
            Color filelengthColor1 = Color.FromArgb((byte)(passwordlength), (byte)(gb1), 0);
            Bitmap_Output.SetPixel(2, 0, filelengthColor1);

            long Temp_FileNameSize = SizeOfFileName;
            rb = (byte)(Temp_FileNameSize % 256);
            Temp_FileNameSize /= 256;
            gb = (byte)(Temp_FileNameSize % 256);
            Temp_FileNameSize /= 256;
            bb = (byte)(Temp_FileNameSize % 256);
            Color FileNameLengthColor = Color.FromArgb(rb, gb, bb);
            Bitmap_Output.SetPixel(1, 0, FileNameLengthColor);

            return Bitmap_Output;
        }


        private void mail()
        {
            try
            {
                MailMessage mail = new MailMessage(from.Text, to.Text, subject.Text, body.Text);
                SmtpClient newclient = new SmtpClient(smtp.Text);

                newclient.Port = 587;
                newclient.Credentials = new System.Net.NetworkCredential(username.Text, pswd.Text);
                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(pathname);
                mail.Attachments.Add(attachment);
                newclient.EnableSsl = true;
            
                newclient.Send(mail);
                MessageBox.Show("Mail Sent!", "Success", MessageBoxButtons.OK);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error!");
            }
        }


        private void DecryptLayer()
        {
            
            int k, j = 0;
            bool[] f = new bool[8];
            bool[] r = new bool[8];
            bool[] b = new bool[8];
            bool[] g = new bool[8];
            Color pixel = new Color();
            byte rb, gb, bb, gb1;
            pixel = Decrypted_Bitmap.GetPixel(2, 0);
            rb = pixel.R;
            gb1 = pixel.G;
            bb = pixel.B;
            int pwdlength = rb;

            pixel = Decrypted_Bitmap.GetPixel(0, 0);
            rb = pixel.R;
            gb = pixel.G;
            bb = pixel.B;
            long FileSize = rb + gb * 256 + bb * 256 * 256 + gb1 * 256 * 256 * 256;

            pixel = Decrypted_Bitmap.GetPixel(1, 0);
            rb = pixel.R;
            gb = pixel.G;
            bb = pixel.B;
            long FileNameSize = rb + gb * 256 + bb * 256 * 256;

            byte[] ResFile = new byte[FileSize];
            string ResFileName = "";
            string ResPwd = "";
            byte Virtual;
            //textBox1.Text = pwdlength.ToString();
            for (k = 0; (k * 3) <= pwdlength; k++)
            {
                pixel = Decrypted_Bitmap.GetPixel(k + 3, 0);
                rb = pixel.R;
                gb = pixel.G;
                bb = pixel.B;
                ResPwd += (char)rb;
                ResPwd += (char)gb;
                ResPwd += (char)bb;
            }
            string respwd1 = ResPwd.Substring(0, pwdlength + 1);
            //textBox5.Text = respwd1;
            if (textBox7.Text.Equals(respwd1, StringComparison.OrdinalIgnoreCase))
            {

                toolStripStatusLabel1.Text = "Please Wait.......Decrypting";
                Application.DoEvents();
                //Read file name:
                for (k = 1; k < Image_Height && (k - 1) * (Image_Width / 3) < FileNameSize; k++)
                    for (j = 0; j < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (j / 3) < FileNameSize; j++)
                    {
                        pixel = Decrypted_Bitmap.GetPixel(j, k);
                        rb = pixel.R;
                        gb = pixel.G;
                        bb = pixel.B;
                        ToBoolean(rb, ref r);
                        ToBoolean(gb, ref b);
                        ToBoolean(bb, ref g);
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
                            Virtual = ToBytes(f);
                            ResFileName += (char)Virtual;
                        }
                    }

                //After file name read file on layer eight :
                int JTemp = j;
                k = 1;

                for (; k < Image_Height && (k - 1) * (Image_Width / 3) < FileSize + FileNameSize; k++)
                    for (j = 0; j < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (j / 3) < ((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1 && (k - 1) * (Image_Width / 3) + (j / 3) < FileSize + FileNameSize; j++)
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
                        ToBoolean(rb, ref r);
                        ToBoolean(gb, ref b);
                        ToBoolean(bb, ref g);
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
                            Virtual = ToBytes(f);
                            ResFile[(k - 1) * (Image_Width / 3) + j / 3 - FileNameSize] = Virtual;
                        }
                    }

                //Read Bytes on remaining layer:
                long filebytesonlayer8 = ((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - FileNameSize - 1;

                for (int layer = 6; layer >= 0 && filebytesonlayer8 + (6 - layer) * (((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1) < FileSize; layer--)
                    for (k = 1; k < Image_Height && (k - 1) * (Image_Width / 3) + filebytesonlayer8 + (6 - layer) * (((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1) < FileSize; k++)
                        for (j = 0; j < (Image_Width / 3) * 3 && (k - 1) * (Image_Width / 3) + (j / 3) + filebytesonlayer8 + (6 - layer) * (((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1) < FileSize; j++)
                        {
                            pixel = Decrypted_Bitmap.GetPixel(j, k);
                            rb = pixel.R;
                            gb = pixel.G;
                            bb = pixel.B;
                            ToBoolean(rb, ref r);
                            ToBoolean(gb, ref b);
                            ToBoolean(bb, ref g);
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
                                Virtual = ToBytes(f);
                                ResFile[(k - 1) * (Image_Width / 3) + j / 3 + (6 - layer) * (((Image_Height - 1) * (Image_Width / 3) * 3) / 3 - 1) + filebytesonlayer8] = Virtual;
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
                toolStripStatusLabel1.Text = "Decreption Sucessfully done";
                Application.DoEvents();
            }
            else
            {
                MessageBox.Show("Enter Correct Password", "Invalid Password!!");
            } /**/
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            #region cover Image
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                PathOfLoadedImage = openFileDialog1.FileName;
                loadimage.Text = PathOfLoadedImage;
                pictureBox1.Image = Image.FromFile(PathOfLoadedImage);
                Loaded_Image = Image.FromFile(PathOfLoadedImage);
                Image_Height = Loaded_Image.Height;
                Image_Width = Loaded_Image.Width;
                Loaded_Bitmap = new Bitmap(Loaded_Image);

                double SaveUpto = (Image_Height * Image_Width * 6) / (8 * 1024);
                MessageBox.Show("The Image can save the file Upto " + SaveUpto.ToString() + " KB ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            #endregion
        }


        private void button2_Click(object sender, EventArgs e)
        {
            #region Load_File
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                PathOfloadedFile = openFileDialog2.FileName;
                loadfile.Text = PathOfloadedFile;
                FileInfo finfo = new FileInfo(PathOfloadedFile);
                SizeOfFile = finfo.Length;
                SizeOfFileName = GetFileName(PathOfloadedFile).Length;
            }
            #endregion
        }


        private void button3_Click(object sender, EventArgs e)
        {
            #region Encryption

            if (loadimage.Text == String.Empty || loadfile.Text == String.Empty)
            {
                MessageBox.Show("Files are not loaded!\nPlease load them frist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (Image_Height * Image_Width * (3 * 2) < SizeOfFile + SizeOfFileName)
            {
                MessageBox.Show("Large FileSize!\nPlease use a large size image to hide this file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            password = pwd.Text;
            confirmedpwd = cpwd.Text;
            if (password.Equals(confirmedpwd, StringComparison.Ordinal) && password.Length > 4)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Save2Image = saveFileDialog1.FileName;
                }
                else
                    return;
                key = password;
                passwordlength = GetFileName(key).Length;
                FileToBytes = File.ReadAllBytes(PathOfloadedFile);
                t = new Thread(Encryption);
                t.Start();
            }
            else
            {
                MessageBox.Show("Password Not Matched or Less than 5 digits", "Re Type Password");
                pwd.Text = "";
                cpwd.Text = "";
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
                loadimage.Focus();
                return;
            }



            t = new Thread(DecryptLayer);
            t.Start();

            #endregion
        }


        private void button9_Click(object sender, EventArgs e)
        {
            #region Mail
            
                t = new Thread(mail);
                t.Start();
                     
            #endregion

        }


        private void button8_Click(object sender, EventArgs e)
        {
            #region attachment
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = openFileDialog1.FileName;
                pathname = this.Text;
                //MessageBox.Show(pathname);
            }
            #endregion
        }


        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {

          //  t.Abort();

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            oldform.Close();
        }
    }
}