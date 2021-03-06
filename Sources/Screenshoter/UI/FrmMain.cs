﻿using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text; 
using System.Windows.Forms;
using Screenshoter.Locale;

namespace Screenshoter.UI
{
    public sealed partial class FrmMain : Form
    {
      
        private string _adbPath;
        private FormWindowState _oldFormState;
        private bool _adbStatus = false;
        private bool _terminatingWithoutAdb = false;
        private bool _allright = false;
        int _developer = 0;

        public FrmMain()
        {
            InitializeComponent();
            Text = Text + @" " + AssemblyVersion;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            labelWaitingForDevice.Text = Strings.DevWa1;
            labelWait2.Text = Strings.DevWa2;
            panel_wait_for_device.Visible = false;
            notifyIcon.BalloonTipTitle = Text; 
            notifyIcon.Text = Text;
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            Resize += FormForTray_Resize;
            AdbCheck();
            Activate();
        }

        #region Core Magic 

        private void AdbCheck()
        {
            if (!File.Exists(TxtAdbPath.Text))
            { 
                ChkAdb.Checked = false;
                ChkAdb.Text = Strings.AdbNotFound;
                var openFileDialog = new OpenFileDialog
                {
                    FileName = "adb.exe",
                    CheckFileExists = true,
                    Multiselect = false,
                    AutoUpgradeEnabled = true,
                    AddExtension = true
                };
                openFileDialog.AutoUpgradeEnabled = true;
                openFileDialog.Filter = @"ADB|adb.exe";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    TxtAdbPath.Text = openFileDialog.FileName;
                    ChkAdb.Checked = true;
                    ChkAdb.Text = Strings.AdbFound;
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Info; 
                    notifyIcon.BalloonTipText =
                        Strings.PreparedToWork;
                    notifyIcon.ShowBalloonTip(30000);
                }
                else
                {

                    if (MessageBox.Show(new Form() { TopMost = true }, Strings.WarnAdbPathIsNull, Strings.Warn, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        AdbCheck();
                    }
                    else
                    {
                        Visible = false;
                        notifyIcon.BalloonTipIcon = ToolTipIcon.None;
                        Application.Exit();
                    }
                }
            }
            else
            { 
                TxtAdbPath.Text = Path.Combine(Application.StartupPath, "adb.exe");
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipText =
                    Strings.PreparedToWork;

                notifyIcon.ShowBalloonTip(30000);
                ChkAdb.Checked = true;
                ChkAdb.Text = Strings.AdbFound; 
            }
        }

    

        private void BtnCustomDeviceSelect_Click(object sender, EventArgs e)
        { 
            _adbStatus = true;
            var frmadb = new FrmAdb(TxtAdbPath.Text);
            frmadb.ShowDialog();
        }

