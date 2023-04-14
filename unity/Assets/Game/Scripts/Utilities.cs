using UnityEngine;

namespace WorldAsSupport
{
    static class Utilities
    {
        public static int LayerMaskToLayer(LayerMask layerMask)
        {
            int layerNumber = 0;
            int layer = layerMask.value;
            while (layer > 0)
            {
                layer = layer >> 1;
                layerNumber++;
            }
            return layerNumber - 1;
        }

        // returns true if the vector position is within the PlayerCamera frustrum
        // note that it doesn't take occlusion into account (use IsVisibleToCamera for that)
        public static bool IsOnscreen(Camera camera, Vector3 position, bool includeY = true)
        {
            Vector3 visTest = camera.WorldToViewportPoint(position);
            return (
                (visTest.x >= 0 && (visTest.y >= 0 || !includeY)) &&
                (visTest.x <= 1 && (visTest.y <= 1 || !includeY)) &&
                visTest.z >= 0
            );
        }

        // returns true if the gameObject is within the PlayerCamera frustrum
        // note that it doesn't take occlusion into account (use IsVisibleToCamera for that)
        public static bool IsOnscreen(Camera camera, GameObject gameObject, bool includeY = true)
        {
            if (gameObject == null)
            {
                return false;
            }
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider == null)
            {
                // otherwise just test the transform position
                return IsOnscreen(camera, gameObject.transform.position, includeY);
            }
            // if the GameObject has a collider, first test the extents the collider
            // keep a count of those onscreen and offscreen in each axis and dir
            int osX = 0;
            int osY = 0;
            int ltX = 0;
            int ltY = 0;
            int gtX = 0;
            int gtY = 0;
            Vector3[] extents = GetColliderVertexPositions(gameObject);
            foreach (Vector3 extent in extents)
            {
                Vector3 visTest = camera.WorldToViewportPoint(extent);
                // negative Z means behind the camera
                if (visTest.z < 0)
                {
                    continue;
                }
                if (visTest.x < 0)
                {
                    ltX += 1;
                    continue;
                }
                if (visTest.x > 1)
                {
                    gtX += 1;
                    continue;
                }
                osX += 1;
                if (includeY && visTest.y < 0)
                {
                    ltY += 1;
                    continue;
                }
                if (includeY && visTest.y > 1)
                {
                    gtY += 1;
                    continue;
                }
                osY += 1;
                // this extent is fully onscreen, so return true
                return true;
            }

            // the collider may be bigger than the viewport
            // check X spanning screen but Y partially onscreen
            if (gtX > 0 && ltX > 0 && osY > 0)
            {
                return true;
            }
            // check Y spanning screen but X partially onscreen
            if (gtY > 0 && ltY > 0 && osX > 0)
            {
                return true;
            }
            // check X and Y spanning screen
            if (gtX > 0 && ltX > 0 && gtY > 0 && ltY > 0)
            {
                return true;
            }

            return false;
        }

        // returns true if there is nothing occluding the view between the PlayerCamera and gameObject
        // note that it doesn't take into account whether the object is within the camera frustrum (use IsOnscreen for that)
        public static bool IsVisibleToCamera(Camera camera, GameObject gameObject, LayerMask occlusionMask)
        {
            Vector3[] extents = GetColliderVertexPositions(gameObject);

            GameObject occludingObject;

            // test collider extents
            foreach (Vector3 extent in extents)
            {
                occludingObject = GetOccludingObject(
                    camera.transform.position,
                    extent,
                    occlusionMask,
                    gameObject
                );
                if (occludingObject == null)
                {
                    // clear line of sight to collider extent, so object is at least partially visible
                    return true;
                }
            }

            return false;
        }

        // returns GameObject occluding the view between the origin and target vector, or null if none
        // note that it doesn't take into account whether the object is within the camera frustrum (use IsOnscreen for that)
        public static GameObject GetOccludingObject(
            Vector3 origin,
            Vector3 target,
            LayerMask layerMask,
            GameObject ignoredGameObject
        )
        {
            RaycastHit hitInfo;
            // int layerMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask("Guide");
            bool isHit = Physics.Linecast(origin, target, out hitInfo, layerMask);
            if (isHit)
            {
                GameObject hitObject = hitInfo.collider.gameObject;
                if (hitObject != null &&
                    hitObject != ignoredGameObject &&
                    hitObject.transform.parent.gameObject != ignoredGameObject)
                {
                    return hitObject;
                }
            }
            return null;
        }

        public static Vector3[] GetColliderVertexPositions(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return new Vector3[] { Vector3.zero };
            }
            // from https://forum.unity.com/threads/get-vertices-of-box-collider.89301/
            Vector3[] verts = new Vector3[8];
            Collider collider = gameObject.GetComponentInChildren<Collider>();
            if (collider == null)
            {
                throw new System.Exception("No Collider on GameObject: " + gameObject.name);
            }

            Bounds b = collider.bounds;

            verts[0] = b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f;
            verts[1] = b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f;
            verts[2] = b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f;
            verts[3] = b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f;
            verts[4] = b.center - new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f;
            verts[5] = b.center - new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f;
            verts[6] = b.center - new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f;
            verts[7] = b.center - new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f;

            return verts;
        }

        // from: https://gamedev.stackexchange.com/questions/136323/how-do-i-get-objects-using-layer-name
        public static GameObject[] FindGameObjectsInLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            var goArray = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            var goList = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == layer)
                {
                    goList.Add(goArray[i]);
                }
            }
            return goList.ToArray();
        }
    }
}