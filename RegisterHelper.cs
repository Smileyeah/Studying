using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebLogWrapper;

namespace HeicWork.Helpers
{

    public class RegistrationStateChangeEventArgs : EventArgs
    {
        public UserInfo User { get; set; }
        public RegistrationState State { get; set; }
    }

    public class RegistionMask
    {
        public string Name { get; set; }
        public string Mask { get; set; }
        public int Level { get; set; }
        public int LimitTime { get; set; }
        public int LimitCount { get; set; }
    }

    public delegate void RegistrationStateChangeEventHandle(object sender, RegistrationStateChangeEventArgs args);

    public class RegisterHelper
    {
        private static readonly object _lock = new object();
        private List<RegistionMask> registionMasks = new List<RegistionMask>();

        private static RegisterHelper Instance;

        public event RegistrationStateChangeEventHandle RegistrationStateChangeEvent;

        private RegistrationState currentState = RegistrationState.REGISTER_RESULT_NONE;

        private RegisterHelper()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source = " + GlobalConfigHelper.dbfile))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    SQLiteHelper sh = new SQLiteHelper(cmd);
                    try
                    {
                        DataTable masks = sh.Select("SELECT * FROM RegMask");
                        registionMasks = TypeConverter.DataTableToList<RegistionMask>(masks);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(ex.Message, ex);
                    }
                    conn.Close();
                }
            }
        }

        public static RegisterHelper GetInstance()
        {
            if (Instance == null)
            {
                lock (_lock)
                {
                    if (Instance == null)
                    {
                        Instance = new RegisterHelper();
                    }
                }
            }
            return Instance;
        }

        public void VerifyCode()
        {
            Task.Factory.StartNew(() =>
            {
                if (RegistrationStateChangeEvent != null)
                {
                    var state = CheckNetworkRegState();
                    if (state == RegistrationState.REGISTER_RESULT_SUCCESS)
                    {
                        UserHelper.GetInstance().UpdateUser(UserHelper.GetInstance().CurrentUserInfo);
                    }
                    else
                    {
                        if (UserHelper.GetInstance().CurrentUserInfo != null)
                        {
                            UserHelper.GetInstance().SetUserEffective(false);
                            UserHelper.GetInstance().UpdateUser(UserHelper.GetInstance().CurrentUserInfo);
                        }
                    }
                    RegistrationStateChangeEvent(this, new RegistrationStateChangeEventArgs() { State = state, User = UserHelper.GetInstance().CurrentUserInfo });
                }
            });
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="code">注册码</param>
        /// <param name="callback">注册回调</param>
        /// <returns></returns>
        public int Register(string name, string code, Action<RegistrationState> callback, bool useNetCheck = true)
        {
            name = name.Trim();
            code = code.Trim();
            int userLevel = VerifyRegistrationCode(name, code);
            RegistionMask mask = registionMasks.Find(p => p.Level == userLevel);
            if (userLevel != -1)
            {
                UserHelper.GetInstance().SetCurrentUserInfo(name, code, false, userLevel, mask.LimitCount, DateTime.Now.ToShortDateString(), mask.LimitTime);
                if (NetworkHelper.IsNetworkAvailable())
                {
                    var state = CheckNetworkRegState();
                    if (state == RegistrationState.REGISTER_RESULT_SUCCESS)
                    {
                        UserHelper.GetInstance().AddUser(UserHelper.GetInstance().CurrentUserInfo);
                        UserHelper.GetInstance().SetUserEffective(true);
                    }
                    if (RegistrationStateChangeEvent != null)
                    {
                        RegistrationStateChangeEvent(this, new RegistrationStateChangeEventArgs() { State = state, User = UserHelper.GetInstance().CurrentUserInfo });
                    }
                    callback(state);
                }
                else
                {
                    callback(RegistrationState.REGISTER_RESULT_NET_FAILED);
                }
            }
            else
            {
                callback(RegistrationState.REGISTER_RESULT_CODE_INVALID);
                return -1;//驗證碼無效
            }
            return 0;
        }


        #region private Property

        private void RaiseStateChangeEvent(RegistrationState state, bool isRegister)
        {
            currentState = state;
            //RegistrationStateChangeEvent?.Invoke(this.GetCurrentRegistration(), isRegister, new RegistrationStateChangeEventArgs() { State = state });
        }

        /// <summary>
        /// 获取网络注册信息
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="code">注册码</param>
        /// <param name="callback">注册回调</param>
        /// <returns></returns>
        private RegistrationState CheckNetworkRegState(bool useNetCheck = true)
        {
            LogHelper.WriteLog("CheckNetworkRegState");
            if (UserHelper.GetInstance().CurrentUserInfo == null)
            {
                return RegistrationState.REGISTER_RESULT_NONE;
            }
            if (!useNetCheck)
            {
                return RegistrationState.REGISTER_RESULT_SUCCESS;
            }
            if (CWebLogWrapper.getInstance() != null)
            {
                LogHelper.WriteWebLogBlock(UserOperation.USER_OPERATION_REGISTER_OPEN);
                for (int i = 5; i > 0; i--)
                {
                    int limitRes = 0;

                    //检测台数限制
                    if (UserHelper.GetInstance().CurrentUserInfo.LimitCount == 0)
                    {
                        limitRes = 1;
                    }
                    else
                    {
                        limitRes = CWebLogWrapper.getInstance().CheckReg(UserHelper.GetInstance().CurrentUserInfo.Code, UserHelper.GetInstance().CurrentUserInfo.LimitCount);
                    }
                    LogHelper.WriteLog("CheckReg return:" + limitRes.ToString());
                    if (limitRes == 0)
                    {
                        return RegistrationState.REGISTER_RESULT_PC_LIMIT;//注册码超过台数限制
                    }

                    //检测时间限制
                    else if (limitRes == 1)
                    {
                        //未加入黑名单
                        if (1 != CWebLogWrapper.getInstance().QueryChargeBack(VersionHelper.GetInstance().CurrentVersion.ProductID, UserHelper.GetInstance().CurrentUserInfo.Name))
                        {
                            if (UserHelper.GetInstance().CurrentUserInfo.LimitTime > 0)//检查注册码时效
                            {
                                string service_reg_time = "";
                                int extern_day = 0;
                                string service_cur_time = "";
                                if (0 == CWebLogWrapper.getInstance().QueryRegTime(VersionHelper.GetInstance().CurrentVersion.ProductID, UserHelper.GetInstance().CurrentUserInfo.Name, ref service_reg_time, ref extern_day, ref service_cur_time))
                                {
                                    DateTime regTime = new DateTime();
                                    DateTime cur_time = new DateTime();
                                    if (DateTime.TryParse(service_reg_time, out regTime) && DateTime.TryParse(service_cur_time, out cur_time))
                                    {
                                        TimeSpan ts = cur_time - regTime;
                                        if (ts.Days > UserHelper.GetInstance().CurrentUserInfo.LimitTime * 365)
                                        {
                                            return RegistrationState.REGISTER_RESULT_EXPIRES;//注册码过期
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            return RegistrationState.REGISTER_RESULT_NAME_INVALID;//检查注册码列入黑名单
                        }
                    }
                }
            }
            return RegistrationState.REGISTER_RESULT_SUCCESS;
        }
        /// <summary>
        /// 注册码校验
        /// </summary>
        /// <param name="set">校验字符集</param>
        /// <param name="name">注册邮箱</param>
        /// <param name="code">注册码</param>
        /// <returns>结果为用户的level, -1为无效</returns>
        private int VerifyRegistrationCode(string name, string code)
        {
            int res = -1;
            foreach (var item in registionMasks)
            {
                byte[] charSet = new byte[256];
                Encoding.ASCII.GetBytes(item.Mask, 0, item.Mask.Length, charSet, 0);
                byte[] data = Encoding.ASCII.GetBytes(name);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] hashTable = md5.ComputeHash(data);
                for (int i = 0; i < 16; i++)
                {
                    hashTable[i] = Convert.ToByte(hashTable[i] ^ charSet[i]);
                }
                StringBuilder resHashTable = new StringBuilder();
                for (int i = 0, j = 0; i < 16; i++)
                {
                    int c = (int)hashTable[i];
                    resHashTable.Append(c.ToString("x2"));
                    if ((i + 1) % 3 == 0 && j < 4 && ((i + 1) / 3 >= 1))
                    {
                        resHashTable.Append("-");
                        j++;
                    }
                }
                if (resHashTable.ToString().ToUpper().CompareTo(code) == 0)
                {
                    res = item.Level;
                    break;
                }
            }
            return res;
        }

        /// <summary>  
        /// AES加密
        /// </summary>  
        /// <param name="plainBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>密文</returns>  
        private string AESEncrypt(String Data, String Key)
        {
            MemoryStream mStream = new MemoryStream();
            RijndaelManaged aes = new RijndaelManaged();

            byte[] plainBytes = Encoding.UTF8.GetBytes(Data);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            //aes.Key = _key;  
            aes.Key = bKey;
            //aes.IV = _iV;  
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }

        /// <summary>  
        /// AES解密
        /// </summary>  
        /// <param name="encryptedBytes">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>明文</returns>  
        private string AESDecrypt(String Data, String Key)
        {
            Byte[] encryptedBytes = Convert.FromBase64String(Data);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(encryptedBytes);
            //mStream.Write( encryptedBytes, 0, encryptedBytes.Length );  
            //mStream.Seek( 0, SeekOrigin.Begin );  
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            //aes.IV = _iV;  
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                byte[] tmp = new byte[encryptedBytes.Length + 32];
                int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
                byte[] ret = new byte[len];
                Array.Copy(tmp, 0, ret, 0, len);
                return Encoding.UTF8.GetString(ret);
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }
        #endregion
    }

}
