using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;   // Assign your AudioSource (attached to the Camera)
    public AudioClip[] playlist;      // Drag & drop your 30 songs into this array in the Inspector
    private int currentTrackIndex = 0;

    void Start()
    {
        if (playlist.Length > 0)
        {
            PlaySong(currentTrackIndex);
        }
    }

    void Update()
    {
        // Check if the song has ended by comparing time and length
        if (playlist.Length > 0 && audioSource.clip != null)
        {
            if (!audioSource.isPlaying && audioSource.time >= audioSource.clip.length - 0.1f)
            {
                PlayNextSong();
            }
        }
    }

    void PlaySong(int index)
    {
        if (index < playlist.Length)
        {
            audioSource.clip = playlist[index]; // Set the next song
            audioSource.Play();                 // Play the song
        }
    }

    void PlayNextSong()
    {
        currentTrackIndex++; // Move to the next song
        if (currentTrackIndex >= playlist.Length)
        {
            currentTrackIndex = 0; // Loop back to the first song if all are played
        }
        PlaySong(currentTrackIndex);
    }
}
