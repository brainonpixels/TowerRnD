using UnityEngine;
using System.Collections;

public class LevelGenerator_0_1 : MonoBehaviour {

	public enum RelativePos
	{
		TOO_CLOSE,
		TOO_FAR,
		JUST_RIGHT
	}

	public Transform[] blockPrefabs;
	public Transform[] platformPrefabs;
	public Transform[] levelPrefabs;
	public Transform platforms;
	public Transform blocks;
	public Transform player;

	public Transform debugMarker;
	public Transform normalMarker;
	public Material debugMat1;
	public Material debugMat2;
	public Material debugMat3;
	public Material debugMat4;

	public Material defaultHandleMaterial;
	public Material selectedHandleMaterial;


	public float heightDistort = 1.0f;

	private float nextBlockAngle = 0.0f;
	private float[] tops;
	private float topsAngle = 0.0f;

	private ArrayList handles = new ArrayList();
	private ArrayList topPlatforms = new ArrayList();
	private float jumpTopDist = 0.0f;
	private int jumpTopPathIndex = 0;
	private float jumpHeight = 0.0f;
	private Vector3[] jumpPath;

	public float maxVerticalDist = 2.9f;
	public float maxHorizontalDist = 1.0f;

	private ProximityZone[] platformClearZones;

	private LevelShape levelShape;

	void Start () {

		RadialPlayerControl pc = player.GetComponent<RadialPlayerControl> ();
		PlatformHandle.debugMaterial = debugMat1;

		jumpPath = new Vector3[70];
		float dt = 0.01666f;
		float x = 0.0f;
		float y = 0.0f;
		float vertSpeed = pc.jumpSpeed;
		float horSpeed = pc.maxSpeed;

		for (int i = 0; i < 70; i++) {
			x = x + horSpeed * dt;
			y = y + vertSpeed * dt;
			if (y > jumpHeight) {
				jumpHeight = y;
				jumpTopDist = x;
				jumpTopPathIndex = i;
			}
			vertSpeed = vertSpeed + pc.gravity * dt;
			jumpPath[i] = new Vector3 (0.0f, x, y);
		}

		platformClearZones = new ProximityZone[3];
		platformClearZones [0] = new BoxZone (0.5f, 0.5f);
		platformClearZones [1] = new BoxZone (0.35f, 1.2f);
		platformClearZones [2] = new BoxZone (0.28f, 2.5f);

		tops = new float[360];
		topsAngle = Mathf.PI * 2.0f / tops.Length;
		for (int i = 0; i < tops.Length; i++)
			tops [i] = 0.0f;

		foreach (Transform t in platforms) {
			topPlatforms.Add (t);
		}

		levelShape = new LevelShape (levelPrefabs[0]);

//		for (int i = 0; i < 360; i++) {			
//			Vector2 pos = levelShape.getPosition ( i );
//			Transform marker = Instantiate (normalMarker);
//			marker.position = RadialMath.radialToEuqlid (new Vector3 ( pos.x, Mathf.Deg2Rad*i, 0.0f ));
//			marker.localEulerAngles = new Vector3 (0.0f, -Mathf.Rad2Deg*pos.y, 0.0f);
//		}

		generateNextPart ();
		generatePlatforms (2000);
	}