        private void BtnAdbPath_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                FileName = "adb.exe",
                CheckFileExists = true,
                Multiselect = false,
                AutoUpgradeEnabled = true,
                AddExtension = true
            };
            openFileDialog.AutoUpgradeEnabled = true;
            openFileDialog.Filter = @"ADB|adb.exe";
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            TxtAdbPath.Text = openFileDialog.FileName;
            ChkAdb.Checked = true;
            ChkAdb.Text = Strings.AdbFound;
        }

        

        private void BtnTakeMeToChurch_Click(object sender, EventArgs e)
        {
           
            _adbStatus = true;
            BtnTakeMeToChurch.Enabled = false;
            _adbPath = TxtAdbPath.Text;
            var ip = IpCustomDeviceIp.Text;
            var port = TxtCustomDevicePort.Text;
            var rnd = new Random();
            var addToName = rnd.Next(1, 9999);
            var path = TxtScreenPath.Text;
            var screenshot = TxtScreenFilename.Text;
            var finalfilepath = TxtCustomPath.Text;
            if (ChkCustomDevice.Checked)
            {
                if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(port))
                {
                    _allright = false; 
                    // adb connect [host:port]
                    try
                    {
                        var pasd = new Process
                        {
                            StartInfo =
                            {
                                FileName = _adbPath,
                                Arguments = "wait-for-device",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true,
                                StandardOutputEncoding = Encoding.GetEncoding(866)
                            }
                        };
                        pasd.Start();
                        panel_wait_for_device.Visible = true;
                        pasd.WaitForExit();
                        panel_wait_for_device.Visible = false;
                        var p0 = new Process
                        {
                            StartInfo =
                            {
                                Arguments = "connect " + ip + ":" + port,
                                UseShellExecute = false,
                                FileName = _adbPath,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true,
                                StandardOutputEncoding = Encoding.GetEncoding(866)
                            }
                        };
                        p0.Start();
                        p0.WaitForExit();
                        _allright = true;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
                        BtnTakeMeToChurch.Enabled = true;
                        _allright = false;
                    }
                }
                else
                {
                    MessageBox.Show(Strings.ErrorIpPort, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    BtnTakeMeToChurch.Enabled = true;
                    _allright = false;
                    return;
                }
            }
            //adb shell screencap -p / sdcard / screenshot_%random%.png
            //MessageBox.Show(_adbPath + " shell screencap -p " + path + screenshot + "_" + addToName + ".png");
            
            try
            {
                _allright = false;
                var pasd = new Process
                {
                    StartInfo =
                    {
                        FileName = _adbPath,
                        Arguments = "wait-for-device",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(866)
                    }
                }; 
                pasd.Start();
                panel_wait_for_device.Visible = true;
                pasd.WaitForExit();
                panel_wait_for_device.Visible = false;
                var p1 = new Process
                {
                    StartInfo =
                    {
                        Arguments = "shell screencap -p " + path + screenshot + "_" + addToName + ".png",
                        UseShellExecute = false,
                        FileName = _adbPath,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(866)
                    }
                };
                p1.Start();
                p1.WaitForExit();
                _allright = true;
            }
            catch (Exception ex1)
            {
                MessageBox.Show(ex1.ToString(), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                BtnTakeMeToChurch.Enabled = true;
                _allright = false;
            }
            //adb pull / sdcard / screenshot_%random%.png
            //MessageBox.Show(_adbPath + " pull " + path + screenshot + "_" + addToName + ".png");
            try
            {
                _allright = false;
                var p2 = new Process
                {
                    StartInfo =
                    {
                        Arguments = "pull " + path + screenshot + "_" + addToName + ".png",
                        UseShellExecute = false,
                        FileName = _adbPath,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(866)
                    }
                };
                p2.Start();
                p2.WaitForExit();
                _allright = true;
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.ToString(), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                BtnTakeMeToChurch.Enabled = true;
                _allright = false;
            }
            //adb shell rm / sdcard / screen_10_ % _rand %.png
            //MessageBox.Show(_adbPath + " shell rm " + path + screenshot + "_" + addToName + ".png");
            try
            {
                _allright = false;
                var p3 = new Process
                {
                    StartInfo =
                    {
                        Arguments = "shell rm " + path + screenshot + "_" + addToName + ".png",
                        UseShellExecute = false,
                        FileName = _adbPath,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.GetEncoding(866)
                    }
                };
                p3.Start();
                p3.WaitForExit();
                _allright = true;
            }
            catch (Exception ex3)
            {
                MessageBox.Show(ex3.ToString(), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                BtnTakeMeToChurch.Enabled = true;
                _allright = false;
            }
            if (ChkCustomPath.Checked)
            {
                if (string.IsNullOrWhiteSpace(finalfilepath))
                {
                    MessageBox.Show(Strings.ErrorSaving,
                        Strings.Error, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    BtnTakeMeToChurch.Enabled = true; 
                }
                else
                {

                    //MessageBox.Show(Path.Combine(finalfilepath, screenshot + "_" + addToName + ".png"));
                    try
                    {
                        if (!_allright)
                        {
                            MessageBox.Show(@"!_allright", Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        else
                        {
                              File.Move(screenshot + "_" + addToName + ".png",
                            Path.Combine(finalfilepath, screenshot + "_" + addToName + ".png")); // Try to move
                        }
                      
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        BtnTakeMeToChurch.Enabled = true;
                    }
                }
            }
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipTitle = Text;
            notifyIcon.BalloonTipText = Strings.ScreenshotDone; 
            notifyIcon.ShowBalloonTip(10000);
            var str = Properties.Resources.Windows_Error8;
            var snd = new SoundPlayer(str);
            snd.Play();
            BtnTakeMeToChurch.Enabled = true;
        }

        private void BtnCustomPath_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop,
                ShowNewFolderButton = true
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                TxtCustomPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        #endregion
        #region UI Magic
        private void ChkAdb_CheckedChanged(object sender, EventArgs e)
        {
           
            ChkAdb.Checked = true;
           
           
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon.Visible = false;
        }

        private static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            FromMinimize();
        }

        private void FormForTray_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized != WindowState) return;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipText = Strings.ProgrammMinimized;
            notifyIcon.ShowBalloonTip(30000);
            Hide();
        }

        private void FromMinimize()
        {
            if (WindowState == FormWindowState.Normal || WindowState == FormWindowState.Maximized)
            {
                _oldFormState = WindowState;
            }
            else
            {
                Show();
                WindowState = _oldFormState;
            }
        }

        private void ChkCustomDevice_CheckedChanged(object sender, EventArgs e)
        {
            IpCustomDeviceIp.Enabled = TxtCustomDevicePort.Enabled = ChkCustomDevice.Checked;
        }

        private void ChkCustomPath_CheckedChanged(object sender, EventArgs e)
        {
            GbCustomPath.Enabled = ChkCustomPath.Checked;
        }



        #endregion

        private void AdbKiller()
        {
            if (_adbStatus == true)
            {
                try
                {
                    _adbPath = TxtAdbPath.Text;
                    var p1 = new Process
                    {
                        StartInfo =
                        {
                            Arguments = "kill-server",
                            UseShellExecute = false,
                            FileName = _adbPath,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.GetEncoding(866)
                        }
                    };

                    p1.Start();
                    p1.WaitForExit();
                    _adbStatus = false;
                    MessageBox.Show(Strings.AdbServerStopped, Strings.Done, MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                catch (Exception ex1)
                {
                   
                        MessageBox.Show(ex1.ToString(), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                 
                }
        }
            else
            {
                if (_terminatingWithoutAdb == false){
                MessageBox.Show(Strings.AdbServerNotStopped, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _terminatingWithoutAdb = true;
            notifyIcon.BalloonTipIcon = ToolTipIcon.None;
            AdbKiller();
        }

        private void BtnAdbKill_Click(object sender, EventArgs e)
        {
            _terminatingWithoutAdb = false;
            AdbKiller();
        }

        private void ChkAdb_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            _developer = _developer + 1;
            if (_developer == 7)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipText = Strings.EasterEgg1 + @" (3).";
                notifyIcon.ShowBalloonTip(500);

            }
            if (_developer == 8)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipText = Strings.EasterEgg1 + @" (2).";
                notifyIcon.ShowBalloonTip(500);
            }
            if (_developer == 9)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipText = Strings.EasterEgg1 + @" (1).";
                notifyIcon.ShowBalloonTip(500);
            }
            if (_developer == 10)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipText = Strings.EasterEgg2;
                notifyIcon.ShowBalloonTip(500);
            }
            if (_developer > 10)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.BalloonTipText = Strings.EasterEgg3;
                notifyIcon.ShowBalloonTip(500);
            }
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            var frmabt = new FrmAbout();
            frmabt.ShowDialog();
        }

    }
}