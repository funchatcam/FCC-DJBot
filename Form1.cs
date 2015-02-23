using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FCC;
using SpotifyAPI.SpotifyLocalAPI;
using System.Threading;
using SpotifyEventHandler = SpotifyAPI.SpotifyLocalAPI.SpotifyEventHandler;
using System.Media;
using System.Data.SQLite;

namespace DJBot
{
    public partial class Form1 : Form
    {
        static FCC.Chat fcc;
        SpotifyLocalAPIClass spotify;
        SpotifyMusicHandler mh;
        SpotifyEventHandler eh;

        // Holds our connection with the database
        SQLiteConnection m_dbConnection;

        public Form1()
        {    

            InitializeComponent();

            connectToDatabase();
            //createTable();
            //fillTable();
            printHighscores();

            Properties.Settings.Default.Answered = false;
            Properties.Settings.Default.Save();

            spotify = new SpotifyLocalAPIClass();
            if (!SpotifyLocalAPIClass.IsSpotifyRunning())
            {
                spotify.RunSpotify();
                Thread.Sleep(5000);
            }

            if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
            {
                spotify.RunSpotifyWebHelper();
                Thread.Sleep(4000);
            }

            if (!spotify.Connect())
            {
                Boolean retry = true;
                while (retry)
                {
                    if (MessageBox.Show("SpotifyLocalAPIClass could'nt load!", "Error", MessageBoxButtons.RetryCancel) == System.Windows.Forms.DialogResult.Retry)
                    {
                        if (spotify.Connect())
                            retry = false;
                        else
                            retry = true;
                    }
                    else
                    {
                        this.Close();
                        return;
                    }
                }
            }
            mh = spotify.GetMusicHandler();
            eh = spotify.GetEventHandler();

            eh.OnTrackChange += new SpotifyEventHandler.TrackChangeEventHandler(trackchange);
            eh.OnTrackTimeChange += new SpotifyEventHandler.TrackTimeChangeEventHandler(timechange);
            eh.OnPlayStateChange += new SpotifyEventHandler.PlayStateEventHandler(playstatechange);
            eh.OnVolumeChange += new SpotifyEventHandler.VolumeChangeEventHandler(volumechange);
            eh.SetSynchronizingObject(this);
            eh.ListenForEvents(true);

            spotify.Update();
            if (mh.IsPlaying())
            {  
                label4.Text = mh.GetCurrentTrack().GetArtistName();
                label5.Text = mh.GetCurrentTrack().GetTrackName();
            }
            else
            {
                label4.Text = "";
                label5.Text = "";
            }
        }

