using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEngine;

namespace Assets.Controllers
{
    public class CameraController : GameBase
    {
        [SerializeField]
        private Direction _cameraDirection = Direction.N;

        public float stickMinZoom, stickMaxZoom;

        public float swivelMinZoom, swivelMaxZoom;

        public float moveSpeedMinZoom, moveSpeedMaxZoom;

        public float rotationSpeed;

        private Transform swivel, stick;

        private float zoom = 1f;

        private float rotationAngle;

        public void Awake()
        {
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.Euler(0, -90, 0);

            swivel = transform.GetChild(0);
            swivel.localPosition = new Vector3(0, 0, 0);
            swivel.localRotation = Quaternion.Euler(45, 0, 0);
            
            stick = swivel.GetChild(0);
            stick.localPosition = new Vector3(0,0,-20);
            stick.localRotation = Quaternion.identity;

            MainCamera = GetComponentInChildren<Camera>();
        }

        public Camera MainCamera { get; private set; }

        public void MoveTo(Vector3 point)
        {
            transform.position = point;
        }

        public void AdjustZoom(float delta)
        {
            zoom = Mathf.Clamp01(zoom + delta);

            float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
            stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
            swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        public Direction Facing
        {
            get { return _cameraDirection; }
            set { _cameraDirection = value; }
        }

        public void AdjustRotation(float delta)
        {
            float newRot;
            if (delta < 0)
            {
                newRot = transform.localRotation.eulerAngles.y - 45;
            }
            else if (delta > 0)
            {
                newRot = transform.localRotation.eulerAngles.y + 45;
            }
            else return;

            var newDirVal = (int)Facing + delta;
            if (newDirVal < 0)
            {
                Facing = Direction.NW;
            }
            else if
                (newDirVal > (int)Direction.NW)
            {
                Facing = Direction.N;
            }
            else
            {
                Facing = (Direction)newDirVal;
            }

            transform.localRotation =Quaternion.Euler(0f, newRot, 0f);

            //transform.localRotation = Quaternion.Euler(0f, newRot, 0f);
            /*
            rotationAngle += delta * rotationSpeed * Time.deltaTime;
            if (rotationAngle < 0f)
            {
                rotationAngle += 360f;
            }
            else if (rotationAngle >= 360f)
            {
                rotationAngle -= 360f;
            }
            //transform.Rotate(Vector3.up, rotationAngle);
            transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
            */
        }

        public void AdjustPosition(float xDelta, float zDelta)
        {
            Vector3 direction =
                transform.localRotation *
                new Vector3(xDelta, 0f, zDelta).normalized;
            float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
            float distance =
                Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) *
                damping * Time.deltaTime;

            Vector3 position = transform.localPosition;
            position += direction * distance;
            transform.localPosition = ClampPosition(position);
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            //Adapted from a dynamically generate map, need the "visable range" so we can clamp camera scroll to it

            //float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
            //position.x = Mathf.Clamp(position.x, 0f, xMax);

            //float zMax = (grid.cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
            //position.z = Mathf.Clamp(position.z, 0f, zMax);

            return position;
        }
    }
}
