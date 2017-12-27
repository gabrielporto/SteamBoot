using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//add
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;

namespace Steam
{

    class SQLite
    {
        private SQLiteConnection m_dbConnection;
        public SQLite()
        {
            m_dbConnection = new SQLiteConnection(@"Data Source=..\..\DB\MyDatabase.sqlite;Version=3;");
        }
        public DataTable ExecuteReader(string sql)
        {
            SQLiteDataReader reader = null;
            DataTable dt = new DataTable();

            AbrirConexao();
            try
            {
                SQLiteCommand comand = new SQLiteCommand(sql, m_dbConnection);
                reader = comand.ExecuteReader();
                dt.Load(reader);
                return dt;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            finally
            {
                FecharConexao();
            }

            return dt;
        }
        public void ExecuteNonQuery(string sql)
        {
            try
            {
                AbrirConexao();
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                //Ja Inserido
                if (ex.ErrorCode != 19)
                {
                    Console.WriteLine(ex);
                }
            }
            finally { FecharConexao(); }

        }
        private void AbrirConexao()
        {
            if (m_dbConnection.State == System.Data.ConnectionState.Closed)
            {
                m_dbConnection.Open();
            }
            else if (m_dbConnection.State != System.Data.ConnectionState.Open)
            {
                AbrirConexao();
            }

        }
        private void FecharConexao()
        {
            if (m_dbConnection.State == System.Data.ConnectionState.Open)
            {
                m_dbConnection.Close();
            }
            else if (m_dbConnection.State != System.Data.ConnectionState.Closed)
            {
                FecharConexao();
            }

        }
    }

    class Card
    {
        public string nome { get; set; }
        public string srcImag { get; set; }
        public string qnt { get; set; }
        public string serie { get; set; }
        public DateTime data_atualizacao { get; set; }
        public string link { get; set; }
        public string id_Badge { get; set; }
        public string link_mercado { get; set; }
        public string link_forum { get; set; }
        public Card(HtmlElement htmlElement)
        {
            this.data_atualizacao = DateTime.Now;

            HtmlElementCollection htmlElemntCollection = htmlElement.GetElementsByTagName("div");

            this.link = htmlElement.Document.Url.ToString();

            bool nome = false;

            foreach (HtmlElement item in htmlElement.GetElementsByTagName("div"))
            {

                if (item.GetAttribute("className") == "game_card_ctn with_zoom")
                {
                    this.srcImag = item.GetElementsByTagName("img")[0].GetAttribute("src");
                }

                if (item.GetAttribute("className") == "badge_card_set_text_qty")
                {
                    this.qnt = item.InnerText;
                }
                if (item.GetAttribute("className") == "badge_card_set_text ellipsis")
                {
                    if (!nome)
                    {
                        this.nome = item.InnerText.Replace("'", "''");
                        nome = true;
                    }
                    else
                    {
                        this.serie = item.InnerText;
                    }

                }
            }

            SQLite sqlite = new SQLite();

            string sql = "select id from  Badge where link = '" + this.link + "'";

            this.id_Badge = sqlite.ExecuteReader(sql).Rows[0]["id"].ToString();

            //Inserir Carta

            sql = "insert into Card values (null,'"
                + this.nome + "','"
                + this.srcImag + "','"
                + this.qnt + "','"
                + this.serie + "','"
                + this.id_Badge + "','"
                + this.data_atualizacao + "')";

            sqlite.ExecuteNonQuery(sql);



        }
    }

