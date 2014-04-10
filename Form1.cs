using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Automerge_nevlabs
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Download_A()
        {
            if (groupBox2.Text.Length > 0)
            {
                richTextBox1.Text = "";
                string[] file1 = File.ReadAllLines(groupBox2.Text);
                for (int i = 0; i < file1.Length; i++)
                {
                    richTextBox1.Text += i + 1 + ">" + file1[i] + System.Environment.NewLine;
                }
            };
        }

        private void Download_B()
        {
            if (groupBox4.Text.Length > 0)
            {
                richTextBox2.Text = "";
                string[] file2 = File.ReadAllLines(groupBox4.Text);
                for (int i = 0; i < file2.Length; i++)
                {
                    richTextBox2.AppendText(i + 1 + ">" + file2[i] + System.Environment.NewLine);
                }
                if (groupBox2.Text.Length > 0)
                {
                    string[] file12 = File.ReadAllLines(groupBox2.Text);
                    string[] file22 = File.ReadAllLines(groupBox4.Text);
                    var m12 = Diff_method.list_diff2(file12, file22);
                    string s1 = richTextBox1.Text;
                    string s2 = richTextBox2.Text;
                    for (int ii = 0; ii < m12.Count; ii++)
                    {
                        int i1 = s2.IndexOf(m12[ii].file2.offset + 1 + ">");
                        int i2 = s2.IndexOf(m12[ii].file2.offset + 1 + m12[ii].file2.length + ">");
                        if ((i2 - i1) > 0)
                        {
                            richTextBox2.Select(i1, i2 - i1);
                            richTextBox2.SelectionBackColor = Color.LawnGreen;
                        };
                        if (i2 == -1 && i1 >= 0)
                        {
                            richTextBox2.Select(i1, s2.Length - i1);
                            richTextBox2.SelectionBackColor = Color.LawnGreen;
                        };
                        if (m12[ii].file1.length > 0)
                        {
                            int i11 = s1.IndexOf(m12[ii].file1.offset + 1 + ">");
                            int i22 = s1.IndexOf(m12[ii].file1.offset + 1 + m12[ii].file1.length + ">");
                            if ((i22 - i11) > 0)
                            {
                                richTextBox1.Select(i11, i22 - i11);
                                richTextBox1.SelectionBackColor = Color.Red;
                            };
                            if (i22 == -1)
                            {
                                richTextBox1.Select(i11, s1.Length - i11);
                                richTextBox1.SelectionBackColor = Color.Red;
                            };
                        };
                    }
                }
            }
        }

        private void Download_C()
        {
            if (groupBox6.Text.Length > 0)
            {
                richTextBox3.Text = "";
                string[] file3 = File.ReadAllLines(groupBox6.Text);
                for (int i = 0; i < file3.Length; i++)
                {
                    richTextBox3.AppendText(i + 1 + ">" + file3[i] + System.Environment.NewLine);
                }
                if (groupBox2.Text.Length > 0)
                {
                    string[] file13 = File.ReadAllLines(groupBox2.Text);
                    string[] file33 = File.ReadAllLines(groupBox6.Text);
                    var m23 = Diff_method.list_diff2(file13, file33);
                    string s11 = richTextBox1.Text;
                    string s3 = richTextBox3.Text;
                    for (int ii3 = 0; ii3 < m23.Count; ii3++)
                    {
                        int i1 = s3.IndexOf(m23[ii3].file2.offset + 1 + ">");
                        int i2 = s3.IndexOf(m23[ii3].file2.offset + 1 + m23[ii3].file2.length + ">");
                        if ((i2 - i1) > 0)
                        {
                            richTextBox3.Select(i1, i2 - i1);
                            richTextBox3.SelectionBackColor = Color.LawnGreen;
                        };
                        if (i2 == -1 && i1 >= 0)
                        {
                            richTextBox3.Select(i1, s3.Length - i1);
                            richTextBox3.SelectionBackColor = Color.LawnGreen;
                        };
                        if (m23[ii3].file1.length > 0)
                        {
                            int i11 = s11.IndexOf(m23[ii3].file1.offset + 1 + ">");
                            int i22 = s11.IndexOf(m23[ii3].file1.offset + 1 + m23[ii3].file1.length + ">");
                            if ((i22 - i11) > 0)
                            {
                                richTextBox1.Select(i11, i22 - i11);
                                richTextBox1.SelectionBackColor = Color.Red;
                            };
                            if (i22 == -1)
                            {
                                richTextBox1.Select(i11, s11.Length - i11);
                                richTextBox1.SelectionBackColor = Color.Red;
                            };
                        };
                    }
                }
            }
        }

        private void Download_D()
        {
            if (groupBox2.Text.Length > 0 && groupBox4.Text.Length > 0 && groupBox6.Text.Length > 0)
            {
                richTextBox4.Text = "";
                string[] file14 = File.ReadAllLines(groupBox2.Text);
                string[] file24 = File.ReadAllLines(groupBox4.Text);
                string[] file34 = File.ReadAllLines(groupBox6.Text);
                if (groupBox8.Text.Length > 0)
                {
                    var chunks4 = Diff_method.FinalMerge(file24, file14, file34, true);
                    var chunks44 = Diff_method.OptimizeResult(chunks4);
                    if (File.Exists(groupBox8.Text)) File.Delete(groupBox8.Text);
                    for (int i = 0; i < chunks44.Count; i++)
                    {
                        richTextBox4.Text += chunks44[i] + System.Environment.NewLine;
                        File.AppendAllText(groupBox8.Text, chunks44[i] + System.Environment.NewLine);
                    }
                    string s4 = richTextBox4.Text;
                    string s01 = "---------->   Начало конфликта   <----------";
                    string s02 = "---------->   Конец конфликта   <----------";
                    int count = (s4.Length - s4.Replace(s01, "").Length) / s01.Length;
                    int find_start = 0;
                    for (int ii4 = 1; ii4 <= count; ii4++)
                    {
                        int i1 = s4.IndexOf(s01, find_start);
                        int i2 = s4.IndexOf(s02, find_start);
                        richTextBox4.Select(i1, i2 - i1 + s02.Length);
                        richTextBox4.SelectionBackColor = Color.LightCoral;
                        find_start = i2 + 1;
                    }
                }
            }
        }

        private void Group_Download()
        {
            Download_A();
            Download_B();
            Download_C();
            Download_D();
        }

        private void OpenFile(String URL)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;
                try
                {
                    string text = File.ReadAllText(file);
                    switch (URL)
                    {
                        case "A":
                            groupBox2.Text = file;
                            Group_Download();
                            break;
                        case "B":
                            groupBox4.Text = file;
                            Group_Download();
                            break;
                        case "C":
                            groupBox6.Text = file;
                            Group_Download();
                            break;
                        case "D":
                            groupBox8.Text = file;
                            Group_Download();
                            break;
                    }
                }
                catch (IOException)
                {
                    using (FileStream fs = File.Create(openFileDialog1.FileName))
                    {
                        fs.Close();
                    }
                    groupBox8.Text = openFileDialog1.FileName;
                    Group_Download();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFile("A");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFile("B");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFile("C");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFile("D");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Group_Download();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Group_Download();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Group_Download();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ToolTip t1 = new ToolTip();
            t1.IsBalloon = true;
            t1.BackColor = Color.Wheat;
            t1.ForeColor = Color.Red;
            t1.SetToolTip(button5, "Загрузить обновленный файл");
            ToolTip  t2 = new ToolTip();
            t2.IsBalloon = true;
            t2.BackColor = Color.Wheat;
            t2.ForeColor = Color.Red;
            t2.SetToolTip(button6, "Загрузить обновленный файл");
            ToolTip t3 = new ToolTip();
            t3.IsBalloon = true;
            t3.BackColor = Color.Wheat;
            t3.ForeColor = Color.Red;
            t3.SetToolTip(button7, "Загрузить обновленный файл");
            ToolTip t5 = new ToolTip();
            t5.IsBalloon = true;
            t5.BackColor = Color.LavenderBlush;
            t5.ForeColor = Color.Black;
            t5.SetToolTip(button1, "Выберите файл-родитель");
            ToolTip t6 = new ToolTip();
            t6.IsBalloon = true;
            t6.BackColor = Color.LavenderBlush;
            t6.ForeColor = Color.Black;
            t6.SetToolTip(button2, "Выберите файл-потомок 1");
            ToolTip t7 = new ToolTip();
            t7.IsBalloon = true;
            t7.BackColor = Color.LavenderBlush;
            t7.ForeColor = Color.Black;
            t7.SetToolTip(button3, "Выберите файл-потомок 2");
            ToolTip t8 = new ToolTip();
            t8.IsBalloon = true;
            t8.BackColor = Color.LavenderBlush;
            t8.ForeColor = Color.Black;
            t8.SetToolTip(button4, "Выберите или напишите файл-результат слияния ");
        }

      }
}
