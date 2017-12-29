using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices; //классы отвечающие за работу с внешними ДЗБ
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Security.Cryptography; //тут лежит мд5 шифрование



namespace Lab2
{
    public partial class Form1 : Form
    {
        static public String drive;
        
        
        public Form1()
        {
            InitializeComponent();

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 


      //1. подключение кернел32.длл
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
    //

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 


        //переменная массив байт куда будем загружать нулевой сектор
      public static  byte[] buffer0sector;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

        //функция чтения 0 сектора
        void read0sector()
        {
          
           // Выбираем диск
         //    metka:
        //     if  ( folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            while (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
             {
                 MessageBox.Show("Выберите правильный путь!");
              //   goto metka;
             }
             drive = folderBrowserDialog1.SelectedPath;

        //настройка функции из кернел.длл
            SafeFileHandle handle = CreateFile(
            lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
            dwDesiredAccess: FileAccess.Read,
            dwShareMode: FileShare.ReadWrite,
            lpSecurityAttributes: IntPtr.Zero,
            dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
            dwFlagsAndAttributes: FileAttributes.Normal,
            hTemplateFile: IntPtr.Zero);

           //чтение нулевого сектора
            using (FileStream disk = new FileStream(handle, FileAccess.Read))
            {
                buffer0sector = new byte[512];

                
                disk.Read(buffer0sector, 0, 512);

                //выводим нулевой сектор в таблицу
                for (int i=0; i<64; i++)
                {
                    dataGridView1.Rows.Add(buffer0sector[i*8+0],buffer0sector[i*8+1],buffer0sector[i*8+2],buffer0sector[i*8+3],buffer0sector[i*8+4],buffer0sector[i*8+5],buffer0sector[i*8+6],buffer0sector[i*8+7]);

                }

                for (int i = 384; i < 416; i++)
                {
             //       textBox1.Invoke(new Action(() => { textBox1.Text += (char)mbrData[i]; }));
                }

                MessageBox.Show("Нулевой сектор прочитан");

            }
        }





/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        //кнопка 1 чтение нулевого сектора
        private void button1_Click(object sender, EventArgs e)
        {
            
            dataGridView1.Rows.Clear();
            read0sector();
            //найдём сумму байт в секторах с 384 по 416 чтобы узнать есть ли там пароль
            int summa = 0;
            for (int i = 384; i < 416; i++)
                summa += buffer0sector[ i];
            //если сумма отличается от нуля, значит пароль в нулевом секторе есть
            if (summa == 0)
            {
                MessageBox.Show("Пароля в нулевом секторе нет! Задайте пароль!");
            }
            //закрасим место с паролем
            for (int i = 48; i < 52; i++)
                for (int j = 0; j < 8; j++)
                    dataGridView1[j, i].Style.BackColor = Color.Red;

            radioButton1.Checked = true;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
      
        //кнопка 2 сохранение пароля
        private void button2_Click(object sender, EventArgs e)
        {
          //  if (textBox1.Text == "") { MessageBox.Show("Пароль не введён!"); return; }

             // Выбираем диск
     //   metka:
           // if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
           while (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Выберите правильный путь!");
       //         goto metka;
            }

        drive = folderBrowserDialog1.SelectedPath;
            
            //читаем информацию из нулевого сектора для проверки есть ли пароль
            SafeFileHandle handle = CreateFile(
lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
dwDesiredAccess: FileAccess.Read,
dwShareMode: FileShare.ReadWrite,
lpSecurityAttributes: IntPtr.Zero,
dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
dwFlagsAndAttributes: FileAttributes.Normal,
hTemplateFile: IntPtr.Zero);

            using (FileStream disk = new FileStream(handle, FileAccess.Read))
            {
                buffer0sector = new byte[512];


                disk.Read(buffer0sector, 0, 512);
            }
            //найдём сумму байт в секторах с 384 по 416 чтобы узнать есть ли там пароль
            int summa = 0;
            for (int i = 384; i < 416; i++)
                summa += buffer0sector[i];
            //если сумма отличается от нуля, значит пароль в нулевом секторе есть
            if (summa > 0)
            {
                //пароль есть и его нужно проверить и изменить
                //создаем новое окно
                Form2 f2 = new Form2();
                f2.ShowDialog();
            }
            else
            {

                //записываем пароль в нулевой сектор
                handle = CreateFile(
                lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
                dwDesiredAccess: FileAccess.Write,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                hTemplateFile: IntPtr.Zero);

                using (FileStream disk = new FileStream(handle, FileAccess.Write))
                {

                    String hashpassword = GetMd5Hash(textBox1.Text);
                    for (int i = 0; i < hashpassword.Length; i++)
                        buffer0sector[384 + i] = (byte)hashpassword[i];

                    disk.Write(buffer0sector, 0, 512);
                    MessageBox.Show("Пароль записан в нулевой сектор!");


                }


            }


        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
       
        //кнопка 3 очистка от пароля
        private void button3_Click(object sender, EventArgs e)
        {


                 // Выбираем диск
      //  metka:
          //  if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            while (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Выберите правильный путь!");
        //        goto metka;
            }
        drive = folderBrowserDialog1.SelectedPath;
            //читаем информацию из нулевого сектора для проверки есть ли пароль
            SafeFileHandle handle = CreateFile(
lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
dwDesiredAccess: FileAccess.Read,
dwShareMode: FileShare.ReadWrite,
lpSecurityAttributes: IntPtr.Zero,
dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
dwFlagsAndAttributes: FileAttributes.Normal,
hTemplateFile: IntPtr.Zero);

            using (FileStream disk = new FileStream(handle, FileAccess.Read))
            {
                buffer0sector = new byte[512];


                disk.Read(buffer0sector, 0, 512);
            }
          

                //записываем пустой пароль в нулевой сектор
                handle = CreateFile(
                lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
                dwDesiredAccess: FileAccess.Write,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                hTemplateFile: IntPtr.Zero);

                using (FileStream disk = new FileStream(handle, FileAccess.Write))
                {

                   
                    for (int i = 0; i < 32; i++)
                        buffer0sector[384 + i] = 0;

                    disk.Write(buffer0sector, 0, 512);
                    MessageBox.Show("Пароль удалён из нулевого сектора!");


                }


            }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
      
        //расчёт хэша мд5
      public  static string GetMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
   
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
 
        private void Form1_Load(object sender, EventArgs e)
        {
            // добавляем столбцы  датагрид
            for (int i = 0; i < 8; i++)
                dataGridView1.Columns.Add(i.ToString(), i.ToString());

        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
        
        //выбор режима отображения таблицы нулевого сектора
        void vibor(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0) return;
           if (radioButton1.Checked) 
            for (int i = 0; i < 64; i++)
                for (int j = 0; j < 8; j++)
                    dataGridView1[j, i].Value = buffer0sector[i*8+j];

           if (radioButton2.Checked)
               for (int i = 0; i < 64; i++)
                   for (int j = 0; j < 8; j++)
                       dataGridView1[j, i].Value = Convert.ToString(buffer0sector[i * 8 + j], 2);

           if (radioButton3.Checked)
               for (int i = 0; i < 64; i++)
                   for (int j = 0; j < 8; j++)
                       dataGridView1[j, i].Value = (char)buffer0sector[i * 8 + j];

           if (radioButton4.Checked)
               for (int i = 0; i < 64; i++)
                   for (int j = 0; j < 8; j++)
                       dataGridView1[j, i].Value = Convert.ToString(buffer0sector[i * 8 + j], 16);
        
        
        }

        private void button4_Click(object sender, EventArgs e)
        {
        // Выбираем диск
      //  metka:
      //      if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            while (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Выберите правильный путь!");
           //     goto metka;
            }
        drive = folderBrowserDialog1.SelectedPath;
            Form3 f3 = new Form3();
            this.Hide();
            f3.ShowDialog();
            this.Show();
        }

     
    }
}
