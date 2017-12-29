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
    public partial class Form2 : Form
    {

       static Form1 f1;



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


        int count_error; //счётчик неправильных вводов пароля
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            f1 = (Form1)this.Parent;
            if (textBox1.Text == "") { MessageBox.Show("Старый пароль не введён!"); return; }
            if (textBox2.Text == "") { MessageBox.Show("Пароль не введён!"); return; }
           
            //введённый старый пароль
            String oldpassword = Form1.GetMd5Hash(textBox1.Text);

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


           
         //сравниваем побитно хеш старого пароля и нового
            for (int ii = 0; ii < 32; ii++)
                if (Form1.buffer0sector[384 + ii] != (byte)oldpassword[ii])
                {
                    MessageBox.Show("Старый пароль введён неверно!");
                    textBox1.SelectAll();
                    textBox1.Focus();
                    count_error++;
                    if (count_error == 3)
                    {
                        textBox1.ReadOnly = true;
                        button1.Enabled = false;
                        MessageBox.Show("У Вас закончились попытки ввести пароль!");
                    }
                 //   goto metka_error;
                    return;
                }
               
                {


                    //записываем пароль в нулевой сектор
                    handle = CreateFile(
                    lpFileName: @"\\.\" + Form1.drive[0] + ":",
                    dwDesiredAccess: FileAccess.Write,
                    dwShareMode: FileShare.ReadWrite,
                    lpSecurityAttributes: IntPtr.Zero,
                    dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                    dwFlagsAndAttributes: FileAttributes.Normal,
                    hTemplateFile: IntPtr.Zero);

                    using (FileStream disk = new FileStream(handle, FileAccess.Write))
                    {

                        String hashpassword = Form1.GetMd5Hash(textBox2.Text);
                        for (int i = 0; i < hashpassword.Length; i++)
                            Form1.buffer0sector[384 + i] = (byte)hashpassword[i];

                        disk.Write(Form1.buffer0sector, 0, 512);



                    }


                    MessageBox.Show("Пароль успешно изменён!");
                    
                }
          //  metka_error:
            //    { }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
           f1 = (Form1)this.Parent;
            count_error = 0; //счётчик неправильных вводов пароля
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
