using UnityEngine;
/// <summary>
/// Script that play Notif sound
/// </summary>

[RequireComponent(typeof(AudioSource))]
public class NotificationAR : MonoBehaviour
{
    private AudioSource ConnectionAS;
    private void Start()
    {
        ConnectionAS = GetComponent<AudioSource>();
    }

    public void Connection()
    {
        ConnectionAS.Stop();
        ConnectionAS.pitch = 1;
        ConnectionAS.volume = 1;
        ConnectionAS.Play();
    }
    
    public void Searching(){
        if(!ConnectionAS.isPlaying)
        {
            ConnectionAS.pitch = 1.5f;
            ConnectionAS.volume = .2f;
            ConnectionAS.Play();
        }

    }

}
