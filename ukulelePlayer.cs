using System;
using System.Threading;
using SoundTouch;
using NAudio.Wave;
using System.IO;

namespace ukulele_tab
{
    /// <summary>
    /// Virtual ukulele to play notes
    /// </summary>
    internal class UkulelePlayer
    {
        readonly int[,] _ukulele_frets = new int[4, 18]; //matrix of ukulele notes
        readonly SoundTouchProcessor pSoundTouch =  new();
        readonly WaveFileReader reader = new("ukulele_g3.wav"); //wav file of original note to be modified to new note

        readonly int SamplesCount = 44100; // = sample rate * length of sound in sec = 44100 * 1
        float[] samples = new float[44100]; //samples array of original wav file -- IEEE 32 bit

        /// <summary>
        /// Constructor:
        /// Initialize ukulele matrix with approriate pitch change values (by semitones)
        /// Set approriate sample rate and num of channels for SoundTouch processor
        /// </summary>
        public UkulelePlayer()
        {
            /*
             [[a4, ...]
              [e4,....]
              [c4, ...]
              [g3, ...]]
             */
            //fourth string: g3
            for (int i = 0; i < 18; i++)
            {
                _ukulele_frets[3, i] = i;
            }
            //thrid string: c4
            for (int i = 0; i < 18; i++)
            {
                _ukulele_frets[2, i] = i + 5;
            }
            //second string: e4
            for (int i = 0; i < 18; i++)
            {
                _ukulele_frets[1, i] = i + 9;
            }
            //first string: a4
            for (int i = 0; i < 18; i++)
            {
                _ukulele_frets[0, i] = i + 14;
            }

            //set sample rate and channel for soundtouch processor
            pSoundTouch.SampleRate = this.SamplesCount;
            pSoundTouch.Channels = 1;

            this.samples = this.GetSamples();
        }

        /// <summary>
        /// Read wav file into samples array to be used for SoundTouch processor
        /// </summary>
        /// <returns>Returns a samples array of the original wav file (G3 note) </returns>
        private float[] GetSamples()
        {
            ISampleProvider samples = reader.ToSampleProvider();
            float[] dest_buffer = new float[this.SamplesCount];
            samples.Read(dest_buffer, 0, this.SamplesCount);

            return dest_buffer;
        }

        /// <summary>
        /// Playback wav stream
        /// </summary>
        /// <param name="stream">wav stream to be played back</param>
        private static void PlayOutput(float[] stream)
        {
            //write a new wav file based on given wav stream
            WaveFormat waveFormat = new(44100, 16, 1);
            //assign unique name based on number of seconds since 1970
            //this allows consecutive notes to be played without relying on the previous note's processes to complete
            //and thus without freezing the app while waiting for the note in process to play
            string name = "output" + DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString()+".wav";
            using (WaveFileWriter writer = new(name, waveFormat))
            {
                writer.WriteSamples(stream, 0, stream.Length);
            }

            //read new wav file into a stream
            var input = new NAudio.Wave.AudioFileReader(name);

            //play the stream
            var wo = new DirectSoundOut();
            wo.Init(input);
            wo.Play();
            
            
            while (wo.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(1000);
            }
            

            //dispose processes when done with playback
            wo.Dispose();
            input.Dispose(); //dispose stream/process in order to delete output.wav
            File.Delete(name); //delete file so the next new note can be created with the same wav file name
        
        }

        /// <summary>
        /// Change the pitch of original note (G3) using SoundTouch in order to get desired note
        /// </summary>
        /// <param name="str_pos">string position of desired note</param>
        /// <param name="fret_pos">fret position of desired note</param>
        /// <returns>Returns wav stream of desired note</returns>
        private float[] Process(int str_pos, int fret_pos)
        {
            pSoundTouch.PitchSemiTones = _ukulele_frets[str_pos, fret_pos];
            pSoundTouch.PutSamples(this.samples, this.SamplesCount);
            
            float[] output = new float[this.SamplesCount];
            pSoundTouch.ReceiveSamples(output, this.SamplesCount);

            pSoundTouch.Flush();
            pSoundTouch.Clear();

            return output;
        }

        /// <summary>
        /// Get and then play desired note 
        /// </summary>
        /// <param name="str_pos">string position of desired note</param>
        /// <param name="fret_pos">fret position of desired note</param>
        /// <remarks>Public method for user interface</remarks>
        public void PlayNote(int str_pos, int fret_pos)
        {
            PlayOutput(this.Process(str_pos, fret_pos));
        }

        /// <summary>
        /// Change the semitones of the ukulele based on given string and tuning
        /// </summary>
        /// <param name="str_pos">string position whose tuning is to be changed</param>
        /// <param name="tuning">the amount of semitones; postive means up; negative means down</param>
        /// <remarks>Public method for user interface</remarks>
        public void ChangeTuning(int str_pos, int tuning)
        {
            for (int i = 0; i < 18; ++i)
            {
                _ukulele_frets[str_pos, i] += tuning;
            }
            
        }
    }

}

