using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Timers;
using System.Linq;
using System.Media;
using Microsoft.VisualBasic.Devices;

namespace ProjetDeTest
{
    public partial class Form1 : Form
    {
        private List<Train> trains = new List<Train>();
        private System.Timers.Timer timer;
        private Random random = new Random();
        private List<string> infoMessages = new List<string>();
        private int currentInfoIndex = 0;
        private System.Timers.Timer infoTimer;
        private System.Timers.Timer secondTimer; // Timer pour les secondes
        private bool soundPlayed = false; // Bool pour suivre si le son a été joué ou non
        private Dictionary<string, string> directionSounds;
       // private bool isFirstTrainSoundPlayed = false;
        private Train lastFirstTrain = null; // Trace du dernier train qui était en première position



        public Form1()
        {
            InitializeComponent();
            InitializeDirectionSounds();
            InitializeTrains();
            timerHour.Interval = 1000; // 1000 ms = 1 seconde
            timerHour.Tick += new EventHandler(timerHour_Tick);
            timerHour.Start();
            StartTimer();
            InitializeInfoMessages();
            lblInfoVoyageur.Text = infoMessages[currentInfoIndex];
            StartInfoTimer();

            currentInfoIndex = 0;

        }

        private string GetRandomDirection()
        {
            string[] possibleDirections = { "Massy-Palaiseau", "Robinson", "Saint-Denis", "Les Baconnets", "Antony" };
            string lastDirection = trains.LastOrDefault()?.Direction;
            string secondLastDirection = trains.Count > 1 ? trains[trains.Count - 2].Direction : "";

            // Filtre les directions possibles pour éviter trois fois la même direction consécutivement
            var validDirections = possibleDirections.Where(d => d != lastDirection || d != secondLastDirection).ToList();

            return validDirections[random.Next(validDirections.Count)];
        }

        private void InitializeDirectionSounds()
        {
            directionSounds = new Dictionary<string, string>
            {
                { "Massy-Palaiseau", @"C:\Users\Totox\OneDrive\Documents\Visual Studio 2022\Projets\ProjetDeTest\MassyPalaiseau.wav" },
                { "Robinson", @"C:\Users\Totox\OneDrive\Documents\Visual Studio 2022\Projets\ProjetDeTest\Robinson.wav" },
                { "Saint-Denis", @"C:\Users\Totox\OneDrive\Documents\Visual Studio 2022\Projets\ProjetDeTest\Saint-Denis.wav" },
                { "Les Baconnets", @"C:\Users\Totox\OneDrive\Documents\Visual Studio 2022\Projets\ProjetDeTest\Baconnets.wav" },
                { "Antony", @"C:\Users\Totox\OneDrive\Documents\Visual Studio 2022\Projets\ProjetDeTest\Antony.wav" }
            };
        }
        
        private void InitializeInfoMessages()
        {
            infoMessages.Add("Travaux à Gare du Nord: accès restreint au Quai 3 en dir. de Mitry - Claye jusqu'au 15 avril.");
            infoMessages.Add("Info voyageurs: la ligne B est perturbée en raison d'un incident technique. Prévoyez des retards.");
            infoMessages.Add("Vigilance: pickpockets signalés sur la ligne D entre Châtelet et Gare de Lyon. Soyez attentifs.");
            infoMessages.Add("Modernisation en cours: l'escalator à St-Michel est hors service jusqu'au 30 mars. Utilisez l'ascenseur.");
            infoMessages.Add("Info travaux: interruption de la ligne A ce week-end entre Auber et Nation. Navettes disponibles.");
            infoMessages.Add("Alerte météo: fortes pluies prévues ce soir. Anticipez des ralentissements sur l'ensemble du réseau.");
            infoMessages.Add("En raison d'un colis suspect, la station Luxembourg est fermée. Reprise du trafic estimée à 14h.");
            infoMessages.Add("Maintenance technique: le WiFi est indisponible en gare de Lyon. Rétablissement espéré demain.");
            infoMessages.Add("Sécurité: Exercice de sécurité en cours à Châtelet. Ne soyez pas alarmés par la présence des secours.");
        }

        private void StartInfoTimer()
        {
            infoTimer = new System.Timers.Timer(9000); // 9000 ms = 9 secondes
            infoTimer.Elapsed += OnInfoTimedEvent;
            infoTimer.AutoReset = true;
            infoTimer.Enabled = true;
        }

        private void OnInfoTimedEvent(Object source, ElapsedEventArgs e)
        {
            // Utilise Invoke pour mettre à jour les messages d'informations sur le thread 
            Invoke(new Action(() =>
            {
                lblInfoVoyageur.Text = infoMessages[currentInfoIndex];
                currentInfoIndex = (currentInfoIndex + 1) % infoMessages.Count;
            }));
        }

        private void InitializeTrains()
        {
            // Le premier train est programmé pour partir entre 1 et 3 minutes au lancement du logiciel
            int firstTrainTime = random.Next(2, 4); // Temps en minutes

            trains.Add(new Train
            {
                Name = GenerateRandomTrainName(),
                Direction = GetRandomDirection(),
                Time = firstTrainTime,
                Comment = String.Empty
            });

            // Génére les temps pour les 4 autres trains, assurant qu'ils partent après le premier et dans les 60 minutes
            int lastTime = firstTrainTime;
            for (int i = 1; i < 5; i++)
            {
            // TODO: Gérer l'affluence du trafic en modifiant les espacements des horaires de passages
                int nextTime = lastTime + random.Next(5, 16); // Les trains suivants sont espacés de 5 à 15 minutes
                if (nextTime > 60)
                {
                    nextTime = 60; // Limite le temps à 60 minutes 
                }
                trains.Add(new Train
                {
                    Name = GenerateRandomTrainName(),
                    Direction = GetRandomDirection(),
                    Time = nextTime,
                    Comment = String.Empty
                });
                lastTime = nextTime; // Mettre à jour le dernier temps programmé
            }

            trains = trains.OrderBy(t => t.Time).ToList(); // Triage des trains en fonction de leurs temps
            UpdateAllTrainDisplays();
        }


