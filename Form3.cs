using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab2
{
    public partial class Form3 : Form
    {


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]  //подключение библиотеки kernel32.dll из папки Windows\system32
        //функция создания файла с перечислением стандартных для неё флагов:
        public static extern SafeFileHandle CreateFile(
        string lpFileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
        [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
        IntPtr hTemplateFile);

        //настройка функции из кернел.длл
        SafeFileHandle handle = CreateFile(
        lpFileName: @"\\.\" + Form1.drive[0] + ":",
        dwDesiredAccess: FileAccess.Read,
        dwShareMode: FileShare.ReadWrite,
        lpSecurityAttributes: IntPtr.Zero,
        dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
        dwFlagsAndAttributes: FileAttributes.Normal,
        hTemplateFile: IntPtr.Zero);



        public Form3()
        {
            InitializeComponent();
        }
        int count_error; //счётчик неправильных вводов пароля
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "") { MessageBox.Show("Пароль не введён!"); return; }


            //введённый пароль преобразуем в хэш
            String password = Form1.GetMd5Hash(textBox1.Text);

            //защита от подмены флешки путем перечитывания нулевого сектора
            //читаем информацию из нулевого сектора для проверки есть ли пароль
            SafeFileHandle handle = CreateFile(
lpFileName: @"\\.\" + Form1.drive[0] + ":",
dwDesiredAccess: FileAccess.Read,
dwShareMode: FileShare.ReadWrite,
lpSecurityAttributes: IntPtr.Zero,
dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
dwFlagsAndAttributes: FileAttributes.Normal,
hTemplateFile: IntPtr.Zero);

            using (FileStream disk = new FileStream(handle, FileAccess.Read))
            {
                Form1.buffer0sector = new byte[512];


                disk.Read(Form1.buffer0sector, 0, 512);
            }



            //сравниваем побитно хеш введённого пароля и пароля из нулевого сектора
            for (int ii = 0; ii < 32; ii++)
                if (Form1.buffer0sector[384 + ii] != (byte)password[ii])
                {
                    MessageBox.Show("Пароль введён неверно!");
                    textBox1.SelectAll();
                    textBox1.Focus();
                    count_error++;
                    if (count_error == 3)
                    {
                        textBox1.ReadOnly = true;
                        button1.Enabled = false;
                        MessageBox.Show("У Вас закончились попытки ввести пароль!");
                        //  this.Close();
                    }
                   // goto metka_error;
                    return;
                }

            //выводим содержимое каталога, если вход успешен
            {
                String[] directories = Directory.GetDirectories(@Form1.drive[0] + ":\\");
            treeView1.Nodes.Clear();
            foreach (String s in directories)
                treeView1.Nodes.Add(s);


            treeView1.Show();
            listView1.Items.Clear();
            listView1.Show();
            }



      //  metka_error:
      //      { }
        }


        //выбор папки открывает список подпапок и файлов
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            listView1.Items.Clear();
            DirectoryInfo dir = new DirectoryInfo(@e.Node.FullPath);
            e.Node.Nodes.Clear();
            foreach (var item in dir.GetDirectories())
            {
                e.Node.Nodes.Add(item.Name);
                e.Node.Expand();
            }
            foreach (var item in dir.GetFiles())
            {
                listView1.Items.Add(item.Name,0);
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
           
            treeView1.ImageList = new ImageList();
            treeView1.ImageList.Images.Add(Image.FromFile("Default.png"));
             treeView1.ImageIndex = 0;

            listView1.LargeImageList = new ImageList();
            listView1.LargeImageList.ImageSize = new Size(70, 45);
            listView1.LargeImageList.Images.Add(Image.FromFile("Files.png"));
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            treeView1.Nodes.Clear();
           treeView1.Hide();
            listView1.Items.Clear();
            listView1.Hide();
        }
    }
}
