using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour {

	#region Statics for indexing
	const int TYPE_A = 0, TYPE_B = 1, TYPE_C = 2, TYPE_D = 3;
	const int A_01 = 0, A_02 = 1, A_03 = 2, A_04 = 3;
	const int B_01 = 4, B_02 = 5, B_03 = 6, B_04 = 7;
	const int C_01 = 8, C_02 = 9, C_03 = 10, C_04 = 11;
	const int D_01 = 12, D_02 = 13, D_03 = 14, D_04 = 15;
	const int NUM_ENEMIES = 16;
	#endregion


	private GameController gc;
	public Waypoint startPoint;

	public bool playing = false;
	private bool skipWaveWaitFlag = false;

	//Max enemies we can ever spawn in a wave is 216 (as of writing)
	private Queue<EnemyManager> waveQueueCurrent = new Queue<EnemyManager>(220);
	private Queue<EnemyManager> waveQueueNext = new Queue<EnemyManager>(220);
	private float[] enemyProbablities = new float[NUM_ENEMIES];


	void Start () {
		gc = GetComponent<GameController>();
		InitializeProbabilities();
		if(playing)
			StartCoroutine(WaveSpawner());
	}


	//Called by a UI button press
	public void SkipWaveWait() {
		if (!isBuildingQueue)
			skipWaveWaitFlag = true;
	}


	void SpawnEnemy(EnemyManager manager, Vector3 startPosition, float offset) {
		Vector3 forward = Common.RemoveYComp(startPoint.next.transform.position - startPoint.transform.position);
		Quaternion spawnRot = Quaternion.LookRotation(forward, Vector3.up);

		GameObject newEnemyObj = (GameObject)Instantiate(manager.prefab, startPosition, spawnRot);
		newEnemyObj.name = manager.name;
		Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();
		newEnemy.Initialize(manager, gc, startPoint.next, offset);
	}


	#region Getting enemies for wave
	void InitializeProbabilities() {
		for(int i = 0; i < enemyProbablities.Length; i++) {
			enemyProbablities[i] = 0f;
		}
	}

	#region Probabilities
	void SetProbabilities(int type, float p1, float p2, float p3, float p4) {
		switch(type) {
		case TYPE_A:
			enemyProbablities[A_01] = p1;
			enemyProbablities[A_02] = p2;
			enemyProbablities[A_03] = p3;
			enemyProbablities[A_04] = p4;
			break;
		case TYPE_B:
			enemyProbablities[B_01] = p1;
			enemyProbablities[B_02] = p2;
			enemyProbablities[B_03] = p3;
			enemyProbablities[B_04] = p4;
			break;
		case TYPE_C:
			enemyProbablities[C_01] = p1;
			enemyProbablities[C_02] = p2;
			enemyProbablities[C_03] = p3;
			enemyProbablities[C_04] = p4;
			break;
		case TYPE_D:
			enemyProbablities[D_01] = p1;
			enemyProbablities[D_02] = p2;
			enemyProbablities[D_03] = p3;
			enemyProbablities[D_04] = p4;
			break;
		}
	}

	void SetProbabilityGroups(int type1, int type2) {

		int w = PlayerStats.wave + 1;
		bool singleType = w <= 5;

		switch(w % 5) {
		case 1:
			if (singleType)
				SetProbabilities(type1, 1f, 0f, 0f, 0f);
			else {
				SetProbabilities(type1, 0f, 0f, 0.1f, 0.2f);
				SetProbabilities(type2, 0.7f, 0f, 0f, 0f);
			}break;
		case 2:
			if (singleType)
				SetProbabilities(type1, 0.75f, 0.25f, 0f, 0f);
			else {
				SetProbabilities(type1, 0f, 0f, 0.05f, 0.2f);
				SetProbabilities(type2, 0.5f, 0.25f, 0f, 0f);
			}break;
		case 3:
			if (singleType)
				SetProbabilities(type1, 0.5f, 0.4f, 0.1f, 0f);
			else {
				SetProbabilities(type1, 0f, 0f, 0f, 0.1f);
				SetProbabilities(type2, 0.25f, 0.35f, 0.3f, 0f);
			}break;
		case 4:
			if (singleType)
				SetProbabilities(type1, 0f, 0.5f, 0.4f, 0.1f);
			else {
				SetProbabilities(type1, 0f, 0f, 0f, 0f);
				SetProbabilities(type2, 0f, 0.5f, 0.3f, 0.2f);
			}break;
		case 0://0 is the highest wave (due to mod op)
			if (singleType) 
				SetProbabilities(type1, 0f, 0.25f, 0.3f, 0.45f);
			else {
				SetProbabilities(type1, 0f, 0f, 0f, 0f);
				SetProbabilities(type2, 0f, 0.25f, 0.3f, 0.45f);
			}break;
		}


	}

	void UpdateProbablities() {
		/*
		 * A become available at wave 0; obsolete at wave 10
		 * B become available at wave 5; obsolete at wave 15
		 * C become available at wave 10; obsolete at wave 20
		 * D become available at wave 15
		 */

		int w = PlayerStats.wave + 1; //wave + 1 because we want the next wave's probabilities
		if (w <= 5)			SetProbabilityGroups(TYPE_A, -1);
		else if (w <= 10)	SetProbabilityGroups(TYPE_A, TYPE_B);
		else if (w <= 15)	SetProbabilityGroups(TYPE_B, TYPE_C);
		else 				SetProbabilityGroups(TYPE_C, TYPE_D);
	}
	#endregion

	int GetNumberEnemiesForWave() {

		//How many to add for each wave range
		const int BASE = 15;
		const int W_1to5 = 4;
		const int W_6to10 = 8;
		const int W_11to15 = 12;
		const int W_16to20 = 16;

		//This is just used to tell what the most enemies to spawn in a wave will ever be
		const int max = BASE + (W_1to5*5) + (W_6to10*5) + (W_11to15*5) + (W_16to20*5);

		int w = PlayerStats.wave;
		int n = BASE;

		if (w > 15) {
			n += (w-15) * W_16to20;
			w = 15;
		}
		if (w > 10) {
			n += (w-10) * W_11to15;
			w = 10;
		}
		if (w > 5) {
			n += (w-5) * W_6to10;
			w = 5;
		}
		if (w > 0) {
			n += (w-0) * W_1to5;
		}

		return n;
	}

	int FetchEnemyIndex() {

		int i = 0;
		//Can skip checking A after wave 10, B after wave 15
		if (PlayerStats.wave > 15)		i = 8;
		else if (PlayerStats.wave > 10)		i = 4;

		float r = Random.value;
		float curr = 0f;
		while (i < NUM_ENEMIES) {
			curr += enemyProbablities[i];
			if (curr >= r)
				return i;
			i++;
		}

		//Shouldn't make it here
		Debug.LogWarning("Error in probablities.  Wave: "+PlayerStats.wave);
		return 0;
	}

	//We use a coroutine to build the queue so that we spread it out over many frames
	bool isBuildingQueue;
	IEnumerator BuildSpawnQueue() {

		isBuildingQueue = true;
		waveQueueNext.Clear();

		//How many enemies will we spawn this wave?
		int numToSpawn = GetNumberEnemiesForWave();
		int count = 0;
		while (count < numToSpawn) {

			int enemyIndex = FetchEnemyIndex(); //get an enemy index from probability table
			waveQueueNext.Enqueue(gc.enemies[enemyIndex]);
			count++;
			yield return null;
		}

		isBuildingQueue = false;
	}

	#endregion


	IEnumerator WaveSpawner() {

		//Cached instructions for coroutine yield calls
		const float start_wait = 5f;
		const float wave_wait = 15f;
		const float line_wait = 1f;
		const float each_wait = 0.2f;

		Vector3 line = startPoint.GetLineStartToEnd();
		Vector3 lineNorm = line.normalized;

		float lineWidthRaw = line.magnitude;
		float lineWidth = lineWidthRaw - 2f*(line.magnitude*Waypoint.trim_percent);

		float lineSecond = lineWidthRaw / 2f;
		float lineThird = lineWidthRaw / 3f;
		float lineFourth = lineWidthRaw / 4f;
		float lineFifth = lineWidthRaw / 5f;
		float lineSixth = lineWidthRaw / 6f;

		Stack<EnemyManager> lineEnemies = new Stack<EnemyManager>(5); //used to store enemies in our list; max 5 per line

		//Build the first spawn queue (for wave 1)
		PlayerStats.wave = 0;
		UpdateProbablities();
		StartCoroutine(BuildSpawnQueue());
		PlayerStats.wave = 1;

		//Start wait
		yield return new WaitForSeconds(start_wait);

		//master loop -> each loop marks a complete wave
		while (PlayerStats.wave <= PlayerStats.maxWaves) {

			//If the spawn queue didnt finish building we need to wait for it (always true on first wave)
			if (isBuildingQueue)
				yield return new WaitUntil( () => !isBuildingQueue);

			//Swap the next queue into the current queue
			waveQueueCurrent = new Queue<EnemyManager>(waveQueueNext);
		

			//Start loading the next queue
			UpdateProbablities();
			StartCoroutine(BuildSpawnQueue());



			//Spawning the wave
			int enemiesThisWave = waveQueueCurrent.Count;
			int enemiesSpawned = 0;
			while (enemiesSpawned < enemiesThisWave) {

				//Get enemies from queue until line is full or queue is empty
				float lineWeight = 0f;
				while (lineWeight < lineWidth) {

					//Done spawning
					if (waveQueueCurrent.Count == 0)
						break;

					//Add next to line if it fits
					lineWeight += waveQueueCurrent.Peek().modelWidth;
					if (lineWeight <= lineWidth)
						lineEnemies.Push(waveQueueCurrent.Dequeue());
				}


				//Unload the line

				//Get the spacings for the line
				int numOnLine = lineEnemies.Count;
				float spawnDistance; //min 1, max 5, on the line
				if (numOnLine == 1) 	 spawnDistance = lineSecond;
				else if (numOnLine == 2) spawnDistance = lineThird;
				else if (numOnLine == 3) spawnDistance = lineFourth;
				else if (numOnLine == 4) spawnDistance = lineFifth;
				else 					 spawnDistance = lineSixth;

				float minSpeed = Mathf.Infinity;
				int i = 1;
				while (lineEnemies.Count > 0) {
					Vector3 linePos = startPoint.transform.position + (i * spawnDistance * lineNorm);
					Vector3 dirToLinePos = linePos - startPoint.transform.position;
					float offset = Vector3.Magnitude(dirToLinePos) / lineWidthRaw;

					EnemyManager enemy = lineEnemies.Pop();
					if (enemy.startSpeed < minSpeed)
						minSpeed = enemy.startSpeed;
					
					SpawnEnemy(enemy, linePos, offset);
					enemiesSpawned++;
					i++;
					yield return new WaitForSeconds(each_wait*(1f/numOnLine)); //Spawn one enemy depending on the number on the line
				}

				yield return new WaitForSeconds(line_wait*(1f/minSpeed)); //Spawn one line depending on the slowest member of the line
			}

			StartCoroutine(WaveClock(wave_wait));
			if (!waitingForNextWave) yield return new WaitUntil( () => waitingForNextWave );
			yield return new WaitWhile( () => waitingForNextWave );

			//Check if we have lost the game
			if (PlayerStats.gameOver)
				break;

			PlayerStats.wave += 1; //update the wave counter
		}
	}

	//Give wave time to PlayerStats as seconds and fractional components
	bool waitingForNextWave = false;

	IEnumerator WaveClock(float waitTimeSeconds) {

		waitingForNextWave = true;
		float currTime = 0f;
		while(currTime < waitTimeSeconds) {

			if (skipWaveWaitFlag)
				break;

			currTime += Time.fixedDeltaTime;
			PlayerStats.waveWaitSeconds = Mathf.CeilToInt(waitTimeSeconds-currTime);
			yield return new WaitForFixedUpdate();
		}
			
		PlayerStats.waveWaitSeconds = 0;
		skipWaveWaitFlag = false;
		waitingForNextWave = false;
	}



}
