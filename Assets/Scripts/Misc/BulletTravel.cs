using UnityEngine;

namespace Misc
{
	public class BulletTravel : MonoBehaviour
	{
	
		public GameObject hitEffect;
		public AudioSource hitCarSource;
		public AudioSource hitGroundSource;
	
		public void StartMoving(Vector3 travelDirection)
		{
			GetComponent<Rigidbody>().velocity = travelDirection;
		}

		private void OnTriggerEnter(Collider collision)
		{
			// Ignore triggers
			if (collision.isTrigger) return;
			if(collision.gameObject.CompareTag("Car")){
				AudioSource explosion = Instantiate(hitCarSource, transform.position, Quaternion.identity);
				explosion.enabled = true;
				explosion.Play();
			}
			else{
				AudioSource explosion = Instantiate(hitGroundSource, transform.position, Quaternion.identity);
				explosion.enabled = true;
				explosion.Play();
			}
			Instantiate(hitEffect, transform.position, Quaternion.identity);
			
			Destroy(GetComponent<Rigidbody>());
			Invoke(nameof(KillingMyself), GetComponent<TrailRenderer>().time);
		}

		private void KillingMyself() {
			Destroy(gameObject);
		}
	}
}
