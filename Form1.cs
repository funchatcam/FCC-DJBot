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

        public Form1()
        {

            

            InitializeComponent();

            SQLiteConnection.CreateFile("djbot.sqlite");

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

        private void Form1_Load(object sender, EventArgs e)
        {
            
            //progressBar1.Maximum = (int)mh.GetCurrentTrack().GetLength() * 100;
            //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
            //pictureBox2.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_640);

            label5.Text = mh.GetCurrentTrack().GetTrackName();
            //linkLabel1.LinkClicked += (senderTwo, args) => Process.Start(mh.GetCurrentTrack().GetTrackURI());
            label4.Text = mh.GetCurrentTrack().GetArtistName();
            //linkLabel2.LinkClicked += (senderTwo, args) => Process.Start(mh.GetCurrentTrack().GetArtistURI());
            //linkLabel3.Text = mh.GetCurrentTrack().GetAlbumName();
            //linkLabel3.LinkClicked += (senderTwo, args) => Process.Start(mh.GetCurrentTrack().GetAlbumURI());

            //label9.Text = mh.IsPlaying().ToString();
            //label11.Text = ((int)(mh.GetVolume() * 100)).ToString();
            //label7.Text = mh.IsAdRunning().ToString();

            eh.OnTrackChange += new SpotifyEventHandler.TrackChangeEventHandler(trackchange);
            eh.OnTrackTimeChange += new SpotifyEventHandler.TrackTimeChangeEventHandler(timechange);
            eh.OnPlayStateChange += new SpotifyEventHandler.PlayStateEventHandler(playstatechange);
            eh.OnVolumeChange += new SpotifyEventHandler.VolumeChangeEventHandler(volumechange);
            eh.SetSynchronizingObject(this);
            eh.ListenForEvents(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fcc = new FCC.Chat();
            fcc.mute_toggle();
        }

        private void button2_Click(object sender, EventArgs e)
        {
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

                        string username = words[0];
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
                                fcc.sendchat("Hello " + username + "!");
                                if(checkBox1.Checked) {
                                    fcc.sendchat("Want some music? Type !djbot");
                                }
                            }
                        }

                        //catch quits
                        if (linetext.Trim().StartsWith("Quit"))
                        {
                            username = words2[1];
                            fcc.sendchat("byeeee " + username + "!");
                        }

                        if (chatline.Trim().StartsWith("!djbot"))
                        {
                            fcc.sendchat("Commands: !play !skip !stop !song !tunes !satchville");
                        }

                        //play
                        if (chatline.Trim().StartsWith("!play"))
                        {
                            spotify.Update();
                            if (!mh.IsPlaying())                          
                            {
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
                        if (chatline.Trim().StartsWith("!playspotify"))
                        {
                            spotify.Update();
                            if (!mh.IsPlaying())
                            {
                                fcc.handsfree_toggle();
                            string url = words[4];
                            fcc.sendchat(username + " requested " + url);
                                
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

                                mh.PlayURL(url);
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing (Chat Tunes Playlist): " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
                            }
                        }

                        //play spotify URI
                        if (chatline.Trim().StartsWith("!satchville"))
                        {
                            spotify.Update();
                            if (!mh.IsPlaying())
                            {
                                fcc.handsfree_toggle();
                                string url = "http://open.spotify.com/user/nyith/playlist/7KEM0n4ibp4wZcQQlgjiD9";

                                mh.PlayURL(url);
                                spotify.Update();
                                label4.Text = mh.GetCurrentTrack().GetArtistName();
                                label5.Text = mh.GetCurrentTrack().GetTrackName();
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing (Satchville Playlist): " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
                            }
                        }

                        //whats this song?
                        if (chatline.Trim().StartsWith("!song"))
                        {
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
                                //pictureBox1.Image = await spotify.GetMusicHandler().GetCurrentTrack().GetAlbumArtAsync(AlbumArtSize.SIZE_160);
                                fcc.sendchat("Now playing: " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());
                            }
                            else
                            {
                                fcc.sendchat("Not playing, !play to start");
                            }
                      
                        }

                        //stop
                        if (chatline.Trim().StartsWith("!stop"))
                        {
                            mh.Pause();
                            fcc.handsfree_toggle();

                        }


                        ///// Name That Tune
                        if (chatline.Trim() == Properties.Settings.Default.CurrentArtist.ToLower())
                        {
                            // Artist 1 point
                            addPoints(username, 1);


                            fcc.sendchat(username + " is correct! 1 point.");


                        }

                    }
                }

            }

            catch (Exception e)
            {
                //log(e.ToString());
            }
        }

        private void addPoints(string username, int points) {
            //int currentPoints = Properties.Settings.Default.String1;
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
            Properties.Settings.Default.Save();

            //pictureBox1.Image = await e.new_track.GetAlbumArtAsync(AlbumArtSize.SIZE_160);
            //pictureBox2.Image = await e.new_track.GetAlbumArtAsync(AlbumArtSize.SIZE_640);
            //label7.Text = mh.IsAdRunning().ToString();

            
            //fcc.sendchat("Now playing: " + mh.GetCurrentTrack().GetArtistName() + " - " + mh.GetCurrentTrack().GetTrackName());

        }
        private void timechange(TrackTimeChangeEventArgs e)
        {
            label6.Text = formatTime(e.track_time) + "/" + formatTime(mh.GetCurrentTrack().GetLength());
            progressBar1.Maximum = (int)mh.GetCurrentTrack().GetLength() * 100;
            progressBar1.Value = (int)e.track_time * 100;
            spotify.Update();
            label4.Text = mh.GetCurrentTrack().GetArtistName();
            label5.Text = mh.GetCurrentTrack().GetTrackName();
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
            //start name that tune game
            //users guess the artist or song title for points
            //the first person who guesses wins and you wait until the next song

            // Artist: 1 point
            // Song title: 2 points

           



        }
    }
}
