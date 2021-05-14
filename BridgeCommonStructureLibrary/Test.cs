using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using CSL.Bridge;
using CSL.Bridge.Crypto;
using CSL.Data;
using CSL.Encryption;

namespace CSL
{
    public class Test
    {
        public static void Main()
        {
            Document.Body.AppendChild(new HTMLParagraphElement() { TextContent = "Encryption Key" });
            HTMLInputElement KeyBox = new HTMLInputElement();
            Document.Body.AppendChild(KeyBox);
            Document.Body.AppendChild(new HTMLParagraphElement() { TextContent = "Text Key" });
            HTMLInputElement TextBox = new HTMLInputElement();
            Document.Body.AppendChild(TextBox);
            HTMLInputElement EncryptButton = new HTMLInputElement() { Type = InputType.Button, Value = "Encrypt" };
            HTMLInputElement DecryptButton = new HTMLInputElement() { Type = InputType.Button, Value = "Decrypt" };
            Document.Body.AppendChild(new HTMLBRElement());
            Document.Body.AppendChild(EncryptButton);
            Document.Body.AppendChild(DecryptButton);
            EncryptButton.OnClick = async (MouseEvent<HTMLInputElement> ev) =>
            {
                try
                {
                    string keybase = KeyBox.Value;
                    if (string.IsNullOrWhiteSpace(keybase))
                    {
                        using (AES256KeyBasedProtector protector = new AES256KeyBasedProtector((key)=> new AesGcm(key) ))
                        {
                            KeyBox.Value = Convert.ToBase64String(protector.GetKey());
                            string PlaintextString = TextBox.Value;
                            TextBox.Value = await protector.Protect(PlaintextString);
                        }
                    }
                    else
                    {
                        using (AES256KeyBasedProtector protector = new AES256KeyBasedProtector(Convert.FromBase64String(keybase), (key) => new AesGcm(key)))
                        {
                            string PlaintextString = TextBox.Value;
                            TextBox.Value = await protector.Protect(PlaintextString);
                        }
                    }
                }
                catch(Exception e)
                {
                    Window.Alert("An error occurred while Encrypting!" + Environment.NewLine + e.Message);
                }
            };
            DecryptButton.OnClick = async (MouseEvent<HTMLInputElement> ev) =>
            {
                string keybase = KeyBox.Value;
                if (string.IsNullOrWhiteSpace(keybase))
                {
                    Window.Alert("There's no key to decrypt with!");
                }
                else
                {
                    using (AES256KeyBasedProtector protector = new AES256KeyBasedProtector(Convert.FromBase64String(keybase), (key) => new AesGcm(key)))
                    {
                        string CryptoTextString = TextBox.Value;
                        TextBox.Value = await protector.Unprotect(CryptoTextString);
                    }
                }
            };
        }
    }
}
