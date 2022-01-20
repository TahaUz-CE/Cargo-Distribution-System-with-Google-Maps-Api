using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using MySql.Data.MySqlClient;
using System.Threading;
using Google.OrTools.ConstraintSolver;

namespace MySqlTry1
{
    public partial class Form1 : Form
    {
        private List<PointLatLng> _points;
        private ArrayList kargoid;
        private ArrayList teslimatAdresi;
        private ArrayList musteriId;
        private ArrayList musteriAd;
        private ArrayList lat;
        private ArrayList lng;
        private ArrayList nokMesafe;
        private ArrayList kimNokMesafe;
        private ArrayList enKisaRota;
        private GMapOverlay markers;
        private GMapOverlay routes;
        private String[] noktalar;
        private String[] asilRota;
        private double rotaCost ;
        



        public Form1()
        {
            InitializeComponent();
            basla();
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GMapProviders.GoogleMap.ApiKey = "APIKEY";
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.ShowCenter = false;
        }

        private void find_Click(object sender, EventArgs e)
        {
            overlayAllRemove();
            basla();       
        }


        private void mySqlRdy()
        {
            string cs = @"server=34.135.94.17;userid=root;password=;database=BridgesCargo";

            var con = new MySqlConnection(cs);
            con.Open();

            string sql = "SELECT * FROM delivery";
            var cmd = new MySqlCommand(sql, con);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                if (!rdr.GetString(6).Equals("Teslim Edildi"))
                {
                    kargoid.Add(rdr.GetString(0));
                    teslimatAdresi.Add(rdr.GetString(1));
                    musteriAd.Add(rdr.GetString(3));
                    musteriId.Add(rdr.GetString(4));
                    lat.Add(rdr.GetString(7));
                    lng.Add(rdr.GetString(8));
                }
                
            }
            noktalar = new String[teslimatAdresi.Count];
        }

        private void fistfind() 
        {
            gMapControl1.DragButton = MouseButtons.Left;
            //double lat = Convert.ToDouble(lat[0]);
            //double lng = Convert.ToDouble(lng[0]);
            //Console.WriteLine(lat);
            //Console.WriteLine(lng);
            gMapControl1.Position = new PointLatLng(Convert.ToDouble(lat[0]), Convert.ToDouble(lng[0]));
            gMapControl1.Zoom = 10;
            gMapControl1.MinZoom = 5;
            gMapControl1.MaxZoom = 100;
        }

        private void rotaCiz()
        {
            for (int i = 0; i < lat.Count-1 ; i++)
            {
                for (int j =i+1; j < lat.Count; j++)
                {
                    var route = GoogleMapProvider.Instance.GetRoute(_points[i], _points[j], false, false, 14);
                    
                    var r = new GMapRoute(route.Points, "My Route" + i)
                    {
                        Stroke = new Pen(Color.Red, 3)
                    };
                    
                    routes.Routes.Add(r);
                    gMapControl1.Overlays.Add(routes);
                    nokMesafe.Add(route.Distance);
                    //Console.WriteLine(_points[i]+"-"+ _points[j]);
                    kimNokMesafe.Add(teslimatAdresi[i]+"-"+teslimatAdresi[j]);
                }
            }

            String yaz;
            for (int i = 0; i < nokMesafe.Count; i++)
            {
                yaz = null;

                yaz = kimNokMesafe[i] + " : " + nokMesafe[i];

                Console.WriteLine(yaz);
            }

            /*foreach (var mesafe in nokMesafe)
                Console.WriteLine(mesafe);
            Console.WriteLine(nokMesafe.Count);*/

        }

        private void haritayaNoktaEkle()
        {
            for (int i = 0; i < lat.Count; i++)
            {
                GMapMarker marker;
                _points.Add(new PointLatLng(Convert.ToDouble(lat[i]), Convert.ToDouble(lng[i])));
                PointLatLng point = new PointLatLng(Convert.ToDouble(lat[i]), Convert.ToDouble(lng[i]));
                if(Convert.ToInt32(kargoid[i]) == 0) {
                    marker = new GMarkerGoogle(point, GMarkerGoogleType.green);
                }
                else
                {
                    marker = new GMarkerGoogle(point, GMarkerGoogleType.red);
                }
                
                marker.ToolTipText = $"Adress : {teslimatAdresi[i]}, \n Customer ID : {musteriId[i]}, \n Customer Name : {musteriAd[i]}";
                markers.Markers.Add(marker);

                gMapControl1.Overlays.Add(markers);
            }
        }

        private void overlayAllRemove()
        {
            gMapControl1.Overlays.Clear();
            gMapControl1.Refresh();
        }

        
        
        private void basla()
        {
                yenidenTanimla();
                mySqlRdy();
                
                fistfind();
                haritayaNoktaEkle();
                
                rotaCiz();
                for (int i = 0; i < teslimatAdresi.Count; i++)
                {
                    noktalar[i] = teslimatAdresi[i].ToString();
                }
                rotaUretici(noktalar);
                enKisaRotaOlustur();
            Console.WriteLine(enKisaRota[0]);
            if (musteriId.Count > 1)
            {
                nextStation.Text = $"SIRADAKI TESLİMAT \n\n Adress : {teslimatAdresi[Convert.ToInt32(enKisaRota[1])]}, \n Customer ID : {musteriId[Convert.ToInt32(enKisaRota[1])]}," +
                    $" \n Customer Name : {musteriAd[Convert.ToInt32(enKisaRota[1])]}";
            }
        }

