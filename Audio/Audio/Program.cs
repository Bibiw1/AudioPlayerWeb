using System;
using NAudio.Wave;
using NAudio.Flac;
using System.IO;
using System.Threading;
using System.Net;
using System.Text;
using System.Linq;

namespace Audio
{
    class Program
    {
        static void server()
        {
            HttpListener server = new HttpListener();
            server.Prefixes.Add("http://127.0.0.1/");
            server.Prefixes.Add("http://localhost/");
            server.Prefixes.Add("http://10.200.1.33/");
            server.Start();

            while (true)
            {
                HttpListenerContext context = server.GetContext();
                HttpListenerResponse response = context.Response;

                if (context.Request.HttpMethod == "GET")
                {
                    string page = "index.html";
                    Stream input = context.Request.InputStream;
                    byte[] bytes = new byte[(int)context.Request.ContentLength64];
                    int n = input.Read(bytes, 0, (int)context.Request.ContentLength64);
                    TextReader tr = new StreamReader(page);
                    string msg = tr.ReadToEnd();

                    byte[] buffer = Encoding.UTF8.GetBytes(msg);
                    tr.Close();
                    response.ContentLength64 = buffer.Length;
                    Stream st = response.OutputStream;
                    st.Write(buffer, 0, buffer.Length);

                    context.Response.Close();
                }
                if (context.Request.HttpMethod == "POST")
                {
                    Stream input = context.Request.InputStream;
                    Stream output = context.Response.OutputStream;
                    byte[] bytes = new byte[(int)context.Request.ContentLength64];
                    int n = input.Read(bytes, 0, (int)context.Request.ContentLength64);

                    input.Close();
                    string result = Encoding.UTF8.GetString(bytes);
                        if (result == "nextTrack")
                        {
                            changetrack = true;
                        }
                        if (result == "getSongers")
                        {
                            byte[] responsebuffer;
                            string songers = songerslist();
                            responsebuffer = Encoding.UTF8.GetBytes(songers);
                            output.Write(responsebuffer, 0, responsebuffer.Length);

                        }
                        if (result.Substring(0, 8) == "getAlbum")
                        {
                            string songer = result.Substring(11);
                            byte[] responsebuffer;
                            string albums = albumslist(songer);
                            responsebuffer = Encoding.UTF8.GetBytes(albums);
                            output.Write(responsebuffer, 0, responsebuffer.Length);

                        }
                        if (result.Substring(0, 8) == "getTrack")
                        {
                            string songer = result.Substring(11);
                            int a = songer.IndexOf(" - ", 0);
                            songer = songer.Substring(0, a);
                            string album = result.Substring(14 + a);
                            byte[] responsebuffer;
                            string tracks = trackslist(songer, album);
                            responsebuffer = Encoding.UTF8.GetBytes(tracks);
                            output.Write(responsebuffer, 0, responsebuffer.Length);

                        }
                        if (result.Substring(0, 8) == "getNameT")
                        {
                            string nameoftrack = getallbypath(path)[0] + " --- " + getallbypath(path)[1] + " --- " + getallbypath(path)[2];
                            
                            byte[] responsebuffer;
                            responsebuffer = Encoding.UTF8.GetBytes(nameoftrack);
                            output.Write(responsebuffer, 0, responsebuffer.Length);

                        }
                    
                        if (result.Substring(0, 8) == "playTrac")
                        {
                            string songer = result.Substring(12);
                            int a = songer.IndexOf(" - ", 0);
                            songer = songer.Substring(0, a);
                            string album = result.Substring(a + 15);
                            int b = album.LastIndexOf(" - ");
                            album = album.Remove(b);
                            string track = result.Substring(18 + songer.Length + album.Length);
                            path = findfile(songer, album, track);
                            changetrack = true;
                            random = false;

                        }
                    if (result.Substring(0, 8) == "plyorstp")
                    {
                        if (onpaused)
                        {
                            player.Play();
                            onpaused = false;
                        }
                        else
                        {
                            onpaused = true;
                            player.Pause();
                        }
                    }
                    if (result.Substring(0, 8) == "setVolum")
                        {
                            volume = (float)Convert.ToDouble(result.Remove(0, 8)) / 100;
                            player.Volume = volume;

                        }
                    output.Close();
                }
            }

        }
        static bool onpaused = false;
        static WaveOut player;
        static bool changetrack = false;
        static string[] files;
        static bool random = true;
        static string path;
        static void Main(string[] args)
        {

            Thread Thread = new Thread(new ThreadStart(ServerThread));
            Thread.Start();
            files = findfiles(@"C:\Музыка");
            Random rnd = new Random();
            while (true)
            {
                if(random) path = files[rnd.Next(files.Length)];
                random = true;
                Console.WriteLine(Path.GetFileName(path));
                playfile(path);
                while ((player.PlaybackState.ToString()=="Playing" && changetrack == false)||onpaused)
                {
                    Thread.Sleep(50);
                }
                player.Stop();
                changetrack = false;
            }
        }
        static float volume;
        static string songerslist()
        {
            string list = "";
            string[] songers = new string[10];
            int a = 0;
            foreach (string s in files)
            {
                bool bil = false;
                string[] alldata = getallbypath(s);
                foreach(string songr in songers)
                {
                    if(alldata[0] == songr)
                    {
                        bil = true;
                    }
                }
                if(bil == false)
                {
                    songers[a] = alldata[0];
                    a++;
                }
            }
            foreach(string songer in songers)
            {
                list += songer + Environment.NewLine;
            }
            return list;
        }
        static string albumslist(string songer)
        {
            string list = "";
            string[] albums = new string[100];
            int a = 0;
            foreach (string s in files)
            {
                bool bil = false;
                string[] alldata = getallbypath(s);
                if (alldata[0] == songer)
                {
                    foreach (string albm in albums)
                    {
                        if (albm == alldata[1]) bil = true;
                    }
                    if(bil == false)
                    {
                        albums[a] = alldata[1];
                        a++;
                    }
                }
            }
            foreach (string album in albums)
            {
                list += album + Environment.NewLine;
            }
            return list;
        }
        static string trackslist(string songer, string album)
        {
            string list = "";
            string[] tracks = new string[100];
            int a = 0;
            foreach (string s in files)
            {
                string[] alldata = getallbypath(s);
                if (songer == alldata[0] && album == alldata[1])
                {
                    tracks[a] = alldata[2] ;
                    a++;
                }
            }
            foreach (string track in tracks)
            {
                list += track + Environment.NewLine;
            }
            return list;
        }
        static void playfile(string path)
        {
            if (path == "") return;
            if (Path.GetExtension(path) == ".mp3")
            {
                Mp3FileReader mp3Reader = new Mp3FileReader(path);
                player = new WaveOut();
                player.Init(mp3Reader);
                player.Volume = 1;
                player.Play();
            }
        }

        static string findfile(string songer, string album, string track)
        {
            foreach (string s in files)
            {
                string[] allinfo = getallbypath(s);
                
                if (songer == allinfo[0] && album == allinfo[1] && track == allinfo[2])
                {
                    return s;
                }

            }
            return "";
        }
        static string[] findfiles(string path)
        {
            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp3")).ToArray();/* || s.EndsWith(".flac")*/
        }
        static void ServerThread()
        {
            server();
        }
        static string[] getallbypath(string path)
        {
            string[] rtrn = new string[3];
            if (path == ""||path==null) return rtrn;
            string albm = path.Remove(0, 10);
            int k = albm.IndexOf(@"\");
            string sngr = albm.Substring(0, k);
            albm = albm.Remove(0, k + 1);
            k = albm.IndexOf(@"\");
            string trck = Path.GetFileNameWithoutExtension(path);
            albm = albm.Substring(0, k);
            rtrn[0] = sngr;
            rtrn[1] = albm;
            rtrn[2] = trck;
            return rtrn;
        }
    }
}
