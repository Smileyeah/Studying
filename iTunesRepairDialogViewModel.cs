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

namespace iTransGo.ViewModels
{
    public enum iTunesRepairState {
        Initialize,
        StartDownload,
        Downloading,
        DownloadTimeOut,
        DownloadFinished,
        DownloadStop,
        StartInstall,
        Installing,
        InstallSucceed,
        InstallFailed
    }

    public class iTunesRepairDialogViewModel:ViewModelBase
    {
        private DateTime latestRaiseTime = DateTime.Now;
        private double latestRaiseSize = 0;
        private DateTime latestHeartbeat = DateTime.Now;

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

        public iTunesRepairDialog dialog;

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

        public ContentControl Content;

        public bool BeginiTunesRepair() {
            iTunesRepairStatus = iTunesRepairState.StartDownload;
            String DownloadPath = System.IO.Path.Combine(Common.GetHardDiskLargestFreeSpace(), "Tenorshare", "iTransGo") + "\\" ;
            if (!Directory.Exists(DownloadPath)) {
                Directory.CreateDirectory(DownloadPath);
            }
            DownloadPath += "iTunesSetup.exe";
            bool isDownloadSuccess = true;
            int noSpeedCount = 0;
            RepairTask = Task.Factory.StartNew(() => {
                DispatcherHelper.CheckBeginInvokeOnUI(() => {
                    iTunesRepairStatus = iTunesRepairState.Downloading;
                    GifPlay();
                });
                TunesCareWrapper.DownloadItunes(DownloadPath, (p, q) => {
                    if (noSpeedCount > 10) {
                        if (iTunesRepairStatus == iTunesRepairState.Downloading) {
                            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                                iTunesRepairStatus = iTunesRepairState.DownloadTimeOut;
                            });
                            Thread.Sleep(1000);
                            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                                GifPlay(false);
                            });
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
                        DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            iTunesRepairStatus = iTunesRepairState.Installing;
                            GifPlay();
                            InstallMessage = msg;
                        });
                    });
                    if (res == 0) {
                        downcountnumber = 5;
                        DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            iTunesRepairStatus = iTunesRepairState.InstallSucceed;
                        });
                        Thread.Sleep(1000);
                        BeginDownCount();
                        DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            GifPlay(false);
                        });
                    }
                    else {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            iTunesRepairStatus = iTunesRepairState.InstallFailed;
                        });
                        Thread.Sleep(1000);
                        BeginDownCount();
                        DispatcherHelper.CheckBeginInvokeOnUI(() => {
                            GifPlay(false);
                        });
                    }
                }
            });
            return true;
        }

        private void GifPlay(bool isReplay = true) {
            MediaElement temp = Helpers.VisualTreeHelper.GetChildObject<MediaElement>(Content, "m_gif");
            temp.Play();
            if (isReplay) {
                temp.MediaEnded += (sender, e) => {
                    MediaElement media = (MediaElement)sender;
                    media.Position = TimeSpan.FromMilliseconds(1);
                    media.Play();
                };
            }
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