        private void yenidenTanimla()
        {
            kargoid = new ArrayList();
            teslimatAdresi = new ArrayList();
            musteriId = new ArrayList();
            musteriAd = new ArrayList();
            lat = new ArrayList();
            lng = new ArrayList();
            nokMesafe = new ArrayList();
            markers = new GMapOverlay("markers");
            routes = new GMapOverlay("routes");
            _points = new List<PointLatLng>();
            enKisaRota = new ArrayList();
            kimNokMesafe = new ArrayList();
            asilRota = new String[10];
            rotaCost = 9999999.9999;
        }

        private void enKisaRotaOlustur()
        {
            
            for (int i = 0; i < asilRota.Length; i++)
            {
                enKisaRota.Add(teslimatAdresi.IndexOf(asilRota[i]));
            }
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (musteriId.Count > 1) {
                string cs = @"server=34.135.94.17;userid=root;password=;database=BridgesCargo";

                var con = new MySqlConnection(cs);
                con.Open();

                String sorgu = "UPDATE " + musteriId[Convert.ToInt32(enKisaRota[1])] + " SET teslimtarihi = '" + "29/10/2021" + "',kargodurum = '" + "Teslim Edildi" + "' WHERE (kargoid = '" + kargoid[Convert.ToInt32(enKisaRota[1])] + "')";
                String sorgu2 = "UPDATE delivery SET teslimtarihi = '" + "29/10/2021" + "',kargodurum = '" + "Teslim Edildi" + "' WHERE (kargoid = '" + kargoid[Convert.ToInt32(enKisaRota[1])] + "')";
                String sorgu3 = "UPDATE delivery SET lat = '" + lat[Convert.ToInt32(enKisaRota[1])] + "',lng = '" + lng[Convert.ToInt32(enKisaRota[1])] + "',teslimadresi = '" + teslimatAdresi[Convert.ToInt32(enKisaRota[1])] + "' WHERE (kargoid = '" + 0 + "')";

                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = sorgu;
                cmd.Connection = con;
                cmd.ExecuteNonQuery();

                MySqlCommand cmd2 = new MySqlCommand();
                cmd2.CommandText = sorgu2;
                cmd2.Connection = con;
                cmd2.ExecuteNonQuery();

                MySqlCommand cmd3 = new MySqlCommand();
                cmd3.CommandText = sorgu3;
                cmd3.Connection = con;
                cmd3.ExecuteNonQuery();

                overlayAllRemove();
                basla();
            }
        }

        private void rotaUretici(string[] noktalarTemp)
        {  

            PrintResult(
                Permute(noktalarTemp)
            );

        }

        private IList<IList<String>> Permute(String[] nums)
        {
            var list = new List<IList<String>>();
            return DoPermute(nums, 0, nums.Length - 1, list);
        }

        private IList<IList<String>> DoPermute(String[] nums, int start, int end, IList<IList<String>> list)
        {
            if (start == end)
            {
                // We have one of our possible n! solutions,
                // add it to the list.
                list.Add(new List<String>(nums));
            }
            else
            {
                for (var i = start; i <= end; i++)
                {
                    Swap(ref nums[start], ref nums[i]);
                    // Console.WriteLine(nums[start]+  "    " + nums[i]);
                    DoPermute(nums, start + 1, end, list);
                    //Console.WriteLine(nums[start] + "    " + nums[i]);
                    Swap(ref nums[start], ref nums[i]);
                    //  Console.WriteLine(nums[start] + "    " + nums[i]);
                }

            }

            return list;
        }

        private void Swap(ref String a, ref String b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        private void PrintResult(IList<IList<String>> lists)
        {
            //string yaz;
            ArrayList rota;
            
            double tempRotaCost;
            
            Console.WriteLine("[");
            foreach (var list in lists)
            {
                if (list[0].Equals(teslimatAdresi[0]))
                {
                    //yaz = null;
                    rota = new ArrayList();
                    foreach (var l in list)
                    {
                        //yaz = yaz + " " + l;
                        rota.Add(l);
                    }
                    //Console.WriteLine(yaz);
                    //String aaz = null;
                    tempRotaCost = 0.0;
                    for (int i = 0 ; i<rota.Count-1 ; i++)
                    {
                        
                        for (int j = 0; j < kimNokMesafe.Count; j++)
                        {
                            if(kimNokMesafe[j].Equals(rota[i]+"-"+rota[i+1]) || kimNokMesafe[j].Equals(rota[i+1] + "-" + rota[i]))
                            {
                                tempRotaCost += Convert.ToDouble(nokMesafe[j]);
                                
                            }
                        }

                        //aaz = aaz + " " + rota[i+1].ToString();
                    }
                    //Console.WriteLine(aaz);
                    //Console.WriteLine(tempRotaCost);

                    if (tempRotaCost < rotaCost)
                    {
                        
                        rotaCost = tempRotaCost;
                        for (int k = 0; k < rota.Count; k++)
                        {
                            asilRota[k] = rota[k].ToString();
                            
                        }
                        
                    }
                }
                
                
            }
            Console.WriteLine("]");
            Console.WriteLine(lists.Count);
            
            String kaz = null;
            
            for (int i = 0; i < asilRota.Length; i++)
            {
                kaz = kaz+ " " + asilRota[i] ;
            }
            Console.WriteLine(kaz);
            Console.WriteLine("Rota Cost : "+rotaCost);
        }

    }
}
