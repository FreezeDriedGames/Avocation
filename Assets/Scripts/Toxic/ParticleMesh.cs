/************************************************************************************
Copyright (C) 2015 by Nicholas LaCroix

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
************************************************************************************/

/************************************************************************************
TODO:	Add support for skinned meshes! I'm unsure if SkinnedMeshRenderer.mesh is
		updated as the skinning is updated. May have to call BakeMesh() on it.
************************************************************************************/

using UnityEngine;
using System.Collections;

namespace Toxic
{

	[AddComponentMenu("Toxic/Rendering/Particle Mesh")]
	[RequireComponent(typeof(ParticleSystem))]
	[ExecuteInEditMode] // So I can preview what it's actually doing in the editor
	public class ParticleMesh : MonoBehaviour
	{
		public float startAngularVelocity = 0.0f;
		public Vector3 startAxisOfRotation = Vector3.zero;
		public bool useVertColor = false;

		private ParticleSystem _ps = null;
		private Vector3[] _verts = null;
		private int[] _tris = null;
		private Color32[] _colors = null;

		private float _spawn_time = 0.0f;
		private float _time_cache = 0.0f;

		void Start()
		{
			SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
			MeshFilter mesh_filter = GetComponent<MeshFilter>();
			_ps = GetComponent<ParticleSystem>();

			// Ideally, I'd like a popup to occur in the editor when we add the component an object.
			// Something like RequireComponent(SkinnedMeshRenderer) || RequireComponent(MeshFilter)
			if (!smr && !mesh_filter) {
				Debug.LogError("'" + name + "' must have either a SkinnedMeshRenderer or a MeshFilter!");
				enabled = false;
				return;
			}

			Mesh mesh = (smr) ? smr.sharedMesh : mesh_filter.sharedMesh;

			_verts = mesh.vertices;
			_tris = mesh.triangles;
			_colors = mesh.colors32;

			_ps.enableEmission = false;

			_spawn_time = 1.0f / _ps.emissionRate;

			if (useVertColor && _colors.Length == 0) {
				Debug.LogError("The mesh used on object '" + name + "' does not have any vertex color data!");
			}
		}

		void Update()
		{
			// For some reason, Time.deltaTime in the editor is always zero. So use the fixedDeltaTime instead when in the editor.
#if UNITY_EDITOR
			_time_cache += Time.fixedDeltaTime;
#else
			_time_cache += Time.deltaTime;
#endif
			int num_particles = Mathf.RoundToInt(_time_cache / _spawn_time);
			_time_cache %= _spawn_time;

			for (int i = 0; i < num_particles; ++i) {
				SpawnParticle();
			}
		}

		private void SpawnParticle()
		{
			ParticleSystem.Particle particle = new ParticleSystem.Particle();
			particle.angularVelocity = startAngularVelocity;
			particle.axisOfRotation = startAxisOfRotation;
			particle.color = _ps.startColor;
			particle.lifetime = _ps.startLifetime;
			particle.randomSeed = (uint)Random.Range(0.0f, (float)uint.MaxValue);
			particle.rotation = _ps.startRotation;
			particle.size = _ps.startSize;
			particle.startLifetime = _ps.startLifetime;
			particle.velocity = new Vector3(_ps.startSpeed, _ps.startSpeed, _ps.startSpeed);

			int triangle_index = Random.Range(0, _tris.Length / 3);
			int vert_index = Random.Range(0, 2);
			int index_origin = _tris[triangle_index * 3 + vert_index];
			int index_a = _tris[triangle_index * 3 + ((vert_index + 1) % 2)];
			int index_b = _tris[triangle_index * 3 + ((vert_index + 2) % 2)];

			Vector3 dir = Vector3.Lerp(_verts[index_a], _verts[index_b], Random.value);
			dir -= _verts[index_origin];
			dir.Normalize();

			particle.velocity = Vector3.Scale(particle.velocity, dir);
			particle.position = _verts[index_origin];

			if (useVertColor && _colors.Length > 0) {
				particle.color = _colors[index_origin];
			}

			// Apparently calling Scale() on the object itself is broken and doesn't actually work ... how do you fuck that up?
			//particle.velocity.Scale(dir);

			if (_ps.simulationSpace == ParticleSystemSimulationSpace.World) {
				particle.position = transform.TransformPoint(particle.position);
				particle.velocity = transform.TransformVector(particle.velocity);
			}

			_ps.Emit(particle);
		}
	}

}