    class Badge
    {
        public string id { get; set; }
        public string link { get; set; }
        public string nome { get; set; }
        public string numCartasTenho { get; set; }
        public string numCartas { get; set; }
        public string nivel { get; set; }
        public string link_mercado { get; set; }
        public string link_forum { get; set; }
        public string data_atualizacao { get; set; }
        public List<Card> cartas { get; set; }
        public string badge_info_title { get; set; }
        public string badge_info_level { get; set; }
        public string badge_info_unlocked { get; set; }
        public Badge() { }
        public Badge(HtmlElement htmlElement)
        {

            this.link = htmlElement.GetElementsByTagName("a")[0].GetAttribute("href");

            var wb = new WebBrowser();

            var we = wb.ReadyState;

            wb.ScriptErrorsSuppressed = true;

            wb.DocumentCompleted += browser_DocumentCompleted;

            wb.Navigate(this.link, "_self", null, "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");

            Console.WriteLine(string.Format("Rastreando pagina {0}", this.link));

            foreach (HtmlElement item in htmlElement.GetElementsByTagName("div"))
            {
                if (item.GetAttribute("className") == "badge_title")
                {
                    this.nome = item.InnerText.Substring(0, item.InnerText.Length - 14).Replace("'", "''");
                }
                if (item.GetAttribute("className") == "badge_progress_info" && !string.IsNullOrEmpty(item.InnerText))
                {
                    this.numCartasTenho = item.InnerText.Split(new string[] { " " }, StringSplitOptions.None)[0];
                    this.numCartas = item.InnerText.Split(new string[] { " " }, StringSplitOptions.None)[2];
                }

                if (item.GetAttribute("className") == "badge_info_description")
                {
                    this.badge_info_title = item.GetElementsByTagName("div")[0].InnerText;
                    this.badge_info_level = item.GetElementsByTagName("div")[1].InnerText;
                    this.badge_info_unlocked = item.GetElementsByTagName("div")[2].InnerText;

                    this.nivel = item.GetElementsByTagName("div")[1].InnerText.Split(new string[] { " " }, StringSplitOptions.None)[1];
                }
            }

            this.data_atualizacao = DateTime.Now.ToString();
            Inserir_Sqlite(this);
            Console.WriteLine(string.Format("FIM Rastreando pagina {0}", this.link));
        }
        public void Inserir_Sqlite(Badge insignia)
        {
            string sql = "insert into Badge values (null,'" + insignia.link + "','"
                                                            + insignia.nome + "','"
                                                            + insignia.numCartasTenho + "','"
                                                            + insignia.numCartas + "','"
                                                            + insignia.nivel + "','"
                                                            + insignia.link_mercado + "','"
                                                            + insignia.link_forum + "','"
                                                            + insignia.badge_info_title + "','"
                                                            + insignia.badge_info_level + "','"
                                                            + insignia.badge_info_unlocked + "','"
                                                            + insignia.data_atualizacao + "')";

            new SQLite().ExecuteNonQuery(sql);
        }
        public void Get_Badge_By_link(string _link)
        {
            SQLite sqlite = new SQLite();

            string sql = "select id," +
                " link," +
                " nome," +
                " numCartasTenho," +
                " numCartas," +
                " nivel," +
                " link_mercado," +
                " link_forum," +
                " badge_info_title," +
                " badge_info_level," +
                " badge_info_unlocked," +
                " data_atualizacao" +
                " from Badge" +
                " where link = '" + _link + "'";

            DataTable dt = sqlite.ExecuteReader(sql);

            this.id = dt.Rows[0]["id"].ToString();
            this.link = dt.Rows[0]["link"].ToString();
            this.nome = dt.Rows[0]["nome"].ToString().Replace("'", "''");
            this.numCartasTenho = dt.Rows[0]["numCartasTenho"].ToString();
            this.numCartas = dt.Rows[0]["numCartas"].ToString();
            this.nivel = dt.Rows[0]["nivel"].ToString();
            this.link_mercado = dt.Rows[0]["link_mercado"].ToString();
            this.link_forum = dt.Rows[0]["link_forum"].ToString();
            this.badge_info_title = dt.Rows[0]["badge_info_title"].ToString();
            this.badge_info_level = dt.Rows[0]["badge_info_level"].ToString();
            this.badge_info_unlocked = dt.Rows[0]["badge_info_unlocked"].ToString();
            this.data_atualizacao = dt.Rows[0]["data_atualizacao"].ToString();

        }
        public void UpDate_Sqlite(Badge insignia)
        {
            string sql = "update Badge set" +
                " nome = '" + insignia.nome + "', " +
                " numCartasTenho = '" + insignia.numCartasTenho + "', " +
                " numCartas = '" + insignia.numCartas + "', " +
                " nivel = '" + insignia.nivel + "', " +
                " link_mercado = '" + insignia.link_mercado + "', " +
                " link_forum = '" + insignia.link_forum + "', " +
                " badge_info_title = '" + insignia.badge_info_title + "', " +
                " badge_info_level = '" + insignia.badge_info_level + "', " +
                " badge_info_unlocked = '" + insignia.badge_info_unlocked + "', " +
                " data_atualizacao = '" + insignia.data_atualizacao + "' " +
                "where link = '" + insignia.link + "'";

            new SQLite().ExecuteNonQuery(sql);
        }
        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Get_Badge_By_link(this.link);

