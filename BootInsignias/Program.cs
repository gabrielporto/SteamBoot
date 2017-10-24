using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//add
using System.Web;

namespace BootInsignias
{
    class Program
    {
        static void Main(string[] args)
        {
            WebBrowser browser = new WebBrowser();
            browser.Navigate("http://www.iana.org/domains/example/");
            HtmlDocument doc = browser.Document;
            //doc.InvokeScript("someScript");
            Console.WriteLine(doc.ToString());
        }
    }
}