	void Awake() {
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.G)) {
			generatePlatforms (1);
		}
	}

	void FixedUpdate() {

//		if ( Input.GetMouseButtonDown(0))
//		{
//			int layerMask = 1 << 9;
//			RaycastHit hit;
//			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
//
//			if (Physics.Raycast (ray, out hit, 1000.0f, layerMask)) {
//
//				foreach (PlatformHandle h in handles) {
//					h.transform.GetComponent<Renderer> ().material = defaultHandleMaterial;
//				}
//
//				hit.transform.GetComponent<Renderer> ().material = selectedHandleMaterial;
//
//				Vector3 radialPos = RadialMath.euqlidToRadial (hit.transform.position);
//
//				for (int i = 0; i < jumpPath.Length; i++) {
//					Vector3 pos = RadialMath.radialToEuqlid ( radialPos + jumpPath[i] );
//					Transform x = Instantiate (debugMarker);
//					x.position = pos;
//					x.localScale = new Vector3 (0.2f, 0.2f, 0.2f);
//				}
//
//				foreach (PlatformHandle h in handles) {
//					if (h.transform != hit.transform) {
//						if (canJump (RadialMath.euqlidToRadial (hit.transform.position), RadialMath.euqlidToRadial (h.transform.position)))
//							h.transform.GetComponent<Renderer> ().material = debugMat1;
//						else
//							h.transform.GetComponent<Renderer> ().material = defaultHandleMaterial;
//					}
//
//				}
//			}
//		}

	}

	private float getRadius(float angle, float height) {
		return 2.5f + Random.value;
	}

	private void generateNextPart() {
		for (int i = 0; i < 100; i++)
			addOneBlock ();
	}

	private void addOneBlock () {

		Transform prefab = chooseNextBlock ();

		BoxCollider prefabBox = (BoxCollider)prefab.GetComponent<Collider> ();

		float angle = nextBlockAngle;
		float angleSize = 0.0f;
		float radius = 0.0f;
		float rotation = 0.0f;
		do {
			Vector2 pos = levelShape.getPosition(angle);
			radius = pos.x;
			rotation = pos.y;
			angleSize = RadialMath.radialSize (radius, prefabBox.size.x, angle-rotation);
			angle += 5.0f * Mathf.Deg2Rad;
		} while ( angle - angleSize / 2.0f < nextBlockAngle );

		//float radius = getRadius (nextBlockAngle, 0.0f);
		//float angleSize = RadialMath.radialSize (radius, prefabBox.size.x);

		float height = 0.0f;
		int startTopsIndex = Mathf.FloorToInt(nextBlockAngle / topsAngle);
		int endTopsIndex = Mathf.FloorToInt((nextBlockAngle + angleSize) / topsAngle);
		int endTopsIndex2 = 0;
		if (endTopsIndex >= tops.Length) {
			endTopsIndex2 = endTopsIndex - tops.Length;
			endTopsIndex = tops.Length - 1;
		}

		for (int i = startTopsIndex; i < endTopsIndex; i++)
			if (tops [i] > height)
				height = tops [i];
		for (int i = 0; i < endTopsIndex2; i++)
			if (tops [i] > height)
				height = tops [i];

		height += Random.value * heightDistort;

		float topEdge = height + prefabBox.size.y;

		for (int i = startTopsIndex; i < endTopsIndex; i++)
			tops [i] = topEdge;
		for (int i = 0; i < endTopsIndex2; i++)
			tops [i] = topEdge;

		Vector3 radialPos = new Vector3 (radius, nextBlockAngle + angleSize/2.0f, height);

		createBlockInstance (prefab, radialPos, rotation);

		nextBlockAngle += angleSize;
		nextBlockAngle = RadialMath.normalize (nextBlockAngle);
	}

	private void createBlockInstance(Transform prefab, Vector3 radialPos, float rotation) {
		Transform instance = Instantiate (prefab);
		instance.parent = blocks;
		instance.position = RadialMath.radialToEuqlid (radialPos);
		instance.localEulerAngles = new Vector3 ( 0.0f, - Mathf.Rad2Deg * rotation ,0.0f );

		foreach (Transform t in instance) {
			if (t.tag == "platform_mount") {
				PlatformHandle handle = new PlatformHandle ();
				handle.transform = t;
				handle.position = t.position;
				handle.radialPosition = RadialMath.euqlidToRadial (handle.position);
				handles.Add (handle);
			}
		}
	}

	private Transform chooseNextBlock() {		
		return blockPrefabs[Random.Range(0, blockPrefabs.Length)];
	}

	private void generatePlatforms(int iterMax) {

		ArrayList handlesInRange = new ArrayList ();
		HandleDistComparer handleComparer = new HandleDistComparer ();

		ArrayList handlesList = new ArrayList ();
		foreach (PlatformHandle h in handles)
			handlesList.Add(h);

		int iter = 0;
		while (topPlatforms.Count > 0 && iter<iterMax) {



			Transform platform = topPlatforms [0] as Transform;

			for (int i = 1; i < topPlatforms.Count; i++) {
				Transform p = topPlatforms [i] as Transform;
				if (p.position.y < platform.position.y)
					platform = p;
			}

			foreach (PlatformHandle h in handles)
				if ((h.position-platform.position).magnitude<0.1f)
					h.transform.GetComponent<Renderer> ().material = debugMat1;

			getHandlesInRange (platform, handlesInRange, handlesList);
			handlesInRange.Sort (handleComparer);

			int added = 0;
			while (added < 5000 && handlesInRange.Count > 0) {

				PlatformHandle h = handlesInRange [0] as PlatformHandle;
				//PlatformHandle h = handlesInRange [Random.Range(0,handlesInRange.Count-1)] as PlatformHandle;
				handlesInRange.Remove (h);

				if (!h.enabled)
					continue;

				createPlatform (h);
				added++;
				h.transform.GetComponent<Renderer> ().material = debugMat2;

				foreach (PlatformHandle otherH in handlesList) {
					if (getRelativePosition (h.radialPosition, otherH.radialPosition) == RelativePos.TOO_CLOSE) {
						otherH.enabled = false;
					}
				}

			}

			topPlatforms.Remove(platform);
			iter++;
		}
	}

	private void createPlatform(PlatformHandle handle) {
		Transform platform = Instantiate (platformPrefabs [0]);
		platform.position = handle.position;
		platform.localEulerAngles = new Vector3 ( 0.0f, handle.transform.parent.localEulerAngles.y ,0.0f );
		platform.parent = platforms;
		topPlatforms.Add (platform);
		handle.enabled = false;
	}

	private void getHandlesInRange(Transform platform, ArrayList list, ArrayList handlesList) {

		list.Clear ();
		Vector3 radialPos = RadialMath.euqlidToRadial (platform.position);

		foreach (PlatformHandle h in handlesList) {

			if (!h.enabled)
				continue;

			RelativePos pos = getRelativePosition (h.radialPosition, radialPos);

			if (pos != RelativePos.JUST_RIGHT) {
				continue;
			}				

			if (canJump (radialPos, h.radialPosition)) {
				h.dist = Mathf.Abs(radialPos.z + 1.0f - h.radialPosition.z);
				list.Add (h);
			}
		}

	}

	private bool canJump(Vector3 from, Vector3 to) {

		float horDist = RadialMath.dist (to.y, from.y) - 0.27f;
		float vertDist = to.z - from.z;

		if (horDist < jumpTopDist)
			return vertDist < jumpHeight;

		for (int i = jumpTopPathIndex; i < jumpPath.Length-1; i++) {
			if (jumpPath [i].y <= horDist && jumpPath [i + 1].y > horDist)
				return vertDist <= jumpPath [i].z;
		}
			
		return false;
	}

	private RelativePos getRelativePosition(Vector3 from, Vector3 to) {

		float horDist = RadialMath.dist (to.y, from.y);
		float vertDist = Mathf.Abs(to.z - from.z);

		if (vertDist > maxVerticalDist || horDist > maxHorizontalDist)
			return RelativePos.TOO_FAR;

		foreach (ProximityZone zone in platformClearZones)
			if (zone.isInZone(horDist, vertDist, from.x))
				return RelativePos.TOO_CLOSE;

		return RelativePos.JUST_RIGHT;
	}

	private class PlatformHandle {

		public static Material debugMaterial;

		public Vector3 position;
		public Vector3 radialPosition;
		public float dist = 0.0f;

		private bool en = true;

		public bool enabled  {
			set { 
				en = value;
				if (!en)
					transform.GetComponent<Renderer> ().material = debugMaterial;
			}
			get { return en; }
		}

		public Transform transform;
	}

	public class HandleDistComparer : IComparer {
		public int Compare( object x, object y )  {
			return ((PlatformHandle)x).dist > ((PlatformHandle)y).dist ? -1 : 1;
		}
	}

	private abstract class ProximityZone {
		public abstract bool isInZone(float horDist, float vertDist, float radius);
	}

	private class BoxZone : ProximityZone {

		private float width;
		private float height;

		public BoxZone(float hor, float vert) {
			width = hor;
			height = vert;
		}

		public override bool isInZone(float horDist, float vertDist, float radius) {
			return horDist<width && vertDist<height;
		}
	}

	private class LevelShape {

		Vector2[] levelPoints; // radius and rotation for each of 360 degrees

		public LevelShape() {			
		}

		public LevelShape(Transform levelPrefab) {
			
			ArrayList positions = new ArrayList();
			ArrayList radialPositions = new ArrayList();
			ArrayList radialPositionsTmp = new ArrayList();

			foreach (Transform t in levelPrefab)
				radialPositionsTmp.Add(RadialMath.euqlidToRadial(t.position));
			
			while (radialPositionsTmp.Count>0) {
				Vector3 minV = new Vector3(0.0f, 10.0f, 0.0f);
				foreach (Vector3 v in radialPositionsTmp)
					if (v.y < minV.y)
						minV = v;
				radialPositionsTmp.Remove(minV);
				radialPositions.Add(minV);
			}

			foreach (Vector3 pos in radialPositions)
				positions.Add( RadialMath.radialToEuqlid(pos));

			levelPoints = new Vector2[360];

			int index = radialPositions.Count-1;
			int index2 = 0;

			for (int i=0; i<360; i++) {
				float angle = Mathf.Deg2Rad * (float)i;

				if (angle>Mathf.PI && index2==0)
					angle = angle - Mathf.PI*2.0f;

				if (angle>((Vector3)radialPositions[index2]).y) {
					index++;
					index2++;
					if (index >= radialPositions.Count) index = 0;
					if (index2 >= radialPositions.Count) index2 = 0;
				}

				Vector3 p1rad = (Vector3)radialPositions[index];
				Vector3 p2rad = (Vector3)radialPositions[index2];
				Vector3 p1 = (Vector3)positions[index];
				Vector3 p2 = (Vector3)positions[index2];

				float rotation = RadialMath.euqlidToRadial( p2-p1 ).y - Mathf.PI/2.0f;

				float N = Mathf.Sin(angle) / Mathf.Cos(angle);
				float xd = p2.x - p1.x;
				float yd = p2.z - p1.z;

				float y = (xd*p1.z - yd*p1.x) / (xd + N*yd);
				float x = y * N * -1.0f;

				Vector3 radPos = RadialMath.euqlidToRadial( new Vector3( x, 0.0f, y ));
				levelPoints[i] = new Vector2(radPos.x, rotation);
			}
		}

		public virtual Vector2 getPosition(int index) {
			return levelPoints [index];
		}

		public virtual Vector2 getPosition(float angle) {
			return levelPoints[ Mathf.FloorToInt( RadialMath.normalize(angle)*Mathf.Rad2Deg ) ];
		}
	}

	private class TestLevelShape : LevelShape {



		public override Vector2 getPosition(float angle) {
			return new Vector2 ( 3.0f, angle  );
		}
	}
}