            var wb = sender as WebBrowser;

            Console.WriteLine(string.Format("Rastreando pagina {0}", wb.Url.AbsoluteUri));

            HtmlDocument doc = wb.Document;

            List<Card> _cartas = new List<Card>();

            foreach (HtmlElement item in doc.All)
            {
                //Console.WriteLine(item.GetAttribute("className"));
                if (item.GetAttribute("className") == "badge_card_set_card owned" || item.GetAttribute("className") == "badge_card_set_card unowned")
                {
                    Card carta = new Card(item);

                    _cartas.Add(carta);
                }

                if (item.GetAttribute("className") == "badge_card_to_collect_links")
                {

                    if (item.GetElementsByTagName("a").Count < 2)
                    {
                        var wb2 = new WebBrowser()
                        {
                            ScriptErrorsSuppressed = true
                        };
                        wb2.DocumentCompleted += browser_DocumentCompleted;
                        wb2.Navigate(this.link, "_self", null, "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");
                        return;
                    }


                    this.link_forum = item.GetElementsByTagName("a")[0].GetAttribute("href");


                    this.link_mercado = item.GetElementsByTagName("a")[1].GetAttribute("href");


                    UpDate_Sqlite(this);



                }
            }

            this.cartas = _cartas;

            Console.WriteLine(string.Format("FIM Rastreando pagina {0}", wb.Url.AbsoluteUri));

        }
    }

    public class Util
    {
        private static List<Badge> todoasListaInsignias = new List<Badge>();
        private static UInt16 maxPag = 1;
        private static bool jaProcessado = false;

        public static void GetAllBadge(WebBrowser _wbbadges)
        {
            //Adicionar o evento para quando for concluido 
            _wbbadges.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowserDocumentCompleted);

            HtmlDocument _doc = _wbbadges.Document;
            string _url = _wbbadges.Url.AbsoluteUri;

            //Pegar a pagina que esta sendo pesquisada
            UInt16 _paginaPesquisa = UInt16.Parse(_url.Split(char.Parse("?"))[1].Substring(2));

            List<Badge> insignias = new List<Badge>();

            if (_url.Contains("badges") &&
                _url.Substring(0, 29).Equals("http://steamcommunity.com/id/") &&
                _url.Substring(_url.Length - 11).Equals(string.Format("/badges?p={0}", _paginaPesquisa)))
            {
                Console.WriteLine(string.Format("Rastreando pagina {0}", _url));


                foreach (HtmlElement item in _doc.All)
                {
                    if (item.GetAttribute("className") == "pagelink")
                    {
                        UInt16 _tryParsePage = 0;
                        UInt16.TryParse(item.InnerHtml, out _tryParsePage);

                        if (_tryParsePage > maxPag)
                        {
                            maxPag = _tryParsePage;
                        }
                    }

                    if (item.GetAttribute("className") == "badge_row is_link")
                    {
                        insignias.Add(new Badge(item));
                    }

                }

                Console.WriteLine(string.Format("Insignias pegas {0}", insignias.Count));
                todoasListaInsignias.AddRange(insignias);
                Console.WriteLine(string.Format("FIM Rastreando pagina {0}", _url));
            }

            if (!jaProcessado)
            {
                jaProcessado = true;
                for (int i = 2; i < maxPag; i++)
                {
                    _wbbadges.Navigate(string.Format("http://steamcommunity.com/id/Liwelin/badges?p={0}", i));
                }
            }
            else
            {

            }

        }

        public static void WebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            GetAllBadge((WebBrowser)sender);
        }
    }
}
