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
	private Vector3 spawnLocation;

	public float maxVerticalDist = 2.9f;
	public float maxHorizontalDist = 1.0f;

	private ProximityZone[] platformClearZones;

	private Level level;

	private Transform levelDebugMarker1;

	private float lastUpdateHeight = 0.0f;

	void Awake () {

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


	}

	public void startLevel() {

		level = new Level (levelPrefabs);

		level.addOneSection ();
		level.addOneSection ();
		generateBlocks ();
		generatePlatforms (2000);

		level.generateIfNeeded (200.0f);

		addOneBlock ();
		createPlatform (handles [0] as PlatformHandle);
		spawnLocation = platforms.GetChild (0).position;

		generateBlocks ();
		generatePlatforms (2000);
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.G)) {
			addOneBlock ();
		}

		float playerHeight = player.position.y;
		if (level.generateIfNeeded (playerHeight)) {
			generateBlocks ();
			generatePlatforms (2000);
		}

	}

	void FixedUpdate() {

		float playerH = player.transform.position.y;
		if (playerH - lastUpdateHeight > 10.0f) {
			lastUpdateHeight = playerH;
			recycle();
		}

	}

	private void recycle() {
		float playerH = player.transform.position.y;

		foreach (Transform platform in platforms) {
			if (platform.position.y < playerH - 6.0f)
				Destroy(platform.gameObject);
		}
	}

	private float getRadius(float angle, float height) {
		return 2.5f + Random.value;
	}

	private void generateBlocks() {
		while (addOneBlock ());
	}

	private bool addOneBlock () {

		int startTopsIndex = Mathf.FloorToInt(nextBlockAngle / topsAngle);
		float startTops = tops [startTopsIndex];

		if (startTops >= level.getTop ())
			return false;

		Vector2 lastPoint = level.getPositionEuqlid(nextBlockAngle, startTops);
		Vector2 nextPoint = level.getNextPoint (nextBlockAngle, startTops);


		int endTopsIndex = Mathf.FloorToInt( RadialMath.euqlidToRadial2(nextPoint).y / topsAngle);
		if (endTopsIndex < startTopsIndex)
			endTopsIndex += 360;


		float topsThreshhold = 1.0f;


		for (int i = startTopsIndex+1; i < endTopsIndex; i++) {
			float h = tops [i % tops.Length];
			if (h > startTops + topsThreshhold) {
				float angle = (i % tops.Length) * topsAngle;
				nextPoint = level.getPositionEuqlid(angle, h);
				break;
			}
		}

		float distToNext = Vector2.Distance (nextPoint, lastPoint);

		Transform prefab = chooseNextBlock (distToNext);
		BoxCollider prefabBox = (BoxCollider)prefab.GetComponent<Collider> ();

		Vector2 startPoint = level.getPositionEuqlid (nextBlockAngle, startTops);
		Vector2 endPoint = level.getPointAtDist (nextBlockAngle, prefabBox.size.x, startTops);
		//levelDebugMarker1.position = new Vector3 (endPoint.x, 0.0f, endPoint.y);

		float rotation = RadialMath.euqlidToRadial2( endPoint-startPoint ).y - Mathf.PI/2.0f;
		Vector2 centerPos = Vector2.Lerp (startPoint, endPoint, 0.5f);

		float startAngle = nextBlockAngle;
		float endAngle = RadialMath.euqlidToRadial2 ( endPoint ).y;

		float height = 0.0f;
		startTopsIndex = Mathf.FloorToInt(startAngle / topsAngle);
		endTopsIndex = Mathf.FloorToInt(endAngle / topsAngle);

		if (endTopsIndex < startTopsIndex)
			endTopsIndex += 360;

		for (int i = startTopsIndex; i < endTopsIndex; i++) {
			if (tops [i % tops.Length] > height)
				height = tops [i % tops.Length];
		}

		//height += Random.value * heightDistort;

		float topEdge = height + prefabBox.size.y;

		for (int i = startTopsIndex; i < endTopsIndex; i++)
			tops [i % tops.Length] = topEdge;

		Vector3 radialPos = RadialMath.euqlidToRadial ( new Vector3( centerPos.x, height, centerPos.y ) );
		//Vector3 radialPos = new Vector3 (radius, nextBlockAngle + angleSize/2.0f, height);

		createBlockInstance (prefab, radialPos, rotation);

		nextBlockAngle = endAngle;

		return true;
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

			if (platform.position.y > level.getTop() - 10.0f)
				return;

			foreach (PlatformHandle h in handles)
				if ((h.position-platform.position).magnitude<0.1f)
					h.transform.GetComponent<Renderer> ().material = debugMat1;

			getHandlesInRange (platform, handlesInRange, handlesList);
			handlesInRange.Sort (handleComparer);

			int added = 0;
			while (addAnotherOne(added) && handlesInRange.Count > 0) {

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

	private bool addAnotherOne(int alreadyAdded) {
		if (alreadyAdded == 0)
			return true;
		if (alreadyAdded == 1)
			return Random.value > 0.5f;
		if (alreadyAdded == 2)
			return Random.value > 0.8f;
		return false;
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

	public Vector3 getSpawningLocation() {
		return spawnLocation;
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

	public abstract class LevelShape {
		
		public abstract Vector2 getPointRad(int index, float height);
		public abstract Vector2 getPoint(int index, float height);
		public abstract int pointsCount();

		private int getNextIndex(float angle, float height) {

			if (RadialMath.angleInRange (angle, getPointRad( pointsCount() - 1, height).y, getPointRad(0, height).y))
				return 0;

			for (int i = 1; i < pointsCount(); i++) {
				int p2 = i;
				int p1 = (i == 0) ? (pointsCount() - 1) : (i - 1);

				if (getPointRad(p1, height).y < angle && getPointRad(p2, height).y >= angle)
					return p2;
			}
			Debug.Log ("Could not find index for angle " + angle);
			return -1;
		}

		private int getPrevIndex(float angle, float height) {
			return getNextIndex (angle, height) % pointsCount();
		}

		public Vector2 getNextPoint(float angle, float height) {
			//return points[ nextPoint[ Mathf.FloorToInt (RadialMath.normalize (angle) * Mathf.Rad2Deg) ] ];
			return getPoint( getNextIndex( angle, height ), height );
		}



		public virtual Vector2 getPosition(float angle, float height) {
			//int i1 = Mathf.FloorToInt (RadialMath.normalize (angle) * Mathf.Rad2Deg);
			//int i2 = (i1<359) ? (i1 + 1) : 0;
			//return Vector2.Lerp( levelPoints[i1], levelPoints[i2], angle-(float)i1) ;

			int index2 = getNextIndex (angle, height);
			int index = (index2==0) ? (pointsCount()-1) : (index2 - 1);

			Vector2 p1 = getPoint(index, height);
			Vector2 p2 = getPoint(index2, height);

			float rotation = RadialMath.euqlidToRadial( p2-p1 ).y - Mathf.PI/2.0f;

			float N = Mathf.Sin(angle) / Mathf.Cos(angle);
			float xd = p2.x - p1.x;
			float yd = p2.y - p1.y;

			float y = (xd*p1.y - yd*p1.x) / (xd + N*yd);
			float x = y * N * -1.0f;

			Vector3 radPos = RadialMath.euqlidToRadial( new Vector3( x, 0.0f, y ));
			return new Vector2(radPos.x, rotation);

		}

		public Vector2 getPositionEuqlid(float angle, float height) {
			Vector2 posRad = getPosition (angle, height);
			Vector3 pos = RadialMath.radialToEuqlid (new Vector3 ( posRad.x, angle, 0.0f  ));
			return new Vector2 (pos.x, pos.z);
		}

		public Vector2 getPointAtDist(float startAngle, float dist, float height) {
			Vector2 startPos = getPositionEuqlid (startAngle, height);
			int index = getNextIndex (startAngle, height);// nextPoint[ Mathf.FloorToInt (RadialMath.normalize (startAngle) * Mathf.Rad2Deg) ];
			while ( Vector2.Distance(startPos, getPoint( (index+1)%pointsCount(), height ) ) < dist )
				index++;

			Vector2 p1 = getPoint( index % pointsCount(), height );
			Vector2 p2 = getPoint( (index+1) % pointsCount(), height );

			float A = (p1 - startPos).magnitude;
			if (A >= dist)
				return Vector2.Lerp(startPos, p1, dist/A );
			
			float a = (p2 - p1).sqrMagnitude;
			float b = 2.0f * (  (p2.x-p1.x)*(p1.x-startPos.x) + (p2.y-p1.y)*(p1.y-startPos.y) );
			float c = (p1 - startPos).sqrMagnitude - dist * dist;
			float t1 = (-b - Mathf.Sqrt (b * b - 4.0f * a * c)) / (2.0f * a);
			float t2 = (-b + Mathf.Sqrt (b * b - 4.0f * a * c)) / (2.0f * a);
			float t = Mathf.Max (t1, t2);
			return Vector2.Lerp (p1, p2, t);
		}
	}
		
	public class LevelShapeSingle : LevelShape {
	
		Vector2[] points;
		Vector2[] pointsRad;

		public LevelShapeSingle() {			
		}

		public LevelShapeSingle(Transform levelPrefab) {

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

			points = new Vector2[radialPositions.Count];
			pointsRad = new Vector2[radialPositions.Count];

			for (int i=0; i<radialPositions.Count; i++) {
				Vector3 radPos = (Vector3)radialPositions[i];
				Vector3 pos = RadialMath.radialToEuqlid( radPos );
				points[i] = new Vector2( pos.x, pos.z );
				pointsRad[i] = new Vector2( radPos.x, radPos.y);
			}
		}
	
		public Vector2[] getPoints() {
			return points;
		}

		public override int pointsCount() {
			return points.Length;
		}

		public override Vector2 getPointRad(int index, float height) {
			return pointsRad [index];
		}

		public override Vector2 getPoint(int index, float height) {
			return points [index];
		}

	}

	public class Level {

		LevelShapeLerp lerp;

		LevelShapeSingle levelA;
		LevelShapeSingle levelB;

		private float top = 0.0f;

		private LevelShapeSingle lastShape;

		private ArrayList sections = new ArrayList();

		private int shapeIndex = 0;

		private Transform[] prefabs;

		public Level(Transform[] levelPrefabs) {
			//levels = new LevelShape[ levelPrefabs.Length ];
			//for (int i=0; i<levelPrefabs.Length; i++)
			//	levels[i] = new LevelShapeSingle( levelPrefabs[i] );

			//levelA = new LevelShapeSingle( levelPrefabs[1] );
			//levelB = new LevelShapeSingle( levelPrefabs[0] );
			//lerp = new LevelShapeLerp( levelA, levelB, 10.0f, 20.0f );
			prefabs = levelPrefabs;
			lastShape = new LevelShapeSingle( prefabs[shapeIndex++] );
			sections.Add( new LevelSection( lastShape, 0.0f, 10.0f ));
			top = 10.0f;

			addOneSection();
		}

		private LevelShape getShapeByHeight(float height) {

			for (int i=sections.Count-1; i>=0; i--) {
				LevelSection s = (LevelSection)sections[i];
				if (s.start<=height && s.end>height)
					return s.shape;
			}

			return ((LevelSection)sections[sections.Count-1]).shape;
		}

		public Vector2 getNextPoint(float angle, float height){
			return getShapeByHeight (height).getNextPoint (angle, height);
		}

		public virtual Vector2 getPosition(float angle, float height) {
			return getShapeByHeight (height).getPosition (angle, height);
		}

		public Vector2 getPositionEuqlid(float angle, float height) {
			return getShapeByHeight (height).getPositionEuqlid (angle, height);
		}

		public Vector2 getPointAtDist(float startAngle, float dist, float height) {
			return getShapeByHeight (height).getPointAtDist (startAngle, dist, height);
		}

		public bool generateIfNeeded(float playerHeight) {
			if (playerHeight > top - 30.0f) {
				addOneSection ();
				return true;
			}
			return false;
		}

		public void addOneSection() {

			LevelShapeSingle newShape = new LevelShapeSingle (prefabs[ (shapeIndex++)%prefabs.Length ] );

			float start = top;
			float middle = start + 10.0f;
			float end = middle + 10.0f;

			sections.Add (new LevelSection ( new LevelShapeLerp( lastShape, newShape, start, middle ), start, middle ));
			sections.Add (new LevelSection (newShape, middle, end));

			top = end;
			lastShape = newShape;
		}

		public float getTop() {
			return top;
		}
	}

	public class LevelSection {
		public LevelShape shape;
		public float start;
		public float end;

		public LevelSection(LevelShape shape, float start,float end) {
					this.shape =shape;
					this.start = start;
					this.end = end;
		}
	}

	public class LevelShapeLerp : LevelShape {

		public Vector2[] pointsFrom;
		public Vector2[] pointsTo;

		public float heightFrom;
		public float heightTo;

		public LevelShapeLerp(LevelShapeSingle from, LevelShapeSingle to, float heightFrom, float heightTo) {

			int pointsCount = Mathf.Max( from.pointsCount(), to.pointsCount() );

			this.heightFrom = heightFrom;
			this.heightTo = heightTo;

			pointsFrom = new Vector2[pointsCount];
			pointsTo = new Vector2[pointsCount];

			if (from.pointsCount() == to.pointsCount()) {
			
				for (int i=0; i<from.pointsCount(); i++) {
					pointsFrom[i] = from.getPoints()[i];
					pointsTo[i] = to.getPoints()[i];
				}

			} else if (from.pointsCount() > to.pointsCount()) {

				for (int i=0; i<from.pointsCount(); i++)
					pointsFrom[i] = from.getPoints()[i];

				for (int i=0; i<from.pointsCount(); i++) {

					Vector2 closest = new Vector2(0.0f, 0.0f);
					float minDist = 10000.0f;
					for (int j=0; j<to.pointsCount(); j++) {
						float dist = Vector2.Distance( to.getPoints()[j], pointsFrom[i]);
						if (dist < minDist) {
							minDist = dist;
							closest = to.getPoints()[j];
						}
					}

					pointsTo[i] = closest;
				}
			} else if (from.pointsCount() < to.pointsCount()) {
				
				for (int i=0; i<to.pointsCount(); i++)
					pointsTo[i] = to.getPoints()[i];
				
				for (int i=0; i<to.pointsCount(); i++) {
					
					Vector2 closest = new Vector2(0.0f, 0.0f);
					float minDist = 10000.0f;
					for (int j=0; j<from.pointsCount(); j++) {
						float dist = Vector2.Distance( from.getPoints()[j], pointsTo[i]);
						if (dist < minDist) {
							minDist = dist;
							closest = from.getPoints()[j];
						}
					}
					
					pointsFrom[i] = closest;
				}
			}

		}

		public override int pointsCount() {
			return pointsFrom.Length;
		}

		public override Vector2 getPointRad(int index, float height) {
			return RadialMath.euqlidToRadial2 ( getPoint (index, height) );
		}

		public override Vector2 getPoint(int index, float height) {
			float t = (height - heightFrom) / (heightTo - heightFrom);
			return Vector2.Lerp (pointsFrom [index], pointsTo [index], t);
		}
	}
}
