using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using iTransGo.Views.Dialogs;
using Tenorshare.UI.Presentation;
using Tenorshare.Util;
using TunesCareDLL;

namespace Test.ViewModels
{
    public class iTunesRepairDialogViewModel:ViewModelBase
    {
        private DateTime latestRaiseTime = DateTime.Now;
        private double latestRaiseSize = 0;

        private Task RepairTask;

        private double downloadProgressValue;
        public double DownloadProgressValue {
            get {
                return downloadProgressValue;
            }
            set {
                downloadProgressValue = value;
                RaisePropertyChanged("DownloadProgressValue");
            }
        }

        private double downloadedFileSize;
        public double DownloadedFileSize {
            get {
                return downloadedFileSize;
            }
            set {
                downloadedFileSize = value;
                RaisePropertyChanged("DownloadedFileSize");
            }
        }

        private double totalFileSize;
        public double TotalFileSize {
            get {
                return totalFileSize;
            }
            set {
                totalFileSize = value;
                RaisePropertyChanged("TotalFileSize");
            }
        }

        private double downloadSpeed;
        public double DownloadSpeed {
            get {
                return downloadSpeed;
            }
            set {
                downloadSpeed = value;
                RaisePropertyChanged("DownloadSpeed");
            }
        }

        private string installMessage;
        public string InstallMessage {
            get {
                return installMessage;
            }
            set {
                installMessage = value;
                RaisePropertyChanged("InstallMessage");
            }
        }
        
        private iTunesRepairState state;
        public iTunesRepairState iTunesRepairStatus {
            get {
                return state;
            }
            set {
                if (state != value) {
                    state = value;
                    RaisePropertyChanged("iTunesRepairStatus");
                }
            }
        }

        public RelayCommand CloseDialogCommand {
            get {
                return new RelayCommand((p) => {
                    if (dialog != null) {
                        dialog.Close();
                    }
                });
            }
        }

        private int downcountnumber;

        public String DownCountNumber {
            get {
                if (downcountnumber > 0) {
                    return downcountnumber.ToString();
                }
                else {
                    return "0";
                }
            }
        }

        public bool BeginiTunesRepair() {
            String DownloadPath = System.IO.Path.Combine(Common.GetHardDiskLargestFreeSpace(), "Test") + "\\" ;
            if (!Directory.Exists(DownloadPath)) {
                Directory.CreateDirectory(DownloadPath);
            }
            DownloadPath += "iTunesSetup.exe";
            bool isDownloadSuccess = true;
            int noSpeedCount = 0;
            RepairTask = Task.Factory.StartNew(() => {
                TunesCareWrapper.DownloadItunes(DownloadPath, (p, q) => {
                    if (noSpeedCount > 10) {
                        if (iTunesRepairStatus == iTunesRepairState.Downloading) {
                            Thread.Sleep(1000);
                        }
                        return;
                    }
                    DispatcherHelper.CheckBeginInvokeOnUI(() => {
                        DownloadProgressValue = Convert.ToDouble(p) / Convert.ToDouble(q) * 100;
                        DownloadedFileSize = Convert.ToDouble(p);
                        TotalFileSize = Convert.ToDouble(q);
                        if (DateTime.Now - latestRaiseTime > new TimeSpan(0, 0, 1)) {
                            DownloadSpeed = DownloadedFileSize - latestRaiseSize;
                            latestRaiseSize = DownloadedFileSize;
                            latestRaiseTime = DateTime.Now;
                            if (DownloadSpeed > 0) {
                                noSpeedCount = 0;
                            }
                            else {
                                noSpeedCount++;
                            }
                        }
                    });
                });
            });
            RepairTask.ContinueWith((p) => {
                if (isDownloadSuccess) {
                    string InstallPath = Path.Combine(Path.GetDirectoryName(DownloadPath), Path.GetFileNameWithoutExtension(DownloadPath));
                    Directory.CreateDirectory(InstallPath);
                    int res = TunesCareWrapper.InstallItunes(DownloadPath, InstallPath, (q, msg, x, y) => {
                            InstallMessage = msg;
                    });
                    if (res == 0) {
                        downcountnumber = 5;
                        Thread.Sleep(1000);
                        BeginDownCount();
                    }
                    else {
                        Thread.Sleep(1000);
                        BeginDownCount();
                    }
                }
            });
            return true;
        }

        private void BeginDownCount() {
            Task.Factory.StartNew(() => {
                while (downcountnumber > 0) {
                    downcountnumber--;
                    DispatcherHelper.CheckBeginInvokeOnUI(() => {
                        RaisePropertyChanged("DownCountNumber");
                    });
                    Thread.Sleep(1000);
                }
                DispatcherHelper.CheckBeginInvokeOnUI(() => {
                    dialog.Close();
                });
            });
        }
    }
}
