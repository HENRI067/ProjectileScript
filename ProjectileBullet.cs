using System.Collections;
using UnityEngine;
/// <summary>
/// A projectile that is meant to be shot from the camera of the player in first person
/// <para>:3</para>
/// </summary>
public class ProjectileBullet : MonoBehaviour
{
    [SerializeField] LayerMask damageAndCollideLayers;
    [SerializeField] Transform collisionEffect;

    ///VARIABLES SET BY THE WEAPON SCRIPT
    int damage;
    float speed;
    float range;
    bool moveBullet;/// the bullet will move after the variables above are Setup by the weapon that spawned it 
                    
    RaycastHit colliderAhead;
    Transform bulletTrail;
    Vector3 startPosition;
    Vector3 endPosition;
    float distanceTraveled;
    float hiddenTrailDistance;

    public void SetUp(int damage, float speed, float range, Vector3 muzzlePosition)
    {
        //SETUP
        this.damage = damage;
        this.speed = speed;
        this.range = range;
        startPosition = transform.position;

        //TRAIL SETUP
        endPosition = startPosition + transform.forward * range;
        bulletTrail = transform.GetChild(0);
        bulletTrail.parent = null;

        ///position trail behind the muzzle to match the projectile's distance from the end position
        bulletTrail.gameObject.SetActive(false);
        Vector3 endToBarrelDirection = (muzzlePosition - endPosition).normalized;
        bulletTrail.position = endPosition + endToBarrelDirection * range;

        ///only render the trail after its past the muzzle
        hiddenTrailDistance = Vector3.Distance(bulletTrail.position, muzzlePosition) - 0.1f;

        moveBullet = true;

    }

    void Update()
    {
        if (moveBullet)
        {
            /// a Raycast checks to see if there is a colliderAhead of the projectile before moving it
            /// if the RaycastHit.point is closer than the next position is to the current position then the bullet will hit the collider instead of moving 
            MoveAndDamage();

            /// moves the trail towards the end position, unless the Raycast from the MoveAndDamage() hit something
            MoveTrail();

            ///
            if (distanceTraveled >= range) DestroyBullet();
        }
    }
    void MoveAndDamage()
    {
        //currentPosition = transform.position
        Vector3 nextPosition = transform.position + transform.forward * speed * Time.deltaTime;
        float distanceToNextPosition = Vector3.Distance(transform.position, nextPosition);

        Physics.Raycast(transform.position, transform.forward, out colliderAhead, range - distanceTraveled, damageAndCollideLayers);

        bool colliderWasHit = false;
        if (colliderAhead.collider != null)
            colliderWasHit = colliderAhead.distance < distanceToNextPosition;


        if (colliderWasHit)
        {
            //is it damageable?
            if (colliderAhead.transform.GetComponent<E_Health>())///it would be better to implement an INTERFACE here instead of checking for a specific script!!!
            {
                colliderAhead.transform.GetComponent<E_Health>().TakeDamage(damage);
            }
            //it's a wall then, display a collision effect
            else
            {
                if (collisionEffect) Instantiate(collisionEffect, colliderAhead.point, transform.rotation);
            }
            //DIE
            DestroyBullet();
        }
        //target won't get hit this frame OR there is no target in the way
        else
        {
            transform.position = nextPosition;
        }
        distanceTraveled = Vector3.Distance(startPosition, transform.position);
    }
    void MoveTrail()
    {
        Vector3 expectedCollisionPoint = endPosition;

        if (distanceTraveled > hiddenTrailDistance)
        {
            ///since the Trail Setup (line 33) moved the trail spawnpoint behind the barrel, we only show it after it passes the barrel,
            ///the downside to this is that if the game has a slow time mechanic or the bullets are slow by default, you will not see the projectile untill it asses the {noTrailDistance}                           
            ///(fire a bullet with the speed of 1 to see what I mean)
            bulletTrail.gameObject.SetActive(true);
            if (colliderAhead.transform != null) expectedCollisionPoint = colliderAhead.point;
        }

        Vector3 trailMoveDirection = (expectedCollisionPoint - bulletTrail.position).normalized;
        bulletTrail.position += trailMoveDirection * speed * Time.deltaTime;
    }

    void DestroyBullet()
    {
        Destroy(bulletTrail.gameObject, 1f);
        Destroy(gameObject);
    }
}