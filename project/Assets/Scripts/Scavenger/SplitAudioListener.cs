using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitAudioListener : MonoBehaviour {

	public Transform playerWrapper;
	private Transform[] playerTransforms;
	private List<List<AudioSource>> audioSources = new List<List<AudioSource>>();
	public float overlapTolerance = 0.3f;

	// Use this for initialization
	void Start () {
		playerTransforms = new Transform[playerWrapper.childCount];
		for (int i = 0; i < playerWrapper.childCount; i++){
			playerTransforms[i] = playerWrapper.GetChild(i);
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdatePosition ();
	}

	private void UpdatePosition(){
		int count = 0;
		Vector3 averagePosition = Vector3.zero;
		foreach(Transform player in playerTransforms){
			if (player.gameObject.GetActive()){
				averagePosition += player.position;
				count++;
			}
		}

		if (count > 0){
			transform.position = averagePosition / count;
		}
	}

	// Populate organized list of audiosources based on clip
	public void StoreAudioSource(AudioSource inputSource){
		bool clipMatch = false;
		foreach (List<AudioSource> sourceGroup in audioSources){
			if (sourceGroup[0].clip == inputSource.clip){
				AudioSource newSource = gameObject.AddComponent<AudioSource>();
				newSource.clip = inputSource.clip;
				newSource.volume = inputSource.volume;
				sourceGroup.Add(newSource);

				clipMatch = true;
				break;
			}
		}

		if (!clipMatch){
			List<AudioSource> newSourceGroup = new List<AudioSource>();
			AudioSource newSource = gameObject.AddComponent<AudioSource>();
			newSource.clip = inputSource.clip;
			newSource.volume = inputSource.volume;
			newSourceGroup.Add(newSource);
			audioSources.Add(newSourceGroup);
			print ("Added");
		}
	}

	// Play audio if conditions are met
	public void PlayAudioSource(AudioSource triggerSource){
		foreach (List<AudioSource> sourceGroup in audioSources){
			if (sourceGroup[0].clip == triggerSource.clip){
				bool overlapAvoided = true;

				foreach (AudioSource checkingSource in sourceGroup){
					if (checkingSource.isPlaying && checkingSource.time < overlapTolerance){
						overlapAvoided = false;
					}
				}

				// If not too close temporally to other source playing, play audio
				if (overlapAvoided){
					AudioSource bestMatch = sourceGroup[0];

					foreach (AudioSource checkingSource in sourceGroup){
						if (!checkingSource.isPlaying || checkingSource.time > bestMatch.time){
							bestMatch = checkingSource;
						}

						break;
					}

					bestMatch.Play();
				}

				break;
			}
		}
	}
}
