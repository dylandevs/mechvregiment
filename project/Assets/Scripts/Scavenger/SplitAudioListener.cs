using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitAudioListener : MonoBehaviour {

	public Transform playerWrapper;
	private Transform[] playerTransforms;
	private List<List<AudioSource>> audioSources = new List<List<AudioSource>>();
	public float overlapTolerance = 0.3f;
	public int maxPoolSize = 15;

	void Awake(){
		playerTransforms = new Transform[playerWrapper.childCount];
		for (int i = 0; i < playerWrapper.childCount; i++){
			playerTransforms[i] = playerWrapper.GetChild(i);
		}
	}

	// Use this for initialization
	void Start () {
		
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
				if (sourceGroup.Count < maxPoolSize){
					AudioSource newSource = gameObject.AddComponent<AudioSource>();
					newSource.clip = inputSource.clip;
					sourceGroup.Add(newSource);

					clipMatch = true;
				}
				break;
			}
		}

		if (!clipMatch){
			List<AudioSource> newSourceGroup = new List<AudioSource>();
			AudioSource newSource = gameObject.AddComponent<AudioSource>();
			newSource.clip = inputSource.clip;
			newSourceGroup.Add(newSource);
			audioSources.Add(newSourceGroup);
			print ("Added");
		}
	}

	// Play audio if conditions are met
	public void PlayAudioSource(AudioSource triggerSource, Vector3 position = default(Vector3)){
		float shortestDistSqr = Vector3.SqrMagnitude (playerTransforms[0].position - position);
		float maxDistSqr = triggerSource.maxDistance * triggerSource.maxDistance;
		foreach(Transform playerPos in playerTransforms){
			if (playerPos.gameObject.GetActive()){
				float checkDistance = Vector3.SqrMagnitude (playerPos.position - position);
				if (checkDistance < shortestDistSqr){
					shortestDistSqr = checkDistance;
				}
			}
		}

		if (shortestDistSqr < maxDistSqr){
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

						bestMatch.volume = triggerSource.volume;

						if (position != Vector3.zero){
							if (shortestDistSqr > 0){
								float newVol = Mathf.Max(0, ((maxDistSqr - shortestDistSqr) / maxDistSqr));

								newVol *= newVol * newVol * newVol * newVol * newVol * newVol * newVol * newVol * newVol;
								newVol *= bestMatch.volume;
								bestMatch.volume = newVol;
							}
						}

						bestMatch.Play();
					}

					break;
				}
			}
		}
	}
}
