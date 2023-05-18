using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nyelvvizsga
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            List<NyelvStatisztika> statisztika = BeolvasAdatok();

            if (statisztika.Count > 0)
            {
                List<string> legnepszerubbNyelvek = GetLegnepszerubbNyelvek(statisztika);
                Console.WriteLine("A legnépszerűbb nyelvek:");
                PrintNyelvek(legnepszerubbNyelvek);

                int ev = GetEv();
                double legnagyobbSikertelenArany = GetLegnagyobbSikertelenArany(statisztika, ev);
                string legnagyobbSikertelenNyelv = GetLegnagyobbSikertelenNyelv(statisztika, ev);
                Console.WriteLine($"\nA legnagyobb sikertelen arány: {legnagyobbSikertelenArany:P2} ({legnagyobbSikertelenNyelv})");

                List<string> nyelvekBeNemVizsgaltEvben = GetNyelvekBeNemVizsgaltEvben(statisztika, ev);
                Console.WriteLine("\nNyelvek, amelyekből nem volt vizsgázó:");
                if (nyelvekBeNemVizsgaltEvben.Count > 0)
                {
                    foreach (string nyelv in nyelvekBeNemVizsgaltEvben)
                    {
                        Console.WriteLine(nyelv);
                    }
                }
                else
                {
                    Console.WriteLine("Minden nyelvből volt vizsgázó");
                }

                OsszesitesMentes(statisztika);
                Console.WriteLine("\nAz adatok összesítése sikeresen elmentve az osszesites.csv fájlba.");
            }

            Console.ReadLine();
        }

        static List<NyelvStatisztika> BeolvasAdatok()
        {
            List<NyelvStatisztika> statisztika = new List<NyelvStatisztika>();
            try
            {
                string[] sikeresAdatok = File.ReadAllLines("sikeres.csv", Encoding.UTF8);
                string[] sikertelenAdatok = File.ReadAllLines("sikertelen.csv", Encoding.UTF8);

                for (int i = 1; i < sikeresAdatok.Length; i++)
                {
                    string[] sikeresData = sikeresAdatok[i].Split(';');
                    string[] sikertelenData = sikertelenAdatok[i].Split(';');

                    NyelvStatisztika nyelvStatisztika = new NyelvStatisztika();
                    nyelvStatisztika.Nyelv = sikeresData[0];
                    nyelvStatisztika.SikeresVizsgak = sikeresData.Skip(1).Select(int.Parse).ToArray();
                    nyelvStatisztika.SikertelenVizsgak = sikertelenData.Skip(1).Select(int.Parse).ToArray();

                    statisztika.Add(nyelvStatisztika);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt az adatok beolvasása során: {ex.Message}");
            }

            return statisztika;
        }

        static List<string> GetLegnepszerubbNyelvek(List<NyelvStatisztika> statisztika)
        {
            List<string> legnepszerubbNyelvek = new List<string>();

            var nyelvek = statisztika.Select(s => s.Nyelv).Distinct();

            foreach (var nyelv in nyelvek)
            {
                int osszesVizsgaSzam = statisztika.Where(s => s.Nyelv == nyelv).Sum(s => s.OsszesVizsgaSzama());
                legnepszerubbNyelvek.Add($"{nyelv} - {osszesVizsgaSzam}");
            }

            legnepszerubbNyelvek = legnepszerubbNyelvek.OrderByDescending(n => int.Parse(n.Split('-')[1].Trim())).Take(3).ToList();

            return legnepszerubbNyelvek;
        }

        static void PrintNyelvek(List<string> nyelvek)
        {
            foreach (var nyelv in nyelvek)
            {
                Console.WriteLine(nyelv);
            }
        }

        static int GetEv()
        {
            int ev;
            bool validEv;

            do
            {
                Console.Write("\nKérem adja meg az évet (2009-2017): ");
                validEv = int.TryParse(Console.ReadLine(), out ev);

                if (!validEv || ev < 2009 || ev > 2017)
                {
                    Console.WriteLine("Hibás év! Kérem adjon meg egy érvényes évet.");
                }
            } while (!validEv || ev < 2009 || ev > 2017);

            return ev;
        }

        static double GetLegnagyobbSikertelenArany(List<NyelvStatisztika> statisztika, int ev)
        {
            double legnagyobbArany = 0;

            foreach (NyelvStatisztika nyelvStatisztika in statisztika)
            {
                double arany = nyelvStatisztika.SikertelenArany(ev);
                if (arany > legnagyobbArany)
                {
                    legnagyobbArany = arany;
                }
            }

            return legnagyobbArany;
        }

        static string GetLegnagyobbSikertelenNyelv(List<NyelvStatisztika> statisztika, int ev)
        {
            string legnagyobbNyelv = "";
            double legnagyobbArany = 0;

            foreach (NyelvStatisztika nyelvStatisztika in statisztika)
            {
                double arany = nyelvStatisztika.SikertelenArany(ev);
                if (arany > legnagyobbArany)
                {
                    legnagyobbArany = arany;
                    legnagyobbNyelv = nyelvStatisztika.Nyelv;
                }
            }

            return legnagyobbNyelv;
        }

        static List<string> GetNyelvekBeNemVizsgaltEvben(List<NyelvStatisztika> statisztika, int ev)
        {
            List<string> nyelvekBeNemVizsgaltEvben = new List<string>();

            foreach (NyelvStatisztika nyelvStatisztika in statisztika)
            {
                if (nyelvStatisztika.OsszesVizsgaSzama(ev) == 0)
                {
                    nyelvekBeNemVizsgaltEvben.Add(nyelvStatisztika.Nyelv);
                }
            }

            return nyelvekBeNemVizsgaltEvben;
        }

        static void OsszesitesMentes(List<NyelvStatisztika> statisztika)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("osszesites.csv", false, Encoding.UTF8))
                {
                    sw.WriteLine("Nyelv;Összes vizsga száma;Sikeres arány");

                    foreach (NyelvStatisztika nyelvStatisztika in statisztika)
                    {
                        double sikeresArany = nyelvStatisztika.SikeresArany();
                        sw.WriteLine($"{nyelvStatisztika.Nyelv};{nyelvStatisztika.OsszesVizsgaSzama()};{sikeresArany:P2}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt az adatok mentése során: {ex.Message}");
            }
        }
    }

    class NyelvStatisztika
    {
        public string Nyelv { get; set; }
        public int[] SikeresVizsgak { get; set; }
        public int[] SikertelenVizsgak { get; set; }

        public int OsszesVizsgaSzama()
        {
            return SikeresVizsgak.Sum() + SikertelenVizsgak.Sum();
        }

        public int OsszesVizsgaSzama(int ev)
        {
            int index = ev - 2009;
            return SikeresVizsgak[index] + SikertelenVizsgak[index];
        }

        public double SikeresArany()
        {
            double osszesVizsgaSzam = OsszesVizsgaSzama();
            double sikeresVizsgaSzam = SikeresVizsgak.Sum();
            return sikeresVizsgaSzam / osszesVizsgaSzam;
        }

        public double SikertelenArany(int ev)
        {
            int index = ev - 2009;
            double osszesVizsgaSzam = SikeresVizsgak[index] + SikertelenVizsgak[index];
            return SikertelenVizsgak[index] / osszesVizsgaSzam;
        }
    }
}
