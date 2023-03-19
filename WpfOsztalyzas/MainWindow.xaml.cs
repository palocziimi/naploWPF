using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfOsztalyzas
{
    public partial class MainWindow : Window
    {
        string fajlNev = "naplo.csv";
        ObservableCollection<Osztalyzat> jegyek = new ObservableCollection<Osztalyzat>();

        public MainWindow()
        {
            InitializeComponent();
            // todo Fájlok kitallózásával tegye lehetővé a naplófájl kiválasztását!
            // Ha nem választ ki semmit, akkor "naplo.csv" legyen az állomány neve. A későbbiekben ebbe fog rögzíteni a program.
            // todo A kiválasztott naplót egyből töltse be és a tartalmát jelenítse meg a datagrid-ben!

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV Állomány (.csv)|*.csv";

            if (dialog.ShowDialog() == true)
            {
                fajlNev = dialog.FileName;
                Frissit(fajlNev);
            }
            else
            {
                fajlNev = "naplo.csv";
            }



        }

        private void btnRogzit_Click(object sender, RoutedEventArgs e)
        {
            //todo Ne lehessen rögzíteni, ha a következők valamelyike nem teljesül!
            // a) - A név legalább két szóból álljon és szavanként minimum 3 karakterből!
            //      Szó = A szöközökkel határolt karaktersorozat.
            // b) - A beírt dátum újabb, mint a mai dátum

            if (!txtNev.Text.Contains(' '))
            {
                MessageBox.Show("A név legalább két szóból álljon!", "Hiba");
                return;
            }

            for (int i = 0; i < txtNev.Text.Split(' ').Length; i++)
            {
                if (txtNev.Text.Split(' ')[i].Length < 3)
                {
                    MessageBox.Show("A név legalább szavanként minimum 3 karakterből álljon!", "Hiba");
                    return;
                }
            }
            
            if (datDatum.Text == "" || Convert.ToDateTime(datDatum.Text) > 
                Convert.ToDateTime(DateTime.Now.ToString(new CultureInfo("hu-HU"))))
            {
                MessageBox.Show("A beírt dátum újabb, mint a mai dátum", "Hiba");
                return;
            }

            //todo A rögzítés mindig az aktuálisan megnyitott naplófájlba történjen!

            //A CSV szerkezetű fájlba kerülő sor előállítása
            string csvSor = $"{(rdoKeresztNev.IsChecked == false ? txtNev.Text : Osztalyzat.ForditottNev(txtNev.Text))};{datDatum.Text};{cboTantargy.Text};{sliJegy.Value}";
            //Megnyitás hozzáfűzéses írása (APPEND)
            StreamWriter sw = new StreamWriter(fajlNev, append: true);
            sw.WriteLine(csvSor);
            sw.Close();
            //todo Az újonnan felvitt jegy is jelenjen meg a datagrid-ben!

            Frissit(fajlNev);

        }

        private void btnBetolt_Click(object sender, RoutedEventArgs e)
        {
            Frissit(fajlNev);
        }

        private void sliJegy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblJegy.Content = sliJegy.Value; //Több alternatíva van e helyett! Legjobb a Data Binding!
        }

        //todo Felület bővítése: Az XAML átszerkesztésével biztosítsa, hogy láthatóak legyenek a következők!
        // - A naplófájl neve
        // - A naplóban lévő jegyek száma
        // - Az átlag

        //todo Új elemek frissítése: Figyeljen rá, ha új jegyet rögzít, akkor frissítse a jegyek számát és az átlagot is!

        //todo Helyezzen el alkalmas helyre 2 rádiónyomógombot!
        //Feliratok: [■] Vezetéknév->Keresztnév [O] Keresztnév->Vezetéknév
        //A táblázatban a név aszerint szerepeljen, amit a rádiónyomógomb mutat!
        //A feladat megoldásához használja fel a ForditottNev metódust!
        //Módosíthatja az osztályban a Nev property hozzáférhetőségét!
        //Megjegyzés: Felételezzük, hogy csak 2 tagú nevek vannak

        private void Frissit(string fileName = "naplo.csv")
        {
            jegyek.Clear();  //A lista előző tartalmát töröljük


            StreamReader sr = new StreamReader(fajlNev); //olvasásra nyitja az állományt
            while (!sr.EndOfStream) //amíg nem ér a fájl végére
            {
                string[] mezok = sr.ReadLine().Split(";"); //A beolvasott sort feltördeli mezőkre
                //A mezők értékeit felhasználva létrehoz egy objektumot
                Osztalyzat ujJegy = new Osztalyzat(mezok[0], mezok[1], mezok[2], int.Parse(mezok[3]));
                jegyek.Add(ujJegy); //Az objektumot a lista végére helyezi

            }
            sr.Close(); //állomány lezárása

            //A Datagrid adatforrása a jegyek nevű lista lesz.
            //A lista objektumokat tartalmaz. Az objektumok lesznek a rács sorai.
            //Az objektum nyilvános tulajdonságai kerülnek be az oszlopokba.
            dgJegyek.ItemsSource = jegyek;

            lblNaploNev.Content = fajlNev;
            lblJegyekSzama.Content = dgJegyek.Items.Count;
            
            double jegyosszeg = 0.0;
            for (int i = 0; i < jegyek.Count; i++) jegyosszeg += jegyek[i].Jegy;
            lblAtlag.Content = Math.Round(jegyosszeg / dgJegyek.Items.Count, 2);
        }
    }
}

