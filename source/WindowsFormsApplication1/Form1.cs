using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices; // setforegraoundwindow

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string keycode;
        string mausu;

        public Form1()
        {
            InitializeComponent();
            comboBox1.DataSource = ProcessTable();
            comboBox1.ValueMember = "PID";
            comboBox1.DisplayMember = "NAME";
            textBox1.Text = "Sample.txt";
        }
        RamGecTools.MouseHook mouseHook = new RamGecTools.MouseHook();
        RamGecTools.KeyboardHook keyboardHook = new RamGecTools.KeyboardHook();

        // http://www.atmarkit.co.jp/fdotnet/dotnettips/024w32api/w32api.html
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("ファイルを開いてください", "Error");
            }
            else
            {
                keycode = "a";
                mausu = "a";
                //キーボードフック
                keyboardHook.KeyDown += new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
                keyboardHook.Install();
                mouseHook.Install();
                // 選択しているプロセスをアクティブ
                int pid = int.Parse(comboBox1.SelectedValue.ToString());
                Process p = Process.GetProcessById(pid);
                SetForegroundWindow(p.MainWindowHandle);
                // キーストロークを送信
                int counter = 0;
                string line;
                // Read the file and display it line by line.
                System.IO.StreamReader file = new System.IO.StreamReader(textBox1.Text);

                //1行ずつ処理
                while ((line = file.ReadLine()) != null)
                {
                    if (keycode == "LCONTROL" || mausu == "osareta")
                    {
                        break;
                    }
                    else
                    {
                        SendKeys.Send(line);
                        SendKeys.Send("{Enter}");
                        counter++;
                    }
                }

                file.Close();
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // プロセス一覧を更新
            comboBox1.DataSource = ProcessTable();
            comboBox1.ValueMember = "PID";
            comboBox1.DisplayMember = "NAME";
        }
        private DataTable ProcessTable()
        {
            // プロセスのリストを取得
            // http://d.hatena.ne.jp/tomoemon/20080430/p2
            Process[] ps = Process.GetProcesses();
            Array.Sort(ps, new ProcComparator());

            DataTable table = new DataTable();
            table.Columns.Add("PID");
            table.Columns.Add("NAME");

            foreach (Process p in ps)
            {
                DataRow row = table.NewRow();
                row.SetField<int>("PID", p.Id);
                row.SetField<string>("NAME", p.ProcessName + " - " + p.MainWindowTitle);

                table.Rows.Add(row);
            }
            table.AcceptChanges();

            return table;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //ファイルを開く
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = false,  // 複数選択の可否
                Filter =  // フィルタ
                "テキストファイル|*.txt|全てのファイル|*.*",
            };

            //ダイアログを表示
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                //パスをtextBoxに設定
                textBox1.Text = dialog.FileName;
            }
        }

        void keyboardHook_KeyDown(RamGecTools.KeyboardHook.VKeys key)
        {
            keycode = key.ToString();
        }

        void mouseHook_MiddleButtonDown(RamGecTools.MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            mausu = "osareta";
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // there's no harm to call Uninstall method repeatedly even if hooks aren't installed
            keyboardHook.Uninstall();
            mouseHook.Uninstall();
        }

    }
    // プロセス名でソート ... for Array.Sort
    public class ProcComparator : IComparer<Process>
    {
        public int Compare(Process p, Process q)
        {
            return p.ProcessName.CompareTo(q.ProcessName);
        }
    }
}