        //dbstuff
        // Creates a connection with our database file.
        void connectToDatabase()
        {
            m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            m_dbConnection.Open();
        }
        // Creates a table named 'highscores' with two columns: name (a string of max 20 characters) and score (an int)
        void createTable()
        {
            string sql = "create table highscores (name varchar(20), score int)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }
        // Inserts some values in the highscores table.
        // As you can see, there is quite some duplicate code here, we'll solve this in part two.
        void fillTable()
        {
            string sql = "insert into highscores (name, score) values ('Me', 3000)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "insert into highscores (name, score) values ('Myself', 6000)";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "insert into highscores (name, score) values ('And I', 9001)";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }
        // Writes the highscores to the console sorted on score in descending order.
        void printHighscores()
        {
            string sql = "select * from highscores order by score desc";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);
            Console.ReadLine();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fcc = new FCC.Chat();
            fcc.mute_toggle();
        }

        private void button2_Click(object sender, EventArgs e)        {
            fcc = new FCC.Chat();
            MessageBox.Show(fcc.get_version());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fcc = new FCC.Chat();
            fcc.handsfree_toggle();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            fcc = new FCC.Chat();
            fcc.sendchat(textBox1.Text);
            textBox1.Text = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            fcc = new FCC.Chat();
            fcc.quit();
        }

        private void roomgrabber()
        {

            fcc = new FCC.Chat();

            try
            {
                string linetext = fcc.get_last_chat_line().TrimEnd(); //grab last line from chat
                int totalLines = this.richTextBox1.Lines.Length; // count lines in textbox

                if (totalLines == 0)
                {
                    richTextBox1.AppendText(linetext + "\n");

                }
                else
                {
                    string lastline = richTextBox1.Lines[totalLines - 2];
                    lastline = lastline.TrimEnd(); //get last line from textbox

                    if (linetext != lastline) //if line is different append to textbox
                    {
                        richTextBox1.AppendText(linetext + "\n");

                        char[] delimiterChars = { ' ' };
                        string[] words = linetext.Split(delimiterChars);

                        char[] delimiterChars2 = { ':' };
                        string[] words2 = linetext.Split(delimiterChars2);

                        string username = words[0].Trim();
                        string chatline = words2[1];

                        //Console.WriteLine(username + " 11 " + chatline);

                        if (checkBox1.Checked)
                        {
                            
                        }
                        else
                        {
                            return;
                        }

                        //catch joins
                        if (linetext.Trim().StartsWith("Join"))
                        {
                            if (checkBox2.Checked)
                            {
                                username = words2[1];
                                
                                if(checkBox1.Checked) {
                                    //fcc.sendchat("Hello " + username + "!");
                                    fcc.sendchat("/msg " + username.Trim() + " Want some music? Type !djbot");
                                }
                            }
                        }

                        //catch quits
                        if (linetext.Trim().StartsWith("Quit"))
                        {
                            username = words2[1];
                            fcc.sendchat("byeeee " + username + "!");
                        }

                        if (chatline.Trim().StartsWith("!djbot") || chatline.Trim().StartsWith("!help"))
                        {
                            fcc.sendchat("Commands: !play !skip !stop !song !game !tunes !satchville");
                        }

                        //play
                        if (chatline.Trim().StartsWith("!play"))
                        {
                            spotify.Update();
                            if (!mh.IsPlaying())                          
                            {
                                Properties.Settings.Default.Answered = false;
                                Properties.Settings.Default.Save();
                                fcc.handsfree_toggle();
                                mh.Play();
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                               // pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing: " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
                            }
                        }

                        //play spotify URI
                        if (chatline.Trim().StartsWith("!spotify"))
                        {
                            spotify.Update();
                            if (!mh.IsPlaying())
                            {
                                fcc.handsfree_toggle();
                            string url = words[4];
                            fcc.sendchat(username + " requested " + url);
                            Properties.Settings.Default.Answered = false;
                            Properties.Settings.Default.Save();
                                mh.PlayURL(url);
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing: " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
                            }
                        }

                        //play spotify URI
                        if (chatline.Trim().StartsWith("!tunes"))
                        {
                            spotify.Update();
                            if (!mh.IsPlaying())
                            {
                                fcc.handsfree_toggle();
                                string url = "http://open.spotify.com/user/nyith/playlist/4DSfVjPuXtlCLSLHWDq1gv";
                                Properties.Settings.Default.Answered = false;
                                Properties.Settings.Default.Save();
                                mh.PlayURL(url);
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing (Chat Tunes Playlist)");
                            }
                            else
                            {
                                string url = "http://open.spotify.com/user/nyith/playlist/4DSfVjPuXtlCLSLHWDq1gv";
                                Properties.Settings.Default.Answered = false;
                                Properties.Settings.Default.Save();
                                mh.PlayURL(url);
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing (Chat Tunes Playlist)");
                            }
                        }

                        //play spotify URI
                        if (chatline.Trim().StartsWith("!satchville"))
                        {
                            string url = "http://open.spotify.com/user/nyith/playlist/7KEM0n4ibp4wZcQQlgjiD9";
                            spotify.Update();
                            if (!mh.IsPlaying())
                            {
                                fcc.handsfree_toggle();
                                
                                Properties.Settings.Default.Answered = false;
                                Properties.Settings.Default.Save();
                                mh.PlayURL(url);
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing (Satchville Playlist)");
                            }
                            else
                            {
                                Properties.Settings.Default.Answered = false;
                                Properties.Settings.Default.Save();
                                mh.PlayURL(url);
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing (Satchville Playlist)");
                            }
                        }

                        //whats this song?
                        if (chatline.Trim().StartsWith("!song"))
                        {

                            if (checkBox3.Checked && Properties.Settings.Default.Answered == false)
                            {
                                fcc.sendchat("Game: Name That Tune is active. Win points by typing Artist or Songtitle!");
                                return;
                            }

                            spotify.Update();
                            if (mh.IsPlaying())
                            {
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing: " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
                            }
                        }

                        //let users skip
                        if (chatline.Trim().StartsWith("!skip"))
                        {
                            spotify.Update();
                            if (mh.IsPlaying())
                            {
                                mh.Skip();
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();

                                Properties.Settings.Default.Answered = false;
                                Properties.Settings.Default.Save();

                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                //fcc.sendchat("Now playing: " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
                            }
                            else
                            {
                                fcc.sendchat("Not playing, !play to start");
                            }
                        }

                        //stop
                        if (chatline.Trim().StartsWith("!stop") || chatline.Trim().StartsWith("!quit"))
                        {
                            mh.Pause();
                            fcc.handsfree_toggle();

                            Properties.Settings.Default.Answered = false;
                            Properties.Settings.Default.Save();
                        }

                        //game
                        if (chatline.Trim().StartsWith("!game"))
                        {
                            if (checkBox3.Checked)
                            {
                                fcc.sendchat("/msg " + username + " Play name that tune by guessing the artist or the song title.");
                            }
                            else
                            {
                                fcc.sendchat("/msg " + username + " Game: Name that tune is disabled right now");
                            }
                        }

                        ///// Name That Tune
                        if (checkBox3.Checked)
                        {

                            string CurrentArtist = Properties.Settings.Default.CurrentArtist.ToLower();
                            string CurrentSongTitle = Properties.Settings.Default.CurrentSongTitle.ToLower();
                            bool Answered = Properties.Settings.Default.Answered;
                            //Properties.Settings.Default.Save();

                            // Artist 1 point
                            if (chatline.Trim() == CurrentArtist)
                            {
                                if (Answered == false)
                                {
                                    Properties.Settings.Default.Answered = true;
                                    Properties.Settings.Default.Save();
                                    addPoints(username, 1);
                                    fcc.sendchat(username + " guessed the ARTIST correctly! Score: " + getUserScore(username));
                                }
                                else
                                {
                                    fcc.sendchat("Already answered!");
                                }
                            }

                            // songtitle 2 points
                            if (chatline.Trim() == CurrentSongTitle )
                            {
                                if (Answered == false)
                                {
                                    Properties.Settings.Default.Answered = true;
                                    Properties.Settings.Default.Save();
                                    addPoints(username, 2);
                                    fcc.sendchat(username + " guessed the SONGTITLE correctly! Score: " + getUserScore(username));
                                }
                                else
                                {
                                    fcc.sendchat("Already answered!");
                                }
                            }

                            //clue
                            if (chatline.Trim().StartsWith("!clue"))
                            {
                                fcc.sendchat("Clue: " + jumble(CurrentArtist) + " : " + jumble(CurrentSongTitle));
                            }

                            //get my score
                            if (chatline.Trim().StartsWith("!myscore"))
                            {
                                string score = getUserScore(username);
                                fcc.sendchat(username + " score: " + score);
                            }

                            //get user score
                            if (chatline.Trim().StartsWith("!userscore"))
                            {
                                string score = getUserScore(username);
                                fcc.sendchat(username + " score: " + score);
                            }

                        } //// name that tune

                    }
                }

            }

            catch (Exception e)
            {
                //log(e.ToString());
            }
        }

        private string jumble(string word)
        {
            var jumble = new StringBuilder(word);
            int length = word.Length;
            var random = new Random();
            for (int i = length - 1; i > 0; i--)
            {
                int j = random.Next(i);
                char temp = jumble[j];
                jumble[j] = word[i];
                jumble[i] = temp;
            }
            return (jumble.ToString());
        }

        private void addPoints(string username, int points) {
            string sql = "insert into highscores values ($username, $points)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("$username", username);
            command.Parameters.AddWithValue("$points", points);

            command.ExecuteNonQuery();
        }

        private string getUserScore(string username)
        {
            string score = "0";

            string sql = "select SUM(score) from highscores where name = $username";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("$username", username);

            SQLiteDataReader sqReader = command.ExecuteReader();

            try
            {
                while (sqReader.Read())
                {
                    Console.WriteLine(sqReader.GetInt32(0));
                    score = sqReader.GetInt32(0).ToString();
                }

                sqReader.Close();
            }

            catch (InvalidCastException e) 
            {
                Console.WriteLine(e);
            }

            return score;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            roomgrabber();
        }


        //spotify bot stuff
        private void volumechange(VolumeChangeEventArgs e)
        {
            //label11.Text = ((int)(mh.GetVolume() * 100)).ToString();
        }
        private void playstatechange(PlayStateEventArgs e)
        {
            //label9.Text = e.playing.ToString();
        }
        private void trackchange(TrackChangeEventArgs e)
        {
            progressBar1.Maximum = (int)mh.GetCurrentTrack().GetLength() * 100;
            //label1.Text = e.new_track.GetTrackName();
            label4.Text = e.new_track.GetArtistName();
            label5.Text = e.new_track.GetTrackName();

            Properties.Settings.Default.CurrentArtist = e.new_track.GetArtistName();
            Properties.Settings.Default.CurrentSongTitle = e.new_track.GetTrackName();
            Properties.Settings.Default.Answered = false;
            Properties.Settings.Default.Save();
        }
        private void timechange(TrackTimeChangeEventArgs e)
        {
            label6.Text = formatTime(e.track_time) + "/" + formatTime(mh.GetCurrentTrack().GetLength());
            progressBar1.Maximum = (int)mh.GetCurrentTrack().GetLength() * 100;
            progressBar1.Value = (int)e.track_time * 100;
            spotify.Update();
        }
        private String formatTime(double sec)
        {
            TimeSpan span = TimeSpan.FromSeconds(sec);
            String secs = span.Seconds.ToString(), mins = span.Minutes.ToString();
            if (secs.Length < 2)
                secs = "0" + secs;
            return mins + ":" + secs;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            mh.Pause();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBox1.Checked)
            //    mh.Mute();
            //else
            //    mh.UnMute();
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            mh.Previous();
        }

        private void button8_Click_1(object sender, EventArgs e)
        {

            mh.Skip();
            spotify.Update();
            label4.Text = mh.GetCurrentTrack().GetArtistName();
            label5.Text = mh.GetCurrentTrack().GetTrackName();
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            //Not working yet
            //if (SpotifyAPI.IsValidSpotifyURI(textBox1.Text))
            //mh.PlayURL(textBox1.Text);
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            mh.Play();
        }

        public void button11_Click(object sender, EventArgs e)
        {
            SystemSounds.Exclamation.Play();
        }

        // button !song
        private void button12_Click(object sender, EventArgs e)
        {
            fcc.sendchat("Now playing: " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
        }

        private void button13_Click(object sender, EventArgs e)
        {
            //clear scores
            string sql = "delete from highscores";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }
    }
}
