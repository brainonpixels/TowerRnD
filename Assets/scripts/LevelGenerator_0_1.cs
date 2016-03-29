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

	private Transform levelDebugMarker1;

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

		levelDebugMarker1 = Instantiate (debugMarker);
		levelDebugMarker1.GetComponent<Renderer> ().material = debugMat1;

//		for (int i = 0; i < 360; i++) {			
//			Vector2 pos = levelShape.getPosition ( i );
//			Transform marker = Instantiate (normalMarker);
//			marker.position = RadialMath.radialToEuqlid (new Vector3 ( pos.x, Mathf.Deg2Rad*i, 0.0f ));
//			marker.localEulerAngles = new Vector3 (0.0f, -Mathf.Rad2Deg*pos.y, 0.0f);
//		}

		//generateNextPart ();
		//generatePlatforms (2000);
	}

	void Awake() {
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.G)) {
			addOneBlock ();
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

		Vector2 p = levelShape.getPosition(nextBlockAngle);
		Vector3 lastPoint = RadialMath.radialToEuqlid (new Vector3 (p.x, nextBlockAngle, 0.0f));

		Vector2 nextPoint = levelShape.getNextPoint (nextBlockAngle);
		float distToNext = Mathf.Sqrt ((nextPoint.x - lastPoint.x) * (nextPoint.x - lastPoint.x) + (nextPoint.y - lastPoint.z) * (nextPoint.y - lastPoint.z));

		Transform prefab = chooseNextBlock (distToNext);
		BoxCollider prefabBox = (BoxCollider)prefab.GetComponent<Collider> ();

		Vector2 startPoint = levelShape.getPositionEuqlid (nextBlockAngle);
		Vector2 endPoint = levelShape.getPointAtDist (nextBlockAngle, prefabBox.size.x);
		levelDebugMarker1.position = new Vector3 (endPoint.x, 0.0f, endPoint.y);

		float rotation = RadialMath.euqlidToRadial2( endPoint-startPoint ).y - Mathf.PI/2.0f;
		Vector2 centerPos = Vector2.Lerp (startPoint, endPoint, 0.5f);

		float startAngle = nextBlockAngle;
		float endAngle = RadialMath.euqlidToRadial2 ( endPoint ).y;

		float height = 0.0f;
		int startTopsIndex = Mathf.FloorToInt(startAngle / topsAngle);
		int endTopsIndex = Mathf.FloorToInt(endAngle / topsAngle);
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

		Vector3 radialPos = RadialMath.euqlidToRadial ( new Vector3( centerPos.x, height, centerPos.y ) );
		//Vector3 radialPos = new Vector3 (radius, nextBlockAngle + angleSize/2.0f, height);

		createBlockInstance (prefab, radialPos, rotation);

		nextBlockAngle = endAngle;
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

	private Transform chooseNextBlock(float width) {
		Transform bestBlock = null;
		float bestWidth = 1000.0f;
		foreach (Transform t in blockPrefabs) {
			float tWidth = ((BoxCollider)t.GetComponent<Collider>()).bounds.size.x;
			if ( Mathf.Abs(tWidth-width) < bestWidth ) {
				bestWidth = Mathf.Abs( tWidth-width );
				bestBlock = t;
			}
		}
		return bestBlock;
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
		int[] nextPoint;
		Vector2[] points;

		public LevelShape() {			
		}

		public LevelShape(Transform levelPrefab) {
			
			//ArrayList positions = new ArrayList();
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

			//foreach (Vector3 pos in radialPositions)
			//	positions.Add( RadialMath.radialToEuqlid(pos));

			points = new Vector2[radialPositions.Count];
			for (int i=0; i<radialPositions.Count; i++) {
				Vector3 pos = RadialMath.radialToEuqlid( (Vector3)radialPositions[i] );
				points[i] = new Vector2( pos.x, pos.z );
			}

			levelPoints = new Vector2[360];
			nextPoint = new int[360];

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
				Vector2 p1 = points[index];
				Vector2 p2 = points[index2];
				nextPoint[i] = index2;

				float rotation = RadialMath.euqlidToRadial( p2-p1 ).y - Mathf.PI/2.0f;

				float N = Mathf.Sin(angle) / Mathf.Cos(angle);
				float xd = p2.x - p1.x;
				float yd = p2.y - p1.y;

				float y = (xd*p1.y - yd*p1.x) / (xd + N*yd);
				float x = y * N * -1.0f;

				Vector3 radPos = RadialMath.euqlidToRadial( new Vector3( x, 0.0f, y ));
				levelPoints[i] = new Vector2(radPos.x, rotation);
			}
		}

		public virtual Vector2 getPosition(int index) {
			return levelPoints [index];
		}

		public Vector2 getNextPoint(float angle) {
			return points[ nextPoint[ Mathf.FloorToInt (RadialMath.normalize (angle) * Mathf.Rad2Deg) ] ];
		}

		public virtual Vector2 getPosition(float angle) {
			int i1 = Mathf.FloorToInt (RadialMath.normalize (angle) * Mathf.Rad2Deg);
			int i2 = (i1<359) ? (i1 + 1) : 0;
			return Vector2.Lerp( levelPoints[i1], levelPoints[i2], angle-(float)i1) ;
		}

		public Vector2 getPositionEuqlid(float angle) {
			Vector2 posRad = getPosition (angle);
			Vector3 pos = RadialMath.radialToEuqlid (new Vector3 ( posRad.x, angle, 0.0f  ));
			return new Vector2 (pos.x, pos.z);
		}

		public Vector2 getPointAtDist(float startAngle, float dist) {
			Vector2 startPos = getPositionEuqlid (startAngle);
			int index = nextPoint[ Mathf.FloorToInt (RadialMath.normalize (startAngle) * Mathf.Rad2Deg) ];
			while ( Vector2.Distance(startPos, points[ (index+1)%points.Length ] ) < dist )
				index++;

			Vector2 p1 = points [index % points.Length];
			Vector2 p2 = points [(index+1) % points.Length];

			float A = (p1 - startPos).magnitude;
			//float C = (p2 - p1).magnitude;
			//float E = (p2 - startPos).magnitude;

			if (A >= dist)
				return Vector2.Lerp(startPos, p1, dist/A );


			// square equasion to solve

			float a = (p2 - p1).sqrMagnitude;
			float b = 2.0f * (  (p2.x-p1.x)*(p1.x-startPos.x) + (p2.y-p1.y)*(p1.y-startPos.y) );
			float c = (p1 - startPos).sqrMagnitude - dist * dist;
			float t1 = (-b - Mathf.Sqrt (b * b - 4.0f * a * c)) / (2.0f * a);
			float t2 = (-b + Mathf.Sqrt (b * b - 4.0f * a * c)) / (2.0f * a);
			float t = Mathf.Max (t1, t2);
			return Vector2.Lerp (p1, p2, t);

//			float cosY = ( A*A + C*C - E*E ) / ( 2.0f * A * C );
//
//			float a = 1.0f;
//			float b = -2.0f * A * cosY;
//			float c = A * A - dist * dist;
//
//			float t1 = (-b - Mathf.Sqrt (b * b - 4.0f * a * c)) / (2.0f * a);
//			float t2 = (-b + Mathf.Sqrt (b * b - 4.0f * a * c)) / (2.0f * a);
//			float t = Mathf.Max (t1, t2);
//
//			return Vector2.Lerp (p1, p2, t/C);
		}
	}

	private class TestLevelShape : LevelShape {



		public override Vector2 getPosition(float angle) {
			return new Vector2 ( 3.0f, angle  );
		}
	}
}
