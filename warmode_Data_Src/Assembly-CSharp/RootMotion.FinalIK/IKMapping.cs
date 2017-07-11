using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKMapping
	{
		[Serializable]
		public class BoneMap
		{
			public Transform transform;

			public IKSolver.Node node;

			public Vector3 defaultLocalPosition;

			public Quaternion defaultLocalRotation;

			public Vector3 localSwingAxis;

			public Vector3 localTwistAxis;

			public Vector3 planePosition;

			public Vector3 ikPosition;

			public Quaternion defaultLocalTargetRotation;

			private Quaternion maintainRotation;

			public float length;

			public Quaternion animatedRotation;

			private IKSolver.Node planeNode1;

			private IKSolver.Node planeNode2;

			private IKSolver.Node planeNode3;

			public Vector3 swingDirection
			{
				get
				{
					return this.transform.rotation * this.localSwingAxis;
				}
			}

			public bool isNodeBone
			{
				get
				{
					return this.node != null;
				}
			}

			private Quaternion targetRotation
			{
				get
				{
					if (this.planeNode1.solverPosition == this.planeNode3.solverPosition)
					{
						return Quaternion.identity;
					}
					return Quaternion.LookRotation(this.planeNode2.solverPosition - this.planeNode1.solverPosition, this.planeNode3.solverPosition - this.planeNode1.solverPosition);
				}
			}

			private Quaternion lastAnimatedTargetRotation
			{
				get
				{
					if (this.planeNode1.transform.position == this.planeNode3.transform.position)
					{
						return Quaternion.identity;
					}
					return Quaternion.LookRotation(this.planeNode2.transform.position - this.planeNode1.transform.position, this.planeNode3.transform.position - this.planeNode1.transform.position);
				}
			}

			public void Initiate(Transform transform, IKSolver solver)
			{
				this.transform = transform;
				IKSolver.Point point = solver.GetPoint(transform);
				if (point != null)
				{
					this.node = (point as IKSolver.Node);
				}
			}

			public void StoreDefaultLocalState()
			{
				this.defaultLocalPosition = this.transform.localPosition;
				this.defaultLocalRotation = this.transform.localRotation;
			}

			public void FixTransform(bool position)
			{
				if (position)
				{
					this.transform.localPosition = this.defaultLocalPosition;
				}
				this.transform.localRotation = this.defaultLocalRotation;
			}

			public void SetLength(IKMapping.BoneMap nextBone)
			{
				this.length = Vector3.Distance(this.transform.position, nextBone.transform.position);
			}

			public void SetLocalSwingAxis(IKMapping.BoneMap swingTarget)
			{
				this.SetLocalSwingAxis(swingTarget, this);
			}

			public void SetLocalSwingAxis(IKMapping.BoneMap bone1, IKMapping.BoneMap bone2)
			{
				this.localSwingAxis = Quaternion.Inverse(this.transform.rotation) * (bone1.transform.position - bone2.transform.position);
			}

			public void SetLocalTwistAxis(Vector3 twistDirection, Vector3 normalDirection)
			{
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				this.localTwistAxis = Quaternion.Inverse(this.transform.rotation) * twistDirection;
			}

			public void SetPlane(IKSolver.Node planeNode1, IKSolver.Node planeNode2, IKSolver.Node planeNode3)
			{
				this.planeNode1 = planeNode1;
				this.planeNode2 = planeNode2;
				this.planeNode3 = planeNode3;
				this.UpdatePlane(true, true);
			}

			public void UpdatePlane(bool rotation, bool position)
			{
				Quaternion lastAnimatedTargetRotation = this.lastAnimatedTargetRotation;
				if (rotation)
				{
					this.defaultLocalTargetRotation = QuaTools.RotationToLocalSpace(this.transform.rotation, lastAnimatedTargetRotation);
				}
				if (position)
				{
					this.planePosition = Quaternion.Inverse(lastAnimatedTargetRotation) * (this.transform.position - this.planeNode1.transform.position);
				}
			}

			public void SetIKPosition()
			{
				this.ikPosition = this.transform.position;
			}

			public void MaintainRotation()
			{
				this.maintainRotation = this.transform.rotation;
			}

			public void SetToIKPosition()
			{
				this.transform.position = this.ikPosition;
			}

			public void FixToNode(float weight, IKSolver.Node fixNode = null)
			{
				if (fixNode == null)
				{
					fixNode = this.node;
				}
				if (weight >= 1f)
				{
					this.transform.position = fixNode.solverPosition;
					return;
				}
				this.transform.position = Vector3.Lerp(this.transform.position, fixNode.solverPosition, weight);
			}

			public Vector3 GetPlanePosition()
			{
				return this.planeNode1.solverPosition + this.targetRotation * this.planePosition;
			}

			public void PositionToPlane()
			{
				this.transform.position = this.GetPlanePosition();
			}

			public void RotateToPlane(float weight)
			{
				Quaternion quaternion = this.targetRotation * this.defaultLocalTargetRotation;
				if (weight >= 1f)
				{
					this.transform.rotation = quaternion;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, quaternion, weight);
			}

			public void Swing(Vector3 swingTarget, float weight)
			{
				this.Swing(swingTarget, this.transform.position, weight);
			}

			public void Swing(Vector3 pos1, Vector3 pos2, float weight)
			{
				Quaternion quaternion = Quaternion.FromToRotation(this.transform.rotation * this.localSwingAxis, pos1 - pos2) * this.transform.rotation;
				if (weight >= 1f)
				{
					this.transform.rotation = quaternion;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, quaternion, weight);
			}

			public void Twist(Vector3 twistDirection, Vector3 normalDirection, float weight)
			{
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				Quaternion quaternion = Quaternion.FromToRotation(this.transform.rotation * this.localTwistAxis, twistDirection) * this.transform.rotation;
				if (weight >= 1f)
				{
					this.transform.rotation = quaternion;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, quaternion, weight);
			}

			public void RotateToMaintain(float weight)
			{
				if (weight <= 0f)
				{
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.maintainRotation, weight);
			}

			public void RotateToEffector(float weight)
			{
				if (!this.isNodeBone)
				{
					return;
				}
				float num = weight * this.node.effectorRotationWeight;
				if (num <= 0f)
				{
					return;
				}
				if (num >= 1f)
				{
					this.transform.rotation = this.node.solverRotation;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.node.solverRotation, num);
			}
		}

		protected IKSolver solver;

		public virtual bool IsValid(IKSolver solver, Warning.Logger logger = null)
		{
			return true;
		}

		protected virtual void OnInitiate()
		{
		}

		public void Initiate(IKSolver solver)
		{
			this.solver = solver;
			this.OnInitiate();
		}

		protected bool BoneIsValid(Transform bone, IKSolver solver, Warning.Logger logger = null)
		{
			if (bone == null)
			{
				if (logger != null)
				{
					logger("IKMappingLimb contains a null reference.");
				}
				return false;
			}
			if (solver.GetPoint(bone) == null)
			{
				if (logger != null)
				{
					logger("IKMappingLimb is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.");
				}
				return false;
			}
			return true;
		}
	}
}
