using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HeicWork.Helpers;
using Tenorshare.UI.Presentation;
using Tenorshare.UI.Windows.Controls;

namespace HeicWork.Views.Dialogs
{
    /// <summary>
    /// RegisterDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterDialog : ModernWindow
    {
        public RegisterDialog()
        {
            InitializeComponent();
        }

        private void Close_Btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            UtilFunc.OpenWebSite(VersionHelper.GetInstance().GetLink(LinkType.Link_Support));
            LogHelper.WriteWebLog(WebLogWrapper.UserOperation.USER_OPERATION_REGISTER_DLG_CONTACT, "");
        }

        private void Buy_Btn_Click(object sender, RoutedEventArgs e)
        {
            UtilFunc.OpenWebSite(VersionHelper.GetInstance().GetLink(LinkType.Link_Purchase));
            LogHelper.WriteWebLog(WebLogWrapper.UserOperation.USER_OPERATION_REGISTER_DLG_PURCHASE, "1");
        }

        private void Register_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (m_TB_Email.Text == "")
            {
                return;
            }
            if (m_TB_Code.Text == "")
            {
                return;
            }
            string email = m_TB_Email.Text;
            string code = m_TB_Code.Text;
            m_Btn_Register.IsEnabled = false;
            LoadingRing.Visibility = Visibility.Visible;
            LogHelper.WriteWebLogBlock(WebLogWrapper.UserOperation.USER_OPERATION_REGISTER_OPEN);
            (new TaskFactory()).StartNew(() =>
            {
                RegisterHelper reg = RegisterHelper.GetInstance();
                reg.Register(email, code, (WebLogWrapper.RegistrationState state) =>
                {
                    if (state == WebLogWrapper.RegistrationState.REGISTER_RESULT_SUCCESS)
                    {
                        //UserOperationLog.LogMessage(UserOperationEx.USER_OPERATION_REGIST_SUCCESS);
                        //注册成功
                        LogHelper.WriteWebLog(WebLogWrapper.UserOperation.USER_OPERATION_REGISTER_DLG_REGISTER, "1");
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            this.Close();
                            MessageDialog.ShowMessage(LanguageHelper.GetInstance().GetString("register_dialog_success"), App.Current.MainWindow, MessageDialogType.CongruatulationWhitOkTemplate);
                        });
                    }
                    else
                    {
                        //UserOperationLog.LogMessage(UserOperationEx.USER_OPERATION_REGIST_FAILED);
                        //注册失败
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            m_Btn_Register.IsEnabled = true;
                            LoadingRing.Visibility = Visibility.Collapsed;
                            String WarningText = "";
                            switch (state)
                            {
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_CODE_INVALID:
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_NAME_INVALID:
                                    WarningText = LanguageHelper.GetInstance().GetString("register_dialog_failed_invalid");
                                    LogHelper.WriteWebLog(WebLogWrapper.UserOperation.USER_OPERATION_REGISTER_DLG_REGISTER, "2");
                                    break;
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_EXPIRES:
                                    WarningText = LanguageHelper.GetInstance().GetString("register_dialog_failed_expires");
                                    break;
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_PC_LIMIT:
                                    WarningText = LanguageHelper.GetInstance().GetString("register_dialog_failed_limit");
                                    LogHelper.WriteWebLog(WebLogWrapper.UserOperation.USER_OPERATION_REGISTER_DLG_REGISTER, "3");
                                    break;
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_NO_PERMISSIN:
                                    WarningText = LanguageHelper.GetInstance().GetString("register_dialog_failed_permisson");
                                    break;
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_NET_FAILED:
                                    WarningText = LanguageHelper.GetInstance().GetString("register_dialog_failed_network");
                                    break;
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_OTHER:
                                case WebLogWrapper.RegistrationState.REGISTER_RESULT_NONE:
                                default:
                                    WarningText = LanguageHelper.GetInstance().GetString("register_dialog_failed_unkown");
                                    break;
                            }
                            if (WarningText.Length > 60)
                            {
                                this.Hide();
                                if (MessageDialog.ShowMessage(WarningText, this, MessageDialogType.ErrorWithOKTemplate) == MessageDialogResult.OK)
                                {
                                    this.Show();
                                }
                                else
                                {
                                    this.Close();
                                }
                            }
                            else
                            {
                                m_ErrorText.Text = WarningText;
                            }
                        });
                    }
                });
            });
        }
        private void m_window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void Input_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Regex reg_Email = new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            Regex reg_Code = new Regex(@"[0-9 A-Z]{6}-[0-9 A-Z]{6}-[0-9 A-Z]{6}-[0-9 A-Z]{6}-[0-9 A-Z]{8}$");
            if (reg_Email.IsMatch(m_TB_Email.Text.Trim()) == false || reg_Code.IsMatch(m_TB_Code.Text.Trim()) == false)
            {
                m_Btn_Register.IsEnabled = false;
            }
            else
            {
                m_Btn_Register.IsEnabled = true;
            }
        }
    }
}
