using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//add
using System.Windows.Forms;
using Steam;

namespace PegarInsignia
{
    class Program
    {
        public static WebBrowser wb;
        public static List<Badge> insignias;
        public static bool fechar = false;
        public static Form form;


        [STAThread]
        static void Main(string[] args)
        {
            wb = new WebBrowser();
            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wb_DocumentCompleted);

            System.Threading.Thread t = new System.Threading.Thread(ThreadStart);
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            
            while (!fechar)
            {
                //Console.WriteLine("FIM");
            }

        }

        private static void ThreadStart()
        {
            wb.Dock = DockStyle.Fill;
            wb.Name = "webBrowser";
            wb.ScrollBarsEnabled = false;
            wb.TabIndex = 0;
            wb.Navigate("https://steamcommunity.com/login/home/?goto=my/profile", "_self", null, "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");

            form = new Form();
            form.WindowState = FormWindowState.Maximized;
            form.Controls.Add(wb);
            form.Name = "Browser";
            form.Visible = false;
            Application.Run(form);
        }

        public static void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            form.Visible = false;
            HtmlDocument doc = wb.Document;
            string html = wb.DocumentText;

            string url = wb.Url.AbsoluteUri;

            if (url == "https://steamcommunity.com/login/home/?goto=my/profile" ||
          url == "https://store.steampowered.com/login/transfer" ||
          url == "https://store.steampowered.com//login/transfer")
            {
                form.Visible = true;
                Console.WriteLine("Deslogado");
                MessageBox.Show("Deslogado");
            }
            else
            {
                //Alterar isso
                if (url == "http://steamcommunity.com/id/Liwelin/badges/")
                {
                    insignias = new List<Badge>();

                    //List<string> links = new List<string>();

                    foreach (HtmlElement item in doc.All)
                    {
                        if (item.GetAttribute("className") == "badge_row is_link")
                        {
                            insignias.Add(new Badge(item));
                        }
                    }

                    Console.WriteLine("FIM");

                }
                else
                {
                    form.Visible = false;
                    //Alterar isso
                    wb.Navigate("http://steamcommunity.com/id/Liwelin/badges/");
                }

                //MessageBox.Show("Logado");
            }

            //MessageBox.Show("Carregou");
        }

    }
}
