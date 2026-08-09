using System.Security.Cryptography;
using System.Text;

namespace BaZi.Services {

    /// <summary>提供資料加密與解密服務 (AES128)</summary>
    public class SecurityService {

        #region Definitions
        private static readonly log4net.ILog LOG4N = log4net.LogManager.GetLogger(nameof(SecurityService));
        #endregion

        #region Fields
        private byte[] mAesKey = [];
        private byte[] mAesIV = [];
        #endregion

        #region Constructor
        public SecurityService() {
            InitializeKeyAndIV();
        }
        #endregion

        #region Methods
        /// <summary>初始化 AES 的 Key 和 IV</summary>
        private void InitializeKeyAndIV() {
            try {
                var keyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "encrypto.key");
                /* 如果檔案存在，則 [0]Key [1]IV */
                if (File.Exists(keyFile)) {
                    var lines = File.ReadAllLines(keyFile);
                    if (lines.Length >= 2) {
                        mAesKey = Convert.FromBase64String(lines[0]);
                        mAesIV = Convert.FromBase64String(lines[1]);
                        return;
                    }
                }

                /* 上面用 return 直接離開，到這裡表示還沒有 encrypto.key 檔
                    * 產生隨機 16 bytes 並存檔 */
                using (var aes = Aes.Create()) {
                    aes.KeySize = 128;
                    aes.GenerateKey();
                    aes.GenerateIV();
                    mAesKey = aes.Key;
                    mAesIV = aes.IV;

                    File.WriteAllLines(
                        keyFile,
                        [
                            Convert.ToBase64String(mAesKey),
                        Convert.ToBase64String(mAesIV)
                        ]
                    );
                }
            } catch (Exception ex) {
                LOG4N.Error("初始化加密金鑰失敗，使用預設值", ex);
                mAesKey = Encoding.UTF8.GetBytes("BaZi123456789012");
                mAesIV = Encoding.UTF8.GetBytes("1234567890123456");
            }
        }

        /// <summary>將 byte[] 使用 AES 加密成 <see cref="string"/></summary>
        /// <param name="data">欲加密的串流資料</param>
        /// <returns>AES 加密後的 Base64 字串</returns>
        public string Encrypt(byte[] data) {
            using (var aes = Aes.Create()) {
                aes.Key = mAesKey;
                aes.IV = mAesIV;

                using (var enc = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write)) {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>將 <see cref="string"/> 使用 AES 解密回 byte[]</summary>
        /// <param name="cipherText">欲解密的 Base64 字串</param>
        /// <returns>解密後的資料</returns>
        public byte[] Decrypt(string cipherText) {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create()) {
                aes.Key = mAesKey;
                aes.IV = mAesIV;

                using (var dec = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var msIn = new MemoryStream(fullCipher))
                using (var cs = new CryptoStream(msIn, dec, CryptoStreamMode.Read))
                using (var msOut = new MemoryStream()) {
                    cs.CopyTo(msOut);
                    return msOut.ToArray();
                }
            }
        }
        #endregion
    }
}