        private void UpdateAllTrainDisplays()
        {
            for (int i = 0; i < trains.Count; i++)
            {
                UpdateTrainDisplay(i);
            }
        }

        private string GenerateRandomTrainName()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string letters = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
            return letters + random.Next(10, 100).ToString();
        }

        private void StartTimer()
        {
            timer = new System.Timers.Timer(60000); // 60000 ms = 1 minute
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            secondTimer = new System.Timers.Timer(1000);  // 1000 ms = 1 seconde
            secondTimer.Elapsed += OnSecondTimedEvent;
            secondTimer.AutoReset = true;  // Doit être AutoReset pour continuer à décrémenter
            secondTimer.Enabled = false;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                foreach (var train in trains)
                {
                    train.Time--;
                    if (train == trains[0] && train.Time == 1)
                    {
                        train.Seconds = 60;  // Pour compter les 60 dernières secondes exactement.
                        secondTimer.Start();
                    }
                }
                UpdateAllTrainDisplays();
            }));
        }



        private void OnSecondTimedEvent(Object source, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                if (trains.Count > 0)
                {
                    trains[0].Seconds--;

                    if (trains[0].Seconds == 3)  // Si il reste 3 secondes
                    {
                        Label lblInfo = this.Controls.Find("lblInfoDestination1", true).FirstOrDefault() as Label;
                        if (lblInfo != null) lblInfo.Text = "A l'arrêt";
                    }

                    if (trains[0].Seconds <= 0)
                    {
                        secondTimer.Stop();
                        trains.RemoveAt(0);
                        AddNewTrain();
                        UpdateAllTrainDisplays();

                        // Vider le label après la suppression du train
                        Label lblInfo = this.Controls.Find("lblInfoDestination1", true).FirstOrDefault() as Label;
                        if (lblInfo != null) lblInfo.Text = "";
                    }
                    else
                    {
                        UpdateTrainDisplay(0); // Mise à jour uniquement du premier train
                    }
                }
            }));
        }


        private void UpdateTrainDisplay(int index)
        {
            if (index >= trains.Count)
                return;

            Train train = trains[index];

            Label lblName = this.Controls.Find($"lblTrainNameDestination{index + 1}", true).FirstOrDefault() as Label;
            Label lblDirection = this.Controls.Find($"lblDestination{index + 1}", true).FirstOrDefault() as Label;
            Label lblTime = this.Controls.Find($"lblTimeDestination{index + 1}", true).FirstOrDefault() as Label;
            Label lblInfo = this.Controls.Find("lblInfoDestination1", true).FirstOrDefault() as Label;

            if (lblName != null)
                lblName.Text = train.Name;
            if (lblDirection != null)
                lblDirection.Text = train.Direction;

            if (index == 0) // Gestion spéciale pour le premier train
            {
                if (train.Time == 1 && train.Seconds <= 10)
                {
                    if (lblTime != null) lblTime.Text = "00";
                    if (lblInfo != null && train.Seconds > 3) lblInfo.Text = "À l'approche";
                    if (!soundPlayed)
                    {
                        SoundPlayer soundPlayer = new SoundPlayer(@"C:\Users\Totox\OneDrive\Documents\Visual Studio 2022\Projets\ProjetDeTest\annonce_protrain1_v2.wav");
                        soundPlayer.Play();
                        soundPlayed = true; // Marquer le son comme joué
                    }
                }
                else
                {
                    if (lblTime != null) lblTime.Text = train.Time > 0 ? train.Time.ToString() + " min" : "GO";
                }

                // Jouer le son spécifique à la direction si le premier train change
                if (lastFirstTrain == null || lastFirstTrain != train)
                {
                    if (directionSounds.ContainsKey(train.Direction))
                    {
                        SoundPlayer directionSoundPlayer = new SoundPlayer(directionSounds[train.Direction]);
                        directionSoundPlayer.Play();
                    }
                    lastFirstTrain = train; // Mettre à jour le "dernier" premier train
                }
            }
            else
            {
                if (lblTime != null) lblTime.Text = train.Time > 0 ? train.Time.ToString() + " min" : "00"; // Afficher "00" au lieu de "Arrivé" pour les autres trains
            }

            if (trains[0] != train) // Réinitialiser si ce n'est plus le premier train
            {
                soundPlayed = false; // Variable utilisée uniquement pour le son "à l'approche"
            }
        }


        private void AddNewTrain()
        {
            Train newTrain = new Train
            {
                Name = GenerateRandomTrainName(),
                Direction = GetRandomDirection(), // Utilisation dela  méthode pour obtenir une direction aléatoire
                Time = trains.Max(t => t.Time) + random.Next(1, 31),
                Comment = String.Empty
            };
            trains.Add(newTrain);
        }


        private class Train
        {
            public string Name { get; set; }
            public string Direction { get; set; }
            public int Time { get; set; }
            public string Comment { get; set; }
            public int Seconds { get; set; } = 0;
        }

        private void timerHour_Tick(object sender, EventArgs e)
        {
            lblHour.Text = DateTime.Now.ToString("HH:mm");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
